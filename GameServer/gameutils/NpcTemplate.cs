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
using DOL.GS.Scripts;
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
		protected byte m_size;
		protected short m_maxSpeed;
		protected byte m_parryChance;
		protected byte m_evadeChance;
		protected byte m_blockChance;
		protected byte m_leftHandSwingChance;
		protected uint m_flags;
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
		protected IList b_spells;
		protected IList m_spelllines;
		protected IList m_abilities;
		protected byte m_aggroLevel;
		protected int m_aggroRange;

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
					m_spells.Add(sp);
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
					int id = int.Parse(styles[i]);
					Style style = SkillBase.GetStyleByID(id, -1);
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

			if (data.Ghost)
			{
				m_flags = (uint)GameNPC.eFlags.TRANSPARENT;
			}

			m_meleeDamageType = (eDamageType)data.MeleeDamageType;
			if (data.MeleeDamageType == 0)
				m_meleeDamageType = eDamageType.Slash;

            m_inventory = data.EquipmentTemplateID;
			m_aggroLevel = data.AggroLevel;
			m_aggroRange = data.AggroRange;
		}

		public NpcTemplate(GameNPC mob)
		{
			if (mob == null)
				throw new ArgumentNullException("data");
			m_templateId = GameServer.Database.GetObjectCount(typeof(DBNpcTemplate));
			m_name = mob.Name;
			m_classType = mob.GetType().ToString();
			m_guildName = mob.GuildName;
			m_model = mob.Model.ToString();
			m_size = mob.Size;
			m_maxSpeed = (short)mob.MaxSpeed;
			m_parryChance = mob.ParryChance;
			m_evadeChance = mob.EvadeChance;
			m_blockChance = mob.BlockChance;
			m_leftHandSwingChance = mob.LeftHandSwingChance;
			//Now for mob stats			
			m_strength = 30;
			m_constitution = 30;
			m_quickness = 30;
			m_intelligence = 30;
			m_piety = 30;
			m_charisma = 30;
			m_empathy = 30;
			m_meleeDamageType = (eDamageType)mob.MeleeDamageType;
			AI.Brain.StandardMobBrain brain = mob.Brain as AI.Brain.StandardMobBrain;
			if (brain != null)
			{
				m_aggroLevel = (byte)brain.AggroLevel;
				m_aggroRange = brain.AggroRange;
			}
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
		public byte Size
		{
			get { return m_size; }
			set { m_size = value; }
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
		public uint Flags
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

		public virtual void SaveIntoDatabase()
		{
			DBNpcTemplate tmp = tmp = (DBNpcTemplate)GameServer.Database.FindObjectByKey(typeof(DBNpcTemplate), TemplateId);
			bool add = false;
			if (tmp == null)
			{
				tmp = new DBNpcTemplate();
				add = true;
			}
			if (TemplateId == 0)
				tmp.TemplateId = GameServer.Database.GetObjectCount(typeof(DBNpcTemplate));
			else tmp.TemplateId = TemplateId;
			tmp.Model = Model;
			tmp.Size = Size;
			tmp.Name = Name;
			tmp.MaxSpeed = MaxSpeed;
			tmp.GuildName = GuildName;
			tmp.ParryChance = ParryChance;
			tmp.EvadeChance = EvadeChance;
			tmp.BlockChance = BlockChance;
			tmp.LeftHandSwingChance = LeftHandSwingChance;
			tmp.Strength = Strength;
			tmp.Constitution = Constitution;
			tmp.Dexterity = Dexterity;
			tmp.Quickness = Quickness;
			tmp.Piety = Piety;
			tmp.Empathy = Empathy;
			tmp.Intelligence = Intelligence;
			tmp.Charisma = Charisma;
			tmp.AggroRange = AggroRange;
			tmp.AggroLevel = AggroLevel;
			if (add)
				GameServer.Database.AddNewObject(tmp);
			else GameServer.Database.SaveObject(tmp);
		}
	}
}