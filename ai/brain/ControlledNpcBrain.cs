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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Events;
using DOL.GS;
using DOL.GS.Spells;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using log4net;

namespace DOL.AI.Brain
{
	/// <summary>
	/// A brain that can be controlled
	/// </summary>
	public class ControlledNpcBrain : StandardMobBrain, IControlledBrain
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		// note that a minimum distance is inforced in GameNPC
		public static readonly short MIN_OWNER_FOLLOW_DIST = 50;
		//4000 - rough guess, needs to be confirmed
		public static readonly short MAX_OWNER_FOLLOW_DIST = 5000; // setting this to max stick distance
		public static readonly short MIN_ENEMY_FOLLOW_DIST = 90;
		public static readonly short MAX_ENEMY_FOLLOW_DIST = 512;

		private int m_tempX = 0;
		private int m_tempY = 0;
		private int m_tempZ = 0;

		/// <summary>
		/// Holds the controlling player of this brain
		/// </summary>
		readonly GameLiving m_owner;

		/// <summary>
		/// Holds the walk state of the brain
		/// </summary>
		private eWalkState m_walkState;

		/// <summary>
		/// Holds the aggression level of the brain
		/// </summary>
		private eAggressionState m_aggressionState;

		/// <summary>
		/// Constructs new controlled npc brain
		/// </summary>
		/// <param name="owner"></param>
		public ControlledNpcBrain(GameLiving owner)
			: base()
		{
			if (owner == null)
				throw new ArgumentNullException("owner");

			m_owner = owner;
			m_aggressionState = eAggressionState.Defensive;
			m_walkState = eWalkState.Follow;
			m_aggroLevel = 99;
			m_aggroMaxRange = 1500;
		}

		private bool m_ismainpet = true;

		/// <summary>
		/// Checks if this NPC is a permanent/charmed or timed pet
		/// </summary>
		public bool IsMainPet
		{
			get { return m_ismainpet; }
			set { m_ismainpet = value; }
		}

		/// <summary>
		/// The number of seconds/10 this brain will stay active even when no player is close
		/// Overriden. Returns int.MaxValue
		/// </summary>
		protected override int NoPlayersStopDelay
		{
			get { return int.MaxValue; }
		}

		/// <summary>
		/// The interval for thinking, 1.5 seconds
		/// </summary>
		public override int ThinkInterval
		{
			get { return 1500; }
		}

		#region Control

		/// <summary>
		/// Gets the controlling owner of the brain
		/// </summary>
		public GameLiving Owner
		{
			get { return m_owner; }
		}

		/// <summary>
		/// Find the player owner of the pets at the top of the tree
		/// </summary>
		/// <returns>Player owner at the top of the tree.  If there was no player, then return null.</returns>
		public virtual GamePlayer GetPlayerOwner()
		{
			GameLiving owner = Owner;
			int i = 0;
			while (owner is GameNPC && owner != null)
			{
				i++;
				if (i > 50)
					throw new Exception("GetPlayerOwner() from " + Owner.Name + "caused a cyclical loop.");
				//If this is a pet, get its owner
				if (((GameNPC)owner).Brain is IControlledBrain)
					owner = ((IControlledBrain)((GameNPC)owner).Brain).Owner;
				//This isn't a pet, that means it's at the top of the tree.  This case will only happen if
				//owner is not a GamePlayer
				else
					break;
			}
			//Return if we found the gameplayer
			if (owner is GamePlayer)
				return (GamePlayer)owner;
			//If the root owner was not a player or npc then make sure we know that something went wrong!
			if (!(owner is GameNPC))
				throw new Exception("Unrecognized owner: " + owner.GetType().FullName);
			//No GamePlayer at the top of the tree
			return null;
		}

		/// <summary>
		/// Gets or sets the walk state of the brain
		/// </summary>
		public virtual eWalkState WalkState
		{
			get { return m_walkState; }
			set
			{
				m_walkState = value;
				UpdatePetWindow();
			}
		}

