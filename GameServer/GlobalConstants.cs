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
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Language;

namespace DOL.GS
{
	/// <summary>
	/// Defines the realms for various packets and search functions etc.
	/// </summary>
	public enum eRealm : byte
	{
		/// <summary>
		/// First realm number, for use in all arrays
		/// </summary>
		_First = 0,
		/// <summary>
		/// No specific realm
		/// </summary>
		None = 0,
		/// <summary>
		/// First player realm number, for use in all arrays
		/// </summary>
		_FirstPlayerRealm = 1,
		/// <summary>
		/// Albion Realm
		/// </summary>
		Albion = 1,
		/// <summary>
		/// Midgard Realm
		/// </summary>
		Midgard = 2,
		/// <summary>
		/// Hibernia Realm
		/// </summary>
		Hibernia = 3,
		/// <summary>
		/// Last player realm number, for use in all arrays
		/// </summary>
		_LastPlayerRealm = 3,

		/// <summary>
		/// LastRealmNumber to allow dynamic allocation of realm specific arrays.
		/// </summary>
		_Last = 3,
		
		/// <summary>
		/// DoorRealmNumber to allow dynamic allocation of realm specific arrays.
		/// </summary>
		Door = 6,
	};

	/// <summary>
	/// Flags applicables to an item
	/// </summary>
	public enum eItemFlags
	{
		CannotBeSoldToMerchants = 0x1,
		CannotBeDestroyed = 0x2,
		CannotBeTradedToOtherPlayers = 0x4,
		CannotBeDropped = 0x8,
		CanBeDroppedAsLoot = 0x10,
	}

	/// <summary>
	/// The privilege level of the client
	/// </summary>
	public enum ePrivLevel : uint
	{
		/// <summary>
		/// Normal player
		/// </summary>
		Player = 1,
		/// <summary>
		/// A GM
		/// </summary>
		GM = 2,
		/// <summary>
		/// An Admin
		/// </summary>
		Admin = 3,
	}

	/// <summary>
	/// Object type sets the type of object, for example sword or shield
	/// </summary>
	public enum eObjectType : byte
	{
		GenericItem = 0,
		GenericWeapon = 1,

		//Albion weapons
		_FirstWeapon = 2,
		CrushingWeapon = 2,
		SlashingWeapon = 3,
		ThrustWeapon = 4,
		Fired = 5,
		TwoHandedWeapon = 6,
		PolearmWeapon = 7,
		Staff = 8,
		Longbow = 9,
		Crossbow = 10,
		Flexible = 24,

		//Midgard weapons
		Sword = 11,
		Hammer = 12,
		Axe = 13,
		Spear = 14,
		CompositeBow = 15,
		Thrown = 16,
		LeftAxe = 17,
		HandToHand = 25,

		//Hibernia weapons
		RecurvedBow = 18,
		Blades = 19,
		Blunt = 20,
		Piercing = 21,
		LargeWeapons = 22,
		CelticSpear = 23,
		Scythe = 26,

		//Mauler weapons
		FistWraps = 27,
		MaulerStaff = 28,
		_LastWeapon = 28,

		//Armor
		_FirstArmor = 31,
		GenericArmor = 31,
		Cloth = 32,
		Leather = 33,
		Studded = 34,
		Chain = 35,
		Plate = 36,
		Reinforced = 37,
		Scale = 38,
		_LastArmor = 38,

		//Misc
		Magical = 41,
		Shield = 42,
		Arrow = 43,
		Bolt = 44,
		Instrument = 45,
		Poison = 46,
		AlchemyTincture = 47,
		SpellcraftGem = 48,

		//housing
		_FirstHouse = 49,
		GardenObject = 49,
		HouseWallObject = 50,
		HouseFloorObject = 51,
		HouseCarpetFirst = 52,
		HouseNPC = 53,
		HouseVault = 54,
		HouseInteriorObject = 55, //Lathe, forge, alchemy table
		HouseTentColor = 56,
		HouseExteriorBanner = 57,
		HouseExteriorShield = 58,
		HouseRoofMaterial = 59,
		HouseWallMaterial = 60,
		HouseDoorMaterial = 61,
		HousePorchMaterial = 62,
		HouseWoodMaterial = 63,
		HouseShutterMaterial = 64,
		HouseInteriorBanner = 66,
		HouseInteriorShield = 67,
		HouseBindstone = 68,
		HouseCarpetSecond = 69,
		HouseCarpetThird = 70,
		HouseCarpetFourth = 71,
		_LastHouse = 71,

		//siege weapons
		SiegeBalista = 80, // need log
		SiegeCatapult = 81, // need log
		SiegeCauldron = 82, // need log
		SiegeRam = 83, // need log
		SiegeTrebuchet = 84, // need log
	}

	/// <summary>
	/// This enumeration holds all equipment
	/// items that can be used by a player
	/// </summary>
	public enum eEquipmentItems : byte
	{
		HORSE = 0x09,
		RIGHT_HAND = 0x0A,
		LEFT_HAND = 0x0B,
		TWO_HANDED = 0x0C,
		RANGED = 0x0D,
		HEAD = 0x15,
		HAND = 0x16,
		FEET = 0x17,
		JEWEL = 0x18,
		TORSO = 0x19,
		CLOAK = 0x1A,
		LEGS = 0x1B,
		ARMS = 0x1C,
		NECK = 0x1D,
		WAIST = 0x20,
		L_BRACER = 0x21,
		R_BRACER = 0x22,
		L_RING = 0x23,
		R_RING = 0x24,
		MYTHICAL = 0x25
	};

	/// <summary>
	/// all known slots
	/// </summary>
	public class Slot
	{
		public const int HORSEARMOR = 7;
		public const int HORSEBARDING = 8;
		public const int HORSE = 9;
		public const int RIGHTHAND = 10;
		public const int LEFTHAND = 11;
		public const int TWOHAND = 12;
		public const int RANGED = 13;
		public const int FIRSTQUIVER = 14;
		public const int SECONDQUIVER = 15;
		public const int THIRDQUIVER = 16;
		public const int FOURTHQUIVER = 17;
		public const int HELM = 21;
		public const int HANDS = 22;
		public const int FEET = 23;
		public const int JEWELRY = 24;
		public const int TORSO = 25;
		public const int CLOAK = 26;
		public const int LEGS = 27;
		public const int ARMS = 28;
		public const int NECK = 29;
		public const int FOREARMS = 30;
		public const int SHIELD = 31;
		public const int WAIST = 32;
		public const int LEFTWRIST = 33;
		public const int RIGHTWRIST = 34;
		public const int LEFTRING = 35;
		public const int RIGHTRING = 36;
		public const int MYTHICAL = 37;
	};

	/// <summary>
	/// This enumeration holds all slots that can wear attackable armor
	/// </summary>
	public enum eArmorSlot : int
	{
		NOTSET = 0x00,
		HEAD = eInventorySlot.HeadArmor,
		HAND = eInventorySlot.HandsArmor,
		FEET = eInventorySlot.FeetArmor,
		TORSO = eInventorySlot.TorsoArmor,
		LEGS = eInventorySlot.LegsArmor,
		ARMS = eInventorySlot.ArmsArmor,
	};

	/// <summary>
	/// The armor ability level for different armor types
	/// </summary>
	public abstract class ArmorLevel
	{
		public const int GenericArmor = 0;
		public const int Cloth = 1;
		public const int Leather = 2;
		public const int Reinforced = 3;
		public const int Studded = 3;
		public const int Scale = 4;
		public const int Chain = 4;
		public const int Plate = 5;
	}

	/// <summary>
	/// Holds all the damage types that
	/// some attack may cause on the target
	/// </summary>
	public enum eDamageType : byte
	{
		_FirstResist = 0,
		Natural = 0,
		Crush = 1,
		Slash = 2,
		Thrust = 3,

		Body = 10,
		Cold = 11,
		Energy = 12,
		Heat = 13,
		Matter = 14,
		Spirit = 15,
		_LastResist = 15,
		/// <summary>
		/// Damage is from a GM via a command
		/// </summary>
		GM = 254,
		/// <summary>
		/// Player is taking falling damage
		/// </summary>
		Falling = 255,
	}

	/// <summary>
	/// Holds the weapon damage type
	/// </summary>
	public enum eWeaponDamageType : byte
	{
		Elemental = 0,
		Crush = 1,
		Slash = 2,
		Thrust = 3,

		Body = 10,
		Cold = 11,
		Energy = 12,
		Heat = 13,
		Matter = 14,
		Spirit = 15,
	}

	/// <summary>
	/// The type of stat
	/// </summary>
	public enum eStat : byte
	{
		UNDEFINED = 0,
		_First = eProperty.Stat_First,
		STR = eProperty.Strength,
		DEX = eProperty.Dexterity,
		CON = eProperty.Constitution,
		QUI = eProperty.Quickness,
		INT = eProperty.Intelligence,
		PIE = eProperty.Piety,
		EMP = eProperty.Empathy,
		CHR = eProperty.Charisma,
		_Last = eProperty.Stat_Last,
	}

	/// <summary>
	/// resists
	/// </summary>
	public enum eResist : byte
	{
		Natural = eProperty.Resist_Natural,
		Crush = eProperty.Resist_Crush,
		Slash = eProperty.Resist_Slash,
		Thrust = eProperty.Resist_Thrust,
		Body = eProperty.Resist_Body,
		Cold = eProperty.Resist_Cold,
		Energy = eProperty.Resist_Energy,
		Heat = eProperty.Resist_Heat,
		Matter = eProperty.Resist_Matter,
		Spirit = eProperty.Resist_Spirit
	}

