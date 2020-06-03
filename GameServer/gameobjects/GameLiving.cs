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
using System.Collections.Specialized;
using System.Linq;
using System.Reflection;

using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using DOL.GS.RealmAbilities;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that each
	/// living object in the world uses
	/// </summary>
	public abstract class GameLiving : GameObject
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region Combat
		/// <summary>
		/// Holds the AttackData object of last attack
		/// </summary>
		public const string LAST_ATTACK_DATA = "LastAttackData";

		protected string m_lastInterruptMessage;
		public string LastInterruptMessage
		{
			get { return m_lastInterruptMessage; }
			set { m_lastInterruptMessage = value; }
		}

		/// <summary>
		/// Holds the AttackData object of the last left-hand attack
		/// </summary>
		public const string LAST_ATTACK_DATA_LH = "LastAttackDataLH";

		/// <summary>
		/// Holds the property for the result the last enemy
		/// </summary>
		public const string LAST_ENEMY_ATTACK_RESULT = "LastEnemyAttackResult";


		#region enums

		/// <summary>
		/// The result of an attack
		/// </summary>
		public enum eAttackResult : int
		{
			/// <summary>
			/// No specific attack
			/// </summary>
			Any = 0,
			/// <summary>
			/// The attack was a hit
			/// </summary>
			HitUnstyled = 1,
			/// <summary>
			/// The attack was a hit
			/// </summary>
			HitStyle = 2,
			/// <summary>
			/// Attack was denied by server rules
			/// </summary>
			NotAllowed_ServerRules = 3,
			/// <summary>
			/// No target for the attack
			/// </summary>
			NoTarget = 5,
			/// <summary>
			/// Target is already dead
			/// </summary>
			TargetDead = 6,
			/// <summary>
			/// Target is out of range
			/// </summary>
			OutOfRange = 7,
			/// <summary>
			/// Attack missed
			/// </summary>
			Missed = 8,
			/// <summary>
			/// The attack was evaded
			/// </summary>
			Evaded = 9,
			/// <summary>
			/// The attack was blocked
			/// </summary>
			Blocked = 10,
			/// <summary>
			/// The attack was parried
			/// </summary>
			Parried = 11,
			/// <summary>
			/// The target is invalid
			/// </summary>
			NoValidTarget = 12,
			/// <summary>
			/// The target is not visible
			/// </summary>
			TargetNotVisible = 14,
			/// <summary>
			/// The attack was fumbled
			/// </summary>
			Fumbled = 15,
			/// <summary>
			/// The attack was Bodyguarded
			/// </summary>
			Bodyguarded = 16,
			/// <summary>
			/// The attack was Phaseshiftet
			/// </summary>
			Phaseshift = 17,
			/// <summary>
			/// The attack was Grappled
			/// </summary>
			Grappled = 18
		}

		/// <summary>
		/// The possible states for a ranged attack
		/// </summary>
		public enum eRangedAttackState : byte
		{
			/// <summary>
			/// No ranged attack active
			/// </summary>
			None = 0,
			/// <summary>
			/// Ranged attack in aim-state
			/// </summary>
			Aim,
			/// <summary>
			/// Player wants to fire the shot/throw NOW!
			/// </summary>
			Fire,
			/// <summary>
			/// Ranged attack will fire when ready
			/// </summary>
			AimFire,
			/// <summary>
			/// Ranged attack will fire and reload when ready
			/// </summary>
			AimFireReload,
			/// <summary>
			/// Ranged attack is ready to be fired
			/// </summary>
			ReadyToFire,
		}

		/// <summary>
		/// The type of range attack
		/// </summary>
		public enum eRangedAttackType : byte
		{
			/// <summary>
			/// A normal ranged attack
			/// </summary>
			Normal = 0,
			/// <summary>
			/// A critical shot is attempted
			/// </summary>
			Critical,
			/// <summary>
			/// A longshot is attempted
			/// </summary>
			Long,
			/// <summary>
			/// A volley shot is attempted
			/// </summary>
			Volley,
			/// <summary>
			/// A sure shot is attempted
			/// </summary>
			SureShot,
			/// <summary>
			/// A rapid shot is attempted
			/// </summary>
			RapidFire,
		}

		/// <summary>
		/// Holds all the ways this living can
		/// be healed
		/// </summary>
		public enum eHealthChangeType : byte
		{
			/// <summary>
			/// The health was changed by something unknown
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// Regeneration changed the health
			/// </summary>
			Regenerate = 1,
			/// <summary>
			/// A spell changed the health
			/// </summary>
			Spell = 2,
			/// <summary>
			/// A potion changed the health
			/// </summary>
			Potion = 3
		}
		/// <summary>
		/// Holds all the ways this living can
		/// be healed
		/// </summary>
		public enum eManaChangeType : byte
		{
			/// <summary>
			/// Unknown mana change
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// Mana was changed by regenerate
			/// </summary>
			Regenerate = 1,
			/// <summary>
			/// Mana was changed by spell
			/// </summary>
			Spell = 2,
			/// <summary>
			/// Mana was changed by potion
			/// </summary>
			Potion = 3
		}
		/// <summary>
		/// Holds all the ways this living can
		/// be healed
		/// </summary>
		public enum eEnduranceChangeType : byte
		{
			/// <summary>
			/// Enduracen was changed by unknown
			/// </summary>
			Unknown = 0,
			/// <summary>
			/// Endurance was changed by Regenerate
			/// </summary>
			Regenerate = 1,
			/// <summary>
			/// Enduracen was changed by spell
			/// </summary>
			Spell = 2,
			/// <summary>
			/// Enduracen was changed by potion
			/// </summary>
			Potion = 3
		}
		/// <summary>
		/// Holds the possible activeWeaponSlot values
		/// </summary>
		public enum eActiveWeaponSlot : byte
		{
			/// <summary>
			/// Weapon slot righthand
			/// </summary>
			Standard = 0x00,
			/// <summary>
			/// Weaponslot twohanded
			/// </summary>
			TwoHanded = 0x01,
			/// <summary>
			/// Weaponslot distance
			/// </summary>
			Distance = 0x02
		}

		/// <summary>
		/// Holds the possible activeQuiverSlot values
		/// </summary>
		public enum eActiveQuiverSlot : byte
		{
			/// <summary>
			/// No quiver slot active
			/// </summary>
			None = 0x00,
			/// <summary>
			/// First quiver slot
			/// </summary>
			First = 0x10,
			/// <summary>
			/// Second quiver slot
			/// </summary>
			Second = 0x20,
			/// <summary>
			/// Third quiver slot
			/// </summary>
			Third = 0x40,
			/// <summary>
			/// Fourth quiver slot
			/// </summary>
			Fourth = 0x80,
		}

		public enum eXPSource
		{
			NPC,
			Player,
			Quest,
			Mission,
			Task,
			Praying,
			GM,
			Other
		}

		#endregion

		/// <summary>
		/// Can this living accept any item regardless of tradable or droppable?
		/// </summary>
		public virtual bool CanTradeAnyItem
		{
			get { return false; }
		}


		/// <summary>
		/// Chance to fumble an attack.
		/// </summary>
		public virtual double ChanceToFumble
		{
			get
			{
				double chanceToFumble = GetModified(eProperty.FumbleChance);
				chanceToFumble *= 0.001;

				if (chanceToFumble > 0.99) chanceToFumble = 0.99;
				if (chanceToFumble < 0) chanceToFumble = 0;

				return chanceToFumble;
			}
		}

		/// <summary>
		/// Chance to be missed by an attack.
		/// </summary>
		public virtual double ChanceToBeMissed
		{
			get
			{
				double chanceToBeMissed = GetModified(eProperty.MissHit);
				chanceToBeMissed *= 0.001;

				if (chanceToBeMissed > 0.99) chanceToBeMissed = 0.99;
				if (chanceToBeMissed < 0) chanceToBeMissed = 0;

				return chanceToBeMissed;
			}
		}

		protected short m_race;
		public virtual short Race
		{
			get { return m_race; }
			set { m_race = value; }
		}

		/// <summary>
		/// The state of the ranged attack
		/// </summary>
		protected eRangedAttackState m_rangedAttackState;
		/// <summary>
		/// The gtype of the ranged attack
		/// </summary>
		protected eRangedAttackType m_rangedAttackType;

		/// <summary>
		/// Gets or Sets the state of a ranged attack
		/// </summary>
		public eRangedAttackState RangedAttackState
		{
			get { return m_rangedAttackState; }
			set { m_rangedAttackState = value; }
		}

		/// <summary>
		/// Gets or Sets the type of a ranged attack
		/// </summary>
		public eRangedAttackType RangedAttackType
		{
			get { return m_rangedAttackType; }
			set { m_rangedAttackType = value; }
		}

		/// <summary>
		/// Holds the quiverslot to be used
		/// </summary>
		protected eActiveQuiverSlot m_activeQuiverSlot;

		/// <summary>
		/// Gets/Sets the current active quiver slot of this living
		/// </summary>
		public virtual eActiveQuiverSlot ActiveQuiverSlot
		{
			get { return m_activeQuiverSlot; }
			set { m_activeQuiverSlot = value; }
		}

		/// <summary>
		/// say if player is stunned or not
		/// </summary>
		protected bool m_stunned;
		/// <summary>
		/// Gets the stunned flag of this living
		/// </summary>
		public bool IsStunned
		{
			get { return m_stunned; }
			set { m_stunned = value; }
		}
		/// <summary>
		/// say if player is mezzed or not
		/// </summary>
		protected bool m_mezzed;
		/// <summary>
		/// Gets the mesmerized flag of this living
		/// </summary>
		public bool IsMezzed
		{
			get { return m_mezzed; }
			set { m_mezzed = value; }
		}

		protected bool m_disarmed = false;
		protected long m_disarmedTime = 0;

		/// <summary>
		/// Is the living disarmed
		/// </summary>
		public bool IsDisarmed
		{
			get { return (m_disarmedTime > 0 && m_disarmedTime > CurrentRegion.Time); }
		}

		/// <summary>
		/// How long is this living disarmed for?
		/// </summary>
		public long DisarmedTime
		{
			get { return m_disarmedTime; }
			set { m_disarmedTime = value; }
		}

		protected bool m_isSilenced = false;
		protected long m_silencedTime = 0;

		/// <summary>
		/// Has this living been silenced?
		/// </summary>
		public bool IsSilenced
		{
			get { return (m_silencedTime > 0 && m_silencedTime > CurrentRegion.Time); }
		}

		/// <summary>
		/// How long is this living silenced for?
		/// </summary>
		public long SilencedTime
		{
			get { return m_silencedTime; }
			set { m_silencedTime = value; }
		}

		/// <summary>
		/// Gets the current strafing mode
		/// </summary>
		public virtual bool IsStrafing
		{
			get { return false; }
			set { }
		}

		/// <summary>
		/// Holds disease counter
		/// </summary>
		protected volatile byte m_diseasedCount;
		/// <summary>
		/// Sets disease state
		/// </summary>
		/// <param name="add">true if disease counter should be increased</param>
		public virtual void Disease(bool active)
		{
			if (active) m_diseasedCount++;
			else m_diseasedCount--;

			if (m_diseasedCount < 0)
			{
				if (log.IsErrorEnabled)
					log.Error("m_diseasedCount is less than zero.\n" + Environment.StackTrace);
			}
		}
		/// <summary>
		/// Gets diseased state
		/// </summary>
		public virtual bool IsDiseased
		{
			get { return m_diseasedCount > 0; }
		}

		protected bool m_isEngaging = false;
		public virtual bool IsEngaging
		{
			get { return m_isEngaging; }
			set { m_isEngaging = value; }
		}

		/// <summary>
		/// Holds the turning disabled counter
		/// </summary>
		protected sbyte m_turningDisabledCount;
		/// <summary>
		/// Gets/Sets wether the player can turn the character
		/// </summary>
		public bool IsTurningDisabled
		{
			get { return m_turningDisabledCount > 0; }
		}
		/// <summary>
		/// Disables the turning for this living
		/// </summary>
		/// <param name="add"></param>
		public virtual void DisableTurning(bool add)
		{
			if (add) m_turningDisabledCount++;
			else m_turningDisabledCount--;

			if (m_turningDisabledCount < 0)
				m_turningDisabledCount=0;
		}

		/// <summary>
		/// List of objects that will gain XP after this living dies
		/// consists of GameObject -> damage(float)
		/// Damage in float because it might contain small amounts
		/// </summary>
		protected readonly HybridDictionary m_xpGainers;
		/// <summary>
		/// Holds the weaponslot to be used
		/// </summary>
		protected eActiveWeaponSlot m_activeWeaponSlot;
		/// <summary>
		/// AttackAction used for making an attack every weapon speed intervals
		/// </summary>
		protected AttackAction m_attackAction;
		/// <summary>
		/// The objects currently attacking this living
		/// To be more exact, the objects that are in combat
		/// and have this living as target.
		/// </summary>
		protected readonly List<GameObject> m_attackers;

		/// <summary>
		/// Returns the current active weapon slot of this living
		/// </summary>
		public virtual eActiveWeaponSlot ActiveWeaponSlot
		{
			get { return m_activeWeaponSlot; }
		}
		/// <summary>
		/// Gets a hashtable holding
		/// gameobject->float
		/// key-value pairs that will define how much
		/// XP these objects get when this n
		/// </summary>
		public virtual HybridDictionary XPGainers
		{
			get
			{
				return m_xpGainers;
			}
		}

		/// <summary>
		/// Create a pet for this living
		/// </summary>
		/// <param name="template"></param>
		/// <returns></returns>
		public virtual GamePet CreateGamePet(INpcTemplate template)
		{
			return new GamePet(template);
		}

		/// <summary>
		/// A new pet has been summoned, do we do anything?
		/// </summary>
		/// <param name="pet"></param>
		public virtual void OnPetSummoned(GamePet pet)
		{
		}


		/// <summary>
		/// last attack tick in either pve or pvp
		/// </summary>
		public virtual long LastAttackTick
		{
			get
			{
				if (m_lastAttackTickPvE > m_lastAttackTickPvP)
					return m_lastAttackTickPvE;
				return m_lastAttackTickPvP;
			}
		}

		/// <summary>
		/// last attack tick for pve
		/// </summary>
		protected long m_lastAttackTickPvE;
		/// <summary>
		/// gets/sets gametick when this living has attacked its target in pve
		/// </summary>
		public virtual long LastAttackTickPvE
		{
			get { return m_lastAttackTickPvE; }
			set
			{
				m_lastAttackTickPvE = value;
				if (this is GameNPC)
				{
					if ((this as GameNPC).Brain is IControlledBrain)
					{
						((this as GameNPC).Brain as IControlledBrain).Owner.LastAttackTickPvE = value;
					}
				}
			}
		}

		/// <summary>
		/// last attack tick for pvp
		/// </summary>
		protected long m_lastAttackTickPvP;
		/// <summary>
		/// gets/sets gametick when this living has attacked its target in pvp
		/// </summary>
		public virtual long LastAttackTickPvP
		{
			get { return m_lastAttackTickPvP; }
			set
			{
				m_lastAttackTickPvP = value;
				if (this is GameNPC)
				{
					if ((this as GameNPC).Brain is IControlledBrain)
					{
						((this as GameNPC).Brain as IControlledBrain).Owner.LastAttackTickPvP = value;
					}
				}
			}
		}

		/// <summary>
		/// gets the last attack or attackedbyenemy tick in pvp
		/// </summary>
		public long LastCombatTickPvP
		{
			get
			{
				if (m_lastAttackTickPvP > m_lastAttackedByEnemyTickPvP)
					return m_lastAttackTickPvP;
				else return m_lastAttackedByEnemyTickPvP;
			}
		}

		/// <summary>
		/// gets the last attack or attackedbyenemy tick in pve
		/// </summary>
		public long LastCombatTickPvE
		{
			get
			{
				if (m_lastAttackTickPvE > m_lastAttackedByEnemyTickPvE)
					return m_lastAttackTickPvE;
				else return m_lastAttackedByEnemyTickPvE;
			}
		}

		/// <summary>
		/// last attacked by enemy tick in either pvp or pve
		/// </summary>
		public virtual long LastAttackedByEnemyTick
		{
			get
			{
				if (m_lastAttackedByEnemyTickPvP > m_lastAttackedByEnemyTickPvE)
					return m_lastAttackedByEnemyTickPvP;
				return m_lastAttackedByEnemyTickPvE;
			}
		}

		/// <summary>
		/// last attacked by enemy tick in pve
		/// </summary>
		protected long m_lastAttackedByEnemyTickPvE;
		/// <summary>
		/// gets/sets gametick when this living was last time attacked by an enemy in pve
		/// </summary>
		public virtual long LastAttackedByEnemyTickPvE
		{
			get { return m_lastAttackedByEnemyTickPvE; }
			set
			{
				m_lastAttackedByEnemyTickPvE = value;
				if (this is GameNPC)
				{
					if ((this as GameNPC).Brain is IControlledBrain)
					{
						((this as GameNPC).Brain as IControlledBrain).Owner.LastAttackedByEnemyTickPvE = value;
					}
				}
			}
		}

		/// <summary>
		/// last attacked by enemy tick in pve
		/// </summary>
		protected long m_lastAttackedByEnemyTickPvP;
		/// <summary>
		/// gets/sets gametick when this living was last time attacked by an enemy in pvp
		/// </summary>
		public virtual long LastAttackedByEnemyTickPvP
		{
			get { return m_lastAttackedByEnemyTickPvP; }
			set
			{
				m_lastAttackedByEnemyTickPvP = value;
				if (this is GameNPC)
				{
					if ((this as GameNPC).Brain is IControlledBrain)
					{
						((this as GameNPC).Brain as IControlledBrain).Owner.LastAttackedByEnemyTickPvP = value;
					}
				}
			}
		}

		/// <summary>
		/// Total damage RvR Value
		/// </summary>
		protected long m_damageRvRMemory;
		/// <summary>
		/// gets the DamageRvR Memory of this living (always 0 for Gameliving)
		/// </summary>
		public virtual long DamageRvRMemory
		{
			get { return 0; }
			set
			{
				m_damageRvRMemory = 0;
			}
		}

		/// <summary>
		/// Gets the swing time left
		/// </summary>
		public virtual int SwingTimeLeft
		{
			get { return (m_attackAction != null && m_attackAction.IsAlive) ? m_attackAction.TimeUntilElapsed : 0; }
		}
		/// <summary>
		/// Decides which style living will use in this moment
		/// </summary>
		/// <returns>Style to use or null if none</returns>
		protected virtual Style GetStyleToUse()
		{
			InventoryItem weapon;
			if (NextCombatStyle == null) return null;
			if (NextCombatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
				weapon = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			else weapon = AttackWeapon;

			if (StyleProcessor.CanUseStyle(this, NextCombatStyle, weapon))
				return NextCombatStyle;

			if (NextCombatBackupStyle == null) return NextCombatStyle;

			return NextCombatBackupStyle;
		}

		/// <summary>
		/// Holds the Style that this living should use next
		/// </summary>
		protected Style m_nextCombatStyle;
		/// <summary>
		/// Holds the backup style for the style that the living should use next
		/// </summary>
		protected Style m_nextCombatBackupStyle;
		
		/// <summary>
		/// Gets or Sets the next combat style to use
		/// </summary>
		public Style NextCombatStyle
		{
			get { return m_nextCombatStyle; }
			set { m_nextCombatStyle = value; }
		}
		/// <summary>
		/// Gets or Sets the next combat backup style to use
		/// </summary>
		public Style NextCombatBackupStyle
		{
			get { return m_nextCombatBackupStyle; }
			set { m_nextCombatBackupStyle = value; }
		}

		/// <summary>
		/// Gets the current attackspeed of this living in milliseconds
		/// </summary>
		/// <param name="weapon">attack weapons</param>
		/// <returns>effective speed of the attack. average if more than one weapon.</returns>
		public virtual int AttackSpeed(params InventoryItem[] weapon)
		{
			double speed = 3000 * (1.0 - (GetModified(eProperty.Quickness) - 60) / 500.0);

			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				speed *= 1.5; // mob archer speed too fast
				
				// Old archery uses archery speed, but new archery uses casting speed
				if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
				    speed *= 1.0 - GetModified(eProperty.ArcherySpeed) * 0.01;
				else
				    speed *= 1.0 - GetModified(eProperty.CastingSpeed) * 0.01;
			}
			else
			{
				speed *= GetModified(eProperty.MeleeSpeed) * 0.01;
			}

			return (int) Math.Max(500.0, speed);
		}
		/// <summary>
		/// Returns the Damage this Living does on an attack
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		/// <returns></returns>
		public virtual double AttackDamage(InventoryItem weapon)
		{
			double effectiveness = 1.00;
			//double effectiveness = Effectiveness;
			double damage = (1.0 + Level / 3.7 + Level * Level / 175.0) * AttackSpeed(weapon) * 0.001;
			if (weapon == null || weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
			{
				//Melee damage buff,debuff,RA
				effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
			}
			else if (weapon.Item_Type == Slot.RANGED && (weapon.Object_Type == (int)eObjectType.Longbow || weapon.Object_Type == (int)eObjectType.RecurvedBow || weapon.Object_Type == (int)eObjectType.CompositeBow))
			{
				// RDSandersJR: Check to see if we are using old archery if so, use RangedDamge
				if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == true)
				{
					effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
				}
				// RDSandersJR: If we are NOT using old archery it should be SpellDamage
				else if (ServerProperties.Properties.ALLOW_OLD_ARCHERY == false)
				{
					effectiveness += GetModified(eProperty.SpellDamage) * 0.01;
				}
			}
			else if (weapon.Item_Type == Slot.RANGED)
			{
				effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
			}
			damage *= effectiveness;
			return damage;
		}

		/// <summary>
		/// Max. Damage possible without style
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		/// <returns></returns>
		public virtual double UnstyledDamageCap(InventoryItem weapon)
		{
			return AttackDamage(weapon) * (2.82 + 0.00009 * AttackSpeed(weapon));
		}

		/// <summary>
		/// Minimum reduction possible to spell casting speed (CastTime * CastingSpeedCap)
		/// </summary>
		public virtual double CastingSpeedReductionCap
		{
			get { return 0.4; }
		}

		/// <summary>
		/// Minimum casting speed allowed, in ticks (milliseconds)
		/// </summary>
		public virtual int MinimumCastingSpeed
		{
			get { return 500; }
		}

		/// <summary>
		/// Can this living cast the given spell while in combat?
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public virtual bool CanCastInCombat(Spell spell)
		{
			// by default npc's can start casting spells while in combat
			return true;
		}


		/// <summary>
		/// Calculate how fast this living can cast a given spell
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public virtual int CalculateCastingTime(SpellLine line, Spell spell)
		{
			int ticks = spell.CastTime;

			if (spell.InstrumentRequirement != 0 ||
			    line.KeyName == GlobalSpellsLines.Item_Spells ||
			    line.KeyName.StartsWith(GlobalSpellsLines.Champion_Lines_StartWith))
			{
				return ticks;
			}


			double percent = DexterityCastTimeReduction;

			ticks = (int)(ticks * Math.Max(CastingSpeedReductionCap, percent));
			if (ticks < MinimumCastingSpeed)
				ticks = MinimumCastingSpeed;

			return ticks;
		}

		/// <summary>
		/// The casting time reduction based on dexterity bonus.
		/// http://daoc.nisrv.com/modules.php?name=DD_DMG_Calculator
		/// Q: Would you please give more detail as to how dex affects a caster?
		/// For instance, I understand that when I have my dex maxed I will cast 25% faster.
		/// How does this work incrementally? And will a lurikeen be able to cast faster in the end than another race?
		/// A: From a dex of 50 to a dex of 250, the formula lets you cast 1% faster for each ten points.
		/// From a dex of 250 to the maximum possible (which as you know depends on your starting total),
		/// your speed increases 1% for every twenty points.
		/// </summary>
		public virtual double DexterityCastTimeReduction
		{
			get
			{
				int dex = GetModified(eProperty.Dexterity);
				if (dex < 60) return 1.0;
				else if (dex < 250) return 1.0 - (dex - 60) * 0.15 * 0.01;
				else return 1.0 - ((dex - 60) * 0.15 + (dex - 250) * 0.05) * 0.01;
			}
		}


		/// <summary>
		/// Returns the AttackRange of this living
		/// </summary>
		public virtual int AttackRange
		{
			get
			{
				//Mobs have a good distance range with distance weapons
				//automatically
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					return Math.Max(32, (int)(2000.0 * GetModified(eProperty.ArcheryRange) * 0.01));
				}
				//Normal mob attacks have 200 ...
				//TODO dragon, big mobs etc...
				return 200;
			}

			set { }
		}

		/// <summary>
		/// calculates weapon stat
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public virtual int GetWeaponStat(InventoryItem weapon)
		{
			return GetModified(eProperty.Strength);
		}

		/// <summary>
		/// calculate item armor factor influenced by quality, con and duration
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public virtual double GetArmorAF(eArmorSlot slot)
		{
			return GetModified(eProperty.ArmorFactor);
		}

		/// <summary>
		/// Calculates armor absorb level
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public virtual double GetArmorAbsorb(eArmorSlot slot)
		{
			return GetModified(eProperty.ArmorAbsorption) * 0.01;
		}

		/// <summary>
		/// Gets the weaponskill of weapon
		/// </summary>
		public virtual double GetWeaponSkill(InventoryItem weapon)
		{
			const double bs = 128.0 / 50.0;	// base factor (not 400)
			return (int)((Level + 1) * bs * (1 + (GetWeaponStat(weapon) - 50) * 0.005) * Level * 2 / 50);
		}

		/// <summary>
		/// Returns the weapon used to attack, null=natural
		/// </summary>
		public virtual InventoryItem AttackWeapon
		{
			get
			{
				if (Inventory != null)
				{
					switch (ActiveWeaponSlot)
					{
							case eActiveWeaponSlot.Standard: return Inventory.GetItem(eInventorySlot.RightHandWeapon);
							case eActiveWeaponSlot.TwoHanded: return Inventory.GetItem(eInventorySlot.TwoHandWeapon);
							case eActiveWeaponSlot.Distance: return Inventory.GetItem(eInventorySlot.DistanceWeapon);
					}
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the chance for a critical hit
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public virtual int AttackCriticalChance(InventoryItem weapon)
		{
			return 0;
		}
		/// <summary>
		/// Returns the chance for a critical hit with a spell
		/// </summary>
		public virtual int SpellCriticalChance
		{
			get { return GetModified(eProperty.CriticalSpellHitChance); }
			set { }
		}
		/// <summary>
		/// Returns the damage type of the current attack
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public virtual eDamageType AttackDamageType(InventoryItem weapon)
		{
			return eDamageType.Natural;
		}

		/// <summary>
		/// Gets the attack-state of this living
		/// </summary>
		public virtual bool AttackState { get; protected set; }

		/// <summary>
		/// Whether or not the living can be attacked.
		/// </summary>
		public override bool IsAttackable
		{
			get
			{
				return (IsAlive &&
				        !IsStealthed &&
				        EffectList.GetOfType<NecromancerShadeEffect>() == null &&
				        ObjectState == GameObject.eObjectState.Active);
			}
		}

		/// <summary>
		/// Whether the living is actually attacking something.
		/// </summary>
		public virtual bool IsAttacking
		{
			get { return (AttackState && (m_attackAction != null) && m_attackAction.IsAlive); }
		}

		/// <summary>
		/// Gets the effective AF of this living
		/// </summary>
		public virtual int EffectiveOverallAF
		{
			get { return 0; }
		}

		/// <summary>
		/// determines the spec level for current AttackWeapon
		/// </summary>
		public virtual int WeaponSpecLevel(InventoryItem weapon)
		{
			if (weapon == null) return 0;

			return 0;	// TODO
		}

		/// <summary>
		/// Gets the weapondamage of currently used weapon
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		public virtual double WeaponDamage(InventoryItem weapon)
		{
			return 0;
		}

		/// <summary>
		/// Whether this living is crowd controlled.
		/// </summary>
		public virtual bool IsCrowdControlled
		{
			get
			{
				return (IsStunned || IsMezzed);
			}
		}

		/// <summary>
		/// Whether this living can actually do anything.
		/// </summary>
		public virtual bool IsIncapacitated
		{
			get
			{
				return (ObjectState != eObjectState.Active || !IsAlive || IsStunned || IsMezzed);
			}
		}
		
		/// <summary>
		/// returns if this living is alive
		/// </summary>
		public virtual bool IsAlive
		{
			get { return Health > 0; }
		}

		/// <summary>
		/// True if living is low on health, else false.
		/// </summary>
		public virtual bool IsLowHealth
		{
			get
			{
				return (Health < 0.1 * MaxHealth);
			}
		}

		protected bool m_isMuted = false;
		/// <summary>
		/// returns if this living is muted
		/// </summary>
		public virtual bool IsMuted
		{
			get { return m_isMuted; }
			set
			{
				m_isMuted = value;
			}
		}

		/// <summary>
		/// Check this flag to see if this living is involved in combat
		/// </summary>
		public virtual bool InCombat
		{
			get
			{
				if ((InCombatPvE || InCombatPvP) == false)
				{
					if (Attackers.Count > 0)
					{
						Attackers.Clear();
					}

					return false;
				}

				return true;
			}
		}

		/// <summary>
		/// Check this flag to see if this living has been involved in combat in the given milliseconds
		/// </summary>
		public virtual bool InCombatInLast(int milliseconds)
		{
			if ((InCombatPvEInLast(milliseconds) || InCombatPvPInLast(milliseconds)) == false)
			{
				if (Attackers.Count > 0)
				{
					Attackers.Clear();
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// checks if the living is involved in pvp combat
		/// </summary>
		public virtual bool InCombatPvP
		{
			get
			{
				Region region = CurrentRegion;
				if (region == null)
					return false;

				if (LastCombatTickPvP == 0)
					return false;

				return LastCombatTickPvP + 10000 >= region.Time;
			}
		}

		/// <summary>
		/// checks if the living is involved in pvp combat in the given milliseconds
		/// </summary>
		public virtual bool InCombatPvPInLast(int milliseconds)
		{
			Region region = CurrentRegion;
			if (region == null)
				return false;

			if (LastCombatTickPvP == 0)
				return false;

			return LastCombatTickPvP + milliseconds >= region.Time;
		}

		/// <summary>
		/// checks if the living is involved in pve combat
		/// </summary>
		public virtual bool InCombatPvE
		{
			get
			{
				Region region = CurrentRegion;
				if (region == null)
					return false;

				if (LastCombatTickPvE == 0)
					return false;

				//if (LastCombatTickPvE + 10000 - region.Time > 0 && this is GameNPC && (this as GameNPC).Brain is IControlledBrain)
				//	log.Debug(Name + " in combat " + (LastCombatTickPvE + 10000 - region.Time));

				return LastCombatTickPvE + 10000 >= region.Time;
			}
		}

		/// <summary>
		/// checks if the living is involved in pve combat in the given milliseconds
		/// </summary>
		public virtual bool InCombatPvEInLast(int milliseconds)
		{
			Region region = CurrentRegion;
			if (region == null)
				return false;

			if (LastCombatTickPvE == 0)
				return false;

			//if (LastCombatTickPvE + 10000 - region.Time > 0 && this is GameNPC && (this as GameNPC).Brain is IControlledBrain)
			//	log.Debug(Name + " in combat " + (LastCombatTickPvE + 10000 - region.Time));

			return LastCombatTickPvE + milliseconds >= region.Time;
		}

		/// <summary>
		/// Returns the amount of experience this living is worth
		/// </summary>
		public virtual long ExperienceValue
		{
			get
			{
				return GetExperienceValueForLevel(Level);
			}
		}

		/// <summary>
		/// Realm point value of this living
		/// </summary>
		public virtual int RealmPointsValue
		{
			get { return 0; }
		}

		/// <summary>
		/// Bounty point value of this living
		/// </summary>
		public virtual int BountyPointsValue
		{
			get { return 0; }
		}

		/// <summary>
		/// Money value of this living
		/// </summary>
		public virtual long MoneyValue
		{
			get { return 0; }
		}

		/// <summary>
		/// How much over the XP cap can this living reward.
		/// 1.0 = none
		/// 2.0 = twice cap
		/// etc.
		/// </summary>
		public virtual double ExceedXPCapAmount
		{
			get { return 1.0; }
		}

		#region XP array

		/// <summary>
		/// Holds pre calculated experience values of the living for special levels
		/// </summary>
		public static readonly long[] XPForLiving =
		{
			// noret: first 52 are from exp table, think mythic has changed some values
			// cause they don't fit the formula; rest are calculated.
			// with this formula group with 8 lv50 players should hit cap on lv67 mobs what looks about correct
			// http://www.daocweave.com/daoc/general/experience_table.htm
			5,					// xp for level 0
			10,					// xp for level 1
			20,					// xp for level 2
			40,					// xp for level 3
			80,					// xp for level 4
			160,				// xp for level 5
			320,				// xp for level 6
			640,				// xp for level 7
			1280,				// xp for level 8
			2560,				// xp for level 9
			5120,				// xp for level 10
			7240,				// xp for level 11
			10240,				// xp for level 12
			14480,				// xp for level 13
			20480,				// xp for level 14
			28980,				// xp for level 15
			40960,				// xp for level 16
			57930,				// xp for level 17
			81920,				// xp for level 18
			115850,				// xp for level 19
			163840,				// xp for level 20
			206435,				// xp for level 21
			231705,				// xp for level 22
			327680,				// xp for level 23
			412850,				// xp for level 24
			520160,				// xp for level 25
			655360,				// xp for level 26
			825702,				// xp for level 27
			1040319,			// xp for level 28
			1310720,			// xp for level 29
			1651404,			// xp for level 30
			2080638,			// xp for level 31
			2621440,			// xp for level 32
			3302807,			// xp for level 33
			4161277,			// xp for level 34
			5242880,			// xp for level 35
			6022488,			// xp for level 36
			6918022,			// xp for level 37
			7946720,			// xp for level 38
			9128384,			// xp for level 39
			10485760,			// xp for level 40
			12044975,			// xp for level 41
			13836043,			// xp for level 42
			15893440,			// xp for level 43
			18258769,			// xp for level 44
			20971520,			// xp for level 45
			24089951,			// xp for level 46
			27672087,			// xp for level 47
			31625241,			// xp for level 48; sshot505.tga
			36513537,			// xp for level 49
			41943040,			// xp for level 50
			48179911,			// xp for level 51
			52428800,			// xp for level 52
			63573760,			// xp for level 53
			73027074,			// xp for level 54
			83886080,			// xp for level 55
			96359802,			// xp for level 56
			110688346,			// xp for level 57
			127147521,			// xp for level 58
			146054148,			// xp for level 59
			167772160,			// xp for level 60
			192719604,			// xp for level 61
			221376692,			// xp for level 62
			254295042,			// xp for level 63
			292108296,			// xp for level 64
			335544320,			// xp for level 65
			385439208,			// xp for level 66
			442753384,			// xp for level 67
			508590084,			// xp for level 68
			584216593,			// xp for level 69
			671088640,			// xp for level 70
			770878416,			// xp for level 71
			885506769,			// xp for level 72
			1017180169,			// xp for level 73
			1168433187,			// xp for level 74
			1342177280,			// xp for level 75
			1541756833,			// xp for level 76
			1771013538,			// xp for level 77
			2034360338,			// xp for level 78
			2336866374,			// xp for level 79
			2684354560,			// xp for level 80
			3083513667,			// xp for level 81
			3542027077,			// xp for level 82
			4068720676,			// xp for level 83
			4673732748,			// xp for level 84
			5368709120,			// xp for level 85
			6167027334,			// xp for level 86
			7084054154,			// xp for level 87
			8137441353,			// xp for level 88
			9347465497,			// xp for level 89
			10737418240,		// xp for level 90
			12334054669,		// xp for level 91
			14168108308,		// xp for level 92
			16274882707,		// xp for level 93
			18694930994,		// xp for level 94
			21474836480,		// xp for level 95
			24668109338,		// xp for level 96
			28336216617,		// xp for level 97
			32549765415,		// xp for level 98
			37389861988,		// xp for level 99
			42949672960			// xp for level 100
		};

		/// <summary>
		/// Holds the level of target at which no exp is given
		/// </summary>
		public static readonly int[] NoXPForLevel =
		{
			-3,		//for level 0
			-2,		//for level 1
			-1,		//for level 2
			0,		//for level 3
			1,		//for level 4
			2,		//for level 5
			3,		//for level 6
			4,		//for level 7
			5,		//for level 8
			6,		//for level 9
			6,		//for level 10
			6,		//for level 11
			6,		//for level 12
			7,		//for level 13
			8,		//for level 14
			9,		//for level 15
			10,		//for level 16
			11,		//for level 17
			12,		//for level 18
			13,		//for level 19
			13,		//for level 20
			13,		//for level 21
			13,		//for level 22
			14,		//for level 23
			15,		//for level 24
			16,		//for level 25
			17,		//for level 26
			18,		//for level 27
			19,		//for level 28
			20,		//for level 29
			21,		//for level 30
			22,		//for level 31
			23,		//for level 32
			24,		//for level 33
			25,		//for level 34
			25,		//for level 35
			25,		//for level 36
			25,		//for level 37
			25,		//for level 38
			25,		//for level 39
			25,		//for level 40
			26,		//for level 41
			27,		//for level 42
			28,		//for level 43
			29,		//for level 44
			30,		//for level 45
			31,		//for level 46
			32,		//for level 47
			33,		//for level 48
			34,		//for level 49
			35,		//for level 50
		};

		#endregion

		/// <summary>
		/// Checks whether object is grey con to this living
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public virtual bool IsObjectGreyCon(GameObject obj)
		{
			return IsObjectGreyCon(this, obj);
		}

		/// <summary>
		/// Checks whether target is grey con to source
		/// </summary>
		/// <param name="source"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		static public bool IsObjectGreyCon(GameObject source, GameObject target)
		{
			int sourceLevel = source.EffectiveLevel;
			if (sourceLevel < GameLiving.NoXPForLevel.Length)
			{
				//if target level is less or equals to level that is grey to source
				if (target.EffectiveLevel <= GameLiving.NoXPForLevel[sourceLevel])
					return true;
			}
			else
			{
				if (source.GetConLevel(target) <= -3)
					return true;
			}
			return false;
		}

		/// <summary>
		/// Calculates the experience value of this living for special levels
		/// </summary>
		/// <param name="level"></param>
		/// <returns></returns>
		public virtual long GetExperienceValueForLevel(int level)
		{
			return GameServer.ServerRules.GetExperienceForLiving(level);
		}

		/// <summary>
		/// Gets/sets the targetObject's visibility
		/// </summary>
		public virtual bool TargetInView
		{
			get
			{
				//always in view for mobs
				return true;
			}
			set
			{
			}
		}

		/// <summary>
		/// Gets or sets the GroundTargetObject's visibility
		/// </summary>
		public virtual bool GroundTargetInView
		{
			get { return true; }
			set { }
		}

		/// <summary>
		/// This method is called to make an attack, it is called from the
		/// attacktimer and should not be called manually
		/// </summary>
		/// <param name="target">the target that is attacked</param>
		/// <param name="weapon">the weapon used for attack</param>
		/// <param name="style">the style used for attack</param>
		/// <param name="effectiveness">damage effectiveness (0..1)</param>
		/// <param name="interruptDuration">the interrupt duration</param>
		/// <param name="dualWield">indicates if both weapons are used for attack</param>
		/// <returns>the object where we collect and modifiy all parameters about the attack</returns>
		protected virtual AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
		{
			return MakeAttack(target, weapon, style, effectiveness, interruptDuration, dualWield, false);
		}


		protected virtual AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield, bool ignoreLOS)
		{
			AttackData ad = new AttackData();
			ad.Attacker = this;
			ad.Target = target as GameLiving;
			ad.Damage = 0;
			ad.CriticalDamage = 0;
			ad.Style = style;
			ad.WeaponSpeed = AttackSpeed(weapon) / 100;
			ad.DamageType = AttackDamageType(weapon);
			ad.ArmorHitLocation = eArmorSlot.NOTSET;
			ad.Weapon = weapon;
			ad.IsOffHand = weapon == null ? false : weapon.Hand == 2;


			if (dualWield)
				ad.AttackType = AttackData.eAttackType.MeleeDualWield;
			else if (weapon == null)
				ad.AttackType = AttackData.eAttackType.MeleeOneHand;
			else switch (weapon.Item_Type)
			{
				default:
				case Slot.RIGHTHAND:
					case Slot.LEFTHAND: ad.AttackType = AttackData.eAttackType.MeleeOneHand; break;
					case Slot.TWOHAND: ad.AttackType = AttackData.eAttackType.MeleeTwoHand; break;
					case Slot.RANGED: ad.AttackType = AttackData.eAttackType.Ranged; break;
			}

			//No target, stop the attack
			if (ad.Target == null)
			{
				ad.AttackResult = (target == null) ? eAttackResult.NoTarget : eAttackResult.NoValidTarget;
				return ad;
			}

			// check region
			if (ad.Target.CurrentRegionID != CurrentRegionID || ad.Target.ObjectState != eObjectState.Active)
			{
				ad.AttackResult = eAttackResult.NoValidTarget;
				return ad;
			}

			//Check if the target is in front of attacker
			if (!ignoreLOS && ad.AttackType != AttackData.eAttackType.Ranged && this is GamePlayer &&
			    !(ad.Target is GameKeepComponent) && !(IsObjectInFront(ad.Target, 120, true) && TargetInView))
			{
				ad.AttackResult = eAttackResult.TargetNotVisible;
				return ad;
			}

			//Target is dead already
			if (!ad.Target.IsAlive)
			{
				ad.AttackResult = eAttackResult.TargetDead;
				return ad;
			}
			//We have no attacking distance!
			if (!this.IsWithinRadius(ad.Target, ad.Target.ActiveWeaponSlot == eActiveWeaponSlot.Standard ? Math.Max(AttackRange, ad.Target.AttackRange) : AttackRange))
			{
				ad.AttackResult = eAttackResult.OutOfRange;
				return ad;
			}

			if (RangedAttackType == eRangedAttackType.Long)
			{
				RangedAttackType = eRangedAttackType.Normal;
			}

			if (!GameServer.ServerRules.IsAllowedToAttack(ad.Attacker, ad.Target, false))
			{
				ad.AttackResult = eAttackResult.NotAllowed_ServerRules;
				return ad;
			}

			if (SpellHandler.FindEffectOnTarget(this, "Phaseshift") != null)
			{
				ad.AttackResult = eAttackResult.Phaseshift;
				return ad;
			}

			// Apply Mentalist RA5L
			SelectiveBlindnessEffect SelectiveBlindness = EffectList.GetOfType<SelectiveBlindnessEffect>();
			if (SelectiveBlindness != null)
			{
				GameLiving EffectOwner = SelectiveBlindness.EffectSource;
				if (EffectOwner == ad.Target)
				{
					if (this is GamePlayer)
						((GamePlayer)this).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)this).Client.Account.Language, "GameLiving.AttackData.InvisibleToYou"), ad.Target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					ad.AttackResult = eAttackResult.NoValidTarget;
					return ad;
				}
			}

			// DamageImmunity Ability
			if ((GameLiving)target != null && ((GameLiving)target).HasAbility(Abilities.DamageImmunity))
			{
				//if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage(string.Format("{0} can't be attacked!", ad.Target.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
				ad.AttackResult = eAttackResult.NoValidTarget;
				return ad;
			}


			//Calculate our attack result and attack damage
			ad.AttackResult = ad.Target.CalculateEnemyAttackResult(ad, weapon);

			// calculate damage only if we hit the target
			if (ad.AttackResult == eAttackResult.HitUnstyled
			    || ad.AttackResult == eAttackResult.HitStyle)
			{
				double damage = AttackDamage(weapon) * effectiveness;

				if (Level > ServerProperties.Properties.MOB_DAMAGE_INCREASE_STARTLEVEL &&
				    ServerProperties.Properties.MOB_DAMAGE_INCREASE_PERLEVEL > 0 &&
				    damage > 0 &&
				    this is GameNPC && (this as GameNPC).Brain is IControlledBrain == false)
				{
					double modifiedDamage = ServerProperties.Properties.MOB_DAMAGE_INCREASE_PERLEVEL * (Level - ServerProperties.Properties.MOB_DAMAGE_INCREASE_STARTLEVEL);
					damage += (modifiedDamage * effectiveness);
				}

				InventoryItem armor = null;

				if (ad.Target.Inventory != null)
					armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

				InventoryItem weaponTypeToUse = null;

				if (weapon != null)
				{
					weaponTypeToUse = new InventoryItem();
					weaponTypeToUse.Object_Type = weapon.Object_Type;
					weaponTypeToUse.SlotPosition = weapon.SlotPosition;

					if ((this is GamePlayer) && Realm == eRealm.Albion
						&& (GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.TwoHandedWeapon) 
						|| GameServer.ServerRules.IsObjectTypesEqual((eObjectType)weapon.Object_Type, eObjectType.PolearmWeapon))
						&& ServerProperties.Properties.ENABLE_ALBION_ADVANCED_WEAPON_SPEC)
					{
						// Albion dual spec penalty, which sets minimum damage to the base damage spec
						if (weapon.Type_Damage == (int)eDamageType.Crush)
						{
							weaponTypeToUse.Object_Type = (int)eObjectType.CrushingWeapon;
						}
						else if (weapon.Type_Damage == (int)eDamageType.Slash)
						{
							weaponTypeToUse.Object_Type = (int)eObjectType.SlashingWeapon;
						}
						else
						{
							weaponTypeToUse.Object_Type = (int)eObjectType.ThrustWeapon;
						}
					}
				}

				int lowerboundary = (WeaponSpecLevel(weaponTypeToUse) - 1) * 50 / (ad.Target.EffectiveLevel + 1) + 75;
				lowerboundary = Math.Max(lowerboundary, 75);
				lowerboundary = Math.Min(lowerboundary, 125);
				damage *= (GetWeaponSkill(weapon) + 90.68) / (ad.Target.GetArmorAF(ad.ArmorHitLocation) + 20 * 4.67);

				// Badge Of Valor Calculation 1+ absorb or 1- absorb
				if (ad.Attacker.EffectList.GetOfType<BadgeOfValorEffect>() != null)
				{
					damage *= 1.0 + Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
				}
				else
				{
					damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
				}
				damage *= (lowerboundary + Util.Random(50)) * 0.01;
				ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) * -0.01);
				damage += ad.Modifier;
				// RA resist check
				int resist = (int)(damage * ad.Target.GetDamageResist(GetResistTypeForDamage(ad.DamageType)) * -0.01);

				eProperty property = ad.Target.GetResistTypeForDamage(ad.DamageType);
				int secondaryResistModifier = ad.Target.SpecBuffBonusCategory[(int)property];
				int resistModifier = 0;
				resistModifier += (int)((ad.Damage + (double)resistModifier) * (double)secondaryResistModifier * -0.01);

				damage += resist;
				damage += resistModifier;
				ad.Modifier += resist;
				ad.Damage = (int)damage;

				// apply total damage cap
				ad.UncappedDamage = ad.Damage;
				ad.Damage = Math.Min(ad.Damage, (int)(UnstyledDamageCap(weapon) * effectiveness));

				if ((this is GamePlayer || (this is GameNPC && (this as GameNPC).Brain is IControlledBrain && this.Realm != 0)) && target is GamePlayer)
				{
					ad.Damage = (int)((double)ad.Damage * ServerProperties.Properties.PVP_MELEE_DAMAGE);
				}
				else if ((this is GamePlayer || (this is GameNPC && (this as GameNPC).Brain is IControlledBrain && this.Realm != 0)) && target is GameNPC)
				{
					ad.Damage = (int)((double)ad.Damage * ServerProperties.Properties.PVE_MELEE_DAMAGE);
				}

				ad.UncappedDamage = ad.Damage;

				//Eden - Conversion Bonus (Crocodile Ring)  - tolakram - critical damage is always 0 here, needs to be moved
				if (ad.Target is GamePlayer && ad.Target.GetModified(eProperty.Conversion) > 0)
				{
					int manaconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 100);
					//int enduconversion=(int)Math.Round((double)manaconversion*(double)ad.Target.MaxEndurance/(double)ad.Target.MaxMana);
					int enduconversion = (int)Math.Round(((double)ad.Damage + (double)ad.CriticalDamage) * (double)ad.Target.GetModified(eProperty.Conversion) / 100);
					if (ad.Target.Mana + manaconversion > ad.Target.MaxMana) manaconversion = ad.Target.MaxMana - ad.Target.Mana;
					if (ad.Target.Endurance + enduconversion > ad.Target.MaxEndurance) enduconversion = ad.Target.MaxEndurance - ad.Target.Endurance;
					if (manaconversion < 1) manaconversion = 0;
					if (enduconversion < 1) enduconversion = 0;
					if (manaconversion >= 1) (ad.Target as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((ad.Target as GamePlayer).Client.Account.Language, "GameLiving.AttackData.GainPowerPoints"), manaconversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					if (enduconversion >= 1) (ad.Target as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((ad.Target as GamePlayer).Client.Account.Language, "GameLiving.AttackData.GainEndurancePoints"), enduconversion), eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					ad.Target.Endurance += enduconversion; if (ad.Target.Endurance > ad.Target.MaxEndurance) ad.Target.Endurance = ad.Target.MaxEndurance;
					ad.Target.Mana += manaconversion; if (ad.Target.Mana > ad.Target.MaxMana) ad.Target.Mana = ad.Target.MaxMana;
				}

				// Tolakram - let's go ahead and make it 1 damage rather than spamming a possible error
				if (ad.Damage == 0)
				{
					ad.Damage = 1;

					// log this as a possible error if we should do some damage to target
					//if (ad.Target.Level <= Level + 5 && weapon != null)
					//{
					//    log.ErrorFormat("Possible Damage Error: {0} Damage = 0 -> miss vs {1}.  AttackDamage {2}, weapon name {3}", Name, (ad.Target == null ? "null" : ad.Target.Name), AttackDamage(weapon), (weapon == null ? "None" : weapon.Name));
					//}

					//ad.AttackResult = eAttackResult.Missed;
				}
			}

			//Add styled damage if style hits and remove endurance if missed
			if (StyleProcessor.ExecuteStyle(this, ad, weapon))
			{
				ad.AttackResult = GameLiving.eAttackResult.HitStyle;
			}

			if ((ad.AttackResult == eAttackResult.HitUnstyled || ad.AttackResult == eAttackResult.HitStyle))
			{
				ad.CriticalDamage = GetMeleeCriticalDamage(ad, weapon);
			}

			// Attacked living may modify the attack data.  Primarily used for keep doors and components.
			ad.Target.ModifyAttack(ad);

			if (ad.AttackResult == eAttackResult.HitStyle)
			{
				if (this is GamePlayer)
				{
					GamePlayer player = this as GamePlayer;

					string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage + ")" : "";
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformPerfectly", ad.Style.Name, damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				else if (this is GameNPC)
				{
					ControlledNpcBrain brain = ((GameNPC)this).Brain as ControlledNpcBrain;

					if (brain != null)
					{
						GamePlayer owner = brain.GetPlayerOwner();
						if (owner != null)
						{
							string damageAmount = (ad.StyleDamage > 0) ? " (+" + ad.StyleDamage + ")" : "";
							owner.Out.SendMessage(LanguageMgr.GetTranslation(owner.Client.Account.Language, "StyleProcessor.ExecuteStyle.PerformsPerfectly", Name, ad.Style.Name, damageAmount), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
					}
				}
			}

			string message = "";
			bool broadcast = true;
			ArrayList excludes = new ArrayList();
			excludes.Add(ad.Attacker);
			excludes.Add(ad.Target);

			switch (ad.AttackResult)
			{
					case eAttackResult.Parried: message = string.Format("{0} attacks {1} and is parried!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;
					case eAttackResult.Evaded: message = string.Format("{0} attacks {1} and is evaded!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;
					case eAttackResult.Missed: message = string.Format("{0} attacks {1} and misses!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false)); break;

				case eAttackResult.Blocked:
					{
						message = string.Format("{0} attacks {1} and is blocked!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
						// guard messages
						if (target != null && target != ad.Target)
						{
							excludes.Add(target);

							// another player blocked for real target
							if (target is GamePlayer)
								((GamePlayer)target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "GameLiving.AttackData.BlocksYou"), ad.Target.GetName(0, true), ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

							// blocked for another player
							if (ad.Target is GamePlayer)
							{
								((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.YouBlock"), ad.Attacker.GetName(0, false), target.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								((GamePlayer)ad.Target).Stealth(false);
							}
						}
						else if (ad.Target is GamePlayer)
						{
							((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.AttacksYou"), ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
						}
						break;
					}
				case eAttackResult.HitUnstyled:
				case eAttackResult.HitStyle:
					{
						if (target != null && target != ad.Target)
						{
							message = string.Format("{0} attacks {1} but hits {2}!", ad.Attacker.GetName(0, true), target.GetName(0, false), ad.Target.GetName(0, false));
							excludes.Add(target);

							// intercept for another player
							if (target is GamePlayer)
								((GamePlayer)target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)target).Client.Account.Language, "GameLiving.AttackData.StepsInFront"), ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

							// intercept by player
							if (ad.Target is GamePlayer)
								((GamePlayer)ad.Target).Out.SendMessage(string.Format(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.AttackData.YouStepInFront"), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
						else
						{
							if (ad.Attacker is GamePlayer)
							{
								string hitWeapon = "weapon";
								if (weapon != null)
									hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
								message = string.Format("{0} attacks {1} with {2} {3}!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false), ad.Attacker.GetPronoun(1, false), hitWeapon);
							}
							else
							{
								message = string.Format("{0} attacks {1} and hits!", ad.Attacker.GetName(0, true), ad.Target.GetName(0, false));
							}
						}
						break;
					}
					default: broadcast = false; break;
			}

			#region Prevent Flight
			if (ad.Attacker is GamePlayer)
			{
				GamePlayer attacker = ad.Attacker as GamePlayer;
				if (attacker.HasAbility(Abilities.PreventFlight) && Util.Chance(10))
				{
					if (IsObjectInFront(ad.Target, 120) && ad.Target.IsMoving)
					{
						bool preCheck = false;
						if (ad.Target is GamePlayer) //only start if we are behind the player
						{
							float angle = ad.Target.GetAngle( ad.Attacker );
							if (angle >= 150 && angle < 210) preCheck = true;
						}
						else preCheck = true;

						if (preCheck)
						{
							Spell spell = SkillBase.GetSpellByID(7083);
							if (spell != null)
							{
								ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells));
								if (spellHandler != null)
								{
									spellHandler.StartSpell(ad.Target);
								}
							}
						}
					}
				}
			}
			#endregion

			#region controlled messages

			if (ad.Attacker is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)ad.Attacker).Brain as IControlledBrain;
				if (brain != null)
				{
					GamePlayer owner = brain.GetPlayerOwner();
					if (owner != null)
					{
						excludes.Add(owner);
						switch (ad.AttackResult)
						{
							case eAttackResult.HitStyle:
							case eAttackResult.HitUnstyled:
								{
									string modmessage = "";
									if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
									if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
									string attackTypeMsg = "attacks";
									if (ad.Attacker.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
									{
										attackTypeMsg = "shoots";
									}
									owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.YourHits"), ad.Attacker.Name, attackTypeMsg, ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
									if (ad.CriticalDamage > 0)
									{
										owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.YourCriticallyHits"), ad.Attacker.Name, ad.Target.GetName(0, false), ad.CriticalDamage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
									}

									break;
								}
							default:
								owner.Out.SendMessage(message, eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
								break;
						}
					}
				}
			}

			if (ad.Target is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)ad.Target).Brain as IControlledBrain;
				if (brain != null)
				{
					GameLiving owner_living = brain.GetLivingOwner();
					excludes.Add(owner_living);
					if (owner_living != null && owner_living is GamePlayer && owner_living.ControlledBrain != null && ad.Target == owner_living.ControlledBrain.Body)
					{
						GamePlayer owner = owner_living as GamePlayer;
						switch (ad.AttackResult)
						{
							case eAttackResult.Blocked:
								owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Blocked"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Parried:
								owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Parried"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Evaded:
								owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Evaded"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Fumbled:
								owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Fumbled"), ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Missed:
								if (ad.AttackType != AttackData.eAttackType.Spell)
									owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.Misses"), ad.Attacker.GetName(0, true), ad.Target.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.HitStyle:
							case eAttackResult.HitUnstyled:
								{
									string modmessage = "";
									if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
									if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
									owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.HitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.Damage, modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
									if (ad.CriticalDamage > 0)
									{
										owner.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(owner.Client.Account.Language, "GameLiving.AttackData.CriticallyHitsForDamage"), ad.Attacker.GetName(0, true), ad.Target.Name, ad.CriticalDamage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
									}
									break;
								}
								default: break;
						}
					}
				}
			}

			#endregion

			// broadcast messages
			if (broadcast)
			{
				Message.SystemToArea(ad.Attacker, message, eChatType.CT_OthersCombat, (GameObject[])excludes.ToArray(typeof(GameObject)));
			}

			ad.Target.StartInterruptTimer(ad, interruptDuration);
			//Return the result
			return ad;
		}


		private RegionAction InterruptTimer { get; set; }

		/// <summary>
		/// Starts the interrupt timer on this living.
		/// </summary>
		/// <param name="attack"></param>
		/// <param name="duration"></param>
		public virtual void StartInterruptTimer(AttackData attack, int duration)
		{
			if(attack != null)
				StartInterruptTimer(duration, attack.AttackType, attack.Attacker);
		}

		/// <summary>
		/// Starts the interrupt timer on this living.
		/// </summary>
		/// <param name="duration"></param>
		/// <param name="attackType"></param>
		/// <param name="attacker"></param>
		public virtual void StartInterruptTimer(int duration, AttackData.eAttackType attackType, GameLiving attacker)
		{
			if (!IsAlive || ObjectState != eObjectState.Active)
			{
				InterruptTime = 0;
				InterruptAction = 0;
				return;
			}
			if (InterruptTime < CurrentRegion.Time + duration)
				InterruptTime = CurrentRegion.Time + duration;

			if (CurrentSpellHandler != null)
				CurrentSpellHandler.CasterIsAttacked(attacker);
			
			if (AttackState && ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				OnInterruptTick(attacker, attackType);
		}

		protected long m_interruptTime = 0;
		public virtual long InterruptTime
		{
			get { return m_interruptTime; }
			set
			{
				if (CurrentRegion != null)
					InterruptAction = CurrentRegion.Time;
				m_interruptTime = value;
			}
		}

		protected long m_interruptAction = 0;
		public virtual long InterruptAction
		{
			get { return m_interruptAction; }
			set { m_interruptAction = value; }
		}

		/// <summary>
		/// Yields true if interrupt action is running on this living.
		/// </summary>
		public virtual bool IsBeingInterrupted
		{
			get { return (m_interruptTime > CurrentRegion.Time); }
		}

		/// <summary>
		/// Base chance this living can be interrupted
		/// </summary>
		public virtual int BaseInterruptChance
		{
			get { return 65; }
		}

		/// <summary>
		/// How long does an interrupt last?
		/// </summary>
		public virtual int SpellInterruptDuration
		{
			get { return ServerProperties.Properties.SPELL_INTERRUPT_DURATION; }
		}

		/// <summary>
		/// The amount of time the caster has to wait before being able to cast again
		/// </summary>
		public virtual int SpellInterruptRecastTime
		{
			get { return ServerProperties.Properties.SPELL_INTERRUPT_RECAST; }
		}

		/// <summary>
		/// Additional interrupt time if interrupted again
		/// </summary>
		public virtual int SpellInterruptRecastAgain
		{
			get { return ServerProperties.Properties.SPELL_INTERRUPT_AGAIN; }
		}

		/// <summary>
		/// Does an attacker interrupt this livings cast?
		/// </summary>
		/// <param name="attacker"></param>
		/// <returns></returns>
		public virtual bool ChanceSpellInterrupt(GameLiving attacker)
		{
			double mod = GetConLevel(attacker);
			double chance = BaseInterruptChance;
			chance += mod * 10;
			chance = Math.Max(1, chance);
			chance = Math.Min(99, chance);
			if (attacker is GamePlayer) chance = 99;
			return Util.Chance((int)chance);
		}

		/// <summary>
		/// Does needed interrupt checks and interrupts this living
		/// </summary>
		/// <param name="attacker">the attacker that is interrupting</param>
		/// <param name="attackType">the attack type</param>
		/// <returns>true if interrupted successfully</returns>
		protected virtual bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if (AttackState && ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				if (RangedAttackType == eRangedAttackType.SureShot)
				{
					if (attackType != AttackData.eAttackType.MeleeOneHand
					    && attackType != AttackData.eAttackType.MeleeTwoHand
					    && attackType != AttackData.eAttackType.MeleeDualWield)
						return false;
				}
				double mod = GetConLevel(attacker);
				double interruptChance = BaseInterruptChance;
				interruptChance += mod * 10;
				interruptChance = Math.Max(1, interruptChance);
				interruptChance = Math.Min(99, interruptChance);
				if (Util.Chance((int)interruptChance))
				{
					StopAttack();
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// The possible results for prechecks for range attacks
		/// </summary>
		protected enum eCheckRangeAttackStateResult
		{
			/// <summary>
			/// Hold the shot/throw
			/// </summary>
			Hold,
			/// <summary>
			/// Fire the shot/throw
			/// </summary>
			Fire,
			/// <summary>
			/// Stop the attack
			/// </summary>
			Stop
		}

		/// <summary>
		/// Check the range attack state and decides what to do
		/// Called inside the AttackTimerCallback
		/// </summary>
		/// <returns></returns>
		protected virtual eCheckRangeAttackStateResult CheckRangeAttackState(GameObject target)
		{
			//Standard livings ALWAYS shot and reload automatically!
			return eCheckRangeAttackStateResult.Fire;
		}

		/// <summary>
		/// Gets/Sets the item that is used for ranged attack
		/// </summary>
		/// <returns>Item that will be used for range/accuracy/damage modifications</returns>
		protected virtual InventoryItem RangeAttackAmmo
		{
			get { return null; }
			set { }
		}

		/// <summary>
		/// Gets/Sets the target for current ranged attack
		/// </summary>
		/// <returns></returns>
		protected virtual GameObject RangeAttackTarget
		{
			get { return TargetObject; }
			set { }
		}

		/// <summary>
		/// Creates an attack action for this living
		/// </summary>
		/// <returns></returns>
		protected virtual AttackAction CreateAttackAction()
		{
			return m_attackAction ?? new AttackAction(this);
		}

		/// <summary>
		/// The attack action of this living
		/// </summary>
		protected class AttackAction : RegionAction
		{
			/// <summary>
			/// Constructs a new attack action
			/// </summary>
			/// <param name="owner">The action source</param>
			public AttackAction(GameLiving owner)
				: base(owner)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving owner = (GameLiving)m_actionSource;

				if (owner.IsMezzed || owner.IsStunned)
				{
					Interval = 100;
					return;
				}

				if (owner.IsCasting && !owner.CurrentSpellHandler.Spell.Uninterruptible)
				{
					Interval = 100;
					return;
				}

				if (!owner.AttackState)
				{
					AttackData ad = owner.TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
					owner.TempProperties.removeProperty(LAST_ATTACK_DATA);
					if (ad != null && ad.Target != null)
						ad.Target.RemoveAttacker(owner);
					Stop();
					return;
				}

				// Don't attack if gameliving is engaging
				if (owner.IsEngaging)
				{
					Interval = owner.AttackSpeed(owner.AttackWeapon); // while gameliving is engageing it doesn't attack.
					return;
				}

				// Store all datas which must not change during the attack
				// double effectiveness = 1.0;
				double effectiveness = owner.Effectiveness;
				int ticksToTarget = 1;
				int interruptDuration = 0;
				int leftHandSwingCount = 0;
				Style combatStyle = null;
				InventoryItem attackWeapon = owner.AttackWeapon;
				InventoryItem leftWeapon = (owner.Inventory == null) ? null : owner.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
				GameObject attackTarget = null;

				if (owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					attackTarget = owner.RangeAttackTarget; // must be do here because RangeAttackTarget is changed in CheckRangeAttackState
					eCheckRangeAttackStateResult rangeCheckresult = owner.CheckRangeAttackState(attackTarget);
					if (rangeCheckresult == eCheckRangeAttackStateResult.Hold)
					{
						Interval = 100;
						return; //Hold the shot another second
					}
					else if (rangeCheckresult == eCheckRangeAttackStateResult.Stop || attackTarget == null)
					{
						owner.StopAttack(); //Stop the attack
						Stop();
						return;
					}

					int model = (attackWeapon == null ? 0 : attackWeapon.Model);
					foreach (GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player == null) continue;
						player.Out.SendCombatAnimation(owner, attackTarget, (ushort)model, 0x00, player.Out.BowShoot, 0x01, 0, ((GameLiving)attackTarget).HealthPercent);
					}

					interruptDuration = owner.AttackSpeed(attackWeapon);

					switch (owner.RangedAttackType)
					{
						case eRangedAttackType.Critical:
							{
								effectiveness *= 2 - 0.3 * owner.GetConLevel(attackTarget);
								if (effectiveness > 2)
									effectiveness *= 2;
								else if (effectiveness < 1.1)
									effectiveness *= 1.1;
							}
							break;

						case eRangedAttackType.SureShot:
							{
								effectiveness *= 0.5;
							}
							break;

						case eRangedAttackType.RapidFire:
							{
								// Source : http://www.camelotherald.com/more/888.shtml
								// - (About Rapid Fire) If you release the shot 75% through the normal timer, the shot (if it hits) does 75% of its normal damage. If you
								// release 50% through the timer, you do 50% of the damage, and so forth - The faster the shot, the less damage it does.

								// Source : http://www.camelotherald.com/more/901.shtml
								// Related note about Rapid Fire interrupts are determined by the speed of the bow is fired, meaning that the time of interruptions for each shot will be scaled
								// down proportionally to bow speed. If that made your eyes bleed, here's an example from someone who would know: "I fire a 5.0 spd bow. Because I am buffed and have
								// stat bonuses, I fire that bow at 3.0 seconds. The resulting interrupt on the caster will last 3.0 seconds. If I rapid fire that same bow, I will fire at 1.5 seconds,
								// and the resulting interrupt will last 1.5 seconds."

								long rapidFireMaxDuration = owner.AttackSpeed(attackWeapon) / 2; // half of the total time
								long elapsedTime = owner.CurrentRegion.Time - owner.TempProperties.getProperty<long>(GamePlayer.RANGE_ATTACK_HOLD_START); // elapsed time before ready to fire
								if (elapsedTime < rapidFireMaxDuration)
								{
									effectiveness *= 0.5 + (double)elapsedTime * 0.5 / (double)rapidFireMaxDuration;
									interruptDuration = (int)(interruptDuration * effectiveness);
								}
							}
							break;
					}

					// calculate Penetrating Arrow damage reduction
					if (attackTarget is GameLiving)
					{
						int PALevel = owner.GetAbilityLevel(Abilities.PenetratingArrow);
						if ((PALevel > 0) && (owner.RangedAttackType != eRangedAttackType.Long))
						{
							GameSpellEffect bladeturn = null;
							lock (((GameLiving)attackTarget).EffectList)
							{
								foreach (IGameEffect effect in ((GameLiving)attackTarget).EffectList)
								{
									if (effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.SpellType == "Bladeturn")
									{
										bladeturn = (GameSpellEffect)effect;
										break;
									}
								}
							}

							if (bladeturn != null && attackTarget != bladeturn.SpellHandler.Caster)
							{
								// Penetrating Arrow work
								effectiveness *= 0.25 + PALevel * 0.25;
							}
						}
					}

					ticksToTarget = 1 + owner.GetDistanceTo( attackTarget ) * 100 / 150; // 150 units per 1/10s
				}
				else
				{
					attackTarget = owner.TargetObject;

					// wait until target is selected
					if (attackTarget == null || attackTarget == owner)
					{
						Interval = 100;
						return;
					}

					AttackData ad = owner.TempProperties.getProperty<object>(LAST_ATTACK_DATA, null) as AttackData;
					if (ad != null && ad.AttackResult == eAttackResult.Fumbled)
					{
						Interval = owner.AttackSpeed(attackWeapon);
						ad.AttackResult = eAttackResult.Missed;
						return; //Don't start the attack if the last one fumbled
					}

					combatStyle = owner.GetStyleToUse();
					if (combatStyle != null && combatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
					{
						attackWeapon = leftWeapon;
					}
					interruptDuration = owner.AttackSpeed(attackWeapon);

					// Damage is doubled on sitting players
					// but only with melee weapons; arrows and magic does normal damage.
					if (attackTarget is GamePlayer && ((GamePlayer)attackTarget).IsSitting)
					{
						effectiveness *= 2;
					}

					ticksToTarget = 1;
				}

				//Genesis : check attack range here, effect: npc's and players will start attack faster instead of waiting another round if the previous failed
				if (attackTarget != null
				    && owner.ActiveWeaponSlot != eActiveWeaponSlot.Distance
				    && !owner.IsWithinRadius( attackTarget, owner.AttackRange ))
				{
					Interval = 100;
					return;
				}
				
				new WeaponOnTargetAction(owner, attackTarget, attackWeapon, leftWeapon, effectiveness, interruptDuration, combatStyle).Start(ticksToTarget);  // really start the attack

				//Are we inactive?
				if (owner.ObjectState != eObjectState.Active)
				{
					Stop();
					return;
				}

				//switch to melee if range to target is less than 200
				if (owner is GameNPC && owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance && owner.TargetObject != null && owner.IsWithinRadius( owner.TargetObject, 200 ) )
				{
					owner.SwitchWeapon(eActiveWeaponSlot.Standard);
				}

				if (owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					//Mobs always shot and reload
					if (owner is GameNPC)
					{
						owner.RangedAttackState = eRangedAttackState.AimFireReload;
					}

					if (owner.RangedAttackState != eRangedAttackState.AimFireReload)
					{
						owner.StopAttack();
						Stop();
						return;
					}
					else
					{
						if (!(owner is GamePlayer) || (owner.RangedAttackType != eRangedAttackType.Long))
						{
							owner.RangedAttackType = eRangedAttackType.Normal;
							lock (owner.EffectList)
							{
								foreach (IGameEffect effect in owner.EffectList) // switch to the correct range attack type
								{
									if (effect is SureShotEffect)
									{
										owner.RangedAttackType = eRangedAttackType.SureShot;
										break;
									}
									else if (effect is RapidFireEffect)
									{
										owner.RangedAttackType = eRangedAttackType.RapidFire;
										break;
									}
									else if (effect is TrueshotEffect)
									{
										owner.RangedAttackType = eRangedAttackType.Long;
										break;
									}
								}
							}
						}

						owner.RangedAttackState = eRangedAttackState.Aim;

						if (owner is GamePlayer)
						{
							owner.TempProperties.setProperty(GamePlayer.RANGE_ATTACK_HOLD_START, 0L);
						}

						int speed = owner.AttackSpeed(attackWeapon);
						byte attackSpeed = (byte)(speed / 100);
						int model = (attackWeapon == null ? 0 : attackWeapon.Model);
						foreach (GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendCombatAnimation(owner, null, (ushort)model, 0x00, player.Out.BowPrepare, attackSpeed, 0x00, 0x00);
						}

						if (owner.RangedAttackType == eRangedAttackType.RapidFire)
						{
							speed /= 2; // can start fire at the middle of the normal time
						}

						Interval = speed;
					}
				}
				else
				{
					if (leftHandSwingCount > 0)
					{
						Interval = owner.AttackSpeed(attackWeapon, leftWeapon);
					}
					else
					{
						Interval = owner.AttackSpeed(attackWeapon);
					}
				}
			}
		}


		/// <summary>
		/// The action when the weapon hurt the target
		/// </summary>
		protected class WeaponOnTargetAction : RegionAction
		{
			/// <summary>
			/// The target of the attack
			/// </summary>
			protected readonly GameObject m_target;

			/// <summary>
			/// The weapon of the attack
			/// </summary>
			protected readonly InventoryItem m_attackWeapon;

			/// <summary>
			/// The weapon in the left hand of the attacker
			/// </summary>
			protected readonly InventoryItem m_leftWeapon;

			/// <summary>
			/// The effectiveness of the attack
			/// </summary>
			protected readonly double m_effectiveness;

			/// <summary>
			/// The interrupt duration of the attack
			/// </summary>
			protected readonly int m_interruptDuration;

			/// <summary>
			/// The combat style of the attack
			/// </summary>
			protected readonly Style m_combatStyle;

			/// <summary>
			/// Constructs a new attack action
			/// </summary>
			/// <param name="owner">The action source</param>
			/// <param name="attackWeapon">the weapon used to attack</param>
			/// <param name="combatStyle">the style used</param>
			/// <param name="effectiveness">the effectiveness</param>
			/// <param name="interruptDuration">the interrupt duration</param>
			/// <param name="leftHandSwingCount">the left hand swing count</param>
			/// <param name="leftWeapon">the left hand weapon used to attack</param>
			/// <param name="target">the target of the attack</param>
			public WeaponOnTargetAction(GameLiving owner, GameObject target, InventoryItem attackWeapon, InventoryItem leftWeapon, double effectiveness, int interruptDuration, Style combatStyle)
				: base(owner)
			{
				m_target = target;
				m_attackWeapon = attackWeapon;
				m_leftWeapon = leftWeapon;
				m_effectiveness = effectiveness;
				m_interruptDuration = interruptDuration;
				m_combatStyle = combatStyle;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving owner = (GameLiving)m_actionSource;
				Style style = m_combatStyle;
				int leftHandSwingCount = 0;
				AttackData mainHandAD = null;
				AttackData leftHandAD = null;
				InventoryItem mainWeapon = m_attackWeapon;
				InventoryItem leftWeapon = m_leftWeapon;
				double leftHandEffectiveness = m_effectiveness;
				double mainHandEffectiveness = m_effectiveness;

				mainHandEffectiveness *= owner.CalculateMainHandEffectiveness(mainWeapon, leftWeapon);
				leftHandEffectiveness *= owner.CalculateLeftHandEffectiveness(mainWeapon, leftWeapon);
					
				// GameNPC can Dual Swing even with no weapon
				if (owner is GameNPC && owner.CanUseLefthandedWeapon)
				{
					leftHandSwingCount = owner.CalculateLeftHandSwingCount();
				}
				else if (owner.CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type != (int)eObjectType.Shield
				    && mainWeapon != null && (mainWeapon.Item_Type == Slot.RIGHTHAND || mainWeapon.Item_Type == Slot.LEFTHAND))
				{
					leftHandSwingCount = owner.CalculateLeftHandSwingCount();
				}

				// CMH
				// 1.89
				//- Pets will no longer continue to attack a character after the character has stealthed.
				// 1.88
				//- Monsters, pets and Non-Player Characters (NPCs) will now halt their pursuit when the character being chased stealths.
				if (owner is GameNPC
				    && m_target is GamePlayer
				    && ((GamePlayer)m_target).IsStealthed)
				{
					// note due to the 2 lines above all npcs stop attacking
					GameNPC npc = (GameNPC)owner;
					npc.StopAttack();
					npc.TargetObject = null;
					Stop(); // stop the full tick timer? looks like other code is doing this

					// target death caused this below, so I'm replicating it
					if (npc.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
					    npc.Inventory != null &&
					    npc.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
						npc.SwitchWeapon(eActiveWeaponSlot.Distance);
					return;
				}

				if (leftHandSwingCount > 0)
				{
					// both hands are used for attack
					mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, mainHandEffectiveness, m_interruptDuration, true);
					if (style == null)
					{
						mainHandAD.AnimationId = -2; // virtual code for both weapons swing animation
					}
				}
				else if (mainWeapon != null)
				{
					// no left hand used, all is simple here
					mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, mainHandEffectiveness, m_interruptDuration, false);
					leftHandSwingCount = 0;
				}
				else
				{
					// one of two hands is used for attack if no style, treated as a main hand attack
					if (style == null && Util.Chance(50))
					{
						mainWeapon = leftWeapon;
						mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, mainHandEffectiveness, m_interruptDuration, true);
						mainHandAD.AnimationId = -1; // virtual code for left weapons swing animation
					}
					else
					{
						mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, mainHandEffectiveness, m_interruptDuration, true);
					}
				}

				owner.TempProperties.setProperty( LAST_ATTACK_DATA, mainHandAD );

				//Notify the target of our attack (sends damage messages, should be before damage)
				// ...but certainly not if the attack never took place, like when the living
				// is out of range!
				if (mainHandAD.Target != null && mainHandAD.AttackResult != eAttackResult.OutOfRange)
				{
					mainHandAD.Target.AddAttacker(owner);
					mainHandAD.Target.OnAttackedByEnemy(mainHandAD);
				}

				// deal damage and start effect
				if (mainHandAD.AttackResult == eAttackResult.HitUnstyled || mainHandAD.AttackResult == eAttackResult.HitStyle)
				{
					owner.DealDamage(mainHandAD);
					if (mainHandAD.IsMeleeAttack)
					{
						owner.CheckWeaponMagicalEffect(mainHandAD, mainWeapon); // proc, poison
						if (mainHandAD.Target is GameLiving)
						{
							GameLiving living = mainHandAD.Target as GameLiving;
							RealmAbilities.L3RAPropertyEnhancer ra = living.GetAbility<RealmAbilities.ReflexAttackAbility>();
							if (ra != null && Util.Chance(ra.Amount))
							{
								AttackData ReflexAttackAD = living.MakeAttack(owner, living.AttackWeapon, null, 1, m_interruptDuration, false, true);
								living.DealDamage(ReflexAttackAD);
								living.SendAttackingCombatMessages(ReflexAttackAD);
							}
						}
					}
				}

				//CMH
				// 1.89:
				// - Characters who are attacked by stealthed archers will now target the attacking archer if the attacked player does not already have a target.
				if( mainHandAD.Attacker.IsStealthed
				   && mainHandAD.AttackType == AttackData.eAttackType.Ranged
				   && ( mainHandAD.AttackResult == eAttackResult.HitUnstyled || mainHandAD.AttackResult == eAttackResult.HitStyle ) )
				{
					if( mainHandAD.Target.TargetObject == null )
					{
						if( mainHandAD.Target is GamePlayer )
						{
							GameClient targetClient = WorldMgr.GetClientByPlayerID( mainHandAD.Target.InternalID, false, false );
							if( targetClient != null )
							{
								targetClient.Out.SendChangeTarget( mainHandAD.Attacker );
							}
						}
					}
				}

				//Send the proper attacking messages to ourself
				owner.SendAttackingCombatMessages(mainHandAD);

				//Notify ourself about the attack
				owner.Notify( GameLivingEvent.AttackFinished, owner, new AttackFinishedEventArgs( mainHandAD ) );

				// remove the left-hand AttackData from the previous attack
				owner.TempProperties.removeProperty( LAST_ATTACK_DATA_LH );

				//now left hand damage
				if (leftHandSwingCount > 0)
				{
					switch (mainHandAD.AttackResult)
					{
						case eAttackResult.HitStyle:
						case eAttackResult.HitUnstyled:
						case eAttackResult.Missed:
						case eAttackResult.Blocked:
						case eAttackResult.Evaded:
						case eAttackResult.Parried:
							for (int i = 0; i < leftHandSwingCount; i++)
							{
								if (m_target is GameLiving && (((GameLiving)m_target).IsAlive == false || ((GameLiving)m_target).ObjectState != eObjectState.Active))
									break;

								// Savage swings - main,left,main,left.
								if ( i % 2 == 0 )
									leftHandAD = owner.MakeAttack( m_target, leftWeapon, null, leftHandEffectiveness, m_interruptDuration, true );
								else
									leftHandAD = owner.MakeAttack(m_target, mainWeapon, null, leftHandEffectiveness, m_interruptDuration, true);

								//Notify the target of our attack (sends damage messages, should be before damage)
								if (leftHandAD.Target != null)
									leftHandAD.Target.OnAttackedByEnemy(leftHandAD);

								// deal damage and start the effect if any
								if (leftHandAD.AttackResult == eAttackResult.HitUnstyled || leftHandAD.AttackResult == eAttackResult.HitStyle)
								{
									owner.DealDamage(leftHandAD);
									if (leftHandAD.IsMeleeAttack)
									{
										owner.CheckWeaponMagicalEffect(leftHandAD, leftWeapon);
									}
								}

								owner.TempProperties.setProperty( LAST_ATTACK_DATA_LH, leftHandAD );

								//Send messages about our left hand attack now
								owner.SendAttackingCombatMessages(leftHandAD);

								//Notify ourself about the attack
								owner.Notify(GameLivingEvent.AttackFinished, owner, new AttackFinishedEventArgs(leftHandAD));
							}
							break;
					}
				}

				if (mainHandAD.AttackType == AttackData.eAttackType.Ranged)
					owner.RangedAttackFinished();

				switch (mainHandAD.AttackResult)
				{
					case eAttackResult.NoTarget:
					case eAttackResult.TargetDead:
						{
							Stop();
							owner.OnTargetDeadOrNoTarget();
							return;
						}
					case eAttackResult.NotAllowed_ServerRules:
					case eAttackResult.NoValidTarget:
						{
							owner.StopAttack();
							Stop();
							return;
						}
					case eAttackResult.OutOfRange:
						break;
				}

				// unstealth before attack animation
				if (owner is GamePlayer)
					((GamePlayer)owner).Stealth(false);

				//Show the animation
				if (mainHandAD.AttackResult != eAttackResult.HitUnstyled && mainHandAD.AttackResult != eAttackResult.HitStyle && leftHandAD != null)
					owner.ShowAttackAnimation(leftHandAD, leftWeapon);
				else
					owner.ShowAttackAnimation(mainHandAD, mainWeapon);

				// (procs) start style effect after any damage
				if (mainHandAD.StyleEffects.Count > 0 && mainHandAD.AttackResult == eAttackResult.HitStyle)
				{
					foreach (ISpellHandler proc in mainHandAD.StyleEffects)
					{
						proc.StartSpell(mainHandAD.Target);
					}
				}

				if (leftHandAD != null && leftHandAD.StyleEffects.Count > 0 && leftHandAD.AttackResult == eAttackResult.HitStyle)
				{
					foreach (ISpellHandler proc in leftHandAD.StyleEffects)
					{
						proc.StartSpell(leftHandAD.Target);
					}
				}

				//mobs dont update the heading after they start attacking
				//so here they update it after they swing
				//update internal heading, do not send update to client
				if (owner is GameNPC)
					(owner as GameNPC).TurnTo(mainHandAD.Target, false);

				Stop();
				return;
			}
		}

		/// <summary>
		/// Sends the proper combat messages depending on our attack data
		/// </summary>
		/// <param name="ad">result of the attack</param>
		protected virtual void SendAttackingCombatMessages(AttackData ad)
		{
			//None for GameLiving - could be a GameNPC DO NOT ADD ANYTHING HERE
		}

		/// <summary>
		/// Does this living allow procs to be cast on it?
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public virtual bool AllowWeaponMagicalEffect(AttackData ad, InventoryItem weapon, Spell weaponSpell)
		{
			if (weapon.Flags == 10) //Itemtemplates with "Flags" set to 10 will not proc on living (ex. Bruiser)
				return false;
			else return true;
		}

		/// <summary>
		/// Check if we can make a proc on a weapon go off.  Weapon Procs
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="weapon"></param>
		public virtual void CheckWeaponMagicalEffect(AttackData ad, InventoryItem weapon)
		{
			if (weapon == null)
				return;

			// Proc chance is 2.5% per SPD, i.e. 10% for a 3.5 SPD weapon. - Tolakram, changed average speed to 3.5

			int procChance = (int)Math.Ceiling(((weapon.ProcChance > 0 ? weapon.ProcChance : 10) * (weapon.SPD_ABS / 35.0)));

            //Error protection and log for Item Proc's
            Spell procSpell = null;
            Spell procSpell1 = null;
            if (this is GamePlayer)
            {
                SpellLine line = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
                if (line != null)
                {
                    procSpell = SkillBase.FindSpell(weapon.ProcSpellID, line);
                    procSpell1 = SkillBase.FindSpell(weapon.ProcSpellID1, line);

                    if (procSpell == null && weapon.ProcSpellID != 0)
                    {
                        log.ErrorFormat("- Proc ID {0} Not Found on item: {1} ", weapon.ProcSpellID, weapon.Template.Id_nb);
                    }
                    if (procSpell1 == null && weapon.ProcSpellID1 != 0)
                    {
                        log.ErrorFormat("- Proc1 ID {0} Not Found on item: {1} ", weapon.ProcSpellID1, weapon.Template.Id_nb);
                    }
                }
            }

            // Proc #1
            if (procSpell != null && Util.Chance(procChance))

                StartWeaponMagicalEffect(weapon, ad, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects), weapon.ProcSpellID, false);

            // Proc #2
            if (procSpell1 != null && Util.Chance(procChance))

                StartWeaponMagicalEffect(weapon, ad, SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects), weapon.ProcSpellID1, false);

			// Poison

			if (weapon.PoisonSpellID != 0)
			{
				if (ad.Target.EffectList.GetOfType<RemedyEffect>() != null)
				{
					if (this is GamePlayer)
						(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "GameLiving.CheckWeaponMagicalEffect.Protected"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					return;
				}

				StartWeaponMagicalEffect(weapon, ad, SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons), weapon.PoisonSpellID, true);

				// Spymaster Enduring Poison

				if (ad.Attacker is GamePlayer)
				{
					GamePlayer PlayerAttacker = ad.Attacker as GamePlayer;
					if (PlayerAttacker.GetSpellLine("Spymaster") != null)
						if (Util.ChanceDouble((double)(15 * 0.0001))) return;
				}
				weapon.PoisonCharges--;
				if (weapon.PoisonCharges <= 0) { weapon.PoisonMaxCharges = 0; weapon.PoisonSpellID = 0; }
			}
		}

		/// <summary>
		/// Make a proc or poison on the weapon go off.
		/// Will assume spell is in GlobalSpellsLines.Item_Effects even if it's not and use the weapons LevelRequirement
		/// Item_Effects must be used here because various spell handlers recognize this line to alter variance and other spell parameters
		/// </summary>
		protected virtual void StartWeaponMagicalEffect(InventoryItem weapon, AttackData ad, SpellLine spellLine, int spellID, bool ignoreLevel)
		{
			if (weapon == null)
				return;

			if (spellLine == null)
			{
				spellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
			}

			if (spellLine != null && ad != null && weapon != null)
			{
				Spell procSpell = SkillBase.FindSpell(spellID, spellLine);

				if (procSpell != null)
				{
					// check with target to see if it allows procs to cast on it (primarily used for keep components)
					if (ad.Target.AllowWeaponMagicalEffect(ad, weapon, procSpell))
					{
						if (ignoreLevel == false)
						{
							int requiredLevel = weapon.Template.LevelRequirement > 0 ? weapon.Template.LevelRequirement : Math.Min(50, weapon.Level);

							if (requiredLevel > Level)
							{
								if (this is GamePlayer)
								{
									(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "GameLiving.StartWeaponMagicalEffect.NotPowerful"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
								}
								return;
							}
						}

						ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(ad.Attacker, procSpell, spellLine);
												
						if (spellHandler != null)
						{
							bool rangeCheck = spellHandler.Spell.Target.ToLower().Equals("enemy") && spellHandler.Spell.Range > 0;

							if (!rangeCheck || ad.Attacker.IsWithinRadius(ad.Target, spellHandler.CalculateSpellRange()))
								spellHandler.StartSpell(ad.Target, weapon);
						}
					}
				}
			}
		}

		/// <summary>
		/// Starts a melee or ranged attack on a given target.
		/// </summary>
		/// <param name="attackTarget">The object to attack.</param>
		public virtual void StartAttack(GameObject attackTarget)
		{
			// Aredhel: Let the brain handle this, no need to call StartAttack
			// if the body can't do anything anyway.
			if (IsIncapacitated)
				return;

			if (IsEngaging)
				CancelEngageEffect();

			AttackState = true;

			int speed = AttackSpeed(AttackWeapon);

			if (speed > 0)
			{
				m_attackAction = CreateAttackAction();

				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					// only start another attack action if we aren't already aiming to shoot
					if (RangedAttackState != eRangedAttackState.Aim)
					{
						RangedAttackState = eRangedAttackState.Aim;

						foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
							player.Out.SendCombatAnimation(this, null, (ushort)(AttackWeapon == null ? 0 : AttackWeapon.Model),
							                               0x00, player.Out.BowPrepare, (byte)(speed / 100), 0x00, 0x00);

						m_attackAction.Start((RangedAttackType == eRangedAttackType.RapidFire) ? speed / 2 : speed);
					}
				}
				else
				{
					if (m_attackAction.TimeUntilElapsed < 500)
						m_attackAction.Start(500);
				}
			}
		}

		/// <summary>
		/// When a ranged attack is finished this is called in order to check LOS for next attack
		/// </summary>
		public virtual void RangedAttackFinished()
		{
		}

		/// <summary>
		/// Remove engage effect on this living if it is present.
		/// </summary>
		private void CancelEngageEffect()
		{
			EngageEffect effect = EffectList.GetOfType<EngageEffect>();

			if (effect != null)
				effect.Cancel(false);

			IsEngaging = false;
		}

		/// <summary>
		/// Interrupts a ranged attack.
		/// </summary>
		protected virtual void InterruptRangedAttack()
		{
			RangedAttackState = eRangedAttackState.None;
			RangedAttackType = eRangedAttackType.Normal;

			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendInterruptAnimation(this);
		}

		/// <summary>
		/// Our target is dead or we don't have a target
		/// </summary>
		public virtual void OnTargetDeadOrNoTarget()
		{
			if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
			{
				StopAttack();
			}

			if (this is GameNPC && ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
			    ((GameNPC)this).Inventory != null &&
			    ((GameNPC)this).Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
			{
				SwitchWeapon(eActiveWeaponSlot.Distance);
			}
		}

		/// <summary>
		/// Stops all attacks this GameLiving is currently making.
		/// </summary>
		public virtual void StopAttack()
		{
			StopAttack(true);
		}

		/// <summary>
		/// Stop all attackes this GameLiving is currently making
		/// </summary>
		/// <param name="forced">Is this a forced stop or is the client suggesting we stop?</param>
		public virtual void StopAttack(bool forced)
		{
			CancelEngageEffect();
			AttackState = false;

			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				InterruptRangedAttack();
		}

		/// <summary>
		/// Minimum melee critical damage as a percentage of the
		/// raw damage.
		/// </summary>
		protected virtual float MinMeleeCriticalDamage
		{
			get { return 0.1f; }
		}

		/// <summary>
		/// Calculates melee critical damage of this living.
		/// </summary>
		/// <param name="ad">The attack data.</param>
		/// <param name="weapon">The weapon used.</param>
		/// <returns>The amount of critical damage.</returns>
		public virtual int GetMeleeCriticalDamage(AttackData attackData, InventoryItem weapon)
		{
			if (Util.Chance(AttackCriticalChance(weapon)))
			{
				int maxCriticalDamage = (attackData.Target is GamePlayer)
					? attackData.Damage / 2
					: attackData.Damage;

				int minCriticalDamage = (int)(attackData.Damage * MinMeleeCriticalDamage);

				return Util.Random(minCriticalDamage, maxCriticalDamage);
			}

			return 0;
		}

		protected bool IsValidTarget
		{
			get
			{
				return EffectList.CountOfType<NecromancerShadeEffect>() <= 0;
			}
		}

		public GamePlayer GetPlayerAttacker(GameLiving living)
		{
			if (living is GamePlayer)
				return living as GamePlayer;

			GameNPC npc = living as GameNPC;

			if (npc != null)
			{
				if (npc.Brain is IControlledBrain && (npc.Brain as IControlledBrain).Owner is GamePlayer)
					return (npc.Brain as IControlledBrain).Owner as GamePlayer;
			}

			return null;
		}

		/// <summary>
		/// Returns the result of an enemy attack,
		/// yes this means WE decide if an enemy hits us or not :-)
		/// </summary>
		/// <param name="ad">AttackData</param>
		/// <param name="weapon">the weapon used for attack</param>
		/// <returns>the result of the attack</returns>
		public virtual eAttackResult CalculateEnemyAttackResult(AttackData ad, InventoryItem weapon)
		{
			if (!IsValidTarget)
				return eAttackResult.NoValidTarget;

			//1.To-Hit modifiers on styles do not any effect on whether your opponent successfully Evades, Blocks, or Parries.  Grab Bag 2/27/03
			//2.The correct Order of Resolution in combat is Intercept, Evade, Parry, Block (Shield), Guard, Hit/Miss, and then Bladeturn.  Grab Bag 2/27/03, Grab Bag 4/4/03
			//3.For every person attacking a monster, a small bonus is applied to each player's chance to hit the enemy. Allowances are made for those who don't technically hit things when they are participating in the raid  for example, a healer gets credit for attacking a monster when he heals someone who is attacking the monster, because that's what he does in a battle.  Grab Bag 6/6/03
			//4.Block, parry, and bolt attacks are affected by this code, as you know. We made a fix to how the code counts people as "in combat." Before this patch, everyone grouped and on the raid was counted as "in combat." The guy AFK getting Mountain Dew was in combat, the level five guy hovering in the back and hoovering up some exp was in combat  if they were grouped with SOMEONE fighting, they were in combat. This was a bad thing for block, parry, and bolt users, and so we fixed it.  Grab Bag 6/6/03
			//5.Positional degrees - Side Positional combat styles now will work an extra 15 degrees towards the rear of an opponent, and rear position styles work in a 60 degree arc rather than the original 90 degree standard. This change should even out the difficulty between side and rear positional combat styles, which have the same damage bonus. Please note that front positional styles are not affected by this change. 1.62
			//http://daoc.catacombs.com/forum.cfm?ThreadKey=511&DefMessage=681444&forum=DAOCMainForum#Defense

			GuardEffect guard = null;
			DashingDefenseEffect dashing = null;
			InterceptEffect intercept = null;
			GameSpellEffect bladeturn = null;
			EngageEffect engage = null;
			// ML effects
			GameSpellEffect phaseshift = null;
			GameSpellEffect grapple = null;
			GameSpellEffect brittleguard = null;

			AttackData lastAD = TempProperties.getProperty<AttackData>(LAST_ATTACK_DATA, null);
			bool defenseDisabled = ad.Target.IsMezzed | ad.Target.IsStunned | ad.Target.IsSitting;

			// If berserk is on, no defensive skills may be used: evade, parry, ...
			// unfortunately this as to be check for every action itself to kepp oder of actions the same.
			// Intercept and guard can still be used on berserked
			//			BerserkEffect berserk = null;

			// get all needed effects in one loop
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (effect is GuardEffect)
					{
						if (guard == null && ((GuardEffect)effect).GuardTarget == this)
							guard = (GuardEffect)effect;
						continue;
					}

					if (effect is DashingDefenseEffect)
					{
						if (dashing == null && ((DashingDefenseEffect)effect).GuardTarget == this)
							dashing = (DashingDefenseEffect)effect; //Dashing
						continue;
					}

					if (effect is BerserkEffect)
					{
						defenseDisabled = true;
						continue;
					}

					if (effect is EngageEffect)
					{
						if (engage == null)
							engage = (EngageEffect)effect;
						continue;
					}

					if (effect is GameSpellEffect)
					{
						switch ((effect as GameSpellEffect).Spell.SpellType)
						{
							case "Phaseshift":
								if (phaseshift == null)
									phaseshift = (GameSpellEffect)effect;
								continue;
							case "Grapple":
								if (grapple == null)
									grapple = (GameSpellEffect)effect;
								continue;
							case "BrittleGuard":
								if (brittleguard == null)
									brittleguard = (GameSpellEffect)effect;
								continue;
							case "Bladeturn":
								if (bladeturn == null)
									bladeturn = (GameSpellEffect)effect;
								continue;
						}
					}

					// We check if interceptor can intercept

					// we can only intercept attacks on livings, and can only intercept when active
					// you cannot intercept while you are sitting
					// if you are stuned or mesmeried you cannot intercept...
					InterceptEffect inter = effect as InterceptEffect;
					if (intercept == null && inter != null && inter.InterceptTarget == this && !inter.InterceptSource.IsStunned && !inter.InterceptSource.IsMezzed
					    && !inter.InterceptSource.IsSitting && inter.InterceptSource.ObjectState == eObjectState.Active && inter.InterceptSource.IsAlive
					    && this.IsWithinRadius(inter.InterceptSource, InterceptAbilityHandler.INTERCEPT_DISTANCE) && Util.Chance(inter.InterceptChance))
					{
						intercept = inter;
						continue;
					}
				}
			}

			bool stealthStyle = false;
			if (ad.Style != null && ad.Style.StealthRequirement && ad.Attacker is GamePlayer && StyleProcessor.CanUseStyle((GamePlayer)ad.Attacker, ad.Style, weapon))
			{
				stealthStyle = true;
				defenseDisabled = true;
				//Eden - brittle guard should not intercept PA
				intercept = null;
				brittleguard = null;
			}

			// Bodyguard - the Aredhel way. Alas, this is not perfect yet as clearly,
			// this code belongs in GamePlayer, but it's a start to end this clutter.
			// Temporarily saving the below information here.
			// Defensive chances (evade/parry) are reduced by 20%, but target of bodyguard
			// can't be attacked in melee until bodyguard is killed or moves out of range.

			if (this is GamePlayer)
			{
				GamePlayer playerAttacker = GetPlayerAttacker(ad.Attacker);

				if (playerAttacker != null)
				{
					GameLiving attacker = ad.Attacker;

					if (attacker.ActiveWeaponSlot != eActiveWeaponSlot.Distance)
					{
						GamePlayer target = this as GamePlayer;
						GamePlayer bodyguard = target.Bodyguard;
						if (bodyguard != null)
						{
							target.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(target.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouWereProtected"), bodyguard.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

							bodyguard.Out.SendMessage(String.Format(LanguageMgr.GetTranslation(bodyguard.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouHaveProtected"), target.Name, attacker.Name), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);

							if (attacker == playerAttacker)
								playerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YouAttempt"), target.Name, target.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							else
								playerAttacker.Out.SendMessage(string.Format(LanguageMgr.GetTranslation(playerAttacker.Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.YourPetAttempts"), target.Name, target.Name, bodyguard.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							return eAttackResult.Bodyguarded;
						}
					}
				}
			}

			if (phaseshift != null)
				return eAttackResult.Missed;

			if (grapple != null)
				return eAttackResult.Grappled;

			if (brittleguard != null)
			{
				if (this is GamePlayer)
					((GamePlayer)this).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)this).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				if (ad.Attacker is GamePlayer)
					((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeIntercepted"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				brittleguard.Cancel(false);
				return eAttackResult.Missed;
			}

			if (intercept != null && !stealthStyle)
			{
				ad.Target = intercept.InterceptSource;
				if (intercept.InterceptSource is GamePlayer)
					intercept.Cancel(false); // can be canceled only outside of the loop
				return eAttackResult.HitUnstyled;
			}

			// i am defender, what con is attacker to me?
			// orange+ should make it harder to block/evade/parry
			double attackerConLevel = -GetConLevel(ad.Attacker);
			//			double levelModifier = -((ad.Attacker.Level - Level) / (Level / 10.0 + 1));

			int attackerCount = m_attackers.Count;

			if (!defenseDisabled)
			{
				double evadeChance = TryEvade( ad, lastAD, attackerConLevel, attackerCount );

				if( Util.ChanceDouble( evadeChance ) )
					return eAttackResult.Evaded;

				if( ad.IsMeleeAttack )
				{
					double parryChance = TryParry( ad, lastAD, attackerConLevel, attackerCount );

					if( Util.ChanceDouble( parryChance ) )
						return eAttackResult.Parried;
				}

				double blockChance = TryBlock( ad, lastAD, attackerConLevel, attackerCount, engage );

				if( Util.ChanceDouble( blockChance ) )
				{
					// reactive effects on block moved to GamePlayer
					return eAttackResult.Blocked;
				}
			}


			// Guard
			if (guard != null &&
			    guard.GuardSource.ObjectState == eObjectState.Active &&
			    guard.GuardSource.IsStunned == false &&
			    guard.GuardSource.IsMezzed == false &&
			    guard.GuardSource.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
			    //				guard.GuardSource.AttackState &&
			    guard.GuardSource.IsAlive &&
			    !stealthStyle)
			{
				// check distance
				if (guard.GuardSource.IsWithinRadius(guard.GuardTarget, GuardAbilityHandler.GUARD_DISTANCE))
				{
					// check player is wearing shield and NO two handed weapon
					InventoryItem leftHand = guard.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					InventoryItem rightHand = guard.GuardSource.AttackWeapon;
					if (((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield) || guard.GuardSource is GameNPC)
					{
						// TODO
						// insert actual formula for guarding here, this is just a guessed one based on block.
						int guardLevel = guard.GuardSource.GetAbilityLevel(Abilities.Guard); // multiply by 3 to be a bit qorse than block (block woudl be 5 since you get guard I with shield 5, guard II with shield 10 and guard III with shield 15)
						double guardchance = 0;
						if (guard.GuardSource is GameNPC)
							guardchance = guard.GuardSource.GetModified(eProperty.BlockChance) * 0.001;
						else
							guardchance = guard.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;
						guardchance *= guardLevel * 0.3 + 0.05;
						guardchance += attackerConLevel * 0.05;
						int shieldSize = 0;
						if (leftHand != null)
							shieldSize = leftHand.Type_Damage;
						if (guard.GuardSource is GameNPC)
							shieldSize = 1;

						if (guardchance < 0.01)
							guardchance = 0.01;
						else if (ad.Attacker is GamePlayer && guardchance > .6)
							guardchance = .6;
						else if (shieldSize == 1 && ad.Attacker is GameNPC && guardchance > .8)
							guardchance = .8;
						else if (shieldSize == 2 && ad.Attacker is GameNPC && guardchance > .9)
							guardchance = .9;
						else if (shieldSize == 3 && ad.Attacker is GameNPC && guardchance > .99)
							guardchance = .99;

						if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) guardchance /= 2;
						if (Util.ChanceDouble(guardchance))
						{
							ad.Target = guard.GuardSource;
							return eAttackResult.Blocked;
						}
					}
				}
			}

			//Dashing Defense
			if (dashing != null &&
			    dashing.GuardSource.ObjectState == eObjectState.Active &&
			    dashing.GuardSource.IsStunned == false &&
			    dashing.GuardSource.IsMezzed == false &&
			    dashing.GuardSource.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
			    dashing.GuardSource.IsAlive &&
			    !stealthStyle)
			{
				// check distance
				if (dashing.GuardSource.IsWithinRadius(dashing.GuardTarget, DashingDefenseEffect.GUARD_DISTANCE))
				{
					// check player is wearing shield and NO two handed weapon
					InventoryItem leftHand = dashing.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					InventoryItem rightHand = dashing.GuardSource.AttackWeapon;
					InventoryItem twoHand = dashing.GuardSource.Inventory.GetItem(eInventorySlot.TwoHandWeapon);
					if ((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield)
					{
						int guardLevel = dashing.GuardSource.GetAbilityLevel(Abilities.Guard); // multiply by 3 to be a bit qorse than block (block woudl be 5 since you get guard I with shield 5, guard II with shield 10 and guard III with shield 15)
						double guardchance = dashing.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;
						guardchance *= guardLevel * 0.25 + 0.05;
						guardchance += attackerConLevel * 0.05;

						if (guardchance > 0.99) guardchance = 0.99;
						if (guardchance < 0.01) guardchance = 0.01;

						int shieldSize = 0;
						if (leftHand != null)
							shieldSize = leftHand.Type_Damage;
						if (m_attackers.Count > shieldSize)
							guardchance /= (m_attackers.Count - shieldSize + 1);
						if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) guardchance /= 2;

						double parrychance = double.MinValue;
						parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);
						if (parrychance != double.MinValue)
						{
							parrychance *= 0.001;
							parrychance += 0.05 * attackerConLevel;
							if (parrychance > 0.99) parrychance = 0.99;
							if (parrychance < 0.01) parrychance = 0.01;
							if (m_attackers.Count > 1) parrychance /= m_attackers.Count / 2;
						}

						if (Util.ChanceDouble(guardchance))
						{
							ad.Target = dashing.GuardSource;
							return eAttackResult.Blocked;
						}
						else if (Util.ChanceDouble(parrychance))
						{
							ad.Target = dashing.GuardSource;
							return eAttackResult.Parried;
						}
					}
					//Check if Player is wearing Twohanded Weapon or nothing in the lefthand slot
					else
					{
						double parrychance = double.MinValue;
						parrychance = dashing.GuardSource.GetModified(eProperty.ParryChance);
						if (parrychance != double.MinValue)
						{
							parrychance *= 0.001;
							parrychance += 0.05 * attackerConLevel;
							if (parrychance > 0.99) parrychance = 0.99;
							if (parrychance < 0.01) parrychance = 0.01;
							if (m_attackers.Count > 1) parrychance /= m_attackers.Count / 2;
						}
						if (Util.ChanceDouble(parrychance))
						{
							ad.Target = dashing.GuardSource;
							return eAttackResult.Parried;
						}
					}
				}
			}

			// Missrate
			int missrate = (ad.Attacker is GamePlayer) ? 20 : 25; //player vs player tests show 20% miss on any level
			missrate -= ad.Attacker.GetModified(eProperty.ToHitBonus);
			// PVE group missrate
			if (this is GameNPC && ad.Attacker is GamePlayer &&
			    ((GamePlayer)ad.Attacker).Group != null &&
			    (int)(0.90 * ((GamePlayer)ad.Attacker).Group.Leader.Level) >= ad.Attacker.Level &&
			    ad.Attacker.IsWithinRadius(((GamePlayer)ad.Attacker).Group.Leader, 3000))
			{
				missrate -= (int)(5 * ((GamePlayer)ad.Attacker).Group.Leader.GetConLevel(this));
			}
			else if (this is GameNPC || ad.Attacker is GameNPC) // if target is not player use level mod
			{
				missrate += (int)(5 * ad.Attacker.GetConLevel(this));
			}

			// experimental missrate adjustment for number of attackers
			if ((this is GamePlayer && ad.Attacker is GamePlayer) == false)
			{
				missrate -= (Math.Max(0, Attackers.Count - 1) * ServerProperties.Properties.MISSRATE_REDUCTION_PER_ATTACKERS);
			}

			// weapon/armor bonus
			int armorBonus = 0;
			if (ad.Target is GamePlayer)
			{
				ad.ArmorHitLocation = ((GamePlayer)ad.Target).CalculateArmorHitLocation(ad);
				InventoryItem armor = null;
				if (ad.Target.Inventory != null)
					armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);
				if (armor != null)
					armorBonus = armor.Bonus;
			}
			if (weapon != null)
			{
				armorBonus -= weapon.Bonus;
			}
			if (ad.Target is GamePlayer && ad.Attacker is GamePlayer)
			{
				missrate += armorBonus;
			}
			else
			{
				missrate += missrate * armorBonus / 100;
			}
			if (ad.Style != null)
			{
				missrate -= ad.Style.BonusToHit; // add style bonus
			}
			if (lastAD != null && lastAD.AttackResult == eAttackResult.HitStyle && lastAD.Style != null)
			{
				// add defence bonus from last executed style if any
				missrate += lastAD.Style.BonusToDefense;
			}
			if (this is GamePlayer && ad.Attacker is GamePlayer && weapon != null)
			{
				missrate -= (int)((ad.Attacker.WeaponSpecLevel(weapon) - 1) * 0.1);
			}
			if (ad.Attacker.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				InventoryItem ammo = RangeAttackAmmo;
				if (ammo != null)
					switch ((ammo.SPD_ABS >> 4) & 0x3)
				{
						// http://rothwellhome.org/guides/archery.htm
						case 0: missrate += 15; break; // Rough
						//						case 1: missrate -= 0; break;
						case 2: missrate -= 15; break; // doesn't exist (?)
						case 3: missrate -= 25; break; // Footed
				}
			}
			if (this is GamePlayer && ((GamePlayer)this).IsSitting)
			{
				missrate >>= 1; //halved
			}

			if (Util.Chance(missrate))
			{
				return eAttackResult.Missed;
			}

			if (ad.IsRandomFumble)
				return eAttackResult.Fumbled;

			if (ad.IsRandomMiss)
				return eAttackResult.Missed;


			// Bladeturn
			// TODO: high level mob attackers penetrate bt, players are tested and do not penetrate (lv30 vs lv20)
			/*
			 * http://www.camelotherald.com/more/31.shtml
			 * - Bladeturns can now be penetrated by attacks from higher level monsters and
			 * players. The chance of the bladeturn deflecting a higher level attack is
			 * approximately the caster's level / the attacker's level.
			 * Please be aware that everything in the game is
			 * level/chance based - nothing works 100% of the time in all cases.
			 * It was a bug that caused it to work 100% of the time - now it takes the
			 * levels of the players involved into account.
			 */
			// "The blow penetrated the magical barrier!"
			if (bladeturn != null)
			{
				bool penetrate = false;

				if (stealthStyle)
					penetrate = true;

				if (ad.Attacker.RangedAttackType == eRangedAttackType.Long // stealth styles pierce bladeturn
				    || (ad.AttackType == AttackData.eAttackType.Ranged && ad.Target != bladeturn.SpellHandler.Caster && ad.Attacker is GamePlayer && ((GamePlayer)ad.Attacker).HasAbility(Abilities.PenetratingArrow)))  // penetrating arrow attack pierce bladeturn
					penetrate = true;


				if (ad.IsMeleeAttack && !Util.ChanceDouble((double)bladeturn.SpellHandler.Caster.Level / (double)ad.Attacker.Level))
					penetrate = true;
				if (penetrate)
				{
					if (ad.Target is GamePlayer) ((GamePlayer)ad.Target).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Target).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowPenetrated"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
				}
				else
				{
					if (this is GamePlayer) ((GamePlayer)this).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)this).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.BlowAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)ad.Attacker).Client.Account.Language, "GameLiving.CalculateEnemyAttackResult.StrikeAbsorbed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
					if (this is GamePlayer)
						((GamePlayer)this).Stealth(false);
					return eAttackResult.Missed;
				}
			}

			if (this is GamePlayer && ((GamePlayer)this).IsOnHorse)
				((GamePlayer)this).IsOnHorse = false;

			return eAttackResult.HitUnstyled;
		}

		protected virtual double TryEvade( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount )
		{
			// Evade
			// 1. A: It isn't possible to give a simple answer. The formula includes such elements
			// as your level, your target's level, your level of evade, your QUI, your DEX, your
			// buffs to QUI and DEX, the number of people attacking you, your target's weapon
			// level, your target's spec in the weapon he is wielding, the kind of attack (DW,
			// ranged, etc), attack radius, angle of attack, the style you used most recently,
			// target's offensive RA, debuffs, and a few others. (The type of weapon - large, 1H,
			// etc - doesn't matter.) ...."

			double evadeChance = 0;
			GamePlayer player = this as GamePlayer;

			GameSpellEffect evadeBuff = SpellHandler.FindEffectOnTarget( this, "EvadeBuff" );
			if( evadeBuff == null )
				evadeBuff = SpellHandler.FindEffectOnTarget( this, "SavageEvadeBuff" );

			if( player != null )
			{
				if (player.HasAbility(Abilities.Advanced_Evade) ||
				    player.EffectList.GetOfType<CombatAwarenessEffect>() != null ||
				    player.EffectList.GetOfType<RuneOfUtterAgilityEffect>() != null)
					evadeChance = GetModified( eProperty.EvadeChance );
				else if( IsObjectInFront( ad.Attacker, 180 ) && ( evadeBuff != null || player.HasAbility( Abilities.Evade ) ) )
				{
					int res = GetModified( eProperty.EvadeChance );
					if( res > 0 )
						evadeChance = res;
				}
			}
			else if( this is GameNPC && IsObjectInFront( ad.Attacker, 180 ) )
				evadeChance = GetModified( eProperty.EvadeChance );

			if( evadeChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting )
			{
				if( attackerCount > 1 )
					evadeChance -= ( attackerCount - 1 ) * 0.03;

				evadeChance *= 0.001;
				evadeChance += 0.01 * attackerConLevel; // 1% per con level distance multiplied by evade level

				if( lastAD != null && lastAD.Style != null )
				{
					evadeChance += lastAD.Style.BonusToDefense * 0.01;
				}

				if( ad.AttackType == AttackData.eAttackType.Ranged )
					evadeChance /= 5.0;

				if( evadeChance < 0.01 )
					evadeChance = 0.01;
				else if( evadeChance > ServerProperties.Properties.EVADE_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer )
					evadeChance = ServerProperties.Properties.EVADE_CAP; //50% evade cap RvR only; http://www.camelotherald.com/more/664.shtml
				else if( evadeChance > 0.995 )
					evadeChance = 0.995;
			}
			if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
			{
				evadeChance = Math.Max(evadeChance - 0.25, 0);
			}
			//Excalibur : infi RR5
			GamePlayer p = ad.Attacker as GamePlayer;
			if (p != null)
			{
				OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();
				if (Overwhelm != null)
				{
					evadeChance = Math.Max(evadeChance - OverwhelmAbility.BONUS, 0);
				}
			}
			return evadeChance;
		}

		protected virtual double TryParry( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount )
		{
			// Parry

			//1.  Dual wielding does not grant more chances to parry than a single weapon.  Grab Bag 9/12/03
			//2.  There is no hard cap on ability to Parry.  Grab Bag 8/13/02
			//3.  Your chances of doing so are best when you are solo, trying to block or parry a style from someone who is also solo. The chances of doing so decrease with grouped, simultaneous attackers.  Grab Bag 7/19/02
			//4.  The parry chance is divided up amongst the attackers, such that if you had a 50% chance to parry normally, and were under attack by two targets, you would get a 25% chance to parry one, and a 25% chance to parry the other. So, the more people or monsters attacking you, the lower your chances to parry any one attacker. -   Grab Bag 11/05/04
			//Your chance to parry is affected by the number of attackers, the size of the weapon youre using, and your spec in parry.

      //Parry % = (5% + 0.5% * Parry) / # of Attackers
			//Parry: (((Dex*2)-100)/40)+(Parry/2)+(Mastery of P*3)+5. < Possible relation to buffs
			//So, if you have parry of 20 you will have a chance of parrying 15% if there is one attacker. If you have parry of 20 you will have a chance of parrying 7.5%, if there are two attackers.
			//From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
			//So, when facing a 2H weapon, you may see a penalty to your evade.
			//
			//http://www.camelotherald.com/more/453.php

      //Also, before this comparison happens, the game looks to see if your opponent is in your forward arc  to determine that arc, make a 120 degree angle, and put yourself at the point.

			double parryChance = 0;

			if( ad.IsMeleeAttack )
			{
				GamePlayer player = this as GamePlayer;
				BladeBarrierEffect BladeBarrier = null;

				GameSpellEffect parryBuff = SpellHandler.FindEffectOnTarget( this, "ParryBuff" );
				if( parryBuff == null )
					parryBuff = SpellHandler.FindEffectOnTarget( this, "SavageParryBuff" );

				if( player != null )
				{
					//BladeBarrier overwrites all parrying, 90% chance to parry any attack, does not consider other bonuses to parry
					BladeBarrier = player.EffectList.GetOfType<BladeBarrierEffect>();
					//They still need an active weapon to parry with BladeBarrier
					if( BladeBarrier != null && ( AttackWeapon != null ) )
					{
						parryChance = 0.90;
					}
					else if( IsObjectInFront( ad.Attacker, 120 ) )
					{
						if( ( player.HasSpecialization( Specs.Parry ) || parryBuff != null ) && ( AttackWeapon != null ) )
							parryChance = GetModified( eProperty.ParryChance );
					}
				}
				else if( this is GameNPC && IsObjectInFront( ad.Attacker, 120 ) )
					parryChance = GetModified( eProperty.ParryChance );

				//If BladeBarrier is up, do not adjust the parry chance.
				if( BladeBarrier != null && !ad.Target.IsStunned && !ad.Target.IsSitting )
				{
					return parryChance;
				}
				else if( parryChance > 0 && !ad.Target.IsStunned && !ad.Target.IsSitting )
				{
					if( attackerCount > 1 )
						parryChance /= attackerCount / 2;

					parryChance *= 0.001;
					parryChance += 0.05 * attackerConLevel;

					if( parryChance < 0.01 )
						parryChance = 0.01;
					else if( parryChance > ServerProperties.Properties.PARRY_CAP && ad.Attacker is GamePlayer && ad.Target is GamePlayer )
						parryChance = ServerProperties.Properties.PARRY_CAP;
					else if( parryChance > 0.995 )
						parryChance = 0.995;
				}
			}
			//Excalibur : infi RR5
			GamePlayer p = ad.Attacker as GamePlayer;
			if (p != null)
			{
				OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();
				if (Overwhelm != null)
				{
					parryChance = Math.Max(parryChance - OverwhelmAbility.BONUS, 0);
				}
			}
			return parryChance;
		}

		protected virtual double TryBlock( AttackData ad, AttackData lastAD, double attackerConLevel, int attackerCount, EngageEffect engage )
		{
			// Block
      
			//1.Quality does not affect the chance to block at this time.  Grab Bag 3/7/03
			//2.Condition and enchantment increases the chance to block  Grab Bag 2/27/03
			//3.There is currently no hard cap on chance to block  Grab Bag 2/27/03 and 8/16/02
			//4.Dual Wielders (enemy) decrease the chance to block  Grab Bag 10/18/02
			//5.Block formula: Shield = base 5% + .5% per spec point. Then modified by dex (.1% per point of dex above 60 and below 300?). Further modified by condition, bonus and shield level
			//8.The shields size only makes a difference when multiple things are attacking you  a small shield can block one attacker, a medium shield can block two at once, and a large shield can block three.  Grab Bag 4/4/03
			//Your chance to block is affected by the number of attackers, the size of the shield youre using, and your spec in block.
			//Shield% = (5% + 0.5% * Shield)
			//Small Shield = 1 attacker
			//Medium Shield = 2 attacker
			//Large Shield = 3 attacker
			//Each attacker above these numbers will reduce your chance to block.
			//From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
			//Block: (((Dex*2)-100)/40)+(Shield/2)+(Mastery of B*3)+5. < Possible relation to buffs
			//
			//http://www.camelotherald.com/more/453.php

			//Also, before this comparison happens, the game looks to see if your opponent is in your forward arc  to determine that arc, make a 120 degree angle, and put yourself at the point.
			//your friend is most likely using a player crafted shield. The quality of the player crafted item will make a significant difference  try it and see.

			double blockChance = 0;
			GamePlayer player = this as GamePlayer;
			InventoryItem lefthand = null;

			if( this is GamePlayer && player != null && IsObjectInFront( ad.Attacker, 120 ) && player.HasAbility( Abilities.Shield ) )
			{
				lefthand = Inventory.GetItem( eInventorySlot.LeftHandWeapon );
				if( lefthand != null && ( player.AttackWeapon == null || player.AttackWeapon.Item_Type == Slot.RIGHTHAND || player.AttackWeapon.Item_Type == Slot.LEFTHAND ) )
				{
					if( lefthand.Object_Type == (int)eObjectType.Shield && IsObjectInFront( ad.Attacker, 120 ) )
						blockChance = GetModified( eProperty.BlockChance ) * lefthand.Quality * 0.01;
				}
			}
			else if( this is GameNPC && IsObjectInFront( ad.Attacker, 120 ) )
			{
				int res = GetModified( eProperty.BlockChance );
				if( res != 0 )
					blockChance = res;
			}
			if( blockChance > 0 && IsObjectInFront( ad.Attacker, 120 ) && !ad.Target.IsStunned && !ad.Target.IsSitting )
			{
				// Reduce block chance if the shield used is too small (valable only for player because npc inventory does not store the shield size but only the model of item)
				int shieldSize = 0;
				if( lefthand != null )
					shieldSize = lefthand.Type_Damage;
				if( player != null && attackerCount > shieldSize )
					blockChance *= (shieldSize / attackerCount);

				blockChance *= 0.001;
				// no chance bonus with ranged attacks?
				//					if (ad.Attacker.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
				//						blockChance += 0.25;
				blockChance += attackerConLevel * 0.05;

				if (blockChance < 0.01)
					blockChance = 0.01;
				else if (blockChance > ServerProperties.Properties.BLOCK_CAP && ad.Attacker is GamePlayer)
					blockChance = ServerProperties.Properties.BLOCK_CAP;
				else if (shieldSize == 1 && ad.Attacker is GameNPC && blockChance > .8)
					blockChance = .8;
				else if (shieldSize == 2 && ad.Attacker is GameNPC && blockChance > .9)
					blockChance = .9;
				else if (shieldSize == 3 && ad.Attacker is GameNPC && blockChance > .99)
					blockChance = .99;

				// Engage raised block change to 85% if attacker is engageTarget and player is in attackstate
				if( engage != null && AttackState && engage.EngageTarget == ad.Attacker )
				{
					// You cannot engage a mob that was attacked within the last X seconds...
					if( engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK )

					{
						if( engage.Owner is GamePlayer )
							(engage.Owner as GamePlayer).Out.SendMessage(string.Format(LanguageMgr.GetTranslation((engage.Owner as GamePlayer).Client.Account.Language, "GameLiving.TryBlock.Engage"), engage.EngageTarget.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					// Check if player has enough endurance left to engage
					else if( engage.Owner.Endurance >= EngageAbilityHandler.ENGAGE_DURATION_LOST )
					{
						engage.Owner.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
						if( engage.Owner is GamePlayer )
							(engage.Owner as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((engage.Owner as GamePlayer).Client.Account.Language, "GameLiving.TryBlock.Blocking"), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
						if( blockChance < 0.85 )
							blockChance = 0.85;
					}
					// if player ran out of endurance cancel engage effect
					else
						engage.Cancel( false );
				}
			}
			if (ad.AttackType == AttackData.eAttackType.MeleeDualWield)
			{
				blockChance = Math.Max(blockChance - 0.25, 0);
			}
			//Excalibur : infi RR5
			GamePlayer p = ad.Attacker as GamePlayer;
			if (p != null)
			{
				OverwhelmEffect Overwhelm = (OverwhelmEffect)p.EffectList.GetOfType<OverwhelmEffect>();
				if (Overwhelm != null)
				{
					blockChance = Math.Max(blockChance - OverwhelmAbility.BONUS, 0);
				}
			}
			return blockChance;
		}

		/// <summary>
		/// Modify the attack done to this living.
		/// This method offers us a chance to modify the attack data prior to the living taking damage.
		/// </summary>
		/// <param name="attackData">The attack data for this attack</param>
		public virtual void ModifyAttack(AttackData attackData)
		{
		}


		/// <summary>
		/// This method is called whenever this living
		/// should take damage from some source
		/// </summary>
		/// <param name="source">the damage source</param>
		/// <param name="damageType">the damage type</param>
		/// <param name="damageAmount">the amount of damage</param>
		/// <param name="criticalAmount">the amount of critical damage</param>
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			base.TakeDamage(source, damageType, damageAmount, criticalAmount);

			double damageDealt = damageAmount + criticalAmount;

			#region PVP DAMAGE

			// Is this a GamePlayer behind the source?
			if (source is GamePlayer || (source is GameNPC && (source as GameNPC).Brain is IControlledBrain && ((source as GameNPC).Brain as IControlledBrain).GetPlayerOwner() != null))
			{
				// Only apply to necropet.
				if (this is NecromancerPet)
				{
					//And if a GamePlayer is behind
					GamePlayer this_necro_pl = null;

					if (this is GameNPC && (this as GameNPC).Brain is IControlledBrain)
						this_necro_pl = ((this as GameNPC).Brain as IControlledBrain).GetPlayerOwner();

					if (this_necro_pl != null && this_necro_pl.Realm != source.Realm && source.Realm != 0)
						DamageRvRMemory += (long)damageDealt + (long)criticalAmount;
				}
			}

			#endregion PVP DAMAGE

			if (source != null && source is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)source).Brain as IControlledBrain;
				if (brain != null)
					source = brain.GetLivingOwner();
			}

			GamePlayer attackerPlayer = source as GamePlayer;
			if (attackerPlayer != null && attackerPlayer != this)
			{
				// Apply Mauler RA5L
				GiftOfPerizorEffect GiftOfPerizor = EffectList.GetOfType<GiftOfPerizorEffect>();
				if (GiftOfPerizor != null)
				{
					int difference = (int)(0.25 * damageDealt); // RA absorb 25% damage
					damageDealt -= difference;
					GamePlayer TheMauler = (GamePlayer)(this.TempProperties.getProperty<object>("GiftOfPerizorOwner", null));
					if (TheMauler != null && TheMauler.IsAlive)
					{
						// Calculate mana using %. % is calculated with target maxhealth and damage difference, apply this % to mauler maxmana
						double manareturned = (difference / this.MaxHealth * TheMauler.MaxMana);
						TheMauler.ChangeMana(source, GameLiving.eManaChangeType.Spell, (int)manareturned);
					}
				}

				Group attackerGroup = attackerPlayer.Group;
				if (attackerGroup != null)
				{
					List<GameLiving> xpGainers = new List<GameLiving>(8);
					// collect "helping" group players in range
					foreach (GameLiving living in attackerGroup.GetMembersInTheGroup())
					{
						if (this.IsWithinRadius(living, WorldMgr.MAX_EXPFORKILL_DISTANCE) && living.IsAlive && living.ObjectState == eObjectState.Active)
							xpGainers.Add(living);
					}

					foreach (GameLiving living in xpGainers)
						this.AddXPGainer(living, (float)(damageDealt / xpGainers.Count));
				}
				else
				{
					this.AddXPGainer(source, (float)damageDealt);
				}
				//DealDamage needs to be called after addxpgainer!
			}
			else if (source != null && source != this)
			{
				AddXPGainer(source, (float)damageAmount + criticalAmount);
			}

			bool wasAlive = IsAlive;

			//[Freya] Nidel: Use2's Flask
			if(this is GamePlayer)
			{
				bool isFatalBlow = (damageAmount + criticalAmount) >= Health;

				if (isFatalBlow)
				{
					GameSpellEffect deadFlask = SpellHandler.FindEffectOnTarget(this, "DeadFlask");
					if(deadFlask != null)
					{
						if(Util.Chance((int)deadFlask.Spell.Value))
						{
							if (IsLowHealth)
								Notify(GameLivingEvent.LowHealth, this, null);
							return;
						}
					}
				}
			}

			Health -= damageAmount + criticalAmount;

			if (!IsAlive)
			{
				if (wasAlive)
					Die(source);

				lock (m_xpGainers.SyncRoot)
					m_xpGainers.Clear();
			}
			else
			{
				if (IsLowHealth)
					Notify(GameLivingEvent.LowHealth, this, null);
			}
		}

		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		public virtual void OnAttackedByEnemy(AttackData ad)
		{
			if (ad.IsHit && ad.CausesCombat)
			{
				Notify(GameLivingEvent.AttackedByEnemy, this, new AttackedByEnemyEventArgs(ad));

				if (this is GameNPC && ActiveWeaponSlot == eActiveWeaponSlot.Distance && this.IsWithinRadius(ad.Attacker, 150))
					((GameNPC)this).SwitchToMelee(ad.Attacker);

				AddAttacker( ad.Attacker );

				if (ad.Attacker.Realm == 0 || this.Realm == 0)
				{
					LastAttackedByEnemyTickPvE = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvE = CurrentRegion.Time;
				}
				else
				{
					LastAttackedByEnemyTickPvP = CurrentRegion.Time;
					ad.Attacker.LastAttackTickPvP = CurrentRegion.Time;
				}
			}
		}

		/// <summary>
		/// Called to display an attack animation of this living
		/// </summary>
		/// <param name="ad">Infos about the attack</param>
		/// <param name="weapon">The weapon used for attack</param>
		public virtual void ShowAttackAnimation(AttackData ad, InventoryItem weapon)
		{
			bool showAnim = false;
			switch (ad.AttackResult)
			{
				case eAttackResult.HitUnstyled:
				case eAttackResult.HitStyle:
				case eAttackResult.Evaded:
				case eAttackResult.Parried:
				case eAttackResult.Missed:
				case eAttackResult.Blocked:
				case eAttackResult.Fumbled:
					showAnim = true; break;
			}

			if (showAnim && ad.Target != null)
			{
				//http://dolserver.sourceforge.net/forum/showthread.php?s=&threadid=836
				byte resultByte = 0;
				int attackersWeapon = (weapon == null) ? 0 : weapon.Model;
				int defendersWeapon = 0;

				switch (ad.AttackResult)
				{
						case eAttackResult.Missed: resultByte = 0; break;
						case eAttackResult.Evaded: resultByte = 3; break;
						case eAttackResult.Fumbled: resultByte = 4; break;
						case eAttackResult.HitUnstyled: resultByte = 10; break;
						case eAttackResult.HitStyle: resultByte = 11; break;

					case eAttackResult.Parried:
						resultByte = 1;
						if (ad.Target != null && ad.Target.AttackWeapon != null)
						{
							defendersWeapon = ad.Target.AttackWeapon.Model;
						}
						break;

					case eAttackResult.Blocked:
						resultByte = 2;
						if (ad.Target != null && ad.Target.Inventory != null)
						{
							InventoryItem lefthand = ad.Target.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
							if (lefthand != null && lefthand.Object_Type == (int)eObjectType.Shield)
							{
								defendersWeapon = lefthand.Model;
							}
						}
						break;
				}

				foreach (GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player == null) continue;
					int animationId;
					switch (ad.AnimationId)
					{
						case -1:
							animationId = player.Out.OneDualWeaponHit;
							break;
						case -2:
							animationId = player.Out.BothDualWeaponHit;
							break;
						default:
							animationId = ad.AnimationId;
							break;
					}
					player.Out.SendCombatAnimation(this, ad.Target, (ushort)attackersWeapon, (ushort)defendersWeapon, animationId, 0, resultByte, ad.Target.HealthPercent);
				}
			}
		}

		/// <summary>
		/// This method is called whenever this living is dealing
		/// damage to some object
		/// </summary>
		/// <param name="ad">AttackData</param>
		public virtual void DealDamage(AttackData ad)
		{
			ad.Target.TakeDamage(ad);
		}

		/// <summary>
		/// Adds a object to the list of objects that will gain xp
		/// after this living dies
		/// </summary>
		/// <param name="xpGainer">the xp gaining object</param>
		/// <param name="damageAmount">the amount of damage, float because for groups it can be split</param>
		public virtual void AddXPGainer(GameObject xpGainer, float damageAmount)
		{
			lock (m_xpGainers.SyncRoot)
			{
				if( m_xpGainers.Contains( xpGainer ) == false )
				{
					m_xpGainers.Add( xpGainer, 0.0f );
				}
				m_xpGainers[xpGainer] = (float)m_xpGainers[xpGainer] + damageAmount;
			}
		}

		/// <summary>
		/// Changes the health
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="healthChangeType">the change type</param>
		/// <param name="changeAmount">the change amount</param>
		/// <returns>the amount really changed</returns>
		public virtual int ChangeHealth(GameObject changeSource, eHealthChangeType healthChangeType, int changeAmount)
		{
			//TODO fire event that might increase or reduce the amount
			int oldHealth = Health;
			Health += changeAmount;
			int healthChanged = Health - oldHealth;

			//Notify our enemies that we were healed by other means than
			//natural regeneration, this allows for aggro on healers!
			if (healthChanged > 0 && healthChangeType != eHealthChangeType.Regenerate)
			{
				IList<GameObject> attackers;
				lock (Attackers) { attackers = new List<GameObject>(m_attackers); }
				EnemyHealedEventArgs args = new EnemyHealedEventArgs(this, changeSource, healthChangeType, healthChanged);
				foreach (GameObject attacker in attackers)
				{
					if (attacker is GameLiving)
					{
						(attacker as GameLiving).Notify(GameLivingEvent.EnemyHealed, attacker, args);
						// Desactivate XPGainer, Heal Rps implentation.
						//(attacker as GameLiving).AddXPGainer(changeSource, healthChanged);
					}
				}
			}
			return healthChanged;
		}

		/// <summary>
		/// Changes the mana
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="manaChangeType">the change type</param>
		/// <param name="changeAmount">the change amount</param>
		/// <returns>the amount really changed</returns>
		public virtual int ChangeMana(GameObject changeSource, eManaChangeType manaChangeType, int changeAmount)
		{
			//TODO fire event that might increase or reduce the amount
			int oldMana = Mana;
			Mana += changeAmount;
			return Mana - oldMana;
		}

		/// <summary>
		/// Changes the endurance
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="enduranceChangeType">the change type</param>
		/// <param name="changeAmount">the change amount</param>
		/// <returns>the amount really changed</returns>
		public virtual int ChangeEndurance(GameObject changeSource, eEnduranceChangeType enduranceChangeType, int changeAmount)
		{
			//TODO fire event that might increase or reduce the amount
			int oldEndurance = Endurance;
			Endurance += changeAmount;
			return Endurance - oldEndurance;
		}

		/// <summary>
		/// Called when an enemy of ours is healed during combat
		/// </summary>
		/// <param name="enemy">the enemy</param>
		/// <param name="healSource">the healer</param>
		/// <param name="changeType">the healtype</param>
		/// <param name="healAmount">the healamount</param>
		public virtual void EnemyHealed(GameLiving enemy, GameObject healSource, eHealthChangeType changeType, int healAmount)
		{
			Notify(GameLivingEvent.EnemyHealed, this, new EnemyHealedEventArgs(enemy, healSource, changeType, healAmount));
		}

		/// <summary>
		/// Returns the list of attackers
		/// </summary>
		public List<GameObject> Attackers
		{
			get { return m_attackers; }
		}

		/// <summary>
		/// Adds an attacker to the attackerlist
		/// </summary>
		/// <param name="attacker">the attacker to add</param>
		public virtual void AddAttacker(GameObject attacker)
		{
			lock (Attackers)
			{
				if (attacker == this) return;
				if (m_attackers.Contains(attacker)) return;
				m_attackers.Add(attacker);
			}
		}
		/// <summary>
		/// Removes an attacker from the list
		/// </summary>
		/// <param name="attacker">the attacker to remove</param>
		public virtual void RemoveAttacker(GameObject attacker)
		{
			//			log.Warn(Name + ": RemoveAttacker "+attacker.Name);
			//			log.Error(Environment.StackTrace);
			lock (Attackers)
			{
				m_attackers.Remove(attacker);
			}
		}
		/// <summary>
		/// Called when this living dies
		/// </summary>
		public virtual void Die(GameObject killer)
		{
			if (this is GameNPC == false && this is GamePlayer == false)
			{
				// deal out exp and realm points based on server rules
				GameServer.ServerRules.OnLivingKilled(this, killer);
			}

			StopAttack();

			List<GameObject> clone;
			lock (Attackers)
			{
				if (m_attackers.Contains(killer) == false)
					m_attackers.Add(killer);
				clone = new List<GameObject>(m_attackers);
			}
			List<GamePlayer> playerAttackers = null;

			foreach (GameObject obj in clone)
			{
				if (obj is GameLiving)
				{
					GamePlayer player = obj as GamePlayer;

					if (obj as GameNPC != null && (obj as GameNPC).Brain is IControlledBrain)
					{
						// Ok, we're a pet - if our Player owner isn't in the attacker list, let's make them a 'virtual' attacker
						player = ((obj as GameNPC).Brain as IControlledBrain).GetPlayerOwner();
						if (player != null)
						{
							if (clone.Contains(player) == false)
							{
								if (playerAttackers == null)
									playerAttackers = new List<GamePlayer>();

								if (playerAttackers.Contains(player) == false)
									playerAttackers.Add(player);
							}

							// Pet gets the killed message as well
							((GameLiving)obj).EnemyKilled(this);
						}
					}

					if (player != null)
					{
						if (playerAttackers == null)
							playerAttackers = new List<GamePlayer>();

						if (playerAttackers.Contains(player) == false)
						{
							playerAttackers.Add(player);
						}

						if (player.Group != null)
						{
							foreach (GamePlayer groupPlayer in player.Group.GetPlayersInTheGroup())
							{
								if (groupPlayer.IsWithinRadius(this, WorldMgr.MAX_EXPFORKILL_DISTANCE) && playerAttackers.Contains(groupPlayer) == false)
								{
									playerAttackers.Add(groupPlayer);
								}
							}
						}
					}
					else
					{
						((GameLiving)obj).EnemyKilled(this);
					}
				}
			}

			if (playerAttackers != null)
			{
				foreach (GamePlayer player in playerAttackers)
				{
					player.EnemyKilled(this);
				}
			}

			foreach (DOL.GS.Quests.DataQuest q in DataQuestList)
			{
				q.Notify(GamePlayerEvent.Dying, this, new DyingEventArgs(killer, playerAttackers));
			}

			m_attackers.Clear();

			// cancel all concentration effects
			ConcentrationEffects.CancelAll();

			// clear all of our targets
			RangeAttackTarget = null;
			TargetObject = null;

			// cancel all left effects
			EffectList.CancelAll();

			// Stop the regeneration timers
			StopHealthRegeneration();
			StopPowerRegeneration();
			StopEnduranceRegeneration();

			//Reduce health to zero
			Health = 0;

			// Remove all last attacked times
			
			LastAttackedByEnemyTickPvE = 0;
			LastAttackedByEnemyTickPvP = 0;
			//Let's send the notification at the end
			Notify(GameLivingEvent.Dying, this, new DyingEventArgs(killer));
		}

		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="expTotal">total amount of xp to gain</param>
		/// <param name="expCampBonus">camp bonus to display</param>
		/// <param name="expGroupBonus">group bonus to display</param>
		/// <param name="expOutpostBonus">outpost bonux to display</param>
		/// <param name="sendMessage">should exp gain message be sent</param>
		/// <param name="allowMultiply">should the xp amount be multiplied</param>
		public virtual void GainExperience(eXPSource xpSource, long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply, bool notify)
		{
			if (expTotal > 0 && notify) Notify(GameLivingEvent.GainedExperience, this, new GainedExperienceEventArgs(expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply, xpSource));
		}
		/// <summary>
		/// Called when this living gains realm points
		/// </summary>
		/// <param name="amount">amount of realm points gained</param>
		public virtual void GainRealmPoints(long amount)
		{
			Notify(GameLivingEvent.GainedRealmPoints, this, new GainedRealmPointsEventArgs(amount));
		}
		/// <summary>
		/// Called when this living gains bounty points
		/// </summary>
		/// <param name="amount"></param>
		public virtual void GainBountyPoints(long amount)
		{
			Notify(GameLivingEvent.GainedBountyPoints, this, new GainedBountyPointsEventArgs(amount));
		}
		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="exp">base amount of xp to gain</param>
		public void GainExperience(eXPSource xpSource, long exp)
		{
			GainExperience(xpSource, exp, 0, 0, 0, true, false, true);
		}

		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="exp">base amount of xp to gain</param>
		/// <param name="allowMultiply">Do we allow the xp to be multiplied</param>
		public void GainExperience(eXPSource xpSource, long exp, bool allowMultiply)
		{
			GainExperience(xpSource, exp, 0, 0, 0, true, allowMultiply, true);
		}

		/// <summary>
		/// Called when an enemy of this living is killed
		/// </summary>
		/// <param name="enemy">enemy killed</param>
		public virtual void EnemyKilled(GameLiving enemy)
		{
			RemoveAttacker(enemy);
			Notify(GameLivingEvent.EnemyKilled, this, new EnemyKilledEventArgs(enemy));
		}
		/// <summary>
		/// Checks whether Living has ability to use lefthanded weapons
		/// </summary>
		public virtual bool CanUseLefthandedWeapon
		{
			get { return false; }
			set { }
		}

		/// <summary>
		/// Calculates how many times left hand swings
		/// </summary>
		/// <returns></returns>
		public virtual int CalculateLeftHandSwingCount()
		{
			return 0;
		}

		/// <summary>
		/// Returns a multiplier used to reduce left hand damage
		/// </summary>
		/// <returns></returns>
		public virtual double CalculateLeftHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
		{
			return 1.0;
		}

		/// <summary>
		/// Returns a multiplier used to reduce right hand damage
		/// </summary>
		/// <returns></returns>
		public virtual double CalculateMainHandEffectiveness(InventoryItem mainWeapon, InventoryItem leftWeapon)
		{
			return 1.0;
		}

		/// <summary>
		/// Holds visible active weapon slots
		/// </summary>
		protected byte m_visibleActiveWeaponSlots = 0xFF; // none by default

		/// <summary>
		/// Gets visible active weapon slots
		/// </summary>
		public byte VisibleActiveWeaponSlots
		{
			get { return m_visibleActiveWeaponSlots; }
			set { m_visibleActiveWeaponSlots=value; }
		}

		/// <summary>
		/// Holds the living's cloak hood state
		/// </summary>
		protected bool m_isCloakHoodUp;

		/// <summary>
		/// Sets/gets the living's cloak hood state
		/// </summary>
		public virtual bool IsCloakHoodUp
		{
			get { return m_isCloakHoodUp; }
			set { m_isCloakHoodUp = value; }
		}

		/// <summary>
		/// Holds the living's cloak hood state
		/// </summary>
		protected bool m_IsCloakInvisible = false;

		/// <summary>
		/// Sets/gets the living's cloak visible state
		/// </summary>
		public virtual bool IsCloakInvisible
		{
			get { return m_IsCloakInvisible; }
			set { m_IsCloakInvisible = value; }
		}

		/// <summary>
		/// Holds the living's helm visible state
		/// </summary>
		protected bool m_IsHelmInvisible = false;

		/// <summary>
		/// Sets/gets the living's cloak hood state
		/// </summary>
		public virtual bool IsHelmInvisible
		{
			get { return m_IsHelmInvisible; }
			set { m_IsHelmInvisible = value; }
		}

		/// <summary>
		/// Switches the active weapon to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public virtual void SwitchWeapon(eActiveWeaponSlot slot)
		{
			if (Inventory == null)
				return;

			//Clean up range attack variables, no matter to what
			//weapon we switch
			RangedAttackState = eRangedAttackState.None;
			RangedAttackType = eRangedAttackType.Normal;

			InventoryItem rightHandSlot = Inventory.GetItem(eInventorySlot.RightHandWeapon);
			InventoryItem leftHandSlot = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			InventoryItem twoHandSlot = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			InventoryItem distanceSlot = Inventory.GetItem(eInventorySlot.DistanceWeapon);

			// simple active slot logic:
			// 0=right hand, 1=left hand, 2=two-hand, 3=range, F=none
			int rightHand = (VisibleActiveWeaponSlots & 0x0F);
			int leftHand = (VisibleActiveWeaponSlots & 0xF0) >> 4;


			// set new active weapon slot
			switch (slot)
			{
				case eActiveWeaponSlot.Standard:
					{
						if (rightHandSlot == null)
							rightHand = 0xFF;
						else
							rightHand = 0x00;

						if (leftHandSlot == null)
							leftHand = 0xFF;
						else
							leftHand = 0x01;
					}
					break;

				case eActiveWeaponSlot.TwoHanded:
					{
						if (twoHandSlot != null && (twoHandSlot.Hand == 1 || this is GameNPC)) // 2h
						{
							rightHand = leftHand = 0x02;
							break;
						}

						// 1h weapon in 2h slot
						if (twoHandSlot == null)
							rightHand = 0xFF;
						else
							rightHand = 0x02;

						if (leftHandSlot == null)
							leftHand = 0xFF;
						else
							leftHand = 0x01;
					}
					break;

				case eActiveWeaponSlot.Distance:
					{
						leftHand = 0xFF; // cannot use left-handed weapons if ranged slot active

						if (distanceSlot == null)
							rightHand = 0xFF;
						else if (distanceSlot.Hand == 1 || this is GameNPC) // NPC equipment does not have hand so always assume 2 handed bow
							rightHand = leftHand = 0x03; // bows use 2 hands, throwing axes 1h
						else
							rightHand = 0x03;
					}
					break;
			}

			m_activeWeaponSlot = slot;

			// pack active weapon slots value back
			m_visibleActiveWeaponSlots = (byte)(((leftHand & 0x0F) << 4) | (rightHand & 0x0F));
		}
		#endregion
		#region Property/Bonus/Buff/PropertyCalculator fields
		/// <summary>
		/// Array for property boni for abilities
		/// </summary>
		protected IPropertyIndexer m_abilityBonus = new PropertyIndexer();
		/// <summary>
		/// Ability bonus property
		/// </summary>
		public virtual IPropertyIndexer AbilityBonus
		{
			get { return m_abilityBonus; }
		}

		/// <summary>
		/// Array for property boni by items
		/// </summary>
		protected IPropertyIndexer m_itemBonus = new PropertyIndexer();
		/// <summary>
		/// Property Item Bonus field
		/// </summary>
		public virtual IPropertyIndexer ItemBonus
		{
			get { return m_itemBonus; }
		}


		/// <summary>
		/// Array for buff boni
		/// </summary>
		protected IPropertyIndexer m_buff1Bonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer BaseBuffBonusCategory
		{
			get { return m_buff1Bonus; }
		}

		/// <summary>
		/// Array for second buff boni
		/// </summary>
		protected IPropertyIndexer m_buff2Bonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer SpecBuffBonusCategory
		{
			get { return m_buff2Bonus; }
		}

		/// <summary>
		/// Array for third debuff boni
		/// </summary>
		protected IPropertyIndexer m_debuffBonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer DebuffCategory
		{
			get { return m_debuffBonus; }
		}

		/// <summary>
		/// Array for forth buff boni
		/// </summary>
		protected IPropertyIndexer m_buff4Bonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer BuffBonusCategory4
		{
			get { return m_buff4Bonus; }
		}

		/// <summary>
		/// Array for first multiplicative buff boni
		/// </summary>
		protected IMultiplicativeProperties m_buffMult1Bonus = new MultiplicativePropertiesHybrid();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IMultiplicativeProperties BuffBonusMultCategory1
		{
			get { return m_buffMult1Bonus; }
		}

		/// <summary>
		/// Array for spec debuff boni
		/// </summary>
		protected IPropertyIndexer m_specDebuffBonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer SpecDebuffCategory
		{
			get { return m_specDebuffBonus; }
		}
		
		/// <summary>
		/// property calculators for each property
		/// look at PropertyCalculator class for more description
		/// </summary>
		internal static readonly IPropertyCalculator[] m_propertyCalc = new IPropertyCalculator[(int)eProperty.MaxProperty+1];

		/// <summary>
		/// retrieve a property value of that living
		/// this value is modified/capped and ready to use
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual int GetModified(eProperty property)
		{
			if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
			{
				return m_propertyCalc[(int)property].CalcValue(this, property);
			}
			else
			{
				log.ErrorFormat("{0} did not find property calculator for property ID {1}.", Name, (int)property);
			}
			return 0;
		}

		//Eden : secondary resists, such AoM, vampiir magic resistance etc, should not apply in CC duration, disease, debuff etc, using a new function
		public virtual int GetModifiedBase(eProperty property)
		{
			if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
			{
				return m_propertyCalc[(int)property].CalcValueBase(this, property);
			}
			else
			{
				log.ErrorFormat("{0} did not find base property calculator for property ID {1}.", Name, (int)property);
			}
			return 0;
		}

		/// <summary>
		/// Retrieve a property value of this living's buff bonuses only;
		/// caps and cap increases apply.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual int GetModifiedFromBuffs(eProperty property)
		{
			if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
			{
				return m_propertyCalc[(int)property].CalcValueFromBuffs(this, property);
			}
			else
			{
				log.ErrorFormat("{0} did not find buff property calculator for property ID {1}.", Name, (int)property);
			}
			return 0;
		}

		/// <summary>
		/// Retrieve a property value of this living's item bonuses only;
		/// caps and cap increases apply.
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual int GetModifiedFromItems(eProperty property)
		{
			if (m_propertyCalc != null && m_propertyCalc[(int)property] != null)
			{
				return m_propertyCalc[(int)property].CalcValueFromItems(this, property);
			}
			else
			{
				log.ErrorFormat("{0} did not find item property calculator for property ID {1}.", Name, (int)property);
			}
			return 0;
		}

		/// <summary>
		/// has to be called after properties were changed and updates are needed
		/// TODO: not sure about property change detection, has to be reviewed
		/// </summary>
		public virtual void PropertiesChanged()
		{
			//			// take last changes as old ones now
			//			for (int i=0; i<m_oldTempProps.Length; i++)
			//			{
			//				m_oldTempProps[i] = m_newTempProps[i];
			//			}
			//
			//			// recalc new array to detect changes later
			//			for (int i=0; i<m_propertyCalc.Length; i++)
			//			{
			//				if (m_propertyCalc[i]!=null)
			//				{
			//					m_newTempProps[i] = m_propertyCalc[i].CalcValue(this, (eProperty)i);
			//				}
			//				else
			//				{
			//					m_newTempProps[i] = 0;
			//				}
			//			}
		}

		#endregion
		#region Stats, Resists
		/// <summary>
		/// The name of the states
		/// </summary>
		public static readonly string[] STAT_NAMES = new string[]{"Unknown Stat","Strength", "Dexterity", "Constitution", "Quickness", "Intelligence",
			"Piety", "Empathy", "Charisma"};

		/// <summary>
		/// base values for char stats
		/// </summary>
		protected readonly short[] m_charStat = new short[8];
		/// <summary>
		/// get a unmodified char stat value
		/// </summary>
		/// <param name="stat"></param>
		/// <returns></returns>
		public virtual int GetBaseStat(eStat stat)
		{
			return m_charStat[stat - eStat._First];
		}
		/// <summary>
		/// changes a base stat value
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="amount"></param>
		public virtual void ChangeBaseStat(eStat stat, short amount)
		{
			m_charStat[stat - eStat._First] += amount;
		}

		/// <summary>
		/// this field is just for convinience and speed purposes
		/// converts the damage types to resist fields
		/// </summary>
		protected static readonly eProperty[] m_damageTypeToResistBonusConversion = new eProperty[] {
			eProperty.Resist_Natural, //0,
			eProperty.Resist_Crush,
			eProperty.Resist_Slash,
			eProperty.Resist_Thrust,
			0, 0, 0, 0, 0, 0,
			eProperty.Resist_Body,
			eProperty.Resist_Cold,
			eProperty.Resist_Energy,
			eProperty.Resist_Heat,
			eProperty.Resist_Matter,
			eProperty.Resist_Spirit
		};
		/// <summary>
		/// gets the resistance value by damage type, refer to eDamageType for constants
		/// </summary>
		/// <param name="damageType"></param>
		/// <returns></returns>
		public virtual eProperty GetResistTypeForDamage(eDamageType damageType)
		{
			if ((int)damageType < m_damageTypeToResistBonusConversion.Length)
			{
				return m_damageTypeToResistBonusConversion[(int)damageType];
			}
			else
			{
				log.ErrorFormat("No resist found for damage type {0} on living {1}!", (int)damageType, Name);
				return 0;
			}
		}
		/// <summary>
		/// gets the resistance value by damage types
		/// </summary>
		/// <param name="damageType">the damag etype</param>
		/// <returns>the resist value</returns>
		public virtual int GetResist(eDamageType damageType)
		{
			return GetModified(GetResistTypeForDamage(damageType));
		}

		public virtual int GetResistBase(eDamageType damageType)
		{
			return GetModifiedBase(GetResistTypeForDamage(damageType));
		}

		/// <summary>
		/// Get the resistance to damage by resist type
		/// </summary>
		/// <param name="property">one of the Resist_XXX properties</param>
		/// <returns>the resist value</returns>
		public virtual int GetDamageResist(eProperty property)
		{
			return SkillBase.GetRaceResist( m_race, (eResist)property );
		}

		/// <summary>
		/// Gets the Damage Resist for a damage type
		/// </summary>
		/// <param name="damageType"></param>
		/// <returns></returns>
		public virtual int GetDamageResist(eDamageType damageType)
		{
			return GetDamageResist(GetResistTypeForDamage(damageType));
		}

		/// <summary>
		/// temp properties
		/// </summary>
		private readonly PropertyCollection m_tempProps = new PropertyCollection();

		/// <summary>
		/// use it to store temporary properties on this living
		/// beware to use unique keys so they do not interfere
		/// </summary>
		public PropertyCollection TempProperties
		{
			get { return m_tempProps; }
		}

		/// <summary>
		/// Gets or Sets the effective level of the Object
		/// </summary>
		public override int EffectiveLevel
		{
			get { return GetModified(eProperty.LivingEffectiveLevel); }
		}

		/// <summary>
		/// returns the level of a specialization
		/// if 0 is returned, the spec is non existent on living
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public virtual int GetBaseSpecLevel(string keyName)
		{
			return Level;
		}

		/// <summary>
		/// returns the level of a specialization + bonuses from RR and Items
		/// if 0 is returned, the spec is non existent on the living
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public virtual int GetModifiedSpecLevel(string keyName)
		{
			return Level;
		}

		#endregion
		#region Regeneration
		/// <summary>
		/// GameTimer used for restoring hp
		/// </summary>
		protected RegionTimer m_healthRegenerationTimer;
		/// <summary>
		/// GameTimer used for restoring mana
		/// </summary>
		protected RegionTimer m_powerRegenerationTimer;
		/// <summary>
		/// GameTimer used for restoring endurance
		/// </summary>
		protected RegionTimer m_enduRegenerationTimer;

		/// <summary>
		/// The default frequency of regenerating health in milliseconds
		/// </summary>
		protected const ushort m_healthRegenerationPeriod = 3000;

		/// <summary>
		/// Interval for health regeneration tics
		/// </summary>
		protected virtual ushort HealthRegenerationPeriod
		{
			get { return m_healthRegenerationPeriod; }
		}

		/// <summary>
		/// The default frequency of regenerating power in milliseconds
		/// </summary>
		protected const ushort m_powerRegenerationPeriod = 3000;

		/// <summary>
		/// Interval for power regeneration tics
		/// </summary>
		protected virtual ushort PowerRegenerationPeriod
		{
			get { return m_powerRegenerationPeriod; }
		}

		/// <summary>
		/// The default frequency of regenerating endurance in milliseconds
		/// </summary>
		protected const ushort m_enduranceRegenerationPeriod = 1000;

		/// <summary>
		/// Interval for endurance regeneration tics
		/// </summary>
		protected virtual ushort EnduranceRegenerationPeriod
		{
			get { return m_enduranceRegenerationPeriod; }
		}

		/// <summary>
		/// The lock object for lazy regen timers initialization
		/// </summary>
		protected readonly object m_regenTimerLock = new object();

		/// <summary>
		/// Starts the health regeneration
		/// </summary>
		public virtual void StartHealthRegeneration()
		{
			if (ObjectState != eObjectState.Active)
				return;
			lock (m_regenTimerLock)
			{
				if (m_healthRegenerationTimer == null)
				{
					m_healthRegenerationTimer = new RegionTimer(this);
					m_healthRegenerationTimer.Callback = new RegionTimerCallback(HealthRegenerationTimerCallback);
				}
				else if (m_healthRegenerationTimer.IsAlive)
				{
					return;
				}

				m_healthRegenerationTimer.Start(HealthRegenerationPeriod);
			}
		}
		/// <summary>
		/// Starts the power regeneration
		/// </summary>
		public virtual void StartPowerRegeneration()
		{
			if (ObjectState != eObjectState.Active)
				return;
			lock (m_regenTimerLock)
			{
				if (m_powerRegenerationTimer == null)
				{
					m_powerRegenerationTimer = new RegionTimer(this);
					m_powerRegenerationTimer.Callback = new RegionTimerCallback(PowerRegenerationTimerCallback);
				}
				else if (m_powerRegenerationTimer.IsAlive)
				{
					return;
				}

				m_powerRegenerationTimer.Start(PowerRegenerationPeriod);
			}
		}
		/// <summary>
		/// Starts the endurance regeneration
		/// </summary>
		public virtual void StartEnduranceRegeneration()
		{
			if (ObjectState != eObjectState.Active)
				return;
			lock (m_regenTimerLock)
			{
				if (m_enduRegenerationTimer == null)
				{
					m_enduRegenerationTimer = new RegionTimer(this);
					m_enduRegenerationTimer.Callback = new RegionTimerCallback(EnduranceRegenerationTimerCallback);
				}
				else if (m_enduRegenerationTimer.IsAlive)
				{
					return;
				}
				m_enduRegenerationTimer.Start(EnduranceRegenerationPeriod);
			}
		}
		/// <summary>
		/// Stop the health regeneration
		/// </summary>
		public virtual void StopHealthRegeneration()
		{
			lock (m_regenTimerLock)
			{
				if (m_healthRegenerationTimer == null)
					return;
				m_healthRegenerationTimer.Stop();
				m_healthRegenerationTimer = null;
			}
		}
		/// <summary>
		/// Stop the power regeneration
		/// </summary>
		public virtual void StopPowerRegeneration()
		{
			lock (m_regenTimerLock)
			{
				if (m_powerRegenerationTimer == null)
					return;
				m_powerRegenerationTimer.Stop();
				m_powerRegenerationTimer = null;
			}
		}
		/// <summary>
		/// Stop the endurance regeneration
		/// </summary>
		public virtual void StopEnduranceRegeneration()
		{
			lock (m_regenTimerLock)
			{
				if (m_enduRegenerationTimer == null)
					return;
				m_enduRegenerationTimer.Stop();
				m_enduRegenerationTimer = null;
			}
		}
		/// <summary>
		/// Timer callback for the hp regeneration
		/// </summary>
		/// <param name="callingTimer">timer calling this function</param>
		protected virtual int HealthRegenerationTimerCallback(RegionTimer callingTimer)
		{
			if (Health < MaxHealth)
			{
				ChangeHealth(this, eHealthChangeType.Regenerate, GetModified(eProperty.HealthRegenerationRate));
			}

			#region PVP DAMAGE

			
			if (this is NecromancerPet)
			{
				GamePlayer this_necro_pl = null;

				this_necro_pl = ((this as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner();

				if (DamageRvRMemory > 0 && this_necro_pl != null)
					DamageRvRMemory -= (long)Math.Max(GetModified(eProperty.HealthRegenerationRate), 0);
			}

			#endregion PVP DAMAGE

			//If we are fully healed, we stop the timer
			if (Health >= MaxHealth)
			{

				#region PVP DAMAGE

				if (this is NecromancerPet)
				{
					GamePlayer this_necro_pl = null;

					this_necro_pl = ((this as NecromancerPet).Brain as IControlledBrain).GetPlayerOwner();

					if (DamageRvRMemory > 0 && this_necro_pl != null)
						DamageRvRMemory = 0;
				}

				#endregion PVP DAMAGE

				//We clean all damagedealers if we are fully healed,
				//no special XP calculations need to be done
				lock (m_xpGainers.SyncRoot)
				{
					m_xpGainers.Clear();
				}

				return 0;
			}

			if (InCombat)
			{
				// in combat each tic is aprox 15 seconds - tolakram
				return HealthRegenerationPeriod * 5;
			}

			//Heal at standard rate
			return HealthRegenerationPeriod;
		}
		/// <summary>
		/// Callback for the power regenerationTimer
		/// </summary>
		/// <param name="selfRegenerationTimer">timer calling this function</param>
		protected virtual int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if (this is GamePlayer &&
			    (((GamePlayer)this).CharacterClass.ID == (int)eCharacterClass.Vampiir ||
			     (((GamePlayer)this).CharacterClass.ID > 59 && ((GamePlayer)this).CharacterClass.ID < 63))) // Maulers
			{
				double MinMana = MaxMana * 0.15;
				double OnePercMana = Math.Ceiling(MaxMana * 0.01);

				if (!InCombat)
				{
					if (ManaPercent < 15)
					{
						ChangeMana(this, eManaChangeType.Regenerate, (int)OnePercMana);
						return 4000;
					}
					else if (ManaPercent > 15)
					{
						ChangeMana(this, eManaChangeType.Regenerate, (int)(-OnePercMana));
						return 1000;
					}

					return 0;
				}
			}
			else
			{
				if (Mana < MaxMana)
				{
					ChangeMana(this, eManaChangeType.Regenerate, GetModified(eProperty.PowerRegenerationRate));
				}

				//If we are full, we stop the timer
				if (Mana >= MaxMana)
				{
					return 0;
				}
			}

			//If we were hit before we regenerated, we regenerate slower the next time
			if (InCombat)
			{
				return (int)(PowerRegenerationPeriod * 3.4);
			}

			//regen at standard rate
			return PowerRegenerationPeriod;
		}
		/// <summary>
		/// Callback for the endurance regenerationTimer
		/// </summary>
		/// <param name="selfRegenerationTimer">timer calling this function</param>
		protected virtual int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if (Endurance < MaxEndurance)
			{
				int regen = GetModified(eProperty.EnduranceRegenerationRate);
				if (regen > 0)
				{
					ChangeEndurance(this, eEnduranceChangeType.Regenerate, regen);
				}
			}
			if (Endurance >= MaxEndurance) return 0;

			return 500 + Util.Random(EnduranceRegenerationPeriod);
		}
		#endregion

		#region Mana/Health/Endurance/Concentration/Delete
		/// <summary>
		/// Amount of mana
		/// </summary>
		protected int m_mana;
		/// <summary>
		/// Amount of endurance
		/// </summary>
		protected int m_endurance;
		/// <summary>
		/// Maximum value that can be in m_endurance
		/// </summary>
		protected int m_maxEndurance;

		/// <summary>
		/// Gets/sets the object health
		/// </summary>
		public override int Health
		{
			get { return m_health; }
			set
			{

				int maxhealth = MaxHealth;
				if (value >= maxhealth)
				{
					m_health = maxhealth;

					// noret: i see a problem here when players get not RPs after this player was healed to full
					// either move this to GameNPC or add a check.

					//We clean all damagedealers if we are fully healed,
					//no special XP calculations need to be done
					lock (m_xpGainers.SyncRoot)
					{
						//DOLConsole.WriteLine(this.Name+": Health=100% -> clear xpgainers");
						m_xpGainers.Clear();
					}
				}
				else if (value > 0)
				{
					m_health = value;
				}
				else
				{
					m_health = 0;
				}

				if (IsAlive && m_health < maxhealth)
				{
					StartHealthRegeneration();
				}
			}
		}

		public override int MaxHealth
		{
			get {	return GetModified(eProperty.MaxHealth); }
		}

		public virtual int Mana
		{
			get
			{
				return m_mana;
			}
			set
			{
				int maxmana = MaxMana;
				m_mana = Math.Min(value, maxmana);
				m_mana = Math.Max(m_mana, 0);
				if (IsAlive && (m_mana < maxmana || (this is GamePlayer && ((GamePlayer)this).CharacterClass.ID == (int)eCharacterClass.Vampiir)
				                || (this is GamePlayer && ((GamePlayer)this).CharacterClass.ID > 59 && ((GamePlayer)this).CharacterClass.ID < 63)))
				{
					StartPowerRegeneration();
				}
			}
		}

		public virtual int MaxMana
		{
			get
			{
				return GetModified(eProperty.MaxMana);
			}
		}

		public virtual byte ManaPercent
		{
			get
			{
				return (byte)(MaxMana <= 0 ? 0 : ((Mana * 100) / MaxMana));
			}
		}

		/// <summary>
		/// Gets/sets the object endurance
		/// </summary>
		public virtual int Endurance
		{
			get { return m_endurance; }
			set
			{
				m_endurance = Math.Min(value, m_maxEndurance);
				m_endurance = Math.Max(m_endurance, 0);
				if (IsAlive && m_endurance < m_maxEndurance)
				{
					StartEnduranceRegeneration();
				}
			}
		}

		/// <summary>
		/// Gets or sets the maximum endurance of this living
		/// </summary>
		public virtual int MaxEndurance
		{
			get { return m_maxEndurance; }
			set
			{
				m_maxEndurance = value;
				Endurance = Endurance; //cut extra end points if there are any or start regeneration
			}
		}

		/// <summary>
		/// Gets the endurance in percent of maximum
		/// </summary>
		public virtual byte EndurancePercent
		{
			get
			{
				return (byte)(MaxEndurance <= 0 ? 0 : ((Endurance * 100) / MaxEndurance));
			}
		}

		/// <summary>
		/// Gets/sets the object concentration
		/// </summary>
		public virtual int Concentration
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets/sets the object maxconcentration
		/// </summary>
		public virtual int MaxConcentration
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets the concentration in percent of maximum
		/// </summary>
		public virtual byte ConcentrationPercent
		{
			get
			{
				return (byte)(MaxConcentration <= 0 ? 0 : ((Concentration * 100) / MaxConcentration));
			}
		}

		/// <summary>
		/// Holds the concentration effects list
		/// </summary>
		private readonly ConcentrationList m_concEffects;
		/// <summary>
		/// Gets the concentration effects list
		/// </summary>
		public ConcentrationList ConcentrationEffects { get { return m_concEffects; } }

		/// <summary>
		/// Cancels all concentration effects by this living and on this living
		/// </summary>
		public void CancelAllConcentrationEffects()
		{
			CancelAllConcentrationEffects(false);
		}

		/// <summary>
		/// Cancels all concentration effects by this living and on this living
		/// </summary>
		public void CancelAllConcentrationEffects(bool leaveSelf)
		{
			// cancel conc spells
			ConcentrationEffects.CancelAll(leaveSelf);

			// cancel all active conc spell effects from other casters
			ArrayList concEffects = new ArrayList();
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.Concentration > 0)
					{
						if (!leaveSelf || leaveSelf && ((GameSpellEffect)effect).SpellHandler.Caster != this)
							concEffects.Add(effect);
					}
				}
			}
			foreach (GameSpellEffect effect in concEffects)
			{
				effect.Cancel(false);
			}
		}

		#endregion
		#region Speed/Heading/Target/GroundTarget/GuildName/SitState/Level
		/// <summary>
		/// The targetobject of this living
		/// This is a weak reference to a GameObject, which
		/// means that the gameobject can be cleaned up even
		/// when this living has a reference on it ...
		/// </summary>
		protected readonly WeakReference m_targetObjectWeakReference;
		/// <summary>
		/// The current speed of this living
		/// </summary>
		protected short m_currentSpeed;
		/// <summary>
		/// The base maximum speed of this living
		/// </summary>
		protected short m_maxSpeedBase;
		/// <summary>
		/// Holds the Living's Coordinate inside the current Region
		/// </summary>
		protected Point3D m_groundTarget;

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public override ushort Heading
		{
			get { return base.Heading; }
			set
			{
				ushort oldHeading = base.Heading;
				base.Heading = value;
				if (base.Heading != oldHeading)
					UpdateTickSpeed();
			}
		}

		private bool m_fixedSpeed = false;

		/// <summary>
		/// Does this NPC have a fixed speed, unchanged by any modifiers?
		/// </summary>
		public virtual bool FixedSpeed
		{
			get { return m_fixedSpeed; }
			set { m_fixedSpeed = value; }
		}

		/// <summary>
		/// Gets or sets the current speed of this living
		/// </summary>
		public virtual short CurrentSpeed
		{
			get
			{
				return m_currentSpeed;
			}
			set
			{
				m_currentSpeed = value;
				UpdateTickSpeed();
			}
		}

		/// <summary>
		/// Gets the maxspeed of this living
		/// </summary>
		public virtual short MaxSpeed
		{
			get
			{
				if (FixedSpeed)
					return MaxSpeedBase;

				return (short)GetModified(eProperty.MaxSpeed);
			}
		}

		/// <summary>
		/// Gets or sets the base max speed of this living
		/// </summary>
		public virtual short MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}

		/// <summary>
		/// Gets or sets the target of this living
		/// </summary>
		public virtual GameObject TargetObject
		{
			get
			{
				return (m_targetObjectWeakReference.Target as GameObject);
			}
			set
			{
				m_targetObjectWeakReference.Target = value;
			}
		}
		public virtual bool IsSitting
		{
			get { return false; }
			set { }
		}
		/// <summary>
		/// Gets the Living's ground-target Coordinate inside the current Region
		/// </summary>
		public virtual Point3D GroundTarget
		{
			get { return m_groundTarget; }
		}

		/// <summary>
		/// Sets the Living's ground-target Coordinates inside the current Region
		/// </summary>
		public virtual void SetGroundTarget(int groundX, int groundY, int groundZ)
		{
			m_groundTarget.X = groundX;
			m_groundTarget.Y = groundY;
			m_groundTarget.Z = groundZ;
		}

		/// <summary>
		/// Gets or Sets the current level of the Object
		/// </summary>
		public override byte Level
		{
			get { return base.Level; }
			set
			{
				base.Level = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player == null)
							continue;
						player.Out.SendLivingDataUpdate(this, false);
					}
				}
			}
		}

		/// <summary>
		/// What is the base, unmodified level of this living
		/// </summary>
		public virtual byte BaseLevel
		{
			get { return Level; }
		}

		/// <summary>
		/// Calculates the level of a skill on this living.  Generally this is simply the level of the skill.
		/// </summary>
		public virtual int CalculateSkillLevel(Skill skill)
		{
			return skill.Level;
		}


		#endregion
		#region Movement
		/// <summary>
		/// The tick speed in X direction.
		/// </summary>
		public double TickSpeedX { get; protected set; }

		/// <summary>
		/// The tick speed in Y direction.
		/// </summary>
		public double TickSpeedY { get; protected set; }

		/// <summary>
		/// The tick speed in Z direction.
		/// </summary>
		public double TickSpeedZ { get; protected set; }

		/// <summary>
		/// Updates tick speed for this living.
		/// </summary>
		protected virtual void UpdateTickSpeed()
		{
			int speed = CurrentSpeed;

			if (speed == 0)
				SetTickSpeed(0, 0, 0);
			else
			{
				// Living will move in the direction it is currently heading.

				double heading = Heading * HEADING_TO_RADIAN;
				SetTickSpeed(-Math.Sin(heading), Math.Cos(heading), 0, speed);
			}
		}

		/// <summary>
		/// Set the tick speed, that is the distance covered in one tick.
		/// </summary>
		/// <param name="dx"></param>
		/// <param name="dy"></param>
		/// <param name="dz"></param>
		protected void SetTickSpeed(double dx, double dy, double dz)
		{
			TickSpeedX = dx;
			TickSpeedY = dy;
			TickSpeedZ = dz;
		}

		/// <summary>
		/// Set the tick speed, that is the distance covered in one tick.
		/// </summary>
		/// <param name="dx"></param>
		/// <param name="dy"></param>
		/// <param name="dz"></param>
		/// <param name="speed"></param>
		protected void SetTickSpeed(double dx, double dy, double dz, int speed)
		{
			double tickSpeed = speed * 0.001;
			SetTickSpeed(dx * tickSpeed, dy * tickSpeed, dz * tickSpeed);
		}

		/// <summary>
		/// The tick at which the movement started.
		/// </summary>
		public int MovementStartTick { get; set; }

		/// <summary>
		/// Elapsed ticks since movement started.
		/// </summary>
		protected int MovementElapsedTicks
		{
			get { return Environment.TickCount - MovementStartTick; }
		}

		/// <summary>
		/// True if the living is moving, else false.
		/// </summary>
		public virtual bool IsMoving
		{
			get { return m_currentSpeed != 0; }
		}

		/// <summary>
		/// The current X position of this living.
		/// </summary>
		public override int X
		{
			get
			{
				return (IsMoving)
					? (int)(base.X + MovementElapsedTicks * TickSpeedX)
					: base.X;
			}
			set
			{
				base.X = value;
			}
		}

		/// <summary>
		/// The current Y position of this living.
		/// </summary>
		public override int Y
		{
			get
			{
				return (IsMoving)
					? (int)(base.Y + MovementElapsedTicks * TickSpeedY)
					: base.Y;
			}
			set
			{
				base.Y = value;
			}
		}

		/// <summary>
		/// The current Z position of this living.
		/// </summary>
		public override int Z
		{
			get
			{
				return (IsMoving)
					? (int)(base.Z + MovementElapsedTicks * TickSpeedZ)
					: base.Z;
			}
			set
			{
				base.Z = value;
			}
		}

		/// <summary>
		/// Moves the item from one spot to another spot, possible even
		/// over region boundaries
		/// </summary>
		/// <param name="regionID">new regionid</param>
		/// <param name="x">new x</param>
		/// <param name="y">new y</param>
		/// <param name="z">new z</param>
		/// <param name="heading">new heading</param>
		/// <returns>true if moved</returns>
		public override bool MoveTo(ushort regionID, int x, int y, int z, ushort heading)
		{
			if (regionID != CurrentRegionID)
				CancelAllConcentrationEffects();

			return base.MoveTo(regionID, x, y, z, heading);
		}

		/// <summary>
		/// The stealth state of this living
		/// </summary>
		public virtual bool IsStealthed
		{
			get { return false; }
		}

		#endregion
		#region Say/Yell/Whisper/Emote/Messages

		private bool m_isSilent = false;

		/// <summary>
		/// Can this living say anything?
		/// </summary>
		public virtual bool IsSilent
		{
			get { return m_isSilent; }
			set { m_isSilent = value; }
		}


		/// <summary>
		/// This function is called when this object receives a Say
		/// </summary>
		/// <param name="source">Source of say</param>
		/// <param name="str">Text that was spoken</param>
		/// <returns>true if the text should be processed further, false if it should be discarded</returns>
		public virtual bool SayReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}
			
			Notify(GameLivingEvent.SayReceive, this, new SayReceiveEventArgs(source, this, str));
			
			return true;
		}

		/// <summary>
		/// Broadcasts a message to all living beings around this object
		/// </summary>
		/// <param name="str">string to broadcast (without any "xxx says:" in front!!!)</param>
		/// <returns>true if text was said successfully</returns>
		public virtual bool Say(string str)
		{
			if (str == null || IsSilent)
			{
				return false;
			}
			
			Notify(GameLivingEvent.Say, this, new SayEventArgs(str));
			
			foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.SAY_DISTANCE))
			{
				GameNPC receiver = npc;
				// don't send say to the target, it will be whispered...
				if (receiver != this && receiver != TargetObject)
				{
					receiver.SayReceive(this, str);
				}
			}
			
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
			{
				GamePlayer receiver = player;
				if (receiver != this)
				{
					receiver.SayReceive(this, str);
				}
			}
			
			// whisper to Targeted NPC.
			if (TargetObject != null && TargetObject is GameNPC)
			{
				GameNPC targetNPC = (GameNPC)TargetObject;
				targetNPC.WhisperReceive(this, str);
			}
			
			return true;
		}

		/// <summary>
		/// This function is called when the living receives a yell
		/// </summary>
		/// <param name="source">GameLiving that was yelling</param>
		/// <param name="str">string that was yelled</param>
		/// <returns>true if the string should be processed further, false if it should be discarded</returns>
		public virtual bool YellReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}
			
			Notify(GameLivingEvent.YellReceive, this, new YellReceiveEventArgs(source, this, str));
			
			return true;
		}

		/// <summary>
		/// Broadcasts a message to all living beings around this object
		/// </summary>
		/// <param name="str">string to broadcast (without any "xxx yells:" in front!!!)</param>
		/// <returns>true if text was yelled successfully</returns>
		public virtual bool Yell(string str)
		{
			if (str == null || IsSilent)
			{
				return false;
			}
			
			Notify(GameLivingEvent.Yell, this, new YellEventArgs(str));
			
			foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.YELL_DISTANCE))
			{
				GameNPC receiver = npc;
				if (receiver != this)
				{
					receiver.YellReceive(this, str);
				}
			}
			
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.YELL_DISTANCE))
			{
				GamePlayer receiver = player;
				if (receiver != this)
				{
					receiver.YellReceive(this, str);
				}
			}
			
			return true;
		}

		/// <summary>
		/// This function is called when the Living receives a whispered text
		/// </summary>
		/// <param name="source">GameLiving that was whispering</param>
		/// <param name="str">string that was whispered</param>
		/// <returns>true if the string should be processed further, false if it should be discarded</returns>
		public virtual bool WhisperReceive(GameLiving source, string str)
		{
			if (source == null || str == null)
			{
				return false;
			}

			GamePlayer player = null;
			if (source != null && source is GamePlayer)
			{
				player = source as GamePlayer;
				long whisperdelay = player.TempProperties.getProperty<long>("WHISPERDELAY");
				if (whisperdelay > 0 && (CurrentRegion.Time - 1500) < whisperdelay && player.Client.Account.PrivLevel == 1)
				{
					//player.Out.SendMessage("Speak slower!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
					return false;
				}
				
				player.TempProperties.setProperty("WHISPERDELAY", CurrentRegion.Time);

				foreach (DOL.GS.Quests.DataQuest q in DataQuestList)
				{
					q.Notify(GamePlayerEvent.WhisperReceive, this, new WhisperReceiveEventArgs(player, this, str));
				}
			}

			Notify(GameLivingEvent.WhisperReceive, this, new WhisperReceiveEventArgs(source, this, str));

			return true;
		}

		/// <summary>
		/// Sends a whisper to a target
		/// </summary>
		/// <param name="target">The target of the whisper</param>
		/// <param name="str">text to whisper (without any "xxx whispers:" in front!!!)</param>
		/// <returns>true if text was whispered successfully</returns>
		public virtual bool Whisper(GameObject target, string str)
		{
			if (target == null || str == null || IsSilent)
			{
				return false;
			}
			
			if (!this.IsWithinRadius(target, WorldMgr.WHISPER_DISTANCE))
			{
				return false;
			}
			
			Notify(GameLivingEvent.Whisper, this, new WhisperEventArgs(target, str));
			
			if (target is GameLiving)
			{
				return ((GameLiving)target).WhisperReceive(this, str);
			}
			
			return false;
		}
		/// <summary>
		/// Makes this living do an emote-animation
		/// </summary>
		/// <param name="emote">the emote animation to show</param>
		public virtual void Emote(eEmote emote)
		{
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendEmoteAnimation(this, emote);
			}
		}

		/// <summary>
		/// A message to this living
		/// </summary>
		/// <param name="message"></param>
		/// <param name="type"></param>
		public virtual void MessageToSelf(string message, eChatType chatType)
		{
			// livings can't talk to themselves
		}

		/// <summary>
		/// A message from something we control
		/// </summary>
		/// <param name="message"></param>
		/// <param name="chatType"></param>
		public virtual void MessageFromControlled(string message, eChatType chatType)
		{
			// ignore for livings
		}


		#endregion
		#region Item/Money

		/// <summary>
		/// Called when the living is about to get an item from someone
		/// else
		/// </summary>
		/// <param name="source">Source from where to get the item</param>
		/// <param name="item">Item to get</param>
		/// <returns>true if the item was successfully received</returns>
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;

			Notify(GameLivingEvent.ReceiveItem, this, new ReceiveItemEventArgs(source, this, item));

			//If the item has been removed by the event handlers : return
			if (item == null || item.OwnerID == null)
			{
				return true;
			}

			if (base.ReceiveItem(source, item) == false)
			{
				if (source is GamePlayer)
				{
					((GamePlayer)source).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)source).Client.Account.Language, "GameLiving.ReceiveItem", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				return false;
			}

			return true;
		}

		/// <summary>
		/// Called when the living is about to get money from someone
		/// else
		/// </summary>
		/// <param name="source">Source from where to get the money</param>
		/// <param name="money">array of money to get</param>
		/// <returns>true if the money was successfully received</returns>
		public override bool ReceiveMoney(GameLiving source, long money)
		{
			if (source == null || money <= 0) return false;

			Notify(GameLivingEvent.ReceiveMoney, this, new ReceiveMoneyEventArgs(source, this, money));

			if (source is GamePlayer)
				((GamePlayer)source).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)source).Client.Account.Language, "GameLiving.ReceiveMoney", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			//call base
			return base.ReceiveMoney(source, money);
		}
		#endregion
		#region Inventory
		/// <summary>
		/// Represent the inventory of all living
		/// </summary>
		protected IGameInventory m_inventory;

		/// <summary>
		/// Get/Set inventory
		/// </summary>
		public IGameInventory Inventory
		{
			get
			{
				return m_inventory;
			}
			set
			{
				m_inventory = value;
			}
		}
		#endregion
		#region Effects
		/// <summary>
		/// currently applied effects
		/// </summary>
		protected readonly GameEffectList m_effects;

		/// <summary>
		/// gets a list of active effects
		/// </summary>
		/// <returns></returns>
		public GameEffectList EffectList
		{
			get { return m_effects; }
		}

		/// <summary>
		/// Creates new effects list for this living.
		/// </summary>
		/// <returns>New effects list instance</returns>
		protected virtual GameEffectList CreateEffectsList()
		{
			return new GameEffectList(this);
		}

		#endregion
		#region Abilities

		/// <summary>
		/// Holds all abilities of the living (KeyName -> Ability)
		/// </summary>
		protected readonly Dictionary<string, Ability> m_abilities = new Dictionary<string, Ability>();

		protected readonly Object m_lockAbilities = new Object();

		/// <summary>
		/// Asks for existence of specific ability
		/// </summary>
		/// <param name="keyName">KeyName of ability</param>
		/// <returns>Does living have this ability</returns>
		public virtual bool HasAbility(string keyName)
		{
			bool hasit = false;
			
			lock (m_lockAbilities)
			{
				hasit = m_abilities.ContainsKey(keyName);
			}
			
			return hasit;
		}

		/// <summary>
		/// Add a new ability to a living
		/// </summary>
		/// <param name="ability"></param>
		public virtual void AddAbility(Ability ability)
		{
			AddAbility(ability, true);
		}

		/// <summary>
		/// Add or update an ability for this living
		/// </summary>
		/// <param name="ability"></param>
		/// <param name="sendUpdates"></param>
		public virtual void AddAbility(Ability ability, bool sendUpdates)
		{
			bool isNewAbility = false;
			lock (m_lockAbilities)
			{
				Ability oldAbility = null;
				m_abilities.TryGetValue(ability.KeyName, out oldAbility);
				
				if (oldAbility == null)
				{
					isNewAbility = true;
					m_abilities.Add(ability.KeyName, ability);
					ability.Activate(this, sendUpdates);
				}
				else
				{
					int oldLevel = oldAbility.Level;
					oldAbility.Level = ability.Level;
					
					isNewAbility |= oldAbility.Level > oldLevel;
				}
				
				if (sendUpdates && (isNewAbility && (this is GamePlayer)))
				{
					(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "GamePlayer.AddAbility.YouLearn", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// Remove an ability from this living
		/// </summary>
		/// <param name="abilityKeyName"></param>
		/// <returns></returns>
		public virtual bool RemoveAbility(string abilityKeyName)
		{
			Ability ability = null;
			lock (m_lockAbilities)
			{
				m_abilities.TryGetValue(abilityKeyName, out ability);
				
				if (ability == null)
					return false;
				
				ability.Deactivate(this, true);
				m_abilities.Remove(ability.KeyName);
			}
			
			if (this is GamePlayer)
				(this as GamePlayer).Out.SendMessage(LanguageMgr.GetTranslation((this as GamePlayer).Client.Account.Language, "GamePlayer.RemoveAbility.YouLose", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return true;
		}

		/// <summary>
		/// returns ability of living or null if non existent
		/// </summary>
		/// <param name="abilityKey"></param>
		/// <returns></returns>
		public Ability GetAbility(string abilityKey)
		{
			Ability ab = null;
			lock (m_lockAbilities)
			{
				m_abilities.TryGetValue(abilityKey, out ab);
			}
			
			return ab;
		}

		/// <summary>
		/// returns ability of living or null if no existant
		/// </summary>
		/// <returns></returns>
		public T GetAbility<T>() where T : Ability
		{
			T tmp;
			lock (m_lockAbilities)
			{
				tmp = (T)m_abilities.Values.FirstOrDefault(a => a.GetType().Equals(typeof(T)));
			}
			
			return tmp;
		}

		/// <summary>
		/// returns ability of living or null if no existant
		/// </summary>
		/// <param name="abilityType"></param>
		/// <returns></returns>
		[Obsolete("Use GetAbility<T>() instead")]
		public Ability GetAbility(Type abilityType)
		{
			lock (m_lockAbilities)
			{
				foreach (Ability ab in m_abilities.Values)
				{
					if (ab.GetType().Equals(abilityType))
						return ab;
				}
			}
			return null;
		}

		/// <summary>
		/// returns the level of ability
		/// if 0 is returned, the ability is non existent on living
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public int GetAbilityLevel(string keyName)
		{
			Ability ab = null;
			
			lock (m_lockAbilities)
			{
				m_abilities.TryGetValue(keyName, out ab);
			}
			
			if (ab == null)
				return 0;

			return Math.Max(1, ab.Level);
		}

		/// <summary>
		/// returns all abilities in a copied list
		/// </summary>
		/// <returns></returns>
		public IList GetAllAbilities()
		{
			List<Ability> list = new List<Ability>();
			lock (m_lockAbilities)
			{
				list = new List<Ability>(m_abilities.Values);
			}
			
			return list;
		}

		#endregion Abilities

		/// <summary>
		/// Checks if living has ability to use items of this type
		/// </summary>
		/// <param name="item"></param>
		/// <returns>true if living has ability to use item</returns>
		public virtual bool HasAbilityToUseItem(ItemTemplate item)
		{
			return GameServer.ServerRules.CheckAbilityToUseItem(this, item);
		}

		/// <summary>
		/// Table of skills currently disabled
		/// skill => disabletimeout (ticks) or 0 when endless
		/// </summary>
		private readonly Dictionary<KeyValuePair<int, Type>, KeyValuePair<long, Skill>> m_disabledSkills = new Dictionary<KeyValuePair<int, Type>, KeyValuePair<long, Skill>>();

		/// <summary>
		/// Gets the time left for disabling this skill in milliseconds
		/// </summary>
		/// <param name="skill"></param>
		/// <returns>milliseconds left for disable</returns>
		public virtual int GetSkillDisabledDuration(Skill skill)
		{
			lock ((m_disabledSkills as ICollection).SyncRoot)
			{
				KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
				if (m_disabledSkills.ContainsKey(key))
				{
					long timeout = m_disabledSkills[key].Key;
					long left = timeout - CurrentRegion.Time;
					if (left <= 0)
					{
						left = 0;
						m_disabledSkills.Remove(key);
					}
					return (int)left;
				}
			}
			return 0;
		}

		/// <summary>
		/// Gets a copy of all disabled skills
		/// </summary>
		/// <returns></returns>
		public virtual ICollection<Skill> GetAllDisabledSkills()
		{
			lock ((m_disabledSkills as ICollection).SyncRoot)
			{
				List<Skill> skillList = new List<Skill>();
				
				foreach(KeyValuePair<long, Skill> disabled in m_disabledSkills.Values)
					skillList.Add(disabled.Value);
				
				return skillList;
			}
		}

		/// <summary>
		/// Grey out some skills on client for specified duration
		/// </summary>
		/// <param name="skill">the skill to disable</param>
		/// <param name="duration">duration of disable in milliseconds</param>
		public virtual void DisableSkill(Skill skill, int duration)
		{
			lock ((m_disabledSkills as ICollection).SyncRoot)
			{
				KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
				if (duration > 0)
				{
					m_disabledSkills[key] = new KeyValuePair<long, Skill>(CurrentRegion.Time + duration, skill);
				}
				else
				{
					m_disabledSkills.Remove(key);
				}
			}
		}
		
		/// <summary>
		/// Grey out collection of skills on client for specified duration
		/// </summary>
		/// <param name="skill">the skill to disable</param>
		/// <param name="duration">duration of disable in milliseconds</param>
		public virtual void DisableSkill(ICollection<Tuple<Skill, int>> skills)
		{
			lock ((m_disabledSkills as ICollection).SyncRoot)
			{
				foreach (Tuple<Skill, int> tuple in skills)
				{
					Skill skill = tuple.Item1;
					int duration = tuple.Item2;
					
					KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
					if (duration > 0)
					{
						m_disabledSkills[key] = new KeyValuePair<long, Skill>(CurrentRegion.Time + duration, skill);
					}
					else
					{
						m_disabledSkills.Remove(key);
					}
				}
			}
		}
		

		/// <summary>
		/// Removes Greyed out skills
		/// </summary>
		/// <param name="skill">the skill to remove</param>
		public virtual void RemoveDisabledSkill(Skill skill)
		{
			lock ((m_disabledSkills as ICollection).SyncRoot)
			{
				KeyValuePair<int, Type> key = new KeyValuePair<int, Type>(skill.ID, skill.GetType());
				if(m_disabledSkills.ContainsKey(key))
					m_disabledSkills.Remove(key);
			}
		}

		#region Broadcasting utils

		/// <summary>
		/// Broadcasts the living equipment to all players around
		/// </summary>
		public virtual void BroadcastLivingEquipmentUpdate()
		{
			if (ObjectState != eObjectState.Active)
				return;
			
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null)
					continue;
				
				player.Out.SendLivingEquipmentUpdate(this);
			}
		}
		
		#endregion
		
		#region Region

		/// <summary>
		/// Creates the item in the world
		/// </summary>
		/// <returns>true if object was created</returns>
		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;

			if (m_attackAction != null)
				m_attackAction.Stop();
			m_attackAction = new AttackAction(this);
			return true;
		}

		/// <summary>
		/// Removes the item from the world
		/// </summary>
		public override bool RemoveFromWorld()
		{
			if (!base.RemoveFromWorld()) return false;

			StopAttack();
			List<GameObject> temp;
			lock (Attackers)
			{
				temp = new List<GameObject>(m_attackers);
				m_attackers.Clear();
			}
			Util.ForEach(temp.OfType<GameLiving>(), o => o.EnemyKilled(this));
			StopHealthRegeneration();
			StopPowerRegeneration();
			StopEnduranceRegeneration();

			if (m_attackAction != null) m_attackAction.Stop();
			if (this is GameNPC && ((GameNPC)this).SpellTimer != null) ((GameNPC)this).SpellTimer.Stop();
			if (m_healthRegenerationTimer != null) m_healthRegenerationTimer.Stop();
			if (m_powerRegenerationTimer != null) m_powerRegenerationTimer.Stop();
			if (m_enduRegenerationTimer != null) m_enduRegenerationTimer.Stop();
			m_attackAction = null;
			m_healthRegenerationTimer = null;
			m_powerRegenerationTimer = null;
			m_enduRegenerationTimer = null;
			return true;
		}

		#endregion
		#region Spell Cast
		/// <summary>
		/// Multiplier for melee and magic.
		/// </summary>
		public virtual double Effectiveness
		{
			get { return 1.0; }
			set { }
		}

		public virtual bool IsCasting
		{
			get { return m_runningSpellHandler != null && m_runningSpellHandler.IsCasting; }
		}

		/// <summary>
		/// Returns true if the living has the spell effect, else false.
		/// </summary>
		/// <param name="spell"></param>
		/// <returns></returns>
		public override bool HasEffect(Spell spell)
		{
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (effect is GameSpellEffect)
					{
						GameSpellEffect spellEffect = effect as GameSpellEffect;

						if (spellEffect.Spell.SpellType == spell.SpellType &&
						    spellEffect.Spell.EffectGroup == spell.EffectGroup)
							return true;
					}
				}
			}

			return base.HasEffect(spell);
		}

		/// <summary>
		/// Checks if the target has a type of effect
		/// </summary>
		/// <param name="target"></param>
		/// <param name="spell"></param>
		/// <returns></returns>
		public override bool HasEffect(Type effectType)
		{
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
					if (effect.GetType() == effectType)
						return true;
			}

			return base.HasEffect(effectType);
		}

		/// <summary>
		/// Holds the currently running spell handler
		/// </summary>
		protected ISpellHandler m_runningSpellHandler;
		/// <summary>
		/// active spellhandler (casting phase) or null
		/// </summary>
		public ISpellHandler CurrentSpellHandler
		{
			// change for warlock
			get { return m_runningSpellHandler; }
			set { m_runningSpellHandler = value; }
		}

		/// <summary>
		/// Callback after spell casting is complete and next spell can be processed
		/// </summary>
		/// <param name="handler"></param>
		public virtual void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			m_runningSpellHandler.CastingCompleteEvent -= new CastingCompleteCallback(OnAfterSpellCastSequence);
			m_runningSpellHandler = null;
		}

		/// <summary>
		/// Immediately stops currently casting spell
		/// </summary>
		public virtual void StopCurrentSpellcast()
		{
			if (m_runningSpellHandler != null)
				m_runningSpellHandler.InterruptCasting();
		}


		/// <summary>
		/// Cast a specific spell from given spell line
		/// </summary>
		/// <param name="spell">spell to cast</param>
		/// <param name="line">Spell line of the spell (for bonus calculations)</param>
		/// <returns>Whether the spellcast started successfully</returns>
		public virtual bool CastSpell(Spell spell, SpellLine line)
		{
			if (IsStunned || IsMezzed)
			{
				Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.CrowdControlled));
				return false;
			}

			if ((m_runningSpellHandler != null && spell.CastTime > 0))
			{
				Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.AlreadyCasting));
				return false;
			}

			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
			if (spellhandler != null)
			{
				if (spell.CastTime > 0)
				{
					m_runningSpellHandler = spellhandler;
					spellhandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				}
				return spellhandler.CastSpell();
			}
			else
			{
				if (log.IsWarnEnabled)
					log.Warn(Name + " wants to cast but spell " + spell.Name + " not implemented yet");
			}

			return false;
		}

		public virtual bool CastSpell(ISpellCastingAbilityHandler ab)
		{
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, ab.Spell, ab.SpellLine);
			if (spellhandler != null)
			{
				// Instant cast abilities should not interfere with the spell queue
				if (spellhandler.Spell.CastTime > 0)
				{
					m_runningSpellHandler = spellhandler;
					m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				}

				spellhandler.Ability = ab;
				return spellhandler.CastSpell();
			}
			return false;
		}

		#endregion
		#region LoadCalculators
		/// <summary>
		/// Load the property calculations
		/// </summary>
		/// <returns></returns>
		public static bool LoadCalculators()
		{
			try
			{
				foreach (Assembly asm in ScriptMgr.GameServerScripts)
				{
					foreach (Type t in asm.GetTypes())
					{
						try
						{
							if (!t.IsClass || t.IsAbstract) continue;
							if (!typeof(IPropertyCalculator).IsAssignableFrom(t)) continue;
							IPropertyCalculator calc = (IPropertyCalculator)Activator.CreateInstance(t);
							foreach (PropertyCalculatorAttribute attr in t.GetCustomAttributes(typeof(PropertyCalculatorAttribute), false))
							{
								for (int i = (int)attr.Min; i <= (int)attr.Max; i++)
								{
									m_propertyCalc[i] = calc;
								}
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("Error while working with type " + t.FullName, e);
						}
					}
				}
				return true;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("GameLiving.LoadCalculators()", e);
				return false;
			}
		}
		#endregion
		#region ControlledNpc

		private byte m_petCount = 0;

		/// <summary>
		/// Gets the pet count for this living
		/// </summary>
		public byte PetCount
		{
			get { return m_petCount; }
			set { m_petCount = value; }
		}

		/// <summary>
		/// Holds the controlled object
		/// </summary>
		protected IControlledBrain[] m_controlledBrain = null;

		/// <summary>
		/// Initializes the ControlledNpcs for the GameLiving class
		/// </summary>
		/// <param name="num">Number of places to allocate.  If negative, sets to null.</param>
		public virtual void InitControlledBrainArray(int num)
		{
			if (num > 0)
			{
				m_controlledBrain = new IControlledBrain[num];
			}
			else
			{
				m_controlledBrain = null;
			}
		}

		/// <summary>
		/// Get or set the ControlledBrain.  Set always uses m_controlledBrain[0]
		/// </summary>
		public virtual IControlledBrain ControlledBrain
		{
			get
			{
				if (m_controlledBrain == null)
					return null;

				return m_controlledBrain[0];
			}
			set
			{
				m_controlledBrain[0] = value;
			}
		}

		public virtual bool IsControlledNPC(GameNPC npc)
		{
			if (npc == null)
			{
				return false;
			}
			IControlledBrain brain = npc.Brain as IControlledBrain;
			if (brain == null)
			{
				return false;
			}
			return brain.GetLivingOwner() == this;
		}

		/// <summary>
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public virtual void SetControlledBrain(IControlledBrain controlledBrain)
		{
		}

		#endregion
		#region Group
		/// <summary>
		/// Holds the group of this living
		/// </summary>
		protected Group m_group;
		/// <summary>
		/// Holds the index of this living inside of the group
		/// </summary>
		protected byte m_groupIndex;

		/// <summary>
		/// Gets or sets the living's group
		/// </summary>
		public Group Group
		{
			get { return m_group; }
			set { m_group = value; }
		}

		/// <summary>
		/// Gets or sets the index of this living inside of the group
		/// </summary>
		public byte GroupIndex
		{
			get { return m_groupIndex; }
			set { m_groupIndex = value; }
		}
		#endregion
		
		/// <summary>
		/// Handle event notifications.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			if (e == GameLivingEvent.Interrupted && args != null)
			{
				if (CurrentSpellHandler != null)
					CurrentSpellHandler.CasterIsAttacked((args as InterruptedEventArgs).Attacker);

				return;
			}

			base.Notify(e, sender, args);
		}
		
		/// <summary>
		/// Constructor to create a new GameLiving
		/// </summary>
		public GameLiving()
			: base()
		{
			m_guildName = string.Empty;
			m_targetObjectWeakReference = new WeakRef(null);
			m_groundTarget = new Point3D(0, 0, 0);

			//Set all combat properties
			m_activeWeaponSlot = eActiveWeaponSlot.Standard;
			m_activeQuiverSlot = eActiveQuiverSlot.None;
			m_rangedAttackState = eRangedAttackState.None;
			m_rangedAttackType = eRangedAttackType.Normal;
			m_xpGainers = new HybridDictionary();
			m_effects = CreateEffectsList();
			m_concEffects = new ConcentrationList(this);
			m_attackers = new List<GameObject>();

			m_health = 1;
			m_mana = 1;
			m_endurance = 1;
			m_maxEndurance = 1;
		}
	}
}
