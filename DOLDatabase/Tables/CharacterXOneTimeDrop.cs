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

namespace DOL
{
	namespace Database
	{
		/// <summary>
		/// List of characters and the one time drops they have received.
		/// </summary>
		[DataTable(TableName="CharacterXOneTimeDrop")]
		public class CharacterXOneTimeDrop : DataObject
		{
			private string m_characterID;
			private string m_itemTemplateID;

			public CharacterXOneTimeDrop()
			{
				m_itemTemplateID = "";
				m_characterID = "";
				AutoSave = false;
			}

			/// <summary>
			/// The DOLCharacters_ID of the player who gets the drop
			/// </summary>
			[DataElement(AllowDbNull = false, Varchar = 100, Index = true)]
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
			/// The item id_nb that was dropped
			/// </summary>
			[DataElement(AllowDbNull = false, Varchar = 100, Index = true)]
			public string ItemTemplateID
			{
				get
				{
					return m_itemTemplateID;
				}
				set
				{
					Dirty = true;
					m_itemTemplateID = value;
				}
			}
		}
	}
}