		/// <summary>
		/// Gets or sets the aggression state of the brain
		/// </summary>
		public virtual eAggressionState AggressionState
		{
			get { return m_aggressionState; }
			set
			{
				m_aggressionState = value;
				m_orderAttackTarget = null;
				if (m_aggressionState == eAggressionState.Passive)
				{
					ClearAggroList();
					Body.StopAttack();
					Body.TargetObject = null;
					if (WalkState == eWalkState.Follow)
						FollowOwner();
					else if (m_tempX > 0 && m_tempY > 0 && m_tempZ > 0)
						Body.WalkTo(m_tempX, m_tempY, m_tempZ, Body.MaxSpeed);
				}
				AttackMostWanted();
			}
		}

		/// <summary>
		/// Attack the target on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Attack(GameObject target)
		{
			if (AggressionState == eAggressionState.Passive)
			{
				AggressionState = eAggressionState.Defensive;
				UpdatePetWindow();
			}
			m_orderAttackTarget = target as GameLiving;

			AttackMostWanted();
		}

		/// <summary>
		/// Follow the target on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Follow(GameObject target)
		{
			WalkState = eWalkState.Follow;
			Body.Follow(target, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		/// <summary>
		/// Stay at current position on command
		/// </summary>
		public virtual void Stay()
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.Stay;
			Body.StopFollowing();
		}

		/// <summary>
		/// Go to owner on command
		/// </summary>
		public virtual void ComeHere()
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.ComeHere;
			Body.StopFollowing();
			Body.WalkTo(Owner, Body.MaxSpeed);
		}

		/// <summary>
		/// Go to targets location on command
		/// </summary>
		/// <param name="target"></param>
		public virtual void Goto(GameObject target)
		{
			m_tempX = Body.X;
			m_tempY = Body.Y;
			m_tempZ = Body.Z;
			WalkState = eWalkState.GoTarget;
			Body.StopFollowing();
			Body.WalkTo(target, Body.MaxSpeed);
		}

		public virtual void SetAggressionState(eAggressionState state)
		{
			AggressionState = state;
			UpdatePetWindow();
		}

		/// <summary>
		/// Updates the pet window
		/// </summary>
		public virtual void UpdatePetWindow()
		{
			if (m_owner is GamePlayer)
				((GamePlayer)m_owner).Out.SendPetWindow(m_body, ePetWindowAction.Update, m_aggressionState, m_walkState);
		}

		/// <summary>
		/// Start following the owner
		/// </summary>
		public virtual void FollowOwner()
		{
			Body.StopAttack();
			if (Owner is GamePlayer
			    && IsMainPet
			    && ((GamePlayer)Owner).CharacterClass.ID != (int)eCharacterClass.Animist
			    && ((GamePlayer)Owner).CharacterClass.ID != (int)eCharacterClass.Theurgist)
				Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
			else if (Owner is GameNPC)
				Body.Follow(Owner, MIN_OWNER_FOLLOW_DIST, MAX_OWNER_FOLLOW_DIST);
		}

		#endregion

		#region AI

		/// <summary>
		/// The attack target ordered by the owner
		/// </summary>
		protected GameLiving m_orderAttackTarget;

