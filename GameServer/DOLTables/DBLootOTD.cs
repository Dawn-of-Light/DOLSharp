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
		/// Account table
		/// </summary>
		[Serializable]//TableName="LootOTD")]
		public class DBLootOTD : DatabaseObject 
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
                : base()
			{
				
				m_autoSave = false;
			}


			/// <summary>
			/// The item template id of the OTD
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public string ItemTemplateID
			{
				get
				{
					return m_itemTemplateID;
				}
				set
				{   
					m_Dirty = true;
					m_itemTemplateID = value;
				}
			}

			/// <summary>
			/// The minimum level require to drop the OTD
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public int MinLevel
			{
				get
				{
					return m_minLevel;
				}
				set
				{   
					m_Dirty = true;
					m_minLevel = value;
				}
			}

			/// <summary>
			/// The class allowed to drop OTD
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public string SerializedClassAllowed
			{
				get
				{
					return m_classAllowed;
				}
				set
				{   
					m_Dirty = true;
					m_classAllowed = value;
				}
			}
			/// <summary>
			/// The mob who drop the OTD
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public string MobName
			{
				get
				{
					return m_mobName;
				}
				set
				{   
					m_Dirty = true;
					m_mobName = value;
				}
			}

	
			/// <summary>
			/// List of charcter the account own
			/// </summary>
			public ItemTemplate item;
            public override void FillObjectRelations()
            {
                item = (ItemTemplate)DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), ItemTemplateID);
                base.FillObjectRelations();
            }
		}
	}
}
