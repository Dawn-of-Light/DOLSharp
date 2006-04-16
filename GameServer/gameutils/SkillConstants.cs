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

namespace DOL.GS
{
	/// <summary>
	/// strong name constants for built in abilities
	/// </summary>
	public abstract class Abilities
	{
		// basics
		public const string Sprint = "Sprint";
		public const string Quickcast = "QuickCast";

		// Armor
		public const string AlbArmor = "AlbArmor";
		public const string HibArmor = "HibArmor";
		public const string MidArmor = "MidArmor";
//		public const string Armor_Cloth = "Cloth";
//		public const string Armor_Leather = "Leather";
//		public const string Armor_Studded = "Studded";
//		public const string Armor_Chain = "Chain";
//		public const string Armor_Plate = "Plate";
//		public const string Armor_Reinforced = "Reinforced";
//		public const string Armor_Scale = "Scale";

		// shields
		public const string Shield = "Shield";
//		public const string Shield_SmallShields = "Shield Ability: Small Shields";
//		public const string Shield_MediumShields = "Shield Ability: Medium Shields";
//		public const string Shield_LargeShields = "Shield Ability: Large Shields";

		// Weaponry
		public const string Weapon_Staves = "Weaponry: Staves";
		public const string Weapon_Slashing = "Weaponry: Slashing";
		public const string Weapon_Crushing = "Weaponry: Crushing";
		public const string Weapon_Thrusting = "Weaponry: Thrusting";
		public const string Weapon_Polearms = "Weaponry: Polearms";
		public const string Weapon_TwoHanded = "Weaponry: Two Handed";
		public const string Weapon_Flexible = "Weaponry: Flexible";
		public const string Weapon_Crossbow = "Weaponry: Crossbow";
		public const string Weapon_CompositeBows = "Weaponry: Composite Bows";
		public const string Weapon_RecurvedBows = "Weaponry: Recurved Bows";
		public const string Weapon_Shortbows  = "Weaponry: Shortbows";
		public const string Weapon_Longbows  = "Weaponry: Longbows";
		public const string Weapon_Axes = "Weaponry: Axes";
		public const string Weapon_LeftAxes = "Weaponry: Left Axes";
		public const string Weapon_Hammers = "Weaponry: Hammers";
		public const string Weapon_Swords = "Weaponry: Swords";
		public const string Weapon_HandToHand = "Weaponry: Hand to Hand";
		public const string Weapon_Spears = "Weaponry: Spears";
		public const string Weapon_Thrown = "Weaponry: Thrown";
		public const string Weapon_Blades = "Weaponry: Blades";
		public const string Weapon_Blunt = "Weaponry: Blunt";
		public const string Weapon_Piercing = "Weaponry: Piercing";
		public const string Weapon_LargeWeapons = "Weaponry: Large Weapons";
		public const string Weapon_CelticSpear = "Weaponry: Celtic Spears";
		public const string Weapon_Scythe = "Weaponry: Scythe";
		// doesn't exist on live but better this than hardcoded classes that can use instruments
		public const string Weapon_Instruments = "Weaponry: Instruments";

		// other
		public const string Advanced_Evade = "Advanced Evade Ability";
		public const string Evade = "Evade";
		public const string Berserk = "Berserk";
		public const string Enhanced_Evade = "Enhanced Evade";
		public const string Intercept = "Intercept";
		public const string Charge = "Charge Ability";
		public const string Flurry = "Flurry Ability";
		public const string Protect = "Protect";
		public const string Critical_Shot = "Critical Shot";
		public const string Camouflage = "Camouflage";
		public const string DirtyTricks = "Dirty Tricks";
		public const string Triple_Wield = "Triple Wield Ability";
		public const string Distraction = "Distraction";
		public const string DetectHidden = "Detect Hidden";

		public const string SafeFall = "Safe Fall";
		public const string ClimbWalls = "Climb Walls";
		public const string DangerSense = "Danger Sense";
		public const string Engage = "Engage";
		public const string Envenom = "Envenom";
		public const string Guard = "Guard";
		public const string PenetratingArrow = "Penetrating Arrow";
		public const string PreventFlight = "Prevent Flight";
		public const string RapidFire = "Rapid Fire";
		public const string Stag = "Stag";
//		public const string Initiate_OfTheHunt = "Initiate of the Hunt";
//		public const string Member_OfTheHunt = "Member of the Hunt";
//		public const string Leader_OfTheHunt = "Leader of the Hunt";
//		public const string Master_OfTheHunt = "Master of the Hunt";
		public const string Stoicism = "Stoicism";
		public const string SureShot = "Sure Shot";
		public const string Tireless = "Tireless";
		public const string VampiirConstitution = "Vampiir Constitution";
		public const string VampiirDexterity = "Vampiir Dexterity";
		public const string VampiirQuickness = "Vampiir Quickness";
		public const string VampiirStrength = "Vampiir Strength";
		public const string Volley = "Volley";

	}

