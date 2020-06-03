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
using System.Reflection;
using DOL.Database;
using DOL.GS.Styles;
using DOL.GS.Spells;
using log4net;
using System.Collections;
using System.Linq;

namespace DOL.GS
{
	/// <summary>
	/// A npc template
	/// </summary>
	public class NpcTemplate : INpcTemplate
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		protected int m_templateId;
        protected string m_translationId;
		protected string m_name;
        protected string m_suffix;
		protected string m_classType;
		protected string m_guildName;
        protected string m_examineArticle;
        protected string m_messageArticle;
		protected string m_model;
		protected ushort m_gender;
		protected string m_size;
		protected string m_level;
		protected string m_equipmentTemplateID;
		protected string m_itemsListTemplateID;
		protected short m_maxSpeed;
		protected byte m_parryChance;
		protected byte m_evadeChance;
		protected byte m_blockChance;
		protected byte m_leftHandSwingChance;
		protected ushort m_flags;
		protected string m_inventory;
		protected eDamageType m_meleeDamageType;
		protected short m_strength;
		protected short m_constitution;
		protected short m_dexterity;
		protected short m_quickness;
		protected short m_piety;
		protected short m_intelligence;
		protected short m_empathy;
		protected short m_charisma;
		protected IList m_styles;
		protected IList m_spells;
		protected IList m_spelllines;
		protected IList m_abilities;
		protected byte m_aggroLevel;
		protected int m_aggroRange;
		protected ushort m_race;
		protected ushort m_bodyType;
		protected int m_maxdistance;
		protected int m_tetherRange;
		protected bool m_replaceMobValues;
		protected byte m_visibleActiveWeaponSlot;

		/// <summary>
		/// Constructs a new NpcTemplate
		/// </summary>
		/// <param name="data">The source npc template data</param>
		public NpcTemplate(DBNpcTemplate data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			m_templateId = data.TemplateId;
            m_translationId = data.TranslationId;
			m_name = data.Name;
            m_suffix = data.Suffix;
			m_classType = data.ClassType;
			m_guildName = data.GuildName;
            m_examineArticle = data.ExamineArticle;
            m_messageArticle = data.MessageArticle;
			m_model = data.Model;
			m_gender = data.Gender;
			m_size = data.Size;
			if (m_size == null)
				m_size = "50";
			m_level = data.Level;
			if (m_level == null)
				m_level = "0";
			m_equipmentTemplateID = data.EquipmentTemplateID;
			m_itemsListTemplateID = data.ItemsListTemplateID;
			m_maxSpeed = data.MaxSpeed;
			m_parryChance = data.ParryChance;
			m_evadeChance = data.EvadeChance;
			m_blockChance = data.BlockChance;
			m_leftHandSwingChance = data.LeftHandSwingChance;
			m_strength = data.Strength;
			m_constitution = data.Constitution;
			m_dexterity = data.Dexterity;
			m_quickness = data.Quickness;
			m_intelligence = data.Intelligence;
			m_piety = data.Piety;
			m_charisma = data.Charisma;
			m_empathy = data.Empathy;

			//Time to add Spells/Styles and Abilties to the templates
			m_abilities = new ArrayList();
			m_spelllines = new ArrayList();
			m_spells = new ArrayList();
			//Adding the spells to an Arraylist here
			if (data.Spells != null && data.Spells.Length > 0)
			{
				string[] spells = data.Spells.Split(';');
				for (int k = 0; k < spells.Length; k++)
				{
					int id = int.Parse(spells[k]);
					Spell sp = SkillBase.GetSpellByID(id);
					if (sp != null)
						m_spells.Add(sp);
					else log.Error("Tried to load a null spell into NPC template " + m_templateId + " spell ID: " + id);
				}
			}

			// Adding Style list to Template NPC
			m_styles = new ArrayList();
			if (data.Styles != null && data.Styles.Length > 0)
			{
				string[] styles = data.Styles.Split(';');
				for (int i = 0; i < styles.Length; i++)
				{
					if (styles[i].Trim().Length == 0) continue;
					string[] styleAndClass = styles[i].Split('|');
					if (styleAndClass.Length != 2) continue;
					string stylePart = styleAndClass[0].Trim();
					string classPart = styleAndClass[1].Trim();
					if (stylePart.Length == 0 || classPart.Length == 0) continue;
					int styleID = int.Parse(stylePart);
					int classID = int.Parse(classPart);
					Style style = SkillBase.GetStyleByID(styleID, classID);
					m_styles.Add(style);
				}
			}
			//Adding Abilities to Template NPC.
			//Certain Abilities have levels will need to fix that down the road. -Batlas

			if (data.Abilities != null && data.Abilities.Length > 0)
			{
				foreach (string splitab in Util.SplitCSV(data.Abilities))
				{
					string[] ab = splitab.Split('|');
					if (splitab.Trim().Length == 0) continue;
					int id = 1;
					if (ab.Length>1)
						id = int.Parse(ab[1]);
					Ability abil = SkillBase.GetAbility(ab[0], id);
					if (abil != null)
						m_abilities.Add(abil);
				}
			}

			m_flags = data.Flags;

			m_meleeDamageType = (eDamageType)data.MeleeDamageType;
			if (data.MeleeDamageType == 0)
				m_meleeDamageType = eDamageType.Slash;

			m_inventory = data.EquipmentTemplateID;
			m_aggroLevel = data.AggroLevel;
			m_aggroRange = data.AggroRange;
			m_race = (ushort)data.Race;
			m_bodyType = (ushort)data.BodyType;
			m_maxdistance = data.MaxDistance;
			m_tetherRange = data.TetherRange;
			m_visibleActiveWeaponSlot = data.VisibleWeaponSlots;
			
			m_replaceMobValues = data.ReplaceMobValues;
		}


