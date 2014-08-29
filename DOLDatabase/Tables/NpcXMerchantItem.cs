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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Link an NPC to a Merchant List (turning it into a merchant).
	/// </summary>
	[DataTable(TableName = "npc_xmerchantitem")]
	public class NpcXMerchantItem : DataObject
	{
		private long m_npc_xmerchantitemID;
		
		/// <summary>
		/// Npc X Merchant Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xmerchantitemID {
			get { return m_npc_xmerchantitemID; }
			set { m_npc_xmerchantitemID = value; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Spot ID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_itemListID;
		
		/// <summary>
		/// ItemList ID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 150)]
		public string ItemListID {
			get { return m_itemListID; }
			set { m_itemListID = value; Dirty = true; }
		}
			
		/// <summary>
		/// Link to MerchantItem Table (1,n)
		/// </summary>
		[Relation(LocalField = "ItemListID", RemoteField = "ItemListID", AutoLoad = true, AutoDelete = false)]
		public MerchantItem[] ItemList;
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcXMerchantItem()
		{
		}
	}
}
