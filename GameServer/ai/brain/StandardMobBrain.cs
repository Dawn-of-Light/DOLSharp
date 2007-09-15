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
using DOL.Database;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.RealmAbilities;
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

		public const int MAX_AGGRO_DISTANCE = 3600;

		/// <summary>
		/// Constructs a new StandardMobBrain
		/// </summary>
		public StandardMobBrain()
			: base()
		{
			m_aggroLevel = 0;
			m_aggroMaxRange = 0;
		}

		/// <summary>
		/// Returns the string representation of the StandardMobBrain
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return base.ToString() + ", m_aggroLevel=" + m_aggroLevel.ToString() + ", m_aggroMaxRange=" + m_aggroMaxRange.ToString();
		}

		#region AI

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			//If the npc is not casting, and we have spells, we run a check to see if we should cast any of them
			//if (!Body.IsCasting && Body.Spells != null && Body.Spells.Count > 0)
			//Instead - lets just let CheckSpells() make all the checks for us
			//Check for just positive spells
			CheckSpells(true);

			//If the npc is returning home, we don't need to think.
			if (Body.IsReturningHome)
				return;

			//If the npc is not in combat, we remove the last attack data temporary property
			if (!Body.InCombat)
				Body.TempProperties.removeProperty(GameLiving.LAST_ATTACK_DATA);

			// check for returning to home if to far away
			if ( Body.MaxDistance != 0 )
			{
				int distance = WorldMgr.GetDistance(Body.SpawnX,Body.SpawnY,Body.SpawnZ, Body.X,Body.Y,Body.Z);
				int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * AggroRange / 100;
				if ( maxdistance > 0 && distance > maxdistance )
					Body.WalkToSpawn(Body.MaxSpeed*2);
			}
			
			//If we have an aggrolevel above 0, we check for players and npcs in the area to attack
			if (!Body.AttackState && AggroLevel > 0)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
				AttackMostWanted();
			}

			//If this NPC can randomly walk around, we allow it to walk around
			if (!Body.AttackState && CanRandomWalk)
			{
				IPoint3D target = CalcRandomWalkTarget();
				if (target != null)
					Body.WalkTo(target, 50);
			}
			else
			{
				//If the npc can move, and the npc is not casting, not moving, and not attacking or in combat
				if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
				{
					//If the npc is not at it's spawn position, we tell it to walk to it's spawn position
					if (Body.X != Body.SpawnX ||
						Body.Y != Body.SpawnY ||
						Body.Z != Body.SpawnZ)
						Body.WalkToSpawn();
				}

				//If we are not attacking, and not casting, and not moving, and we aren't facing our spawn heading, we turn to the spawn heading
				if (!Body.AttackState && !Body.IsCasting && !Body.IsMoving
					&& Body.Heading != Body.SpawnHeading)
					Body.TurnTo(Body.SpawnHeading);
			}
		}

		/// <summary>
		/// Check for aggro against close NPCs
		/// </summary>
		protected virtual void CheckNPCAggro()
		{
			if (Body.AttackState)
				return;
			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange))
			{
				if (m_aggroTable.ContainsKey(npc))
					continue; // add only new NPCs
				if (!npc.IsAlive || npc.ObjectState != GameObject.eObjectState.Active)
					continue;
				if (npc is GameHorse)
					continue; //do not attack horses

				if (CalculateAggroLevelToTarget(npc) > 0)
				{
					AddToAggroList(npc, (npc.Level + 1) << 1);
				}
			}
		}

		/// <summary>
		/// Check for aggro against players
		/// </summary>
		protected virtual void CheckPlayerAggro()
		{
			//Check if we are already attacking, return if yes
			if (Body.AttackState)
				return;
			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange))
			{
				if (m_aggroTable.ContainsKey(player))
					continue; // add only new players
				if (!player.IsAlive || player.ObjectState != GameObject.eObjectState.Active || player.IsStealthed)
					continue;
				if (player.Steed != null)
					continue; //do not attack players on steed

				if (CalculateAggroLevelToTarget(player) > 0)
				{
					AddToAggroList(player, player.EffectiveLevel << 1);
				}
			}
		}

		/// <summary>
		/// The interval for thinking, min 1.5 seconds
		/// 10 seconds for 0 aggro mobs
		/// </summary>
		public override int ThinkInterval
		{
			get { return Math.Max(1500, 10000 - AggroLevel * 100); }
		}

		/// <summary>
		/// If this brain is part of a formation, it edits it's values accordingly.
		/// </summary>
		/// <param name="x">The x-coordinate to refer to and change</param>
		/// <param name="y">The x-coordinate to refer to and change</param>
		/// <param name="z">The x-coordinate to refer to and change</param>
		public virtual bool CheckFormation(ref int x, ref int y, ref int z)
		{
			return false;
		}

		#endregion

		#region Aggro

		/// <summary>
		/// Max Aggro range in that this npc searches for enemies
		/// </summary>
		protected int m_aggroMaxRange;
		/// <summary>
		/// Aggressive Level of this npc
		/// </summary>
		protected int m_aggroLevel;
		/// <summary>
		/// List of livings that this npc has aggro on, living => aggroamount
		/// </summary>
		protected readonly Hashtable m_aggroTable = new Hashtable();

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
			get { return m_aggroMaxRange; }
			set { m_aggroMaxRange = value; }
		}

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro		
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public virtual void AddToAggroList(GameLiving living, int aggroamount)
		{
			if (m_body.IsConfused) return;

			if (living == null) return;
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
					if (protect.ProtectSource.IsStunned) continue;
					if (protect.ProtectSource.IsMezzed) continue;
					if (protect.ProtectSource.IsSitting) continue;
					if (protect.ProtectSource.ObjectState != GameObject.eObjectState.Active) continue;
					if (!protect.ProtectSource.IsAlive) continue;
					if (!protect.ProtectSource.InCombat) continue;

					if (!WorldMgr.CheckDistance(living, protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))
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

				//This shouldn't be called here either.  Just let a function do exactly
				//what it says.  We can call this after the function call if we want.
				//AttackMostWanted();
			}
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
			//This makes no sense to put here - this function just removes something from a list.
			//It shouldn't make the thing start attacking something.
			//AttackMostWanted();
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
			GameLiving target = CalculateNextAttackTarget();
			if (target != null)
			{
				if (!Body.AttackState || target != Body.TargetObject)
				{
					if (!CheckSpells(false))
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
			//DOLConsole.WriteLine(this.Name+": CalculateAttackObject()");
			GameLiving maxAggroObject = null;
			lock (m_aggroTable.SyncRoot)
			{
				double maxAggro = 0;
				IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
				while (aggros.MoveNext())
				{
					GameLiving living = (GameLiving)aggros.Key;
					long amount = (long)aggros.Value;
					//DOLConsole.WriteLine(this.Name+": check aggro "+living.Name+" "+amount);

					if (living.IsAlive
						&& amount > maxAggro
						&& living.CurrentRegion == Body.CurrentRegion
						&& living.ObjectState == GameObject.eObjectState.Active)
					{
						int distance = WorldMgr.GetDistance(Body, living);
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

			if (maxAggroObject == null)
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
		public virtual int CalculateAggroLevelToTarget(GameLiving target)
		{
			if (GameServer.ServerRules.IsSameRealm(Body, target, true)) return 0;
			if (target.IsObjectGreyCon(Body)) return 0;	// only attack if green+ to target
			//if (Level <= 3) return 0;	// workaround, dont aggro for newbie mobs
			if (Body.Faction != null && target is GamePlayer)
			{
				GamePlayer player = (GamePlayer)target;
				AggroLevel = Body.Faction.GetAggroToFaction(player);
			}
			if (AggroLevel >= 100) return 100;
			return AggroLevel;
		}

		/// <summary>
		/// Receives all messages of the body
		/// </summary>
		/// <param name="e">The event received</param>
		/// <param name="sender">The event sender</param>
		/// <param name="args">The event arguments</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			if (!IsActive) return;

			if (sender == Body)
			{
				if (e == GameObjectEvent.TakeDamage)
				{
					TakeDamageEventArgs eArgs = args as TakeDamageEventArgs;
					if (eArgs == null || eArgs.DamageSource is GameLiving == false) return;

					int aggro = eArgs.DamageAmount + eArgs.CriticalAmount;
					if (eArgs.DamageSource is GameNPC)
					{
						// owner gets 25% of aggro
						IControlledBrain brain = ((GameNPC)eArgs.DamageSource).Brain as IControlledBrain;
						if (brain != null)
						{
							AddToAggroList(brain.Owner, (int)Math.Max(1, aggro * 0.25));
							aggro = (int)Math.Max(1, aggro * 0.75);
						}
					}
					AddToAggroList((GameLiving)eArgs.DamageSource, aggro);
					return;
				}

				if (e == GameLivingEvent.AttackedByEnemy)
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

				//				if (e == GameLivingEvent.EnemyHealed)
				//				{
				//					EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
				//					if (eArgs != null && eArgs.HealSource is GameLiving)
				//					{
				//						int aggro = eArgs.HealAmount;
				//
				//						// check protect only if gameplayer and aggro > 0
				//						if (eArgs.HealSource is GamePlayer && aggro > 0)
				//						{
				//							GamePlayer player = (GamePlayer)eArgs.HealSource;
				//							foreach (ProtectEffect protect in player.EffectList.GetAllOfType(typeof(ProtectEffect))) 
				//							{
				//								// max 2 protect by player (1 active protect by player is possible cf: "X is already protecting Y")
				//								// check all requirement to transfer the aggro
				//								if (protect.ProtectTarget == player
				//									&& !protect.ProtectSource.Stun && !protect.ProtectSource.Mez && !protect.ProtectSource.Sitting
				//									&& protect.ProtectSource.ObjectState == GameObject.eObjectState.Active
				//									&& protect.ProtectSource.InCombat && protect.ProtectSource.Alive											
				//									&& WorldMgr.CheckDistance(player, protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))	
				//								{
				//									// P I: prevent 10-20% of aggro amount
				//									// P II: prevent 20-30% of aggro amount
				//									// P III: prevent 30-40% of aggro amount
				//									int abilityLevel = protect.ProtectSource.GetAbilityLevel(Abilities.Protect);
				//									int protectAmount = (int)((abilityLevel * 0.10 + DOL.GS.Util.RandomDouble() * 0.10) * aggro);
				//									if (protectAmount > 0)
				//									{
				//										aggro -= protectAmount;
				//										AddToAggroList(protect.ProtectSource, protectAmount);
				//
				//										protect.ProtectSource.Out.SendMessage("You are protecting " + player.GetName(0, false) + " and distract " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				//										player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);				
				//									}    
				//								}
				//							}		
				//						}
				//
				//						AddToAggroList((GameLiving)eArgs.HealSource, aggro);
				//					}
				//					return;
				//				}

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
						RemoveFromAggroList(eArgs.Target);
					}
					return;
				}

				if (e == GameLivingEvent.Dying)
				{
					// clean aggro table
					ClearAggroList();
					return;
				}

				if (e == GameNPCEvent.FollowLostTarget)
				{
					FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
					if (eArgs == null) return;
					OnFollowLostTarget(eArgs.LostTarget);
					return;
				}

			}
		}

		/// <summary>
		/// Lost follow target event
		/// </summary>
		/// <param name="target"></param>
		protected virtual void OnFollowLostTarget(GameObject target)
		{
			AttackMostWanted();
			if (!Body.AttackState)
				Body.WalkToSpawn();
		}

		/// <summary>
		/// Attacked by enemy event
		/// </summary>
		/// <param name="ad"></param>
		protected virtual void OnAttackedByEnemy(AttackData ad)
		{
			if (!Body.AttackState 
				&& Body.IsAlive 
				&& Body.ObjectState == GameObject.eObjectState.Active 
				&& ad.IsHit)
			{
				Body.StartAttack(ad.Attacker);
				BringFriends(ad);
			}
		}

		protected virtual void BringFriends(AttackData ad)
		{
			// some experimental code workaround for bring a friend
			/////////////////////////////////////////////////////////////
			if (ad.Attacker is GamePlayer)
			{
				GamePlayer player = (GamePlayer)ad.Attacker;
				ArrayList nearPlayers = new ArrayList();
				foreach (GamePlayer p in ad.Attacker.GetPlayersInRadius(500))
				{
					if (p != player)
					{
						nearPlayers.Add(p);
					}
				}
				ArrayList inRangeGroupPlayers = new ArrayList();
				if (player.PlayerGroup != null)
				{
					foreach (GamePlayer p in ad.Attacker.GetPlayersInRadius(1500))
					{
						if (p.PlayerGroup == player.PlayerGroup && p != player)
						{
							inRangeGroupPlayers.Add(p);
						}
					}
				}
				GamePlayer victim = null;
				GamePlayer victim2 = null;
				if (nearPlayers.Count >= 3)
				{

					// roulette selection
					int i = DOL.GS.Util.Random(nearPlayers.Count - 1);
					victim = (GamePlayer)nearPlayers[i];
					if (nearPlayers.Count >= 6)
					{
						nearPlayers.RemoveAt(i);
						i = DOL.GS.Util.Random(nearPlayers.Count - 1);
						victim2 = (GamePlayer)nearPlayers[i];
					}

				}
				else if (player.PlayerGroup != null && player.PlayerGroup.PlayerCount >= 4 && inRangeGroupPlayers.Count > 0)
				{

					// roulette selection
					int i = DOL.GS.Util.Random(inRangeGroupPlayers.Count - 1);
					victim = (GamePlayer)inRangeGroupPlayers[i];
					if (player.PlayerGroup.PlayerCount >= 7 && inRangeGroupPlayers.Count > 1)
					{
						inRangeGroupPlayers.RemoveAt(i);
						i = DOL.GS.Util.Random(inRangeGroupPlayers.Count - 1);
						victim2 = (GamePlayer)inRangeGroupPlayers[i];
					}

				}

				// find a friend to attack selected player
				if (victim != null)
				{
					GameNPC npc = FindFriendForAttack();
					if (npc != null)
					{
						npc.StartAttack(victim);
						if (victim2 != null)
						{
							npc = FindFriendForAttack();
							if (npc != null)
							{
								npc.StartAttack(victim2);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// searches for a friend to group for combat
		/// </summary>
		/// <returns></returns>
		protected GameNPC FindFriendForAttack()
		{
			foreach (GameNPC npc in Body.GetNPCsInRadius(600))
			{
				if (npc.Name == Body.Name && !npc.InCombat && npc.Brain is IControlledBrain == false)
				{
					return npc;
				}
			}
			return null;
		}

		#endregion

		#region Spells

		/// <summary>
		/// Checks if any spells need casting
		/// </summary>
		/// <param name="Positive">whether to check positive (true) spells or negative (false) spells</param>
		/// <remarks>Specify whether we should check for positive (buffs, heals, etc) or negative (damages, taunts, stuns, etc).</remarks>
		public virtual bool CheckSpells(bool Positive)
		{
			//Make sure owns a body, has spells, and isn't casting
			//By checking IsCasting here - we should be able to save a lot of processor time
			//doing worthless checks just to find out the body is casting
			if (this.Body != null && this.Body.Spells != null && this.Body.Spells.Count > 0 && !Body.IsCasting)
			{
				bool casted = false;
				if (Positive)
				{

					foreach (Spell spell in Body.Spells)
					{
						//Maybe Body.InCombat?
						if (!Body.AttackState)
						{
							if (CheckPositiveSpells(spell))
								casted = true;
						}
					}
				}
				else
				{
					foreach (Spell spell in Body.Spells)
					{
						if (spell.CastTime > 0)
						{
							if (Body.CurrentRegion.Time - Body.LastAttackedByEnemyTick > 10 * 1000)
								if (CheckNegativeSpells(spell))
									casted = true;
						}
						else
							CheckInstantSpells(spell);
					}
				}
				if (this is IControlledBrain && !Body.AttackState)
					((IControlledBrain)this).Follow(((IControlledBrain)this).Owner);
				return casted;
			}
			return false;
		}

		/// <summary>
		/// Checks the Positive Spells.  Handles buffs, heals, etc.
		/// </summary>
		protected virtual bool CheckPositiveSpells(Spell spell)
		{
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			switch (spell.SpellType)
			{
				#region Buffs
				case "StrengthConstitutionBuff":
				case "DexterityQuicknessBuff":
				case "StrengthBuff":
				case "DexterityBuff":
				case "ConstitutionBuff":
				case "ArmorFactorBuff":
				case "ArmorAbsorbtionBuff":
				case "CombatSpeedBuff":
				case "MeleeDamageBuff":
				case "AcuityBuff":
				case "HealthRegenBuff":
				case "DamageAdd":
				case "DamageShield":
				case "BodyResistBuff":
				case "ColdResistBuff":
				case "EnergyResistBuff":
				case "HeatResistBuff":
				case "MatterResistBuff":
				case "SpiritResistBuff":
				case "BodySpiritEnergyBuff":
				case "HeatColdMatterBuff":
				case "CrushSlashThrustBuff":
				case "OffensiveProc":
				case "DefensiveProc":
				case "Bladeturn":
					{
						//Buff self
						if (!LivingHasEffect(Body, spell))
						{
							Body.TargetObject = Body;
							break;
						}

						//ControlledNpc now overrides this part
						//if (spell.Target == "Realm" && this is IControlledBrain)
						//{
						//    GameLiving owner = (this as IControlledBrain).Owner;
						//    GamePlayer player = null;
						//    //Buff owner
						//    if (!LivingHasEffect(owner, spell))
						//    {
						//        Body.TargetObject = owner;
						//        break;
						//    }

						//    if (owner is GameNPC)
						//    {
						//        //Buff other minions
						//        foreach (IControlledBrain icb in ((GameNPC)owner).ControlledNpcList)
						//        {
						//            if (icb == null)
						//                continue;
						//            if (!LivingHasEffect(icb.Body, spell))
						//            {
						//                Body.TargetObject = icb.Body;
						//                break;
						//            }
						//        }
						//        player = (GamePlayer)((IControlledBrain)((GameNPC)owner).Brain).Owner;
						//    }
						//    else
						//        player = (GamePlayer)owner;

						//    //Buff player
						//    if (player != null)
						//    {
						//        if (!LivingHasEffect(player, spell))
						//        {
						//            Body.TargetObject = player;
						//            break;
						//        }

						//        ////Buff group
						//        //if (player.PlayerGroup != null)
						//        //{
						//        //    foreach (GamePlayer gplayer in player.PlayerGroup.GetPlayersInTheGroup())
						//        //    {
						//        //        if (!HasEffect(gplayer, spell))
						//        //        {
						//        //            this.TargetObject = gplayer;
						//        //            this.CastSpell(spell, spellline);
						//        //            this.TargetObject = lastTarget;
						//        //            return;
						//        //        }
						//        //    }
						//        //}
						//    }
						//}
						break;
					}
				#endregion

				#region Disease Cure/Poison Cure/Summon
				case "CureDisease":
					if (!Body.IsDiseased)
						break;
					Body.TargetObject = Body;
					break;
				case "Summon":
					Body.TargetObject = Body;
					break;
				case "SummonMinion":
					//If the list is null, lets make sure it gets initialized!
					if (Body.ControlledNpcList == null)
						Body.InitControlledNpc(2);
					else
					{
						//Let's check to see if the list is full - if it is, we can't cast another minion.
						//If it isn't, let them cast.
						IControlledBrain[] icb = Body.ControlledNpcList;
						int numberofpets = 0;
						for (int i = 0; i < icb.Length; i++)
						{
							if (icb[i] != null)
								numberofpets++;
						}
						if (numberofpets >= icb.Length)
							break;
					}
					Body.TargetObject = Body;
					break;
				#endregion

				#region Heals
				case "Heal":
					//Heal self
					if (Body.HealthPercent < 75)
					{
						Body.TargetObject = Body;
						break;
					}

					//ControlledNpc now overrides this part
					//if (this is IControlledBrain)
					//{
					//    //Heal owner
					//    GameLiving owner = (this as IControlledBrain).Owner;
					//    if (owner.HealthPercent < 75)
					//    {
					//        Body.TargetObject = owner;
					//        break;
					//    }

					//    GamePlayer player;
					//    //Get the minion's GamePlayer
					//    if (owner is GamePlayer)
					//        player = (GamePlayer)owner;
					//    else
					//        player = (GamePlayer)((IControlledBrain)((GameNPC)owner).Brain).Owner;

					//    //If minion, heal player and other minions
					//    if (Body.IsMinion)
					//    {
					//        if (player.HealthPercent < 75)
					//        {
					//            Body.TargetObject = player;
					//            break;
					//        }

					//        if (owner is GameNPC)
					//        {
					//            //Heal other minions
					//            foreach (IControlledBrain icb in ((GameNPC)owner).ControlledNpcList)
					//            {
					//                if (icb == null)
					//                    continue;
					//                if (icb.Body.HealthPercent < 75)
					//                {
					//                    Body.TargetObject = icb.Body;
					//                    break;
					//                }
					//            }
					//        }
					//        //Heal group
					//        /*
					//        if (player.PlayerGroup != null)
					//        {
					//            foreach (GamePlayer gplayer in player.PlayerGroup.GetPlayersInTheGroup())
					//            {
					//                if (gplayer.HealthPercent < 75)
					//                    CheckSpellsByType("Heal", gplayer);

					//            }
					//        }*/
					//    }

					//    if (player.PlayerGroup != null && player.CharacterClass.ID == (int)eCharacterClass.Enchanter)
					//    {
					//        foreach (GamePlayer gplayer in player.PlayerGroup.GetPlayersInTheGroup())
					//        {
					//            if (gplayer.HealthPercent < 75)
					//            {
					//                Body.TargetObject = gplayer;
					//                break;
					//            }
					//        }
					//    }
					//}
					break;
				#endregion
			}
			if (Body.TargetObject != null)
			{
				if (Body.IsMoving)
					Body.StopFollow();
				Body.TurnTo(Body.TargetObject);
				Body.CastSpell(spell, m_mobSpellLine);
				Body.TargetObject = lastTarget;
				return true;
			}
			Body.TargetObject = lastTarget;
			return false;
		}

		/// <summary>
		/// Checks the Negative Spells.  Handles dds, debuffs, etc.
		/// </summary>
		protected virtual bool CheckNegativeSpells(Spell spell)
		{
			if (spell.Target != "Enemy" && spell.Target != "Area")
				return false;
			if (Body.TargetObject != null)
			{
				if (LivingHasEffect((GameLiving)Body.TargetObject, spell))
					return false;
				if (Body.IsMoving)
					Body.StopFollow();
				Body.TurnTo(Body.TargetObject);
				Body.CastSpell(spell, m_mobSpellLine);
				return true;
			}
			return false;
		}

		/// <summary>
		/// Checks Instant Spells.  Handles Taunts, shouts, stuns, etc.
		/// </summary>
		protected virtual bool CheckInstantSpells(Spell spell)
		{
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			switch (spell.SpellType)
			{
				#region Enemy Spells
				case "DirectDamage":
				case "Lifedrain":
				case "DamageOverTime":
				case "Disease":
				case "Stun":
				case "Mez":
				case "Taunt":
					Body.TargetObject = lastTarget;
					break;
				#endregion

				#region Combat Spells
				case "ComabtHeal":
				case "DamageAdd":
				case "ArmorFactorBuff":
				case "EnduranceRegenBuff":
				case "CombatSpeedBuff":
				case "AblativeArmor":
				case "Bladeturn":
					Body.TargetObject = Body;
					//if(LivingHasEffect(Body, spell))
					//    return false;
					break;
				#endregion
			}
			//This will take care of any thing with "debuff" in its name
			if (Body.TargetObject == null && this.Body.CanFight && spell.SpellType.ToLower().Contains("debuff"))
				Body.TargetObject = lastTarget;

			if (Body.TargetObject != null)
			{
				if (LivingHasEffect((GameLiving)Body.TargetObject, spell))
					return false;
				Body.CastSpell(spell, m_mobSpellLine);
				Body.TargetObject = lastTarget;
				return true;
			}
			Body.TargetObject = lastTarget;
			return false;
		}

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public virtual void CheckAbilities()
		{
			//load up abilities
			if (Body.Abilities != null && Body.Abilities.Count > 0)
			{
				foreach (Ability ab in Body.Abilities.Values)
				{
					switch (ab.KeyName)
					{
						case Abilities.Intercept:
							//Intercept for ControlledBrain: is there any cases for other types?  Maybe are there
							//any ML trials that send out an 'Intercept' like effect that blocks players from attacks?
							if (this is IControlledBrain)
							{
								if (Body.EffectList.GetOfType(typeof(InterceptEffect)) == null)
								{
									new InterceptEffect().Start(Body, (this as IControlledBrain).Owner);
								}
							}
							break;
					}
				}
			}
		}

		protected static SpellLine m_mobSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);

		/// <summary>
		/// Checks if the living target has a spell effect
		/// </summary>
		/// <param name="target">The target living object</param>
		/// <param name="spell">The spell to check</param>
		/// <returns>True if the living has the effect</returns>
		protected bool LivingHasEffect(GameLiving target, Spell spell)
		{
			if (target == null)
				return true;

			//Check through each effect in the target's effect list
			foreach (IGameEffect effect in target.EffectList)
			{
				//If the effect we are checking is not a gamespelleffect keep going
				if (effect is GameSpellEffect == false)
					continue;

				GameSpellEffect speffect = effect as GameSpellEffect;
				//if the effect's spell's spelltype is not the same as the checking spell's spelltype keep going
				if (speffect.Spell.SpellType != spell.SpellType)
					continue;

				//if the effect's spell's effectgroup is the same as the checking spell's spellgroup return the answer true
				if (speffect.Spell.EffectGroup == spell.EffectGroup)
					return true;
			}
			//the answer is no, the effect has not been found
			return false;
		}
		#endregion

		#region Random Walk
		public virtual bool CanRandomWalk
		{
			get
			{
				if (!DOL.GS.ServerProperties.Properties.ALLOW_ROAM)
					return false;
				if (Body.MaxSpeedBase == 0)
					return false;
				if (Body.InCombat || Body.IsMoving || Body.IsCasting)
					return false;
				if (Body.Realm != 0)
					return false;
				if (Body.Name == "horse")
					return false;
//				if (!char.IsLower(Body.Name[0]))
				if (!char.IsLower(Body.Name[0]) && (DOL.GS.ServerProperties.Properties.SERV_LANGUAGE != "DE"))
					return false;
				if (Body.CurrentRegion.IsDungeon)
					return false;
				//if (Body.Name.StartsWith("ambient"))
					//return false;
				if (Util.Chance(70))
					return false;
				return true;
			}
		}

		public virtual IPoint3D CalcRandomWalkTarget()
		{
			int roamingRadius = 300;
			roamingRadius = Util.Random(0, Math.Max(100, roamingRadius));

			double angle = Util.Random(0, 360) / (2 * Math.PI);
			double targetX = Body.SpawnX + Util.Random(-roamingRadius, roamingRadius);
			double targetY = Body.SpawnY + Util.Random(-roamingRadius, roamingRadius);
			//double targetZ = (Body.IsUnderwater) ? Body.SpawnZ : 0; /*(Body.Flags & (uint)GameNPC.eFlags.FLYING) == (uint)GameNPC.eFlags.FLYING ||  + Util.Random(-100, 100)*/

			return new Point3D((int)targetX, (int)targetY, Body.SpawnZ);
		}

		#endregion
	}
}
