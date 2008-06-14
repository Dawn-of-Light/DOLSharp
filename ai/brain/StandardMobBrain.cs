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
using DOL.GS.Movement;
using DOL.GS.RealmAbilities;
using DOL.Language;
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
			// If the NPC is tethered and has been pulled too far it will
			// de-aggro and return to its spawn point.
			if (Body.IsOutOfTetherRange)
			{
				Body.StopFollow();
				ClearAggroList();
				Body.WalkToSpawn();
				return;
			}

			//Instead - lets just let CheckSpells() make all the checks for us
			//Check for just positive spells
			CheckSpells(eCheckSpellType.Defensive);

			//If the npc is returning home, we don't need to think.
			if (Body.IsReturningHome)
				return;

			//If the npc is not in combat, we remove the last attack data temporary property
			if (!Body.InCombat)
				Body.TempProperties.removeProperty(GameLiving.LAST_ATTACK_DATA);

			// check for returning to home if to far away
			if (Body.MaxDistance != 0)
			{
				int distance = WorldMgr.GetDistance(Body.SpawnX, Body.SpawnY, Body.SpawnZ, Body.X, Body.Y, Body.Z);
				int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * AggroRange / 100;
				if (maxdistance > 0 && distance > maxdistance)
					Body.WalkToSpawn(Body.MaxSpeed * 2);
			}

			//If we have an aggrolevel above 0, we check for players and npcs in the area to attack
			if (!Body.AttackState && AggroLevel > 0)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
			}
			if (IsAggroing)
				AttackMostWanted(); // Only start an attack if there actually is something to attack

			//Mob will now always walk on their path
			if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving
				&& !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath
				&& Body.PathID != null && Body.PathID != "" && Body.PathID != "NULL")
			{
				PathPoint path = MovementMgr.LoadPath(Body.PathID);
				Body.CurrentWayPoint = path;
				Body.MoveOnPath(path.MaxSpeed);
			}

			//If this NPC can randomly walk around, we allow it to walk around
			if (!Body.AttackState && CanRandomWalk && Util.Chance(10))
			{
				IPoint3D target = CalcRandomWalkTarget();
				if (target != null)
					Body.WalkTo(target, 50);
			}
			//If the npc can move, and the npc is not casting, not moving, and not attacking or in combat
			else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat)
			{
				//If the npc is not at it's spawn position, we tell it to walk to it's spawn position
				if (Body.X != Body.SpawnX || Body.Y != Body.SpawnY || Body.Z != Body.SpawnZ)
					Body.WalkToSpawn();
			}

			//If we are not attacking, and not casting, and not moving, and we aren't facing our spawn heading, we turn to the spawn heading
			if (!Body.InCombat && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && WorldMgr.GetDistance(Body.SpawnX, Body.SpawnY, Body.SpawnZ, Body.X, Body.Y, Body.Z) > 500)
			{
				Body.WalkToSpawn(); // Mobs do not walk back at 2x their speed..
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
				// Don't aggro on immune players.

				if (player.EffectList.GetOfType(typeof(ShadeEffect)) != null)
					continue;

				int aggrolevel = 0;

				if (Body.Faction != null)
				{
					aggrolevel = Body.Faction.GetAggroToFaction(player);
					if (aggrolevel < 0)
						aggrolevel = 0;
				}

				if (aggrolevel <= 0 && AggroLevel <= 0)
					return;

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

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public virtual void CheckAbilities()
		{
			//See CNPC
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
		/// Checks whether living has someone on its aggrolist		
		/// </summary>
		public virtual bool IsAggroing
		{
			get
			{
				bool aggroing = false;
				lock (m_aggroTable.SyncRoot)
				{
					aggroing = (m_aggroTable.Count > 0);
				}
				return aggroing;
			}
		}

		/// <summary>
		/// Add aggro table of this brain to that of another living.
		/// </summary>
		/// <param name="brain">The target brain.</param>
		public void AddAggroListTo(StandardMobBrain brain)
		{
			// TODO: This should actually be the other way round, but access
			// to m_aggroTable is restricted and needs to be threadsafe.

			// do not modify aggro list if dead
			if (!brain.Body.IsAlive) return;

			lock (m_aggroTable.SyncRoot)
			{
				IDictionaryEnumerator dictEnum = m_aggroTable.GetEnumerator();
				while (dictEnum.MoveNext())
					brain.AddToAggroList((GameLiving)dictEnum.Key, Body.MaxHealth);
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
			if (m_body.IsConfused) return;

			// tolakram - duration spell effects will attempt to add to aggro after npc is dead
			if (!m_body.IsAlive) return;

			if (living == null) return;
			//			log.Debug(Body.Name + ": AddToAggroList="+(living==null?"(null)":living.Name)+", "+aggroamount);

			// only protect if gameplayer and aggroamout > 0
			if (living is GamePlayer && aggroamount > 0)
			{
				GamePlayer player = (GamePlayer)living;

				if (player.Group != null)
				{ // player is in group, add whole group to aggro list
					lock (m_aggroTable.SyncRoot)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							if (m_aggroTable[p] == null)
							{
								m_aggroTable[p] = 1L;	// add the missing group member on aggro table
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
						protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client, "AI.Brain.StandardMobBrain.YouProtDist", player.GetName(0, false), Body.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						//player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

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
					if (!CheckSpells(eCheckSpellType.Offensive))
						Body.StartAttack(target);
				}
			}
		}

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected virtual GameLiving CalculateNextAttackTarget()
		{
			GameLiving maxAggroObject = null;
			lock (m_aggroTable.SyncRoot)
			{
				double maxAggro = 0;
				IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
				while (aggros.MoveNext())
				{
					GameLiving living = (GameLiving)aggros.Key;

					// Don't bother about necro shade, can't attack it anyway.
					if (living.EffectList.GetOfType(typeof(NecromancerShadeEffect)) != null)
						continue;

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
				else if (e == GameLivingEvent.AttackedByEnemy)
				{
					AttackedByEnemyEventArgs eArgs = args as AttackedByEnemyEventArgs;
					if (eArgs == null) return;
					OnAttackedByEnemy(eArgs.AttackData);
					return;
				}
				else if (e == GameLivingEvent.EnemyHealed)
				{
					EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
					if (eArgs != null && eArgs.HealSource is GameLiving)
					{
						//Higher Aggro amount and NO peace flag npcs!
						if (eArgs.HealSource is GamePlayer || (eArgs.HealSource is GameNPC && (((GameNPC)eArgs.HealSource).Flags & (uint)GameNPC.eFlags.PEACE) == 0))
							AddToAggroList((GameLiving)eArgs.HealSource, (eArgs.HealAmount * 2));
					}
					return;
				}
				else if (e == GameLivingEvent.EnemyKilled)
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
				else if (e == GameLivingEvent.Dying)
				{
					// clean aggro table
					ClearAggroList();
					return;
				}
				else if (e == GameNPCEvent.FollowLostTarget)
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

		#endregion

		#region Bring a Friend

		/// <summary>
		/// Mobs within this range will be called upon to add on a group
		/// of players inside of a dungeon.
		/// </summary>
		protected static ushort m_BAFReinforcementsRange = 2000;

		/// <summary>
		/// Players within this range around the puller will be subject
		/// to attacks from adds.
		/// </summary>
		protected static ushort m_BAFTargetPlayerRange = 3000;

		/// <summary>
		/// BAF range for adds close to the pulled mob.
		/// </summary>
		public virtual ushort BAFCloseRange
		{
			get { return (ushort)((AggroRange * 2) / 5); }
		}

		/// <summary>
		/// BAF range for group adds in dungeons.
		/// </summary>
		public virtual ushort BAFReinforcementsRange
		{
			get { return m_BAFReinforcementsRange; }
			set { m_BAFReinforcementsRange = (value > 0) ? (ushort)value : (ushort)0; }
		}

		/// <summary>
		/// Range for potential targets around the puller.
		/// </summary>
		public virtual ushort BAFTargetPlayerRange
		{
			get { return m_BAFTargetPlayerRange; }
			set { m_BAFTargetPlayerRange = (value > 0) ? (ushort)value : (ushort)0; }
		}

		/// <summary>
		/// Bring friends when this living is attacked. There are 2
		/// different mechanisms for BAF:
		/// 1) Any mobs of the same faction within a certain (short) range
		///    around the pulled mob will add on the puller, anywhere.
		/// 2) In dungeons, group size is taken into account as well, the
		///    bigger the group, the more adds will come, even if they are
		///    not close to the pulled mob.
		/// </summary>
		/// <param name="attackData">The data associated with the puller's attack.</param>
		protected virtual void BringFriends(AttackData attackData)
		{
			// Only add on players.

			GameLiving attacker = attackData.Attacker;
			if (attacker is GamePlayer)
			{
				BringCloseFriends(attackData);
				if (attacker.CurrentRegion.IsDungeon)
					BringReinforcements(attackData);
			}
		}

		/// <summary>
		/// Get mobs close to the pulled mob to add on the puller and his
		/// group as well.
		/// </summary>
		/// <param name="attackData">The data associated with the puller's attack.</param>
		protected virtual void BringCloseFriends(AttackData attackData)
		{
			// Have every friend within close range add on the attacker's
			// group.

			GamePlayer attacker = (GamePlayer)attackData.Attacker;

			foreach (GameNPC npc in Body.GetNPCsInRadius(BAFCloseRange))
			{
				if (npc.IsFriend(Body) && npc.IsAvailable && npc.IsAggressive)
				{
					StandardMobBrain brain = (StandardMobBrain)npc.Brain;
					brain.AddToAggroList(PickTarget(attacker), 1);
					brain.AttackMostWanted();
				}
			}
		}

		/// <summary>
		/// Get mobs to add on the puller's group, their numbers depend on the 
		/// group's size.
		/// </summary>
		/// <param name="attackData">The data associated with the puller's attack.</param>
		protected virtual void BringReinforcements(AttackData attackData)
		{
			// Determine how many friends to bring, as a rule of thumb, allow for
			// max 2 players dealing with 1 mob. Only players from the group the
			// original attacker is in will be taken into consideration.
			// Example: A group of 3 or 4 players will get 1 add, a group of 7 or 8
			// players will get 3 adds.

			GamePlayer attacker = (GamePlayer)attackData.Attacker;
			Group attackerGroup = attacker.Group;
			int numAttackers = (attackerGroup == null) ? 1 : attackerGroup.MemberCount;
			int maxAdds = (numAttackers + 1) / 2 - 1;
			if (maxAdds > 0)
			{
				// Bring friends, try mobs in the neighbourhood first. If there
				// aren't any, try getting some from farther away.

				int numAdds = 0;
				ushort range = 250;

				while (numAdds < maxAdds && range <= BAFReinforcementsRange)
				{
					foreach (GameNPC npc in Body.GetNPCsInRadius(range))
					{
						if (numAdds >= maxAdds) break;

						// If it's a friend, have it attack a random target in the 
						// attacker's group.

						if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable)
						{
							StandardMobBrain brain = (StandardMobBrain)npc.Brain;
							brain.AddToAggroList(PickTarget(attacker), 1);
							brain.AttackMostWanted();
							++numAdds;
						}
					}

					// Increase the range for finding friends to join the fight.

					range *= 2;
				}
			}
		}

		/// <summary>
		/// Pick a random target from the attacker's group that is within a certain
		/// range of the original puller.
		/// </summary>
		/// <param name="attacker">The original attacker.</param>
		/// <returns></returns>
		protected virtual GamePlayer PickTarget(GamePlayer attacker)
		{
			Group attackerGroup = attacker.Group;

			// If no group, pick the attacker himself.

			if (attackerGroup == null) return attacker;

			// Make a list of all players in the attacker's group within
			// a certain range around the puller.

			ArrayList attackersInRange = new ArrayList();

			foreach (GamePlayer player in attackerGroup.GetPlayersInTheGroup())
				if (WorldMgr.CheckDistance(attacker, player, BAFTargetPlayerRange))
					attackersInRange.Add(player);

			// Pick a random player from the list.

			return (GamePlayer)(attackersInRange[Util.Random(1, attackersInRange.Count) - 1]);
		}

		#endregion

		#region Spells

		public enum eCheckSpellType
		{
			Offensive,
			Defensive
		}

		/// <summary>
		/// Checks if any spells need casting
		/// </summary>
		/// <param name="type">Which type should we go through and check for?</param>
		/// <returns></returns>
		public bool CheckSpells(eCheckSpellType type)
		{
			//Make sure owns a body, has spells, and isn't casting
			//By checking IsCasting here - we should be able to save a lot of processor time
			//doing worthless checks just to find out the body is casting
			//Prevent mob from casting if an interrupt action is running (saves CPU time and makes
			//the mob look less daft)
			if (this.Body != null && this.Body.Spells != null && this.Body.Spells.Count > 0 && !Body.IsCasting)
			{
				bool casted = false;
				if (type == eCheckSpellType.Defensive)
				{
					foreach (Spell spell in Body.Spells)
					{
						// Why wouldn't a healer type mob try to save his ass even when in melee?
						// Allow defensive spells
						//if (!Body.AttackState)
						//{
						if (!Body.IsBeingInterrupted && Body.GetSkillDisabledDuration(spell) == 0 && CheckDefensiveSpells(spell))
						{
							casted = true;
							break;
						}
						//}
					}
				}
				else
				{
					foreach (Spell spell in Body.Spells)
					{
						if (Body.GetSkillDisabledDuration(spell) == 0)
						{
							if (spell.CastTime > 0)
							{
								if (!Body.IsBeingInterrupted && Body.CurrentRegion.Time - Body.LastAttackedByEnemyTick > 10 * 1000 && Util.Chance(50) && CheckOffensiveSpells(spell))
								{
									casted = true;
									break;
								}
							}
							else
								CheckInstantSpells(spell);
						}
					}
				}
				if (this is IControlledBrain && !Body.AttackState)
					((IControlledBrain)this).Follow(((IControlledBrain)this).Owner);
				return casted;
			}
			return false;
		}

		/// <summary>
		/// Checks defensive spells.  Handles buffs, heals, etc.
		/// </summary>
		protected virtual bool CheckDefensiveSpells(Spell spell)
		{
			if (spell == null) return false;
			if (Body.GetSkillDisabledDuration(spell) > 0) return false;
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
						// Buff self, if not in melee, but not each and every mob
						// at the same time, because it looks silly.
						if (!LivingHasEffect(Body, spell) && !Body.AttackState && Util.Chance(40))
						{
							Body.TargetObject = Body;
							break;
						}
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
					// Heal self when dropping below 30%.
					if (Body.HealthPercent < 30)
					{
						Body.TargetObject = Body;
						break;
					}

					break;
				#endregion
			}

			if (Body.TargetObject != null)
			{
				if (Body.IsMoving && spell.CastTime > 0)
					Body.StopFollow();

				if (Body.TargetObject != Body && spell.CastTime > 0)
					Body.TurnTo(Body.TargetObject);

				Body.CastSpell(spell, m_mobSpellLine);

				Body.TargetObject = lastTarget;
				return true;
			}

			Body.TargetObject = lastTarget;

			return false;
		}

		/// <summary>
		/// Checks offensive spells.  Handles dds, debuffs, etc.
		/// </summary>
		protected virtual bool CheckOffensiveSpells(Spell spell)
		{
			if (spell.Target.ToLower() != "enemy" && spell.Target.ToLower() != "area" && spell.Target.ToLower() != "cone")
				return false;

			if (Body.TargetObject != null && !LivingHasEffect((GameLiving)Body.TargetObject, spell))
			{
				if (Body.IsMoving && spell.CastTime > 0)
					Body.StopFollow();

				if (Body.TargetObject != Body && spell.CastTime > 0)
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
				case "DexterityDebuff":
				case "DamageOverTime":
				case "Disease":
				case "Stun":
				case "Mez":
				case "Taunt":
					if (!LivingHasEffect(lastTarget as GameLiving, spell))
						Body.TargetObject = lastTarget;
					break;
				#endregion

				#region Combat Spells
				case "CombatHeal":
				case "DamageAdd":
				case "ArmorFactorBuff":
				case "DexterityQuicknessBuff":
				case "EnduranceRegenBuff":
				case "CombatSpeedBuff":
				case "AblativeArmor":
				case "Bladeturn":
					if (!LivingHasEffect(Body, spell))
						Body.TargetObject = Body;
					break;
				#endregion
			}

			if (Body.TargetObject != null)
			{
				Body.CastSpell(spell, m_mobSpellLine);
				Body.TargetObject = lastTarget;

				return true;
			}

			Body.TargetObject = lastTarget;
			return false;
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
				if (Body.RoamingRange <= 0)
					return false;
				return true;
			}
		}

		public virtual IPoint3D CalcRandomWalkTarget()
		{
			int roamingRadius = 300;

			if (Body.RoamingRange > 0)
			{
				roamingRadius = Body.RoamingRange;
			}

			roamingRadius = Util.Random(0, Math.Max(100, roamingRadius));

			double angle = Util.Random(0, 360) / (2 * Math.PI);
			double targetX = Body.SpawnX + Util.Random(-roamingRadius, roamingRadius);
			double targetY = Body.SpawnY + Util.Random(-roamingRadius, roamingRadius);
			//double targetZ = (Body.IsUnderwater) ? Body.SpawnZ : 0;
			/*(Body.Flags & (uint)GameNPC.eFlags.FLYING) == (uint)GameNPC.eFlags.FLYING ||  + Util.Random(-100, 100)*/

			return new Point3D((int)targetX, (int)targetY, Body.SpawnZ);
		}

		#endregion
	}
}
