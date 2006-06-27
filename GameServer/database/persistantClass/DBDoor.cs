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

namespace DOL.GS.Database
{
	/// <summary>
	/// DBDoor is database of door with state of door and X,Y,Z
	/// </summary>
	public class DBDoor
	{
		private int m_id;	
		private string m_name;
		private int	m_xpos;
		private int	m_ypos;
		private int	m_zpos;
		private int m_heading;
		//all following variable is for keep door
		private int m_health;
		private int m_keepid;

		/// <summary>
		/// Create a door row
		/// </summary>
		public DBDoor()
		{
			m_zpos=0;
			m_ypos=0;
			m_xpos=0;
			m_heading=0;
			m_name="";
			m_keepid = 0;
		}

		/// <summary>
		/// The unique id of this door
		/// </summary>
		public int DoorID
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
		/// Name of door
		/// </summary>
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		/// <summary>
		/// X position of door
		/// </summary>
		public int X
		{
			get
			{
				return m_xpos;
			}
			set
			{
				m_xpos = value;
			}
		}

		/// <summary>
		/// Y position of door
		/// </summary>
		public int Y
		{
			get
			{
				return m_ypos;
			}
			set
			{
				m_ypos = value;
			}
		}

		/// <summary>
		/// Z position of door
		/// </summary>
		public int Z
		{
			get
			{
				return m_zpos;
			}
			set
			{
				m_zpos = value;
			}
		}
		
		/// <summary>
		/// Heading of door
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
		/// Keep id when it is keep door
		/// </summary>
		public int KeepID
		{
			get
			{
				return m_keepid;
			}
			set
			{
				m_keepid = value;
			}
		}

		/// <summary>
		/// Health of door when keep door
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
	}
}
