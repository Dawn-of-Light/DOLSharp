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
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// Keep guard is gamemob with just different brain and load from other DB table
	/// </summary>
	public class GameKeepGuard : GameMob, IKeepItem
	{
		private string m_templateID = "";
		public string TemplateID
		{
			get { return m_templateID; }
		}

		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
		}

		private DBKeepPosition m_position;
		public DBKeepPosition Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		public bool IsTowerGuard
		{
			get
			{
				if (this.Component != null && this.Component.Keep != null)
				{
					return this.Component.Keep is GameKeepTower;
				}
				return false;
			}
		}

		public bool IsPortalKeepGuard
		{
			get
			{
				if (this.IsTowerGuard && this.Component.Keep.KeepComponents.Count > 1)
					return true;
				return false;
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
				return base.Level;
			}
			set
			{
				if (this.IsRespawning)
					m_Level = value;
				else
					base.Level = value;
			}
		}

		/// <summary>
		/// Bools holding weather this guard is male or female.
		/// </summary>
		public bool IsMale = true;

		/// <summary>
		/// Guards always have Mana to cast spells
		/// </summary>
		public override int Mana
		{
			get { return 50000; }
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
				case eActiveWeaponSlot.Distance: speed = 60; break;
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
					lock (guard.Attackers.SyncRoot)
					{
						foreach (GameLiving living in guard.Attackers)
						{
							if (WorldMgr.GetDistance(guard, living) <= guard.AttackRange)
							{
								guard.StartAttack(living);
								return;
							}
						}
					}
					if (WorldMgr.GetDistance(guard, guard.TargetObject) <= 2000)
					{
						if (guard.MaxSpeedBase == 0 || (guard is GuardArcher && !guard.BeenAttackedRecently))
							guard.SwitchToRanged(guard.TargetObject);
						else if (guard is GuardCaster)
							(guard as GuardCaster).StartSpellAttack(guard.TargetObject);
					}
				}
				return;
			}

			if (guard.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				if (WorldMgr.GetDistance(guard, guard.TargetObject) > guard.AttackRange)
				{
					guard.StopAttack();
					return;
				}
			}

			if (guard.TargetObject is GamePlayer == false)
				return;

			GamePlayer player = guard.TargetObject as GamePlayer;
			player.Out.SendCheckLOS(guard, player, new CheckLOSResponse(guard.GuardStopAttackCheckLOS));
		}

		public const string Last_LOS_Target_Property = "last_LOS_checkTarget";
		public const string Last_LOS_Tick_Property = "last_LOS_checkTick";

		/// <summary>
		/// Override for StartAttack which chooses Ranged or Melee attack
		/// </summary>
		/// <param name="attackTarget"></param>
		public override void StartAttack(GameObject attackTarget)
		{
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

			//Prevent spam for LOS checks multiple times..
			GameObject lastTarget = (GameObject)this.TempProperties.getObjectProperty(Last_LOS_Target_Property, null);
			if (lastTarget != null && lastTarget == attackTarget)
			{
				long lastTick = this.TempProperties.getLongProperty(Last_LOS_Tick_Property, 0);
				if (lastTick != 0 && CurrentRegion.Time - lastTick < 5 * 1000)
					return;
			}


			GamePlayer LOSChecker = null;
			if (attackTarget is GamePlayer)
			{
				LOSChecker = attackTarget as GamePlayer;
			}
			else
			{
				foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					LOSChecker = player;
					break;
				}
			}
			if (LOSChecker == null)
				return;

			this.TempProperties.setProperty(Last_LOS_Target_Property, attackTarget);
			this.TempProperties.setProperty(Last_LOS_Tick_Property, CurrentRegion.Time);
			TargetObject = attackTarget;
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


				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					if (IsMoving)
						StopFollow();
				}
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
			}
		}

		public void GuardStartSpellHealCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			if ((response & 0x100) == 0x100)
			{
				SpellMgr.CastHealSpell(this, TargetObject as GameLiving);
			}
		}

		/// <summary>
		/// Easy method to determine if the attack result was a hit
		/// </summary>
		/// <param name="result"></param>
		/// <returns></returns>
		public static bool IsHit(GameLiving.eAttackResult result)
		{
			if (result == GameLiving.eAttackResult.HitUnstyled ||
result == GameLiving.eAttackResult.HitStyle ||
result == GameLiving.eAttackResult.Missed ||
result == GameLiving.eAttackResult.Blocked ||
result == GameLiving.eAttackResult.Evaded ||
result == GameLiving.eAttackResult.Fumbled ||
result == GameLiving.eAttackResult.Parried)
				return true;
			return false;
		}

				/// <summary>
		/// Method to see if the Guard has been left alone long enough to use Ranged attacks
		/// </summary>
		/// <param name="npc">The guard object</param>
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
		/// Method to switch the guard to Melee attacks
		/// </summary>
		/// <param name="target"></param>
		public void SwitchToMelee(GameObject target)
		{
			InventoryItem twohand = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			InventoryItem righthand = Inventory.GetItem(eInventorySlot.RightHandWeapon);

			if (twohand != null && righthand == null)
				SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			else if (twohand != null && righthand != null)
			{
				if (Util.Chance(50))
					SwitchWeapon(eActiveWeaponSlot.TwoHanded);
				else SwitchWeapon(eActiveWeaponSlot.Standard);
			}
			else SwitchWeapon(eActiveWeaponSlot.Standard);
			StopAttack();
			StopFollow();
			//Follow(target, 90, 2000);	// follow at stickrange
			StartAttack(target);
		}

		/// <summary>
		/// Method to switch the guard to Ranged attacks
		/// </summary>
		/// <param name="target"></param>
		public void SwitchToRanged(GameObject target)
		{
			SwitchWeapon(eActiveWeaponSlot.Distance);
			//Follow(target, 2000, 2000);	// follow at stickrange
			StartAttack(target);
			StopFollow();
		}

		/// <summary>
		/// Because of Spell issues, we will always return this true
		/// </summary>
		/// <param name="target"></param>
		/// <param name="viewangle"></param>
		/// <param name="rangeCheck"></param>
		/// <returns></returns>
		public override bool IsObjectInFront(GameObject target, double viewangle)
		{
			return true;
		}

		/// <summary>
		/// If guards cant move, they cant be interupted from range attack
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="attackType"></param>
		/// <returns></returns>
		protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if (this.MaxSpeedBase == 0)
			{
				if (attackType == AttackData.eAttackType.Ranged || attackType == AttackData.eAttackType.Spell)
				{
					if (WorldMgr.GetDistance(this, attacker) > 150)
						return false;
				}
			}

			StopAttack();
			SwitchToMelee(attacker);

			return base.OnInterruptTick(attacker, attackType);
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
				if (ActiveWeaponSlot == eActiveWeaponSlot.Standard ||
	ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
				{
					//if we are targeting something, and the distance to the target object is greater than the attack range
					if (TargetObject != null && WorldMgr.GetDistance(TargetObject, this) > AttackRange)
					{
						//stop the attack
						StopAttack();
						//if the distance to the attacker is less than the attack range
						if (WorldMgr.GetDistance(this, ad.Attacker) <= AttackRange)
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

			int inArea = GetEnemyCountInArea(guard);

			string message = guard.Name + " has been killed in " + guard.Component.Keep.Name + " with " + inArea + " enemy player(s) in the area!";
			KeepGuildMgr.SendMessageToGuild(message, guard.Component.Keep.Guild);
		}

		/// <summary>
		/// Gets the count of enemies in the Area
		/// </summary>
		/// <param name="guard">The guard object</param>
		/// <returns></returns>
		public static int GetEnemyCountInArea(GameKeepGuard guard)
		{
			int inArea = 0;
			foreach (GamePlayer NearbyPlayers in guard.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToAttack(guard, NearbyPlayers, true))
					inArea++;
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

		public override bool AddToWorld()
		{
			if (!base.AddToWorld())
				return false;
			GameEventMgr.AddHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));
			return true;
		}

		/// <summary>
		/// When we remove from world, we remove our special handler
		/// </summary>
		/// <returns></returns>
		public override bool RemoveFromWorld()
		{
			GameEventMgr.RemoveHandler(this, GameNPCEvent.AttackFinished, new DOLEventHandler(AttackFinished));
			return base.RemoveFromWorld();
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
			TemplateMgr.RefreshTemplate(this);
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
			list.Add("You target [" + GetName(0, false) + "]");
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and is a realm guard.");
			if (this.Component != null)
			{
				if (this.Component.Keep.Level > 1 && GameServer.ServerRules.IsSameRealm(player, this, true))
					list.Add(GetPronoun(0, true) + " has upgraded equipment (" + this.Component.Keep.Level + ").");
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
						if (IsMale)
							s = "He";
						else s = "She";
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
				case 1:
					{
						// Possessive
						if (IsMale)
							s = "His";
						else s = "Hers";
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
				case 2:
					{
						// Objective
						if (IsMale)
							s = "Him";
						else s = "Her";
						if (!firstLetterUppercase)
							s = s.ToLower();
						break;
					}
			}
			return s;
		}

		public override byte BlockChance
		{
			get
			{
				if (ActiveWeaponSlot != eActiveWeaponSlot.Standard)
					return 0;
				return base.BlockChance;
			}
			set
			{
				base.BlockChance = value;
			}
		}

		#region Database

		public override void LoadFromDatabase(DataObject mobobject)
		{
			base.LoadFromDatabase(mobobject);
			TemplateMgr.RefreshTemplate(this);
		}

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;
			component.Keep.Guards[m_templateID] = this;
			PositionMgr.LoadGuardPosition(pos, this);
			TemplateMgr.RefreshTemplate(this);
			this.AddToWorld();
		}

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
		/// <param name="guild"> the guild owner of the keep</param>
		public void ChangeGuild()
		{
			Guild guild = this.Component.Keep.Guild;
			string guildname = "";
			if (guild != null)
				guildname = guild.Name;

			this.GuildName = guildname;

			if (this.Inventory == null)
				return;

			int emblem = 0;
			if (guild != null)
				emblem = guild.theGuildDB.Emblem;
			InventoryItem lefthand = this.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (lefthand != null)
				lefthand.Emblem = emblem;

			InventoryItem cloak = this.Inventory.GetItem(eInventorySlot.Cloak);
			if (cloak != null)
				cloak.Emblem = emblem;
			this.UpdateNPCEquipmentAppearance();
		}
	}
}
