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
		protected string m_name;
		protected string m_classType;
		protected string m_guildName;
		protected string m_model;
		protected string m_size;
		protected string m_level;
		protected string m_equipmentTemplateID;
		protected short m_maxSpeed;
		protected byte m_parryChance;
		protected byte m_evadeChance;
		protected byte m_blockChance;
		protected byte m_leftHandSwingChance;
		protected byte m_flags;
		protected string m_inventory;
		protected eDamageType m_meleeDamageType;
		protected int m_strength;
		protected int m_constitution;
		protected int m_dexterity;
		protected int m_quickness;
		protected int m_piety;
		protected int m_intelligence;
		protected int m_empathy;
		protected int m_charisma;
		protected IList m_styles;
		protected IList m_spells;
		protected IList m_spelllines;
		protected IList m_abilities;
		protected byte m_aggroLevel;
		protected int m_aggroRange;
		protected int m_bodyType;
		protected int m_maxdistance;
		protected int m_tetherRange;

		/// <summary>
		/// Constructs a new NpcTemplate
		/// </summary>
		/// <param name="data">The source npc template data</param>
		public NpcTemplate(DBNpcTemplate data)
		{
			if (data == null)
				throw new ArgumentNullException("data");

			m_templateId = data.TemplateId;
			m_name = data.Name;
			m_classType = data.ClassType;
			m_guildName = data.GuildName;
			m_model = data.Model;
			m_size = data.Size;
			if (m_size == null)
				m_size = "50";
			m_level = data.Level;
			if (m_level == null)
				m_level = "0";
			m_equipmentTemplateID = data.EquipmentTemplateID;
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
				string[] serializedab = data.Abilities.Split(';');
				foreach (string splitab in serializedab)
				{
					string[] ab = splitab.Split('|');
					if (splitab.Trim().Length == 0) continue;
					int id = int.Parse(ab[1]);
					Ability abil = SkillBase.GetAbility(ab[0], id);
					if (abil != null)
						m_abilities.Add(abil);
				}
			}

			#warning Graveen: once NpcTemplate 'll be finalized, must be changed to Flags - db converter to do
			m_flags = data.Flags;

			m_meleeDamageType = (eDamageType)data.MeleeDamageType;
			if (data.MeleeDamageType == 0)
				m_meleeDamageType = eDamageType.Slash;

			m_inventory = data.EquipmentTemplateID;
			m_aggroLevel = data.AggroLevel;
			m_aggroRange = data.AggroRange;
			m_bodyType = data.BodyType;
			m_maxdistance = data.MaxDistance;
			m_tetherRange = data.TetherRange;
		}


		public NpcTemplate( GameNPC mob )
		{
			if (mob == null)
				throw new ArgumentNullException("data");

			m_blockChance = mob.BlockChance;
			m_bodyType = mob.BodyType;
			m_charisma = mob.Charisma;
			m_classType = mob.GetType().ToString();
			m_constitution = mob.Constitution;
			m_dexterity = mob.Dexterity;
			m_empathy = mob.Empathy;
			m_equipmentTemplateID = mob.EquipmentTemplateID;
			m_evadeChance = mob.EvadeChance;
			m_flags = (byte)mob.Flags;
			m_guildName = mob.GuildName;
			m_intelligence = mob.Intelligence;
			m_maxdistance = mob.MaxDistance;
			m_maxSpeed = (short)mob.MaxSpeedBase;
			m_meleeDamageType = (eDamageType)mob.MeleeDamageType;
			m_model = mob.Model.ToString();
			m_leftHandSwingChance = mob.LeftHandSwingChance;
			m_level = mob.Level.ToString();
			m_name = mob.Name;
			m_parryChance = mob.ParryChance;
			m_piety = mob.Piety;
			m_quickness = mob.Quickness;
			m_strength = mob.Strength;
			m_size = mob.Size.ToString();
			m_templateId = GetNextFreeTemplateId();
			m_tetherRange = mob.TetherRange;

			if ( mob.Abilities != null && mob.Abilities.Count > 0 )
			{
				if ( m_abilities == null )
					m_abilities = new ArrayList( mob.Abilities.Count );

				foreach ( Ability mobAbility in mob.Abilities )
				{
					m_abilities.Add( mobAbility );
				}
			}

			if ( mob.Spells != null && mob.Spells.Count > 0 )
			{
				if ( m_spells == null )
					m_spells = new ArrayList( mob.Spells.Count );

				foreach ( Spell mobSpell in mob.Spells )
				{
					m_spells.Add( mobSpell );
				}
			}

			if ( mob.Styles != null && mob.Styles.Count > 0 )
			{
				if ( m_styles == null )
					m_styles = new ArrayList( mob.Styles.Count );

				foreach ( Style mobStyle in mob.Styles )
				{
					m_styles.Add( mobStyle );
				}
			}

			AI.Brain.StandardMobBrain brain = mob.Brain as AI.Brain.StandardMobBrain;
			if (brain != null)
			{
				m_aggroLevel = (byte)brain.AggroLevel;
				m_aggroRange = brain.AggroRange;
			}
		}

		protected int GetNextFreeTemplateId()
		{
			// this is bad - IDs are all over the place; there's no guarantee that existing templates have IDs in the range 0 - (Count -1)
			// if a proper templateID is not externally set before saving, it will overwrite any existing template with this ID
			// TODO : change when pk is made to be int
			return GameServer.Database.GetObjectCount(typeof(DBNpcTemplate));
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
		/// Gets the template npc name
		/// </summary>
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
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
		/// Gets the template npc model
		/// </summary>
		public string Model
		{
			get { return m_model; }
			set { m_model = value; }
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
		public byte Flags
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

		public int Strength
		{
			get { return m_strength; }
			set { m_strength = value; }
		}

		public int Constitution
		{
			get { return m_constitution; }
			set { m_constitution = value; }
		}

		public int Dexterity
		{
			get { return m_dexterity; }
			set { m_dexterity = value; }
		}

		public int Quickness
		{
			get { return m_quickness; }
			set { m_quickness = value; }
		}

		public int Piety
		{
			get { return m_piety; }
			set { m_piety = value; }
		}

		public int Intelligence
		{
			get { return m_intelligence; }
			set { m_intelligence = value; }
		}

		public int Empathy
		{
			get { return m_empathy; }
			set { m_empathy = value; }
		}

		public int Charisma
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

		public int BodyType
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
			get
			{
				return m_maxdistance;
			}
			set
			{
				m_maxdistance = value;
			}
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

		public virtual void SaveIntoDatabase()
		{
			DBNpcTemplate tmp = (DBNpcTemplate)GameServer.Database.FindObjectByKey(typeof(DBNpcTemplate), TemplateId);
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
			tmp.EvadeChance = EvadeChance;
			tmp.Flags = Flags;
			tmp.GuildName = GuildName;
			tmp.Intelligence = Intelligence;
			tmp.LeftHandSwingChance = LeftHandSwingChance;
			tmp.Level = Level;
			tmp.MaxDistance = MaxDistance;
			tmp.MaxSpeed = MaxSpeed;
			tmp.MeleeDamageType = (byte)MeleeDamageType;
			tmp.Model = Model;
			tmp.Name = Name;
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
				GameServer.Database.AddNewObject(tmp);
			else
				GameServer.Database.SaveObject(tmp);
		}
	}
}