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

using DOL.Database;
using DOL.Database.Attributes;
using System.Collections;

namespace DOL.Database
{
	/// <summary>
	/// The database side of GameMob
	/// </summary>
	[DataTable(TableName = "Mob")]
	public class Mob : DataObject
	{
		private string m_type;
		private string m_name;
		private string m_guild;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_speed;
		private ushort m_heading;
		private ushort m_region;
		private ushort m_model;
		private byte m_size;
		private byte m_level;
		private byte m_realm;
		private uint m_flags;
		private int m_aggrolevel;
		private int m_aggrorange;
		private int m_meleeDamageType;
		private int m_respawnInterval;
		private int m_faction;

		private string m_equipmentTemplateID;

		private string m_itemsListTemplateID;

		private int m_npcTemplateID;
		private int m_bodyType;

		private int m_inHouse;

		static bool m_autoSave;

		/// <summary>
		/// The Constructor
		/// </summary>
		public Mob()
		{
			m_autoSave = false;
			m_type = "DOL.GS.GameNPC";
			m_equipmentTemplateID = "";
			m_npcTemplateID = -1;
			m_meleeDamageType = 2; // slash by default
			m_respawnInterval = -1; // randow respawn by default
			m_guild = "";
			m_bodyType = 0;
			m_inHouse = -1;
		}

		/// <summary>
		/// AutoSave
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		/// <summary>
		/// The Mob's ClassType
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string ClassType
		{
			get
			{
				return m_type;
			}
			set
			{
				Dirty = true;
				m_type = value;
			}
		}

		/// <summary>
		/// The Mob's Name
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
		/// The Mob's Guild Name
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Guild
		{
			get
			{
				return m_guild;
			}
			set
			{
				Dirty = true;
				m_guild = value;
			}
		}

		/// <summary>
		/// The Mob's X Position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				Dirty = true;
				m_x = value;
			}
		}

		/// <summary>
		/// The Mob's Y Position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				Dirty = true;
				m_y = value;
			}
		}

		/// <summary>
		/// The Mob's Z Position
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				Dirty = true;
				m_z = value;
			}
		}

		/// <summary>
		/// The Mob's Max Speed
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Speed
		{
			get
			{
				return m_speed;
			}
			set
			{
				Dirty = true;
				m_speed = value;
			}
		}

		/// <summary>
		/// The Mob's Heading
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Heading
		{
			get
			{
				return m_heading;
			}
			set
			{
				Dirty = true;
				m_heading = value;
			}
		}

		/// <summary>
		/// The Mob's Region ID
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Region
		{
			get
			{
				return m_region;
			}
			set
			{
				Dirty = true;
				m_region = value;
			}
		}

		/// <summary>
		/// The Mob's Model
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public ushort Model
		{
			get
			{
				return m_model;
			}
			set
			{
				Dirty = true;
				m_model = value;
			}
		}

		/// <summary>
		/// The Mob's Size
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Size
		{
			get
			{
				return m_size;
			}
			set
			{
				Dirty = true;
				m_size = value;
			}
		}

		/// <summary>
		/// The Mob's Level
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}

		/// <summary>
		/// The Mob's Realm
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public byte Realm
		{
			get
			{
				return m_realm;
			}
			set
			{
				Dirty = true;
				m_realm = value;
			}
		}

		/// <summary>
		/// The Mob's Equipment Template ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string EquipmentTemplateID
		{
			get
			{
				return m_equipmentTemplateID;
			}
			set
			{
				Dirty = true;
				m_equipmentTemplateID = value;
			}
		}

		/// <summary>
		/// The Mob's Items List Template ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string ItemsListTemplateID
		{
			get
			{
				return m_itemsListTemplateID;
			}
			set
			{
				Dirty = true;
				m_itemsListTemplateID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int NPCTemplateID
		{
			get
			{
				return m_npcTemplateID;
			}
			set
			{
				Dirty = true;
				m_npcTemplateID = value;
			}
		}

		/// <summary>
		/// The Mob's Flags
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public uint Flags
		{
			get
			{
				return m_flags;
			}
			set
			{
				Dirty = true;
				m_flags = value;
			}
		}

		/// <summary>
		/// The Mob's Aggro Level
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int AggroLevel
		{
			get { return m_aggrolevel; }
			set { Dirty = true; m_aggrolevel = value; }
		}

		/// <summary>
		/// The Mob's Aggro Range
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int AggroRange
		{
			get { return m_aggrorange; }
			set { Dirty = true; m_aggrorange = value; }
		}

		/// <summary>
		/// The Mob's Melee Damage Type
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set { Dirty = true; m_meleeDamageType = value; }
		}

		/// <summary>
		/// The Mob's Respawn Interval in seconds
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int RespawnInterval
		{
			get { return m_respawnInterval; }
			set { Dirty = true; m_respawnInterval = value; }
		}

		/// <summary>
		/// The Mob's Faction ID
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int FactionID
		{
			get { return m_faction; }
			set { Dirty = true; m_faction = value; }
		}

		/// <summary>
		/// The mob's body type
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int BodyType
		{
			get { return m_bodyType; }
			set { Dirty = true; m_bodyType = value; }
		}

		/// <summary>
		/// The mob's current house
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int HouseNumber
		{
			get { return m_inHouse; }
			set { Dirty = true; m_inHouse = value; }
		}
	}
}

