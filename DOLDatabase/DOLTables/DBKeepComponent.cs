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
	/// DB Keep component is database of keep
	/// </summary>
	[DataTable(TableName="KeepComponent")]
	public class DBKeepComponent : DataObject
	{
		static bool	m_autoSave;
		private int m_skin;//todo eskin
		private int m_x;
		private int m_y;
		private int m_heading;
		private int m_height;
		private int m_health;
		private int m_keepID;
		private int m_keepComponentID;

		/// <summary>
		/// Create a component of keep (wall, tower,gate, ...)
		/// </summary>
		public DBKeepComponent()
		{
			m_autoSave=false;
			m_skin = 0;
			m_x = 0;
			m_y = 0;
			m_heading = 0;
			m_height = 0;
			m_health = 0;
			m_keepID = 0;
			m_keepComponentID = 0;
		}

		/// <summary>
		/// Create a component of keep (wall, tower,gate, ...)
		/// </summary>
		public DBKeepComponent(int componentID, int componentSkinID, int componentX, int componentY, int componentHead, int componentHeight, int componentHealth, int keepid) : this()
		{
			m_autoSave=false;
			m_skin = componentSkinID;
			m_x = componentX;
			m_y = componentY;
			m_heading = componentHead;
			m_height = componentHeight;
			m_health = componentHealth;
			m_keepID = 0;
			m_keepComponentID = componentID;
		}

		/// <summary>
		/// autosave table
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
		/// X position of component
		/// </summary>
		[DataElement(AllowDbNull=true)]
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
		/// Y position of component
		/// </summary>
		[DataElement(AllowDbNull=true)]
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
		/// Heading of component
		/// </summary>
		[DataElement(AllowDbNull=true)]
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
		/// Health of component
		/// </summary>
		[DataElement(AllowDbNull=false)]
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
		
		/// <summary>
		/// Skin of component (see enum skin in GameKeepComponent)
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int Skin
		{
			get
			{
				return m_skin;
			}
			set
			{   
				Dirty = true;
				m_skin = value;
			}
		}

		/// <summary>
		/// Index of keep
		/// </summary>
		[DataElement(AllowDbNull=true, Index=true)]
		public int KeepID
		{
			get
			{
				return m_keepID;
			}
			set
			{   
				Dirty = true;
				m_keepID = value;
			}
		}

		/// <summary>
		/// Index of component
		/// </summary>
		[DataElement(AllowDbNull=true)]
		public int ID
		{
			get
			{
				return m_keepComponentID;
			}
			set
			{   
				Dirty = true;
				m_keepComponentID = value;
			}
		}
		
	}
}
