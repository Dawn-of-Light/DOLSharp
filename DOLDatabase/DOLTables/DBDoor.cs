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
	[DataTable(TableName = "door")]
	public class DBDoor : DataObject
	{
		static bool m_autoSave;

		private int m_xpos;
		private int m_ypos;
		private int m_zpos;
		private int m_heading;
		private string m_name;
		private int m_internalID;

		/// <summary>
		/// Create a door row
		/// </summary>
		public DBDoor()
		{
			m_autoSave = false;
			ObjectId = 0;
			m_zpos = 0;
			m_ypos = 0;
			m_xpos = 0;
			m_heading = 0;
			m_name = "";
			m_internalID = 0;
		}

		/// <summary>
		/// Auto save table
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
		[DataElement(AllowDbNull = true)]
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
	}
}