	/// <summary>
	/// colors
	/// </summary>
	public enum eColor : byte
	{
		White = 0,
		Old_Red = 1,
		Old_Green = 2,
		Old_Blue = 3,
		Old_Yellow = 4,
		Old_Purple = 5,
		Old_Turquoise = 7,
		Gray = 6,
		Leather_Yellow = 8,
		Leather_Red = 9,
		Leather_Green = 10,
		Leather_Orange = 11,
		Leather_Violet = 12,
		Leather_Forest_Green = 13,
		Leather_Blue = 14,
		Leather_Purple = 15,
		Bronze = 16,
		Iron = 17,
		Steel = 18,
		Alloy = 19,
		Fine_Alloy = 20,
		Mithril = 21,
		Asterite = 22,
		Eog = 23,
		Xenium = 24,
		Vaanum = 25,
		Adamantium = 26,
		Cloth_Red = 27,
		Cloth_Orange = 28,
		Cloth_Yellow_Orange = 29,
		Cloth_Yellow = 30,
		Cloth_Yellow_Green = 31,
		Cloth_Green = 32,
		Cloth_Blue_Green = 33,
		Cloth_Turquoise = 34,
		Cloth_Light_Blue = 35,
		Cloth_Blue = 36,
		Cloth_Blue_Violet = 37,
		Cloth_Violet = 38,
		Cloth_Bright_Violet = 39,
		Cloth_Purple = 40,
		Cloth_Bright_Purple = 41,
		Cloth_Purple_Red = 42,
		Cloth_Black = 43,
		Cloth_Brown = 44,
		Metal_Blue = 45,
		Metal_Green = 46,
		Metal_Yellow = 47,
		Metal_Gold = 48,
		Metal_Red = 49,
		Metal_Purple = 50,
		Blue_1 = 51,
		Blue_2 = 52,
		Blue_3 = 53,
		Blue_4 = 54,
		Turquoise_1 = 55,
		Turquoise_2 = 56,
		Turquoise_3 = 57,
		Teal_1 = 58,
		Teal_2 = 59,
		Teal_3 = 60,
		Brown_1 = 61,
		Brown_2 = 62,
		Brown_3 = 63,
		Red_1 = 64,
		Red_2 = 65,
		Red_3 = 66,
		Red_4 = 67,
		Green_1 = 68,
		Green_2 = 69,
		Green_3 = 70,
		Green_4 = 71,
		Gray_1 = 72,
		Gray_2 = 73,
		Gray_3 = 74,
		Orange_1 = 75,
		Orange_2 = 76,
		Orange_3 = 77,
		Purple_1 = 78,
		Purple_2 = 79,
		Purple_3 = 80,
		Purple_4 = 87,
		Yellow_1 = 81,
		Yellow_2 = 82,
		Yellow_3 = 83,
		Violet = 84,
		Mauve = 85,
		Blue_42 = 86
	}

	/// <summary>
	/// Ressurection sickness types
	/// </summary>
	public enum eResurectionSicknessType : int
	{
		PvMSickness = 0,
		RvRSickness = 1,
		NoSickness = 2
	}

	/// <summary>
	/// instrument types
	/// </summary>
	public enum eInstrumentType : int
	{
		Drum = 1,
		Lute = 2,
		Flute = 3,
		Harp = 4,
	}

	/// <summary>
	/// All property types for check using SkillBase.CheckPropertyType. Must be unique bits set.
	/// </summary>
	[Flags]
	public enum ePropertyType : ushort
	{
		Focus = 1,
		Resist = 1 << 1,
		Skill = 1 << 2,
		SkillMeleeWeapon = 1 << 3,
		SkillMagical = 1 << 4,
		SkillDualWield = 1 << 5,
		SkillArchery = 1 << 6,
		ResistMagical = 1 << 7,
		Albion = 1 << 8,
		Midgard = 1 << 9,
		Hibernia = 1 << 10,
		Common = 1 << 11,
		CapIncrease = 1 << 12,
	}

	/// <summary>
	/// all available and buffable/bonusable properties on livings
	/// </summary>
	public enum eProperty : byte
	{
		Undefined = 0,
		// Note, these are set in the ItemDB now.  Changing
		//any order will screw things up.
		// char stats
		#region Stats
		Stat_First = 1,
		Strength = 1,
		Dexterity = 2,
		Constitution = 3,
		Quickness = 4,
		Intelligence = 5,
		Piety = 6,
		Empathy = 7,
		Charisma = 8,
		Stat_Last = 8,

		MaxMana = 9,
		MaxHealth = 10,
		#endregion

		#region Resists
		// resists
		Resist_First = 11,
		Resist_Body = 11,
		Resist_Cold = 12,
		Resist_Crush = 13,
		Resist_Energy = 14,
		Resist_Heat = 15,
		Resist_Matter = 16,
		Resist_Slash = 17,
		Resist_Spirit = 18,
		Resist_Thrust = 19,
		Resist_Last = 19,
		#endregion

		#region Skills
		// skills
		Skill_First = 20,
		Skill_Two_Handed = 20,
		Skill_Body = 21,
		Skill_Chants = 22,
		Skill_Critical_Strike = 23,
		Skill_Cross_Bows = 24,
		Skill_Crushing = 25,
		Skill_Death_Servant = 26,
		Skill_DeathSight = 27,
		Skill_Dual_Wield = 28,
		Skill_Earth = 29,
		Skill_Enhancement = 30,
		Skill_Envenom = 31,
		Skill_Fire = 32,
		Skill_Flexible_Weapon = 33,
		Skill_Cold = 34,
		Skill_Instruments = 35,
		Skill_Long_bows = 36,
		Skill_Matter = 37,
		Skill_Mind = 38,
		Skill_Pain_working = 39,
		Skill_Parry = 40,
		Skill_Polearms = 41,
		Skill_Rejuvenation = 42,
		Skill_Shields = 43,
		Skill_Slashing = 44,
		Skill_Smiting = 45,
		Skill_SoulRending = 46,
		Skill_Spirit = 47,
		Skill_Staff = 48,
		Skill_Stealth = 49,
		Skill_Thrusting = 50,
		Skill_Wind = 51,
		Skill_Sword = 52,
		Skill_Hammer = 53,
		Skill_Axe = 54,
		Skill_Left_Axe = 55,
		Skill_Spear = 56,
		Skill_Mending = 57,
		Skill_Augmentation = 58,
		//Skill_Cave_Magic = 59,
		Skill_Darkness = 60,
		Skill_Suppression = 61,
		Skill_Runecarving = 62,
		Skill_Stormcalling = 63,
		Skill_BeastCraft = 64,
		Skill_Light = 65,
		Skill_Void = 66,
		Skill_Mana = 67,
		Skill_Composite = 68,
		Skill_Battlesongs = 69,
		Skill_Enchantments = 70,
		// 71 Available for a Skill
		Skill_Blades = 72,
		Skill_Blunt = 73,
		Skill_Piercing = 74,
		Skill_Large_Weapon = 75,
		Skill_Mentalism = 76,
		Skill_Regrowth = 77,
		Skill_Nurture = 78,
		Skill_Nature = 79,
		Skill_Music = 80,
		Skill_Celtic_Dual = 81,
		Skill_Celtic_Spear = 82,
		Skill_RecurvedBow = 83,
		Skill_Valor = 84,
		Skill_Subterranean = 85,
		Skill_BoneArmy = 86,
		Skill_Verdant = 87,
		Skill_Creeping = 88,
		Skill_Arboreal = 89,
		Skill_Scythe = 90,
		Skill_Thrown_Weapons = 91,
		Skill_HandToHand = 92,
		Skill_ShortBow = 93,
		Skill_Pacification = 94,
		Skill_Savagery = 95,
		Skill_Nightshade = 96,
		Skill_Pathfinding = 97,
		Skill_Summoning = 98,
		Skill_Dementia = 99,
		Skill_ShadowMastery = 100,
		Skill_VampiiricEmbrace = 101,
		Skill_EtherealShriek = 102,
		Skill_PhantasmalWail = 103,
		Skill_SpectralForce = 104,
		Skill_OdinsWill = 105,
		Skill_Cursing = 106,
		Skill_Hexing = 107,
		Skill_Witchcraft = 108,
		Skill_MaulerStaff = 109,
		Skill_FistWraps = 110,
		Skill_Power_Strikes = 111,
		Skill_Magnetism = 112,
		Skill_Aura_Manipulation = 113,
		Skill_SpectralGuard = 114,
		Skill_Archery = 115,
		Skill_Last = 115,
		#endregion

		// 116 - 119 Available

