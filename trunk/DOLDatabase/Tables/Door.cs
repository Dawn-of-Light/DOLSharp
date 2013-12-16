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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// DBDoor is database of door with state of door and X,Y,Z
	/// </summary>
	[DataTable(TableName = "Door")]
	public class DBDoor : DataObject
	{
		private int m_xpos;
		private int m_ypos;
		private int m_zpos;
		private int m_heading;
		private string m_name;
		private int m_type;
		private int m_internalID;
		private byte m_level;
		private byte m_realm;
		private string m_guild;
		private uint m_flags;
		// private int m_constitution;
		private int m_locked;
		private int m_health;
		private int m_maxHealth;
		
		/// <summary>
		/// Create a door row
		/// </summary>
		public DBDoor()
		{
			m_zpos = 0;
			m_ypos = 0;
			m_xpos = 0;
			m_heading = 0;
			m_name = "";
			m_internalID = 0;
		}

		/// <summary>
		/// Name of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
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

		[DataElement(AllowDbNull = true)]
		public int Type
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
		/// Z position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Z
		{
			get
			{
				return m_zpos;
			}
			set
			{
				Dirty = true;
				m_zpos = value;
			}
		}

		/// <summary>
		/// Y position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Y
		{
			get
			{
				return m_ypos;
			}
			set
			{
				Dirty = true;
				m_ypos = value;
			}
		}

		/// <summary>
		/// X position of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int X
		{
			get
			{
				return m_xpos;
			}
			set
			{
				Dirty = true;
				m_xpos = value;
			}
		}

		/// <summary>
		/// Heading of door
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public int Heading
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
		/// Internal index of Door
		/// </summary>
		[DataElement(AllowDbNull = true, Index = true)]
		public int InternalID
		{
			get
			{
				return m_internalID;
			}
			set
			{
				Dirty = true;
				m_internalID = value;
			}
		}
		
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

		/* 	[DataElement(AllowDbNull = false)]
			public int Constitution
			{
				get
				{
					return m_constitution;
				}
				set
				{
					Dirty = true;
					m_constitution = value;
				}
			}
		 */
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
		
		[DataElement(AllowDbNull = false)]
		public int Locked
		{
			get
			{
				return m_locked;
			}
			set
			{
				Dirty = true;
				m_locked = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int Health
		{
			get
			{
				return m_health;
			}
			set
			{
				Dirty = true;
				m_health = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public int MaxHealth
		{
			get
			{
				return m_maxHealth;
			}
			set
			{
				Dirty = true;
				m_maxHealth = value;
			}
		}
	}
}
