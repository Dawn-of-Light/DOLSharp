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
using System.Collections.Specialized;
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.PacketHandler;
using DOL.GS.PropertyCalc;
using DOL.GS.Scripts;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using log4net;

namespace DOL.GS
{

	#region AttackData

	/// <summary>
	/// Holds all data for an Attack
	/// </summary>
	public class AttackData
	{
		private GameLiving     m_attacker = null;
		private GameLiving     m_target = null;
		private eArmorSlot     m_hitArmorSlot = eArmorSlot.UNKNOWN;
		private int            m_damage = 0;
		private int            m_critdamage = 0;
		private int            m_uncappeddamage = 0;
		private int            m_styledamage = 0;
		private int            m_modifier = 0;
		private eDamageType    m_damageType = 0;
		private Style          m_style = null;
		private eAttackType    m_attackType = eAttackType.Unknown;
		private GameLiving.eAttackResult m_attackResult = GameLiving.eAttackResult.Any;
		private ISpellHandler  m_styleEffect;
		private int            m_animationId;
		private int            m_weaponSpeed;

		/// <summary>
		/// Constructs new AttackData
		/// </summary>
		public AttackData()
		{
		}

		/// <summary>
		/// Sets or gets the modifier (resisted damage)
		/// </summary>
		public int Modifier {
			get { return m_modifier; }
			set { m_modifier = value; }
		}

		/// <summary>
		/// Sets or gets the attacker
		/// </summary>
		public GameLiving Attacker {
			get { return m_attacker; }
			set { m_attacker = value; }
		}

		/// <summary>
		/// Sets or gets the attack target
		/// </summary>
		public GameLiving Target {
			get { return m_target; }
			set { m_target = value; }
		}

		/// <summary>
		/// Sets or gets the armor hit location
		/// </summary>
		public eArmorSlot ArmorHitLocation {
			get { return m_hitArmorSlot; }
			set { m_hitArmorSlot = value; }
		}

		/// <summary>
		/// Sets or gets the damage
		/// </summary>
		public int Damage {
			get { return m_damage; }
			set { m_damage = value; }
		}

		/// <summary>
		/// Sets or gets the uncapped damage
		/// </summary>
		public int UncappedDamage {
			get { return m_uncappeddamage; }
			set { m_uncappeddamage = value; }
		}

		/// <summary>
		/// Sets or gets the critical damage
		/// </summary>
		public int CriticalDamage {
			get { return m_critdamage; }
			set { m_critdamage = value; }
		}

		/// <summary>
		/// Sets or gets the style damage
		/// </summary>
		public int StyleDamage {
			get { return m_styledamage; }
			set { m_styledamage = value; }
		}

		/// <summary>
		/// Sets or gets the damage type
		/// </summary>
		public eDamageType DamageType {
			get { return m_damageType; }
			set { m_damageType = value; }
		}

		/// <summary>
		/// Sets or gets the style used
		/// </summary>
		public Style Style
		{
			get { return m_style; }
			set { m_style = value; }
		}

		/// <summary>
		/// Sets or gets the attack result
		/// </summary>
		public GameLiving.eAttackResult AttackResult
		{
			get { return m_attackResult; }
			set { m_attackResult = value; }
		}

		/// <summary>
		/// Sets or gets the style effect
		/// </summary>
		public ISpellHandler StyleEffect
		{
			get { return m_styleEffect; }
			set { m_styleEffect = value; }
		}

		/// <summary>
		/// Sets or gets the weapon speed
		/// </summary>
		public int WeaponSpeed
		{
			get { return m_weaponSpeed; }
			set { m_weaponSpeed = value; }
		}

		/// <summary>
		/// Checks whether attack type is one of melee types
		/// </summary>
		public bool IsMeleeAttack
		{
			get
			{
				return m_attackType == eAttackType.MeleeOneHand
					|| m_attackType == eAttackType.MeleeTwoHand
					|| m_attackType == eAttackType.MeleeDualWield;
			}
		}

		/// <summary>
		/// The type of attack
		/// </summary>
		public enum eAttackType : int
		{
			/// <summary>
			/// Attack type has not been set yet
			/// </summary>
			Unknown = -1,
			/// <summary>
			/// Attack is done using a weapon in one hand
			/// </summary>
			MeleeOneHand = 1,
			/// <summary>
			/// Attack is done using one weapon in each hand
			/// </summary>
			MeleeDualWield = 2,
			/// <summary>
			/// Attack is done using one same weapon in both hands
			/// </summary>
			MeleeTwoHand = 3,
			/// <summary>
			/// Attack is done using a weapon in ranged slot
			/// </summary>
			Ranged = 4,
			/// <summary>
			/// Attack is done with a spell
			/// </summary>
			Spell = 5,
		}

		/// <summary>
		/// Sets or gets the attack type
		/// </summary>
		public eAttackType AttackType
		{
			get { return m_attackType; }
			set { m_attackType = value; }
		}

		/// <summary>
		/// Sets or gets the attack animation ID
		/// </summary>
		public int AnimationId
		{
			get { return m_animationId; }
			set { m_animationId = value; }
		}
	}

	#endregion