		Focus_Darkness = 120,
		Focus_Suppression = 121,
		Focus_Runecarving = 122,
		Focus_Spirit = 123,
		Focus_Fire = 124,
		Focus_Air = 125,
		Focus_Cold = 126,
		Focus_Earth = 127,
		Focus_Light = 128,
		Focus_Body = 129,
		Focus_Matter = 130,
		// 131 Available
		Focus_Mind = 132,
		Focus_Void = 133,
		Focus_Mana = 134,
		Focus_Enchantments = 135,
		Focus_Mentalism = 136,
		Focus_Summoning = 137,
		Focus_BoneArmy = 138,
		Focus_PainWorking = 139,
		Focus_DeathSight = 140,
		Focus_DeathServant = 141,
		Focus_Verdant = 142,
		Focus_CreepingPath = 143,
		Focus_Arboreal = 144,
		MaxSpeed = 145,
		// 146 Available
		MaxConcentration = 147,
		ArmorFactor = 148,
		ArmorAbsorption = 149,
		HealthRegenerationRate = 150,
		PowerRegenerationRate = 151,
		EnduranceRegenerationRate = 152,
		SpellRange = 153,
		ArcheryRange = 154,
		MeleeSpeed = 155,
		Acuity = 156,
		Focus_EtherealShriek = 157,
		Focus_PhantasmalWail = 158,
		Focus_SpectralForce = 159,
		Focus_Cursing = 160,
		Focus_Hexing = 161,
		Focus_Witchcraft = 162,
		AllMagicSkills = 163,
		AllMeleeWeaponSkills = 164,
		AllFocusLevels = 165,
		LivingEffectiveLevel = 166,
		AllDualWieldingSkills = 167,
		AllArcherySkills = 168,
		EvadeChance = 169,
		BlockChance = 170,
		ParryChance = 171,
		FatigueConsumption = 172,
		MeleeDamage = 173,
		RangedDamage = 174,
		FumbleChance = 175,
		MesmerizeDurationReduction = 176,
		StunDurationReduction = 177,
		SpeedDecreaseDurationReduction = 178,
		BladeturnReinforcement = 179,
		DefensiveBonus = 180,
		SpellFumbleChance = 181,
		NegativeReduction = 182,
		PieceAblative = 183,
		ReactionaryStyleDamage = 184,
		SpellPowerCost = 185,
		StyleCostReduction = 186,
		ToHitBonus = 187,

		#region TOA
		//TOA
		ToABonus_First = 188,
		ArcherySpeed = 188,
		ArrowRecovery = 189,
		BuffEffectiveness = 190,
		CastingSpeed = 191,
		DeathExpLoss = 192,
		DebuffEffectivness = 193,
		Fatigue = 194,
		HealingEffectiveness = 195,
		PowerPool = 196,
		ResistPierce = 197,
		SpellDamage = 198,
		SpellDuration = 199,
		StyleDamage = 200,
		ToABonus_Last = 200,
		#endregion

		#region Cap Bonuses
		//Caps bonuses
		StatCapBonus_First = 201,
		StrCapBonus = 201,
		DexCapBonus = 202,
		ConCapBonus = 203,
		QuiCapBonus = 204,
		IntCapBonus = 205,
		PieCapBonus = 206,
		EmpCapBonus = 207,
		ChaCapBonus = 208,
		AcuCapBonus = 209,
		MaxHealthCapBonus = 210,
		PowerPoolCapBonus = 211,
		StatCapBonus_Last = 211,
		#endregion

		WeaponSkill = 212,
		AllSkills = 213,
		CriticalMeleeHitChance = 214,
		CriticalArcheryHitChance = 215,
		CriticalSpellHitChance = 216,
		WaterSpeed = 217,
		SpellLevel = 218,
		MissHit = 219,
		KeepDamage = 220,

		#region Resist Cap Increases
		//Resist cap increases
		ResCapBonus_First = 221,
		BodyResCapBonus = 221,
		ColdResCapBonus = 222,
		CrushResCapBonus = 223,
		EnergyResCapBonus = 224,
		HeatResCapBonus = 225,
		MatterResCapBonus = 226,
		SlashResCapBonus = 227,
		SpiritResCapBonus = 228,
		ThrustResCapBonus = 229,
		ResCapBonus_Last = 229,
		#endregion

		DPS = 230,
		MagicAbsorption = 231,
		CriticalHealHitChance = 232,

        MythicalSafeFall = 233,
        MythicalDiscumbering = 234,
        MythicalCoin = 235,
        // 236 - 246 used for Mythical Stat Cap
        MythicalStatCapBonus_First = 236,
        MythicalStrCapBonus = 236,
        MythicalDexCapBonus = 237,
        MythicalConCapBonus = 238,
        MythicalQuiCapBonus = 239,
        MythicalIntCapBonus = 240,
        MythicalPieCapBonus = 241,
        MythicalEmpCapBonus = 242,
        MythicalChaCapBonus = 243,
        MythicalAcuCapBonus = 244,
        MythicalStatCapBonus_Last = 244,

		BountyPoints = 247,
		XpPoints = 248,
		Resist_Natural = 249,
		ExtraHP = 250,
		Conversion = 251,
		StyleAbsorb = 252,
		RealmPoints = 253,
		ArcaneSyphon = 254,
		LivingEffectiveness = 255,
		MaxProperty = 255
	}

	/// <summary>
	/// available races
	/// </summary>
	public enum eRace : byte
	{
		Unknown = 0,
		Briton = 1,
		Avalonian = 2,
		Highlander = 3,
		Saracen = 4,
		Norseman = 5,
		Troll = 6,
		Dwarf = 7,
		Kobold = 8,
		Celt = 9,
		Firbolg = 10,
		Elf = 11,
		Lurikeen = 12,
		Inconnu = 13,
		Valkyn = 14,
		Sylvan = 15,
		HalfOgre = 16,
		Frostalf = 17,
		Shar = 18,
		AlbionMinotaur = 19,
		MidgardMinotaur = 20,
		HiberniaMinotaur = 21,
		Korazh = 19,
		Deifrang = 20,
		Graoch = 21,
	}

	/// <summary>
	/// What buff category a spell belongs too
	/// </summary>
	public enum eBuffBonusCategory : int
	{
		BaseBuff = 1,
		SpecBuff = 2,
		Debuff = 3,
		Other = 4,
		SpecDebuff = 5,
		AbilityBuff = 6,
	}

	/// <summary>
	/// Holds all character classes
	/// </summary>
	public enum eCharacterClass : byte
	{
		Unknown = 0,

		//base classes
		Acolyte = 16,
		AlbionRogue = 17,
		Disciple = 20,
		Elementalist = 15,
		Fighter = 14,
		Forester = 57,
		Guardian = 52,
		Mage = 18,
		Magician = 51,
		MidgardRogue = 38,
		Mystic = 36,
		Naturalist = 53,
		Seer = 37,
		Stalker = 54,
		Viking = 35,

		//alb classes
		Armsman = 2,
		Cabalist = 13,
		Cleric = 6,
		Friar = 10,
		Heretic = 33,
		Infiltrator = 9,
		Mercenary = 11,
		Minstrel = 4,
		Necromancer = 12,
		Paladin = 1,
		Reaver = 19,
		Scout = 3,
		Sorcerer = 8,
		Theurgist = 5,
		Wizard = 7,
		MaulerAlb = 60,

		//mid classes
		Berserker = 31,
		Bonedancer = 30,
		Healer = 26,
		Hunter = 25,
		Runemaster = 29,
		Savage = 32,
		Shadowblade = 23,
		Shaman = 28,
		Skald = 24,
		Spiritmaster = 27,
		Thane = 21,
		Valkyrie = 34,
		Warlock = 59,
		Warrior = 22,
		MaulerMid = 61,

		//hib classes
		Animist = 55,
		Bainshee = 39,
		Bard = 48,
		Blademaster = 43,
		Champion = 45,
		Druid = 47,
		Eldritch = 40,
		Enchanter = 41,
		Hero = 44,
		Mentalist = 42,
		Nightshade = 49,
		Ranger = 50,
		Valewalker = 56,
		Vampiir = 58,
		Warden = 46,
		MaulerHib = 62,
	}

	/// <summary>
	/// Customisable face part
	/// </summary>
	public enum eCharFacePart : byte
	{
		EyeSize = 0,
		LipSize = 1,
		EyeColor = 2,
		HairColor = 3,
		FaceType = 4,
		HairStyle = 5,
		MoodType = 6,
		_Last = 6,
	}

	/// <summary>
	/// the size of a shield
	/// </summary>
	public abstract class ShieldLevel
	{
		public const int Small = 1;
		public const int Medium = 2;
		public const int Large = 3;
	}

	/// <summary>
	/// The type of instrument
	/// </summary>
	public abstract class InstrumentType
	{
		public const int Drum = 1;
		public const int Lute = 2;
		public const int Flute = 3;
		public const int Harp = 4;
	}

	/// <summary>
	/// the material type
	/// </summary>
	public enum eMaterial : byte
	{
		Cloth = 0,
		Leather = 1,
		LeatherAndMetal = 2,
		Metal = 3,
		Wood = 4,
	}

	/// <summary>
	/// The gender.
	/// </summary>
	public enum eGender : byte
	{
		Neutral = 0,
		Male = 1,
		Female = 2
	}

	public enum eClientExpansion : int
	{
		None = 1,
		ShroudedIsles = 2,
		TrialsOfAtlantis = 3,
		Catacombs = 4,
		DarknessRising = 5,
		Labyrinth = 6
	}

	public enum eQuestIndicator : byte
	{
		None = 0x00,
		Available = 0x01,
		Finish = 0x02,
		Lesson = 0x04,
		Lore = 0x08,
		Pending = 0x10, // patch 0031
	}

	/// <summary>
	/// strong name constants of spell line used in the world (poison, proc ect ...)
	/// </summary>
	public abstract class GlobalSpellsLines
	{
		public const string Combat_Styles_Effect = "Combat Style Effects";
		public const string Mundane_Poisons = "Mundane Poisons";
		public const string Reserved_Spells = "Reserved Spells"; // Masterlevels
		public const string SiegeWeapon_Spells = "SiegeWeapon Spells";
		public const string Item_Effects = "Item Effects";
		public const string Potions_Effects = "Potions";
		public const string Mob_Spells = "Mob Spells";
		public const string Character_Abilities = "Character Abilities"; // dirty tricks, flurry ect...
		public const string Item_Spells = "Item Spells";	// Combine scroll etc.
		public const string Champion_Lines_StartWith = "Champion ";
        public const string Realm_Spells = "Realm Spells"; // Resurrection illness, Speed of the realm
    }

