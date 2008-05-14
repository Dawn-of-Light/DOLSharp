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
using System.Collections.Generic;
using DOL.Database2;


namespace DOL
{
	namespace Database2
	{
		/// <summary>
		/// Crafted item table
		/// </summary>
		[Serializable]//TableName="CraftedItem")]
		public class DBCraftedItem : DatabaseObject
		{
			private string m_craftedItemID;
			private int m_craftinglevel;
			private string m_id_nb;
			private int m_craftingSkillType;
			static bool	m_autoSave;

			/// <summary>
			/// Create an crafted item
			/// </summary>
            public DBCraftedItem()
                : base()
			{
				m_autoSave=false;
			}


			/// <summary>
			/// Crafting id of item to craft
			/// </summary>
			////[PrimaryKey]
			public string CraftedItemID
			{
				get
				{
					return m_craftedItemID;
				}
				set
				{
					m_Dirty = true;
					m_craftedItemID = value;
				}
			}

			/// <summary>
			/// Index of item to craft
			/// </summary>
			//[DataElement(AllowDbNull=false, Index=true)]
			public string Id_nb
			{
				get
				{
					return m_id_nb;
				}
				set
				{
					m_Dirty = true;
					m_id_nb = value;
				}
			}

			/// <summary>
			/// Crafting level of this item
			/// </summary>
			//[DataElement(AllowDbNull=false,Unique=false)]
			public int CraftingLevel
			{
				get
				{
					return m_craftinglevel;
				}
				set
				{
					m_Dirty = true;
					m_craftinglevel = value;
				}
			}
			
			/// <summary>
			/// Crafting skill needed to craft this item
			/// </summary>
			//[DataElement(AllowDbNull=false)]
			public int CraftingSkillType
			{
				get
				{
					return m_craftingSkillType;
				}
				set
				{
					m_Dirty = true;
					m_craftingSkillType = value;
				}
			}

			/// <summary>
			/// List of raw material needed
			/// </summary>
			public List<DBCraftedXItem> RawMaterials;

			/// <summary>
			/// The item to craft
			/// </summary>
			public ItemTemplate ItemTemplate;
            public override void FillObjectRelations()
            {
                RawMaterials = DatabaseLayer.Instance.SelectObjects<DBCraftedXItem>("CraftedId_nb", Id_nb);
                ItemTemplate = (ItemTemplate)DatabaseLayer.Instance.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), Id_nb);
            }
		}
	}
}
