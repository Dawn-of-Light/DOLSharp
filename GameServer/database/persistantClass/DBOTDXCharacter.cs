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
	/// character which have already drop otd is in this table
	/// </summary>
	public class DBOTDXCharacter
	{
		private int		m_id;
		private string	m_lootOTD_ID;
		private string	m_characterName;
		
		
		/// <summary>
		/// Create account row in DB
		/// </summary>
		public DBOTDXCharacter() 
		{
			m_lootOTD_ID = "";
			m_characterName = "";
		}

		public int OTDXCharacterID
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
		/// The character name of player who have ever drop otd
		/// </summary>
		public string CharacterName
		{
			get
			{
				return m_characterName;
			}
			set
			{   
				m_characterName = value;
			}
		}

		/// <summary>
		/// The object id of the DBLootOtd
		/// </summary>
		public string LootOTD_ID
		{
			get
			{
				return m_lootOTD_ID;
			}
			set
			{   
				m_lootOTD_ID = value;
			}
		}
	}
}
