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

using DOL.Database2;


namespace DOL
{
	namespace Database2
	{
		/// <summary>
		/// character which have already drop otd is in this table
		/// </summary>
		[Serializable]//TableName="OTDXCharacter")]
		public class DBOTDXCharacter : DatabaseObject 
		{
			private UInt64 m_lootOTD_ID;
			private string m_CharacterName;
			
			private static bool m_autoSave;

			/// <summary>
			/// Create account row in DB
			/// </summary>
            public DBOTDXCharacter()
                : base()
			{
				m_lootOTD_ID = 0;
				m_CharacterName = "";
				m_autoSave = false;
			}


			/// <summary>
			/// The character name of player who have ever drop otd
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public string CharacterName
			{
				get
				{
					return m_CharacterName;
				}
				set
				{   
					m_Dirty = true;
					m_CharacterName = value;
				}
			}

			/// <summary>
			/// The object id of the DBLootOtd
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public UInt64 LootOTD_ID
			{
				get
				{
					return m_lootOTD_ID;
				}
				set
				{   
					m_Dirty = true;
					m_lootOTD_ID = value;
				}
			}
		}
	}
}
