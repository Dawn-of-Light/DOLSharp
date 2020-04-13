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
        #region Variables
        private int m_templateId;
        private string m_translationId = string.Empty;
		private string m_name = string.Empty;
        private string m_suffix = string.Empty;
		private string m_classType = string.Empty;
		private string m_guildName = string.Empty;
        private string m_examineArticle = string.Empty;
        private string m_messageArticle = string.Empty;
		private string m_model;
		private ushort m_gender;
		private string m_size = "50";
		private string m_level = "0";
		private short m_maxSpeed = 50;
		private string m_equipmentTemplateID = string.Empty;
		private string m_itemsListTemplateID = string.Empty;
		private ushort m_flags;
		private byte m_meleeDamageType = 1;
		private byte m_parryChance;
		private byte m_evadeChance;
		private byte m_blockChance;
		private byte m_leftHandSwingChance;
		private string m_spells = string.Empty;
		private string m_styles = string.Empty;
		private short m_strength = 0;
		private short m_constitution = 0;
		private short m_dexterity = 0;
		private short m_quickness = 0;
		private short m_intelligence = 0;
		private short m_piety = 0;
		private short m_empathy = 0;
		private short m_charisma = 0;
		private string m_abilities = string.Empty;
		private byte m_aggroLevel = 0;
		private int m_aggroRange = 0;
		private int m_race = 0;
		private int m_bodyType = 0;
		private int m_maxdistance = 0;
		private int m_tetherRange = 0;
		private byte m_visibleWeaponSlots = 0;
		private bool m_replaceMobValues =  false;
		private string m_packageID = string.Empty;
        #endregion Variables

        /// <summary>
		/// Constructor
		/// </summary>
		public DBNpcTemplate()
		{
		}

        #region Properties
        /// <summary>
		/// Template ID
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
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
        /// Gets or sets the translation id
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string TranslationId
        {
            get { return m_translationId; }
            set
            {
                Dirty = true;
                m_translationId = value;
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
        /// Gets or sets the name suffix (currently used by necromancer pets).
        /// 
        /// The XYZ spell is no longer in the Death Servant's queue.
        /// 
        /// 's = the suffix.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string Suffix
        {
            get { return m_suffix; }
            set
            {
                Dirty = true;
                m_suffix = value;
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
        /// Gets or sets the examine article.
        /// 
        /// You examine the Tree.
        /// 
        /// the = the examine article.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string ExamineArticle
        {
            get { return m_examineArticle; }
            set
            {
                Dirty = true;
                m_examineArticle = value;
            }
        }

        /// <summary>
        /// Gets or sets the message article.
        /// 
        /// GamePlayer has been killed by a Tree.
        /// 
        /// a = the message article.
        /// </summary>
        [DataElement(AllowDbNull = true)]
        public string MessageArticle
        {
            get { return m_messageArticle; }
            set
            {
                Dirty = true;
                m_messageArticle = value;
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
		/// Model
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Gender
		{
			get { return m_gender; }
			set
			{
				Dirty = true;
				m_gender = value;
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
		/// A MerchantList for this mob
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string ItemsListTemplateID
		{
			get { return m_itemsListTemplateID; }
			set
			{
				Dirty = true;
				m_itemsListTemplateID = value;
			}
		}

		/// <summary>
		/// Flags
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Flags
		{
			get { return m_flags; }
			set
			{
				Dirty = true;
				m_flags = value;
			}
		}

		/// <summary>
		/// MeleeDamageType
		/// </summary>
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = false)]
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

		[DataElement(AllowDbNull = false)]
		public short Strength
		{
			get { return m_strength; }
			set
			{
				Dirty = true;
				m_strength = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Constitution
		{
			get { return m_constitution; }
			set
			{
				Dirty = true;
				m_constitution = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Dexterity
		{
			get { return m_dexterity; }
			set
			{
				Dirty = true;
				m_dexterity = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Quickness
		{
			get { return m_quickness; }
			set
			{
				Dirty = true;
				m_quickness = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Intelligence
		{
			get { return m_intelligence; }
			set
			{
				Dirty = true;
				m_intelligence = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Piety
		{
			get { return m_piety; }
			set
			{
				Dirty = true;
				m_piety = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Charisma
		{
			get { return m_charisma; }
			set
			{
				Dirty = true;
				m_charisma = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public short Empathy
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

		[DataElement(AllowDbNull = false)]
		public byte AggroLevel
		{
			get { return m_aggroLevel; }
			set
			{
				Dirty = true;
				m_aggroLevel = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int AggroRange
		{
			get { return m_aggroRange; }
			set
			{
				Dirty = true;
				m_aggroRange = value;
			}
		}

		[DataElement( AllowDbNull = false)]
		public int Race
		{
			get { return m_race; }
			set
			{
				Dirty = true;
				m_race = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int BodyType
		{
			get { return m_bodyType; }
			set
			{
				Dirty = true;
				m_bodyType = value;
			}
		}
		
		/// <summary>
		/// The Mob's max distance from its spawn before return automatically
		/// if MaxDistance > 0 ... the amount is the normal value
		/// if MaxDistance = 0 ... no maxdistance check
		/// if MaxDistance less than 0 ... the amount is calculated in procent of the value and the aggrorange (in StandardMobBrain)
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int MaxDistance
		{
			get
			{
				return m_maxdistance;
			}
			set
			{
				Dirty = true;
				m_maxdistance = value;
			}
		}

		/// <summary>
		/// The mob's tether range; if mob is pulled farther than this distance
		/// it will return to its spawn point.
		/// if TetherRange > 0 ... the amount is the normal value
		/// if TetherRange less than or equal to 0 ... no tether check
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int TetherRange
		{
			get
			{
				return m_tetherRange;
			}
			set
			{
				Dirty = true;
				m_tetherRange = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte VisibleWeaponSlots
		{
			get
			{
				return m_visibleWeaponSlots;
			}
			set
			{
				Dirty = true;
				m_visibleWeaponSlots = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public bool ReplaceMobValues
		{
			get
			{
				return m_replaceMobValues;
			}
			set
			{
				Dirty = true;
				m_replaceMobValues = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string PackageID
		{
			get { return m_packageID; }
			set
			{
				Dirty = true;
				m_packageID = value;
			}
        }
        #endregion Properties
    }
}