	/// <summary>
	/// strong name constants for built in specs
	/// </summary>
	public abstract class Specs 
	{
		public const string Slash = "Slash";
		public const string Crush = "Crush";
		public const string Thrust = "Thrust";
		public const string Piercing = "Piercing";
		public const string Staff = "Staff";
		public const string Flexible = "Flexible";
		public const string ShortBow = "Shorbow";
		public const string Longbow = "Longbows";
		public const string Crossbow = "Crossbows";
		public const string CompositeBow = "Composite Bow";
		public const string RecurveBow = "Recurve Bow";
		public const string Sword = "Sword";
		public const string Axe = "Axe";
		public const string Hammer = "Hammer";
		public const string Shields = "Shields";
		public const string Left_Axe = "Left Axe";
		public const string Two_Handed = "Two Handed";
		public const string Thrown_Weapons = "Thrown Weapons";
		public const string Polearms = "Polearm";
		public const string Blades = "Blades";
		public const string Blunt = "Blunt";
		public const string Critical_Strike = "Critical Strike";
		public const string Dual_Wield = "Dual Wield";
		public const string Spear = "Spear";
		public const string Large_Weapons = "Large Weapons";
		public const string HandToHand = "Hand to Hand";
		public const string Scythe = "Scythe";
		public const string Celtic_Dual = "Celtic Dual";
		public const string Celtic_Spear = "Celtic Spear";

		public const string Stealth = "Stealth";
		public const string Parry = "Parry";
		public const string Envenom = "Envenom";
		//public const string Arborial_Mastery = "Arborial Mastery";
		public const string Savagery = "Savagery";
		public const string Pathfinding = "Pathfinding";
		public const string Nightshade_Magic = "Nightshade Magic";

		public const string Augmentation  = "Augmentation";
		public const string Battlesongs   = "Battlesongs";
		public const string Beastcraft    = "Beastcraft";
		public const string Body_Magic    = "Body Magic";
		public const string Chants        = "Chants";
		public const string Cold_Magic    = "Cold Magic";
		public const string Darkness      = "Darkness";
		public const string Earth_Magic   = "Earth Magic";
		public const string Enchantments  = "Enchantments";
		public const string Enhancement   = "Enhancement";
		public const string Fire_Magic    = "Fire Magic";
		public const string Instruments   = "Instruments";
		public const string Light         = "Light";
		public const string Mana          = "Mana";
		public const string Matter_Magic  = "Matter Magic";
		public const string Mending       = "Mending";
		public const string Mentalism     = "Mentalism";
		public const string Mind_Magic    = "Mind Magic";
		public const string Music         = "Music";
		public const string Nature        = "Nature";
		public const string Nurture       = "Nurture";
		public const string Pacification  = "Pacification";

		public const string Regrowth      = "Regrowth";
		public const string Rejuvenation  = "Rejuvenation";
		public const string Runecarving   = "Runecarving";
		public const string Smite         = "Smite";
		public const string Spirit_Magic  = "Spirit Magic";
		public const string Stormcalling  = "Stormcalling";
		public const string Subterranean  = "Subterranean";
		public const string Summoning     = "Summoning";
		public const string Suppression   = "Suppression";
		public const string Valor         = "Valor";
		public const string Void          = "Void";
		public const string Wind_Magic    = "Wind Magic";

		public const string Arboreal_Path = "Arboreal Path"; //Forester
		public const string Bonedancing   = "Bonedancing"; //Bonedancer
		public const string Creeping_Path = "Creeping Path"; //Animist
		public const string Cursing 	  = "Cursing"; //Warlock
		public const string Death_Servant = "Death Servant"; //Necro
		public const string Deathsight    = "Deathsight"; //Necro
		public const string Dementia      = "Dementia"; //Vampiir
		public const string EtherealShriek = "Ethereal Shriek"; //Bainshee
		public const string Hexing 	  = "Hexing"; //Warlock
		public const string Necro_Deathsight = "Necro Deathsight"; //Necro
		public const string Necro_Painworking = "Necro Painworking"; //Necro
		public const string OdinsWill  	  = "Odin's Will"; //Valkyrie
		public const string Painworking   = "Painworking"; //Necro
		public const string PhantasmalWail = "Phantasmal Wail"; //Bainshee
		public const string ShadowMastery = "Shadow Mastery"; //Vampiir
		public const string Soulrending   = "Soulrending"; //Reaver
		public const string SpectralForce = "Spectral Force"; //Bainshee
		public const string VampiiricEmbrace = "Vampiiric Embrace"; //Vampiir
		public const string Verdant_Path  = "Verdant Path"; //Animist
		public const string Witchcraft  = "Witchcraft"; //Warlock
	}		
}