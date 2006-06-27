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

namespace DOL.GS.Database
{
	/// <summary>
	/// DB Keep component is database of keep
	/// </summary>
	public class DBKeepComponent
	{
		private int m_id;
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
			m_skin = componentSkinID;
			m_x = componentX;
			m_y = componentY;
			m_heading = componentHead;
			m_height = componentHeight;
			m_health = componentHealth;
			m_keepID = 0;
			m_keepComponentID = componentID;
		}

		public int KeepComponentID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}

		/// <summary>
		/// X position of component
		/// </summary>
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{   
				m_x = value;
			}
		}

		/// <summary>
		/// Y position of component
		/// </summary>
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{   
				m_y = value;
			}
		}

		/// <summary>
		/// Heading of component
		/// </summary>
		public int Heading
		{
			get
			{
				return m_heading;
			}
			set
			{   
				m_heading = value;
			}
		}

		/// <summary>
		/// Height of component
		/// </summary>
		public int Height
		{
			get
			{
				return m_height;
			}
			set
			{   
				m_height = value;
			}
		}

		/// <summary>
		/// Health of component
		/// </summary>
		public int Health
		{
			get
			{
				return m_health;
			}
			set
			{   
				m_health = value;
			}
		}
		
		/// <summary>
		/// Skin of component (see enum skin in GameKeepComponent)
		/// </summary>
		public int Skin
		{
			get
			{
				return m_skin;
			}
			set
			{   
				m_skin = value;
			}
		}

		/// <summary>
		/// Index of keep
		/// </summary>
		public int KeepID
		{
			get
			{
				return m_keepID;
			}
			set
			{   
				m_keepID = value;
			}
		}

		/// <summary>
		/// Index of component
		/// </summary>
		public int ID
		{
			get
			{
				return m_keepComponentID;
			}
			set
			{   
				m_keepComponentID = value;
			}
		}
		
	}
}
