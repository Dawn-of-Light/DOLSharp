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

namespace DOL.Database
{
	/// <summary>
	/// Factions object for database
	/// </summary>
	[DataTable(TableName="linked_faction")]
	public class DBLinkedFaction : DataObject
	{
		private int	m_factionID;
		private int	m_linkedFactionID;
		private bool	m_friend;
		static bool		m_autoSave;

		/// <summary>
		/// Create faction linked to an other
		/// </summary>
		public DBLinkedFaction()
		{
			m_factionID = 0;
			m_linkedFactionID = 0;
			m_friend = true;
			m_autoSave = false;
		}

		/// <summary>
		/// autosave table
		/// </summary>
		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
		}

		/// <summary>
		/// Index of faction 
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public int FactionID
		{
			get 
            {
                return m_factionID;
            }
			set	
			{
				Dirty = true;
                m_factionID = value;
            }
		}		

		/// <summary>
		/// The linked faction index
		/// </summary>
		[DataElement(AllowDbNull=true,Unique=false)]
		public int LinkedFactionID
		{
			get 
            {
                return m_linkedFactionID;
            }
			set	
			{
				Dirty = true;
                m_linkedFactionID = value;
            }
		}

		/// <summary>
		/// Is faction linked is friend or enemy
		/// </summary>
		[DataElement(AllowDbNull=true,Unique=false)]
		public bool IsFriend
		{
			get 
			{
				return m_friend;
			}
			set	
			{
				Dirty = true;
				m_friend = value;
			}
		}
	}
}