		public NpcTemplate( GameNPC mob )
		{
			if (mob == null)
				throw new ArgumentNullException("data");

            m_translationId = mob.TranslationId;
			m_blockChance = mob.BlockChance;
			m_race = (ushort)mob.Race;
			m_bodyType = mob.BodyType;
			m_charisma = mob.Charisma;
			m_classType = mob.GetType().ToString();
			m_constitution = mob.Constitution;
			m_dexterity = mob.Dexterity;
			m_empathy = mob.Empathy;
			m_equipmentTemplateID = mob.EquipmentTemplateID;
			m_evadeChance = mob.EvadeChance;
			m_flags = (ushort)mob.Flags;
			m_guildName = mob.GuildName;
            m_examineArticle = mob.ExamineArticle;
            m_messageArticle = mob.MessageArticle;
			m_intelligence = mob.Intelligence;
			m_maxdistance = mob.MaxDistance;
			m_maxSpeed = (short)mob.MaxSpeedBase;
			m_meleeDamageType = (eDamageType)mob.MeleeDamageType;
			m_model = mob.Model.ToString();
			m_leftHandSwingChance = mob.LeftHandSwingChance;
			m_level = mob.Level.ToString();
			m_name = mob.Name;
            m_suffix = mob.Suffix;
			m_parryChance = mob.ParryChance;
			m_piety = mob.Piety;
			m_quickness = mob.Quickness;
			m_strength = mob.Strength;
			m_size = mob.Size.ToString();
			m_templateId = GetNextFreeTemplateId();
			m_tetherRange = mob.TetherRange;
			m_visibleActiveWeaponSlot = mob.VisibleActiveWeaponSlots;
			
			if ( mob.Abilities != null && mob.Abilities.Count > 0 )
			{
				try
				{
					if (m_abilities == null)
						m_abilities = new ArrayList(mob.Abilities.Count);

					foreach (Ability mobAbility in mob.Abilities.Values)
					{
						m_abilities.Add(mobAbility);
					}
				}
				catch (Exception ex)
				{
					log.Error("Trapped Error: ", ex);
				}
			}

			if ( mob.Spells != null && mob.Spells.Count > 0 )
			{
				try
				{
					if (m_spells == null)
						m_spells = new ArrayList(mob.Spells.Count);

					foreach (Spell mobSpell in mob.Spells)
					{
						m_spells.Add(mobSpell);
					}
				}
				catch (Exception ex)
				{
					log.Error("Trapped Error: ", ex);
				}
			}

			if ( mob.Styles != null && mob.Styles.Count > 0 )
			{
				try
				{
					if (m_styles == null)
						m_styles = new ArrayList(mob.Styles.Count);

					foreach (Style mobStyle in mob.Styles)
					{
						m_styles.Add(mobStyle);
					}
				}
				catch (Exception ex)
				{
					log.Error("Trapped Error: ", ex);
				}
			}

			AI.Brain.StandardMobBrain brain = mob.Brain as AI.Brain.StandardMobBrain;
			if (brain != null)
			{
				m_aggroLevel = (byte)brain.AggroLevel;
				m_aggroRange = brain.AggroRange;
			}

			if (string.IsNullOrEmpty(ItemsListTemplateID) == false)
			{
				GameMerchant merchant = mob as GameMerchant;

				if (merchant != null)
				{
					merchant.TradeItems = new MerchantTradeItems(ItemsListTemplateID);
				}
			}
		}

