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

using System.Reflection;
using log4net;


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
		public DBKeepPosition Position
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
				if (this.Component != null && this.Component.AbstractKeep != null)
				{
					return this.Component.AbstractKeep is GameKeepTower;
				}
				return false;
			}
		}

		public bool IsPortalKeepGuard
		{
			get
			{
				if (this.Component == null || this.Component.AbstractKeep == null)
					return false;
				return this.Component.AbstractKeep.IsPortalKeep;
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
				if(IsPortalKeepGuard)
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
                            if ( guard.IsWithinRadius( living, guard.AttackRange ) )
							{
								guard.StartAttack(living);
								return;
							}
						}
					}

                    if ( guard.IsWithinRadius( guard.TargetObject, guard.AttackRangeDistance ) )
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
                if ( !guard.IsWithinRadius( guard.TargetObject, guard.AttackRange ) )
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

				if (TargetObject != null && TargetPosition is GameLiving)
				{
					(this.Brain as KeepGuardBrain).RemoveFromAggroList(TargetObject as GameLiving);
				}
			}
		}

		public void GuardStartSpellHealCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100 && HealTarget != null)
			{
				TargetObject = HealTarget;
				SpellMgr.CastHealSpell(this, HealTarget as GameLiving);
			}
		}
		public void GuardStartSpellNukeCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				SpellMgr.CastNukeSpell(this, TargetObject as GameLiving);
			}
		}

		/// <summary>
		/// Method to see if the Guard has been left alone long enough to use Ranged attacks
		/// </summary>
		/// <returns></returns>
		public bool CanUseRanged
		{
			get
			{
				if (this.ObjectState != GameObject.eObjectState.Active) return false;
				if (this is GuardFighter) return false;
				if (this is GuardArcher || this is GuardLord)
				{
					if (this.Inventory == null) return false;
					if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) == null) return false;
					if (this.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance) return false;
				}
				if (this is GuardCaster || this is GuardHealer)
				{
					if (this.CurrentSpellHandler != null) return false;
				}
				return !this.BeenAttackedRecently;
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
                    if( TargetObject != null && !this.IsWithinRadius( TargetObject, AttackRange ) )
					{
						//stop the attack
						StopAttack();
						//if the distance to the attacker is less than the attack range
						if ( this.IsWithinRadius( ad.Attacker, AttackRange ) )
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
			if (guard.Component.AbstractKeep == null) return;
			if (guard.Component.AbstractKeep.Guild == null) return;

			int inArea = guard.GetEnemyCountInArea();
            string message = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GameKeepGuard.GuardSpam.Killed", guard.Name, guard.Component.AbstractKeep.Name, inArea);
            KeepGuildMgr.SendMessageToGuild(message, guard.Component.AbstractKeep.Guild);
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
				if (this.Component != null)
				{
					if (GameServer.KeepManager.IsEnemy(this.Component.AbstractKeep, NearbyPlayers))
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
			
			if(IsPortalKeepGuard&&(Brain as KeepGuardBrain!=null))
			{
				(this.Brain as KeepGuardBrain).AggroRange=2000;
				(this.Brain as KeepGuardBrain).AggroLevel=99;
			}
			
			GameEventMgr.AddHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));

			if (PatrolGroup != null && !m_changingPositions)
			{
				bool foundGuard = false;
				foreach (GameKeepGuard guard in PatrolGroup.PatrolGuards)
				{
					if (guard.IsAlive && guard.CurrentWayPoint != null)
					{
						CurrentWayPoint = guard.CurrentWayPoint;
						m_changingPositions = true;
						MoveTo(guard.CurrentRegionID, guard.X - Util.Random(200, 350), guard.Y - Util.Random(200, 350), guard.Z, guard.Heading);
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

		/// <summary>
		/// When we remove from world, we remove our special handler
		/// </summary>
		/// <returns></returns>
		public override bool RemoveFromWorld()
		{
			if (base.RemoveFromWorld())
			{
				GameEventMgr.RemoveHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));
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
		/// When guards respawn we refresh them, if a patrol guard respawns we
		/// call a special function to update leadership
		/// </summary>
		/// <param name="respawnTimer"></param>
		/// <returns></returns>
		protected override int RespawnTimerCallback(RegionTimer respawnTimer)
		{
			int temp = base.RespawnTimerCallback(respawnTimer);
			if (Component != null && Component.AbstractKeep != null)
			{
				Component.AbstractKeep.TemplateManager.GetMethod("RefreshTemplate").Invoke(null, new object[] { this });
			}
			else
			{
				TemplateMgr.RefreshTemplate(this);
			}
			return temp;
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
					if (this.Component.AbstractKeep.Level > 1 && this.Component.AbstractKeep.Level < 250 && GameServer.ServerRules.IsSameRealm(player, this, true))
                        text = LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.Upgraded", GetPronoun(0, true), this.Component.AbstractKeep.Level);
					if (ServerProperties.Properties.USE_KEEP_BALANCING && this.Component.AbstractKeep.Region == 163 && !(this.Component.AbstractKeep is GameKeepTower))
                        text += LanguageMgr.GetTranslation(player.Client.Account.Language, "GameKeepGuard.GetExamineMessages.Balancing", GetPronoun(0, true), (Component.AbstractKeep.BaseLevel - 50).ToString());
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
					Component.AbstractKeep = keepArea.Keep;
					m_dataObjectID = mobobject.ObjectId;
					// mob reload command might be reloading guard, so check to make sure it isn't already added
					if (Component.AbstractKeep.Guards.ContainsKey(sKey) == false)
						Component.AbstractKeep.Guards.Add(sKey, this);
					// break; This is a bad idea.  If there are multiple KeepAreas, we should put a guard on each
				}
			}

			if (Component != null && Component.AbstractKeep != null)
			{
				Component.AbstractKeep.TemplateManager.GetMethod("RefreshTemplate").Invoke(null, new object[] { this });
			}
			else
			{
				TemplateMgr.RefreshTemplate(this);
			}
		}

		public void DeleteObject()
		{
			if (Component != null)
			{
				if (Component.AbstractKeep != null)
				{
					string skey = m_dataObjectID;
					if (Component.AbstractKeep.Guards.ContainsKey(skey))
						Component.AbstractKeep.Guards.Remove(skey);
					else if (log.IsWarnEnabled)
						log.Warn($"Can't find {Position.ClassType} with dataObjectId {m_dataObjectID} in Component InternalID {Component.InternalID} Guard list.");
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
			Position = null;
			TempProperties.removeAllProperties();

			base.Delete();

			SetOwnBrain(null);
			CurrentRegion = null;

			GameEventMgr.RemoveAllHandlersForObject(this);
		}

		public override void Delete()
		{
			if (HookPoint != null && Component != null)
				Component.AbstractKeep.Guards.Remove(m_templateID); //Remove(this.ObjectID); LoadFromPosition() uses position.TemplateID as the insertion key

			TempProperties.removeAllProperties();

			base.Delete();
		}

		public override void DeleteFromDatabase()
		{
			foreach (AbstractArea area in this.CurrentAreas)
			{
				if (area is KeepArea && Component != null)
				{
					Component.AbstractKeep.Guards.Remove(m_dataObjectID); //Remove(this.InternalID); LoadFromDatabase() adds using m_dataObjectID
					// break; This is a bad idea.  If there are multiple KeepAreas, we could end up with instantiated keep items that are no longer in the DB
				}
			}
			base.DeleteFromDatabase();
		}

		/// <summary>
		/// Load the guard from a position
		/// </summary>
		/// <param name="pos">The position for the guard</param>
		/// <param name="component">The component it is being spawned on</param>
		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;
			component.AbstractKeep.Guards.Add(m_templateID, this);
			PositionMgr.LoadGuardPosition(pos, this);
			if (Component != null && Component.AbstractKeep != null)
				Component.AbstractKeep.TemplateManager.GetMethod("RefreshTemplate").Invoke(null, new object[] { this });
			else
				TemplateMgr.RefreshTemplate(this);
			this.AddToWorld();
		}

		/// <summary>
		/// Move a guard to a position
		/// </summary>
		/// <param name="position">The new position for the guard</param>
		public void MoveToPosition(DBKeepPosition position)
		{
			PositionMgr.LoadGuardPosition(position, this);
			if (!this.InCombat)
				this.MoveTo(this.CurrentRegionID, this.X, this.Y, this.Z, this.Heading);
		}
		#endregion

		/// <summary>
		/// Change guild of guard (emblem on equipment) when keep is claimed
		/// </summary>
		public void ChangeGuild()
		{
			ClothingMgr.EquipGuard(this);

			Guild guild = this.Component.AbstractKeep.Guild;
			string guildname = "";
			if (guild != null)
				guildname = guild.Name;

			this.GuildName = guildname;

			if (this.Inventory == null)
				return;

			int emblem = 0;
			if (guild != null)
				emblem = guild.Emblem;
			InventoryItem lefthand = this.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (lefthand != null)
				lefthand.Emblem = emblem;

			InventoryItem cloak = this.Inventory.GetItem(eInventorySlot.Cloak);
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
		/// <param name="tx"></param>
		/// <param name="ty"></param>
		/// <param name="tz"></param>
		/// <param name="speed"></param>
		public override void WalkTo(int tx, int ty, int tz, short speed)
		{
			int offX = 0; int offY = 0;
			if (IsMovingOnPath && PatrolGroup != null)
				PatrolGroup.GetMovementOffset(this, out offX, out offY);
			base.WalkTo(tx - offX, ty - offY, tz, speed);
		}

		/// <summary>
		/// Walk to the spawn point, always max speed for keep guards, or continue patrol.
		/// </summary>
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

	}
}