	/// <summary>
	/// This class holds all information that each
	/// living object in the world uses
	/// </summary>
	public abstract class GameLiving : GameObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Combat
		/// <summary>
		/// Holds the Attack Data object of last attack
		/// </summary>
		public const string LAST_ATTACK_DATA="LastAttackData";
		/// <summary>
		/// Holds the property for the result the last enemy
		/// </summary>
		public const string LAST_ENEMY_ATTACK_RESULT="LastEnemyAttackResult";
		/// <summary>
		/// Holds the property for game tick of last attack
		/// </summary>
		protected const string LAST_ATTACK_TICK="LastAttackTick";
		/// <summary>
		/// The result of an attack
		/// </summary>
		public enum eAttackResult:int
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
			Fumbled = 15
		}

		/// <summary>
		/// The possible states for a ranged attack
		/// </summary>
		public enum eRangeAttackState : byte
		{
			/// <summary>
			/// No ranged attack active
			/// </summary>
			None=0,
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
		public enum eRangeAttackType : byte
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
		/// The state of the ranged attack
		/// </summary>
		protected eRangeAttackState m_rangeAttackState;
		/// <summary>
		/// The gtype of the ranged attack
		/// </summary>
		protected eRangeAttackType m_rangeAttackType;

		/// <summary>
		/// Gets or Sets the state of a ranged attack
		/// </summary>
		public eRangeAttackState RangeAttackState
		{
			get { return m_rangeAttackState; }
			set
			{
				m_rangeAttackState = value;
				//DOLConsole.WriteLine("("+this.Name+") RangeAttackState="+value);
			}
		}

		/// <summary>
		/// Gets or Sets the type of a ranged attack
		/// </summary>
		public eRangeAttackType RangeAttackType
		{
			get { return m_rangeAttackType; }
			set { m_rangeAttackType = value; }
		}

		/// <summary>
		/// Holds all the ways this living can
		/// be healed
		/// </summary>
		public enum eHealthChangeType: byte
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
		public enum eManaChangeType: byte
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
		public enum eEnduranceChangeType: byte
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
		public enum eActiveWeaponSlot: byte
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
		public enum eActiveQuiverSlot: byte
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

		/// <summary>
		/// Holds the quiverslot to be used
		/// </summary>
		protected eActiveQuiverSlot	m_activeQuiverSlot;

		/// <summary>
		/// Gets/Sets the current active quiver slot of this living
		/// </summary>
		public virtual eActiveQuiverSlot ActiveQuiverSlot
		{
			get	{ return m_activeQuiverSlot; }
			set	{ m_activeQuiverSlot = value; }
		}

		/// <summary>
		/// say if player is stun or not
		/// </summary>
		protected bool m_stuned;
		/// <summary>
		/// Gets the stunned flag of this living
		/// </summary>
		public bool Stun
		{
			get { return m_stuned; }
			set { m_stuned = value; }
		}
		/// <summary>
		/// say if player is mez or not
		/// </summary>
		protected bool m_mezzed;
		/// <summary>
		/// Gets the mesmerized flag of this living
		/// </summary>
		public bool Mez
		{
			get { return m_mezzed; }
			set { m_mezzed = value; }
		}

		/// <summary>
		/// Holds disease counter
		/// </summary>
		protected sbyte m_diseasedCount;
		/// <summary>
		/// Sets disease state
		/// </summary>
		/// <param name="add">true if disease counter should be increased</param>
		public virtual void Disease(bool add)
		{
			if (add) m_diseasedCount++;
			else m_diseasedCount--;

			if (m_diseasedCount < 0)
			{
				if (log.IsErrorEnabled)
					log.Error("m_diseasedCount is less than zero.\n"+Environment.StackTrace);
			}
		}
		/// <summary>
		/// Gets diseased state
		/// </summary>
		public virtual bool IsDiseased
		{
			get { return m_diseasedCount > 0; }
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
			{
				if (log.IsErrorEnabled)
					log.Error("m_turningDisabledCount is less than zero.\n"+Environment.StackTrace);
			}
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
		protected eActiveWeaponSlot	m_activeWeaponSlot;
		/// <summary>
		/// AttackAction used for making an attack every weapon speed intervals
		/// </summary>
		protected AttackAction m_attackAction;
		/// <summary>
		/// The objects currently attacking this living
		/// To be more exact, the objects that are in combat
		/// and have this living as target.
		/// </summary>
		protected readonly ArrayList	m_attackers;
		/// <summary>
		/// Returns the current active weapon slot of this living
		/// </summary>
		public virtual eActiveWeaponSlot ActiveWeaponSlot
		{
			get	{ return m_activeWeaponSlot; }
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
		/// last attack tick
		/// </summary>
		protected long m_lastAttackTick;
		/// <summary>
		/// gets/sets gametick when this living has attacked its target
		/// </summary>
		public virtual long LastAttackTick
		{
			get { return m_lastAttackTick; }
			set { m_lastAttackTick = value; }
		}
		/// <summary>
		/// last attacked by enemy tick
		/// </summary>
		protected long m_lastAttackedByEnemyTick;
		/// <summary>
		/// gets/sets gametick when this living was last time attacked by an enemy
		/// </summary>
		public virtual long LastAttackedByEnemyTick
		{
			get { return m_lastAttackedByEnemyTick; }
			set { m_lastAttackedByEnemyTick = value; }
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
			return null;
		}
		/// <summary>
		/// Gets the current attackspeed of this living in milliseconds
		/// </summary>
		/// <param name="weapon">attack weapons</param>
		/// <returns>effective speed of the attack. average if more than one weapon.</returns>
		public virtual int AttackSpeed(params InventoryItem[] weapon)
		{
			//TODO needs to come from the DB
			double speed = 3400 * (1.0 - (GetModified(eProperty.Quickness) - 60)/500.0);
			//Combat Speed buff and debuff
			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				speed *= GetModified(eProperty.ArcherySpeed)*0.01;
			}
			else
			{
				speed *= GetModified(eProperty.MeleeSpeed)*0.01;
			}

			return (int)speed;
		}
		/// <summary>
		/// Returns the Damage this Living does on an attack
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		/// <returns></returns>
		public virtual double AttackDamage(InventoryItem weapon)
		{
			double damage = (1.0 + Level/3.7 + Level*Level/175.0) * AttackSpeed(weapon) * 0.001;
			if(weapon == null || weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type ==  Slot.TWOHAND)
			{
				//Melee damage buff and debuff
				damage *= GetModified(eProperty.MeleeDamage)*0.01;
			}
			else if(weapon.Item_Type == Slot.RANGED)
			{
				//Ranged damage buff and debuff
				damage *= GetModified(eProperty.RangedDamage)*0.01;
			}

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
		/// Returns the AttackRange of this living
		/// </summary>
		public virtual int AttackRange
		{
			get
			{
				//Mobs have a good distance range with distance weapons
				//automatically
				if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					return Math.Max(32, (int)(2000.0 * GetModified(eProperty.ArcheryRange)*0.01));
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
			return GetModified(eProperty.ArmorAbsorbtion) * 0.01;
		}

		/// <summary>
		/// Calculates keep bonuses
		/// </summary>
		/// <returns></returns>
		public virtual double GetKeepBonuses()
		{
			return 0;
		}
		/// <summary>
		/// Gets the weaponskill of weapon
		/// </summary>
		public virtual double GetWeaponSkill(InventoryItem weapon)
		{
				const double bs = 128.0 / 50.0;	// base factor (not 400)
				return (int)((Level+1) * bs * (1 + (GetWeaponStat(weapon) - 50) * 0.005) * (Level*2/50));
		}

		/// <summary>
		/// Returns the weapon used to attack, null=natural
		/// </summary>
		public virtual InventoryItem AttackWeapon
		{
			get
			{
				if(Inventory != null)
				{
					switch (ActiveWeaponSlot)
					{
						case eActiveWeaponSlot.Standard : return Inventory.GetItem(eInventorySlot.RightHandWeapon);
						case eActiveWeaponSlot.TwoHanded: return Inventory.GetItem(eInventorySlot.TwoHandWeapon);
						case eActiveWeaponSlot.Distance : return Inventory.GetItem(eInventorySlot.DistanceWeapon);
					}
				}
				return null;
			}
			set { }
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
			get { return 0; }
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
		/// Stores the attack state of this living
		/// </summary>
		protected bool m_attackState;
		/// <summary>
		/// Gets the attack-state of this living
		/// </summary>
		public virtual bool AttackState
		{
			get { return m_attackState; }
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
			if (weapon==null) return 0;
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
		/// returns if this living is alive
		/// </summary>
		public virtual bool Alive
		{
			get { return Health>0; }
		}
		/// <summary>
		/// Check this flag to see wether this living is involved in combat
		/// </summary>
		public virtual bool InCombat
		{
			get {
				Region region = CurrentRegion;
				if (region == null)
					return false;
				return LastAttackTick+10000 >= region.Time || LastAttackedByEnemyTick+10000 >= region.Time;
			}
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

		/// <summary>
		/// Checks whether object is grey con to this living
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public bool IsObjectGreyCon(GameObject obj)
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
			if(sourceLevel < GameLiving.NoXPForLevel.Length)
			{
				//if target level is less or equals to level that is grey to source
				if(target.EffectiveLevel <= GameLiving.NoXPForLevel[sourceLevel])
					return true;
			}
			else
			{
				if(source.GetConLevel(target) <= -3)
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
		/// <param name="dualWield">indicates if both weapons are used for attack</param>
		/// <returns>the object where we collect and modifiy all parameters about the attack</returns>
		protected virtual AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
		{
			AttackData ad = new AttackData();
			ad.Attacker = this;
			ad.Target = target as GameLiving;
			ad.Damage = 0;
			ad.CriticalDamage = 0;
			ad.Style = style;
			ad.WeaponSpeed = (weapon == null ? AttackSpeed(null)/100 : weapon.SPD_ABS);
			ad.DamageType = AttackDamageType(weapon);
			ad.ArmorHitLocation = eArmorSlot.UNKNOWN;

			if (dualWield)
				ad.AttackType = AttackData.eAttackType.MeleeDualWield;
			else if (weapon == null)
				ad.AttackType = AttackData.eAttackType.MeleeOneHand;
			else switch (weapon.Item_Type)
			{
				default:
				case Slot.RIGHTHAND:
				case Slot.LEFTHAND: ad.AttackType = AttackData.eAttackType.MeleeOneHand; break;
				case Slot.TWOHAND : ad.AttackType = AttackData.eAttackType.MeleeTwoHand; break;
				case Slot.RANGED  : ad.AttackType = AttackData.eAttackType.Ranged; break;
			}

			//No target, stop the attack
			if(ad.Target==null) {
				ad.AttackResult = (target==null) ? eAttackResult.NoTarget : eAttackResult.NoValidTarget;
				return ad;
			}

			// check region
			if (ad.Target.CurrentRegionID != CurrentRegionID || ad.Target.ObjectState != eObjectState.Active) {
				ad.AttackResult = eAttackResult.NoValidTarget;
				return ad;
			}

			//Check if the target is in front of attacker
			if(this is GamePlayer && !(ad.Target is GameKeepComponent) && !IsObjectInFront(ad.Target, 120)) {
				ad.AttackResult = eAttackResult.TargetNotVisible;
				return ad;
			}

			//Target is dead already
			if(!ad.Target.Alive) {
				ad.AttackResult = eAttackResult.TargetDead;
				return ad;
			}

			//We have no attacking distance!
			if (!WorldMgr.CheckDistance(this,ad.Target,AttackRange)){
				//DOLConsole.LogLine(this.Name+"("+X+","+Y+","+Z+") attacks but "+ad.Target.Name+"("+ad.Target.X+","+ad.Target.Y+","+ad.Target.Z+") out of range "+dist+" > "+AttackRange);
				ad.AttackResult = eAttackResult.OutOfRange;
				return ad;
			}

			if(!GameServer.ServerRules.IsAllowedToAttack(ad.Attacker, ad.Target, false)) {
				ad.AttackResult = eAttackResult.NotAllowed_ServerRules;
				return ad;
			}

			//Calculate our attack result and attack damage
			ad.AttackResult = ad.Target.CalculateEnemyAttackResult(ad, weapon);

			// calculate damage only if we hit the target
			if (ad.AttackResult == eAttackResult.HitUnstyled
				|| ad.AttackResult == eAttackResult.HitStyle)
			{
				double damage = AttackDamage(weapon) * effectiveness;

				InventoryItem armor = null;
				if (ad.Target.Inventory != null)
					armor = ad.Target.Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

				int lowerboundary = (WeaponSpecLevel(weapon)-1) * 50 / (ad.Target.EffectiveLevel + 1) + 75;
				lowerboundary = Math.Max(lowerboundary, 75);
				lowerboundary = Math.Min(lowerboundary, 125);
				damage *= (GetWeaponSkill(weapon) + 90.68) / (ad.Target.GetArmorAF(ad.ArmorHitLocation) + 20*4.67);
				damage *= 1.0 - Math.Min(0.85, ad.Target.GetArmorAbsorb(ad.ArmorHitLocation));
				damage *= (lowerboundary + Util.Random(50)) * 0.01;
				ad.Modifier = (int)(damage * (ad.Target.GetResist(ad.DamageType) + SkillBase.GetArmorResist(armor, ad.DamageType)) * -0.01);
				damage += ad.Modifier;
				ad.Damage = (int)damage;

				// apply total damage cap
				ad.UncappedDamage = ad.Damage;
				ad.Damage = Math.Min(ad.Damage, (int)(UnstyledDamageCap(weapon)*effectiveness));

				// patch to missed when 0 damage
				if (ad.Damage==0)
				{
					if (log.IsDebugEnabled)
						log.Debug("Damage=0 -> miss "+AttackDamage(weapon));
					ad.AttackResult = eAttackResult.Missed;
				}
			}

				//Add styled damage if style hits and remove endurance if missed
			if (StyleProcessor.ExecuteStyle(this, ad, weapon)) {
				ad.AttackResult = GameLiving.eAttackResult.HitStyle;
			}

			if (ad.AttackResult == eAttackResult.HitUnstyled
				|| ad.AttackResult == eAttackResult.HitStyle)
			{
				ad.CriticalDamage = CalculateCriticalDamage(ad, weapon);
			}

			string message = "";
			bool broadcast = true;
			ArrayList excludes = new ArrayList();
			excludes.Add(ad.Attacker);
			excludes.Add(ad.Target);

			switch(ad.AttackResult)
			{
				case eAttackResult.Parried: message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" and is parried!"; break;
				case eAttackResult.Evaded : message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" and is evaded!"; break;
				case eAttackResult.Missed : message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" and misses!"; break;
				case eAttackResult.Blocked:
				{
					message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" and is blocked!";
					// guard messages
					if (target != null && target != ad.Target)
					{
						excludes.Add(target);

						// another player blocked for real target
						if (target is GamePlayer)
							((GamePlayer)target).Out.SendMessage(string.Format("{0} blocks you from {1}'s attack!", ad.Target.GetName(0, true), ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
						// blocked for another player
						if (ad.Target is GamePlayer)
							((GamePlayer)ad.Target).Out.SendMessage(string.Format("You block {0}'s attack against {1}!", ad.Attacker.GetName(0, false), target.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					}
					else if (ad.Target is GamePlayer)
					{
						((GamePlayer)ad.Target).Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you block the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					}
					break;
				}
				case eAttackResult.HitUnstyled:
				case eAttackResult.HitStyle:
				{
					// intercept messages
					if (target != null && target != ad.Target)
					{
						message = string.Format("{0} attacks {1} but hits {2}!", ad.Attacker.GetName(0, true), target.GetName(0, false), ad.Target.GetName(0, false));
						excludes.Add(target);

						// intercept for another player
						if (target is GamePlayer)
							((GamePlayer)target).Out.SendMessage(ad.Target.GetName(0, true) + " steps in front of you and takes the blow!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						// intercept by player
						if (ad.Target is GamePlayer)
							((GamePlayer)ad.Target).Out.SendMessage("You step in front of " + target.GetName(0, false) + " and take the blow!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					}
					else
					{
						// TODO: weapon type names
						// "PLAYER attacks the PLAYER with his dirk!"
						// "PLAYER attacks the PLAYER with his sword!"
						// "PLAYER attacks the PLAYER with her staff!"
						// "PLAYER attacks the PLAYER with his shield!"

						if(ad.Attacker is GamePlayer)
						{
							message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" with "+ ad.Attacker.GetPronoun(1, false) +" weapon!";
						}
						else
						{
							message = ad.Attacker.GetName(0, true) +" attacks "+ ad.Target.GetName(0, false) +" and hits!";
						}
					}
					break;
				}
				default: broadcast = false; break;
			}

			#region controlled messages

			if (ad.Attacker is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)ad.Attacker).Brain as IControlledBrain;
				if(brain != null)
				{
					GamePlayer owner = brain.Owner;
					if (owner != null && owner.ControlledNpc != null && ad.Attacker == owner.ControlledNpc.Body)
					{
						excludes.Add(owner);
						switch (ad.AttackResult)
						{
							case eAttackResult.HitStyle:
							case eAttackResult.HitUnstyled:
								{
									string modmessage = "";
									if (ad.Modifier > 0) modmessage = " (+"+ad.Modifier+")";
									if (ad.Modifier < 0) modmessage = " ("+ad.Modifier+")";
									string attackTypeMsg = "attack";
									if(ad.Attacker.ActiveWeaponSlot==eActiveWeaponSlot.Distance)
										attackTypeMsg = "shot";
									owner.Out.SendMessage(string.Format("Your {0} {1} {2} and hit for {3}{4} damage!", ad.Attacker.Name, attackTypeMsg, ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
									if (ad.CriticalDamage > 0)
										owner.Out.SendMessage("Your " + ad.Attacker.Name + " critical hit " + ad.Target.GetName(0, false) + " for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
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
					GamePlayer owner = brain.Owner;
					excludes.Add(owner);
					if (owner != null && owner.ControlledNpc != null && ad.Target == owner.ControlledNpc.Body)
					{
						switch (ad.AttackResult)
						{
							case eAttackResult.Blocked:
								owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks your " + ad.Target.Name + " and pet block the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Parried:
								owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks your " + ad.Target.Name + " and pet parry the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Evaded:
								owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks your " + ad.Target.Name + " and pet evade the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Fumbled:
								owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " fumbled!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.Missed:
								if(ad.AttackType != AttackData.eAttackType.Spell)
									owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks your " + ad.Target.Name + " and misses!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
								break;
							case eAttackResult.HitStyle:
							case eAttackResult.HitUnstyled:
								{
									string modmessage = "";
									if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
									if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
									owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " hits your " + ad.Target.Name + " for " + ad.Damage + modmessage + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
									if (ad.CriticalDamage > 0)
									{
										owner.Out.SendMessage(ad.Attacker.GetName(0, true) + " critical hits your " + ad.Target.Name + " for an additional " + ad.CriticalDamage + " damage.", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
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
			if(broadcast)
			{
				Message.SystemToArea(ad.Attacker, message, eChatType.CT_OthersCombat, (GameObject[])excludes.ToArray(typeof(GameObject)));
			}

			// start interrupt timer for casting victims
			ad.Target.StartInterruptTimer(interruptDuration, ad.AttackType, this);

			//Return the result
			return ad;
		}

		/// <summary>
		/// Starts interrupt timer on this living
		/// </summary>
		/// <param name="duration">The full interrupt duration in milliseconds</param>
		/// <param name="attacker">The source of interrupts</param>
		public virtual void StartInterruptTimer(int duration, AttackData.eAttackType attackType, GameLiving attacker)
		{
			new InterruptAction(this, attacker, duration, attackType).Start(1);
		}

		/// <summary>
		/// Interrupts the target for the specified duration
		/// </summary>
		protected class InterruptAction : RegionAction
		{
			/// <summary>
			/// Holds the interrupt source
			/// </summary>
			protected readonly GameLiving m_attacker;
			/// <summary>
			/// The full duration of interrupts in milliseconds
			/// </summary>
			protected int m_duration;
			/// <summary>
			/// Holds the interrupt attack data
			/// </summary>
			protected AttackData.eAttackType m_attackType;

			/// <summary>
			/// Constructs a new interrupt action
			/// </summary>
			/// <param name="target">The interrupt target</param>
			/// <param name="attacker">The attacker that is interrupting</param>
			/// <param name="duration">The interrupt duration in milliseconds</param>
			public InterruptAction(GameLiving target, GameLiving attacker, int duration, AttackData.eAttackType attackType) : base(target)
			{
				if (attacker == null)
					throw new ArgumentNullException("attacker");
				m_attacker = attacker;
				m_duration = duration;
				m_attackType = attackType;
				Interval = 1000;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving target = (GameLiving)m_actionSource;
				if (!target.Alive || target.ObjectState != eObjectState.Active)
				{
					Stop();
					return;
				}

				m_duration -= Interval;
				if (m_duration <= 0)
					Interval = 0;

				if (target.CurrentSpellHandler != null)
					target.CurrentSpellHandler.CasterIsAttacked(m_attacker);
				if (target.AttackState && target.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
					target.OnInterruptTick(m_attacker, m_attackType);
			}
		}

		/// <summary>
		/// Does needed interrupt checks and interrupts this living
		/// </summary>
		/// <param name="attacker">the attacker that is interrupting</param>
		/// <returns>true if interrupted successfully</returns>
		protected virtual bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if(AttackState && ActiveWeaponSlot==eActiveWeaponSlot.Distance)
			{
				if(RangeAttackType==eRangeAttackType.SureShot)
				{
					if(!attacker.AttackState
					|| attackType != AttackData.eAttackType.MeleeOneHand
					&& attackType != AttackData.eAttackType.MeleeTwoHand
					&& attackType != AttackData.eAttackType.MeleeDualWield)
						return false;
				}
				double mod = GetConLevel(attacker);
				double interruptChance = 65;
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
		protected virtual ItemTemplate RangeAttackAmmo
		{
			get { return null; }
			set {}
		}

		/// <summary>
		/// Gets/Sets the target for current ranged attack
		/// </summary>
		/// <returns></returns>
		protected virtual GameObject RangeAttackTarget
		{
			get { return TargetObject; }
			set {}
		}

		/// <summary>
		/// Creates an attack action for this living
		/// </summary>
		/// <returns></returns>
		protected virtual AttackAction CreateAttackAction()
		{
			return new AttackAction(this);
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
			public AttackAction(GameLiving owner) : base(owner)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameLiving owner = (GameLiving)m_actionSource;

				if (owner.Mez || owner.Stun)
				{
					Interval = 100;
					return;
				}

				if(!owner.AttackState)
				{
					AttackData ad = owner.TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
					owner.TempProperties.removeProperty(LAST_ATTACK_DATA);
					if(ad != null && ad.Target != null)
						ad.Target.RemoveAttacker(owner);
					Stop();
					return;
				}

				// Don't attack if gameliving is engaging
				EngageEffect engage = (EngageEffect)owner.EffectList.GetOfType(typeof(EngageEffect));
				if (engage != null && engage.EngageSource == owner)
				{
					Interval = owner.AttackSpeed(owner.AttackWeapon); // while gameliving is engageing it doesn't attack.
					return;
				}

				// Store all datas witch must not change during the attack
				double effectiveness = 1;
				int ticksToTarget = 1;
				int interruptDuration = 0;
				int leftHandSwingCount = 0;
				Style combatStyle = null;
				InventoryItem attackWeapon = owner.AttackWeapon;
				InventoryItem leftWeapon = (owner.Inventory == null) ? null : owner.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
				GameObject attackTarget = null;

				if(owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					attackTarget = owner.RangeAttackTarget; // must be do here because RangeAttackTarget is changed in CheckRangeAttackState
					eCheckRangeAttackStateResult rangeCheckresult = owner.CheckRangeAttackState(attackTarget);
					if(rangeCheckresult==eCheckRangeAttackStateResult.Hold)
					{
						Interval = 1000;
						return; //Hold the shot another second
					}
					else if(rangeCheckresult==eCheckRangeAttackStateResult.Stop)
					{
						owner.StopAttack(); //Stop the attack
						Stop();
						return;
					}

					int model = (attackWeapon==null ? 0 : attackWeapon.Model);
					foreach(GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendCombatAnimation(owner, attackTarget, (ushort)model, 0x00, 0x01F7, 0x01, 0x80, ((GameLiving)attackTarget).HealthPercent);
					}

					interruptDuration = owner.AttackSpeed(attackWeapon);

					switch (owner.RangeAttackType)
					{
						case eRangeAttackType.Critical:
						{
							effectiveness = 2 - 0.3 * owner.GetConLevel(attackTarget);
							if(effectiveness > 2)
								effectiveness = 2;
							else if(effectiveness < 1.1)
								effectiveness = 1.1;
						}
							break;

						case eRangeAttackType.SureShot:
						{
							effectiveness = 0.5;
						}
							break;

						case eRangeAttackType.RapidFire:
						{
							// Source : http://www.camelotherald.com/more/888.shtml
							// - (About Rapid Fire) If you release the shot 75% through the normal timer, the shot (if it hits) does 75% of its normal damage. If you
							// release 50% through the timer, you do 50% of the damage, and so forth - The faster the shot, the less damage it does.

							// Source : http://www.camelotherald.com/more/901.shtml
							// Related note about Rapid Fire – interrupts are determined by the speed of the bow is fired, meaning that the time of interruptions for each shot will be scaled
							// down proportionally to bow speed. If that made your eyes bleed, here's an example from someone who would know: "I fire a 5.0 spd bow. Because I am buffed and have
							// stat bonuses, I fire that bow at 3.0 seconds. The resulting interrupt on the caster will last 3.0 seconds. If I rapid fire that same bow, I will fire at 1.5 seconds,
							// and the resulting interrupt will last 1.5 seconds."

							long rapidFireMaxDuration = owner.AttackSpeed(attackWeapon) / 2; // half of the total time
							long elapsedTime = owner.CurrentRegion.Time - owner.TempProperties.getLongProperty(GamePlayer.RANGE_ATTACK_HOLD_START, 0L); // elapsed time before ready to fire
							if(elapsedTime < rapidFireMaxDuration)
							{
								effectiveness = 0.5 + (double)elapsedTime * 0.5 /(double)rapidFireMaxDuration;
								interruptDuration = (int)(interruptDuration * effectiveness);
							}
						}
							break;
					}

					// calculate Penetrating Arrow damage reduction
					if (owner is GamePlayer && attackTarget is GameLiving)
					{
						int PALevel = ((GamePlayer)owner).GetAbilityLevel(Abilities.PenetratingArrow);
						if(PALevel > 0)
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

							if(bladeturn != null && attackTarget != bladeturn.SpellHandler.Caster)
							{
								// Penetrating Arrow work
								effectiveness *= 0.25 + PALevel * 0.25;
							}
						}
					}

					ticksToTarget = 1 + WorldMgr.GetDistance(owner, attackTarget) * 100 / 150; // 150 units per 1/10s
				}
				else
				{
					attackTarget = owner.TargetObject;

					// wait until target is selected
					if(attackTarget == null || attackTarget == owner)
					{
						Interval = 100;
						return;
					}

					AttackData ad = owner.TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
					if(ad != null && ad.AttackResult == eAttackResult.Fumbled)
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

					// calculate LA damage reduction
					if (owner is GamePlayer)
					{
						if(owner.CanUseLefthandedWeapon && leftWeapon != null && leftWeapon.Object_Type != (int)eObjectType.Shield
						&& attackWeapon != null && (attackWeapon.Item_Type == Slot.RIGHTHAND || attackWeapon.Item_Type == Slot.LEFTHAND))
						{
							leftHandSwingCount = owner.CalculateLeftHandSwingCount();

							int LASpec = ((GamePlayer)owner).GetModifiedSpecLevel(Specs.Left_Axe);
							if (LASpec > 0)
								effectiveness = 0.625 + 0.0034 * LASpec;
						}
					}

					// Damage is doubled on sitting players
					// but only with melee weapons; arrows and magic does normal damage.
					if (attackTarget is GamePlayer && ((GamePlayer)attackTarget).Sitting)
					{
						effectiveness *= 2;
					}

					ticksToTarget = 1;
				}

				new WeaponOnTargetAction(owner, attackTarget, attackWeapon, leftWeapon, leftHandSwingCount, effectiveness, interruptDuration , combatStyle).Start(ticksToTarget);  // really start the attack


				//Are we inactive?
				if (owner.ObjectState != eObjectState.Active)
				{
					Stop();
					return;
				}

				if(owner.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					if(owner.RangeAttackState != eRangeAttackState.AimFireReload)
					{
						owner.StopAttack();
						Stop();
						return;
					}
					else
					{
						owner.RangeAttackType = eRangeAttackType.Normal;
						lock (owner.EffectList)
						{
							foreach (IGameEffect effect in owner.EffectList) // switch to the correct range attack type
							{
								if (effect is SureShotEffect)
								{
									owner.RangeAttackType = eRangeAttackType.SureShot;
									break;
								}

								if(effect is RapidFireEffect)
								{
									owner.RangeAttackType = eRangeAttackType.RapidFire;
									break;
								}
							}
						}

						owner.RangeAttackState = eRangeAttackState.Aim;
						if(owner is GamePlayer)
						{
							owner.TempProperties.setProperty(GamePlayer.RANGE_ATTACK_HOLD_START, 0L);
						}

						int speed = owner.AttackSpeed(attackWeapon);
						byte attackSpeed = (byte)(speed/100);
						int model = (attackWeapon==null ? 0 : attackWeapon.Model);
						foreach(GamePlayer player in owner.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendCombatAnimation(owner,null,(ushort)model,0x00,0x01F4,attackSpeed,0x80,0x00);
						}

						if(owner.RangeAttackType == eRangeAttackType.RapidFire)
						{
							speed /=2; // can start fire at the middle of the normal time
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
			/// The number of swing witch must be done by the left weapon
			/// </summary>
			protected readonly int m_leftHandSwingCount;

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
			public WeaponOnTargetAction(GameLiving owner, GameObject target, InventoryItem attackWeapon, InventoryItem leftWeapon, int leftHandSwingCount, double effectiveness, int interruptDuration, Style combatStyle) : base(owner)
			{
				m_target = target;
				m_attackWeapon = attackWeapon;
				m_leftWeapon = leftWeapon;
				m_leftHandSwingCount = leftHandSwingCount;
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
				int leftHandSwingCount = m_leftHandSwingCount;
				AttackData mainHandAD = null;
				AttackData leftHandAD = null;
				InventoryItem mainWeapon = m_attackWeapon;
				InventoryItem leftWeapon = m_leftWeapon;

				if (!owner.CanUseLefthandedWeapon
					|| (mainWeapon != null && mainWeapon.Item_Type != Slot.RIGHTHAND && mainWeapon.Item_Type != Slot.LEFTHAND)
					|| leftWeapon == null
					|| leftWeapon.Object_Type == (int)eObjectType.Shield)
				{
					// no left hand used, all is simple here
					mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, m_effectiveness, m_interruptDuration, false);
					leftHandSwingCount = 0;
				}
				else if (leftHandSwingCount > 0)
				{
					// both hands are used for attack
					mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, m_effectiveness, m_interruptDuration, true);
					if (style == null)
						mainHandAD.AnimationId = 0x1F6; // both weapons swing animation
				}
				else
				{
					// one of two hands is used for attack if no style
					if (style == null && Util.Chance(50))
					{
						mainWeapon = leftWeapon;
						mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, m_effectiveness, m_interruptDuration, true);
						mainHandAD.AnimationId = 0x1F5;
					}
					else
					{
						mainHandAD = owner.MakeAttack(m_target, mainWeapon, style, m_effectiveness, m_interruptDuration, true);
					}
				}

				//Notify the target of our attack (sends damage messages, should be before damage)
				if(mainHandAD.Target != null)
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
						owner.StartWeaponMagicalEffect(mainHandAD, mainWeapon); // proc, poison
					}
				}

				owner.TempProperties.setProperty(LAST_ATTACK_DATA, mainHandAD);

				//Notify ourself about the attack
				owner.Notify(GameLivingEvent.AttackFinished, owner, new AttackFinishedEventArgs(mainHandAD));

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
							for (int i=0; i<leftHandSwingCount; i++)
							{
								if (m_target is GameLiving && ( ((GameLiving)m_target).Alive == false || ((GameLiving)m_target).ObjectState != eObjectState.Active))
									break;

								leftHandAD = owner.MakeAttack(m_target, leftWeapon, null, m_effectiveness, m_interruptDuration, true);

								//Notify the target of our attack (sends damage messages, should be before damage)
								if(leftHandAD.Target != null)
									leftHandAD.Target.OnAttackedByEnemy(leftHandAD);

								// deal damage and start the effect if any
								if (leftHandAD.AttackResult == eAttackResult.HitUnstyled || leftHandAD.AttackResult == eAttackResult.HitStyle)
								{
									owner.DealDamage(leftHandAD);
									if (leftHandAD.IsMeleeAttack)
									{
										owner.StartWeaponMagicalEffect(leftHandAD, leftWeapon);
									}
								}

								//Notify ourself about the attack
								owner.Notify(GameLivingEvent.AttackFinished, owner, new AttackFinishedEventArgs(leftHandAD));
							}
							break;
					}
				}

				switch(mainHandAD.AttackResult)
				{
					case eAttackResult.NoTarget:
					case eAttackResult.TargetDead:
					{
						if(owner.ActiveWeaponSlot != eActiveWeaponSlot.Distance) owner.StopAttack();
						Stop();
						return;
					}
					case eAttackResult.NotAllowed_ServerRules:
					case eAttackResult.NoValidTarget:
					{
						owner.StopAttack();
						Stop();
						return;
					}
				}

				// unstealth before attack animation
				if(owner is GamePlayer)
					((GamePlayer)owner).Stealth(false);

				//Show the animation
				if (mainHandAD.AttackResult != eAttackResult.HitUnstyled && mainHandAD.AttackResult != eAttackResult.HitStyle && leftHandAD != null)
					owner.ShowAttackAnimation(leftHandAD, leftWeapon);
				else
					owner.ShowAttackAnimation(mainHandAD, mainWeapon);

				// start style effect after any damage
				if (mainHandAD.StyleEffect != null && mainHandAD.AttackResult == eAttackResult.HitStyle)
				{
					mainHandAD.StyleEffect.StartSpell(mainHandAD.Target);
				}

				Stop();
				return;
			}
		}

		/// <summary>
		/// Starts the weapon proc if any
		/// </summary>
		/// <param name="ad"></param>
		/// <param name="weapon"></param>
		protected virtual void StartWeaponMagicalEffect(AttackData ad, InventoryItem weapon)
		{
			if (weapon != null)
			{
				if(weapon.ProcSpellID != 0)
				{
					// random chance (4.0spd = 10%)
					if (!Util.ChanceDouble(weapon.SPD_ABS*0.0025))
						return;

					SpellLine procEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
					if (procEffectLine != null)
					{
						IList spells = SkillBase.GetSpellList(procEffectLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spell in spells)
							{
								if (spell.ID == weapon.ProcSpellID)
								{
									if(spell.Level <= Level)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(ad.Attacker, spell, procEffectLine);
										if (spellHandler != null)
										{
											spellHandler.StartSpell(ad.Target);
										}
									}
									break;
								}
							}
						}
					}
				}

				// poison
				if(weapon.SpellID != 0)
				{
					SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
					if (poisonLine != null)
					{
						IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
						if (spells != null)
						{
							foreach (Spell spell in spells)
							{
								if (spell.ID == weapon.SpellID)
								{
									if(spell.Level <= Level)
									{
										ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, poisonLine);
										if (spellHandler != null)
										{
											spellHandler.StartSpell(ad.Target);
										}
									}
									break;
								}
							}
						}
					}
					weapon.Charges --;
					if(weapon.Charges <= 0) { weapon.MaxCharges = 0; weapon.SpellID = 0; }
				}
			}
		}

		/// <summary>
		/// Starts a melee attack on a target
		/// </summary>
		/// <param name="attackTarget">The object to attack</param>
		public virtual void StartAttack(GameObject attackTarget)
		{
			if (Mez || Stun ) return;
//			if (AttackState)
//				StopAttack(); // interrupts range attack animation

			if (!Alive || ObjectState!=eObjectState.Active) return;

			m_attackState = true;

			// cancel engage effect if exist
			EngageEffect effect = (EngageEffect)EffectList.GetOfType(typeof(EngageEffect));
			if(effect != null && effect.EngageSource == this)
				effect.Cancel(false);

			InventoryItem weapon = AttackWeapon;
			int speed = AttackSpeed(weapon);
			if (speed > 0)
			{
				if (m_attackAction == null)
					m_attackAction = CreateAttackAction();
				if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					RangeAttackState = eRangeAttackState.Aim;
					byte attackSpeed = (byte)(speed/100);
					int model = (weapon==null ? 0 : weapon.Model);
					foreach(GamePlayer p in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						p.Out.SendCombatAnimation(this,null,(ushort)model,0x00,0x01F4,attackSpeed,0x80,0x00);
					}

					// From : http://www.camelotherald.com/more/888.shtml
					// - When an Archer has this skill, at any time after halfway through their normal bow timer they can release the shot.
					if(RangeAttackType == eRangeAttackType.RapidFire)
					{
						speed /=2; // can start fire at the middle of the normal time
					}

					m_attackAction.Start(speed); //shooting a bow ALWAYS takes the speed time until ready!
				}
				else
				{
					if (m_attackAction.TimeUntilElapsed < 500) // wait at least 500ms before attack
						m_attackAction.Start(500);
				}
			}
		}

		/// <summary>
		/// Interrupts a Range Attack
		/// </summary>
		protected virtual void InterruptRangeAttack()
		{
			//Clear variables
			RangeAttackState=eRangeAttackState.None;
			RangeAttackType=eRangeAttackType.Normal;

			foreach(GamePlayer p in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				p.Out.SendInterruptAnimation(this);
		}

		/// <summary>
		/// Stops all attacks this gameliving is currently making
		/// </summary>
		public virtual void StopAttack()
		{
			// cancel engage effect if exist
			EngageEffect effect = (EngageEffect)EffectList.GetOfType(typeof(EngageEffect));
			if(effect != null && effect.EngageSource == this)
				effect.Cancel(false);

			m_attackState = false;
			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				InterruptRangeAttack();

//			log.Debug(this.Name+": StopAttack()");
		}


		/// <summary>
		/// Calculates melee critical damage of this player
		/// </summary>
		/// <param name="ad">The attack data</param>
		/// <param name="weapon">The weapon used</param>
		/// <returns>The amount of critical damage</returns>
		public virtual int CalculateCriticalDamage(AttackData ad, InventoryItem weapon)
		{
			if(Util.Chance(AttackCriticalChance(weapon)))
			{
				int critMax;
				// Critical damage to players is 50%, low limit should be around 20% but not sure
				// zerkers in Berserk do up to 99%
				if(ad.Target is GamePlayer)
					critMax = ad.Damage>>1;
				else
					critMax = ad.Damage;

				//think min crit dmage is 10% of damage
				return Util.Random(ad.Damage/10, critMax);
			}
			return 0;
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
			//1.To-Hit modifiers on styles do not any effect on whether your opponent successfully Evades, Blocks, or Parries. – Grab Bag 2/27/03
			//2.The correct Order of Resolution in combat is Intercept, Evade, Parry, Block (Shield), Guard, Hit/Miss, and then Bladeturn. – Grab Bag 2/27/03, Grab Bag 4/4/03
			//3.For every person attacking a monster, a small bonus is applied to each player's chance to hit the enemy. Allowances are made for those who don't technically hit things when they are participating in the raid – for example, a healer gets credit for attacking a monster when he heals someone who is attacking the monster, because that's what he does in a battle. – Grab Bag 6/6/03
			//4.Block, parry, and bolt attacks are affected by this code, as you know. We made a fix to how the code counts people as "in combat." Before this patch, everyone grouped and on the raid was counted as "in combat." The guy AFK getting Mountain Dew was in combat, the level five guy hovering in the back and hoovering up some exp was in combat – if they were grouped with SOMEONE fighting, they were in combat. This was a bad thing for block, parry, and bolt users, and so we fixed it. – Grab Bag 6/6/03
			//5.Positional degrees - Side Positional combat styles now will work an extra 15 degrees towards the rear of an opponent, and rear position styles work in a 60 degree arc rather than the original 90 degree standard. This change should even out the difficulty between side and rear positional combat styles, which have the same damage bonus. Please note that front positional styles are not affected by this change. – 1.62
			//http://daoc.catacombs.com/forum.cfm?ThreadKey=511&DefMessage=681444&forum=DAOCMainForum#Defense

			GuardEffect guard = null;
			InterceptEffect intercept = null;
			GameSpellEffect bladeturn = null;
			EngageEffect engage = null;
			AttackData lastAD = (AttackData)TempProperties.getObjectProperty(LAST_ATTACK_DATA, null);
			bool defenceDisabled = ad.Target.Mez | ad.Target.Stun | ad.Target.Sitting;

			// If berserk is on, no defensive skills may be used: evade, parry, ...
			// unfortunately this as to be check for every action itself to kepp oder of actions the same.
			// Intercept and guard can still be used on berserked
//			BerserkEffect berserk = null;

			// get all needed effects in one loop
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (guard == null && effect is GuardEffect && ((GuardEffect)effect).GuardTarget == this) guard = (GuardEffect)effect;
					if (bladeturn == null && effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.SpellType == "Bladeturn") bladeturn = (GameSpellEffect)effect;
					if (effect is BerserkEffect) defenceDisabled = true;
					if (engage == null && effect is EngageEffect) engage = (EngageEffect)effect;
					if (intercept != null) continue; // already found

					// We check if interceptor can intercept

					// we can only intercept attacks on livings, and can only intercept when active
					// you cannot intercept while you are sitting
					// if you are stuned or mesmeried you cannot intercept...
					InterceptEffect inter = effect as InterceptEffect;
					if (inter == null) continue;
					if (inter.InterceptTarget != this) continue;
					if (inter.InterceptSource.Stun) continue;
					if (inter.InterceptSource.Mez) continue;
					if (inter.InterceptSource.Sitting) continue;
					if (inter.InterceptSource.ObjectState != eObjectState.Active) continue;
					if (inter.InterceptSource.Alive == false) continue;
					if (!WorldMgr.CheckDistance(this, inter.InterceptSource,InterceptAbilityHandler.INTERCEPT_DISTANCE)) continue;
					if (Util.Chance(50)) continue; // TODO: proper chance formula?
					intercept = inter;
				}
			}

			bool stealthStyle = false;
			if (ad.Style != null && ad.Style.StealthRequirement && ad.Attacker is GamePlayer && StyleProcessor.CanUseStyle((GamePlayer)ad.Attacker, ad.Style, weapon))
			{
				stealthStyle = true;
				defenceDisabled = true;
			}

			// Intercept
			if (intercept != null && !stealthStyle)
			{
				ad.Target = intercept.InterceptSource;
				intercept.Cancel(false); // can be canceled only outside of the loop
				return eAttackResult.HitUnstyled;
			}

			// i am defender, what con is attacker to me?
			// orange+ should make it harder to block/evade/parry
			double attackerConLevel = -GetConLevel(ad.Attacker);
//			double levelModifier = -((ad.Attacker.Level - Level) / (Level / 10.0 + 1));


			if (!defenceDisabled)
			{
				// Evade
				// 1. A: It isn't possible to give a simple answer. The formula includes such elements as your level, your target's level, your level of evade, your QUI, your DEX, your buffs to QUI and DEX, the number of people attacking you, your target's weapon level, your target's spec in the weapon he is wielding, the kind of attack (DW, range, etc), attack radius, angle of attack, the style you used most recently, target's offensive RA, debuffs, and a few others. (The type of weapon - large, 1H, etc - doesn't matter.) ...."
				double evadeChance = double.MinValue;
				GamePlayer player = this as GamePlayer;
				if (player != null)
				{
					if ((player.HasAbility(Abilities.Evade) && IsObjectInFront(ad.Attacker, 180)) || player.HasAbility(Abilities.Advanced_Evade))
						evadeChance = GetModified(eProperty.EvadeChance);
				}
				else if (IsObjectInFront(ad.Attacker, 180))
				{
					int res = GetModified(eProperty.EvadeChance);
					if (res != 0)
						evadeChance = res;
				}
				if (evadeChance != double.MinValue)
				{
					evadeChance *= 0.001;
					evadeChance += 0.01 * attackerConLevel; // 1% per con level distance multiplied by evade level
					if (lastAD != null && lastAD.Style != null)
					{
						evadeChance += lastAD.Style.BonusToDefense*0.01;
					}
					if (m_attackers.Count > 1) evadeChance -= (m_attackers.Count-1) * 0.03;
					if (evadeChance < 0.01) evadeChance = 0.01;
					else if (evadeChance > 0.5 && ad.Attacker is GamePlayer && ad.Target is GamePlayer)
						evadeChance = 0.5; //50% evade cap RvR only; http://www.camelotherald.com/more/664.shtml
					else if (evadeChance > 1.0) evadeChance = 1.0;

					if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) evadeChance /= 2.0;
					if (ad.AttackType == AttackData.eAttackType.Ranged) evadeChance /= 5.0;

					if (Util.ChanceDouble(evadeChance))
					{
						return eAttackResult.Evaded;
					}
				}

				// Parry
				//1.  Dual wielding does not grant more chances to parry than a single weapon. – Grab Bag 9/12/03
				//2.  There is no hard cap on ability to Parry. – Grab Bag 8/13/02
				//3.  Your chances of doing so are best when you are solo, trying to block or parry a style from someone who is also solo. The chances of doing so decrease with grouped, simultaneous attackers. – Grab Bag 7/19/02
				//4.  The parry chance is divided up amongst the attackers, such that if you had a 50% chance to parry normally, and were under attack by two targets, you would get a 25% chance to parry one, and a 25% chance to parry the other. So, the more people or monsters attacking you, the lower your chances to parry any one attacker. -  – Grab Bag 11/05/04
				//Your chance to parry is affected by the number of attackers, the size of the weapon you’re using, and your spec in parry.
				//Parry % = (5% + 0.5% * Parry) / # of Attackers
				//Parry: (((Dex*2)-100)/40)+(Parry/2)+(Mastery of P*3)+5. < Possible relation to buffs
				//So, if you have parry of 20 you will have a chance of parrying 15% if there is one attacker. If you have parry of 20 you will have a chance of parrying 7.5%, if there are two attackers.
				//From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
				//So, when facing a 2H weapon, you may see a penalty to your evade.
				//
				//http://www.camelotherald.com/more/453.php
				//Also, before this comparison happens, the game looks to see if your opponent is in your forward arc – to determine that arc, make a 120 degree angle, and put yourself at the point.
				if (ad.IsMeleeAttack)
				{
					double parryChance = double.MinValue;
					if (player != null)
					{
						if (player.HasSpecialization(Specs.Parry) && IsObjectInFront(ad.Attacker, 120) && AttackWeapon!=null)
						{
							parryChance = GetModified(eProperty.ParryChance);
						}
					}
					else if (IsObjectInFront(ad.Attacker, 120))
					{
						int res = GetModified(eProperty.ParryChance);
						if (res != 0)
							parryChance = res;
					}
					if (parryChance != double.MinValue)
					{
						parryChance *= 0.001;
						parryChance += 0.05 * attackerConLevel;
						if (parryChance < 0.01) parryChance = 0.01;
						if (parryChance > 0.99) parryChance = 0.99;

						if (m_attackers.Count > 1) parryChance /= m_attackers.Count;
						if (weapon != null && weapon.Hand == 1) parryChance /= 2;

						if (Util.ChanceDouble(parryChance))
						{
							return eAttackResult.Parried;
						}
					}
				}

				// Block
				//1.Quality does not affect the chance to block at this time. – Grab Bag 3/7/03
				//2.Condition and enchantment increases the chance to block – Grab Bag 2/27/03
				//3.There is currently no hard cap on chance to block – Grab Bag 2/27/03 and 8/16/02
				//4.Dual Wielders (enemy) decrease the chance to block – Grab Bag 10/18/02
				//5.Block formula: Shield = base 5% + .5% per spec point. Then modified by dex (.1% per point of dex above 60 and below 300?). Further modified by condition, bonus and shield level
				//8.The shield’s size only makes a difference when multiple things are attacking you – a small shield can block one attacker, a medium shield can block two at once, and a large shield can block three. – Grab Bag 4/4/03
				//Your chance to block is affected by the number of attackers, the size of the shield you’re using, and your spec in block.
				//Shield% = (5% + 0.5% * Shield)
				//Small Shield = 1 attacker
				//Medium Shield = 2 attacker
				//Large Shield = 3 attacker
				//Each attacker above these numbers will reduce your chance to block.
				//From Grab Bag: "Dual wielders throw an extra wrinkle in. You have half the chance of shield blocking a dual wielder as you do a player using only one weapon. Your chance to parry is halved if you are facing a two handed weapon, as opposed to a one handed weapon."
				//Block: (((Dex*2)-100)/40)+(Shield/2)+(Mastery of B*3)+5. < Possible relation to buffs
				//
				//http://www.camelotherald.com/more/453.php
				//Also, before this comparison happens, the game looks to see if your opponent is in your forward arc – to determine that arc, make a 120 degree angle, and put yourself at the point.
				//your friend is most likely using a player crafted shield. The quality of the player crafted item will make a significant difference – try it and see.
				double blockChance = double.MinValue;
				if (player != null)
				{
					InventoryItem lefthand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					if (lefthand!=null && (player.AttackWeapon==null || player.AttackWeapon.Item_Type==Slot.RIGHTHAND || player.AttackWeapon.Item_Type==Slot.LEFTHAND))
					{
						if (lefthand.Object_Type == (int)eObjectType.Shield && IsObjectInFront(ad.Attacker, 120))
						{
							blockChance = GetModified(eProperty.BlockChance) * lefthand.Quality * 0.01;
						}
					}
				}
				else if (IsObjectInFront(ad.Attacker, 120))
				{
					int res = GetModified(eProperty.BlockChance);
					if (res != 0)
						blockChance = res;
				}
				if (blockChance != double.MinValue)
				{
					blockChance *= 0.001;
					// no chance bonus with ranged attacks?
//					if (ad.Attacker.ActiveWeaponSlot == GameLiving.eActiveWeaponSlot.Distance)
//						blockChance += 0.25;
					blockChance += attackerConLevel * 0.05;
					if (blockChance > 0.99) blockChance = 0.99;
					if (blockChance < 0.01) blockChance = 0.01;

					// Reduce block chance if the shield used is too small (valable only for player because npc inventory does not store the shield size but only the model of item)
					if(player != null && Inventory.GetItem(eInventorySlot.LeftHandWeapon).Type_Damage - m_attackers.Count < 0) blockChance /= m_attackers.Count;
					if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) blockChance /= 2;

					// Engage raised block change to 85% if attacker is engageTarget and player is in attackstate
					if (engage!=null && AttackState && engage.EngageTarget==ad.Attacker )
					{
						// You cannot engage a mob that was attacked within the last X seconds...
						if (engage.EngageTarget.LastAttackedByEnemyTick > engage.EngageTarget.CurrentRegion.Time - EngageAbilityHandler.ENGAGE_ATTACK_DELAY_TICK)
						{
							engage.EngageSource.Out.SendMessage(engage.EngageTarget.GetName(0,true)+" has been attacked recently and you are unable to engage.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						// Check if player has enough endurance left to engage
						else if (engage.EngageSource.Endurance>=EngageAbilityHandler.ENGAGE_DURATION_LOST)
						{
							engage.EngageSource.Endurance -= EngageAbilityHandler.ENGAGE_DURATION_LOST;
							engage.EngageSource.Out.SendMessage("You concentrate on blocking the blow!", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

							if (blockChance < 0.85)
								blockChance = 0.85;
						}
						// if player ran out of endurance cancel engage effect
						else
						{
							engage.Cancel(false);
						}
					}

					if (Util.ChanceDouble(blockChance))
					{
						return eAttackResult.Blocked;
					}
				}
			}


			// Guard
			if (guard != null &&
				guard.GuardSource.ObjectState == eObjectState.Active &&
				guard.GuardSource.Stun == false &&
				guard.GuardSource.Mez == false &&
				guard.GuardSource.ActiveWeaponSlot != eActiveWeaponSlot.Distance &&
//				guard.GuardSource.AttackState &&
				guard.GuardSource.Alive &&
				!stealthStyle)
			{
				// check distance
				if (WorldMgr.CheckDistance(guard.GuardSource, guard.GuardTarget, GuardAbilityHandler.GUARD_DISTANCE))
				{
					// check player is wearing shield and NO two handed weapon
					InventoryItem leftHand  = guard.GuardSource.Inventory.GetItem(eInventorySlot.LeftHandWeapon);
					InventoryItem rightHand = guard.GuardSource.AttackWeapon;
					if ((rightHand == null || rightHand.Hand != 1) && leftHand != null && leftHand.Object_Type == (int)eObjectType.Shield)
					{
						// TODO
						// insert actual formula for guarding here, this is just a guessed one based on block.
						int    guardLevel  = guard.GuardSource.GetAbilityLevel(Abilities.Guard); // multiply by 3 to be a bit qorse than block (block woudl be 5 since you get guard I with shield 5, guard II with shield 10 and guard III with shield 15)
						double guardchance = guard.GuardSource.GetModified(eProperty.BlockChance) * leftHand.Quality * 0.00001;
						guardchance *= guardLevel*0.25 + 0.05;
						guardchance += attackerConLevel * 0.05;

						if (guardchance > 0.99) guardchance = 0.99;
						if (guardchance < 0.01) guardchance = 0.01;

						if(leftHand.Type_Damage - m_attackers.Count < 0) guardchance /= m_attackers.Count;
						if (ad.AttackType == AttackData.eAttackType.MeleeDualWield) guardchance /= 2;

						if (Util.ChanceDouble(guardchance))
						{
							ad.Target = guard.GuardSource;
							return eAttackResult.Blocked;
						}
					}
				}
			}


			// Missrate
			int missrate = (ad.Attacker is GamePlayer) ? 20 : 25; //player vs player tests show 20% miss on any level
			if (this is GameNPC || ad.Attacker is GameNPC) // if target is not player use level mod
			{
				missrate += (int)(5 * ad.Attacker.GetConLevel(this));
			}
			// weapon/armor bonus
			int armorBonus = 0;
			if(ad.Target is GamePlayer)
			{
				ad.ArmorHitLocation = ((GamePlayer)ad.Target).CalculateArmorHitLocation();
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
			if(ad.Attacker.ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				ItemTemplate ammo = RangeAttackAmmo;
				if(ammo!=null)
					switch((ammo.SPD_ABS>>4)&0x3)
					{
						// http://rothwellhome.org/guides/archery.htm
						case 0: missrate += 15; break; // Rough
//						case 1: missrate -= 0; break;
						case 2: missrate -= 15; break; // doesn't exist (?)
						case 3: missrate -= 25; break; // Footed
					}
			}
			if (this is GamePlayer && ((GamePlayer)this).Sitting)
			{
				missrate >>= 1; //halved
			}
			if (Util.Chance(missrate))
			{
				//DOLConsole.WriteLine("Random Miss...");
				return eAttackResult.Missed;
			}


			// Fumble
			if (ad.IsMeleeAttack)
			{
				double fumbleChance = ad.Attacker.GetModified(eProperty.FumbleChance);
				fumbleChance *= 0.001;

				if (fumbleChance > 0.99) fumbleChance = 0.99;
				if (fumbleChance < 0) fumbleChance = 0;

				if (Util.ChanceDouble(fumbleChance))
				{
					return eAttackResult.Fumbled;
				}
			}


			// Bladeturn
			// TODO: high level mob attackers penetrate bt, players are tested and do not penetrate (lv30 vs lv20)
			// "The blow penetrated the magical barrier!"
			if(bladeturn != null)
			{
				if (stealthStyle // stealth styles pierce bladeturn
				|| (ad.AttackType == AttackData.eAttackType.Ranged && ad.Target != bladeturn.SpellHandler.Caster && ad.Attacker is GamePlayer && ((GamePlayer)ad.Attacker).HasAbility(Abilities.PenetratingArrow)))  // penetrating arrow attack pierce bladeturn
				{
					if (ad.Target is GamePlayer) ((GamePlayer)ad.Target).Out.SendMessage("The blow penetrated the magical barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
				}
				else
				{
					if (this is GamePlayer) ((GamePlayer)this).Out.SendMessage("The blow was absorbed by a magical barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					if (ad.Attacker is GamePlayer) ((GamePlayer)ad.Attacker).Out.SendMessage("Your strike was absorbed by a magical barrier!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					bladeturn.Cancel(false);
					return eAttackResult.Missed;
				}
			}

			return eAttackResult.HitUnstyled;
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

			if (source is GameNPC)
			{
				IControlledBrain brain = ((GameNPC)source).Brain as IControlledBrain;
				if (brain != null)
					source = brain.Owner;
				damageDealt *= 0.75;
			}

			GamePlayer attackerPlayer = source as GamePlayer;
			if (attackerPlayer != null && attackerPlayer != this)
			{
				PlayerGroup attackerGroup = attackerPlayer.PlayerGroup;
				if (attackerGroup != null)
				{
					ArrayList xpGainers = new ArrayList(8);
					lock (attackerGroup)
					{
						// collect "helping" group players in range
						for (int i = 0; i < attackerGroup.PlayerCount; i++)
						{
							GamePlayer player = attackerGroup[i];
							if (WorldMgr.CheckDistance(player, this, WorldMgr.MAX_EXPFORKILL_DISTANCE) && player.Alive && player.ObjectState == eObjectState.Active)
								xpGainers.Add(player);
						}
					}
					// add players in range for exp to exp gainers
					for (int i = 0; i < xpGainers.Count; i++)
					{
						this.AddXPGainer((GamePlayer)xpGainers[i], (float)(damageDealt/xpGainers.Count));
					}
				}
				else
				{
					this.AddXPGainer(source, (float) damageDealt);
				}
				//DealDamage needs to be called after addxpgainer!
			}
			else if(source != null)
			{
				AddXPGainer(source, (float) damageAmount + criticalAmount);
			}

			bool oldAlive = Alive;
			Health-=damageAmount+criticalAmount;
			if(!Alive)
			{
				if (oldAlive) // check if living was already dead
				{
					Die(source);
				}
				lock(m_xpGainers.SyncRoot)
				{
					m_xpGainers.Clear();
				}
			}
		}

		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		public virtual void OnAttackedByEnemy(AttackData ad)
		{
			Notify(GameLivingEvent.AttackedByEnemy, this, new AttackedByEnemyEventArgs(ad) );
			AddAttacker(ad.Attacker);
			switch(ad.AttackResult)
			{
			case eAttackResult.Blocked:
			case eAttackResult.Evaded:
			case eAttackResult.HitUnstyled:
			case eAttackResult.HitStyle:
			case eAttackResult.Missed:
			case eAttackResult.Parried:
			case eAttackResult.Fumbled:
				LastAttackedByEnemyTick = CurrentRegion.Time;
				ad.Attacker.LastAttackTick = CurrentRegion.Time;
				break;
			}
		}

		/// <summary>
		/// Called to display an attack animation of this living
		/// </summary>
		/// <param name="ad">Infos about the attack</param>
		/// <param name="weapon">The weapon used for attack</param>
		public virtual void ShowAttackAnimation(AttackData ad, InventoryItem weapon)
		{
			bool showAnim=false;
			switch(ad.AttackResult)
			{
				case eAttackResult.HitUnstyled:
				case eAttackResult.HitStyle:
				case eAttackResult.Evaded:
				case eAttackResult.Parried:
				case eAttackResult.Missed:
				case eAttackResult.Blocked:
				case eAttackResult.Fumbled:
					showAnim=true;break;
			}

			if(showAnim && ad.Target!=null)
			{
				//http://dolserver.sourceforge.net/forum/showthread.php?s=&threadid=836
				byte resultByte = 0;
				int attackersWeapon = (weapon==null) ? 0 : weapon.Model;
				int defendersWeapon = 0;

				switch (ad.AttackResult)
				{
					case eAttackResult.Missed      : resultByte =  0; break;
					case eAttackResult.Evaded      : resultByte =  3; break;
					case eAttackResult.Fumbled     : resultByte =  4; break;
					case eAttackResult.HitUnstyled : resultByte = 10; break;
					case eAttackResult.HitStyle    : resultByte = 11; break;

					case eAttackResult.Parried:
						resultByte =  1;
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
							if (lefthand!=null && lefthand.Object_Type==(int)eObjectType.Shield)
							{
								defendersWeapon = lefthand.Model;
							}
						}
						break;
				}

				foreach(GamePlayer player in ad.Target.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendCombatAnimation(this,ad.Target,(ushort)attackersWeapon,(ushort)defendersWeapon,ad.AnimationId,0,resultByte,ad.Target.HealthPercent);
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
			//TODO fire event that we are dealing damage

			ad.Target.TakeDamage(this,ad.DamageType,ad.Damage,ad.CriticalDamage);
		}

		/// <summary>
		/// Adds a object to the list of objects that will gain xp
		/// after this living dies
		/// </summary>
		/// <param name="xpGainer">the xp gaining object</param>
		/// <param name="damageAmount">the amount of damage, float because for groups it can be split</param>
		public virtual void AddXPGainer(GameObject xpGainer, float damageAmount)
		{
			//DOLConsole.WriteLine(this.Name+":AddXPGainer("+xpGainer+","+damageAmount+")");
			lock(m_xpGainers.SyncRoot)
			{
				if(m_xpGainers[xpGainer]==null)
				{
					//DOLConsole.WriteLine(this.Name+":create - m_xpGainers[xpGainer]=0");
					m_xpGainers[xpGainer]=(float)0;
				}
				//DOLConsole.WriteLine(this.Name+":alt - m_xpGainers[xpGainer]="+m_xpGainers[xpGainer]);
				m_xpGainers[xpGainer]=(float)m_xpGainers[xpGainer]+damageAmount;
				//DOLConsole.WriteLine(this.Name+":neu - m_xpGainers[xpGainer]="+m_xpGainers[xpGainer]);
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
			Health+=changeAmount;
			int healthChanged = Health - oldHealth;

			//Nofiy our enemies that we were healed by other means than
			//natural regeneration, this allows for aggro on healers!
			if(healthChangeType != eHealthChangeType.Regenerate)
			{
				IList attackers;
				lock (m_attackers.SyncRoot) { attackers = (IList)m_attackers.Clone(); }

				foreach(GameObject attacker in attackers)
				{
					if(attacker is GameLiving && attacker != TargetObject)
						((GameLiving)attacker).Notify(
							GameLivingEvent.EnemyHealed,
							(GameLiving)attacker,
							new EnemyHealedEventArgs(this, changeSource, healthChangeType, changeAmount));
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
			Mana+=changeAmount;
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
			Endurance+=changeAmount;
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
			//TODO fire event
		}

		/// <summary>
		/// Returns the list of attackers
		/// </summary>
		public virtual IList Attackers
		{
			get{ return m_attackers; }
		}
		/// <summary>
		/// Adds an attacker to the attackerlist
		/// </summary>
		/// <param name="attacker">the attacker to add</param>
		public virtual void AddAttacker(GameObject attacker)
		{
			lock(m_attackers.SyncRoot)
			{
				if (attacker == this) return;
				if(m_attackers.Contains(attacker)) return;
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
			lock(m_attackers.SyncRoot)
			{
				//TODO fire event, that an attacker was removed
				m_attackers.Remove(attacker);
			}
		}
		/// <summary>
		/// Called when this living dies
		/// </summary>
		public virtual void Die(GameObject killer)
		{
//			log.Debug(this.Name+": Die()");

			Notify(GameLivingEvent.Dying, this, new DyingEventArgs(killer));

			//Stop attacks
			StopAttack();

			ArrayList temp;

			//Send our attackers some note
			lock(m_attackers.SyncRoot)
			{
				temp = (ArrayList) m_attackers.Clone();
				m_attackers.Clear();
			}

			foreach(GameObject obj in temp)
				if(obj is GameLiving)
					((GameLiving)obj).EnemyKilled(this);

			// cancel all concentration effects
			ConcentrationEffects.CancelAll();

			// cancel all left effects
			EffectList.CancelAll();

			StopHealthRegeneration();
			StopPowerRegeneration();
			StopEnduranceRegeneration();

			//Reduce health to zero
			Health=0;
		}

		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="expBase">base amount of xp to gain</param>
		/// <param name="expCampBonus">camp bonus to base exp</param>
		/// <param name="expGroupBonus">group bonus to base exp</param>
		/// <param name="sendMessage">should exp gain message be sent</param>
		public virtual void GainExperience(long expBase, long expCampBonus, long expGroupBonus, bool sendMessage)
		{
			if(expBase > 0) Notify(GameLivingEvent.GainedExperience, this, new GainedExperienceEventArgs(expBase, expCampBonus, expGroupBonus, sendMessage));
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
		}
		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="exp">base amount of xp to gain</param>
		public void GainExperience(long exp)
		{
			GainExperience(exp, 0, 0, true);
		}
		/// <summary>
		/// Called when an enemy of this living is killed
		/// </summary>
		/// <param name="enemy">enemy killed</param>
		public virtual void EnemyKilled(GameLiving enemy)
		{
//			log.Debug(Name + ": EnemyKilled " + enemy.Name);
			RemoveAttacker(enemy);
			Notify(GameLivingEvent.EnemyKilled,this,new EnemyKilledEventArgs(enemy));
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
		/// Holds visible active weapon slots
		/// </summary>
		protected byte m_visibleActiveWeaponSlots = 0xFF; // none by default

		/// <summary>
		/// Gets visible active weapon slots
		/// </summary>
		public byte VisibleActiveWeaponSlots
		{
			get { return m_visibleActiveWeaponSlots; }
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
		/// Switches the active weapon to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public virtual void SwitchWeapon(eActiveWeaponSlot slot)
		{
			//Clean up range attack variables, no matter to what
			//weapon we switch
			RangeAttackState=eRangeAttackState.None;
			RangeAttackType=eRangeAttackType.Normal;

			InventoryItem rightHandSlot = Inventory.GetItem(eInventorySlot.RightHandWeapon);
			InventoryItem leftHandSlot  = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			InventoryItem twoHandSlot   = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			InventoryItem distanceSlot  = Inventory.GetItem(eInventorySlot.DistanceWeapon);

			// simple active slot logic:
			// 0=right hand, 1=left hand, 2=two-hand, 3=range, F=none
			int rightHand = (VisibleActiveWeaponSlots & 0x0F);
			int leftHand  = (VisibleActiveWeaponSlots & 0xF0) >> 4;


			// set new active weapon slot
			switch(slot)
			{
				case eActiveWeaponSlot.Standard:
					if (rightHandSlot == null)
						rightHand = 0xFF;
					else rightHand = 0x00;

					if (leftHandSlot == null)
						leftHand = 0xFF;
					else leftHand = 0x01;
					break;

				case eActiveWeaponSlot.TwoHanded:
					if (twoHandSlot != null && twoHandSlot.Hand == 1) // 2h
					{
						rightHand = leftHand = 0x02;
						break;
					}

					// 1h weapon in 2h slot
					if (twoHandSlot == null)
						rightHand = 0xFF;
					else rightHand = 0x02;

					if (leftHandSlot == null)
						leftHand = 0xFF;
					else leftHand = 0x01;
					break;

				case eActiveWeaponSlot.Distance:
					leftHand = 0xFF; // cannot use left-handed weapons if ranged slot active
					if (distanceSlot == null)
						rightHand = 0xFF;
					else if (distanceSlot.Hand == 1) // bows use 2 hands, trowing axes 1h
						rightHand = leftHand = 0x03;
					else rightHand = 0x03;
					break;
			}

			m_activeWeaponSlot = slot;

			// pack active weapon slots value back
			m_visibleActiveWeaponSlots = (byte)(((leftHand & 0x0F) << 4) | (rightHand & 0x0F));
		}
		#endregion
		#region Property/Bonus/Buff/PropertyCalculator fields

		/// <summary>
		/// Array for property boni by items
		/// </summary>
		protected IPropertyIndexer m_itemBonus = new PropertyIndexer();
		/// <summary>
		/// Property Item Bonus field
		/// </summary>
		public IPropertyIndexer ItemBonus {
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
		public IPropertyIndexer BuffBonusCategory1 {
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
		public IPropertyIndexer BuffBonusCategory2 {
			get { return m_buff2Bonus; }
		}

		/// <summary>
		/// Array for third buff boni
		/// </summary>
		protected IPropertyIndexer m_buff3Bonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer BuffBonusCategory3 {
			get { return m_buff3Bonus; }
		}

		/// <summary>
		/// Array for forth buff boni
		/// </summary>
		protected IPropertyIndexer m_buff4Bonus = new PropertyIndexer();
		/// <summary>
		/// Property Buff bonus category
		/// what it means depends from the PropertyCalculator for a property element
		/// </summary>
		public IPropertyIndexer BuffBonusCategory4 {
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
		public IMultiplicativeProperties BuffBonusMultCategory1 {
			get { return m_buffMult1Bonus; }
		}

		/// <summary>
		/// property calculators for each property
		/// look at PropertyCalculator class for more description
		/// </summary>
		internal static readonly IPropertyCalculator[] m_propertyCalc = new IPropertyCalculator[(int)eProperty.MaxProperty];

		/// <summary>
		/// retrieve a property value of that living
		/// this value is modified/capped and ready to use
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public virtual int GetModified(eProperty property)
		{
			if (m_propertyCalc!=null && m_propertyCalc[(int)property]!=null)
			{
				return m_propertyCalc[(int)property].CalcValue(this, property);
			}
			else
			{
				if (log.IsErrorEnabled)
					log.Error("No calculator for requested Property found: "+property+"\n"+Environment.StackTrace);
			}
			return 0;
		}

		/// <summary>
		/// Old temp properties
		/// </summary>
		//protected int[] m_oldTempProps = new int[(int)eProperty.MaxProperty];

		/// <summary>
		/// New temp properties
		/// </summary>
		//protected int[] m_newTempProps = new int[(int)eProperty.MaxProperty];

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
		public int GetBaseStat(eStat stat)
		{
			return m_charStat[stat-eStat._First];
		}
		/// <summary>
		/// changes a base stat value
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="amount"></param>
		public virtual void ChangeBaseStat(eStat stat, short amount)
		{
			m_charStat[stat-eStat._First] += amount;
		}

		/// <summary>
		/// this field is just for convinience and speed purposes
		/// converts the damage types to resist fields
		/// </summary>
		protected static readonly eProperty[] m_damageTypeToResistBonusConversion = new eProperty[] {
			0,
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
			if ((int)damageType < m_damageTypeToResistBonusConversion.Length) {
				return m_damageTypeToResistBonusConversion[(int)damageType];
			} else {
				return 0;
			}
		}
		/// <summary>
		/// gets the resistance value by damage types
		/// </summary>
		/// <param name="damageType"></param>
		/// <returns></returns>
		public virtual int GetResist(eDamageType damageType)
		{
			return GetModified(GetResistTypeForDamage(damageType));
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
			get { return m_tempProps;	}
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
		protected RegionTimer	m_healthRegenerationTimer;
		/// <summary>
		/// GameTimer used for restoring mana
		/// </summary>
		protected RegionTimer	m_powerRegenerationTimer;
		/// <summary>
		/// GameTimer used for restoring endurance
		/// </summary>
		protected RegionTimer	m_enduRegenerationTimer;
		/// <summary>
		/// The default frequency of regenerating health in milliseconds
		/// </summary>
		protected int m_healthRegenerationPeriod;
		/// <summary>
		/// The default frequency of regenerating power in milliseconds
		/// </summary>
		protected int m_powerRegenerationPeriod;
		/// <summary>
		/// The default frequency of regenerating endurance in milliseconds
		/// </summary>
		protected int m_enduRegenerationPeriod;
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
					return;
				m_healthRegenerationTimer.Start(m_healthRegenerationPeriod);
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
				else if (m_powerRegenerationTimer.IsAlive) return;
				m_powerRegenerationTimer.Start(m_powerRegenerationPeriod);
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
				else if (m_enduRegenerationTimer.IsAlive) return;
				m_enduRegenerationTimer.Start(m_enduRegenerationPeriod);
			}
		}
		/// <summary>
		/// Stop the health regeneration
		/// </summary>
		public virtual void StopHealthRegeneration() {
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
		public virtual void StopPowerRegeneration() {
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
		public virtual void StopEnduranceRegeneration() {
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
			if(Health < MaxHealth)
			{
				ChangeHealth(this, eHealthChangeType.Regenerate, GetModified(eProperty.HealthRegenerationRate));
			}

			//If we are fully healed, we stop the timer
			if(Health>=MaxHealth) {
				return 0;
			}

			int variance = m_healthRegenerationPeriod/1000;
			int periodVariance = Util.Random(-variance, variance);

			//If we were hit before we regenerated, we regenerate slower the next time
			if (InCombat)
			{
				return m_healthRegenerationPeriod * 2 + periodVariance;
			}

			//Sitting livings heal faster
			if(Sitting) {
				return m_healthRegenerationPeriod / 2 + periodVariance;
			}

			//Heal at standard rate
			return m_healthRegenerationPeriod + periodVariance;
		}
		/// <summary>
		/// Callback for the power regenerationTimer
		/// </summary>
		/// <param name="selfRegenerationTimer">timer calling this function</param>
		protected virtual int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if (Mana < MaxMana)
			{
				ChangeMana(this, eManaChangeType.Regenerate, GetModified(eProperty.PowerRegenerationRate));
			}

			//If we are full, we stop the timer
			if (Mana>=MaxMana) {
				return 0;
			}

			int variance = m_powerRegenerationPeriod/1000;
			int periodVariance = Util.Random(-variance, variance);

			//If we were hit before we regenerated, we regenerate slower the next time
			if (InCombat)
			{
				return m_powerRegenerationPeriod * 2 + periodVariance;
			}

			//Sitting livings regen faster
			if(Sitting) {
				return m_powerRegenerationPeriod / 2 + periodVariance;
			}

			//regen at standard rate
			return m_powerRegenerationPeriod + periodVariance;
		}
		/// <summary>
		/// Callback for the endurance regenerationTimer
		/// </summary>
		/// <param name="selfRegenerationTimer">timer calling this function</param>
		protected virtual int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if (Endurance<MaxEndurance)
			{
				int regen = GetModified(eProperty.EnduranceRegenerationRate);
				if (regen > 0)
				{
					ChangeEndurance(this, eEnduranceChangeType.Regenerate, regen);
				}
			}
			if (Endurance >= MaxEndurance) return 0;

			return 500 + Util.Random(1000); // 1000ms +-500ms;
		}
		#endregion
		#region Mana/Health/Endurance/Concentration/Delete
		/// <summary>
		/// Health of the object
		/// </summary>
		protected int m_health;
		/// <summary>
		/// Maxiumum value that can be in m_health
		/// </summary>
		//protected int m_maxHealth;
		/// <summary>
		/// Amount of mana
		/// </summary>
		protected int m_mana;
		/// <summary>
		/// Maximum value that can be in m_mana
		/// </summary>
		//protected int m_maxMana;
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
		public virtual int Health
		{
			get { return m_health; }
			set
			{

				int maxhealth = MaxHealth;
				if(value >= maxhealth)
				{
					m_health = maxhealth;

					// noret: i see a problem here when players get not RPs after this player was healed to full
					// either move this to GameNPC or add a check.

					//We clean all damagedealers if we are fully healed,
					//no special XP calculations need to be done
					lock(m_xpGainers.SyncRoot)
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

				if (Alive && m_health < maxhealth)
				{
					StartHealthRegeneration();
				}
			}
		}

		/// <summary>
		/// Gets/sets the maximum amount of health
		/// </summary>
		public virtual int MaxHealth
		{
			get	{ return GetModified(eProperty.MaxHealth); }
//			set
//			{
//				m_maxHealth = value;
//				Health = Health; //cut extra hit points if there are any or start regeneration
//			}
		}

		/// <summary>
		/// Gets the Health in percent 0..100
		/// </summary>
		public virtual byte HealthPercent
		{
			get
			{
				return (byte) (MaxHealth <= 0 ? 0 : Health * 100 / MaxHealth);
			}
		}

		/// <summary>
		/// Gets/sets the object mana
		/// </summary>
		public virtual int Mana
		{
			get { return m_mana; }
			set
			{
				int maxmana = MaxMana;
				m_mana = Math.Min(value, maxmana);
				m_mana = Math.Max(m_mana, 0);

				if(Alive && m_mana < maxmana) {
					StartPowerRegeneration();
				}
			}
		}

		/// <summary>
		/// Gets/sets the maximum amount of mana
		/// </summary>
		public virtual int MaxMana
		{
			get
			{
				return GetModified(eProperty.MaxMana);
			}
//			set
//			{
//				m_maxMana = value;
//				Mana=Mana; //cut extra mana points if there are any or start regeneration
//			}
		}

		/// <summary>
		/// Gets the Mana in percent 0..100
		/// </summary>
		public virtual byte ManaPercent
		{
			get
			{
				return (byte) (MaxMana <= 0 ? 0 : ((Mana*100)/MaxMana));
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
				if (Alive && m_endurance < m_maxEndurance) {
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
				m_maxEndurance=value;
				Endurance=Endurance; //cut extra end points if there are any or start regeneration
			}
		}

		/// <summary>
		/// Gets the endurance in percent of maximum
		/// </summary>
		public virtual byte EndurancePercent
		{
			get
			{
				return (byte) (MaxEndurance <= 0 ? 0 : ((Endurance*100)/MaxEndurance));
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
				return (byte) (MaxConcentration <= 0 ? 0 : ((Concentration*100)/MaxConcentration));
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
			// cancel conc spells
			ConcentrationEffects.CancelAll();

			// cancel all active conc spell effects from other casters
			ArrayList concEffects = new ArrayList();
			lock (EffectList)
			{
				foreach (IGameEffect effect in EffectList)
				{
					if (effect is GameSpellEffect && ((GameSpellEffect)effect).Spell.Concentration > 0)
					{
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
		protected int		m_currentSpeed;
		/// <summary>
		/// The base maximum speed of this living
		/// </summary>
		protected int		m_maxSpeedBase;
		/// <summary>
		/// The guildname of this living
		/// </summary>
		protected string	m_guildName;
		/// <summary>
		/// Holds the Living's Coordinate inside the current Region
		/// </summary>
		protected Point3D	m_groundTarget;

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public override ushort Heading
		{
			get { return base.Heading; }
			set
			{
				base.Heading = value;
				RecalculatePostionAddition();
			}
		}
		/// <summary>
		/// Gets or sets the current speed of this living
		/// </summary>
		public virtual int CurrentSpeed
		{
			get
			{
				return m_currentSpeed;
			}
			set
			{
				m_currentSpeed = value;
				RecalculatePostionAddition();
			}
		}
		/// <summary>
		/// Gets the maxspeed of this living
		/// </summary>
		public virtual int MaxSpeed
		{
			get
			{
				return GetModified(eProperty.MaxSpeed);
			}
		}

		/// <summary>
		/// Gets or sets the base max speed of this living
		/// </summary>
		public virtual int MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}

		/// <summary>
		/// Gets or sets the guildname of this living
		/// </summary>
		public virtual string GuildName
		{
			get
			{
				return m_guildName;
			}
			set
			{
				m_guildName = value;
			}
		}
		/// <summary>
		/// Gets or sets the target of this living
		/// </summary>
		public GameObject TargetObject
		{
			get
			{
				return (m_targetObjectWeakReference.Target as GameObject);
			}
			set
			{
				GameObject previousTarget = m_targetObjectWeakReference.Target as GameObject;
				if (previousTarget is GameLiving)
					((GameLiving)previousTarget).RemoveAttacker(this);

				GameObject newTarget = value;
				m_targetObjectWeakReference.Target = newTarget;

				if (AttackState && newTarget is GameLiving)
					((GameLiving)newTarget).AddAttacker(this);
			}
		}
		/// <summary>
		/// Gets the current sit state
		/// </summary>
		public virtual bool Sitting
		{
			get
			{
				return false;
			}
			set
			{
			}
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
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendLivingDataUpdate(this, false);
					}
				}
			}
		}

		#endregion
		#region Movement
		//Movement relevant variables
		/// <summary>
		/// Holds when the movement started
		/// </summary>
		private int  m_movementStartTick;

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		private IList m_currentAreas;
		
		/// <summary>
		/// The X addition per coordinate of forward movement
		/// </summary>
		protected float m_xAddition;
		
		/// <summary>
		/// The Y addition per coordinate of forward movement
		/// </summary>
		protected float m_yAddition;
		
		/// <summary>
		/// The Z addition per coordinate of forward movement
		/// </summary>
		protected float m_zAddition;

		/// <summary>
		/// Gets the X addition per coordinate of forward movement
		/// </summary>
		public float XAddition
		{
			get { return m_xAddition; }
		}

		/// <summary>
		/// Gets the Y addition per coordinate of forward movement
		/// </summary>
		public float YAddition
		{
			get { return m_yAddition; }
		}

		/// <summary>
		/// Gets the Z addition per coordinate of forward movement
		/// </summary>
		public float ZAddition
		{
			get { return m_zAddition; }
		}

		/// <summary>
		/// Recalculates position addition values of this living
		/// </summary>
		protected virtual void RecalculatePostionAddition()
		{
			int speed = CurrentSpeed;
			if (speed == 0)
			{
				m_xAddition = m_yAddition = m_zAddition = 0f;
			}
			else
			{
				float h = (float)(Heading/HEADING_CONST);
				m_xAddition = (float)(-Math.Sin(h)*speed*0.001);
				m_yAddition = (float)(Math.Cos(h)*speed*0.001);
				m_zAddition = 0f;
			}
//			log.WarnFormat("{0} living pos addition set to ({1} {2} {3})", Name, m_xAddition, m_yAddition, m_zAddition);
		}
		
		/// <summary>
		/// Gets or sets the MovementStartTick
		/// </summary>
		public int MovementStartTick
		{
			get
			{
				return m_movementStartTick;
			}
			set
			{
				m_movementStartTick=value;
			}
		}
		/// <summary>
		/// Returns if the npc is moving or not
		/// </summary>
		public virtual bool IsMoving
		{
			get
			{
				return m_currentSpeed!=0;
			}
		}
		/// <summary>
		/// Gets the current position of this living.
		/// </summary>
		public override int X
		{
			get
			{
				if(!IsMoving)
					return base.X;
				return (int)(base.X+((Environment.TickCount-MovementStartTick)*m_xAddition));
			}
			set
			{
				base.X=value;
			}
		}
		/// <summary>
		/// Gets the current position of this living.
		/// </summary>
		public override int Y
		{
			get
			{
				if(!IsMoving)
					return base.Y;
				return (int)(base.Y+((Environment.TickCount-MovementStartTick)*m_yAddition));
			}
			set
			{
				base.Y=value;
			}
		}
		/// <summary>
		/// Gets the current position of this living.
		/// </summary>
		public override int Z
		{
			get
			{
				if(!IsMoving)
					return base.Z;
				return (int)(base.Z+((Environment.TickCount-MovementStartTick)*m_zAddition));
			}
			set
			{
				base.Z=value;
			}
		}

		/// <summary>
		/// Gets the position this object will have in the future
		/// </summary>
		/// <param name="x">out future x</param>
		/// <param name="y">out future y</param>
		/// <param name="timeDiff">the difference between now and "the future" in ms</param>
		public virtual void GetFuturePosition(out int x, out int y, int timeDiff)
		{
			x=(int)(X+(timeDiff*m_xAddition));
			y=(int)(Y+(timeDiff*m_yAddition));
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
			{
				CancelAllConcentrationEffects();
			}
			return base.MoveTo(regionID, x, y, z, heading);
		}

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		public IList CurrentAreas
		{
			get { return m_currentAreas; }
			set { m_currentAreas = value; }
		}

		/// <summary>
		/// The stealth state of this living
		/// </summary>
		public virtual bool IsStealthed
		{
			get { return false; }
		}

		#endregion
		#region Say/Yell/Whisper/Emote
		/// <summary>
		/// This function is called when this object receives a Say
		/// </summary>
		/// <param name="source">Source of say</param>
		/// <param name="str">Text that was spoken</param>
		/// <returns>true if the text should be processed further, false if it should be discarded</returns>
		public virtual bool SayReceive(GameLiving source, string str)
		{
			if(source==null || str==null) return false;
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
			if(str==null) return false;
			Notify(GameLivingEvent.Say, this, new SayEventArgs(str));
			foreach(GameNPC npc in GetNPCsInRadius(WorldMgr.SAY_DISTANCE))
				if(npc!=this)
					npc.SayReceive(this,str);
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.SAY_DISTANCE))
				if(player!=this)
					player.SayReceive(this,str);
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
			if(source==null || str==null) return false;
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
			if(str==null) return false;
			Notify(GameLivingEvent.Yell, this, new YellEventArgs(str));
			foreach(GameNPC npc in GetNPCsInRadius(WorldMgr.YELL_DISTANCE))
				if(npc!=this)
					npc.YellReceive(this,str);
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.YELL_DISTANCE))
				if(player!=this)
					player.YellReceive(this,str);
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
			if(source==null || str==null) return false;
			Notify(GameLivingEvent.WhisperReceive, this, new WhisperReceiveEventArgs(source, this, str));
			return true;
		}

		/// <summary>
		/// Sends a whisper to a target
		/// </summary>
		/// <param name="target">The target of the whisper</param>
		/// <param name="str">text to whisper (without any "xxx whispers:" in front!!!)</param>
		/// <returns>true if text was whispered successfully</returns>
		public virtual bool Whisper(GameLiving target, string str)
		{
			if(target==null || str==null) return false;
			if(!WorldMgr.CheckDistance(this,target,WorldMgr.WHISPER_DISTANCE))
				return false;
			Notify(GameLivingEvent.Whisper, this, new WhisperEventArgs(target, str));
			return target.WhisperReceive(this,str);
		}
		/// <summary>
		/// Makes this living do an emote-animation
		/// </summary>
		/// <param name="emote">the emote animation to show</param>
		public virtual void Emote(eEmote emote)
		{
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendEmoteAnimation(this,emote);
			}
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
			if(source==null || item==null) return false;

			Notify(GameLivingEvent.ReceiveItem, this, new ReceiveItemEventArgs(source, this, item));

			//If the item has been removed by the event handlers : return
			if(item == null || item.OwnerID == null)
			{
				return true;
			}

			if(source is GamePlayer)
				((GamePlayer)source).Out.SendMessage(Name+" doesn't want this item!",eChatType.CT_System,eChatLoc.CL_SystemWindow);

			return base.ReceiveItem(source, item);
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
			if(source==null || money<=0) return false;

			Notify(GameLivingEvent.ReceiveMoney, this, new ReceiveMoneyEventArgs(source, this, money));

			if(source is GamePlayer)
				((GamePlayer)source).Out.SendMessage(Name+" doesn't want your money!",eChatType.CT_System,eChatLoc.CL_SystemWindow);

			//call base
			return base.ReceiveMoney(source,money);
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
			ArrayList temp;
			lock(m_attackers.SyncRoot)
			{
				temp = (ArrayList) m_attackers.Clone();
				m_attackers.Clear();
			}
			foreach(GameObject obj in temp)
				if(obj is GameLiving)
					((GameLiving)obj).EnemyKilled(this);

			CancelAllConcentrationEffects();
			ConcentrationEffects.CancelAll();
			EffectList.CancelAll();
			StopHealthRegeneration();
			StopPowerRegeneration();
			StopEnduranceRegeneration();

			if (m_attackAction != null) m_attackAction.Stop();
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
		/// Holds the active spell handlers for this living
		/// </summary>
//		protected readonly ArrayList m_activeSpellHandlers = new ArrayList(1);
		/// <summary>
		/// spell handlers that are currently running duration spells
		/// </summary>
//		public IList ActiveSpellHandlers
//		{
//			get { return m_activeSpellHandlers; }
//		}

		/// <summary>
		/// Holds the currently running spell handler
		/// </summary>
		protected ISpellHandler m_runningSpellHandler;
		/// <summary>
		/// active spellhandler (casting phase) or null
		/// </summary>
		public ISpellHandler CurrentSpellHandler
		{
			get { return m_runningSpellHandler; }
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
		public virtual void CastSpell(Spell spell, SpellLine line)
		{
			if (this.Stun || this.Mez ) return;
			if (m_runningSpellHandler!=null && spell.CastTime>0) return;

			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
			if (spellhandler != null)
			{
				// If played is engage when casting cancel engage
				EngageEffect engage = (EngageEffect) EffectList.GetOfType(typeof(EngageEffect));
				if (engage!=null)
				{
					engage.Cancel(false);
				}

				m_runningSpellHandler = spellhandler;
				spellhandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				spellhandler.CastSpell();
			} else {
				if (log.IsWarnEnabled)
					log.Warn(Name+" wants to cast but spell "+spell.Name+" not implemented yet");
			}
		}

		#endregion
		#region LoadCalculators
		public static bool LoadCalculators()
		{
			try
			{
				ArrayList asms = new ArrayList();
				asms.Add(typeof(GameServer).Assembly);
				asms.AddRange(ScriptMgr.Scripts);
				foreach (Assembly asm in asms)
				{
					foreach (Type t in asm.GetTypes())
					{
						try
						{
							if (!t.IsClass) continue;
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
								log.Error("Error while working with type "+t.FullName, e);
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

		/// <summary>
		/// Constructor to create a new GameLiving
		/// </summary>
		public GameLiving() : base()
		{
			m_guildName = string.Empty;
			m_targetObjectWeakReference = new WeakRef(null);
			m_groundTarget = new Point3D(0,0,0);

			//Set all combat properties
			m_activeWeaponSlot = eActiveWeaponSlot.Standard;
			m_activeQuiverSlot = eActiveQuiverSlot.None;
			m_rangeAttackState = eRangeAttackState.None;
			m_rangeAttackType = eRangeAttackType.Normal;
			m_healthRegenerationPeriod = 6000;
			m_powerRegenerationPeriod = 6000;
			m_enduRegenerationPeriod = 1000;
			m_xpGainers = new HybridDictionary();
			m_effects = CreateEffectsList();
			m_concEffects = new ConcentrationList(this);
			m_attackers = new ArrayList(1);

			m_health = 1;
			m_mana = 1;
			m_endurance = 1;
			m_maxEndurance = 1;

			m_currentAreas = new ArrayList(1);
		}
	}
}
