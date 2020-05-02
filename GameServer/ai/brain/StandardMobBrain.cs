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
using System.Collections.Generic;
using System.Reflection;
using DOL.Events;
using DOL.GS;
using DOL.GS.Effects;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;
using DOL.GS.SkillHandler;
using DOL.GS.Keeps;
using DOL.Language;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// Standard brain for standard mobs
	/// </summary>
	public class StandardMobBrain : APlayerVicinityBrain, IOldAggressiveBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public const int MAX_AGGRO_DISTANCE = 3600;
		public const int MAX_AGGRO_LIST_DISTANCE = 6000;
		public const int MAX_PET_AGGRO_DISTANCE = 512; // Tolakram - Live test with caby pet - I was extremely close before auto aggro
		
		// Used for AmbientBehaviour "Seeing" - maintains a list of GamePlayer in range
		public List<GamePlayer> PlayersSeen = new List<GamePlayer>();
		
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

		public override bool Stop()
		{
			// tolakram - when the brain stops, due to either death or no players in the vicinity, clear the aggro list
			if (base.Stop())
			{
				ClearAggroList();
				return true;
			}

			return false;
		}

		#region AI

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			//Satyr:
			//This is a general information. When i review this Think-Procedure and the interaction between it and some
			//code of GameNPC.cs i have the feeling this is a mixture of much ideas of diffeent people, much unfinished
			//features like random-walk which does not actually fit to the rest of this Brain-logic.
			//In other words:
			//If somebody feeling like redoing this stuff completly i would appreciate it. It might be worth redoing
			//instead of trying desperately to make something work that is simply chaoticly moded by too much
			//diffeent inputs.
			//For NOW i made the aggro working the following way (close to live but not yet 100% equal):
			//Mobs will not aggro on their way back home (in fact they should even under some special circumstances)
			//They will completly forget all Aggro when respawned and returned Home.


			// If the NPC is tethered and has been pulled too far it will
			// de-aggro and return to its spawn point.
			if (Body.IsOutOfTetherRange && !Body.InCombat)
			{
				Body.WalkToSpawn();
				return;
			}
			// If the NPC is Moving on path, it can detect closed doors and open them
			if(Body.IsMovingOnPath) DetectDoor();
			//Instead - lets just let CheckSpells() make all the checks for us
			//Check for just positive spells
			CheckSpells(eCheckSpellType.Defensive);

			// Note: Offensive spells are checked in GameNPC:SpellAction timer

			// check for returning to home if to far away
			if (Body.MaxDistance != 0 && !Body.IsReturningHome)
			{
				int distance = Body.GetDistanceTo( Body.SpawnPoint );
				int maxdistance = Body.MaxDistance > 0 ? Body.MaxDistance : -Body.MaxDistance * AggroRange / 100;
				if (maxdistance > 0 && distance > maxdistance)
				{
					Body.WalkToSpawn();
					return;
				}
			}

			//If this NPC can randomly walk around, we allow it to walk around
			if (!Body.AttackState && CanRandomWalk && !Body.IsRoaming && Util.Chance(DOL.GS.ServerProperties.Properties.GAMENPC_RANDOMWALK_CHANCE))
			{
				IPoint3D target = CalcRandomWalkTarget();
				if (target != null)
				{
					if (Util.IsNearDistance(target.X, target.Y, target.Z, Body.X, Body.Y, Body.Z, GameNPC.CONST_WALKTOTOLERANCE))
					{
						Body.TurnTo(Body.GetHeading(target));
					}
					else
					{
						Body.WalkTo(target, 50);
					}

					Body.FireAmbientSentence(GameNPC.eAmbientTrigger.roaming);
				}
			}
			//If the npc can move, and the npc is not casting, not moving, and not attacking or in combat
			else if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath)
			{
				//If the npc is not at it's spawn position, we tell it to walk to it's spawn position
				//Satyr: If we use a tolerance to stop their Way back home we also need the same
				//Tolerance to check if we need to go home AGAIN, otherwise we might be told to go home
				//for a few units only and this may end before the next Arrive-At-Target Event is fired and in this case
				//We would never lose the state "IsReturningHome", which is then followed by other erros related to agro again to players
				if ( !Util.IsNearDistance( Body.X, Body.Y, Body.Z, Body.SpawnPoint.X, Body.SpawnPoint.Y, Body.SpawnPoint.Z, GameNPC.CONST_WALKTOTOLERANCE ) )
					Body.WalkToSpawn();
				else if (Body.Heading != Body.SpawnHeading)
					Body.Heading = Body.SpawnHeading;
			}

			//Mob will now always walk on their path
			if (Body.MaxSpeedBase > 0 && Body.CurrentSpellHandler == null && !Body.IsMoving
			    && !Body.AttackState && !Body.InCombat && !Body.IsMovingOnPath
			    && Body.PathID != null && Body.PathID != "" && Body.PathID != "NULL")
			{
				PathPoint path = MovementMgr.LoadPath(Body.PathID);
				if (path != null)
				{
					Body.CurrentWayPoint = path;
					Body.MoveOnPath((short)path.MaxSpeed);
				}
				else
				{
					log.ErrorFormat("Path {0} not found for mob {1}.", Body.PathID, Body.Name);
				}
			}

			//If we are not attacking, and not casting, and not moving, and we aren't facing our spawn heading, we turn to the spawn heading
			if( !Body.IsMovingOnPath && !Body.InCombat && !Body.AttackState && !Body.IsCasting && !Body.IsMoving && Body.IsWithinRadius( Body.SpawnPoint, 500 ) == false )
			{
				Body.WalkToSpawn(); // Mobs do not walk back at 2x their speed..
				Body.IsReturningHome = false; // We are returning to spawn but not the long walk home, so aggro still possible
			}

			if (Body.IsReturningHome == false)
			{
				if (!Body.AttackState && AggroRange > 0)
				{
					var currentPlayersSeen = new List<GamePlayer>();
					foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange, true))
					{
						if (!PlayersSeen.Contains(player))
						{
							Body.FireAmbientSentence(GameNPC.eAmbientTrigger.seeing, player as GameLiving);
							PlayersSeen.Add(player);
						}
						currentPlayersSeen.Add(player);
					}
					
					for (int i=0; i<PlayersSeen.Count; i++)
					{
						if (!currentPlayersSeen.Contains(PlayersSeen[i])) PlayersSeen.RemoveAt(i);
					}
					
				}
				
				//If we have an aggrolevel above 0, we check for players and npcs in the area to attack
				if (!Body.AttackState && AggroLevel > 0)
				{
					CheckPlayerAggro();
					CheckNPCAggro();
				}

				if (HasAggro)
				{
					Body.FireAmbientSentence(GameNPC.eAmbientTrigger.fighting, Body.TargetObject as GameLiving);
					AttackMostWanted();
					return;
				}
				else
				{
					if (Body.AttackState)
						Body.StopAttack();

					Body.TargetObject = null;
				}
			}
		}

		/// <summary>
		/// Check for aggro against close NPCs
		/// </summary>
		protected virtual void CheckNPCAggro()
		{
			if (Body.AttackState)
				return;

			foreach (GameNPC npc in Body.GetNPCsInRadius((ushort)AggroRange, Body.CurrentRegion.IsDungeon ? false : true))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(Body, npc, true)) continue;
				if (m_aggroTable.ContainsKey(npc))
					continue; // add only new NPCs
				if (!npc.IsAlive || npc.ObjectState != GameObject.eObjectState.Active)
					continue;
				if (npc is GameTaxi)
					continue; //do not attack horses

				if (CalculateAggroLevelToTarget(npc) > 0)
				{
					if (npc.Brain is ControlledNpcBrain) // This is a pet or charmed creature, checkLOS
						AddToAggroList(npc, (npc.Level + 1) << 1, true);
					else
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

			foreach (GamePlayer player in Body.GetPlayersInRadius((ushort)AggroRange, true))
			{
				if (!GameServer.ServerRules.IsAllowedToAttack(Body, player, true)) continue;
				// Don't aggro on immune players.

				if (player.EffectList.GetOfType<NecromancerShadeEffect>() != null)
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
					AddToAggroList(player, player.EffectiveLevel << 1, true);
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
		protected readonly Dictionary<GameLiving, long> m_aggroTable = new Dictionary<GameLiving, long>();

		/// <summary>
		/// The aggression table for this mob
		/// </summary>
		public Dictionary<GameLiving, long> AggroTable
		{
			get { return m_aggroTable; }
		}

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
		public virtual bool HasAggro
		{
			get
			{
				bool hasAggro = false;
				lock ((m_aggroTable as ICollection).SyncRoot)
				{
					hasAggro = (m_aggroTable.Count > 0);
				}
				return hasAggro;
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

			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				Dictionary<GameLiving, long>.Enumerator dictEnum = m_aggroTable.GetEnumerator();
				while (dictEnum.MoveNext())
					brain.AddToAggroList(dictEnum.Current.Key, Body.MaxHealth);
			}
		}

		// LOS Check on natural aggro (aggrorange & aggrolevel)
		// This part is here due to los check constraints;
		// Otherwise, it should be in CheckPlayerAggro() method.
		private bool m_AggroLOS;
		public virtual bool AggroLOS
		{
			get { return m_AggroLOS; }
			set { m_AggroLOS = value; }
		}
		private void CheckAggroLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
				AggroLOS=true;
			else
				AggroLOS=false;
		}
		
		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public virtual void AddToAggroList(GameLiving living, int aggroamount)
		{
			AddToAggroList(living, aggroamount, false);
		}
		
		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		/// <param name="CheckLOS"></param>
		public virtual void AddToAggroList(GameLiving living, int aggroamount, bool CheckLOS)
		{
			if (m_body.IsConfused) return;

			// tolakram - duration spell effects will attempt to add to aggro after npc is dead
			if (!m_body.IsAlive) return;

			if (living == null) return;

			// Check LOS (walls, pits, etc...) before  attacking, player + pet
			// Be sure the aggrocheck is triggered by the brain on Think() method
			if (DOL.GS.ServerProperties.Properties.ALWAYS_CHECK_LOS && CheckLOS)
			{
				GamePlayer thisLiving = null;
				if (living is GamePlayer)
					thisLiving = (GamePlayer)living;
				else if (living is GameNPC && (living as GameNPC).Brain is IControlledBrain)
					thisLiving = ((living as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

				if (thisLiving != null)
				{
					thisLiving.Out.SendCheckLOS (Body, living, new CheckLOSResponse(CheckAggroLOS));
					if (!AggroLOS) return;
				}
			}

			BringFriends(living);

			//Handle trigger to say sentance on first aggro.
			if (m_aggroTable.Count < 1)
			{
				Body.FireAmbientSentence(GameNPC.eAmbientTrigger.aggroing, living);
			}

			// only protect if gameplayer and aggroamout > 0
			if (living is GamePlayer && aggroamount > 0)
			{
				GamePlayer player = (GamePlayer)living;
				
				if (player.Group != null)
				{ // player is in group, add whole group to aggro list
					lock ((m_aggroTable as ICollection).SyncRoot)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							if (!m_aggroTable.ContainsKey(p))
							{
								m_aggroTable[p] = 1L;	// add the missing group member on aggro table
							}
						}
					}
				}

				//ProtectEffect protect = (ProtectEffect) player.EffectList.GetOfType(typeof(ProtectEffect));
				foreach (ProtectEffect protect in player.EffectList.GetAllOfType<ProtectEffect>())
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

					if (!living.IsWithinRadius(protect.ProtectSource, ProtectAbilityHandler.PROTECT_DISTANCE))
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
						protect.ProtectSource.Out.SendMessage(LanguageMgr.GetTranslation(protect.ProtectSource.Client.Account.Language, "AI.Brain.StandardMobBrain.YouProtDist", player.GetName(0, false),
						                                                                 Body.GetName(0, false, protect.ProtectSource.Client.Account.Language, Body)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						//player.Out.SendMessage("You are protected by " + protect.ProtectSource.GetName(0, false) + " from " + Body.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

						lock ((m_aggroTable as ICollection).SyncRoot)
						{
							if (m_aggroTable.ContainsKey(protect.ProtectSource))
								m_aggroTable[protect.ProtectSource] += protectAmount;
							else
								m_aggroTable[protect.ProtectSource] = protectAmount;
						}
					}
				}
			}

			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				if (m_aggroTable.ContainsKey(living))
				{
					long amount = m_aggroTable[living];
					amount += aggroamount;

					// can't be removed this way, set to minimum
					if (amount <= 0)
						amount = 1L;

					m_aggroTable[living] = amount;
				}
				else
				{
					if (aggroamount > 0)
					{
						m_aggroTable[living] = aggroamount;
					}
					else
					{
						m_aggroTable[living] = 1L;
					}
				}
			}
		}

		/// <summary>
		/// Get current amount of aggro on aggrotable
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public virtual long GetAggroAmountForLiving(GameLiving living)
		{
			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				if (m_aggroTable.ContainsKey(living))
				{
					return m_aggroTable[living];
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
			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				m_aggroTable.Remove(living);
			}
		}

		/// <summary>
		/// Remove all livings from the aggrolist
		/// </summary>
		public virtual void ClearAggroList()
		{
			CanBAF = true; // Mobs that drop out of combat can BAF again

			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				m_aggroTable.Clear();
				Body.TempProperties.removeProperty(Body.Attackers);
			}
		}

		/// <summary>
		/// Makes a copy of current aggro list
		/// </summary>
		/// <returns></returns>
		public virtual Dictionary<GameLiving, long> CloneAggroList()
		{
			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				return new Dictionary<GameLiving, long>(m_aggroTable);
			}
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		protected virtual void AttackMostWanted()
		{
			if (!IsActive)
				return;

			Body.TargetObject = CalculateNextAttackTarget();

			if (Body.TargetObject != null)
			{
				if (!CheckSpells(eCheckSpellType.Offensive))
				{
					Body.StartAttack(Body.TargetObject);
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
			lock ((m_aggroTable as ICollection).SyncRoot)
			{
				double maxAggro = 0;
				Dictionary<GameLiving, long>.Enumerator aggros = m_aggroTable.GetEnumerator();
				List<GameLiving> removable = new List<GameLiving>();
				while (aggros.MoveNext())
				{
					GameLiving living = aggros.Current.Key;

					// check to make sure this target is still valid
					if (living.IsAlive == false ||
					    living.ObjectState != GameObject.eObjectState.Active ||
					    living.IsStealthed ||
					    Body.GetDistanceTo(living, 0) > MAX_AGGRO_LIST_DISTANCE ||
					    GameServer.ServerRules.IsAllowedToAttack(Body, living, true) == false)
					{
						removable.Add(living);
						continue;
					}

					// Don't bother about necro shade, can't attack it anyway.
					if (living.EffectList.GetOfType<NecromancerShadeEffect>() != null)
						continue;
					
					long amount = aggros.Current.Value;

					if (living.IsAlive
					    && amount > maxAggro
					    && living.CurrentRegion == Body.CurrentRegion
					    && living.ObjectState == GameObject.eObjectState.Active)
					{
						int distance = Body.GetDistanceTo( living );
						int maxAggroDistance = (this is IControlledBrain) ? MAX_PET_AGGRO_DISTANCE : MAX_AGGRO_DISTANCE;

						if (distance <= maxAggroDistance)
						{
							double aggro = amount * Math.Min(500.0 / distance, 1);
							if (aggro > maxAggro)
							{
								maxAggroObject = living;
								maxAggro = aggro;
							}
						}
					}
				}

				foreach (GameLiving l in removable)
				{
					RemoveFromAggroList(l);
					Body.RemoveAttacker(l);
				}
			}

			if (maxAggroObject == null)
			{
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
			// Withdraw if can't attack.
			if (GameServer.ServerRules.IsAllowedToAttack(Body, target, true) == false)
				return 0;
			
			// Get owner if target is pet
			GameLiving realTarget = target;
			if (target is GameNPC)
			{
				if (((GameNPC)target).Brain is IControlledBrain)
				{
					GameLiving owner = (((GameNPC)target).Brain as IControlledBrain).GetLivingOwner();
					if (owner != null)
						realTarget = owner;
				}
			}
			
			// only attack if green+ to target
			if (realTarget.IsObjectGreyCon(Body))
				return 0;	

			// If this npc have Faction return the AggroAmount to Player
			if (Body.Faction != null)
			{
				if (realTarget is GamePlayer)
				{
					return Math.Min(100, Body.Faction.GetAggroToFaction((GamePlayer)realTarget));
				}
				else if (realTarget is GameNPC && Body.Faction.EnemyFactions.Contains(((GameNPC)realTarget).Faction))
				{
					return 100;
				}
			}
			
			//we put this here to prevent aggroing non-factions npcs
			if(Body.Realm == eRealm.None && realTarget is GameNPC)
				return 0;
			
			return Math.Min(100, AggroLevel);
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
				else if (e == GameLivingEvent.Dying)
				{
					// clean aggro table
					ClearAggroList();
					return;
				}
				else if (e == GameNPCEvent.FollowLostTarget) // this means we lost the target
				{
					FollowLostTargetEventArgs eArgs = args as FollowLostTargetEventArgs;
					if (eArgs == null) return;
					OnFollowLostTarget(eArgs.LostTarget);
					return;
				}
				else if (e == GameLivingEvent.CastFailed)
				{
					CastFailedEventArgs realArgs = args as CastFailedEventArgs;
					if (realArgs == null || realArgs.Reason == CastFailedEventArgs.Reasons.AlreadyCasting || realArgs.Reason == CastFailedEventArgs.Reasons.CrowdControlled)
						return;
					Body.StartAttack(Body.TargetObject);
				}
			}

			if (e == GameLivingEvent.EnemyHealed)
			{
				EnemyHealedEventArgs eArgs = args as EnemyHealedEventArgs;
				if (eArgs != null && eArgs.HealSource is GameLiving)
				{
					// first check to see if the healer is in our aggrolist so we don't go attacking anyone who heals
					if (m_aggroTable.ContainsKey(eArgs.HealSource as GameLiving))
					{
						if (eArgs.HealSource is GamePlayer || (eArgs.HealSource is GameNPC && (((GameNPC)eArgs.HealSource).Flags & GameNPC.eFlags.PEACE) == 0))
						{
							AddToAggroList((GameLiving)eArgs.HealSource, eArgs.HealAmount);
						}
					}
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

					Body.Attackers.Remove(eArgs.Target);
					AttackMostWanted();
				}
				return;
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
			    && Body.ObjectState == GameObject.eObjectState.Active)
			{
				if (ad.AttackResult == GameLiving.eAttackResult.Missed)
				{
					AddToAggroList(ad.Attacker, 1);
				}

				Body.StartAttack(ad.Attacker);
			}
		}

		#endregion

		#region Bring a Friend
		/// <summary>
		/// Initial range to try to get BAFs from.
		/// May be overloaded for specific brain types, ie. dragons or keep guards
		/// </summary>
		protected virtual ushort BAFInitialRange
		{
			get { return 250; }
		}

		/// <summary>
		/// Max range to try to get BAFs from.
		/// May be overloaded for specific brain types, ie.dragons or keep guards
		/// </summary>
		protected virtual ushort BAFMaxRange
		{
			get { return 2000; }
		}

		/// <summary>
		/// Max range to try to look for nearby players.
		/// May be overloaded for specific brain types, ie.dragons or keep guards
		/// </summary>
		protected virtual ushort BAFPlayerRange
		{
			get { return 5000; }
		}

		/// <summary>
		/// Can the mob bring a friend?
		/// Set to false when a mob BAFs or is brought by a friend.
		/// </summary>
		public virtual bool CanBAF { get; set; } = true;

		/// <summary>
		/// Bring friends when this mob aggros
		/// </summary>
		/// <param name="attacker">Whoever triggered the BAF</param>
		protected virtual void BringFriends(GameLiving attacker)
		{
			if (!CanBAF)
				return;
			
			GamePlayer puller;  // player that triggered the BAF

			// Only BAF on players and pets of players
			if (attacker is GamePlayer)
				puller = (GamePlayer)attacker;
			else if (attacker is GamePet pet && pet.Owner is GamePlayer owner)
				puller = owner;
			else if (attacker is BDSubPet bdSubPet && bdSubPet.Owner is GamePet bdPet && bdPet.Owner is GamePlayer bdOwner)
				puller = bdOwner;
			else
				return;

			CanBAF = false; // Mobs only BAF once per fight

			int numAttackers = 0;

			List<GamePlayer> victims = null; // Only instantiated if we're tracking potential victims
			
			// These are only used if we have to check for duplicates
			HashSet<String> countedVictims = null;
			HashSet<String> countedAttackers = null;

			BattleGroup bg = puller.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null) as BattleGroup;

			// Check group first to minimize the number of HashSet.Add() calls
			if (puller.Group is Group group)
			{
				if (DOL.GS.ServerProperties.Properties.BAF_MOBS_COUNT_BG_MEMBERS && bg != null)
					countedAttackers = new HashSet<String>(); // We have to check for duplicates when counting attackers

				if (!DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_PULLER)
				{
					if (DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_BG_MEMBERS && bg != null)
					{
						// We need a large enough victims list for group and BG, and also need to check for duplicate victims
						victims = new List<GamePlayer>(group.MemberCount + bg.PlayerCount - 1);
						countedVictims = new HashSet<String>();
					}
					else
						victims = new List<GamePlayer>(group.MemberCount);
				}

				foreach (GamePlayer player in group.GetPlayersInTheGroup())
					if (player != null && (player.InternalID == puller.InternalID || player.IsWithinRadius(puller, BAFPlayerRange, true)))
					{
						numAttackers++;

						if (countedAttackers != null)
							countedAttackers.Add(player.InternalID);

						if (victims != null)
						{
							victims.Add(player);

							if (countedVictims != null)
								countedVictims.Add(player.InternalID);
						}
					}
			} // if (puller.Group is Group group)

			// Do we have to count BG members, or add them to victims list?
			if ((bg != null) && (DOL.GS.ServerProperties.Properties.BAF_MOBS_COUNT_BG_MEMBERS
				|| (DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_BG_MEMBERS && !DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_PULLER)))
			{
				if (victims == null && DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_BG_MEMBERS && !DOL.GS.ServerProperties.Properties.BAF_MOBS_ATTACK_PULLER)
					// Puller isn't in a group, so we have to create the victims list for the BG
					victims = new List<GamePlayer>(bg.PlayerCount);

				foreach (GamePlayer player in bg.GetPlayersInTheBattleGroup())
					if (player != null && (player.InternalID == puller.InternalID || player.IsWithinRadius(puller, BAFPlayerRange, true)))
					{
						if (DOL.GS.ServerProperties.Properties.BAF_MOBS_COUNT_BG_MEMBERS
							&& (countedAttackers == null || !countedAttackers.Contains(player.InternalID)))
								numAttackers++;

						if (victims != null && (countedVictims == null || !countedVictims.Contains(player.InternalID)))
							victims.Add(player);
					}
			} // if ((bg != null) ...

			if (numAttackers == 0)
				// Player is alone
				numAttackers = 1;

			int percentBAF = DOL.GS.ServerProperties.Properties.BAF_INITIAL_CHANCE
				+ ((numAttackers - 1) * DOL.GS.ServerProperties.Properties.BAF_ADDITIONAL_CHANCE);

			int maxAdds = percentBAF / 100; // Multiple of 100 are guaranteed BAFs

			// Calculate chance of an addition add based on the remainder
			if (Util.Chance(percentBAF % 100))
				maxAdds++;

			if (maxAdds > 0)
			{
				int numAdds = 0; // Number of mobs currently BAFed
				ushort range = BAFInitialRange; // How far away to look for friends

				// Try to bring closer friends before distant ones.
				while (numAdds < maxAdds && range <= BAFMaxRange)
				{
					foreach (GameNPC npc in Body.GetNPCsInRadius(range))
					{
						if (numAdds >= maxAdds)
							break;

						// If it's a friend, have it attack
						if (npc.IsFriend(Body) && npc.IsAggressive && npc.IsAvailable && npc.Brain is StandardMobBrain brain)
						{
							brain.CanBAF = false; // Mobs brought cannot bring friends of their own

							GamePlayer target;
							if (victims != null && victims.Count > 0)
								target = victims[Util.Random(0, victims.Count - 1)];
							else
								target = puller;

							brain.AddToAggroList(target, 1);
							brain.AttackMostWanted();
							numAdds++;
						}
					}// foreach

					// Increase the range for finding friends to join the fight.
					range *= 2;
				} // while
			} // if (maxAdds > 0)
		} // BringFriends()

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
		public virtual bool CheckSpells(eCheckSpellType type)
		{
			if (Body.IsCasting)
				return true;

			bool casted = false;

			if (Body != null && Body.Spells != null && Body.Spells.Count > 0)
			{
				ArrayList spell_rec = new ArrayList();
				Spell spellToCast = null;
				bool needpet = false;
				bool needheal = false;

				if (type == eCheckSpellType.Defensive)
				{
					foreach (Spell spell in Body.Spells)
					{
						if (Body.GetSkillDisabledDuration(spell) > 0) continue;
						if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone") continue;
						// If we have no pets
						if (Body.ControlledBrain == null)
						{
							if (spell.SpellType.ToLower() == "pet") continue;
							if (spell.SpellType.ToLower().Contains("summon"))
							{
								spell_rec.Add(spell);
								needpet = true;
							}
						}
						if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null)
						{
							if (Util.Chance(30) && Body.ControlledBrain != null && spell.SpellType.ToLower() == "heal" &&
							    Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range &&
							    Body.ControlledBrain.Body.HealthPercent < DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD
							    && spell.Target.ToLower() != "self")
							{
								spell_rec.Add(spell);
								needheal = true;
							}
							if (LivingHasEffect(Body.ControlledBrain.Body, spell) && (spell.Target.ToLower() != "self")) continue;
						}
						if (!needpet && !needheal)
							spell_rec.Add(spell);
					}
					if (spell_rec.Count > 0)
					{
						spellToCast = (Spell)spell_rec[Util.Random((spell_rec.Count - 1))];
						if (!Body.IsReturningToSpawnPoint)
						{
							if (spellToCast.Uninterruptible && CheckDefensiveSpells(spellToCast))
								casted = true;
							else
								if (!Body.IsBeingInterrupted && CheckDefensiveSpells(spellToCast))
									casted = true;
						}
					}
				}
				else if (type == eCheckSpellType.Offensive)
				{
					foreach (Spell spell in Body.Spells)
					{

						if (Body.GetSkillDisabledDuration(spell) == 0)
						{
							if (spell.CastTime > 0)
							{
								if (spell.Target.ToLower() == "enemy" || spell.Target.ToLower() == "area" || spell.Target.ToLower() == "cone")
									spell_rec.Add(spell);
							}
						}
					}
					if (spell_rec.Count > 0)
					{
						spellToCast = (Spell)spell_rec[Util.Random((spell_rec.Count - 1))];


						if (spellToCast.Uninterruptible && CheckOffensiveSpells(spellToCast))
							casted = true;
						else
							if (!Body.IsBeingInterrupted && CheckOffensiveSpells(spellToCast))
								casted = true;
					}
				}

				return casted;
			}
			return casted;
		}

		/// <summary>
		/// Checks defensive spells.  Handles buffs, heals, etc.
		/// </summary>
		protected virtual bool CheckDefensiveSpells(Spell spell)
		{
			if (spell == null) return false;
			if (Body.GetSkillDisabledDuration(spell) > 0) return false;

			bool casted = false;

			// clear current target, set target based on spell type, cast spell, return target to original target
			GameObject lastTarget = Body.TargetObject;

			Body.TargetObject = null;
			switch (spell.SpellType.ToUpper())
			{
                #region Buffs
                case "AcuityBuff":
                case "AFHITSBUFF":
                case "ALLMAGICRESISTSBUFF":
                case "ARMORABSORPTIONBUFF":
                case "ARMORFACTORBUFF":
                case "BODYRESISTBUFF":
                case "BODYSPIRITENERGYBUFF":
                case "BUFF":
                case "CELERITYBUFF":
                case "COLDRESISTBUFF":
                case "COMBATSPEEDBUFF":
                case "CONSTITUTIONBUFF":
                case "COURAGEBUFF":
                case "CRUSHSLASHTHRUSTBUFF":
                case "DEXTERITYBUFF":
                case "DEXTERITYQUICKNESSBUFF":
                case "EFFECTIVENESSBUFF":
                case "ENDURANCEREGENBUFF":
                case "ENERGYRESISTBUFF":
                case "FATIGUECONSUMPTIONBUFF":
                case "FELXIBLESKILLBUFF":
                case "HASTEBUFF":
                case "HEALTHREGENBUFF":
                case "HEATCOLDMATTERBUFF":
                case "HEATRESISTBUFF":
                case "HEROISMBUFF":
                case "KEEPDAMAGEBUFF":
                case "MAGICRESISTSBUFF":
                case "MATTERRESISTBUFF":
                case "MELEEDAMAGEBUFF":
                case "MESMERIZEDURATIONBUFF":
                case "MLABSBUFF":
                case "PALADINARMORFACTORBUFF":
                case "PARRYBUFF":
                case "POWERHEALTHENDURANCEREGENBUFF":
                case "POWERREGENBUFF":
                case "SAVAGECOMBATSPEEDBUFF":
                case "SAVAGECRUSHRESISTANCEBUFF":
                case "SAVAGEDPSBUFF":
                case "SAVAGEPARRYBUFF":
                case "SAVAGESLASHRESISTANCEBUFF":
                case "SAVAGETHRUSTRESISTANCEBUFF":
                case "SPIRITRESISTBUFF":
                case "STRENGTHBUFF":
                case "STRENGTHCONSTITUTIONBUFF":
                case "SUPERIORCOURAGEBUFF":
                case "TOHITBUFF":
                case "WEAPONSKILLBUFF":
                case "DAMAGEADD":
                case "OFFENSIVEPROC":
                case "DEFENSIVEPROC":
                case "DAMAGESHIELD":
                    {
						// Buff self, if not in melee, but not each and every mob
						// at the same time, because it looks silly.
						if (!LivingHasEffect(Body, spell) && !Body.AttackState && Util.Chance(40) && spell.Target.ToLower() != "pet")
						{
							Body.TargetObject = Body;
							break;
						}
						if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Util.Chance(40) && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && !LivingHasEffect(Body.ControlledBrain.Body, spell) && spell.Target.ToLower() != "self")
						{
                            Body.TargetObject = Body.ControlledBrain.Body;
							break;
						}
						break;
					}
					#endregion Buffs

					#region Disease Cure/Poison Cure/Summon
				case "CUREDISEASE":
					if (Body.IsDiseased)
					{
						Body.TargetObject = Body;
						break;
					}
					if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && Body.ControlledBrain.Body.IsDiseased
					    && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && spell.Target.ToLower() != "self")
					{
						Body.TargetObject = Body.ControlledBrain.Body;
						break;
					}
					break;
				case "CUREPOISON":
					if (LivingIsPoisoned(Body))
					{
						Body.TargetObject = Body;
						break;
					}
					if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null && LivingIsPoisoned(Body.ControlledBrain.Body)
					    && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range && spell.Target.ToLower() != "self")
					{
						Body.TargetObject = Body.ControlledBrain.Body;
						break;
					}
					break;
				case "SUMMON":
					Body.TargetObject = Body;
					break;
				case "SUMMONMINION":
					//If the list is null, lets make sure it gets initialized!
					if (Body.ControlledNpcList == null)
						Body.InitControlledBrainArray(2);
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
                #endregion Disease Cure/Poison Cure/Summon

                #region Heals
                case "COMBATHEAL":
                case "HEAL":
                case "HEALOVERTIME":
                case "MERCHEAL":
                case "OMNIHEAL":
                case "PBAEHEAL":
                case "SPREADHEAL":
                    if (spell.Target.ToLower() == "self")
					{
						// if we have a self heal and health is less than 75% then heal, otherwise return false to try another spell or do nothing
						if (Body.HealthPercent < DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD)
						{
							Body.TargetObject = Body;
						}
						break;
					}

					// Chance to heal self when dropping below 30%, do NOT spam it.
					if (Body.HealthPercent < (DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD / 2.0)
						&& Util.Chance(10) && spell.Target.ToLower() != "pet")
					{
						Body.TargetObject = Body;
						break;
					}

					if (Body.ControlledBrain != null && Body.ControlledBrain.Body != null
					    && Body.GetDistanceTo(Body.ControlledBrain.Body) <= spell.Range 
					    && Body.ControlledBrain.Body.HealthPercent < DOL.GS.ServerProperties.Properties.NPC_HEAL_THRESHOLD 
					    && spell.Target.ToLower() != "self")
					{
						Body.TargetObject = Body.ControlledBrain.Body;
						break;
					}
					break;
					#endregion

					//case "SummonAnimistFnF":
					//case "SummonAnimistPet":
				case "SUMMONCOMMANDER":
				case "SUMMONDRUIDPET":
				case "SUMMONHUNTERPET":
				case "SUMMONNECROPET":
				case "SUMMONUNDERHILL":
				case "SUMMONSIMULACRUM":
				case "SUMMONSPIRITFIGHTER":
					//case "SummonTheurgistPet":
					if (Body.ControlledBrain != null)
						break;
					Body.TargetObject = Body;
					break;

				default:
					//log.Warn($"CheckDefensiveSpells() encountered an unknown spell type [{spell.SpellType}]");
					break;
			}

			if (Body.TargetObject != null && (spell.Duration == 0 || (Body.TargetObject is GameLiving living && LivingHasEffect(living, spell) == false)))
            {
				casted = Body.CastSpell(spell, m_mobSpellLine);

				if (casted && spell.CastTime > 0)
				{
					if (Body.IsMoving)
						Body.StopFollowing();

					if (Body.TargetObject != Body)
						Body.TurnTo(Body.TargetObject);
				}
			}

			Body.TargetObject = lastTarget;

			return casted;
		}

		/// <summary>
		/// Checks offensive spells.  Handles dds, debuffs, etc.
		/// </summary>
		protected virtual bool CheckOffensiveSpells(Spell spell)
		{
			if (spell.Target.ToLower() != "enemy" && spell.Target.ToLower() != "area" && spell.Target.ToLower() != "cone")
				return false;

			bool casted = false;

			if (Body.TargetObject is GameLiving living && (spell.Duration == 0 || (!living.HasEffect(spell) || spell.SpellType.ToUpper() == "DIRECTDAMAGEWITHDEBUFF")))
            {
				// Offensive spells require the caster to be facing the target
				if (Body.TargetObject != Body)
					Body.TurnTo(Body.TargetObject);

				casted = Body.CastSpell(spell, m_mobSpellLine);

				if (casted && spell.CastTime > 0 && Body.IsMoving)
					Body.StopFollowing();
			}
			return casted;
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
				case "StrengthConstitutionDebuff":
				case "CombatSpeedDebuff":
				case "DamageOverTime":
				case "MeleeDamageDebuff":
				case "AllStatsPercentDebuff":
				case "CrushSlashThrustDebuff":
				case "EffectivenessDebuff":
				case "Disease":
				case "Stun":
				case "Mez":
				case "Taunt":
					if (!LivingHasEffect(lastTarget as GameLiving, spell))
					{
						Body.TargetObject = lastTarget;
					}
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
				case "OffensiveProc":
					if (!LivingHasEffect(Body, spell))
					{
						Body.TargetObject = Body;
					}
					break;
					#endregion
			}

			if (Body.TargetObject != null && (spell.Duration == 0 || (Body.TargetObject is GameLiving living && LivingHasEffect(living, spell) == false)))
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
		public static bool LivingHasEffect(GameLiving target, Spell spell)
		{
			if (target == null)
				return true;

			if (target is GamePlayer && (target as GamePlayer).CharacterClass.ID == (int)eCharacterClass.Vampiir)
			{
				switch (spell.SpellType)
				{
					case "StrengthConstitutionBuff":
					case "DexterityQuicknessBuff":
					case "StrengthBuff":
					case "DexterityBuff":
					case "ConstitutionBuff":
					case "AcuityBuff":

						return true;
				}
			}

			lock (target.EffectList)
			{
				//Check through each effect in the target's effect list
				foreach (IGameEffect effect in target.EffectList)
				{
					//If the effect we are checking is not a gamespelleffect keep going
					if (effect is GameSpellEffect == false)
						continue;

					GameSpellEffect speffect = effect as GameSpellEffect;

					//if the effect effectgroup is the same as the checking spells effectgroup then these are considered the same
					if (speffect.Spell.EffectGroup == spell.EffectGroup)
						return true;
				}
			}

			//the answer is no, the effect has not been found
			return false;
		}

		protected bool LivingIsPoisoned(GameLiving target)
		{
			foreach (IGameEffect effect in target.EffectList)
			{
				//If the effect we are checking is not a gamespelleffect keep going
				if (effect is GameSpellEffect == false)
					continue;

				GameSpellEffect speffect = effect as GameSpellEffect;

				// if this is a DOT then target is poisoned
				if (speffect.Spell.SpellType == "DamageOverTime")
					return true;
			}

			return false;
		}


		#endregion

		#region Random Walk
		public virtual bool CanRandomWalk
		{
			get
			{
				/* Roaming:
				   <0 means random range
				   0 means no roaming
				   >0 means range of roaming
				   defaut roaming range is defined in CanRandomWalk method
				 */
				if (!DOL.GS.ServerProperties.Properties.ALLOW_ROAM)
					return false;
				if (Body.RoamingRange == 0)
					return false;
				if (!string.IsNullOrWhiteSpace(Body.PathID))
					return false;
				return true;
			}
		}

		public virtual IPoint3D CalcRandomWalkTarget()
		{
			int maxRoamingRadius = Body.CurrentRegion.IsDungeon ? 5 : 500;

			if (Body.RoamingRange > 0)
				maxRoamingRadius = Body.RoamingRange;

			double targetX = Body.SpawnPoint.X + Util.Random( -maxRoamingRadius, maxRoamingRadius);
			double targetY = Body.SpawnPoint.Y + Util.Random( -maxRoamingRadius, maxRoamingRadius);

			return new Point3D( (int)targetX, (int)targetY, Body.SpawnPoint.Z );
		}

		#endregion
		#region DetectDoor
		public virtual void DetectDoor()
		{
			ushort range= (ushort)((ThinkInterval/800)*Body.CurrentWayPoint.MaxSpeed);
			
			foreach (IDoor door in Body.CurrentRegion.GetDoorsInRadius(Body.X, Body.Y, Body.Z, range, false))
			{
				if (door is GameKeepDoor)
				{
					if (Body.Realm != door.Realm) return;
					door.Open();
					//Body.Say("GameKeep Door is near by");
					//somebody can insert here another action for GameKeep Doors
					return;
				}
				else
				{
					door.Open();
					return ;
				}
			}
			return;
		}
		#endregion
	}
}