		/// <summary>
		/// Starts the brain thinking and resets the inactivity countdown
		/// </summary>
		/// <returns>true if started</returns>
		public override bool Start()
		{
			if (!base.Start()) return false;
			if (WalkState == eWalkState.Follow)
				FollowOwner();
			// [Ganrod] On supprime la cible du pet au moment  du contrôle.
			Body.TargetObject = null;
			GameEventMgr.AddHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));

			return true;
		}

		/// <summary>
		/// Stops the brain thinking
		/// </summary>
		/// <returns>true if stopped</returns>
		public override bool Stop()
		{
			if (!base.Stop()) return false;
			GameEventMgr.RemoveHandler(Owner, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(OnOwnerAttacked));

			GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
			return true;
		}

		/// <summary>
		/// Do the mob AI
		/// </summary>
		public override void Think()
		{
			GamePlayer playerowner = GetPlayerOwner();
			
			if (!playerowner.CurrentUpdateArray[Body.ObjectID - 1])
			{
				playerowner.Out.SendObjectUpdate(Body);
				playerowner.CurrentUpdateArray[Body.ObjectID - 1] = true;
			}

			//See if the pet is too far away, if so release it!
			if (Owner is GamePlayer && IsMainPet)
			{
				if (!Body.IsWithinRadius(Owner, MAX_OWNER_FOLLOW_DIST))
					(Owner as GamePlayer).CommandNpcRelease();
			}

			//Check for buffs, heals, etc
			if (Owner is GameNPC ||
			    (Owner is GamePlayer && ((WalkState == eWalkState.ComeHere && AggressionState != eAggressionState.Aggressive) || WalkState == eWalkState.Follow)))
			{
				CheckSpells(eCheckSpellType.Defensive);
			}

			if (AggressionState == eAggressionState.Aggressive)
			{
				CheckPlayerAggro();
				CheckNPCAggro();
				AttackMostWanted();
			}
			// Do not discover stealthed players
			if ( Body.TargetObject != null)
				if (Body.TargetObject is GamePlayer)
					if (Body.IsAttacking && (Body.TargetObject as GamePlayer).IsStealthed)
					{
						Body.StopAttack();
						FollowOwner();
					}
		}

		/// <summary>
		/// Checks the Abilities
		/// </summary>
		public override void CheckAbilities()
		{
			////load up abilities
			if (Body.Abilities != null && Body.Abilities.Count > 0)
			{
				foreach (Ability ab in Body.Abilities.Values)
				{
					switch (ab.KeyName)
					{
						case GS.Abilities.Intercept:
							{
								GamePlayer player = Owner as GamePlayer;
								//the pet should intercept even if a player is till intercepting for the owner
								new InterceptEffect().Start(Body, player);
								break;
							}
						case GS.Abilities.Guard:
							{
								GamePlayer player = Owner as GamePlayer;
								new GuardEffect().Start(Body, player);
								break;
							}
						case Abilities.ChargeAbility:
							{
								if ( !Body.IsWithinRadius( Body.TargetObject, 500 ) )
								{
									ChargeAbility charge = Body.GetAbility(typeof(ChargeAbility)) as ChargeAbility;
									if (charge != null && Body.GetSkillDisabledDuration(charge) <= 0)
									{
										charge.Execute(Body);
									}
								}
								break;
							}
					}
				}
			}
		}
		
		public override bool CheckSpells(eCheckSpellType type)
		{
			if (Body == null || Body.Spells == null || Body.Spells.Count < 1)
				return false;
			
			if (Body.IsCasting)
				return true;
			
			bool casted = false;
			if (type == eCheckSpellType.Defensive)
			{
				foreach (Spell spell in Body.Spells)
				{
					if (!Body.IsBeingInterrupted && Body.GetSkillDisabledDuration(spell) == 0 && CheckDefensiveSpells(spell))
					{
						casted = true;
						break;
					}
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
							if (!Body.IsBeingInterrupted && CheckOffensiveSpells(spell))
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

		/// <summary>
		/// Checks the Positive Spells.  Handles buffs, heals, etc.
		/// </summary>
		protected override bool CheckDefensiveSpells(Spell spell)
		{
			GameObject lastTarget = Body.TargetObject;
			Body.TargetObject = null;
			GamePlayer player = null;
			GameLiving owner = null;

			switch (spell.SpellType)
			{
					#region Buffs
				case "StrengthConstitutionBuff":
				case "DexterityQuicknessBuff":
				case "StrengthBuff":
				case "DexterityBuff":
				case "ConstitutionBuff":
				case "ArmorFactorBuff":
				case "ArmorAbsorptionBuff":
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

						if (spell.Target == "Realm")
						{
							owner = (this as IControlledBrain).Owner;
							player = null;
							//Buff owner
							if (!LivingHasEffect(owner, spell))
							{
								Body.TargetObject = owner;
								break;
							}

							if (owner is GameNPC)
							{
								//Buff other minions
								foreach (IControlledBrain icb in ((GameNPC)owner).ControlledNpcList)
								{
									if (icb == null)
										continue;
									if (!LivingHasEffect(icb.Body, spell))
									{
										Body.TargetObject = icb.Body;
										break;
									}
								}
							}
							player = GetPlayerOwner();

							//Buff player
							if (player != null)
							{
								if (!LivingHasEffect(player, spell))
								{
									Body.TargetObject = player;
									break;
								}
							}
						}
					}
					break;
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
					#endregion

					#region Heals
				case "Heal":
					//Heal self
					if (Body.HealthPercent < 75)
					{
						Body.TargetObject = Body;
						break;
					}

					//Heal owner
					owner = (this as IControlledBrain).Owner;
					if (owner.HealthPercent < 75)
					{
						Body.TargetObject = owner;
						break;
					}

					player = GetPlayerOwner();

					if (player.Group != null && player.CharacterClass.ID == (int)eCharacterClass.Enchanter)
					{
						foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
						{
							if (p.HealthPercent < 75)
							{
								Body.TargetObject = p;
								break;
							}
						}
					}
					break;
					#endregion
			}

			if (Body.TargetObject != null)
			{
				if (Body.IsMoving)
					Body.StopFollowing();

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
		/// Lost follow target event
		/// </summary>
		/// <param name="target"></param>
		protected override void OnFollowLostTarget(GameObject target)
		{
			if (target == Owner)
			{
				GameEventMgr.Notify(GameLivingEvent.PetReleased, Body);
				return;
			}

			FollowOwner();
		}

		/// <summary>
		/// Add living to the aggrolist
		/// aggroamount can be negative to lower amount of aggro
		/// </summary>
		/// <param name="living"></param>
		/// <param name="aggroamount"></param>
		public override void AddToAggroList(GameLiving living, int aggroamount)
		{
			if (living == Owner)
				return;
			if (living.IsStealthed) return;
			base.AddToAggroList(living, aggroamount);
		}

		public override int CalculateAggroLevelToTarget(GameLiving target)
		{
			// only attack if target is green+ to OWNER; always attack higher levels regardless of CON
			if (GameServer.ServerRules.IsSameRealm(Body, target, true) || Owner.IsObjectGreyCon(target)) return 0;
			return AggroLevel > 100 ? 100 : AggroLevel;
		}

		/// <summary>
		/// Returns the best target to attack
		/// </summary>
		/// <returns>the best target</returns>
		protected override GameLiving CalculateNextAttackTarget()
		{
			if (AggressionState == eAggressionState.Passive)
				return null;
			if (m_orderAttackTarget != null)
			{
				if (m_orderAttackTarget.IsAlive && m_orderAttackTarget.ObjectState == GameObject.eObjectState.Active && !m_orderAttackTarget.IsStealthed)
					return m_orderAttackTarget;
				m_orderAttackTarget = null;
			}

			//VaNaTiC->
			lock (m_aggroTable.SyncRoot)
			{
				IDictionaryEnumerator aggros = m_aggroTable.GetEnumerator();
				List<GameLiving> removable = new List<GameLiving>();
				while (aggros.MoveNext())
				{
					GameLiving living = (GameLiving)aggros.Key;
					// 1st we check only if living is mezzed,
					// cause if so there is no need to look
					// through all effects for a root
					if (living.IsMezzed || living.IsStealthed)
					{
						removable.Add(living);
					}
					else
					{
						// no mezz was found, then we have to look if living is rooted
						GameSpellEffect root = SpellHandler.FindEffectOnTarget(living, "SpeedDecrease");
						if (root != null && root.Spell.Value == 99)
						{
							removable.Add(living);
						}
					}
				}
				foreach (GameLiving living in removable)
				{
					RemoveFromAggroList(living);
				}
			}
			//VaNaTiC<-

			return base.CalculateNextAttackTarget();
		}

		/// <summary>
		/// Selects and attacks the next target or does nothing
		/// </summary>
		protected override void AttackMostWanted()
		{
			if (!IsActive) return;
			GameLiving target = CalculateNextAttackTarget();

			if (target != null)
			{
				//if (!Body.AttackState || target != Body.TargetObject)
				if (!Body.IsAttacking || target != Body.TargetObject)
				{
					Body.TargetObject = target;

					if (target is GamePlayer)
					{
						Body.LastAttackTickPvP = Body.CurrentRegion.Time;
						Owner.LastAttackedByEnemyTickPvP = Body.CurrentRegion.Time;
					}
					else
					{
						Body.LastAttackTickPvE = Body.CurrentRegion.Time;
						Owner.LastAttackedByEnemyTickPvE = Body.CurrentRegion.Time;
					}

					List<GameSpellEffect> effects = new List<GameSpellEffect>();

					lock (Body.EffectList)
					{
						foreach (IGameEffect effect in Body.EffectList)
						{
							if (effect is GameSpellEffect && (effect as GameSpellEffect).SpellHandler is SpeedEnhancementSpellHandler)
							{
								effects.Add(effect as GameSpellEffect);
							}
						}
					}

					lock (Owner.EffectList)
					{
						foreach (IGameEffect effect in Owner.EffectList)
						{
							if (effect is GameSpellEffect && (effect as GameSpellEffect).SpellHandler is SpeedEnhancementSpellHandler)
							{
								effects.Add(effect as GameSpellEffect);
							}
						}
					}

					foreach (GameSpellEffect effect in effects)
					{
						effect.Cancel(false);
					}

					if (!CheckSpells(eCheckSpellType.Offensive))
					{
						Body.StartAttack(target);
					}
				}
			}
			else
			{
				//if (Body.AttackState)
				if (Body.IsAttacking)
					Body.StopAttack();

				if (Body.SpellTimer != null && Body.SpellTimer.IsAlive)
					Body.SpellTimer.Stop();

				if (WalkState == eWalkState.Follow)
					FollowOwner();
				else if (m_tempX > 0 && m_tempY > 0 && m_tempZ > 0)
					Body.WalkTo(m_tempX, m_tempY, m_tempZ, Body.MaxSpeed);
			}
		}

		/// <summary>
		/// Owner attacked event
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnOwnerAttacked(DOLEvent e, object sender, EventArgs arguments)
		{
			// theurgist pets don't help their owner
			//edit for BD - possibly add support for Theurgist GameNPCs
			if (Owner is GamePlayer && ((GamePlayer)Owner).CharacterClass.ID == (int)eCharacterClass.Theurgist)
				return;

			AttackedByEnemyEventArgs args = arguments as AttackedByEnemyEventArgs;
			if (args == null) return;
			if (args.AttackData.Target is GamePlayer && (args.AttackData.Target as GamePlayer).ControlledNpcBrain != this)
				return;
			// react only on these attack results
			switch (args.AttackData.AttackResult)
			{
				case GameLiving.eAttackResult.Blocked:
				case GameLiving.eAttackResult.Evaded:
				case GameLiving.eAttackResult.Fumbled:
				case GameLiving.eAttackResult.HitStyle:
				case GameLiving.eAttackResult.HitUnstyled:
				case GameLiving.eAttackResult.Missed:
				case GameLiving.eAttackResult.Parried:
					AddToAggroList(args.AttackData.Attacker, args.AttackData.Attacker.EffectiveLevel + args.AttackData.Damage + args.AttackData.CriticalDamage);
					break;
			}
			AttackMostWanted();
		}

		protected override void BringFriends(AttackData ad)
		{
			// don't
		}

		public override bool CheckFormation(ref int x, ref int y, ref int z) { return false; }

		#endregion
	}
}