		protected int GetNextFreeTemplateId()
		{
			var objs = GameServer.Database.SelectAllObjects<DBNpcTemplate>();
			int free_id = 1;
			int doubleidcheck = 0;
			foreach (DBNpcTemplate dbtemplate in objs.OrderBy(x => x.TemplateId))
			{
				if (dbtemplate.TemplateId == free_id)
				{
					doubleidcheck = dbtemplate.TemplateId;
					free_id++;
				}
				else
				{
					if (dbtemplate.TemplateId == doubleidcheck)
						continue;
					else
						break;
				}
			}

			return free_id;
		}


		public NpcTemplate()
			: base()
		{ }

		/// <summary>
		/// Gets the npc template ID
		/// </summary>
		public int TemplateId
		{
			get { return m_templateId; }
			set { m_templateId = value; }
		}

        /// <summary>
        /// Gets the translation id.
        /// </summary>
        public string TranslationId
        {
            get { return m_translationId; }
            set { m_translationId = value; }
        }

		/// <summary>
		/// Gets the template npc name
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}

        /// <summary>
        /// Gets the suffix.
        /// </summary>
        public string Suffix
        {
            get { return m_suffix; }
            set { m_suffix = value; }
        }

		/// <summary>
		/// Gets the template npc class type
		/// </summary>
		public string ClassType
		{
			get { return m_classType; }
			set { m_classType = value; }
		}

		/// <summary>
		/// Gets the template npc guild name
		/// </summary>
		public string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
		}

        /// <summary>
        /// Gets the examine article.
        /// </summary>
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set { m_examineArticle = value; }
        }

        /// <summary>
        /// Gets the message article.
        /// </summary>
        public string MessageArticle
        {
            get { return m_messageArticle; }
            set { m_messageArticle = value; }
        }

		/// <summary>
		/// Gets the template npc model
		/// </summary>
		public string Model
		{
			get { return m_model; }
			set { m_model = value; }
		}
		
		/// <summary>
		/// Gets the template npc model
		/// </summary>
		public ushort Gender
		{
			get { return m_gender; }
			set { m_gender = value; }
		}
		
		/// <summary>
		/// Gets the template npc size
		/// </summary>
		public string Size
		{
			get { return m_size; }
			set { m_size = value; }
		}

		public string Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		public string EquipmentTemplateID
		{
			get { return m_equipmentTemplateID; }
			set { m_equipmentTemplateID = value; }
		}

		public string ItemsListTemplateID
		{
			get { return m_itemsListTemplateID; }
			set { m_itemsListTemplateID = value; }
		}

		/// <summary>
		/// Gets the template npc max speed
		/// </summary>
		public short MaxSpeed
		{
			get { return m_maxSpeed; }
			set { m_maxSpeed = value; }
		}
		/// <summary>
		/// Gets the template npc flags
		/// </summary>
		public ushort Flags
		{
			get { return m_flags; }
			set { m_flags = value; }
		}
		/// <summary>
		/// Gets the template npc inventory
		/// </summary>
		public string Inventory
		{
			get { return m_inventory; }
			set { m_inventory = value; }
		}
		/// <summary>
		/// Gets the template npc melee damage type
		/// </summary>
		public eDamageType MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set { m_meleeDamageType = value; }
		}
		/// <summary>
		/// Gets the template npc parry chance
		/// </summary>
		public byte ParryChance
		{
			get { return m_parryChance; }
			set { m_parryChance = value; }
		}
		/// <summary>
		/// Gets the template npc evade chance
		/// </summary>
		public byte EvadeChance
		{
			get { return m_evadeChance; }
			set { m_evadeChance = value; }
		}
		/// <summary>
		/// Gets the template npc block chance
		/// </summary>
		public byte BlockChance
		{
			get { return m_blockChance; }
			set { m_blockChance = value; }
		}
		/// <summary>
		/// Gets the template npc left hand swing chance
		/// </summary>
		public byte LeftHandSwingChance
		{
			get { return m_leftHandSwingChance; }
			set { m_leftHandSwingChance = value; }
		}
		/// <summary>
		/// Gets the template npc spells name array
		/// </summary>
		public IList Spells
		{
			get { return m_spells; }
			set { m_spells = value; }
		}
		/// <summary>
		/// Gets the template npc styles name array
		/// </summary>
		public IList Styles
		{
			get { return m_styles; }
			set { m_styles = value; }
		}
		/// <summary>
		/// Gets the template npc spellLines
		/// </summary>
		public IList SpellLines
		{
			get { return m_spelllines; }
			set { m_spelllines = value; }
		}
		///<summary>
		///Gets the template npc Abilities
		///</summary>
		public IList Abilities
		{
			get { return m_abilities; }
			set { m_abilities = value; }
		}

		public short Strength
		{
			get { return m_strength; }
			set { m_strength = value; }
		}

		public short Constitution
		{
			get { return m_constitution; }
			set { m_constitution = value; }
		}

		public short Dexterity
		{
			get { return m_dexterity; }
			set { m_dexterity = value; }
		}

		public short Quickness
		{
			get { return m_quickness; }
			set { m_quickness = value; }
		}

		public short Piety
		{
			get { return m_piety; }
			set { m_piety = value; }
		}

		public short Intelligence
		{
			get { return m_intelligence; }
			set { m_intelligence = value; }
		}

		public short Empathy
		{
			get { return m_empathy; }
			set { m_empathy = value; }
		}

		public short Charisma
		{
			get { return m_charisma; }
			set { m_charisma = value; }
		}

		public byte AggroLevel
		{
			get { return m_aggroLevel; }
			set { m_aggroLevel = value; }
		}

		public int AggroRange
		{
			get { return m_aggroRange; }
			set { m_aggroRange = value; }
		}

		public ushort Race
		{
			get { return m_race; }
			set { m_race = value; }
		}

		public ushort BodyType
		{
			get { return m_bodyType; }
			set { m_bodyType = value; }
		}

		/// <summary>
		/// The Mob's max distance from its spawn before return automatically
		/// if MaxDistance > 0 ... the amount is the normal value
		/// if MaxDistance = 0 ... no maxdistance check
		/// if MaxDistance less than 0 ... the amount is calculated in procent of the value and the aggrorange (in StandardMobBrain)
		/// </summary>
		public int MaxDistance
		{
			get { return m_maxdistance;	}
			set	{ m_maxdistance = value;	}
		}

		/// <summary>
		/// The mob's tether range; if mob is pulled farther than this distance
		/// it will return to its spawn point.
		/// if TetherRange > 0 ... the amount is the normal value
		/// if TetherRange less or equal 0 ... no tether check
		/// </summary>
		public int TetherRange
		{
			get { return m_tetherRange; }
			set { m_tetherRange = value; }
		}
		
		/// <summary>
		/// Should we get this template values as reference or mob's one ?
		/// </summary>
		public bool ReplaceMobValues
		{
			get { return m_replaceMobValues;  }
			set { m_replaceMobValues = value; }
		}

		/// <summary>
		/// The visible and active weapon slot
		/// </summary>
		public byte VisibleActiveWeaponSlot {
			get { return m_visibleActiveWeaponSlot; }
			set { m_visibleActiveWeaponSlot = value; }
		}

		public virtual void SaveIntoDatabase()
		{
			DBNpcTemplate tmp = GameServer.Database.FindObjectByKey<DBNpcTemplate>(TemplateId);
			bool add = false;

			if (tmp == null)
			{
				tmp = new DBNpcTemplate();
				add = true;
			}

			if (TemplateId == 0)
				tmp.TemplateId = GetNextFreeTemplateId();
			else
				tmp.TemplateId = TemplateId;

            tmp.TranslationId = TranslationId;
			tmp.AggroLevel = AggroLevel;
			tmp.AggroRange = AggroRange;
			tmp.BlockChance = BlockChance;
			tmp.BodyType = BodyType;
			tmp.Charisma = Charisma;
			tmp.ClassType = ClassType;
			tmp.Constitution = Constitution;
			tmp.Dexterity = Dexterity;
			tmp.Empathy = Empathy;
			tmp.EquipmentTemplateID = EquipmentTemplateID;
			tmp.ItemsListTemplateID = ItemsListTemplateID;
			tmp.EvadeChance = EvadeChance;
			tmp.Flags = Flags;
			tmp.GuildName = GuildName;
            tmp.ExamineArticle = ExamineArticle;
            tmp.MessageArticle = MessageArticle;
			tmp.Intelligence = Intelligence;
			tmp.LeftHandSwingChance = LeftHandSwingChance;
			tmp.Level = Level;
			tmp.MaxDistance = MaxDistance;
			tmp.MaxSpeed = MaxSpeed;
			tmp.MeleeDamageType = (byte)MeleeDamageType;
			tmp.Model = Model;
			tmp.Name = Name;
            tmp.Suffix = Suffix;
			tmp.ParryChance = ParryChance;
			tmp.Piety = Piety;
			tmp.Quickness = Quickness;
			tmp.Size = Size;
			tmp.Strength = Strength;
			tmp.TetherRange = TetherRange;

			if( m_abilities != null && m_abilities.Count > 0 )
			{
				string serializedAbilities = "";

				foreach ( Ability npcAbility in m_abilities )
				{
					if ( npcAbility != null )
						if ( serializedAbilities.Length > 0 )
							serializedAbilities += ";";

					serializedAbilities += npcAbility.Name + "|" + npcAbility.Level;
				}

				tmp.Abilities = serializedAbilities;
			}

			if ( m_spells != null && m_spells.Count > 0 )
			{
				string serializedSpells = "";

				foreach ( Spell npcSpell in m_spells )
				{
					if ( npcSpell != null )
						if ( serializedSpells.Length > 0 )
							serializedSpells += ";";

					serializedSpells += npcSpell.ID;
				}

				tmp.Spells = serializedSpells;
			}

			if ( m_styles != null && m_styles.Count > 0 )
			{
				string serializedStyles = "";

				foreach ( Style npcStyle in m_styles )
				{
					if ( npcStyle != null )
						if ( serializedStyles.Length > 0 )
							serializedStyles += ";";

					serializedStyles += npcStyle.ID + "|" + npcStyle.ClassID;
				}

				tmp.Styles = serializedStyles;
			}

			if (add)
				GameServer.Database.AddObject(tmp);
			else
				GameServer.Database.SaveObject(tmp);
		}
	}
}