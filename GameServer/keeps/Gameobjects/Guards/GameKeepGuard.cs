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
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.GS.ServerProperties;
using System.Collections.Generic;
using DOL.GS.Geometry;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Keep guard is gamemob with just different brain and load from other DB table
	/// </summary>
	public class GameKeepGuard : GameNPC, IKeepItem
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private Patrol m_Patrol = null;
		public Patrol PatrolGroup
		{
			get { return m_Patrol; }
			set { m_Patrol = value; }
		}

		private string m_templateID = "";
		public string TemplateID
		{
			get { return m_templateID; }
			set { m_templateID = value; }
		}

		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
			set { m_component = value; }
		}

		private DBKeepPosition m_position;
		public DBKeepPosition DbKeepPosition
		{
			get { return m_position; }
			set { m_position = value; }
		}

		private GameKeepHookPoint m_hookPoint;
		public GameKeepHookPoint HookPoint
		{
			get { return m_hookPoint; }
			set { m_hookPoint = value; }
		}

		private eRealm m_modelRealm = eRealm.None;
		public eRealm ModelRealm
		{
			get { return m_modelRealm; }
			set { m_modelRealm = value; }
		}

		public bool IsTowerGuard
		{
			get
			{
				if (Component != null && Component.Keep != null)
				{
					return Component.Keep is GameKeepTower;
				}
				return false;
			}
		}

		public bool IsPortalKeepGuard
		{
			get
			{
				if (Component == null || Component.Keep == null)
					return false;
				return Component.Keep.IsPortalKeep;
			}
		}

		/// <summary>
		/// We do this because if we set level when a guard is waiting to respawn,
		/// the guard will never respawn because the guard is given full health and
		/// is then considered alive
		/// </summary>
		public override byte Level
		{
			get
			{
				if (IsPortalKeepGuard)
					return 255;

				return base.Level;
			}
			set
			{
				if (this.IsRespawning)
					m_level = value;
				else
					base.Level = value;
			}
		}

		/// <summary>
		/// Guards always have Mana to cast spells
		/// </summary>
		public override int Mana
		{
			get { return 50000; }
		}

		public override int MaxHealth
		{
			get { return GetModified(eProperty.MaxHealth) + (base.Level * 4); }
		}

		private bool m_changingPositions = false;

		public GameLiving HealTarget = null;

		/// <summary>
		/// The keep lord is under attack, go help them
		/// </summary>
		/// <param name="lord"></param>
		/// <returns>Whether or not we are responding</returns>
		public virtual bool AssistLord(GuardLord lord)
		{
			Follow(lord, GameNPC.STICKMINIMUMRANGE, int.MaxValue);
			return true;
		}

		#region Combat

		/// <summary>
		/// Here we set the speeds we want our guards to have, this affects weapon damage
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override int AttackSpeed(params InventoryItem[] weapon)
		{
			//speed 1 second = 10
			int speed = 0;
			switch (ActiveWeaponSlot)
			{
				case eActiveWeaponSlot.Distance: speed = 45; break;
				case eActiveWeaponSlot.TwoHanded: speed = 40; break;
				default: speed = 24; break;
			}
			speed = speed + Util.Random(11);
			return speed * 100;
		}

		/// <summary>
		/// When moving guards have difficulty attacking players, so we double there attack range)
		/// </summary>
		public override int AttackRange
		{
			get
			{
				int range = base.AttackRange;
				if (IsMoving && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
					range *= 2;
				return range;
			}
			set
			{
				base.AttackRange = value;
			}
		}

		/// <summary>
		/// The distance attack range
		/// </summary>
		public virtual int AttackRangeDistance
		{
			get
			{
				return 0;
			}
		}

		/// <summary>
		/// We need an event after an attack is finished so we know when players are unreachable by archery
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		public static void AttackFinished(DOLEvent e, object sender, EventArgs arguments)
		{
			GameKeepGuard guard = sender as GameKeepGuard;
			if (guard.TargetObject == null)
				return;
			if (!guard.AttackState)
				return;
			if (guard is GuardArcher == false && guard is GuardLord == false && guard is GuardCaster == false)
				return;

			AttackFinishedEventArgs afargs = arguments as AttackFinishedEventArgs;

			if (guard.ActiveWeaponSlot != eActiveWeaponSlot.Distance && !guard.IsMoving)
			{
				eAttackResult result = afargs.AttackData.AttackResult;
				if (result == eAttackResult.OutOfRange)
				{
					guard.StopAttack();
					lock (guard.Attackers)
					{
						foreach (GameLiving living in guard.Attackers)
						{
							if (guard.IsWithinRadius(living, guard.AttackRange))
							{
								guard.StartAttack(living);
								return;
							}
						}
					}

					if (guard.IsWithinRadius(guard.TargetObject, guard.AttackRangeDistance))
					{
						if (guard.MaxSpeedBase == 0 || (guard is GuardArcher && !guard.BeenAttackedRecently))
							guard.SwitchToRanged(guard.TargetObject);
					}
				}
				return;
			}

			if (guard.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				if (GameServer.ServerRules.IsAllowedToAttack(guard, guard.TargetObject as GameLiving, true) == false)
				{
					guard.StopAttack();
					return;
				}
				if (!guard.IsWithinRadius(guard.TargetObject, guard.AttackRange))
				{
					guard.StopAttack();
					return;
				}
			}

			GamePlayer player = null;

			if (guard.TargetObject is GamePlayer)
			{
				player = guard.TargetObject as GamePlayer;
			}
			else if (guard.TargetObject is GameNPC)
			{
				GameNPC npc = (guard.TargetObject as GameNPC);

				if (npc.Brain != null && ((npc is GameKeepGuard) == false) && npc.Brain is IControlledBrain)
				{
					player = (npc.Brain as IControlledBrain).GetPlayerOwner();
				}
			}

			if (player != null)
			{
				player.Out.SendCheckLOS(guard, guard.TargetObject, new CheckLOSResponse(guard.GuardStopAttackCheckLOS));
			}
		}

		/// <summary>
		/// Override for StartAttack which chooses Ranged or Melee attack
		/// </summary>
		/// <param name="attackTarget"></param>
		public override void StartAttack(GameObject attackTarget)
		{
			if (IsPortalKeepGuard)
			{
				base.StartAttack(attackTarget);
				return;
			}

			if (AttackState || CurrentSpellHandler != null)
				return;

			if (attackTarget is GameLiving == false)
				return;
			GameLiving target = attackTarget as GameLiving;
			if (target == null || target.IsAlive == false)
				return;

			//we dont send LOS checks for people we cant attack
			if (!GameServer.ServerRules.IsAllowedToAttack(this, target, true))
				return;

			//Prevent spam for LOS to same target multiple times

			GameObject lastTarget = (GameObject)this.TempProperties.getProperty<object>(LAST_LOS_TARGET_PROPERTY, null);
			long lastTick = this.TempProperties.getProperty<long>(LAST_LOS_TICK_PROPERTY);

			if (lastTarget != null && lastTarget == attackTarget)
			{
				if (lastTick != 0 && CurrentRegion.Time - lastTick < ServerProperties.Properties.KEEP_GUARD_LOS_CHECK_TIME * 1000)
					return;
			}

			GamePlayer LOSChecker = null;
			if (attackTarget is GamePlayer)
			{
				LOSChecker = attackTarget as GamePlayer;
			}
			else if (attackTarget is GameNPC && (attackTarget as GameNPC).Brain is IControlledBrain)
			{
				LOSChecker = ((attackTarget as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
			}
			else
			{
				// try to find another player to use for checking line of site
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					LOSChecker = player;
					break;
				}
			}

			if (LOSChecker == null)
			{
				return;
			}

			lock (LOS_LOCK)
			{
				int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);

				if (count > 10)
				{
					log.DebugFormat("{0} LOS count check exceeds 10, aborting LOS check!", Name);

					// Now do a safety check.  If it's been a while since we sent any check we should clear count
					if (lastTick == 0 || CurrentRegion.Time - lastTick > ServerProperties.Properties.LOS_PLAYER_CHECK_FREQUENCY * 1000)
					{
						log.Debug("LOS count reset!");
						TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, 0);
					}

					return;
				}

				count++;
				TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, count);

				TempProperties.setProperty(LAST_LOS_TARGET_PROPERTY, attackTarget);
				TempProperties.setProperty(LAST_LOS_TICK_PROPERTY, CurrentRegion.Time);
				TargetObject = attackTarget;
			}

			LOSChecker.Out.SendCheckLOS(this, attackTarget, new CheckLOSResponse(this.GuardStartAttackCheckLOS));
		}

		/// <summary>
		/// We only attack if we have LOS
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		/// <param name="targetOID"></param>
		public void GuardStartAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			lock (LOS_LOCK)
			{
				int count = TempProperties.getProperty<int>(NUM_LOS_CHECKS_INPROGRESS, 0);
				count--;
				TempProperties.setProperty(NUM_LOS_CHECKS_INPROGRESS, Math.Max(0, count));
			}

			if ((response & 0x100) == 0x100)
			{
				if (this is GuardArcher || this is GuardLord)
				{
					if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
					{
						if (CanUseRanged)
							SwitchToRanged(TargetObject);
					}
				}

				base.StartAttack(TargetObject);
			}
			else if (TargetObject != null && TargetObject is GameLiving)
			{
				(this.Brain as KeepGuardBrain).RemoveFromAggroList(TargetObject as GameLiving);
			}
		}

		/// <summary>
		/// If we don't have LOS we stop attack
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		/// <param name="targetOID"></param>
		public void GuardStopAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) != 0x100)
			{
				StopAttack();

				if (TargetObject != null && TargetObject is GameLiving)
				{
					(this.Brain as KeepGuardBrain).RemoveFromAggroList(TargetObject as GameLiving);
				}
			}
		}

		public void GuardStartSpellHealCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100 && HealTarget != null)
			{
				Spell healSpell = GetGuardHealSmallSpell(Realm);

				if (healSpell != null && !IsStunned && !IsMezzed)
				{
					StopAttack();
					TargetObject = HealTarget;
					CastSpell(healSpell, GuardSpellLine);
				}
			}
		}

		private Spell GetGuardHealSmallSpell(eRealm realm)
		{
			switch (realm)
			{
				case eRealm.None:
				case eRealm.Albion:
					return GuardSpellDB.AlbGuardHealSmallSpell;
				case eRealm.Midgard:
					return GuardSpellDB.MidGuardHealSmallSpell;
				case eRealm.Hibernia:
					return GuardSpellDB.HibGuardHealSmallSpell;
			}
			return null;
		}

		public void CheckAreaForHeals()
		{
			GameLiving target = null;
			GamePlayer LOSChecker = null;

			foreach (GamePlayer player in GetPlayersInRadius(2000))
			{
				LOSChecker = player;

				if (!player.IsAlive) continue;
				if (GameServer.ServerRules.IsSameRealm(player, this, true))
				{
					if (player.HealthPercent < Properties.KEEP_HEAL_THRESHOLD)
					{
						target = player;
						break;
					}
				}
			}

			if (target == null)
			{
				foreach (GameNPC npc in GetNPCsInRadius(2000))
				{
					if (npc is GameSiegeWeapon) continue;
					if (GameServer.ServerRules.IsSameRealm(npc, this, true))
					{
						if (npc.HealthPercent < Properties.KEEP_HEAL_THRESHOLD)
						{
							target = npc;
							break;
						}
					}
				}
			}

			if (target != null)
			{
				if (LOSChecker == null)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						LOSChecker = player;
						break;
					}
				}
				if (LOSChecker == null)
					return;
				if (!target.IsAlive) return;

				HealTarget = target;
				LOSChecker.Out.SendCheckLOS(this, target, new CheckLOSResponse(GuardStartSpellHealCheckLOS));
			}
		}

		public void CheckForNuke()
		{
			GameLiving target = TargetObject as GameLiving;
			if (target == null) return;
			if (!target.IsAlive) return;
			if (target is GamePlayer && !GameServer.KeepManager.IsEnemy(this, target as GamePlayer, true)) return;
			if (!IsWithinRadius(target, WorldMgr.VISIBILITY_DISTANCE)) { TargetObject = null; return; }
			GamePlayer LOSChecker = null;
			if (target is GamePlayer) LOSChecker = target as GamePlayer;
			else
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					LOSChecker = player;
					break;
				}
			}
			if (LOSChecker == null) return;
			LOSChecker.Out.SendCheckLOS(this, target, new CheckLOSResponse(GuardStartSpellNukeCheckLOS));
		}

		public void GuardStartSpellNukeCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				switch (Realm)
				{
					case eRealm.None:
					case eRealm.Albion: LaunchSpell(47, "Pyromancy"); break;
					case eRealm.Midgard: LaunchSpell(48, "Runecarving"); break;
					case eRealm.Hibernia: LaunchSpell(47, "Way of the Eclipse"); break;
				}
			}
		}

		private void LaunchSpell(int spellLevel, string spellLineName)
		{
			if (TargetObject == null)
				return;

			Spell castSpell = null;

			SpellLine castLine = SkillBase.GetSpellLine(spellLineName);
			List<Spell> spells = SkillBase.GetSpellList(castLine.KeyName);

			foreach (Spell spell in spells)
			{
				if (spell.Level == spellLevel)
				{
					castSpell = spell;
					break;
				}
			}
			if (AttackState)
				StopAttack();
			if (IsMoving)
				StopFollowing();
			TurnTo(TargetObject);
			CastSpell(castSpell, castLine);
		}

		/// <summary>
		/// Method to see if the Guard has been left alone long enough to use Ranged attacks
		/// </summary>
		/// <returns></returns>
		public bool CanUseRanged
		{
			get
			{
				if (ObjectState != eObjectState.Active) return false;
				if (this is GuardFighter) return false;
				if (this is GuardArcher || this is GuardLord)
				{
					if (Inventory == null) return false;
					if (Inventory.GetItem(eInventorySlot.DistanceWeapon) == null) return false;
					if (ActiveWeaponSlot == eActiveWeaponSlot.Distance) return false;
				}
				if (this is GuardCaster || this is GuardHealer)
				{
					if (CurrentSpellHandler != null) return false;
				}
				return !BeenAttackedRecently;
			}
		}

		/// <summary>
		/// Because of Spell issues, we will always return this true
		/// </summary>
		/// <param name="target"></param>
		/// <param name="viewangle"></param>
		/// <returns></returns>
		public override bool IsObjectInFront(GameObject target, double viewangle, bool rangeCheck = true)
		{
			return true;
		}

		/// <summary>
		/// Static archers attack with melee the closest if being engaged in melee
		/// </summary>
		/// <param name="ad"></param>
		public override void OnAttackedByEnemy(AttackData ad)
		{
			//this is for static archers only
			if (MaxSpeedBase == 0)
			{
				//if we are currently fighting in melee
				if (ActiveWeaponSlot == eActiveWeaponSlot.Standard || ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
				{
					//if we are targeting something, and the distance to the target object is greater than the attack range
					if (TargetObject != null && !IsWithinRadius(TargetObject, AttackRange))
					{
						//stop the attack
						StopAttack();
						//if the distance to the attacker is less than the attack range
						if (IsWithinRadius(ad.Attacker, AttackRange))
						{
							//attack it
							StartAttack(ad.Attacker);
						}
					}
				}
			}
			base.OnAttackedByEnemy(ad);
		}

		/// <summary>
		/// When guards Die and it isnt a keep reset (this killer) we call GuardSpam function
		/// </summary>
		/// <param name="killer"></param>
		public override void Die(GameObject killer)
		{
			if (killer != this)
				GuardSpam(this);
			base.Die(killer);
			if (RespawnInterval == -1)
				Delete();
		}

		#region Guard Spam
		/// <summary>
		/// Sends message to guild for guard death with enemy count in area
		/// </summary>
		/// <param name="guard">The guard object</param>
		public static void GuardSpam(GameKeepGuard guard)
		{
			if (guard.Component == null) return;
			if (guard.Component.Keep == null) return;
			if (guard.Component.Keep.Guild == null) return;

			int inArea = guard.GetEnemyCountInArea();
			string message = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "GameKeepGuard.GuardSpam.Killed", guard.Name, guard.Component.Keep.Name, inArea);
			KeepGuildMgr.SendMessageToGuild(message, guard.Component.Keep.Guild);
		}

		/// <summary>
		/// Gets the count of enemies in the Area
		/// </summary>
		/// <returns></returns>
		public int GetEnemyCountInArea()
		{
			int inArea = 0;
			foreach (GamePlayer NearbyPlayers in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (Component != null)
				{
					if (GameServer.KeepManager.IsEnemy(Component.Keep, NearbyPlayers))
						inArea++;
				}
				else
				{
					if (GameServer.ServerRules.IsAllowedToAttack(this, NearbyPlayers, true))
						inArea++;
				}
			}
			return inArea;
		}


		#endregion

		/// <summary>
		/// Has the NPC been attacked recently.. currently 10 seconds
		/// </summary>
		public bool BeenAttackedRecently
		{
			get
			{
				return CurrentRegion.Time - LastAttackedByEnemyTick < 10 * 1000;
			}
		}
		#endregion

		/// <summary>
		/// When we add a guard to the world, we also attach an AttackFinished handler
		/// We use this to check LOS and range issues for our ranged guards
		/// </summary>
		/// <returns></returns>
		public override bool AddToWorld()
		{
			base.RoamingRange = 0;
			base.TetherRange = 10000;

			if (!base.AddToWorld())
				return false;

			if (IsPortalKeepGuard && (Brain as KeepGuardBrain != null))
			{
				(this.Brain as KeepGuardBrain).AggroRange = 2000;
				(this.Brain as KeepGuardBrain).AggroLevel = 99;
			}

			GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackFinished));

			if (PatrolGroup != null && !m_changingPositions)
			{
				bool foundGuard = false;
				foreach (GameKeepGuard guard in PatrolGroup.PatrolGuards)
				{
					if (guard.IsAlive && guard.CurrentWayPoint != null)
					{
						CurrentWayPoint = guard.CurrentWayPoint;
						m_changingPositions = true;
                        var offset = Vector.Create(x: Util.Random(200, 350), y: Util.Random(200, 350));
						MoveTo(guard.Position - offset);
						m_changingPositions = false;
						foundGuard = true;
						break;
					}
				}

				if (!foundGuard)
					CurrentWayPoint = PatrolGroup.PatrolPath;

				MoveOnPath(Patrol.PATROL_SPEED);
			}

			return true;
		}

		public override bool Spawn()
		{
			var result = base.Spawn();
			RefreshTemplate();
			return result;
		}

		/// <summary>
		/// When we remove from world, we remove our special handler
		/// </summary>
		/// <returns></returns>
		public override bool RemoveFromWorld()
		{
			if (base.RemoveFromWorld())
			{
				GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(AttackFinished));
				return true;
			}

			return false;
		}

		/// <summary>
		/// Method to stop a guards respawn
		/// </summary>
		public void StopRespawn()
		{
			if (IsRespawning)
				m_respawnTimer.Stop();
		}

		/// <summary>
		/// Gets the messages when you click on a guard
		/// </summary>
		/// <param name="player">The player that has done the clicking</param>
		/// <returns></returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			//You target [Armwoman]
			//You examine the Armswoman. She is friendly and is a realm guard.
			//She has upgraded equipment (5).
			IList list = new ArrayList(4);
			list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.YouTarget", GetName(0, false)));

			if (Realm != eRealm.None)
			{
				list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
				if (this.Component != null)
				{
					string text = "";
					if (Component.Keep.Level > 1 && Component.Keep.Level < 250 && GameServer.ServerRules.IsSameRealm(player, this, true))
						text = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.Upgraded", GetPronoun(0, true), Component.Keep.Level);
					if (Properties.USE_KEEP_BALANCING && Component.Keep.Region == 163 && !(Component.Keep is GameKeepTower))
						text += LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.Balancing", GetPronoun(0, true), (Component.Keep.BaseLevel - 50).ToString());
					if (text != "")
						list.Add(text);
				}
			}
			return list;
		}

		/// <summary>
		/// Gets the pronoun for the guards gender
		/// </summary>
		/// <param name="form">Form of the pronoun</param>
		/// <param name="firstLetterUppercase">Weather or not we want the first letter uppercase</param>
		/// <returns></returns>
		public override string GetPronoun(int form, bool firstLetterUppercase)
		{
			string s = "";
			switch (form)
			{
				default:
					{
						// Subjective
						if (Gender == GS.eGender.Male)
							s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.He");
						else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.She");
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
				case 1:
					{
						// Possessive
						if (Gender == eGender.Male)
							s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.His");
						else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Hers");
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
				case 2:
					{
						// Objective
						if (Gender == eGender.Male)
							s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Him");
						else s = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GetPronoun.Her");
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
			}
			return s;
		}

		#region Database

		string m_dataObjectID = "";

		/// <summary>
		/// Load the guard from the database
		/// </summary>
		/// <param name="mobobject">The database mobobject</param>
		public override void LoadFromDatabase(DataObject mobobject)
		{
			if (mobobject == null) return;
			base.LoadFromDatabase(mobobject);
			string sKey = mobobject.ObjectId;
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea keepArea)
				{
					Component = new GameKeepComponent();
					Component.Keep = keepArea.Keep;
					m_dataObjectID = mobobject.ObjectId;
					// mob reload command might be reloading guard, so check to make sure it isn't already added
					if (Component.Keep.Guards.ContainsKey(sKey) == false)
						Component.Keep.Guards.Add(sKey, this);
					// break; This is a bad idea.  If there are multiple KeepAreas, we should put a guard on each
				}
			}

			RefreshTemplate();
		}

		public void DeleteObject()
		{
			if (Component != null)
			{
				if (Component.Keep != null)
				{
					string skey = m_dataObjectID;
					if (Component.Keep.Guards.ContainsKey(skey))
						Component.Keep.Guards.Remove(skey);
					else if (log.IsWarnEnabled)
						log.Warn($"Can't find {DbKeepPosition.ClassType} with dataObjectId {m_dataObjectID} in Component InternalID {Component.InternalID} Guard list.");
				}
				else if (log.IsWarnEnabled)
					log.Warn($"Keep is null on delete of guard {Name} with dataObjectId {m_dataObjectID}");

				Component.Delete();
			}
			else if (log.IsWarnEnabled)
				log.Warn($"Component is null on delete of guard {Name} with dataObjectId {m_dataObjectID}");

			HookPoint = null;
			Component = null;
			if (Inventory != null)
				Inventory.ClearInventory();
			Inventory = null;
			DbKeepPosition = null;
			TempProperties.removeAllProperties();

			base.Delete();

			SetOwnBrain(null);
			CurrentRegion = null;

			GameEventMgr.RemoveAllHandlersForObject(this);
		}

		public override void Delete()
		{
			if (HookPoint != null && Component != null)
				Component.Keep.Guards.Remove(m_templateID); //Remove(this.ObjectID); LoadFromPosition() uses position.TemplateID as the insertion key

			TempProperties.removeAllProperties();

			base.Delete();
		}

		public override void DeleteFromDatabase()
		{
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea && Component != null)
				{
					Component.Keep.Guards.Remove(m_dataObjectID); //Remove(this.InternalID); LoadFromDatabase() adds using m_dataObjectID
																		  // break; This is a bad idea.  If there are multiple KeepAreas, we could end up with instantiated keep items that are no longer in the DB
				}
			}
			base.DeleteFromDatabase();
		}

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;
			component.Keep.Guards.Add(m_templateID + component.ID, this);
			PositionMgr.LoadGuardPosition(pos, this);
			RefreshTemplate();
			this.AddToWorld();
		}

		/// <summary>
		/// Move a guard to a position
		/// </summary>
		/// <param name="position">The new position for the guard</param>
		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadGuardPosition(position, this);
			if (!InCombat) MoveTo(Position);
		}
		#endregion

		/// <summary>
		/// Change guild of guard (emblem on equipment) when keep is claimed
		/// </summary>
		public void ChangeGuild()
		{
			ClothingMgr.EquipGuard(this);

			Guild guild = Component.Keep.Guild;
			string guildname = "";
			if (guild != null)
				guildname = guild.Name;

			GuildName = guildname;

			if (Inventory == null)
				return;

			int emblem = 0;
			if (guild != null)
				emblem = guild.Emblem;
			InventoryItem lefthand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (lefthand != null)
				lefthand.Emblem = emblem;

			InventoryItem cloak = Inventory.GetItem(eInventorySlot.Cloak);
			if (cloak != null)
			{
				cloak.Emblem = emblem;

				if (cloak.Emblem != 0)
					cloak.Model = 558; // change to a model that looks ok with an emblem

			}
			if (IsAlive)
			{
				BroadcastLivingEquipmentUpdate();
			}
		}

		/// <summary>
		/// Adding special handling for walking to a point for patrol guards to be in a formation
		/// </summary>
		public override void WalkTo(Coordinate destination, short speed)
		{
			int offX = 0; int offY = 0;
			if (IsMovingOnPath && PatrolGroup != null)
            {
                PatrolGroup.GetMovementOffset(this, out offX, out offY);
            }
            var offset = Vector.Create(x: offX, y: offY );
            base.WalkTo(destination - offset, speed);
		}

		public override void WalkToSpawn()
		{
			if (PatrolGroup != null)
			{
				StopAttack();
				StopFollowing();

				StandardMobBrain brain = Brain as StandardMobBrain;
				if (brain != null && brain.HasAggro)
				{
					brain.ClearAggroList();
				}

				PatrolGroup.StartPatrol();
			}
			else
			{
				WalkToSpawn(MaxSpeed);
			}
		}

		public void RefreshTemplate()
		{
			SetRealm();
			SetGuild();
			SetRespawnTime();
			SetGender();
			SetModel();
			SetName();
			SetBlockEvadeParryChance();
			SetBrain();
			SetSpeed();
			SetLevel();
			SetResists();
			SetStats();
			SetAggression();
			ClothingMgr.EquipGuard(this);
			ClothingMgr.SetEmblem(this);
		}

		protected virtual void SetName()
		{
			if (Realm == eRealm.None)
			{
				Name = LanguageMgr.GetTranslation(Properties.SERV_LANGUAGE, "SetGuardName.Renegade", Name);
			}
		}

		protected virtual void SetBlockEvadeParryChance()
		{
			BlockChance = 0;
			EvadeChance = 0;
			ParryChance = 0;
		}

		protected virtual KeepGuardBrain GetBrain() => new KeepGuardBrain();

		protected virtual void SetBrain()
		{
			if (Brain is KeepGuardBrain == false)
			{
				KeepGuardBrain brain = GetBrain();
				AddBrain(brain);
				brain.guard = this;
			}
		}

		protected virtual void SetSpeed()
		{
			if (IsPortalKeepGuard)
			{
				MaxSpeedBase = 575;
			}
			if (Level < 250)
			{
				if (Realm == eRealm.None)
				{
					MaxSpeedBase = 200;
				}
				else if (Level < 50)
				{
					MaxSpeedBase = 210;
				}
				else
				{
					MaxSpeedBase = 250;
				}
			}
			else
			{
				MaxSpeedBase = 575;
			}
		}

		private void SetResists()
		{
			for (int i = (int)eProperty.Resist_First; i <= (int)eProperty.Resist_Last; i++)
			{
				if (this is GuardLord)
				{
					BaseBuffBonusCategory[i] = 40;
				}
				else if (Level < 50)
				{
					BaseBuffBonusCategory[i] = Level / 2 + 1;
				}
				else
				{
					BaseBuffBonusCategory[i] = 26;
				}
			}
		}

		protected virtual void SetStats()
		{
			Strength = (short)(Properties.GUARD_AUTOSET_STR_BASE + (10 * Level * Properties.GUARD_AUTOSET_STR_MULTIPLIER));
			Dexterity = (short)(Properties.GUARD_AUTOSET_DEX_BASE + (Level * Properties.GUARD_AUTOSET_DEX_MULTIPLIER));
			Constitution = (short)(Properties.GUARD_AUTOSET_CON_BASE + (Level * Properties.GUARD_AUTOSET_CON_MULTIPLIER));
			Quickness = (short)(Properties.GUARD_AUTOSET_QUI_BASE + (Level * Properties.GUARD_AUTOSET_QUI_MULTIPLIER));
			Intelligence = (short)(Properties.GUARD_AUTOSET_INT_BASE + (Level * Properties.GUARD_AUTOSET_INT_MULTIPLIER));
		}

		private void SetRealm()
		{
			if (Component != null)
			{
				Realm = Component.Keep.Realm;
			}
			else
			{
				Realm = CurrentZone.Realm;
			}

			if (Realm != eRealm.None)
			{
				ModelRealm = Realm;
			}
			else
			{
				ModelRealm = (eRealm)Util.Random(1, 3);
			}
		}

		private void SetGuild()
		{
			if (Component == null)
			{
				GuildName = "";
			}
			else if (Component.Keep.Guild == null)
			{
				GuildName = "";
			}
			else
			{
				GuildName = Component.Keep.Guild.Name;
			}
		}

		protected virtual void SetRespawnTime()
		{
			int iVariance = 1000 * Math.Abs(Properties.GUARD_RESPAWN_VARIANCE);
			int iRespawn = 60 * ((Math.Abs(Properties.GUARD_RESPAWN) * 1000) +
				(Util.Random(-iVariance, iVariance)));

			RespawnInterval = (iRespawn > 1000) ? iRespawn : 1000; // Make sure we don't end up with an impossibly low respawn interval.
		}

		protected virtual void SetAggression() { }

		public void SetLevel()
		{
			if (Component != null)
			{
				Component.Keep.SetGuardLevel(this);
			}
		}

		private void SetGender()
		{
			//portal keep guards are always male
			if (IsPortalKeepGuard)
			{
				Gender = eGender.Male;
			}
			else
			{
				if (Util.Chance(50))
				{
					Gender = eGender.Male;
				}
				else
				{
					Gender = eGender.Female;
				}
			}
		}

		protected virtual CharacterClass GetClass()
        {
			return CharacterClass.None;
		}

		protected virtual void SetModel()
		{
			if (!Properties.AUTOMODEL_GUARDS_LOADED_FROM_DB && !LoadedFromScript)
			{
				return;
			}
			
			var possibleRaces = GetClass().EligibleRaces.FindAll(s => s.GetModel(Gender) != eLivingModel.None);
			if (possibleRaces.Count > 0)
			{
				var indexPick = Util.Random(0, possibleRaces.Count - 1);
				Model = (ushort)possibleRaces[indexPick].GetModel(Gender);
			}
		}

		private static SpellLine GuardSpellLine { get; } = new SpellLine("GuardSpellLine", "Guard Spells", "unknown", false);
	}

	public class GuardSpellDB
    {
		private static Spell m_albLordHealSpell;
		private static Spell m_midLordHealSpell;
		private static Spell m_hibLordHealSpell;

		private static DBSpell BaseHealSpell
        {
			get
            {
				DBSpell spell = new DBSpell();
				spell.AllowAdd = false;
				spell.CastTime = 2;
				spell.Name = "Guard Heal";
				spell.Range = WorldMgr.VISIBILITY_DISTANCE;
				spell.Type = "Heal";
				return spell;
			}
        }

		private static DBSpell LordBaseHealSpell
		{
			get
			{
				DBSpell spell = BaseHealSpell;
				spell.CastTime = 2;
				spell.Target = "Self";
				spell.Value = -2.5; // 2.5% of caster health instead of constant value
				if (GameServer.Instance.Configuration.ServerType != eGameServerType.GST_PvE)
					spell.Uninterruptible = true;
				return spell;
			}
		}

		private static DBSpell GuardBaseHealSpell
		{
			get
			{
				DBSpell spell = BaseHealSpell;
				spell.CastTime = 2;
				spell.Value = 200;
				spell.Target = "Realm";
				return spell;
			}
		}

		public static Spell AlbLordHealSpell
		{
			get
			{
				if (m_albLordHealSpell == null)
				{
					DBSpell spell = LordBaseHealSpell;
					spell.ClientEffect = 1340;
					spell.SpellID = 90001;
					m_albLordHealSpell = new Spell(spell, 50);
				}
				return m_albLordHealSpell;
			}
		}

		public static Spell MidLordHealSpell
		{
			get
			{
				if (m_midLordHealSpell == null)
				{
					DBSpell spell = LordBaseHealSpell;
					spell.ClientEffect = 3011;
					spell.SpellID = 90002;
					m_midLordHealSpell = new Spell(spell, 50);
				}
				return m_midLordHealSpell;
			}
		}

		public static Spell HibLordHealSpell
		{
			get
			{
				if (m_hibLordHealSpell == null)
				{
					DBSpell spell = LordBaseHealSpell;
					spell.ClientEffect = 3030;
					spell.SpellID = 90003;
					m_hibLordHealSpell = new Spell(spell, 50);
				}
				return m_hibLordHealSpell;
			}
		}

		private static Spell m_albGuardHealSmallSpell;
		private static Spell m_midGuardHealSmallSpell;
		private static Spell m_hibGuardHealSmallSpell;

		public static Spell AlbGuardHealSmallSpell
		{
			get
			{
				if (m_albGuardHealSmallSpell == null)
				{
					DBSpell spell = GuardBaseHealSpell;
					spell.ClientEffect = 1340;
					spell.SpellID = 90004;
					m_albGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_albGuardHealSmallSpell;
			}
		}

		public static Spell MidGuardHealSmallSpell
		{
			get
			{
				if (m_midGuardHealSmallSpell == null)
				{
					DBSpell spell = GuardBaseHealSpell;
					spell.ClientEffect = 3011;
					spell.SpellID = 90005;
					m_midGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_midGuardHealSmallSpell;
			}
		}

		public static Spell HibGuardHealSmallSpell
		{
			get
			{
				if (m_hibGuardHealSmallSpell == null)
				{
					DBSpell spell = GuardBaseHealSpell;
					spell.ClientEffect = 3030;
					spell.SpellID = 90006;
					m_hibGuardHealSmallSpell = new Spell(spell, 50);
				}
				return m_hibGuardHealSmallSpell;
			}
		}
	}
}
