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
		/// Account table
		/// </summary>
		[DataTable(TableName="LootOTD")]
		public class DBLootOTD : DataObject 
		{
			private string m_itemTemplateID;
			private int m_minLevel;
			private string m_classAllowed;
			private string m_mobName;
			
			private static bool m_autoSave;

			/// <summary>
			/// Create account row in DB
			/// </summary>
			public DBLootOTD() 
			{
				
				m_autoSave = false;
			}

			/// <summary>
			/// Auto save this table
			/// </summary>
			override public  bool AutoSave
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
			/// The item template id of the OTD
			/// </summary>
			[DataElement(AllowDbNull=false)]
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

			/// <summary>
			/// The minimum level require to drop the OTD
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public int MinLevel
			{
				get
				{
					return m_minLevel;
				}
				set
				{   
					Dirty = true;
					m_minLevel = value;
				}
			}

			/// <summary>
			/// The class allowed to drop OTD
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public string SerializedClassAllowed
			{
				get
				{
					return m_classAllowed;
				}
				set
				{   
					Dirty = true;
					m_classAllowed = value;
				}
			}
			/// <summary>
			/// The mob who drop the OTD
			/// </summary>
			[DataElement(AllowDbNull=false)]
			public string MobName
			{
				get
				{
					return m_mobName;
				}
				set
				{   
					Dirty = true;
					m_mobName = value;
				}
			}

	
			/// <summary>
			/// List of charcter the account own
			/// </summary>
			[Relation(LocalField = "ItemTemplateID", RemoteField = "Id_nb", AutoLoad = true, AutoDelete=false)]
			public ItemTemplate item;
		}
	}
}
