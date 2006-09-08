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
using System.Text;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.GS.RealmAbilities;
using DOL.GS.Scripts;
using DOL.GS.Styles;
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

		public virtual string Name
		{
			get { return m_name; }
		}

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

		protected static readonly Hashtable m_specsByName = new Hashtable();
		protected static readonly Hashtable m_abilitiesByName = new Hashtable();
		protected static readonly Hashtable m_spellLinesByName = new Hashtable();
		//protected SkillBase m_instance;
		protected static readonly Hashtable m_abilityActionHandler = new Hashtable();
		protected static readonly Hashtable m_implementationTypeCache = new Hashtable();
		protected static readonly Hashtable m_specActionHandler = new Hashtable();
		protected static HybridDictionary[] m_raceResists = null;

		// global table for spellLine => List of spells
		protected static readonly Hashtable m_spellLists = new Hashtable();

		// global table for spec => List of styles
		protected static readonly Hashtable m_styleLists = new Hashtable();

		// global table for spec => list of spec dependend abilities
		protected static readonly Hashtable m_specAbilities = new Hashtable();

		// lookup table for styles
		protected static readonly Hashtable m_stylesByID = new Hashtable();
		protected static readonly Hashtable m_stylesByIDClass = new Hashtable();

		// lookup table for property names
		protected static readonly Hashtable m_propertyNames = new Hashtable();

		// class id => realm ability list
		protected static readonly Hashtable m_classRealmAbilities = new Hashtable();

		// all spells by id
		protected static readonly Hashtable m_spells = new Hashtable(3000);

		static SkillBase()
		{
			InitArmorResists();
			InitPropertyTypes();
		}

		private SkillBase()
		{
		}

		public static void LoadSkills()
		{
			RegisterPropertyNames();

			//load all spells
			if (log.IsInfoEnabled)
				log.Info("Loading spells...");
			Hashtable spells = new Hashtable(5000);
			DataObject[] spelldb = GameServer.Database.SelectAllObjects(typeof(DBSpell));
			for (int i = 0; i < spelldb.Length; i++)
			{
				DBSpell spell = (DBSpell)spelldb[i];
				spells[spell.SpellID] = spell;
				m_spells[spell.SpellID] = new Spell(spell, 1);
			}
			if (log.IsInfoEnabled)
				log.Info("Spells loaded: " + spelldb.Length);


			// load all spell lines
			DataObject[] dbo = GameServer.Database.SelectAllObjects(typeof(DBSpellLine));
			for (int i = 0; i < dbo.Length; i++)
			{
				string lineID = ((DBSpellLine)dbo[i]).KeyName;
				string lineName = ((DBSpellLine)dbo[i]).Name;
				string spec = ((DBSpellLine)dbo[i]).Spec;
				bool baseline = ((DBSpellLine)dbo[i]).IsBaseLine;
				ArrayList spell_list = new ArrayList();
				DBLineXSpell[] dbo2 = (DBLineXSpell[])GameServer.Database.SelectObjects(typeof(DBLineXSpell), "LineName = '" + GameServer.Database.Escape(lineID) + "'");
				foreach (DBLineXSpell lxs in dbo2)
				{
					DBSpell spell = (DBSpell)spells[lxs.SpellID];
					if (spell == null)
					{
						log.WarnFormat("Spell with ID {0} not found but is referenced from LineXSpell table", lxs.SpellID);
						continue;
					}
					// find right position for insert
					int insertpos = 0;
					for (insertpos = 0; insertpos < spell_list.Count; insertpos++)
					{
						if (lxs.Level < ((Spell)spell_list[insertpos]).Level)
							break;
					}
					spell_list.Insert(insertpos, new Spell(spell, lxs.Level));
				}
				m_spellLists[lineID] = spell_list;
				RegisterSpellLine(new SpellLine(lineID, lineName, spec, baseline));
				if (log.IsDebugEnabled)
					log.Debug("SpellLine: " + lineID + ", " + dbo2.Length + " spells");
			}
			if (log.IsInfoEnabled)
				log.Info("Total spell lines loaded: " + dbo.Length);

			// load Abilities
			log.Info("Loading Abilities...");
			DataObject[] abilities = GameServer.Database.SelectAllObjects(typeof(DBAbility));
			if (abilities != null)
			{
				foreach (DBAbility dba in abilities)
				{
					m_abilitiesByName[dba.KeyName] = dba;
					if (dba.Implementation != null && dba.Implementation.Length > 0)
					{
						if (m_implementationTypeCache[dba.Implementation] == null)
						{ // not in cache yet
							Type type = ScriptMgr.GetType(dba.Implementation);
							if (type != null)
							{
								if (type != new Ability(dba).GetType() && type.IsSubclassOf(new Ability(dba).GetType()))
								{
									m_implementationTypeCache[dba.Implementation] = type;
								}
								else
								{
									log.Warn("Ability implementation " + dba.Implementation + " is not derived from Ability. Cannot be used.");
								}
							}
							else
							{
								log.Warn("Ability implementation " + dba.Implementation + " for ability " + dba.Name + " not found");
							}
						}
					}
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total abilities loaded: " + ((abilities != null) ? abilities.Length : 0));

			log.Info("Loading class to realm ability associations...");
			DataObject[] classxra = GameServer.Database.SelectAllObjects(typeof(ClassXRealmAbility));
			int count = 0;
			if (classxra != null)
			{
				foreach (ClassXRealmAbility cxra in classxra)
				{
					IList raList = (IList)m_classRealmAbilities[cxra.CharClass];
					if (raList == null)
					{
						raList = new ArrayList();
						m_classRealmAbilities[cxra.CharClass] = raList;
					}
					Ability ab = GetAbility(cxra.AbilityKey, 1);
					if (ab.Name.StartsWith("?"))
					{
						log.Warn("Realm Ability " + cxra.AbilityKey + " assigned to class " + cxra.CharClass + " but does not exist");
					}
					else
					{
						if (ab is RealmAbility)
						{
							raList.Add(ab);
							count++;
						}
						else
						{
							log.Warn(ab.Name + " is not a Realm Ability, this most likely is because no Implementation is set or an Implementation is set and is not a Realm Ability");
						}
					}
				}
			}
			log.Info("Realm Abilities assigned to classes: " + count);

			// load Specialization & styles
			if (log.IsInfoEnabled)
				log.Info("Loading specialization & styles...");
			DataObject[] specabilities = GameServer.Database.SelectAllObjects(typeof(DBSpecXAbility));
			if (specabilities != null)
			{
				foreach (DBSpecXAbility sxa in specabilities)
				{
					ArrayList list = (ArrayList)m_specAbilities[sxa.Spec];
					if (list == null)
					{
						list = new ArrayList();
						m_specAbilities[sxa.Spec] = list;
					}
					DBAbility dba = (DBAbility)m_abilitiesByName[sxa.AbilityKey];
					if (dba != null)
					{
						list.Add(new Ability(dba, sxa.AbilityLevel, sxa.Spec, sxa.SpecLevel));
					}
					else
					{
						if (log.IsWarnEnabled)
							log.Warn("Associated ability " + sxa.AbilityKey + " for specialization " + sxa.Spec + " not found!");
					}
				}
			}

			DataObject[] specs = GameServer.Database.SelectAllObjects(typeof(DBSpecialization));
			if (specs != null)
			{
				foreach (DBSpecialization spec in specs)
				{
					ArrayList styleList = new ArrayList();
					if (spec.Styles != null)
					{
						foreach (DBStyle style in spec.Styles)
						{
							// find right position for insert
							int insertpos = 0;
							for (insertpos = 0; insertpos < styleList.Count; insertpos++)
							{
								if (style.SpecLevelRequirement < ((Style)styleList[insertpos]).SpecLevelRequirement)
									break;
							}
							Style st = new Style(style);
							styleList.Insert(insertpos, st);
							m_stylesByID[(int)st.ID] = st;
						}
					}
					m_styleLists[spec.KeyName] = styleList; // also adds empty lists, so we dont have to generate empty lists later
					RegisterSpec(new Specialization(spec.KeyName, spec.Name, spec.Icon));
					int specAbCount = 0;
					if (m_specAbilities[spec.KeyName] != null)
					{
						specAbCount = ((ArrayList)m_specAbilities[spec.KeyName]).Count;
					}
					if (log.IsDebugEnabled)
						log.Debug("Specialization: " + spec.Name + ", " + styleList.Count + " styles, " + specAbCount + " abilities");
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total specializations loaded: " + ((specs != null) ? specs.Length : 0));

			DBStyleSubstitute[] stylesByClass = (DBStyleSubstitute[])GameServer.Database.SelectAllObjects(typeof(DBStyleSubstitute));
			foreach (DBStyleSubstitute style in stylesByClass)
			{
				Style st = new Style((DBStyle)style);
				m_stylesByIDClass[((long)st.ID << 32) | (uint)style.ClassId] = st;
			}
			if (log.IsInfoEnabled)
				log.Info("Total style substitue loaded: " + ((stylesByClass != null) ? stylesByClass.Length : 0));

			// load skill action handlers
			//Search for ability handlers in the gameserver first
			if (log.IsInfoEnabled)
				log.Info("Searching ability handlers in GameServer");
			Hashtable ht = ScriptMgr.FindAllAbilityActionHandler(Assembly.GetExecutingAssembly());
			foreach (DictionaryEntry entry in ht)
			{
				if (log.IsDebugEnabled)
					log.Debug("\tFound ability handler for " + (string)entry.Key);
				m_abilityActionHandler[entry.Key] = entry.Value;
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
					if (log.IsDebugEnabled)
					{
						if (m_abilityActionHandler.ContainsKey(entry.Key))
							log.Debug("\tFound new ability handler for " + (string)entry.Key);
						else
							log.Debug("\tFound ability handler for " + (string)entry.Key);
					}
					m_abilityActionHandler[entry.Key] = entry.Value;
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total ability handlers loaded: " + m_abilityActionHandler.Keys.Count);

			//Search for skill handlers in gameserver first
			if (log.IsInfoEnabled)
				log.Info("Searching skill handlers in GameServer.");
			ht = ScriptMgr.FindAllSpecActionHandler(Assembly.GetExecutingAssembly());
			foreach (DictionaryEntry entry in ht)
			{
				if (log.IsDebugEnabled)
					log.Debug("\tFound skill handler for " + (string)entry.Key);
				m_specActionHandler[entry.Key] = entry.Value;
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
					if (log.IsDebugEnabled)
					{
						if (m_abilityActionHandler.ContainsKey(entry.Key))
							log.Debug("Found new skill handler for " + (string)entry.Key);
						else
							log.Debug("Found skill handler for " + (string)entry.Key);
					}
					m_specActionHandler[entry.Key] = entry.Value;
				}
			}
			if (log.IsInfoEnabled)
				log.Info("Total skill handlers loaded: " + m_specActionHandler.Keys.Count);

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
			const int mod = 10;

			// melee resists (slash, crush, thrust)

			// alb armor - neutral to slash
			// plate and leather resistant to thrust
			// chain and studded vulnerable to thrust
			WriteMeleeResists(eRealm.Albion, eObjectType.Leather, 0, -mod, mod);
			WriteMeleeResists(eRealm.Albion, eObjectType.Plate, 0, -mod, mod);
			WriteMeleeResists(eRealm.Albion, eObjectType.Studded, 0, mod, -mod);
			WriteMeleeResists(eRealm.Albion, eObjectType.Chain, 0, mod, -mod);


			// hib armor - neutral to thrust
			// reinforced and leather vulnerable to crush
			// scale resistant to crush
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Reinforced, mod, -mod, 0);
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Leather, mod, -mod, 0);
			WriteMeleeResists(eRealm.Hibernia, eObjectType.Scale, -mod, mod, 0);


			// mid armor - neutral to crush
			// studded and leather resistant to thrust
			// chain vulnerabel to thrust
			WriteMeleeResists(eRealm.Midgard, eObjectType.Studded, -mod, 0, mod);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Leather, -mod, 0, mod);
			WriteMeleeResists(eRealm.Midgard, eObjectType.Chain, mod, 0, -mod);


			// magical damage (Heat, Cold, Matter, Energy)
			// Leather
			WriteMagicResists(eRealm.Albion, eObjectType.Leather, 15, -10, -5, 0);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Leather, 15, -10, -5, 0);
			WriteMagicResists(eRealm.Midgard, eObjectType.Leather, 15, -10, -5, 0);

			// Reinforced/Studded
			WriteMagicResists(eRealm.Albion, eObjectType.Studded, -10, 5, 5, 5);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Reinforced, -10, 5, 5, 5);
			WriteMagicResists(eRealm.Midgard, eObjectType.Studded, -10, 5, 5, 5);

			// Chain
			WriteMagicResists(eRealm.Albion, eObjectType.Chain, -10, 0, 0, 10);
			WriteMagicResists(eRealm.Midgard, eObjectType.Chain, -10, 0, 0, 10);

			// Scale/Plate
			WriteMagicResists(eRealm.Albion, eObjectType.Plate, -10, 10, -10, 10);
			WriteMagicResists(eRealm.Hibernia, eObjectType.Scale, -10, 10, -10, 10);
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

		#region Property types

		/// <summary>
		/// Check if property belongs to all of specified types
		/// </summary>
		/// <param name="prop">The property to check</param>
		/// <param name="type">The types to check</param>
		/// <returns>true if property belongs to all types</returns>
		public static bool CheckPropertyType(eProperty prop, ePropertyType type)
		{
			int property = (int)prop;
			if (property < 0 || property >= m_propertyTypes.Length) return false;
			return (m_propertyTypes[property] & type) == type;
		}

		/// <summary>
		/// Holds all property types
		/// </summary>
		private static readonly ePropertyType[] m_propertyTypes = new ePropertyType[(int)eProperty.MaxProperty];

		/// <summary>
		/// Init property types table
		/// </summary>
		private static void InitPropertyTypes()
		{
			// resists
			m_propertyTypes[(int)eProperty.Resist_Body] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Cold] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Crush] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Energy] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Heat] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Matter] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Slash] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Spirit] = ePropertyType.Resist;
			m_propertyTypes[(int)eProperty.Resist_Thrust] = ePropertyType.Resist;

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

			m_propertyTypes[(int)eProperty.Skill_Body] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Chants] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Death_Servant] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_DeathSight] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Earth] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Enhancement] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Envenom] = ePropertyType.Skill | ePropertyType.SkillMagical;
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
			m_propertyTypes[(int)eProperty.Skill_OdinsWill] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Cursing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Hexing] = ePropertyType.Skill | ePropertyType.SkillMagical;
			m_propertyTypes[(int)eProperty.Skill_Witchcraft] = ePropertyType.Skill | ePropertyType.SkillMagical;

			m_propertyTypes[(int)eProperty.Skill_Dual_Wield] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Left_Axe] = ePropertyType.Skill | ePropertyType.SkillDualWield;
			m_propertyTypes[(int)eProperty.Skill_Celtic_Dual] = ePropertyType.Skill | ePropertyType.SkillDualWield;

			m_propertyTypes[(int)eProperty.Skill_Long_bows] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_Composite] = ePropertyType.Skill | ePropertyType.SkillArchery;
			m_propertyTypes[(int)eProperty.Skill_RecurvedBow] = ePropertyType.Skill | ePropertyType.SkillArchery;

			m_propertyTypes[(int)eProperty.Skill_Parry] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Shields] = ePropertyType.Skill;

			m_propertyTypes[(int)eProperty.Skill_Stealth] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_Cross_Bows] = ePropertyType.Skill;
			m_propertyTypes[(int)eProperty.Skill_ShortBow] = ePropertyType.Skill;
		}

		#endregion

		public static void RegisterPropertyNames()
		{
			#region register...
			m_propertyNames[eProperty.Strength] = "Strength";
			m_propertyNames[eProperty.Dexterity] = "Dexterity";
			m_propertyNames[eProperty.Constitution] = "Constitution";
			m_propertyNames[eProperty.Quickness] = "Quickness";
			m_propertyNames[eProperty.Intelligence] = "Intelligence";
			m_propertyNames[eProperty.Piety] = "Piety";
			m_propertyNames[eProperty.Empathy] = "Empathy";
			m_propertyNames[eProperty.Charisma] = "Charisma";

			m_propertyNames[eProperty.MaxMana] = "Power";
			m_propertyNames[eProperty.MaxHealth] = "Hits";

			// resists (does not say "resist" on live server)
			m_propertyNames[eProperty.Resist_Body] = "Body";
			m_propertyNames[eProperty.Resist_Cold] = "Cold";
			m_propertyNames[eProperty.Resist_Crush] = "Crush";
			m_propertyNames[eProperty.Resist_Energy] = "Energy";
			m_propertyNames[eProperty.Resist_Heat] = "Heat";
			m_propertyNames[eProperty.Resist_Matter] = "Matter";
			m_propertyNames[eProperty.Resist_Slash] = "Slash";
			m_propertyNames[eProperty.Resist_Spirit] = "Spirit";
			m_propertyNames[eProperty.Resist_Thrust] = "Thrust";

			// skills
			m_propertyNames[eProperty.Skill_Two_Handed] = "Two Handed";
			m_propertyNames[eProperty.Skill_Body] = "Body Magic";
			m_propertyNames[eProperty.Skill_Chants] = "Chants";
			m_propertyNames[eProperty.Skill_Critical_Strike] = "Critical Strike";
			m_propertyNames[eProperty.Skill_Cross_Bows] = "Crossbows";
			m_propertyNames[eProperty.Skill_Crushing] = "Crushing";
			m_propertyNames[eProperty.Skill_Death_Servant] = "Death Servant";
			m_propertyNames[eProperty.Skill_DeathSight] = "Deathsight";
			m_propertyNames[eProperty.Skill_Dual_Wield] = "Dual Wield";
			m_propertyNames[eProperty.Skill_Earth] = "Earth Magic";
			m_propertyNames[eProperty.Skill_Enhancement] = "Enhancement";
			m_propertyNames[eProperty.Skill_Envenom] = "Envenom";
			m_propertyNames[eProperty.Skill_Fire] = "Fire Magic";
			m_propertyNames[eProperty.Skill_Flexible_Weapon] = "Flexible Weapon";
			m_propertyNames[eProperty.Skill_Cold] = "Cold Magic";
			m_propertyNames[eProperty.Skill_Instruments] = "Instruments";
			m_propertyNames[eProperty.Skill_Long_bows] = "Longbows";
			m_propertyNames[eProperty.Skill_Matter] = "Matter Magic";
			m_propertyNames[eProperty.Skill_Mind] = "Mind Magic";
			m_propertyNames[eProperty.Skill_Pain_working] = "Painworking";
			m_propertyNames[eProperty.Skill_Parry] = "Parry";
			m_propertyNames[eProperty.Skill_Polearms] = "Polearms";
			m_propertyNames[eProperty.Skill_Rejuvenation] = "Rejuvenation";
			m_propertyNames[eProperty.Skill_Shields] = "Shields";
			m_propertyNames[eProperty.Skill_Slashing] = "Slashing";
			m_propertyNames[eProperty.Skill_Smiting] = "Smiting";
			m_propertyNames[eProperty.Skill_SoulRending] = "Soulrending";
			m_propertyNames[eProperty.Skill_Spirit] = "Spirit Magic";
			m_propertyNames[eProperty.Skill_Staff] = "Staff";
			m_propertyNames[eProperty.Skill_Stealth] = "Stealth";
			m_propertyNames[eProperty.Skill_Thrusting] = "Thrusting";
			m_propertyNames[eProperty.Skill_Wind] = "Wind Magic";
			m_propertyNames[eProperty.Skill_Sword] = "Sword";
			m_propertyNames[eProperty.Skill_Hammer] = "Hammer";
			m_propertyNames[eProperty.Skill_Axe] = "Axe";
			m_propertyNames[eProperty.Skill_Left_Axe] = "Left Axe";
			m_propertyNames[eProperty.Skill_Spear] = "Spear";
			m_propertyNames[eProperty.Skill_Mending] = "Mending";
			m_propertyNames[eProperty.Skill_Augmentation] = "Augmentation";
			//m_propertyNames[eProperty.Skill_Cave_Magic] = "Cave Magic";
			m_propertyNames[eProperty.Skill_Darkness] = "Darkness";
			m_propertyNames[eProperty.Skill_Suppression] = "Suppression";
			m_propertyNames[eProperty.Skill_Runecarving] = "Runecarving";
			m_propertyNames[eProperty.Skill_Stormcalling] = "Stormcalling";
			m_propertyNames[eProperty.Skill_BeastCraft] = "Beastcraft";
			m_propertyNames[eProperty.Skill_Light] = "Light Magic";
			m_propertyNames[eProperty.Skill_Void] = "Void Magic";
			m_propertyNames[eProperty.Skill_Mana] = "Mana Magic";
			m_propertyNames[eProperty.Skill_Composite] = "Composite";
			m_propertyNames[eProperty.Skill_Battlesongs] = "Battlesongs";
			m_propertyNames[eProperty.Skill_Enchantments] = "Enchantment";

			m_propertyNames[eProperty.Skill_Blades] = "Blades";
			m_propertyNames[eProperty.Skill_Blunt] = "Blunt";
			m_propertyNames[eProperty.Skill_Piercing] = "Piercing";
			m_propertyNames[eProperty.Skill_Large_Weapon] = "Large Weapon";
			m_propertyNames[eProperty.Skill_Mentalism] = "Mentalism";
			m_propertyNames[eProperty.Skill_Regrowth] = "Regrowth";
			m_propertyNames[eProperty.Skill_Nurture] = "Nurture";
			m_propertyNames[eProperty.Skill_Nature] = "Nature";
			m_propertyNames[eProperty.Skill_Music] = "Music";
			m_propertyNames[eProperty.Skill_Celtic_Dual] = "Celtic Dual";
			m_propertyNames[eProperty.Skill_Celtic_Spear] = "Celtic Spear";
			m_propertyNames[eProperty.Skill_RecurvedBow] = "Recurved Bow";
			m_propertyNames[eProperty.Skill_Valor] = "Valor";
			m_propertyNames[eProperty.Skill_Subterranean] = "Cave Magic";
			m_propertyNames[eProperty.Skill_BoneArmy] = "Bone Army";
			m_propertyNames[eProperty.Skill_Verdant] = "Verdant";
			m_propertyNames[eProperty.Skill_Creeping] = "Creeping";
			m_propertyNames[eProperty.Skill_Arboreal] = "Arboreal";
			m_propertyNames[eProperty.Skill_Scythe] = "Scythe";
			m_propertyNames[eProperty.Skill_Thrown_Weapons] = "Thrown Weapons";
			m_propertyNames[eProperty.Skill_HandToHand] = "Hand To Hand";
			m_propertyNames[eProperty.Skill_ShortBow] = "Short Bow";
			m_propertyNames[eProperty.Skill_Pacification] = "Pacification";
			m_propertyNames[eProperty.Skill_Savagery] = "Savagery";
			m_propertyNames[eProperty.Skill_Nightshade] = "Nightshade Magic";
			m_propertyNames[eProperty.Skill_Pathfinding] = "Pathfinding";
			m_propertyNames[eProperty.Skill_Summoning] = "Summoning";
			//Catacombs skills
			m_propertyNames[eProperty.Skill_Dementia] = "Dementia";
			m_propertyNames[eProperty.Skill_ShadowMastery] = "Shadow Mastery";
			m_propertyNames[eProperty.Skill_VampiiricEmbrace] = "Vampiiric Embrace";
			m_propertyNames[eProperty.Skill_EtherealShriek] = "Ethereal Shriek";
			m_propertyNames[eProperty.Skill_PhantasmalWail] = "Phantasmal Wail";
			m_propertyNames[eProperty.Skill_SpectralForce] = "Spectral Force";
			m_propertyNames[eProperty.Skill_OdinsWill] = "Odin's Will";
			m_propertyNames[eProperty.Skill_Cursing] = "Cursing";
			m_propertyNames[eProperty.Skill_Hexing] = "Hexing";
			m_propertyNames[eProperty.Skill_Witchcraft] = "Witchcraft";


			// Classic Focii
			m_propertyNames[eProperty.Focus_Darkness] = "Darkness Focus";
			m_propertyNames[eProperty.Focus_Suppression] = "Suppression Focus";
			m_propertyNames[eProperty.Focus_Runecarving] = "Runecarving Focus";
			m_propertyNames[eProperty.Focus_Spirit] = "Spirit Magic Focus";
			m_propertyNames[eProperty.Focus_Fire] = "Fire Magic Focus";
			m_propertyNames[eProperty.Focus_Air] = "Wind Magic Focus";
			m_propertyNames[eProperty.Focus_Cold] = "Cold Magic Focus";
			m_propertyNames[eProperty.Focus_Earth] = "Earth Magic Focus";
			m_propertyNames[eProperty.Focus_Light] = "Light Magic Focus";
			m_propertyNames[eProperty.Focus_Body] = "Body Magic Focus";
			m_propertyNames[eProperty.Focus_Matter] = "Matter Magic Focus";
			m_propertyNames[eProperty.Focus_Mind] = "Mind Magic Focus";
			m_propertyNames[eProperty.Focus_Void] = "Void Magic Focus";
			m_propertyNames[eProperty.Focus_Mana] = "Mana Magic Focus";
			m_propertyNames[eProperty.Focus_Enchantments] = "Enchantment Focus";
			m_propertyNames[eProperty.Focus_Mentalism] = "Mentalism Focus";
			m_propertyNames[eProperty.Focus_Summoning] = "Summoning Focus";
			// SI Focii
			// Mid
			m_propertyNames[eProperty.Focus_BoneArmy] = "Bone Army Focus";
			// Alb
			m_propertyNames[eProperty.Focus_PainWorking] = "Painworking Focus";
			m_propertyNames[eProperty.Focus_DeathSight] = "Deathsight Focus";
			m_propertyNames[eProperty.Focus_DeathServant] = "Deathservant Focus";
			// Hib
			m_propertyNames[eProperty.Focus_Verdant] = "Verdant Focus";
			m_propertyNames[eProperty.Focus_CreepingPath] = "Creeping Path Focus";
			m_propertyNames[eProperty.Focus_Arboreal] = "Arboreal Focus";
			// Catacombs Focii
			m_propertyNames[eProperty.Focus_EtherealShriek] = "Ethereal Shriek Focus";
			m_propertyNames[eProperty.Focus_PhantasmalWail] = "Phantasmal Wail Focus";
			m_propertyNames[eProperty.Focus_SpectralForce] = "Spectral Force Focus";
			m_propertyNames[eProperty.Focus_Cursing] = "Cursing Focus";
			m_propertyNames[eProperty.Focus_Hexing] = "Hexing Focus";
			m_propertyNames[eProperty.Focus_Witchcraft] = "Witchcraft Focus";

			m_propertyNames[eProperty.MaxSpeed] = "Maximum Speed";
			m_propertyNames[eProperty.MaxConcentration] = "Concentration";

			m_propertyNames[eProperty.ArmorFactor] = "Armorfactor";
			m_propertyNames[eProperty.ArmorAbsorbtion] = "Armor Absorbsion";

			m_propertyNames[eProperty.HealthRegenerationRate] = "Health Regeneration";
			m_propertyNames[eProperty.PowerRegenerationRate] = "Power Regeneration";
			m_propertyNames[eProperty.EnduranceRegenerationRate] = "Endurance Regeneration";
			m_propertyNames[eProperty.SpellRange] = "Spell Range";
			m_propertyNames[eProperty.ArcheryRange] = "Archery Range";
			m_propertyNames[eProperty.Acuity] = "Acuity";

			m_propertyNames[eProperty.AllMagicSkills] = "ALL magic skills";
			m_propertyNames[eProperty.AllMeleeWeaponSkills] = "ALL melee weapon skills";
			m_propertyNames[eProperty.AllFocusLevels] = "ALL spell lines";
			m_propertyNames[eProperty.AllDualWieldingSkills] = "ALL Dual Wielding Skills";
			m_propertyNames[eProperty.AllArcherySkills] = "ALL Archery Skills";

			m_propertyNames[eProperty.LivingEffectiveLevel] = "Effective level";

			m_propertyNames[eProperty.PowerPool] = "Power";

			//Added by Fooljam : Missing TOA/Catacomb bonusses names in item properties.
			//Date : 20-Jan-2005
			//Missing bonusses begin
			m_propertyNames[eProperty.EvadeChance] = "Evade Chance";
			m_propertyNames[eProperty.BlockChance] = "Block Chance";
			m_propertyNames[eProperty.ParryChance] = "Parry Chance";
			m_propertyNames[eProperty.FumbleChance] = "Fumble Chance";
			m_propertyNames[eProperty.MeleeDamage] = "Melee Damage";
			m_propertyNames[eProperty.RangedDamage] = "Ranged Damage";
			m_propertyNames[eProperty.MesmerizeDuration] = "Mesmerize Duration";
			m_propertyNames[eProperty.StunDuration] = "Stun Duration";
			m_propertyNames[eProperty.SpeedDecreaseDuration] = "Speed Decrease Duration";
			m_propertyNames[eProperty.BladeturnReinforcement] = "Bladeturn Reinforcement";
			m_propertyNames[eProperty.DefensiveBonus] = "Defensive Bonus";
			m_propertyNames[eProperty.PieceAblative] = "Piece Ablative";
			m_propertyNames[eProperty.NegativeReduction] = "Negative Reduction";
			m_propertyNames[eProperty.ReactionaryStyleDamage] = "Reactionary Style Damage";
			m_propertyNames[eProperty.SpellPowerCost] = "Spell Power Cost";
			m_propertyNames[eProperty.StyleCostReduction] = "Style Cost Reduction";
			m_propertyNames[eProperty.ToHitBonus] = "To Hit Bonus";
			m_propertyNames[eProperty.ArcherySpeed] = "Archery Speed";
			m_propertyNames[eProperty.ArrowRecovery] = "Arrow Recovery";
			m_propertyNames[eProperty.BuffEffectiveness] = "Buff Effectiveness";
			m_propertyNames[eProperty.CastingSpeed] = "Casting Speed";
			m_propertyNames[eProperty.DeathExpLoss] = "Death Exp Loss";
			m_propertyNames[eProperty.DebuffEffectivness] = "Debuff Effectivness";
			m_propertyNames[eProperty.Fatigue] = "Fatigue";
			m_propertyNames[eProperty.HealingEffectiveness] = "Healing Effectiveness";
			m_propertyNames[eProperty.PowerPool] = "Power Pool";
			m_propertyNames[eProperty.ResistPierce] = "Resist Pierce";
			m_propertyNames[eProperty.SpellDamage] = "Magic Damage";//fooljam fix : Was the wrong name (Spell Damage)
			m_propertyNames[eProperty.SpellDuration] = "Spell Duration";
			m_propertyNames[eProperty.StyleDamage] = "Style Damage";
			//Missing bonusses end

			m_propertyNames[eProperty.StrCapBonus] = "Bonus to Strength attribute bonus cap";
			m_propertyNames[eProperty.DexCapBonus] = "Bonus to Dexterity attribute bonus cap";
			m_propertyNames[eProperty.ConCapBonus] = "Bonus to Constitution attribute bonus cap";
			m_propertyNames[eProperty.QuiCapBonus] = "Bonus to Quickness attribute bonus cap";
			m_propertyNames[eProperty.IntCapBonus] = "Bonus to Intelligence attribute bonus cap";
			m_propertyNames[eProperty.PieCapBonus] = "Bonus to Piety attribute bonus cap";
			m_propertyNames[eProperty.ChaCapBonus] = "Bonus to Charisma attribute bonus cap";
			m_propertyNames[eProperty.EmpCapBonus] = "Bonus to Empathy attribute bonus cap";
			m_propertyNames[eProperty.AcuCapBonus] = "Bonus to Acuity attribute bonus cap";
			m_propertyNames[eProperty.MaxHealthCapBonus] = "Bonus to hit points bonus cap";
			m_propertyNames[eProperty.PowerPoolCapBonus] = "Bonus to power bonus cap";
			#endregion
		}

		public static void RegisterAbility(Ability ability)
		{
			DBAbility dba = new DBAbility();
			dba.KeyName = ability.KeyName;
			dba.Name = ability.Name;
			dba.IconID = ability.Icon;
			m_abilitiesByName[ability.KeyName] = dba;
		}

		/// <summary>
		/// register new AbilityActionHandler
		/// if a previous handler exists it will be overridden
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="classtype"></param>
		public static void RegisterAbilityHandler(string keyName, Type classtype)
		{
			m_abilityActionHandler[keyName] = classtype;
		}

		/// <summary>
		/// Gets a new AbilityActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static IAbilityActionHandler GetAbilityActionHandler(string keyName)
		{
			Type handlerType = m_abilityActionHandler[keyName] as Type;
			if (handlerType == null)
				return null;
			try
			{
				return Activator.CreateInstance(handlerType) as IAbilityActionHandler;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Can't instantiate AbilityActionHandler " + handlerType, e);
			}
			return null;
		}

		/// <summary>
		/// register new SpecActionHandler
		/// if a previous handler exists it will be overridden
		/// </summary>
		/// <param name="keyName"></param>
		/// <param name="classtype"></param>
		public static void RegisterSpecHandler(string keyName, Type classtype)
		{
			m_specActionHandler[keyName] = classtype;
		}

		/// <summary>
		/// Gets a new SpecActionHandler instance associated with given KeyName
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public static ISpecActionHandler GetSpecActionHandler(string keyName)
		{
			Type handlerType = m_specActionHandler[keyName] as Type;
			if (handlerType == null)
				return null;
			try
			{
				return Activator.CreateInstance(handlerType) as ISpecActionHandler;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Can't instantiate SpecActionHandler " + handlerType, e);
			}
			return null;
		}

		public static void RegisterSpellLine(SpellLine line)
		{
			m_spellLinesByName[line.KeyName] = line;
		}

		public static void RegisterSpec(Specialization spec)
		{
			m_specsByName[spec.KeyName] = spec;
		}

		/// <summary>
		/// returns level 1 instantiated realm abilities, only for readonly use!
		/// </summary>
		/// <param name="classID"></param>
		/// <returns></returns>
		public static IList GetClassRealmAbilities(int classID)
		{
			return (IList)m_classRealmAbilities[classID];
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
			DBAbility dba = m_abilitiesByName[keyname] as DBAbility;
			if (dba != null)
			{
				//DOLConsole.WriteLine("loaded ability "+keyname+" level "+level);
				Type type = null;
				if (dba.Implementation != null && dba.Implementation.Length > 0)
				{
					type = m_implementationTypeCache[dba.Implementation] as Type;
				}
				if (type == null)
				{
					return new Ability(dba, level);
				}
				else
				{
					return (Ability)Activator.CreateInstance(type, new object[] { dba, level });
				}
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
			{
				return new SpellLine("mob spells", "mob spells", "", true);
			}
			SpellLine line = m_spellLinesByName[keyname] as SpellLine;
			if (line != null)
			{
				return (SpellLine)line.Clone();
			}
			if (log.IsWarnEnabled)
				log.Warn("Spell-Line " + keyname + " unknown");
			return new SpellLine(keyname, "?" + keyname, "", true);
		}


		/// <summary>
		///
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public static Specialization GetSpecialization(string keyname)
		{
			Specialization spec = m_specsByName[keyname] as Specialization;
			if (spec != null)
			{
				return (Specialization)spec.Clone();
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
		public static IList GetStyleList(string specID, int classId)
		{
			IList list = (IList)m_styleLists[specID];
			if (list == null)
			{
				return new ArrayList(0);
			}

			IList newStyles = new ArrayList(m_styleLists.Count);
			foreach (Style style in list)
			{
				Style st = GetStyleByID(style.ID, classId);
				newStyles.Add(st);
			}
			return newStyles;
		}

		/// <summary>
		/// returns spec dependend abilities
		/// </summary>
		/// <param name="specID">KeyName of spec</param>
		/// <returns>list of abilities or empty list</returns>
		public static IList GetSpecAbilityList(string specID)
		{
			IList list = (IList)m_specAbilities[specID];
			if (list == null)
			{
				list = new ArrayList(0);
			}
			return list;
		}

		/// <summary>
		/// return all spells for a specific spell-line
		/// if no spells are associated or spell-line is unknown the list will be empty
		/// </summary>
		/// <param name="spellLineID">KeyName of spell-line</param>
		/// <returns>list of spells, never null</returns>
		public static IList GetSpellList(string spellLineID)
		{
			IList list = (IList)m_spellLists[spellLineID];
			if (list == null)
			{
				list = new ArrayList(0);
			}
			return list;
		}

		public static Style GetStyleByID(int styleID)
		{
			return m_stylesByID[styleID] as Style;
		}

		/// <summary>
		/// find style with specific id
		/// </summary>
		/// <param name="styleID">id of style</param>
		/// <param name="classId">ClassID for which style list is requested</param>
		/// <returns>style or null if not found</returns>
		public static Style GetStyleByID(int styleID, int ClassId)
		{
			long key = ((long)styleID << 32) | (uint)ClassId;
			Style style = (Style)m_stylesByIDClass[key];
			if (style == null)
				style = GetStyleByID(styleID);
			return style;
		}

		/// <summary>
		/// Returns spell with id, level of spell is always 1
		/// </summary>
		/// <param name="spellID"></param>
		/// <returns></returns>
		public static Spell GetSpellByID(int spellID)
		{
			return m_spells[spellID] as Spell;
		}

		/// <summary>
		/// Get display name of property
		/// </summary>
		/// <param name="prop"></param>
		/// <returns></returns>
		public static string GetPropertyName(eProperty prop)
		{
			string name = (string)m_propertyNames[prop];
			if (name == null)
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
			// when first called create the resist table
			if (m_raceResists == null)
			{
				m_raceResists = new HybridDictionary[(int)eRace._Last + 3];

				#region init the table

				// http://camelot.allakhazam.com/Start_Stats.html
				// Alb
				m_raceResists[(int)eRace.Avalonian] = new HybridDictionary();
				m_raceResists[(int)eRace.Avalonian][eResist.Crush] = 2;
				m_raceResists[(int)eRace.Avalonian][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Avalonian][eResist.Spirit] = 5;

				m_raceResists[(int)eRace.Briton] = new HybridDictionary();
				m_raceResists[(int)eRace.Briton][eResist.Crush] = 2;
				m_raceResists[(int)eRace.Briton][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Briton][eResist.Matter] = 5;

				m_raceResists[(int)eRace.Highlander] = new HybridDictionary();
				m_raceResists[(int)eRace.Highlander][eResist.Crush] = 3;
				m_raceResists[(int)eRace.Highlander][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Highlander][eResist.Cold] = 5;

				m_raceResists[(int)eRace.Saracen] = new HybridDictionary();
				m_raceResists[(int)eRace.Saracen][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Saracen][eResist.Thrust] = 3;
				m_raceResists[(int)eRace.Saracen][eResist.Heat] = 5;

				m_raceResists[(int)eRace.Inconnu] = new HybridDictionary();
				m_raceResists[(int)eRace.Inconnu][eResist.Crush] = 2;
				m_raceResists[(int)eRace.Inconnu][eResist.Thrust] = 3;
				m_raceResists[(int)eRace.Inconnu][eResist.Heat] = 5;
				m_raceResists[(int)eRace.Inconnu][eResist.Spirit] = 5;

				m_raceResists[(int)eRace.HalfOgre] = new HybridDictionary();
				m_raceResists[(int)eRace.HalfOgre][eResist.Thrust] = 2;
				m_raceResists[(int)eRace.HalfOgre][eResist.Slash] = 3;
				m_raceResists[(int)eRace.HalfOgre][eResist.Matter] = 5;

				// Hib
				m_raceResists[(int)eRace.Celt] = new HybridDictionary();
				m_raceResists[(int)eRace.Celt][eResist.Crush] = 2;
				m_raceResists[(int)eRace.Celt][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Celt][eResist.Spirit] = 5;

				m_raceResists[(int)eRace.Elf] = new HybridDictionary();
				m_raceResists[(int)eRace.Elf][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Elf][eResist.Thrust] = 3;
				m_raceResists[(int)eRace.Elf][eResist.Spirit] = 5;

				m_raceResists[(int)eRace.Firbolg] = new HybridDictionary();
				m_raceResists[(int)eRace.Firbolg][eResist.Crush] = 3;
				m_raceResists[(int)eRace.Firbolg][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Firbolg][eResist.Heat] = 5;

				m_raceResists[(int)eRace.Lurikeen] = new HybridDictionary();
				m_raceResists[(int)eRace.Lurikeen][eResist.Crush] = 5;
				m_raceResists[(int)eRace.Lurikeen][eResist.Energy] = 5;

				m_raceResists[(int)eRace.Sylvan] = new HybridDictionary();
				m_raceResists[(int)eRace.Sylvan][eResist.Crush] = 3;
				m_raceResists[(int)eRace.Sylvan][eResist.Thrust] = 2;
				m_raceResists[(int)eRace.Sylvan][eResist.Matter] = 5;
				m_raceResists[(int)eRace.Sylvan][eResist.Energy] = 5;

				m_raceResists[(int)eRace.Shar] = new HybridDictionary();
				m_raceResists[(int)eRace.Shar][eResist.Crush] = 5;
				m_raceResists[(int)eRace.Shar][eResist.Energy] = 5;

				// Mid
				m_raceResists[(int)eRace.Dwarf] = new HybridDictionary();
				m_raceResists[(int)eRace.Dwarf][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Dwarf][eResist.Thrust] = 3;
				m_raceResists[(int)eRace.Dwarf][eResist.Body] = 5;

				m_raceResists[(int)eRace.Kobold] = new HybridDictionary();
				m_raceResists[(int)eRace.Kobold][eResist.Crush] = 5;
				m_raceResists[(int)eRace.Kobold][eResist.Matter] = 5;

				m_raceResists[(int)eRace.Troll] = new HybridDictionary();
				m_raceResists[(int)eRace.Troll][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Troll][eResist.Thrust] = 2;
				m_raceResists[(int)eRace.Troll][eResist.Matter] = 5;

				m_raceResists[(int)eRace.Norseman] = new HybridDictionary();
				m_raceResists[(int)eRace.Norseman][eResist.Crush] = 2;
				m_raceResists[(int)eRace.Norseman][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Norseman][eResist.Cold] = 5;

				m_raceResists[(int)eRace.Valkyn] = new HybridDictionary();
				m_raceResists[(int)eRace.Valkyn][eResist.Slash] = 3;
				m_raceResists[(int)eRace.Valkyn][eResist.Thrust] = 2;
				m_raceResists[(int)eRace.Valkyn][eResist.Cold] = 5;
				m_raceResists[(int)eRace.Valkyn][eResist.Body] = 5;

				m_raceResists[(int)eRace.Frostalf] = new HybridDictionary();
				m_raceResists[(int)eRace.Frostalf][eResist.Slash] = 2;
				m_raceResists[(int)eRace.Frostalf][eResist.Thrust] = 3;
				m_raceResists[(int)eRace.Frostalf][eResist.Spirit] = 5;

				m_raceResists[(int)eRace.AlbionMinotaur] = new HybridDictionary();
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Body] = 5; //unofficial
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Cold] = 5; //unofficial
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Energy] = 5; //unofficial
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Heat] = 5; //unofficial
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Matter] = 5; //unofficial
				m_raceResists[(int)eRace.AlbionMinotaur][eResist.Spirit] = 5; //unofficial
				

				m_raceResists[(int)eRace.MidgardMinotaur] = new HybridDictionary();
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Body] = 5; //unofficial
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Cold] = 5; //unofficial
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Energy] = 5; //unofficial
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Heat] = 5; //unofficial
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Matter] = 5; //unofficial
				m_raceResists[(int)eRace.MidgardMinotaur][eResist.Spirit] = 5; //unofficial
				
				m_raceResists[(int)eRace.HiberniaMinotaur] = new HybridDictionary();
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Body] = 5; //unofficial
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Cold] = 5; //unofficial
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Energy] = 5; //unofficial
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Heat] = 5; //unofficial
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Matter] = 5; //unofficial
				m_raceResists[(int)eRace.HiberniaMinotaur][eResist.Spirit] = 5; //unofficial
				
				#endregion
			}

			HybridDictionary resists = m_raceResists[(int)race];
			if (resists == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("No resists for race " + race + " defined");
				return 0;
			}
			if (resists[type] == null)
			{
				return 0;
			}
			return (int)resists[type];
		}

		/// <summary>
		/// Holds object type to spec convertion table
		/// </summary>
		protected static Hashtable m_objectTypeToSpec = null;

		/// <summary>
		/// Holds spec to skill table
		/// </summary>
		protected static Hashtable m_specToSkill = null;

		/// <summary>
		/// Holds spec to focus table
		/// </summary>
		protected static Hashtable m_specToFocus = null;

		/// <summary>
		/// Convert object type to spec needed to use that object
		/// </summary>
		/// <param name="objectType">type of the object</param>
		/// <returns>spec names needed to use that object type</returns>
		public static string ObjectTypeToSpec(eObjectType objectType)
		{
			// fill the table first time it is used
			if (m_objectTypeToSpec == null)
			{
				m_objectTypeToSpec = new Hashtable(64);

				#region init the table

				m_objectTypeToSpec[(int)eObjectType.Staff] = Specs.Staff;
				m_objectTypeToSpec[(int)eObjectType.Fired] = Specs.ShortBow;

				//alb
				m_objectTypeToSpec[(int)eObjectType.CrushingWeapon] = Specs.Crush;
				m_objectTypeToSpec[(int)eObjectType.SlashingWeapon] = Specs.Slash;
				m_objectTypeToSpec[(int)eObjectType.ThrustWeapon] = Specs.Thrust;
				m_objectTypeToSpec[(int)eObjectType.TwoHandedWeapon] = Specs.Two_Handed;
				m_objectTypeToSpec[(int)eObjectType.PolearmWeapon] = Specs.Polearms;
				m_objectTypeToSpec[(int)eObjectType.Flexible] = Specs.Flexible;
				m_objectTypeToSpec[(int)eObjectType.Longbow] = Specs.Longbow;
				m_objectTypeToSpec[(int)eObjectType.Crossbow] = Specs.Crossbow;
				//TODO: case 5: abilityCheck = Abilities.Weapon_Thrown; break;

				//mid
				m_objectTypeToSpec[(int)eObjectType.Hammer] = Specs.Hammer;
				m_objectTypeToSpec[(int)eObjectType.Sword] = Specs.Sword;
				m_objectTypeToSpec[(int)eObjectType.LeftAxe] = Specs.Left_Axe;
				m_objectTypeToSpec[(int)eObjectType.Axe] = Specs.Axe;
				m_objectTypeToSpec[(int)eObjectType.HandToHand] = Specs.HandToHand;
				m_objectTypeToSpec[(int)eObjectType.Spear] = Specs.Spear;
				m_objectTypeToSpec[(int)eObjectType.CompositeBow] = Specs.CompositeBow;
				m_objectTypeToSpec[(int)eObjectType.Thrown] = Specs.Thrown_Weapons;

				//hib
				m_objectTypeToSpec[(int)eObjectType.Blunt] = Specs.Blunt;
				m_objectTypeToSpec[(int)eObjectType.Blades] = Specs.Blades;
				m_objectTypeToSpec[(int)eObjectType.Piercing] = Specs.Piercing;
				m_objectTypeToSpec[(int)eObjectType.LargeWeapons] = Specs.Large_Weapons;
				m_objectTypeToSpec[(int)eObjectType.CelticSpear] = Specs.Celtic_Spear;
				m_objectTypeToSpec[(int)eObjectType.Scythe] = Specs.Scythe;
				m_objectTypeToSpec[(int)eObjectType.RecurvedBow] = Specs.RecurveBow;

				m_objectTypeToSpec[(int)eObjectType.Shield] = Specs.Shields;
				m_objectTypeToSpec[(int)eObjectType.Poison] = Specs.Envenom;
				//TODO: case 45: abilityCheck = Abilities.instruments; break;

				#endregion
			}

			string res = (string)m_objectTypeToSpec[(int)objectType];
			if (res == null)
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
			if (m_specToSkill == null)
			{
				m_specToSkill = new Hashtable(80);

				#region init the table

				//Weapon specs
				//Alb
				m_specToSkill[Specs.Thrust] = eProperty.Skill_Thrusting;
				m_specToSkill[Specs.Slash] = eProperty.Skill_Slashing;
				m_specToSkill[Specs.Crush] = eProperty.Skill_Crushing;
				m_specToSkill[Specs.Polearms] = eProperty.Skill_Polearms;
				m_specToSkill[Specs.Two_Handed] = eProperty.Skill_Two_Handed;
				m_specToSkill[Specs.Staff] = eProperty.Skill_Staff;
				m_specToSkill[Specs.Dual_Wield] = eProperty.Skill_Dual_Wield;
				m_specToSkill[Specs.Flexible] = eProperty.Skill_Flexible_Weapon;
				m_specToSkill[Specs.Longbow] = eProperty.Skill_Long_bows;
				m_specToSkill[Specs.Crossbow] = eProperty.Skill_Cross_Bows;
				//Mid
				m_specToSkill[Specs.Sword] = eProperty.Skill_Sword;
				m_specToSkill[Specs.Axe] = eProperty.Skill_Axe;
				m_specToSkill[Specs.Hammer] = eProperty.Skill_Hammer;
				m_specToSkill[Specs.Left_Axe] = eProperty.Skill_Left_Axe;
				m_specToSkill[Specs.Spear] = eProperty.Skill_Spear;
				m_specToSkill[Specs.CompositeBow] = eProperty.Skill_Composite;
				m_specToSkill[Specs.Thrown_Weapons] = eProperty.Skill_Thrown_Weapons;
				m_specToSkill[Specs.HandToHand] = eProperty.Skill_HandToHand;
				//Hib
				m_specToSkill[Specs.Blades] = eProperty.Skill_Blades;
				m_specToSkill[Specs.Blunt] = eProperty.Skill_Blunt;
				m_specToSkill[Specs.Piercing] = eProperty.Skill_Piercing;
				m_specToSkill[Specs.Large_Weapons] = eProperty.Skill_Large_Weapon;
				m_specToSkill[Specs.Celtic_Dual] = eProperty.Skill_Celtic_Dual;
				m_specToSkill[Specs.Celtic_Spear] = eProperty.Skill_Celtic_Spear;
				m_specToSkill[Specs.RecurveBow] = eProperty.Skill_RecurvedBow;
				m_specToSkill[Specs.Scythe] = eProperty.Skill_Scythe;

				//Magic specs
				//Alb
				m_specToSkill[Specs.Matter_Magic] = eProperty.Skill_Matter;
				m_specToSkill[Specs.Body_Magic] = eProperty.Skill_Body;
				m_specToSkill[Specs.Spirit_Magic] = eProperty.Skill_Spirit;
				m_specToSkill[Specs.Rejuvenation] = eProperty.Skill_Rejuvenation;
				m_specToSkill[Specs.Enhancement] = eProperty.Skill_Enhancement;
				m_specToSkill[Specs.Smite] = eProperty.Skill_Smiting;
				m_specToSkill[Specs.Instruments] = eProperty.Skill_Instruments;
				m_specToSkill[Specs.Deathsight] = eProperty.Skill_DeathSight;
				m_specToSkill[Specs.Painworking] = eProperty.Skill_Pain_working;
				m_specToSkill[Specs.Death_Servant] = eProperty.Skill_Death_Servant;
				m_specToSkill[Specs.Chants] = eProperty.Skill_Chants;
				m_specToSkill[Specs.Mind_Magic] = eProperty.Skill_Mind;
				m_specToSkill[Specs.Earth_Magic] = eProperty.Skill_Earth;
				m_specToSkill[Specs.Cold_Magic] = eProperty.Skill_Cold;
				m_specToSkill[Specs.Fire_Magic] = eProperty.Skill_Fire;
				m_specToSkill[Specs.Wind_Magic] = eProperty.Skill_Wind;
				m_specToSkill[Specs.Soulrending] = eProperty.Skill_SoulRending;
				//Mid
				m_specToSkill[Specs.Darkness] = eProperty.Skill_Darkness;
				m_specToSkill[Specs.Suppression] = eProperty.Skill_Suppression;
				m_specToSkill[Specs.Runecarving] = eProperty.Skill_Runecarving;
				m_specToSkill[Specs.Summoning] = eProperty.Skill_Summoning;
				m_specToSkill[Specs.Bonedancing] = eProperty.Skill_BoneArmy;
				m_specToSkill[Specs.Mending] = eProperty.Skill_Mending;
				m_specToSkill[Specs.Augmentation] = eProperty.Skill_Augmentation;
				m_specToSkill[Specs.Pacification] = eProperty.Skill_Pacification;
				m_specToSkill[Specs.Subterranean] = eProperty.Skill_Subterranean;
				m_specToSkill[Specs.Beastcraft] = eProperty.Skill_BeastCraft;
				m_specToSkill[Specs.Stormcalling] = eProperty.Skill_Stormcalling;
				m_specToSkill[Specs.Battlesongs] = eProperty.Skill_Battlesongs;
				m_specToSkill[Specs.Savagery] = eProperty.Skill_Savagery;
				m_specToSkill[Specs.OdinsWill] = eProperty.Skill_OdinsWill;
				m_specToSkill[Specs.Cursing] = eProperty.Skill_Cursing;
				m_specToSkill[Specs.Hexing] = eProperty.Skill_Hexing;
				m_specToSkill[Specs.Witchcraft] = eProperty.Skill_Witchcraft;

				//Hib
				m_specToSkill[Specs.Arboreal_Path] = eProperty.Skill_Arboreal;
				m_specToSkill[Specs.Creeping_Path] = eProperty.Skill_Creeping;
				m_specToSkill[Specs.Verdant_Path] = eProperty.Skill_Verdant;
				m_specToSkill[Specs.Regrowth] = eProperty.Skill_Regrowth;
				m_specToSkill[Specs.Nurture] = eProperty.Skill_Nurture;
				m_specToSkill[Specs.Music] = eProperty.Skill_Music;
				m_specToSkill[Specs.Valor] = eProperty.Skill_Valor;
				m_specToSkill[Specs.Nature] = eProperty.Skill_Nature;
				m_specToSkill[Specs.Light] = eProperty.Skill_Light;
				m_specToSkill[Specs.Void] = eProperty.Skill_Void;
				m_specToSkill[Specs.Mana] = eProperty.Skill_Mana;
				m_specToSkill[Specs.Enchantments] = eProperty.Skill_Enchantments;
				m_specToSkill[Specs.Mentalism] = eProperty.Skill_Mentalism;
				m_specToSkill[Specs.Nightshade_Magic] = eProperty.Skill_Nightshade;
				m_specToSkill[Specs.Pathfinding] = eProperty.Skill_Pathfinding;
				m_specToSkill[Specs.Dementia] = eProperty.Skill_Dementia;
				m_specToSkill[Specs.ShadowMastery] = eProperty.Skill_ShadowMastery;
				m_specToSkill[Specs.VampiiricEmbrace] = eProperty.Skill_VampiiricEmbrace;
				m_specToSkill[Specs.EtherealShriek] = eProperty.Skill_EtherealShriek;
				m_specToSkill[Specs.PhantasmalWail] = eProperty.Skill_PhantasmalWail;
				m_specToSkill[Specs.SpectralGuard] = eProperty.Skill_SpectralForce;

				//Other
				m_specToSkill[Specs.Critical_Strike] = eProperty.Skill_Critical_Strike;
				m_specToSkill[Specs.Stealth] = eProperty.Skill_Stealth;
				m_specToSkill[Specs.Shields] = eProperty.Skill_Shields;
				m_specToSkill[Specs.Envenom] = eProperty.Skill_Envenom;
				m_specToSkill[Specs.Parry] = eProperty.Skill_Parry;
				m_specToSkill[Specs.ShortBow] = eProperty.Skill_ShortBow;

				#endregion
			}

			object res = m_specToSkill[specKey];
			if (res == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("No skill property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return (eProperty)res;
		}

		/// <summary>
		/// Convert spec to focus
		/// </summary>
		/// <param name="specKey"></param>
		/// <returns></returns>
		public static eProperty SpecToFocus(string specKey)
		{
			if (m_specToFocus == null)
			{
				m_specToFocus = new Hashtable(25);

				#region init the table

				m_specToFocus[Specs.Darkness] = eProperty.Focus_Darkness;
				m_specToFocus[Specs.Suppression] = eProperty.Focus_Suppression;
				m_specToFocus[Specs.Runecarving] = eProperty.Focus_Runecarving;
				m_specToFocus[Specs.Spirit_Magic] = eProperty.Focus_Spirit;
				m_specToFocus[Specs.Fire_Magic] = eProperty.Focus_Fire;
				m_specToFocus[Specs.Wind_Magic] = eProperty.Focus_Air;
				m_specToFocus[Specs.Cold_Magic] = eProperty.Focus_Cold;
				m_specToFocus[Specs.Earth_Magic] = eProperty.Focus_Earth;
				m_specToFocus[Specs.Light] = eProperty.Focus_Light;
				m_specToFocus[Specs.Body_Magic] = eProperty.Focus_Body;
				m_specToFocus[Specs.Mind_Magic] = eProperty.Focus_Mind;
				m_specToFocus[Specs.Matter_Magic] = eProperty.Focus_Matter;
				m_specToFocus[Specs.Void] = eProperty.Focus_Void;
				m_specToFocus[Specs.Mana] = eProperty.Focus_Mana;
				m_specToFocus[Specs.Enchantments] = eProperty.Focus_Enchantments;
				m_specToFocus[Specs.Mentalism] = eProperty.Focus_Mentalism;
				m_specToFocus[Specs.Summoning] = eProperty.Focus_Summoning;
				// SI
				m_specToFocus[Specs.Bonedancing] = eProperty.Focus_BoneArmy;
				m_specToFocus[Specs.Painworking] = eProperty.Focus_PainWorking;
				m_specToFocus[Specs.Deathsight] = eProperty.Focus_DeathSight;
				m_specToFocus[Specs.Death_Servant] = eProperty.Focus_DeathServant;
				m_specToFocus[Specs.Verdant_Path] = eProperty.Focus_Verdant;
				m_specToFocus[Specs.Creeping_Path] = eProperty.Focus_CreepingPath;
				m_specToFocus[Specs.Arboreal_Path] = eProperty.Focus_Arboreal;
				// Catacombs
				m_specToFocus[Specs.EtherealShriek] = eProperty.Focus_EtherealShriek;
				m_specToFocus[Specs.PhantasmalWail] = eProperty.Focus_PhantasmalWail;
				m_specToFocus[Specs.SpectralGuard] = eProperty.Focus_SpectralForce;
				m_specToFocus[Specs.Cursing] = eProperty.Focus_Cursing;
				m_specToFocus[Specs.Hexing] = eProperty.Focus_Hexing;
				m_specToFocus[Specs.Witchcraft] = eProperty.Focus_Witchcraft;

				#endregion
			}

			object res = m_specToFocus[specKey];
			if (res == null)
			{
				//				if (log.IsWarnEnabled)
				//					log.Warn("No focus property found for spec " + specKey);
				return eProperty.Undefined;
			}
			return (eProperty)res;
		}
	}
}
