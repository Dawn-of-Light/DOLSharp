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
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Styles;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Skill Attribute
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
	public class SkillHandlerAttribute : Attribute
	{
		protected string m_keyName;

		public SkillHandlerAttribute(string keyName)
		{
			m_keyName = keyName;
		}

		public string KeyName
		{
			get { return m_keyName; }
		}
	}

	/// <summary>
	/// base class for skills
	/// </summary>
	public abstract class Skill
	{
		protected ushort m_id;
		protected string m_name;
		protected int m_level;

		/// <summary>
		/// Construct a Skill from the name, an id, and a level
		/// </summary>
		/// <param name="name"></param>
		/// <param name="id"></param>
		/// <param name="level"></param>
		public Skill(string name, ushort id, int level)
		{
			m_id = id;
			m_name = name;
			m_level = level;
		}

		/// <summary>
		/// in most cases it is icon id or other specifiing id for client
		/// like spell id or style id in spells
		/// </summary>
		public virtual ushort ID
		{
			get { return m_id; }
		}

		/// <summary>
		/// The Skill Name
		/// </summary>
		public virtual string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

		/// <summary>
		/// The Skill Level
		/// </summary>
		public virtual int Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		/// <summary>
		/// the type of the skill
		/// </summary>
		public virtual eSkillPage SkillType
		{
			get { return eSkillPage.Abilities; }
		}

		/// <summary>
		/// Clone a skill
		/// </summary>
		/// <returns></returns>
		public virtual Skill Clone()
		{
			return (Skill)MemberwiseClone();
		}
	}

	/// <summary>
	/// the named skill is used for identification purposes
	/// the name is strong and must be unique for one type of skill page
	/// so better make the name real unique
	/// </summary>
	public class NamedSkill : Skill
	{
		private string m_keyName;

		/// <summary>
		/// Construct a named skill from the keyname, name, id and level
		/// </summary>
		/// <param name="keyName">The keyname</param>
		/// <param name="name">The name</param>
		/// <param name="id">The ID</param>
		/// <param name="level">The level</param>
		public NamedSkill(string keyName, string name, ushort id, int level)
			: base(name, id, level)
		{
			m_keyName = keyName;
		}

		/// <summary>
		/// Returns the string representation of the Skill
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(32)
				.Append("KeyName=").Append(KeyName)
				.Append(", ID=").Append(ID)
				.ToString();
		}

		/// <summary>
		/// strong identification name
		/// </summary>
		public virtual string KeyName
		{
			get { return m_keyName; }
		}
	}

	public class Song : Spell
	{
		public Song(DBSpell spell, int requiredLevel)
			: base(spell, requiredLevel)
		{
		}

		public override eSkillPage SkillType
		{
			get { return eSkillPage.Songs; }
		}
	}

	public class SpellLine : NamedSkill
	{
		protected bool m_isBaseLine;
		protected string m_spec;

		public SpellLine(string keyname, string name, string spec, bool baseline)
			: base(keyname, name, 0, 1)
		{
			m_isBaseLine = baseline;
			m_spec = spec;
		}

		//		public IList GetSpellsForLevel() {
		//			ArrayList list = new ArrayList();
		//			for (int i = 0; i < m_spells.Length; i++) {
		//				if (m_spells[i].Level <= Level) {
		//					list.Add(m_spells[i]);
		//				}
		//			}
		//			return list;
		//		}

		public string Spec
		{
			get { return m_spec; }
		}

		public bool IsBaseLine
		{
			get { return m_isBaseLine; }
		}

		public override eSkillPage SkillType
		{
			get { return eSkillPage.Spells; }
		}
	}


	public enum eSkillPage
	{
		Specialization = 0x00,
		Abilities = 0x01,
		Styles = 0x02,
		Spells = 0x03,
		Songs = 0x04,
		AbilitiesSpell = 0x05,
		RealmAbilities = 0x06
	}

	/// <summary>
	///
	/// </summary>
	public class SkillBase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected static readonly Dictionary<string, Specialization> m_specsByName = new Dictionary<string, Specialization>();
		protected static readonly Dictionary<string, DBAbility> m_abilitiesByName = new Dictionary<string, DBAbility>();
		protected static readonly Dictionary<string, SpellLine> m_spellLinesByName = new Dictionary<string, SpellLine>();

		protected static readonly Dictionary<string, Type> m_abilityActionHandler = new Dictionary<string, Type>();
		protected static readonly Dictionary<string, Type> m_implementationTypeCache = new Dictionary<string, Type>();
		protected static readonly Dictionary<string, Type> m_specActionHandler = new Dictionary<string, Type>();

		// global table for spellLine => List of spells
		protected static readonly Dictionary<string, List<Spell>> m_spellLists = new Dictionary<string, List<Spell>>();

		// global table for spec => List of styles
		protected static readonly Dictionary<string, List<Style>> m_styleLists = new Dictionary<string, List<Style>>();

		// global table for spec => list of spec dependend abilities
		protected static readonly Dictionary<string, List<Ability>> m_specAbilities = new Dictionary<string, List<Ability>>();

		/// <summary>
		/// (procs) global table for style => list of styles dependend spells
		/// [StyleID, [ClassID, DBStyleXSpell]]
		/// ClassID for normal style is 0
		/// </summary>
		protected static readonly Dictionary<int, Dictionary<int, List<DBStyleXSpell>>> m_styleSpells = new Dictionary<int, Dictionary<int, List<DBStyleXSpell>>>();

		// lookup table for styles
		protected static readonly Dictionary<long, Style> m_stylesByIDClass = new Dictionary<long, Style>();

		// class id => realm ability list
		protected static readonly Dictionary<int, List<RealmAbility>> m_classRealmAbilities = new Dictionary<int, List<RealmAbility>>();

		// all spells by id
		protected static readonly Dictionary<int, Spell> m_spells = new Dictionary<int, Spell>(5000);

		#region Initialization Tables

		/// <summary>
		/// Holds object type to spec convertion table
		/// </summary>
		protected static Dictionary<eObjectType, string> m_objectTypeToSpec = new Dictionary<eObjectType, string>();

		/// <summary>
		/// Holds spec to skill table
		/// </summary>
		protected static Dictionary<string, eProperty> m_specToSkill = new Dictionary<string, eProperty>();

		/// <summary>
		/// Holds spec to focus table
		/// </summary>
		protected static Dictionary<string, eProperty> m_specToFocus = new Dictionary<string, eProperty>();

		/// <summary>
		/// Holds all property types
		/// </summary>
		private static readonly ePropertyType[] m_propertyTypes = new ePropertyType[(int)eProperty.MaxProperty];

		/// <summary>
		/// table for property names
		/// </summary>
		protected static readonly Dictionary<eProperty, string> m_propertyNames = new Dictionary<eProperty, string>();

		/// <summary>
		/// Table to hold the race resists
		/// </summary>
		protected static Dictionary<eResist, int>[] m_raceResists = new Dictionary<eResist,int>[(int)eRace._Last + 3];

		/// <summary>
		/// Initialize the object type hashtable
		/// </summary>
		private static void InitializeObjectTypeToSpec()
		{
			m_objectTypeToSpec.Add(eObjectType.Staff, Specs.Staff);
			m_objectTypeToSpec.Add(eObjectType.Fired, Specs.ShortBow);

			m_objectTypeToSpec.Add(eObjectType.FistWraps, Specs.Fist_Wraps);
			m_objectTypeToSpec.Add(eObjectType.MaulerStaff, Specs.Mauler_Staff);

			//alb
			m_objectTypeToSpec.Add(eObjectType.CrushingWeapon, Specs.Crush);
			m_objectTypeToSpec.Add(eObjectType.SlashingWeapon, Specs.Slash);
			m_objectTypeToSpec.Add(eObjectType.ThrustWeapon, Specs.Thrust);
			m_objectTypeToSpec.Add(eObjectType.TwoHandedWeapon, Specs.Two_Handed);
			m_objectTypeToSpec.Add(eObjectType.PolearmWeapon, Specs.Polearms);
			m_objectTypeToSpec.Add(eObjectType.Flexible, Specs.Flexible);
			m_objectTypeToSpec.Add(eObjectType.Longbow, Specs.Archery);
			m_objectTypeToSpec.Add(eObjectType.Crossbow, Specs.Crossbow);
			//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown); break);

			//mid
			m_objectTypeToSpec.Add(eObjectType.Hammer, Specs.Hammer);
			m_objectTypeToSpec.Add(eObjectType.Sword, Specs.Sword);
			m_objectTypeToSpec.Add(eObjectType.LeftAxe, Specs.Left_Axe);
			m_objectTypeToSpec.Add(eObjectType.Axe, Specs.Axe);
			m_objectTypeToSpec.Add(eObjectType.HandToHand, Specs.HandToHand);
			m_objectTypeToSpec.Add(eObjectType.Spear, Specs.Spear);
			m_objectTypeToSpec.Add(eObjectType.CompositeBow, Specs.Archery);
			m_objectTypeToSpec.Add(eObjectType.Thrown, Specs.Thrown_Weapons);

			//hib
			m_objectTypeToSpec.Add(eObjectType.Blunt, Specs.Blunt);
			m_objectTypeToSpec.Add(eObjectType.Blades, Specs.Blades);
			m_objectTypeToSpec.Add(eObjectType.Piercing, Specs.Piercing);
			m_objectTypeToSpec.Add(eObjectType.LargeWeapons, Specs.Large_Weapons);
			m_objectTypeToSpec.Add(eObjectType.CelticSpear, Specs.Celtic_Spear);
			m_objectTypeToSpec.Add(eObjectType.Scythe, Specs.Scythe);
			m_objectTypeToSpec.Add(eObjectType.RecurvedBow, Specs.Archery);

			m_objectTypeToSpec.Add(eObjectType.Shield, Specs.Shields);
			m_objectTypeToSpec.Add(eObjectType.Poison, Specs.Envenom);
		}

		/// <summary>
		/// Initialize the spec to skill table
		/// </summary>
		private static void InitializeSpecToSkill()
		{
			#region Weapon Specs

			//Weapon specs
			//Alb
			m_specToSkill.Add(Specs.Thrust, eProperty.Skill_Thrusting);
			m_specToSkill.Add(Specs.Slash, eProperty.Skill_Slashing);
			m_specToSkill.Add(Specs.Crush, eProperty.Skill_Crushing);
			m_specToSkill.Add(Specs.Polearms, eProperty.Skill_Polearms);
			m_specToSkill.Add(Specs.Two_Handed, eProperty.Skill_Two_Handed);
			m_specToSkill.Add(Specs.Staff, eProperty.Skill_Staff);
			m_specToSkill.Add(Specs.Dual_Wield, eProperty.Skill_Dual_Wield);
			m_specToSkill.Add(Specs.Flexible, eProperty.Skill_Flexible_Weapon);
			m_specToSkill.Add(Specs.Longbow, eProperty.Skill_Long_bows);
			m_specToSkill.Add(Specs.Crossbow, eProperty.Skill_Cross_Bows);
			//Mid
			m_specToSkill.Add(Specs.Sword, eProperty.Skill_Sword);
			m_specToSkill.Add(Specs.Axe, eProperty.Skill_Axe);
			m_specToSkill.Add(Specs.Hammer, eProperty.Skill_Hammer);
			m_specToSkill.Add(Specs.Left_Axe, eProperty.Skill_Left_Axe);
			m_specToSkill.Add(Specs.Spear, eProperty.Skill_Spear);
			m_specToSkill.Add(Specs.CompositeBow, eProperty.Skill_Composite);
			m_specToSkill.Add(Specs.Thrown_Weapons, eProperty.Skill_Thrown_Weapons);
			m_specToSkill.Add(Specs.HandToHand, eProperty.Skill_HandToHand);
			//Hib
			m_specToSkill.Add(Specs.Blades, eProperty.Skill_Blades);
			m_specToSkill.Add(Specs.Blunt, eProperty.Skill_Blunt);
			m_specToSkill.Add(Specs.Piercing, eProperty.Skill_Piercing);
			m_specToSkill.Add(Specs.Large_Weapons, eProperty.Skill_Large_Weapon);
			m_specToSkill.Add(Specs.Celtic_Dual, eProperty.Skill_Celtic_Dual);
			m_specToSkill.Add(Specs.Celtic_Spear, eProperty.Skill_Celtic_Spear);
			m_specToSkill.Add(Specs.RecurveBow, eProperty.Skill_RecurvedBow);
			m_specToSkill.Add(Specs.Scythe, eProperty.Skill_Scythe);

			#endregion

			#region Magic Specs

			//Magic specs
			//Alb
			m_specToSkill.Add(Specs.Matter_Magic, eProperty.Skill_Matter);
			m_specToSkill.Add(Specs.Body_Magic, eProperty.Skill_Body);
			m_specToSkill.Add(Specs.Spirit_Magic, eProperty.Skill_Spirit);
			m_specToSkill.Add(Specs.Rejuvenation, eProperty.Skill_Rejuvenation);
			m_specToSkill.Add(Specs.Enhancement, eProperty.Skill_Enhancement);
			m_specToSkill.Add(Specs.Smite, eProperty.Skill_Smiting);
			m_specToSkill.Add(Specs.Instruments, eProperty.Skill_Instruments);
			m_specToSkill.Add(Specs.Deathsight, eProperty.Skill_DeathSight);
			m_specToSkill.Add(Specs.Painworking, eProperty.Skill_Pain_working);
			m_specToSkill.Add(Specs.Death_Servant, eProperty.Skill_Death_Servant);
			m_specToSkill.Add(Specs.Chants, eProperty.Skill_Chants);
			m_specToSkill.Add(Specs.Mind_Magic, eProperty.Skill_Mind);
			m_specToSkill.Add(Specs.Earth_Magic, eProperty.Skill_Earth);
			m_specToSkill.Add(Specs.Cold_Magic, eProperty.Skill_Cold);
			m_specToSkill.Add(Specs.Fire_Magic, eProperty.Skill_Fire);
			m_specToSkill.Add(Specs.Wind_Magic, eProperty.Skill_Wind);
			m_specToSkill.Add(Specs.Soulrending, eProperty.Skill_SoulRending);
			//Mid
			m_specToSkill.Add(Specs.Darkness, eProperty.Skill_Darkness);
			m_specToSkill.Add(Specs.Suppression, eProperty.Skill_Suppression);
			m_specToSkill.Add(Specs.Runecarving, eProperty.Skill_Runecarving);
			m_specToSkill.Add(Specs.Summoning, eProperty.Skill_Summoning);
			m_specToSkill.Add(Specs.BoneArmy, eProperty.Skill_BoneArmy);
			m_specToSkill.Add(Specs.Mending, eProperty.Skill_Mending);
			m_specToSkill.Add(Specs.Augmentation, eProperty.Skill_Augmentation);
			m_specToSkill.Add(Specs.Pacification, eProperty.Skill_Pacification);
			m_specToSkill.Add(Specs.Subterranean, eProperty.Skill_Subterranean);
			m_specToSkill.Add(Specs.Beastcraft, eProperty.Skill_BeastCraft);
			m_specToSkill.Add(Specs.Stormcalling, eProperty.Skill_Stormcalling);
			m_specToSkill.Add(Specs.Battlesongs, eProperty.Skill_Battlesongs);
			m_specToSkill.Add(Specs.Savagery, eProperty.Skill_Savagery);
			m_specToSkill.Add(Specs.OdinsWill, eProperty.Skill_OdinsWill);
			m_specToSkill.Add(Specs.Cursing, eProperty.Skill_Cursing);
			m_specToSkill.Add(Specs.Hexing, eProperty.Skill_Hexing);
			m_specToSkill.Add(Specs.Witchcraft, eProperty.Skill_Witchcraft);

			//Hib
			m_specToSkill.Add(Specs.Arboreal_Path, eProperty.Skill_Arboreal);
			m_specToSkill.Add(Specs.Creeping_Path, eProperty.Skill_Creeping);
			m_specToSkill.Add(Specs.Verdant_Path, eProperty.Skill_Verdant);
			m_specToSkill.Add(Specs.Regrowth, eProperty.Skill_Regrowth);
			m_specToSkill.Add(Specs.Nurture, eProperty.Skill_Nurture);
			m_specToSkill.Add(Specs.Music, eProperty.Skill_Music);
			m_specToSkill.Add(Specs.Valor, eProperty.Skill_Valor);
			m_specToSkill.Add(Specs.Nature, eProperty.Skill_Nature);
			m_specToSkill.Add(Specs.Light, eProperty.Skill_Light);
			m_specToSkill.Add(Specs.Void, eProperty.Skill_Void);
			m_specToSkill.Add(Specs.Mana, eProperty.Skill_Mana);
			m_specToSkill.Add(Specs.Enchantments, eProperty.Skill_Enchantments);
			m_specToSkill.Add(Specs.Mentalism, eProperty.Skill_Mentalism);
			m_specToSkill.Add(Specs.Nightshade_Magic, eProperty.Skill_Nightshade);
			m_specToSkill.Add(Specs.Pathfinding, eProperty.Skill_Pathfinding);
			m_specToSkill.Add(Specs.Dementia, eProperty.Skill_Dementia);
			m_specToSkill.Add(Specs.ShadowMastery, eProperty.Skill_ShadowMastery);
			m_specToSkill.Add(Specs.VampiiricEmbrace, eProperty.Skill_VampiiricEmbrace);
			m_specToSkill.Add(Specs.EtherealShriek, eProperty.Skill_EtherealShriek);
			m_specToSkill.Add(Specs.PhantasmalWail, eProperty.Skill_PhantasmalWail);
			m_specToSkill.Add(Specs.SpectralForce, eProperty.Skill_SpectralForce);
			m_specToSkill.Add(Specs.SpectralGuard, eProperty.Skill_SpectralGuard);

			#endregion

			#region Other

			//Other
			m_specToSkill.Add(Specs.Critical_Strike, eProperty.Skill_Critical_Strike);
			m_specToSkill.Add(Specs.Stealth, eProperty.Skill_Stealth);
			m_specToSkill.Add(Specs.Shields, eProperty.Skill_Shields);
			m_specToSkill.Add(Specs.Envenom, eProperty.Skill_Envenom);
			m_specToSkill.Add(Specs.Parry, eProperty.Skill_Parry);
			m_specToSkill.Add(Specs.ShortBow, eProperty.Skill_ShortBow);
			m_specToSkill.Add(Specs.Mauler_Staff, eProperty.Skill_MaulerStaff);
			m_specToSkill.Add(Specs.Fist_Wraps, eProperty.Skill_FistWraps);
			m_specToSkill.Add(Specs.Aura_Manipulation, eProperty.Skill_Aura_Manipulation);
			m_specToSkill.Add(Specs.Magnetism, eProperty.Skill_Magnetism);
			m_specToSkill.Add(Specs.Power_Strikes, eProperty.Skill_Power_Strikes);

			m_specToSkill.Add(Specs.Archery, eProperty.Skill_Archery);

			#endregion
		}

		/// <summary>
		/// Initialize the spec to focus tables
		/// </summary>
		private static void InitializeSpecToFocus()
		{
			m_specToFocus.Add(Specs.Darkness, eProperty.Focus_Darkness);
			m_specToFocus.Add(Specs.Suppression, eProperty.Focus_Suppression);
			m_specToFocus.Add(Specs.Runecarving, eProperty.Focus_Runecarving);
			m_specToFocus.Add(Specs.Spirit_Magic, eProperty.Focus_Spirit);
			m_specToFocus.Add(Specs.Fire_Magic, eProperty.Focus_Fire);
			m_specToFocus.Add(Specs.Wind_Magic, eProperty.Focus_Air);
			m_specToFocus.Add(Specs.Cold_Magic, eProperty.Focus_Cold);
			m_specToFocus.Add(Specs.Earth_Magic, eProperty.Focus_Earth);
			m_specToFocus.Add(Specs.Light, eProperty.Focus_Light);
			m_specToFocus.Add(Specs.Body_Magic, eProperty.Focus_Body);
			m_specToFocus.Add(Specs.Mind_Magic, eProperty.Focus_Mind);
			m_specToFocus.Add(Specs.Matter_Magic, eProperty.Focus_Matter);
			m_specToFocus.Add(Specs.Void, eProperty.Focus_Void);
			m_specToFocus.Add(Specs.Mana, eProperty.Focus_Mana);
			m_specToFocus.Add(Specs.Enchantments, eProperty.Focus_Enchantments);
			m_specToFocus.Add(Specs.Mentalism, eProperty.Focus_Mentalism);
			m_specToFocus.Add(Specs.Summoning, eProperty.Focus_Summoning);
			// SI
			m_specToFocus.Add(Specs.BoneArmy, eProperty.Focus_BoneArmy);
			m_specToFocus.Add(Specs.Painworking, eProperty.Focus_PainWorking);
			m_specToFocus.Add(Specs.Deathsight, eProperty.Focus_DeathSight);
			m_specToFocus.Add(Specs.Death_Servant, eProperty.Focus_DeathServant);
			m_specToFocus.Add(Specs.Verdant_Path, eProperty.Focus_Verdant);
			m_specToFocus.Add(Specs.Creeping_Path, eProperty.Focus_CreepingPath);
			m_specToFocus.Add(Specs.Arboreal_Path, eProperty.Focus_Arboreal);
			// Catacombs
			m_specToFocus.Add(Specs.EtherealShriek, eProperty.Focus_EtherealShriek);
			m_specToFocus.Add(Specs.PhantasmalWail, eProperty.Focus_PhantasmalWail);
			m_specToFocus.Add(Specs.SpectralForce, eProperty.Focus_SpectralForce);
			m_specToFocus.Add(Specs.Cursing, eProperty.Focus_Cursing);
			m_specToFocus.Add(Specs.Hexing, eProperty.Focus_Hexing);
			m_specToFocus.Add(Specs.Witchcraft, eProperty.Focus_Witchcraft);
		}

		/// <summary>
		/// Init property types table
		/// </summary>
		private static void InitPropertyTypes()
		{
			#region Resist

			// resists
			m_propertyTypes[(int)eProperty.Resist_Natural] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Body] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Cold] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Crush] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Energy] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Heat] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Matter] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Slash] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Spirit] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Thrust] = ePropertyType.Resist;

			#endregion

			#region Focus

			// focuses
			m_propertyTypes[(int)eProperty.Focus_Darkness] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Suppression] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Runecarving] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Spirit] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Fire] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Air] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Cold] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Earth] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Light] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Body] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Matter] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mind] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Void] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mana] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Enchantments] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Mentalism] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Summoning] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_BoneArmy] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_PainWorking] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_DeathSight] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_DeathServant] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Verdant] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_CreepingPath] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Arboreal] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_EtherealShriek] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_PhantasmalWail] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_SpectralForce] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Cursing] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Hexing] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.Focus_Witchcraft] = ePropertyType.Focus;
			m_propertyTypes[(int)eProperty.AllFocusLevels] = ePropertyType.Focus;

			#endregion


			/*
			 * http://www.camelotherald.com/more/1036.shtml
			 * "- ALL melee weapon skills - This bonus will increase your
			 * skill in many weapon types. This bonus does not increase shield,
			 * parry, archery skills, or dual wield skills (hand to hand is the
			 * exception, as this skill is also the main weapon skill associated
			 * with hand to hand weapons, and not just the off-hand skill). If
			 * your item has "All melee weapon skills: +3" and your character
			 * can train in hammer, axe and sword, your item should give you
			 * a +3 increase to all three."
			 */

			#region Melee Skills

			// skills
			m_propertyTypes[(int)eProperty.Skill_Two_Handed] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Critical_Strike] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Crushing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Flexible_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Polearms] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Slashing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Staff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Thrusting] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Sword] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Hammer] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Axe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Blades] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Blunt] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Piercing] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Large_Weapon] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Celtic_Spear] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Scythe] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_Thrown_Weapons] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_HandToHand] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_FistWraps] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;
			m_propertyTypes[(int)eProperty.Skill_MaulerStaff] = ePropertyType.Skill | ePropertyType.SkillMeleeWeapon;

			m_propertyTypes[(int)eProperty.Skill_Dual_Wield] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Left_Axe] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Celtic_Dual] = ePropertyType.Skill | ePropertyType.SkillDualWield;

			#endregion

			#region Magical Skills

			m_propertyTypes[(int)eProperty.Skill_Power_Strikes] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Magnetism] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Aura_Manipulation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Body] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Chants] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Death_Servant] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_DeathSight] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Earth] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Enhancement] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Fire] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Cold] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Instruments] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Matter] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mind] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pain_working] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Rejuvenation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Smiting] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SoulRending] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Spirit] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Wind] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mending] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Augmentation] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Darkness] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Suppression] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Runecarving] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Stormcalling] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_BeastCraft] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Light] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Void] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mana] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Battlesongs] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Enchantments] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Mentalism] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Regrowth] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nurture] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nature] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Music] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Valor] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Subterranean] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_BoneArmy] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Verdant] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Creeping] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Arboreal] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pacification] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Savagery] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Nightshade] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Pathfinding] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Summoning] = ePropertyType.Skill | ePropertyType.SkillMagical;

			// no idea about these
			m_propertyTypes[(int)eProperty.Skill_Dementia] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_ShadowMastery] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_VampiiricEmbrace] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_EtherealShriek] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_PhantasmalWail] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SpectralForce] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_SpectralGuard] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_OdinsWill] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Cursing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Hexing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Witchcraft] = ePropertyType.Skill | ePropertyType.SkillMagical;

			#endregion

			#region Other

			m_propertyTypes[(int)eProperty.Skill_Long_bows] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_Composite] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_RecurvedBow] = ePropertyType.Skill | ePropertyType.SkillArchery;

			m_propertyTypes[(int)eProperty.Skill_Parry] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Shields] = ePropertyType.Skill;

			m_propertyTypes[(int)eProperty.Skill_Stealth] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Cross_Bows] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_ShortBow] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Envenom] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Archery] = ePropertyType.Skill | ePropertyType.SkillArchery;

			#endregion
		}

		/// <summary>
		/// Initializes the race resist table
		/// </summary>
		private static void InitializeRaceResists()
		{
			#region Albion
			// http://camelot.allakhazam.com/Start_Stats.html
			// Alb
			m_raceResists[(int)eRace.Avalonian] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Avalonian].Add(eResist.Crush, 2);
			m_raceResists[(int)eRace.Avalonian].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Avalonian].Add(eResist.Spirit, 5);

			m_raceResists[(int)eRace.Briton] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Briton].Add(eResist.Crush, 2);
			m_raceResists[(int)eRace.Briton].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Briton].Add(eResist.Matter, 5);

			m_raceResists[(int)eRace.Highlander] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Highlander].Add(eResist.Crush, 3);
			m_raceResists[(int)eRace.Highlander].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Highlander].Add(eResist.Cold, 5);

			m_raceResists[(int)eRace.Saracen] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Saracen].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Saracen].Add(eResist.Thrust, 3);
			m_raceResists[(int)eRace.Saracen].Add(eResist.Heat, 5);

			m_raceResists[(int)eRace.Inconnu] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Inconnu].Add(eResist.Crush, 2);
			m_raceResists[(int)eRace.Inconnu].Add(eResist.Thrust, 3);
			m_raceResists[(int)eRace.Inconnu].Add(eResist.Heat, 5);
			m_raceResists[(int)eRace.Inconnu].Add(eResist.Spirit, 5);

			m_raceResists[(int)eRace.HalfOgre] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.HalfOgre].Add(eResist.Thrust, 2);
			m_raceResists[(int)eRace.HalfOgre].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.HalfOgre].Add(eResist.Matter, 5);

			#endregion

			#region Hibernia

			// Hib
			m_raceResists[(int)eRace.Celt] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Celt].Add(eResist.Crush, 2);
			m_raceResists[(int)eRace.Celt].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Celt].Add(eResist.Spirit, 5);

			m_raceResists[(int)eRace.Elf] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Elf].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Elf].Add(eResist.Thrust, 3);
			m_raceResists[(int)eRace.Elf].Add(eResist.Spirit, 5);

			m_raceResists[(int)eRace.Firbolg] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Firbolg].Add(eResist.Crush, 3);
			m_raceResists[(int)eRace.Firbolg].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Firbolg].Add(eResist.Heat, 5);

			m_raceResists[(int)eRace.Lurikeen] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Lurikeen].Add(eResist.Crush, 5);
			m_raceResists[(int)eRace.Lurikeen].Add(eResist.Energy, 5);

			m_raceResists[(int)eRace.Sylvan] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Sylvan].Add(eResist.Crush, 3);
			m_raceResists[(int)eRace.Sylvan].Add(eResist.Thrust, 2);
			m_raceResists[(int)eRace.Sylvan].Add(eResist.Matter, 5);
			m_raceResists[(int)eRace.Sylvan].Add(eResist.Energy, 5);

			m_raceResists[(int)eRace.Shar] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Shar].Add(eResist.Crush, 5);
			m_raceResists[(int)eRace.Shar].Add(eResist.Energy, 5);

			#endregion

			#region Midgard
			// Mid
			m_raceResists[(int)eRace.Dwarf] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Dwarf].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Dwarf].Add(eResist.Thrust, 3);
			m_raceResists[(int)eRace.Dwarf].Add(eResist.Body, 5);

			m_raceResists[(int)eRace.Kobold] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Kobold].Add(eResist.Crush, 5);
			m_raceResists[(int)eRace.Kobold].Add(eResist.Matter, 5);

			m_raceResists[(int)eRace.Troll] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Troll].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Troll].Add(eResist.Thrust, 2);
			m_raceResists[(int)eRace.Troll].Add(eResist.Matter, 5);

			m_raceResists[(int)eRace.Norseman] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Norseman].Add(eResist.Crush, 2);
			m_raceResists[(int)eRace.Norseman].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Norseman].Add(eResist.Cold, 5);

			m_raceResists[(int)eRace.Valkyn] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Valkyn].Add(eResist.Slash, 3);
			m_raceResists[(int)eRace.Valkyn].Add(eResist.Thrust, 2);
			m_raceResists[(int)eRace.Valkyn].Add(eResist.Cold, 5);
			m_raceResists[(int)eRace.Valkyn].Add(eResist.Body, 5);

			m_raceResists[(int)eRace.Frostalf] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.Frostalf].Add(eResist.Slash, 2);
			m_raceResists[(int)eRace.Frostalf].Add(eResist.Thrust, 3);
			m_raceResists[(int)eRace.Frostalf].Add(eResist.Spirit, 5);

			#endregion

			m_raceResists[(int)eRace.AlbionMinotaur] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.AlbionMinotaur].Add(eResist.Crush, 4);
			m_raceResists[(int)eRace.AlbionMinotaur].Add(eResist.Cold, 3);
			m_raceResists[(int)eRace.AlbionMinotaur].Add(eResist.Heat, 3);

			m_raceResists[(int)eRace.MidgardMinotaur] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.MidgardMinotaur].Add(eResist.Crush, 4);
			m_raceResists[(int)eRace.MidgardMinotaur].Add(eResist.Cold, 3);
			m_raceResists[(int)eRace.MidgardMinotaur].Add(eResist.Heat, 3);

			m_raceResists[(int)eRace.HiberniaMinotaur] = new Dictionary<eResist, int>();
			m_raceResists[(int)eRace.HiberniaMinotaur].Add(eResist.Crush, 4);
			m_raceResists[(int)eRace.HiberniaMinotaur].Add(eResist.Cold, 3);
			m_raceResists[(int)eRace.HiberniaMinotaur].Add(eResist.Heat, 3);


		}

		private static void RegisterPropertyNames()
		{
			#region register...
			m_propertyNames.Add(eProperty.Strength, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.Strength"));
			m_propertyNames.Add(eProperty.Dexterity, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Dexterity"));
			m_propertyNames.Add(eProperty.Constitution, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Constitution"));
			m_propertyNames.Add(eProperty.Quickness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Quickness"));
			m_propertyNames.Add(eProperty.Intelligence, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Intelligence"));
			m_propertyNames.Add(eProperty.Piety, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                "SkillBase.RegisterPropertyNames.Piety"));
			m_propertyNames.Add(eProperty.Empathy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Empathy"));
			m_propertyNames.Add(eProperty.Charisma, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.Charisma"));

			m_propertyNames.Add(eProperty.MaxMana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Power"));
			m_propertyNames.Add(eProperty.MaxHealth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Hits"));

			// resists (does not say "resist" on live server)
			m_propertyNames.Add(eProperty.Resist_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Body"));
			m_propertyNames.Add(eProperty.Resist_Natural, "Essence");
			m_propertyNames.Add(eProperty.Resist_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Cold"));
			m_propertyNames.Add(eProperty.Resist_Crush, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Crush"));
			m_propertyNames.Add(eProperty.Resist_Energy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Energy"));
			m_propertyNames.Add(eProperty.Resist_Heat, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Heat"));
			m_propertyNames.Add(eProperty.Resist_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Matter"));
			m_propertyNames.Add(eProperty.Resist_Slash, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Slash"));
			m_propertyNames.Add(eProperty.Resist_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Spirit"));
			m_propertyNames.Add(eProperty.Resist_Thrust, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Thrust"));

			// Eden - Mythirian bonus
			m_propertyNames.Add(eProperty.BodyResCapBonus, "Body cap");
			m_propertyNames.Add(eProperty.ColdResCapBonus, "Cold cap");
			m_propertyNames.Add(eProperty.CrushResCapBonus, "Crush cap");
			m_propertyNames.Add(eProperty.EnergyResCapBonus, "Energy cap");
			m_propertyNames.Add(eProperty.HeatResCapBonus, "Heat cap");
			m_propertyNames.Add(eProperty.MatterResCapBonus, "Matter cap");
			m_propertyNames.Add(eProperty.SlashResCapBonus, "Slash cap");
			m_propertyNames.Add(eProperty.SpiritResCapBonus, "Spirit cap");
			m_propertyNames.Add(eProperty.ThrustResCapBonus, "Thrust cap");
			//Eden - special actifacts bonus
			m_propertyNames.Add(eProperty.Conversion, "Conversion");
			m_propertyNames.Add(eProperty.ExtraHP, "Extra Health Points");
			m_propertyNames.Add(eProperty.StyleAbsorb, "Style Absorb");
			m_propertyNames.Add(eProperty.ArcaneSyphon, "Arcane Syphon");
			m_propertyNames.Add(eProperty.RealmPoints, "Realm Points");
			//[Freya] Nidel
			m_propertyNames.Add(eProperty.BountyPoints, "Bounty Points");
			m_propertyNames.Add(eProperty.XpPoints, "Xp points");

			// skills
			m_propertyNames.Add(eProperty.Skill_Two_Handed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.TwoHanded"));
			m_propertyNames.Add(eProperty.Skill_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.BodyMagic"));
			m_propertyNames.Add(eProperty.Skill_Chants, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Chants"));
			m_propertyNames.Add(eProperty.Skill_Critical_Strike, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.CriticalStrike"));
			m_propertyNames.Add(eProperty.Skill_Cross_Bows, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Crossbows"));
			m_propertyNames.Add(eProperty.Skill_Crushing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Crushing"));
			m_propertyNames.Add(eProperty.Skill_Death_Servant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.DeathServant"));
			m_propertyNames.Add(eProperty.Skill_DeathSight, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Deathsight"));
			m_propertyNames.Add(eProperty.Skill_Dual_Wield, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.DualWield"));
			m_propertyNames.Add(eProperty.Skill_Earth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EarthMagic"));
			m_propertyNames.Add(eProperty.Skill_Enhancement, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Enhancement"));
			m_propertyNames.Add(eProperty.Skill_Envenom, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Envenom"));
			m_propertyNames.Add(eProperty.Skill_Fire, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.FireMagic"));
			m_propertyNames.Add(eProperty.Skill_Flexible_Weapon, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.FlexibleWeapon"));
			m_propertyNames.Add(eProperty.Skill_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ColdMagic"));
			m_propertyNames.Add(eProperty.Skill_Instruments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Instruments"));
			m_propertyNames.Add(eProperty.Skill_Long_bows, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Longbows"));
			m_propertyNames.Add(eProperty.Skill_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.MatterMagic"));
			m_propertyNames.Add(eProperty.Skill_Mind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MindMagic"));
			m_propertyNames.Add(eProperty.Skill_Pain_working, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Painworking"));
			m_propertyNames.Add(eProperty.Skill_Parry, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Parry"));
			m_propertyNames.Add(eProperty.Skill_Polearms, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Polearms"));
			m_propertyNames.Add(eProperty.Skill_Rejuvenation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Rejuvenation"));
			m_propertyNames.Add(eProperty.Skill_Shields, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Shields"));
			m_propertyNames.Add(eProperty.Skill_Slashing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Slashing"));
			m_propertyNames.Add(eProperty.Skill_Smiting, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Smiting"));
			m_propertyNames.Add(eProperty.Skill_SoulRending, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Soulrending"));
			m_propertyNames.Add(eProperty.Skill_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.SpiritMagic"));
			m_propertyNames.Add(eProperty.Skill_Staff, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Staff"));
			m_propertyNames.Add(eProperty.Skill_Stealth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Stealth"));
			m_propertyNames.Add(eProperty.Skill_Thrusting, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Thrusting"));
			m_propertyNames.Add(eProperty.Skill_Wind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.WindMagic"));
			m_propertyNames.Add(eProperty.Skill_Sword, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Sword"));
			m_propertyNames.Add(eProperty.Skill_Hammer, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Hammer"));
			m_propertyNames.Add(eProperty.Skill_Axe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.Axe"));
			m_propertyNames.Add(eProperty.Skill_Left_Axe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.LeftAxe"));
			m_propertyNames.Add(eProperty.Skill_Spear, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Spear"));
			m_propertyNames.Add(eProperty.Skill_Mending, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Mending"));
			m_propertyNames.Add(eProperty.Skill_Augmentation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Augmentation"));
			m_propertyNames.Add(eProperty.Skill_Darkness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Darkness"));
			m_propertyNames.Add(eProperty.Skill_Suppression, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Suppression"));
			m_propertyNames.Add(eProperty.Skill_Runecarving, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Runecarving"));
			m_propertyNames.Add(eProperty.Skill_Stormcalling, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Stormcalling"));
			m_propertyNames.Add(eProperty.Skill_BeastCraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.BeastCraft"));
			m_propertyNames.Add(eProperty.Skill_Light, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.LightMagic"));
			m_propertyNames.Add(eProperty.Skill_Void, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.VoidMagic"));
			m_propertyNames.Add(eProperty.Skill_Mana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ManaMagic"));
			m_propertyNames.Add(eProperty.Skill_Composite, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Composite"));
			m_propertyNames.Add(eProperty.Skill_Battlesongs, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Battlesongs"));
			m_propertyNames.Add(eProperty.Skill_Enchantments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Enchantment"));

			m_propertyNames.Add(eProperty.Skill_Blades, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Blades"));
			m_propertyNames.Add(eProperty.Skill_Blunt, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Blunt"));
			m_propertyNames.Add(eProperty.Skill_Piercing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Piercing"));
			m_propertyNames.Add(eProperty.Skill_Large_Weapon, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.LargeWeapon"));
			m_propertyNames.Add(eProperty.Skill_Mentalism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Mentalism"));
			m_propertyNames.Add(eProperty.Skill_Regrowth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Regrowth"));
			m_propertyNames.Add(eProperty.Skill_Nurture, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Nurture"));
			m_propertyNames.Add(eProperty.Skill_Nature, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Nature"));
			m_propertyNames.Add(eProperty.Skill_Music, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Music"));
			m_propertyNames.Add(eProperty.Skill_Celtic_Dual, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.CelticDual"));
			m_propertyNames.Add(eProperty.Skill_Celtic_Spear, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CelticSpear"));
			m_propertyNames.Add(eProperty.Skill_RecurvedBow, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.RecurvedBow"));
			m_propertyNames.Add(eProperty.Skill_Valor, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.Valor"));
			m_propertyNames.Add(eProperty.Skill_Subterranean, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CaveMagic"));
			m_propertyNames.Add(eProperty.Skill_BoneArmy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.BoneArmy"));
			m_propertyNames.Add(eProperty.Skill_Verdant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Verdant"));
			m_propertyNames.Add(eProperty.Skill_Creeping, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Creeping"));
			m_propertyNames.Add(eProperty.Skill_Arboreal, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Arboreal"));
			m_propertyNames.Add(eProperty.Skill_Scythe, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Scythe"));
			m_propertyNames.Add(eProperty.Skill_Thrown_Weapons, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.ThrownWeapons"));
			m_propertyNames.Add(eProperty.Skill_HandToHand, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.HandToHand"));
			m_propertyNames.Add(eProperty.Skill_ShortBow, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ShortBow"));
			m_propertyNames.Add(eProperty.Skill_Pacification, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.Pacification"));
			m_propertyNames.Add(eProperty.Skill_Savagery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Savagery"));
			m_propertyNames.Add(eProperty.Skill_Nightshade, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.NightshadeMagic"));
			m_propertyNames.Add(eProperty.Skill_Pathfinding, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.Pathfinding"));
			m_propertyNames.Add(eProperty.Skill_Summoning, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Summoning"));
			m_propertyNames.Add(eProperty.Skill_Archery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Archery"));

			// Mauler
			m_propertyNames.Add(eProperty.Skill_FistWraps, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.FistWraps"));
			m_propertyNames.Add(eProperty.Skill_MaulerStaff, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.MaulerStaff"));
			m_propertyNames.Add(eProperty.Skill_Power_Strikes, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.PowerStrikes"));
			m_propertyNames.Add(eProperty.Skill_Magnetism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.Magnetism"));
			m_propertyNames.Add(eProperty.Skill_Aura_Manipulation, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                  "SkillBase.RegisterPropertyNames.AuraManipulation"));


			//Catacombs skills
			m_propertyNames.Add(eProperty.Skill_Dementia, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.Dementia"));
			m_propertyNames.Add(eProperty.Skill_ShadowMastery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.ShadowMastery"));
			m_propertyNames.Add(eProperty.Skill_VampiiricEmbrace, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.VampiiricEmbrace"));
			m_propertyNames.Add(eProperty.Skill_EtherealShriek, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EtherealShriek"));
			m_propertyNames.Add(eProperty.Skill_PhantasmalWail, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.PhantasmalWail"));
			m_propertyNames.Add(eProperty.Skill_SpectralForce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralForce"));
			m_propertyNames.Add(eProperty.Skill_SpectralGuard, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralGuard"));
			m_propertyNames.Add(eProperty.Skill_OdinsWill, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.OdinsWill"));
			m_propertyNames.Add(eProperty.Skill_Cursing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.Cursing"));
			m_propertyNames.Add(eProperty.Skill_Hexing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.Hexing"));
			m_propertyNames.Add(eProperty.Skill_Witchcraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Witchcraft"));


			// Classic Focii
			m_propertyNames.Add(eProperty.Focus_Darkness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.DarknessFocus"));
			m_propertyNames.Add(eProperty.Focus_Suppression, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.SuppressionFocus"));
			m_propertyNames.Add(eProperty.Focus_Runecarving, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.RunecarvingFocus"));
			m_propertyNames.Add(eProperty.Focus_Spirit, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.SpiritMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Fire, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.FireMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Air, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.WindMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Cold, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ColdMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Earth, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EarthMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Light, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.LightMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Body, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.BodyMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Matter, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.MatterMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Mind, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MindMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Void, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.VoidMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Mana, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ManaMagicFocus"));
			m_propertyNames.Add(eProperty.Focus_Enchantments, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.EnchantmentFocus"));
			m_propertyNames.Add(eProperty.Focus_Mentalism, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.MentalismFocus"));
			m_propertyNames.Add(eProperty.Focus_Summoning, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.SummoningFocus"));
			// SI Focii
			// Mid
			m_propertyNames.Add(eProperty.Focus_BoneArmy, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.BoneArmyFocus"));
			// Alb
			m_propertyNames.Add(eProperty.Focus_PainWorking, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.PainworkingFocus"));
			m_propertyNames.Add(eProperty.Focus_DeathSight, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.DeathsightFocus"));
			m_propertyNames.Add(eProperty.Focus_DeathServant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.DeathservantFocus"));
			// Hib
			m_propertyNames.Add(eProperty.Focus_Verdant, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.VerdantFocus"));
			m_propertyNames.Add(eProperty.Focus_CreepingPath, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.CreepingPathFocus"));
			m_propertyNames.Add(eProperty.Focus_Arboreal, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ArborealFocus"));
			// Catacombs Focii
			m_propertyNames.Add(eProperty.Focus_EtherealShriek, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EtherealShriekFocus"));
			m_propertyNames.Add(eProperty.Focus_PhantasmalWail, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.PhantasmalWailFocus"));
			m_propertyNames.Add(eProperty.Focus_SpectralForce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                              "SkillBase.RegisterPropertyNames.SpectralForceFocus"));
			m_propertyNames.Add(eProperty.Focus_Cursing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.CursingFocus"));
			m_propertyNames.Add(eProperty.Focus_Hexing, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.HexingFocus"));
			m_propertyNames.Add(eProperty.Focus_Witchcraft, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.WitchcraftFocus"));

			m_propertyNames.Add(eProperty.MaxSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                   "SkillBase.RegisterPropertyNames.MaximumSpeed"));
			m_propertyNames.Add(eProperty.MaxConcentration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.Concentration"));

			m_propertyNames.Add(eProperty.ArmorFactor, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.BonusToArmorFactor"));
			m_propertyNames.Add(eProperty.ArmorAbsorbtion, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                          "SkillBase.RegisterPropertyNames.BonusToArmorAbsorption"));

			m_propertyNames.Add(eProperty.HealthRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.HealthRegeneration"));
			m_propertyNames.Add(eProperty.PowerRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.PowerRegeneration"));
			m_propertyNames.Add(eProperty.EnduranceRegenerationRate, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                    "SkillBase.RegisterPropertyNames.EnduranceRegeneration"));
			m_propertyNames.Add(eProperty.SpellRange, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.SpellRange"));
			m_propertyNames.Add(eProperty.ArcheryRange, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ArcheryRange"));
			m_propertyNames.Add(eProperty.Acuity, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                 "SkillBase.RegisterPropertyNames.Acuity"));

			m_propertyNames.Add(eProperty.AllMagicSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.AllMagicSkills"));
			m_propertyNames.Add(eProperty.AllMeleeWeaponSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.AllMeleeWeaponSkills"));
			m_propertyNames.Add(eProperty.AllFocusLevels, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.ALLSpellLines"));
			m_propertyNames.Add(eProperty.AllDualWieldingSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.AllDualWieldingSkills"));
			m_propertyNames.Add(eProperty.AllArcherySkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                           "SkillBase.RegisterPropertyNames.AllArcherySkills"));

			m_propertyNames.Add(eProperty.LivingEffectiveLevel, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.EffectiveLevel"));


			//Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
			//Date : 20-Jan-2005
			//Missing bonusses begin
			m_propertyNames.Add(eProperty.EvadeChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EvadeChance"));
			m_propertyNames.Add(eProperty.BlockChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.BlockChance"));
			m_propertyNames.Add(eProperty.ParryChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.ParryChance"));
			m_propertyNames.Add(eProperty.FumbleChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.FumbleChance"));
			m_propertyNames.Add(eProperty.MeleeDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.MeleeDamage"));
			m_propertyNames.Add(eProperty.RangedDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.RangedDamage"));
			m_propertyNames.Add(eProperty.MesmerizeDuration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.MesmerizeDuration"));
			m_propertyNames.Add(eProperty.StunDuration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.StunDuration"));
			m_propertyNames.Add(eProperty.SpeedDecreaseDuration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                "SkillBase.RegisterPropertyNames.SpeedDecreaseDuration"));
			m_propertyNames.Add(eProperty.BladeturnReinforcement, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.BladeturnReinforcement"));
			m_propertyNames.Add(eProperty.DefensiveBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.DefensiveBonus"));
			m_propertyNames.Add(eProperty.PieceAblative, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.PieceAblative"));
			m_propertyNames.Add(eProperty.NegativeReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.NegativeReduction"));
			m_propertyNames.Add(eProperty.ReactionaryStyleDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.ReactionaryStyleDamage"));
			m_propertyNames.Add(eProperty.SpellPowerCost, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                         "SkillBase.RegisterPropertyNames.SpellPowerCost"));
			m_propertyNames.Add(eProperty.StyleCostReduction, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.StyleCostReduction"));
			m_propertyNames.Add(eProperty.ToHitBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.ToHitBonus"));
			m_propertyNames.Add(eProperty.ArcherySpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ArcherySpeed"));
			m_propertyNames.Add(eProperty.ArrowRecovery, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.ArrowRecovery"));
			m_propertyNames.Add(eProperty.BuffEffectiveness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.StatBuffSpells"));
			m_propertyNames.Add(eProperty.CastingSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.CastingSpeed"));
			m_propertyNames.Add(eProperty.DeathExpLoss, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ExperienceLoss"));
			m_propertyNames.Add(eProperty.DebuffEffectivness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                             "SkillBase.RegisterPropertyNames.DebuffEffectivness"));
			m_propertyNames.Add(eProperty.Fatigue, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                  "SkillBase.RegisterPropertyNames.Fatigue"));
			m_propertyNames.Add(eProperty.HealingEffectiveness, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                               "SkillBase.RegisterPropertyNames.HealingEffectiveness"));
			m_propertyNames.Add(eProperty.PowerPool, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.PowerPool"));
			//Magiekraftvorrat
			m_propertyNames.Add(eProperty.ResistPierce, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                       "SkillBase.RegisterPropertyNames.ResistPierce"));
			m_propertyNames.Add(eProperty.SpellDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.MagicDamageBonus"));
			m_propertyNames.Add(eProperty.SpellDuration, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                        "SkillBase.RegisterPropertyNames.SpellDuration"));
			m_propertyNames.Add(eProperty.StyleDamage, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.StyleDamage"));
			m_propertyNames.Add(eProperty.MeleeSpeed, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                     "SkillBase.RegisterPropertyNames.MeleeSpeed"));
			//Missing bonusses end

			m_propertyNames.Add(eProperty.StrCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.StrengthBonusCap"));
			m_propertyNames.Add(eProperty.DexCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.DexterityBonusCap"));
			m_propertyNames.Add(eProperty.ConCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.ConstitutionBonusCap"));
			m_propertyNames.Add(eProperty.QuiCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.QuicknessBonusCap"));
			m_propertyNames.Add(eProperty.IntCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.IntelligenceBonusCap"));
			m_propertyNames.Add(eProperty.PieCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.PietyBonusCap"));
			m_propertyNames.Add(eProperty.ChaCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.CharismaBonusCap"));
			m_propertyNames.Add(eProperty.EmpCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.EmpathyBonusCap"));
			m_propertyNames.Add(eProperty.AcuCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.AcuityBonusCap"));
			m_propertyNames.Add(eProperty.MaxHealthCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.HitPointsBonusCap"));
			m_propertyNames.Add(eProperty.PowerPoolCapBonus, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                            "SkillBase.RegisterPropertyNames.PowerBonusCap"));
			m_propertyNames.Add(eProperty.WeaponSkill, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                      "SkillBase.RegisterPropertyNames.WeaponSkill"));
			m_propertyNames.Add(eProperty.AllSkills, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                    "SkillBase.RegisterPropertyNames.AllSkills"));
			m_propertyNames.Add(eProperty.CriticalArcheryHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                   "SkillBase.RegisterPropertyNames.CriticalArcheryHit"));
			m_propertyNames.Add(eProperty.CriticalMeleeHitChance, LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE,
			                                                                                 "SkillBase.RegisterPropertyNames.CriticalMeleeHit"));
			#endregion
		}

		#endregion

		static SkillBase()
		{
			RegisterPropertyNames();
			InitArmorResists();
			InitPropertyTypes();
			InitializeObjectTypeToSpec();
			InitializeSpecToSkill();
			InitializeSpecToFocus();
			InitializeRaceResists();
		}

		public static void LoadSkills()
		{
			#region Load Spells

			//load all spells
			if (log.IsInfoEnabled)
				log.Info("Loading spells...");

			DataObject[] spelldb = GameServer.Database.SelectAllObjects(typeof(DBSpell));

			Dictionary<int, DBSpell> spells = new Dictionary<int, DBSpell>(spelldb.Length);

			foreach (DBSpell spell in spelldb)
			{
				spells.Add(spell.SpellID, spell);
				try
				{
					m_spells.Add(spell.SpellID, new Spell(spell, 1));
				}
				catch (Exception e)
				{
					log.Error(e.Message + " et spellid = " + spell.SpellID + " spell.TS= " + spell.ToString());
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Spells loaded: " + spelldb.Length);

			#endregion

			#region Load Spell Lines

			// load all spell lines
			DataObject[] dbo = GameServer.Database.SelectAllObjects(typeof(DBSpellLine));

			foreach (DBSpellLine line in dbo)
			{
				List<Spell> spell_list = new List<Spell>();

				DBLineXSpell[] dbo2 = (DBLineXSpell[])GameServer.Database.SelectObjects(typeof(DBLineXSpell), "LineName = '" + GameServer.Database.Escape(line.KeyName) + "'");

				foreach (DBLineXSpell lxs in dbo2)
				{
					DBSpell spell;
					if (!spells.TryGetValue(lxs.SpellID, out spell))
					{
						log.WarnFormat("Spell with ID {0} not found but is referenced from LineXSpell table", lxs.SpellID);
						continue;
					}
					spell_list.Add(new Spell(spell, lxs.Level));
				}

				spell_list.Sort(delegate(Spell sp1, Spell sp2) { return sp1.Level.CompareTo(sp2.Level); });
				m_spellLists.Add(line.KeyName, spell_list);

				RegisterSpellLine(new SpellLine(line.KeyName, line.Name, line.Spec, line.IsBaseLine));
				if (log.IsDebugEnabled)
					log.Debug("SpellLine: " + line.KeyName + ", " + dbo2.Length + " spells");
			}
			if (log.IsInfoEnabled)
				log.Info("Total spell lines loaded: " + dbo.Length);

			#endregion

			#region Load Abilities

			// load Abilities
			log.Info("Loading Abilities...");
			DataObject[] abilities = GameServer.Database.SelectAllObjects(typeof(DBAbility));
			if (abilities != null)
			{
				foreach (DBAbility dba in abilities)
				{
					m_abilitiesByName.Add(dba.KeyName, dba);

					if (dba.Implementation != null && dba.Implementation.Length > 0 && !m_implementationTypeCache.ContainsKey(dba.Implementation))
					{
						Type type = ScriptMgr.GetType(dba.Implementation);
						if (type != null)
						{
							Type typeCheck = new Ability(dba).GetType();
							if (type != typeCheck && type.IsSubclassOf(typeCheck))
								m_implementationTypeCache.Add(dba.Implementation, type);
							else
								log.Warn("Ability implementation " + dba.Implementation + " is not derived from Ability. Cannot be used.");
						}
						else
							log.Warn("Ability implementation " + dba.Implementation + " for ability " + dba.Name + " not found");
					}
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total abilities loaded: " + ((abilities != null) ? abilities.Length : 0));

			#endregion

			#region Load Class To Realm Ability

			log.Info("Loading class to realm ability associations...");
			DataObject[] classxra = GameServer.Database.SelectAllObjects(typeof(ClassXRealmAbility));

			if (classxra != null)
			{
				foreach (ClassXRealmAbility cxra in classxra)
				{
					List<RealmAbility> raList;

					if (!m_classRealmAbilities.ContainsKey(cxra.CharClass))
					{
						raList = new List<RealmAbility>();
						m_classRealmAbilities[cxra.CharClass] = raList;
					}
					else
						raList = m_classRealmAbilities[cxra.CharClass];

					Ability ab = GetAbility(cxra.AbilityKey, 1);

					if (ab.Name.StartsWith("?"))
						log.Warn("Realm Ability " + cxra.AbilityKey + " assigned to class " + cxra.CharClass + " but does not exist");
					else
					{
						if (ab is RealmAbility)
							raList.Add(ab as RealmAbility);
						else
							log.Warn(ab.Name + " is not a Realm Ability, this most likely is because no Implementation is set or an Implementation is set and is not a Realm Ability");
					}
				}
			}
			log.Info("Realm Abilities assigned to classes!");

			#endregion

			#region Load Procs

			//(procs) load all Procs
			if (log.IsInfoEnabled)
				log.Info("Loading procs...");
			DataObject[] stylespells = GameServer.Database.SelectAllObjects(typeof(DBStyleXSpell));
			if (stylespells != null)
			{
				foreach (DBStyleXSpell proc in stylespells)
				{
					Dictionary<int, List<DBStyleXSpell>> styleClasses;
					if (m_styleSpells.ContainsKey(proc.StyleID))
						styleClasses = m_styleSpells[proc.StyleID];
					else
					{
						styleClasses = new Dictionary<int, List<DBStyleXSpell>>();
						m_styleSpells.Add(proc.StyleID, styleClasses);
					}

					List<DBStyleXSpell> classSpells;
					if (styleClasses.ContainsKey(proc.ClassID))
						classSpells = styleClasses[proc.ClassID];
					else
					{
						classSpells = new List<DBStyleXSpell>();
						styleClasses.Add(proc.ClassID, classSpells);
					}

					classSpells.Add(proc);
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total procs loaded: " + ((stylespells != null) ? stylespells.Length : 0));

			#endregion

			#region Load Spec To Ability

			// load Specialization & styles
			if (log.IsInfoEnabled)
				log.Info("Loading specialization & styles...");
			DataObject[] specabilities = GameServer.Database.SelectAllObjects(typeof(DBSpecXAbility));
			if (specabilities != null)
			{
				foreach (DBSpecXAbility sxa in specabilities)
				{
					List<Ability> list;
					if (!m_specAbilities.TryGetValue(sxa.Spec, out list))
					{
						list = new List<Ability>();
						m_specAbilities.Add(sxa.Spec, list);
					}

					DBAbility dba;
					if (m_abilitiesByName.TryGetValue(sxa.AbilityKey, out dba))
						list.Add(new Ability(dba, sxa.AbilityLevel, sxa.Spec, sxa.SpecLevel));
					else if (log.IsWarnEnabled)
						log.Warn("Associated ability " + sxa.AbilityKey + " for specialization " + sxa.Spec + " not found!");
				}
			}

			#endregion

			#region Load Specializations

			DataObject[] specs = GameServer.Database.SelectAllObjects(typeof(DBSpecialization));
			if (specs != null)
			{
				foreach (DBSpecialization spec in specs)
				{
					if (spec.Styles != null)
					{
						foreach (DBStyle style in spec.Styles)
						{
							string hashKey = string.Format("{0}|{1}", style.SpecKeyName, style.ClassId);
							List<Style> styleList;
							if (!m_styleLists.TryGetValue(hashKey, out styleList))
							{
								styleList = new List<Style>();
								m_styleLists.Add(hashKey, styleList);
							}

							Style st = new Style(style);

							//(procs) Add procs to the style, 0 is used for normal style
							if (m_styleSpells.ContainsKey(st.ID))
							{
								// now we add every proc to the style (even if ClassID != 0)
								foreach (byte classID in Enum.GetValues(typeof(eCharacterClass)))
								{
									if (m_styleSpells[st.ID].ContainsKey(classID))
									{
										foreach (DBStyleXSpell styleSpells in m_styleSpells[st.ID][classID])
											st.Procs.Add(styleSpells);
									}
								}
							}
							styleList.Add(st);

							long styleKey = ((long)st.ID << 32) | (uint)style.ClassId;
							if (!m_stylesByIDClass.ContainsKey(styleKey))
								m_stylesByIDClass.Add(styleKey, st);
						}
					}
					RegisterSpec(new Specialization(spec.KeyName, spec.Name, spec.Icon));
					int specAbCount = 0;
					if (m_specAbilities.ContainsKey(spec.KeyName))
						specAbCount = m_specAbilities[spec.KeyName].Count;

					if (log.IsDebugEnabled)
					{
						int styleCount = 0;
						if (spec.Styles != null)
							styleCount = spec.Styles.Length;
						log.Debug("Specialization: " + spec.Name + ", " + styleCount + " styles, " + specAbCount + " abilities");
					}
				}

				// We've added all the styles to their respective lists.  Now lets go through and sort them by their level

				SortStylesByLevel();
			}
			if (log.IsInfoEnabled)
				log.Info("Total specializations loaded: " + ((specs != null) ? specs.Length : 0));

			#endregion

			#region Load Ability Handlers

			// load skill action handlers
			//Search for ability handlers in the gameserver first
			if (log.IsInfoEnabled)
				log.Info("Searching ability handlers in GameServer");
			Hashtable ht = ScriptMgr.FindAllAbilityActionHandler(Assembly.GetExecutingAssembly());
			foreach (DictionaryEntry entry in ht)
			{
				string key = (string)entry.Key;

				if (log.IsDebugEnabled)
					log.Debug("\tFound ability handler for " + key);

				if (!m_abilityActionHandler.ContainsKey(key))
					m_abilityActionHandler.Add(key, (Type)entry.Value);
				else if (log.IsWarnEnabled)
					log.Warn("Duplicate type handler for: " + key);
			}

			//Now search ability handlers in the scripts directory and overwrite the ones
			//found from gameserver
			if (log.IsInfoEnabled)
				log.Info("Searching AbilityHandlers in Scripts");
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				ht = ScriptMgr.FindAllAbilityActionHandler(asm);
				foreach (DictionaryEntry entry in ht)
				{
					string message;
					string key = (string)entry.Key;

					if (m_abilityActionHandler.ContainsKey(key))
					{
						message = "\tFound new ability handler for " + key;
						m_abilityActionHandler[key] = (Type)entry.Value;
					}
					else
					{
						message = "\tFound ability handler for " + key;
						m_abilityActionHandler.Add(key, (Type)entry.Value);
					}

					if (log.IsDebugEnabled)
						log.Debug(message);
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total ability handlers loaded: " + m_abilityActionHandler.Keys.Count);

			#endregion

			#region Load Skill Handlers

			//Search for skill handlers in gameserver first
			if (log.IsInfoEnabled)
				log.Info("Searching skill handlers in GameServer.");
			ht = ScriptMgr.FindAllSpecActionHandler(Assembly.GetExecutingAssembly());
			foreach (DictionaryEntry entry in ht)
			{
				string key = (string)entry.Key;

				if (log.IsDebugEnabled)
					log.Debug("\tFound skill handler for " + key);

				if (!m_specActionHandler.ContainsKey(key))
					m_specActionHandler.Add(key, (Type)entry.Value);
				else if (log.IsWarnEnabled)
					log.Warn("Duplicate type handler for: " + key);
			}
			//Now search skill handlers in the scripts directory and overwrite the ones
			//found from the gameserver
			if (log.IsInfoEnabled)
				log.Info("Searching skill handlers in Scripts.");
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				ht = ScriptMgr.FindAllSpecActionHandler(asm);
				foreach (DictionaryEntry entry in ht)
				{
					string message;
					string key = (string)entry.Key;

					if (m_specActionHandler.ContainsKey(key))
					{
						message = "Found new skill handler for " + key;
						m_specActionHandler[key] = (Type)entry.Value;
					}
					else
					{
						message = "Found skill handler for " + key;
						m_specActionHandler.Add(key, (Type)entry.Value);
					}

					if (log.IsDebugEnabled)
						log.Debug(message);
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total skill handlers loaded: " + m_specActionHandler.Keys.Count);

			#endregion

		}

		#region Armor resists

		// lookup table for armor resists
		private const int REALM_BITCOUNT = 2;
		private const int DAMAGETYPE_BITCOUNT = 4;
		private const int ARMORTYPE_BITCOUNT = 3;
		private static readonly int[] m_armorResists = new int[1 << (REALM_BITCOUNT + DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT)];

		/// <summary>
		/// Gets the natural armor resist to the give damage type
		/// </summary>
		/// <param name="armor"></param>
		/// <param name="damageType"></param>
		/// <returns>resist value</returns>
		public static int GetArmorResist(ItemTemplate armor, eDamageType damageType)
		{
			if (armor == null) return 0;
			int realm = armor.Realm - (int)eRealm._First;
			int armorType = armor.Object_Type - (int)eObjectType._FirstArmor;
			int damage = damageType - eDamageType._FirstResist;
			if (realm < 0 || realm > eRealm._LastPlayerRealm - eRealm._First) return 0;
			if (armorType < 0 || armorType > eObjectType._LastArmor - eObjectType._FirstArmor) return 0;
			if (damage < 0 || damage > eDamageType._LastResist - eDamageType._FirstResist) return 0;

			const int realmBits = DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT;

			return m_armorResists[(realm << realmBits) | (armorType << DAMAGETYPE_BITCOUNT) | damage];
		}

		private static void InitArmorResists()
		{
			const int value_high 	= 10;
			const int value_low 	= 5;
			const int value_neutral = 0;

			// melee resists (slash, crush, thrust)

			// alb armor - neutral to slash
			// plate and leather resistant to thrust
			// chain and studded vulnerable to thrust
			WriteMeleeResists(eRealm.Albion, eObjectType.Leather, value_neutral, -value_low, value_high);
			WriteMeleeResists(eRealm.Albion, eObjectType.Plate,   value_neutral, -value_low, value_high);
			WriteMeleeResists(eRealm.Albion, eObjectType.Studded, value_neutral, value_high, -value_low);
			WriteMeleeResists(eRealm.Albion, eObjectType.Chain,   value_neutral, value_high, -value_low);


			// hib armor - neutral to thrust
			// reinforced and leather vulnerable to crush
			// scale resistant to crush
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Reinforced, value_high, -value_low, value_neutral);
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Leather,    value_high, -value_low, value_neutral);
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Scale,      -value_low, value_high, value_neutral);


			// mid armor - neutral to crush
			// studded and leather resistant to thrust
			// chain vulnerabel to thrust
			WriteMeleeResists(eRealm.Midgard, eObjectType.Studded, -value_low, value_neutral, value_high);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Leather, -value_low, value_neutral, value_high);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Chain,   value_high, value_neutral, -value_low);


			// magical damage (Heat, Cold, Matter, Energy)
			// Leather
			WriteMagicResists(eRealm.Albion,   eObjectType.Leather, -value_low, value_high, value_low, value_neutral);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Leather, -value_low, value_high, value_low, value_neutral);
			WriteMagicResists(eRealm.Midgard,  eObjectType.Leather, -value_low, value_high, value_low, value_neutral);

			// Reinforced/Studded
			WriteMagicResists(eRealm.Albion,   eObjectType.Studded,    value_high, -value_low, -value_low, -value_low);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Reinforced, value_high, -value_low, -value_low, -value_low);
			WriteMagicResists(eRealm.Midgard,  eObjectType.Studded,    value_high, -value_low, -value_low, -value_low);

			// Chain
			WriteMagicResists(eRealm.Albion,  eObjectType.Chain, value_high, value_neutral, value_neutral, -value_low);
			WriteMagicResists(eRealm.Midgard, eObjectType.Chain, value_high, value_neutral, value_neutral, -value_low);

			// Scale/Plate
			WriteMagicResists(eRealm.Albion,   eObjectType.Plate, value_high, -value_low, value_high, -value_low);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Scale, value_high, -value_low, value_high, -value_low);
		}

		private static void WriteMeleeResists(eRealm realm, eObjectType armorType, int slash, int crush, int thrust)
		{
			if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
				throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
			if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
				throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");

			int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
			off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
			m_armorResists[off + (eDamageType.Slash - eDamageType._FirstResist)] = slash;
			m_armorResists[off + (eDamageType.Crush - eDamageType._FirstResist)] = crush;
			m_armorResists[off + (eDamageType.Thrust - eDamageType._FirstResist)] = thrust;
		}

		private static void WriteMagicResists(eRealm realm, eObjectType armorType, int heat, int cold, int matter, int energy)
		{
			if (realm < eRealm._First || realm > eRealm._LastPlayerRealm)
				throw new ArgumentOutOfRangeException("realm", realm, "Realm should be between _First and _LastPlayerRealm.");
			if (armorType < eObjectType._FirstArmor || armorType > eObjectType._LastArmor)
				throw new ArgumentOutOfRangeException("armorType", armorType, "Armor type should be between _FirstArmor and _LastArmor");

			int off = (realm - eRealm._First) << (DAMAGETYPE_BITCOUNT + ARMORTYPE_BITCOUNT);
			off |= (armorType - eObjectType._FirstArmor) << DAMAGETYPE_BITCOUNT;
			m_armorResists[off + (eDamageType.Heat - eDamageType._FirstResist)] = -heat;
			m_armorResists[off + (eDamageType.Cold - eDamageType._FirstResist)] = -cold;
			m_armorResists[off + (eDamageType.Matter - eDamageType._FirstResist)] = -matter;
			m_armorResists[off + (eDamageType.Energy - eDamageType._FirstResist)] = -energy;
		}

		#endregion

		/// <summary>
		/// Check if property belongs to all of specified types
		/// </summary>
		/// <param name="prop">The property to check</param>
		/// <param name="type">The types to check</param>
		/// <returns>true if property belongs to all types</returns>
		public static bool CheckPropertyType(eProperty prop, ePropertyType type)
		{
			int property = (int)prop;
			if (property < 0 || property >= m_propertyTypes.Length)
				return false;

			return (m_propertyTypes[property] & type) == type;
		}

		/// <summary>
		/// Gets a new AbilityActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static IAbilityActionHandler GetAbilityActionHandler(string keyName)
		{
			Type handlerType;
			if (m_abilityActionHandler.TryGetValue(keyName, out handlerType))
			{
				try
				{
					return Activator.CreateInstance(handlerType) as IAbilityActionHandler;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Can't instantiate AbilityActionHandler " + handlerType, e);
				}
			}
			return null;
		}

		/// <summary>
		/// Gets a new SpecActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static ISpecActionHandler GetSpecActionHandler(string keyName)
		{
			Type handlerType;
			if (m_specActionHandler.TryGetValue(keyName, out handlerType))
			{
				try
				{
					return Activator.CreateInstance(handlerType) as ISpecActionHandler;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Can't instantiate SpecActionHandler " + handlerType, e);
				}
			}
			return null;
		}

		public static void RegisterSpellLine(SpellLine line)
		{
			if (m_spellLinesByName.ContainsKey(line.KeyName))
				m_spellLinesByName[line.KeyName] = line;
			else
				m_spellLinesByName.Add(line.KeyName, line);
		}

		/// <summary>
		/// Add a new style to a specialization.  If the specialization does not exist it will be created.
		/// After adding all styles call SortStyles to sort the list by level
		/// </summary>
		/// <param name="style"></param>
		public static void AddStyle(Specialization spec, DBStyle style)
		{
			string hashKey = string.Format("{0}|{1}", style.SpecKeyName, style.ClassId);
			List<Style> styleList;
			if (!m_styleLists.TryGetValue(hashKey, out styleList))
			{
				styleList = new List<Style>();
				m_styleLists.Add(hashKey, styleList);
			}

			Style st = new Style(style);

			//(procs) Add procs to the style, 0 is used for normal style
			if (m_styleSpells.ContainsKey(st.ID))
			{
				// now we add every proc to the style (even if ClassID != 0)
				foreach (byte classID in Enum.GetValues(typeof(eCharacterClass)))
				{
					if (m_styleSpells[st.ID].ContainsKey(classID))
					{
						foreach (DBStyleXSpell styleSpells in m_styleSpells[st.ID][classID])
							st.Procs.Add(styleSpells);
					}
				}
			}
			styleList.Add(st);

			long styleKey = ((long)st.ID << 32) | (uint)style.ClassId;
			if (!m_stylesByIDClass.ContainsKey(styleKey))
				m_stylesByIDClass.Add(styleKey, st);

			if (!m_specsByName.ContainsKey(spec.KeyName))
				RegisterSpec(spec);
		}

		public static void SortStylesByLevel()
		{
			Dictionary<string, List<Style>>.Enumerator enumer = m_styleLists.GetEnumerator();
			while (enumer.MoveNext())
				enumer.Current.Value.Sort(delegate(Style style1, Style style2) { return style1.SpecLevelRequirement.CompareTo(style2.SpecLevelRequirement); });
		}

		public static void RegisterSpec(Specialization spec)
		{
			if (m_specsByName.ContainsKey(spec.KeyName))
				m_specsByName[spec.KeyName] = spec;
			else
				m_specsByName.Add(spec.KeyName, spec);
		}

		public static void UnRegisterSpellLine(string LineKeyName)
		{
			if (m_spellLinesByName.ContainsKey(LineKeyName))
				m_spellLinesByName.Remove(LineKeyName);
		}

		/// <summary>
		/// returns level 1 instantiated realm abilities, only for readonly use!
		/// </summary>
		/// <param name="classID"></param>
		/// <returns></returns>
		public static List<RealmAbility> GetClassRealmAbilities(int classID)
		{
			if (m_classRealmAbilities.ContainsKey(classID))
				return m_classRealmAbilities[classID];
			else
				return new List<RealmAbility>();
		}

		public static Ability getClassRealmAbility(int charclass)
		{
			List<RealmAbility> abis = GetClassRealmAbilities(charclass);
			foreach (Ability ab in abis)
			{
				if (ab is RR5RealmAbility)
					return ab;
			}
			return null;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Ability GetAbility(string keyname)
		{
			return GetAbility(keyname, 1);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="keyname"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public static Ability GetAbility(string keyname, int level)
		{
			if (m_abilitiesByName.ContainsKey(keyname))
			{
				DBAbility dba = m_abilitiesByName[keyname];

				Type type = null;
				if (dba.Implementation != null && m_implementationTypeCache.ContainsKey(dba.Implementation))
					type = m_implementationTypeCache[dba.Implementation];
				else
					return new Ability(dba, level);

				return (Ability)Activator.CreateInstance(type, new object[] { dba, level });
			}

			if (log.IsWarnEnabled)
				log.Warn("Ability '" + keyname + "' unknown");

			return new Ability(keyname, "?" + keyname, "", 0, 0);
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static SpellLine GetSpellLine(string keyname)
		{
			if (keyname == GlobalSpellsLines.Mob_Spells)
				return new SpellLine("Mob Spells", "Mob Spells", "", true);

			if (m_spellLinesByName.ContainsKey(keyname))
				return m_spellLinesByName[keyname].Clone() as SpellLine;

			if (log.IsWarnEnabled)
				log.Warn("Spell-Line " + keyname + " unknown");

			return new SpellLine(keyname, "?" + keyname, "", true);
		}

		public static void CleanSpellList(string spellLineID)
		{
			if (m_spellLists.ContainsKey(spellLineID))
				m_spellLists.Remove(spellLineID);
		}

		public static void AddSpellToList(string spellLineID, int SpellID)
		{
			List<Spell> list;

			if (!m_spellLists.TryGetValue(spellLineID, out list))
			{
				list = new List<Spell>();
				m_spellLists.Add(spellLineID, list);
			}

			Spell spl = GetSpellByID(SpellID);

			if (spl != null)
				list.Add(spl);
			else
				log.Error("Missing CL Spell: " + SpellID);

			list.Sort(delegate(Spell sp1, Spell sp2) { return sp1.ID.CompareTo(sp2.ID); });

			for (int i = 0; i < list.Count; i++)
				list[i].Level = i + 1;
		}

		public static bool AddSpellToSpellLine(string spellLineID, Spell spellparam)
		{
			if (spellparam == null)
				return false;

			List<Spell> list = null;

			if (m_spellLists.ContainsKey(spellLineID))
				list = m_spellLists[spellLineID];

			// Make a copy of the spell passed in, making this a unique spell so we can set the level
			Spell newspell = spellparam.Copy();

			if (list != null)
			{
				if (list.Count > 49)
					return false;  // can only have spells up to level 50

				foreach (Spell spell in list)
				{
					if (spell.Name == spellparam.Name)
						return false; // spell already in spellline
				}

				newspell.Level = list.Count + 1;
				list.Add(newspell);
			}
			else
			{
				list = new List<Spell>();
				newspell.Level = 1;
				list.Add(newspell);
				m_spellLists[spellLineID] = list;
			}

			return true;
		}

		/// <summary>
		///
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Specialization GetSpecialization(string keyname)
		{
			if (m_specsByName.ContainsKey(keyname))
			{
				Specialization spec = m_specsByName[keyname] as Specialization;
				if (spec != null)
				{
					return (Specialization)spec.Clone();
				}
			}
			if (log.IsWarnEnabled)
				log.Warn("Specialization " + keyname + " unknown");
			return new Specialization(keyname, "?" + keyname, 0);
		}

		/// <summary>
		/// return all styles for a specific specialization
		/// if no style are associated or spec is unknown the list will be empty
		/// </summary>
		/// <param name="specID">KeyName of spec</param>
		/// <param name="classId">ClassID for which style list is requested</param>
		/// <returns>list of styles, never null</returns>
		public static List<Style> GetStyleList(string specID, int classId)
		{
			List<Style> list;
			if (m_styleLists.TryGetValue(specID + "|" + classId, out list))
				return list;
			else
				return new List<Style>(0);
		}

		/// <summary>
		/// returns spec dependend abilities
		/// </summary>
		/// <param name="specID">KeyName of spec</param>
		/// <returns>list of abilities or empty list</returns>
		public static List<Ability> GetSpecAbilityList(string specID)
		{
			List<Ability> list;
			if (m_specAbilities.TryGetValue(specID, out list))
				return list;
			else
				return new List<Ability>(0);
		}

		/// <summary>
		/// return all spells for a specific spell-line
		/// if no spells are associated or spell-line is unknown the list will be empty
		/// </summary>
		/// <param name="spellLineID">KeyName of spell-line</param>
		/// <returns>list of spells, never null</returns>
		public static List<Spell> GetSpellList(string spellLineID)
		{
			List<Spell> list;
			if (m_spellLists.TryGetValue(spellLineID, out list))
				return list;
			else
				return new List<Spell>(0);
		}

		/// <summary>
		/// find style with specific id
		/// </summary>
		/// <param name="styleID">id of style</param>
		/// <param name="classId">ClassID for which style list is requested</param>
		/// <returns>style or null if not found</returns>
		public static Style GetStyleByID(int styleID, int classId)
		{
			long key = ((long)styleID << 32) | (uint)classId;
			Style style;
			if (m_stylesByIDClass.TryGetValue(key, out style))
				return style;
			return null;
		}

		/// <summary>
		/// Returns spell with id, level of spell is always 1
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static Spell GetSpellByID(int spellID)
		{
			Spell spell;
			if (m_spells.TryGetValue(spellID, out spell))
				return spell;
			return null;
		}

		/// <summary>
		/// Get display name of property
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static string GetPropertyName(eProperty prop)
		{
			string name;
			if (!m_propertyNames.TryGetValue(prop, out name))
			{
				name = "Property" + ((int)prop);
			}
			return name;
		}

		/// <summary>
		/// determine race dependend base resist
		/// </summary>
		/// <param name="race"></param>
		/// <param name="type"></param>
		/// <returns></returns>
		public static int GetRaceResist(eRace race, eResist type)
		{
			Dictionary<eResist, int> resists = m_raceResists[(int)race];
			if (resists == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("No resists for race " + race + " defined");
				return 0;
			}

			if (!resists.ContainsKey(type))
				return 0;

			return resists[type];
		}

		/// <summary>
		/// Convert object type to spec needed to use that object
		/// </summary>
		/// <param name="objectType">type of the object</param>
		/// <returns>spec names needed to use that object type</returns>
		public static string ObjectTypeToSpec(eObjectType objectType)
		{
			string res = null;
			if (!m_objectTypeToSpec.TryGetValue(objectType, out res))
				if (log.IsWarnEnabled)
				log.Warn("Not found spec for object type " + objectType);
			return res;
		}

		/// <summary>
		/// Convert spec to skill property
		/// </summary>
		/// <param name="specKey"></param>
		/// <returns></returns>
		public static eProperty SpecToSkill(string specKey)
		{
			eProperty res;
			if (!m_specToSkill.TryGetValue(specKey, out res))
			{
				//if (log.IsWarnEnabled)
				//log.Warn("No skill property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return res;
		}

		/// <summary>
		/// Convert spec to focus
		/// </summary>
		/// <param name="specKey"></param>
		/// <returns></returns>
		public static eProperty SpecToFocus(string specKey)
		{
			eProperty res;
			if (!m_specToFocus.TryGetValue(specKey, out res))
			{
				//if (log.IsWarnEnabled)
				//log.Warn("No skill property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return res;
		}
	}
}