	public static class GlobalConstants
	{
		private static readonly Dictionary<GameLiving.eAttackResult, byte> AttackResultByte = new Dictionary<GameLiving.eAttackResult, byte>()
	    {
			{GameLiving.eAttackResult.Missed, 0},
			{GameLiving.eAttackResult.Parried, 1},
			{GameLiving.eAttackResult.Blocked, 2},
			{GameLiving.eAttackResult.Evaded, 3},
			{GameLiving.eAttackResult.Fumbled, 4},
			{GameLiving.eAttackResult.HitUnstyled, 10},
			{GameLiving.eAttackResult.HitStyle, 11},
			{GameLiving.eAttackResult.Any, 20},
	    };
		
		public static byte GetAttackResultByte(GameLiving.eAttackResult attResult)
		{
			if (AttackResultByte.ContainsKey(attResult))
			{
				return AttackResultByte[attResult];
			}
			
			return 0;
		}
		
		public static bool IsExpansionEnabled(int expansion)
		{
			bool enabled = true;
			foreach (string ex in Util.SplitCSV(ServerProperties.Properties.DISABLED_EXPANSIONS, true))
			{
				int exNum = 0;
				if (int.TryParse(ex, out exNum))
				{
					if (exNum == expansion)
					{
						enabled = false;
						break;
					}
				}
			}

			return enabled;
		}


		public static string StatToName(eStat stat)
		{
			switch (stat)
			{
				case eStat.STR:
					return "Strength";
				case eStat.DEX:
					return "Dexterity";
				case eStat.CON:
					return "Constitution";
				case eStat.QUI:
					return "Quickness";
				case eStat.INT:
					return "Intelligence";
				case eStat.PIE:
					return "Piety";
				case eStat.EMP:
					return "Empathy";
				case eStat.CHR:
					return "Charisma";
			}

			return "Unknown";
		}

		/// <summary>
		/// Check an Object_Type to determine if it's a Bow weapon
		/// </summary>
		/// <param name="objectType"></param>
		/// <returns></returns>
		public static bool IsBowWeapon(eObjectType objectType)
		{
			return (objectType == eObjectType.CompositeBow || objectType == eObjectType.Longbow || objectType == eObjectType.RecurvedBow);
		}
		/// <summary>
		/// Check an Object_Type to determine if it's a weapon
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsWeapon(int objectTypeID)
		{
			if ((objectTypeID >= 1 && objectTypeID <= 28) || objectTypeID == (int)eObjectType.Shield) return true;
			return false;
		}
		/// <summary>
		/// Check an Object_Type to determine if it's armor
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsArmor(int objectTypeID)
		{
			if (objectTypeID >= 32 && objectTypeID <= 38) return true;
			return false;
		}
		/// <summary>
		/// Offensive, Defensive, or Positional
		/// </summary>
		public static string StyleOpeningTypeToName(int openingType)
		{
			return Enum.GetName(typeof(Styles.Style.eOpening), openingType);
		}
		/// <summary>
		/// Position, Back, Side, Front
		/// </summary>
		public static string StyleOpeningPositionToName(int openingRequirement)
		{
			return Enum.GetName(typeof(Styles.Style.eOpeningPosition), openingRequirement);
		}
		/// <summary>
		/// Attack Result. Any, Miss, Hit, Parry, Block, Evade, Fumble, Style.
		/// </summary>
		public static string StyleAttackResultToName(int attackResult)
		{
			return Enum.GetName(typeof(Styles.Style.eAttackResultRequirement), attackResult);
		}

		public static string InstrumentTypeToName(int instrumentTypeID)
		{
			return Enum.GetName(typeof(eInstrumentType), instrumentTypeID);
		}

		public static string AmmunitionTypeToDamageName(int ammutype)
		{
			ammutype &= 0x3;
			switch (ammutype)
			{
					case 1: return "medium";
					case 2: return "heavy";
					case 3: return "X-heavy";
			}
			return "light";
		}

		public static string AmmunitionTypeToRangeName(int ammutype)
		{
			ammutype = (ammutype >> 2) & 0x3;
			switch (ammutype)
			{
					case 1: return "medium";
					case 2: return "long";
					case 3: return "X-long";
			}
			return "short";
		}

		public static string AmmunitionTypeToAccuracyName(int ammutype)
		{
			ammutype = (ammutype >> 4) & 0x3;
			switch (ammutype)
			{
					case 1: return "normal";
					case 2: return "improved";
					case 3: return "enhanced";
			}
			return "reduced";
		}

		public static string ShieldTypeToName(int shieldTypeID)
		{
			return Enum.GetName(typeof(ShieldLevel), shieldTypeID);
		}

