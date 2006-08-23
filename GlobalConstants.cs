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
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	public enum eObjectType : byte
	{
		GenericItem = 0,
		GenericWeapon = 1,

		//Albion weapons
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
		R_RING = 0x24
	};

	/// <summary>
	/// all known slots
	/// </summary>
	public class Slot
	{
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
		public const int JEWELLERY = 24;
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
	};

	/// <summary>
	/// This enumeration holds all slots that can wear attackable armor
	/// </summary>
	public enum eArmorSlot : int
	{
		UNKNOWN = 0x00,
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

		Falling = 255,
	}

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
		CHR = eProperty.Charisma
	}

	/// <summary>
	/// resists
	/// </summary>
	public enum eResist : byte
	{
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

	public enum eResurectionSicknessType : int
	{
		PvMSickness = 0,
		RvRSickness = 1,
		NoSickness = 2
	}

	/// <summary>
	/// All property types for check using SkillBase.CheckPropertyType. Must be unique bits set.
	/// </summary>
	[Flags]
	public enum ePropertyType : byte
	{
		Focus = 1,
		Resist = 1 << 1,
		Skill = 1 << 2,
		SkillMeleeWeapon = 1 << 3,
		SkillMagical = 1 << 4,
		SkillDualWield = 1 << 5,
		SkillArchery = 1 << 6,
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
		Skill_Last = 108,

		// Classic Focii
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

		Focus_Mind = 132,
		Focus_Void = 133,
		Focus_Mana = 134,
		Focus_Enchantments = 135,
		Focus_Mentalism = 136,
		Focus_Summoning = 137,
		// SI Focii
		// Mid
		Focus_BoneArmy = 138,
		// Alb
		Focus_PainWorking = 139,
		Focus_DeathSight = 140,
		Focus_DeathServant = 141,
		// Hib
		Focus_Verdant = 142,
		Focus_CreepingPath = 143,
		Focus_Arboreal = 144,
		// Catacombs Focii
		Focus_EtherealShriek = 157,
		Focus_PhantasmalWail = 158,
		Focus_SpectralForce = 159,
		Focus_Cursing = 160,
		Focus_Hexing = 161,
		Focus_Witchcraft = 162,

		MaxSpeed = 145,
		MaxConcentration = 147,

		ArmorFactor = 148,
		ArmorAbsorbtion = 149,

		HealthRegenerationRate = 150,
		PowerRegenerationRate = 151,
		EnduranceRegenerationRate = 152,

		SpellRange = 153,
		ArcheryRange = 154,

		MeleeSpeed = 155,
		Acuity = 156,
		DPS = 157,
		MagicAbsorbtion = 158,

		AllMagicSkills = 163,
		AllMeleeWeaponSkills = 164,
		AllFocusLevels = 165,
		AllDualWieldingSkills = 167,
		AllArcherySkills = 168,

		LivingEffectiveLevel = 166,

		EvadeChance = 169,
		BlockChance = 170,
		ParryChance = 171,
		FatigueConsumption = 172,
		FumbleChance = 175,

		MeleeDamage = 173,
		RangedDamage = 174,

		MesmerizeDuration = 176,
		StunDuration = 177,
		SpeedDecreaseDuration = 178,

		//Catacombs
		BladeturnReinforcement = 179,
		DefensiveBonus = 180,
		NegativeReduction = 182,
		PieceAblative = 183,
		ReactionaryStyleDamage = 184,
		SpellPowerCost = 185,
		StyleCostReduction = 186,
		ToHitBonus = 187,

		//TOA
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
		StatCapBonus_Last = 208,
		AcuCapBonus = 209,
		MaxHealthCapBonus = 210,
		PowerPoolCapBonus = 211,

		WeaponSkill = 212,
		AllSkills = 213,
		CriticalMeleeHitChance = 214,
		CriticalArcheryHitChance = 215,

		MaxProperty = 255,
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
		Minotaur = 19,

		_Last = 19,
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

	public abstract class ShieldLevel
	{
		public const int Small = 1;
		public const int Medium = 2;
		public const int Large = 3;
	}

	public enum eMaterial : byte
	{
		Cloth = 0,
		Leather = 1,
		LeatherAndMetal = 2,
		Metal = 3,
		Wood = 4,
	}

	/// <summary>
	/// strong name constants of spell line used in the world (poison, proc ect ...)
	/// </summary>
	public abstract class GlobalSpellsLines
	{
		public const string Combat_Styles_Effect = "Combat Style Effects";
		public const string Mundane_Poisons = "Mundane Poisons";
		public const string Reserved_Spells = "Reserved Spells"; // Resurrection illness, Speed of the realm
		public const string SiegeWeapon_Spells = "SiegeWeapon Spells";
		public const string Item_Effects = "Item Effects";
		public const string Potions_Effects = "Potions";
		public const string Mob_Spells = "Mob Spells";
		public const string Character_Abilities = "Character Abilities"; // dirty tricks, flurry ect...
	}

	public class GlobalConstants
	{
		/// <summary>
		/// ITEMTYPE != OBJECTYPE
		/// ITEMTYPE = SLOT
		/// OBJECTTYPE = REAL TYPE
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsWeapon(int objectTypeID)
		{
			if ((objectTypeID >= 1 && objectTypeID <= 26) || objectTypeID == (int)eObjectType.Shield) return true;
			return false;
		}
		/// <summary>
		/// ITEMTYPE != OBJECTYPE
		/// ITEMTYPE = SLOT
		/// OBJECTTYPE = REAL TYPE
		/// </summary>
		/// <param name="objectTypeID"></param>
		/// <returns></returns>
		public static bool IsArmor(int objectTypeID)
		{
			if (objectTypeID >= 32 && objectTypeID <= 38) return true;
			return false;
		}

		public static string InstrumentTypeToName(int instrumentTypeID)
		{
			switch (instrumentTypeID)
			{
				case 1: return "drum";
				case 2: return "lute";
				case 3: return "flute";
			}
			return "unknown";
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
			switch (shieldTypeID)
			{
				case 1: return "small";
				case 2: return "medium";
				case 3: return "larget";
			}
			return "none";
		}

		public static string WeaponDamageTypeToName(int weaponDamageTypeID)
		{
			return ((eWeaponDamageType)weaponDamageTypeID).ToString();
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
			}
			return "generic inventory";
		}

		//This method translates a string to an InventorySlotID
		public static byte NameToSlot(string name)
		{
			switch (name)
			{
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

		public static string CraftLevelToCraftTitle(int craftLevel)
		{
			switch ((int)craftLevel / 100)
			{
				case 0: return "Helper";
				case 1: return "Junior Apprentice";
				case 2: return "Apprentice";
				case 3: return "Neophyte";
				case 4: return "Assistant";
				case 5: return "Junior";
				case 6: return "Journeyman";
				case 7: return "Senior";
				case 8: return "Master";
				case 9: return "Grandmaster";
				case 10: return "Legendary";
				case 11: return "Legendary Grandmaster";
				default: return "";
			}
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
				default: return 0;
			}
		}
	}
}
