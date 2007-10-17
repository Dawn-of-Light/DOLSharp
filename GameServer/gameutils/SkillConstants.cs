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
		/// <summary>
		/// Player Sprint Ability
		/// </summary>
		public const string Sprint = "Sprint";
		/// <summary>
		/// Player Quickcast Ability
		/// </summary>
		public const string Quickcast = "QuickCast";

		// Armor
		/// <summary>
		/// All Albion Armors
		/// </summary>
		public const string AlbArmor = "AlbArmor";
		/// <summary>
		/// All Hibernian Armors
		/// </summary>
		public const string HibArmor = "HibArmor";
		/// <summary>
		/// All Midgard Armors
		/// </summary>
		public const string MidArmor = "MidArmor";

		// shields
		/// <summary>
		/// All Shields
		/// </summary>
		public const string Shield = "Shield";

		// Weaponry
		/// <summary>
		/// Staves / Staff weapons
		/// </summary>
		public const string Weapon_Staves = "Weaponry: Staves";
        /// <summary>
        /// Staves / Staff weapons
        /// </summary>
        public const string Weapon_Archery = "Weaponry: Archery";
		/// <summary>
		/// Slashing Weapons
		/// </summary>
		public const string Weapon_Slashing = "Weaponry: Slashing";
		/// <summary>
		/// Crushing Weapons
		/// </summary>
		public const string Weapon_Crushing = "Weaponry: Crushing";
		/// <summary>
		/// Thrusting Weapons
		/// </summary>
		public const string Weapon_Thrusting = "Weaponry: Thrusting";
		/// <summary>
		/// Polearm Weapons
		/// </summary>
		public const string Weapon_Polearms = "Weaponry: Polearms";
		/// <summary>
		/// Two Handed Weapons
		/// </summary>
		public const string Weapon_TwoHanded = "Weaponry: Two Handed";
		/// <summary>
		/// Flexible Weapons
		/// </summary>
		public const string Weapon_Flexible = "Weaponry: Flexible";
		/// <summary>
		/// Crossbow Weapons
		/// </summary>
		public const string Weapon_Crossbow = "Weaponry: Crossbow";
		/// <summary>
		/// Composite Bow Weapons
		/// </summary>
		public const string Weapon_CompositeBows = "Weaponry: Composite Bows";
		/// <summary>
		/// Recurved Bow Weapons
		/// </summary>
		public const string Weapon_RecurvedBows = "Weaponry: Recurved Bows";
		/// <summary>
		/// Shortbow Weapons
		/// </summary>
		public const string Weapon_Shortbows  = "Weaponry: Shortbows";
		/// <summary>
		/// Longbow Weapons
		/// </summary>
		public const string Weapon_Longbows  = "Weaponry: Longbows";
		/// <summary>
		/// Axe Weapons
		/// </summary>
		public const string Weapon_Axes = "Weaponry: Axes";
		/// <summary>
		/// Left Axe Weapons
		/// </summary>
		public const string Weapon_LeftAxes = "Weaponry: Left Axes";
		/// <summary>
		/// Hammer Weapons
		/// </summary>
		public const string Weapon_Hammers = "Weaponry: Hammers";
		/// <summary>
		/// Sword Weapons
		/// </summary>
		public const string Weapon_Swords = "Weaponry: Swords";
		/// <summary>
		/// Hand to Hand Weapons
		/// </summary>
		public const string Weapon_HandToHand = "Weaponry: Hand to Hand";
		/// <summary>
		/// Spear Weapons
		/// </summary>
		public const string Weapon_Spears = "Weaponry: Spears";
		/// <summary>
		/// Thrown Weapons
		/// </summary>
		public const string Weapon_Thrown = "Weaponry: Thrown";
		/// <summary>
		/// Blade Weapons
		/// </summary>
		public const string Weapon_Blades = "Weaponry: Blades";
		/// <summary>
		/// Blunt Weapons
		/// </summary>
		public const string Weapon_Blunt = "Weaponry: Blunt";
		/// <summary>
		/// Pierce Weapons
		/// </summary>
		public const string Weapon_Piercing = "Weaponry: Piercing";
		/// <summary>
		/// Large Weapons
		/// </summary>
		public const string Weapon_LargeWeapons = "Weaponry: Large Weapons";
		/// <summary>
		/// Celtic Spear Weapons
		/// </summary>
		public const string Weapon_CelticSpear = "Weaponry: Celtic Spears";
		/// <summary>
		/// Scythe Weapons
		/// </summary>
		public const string Weapon_Scythe = "Weaponry: Scythe";
		/// <summary>
		/// Instruments (doesn't exist on live but better this than hardcoded classes that can use instruments)
		/// </summary>
		public const string Weapon_Instruments = "Weaponry: Instruments";
		/// <summary>
		/// Maulers Staff Weapons
		/// </summary>
		public const string Weapon_MaulerStaff = "Weaponry: Mauler Staff";
		/// <summary>
		/// Fist Wraps Weapons
		/// </summary>
		public const string Weapon_FistWraps = "Weaponry: Fist Wraps";

		// other
		/// <summary>
		/// Advanced Evade Ability
		/// </summary>
		public const string Advanced_Evade = "Advanced Evade Ability";
		/// <summary>
		/// Evade Ability
		/// </summary>
		public const string Evade = "Evade";
		/// <summary>
		/// Berserk Ability
		/// </summary>
		public const string Berserk = "Berserk";
		/// <summary>
		/// Intercept Ability
		/// </summary>
		public const string Intercept = "Intercept";
		/// <summary>
		/// Intercept Ability
		/// </summary>
		public const string ChargeAbility = "Charge";
		/// <summary>
		/// Flurry Ability
		/// </summary>
		public const string Flurry = "Flurry";
		/// <summary>
		/// Protect Ability
		/// </summary>
		public const string Protect = "Protect";
		/// <summary>
		/// Critical Shot Ability
		/// </summary>
		public const string Critical_Shot = "Critical Shot";
		/// <summary>
		/// Camouflage Ability
		/// </summary>
		public const string Camouflage = "Camouflage";
		/// <summary>
		/// Dirty Tricks Ability
		/// </summary>
		public const string DirtyTricks = "Dirty Tricks";
		/// <summary>
		/// Triple Wield Ability
		/// </summary>
		public const string Triple_Wield = "Triple Wield Ability";
		/// <summary>
		/// Distraction Ability
		/// </summary>
		public const string Distraction = "Distraction";
		/// <summary>
		/// Detect Hidden Ability
		/// </summary>
		public const string DetectHidden = "Detect Hidden";
		/// <summary>
		/// Safe Fall Ability
		/// </summary>
		public const string SafeFall = "Safe Fall";
		/// <summary>
		/// Climb Walls Ability
		/// </summary>
		public const string ClimbWalls = "Climb Walls";
		/// <summary>
		/// Danger Sense Ability
		/// </summary>
		public const string DangerSense = "Danger Sense";
		/// <summary>
		/// Engage Ability
		/// </summary>
		public const string Engage = "Engage";
		/// <summary>
		/// Envonom Ability
		/// </summary>
		public const string Envenom = "Envenom";
		/// <summary>
		/// Guard Ability
		/// </summary>
		public const string Guard = "Guard";
		/// <summary>
		/// Penetrating Arrow Ability
		/// </summary>
		public const string PenetratingArrow = "Penetrating Arrow";
		/// <summary>
		/// Prevent Flight Ability
		/// </summary>
		public const string PreventFlight = "Prevent Flight";
		/// <summary>
		/// Rapid Fire Ability
		/// </summary>
		public const string RapidFire = "Rapid Fire";
		/// <summary>
		/// Stag Ability
		/// </summary>
		public const string Stag = "Stag";
		/// <summary>
		/// Stoicism Ability
		/// </summary>
		public const string Stoicism = "Stoicism";
		/// <summary>
		/// Sure Shot Ability
		/// </summary>
		public const string SureShot = "Sure Shot";
		/// <summary>
		/// Tireless Ability
		/// </summary>
		public const string Tireless = "Tireless";
		/// <summary>
		/// Vampiir Constitution Ability
		/// </summary>
		public const string VampiirConstitution = "Vampiir Constitution";
		/// <summary>
		/// Vampiir Dexterity Ability
		/// </summary>
		public const string VampiirDexterity = "Vampiir Dexterity";
		/// <summary>
		/// Vampiir Quickness Ability
		/// </summary>
		public const string VampiirQuickness = "Vampiir Quickness";
		/// <summary>
		/// Vampiir Strength Ability
		/// </summary>
		public const string VampiirStrength = "Vampiir Strength";
		/// <summary>
		/// Vampiir Bolt Ability
		/// </summary>
		public const string VampiirBolt = "Vampiir Bolt";
		/// <summary>
		/// Volley Ability
		/// </summary>
		public const string Volley = "Volley";

		//new maintank skills with 1.81
		/// <summary>
		/// Scars of Battle Ability
		/// </summary>
		public const string ScarsOfBattle = "Scars of Battle";
		/// <summary>
		/// Memories of War Ability
		/// </summary>
		public const string MemoriesOfWar = "Memories of War";
		//moved from Armsman RR5 RA to Armsman 25 spec crossbow.
		/// <summary>
		/// Snapshot Ability
		/// </summary>
		public const string Snapshot = "Snapshot";
		/// <summary>
		/// Rampage Ability
		/// </summary>		
        public const string Rampage = "Rampage";
		/// <summary>
		/// Metal guard Ability
		/// </summary>	
        public const string MetalGuard = "Metal Guard";
		/// <summary>
		/// Fury Ability
		/// </summary>	        
        public const string Fury = "Fury";
        /// <summary>
        /// Fury Ability
        /// </summary>	        
        public const string Bodyguard = "Bodyguard";
		/// <summary>
		/// Bolstering Roar Ability
		/// </summary>	        
        public const string BolsteringRoar = "Bolstering Roar";
 		/// <summary>
		/// Taunting Shout Ability
		/// </summary>	        
        public const string TauntingShout = "Taunting Shout";
		//NPC only abilities
		/// <summary>
		/// Crowd Control Immunity - NPC Only ability
		/// </summary>
		public const string CCImmunity = "CCImmunity"; 

	}

	/// <summary>
	/// strong name constants for built in specs
	/// </summary>
	public abstract class Specs 
	{
		/// <summary>
		/// Slash Weapon Spec
		/// </summary>
		public const string Slash = "Slash";
		/// <summary>
		/// Crush Weapon Spec
		/// </summary>
		public const string Crush = "Crush";
		/// <summary>
		/// Thrust Weapon Spec
		/// </summary>
		public const string Thrust = "Thrust";
		/// <summary>
		/// Pierce Weapon Spec
		/// </summary>
		public const string Piercing = "Piercing";
		/// <summary>
		/// Staff Weapon Spec
		/// </summary>
		public const string Staff = "Staff";
		/// <summary>
		/// Flexible Weapon Spec
		/// </summary>
		public const string Flexible = "Flexible";
		/// <summary>
		/// Shortbow Weapon Spec
		/// </summary>
		public const string ShortBow = "Shorbow";
		/// <summary>
		/// Longbow Weapon Spec
		/// </summary>
		public const string Longbow = "Longbows";
		/// <summary>
		/// Crossbow Weapon Spec
		/// </summary>
		public const string Crossbow = "Crossbows";
		/// <summary>
		/// Composite Bow Weapon Spec
		/// </summary>
		public const string CompositeBow = "Composite Bow";
		/// <summary>
		/// Recurve Bow Weapon Spec
		/// </summary>
		public const string RecurveBow = "Recurve Bow";
		/// <summary>
		/// Sword Weapon Spec
		/// </summary>
		public const string Sword = "Sword";
		/// <summary>
		/// Axe Weapon Spec
		/// </summary>
		public const string Axe = "Axe";
		/// <summary>
		/// Hammer Weapon Spec
		/// </summary>
		public const string Hammer = "Hammer";
		/// <summary>
		/// Shields Spec
		/// </summary>
		public const string Shields = "Shields";
		/// <summary>
		/// Left Axe Weapon Spec
		/// </summary>
		public const string Left_Axe = "Left Axe";
		/// <summary>
		/// Two Handed Weapon Spec
		/// </summary>
		public const string Two_Handed = "Two Handed";
		/// <summary>
		/// Thrown Weapon Spec
		/// </summary>
		public const string Thrown_Weapons = "Thrown Weapons";
		/// <summary>
		/// Polearm Weapon Spec
		/// </summary>
		public const string Polearms = "Polearm";
		/// <summary>
		/// Blade Weapon Spec
		/// </summary>
		public const string Blades = "Blades";
		/// <summary>
		/// Blunt Weapon Spec
		/// </summary>
		public const string Blunt = "Blunt";
		/// <summary>
		/// Critical Strike Spec
		/// </summary>
		public const string Critical_Strike = "Critical Strike";
		/// <summary>
		/// Dual Wield Spec
		/// </summary>
		public const string Dual_Wield = "Dual Wield";
		/// <summary>
		/// Spear Weapon Spec
		/// </summary>
		public const string Spear = "Spear";
		/// <summary>
		/// Large Weapons Spec
		/// </summary>
		public const string Large_Weapons = "Large Weapons";
		/// <summary>
		/// Hand to Hand Weapon Spec
		/// </summary>
		public const string HandToHand = "Hand to Hand";
		/// <summary>
		/// Scythe Weapon Spec
		/// </summary>
		public const string Scythe = "Scythe";
		/// <summary>
		/// Celtic Dual Spec
		/// </summary>
		public const string Celtic_Dual = "Celtic Dual";
		/// <summary>
		/// Celtic Spear Weapon Spec
		/// </summary>
		public const string Celtic_Spear = "Celtic Spear";
		/// <summary>
		/// Stealth Spec
		/// </summary>
		public const string Stealth = "Stealth";
		/// <summary>
		/// Parry Spec
		/// </summary>
		public const string Parry = "Parry";
		/// <summary>
		/// Envenom Spec
		/// </summary>
		public const string Envenom = "Envenom";
		/// <summary>
		/// Savagery Magic Spec
		/// </summary>
		public const string Savagery = "Savagery";
		/// <summary>
		/// Pathfinding Magic Spec
		/// </summary>
		public const string Pathfinding = "Pathfinding";
		/// <summary>
		/// Nightshade Magic Spec
		/// </summary>
		public const string Nightshade_Magic = "Nightshade Magic";
		/// <summary>
		/// Augmentation Magic Spec
		/// </summary>
		public const string Augmentation  = "Augmentation";
		/// <summary>
		/// Battlesongs Magic Spec
		/// </summary>
		public const string Battlesongs   = "Battlesongs";
		/// <summary>
		/// Beastcraft Magic Spec
		/// </summary>
		public const string Beastcraft    = "Beastcraft";
		/// <summary>
		/// Body Magic Spec
		/// </summary>
		public const string Body_Magic    = "Body Magic";
		/// <summary>
		/// Chants Magic Spec
		/// </summary>
		public const string Chants        = "Chants";
		/// <summary>
		/// Cold Magic Spec
		/// </summary>
		public const string Cold_Magic    = "Cold Magic";
		/// <summary>
		/// Darkness Magic Spec
		/// </summary>
		public const string Darkness      = "Darkness";
		/// <summary>
		/// Earth Magic Spec
		/// </summary>
		public const string Earth_Magic   = "Earth Magic";
		/// <summary>
		/// Enchantments Magic Spec
		/// </summary>
		public const string Enchantments  = "Enchantments";
		/// <summary>
		/// Enhancement Magic Spec
		/// </summary>
		public const string Enhancement   = "Enhancement";
		/// <summary>
		/// FireMagic Spec
		/// </summary>
		public const string Fire_Magic    = "Fire Magic";
		/// <summary>
		/// Instruments Magic Spec
		/// </summary>
		public const string Instruments   = "Instruments";
		/// <summary>
		/// Light Magic Spec
		/// </summary>
		public const string Light         = "Light";
		/// <summary>
		/// Mana Magic Spec
		/// </summary>
		public const string Mana          = "Mana";
		/// <summary>
		/// Matter Magic Spec
		/// </summary>
		public const string Matter_Magic  = "Matter Magic";
		/// <summary>
		/// Mending Magic Spec
		/// </summary>
		public const string Mending       = "Mending";
		/// <summary>
		/// Mentalism Magic Spec
		/// </summary>
		public const string Mentalism     = "Mentalism";
		/// <summary>
		/// Mind Magic Spec
		/// </summary>
		public const string Mind_Magic    = "Mind Magic";
		/// <summary>
		/// Music Magic Spec
		/// </summary>
		public const string Music         = "Music";
		/// <summary>
		/// Nature Magic Spec
		/// </summary>
		public const string Nature        = "Nature";
		/// <summary>
		/// Nurture Magic Spec
		/// </summary>
		public const string Nurture       = "Nurture";
		/// <summary>
		/// Pacification Magic Spec
		/// </summary>
		public const string Pacification  = "Pacification";
		/// <summary>
		/// Regrowth Magic Spec
		/// </summary>
		public const string Regrowth      = "Regrowth";
		/// <summary>
		/// Rejuvenation Magic Spec
		/// </summary>
		public const string Rejuvenation  = "Rejuvenation";
		/// <summary>
		/// Runecarving Magic Spec
		/// </summary>
		public const string Runecarving   = "Runecarving";
		/// <summary>
		/// Smite Magic Spec
		/// </summary>
		public const string Smite         = "Smite";
		/// <summary>
		/// Spirit Magic Spec
		/// </summary>
		public const string Spirit_Magic  = "Spirit Magic";
		/// <summary>
		/// Stormcalling Magic Spec
		/// </summary>
		public const string Stormcalling  = "Stormcalling";
		/// <summary>
		/// Subterranean Magic Spec
		/// </summary>
		public const string Subterranean  = "Subterranean";
		/// <summary>
		/// Summoning Magic Spec
		/// </summary>
		public const string Summoning     = "Summoning";
		/// <summary>
		/// Supression Magic Spec
		/// </summary>
		public const string Suppression   = "Suppression";
		/// <summary>
		/// Valor Magic Spec
		/// </summary>
		public const string Valor         = "Valor";
		/// <summary>
		/// Void Magic Spec
		/// </summary>
		public const string Void          = "Void";
		/// <summary>
		/// Wind Magic Spec
		/// </summary>
		public const string Wind_Magic    = "Wind Magic";
		/// <summary>
		/// Arboreal Path Magic Spec
		/// </summary>
		public const string Arboreal_Path = "Arboreal Path"; //Forester
		/// <summary>
		/// BoneArmy Magic Spec
		/// </summary>
		public const string BoneArmy   = "Bone Army"; //Bonedancer
		/// <summary>
		/// Creeping Path Magic Spec
		/// </summary>
		public const string Creeping_Path = "Creeping Path"; //Animist
		/// <summary>
		/// Cursing Magic Spec
		/// </summary>
		public const string Cursing 	  = "Cursing"; //Warlock
		/// <summary>
		/// Death Servant Magic Spec
		/// </summary>
		public const string Death_Servant = "Death Servant"; //Necro
		/// <summary>
		/// Deathsight Magic Spec
		/// </summary>
		public const string Deathsight    = "Deathsight"; //Necro
		/// <summary>
		/// Dementia Magic Spec
		/// </summary>
		public const string Dementia      = "Dementia"; //Vampiir
		/// <summary>
		/// Ethereal Shriek Magic Spec
		/// </summary>
		public const string EtherealShriek = "Ethereal Shriek"; //Bainshee
		/// <summary>
		/// Hexing Magic Spec
		/// </summary>
		public const string Hexing 	  = "Hexing"; //Warlock
		/// <summary>
		/// Odins Will Magic Spec
		/// </summary>
		public const string OdinsWill  	  = "Odin's Will"; //Valkyrie
		/// <summary>
		/// Painworking Magic Spec
		/// </summary>
		public const string Painworking   = "Painworking"; //Necro
		/// <summary>
		/// Phantasmal Wail Magic Spec
		/// </summary>
		public const string PhantasmalWail = "Phantasmal Wail"; //Bainshee
		/// <summary>
		/// Shadow Mastery Magic Spec
		/// </summary>
		public const string ShadowMastery = "Shadow Mastery"; //Vampiir
		/// <summary>
		/// Soulrending Magic Spec
		/// </summary>
		public const string Soulrending   = "Soulrending"; //Reaver
		/// <summary>
		/// Spectral Force Magic Spec
		/// </summary>
		public const string SpectralForce = "Spectral Force"; //Bainshee
        /// <summary>
        /// Spectral Guard Magic Spec
        /// </summary>
        public const string SpectralGuard = "Spectral Guard"; //Bainshee
		/// <summary>
		/// Vampiiric Embrace Magic Spec
		/// </summary>
		public const string VampiiricEmbrace = "Vampiiric Embrace"; //Vampiir
		/// <summary>
		///  Verdant Path Magic Spec
		/// </summary>
		public const string Verdant_Path  = "Verdant Path"; //Animist
		/// <summary>
		/// Witchcraft Magic Spec
		/// </summary>
		public const string Witchcraft  = "Witchcraft"; //Warlock
		/// <summary>
		/// Mauler Aura Manipulation Spec
		/// </summary>
		public const string Aura_Manipulation = "Aura Manipulation";
		/// <summary>
		/// Mauler Magnetism Spec
		/// </summary>
		public const string Magnetism = "Magnetism";
		/// <summary>
		/// Mauler Power Strikes Spec
		/// </summary>
		public const string Power_Strikes = "Power Strikes";
		/// <summary>
		/// Mauler Mauler Staff Spec
		/// </summary>
		public const string Mauler_Staff = "Mauler Staff";
		/// <summary>
		/// Mauler Fist Wraps Spec
		/// </summary>
		public const string Fist_Wraps = "Fist Wraps";
	}		
}
