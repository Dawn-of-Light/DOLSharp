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
	/// Aggro level of faction against character
	/// </summary>
	/// 
	[DataTable(TableName="FactionAggroLevel")]
	public class DBFactionAggroLevel : DataObject
	{
		private string     m_characterID;
		private int	    m_factionID;
		private int 	m_AggroLevel;
		static bool		m_autoSave;

		/// <summary>
		/// Create faction aggro level against character
		/// </summary>
		public DBFactionAggroLevel()
		{
			m_characterID = "";
			m_factionID = 0;
			m_AggroLevel = 0;
			m_autoSave = false;
		}

		/// <summary>
		/// Autosave table
		/// </summary>
		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
		}
		
		/// <summary>
		/// Character
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public string CharacterID
		{
			get 
			{
				return m_characterID;
			}
			set	
			{
				Dirty = true;
				m_characterID = value;
			}
		}		

		/// <summary>
		/// index of this faction
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
		/// aggro level/ relationship of faction against character
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public int AggroLevel
		{
			get 
			{
				return m_AggroLevel;
			}
			set	
			{
				Dirty = true;
				m_AggroLevel = value;
			}
		}		
		
	}
}
