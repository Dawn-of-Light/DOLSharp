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
 * 
 * This DB holds all information needed for all types of NPCs.
 * It just hold the Information about the different Types, not if and where it
 * is on the world. This is handled in MobDB. With this, it is possible to assign
 * Spells, Brains and other stuff to Mobs, that was just possible to get 
 * hardcoded before.
 */

using System;
using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// DataTable class for NPC Templates
	/// </summary>
	[DataTable(TableName = "NpcTemplate")]
	public class DBNpcTemplate : DataObject
	{
		private int m_templateId;
		private string m_name = "";
		private string m_classType = "";
		private string m_guildName = "";
		private string m_model;
		private string m_size = "50";
		private string m_level = "0";
		private short m_maxSpeed = 50;
		private string m_equipmentTemplateID = "";
		private bool m_ghost;
		private byte m_meleeDamageType = 1;
		private byte m_parryChance;
		private byte m_evadeChance;
		private byte m_blockChance;
		private byte m_leftHandSwingChance;
		private string m_spells = "";
		private string m_styles = "";
		private int m_strength = 0;
		private int m_constitution = 0;
		private int m_dexterity = 0;
		private int m_quickness = 0;
		private int m_intelligence = 0;
		private int m_piety = 0;
		private int m_empathy = 0;
		private int m_charisma = 0;
		private string m_abilities = "";
		private byte m_aggroLevel = 0;
		private int m_aggroRange = 0;
		private int m_bodyType = 0;

		private static bool m_autoSave;

		/// <summary>
		/// Constructor
		/// </summary>
		public DBNpcTemplate()
		{
			m_autoSave = false;
		}

		/// <summary>
		/// AutoSave in table?
		/// </summary>
		public override bool AutoSave
		{
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		/// <summary>
		/// Template ID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int TemplateId
		{
			get { return m_templateId; }
			set
			{
				Dirty = true;
				m_templateId = value;
			}
		}

		/// <summary>
		/// Name
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get { return m_name; }
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
		/// Class Type
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string ClassType
		{
			get { return m_classType; }
			set
			{
				Dirty = true;
				m_classType = value;
			}
		}

		/// <summary>
		/// GuildName
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string GuildName
		{
			get { return m_guildName; }
			set
			{
				Dirty = true;
				m_guildName = value;
			}
		}

		/// <summary>
		/// Model
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Model
		{
			get { return m_model; }
			set
			{
				Dirty = true;
				m_model = value;
			}
		}

		/// <summary>
		/// Size
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Size
		{
			get { return m_size; }
			set
			{
				Dirty = true;
				m_size = value;
			}
		}

		/// <summary>
		/// Level Range
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Level
		{
			get { return m_level; }
			set
			{
				Dirty = true;
				m_level = value;
			}
		}

		/// <summary>
		/// MaxSpeed
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public short MaxSpeed
		{
			get { return m_maxSpeed; }
			set
			{
				Dirty = true;
				m_maxSpeed = value;
			}
		}

		/// <summary>
		/// EquipmentTemplateID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string EquipmentTemplateID
		{
			get { return m_equipmentTemplateID; }
			set
			{
				Dirty = true;
				m_equipmentTemplateID = value;
			}
		}

		/// <summary>
		/// Ghost
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Ghost
		{
			get { return m_ghost; }
			set
			{
				Dirty = true;
				m_ghost = value;
			}
		}

		/// <summary>
		/// MeleeDamageType
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set
			{
				Dirty = true;
				m_meleeDamageType = value;
			}
		}

		/// <summary>
		/// ParryChance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte ParryChance
		{
			get { return m_parryChance; }
			set
			{
				Dirty = true;
				m_parryChance = value;
			}
		}

		/// <summary>
		/// EvadeChance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte EvadeChance
		{
			get { return m_evadeChance; }
			set
			{
				Dirty = true;
				m_evadeChance = value;
			}
		}

		/// <summary>
		/// BlockChance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte BlockChance
		{
			get { return m_blockChance; }
			set
			{
				Dirty = true;
				m_blockChance = value;
			}
		}

		/// <summary>
		/// LeftHandSwingChance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte LeftHandSwingChance
		{
			get { return m_leftHandSwingChance; }
			set
			{
				Dirty = true;
				m_leftHandSwingChance = value;
			}
		}

		/// <summary>
		/// Spells
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Spells
		{
			get { return m_spells; }
			set
			{
				Dirty = true;
				m_spells = value;
			}
		}

		/// <summary>
		/// Styles
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Styles
		{
			get { return m_styles; }
			set
			{
				Dirty = true;
				m_styles = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Strength
		{
			get { return m_strength; }
			set
			{
				Dirty = true;
				m_strength = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Constitution
		{
			get { return m_constitution; }
			set
			{
				Dirty = true;
				m_constitution = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Dexterity
		{
			get { return m_dexterity; }
			set
			{
				Dirty = true;
				m_dexterity = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Quickness
		{
			get { return m_quickness; }
			set
			{
				Dirty = true;
				m_quickness = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Intelligence
		{
			get { return m_intelligence; }
			set
			{
				Dirty = true;
				m_intelligence = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Piety
		{
			get { return m_piety; }
			set
			{
				Dirty = true;
				m_piety = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Charisma
		{
			get { return m_charisma; }
			set
			{
				Dirty = true;
				m_charisma = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Empathy
		{
			get { return m_empathy; }
			set
			{
				Dirty = true;
				m_empathy = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Abilities
		{
			get { return m_abilities; }
			set
			{
				Dirty = true;
				m_abilities = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte AggroLevel
		{
			get { return m_aggroLevel; }
			set
			{
				Dirty = true;
				m_aggroLevel = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int AggroRange
		{
			get { return m_aggroRange; }
			set
			{
				Dirty = true;
				m_aggroRange = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int BodyType
		{
			get { return m_bodyType; }
			set
			{
				Dirty = true;
				m_bodyType = value;
			}
		}
	}
}