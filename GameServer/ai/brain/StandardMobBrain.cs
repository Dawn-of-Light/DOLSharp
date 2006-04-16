/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using System.Reflection;
using System.Text;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Standard brain for standard mobs
	/// </summary>
	public class StandardMobBrain : APlayerVicinityBrain, IAggressiveBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public const int MAX_AGGRO_DISTANCE = 2400;

		/// <summary>
		/// Returns the string representation of the StandardMobBrain
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" m_aggroLevel=").Append(m_aggroLevel)
				.Append(" m_aggroRange=").Append(m_aggroRange)
				.ToString();
		}

		#region AI

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			GameMob bodyMob = Body as GameMob;
			if (AggroLevel > 0 || (bodyMob != null && bodyMob.Faction != null))
			{
				CheckAggro();
			}
		}

		/// <summary>
		/// Check for aggro against close living
		/// </summary>
		protected virtual void CheckAggro()
		{
			if (Body.AttackState)
				return;
			foreach(GameLiving target in Body.GetInRadius(typeof(GameLiving), (ushort) AggroRange))
			{
				if (m_aggroTable.ContainsKey(target))
					continue; // add only new NPCs
				if (!target.Alive || target.ObjectState != eObjectState.Active || target.IsStealthed)
					continue;

				if (Util.Chance(CalculateAggroChance(target)))
				{
					GamePlayer playerTarget = target as GamePlayer;
					if(playerTarget != null)
					{
						if (playerTarget.Steed != null) continue; //do not attack players on steed
						AddToAggroList(playerTarget, playerTarget.EffectiveLevel<<1);
					}
					else
					{
						AddToAggroList(target, (target.Level+1)<<1);
					}
				}
			}
		}

		#endregion

		#region Aggro

		/// <summary>
		/// Max Aggro range in that this npc searches for enemies
		/// </summary>
		protected int m_aggroRange = 0;
		/// <summary>
		/// Aggressive Level of this npc
		/// </summary>
		protected int m_aggroLevel = 0;
		/// <summary>
		/// List of livings that this npc has aggro on, living => aggroamount
		/// </summary>
		protected readonly Hashtable	m_aggroTable = new Hashtable();

		/// <summary>
		/// Aggressive Level in % 0..100, 0 means not Aggressive
		/// </summary>
		public virtual int AggroLevel
		{
			get { return m_aggroLevel; }
			set { m_aggroLevel = value; }
		}

		/// <summary>
		/// Range in that this npc aggros
		/// </summary>
		public virtual int AggroRange
		{
			get { return m_aggroRange; }
			set { m_aggroRange = value; }
		}

        public void Aggro(GameLiving living, int aggroamount)
        {
            //			log.Debug(Body.Name + ": AddToAggroList="+(living==null?"(null)":living.Name)+", "+aggroamount);

            // only protect if gameplayer and aggroamout > 0
            if (living is GamePlayer && aggroamount > 0)
            {
                GamePlayer player = (GamePlayer)living;

                if (player.PlayerGroup != null)
                { // player is in group, add whole group to aggro list
                    lock (m_aggroTable.SyncRoot)
                    {
                        foreach (GamePlayer groupPlayer in player.PlayerGroup.GetPlayersInTheGroup())
                        {
                            if (m_aggroTable[groupPlayer] == null)
                            {
                                m_aggroTable[groupPlayer] = 1L;	// add the missing group member on aggro table
                            }
                        }
                    }
                }

                //ProtectEffect protect = (ProtectEffect) player.EffectList.GetOfType(typeof(ProtectEffect));
                foreach (ProtectEffect protect in player.EffectList.GetAllOfType(typeof(ProtectEffect)))
                {
                    // if no aggro left => break
                    if (aggroamount <= 0) break;

                    //if (protect==null) continue;
                    if (protect.ProtectTarget != living) continue;
                    if (protect.ProtectSource.Stun) continue;
                    if (protect.ProtectSource.Mez) continue;
                    if (protect.ProtectSource.Sitting) continue;
                    if (protect.ProtectSource.ObjectState != eObjectState.Active) continue;
                    if (!protect.ProtectSource.Alive) continue;
                    if (!protect.ProtectSource.InCombat) continue;

                    if (!living.Position.CheckSquareDistance(protect.ProtectSource.Position, (uint)(ProtectAbilityHandler.PROTECT_DISTANCE * ProtectAbilityHandler.PROTECT_DISTANCE)))
                        continue;
                    // P I: prevents 10% of aggro amount
                    // P II: prevents 20% of aggro amount
                    // P III: prevents 30% of aggro amount
                    // guessed percentages, should never be higher than or equal to 50%
                    int abilityLevel = protect.ProtectSource.GetAbilityLevel(Abilities.Protect);
                    int protectAmount = (int)((abilityLevel * 0.10) * aggroamount);

                    if (protectAmount > 0)
                    {
                        aggroamount -= protectAmount;
                        protect.ProtectSource.Out.SendMessage("You are protecting " + player.GetName(0, false) + " and distract " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        //						player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

                        lock (m_aggroTable.SyncRoot)
                        {
                            if (m_aggroTable[protect.ProtectSource] != null)
                            {
                                long amount = (long)m_aggroTable[protect.ProtectSource];
                                amount += protectAmount;
                                m_aggroTable[protect.ProtectSource] = amount;
                            }
                            else
                            {
                                m_aggroTable[protect.ProtectSource] = (long)protectAmount;
                            }
                        }
                    }
                }
            }

            lock (m_aggroTable.SyncRoot)
            {
                if (m_aggroTable[living] != null)
                {
                    long amount = (long)m_aggroTable[living];
                    amount += aggroamount;
                    if (amount <= 0)
                    {
                        m_aggroTable.Remove(living);
                    }
                    else
                    {
                        m_aggroTable[living] = amount;
                    }
                }
                else
                {
                    if (aggroamount > 0)
                    {
                        m_aggroTable[living] = (long)aggroamount;
                    }
                }

                AttackMostWanted();
            }
        }
		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro		
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public virtual void AddToAggroList(GameLiving living, int aggroamount)
		{
            if (!Body.InCombat) BringFriends(living);
            Aggro(living, aggroamount);
		}

		/// <summary>
		/// Get current amount of aggro on aggrotable
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public virtual long GetAggroAmountForLiving(GameLiving living)
		{
			lock (m_aggroTable.SyncRoot) 
			{
				if (m_aggroTable[living] != null) 
				{
					return (long)m_aggroTable[living];
				}
				return 0;
			}
		}

		/// <summary>
		/// Remove one living from aggro list
		/// </summary>
		/// <param name="living"></param>
		public virtual void RemoveFromAggroList(GameLiving living)
		{
			lock (m_aggroTable.SyncRoot)
			{
				m_aggroTable.Remove(living);
			}
			AttackMostWanted();
		}

		/// <summary>
		/// Remove all livings from the aggrolist
		/// </summary>
		public virtual void ClearAggroList()
		{
			lock (m_aggroTable.SyncRoot)
			{
				m_aggroTable.Clear();
			}
		}

		/// <summary>
		/// Makes a copy of current aggro list
		/// </summary>
		/// <returns></returns>
		public virtual Hashtable CloneAggroList()
		{
			lock (m_aggroTable.SyncRoot)
			{
				return (Hashtable)m_aggroTable.Clone();
			}
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		protected virtual void AttackMostWanted()
		{
			if (!IsActive)
				return;
			Body.Buff(Body);
			GameLiving target = CalculateNextAttackTarget();
			if (target!=null)
			{
				if (!Body.AttackState || target!=Body.TargetObject) 
				{
					if (!Body.StartSpellAttack(target))
						Body.StartAttack(target);
				}
			}
			else 
			{
				Body.WalkToSpawn();	
			}
		}

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected virtual GameLiving CalculateNextAttackTarget()
		{
			GameLiving maxAggroObject=null;
			lock (m_aggroTable.SyncRoot)
			{
				double maxAggro = 0;
				IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
				while (aggros.MoveNext())
				{
					GameLiving living = (GameLiving)aggros.Key;
					long amount = (long)aggros.Value;
					//DOLConsole.WriteLine(this.Name+": check aggro "+living.Name+" "+amount);

					if(living.Alive && !living.IsStealthed
						&& amount > maxAggro
						&& living.Region == Body.Region
						&& living.ObjectState == eObjectState.Active)
					{
						int distance = Body.Position.GetDistance(living.Position);
						if (distance < MAX_AGGRO_DISTANCE) 
						{
							double aggro = amount * Math.Min(500.0 / distance, 1);
							if (aggro > maxAggro) 
							{
								maxAggroObject = living;
								maxAggro = aggro;
							}
							//DOLConsole.WriteLine(this.Name+": max aggro "+living.Name+" "+amount);
						}
					}
				}
			}

			if (maxAggroObject==null) 
			{	// nobody left
				m_aggroTable.Clear();
			}			
			return maxAggroObject;
		}

		/// <summary>
		/// calculate the aggro of this npc against another living
		/// </summary>
		/// <param name="target"></param>
		/// <returns></returns>
		public virtual int CalculateAggroChance(GameLiving target) 
		{
			if (GameServer.ServerRules.IsSameRealm(Body, target, true)) return 0;
			if (target.IsObjectGreyCon(Body)) return 0;	// only attack if green+ to target
			
			// TODO change the aggro based on the distance with the target
			int aggroLevel = AggroLevel;
			
			GameMob bodyMob = Body as GameMob;
			GamePlayer targetPlayer = target as GamePlayer;
			if (bodyMob != null && targetPlayer != null && bodyMob.Faction != null)
			{
				aggroLevel += targetPlayer.GetFactionAggroLevel(bodyMob.Faction);
			}

			if (aggroLevel > 100) return 100;
			if (aggroLevel < 0) return 0;
			return aggroLevel;
		}

		/// <summary>
		/// Receives all messages of the body
		/// </summary>
		/// <param name="e">The event received</param>
		/// <param name="sender">The event sender</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (!IsActive) return;

			if (sender == Body)
			{
				if (e == GameLivingBaseEvent.TakeDamage)
				{
					TakeDamageEventArgs eArgs = args as TakeDamageEventArgs;
					if (eArgs == null) return;

					int aggro = eArgs.DamageAmount + eArgs.CriticalAmount;
					if (eArgs.DamageSource is GameNPC)
					{
						// owner gets 25% of aggro
						IControlledBrain brain = ((GameNPC)eArgs.DamageSource).Brain as IControlledBrain;
						if (brain != null)
						{
							AddToAggroList(brain.Owner, (int)Math.Max(1, aggro*0.25));
							aggro = (int)Math.Max(1, aggro * 0.75);
						}
					}
					AddToAggroList(eArgs.DamageSource, aggro);
					return;
				}

				if (e == GameLivingBaseEvent.AttackedByEnemy)
				{
					AttackedByEnemyEventArgs eArgs = args as AttackedByEnemyEventArgs;
					if (eArgs == null) return;
					OnAttackedByEnemy(eArgs.AttackData);
					return;
				}

				if (e == GameLivingEvent.EnemyHealed)
				{
					EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
					if (eArgs != null && eArgs.HealSource is GameLiving)
						AddToAggroList((GameLiving)eArgs.HealSource, eArgs.HealAmount);
					return;
				}

				if (e == GameLivingEvent.EnemyKilled)
				{
					EnemyKilledEventArgs eArgs = args as EnemyKilledEventArgs;
					if (eArgs != null)
					{
						// transfer all controlled target aggro to the owner
						if (eArgs.Target is GameNPC)
						{
							IControlledBrain controlled = ((GameNPC)eArgs.Target).Brain as IControlledBrain;
							if (controlled != null)
							{
								long contrAggro = GetAggroAmountForLiving(controlled.Body);
								AddToAggroList(controlled.Owner, (int)contrAggro);
							}
						}
						RemoveFromAggroList((GameLiving) eArgs.Target);
					}
					return;
				}

				if (e == GameLivingBaseEvent.Dying)
				{
					// clean aggro table
					ClearAggroList();
					return;
				}

				if (e == GameNPCEvent.FollowLostTarget)
				{
					FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
					if (eArgs == null) return;
					AttackMostWanted();
					return;
				}
			}
		}

		/// <summary>
		/// Attacked by enemy event
		/// </summary>
		/// <param name="ad"></param>
		protected virtual void OnAttackedByEnemy(AttackData ad)
		{
			if (!Body.AttackState && Body.Alive && Body.ObjectState==eObjectState.Active &&																									
				(ad.AttackResult == GameLiving.eAttackResult.HitUnstyled || 
				ad.AttackResult == GameLiving.eAttackResult.HitStyle || 
				ad.AttackResult == GameLiving.eAttackResult.Missed || 
				ad.AttackResult == GameLiving.eAttackResult.Blocked || 
				ad.AttackResult == GameLiving.eAttackResult.Evaded || 
				ad.AttackResult == GameLiving.eAttackResult.Fumbled || 
				ad.AttackResult == GameLiving.eAttackResult.Parried))
			{
				Body.StartAttack(ad.Attacker);
			}
		}

		protected virtual void BringFriends(GameLiving living)
		{
            if (living is GamePlayer) 
			{
                GamePlayer player = (GamePlayer)living;
                ArrayList inRangeGroupPlayers = new ArrayList();
                if (player.PlayerGroup != null)
                {
                    foreach (GamePlayer p in player.PlayerGroup)
                    {
                        //low cpu usage to use check distance better than get in radius
                        if (p.Position.CheckDistance(player.Position, 1500) && p != player)
                        {
                            inRangeGroupPlayers.Add(p);
                        }
                    }
                }
                if (inRangeGroupPlayers.Count >= 3)
                {
                    int i = DOL.GS.Util.Random(inRangeGroupPlayers.Count - 1);
                    GameNPC npc = FindFriendForAttack();
					if (npc != null) 
					{
                        ((StandardMobBrain)npc.Brain).Aggro((GamePlayer)inRangeGroupPlayers[i],2);
					}
                    if (inRangeGroupPlayers.Count >= 5)
                    {
                        inRangeGroupPlayers.RemoveAt(i);
                         i = DOL.GS.Util.Random(inRangeGroupPlayers.Count - 1);
                         npc = FindFriendForAttack();
					    if (npc != null) 
					    {
                            ((StandardMobBrain)npc.Brain).Aggro((GamePlayer)inRangeGroupPlayers[i],2);
					    }
                    }
                }
            }
		}

		/// <summary>
		/// searches for a friend to group for combat
		/// </summary>
		/// <returns></returns>
		protected virtual GameNPC FindFriendForAttack()
		{
			foreach (GameNPC npc in Body.GetInRadius(typeof(GameNPC), 600)) 
			{
                if (npc is GameMob && npc.Name == Body.Name && !npc.InCombat && npc.Brain is IControlledBrain == false && (npc.Brain is IAggressiveBrain)) 
				{
					return npc;
				}
			}
			return null;
		}

		#endregion
	}
}
