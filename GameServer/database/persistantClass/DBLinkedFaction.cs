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
	/// Factions object for database
	/// </summary>
	public class DBLinkedFaction
	{
		private int	m_id;	
		private int	m_factionID;
		private int	m_linkedFactionID;
		private bool	m_friend;

		/// <summary>
		/// Create faction linked to an other
		/// </summary>
		public DBLinkedFaction()
		{
			m_factionID = 0;
			m_linkedFactionID = 0;
			m_friend = true;
		}

		
		public int DBLinkedFactionID
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
		/// Index of faction 
		/// </summary>
		public int FactionID
		{
			get 
            {
                return m_factionID;
            }
			set	
			{
				m_factionID = value;
            }
		}		

		/// <summary>
		/// The linked faction index
		/// </summary>
		public int LinkedFactionID
		{
			get 
            {
                return m_linkedFactionID;
            }
			set	
			{
				m_linkedFactionID = value;
            }
		}

		/// <summary>
		/// Is faction linked is friend or enemy
		/// </summary>
		public bool IsFriend
		{
			get 
			{
				return m_friend;
			}
			set	
			{
				m_friend = value;
			}
		}
	}
}