		public static string ArmorLevelToName(int armorLevel, eRealm realm)
		{
			switch (realm)
			{
				case eRealm.Albion:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Chain: return "chain";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Plate: return "plate";
								case ArmorLevel.Studded: return "studded";
								default: return "undefined";
						}
					}
				case eRealm.Midgard:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Chain: return "chain";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Studded: return "studded";
								default: return "undefined";
						}
					}
				case eRealm.Hibernia:
					{
						switch (armorLevel)
						{
								case ArmorLevel.Cloth: return "cloth";
								case ArmorLevel.Scale: return "scale";
								case ArmorLevel.Leather: return "leather";
								case ArmorLevel.Reinforced: return "reinforced";
								default: return "undefined";
						}
					}
					default: return "undefined";
			}
		}

		public static string WeaponDamageTypeToName(int weaponDamageTypeID)
		{
			return Enum.GetName(typeof(eWeaponDamageType), weaponDamageTypeID);
		}

		public static string NameToShortName(string name)
		{
			string[] values = name.Trim().ToLower().Split(' ');
			for (int i = 0; i < values.Length; i++)
			{
				if (values[i].Length == 0) continue;
				if (i > 0 && values[i] == "of")
					return values[i - 1];
			}
			return values[values.Length - 1];
		}

		public static string ItemHandToName(int handFlag)
		{
			if (handFlag == 1) return "twohanded";
			if (handFlag == 2) return "lefthand";
			return "both";
		}

		public static string ObjectTypeToName(int objectTypeID)
		{
			switch (objectTypeID)
			{
					case 0: return "generic (item)";
					case 1: return "generic (weapon)";
					case 2: return "crushing (weapon)";
					case 3: return "slashing (weapon)";
					case 4: return "thrusting (weapon)";
					case 5: return "fired (weapon)";
					case 6: return "twohanded (weapon)";
					case 7: return "polearm (weapon)";
					case 8: return "staff (weapon)";
					case 9: return "longbow (weapon)";
					case 10: return "crossbow (weapon)";
					case 11: return "sword (weapon)";
					case 12: return "hammer (weapon)";
					case 13: return "axe (weapon)";
					case 14: return "spear (weapon)";
					case 15: return "composite bow (weapon)";
					case 16: return "thrown (weapon)";
					case 17: return "left axe (weapon)";
					case 18: return "recurve bow (weapon)";
					case 19: return "blades (weapon)";
					case 20: return "blunt (weapon)";
					case 21: return "piercing (weapon)";
					case 22: return "large (weapon)";
					case 23: return "celtic spear (weapon)";
					case 24: return "flexible (weapon)";
					case 25: return "hand to hand (weapon)";
					case 26: return "scythe (weapon)";
					case 27: return "fist wraps (weapon)";
					case 28: return "mauler staff (weapon)";
					case 31: return "generic (armor)";
					case 32: return "cloth (armor)";
					case 33: return "leather (armor)";
					case 34: return "studded leather (armor)";
					case 35: return "chain (armor)";
					case 36: return "plate (armor)";
					case 37: return "reinforced (armor)";
					case 38: return "scale (armor)";
					case 41: return "magical (item)";
					case 42: return "shield (armor)";
					case 43: return "arrow (item)";
					case 44: return "bolt (item)";
					case 45: return "instrument (item)";
					case 46: return "poison (item)";
					case 47: return "alchemy tincture";
					case 48: return "spellcrafting gem";
					case 49: return "garden object";
					case 50: return "house wall object";
					case 51: return "house floor object";
					case 53: return "house npc";
					case 54: return "house vault";
					case 55: return "house crafting object";
					case 68: return "house bindstone";
			}
			return "unknown (item)";
		}

		//This method translates an InventoryTypeID to a string
		public static string SlotToName(int slotID)
		{
			switch (slotID)
			{
				case 0x0A: return "righthand";
				case 0x0B: return "lefthand";
				case 0x0C: return "twohanded";
				case 0x0D: return "distance";
				case 0x15: return "head";
				case 0x16: return "hand";
				case 0x17: return "feet";
				case 0x18: return "jewel";
				case 0x19: return "torso";
				case 0x1A: return "cloak";
				case 0x1B: return "legs";
				case 0x1C: return "arms";
				case 0x1D: return "neck";
				case 0x20: return "belt";
				case 0x21: return "leftbracer";
				case 0x22: return "rightbracer";
				case 0x23: return "leftring";
				case 0x24: return "rightring";
				case 0x25: return "mythirian";
				case 96: return "leftfront saddlebag";
				case 97: return "rightfront saddlebag";
				case 98: return "leftrear saddlebag";
				case 99: return "rightrear saddlebag";
			}
			return "generic inventory";
		}

		//This method translates a string to an InventorySlotID
		public static byte NameToSlot(string name)
		{
			switch (name)
			{
					//Horses
					case "mount": return 0xA9;
					//Righthand Weapon Type
					case "righthand": return 0x0A;
					case "right": return 0x0A;
					case "ri": return 0x0A;

					//Lefthand Weapon Type
					case "lefthand": return 0x0B;
					case "left": return 0x0B;
					case "lef": return 0x0B;

					//Twohanded Weapon Type
					case "twohanded": return 0x0C;
					case "two": return 0x0C;
					case "tw": return 0x0C;

					//Distance Weapon Type
					case "distance": return 0x0D;
					case "dist": return 0x0D;
					case "di": return 0x0D;
					case "bow": return 0x0D;
					case "crossbow": return 0x0D;
					case "longbow": return 0x0D;
					case "throwing": return 0x0D;
					case "thrown": return 0x0D;
					case "fire": return 0x0D;
					case "firing": return 0x0D;

					//Head Armor Type
					case "head": return 0x15;
					case "helm": return 0x15;
					case "he": return 0x15;

					//Hand Armor Type
					case "hands": return 0x16;
					case "hand": return 0x16;
					case "ha": return 0x16;
					case "gloves": return 0x16;
					case "glove": return 0x16;
					case "gl": return 0x16;

					//Boot Armor Type
					case "boots": return 0x17;
					case "boot": return 0x17;
					case "boo": return 0x17;
					case "feet": return 0x17;
					case "fe": return 0x17;
					case "foot": return 0x17;
					case "fo": return 0x17;

					//Jewel Type
					case "jewels": return 0x18;
					case "jewel": return 0x18;
					case "je": return 0x18;
					case "j": return 0x18;
					case "gems": return 0x18;
					case "gem": return 0x18;
					case "gemstone": return 0x18;
					case "stone": return 0x18;

					//Body Armor Type
					case "torso": return 0x19;
					case "to": return 0x19;
					case "body": return 0x19;
					case "bod": return 0x19;
					case "robes": return 0x19;
					case "robe": return 0x19;
					case "ro": return 0x19;

					//Cloak Armor Type
					case "cloak": return 0x1A;
					case "cloa": return 0x1A;
					case "clo": return 0x1A;
					case "cl": return 0x1A;
					case "cape": return 0x1A;
					case "ca": return 0x1A;
					case "gown": return 0x1A;
					case "mantle": return 0x1A;
					case "ma": return 0x1A;
					case "shawl": return 0x1A;

					//Leg Armor Type
					case "legs": return 0x1B;
					case "leg": return 0x1B;

					//Arms Armor Type
					case "arms": return 0x1C;
					case "arm": return 0x1C;
					case "ar": return 0x1C;

					//Neck Armor Type
					case "neck": return 0x1D;
					case "ne": return 0x1D;
					case "scruff": return 0x1D;
					case "nape": return 0x1D;
					case "throat": return 0x1D;
					case "necklace": return 0x1D;
					case "necklet": return 0x1D;

					//Belt Armor Type
					case "belt": return 0x20;
					case "b": return 0x20;
					case "girdle": return 0x20;
					case "waistbelt": return 0x20;

					//Left Bracers Type
					case "leftbracers": return 0x21;
					case "leftbracer": return 0x21;
					case "leftbr": return 0x21;
					case "lbracers": return 0x21;
					case "lbracer": return 0x21;
					case "leb": return 0x21;
					case "lbr": return 0x21;
					case "lb": return 0x21;

					//Right Bracers Type
					case "rightbracers": return 0x22;
					case "rightbracer": return 0x22;
					case "rightbr": return 0x22;
					case "rbracers": return 0x22;
					case "rbracer": return 0x22;
					case "rib": return 0x22;
					case "rbr": return 0x22;
					case "rb": return 0x22;

					//Left Ring Type
					case "leftrings": return 0x23;
					case "leftring": return 0x23;
					case "leftr": return 0x23;
					case "lrings": return 0x23;
					case "lring": return 0x23;
					case "lri": return 0x23;
					case "ler": return 0x23;
					case "lr": return 0x23;

					//Right Ring Type
					case "rightrings": return 0x24;
					case "rightring": return 0x24;
					case "rightr": return 0x24;
					case "rrings": return 0x24;
					case "rring": return 0x24;
					case "rri": return 0x24;
					case "rir": return 0x24;
					case "rr": return 0x24;

					//Mythirians
					case "myth": return 0x25;
					case "mythirian": return 0x25;
					case "mythirians": return 0x25;
			}
			return 0x00;
		}
		public static string RealmToName(eRealm realm)
		{
			switch (realm)
			{
					case eRealm.None: return "None";
					case eRealm.Albion: return "Albion";
					case eRealm.Midgard: return "Midgard";
					case eRealm.Hibernia: return "Hibernia";
					case eRealm.Door: return "Door";
					default: return "";
			}
		}
		public static int EmblemOfRealm(eRealm realm)
		{
			switch (realm)
			{
					case eRealm.None: return 0;
					case eRealm.Albion: return 464;
					case eRealm.Midgard: return 465;
					case eRealm.Hibernia: return 466;
					default: return 0;
			}
		}

		public static string PropertyToName(eProperty property)
		{
			switch (property)
			{
					case eProperty.Strength: return "Strength";
					case eProperty.Dexterity: return "Dexterity";
					case eProperty.Constitution: return "Constitution";
					case eProperty.Quickness: return "Quickness";
					case eProperty.Intelligence: return "Intelligence";
					case eProperty.Piety: return "Piety";
					case eProperty.Empathy: return "Empathy";
					case eProperty.Charisma: return "Charisma";
					case eProperty.Resist_Body: return "Body Resist";
					case eProperty.Resist_Cold: return "Cold Resist";
					case eProperty.Resist_Crush: return "Crush Resist";
					case eProperty.Resist_Energy: return "Energy Resist";
					case eProperty.Resist_Heat: return "Heat Resist";
					case eProperty.Resist_Matter: return "Matter Resist";
					case eProperty.Resist_Slash: return "Slash Resist";
					case eProperty.Resist_Spirit: return "Spirit Resist";
					case eProperty.Resist_Thrust: return "Thrust Resist";
					case eProperty.Resist_Natural: return "Essence Resist";
					default: return "not implemented";
			}
		}

		public static string DamageTypeToName(eDamageType damage)
		{
			switch (damage)
			{
					case eDamageType.Body: return "Body";
					case eDamageType.Cold: return "Cold";
					case eDamageType.Crush: return "Crush";
					case eDamageType.Energy: return "Energy";
					case eDamageType.Falling: return "Falling";
					case eDamageType.Heat: return "Heat";
					case eDamageType.Matter: return "Matter";
					case eDamageType.Natural: return "Natural";
					case eDamageType.Slash: return "Slash";
					case eDamageType.Spirit: return "Spirit";
					case eDamageType.Thrust: return "Thrust";
					default: return "unknown damagetype " + damage.ToString();
			}
		}

		public static string CraftLevelToCraftTitle(GameClient client, int craftLevel)
		{
			switch ((int)(craftLevel / 100))
			{
                case 0: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Helper");
                case 1: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.JuniorApprentice");
                case 2: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Apprentice");
                case 3: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Neophyte");
                case 4: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Assistant");
                case 5: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Junior");
                case 6: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Journeyman");
                case 7: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Senior");
                case 8: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Master");
                case 9: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Grandmaster");
                case 10: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.Legendary");
                case 11: return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.LegendaryGrandmaster");
			}
			if (craftLevel > 1100)
			{
                return LanguageMgr.GetTranslation(client.Account.Language, "CraftLevelToCraftTitle.LegendaryGrandmaster");
			}
			return "";
		}

		public static eRealm GetBonusRealm(eProperty bonus)
		{
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Albion))
				return eRealm.Albion;
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Midgard))
				return eRealm.Midgard;
			if (SkillBase.CheckPropertyType(bonus, ePropertyType.Hibernia))
				return eRealm.Hibernia;
			return eRealm.None;
		}

		public static eRealm[] GetItemTemplateRealm(ItemTemplate item)
		{
			switch ((eObjectType)item.Object_Type)
			{
					//Albion
				case eObjectType.CrushingWeapon:
				case eObjectType.SlashingWeapon:
				case eObjectType.ThrustWeapon:
				case eObjectType.TwoHandedWeapon:
				case eObjectType.PolearmWeapon:
				case eObjectType.Staff:
				case eObjectType.Longbow:
				case eObjectType.Crossbow:
				case eObjectType.Flexible:
				case eObjectType.Plate:
				case eObjectType.Bolt:
					return new eRealm[] { eRealm.Albion };

					//Midgard
				case eObjectType.Sword:
				case eObjectType.Hammer:
				case eObjectType.Axe:
				case eObjectType.Spear:
				case eObjectType.CompositeBow:
				case eObjectType.Thrown:
				case eObjectType.LeftAxe:
				case eObjectType.HandToHand:
					return new eRealm[] { eRealm.Midgard };

					//Hibernia
				case eObjectType.Fired:
				case eObjectType.RecurvedBow:
				case eObjectType.Blades:
				case eObjectType.Blunt:
				case eObjectType.Piercing:
				case eObjectType.LargeWeapons:
				case eObjectType.CelticSpear:
				case eObjectType.Scythe:
				case eObjectType.Reinforced:
				case eObjectType.Scale:
					return new eRealm[] { eRealm.Hibernia };

					//Special
				case eObjectType.Studded:
				case eObjectType.Chain:
					return new eRealm[] { eRealm.Albion, eRealm.Midgard };

				case eObjectType.Instrument:
					return new eRealm[] { eRealm.Albion, eRealm.Hibernia };

					//Common Armor
				case eObjectType.Cloth:
				case eObjectType.Leather:
					//Misc
				case eObjectType.GenericItem:
				case eObjectType.GenericWeapon:
				case eObjectType.GenericArmor:
				case eObjectType.Magical:
				case eObjectType.Shield:
				case eObjectType.Arrow:
				case eObjectType.Poison:
				case eObjectType.AlchemyTincture:
				case eObjectType.SpellcraftGem:
				case eObjectType.GardenObject:
				case eObjectType.SiegeBalista:
				case eObjectType.SiegeCatapult:
				case eObjectType.SiegeCauldron:
				case eObjectType.SiegeRam:
				case eObjectType.SiegeTrebuchet:
					break;
			}

			eRealm realm = eRealm.None;

			if (item.Bonus1Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus1Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus2Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus2Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus3Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus3Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus4Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus4Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus5Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus5Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus6Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus6Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus7Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus7Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus8Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus8Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus9Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus9Type)) != eRealm.None)
				return new eRealm[] { realm };

			if (item.Bonus10Type > 0 && (realm = GetBonusRealm((eProperty)item.Bonus10Type)) != eRealm.None)
				return new eRealm[] { realm };

			return new eRealm[] { realm };

		}

		public static byte GetSpecToInternalIndex(string name)
		{
			switch (name)
			{
					case Specs.Slash: return 0x01;
					case Specs.Thrust: return 0x02;
					case Specs.Parry: return 0x08;
					case Specs.Sword: return 0x0E;
					case Specs.Hammer: return 0x10;
					case Specs.Axe: return 0x11;
					case Specs.Left_Axe: return 0x12;
					case Specs.Stealth: return 0x13;
					case Specs.Spear: return 0x1A;
					case Specs.Mending: return 0x1D;
					case Specs.Augmentation: return 0x1E;
					case Specs.Crush: return 0x21;
					case Specs.Pacification: return 0x22;
					//				case Specs.Cave_Magic:      return 0x25; ?
					case Specs.Darkness: return 0x26;
					case Specs.Suppression: return 0x27;
					case Specs.Runecarving: return 0x2A;
					case Specs.Shields: return 0x2B;
					case Specs.Flexible: return 0x2E;
					case Specs.Staff: return 0x2F;
					case Specs.Summoning: return 0x30;
					case Specs.Stormcalling: return 0x32;
					case Specs.Beastcraft: return 0x3E;
					case Specs.Polearms: return 0x40;
					case Specs.Two_Handed: return 0x41;
					case Specs.Fire_Magic: return 0x42;
					case Specs.Wind_Magic: return 0x43;
					case Specs.Cold_Magic: return 0x44;
					case Specs.Earth_Magic: return 0x45;
					case Specs.Light: return 0x46;
					case Specs.Matter_Magic: return 0x47;
					case Specs.Body_Magic: return 0x48;
					case Specs.Spirit_Magic: return 0x49;
					case Specs.Mind_Magic: return 0x4A;
					case Specs.Void: return 0x4B;
					case Specs.Mana: return 0x4C;
					case Specs.Dual_Wield: return 0x4D;
					case Specs.CompositeBow: return 0x4E;
					case Specs.Battlesongs: return 0x52;
					case Specs.Enhancement: return 0x53;
					case Specs.Enchantments: return 0x54;
					case Specs.Rejuvenation: return 0x58;
					case Specs.Smite: return 0x59;
					case Specs.Longbow: return 0x5A;
					case Specs.Crossbow: return 0x5B;
					case Specs.Chants: return 0x61;
					case Specs.Instruments: return 0x62;
					case Specs.Blades: return 0x65;
					case Specs.Blunt: return 0x66;
					case Specs.Piercing: return 0x67;
					case Specs.Large_Weapons: return 0x68;
					case Specs.Mentalism: return 0x69;
					case Specs.Regrowth: return 0x6A;
					case Specs.Nurture: return 0x6B;
					case Specs.Nature: return 0x6C;
					case Specs.Music: return 0x6D;
					case Specs.Celtic_Dual: return 0x6E;
					case Specs.Celtic_Spear: return 0x70;
					case Specs.RecurveBow: return 0x71;
					case Specs.Valor: return 0x72;
					case Specs.Pathfinding: return 0x74;
					case Specs.Envenom: return 0x75;
					case Specs.Critical_Strike: return 0x76;
					case Specs.Deathsight: return 0x78;
					case Specs.Painworking: return 0x79;
					case Specs.Death_Servant: return 0x7A;
					case Specs.Soulrending: return 0x7B;
					case Specs.HandToHand: return 0x7C;
					case Specs.Scythe: return 0x7D;
					//				case Specs.Bone_Army:       return 0x7E; ?
					case Specs.Arboreal_Path: return 0x7F;
					case Specs.Creeping_Path: return 0x81;
					case Specs.Verdant_Path: return 0x82;
					case Specs.OdinsWill: return 0x85;
					case Specs.SpectralForce: return 0x86; // Spectral Guard ?
					case Specs.PhantasmalWail: return 0x87;
					case Specs.EtherealShriek: return 0x88;
					case Specs.ShadowMastery: return 0x89;
					case Specs.VampiiricEmbrace: return 0x8A;
					case Specs.Dementia: return 0x8B;
					case Specs.Witchcraft: return 0x8C;
					case Specs.Cursing: return 0x8D;
					case Specs.Hexing: return 0x8E;
					case Specs.Fist_Wraps: return 0x93;
					case Specs.Mauler_Staff: return 0x94;
					case Specs.SpectralGuard: return 0x95;
					case Specs.Archery : return 0x9B;
					default: return 0;
			}
		}
		
		// webdisplay enums: they are processed via /webdisplay command
		public enum eWebDisplay: byte
		{
			all 		= 0x00,
			position 	= 0x01,
			template	= 0x02,
			equipment	= 0x04,
			craft		= 0x08,			
		}
		
		#region AllowedClassesRaces
		/// <summary>
		/// All possible player races
		/// </summary>
		public static readonly Dictionary<eRace, Dictionary<eStat, int>> STARTING_STATS_DICT = new Dictionary<eRace, Dictionary<eStat, int>>()
		{ 
			{ eRace.Unknown, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Briton, new Dictionary<eStat, int>()			{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Avalonian, new Dictionary<eStat, int>()			{{eStat.STR, 45}, {eStat.CON, 45}, {eStat.DEX, 60}, {eStat.QUI, 70}, {eStat.INT, 80}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Highlander, new Dictionary<eStat, int>()		{{eStat.STR, 70}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Saracen, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 50}, {eStat.DEX, 80}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Norseman, new Dictionary<eStat, int>()			{{eStat.STR, 70}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Troll, new Dictionary<eStat, int>()				{{eStat.STR, 100}, {eStat.CON, 70}, {eStat.DEX, 35}, {eStat.QUI, 35}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Dwarf, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 80}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Kobold, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 50}, {eStat.DEX, 70}, {eStat.QUI, 70}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Celt, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 60}, {eStat.DEX, 60}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Firbolg, new Dictionary<eStat, int>()			{{eStat.STR, 90}, {eStat.CON, 60}, {eStat.DEX, 40}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 70}, {eStat.CHR, 60}, }},
			{ eRace.Elf, new Dictionary<eStat, int>()				{{eStat.STR, 40}, {eStat.CON, 40}, {eStat.DEX, 75}, {eStat.QUI, 75}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Lurikeen, new Dictionary<eStat, int>()			{{eStat.STR, 40}, {eStat.CON, 40}, {eStat.DEX, 80}, {eStat.QUI, 80}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Inconnu, new Dictionary<eStat, int>()			{{eStat.STR, 50}, {eStat.CON, 60}, {eStat.DEX, 70}, {eStat.QUI, 50}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Valkyn, new Dictionary<eStat, int>()			{{eStat.STR, 55}, {eStat.CON, 45}, {eStat.DEX, 65}, {eStat.QUI, 75}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Sylvan, new Dictionary<eStat, int>()			{{eStat.STR, 70}, {eStat.CON, 60}, {eStat.DEX, 55}, {eStat.QUI, 45}, {eStat.INT, 70}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.HalfOgre, new Dictionary<eStat, int>()			{{eStat.STR, 90}, {eStat.CON, 70}, {eStat.DEX, 40}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Frostalf, new Dictionary<eStat, int>()			{{eStat.STR, 55}, {eStat.CON, 55}, {eStat.DEX, 55}, {eStat.QUI, 60}, {eStat.INT, 60}, {eStat.PIE, 75}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.Shar, new Dictionary<eStat, int>()				{{eStat.STR, 60}, {eStat.CON, 80}, {eStat.DEX, 50}, {eStat.QUI, 50}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.AlbionMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.MidgardMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
			{ eRace.HiberniaMinotaur, new Dictionary<eStat, int>()	{{eStat.STR, 80}, {eStat.CON, 70}, {eStat.DEX, 50}, {eStat.QUI, 40}, {eStat.INT, 60}, {eStat.PIE, 60}, {eStat.EMP, 60}, {eStat.CHR, 60}, }},
		};
		/// <summary>
		/// All possible player starting classes
		/// </summary>
		public static readonly Dictionary<eRealm, List<eCharacterClass>> STARTING_CLASSES_DICT = new Dictionary<eRealm, List<eCharacterClass>>()
		{
			// pre 1.93
			{eRealm.Albion, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
				// post 1.93
				eCharacterClass.Paladin, 		// Paladin = 1,
				eCharacterClass.Armsman, 		// Armsman = 2,
				eCharacterClass.Scout, 	    // Scout = 3,
				eCharacterClass.Minstrel, 	    // Minstrel = 4,
				eCharacterClass.Theurgist, 	// Theurgist = 5,
				eCharacterClass.Cleric, 		// Cleric = 6,
				eCharacterClass.Wizard, 	    // Wizard = 7,
				eCharacterClass.Sorcerer, 		// Sorcerer = 8,
				eCharacterClass.Infiltrator, 	// Infiltrator = 9,
				eCharacterClass.Friar, 		// Friar = 10,
				eCharacterClass.Mercenary, 	// Mercenary = 11,
				eCharacterClass.Necromancer, 	// Necromancer = 12,
				eCharacterClass.Cabalist, 		// Cabalist = 13,
				eCharacterClass.Fighter, 		// Fighter = 14,
				eCharacterClass.Elementalist, 	// Elementalist = 15,
				eCharacterClass.Acolyte, 		// Acolyte = 16,
				eCharacterClass.AlbionRogue, 	// AlbionRogue = 17,
				eCharacterClass.Mage, 			// Mage = 18,
				eCharacterClass.Reaver, 		// Reaver = 19,
				eCharacterClass.Disciple,		// Disciple = 20,
				eCharacterClass.Heretic, 		// Heretic = 33,
				eCharacterClass.MaulerAlb		// Mauler_Alb = 60,
			}},
			{eRealm.Midgard, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Thane, 		// Thane = 21,
				eCharacterClass.Warrior, 		// Warrior = 22,
				eCharacterClass.Shadowblade, 	// Shadowblade = 23,
				eCharacterClass.Skald, 		// Skald = 24,
				eCharacterClass.Hunter, 	    // Hunter = 25,
				eCharacterClass.Healer, 		// Healer = 26,
				eCharacterClass.Spiritmaster,  // Spiritmaster = 27,
				eCharacterClass.Shaman, 		// Shaman = 28,
				eCharacterClass.Runemaster, 	// Runemaster = 29,
				eCharacterClass.Bonedancer, 	// Bonedancer = 30,
				eCharacterClass.Berserker, 	// Berserker = 31,
				eCharacterClass.Savage, 		// Savage = 32,
				eCharacterClass.Valkyrie, 		// Valkyrie = 34,
				eCharacterClass.Viking, 		// Viking = 35,
				eCharacterClass.Mystic, 		// Mystic = 36,
				eCharacterClass.Seer, 			// Seer = 37,
				eCharacterClass.MidgardRogue,	// MidgardRogue = 38,
				eCharacterClass.Warlock, 		// Warlock = 59,
				eCharacterClass.MaulerMid		// Mauler_Mid = 61,
			}},
			{eRealm.Hibernia, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
				// post 1.93
				eCharacterClass.Bainshee, 		// Bainshee = 39,
				eCharacterClass.Eldritch, 		// Eldritch = 40,
				eCharacterClass.Enchanter, 	// Enchanter = 41,
				eCharacterClass.Mentalist, 	// Mentalist = 42,
				eCharacterClass.Blademaster, 	// Blademaster = 43,
				eCharacterClass.Hero, 		    // Hero = 44,
				eCharacterClass.Champion, 		// Champion = 45,
				eCharacterClass.Warden, 	    // Warden = 46,
				eCharacterClass.Druid, 	    // Druid = 47,
				eCharacterClass.Bard, 	        // Bard = 48,
				eCharacterClass.Nightshade, 	// Nightshade = 49,
				eCharacterClass.Ranger, 		// Ranger = 50,
				eCharacterClass.Magician, 		// Magician = 51,
				eCharacterClass.Guardian, 		// Guardian = 52,
				eCharacterClass.Naturalist, 	// Naturalist = 53,
				eCharacterClass.Stalker, 		// Stalker = 54,
				eCharacterClass.Animist, 		// Animist = 55,
				eCharacterClass.Valewalker, 	// Valewalker = 56,
				eCharacterClass.Forester, 		// Forester = 57,
				eCharacterClass.Vampiir, 		// Vampiir = 58,
				eCharacterClass.MaulerHib	 	// Mauler_Hib = 62,
			}},
		};
		
		/// <summary>
		/// Allowed Classes for Each Races
		/// </summary>
		public static Dictionary<eRace, List<eCharacterClass>> RACES_CLASSES_DICT = new Dictionary<eRace, List<eCharacterClass>>()
		{
			{eRace.Unknown, new List<eCharacterClass>()},
			// pre 1.93
			{eRace.Briton, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
				// post 1.93
				eCharacterClass.Armsman,
				eCharacterClass.Reaver,
				eCharacterClass.Mercenary,
				eCharacterClass.Paladin,
				eCharacterClass.Cleric,
				eCharacterClass.Heretic,
				eCharacterClass.Friar,
				eCharacterClass.Sorcerer,
				eCharacterClass.Cabalist,
				eCharacterClass.Theurgist,
				eCharacterClass.Necromancer,
				eCharacterClass.MaulerAlb,
				eCharacterClass.Wizard,
				eCharacterClass.Minstrel,
				eCharacterClass.Infiltrator,
				eCharacterClass.Scout,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte,
				eCharacterClass.Mage,
				eCharacterClass.Elementalist,
				eCharacterClass.AlbionRogue,
				eCharacterClass.Disciple
				}},
			{eRace.Avalonian, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist,
				// post 1.93
				eCharacterClass.Paladin,
				eCharacterClass.Cleric,
				eCharacterClass.Wizard,
				eCharacterClass.Theurgist,
				eCharacterClass.Armsman,
				eCharacterClass.Mercenary,
				eCharacterClass.Sorcerer,
				eCharacterClass.Cabalist,
				eCharacterClass.Heretic,
				eCharacterClass.Friar,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte,
				eCharacterClass.Mage,
				eCharacterClass.Elementalist
				}},
			{eRace.Highlander, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.AlbionRogue,
				// post 1.93
				eCharacterClass.Armsman,
				eCharacterClass.Mercenary,
				eCharacterClass.Paladin,
				eCharacterClass.Cleric,
				eCharacterClass.Minstrel,
				eCharacterClass.Scout,
				eCharacterClass.Friar,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte,
				eCharacterClass.AlbionRogue
				}},
			{eRace.Saracen, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Mage, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
				// post 1.93
				eCharacterClass.Sorcerer,
				eCharacterClass.Cabalist,
				eCharacterClass.Paladin,
				eCharacterClass.Reaver,
				eCharacterClass.Mercenary,
				eCharacterClass.Armsman,
				eCharacterClass.Infiltrator,
				eCharacterClass.Minstrel,
				eCharacterClass.Scout,
				eCharacterClass.Necromancer,
				eCharacterClass.Fighter,
				eCharacterClass.Mage,
				eCharacterClass.AlbionRogue,
				eCharacterClass.Disciple
				}},
			
			{eRace.Norseman, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Healer,
				eCharacterClass.Warrior,
				eCharacterClass.Berserker,
				eCharacterClass.Thane,
				eCharacterClass.Warlock,
				eCharacterClass.Skald,
				eCharacterClass.Valkyrie,
				eCharacterClass.Spiritmaster,
				eCharacterClass.Runemaster,
				eCharacterClass.Savage,
				eCharacterClass.MaulerMid,
				eCharacterClass.Shadowblade,
				eCharacterClass.Hunter,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.Seer,
				eCharacterClass.MidgardRogue
				}},
			{eRace.Troll, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer,
				// post 1.93
				eCharacterClass.Berserker,
				eCharacterClass.Warrior,
				eCharacterClass.Savage,
				eCharacterClass.Thane,
				eCharacterClass.Skald,
				eCharacterClass.Bonedancer,
				eCharacterClass.Shaman,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.Seer
				}},
			{eRace.Dwarf, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Healer,
				eCharacterClass.Thane,
				eCharacterClass.Berserker,
				eCharacterClass.Warrior,
				eCharacterClass.Savage,
				eCharacterClass.Skald,
				eCharacterClass.Valkyrie,
				eCharacterClass.Runemaster,
				eCharacterClass.Hunter,
				eCharacterClass.Shaman,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.Seer,
				eCharacterClass.MidgardRogue
				}},
			{eRace.Kobold, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Shaman,
				eCharacterClass.Warrior,
				eCharacterClass.Skald,
				eCharacterClass.Savage,
				eCharacterClass.Runemaster,
				eCharacterClass.Spiritmaster,
				eCharacterClass.Bonedancer,
				eCharacterClass.Warlock,
				eCharacterClass.Hunter,
				eCharacterClass.Shadowblade,
				eCharacterClass.MaulerMid,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.Seer,
				eCharacterClass.MidgardRogue
				}},
				
			{eRace.Celt, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
				// post 1.93
				eCharacterClass.Bard,
				eCharacterClass.Druid,
				eCharacterClass.Warden,
				eCharacterClass.Blademaster,
				eCharacterClass.Hero,
				eCharacterClass.Vampiir,
				eCharacterClass.Champion,
				eCharacterClass.MaulerHib,
				eCharacterClass.Mentalist,
				eCharacterClass.Bainshee,
				eCharacterClass.Ranger,
				eCharacterClass.Animist,
				eCharacterClass.Valewalker,
				eCharacterClass.Nightshade,
				eCharacterClass.Guardian,
				eCharacterClass.Stalker,
				eCharacterClass.Naturalist,
				eCharacterClass.Magician,
				eCharacterClass.Forester
				}},
			{eRace.Firbolg, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Naturalist, eCharacterClass.Forester,
				// post 1.93
				eCharacterClass.Bard,
				eCharacterClass.Druid,
				eCharacterClass.Warden,
				eCharacterClass.Hero,
				eCharacterClass.Blademaster,
				eCharacterClass.Animist,
				eCharacterClass.Valewalker,
				eCharacterClass.Guardian,
				eCharacterClass.Naturalist,
				eCharacterClass.Forester
				}},
			{eRace.Elf, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
				// post 1.93
				eCharacterClass.Blademaster,
				eCharacterClass.Champion,
				eCharacterClass.Ranger,
				eCharacterClass.Nightshade,
				eCharacterClass.Bainshee,
				eCharacterClass.Enchanter,
				eCharacterClass.Eldritch,
				eCharacterClass.Mentalist,
				eCharacterClass.Guardian,
				eCharacterClass.Stalker,
				eCharacterClass.Magician
				}},
			{eRace.Lurikeen, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
				// post 1.93
				eCharacterClass.Hero,
				eCharacterClass.Champion,
				eCharacterClass.Vampiir,
				eCharacterClass.Eldritch,
				eCharacterClass.Enchanter,
				eCharacterClass.Mentalist,
				eCharacterClass.Bainshee,
				eCharacterClass.Nightshade,
				eCharacterClass.Ranger,
				eCharacterClass.MaulerHib,
				eCharacterClass.Guardian,
				eCharacterClass.Stalker,
				eCharacterClass.Magician
				}},
			
			{eRace.Inconnu, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
				// post 1.93
				eCharacterClass.Reaver,
				eCharacterClass.Sorcerer,
				eCharacterClass.Cabalist,
				eCharacterClass.Heretic,
				eCharacterClass.Necromancer,
				eCharacterClass.Armsman,
				eCharacterClass.Mercenary,
				eCharacterClass.Infiltrator,
				eCharacterClass.Scout,
				eCharacterClass.MaulerAlb,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte,
				eCharacterClass.Mage,
				eCharacterClass.AlbionRogue,
				eCharacterClass.Disciple
				}},
			
			{eRace.Valkyn, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Savage,
				eCharacterClass.Berserker,
				eCharacterClass.Bonedancer,
				eCharacterClass.Warrior,
				eCharacterClass.Shadowblade,
				eCharacterClass.Hunter,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.MidgardRogue
				}},
			
			{eRace.Sylvan, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Naturalist, eCharacterClass.Forester,
				// post 1.93
				eCharacterClass.Animist,
				eCharacterClass.Druid,
				eCharacterClass.Valewalker,
				eCharacterClass.Hero,
				eCharacterClass.Warden,
				eCharacterClass.Guardian,
				eCharacterClass.Naturalist,
				eCharacterClass.Forester
				}},
			
			{eRace.HalfOgre, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Mage, eCharacterClass.Elementalist,
				// post 1.93
				eCharacterClass.Wizard,
				eCharacterClass.Theurgist,
				eCharacterClass.Cabalist,
				eCharacterClass.Sorcerer,
				eCharacterClass.Mercenary,
				eCharacterClass.Armsman,
				eCharacterClass.Fighter,
				eCharacterClass.Mage,
				eCharacterClass.Elementalist
				}},
			
			{eRace.Frostalf, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Healer,
				eCharacterClass.Shaman,
				eCharacterClass.Thane,
				eCharacterClass.Spiritmaster,
				eCharacterClass.Runemaster,
				eCharacterClass.Warlock,
				eCharacterClass.Valkyrie,
				eCharacterClass.Hunter,
				eCharacterClass.Shadowblade,
				eCharacterClass.Viking,
				eCharacterClass.Mystic,
				eCharacterClass.Seer,
				eCharacterClass.MidgardRogue
				}},
			
			{eRace.Shar, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Magician,
				// post 1.93
				eCharacterClass.Champion,
				eCharacterClass.Hero,
				eCharacterClass.Blademaster,
				eCharacterClass.Vampiir,
				eCharacterClass.Ranger,
				eCharacterClass.Mentalist,
				eCharacterClass.Guardian,
				eCharacterClass.Stalker,
				eCharacterClass.Magician
				}},
			
			{eRace.AlbionMinotaur, new List<eCharacterClass>() {eCharacterClass.Fighter, eCharacterClass.Acolyte, eCharacterClass.Mage, eCharacterClass.Elementalist, eCharacterClass.AlbionRogue, eCharacterClass.Disciple,
				// post 1.93
				eCharacterClass.Heretic,
				eCharacterClass.MaulerAlb,
				eCharacterClass.Armsman,
				eCharacterClass.Mercenary,
				eCharacterClass.Fighter,
				eCharacterClass.Acolyte
				}},
			
			{eRace.MidgardMinotaur, new List<eCharacterClass>() {eCharacterClass.Viking, eCharacterClass.Mystic, eCharacterClass.Seer, eCharacterClass.MidgardRogue,
				// post 1.93
				eCharacterClass.Berserker,
				eCharacterClass.MaulerMid,
				eCharacterClass.Thane,
				eCharacterClass.Viking,
				eCharacterClass.Warrior
				}},
			
			{eRace.HiberniaMinotaur, new List<eCharacterClass>() {eCharacterClass.Guardian, eCharacterClass.Stalker, eCharacterClass.Naturalist, eCharacterClass.Magician, eCharacterClass.Forester,
				// post 1.93
				eCharacterClass.Hero,
				eCharacterClass.Blademaster,
				eCharacterClass.MaulerHib,
				eCharacterClass.Warden,
				eCharacterClass.Guardian,
				eCharacterClass.Naturalist
				}},
		};
		
		/// <summary>
		/// Race to Gender Constraints
		/// </summary>
		public static readonly Dictionary<eRace, eGender> RACE_GENDER_CONSTRAINTS_DICT = new Dictionary<eRace, eGender>()
		{
			{eRace.AlbionMinotaur, eGender.Male},
			{eRace.MidgardMinotaur, eGender.Male},
			{eRace.HiberniaMinotaur, eGender.Male},
		};
		
		/// <summary>
		/// Class to Gender Constraints
		/// </summary>
		public static readonly Dictionary<eCharacterClass, eGender> CLASS_GENDER_CONSTRAINTS_DICT = new Dictionary<eCharacterClass, eGender>()
		{
			{eCharacterClass.Valkyrie, eGender.Female},
			{eCharacterClass.Bainshee, eGender.Female},
		};
		
		/// <summary>
		/// Holds all realm rank names
		/// sirru mod 20.11.06
		/// </summary>
		public static string[, ,] REALM_RANK_NAMES = new string[,,]
		{
			// Albion
			{
				// Male
				{
					"Guardian",
					"Warder",
					"Myrmidon",
					"Gryphon Knight",
					"Eagle Knight",
					"Phoenix Knight",
					"Alerion Knight",
					"Unicorn Knight",
					"Lion Knight",
					"Dragon Knight",
					"Lord",
					"Baronet",
					"Baron",
					"Arch Duke"
				}
				,
				// Female
				{
					"Guardian",
					"Warder",
					"Myrmidon",
					"Gryphon Knight",
					"Eagle Knight",
					"Phoenix Knight",
					"Alerion Knight",
					"Unicorn Knight",
					"Lion Knight",
					"Dragon Knight",
					"Lady",
					"Baronetess",
					"Baroness",
					"Arch Duchess",
				}
			}
			,
			// Midgard
			{
				// Male
				{
					"Skiltvakten",
					"Isen Vakten",
					"Flammen Vakten",
					"Elding Vakten",
					"Stormur Vakten",
					"Isen Herra",
					"Flammen Herra",
					"Elding Herra",
					"Stormur Herra",
					"Einherjar",
					"Herra",
					"Hersir",
					"Vicomte",
					"Stor Jarl"
				}
				,
				// Female
				{
					"Skiltvakten",
					"Isen Vakten",
					"Flammen Vakten",
					"Elding Vakten",
					"Stormur Vakten",
					"Isen Fru",
					"Flammen Fru",
					"Elding Fru",
					"Stormur Fru",
					"Einherjar",
					"Fru",
					"Baronsfru",
					"Vicomtessa",
					"Stor Hurfru",
				}
			}
			,
			// Hibernia
			{
				// Male
				{
					"Savant",
					"Cosantoir",
					"Brehon",
					"Grove Protector",
					"Raven Ardent",
					"Silver Hand",
					"Thunderer",
					"Gilded Spear",
					"Tiarna",
					"Emerald Ridere",
					"Barun",
					"Ard Tiarna",
					"Ciann Cath",
					"Ard Diuc"
				}
				,
				// Female
				{
					"Savant",
					"Cosantoir",
					"Brehon",
					"Grove Protector",
					"Raven Ardent",
					"Silver Hand",
					"Thunderer",
					"Gilded Spear",
					"Bantiarna",
					"Emerald Ridere",
					"Banbharun",
					"Ard Bantiarna",
					"Ciann Cath",
					"Ard Bandiuc"
				}
			}
		};
		
		/// <summary>
		/// Translate Given Race/Gender Combo in Client Language
		/// </summary>
		/// <param name="client"></param>
		/// <param name="race"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		public static string RaceToTranslatedName(this GameClient client, int race, int gender)
		{
			eRace r = (eRace)race;
			string translationID = string.Format("GamePlayer.PlayerRace.{0}", r.ToString("F")); //Returns 'Unknown'

			if (r != 0)
			{
				switch ((eGender)gender)
				{
					case eGender.Female:
						translationID = string.Format("GamePlayer.PlayerRace.Female.{0}", r.ToString("F"));
						break;
					default:
						translationID = string.Format("GamePlayer.PlayerRace.Male.{0}", r.ToString("F"));
						break;
				}
			}
			
            return LanguageMgr.GetTranslation(client, translationID);
		}
		
		/// <summary>
		/// Translate Given Race/Gender Combo in Player Language
		/// </summary>
		/// <param name="player"></param>
		/// <param name="race"></param>
		/// <param name="gender"></param>
		/// <returns></returns>
		public static string RaceToTranslatedName(this GamePlayer player, int race, eGender gender)
		{
			if (player.Client != null)
				return player.Client.RaceToTranslatedName(race, (int)gender);
			
			return string.Format("!{0} - {1}!", ((eRace)race).ToString("F"), gender.ToString("F"));
		}
		#endregion
		
	}
	
	public static class Constants
	{
		public static int USE_AUTOVALUES = -1;
	}
}
