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
	/// 
	/// </summary>
	public class DBPathPoint
	{
		protected string m_id;
		protected int m_step;
		protected int m_x;
		protected int m_y;
		protected int m_z;
		protected int m_maxspeed;
		protected int m_waitTime;

		public String PathPointID
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

		public int Step
		{
			get 
			{ 
				return m_step; 
			}
			set 
			{ 
				m_step = value; 
			}
		}

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

		public int Z
		{
			get 
			{ 
				return m_z;
			}
			set
			{ 
				m_z = value;
			}
		}

		/// <summary>
		/// Maximum speed, 0 = no limit
		/// </summary>
		public int MaxSpeed 
		{
			get 
			{ 
				return m_maxspeed; }
			set 
			{ 
				m_maxspeed = value;
			}
		}

		public int WaitTime 
		{
			get 
			{ 
				return m_waitTime;
			}
			set
			{ 
				m_waitTime = value;
			}
		}
	}
}
