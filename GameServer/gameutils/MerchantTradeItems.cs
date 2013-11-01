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
using System.Collections.Generic;
using System.Reflection;

using DOL.Database;

using log4net;

namespace DOL.GS
{
	public enum eMerchantWindowSlot : short
	{
		FirstEmptyInPage = -2,
		Invalid = -1,

		FirstInPage = 0,
		LastInPage = MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS - 1,
	}

	/// <summary>
	/// This class represents a full merchant item list
	/// and contains functions that can be used to
	/// add and remove items
	/// </summary>
	public class MerchantTradeItems
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// The maximum number of items on one page
		/// </summary>
		public const ushort MAX_ITEM_IN_TRADEWINDOWS = 30;

		/// <summary>
		/// The maximum number of pages supported by clients
		/// </summary>
		public const byte MAX_PAGES_IN_TRADEWINDOWS = 5;
		
		/// <summary>
		/// Cached Database content
		/// </summary>
		protected static Dictionary<string, Dictionary<ushort, ItemTemplate>> m_cachedItemList = new Dictionary<string, Dictionary<ushort, ItemTemplate>>();

		/// <summary>
		/// Holds item template instances defined with script
		/// </summary>
		protected Dictionary<ushort, ItemTemplate> m_usedItemsTemplates;

		/// <summary>
		/// Item list id
		/// </summary>
		protected readonly string m_itemsListID;

		/// <summary>
		/// Item list id
		/// </summary>
		public string ItemsListID
		{
			get { return m_itemsListID; }
		}

		
		#region Constructor/Declaration

		// for client one page is 30 items, just need to use scrollbar to see them all
		// item29 will be on page 0
		// item30 will be on page 1

		public MerchantTradeItems()
		{
			m_itemsListID = "";
			m_usedItemsTemplates = new Dictionary<ushort, ItemTemplate>();
		}
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="itemsListId"></param>
		public MerchantTradeItems(string itemsListId)
		{
			bool load = false;
			if(itemsListId != null)
			{
				m_itemsListID = itemsListId.ToLower();
			}
			else
			{
				m_itemsListID = "";
			}
			
			if(!Util.IsEmpty(ItemsListID))
			{
				lock(((ICollection)m_cachedItemList).SyncRoot)
				{
					if(m_cachedItemList.ContainsKey(ItemsListID))
					{
						m_usedItemsTemplates = m_cachedItemList[ItemsListID];
					}
					else 
					{
						// Load it from DB !
						load = true;
					}
				}
				
				if(load)
					LoadFromDatabase();
			   	
			}
			else 
			{
				m_usedItemsTemplates = new Dictionary<ushort, ItemTemplate>();
			}
		}

		/// <summary>
		/// Load Items from database ONCE !
		/// </summary>
		public void LoadFromDatabase()
		{
			if(!Util.IsEmpty(ItemsListID))
			{

				IList<MerchantItem> itemList = GameServer.Database.SelectObjects<MerchantItem>("ItemListID = '" + GameServer.Database.Escape(ItemsListID) + "'");

				System.Text.StringBuilder itemtemplateString = new System.Text.StringBuilder();
				
				// build query list
				foreach(MerchantItem merc in itemList) 
				{
					itemtemplateString.AppendFormat("'{0}',", GameServer.Database.Escape(merc.ItemTemplateID));
				}
				
				//if we have an item template string we remove last coma and query with it
				if(itemtemplateString.Length > 0) 
				{
					
					itemtemplateString.Length--;
										
					Dictionary<string, ItemTemplate> itemtemplateDict = new Dictionary<string, ItemTemplate>();
					
					// Populate dict with Id_nb key
					foreach(ItemTemplate item in GameServer.Database.SelectObjects<ItemTemplate>("Id_nb IN("+itemtemplateString+")"))
					{
						if(!itemtemplateDict.ContainsKey(item.Id_nb.ToLower()) && item != null)
							itemtemplateDict[item.Id_nb.ToLower()] = item;
					}
					
					// Update cache with current item list.
					lock(((ICollection)m_cachedItemList).SyncRoot)
					{
						if(m_cachedItemList.ContainsKey(ItemsListID))
							m_cachedItemList.Remove(ItemsListID);
						
						m_cachedItemList[ItemsListID] = new Dictionary<ushort, ItemTemplate>();
					
						foreach(MerchantItem merc in itemList)
						{
							if (log.IsErrorEnabled && !itemtemplateDict.ContainsKey(merc.ItemTemplateID.ToLower()))
							{
								log.Error("Loading merchant items list (" + ItemsListID + "): itemtemplate "+merc.ItemTemplateID+" not found.");
							}
							
							if(log.IsErrorEnabled && m_cachedItemList[ItemsListID].ContainsKey((ushort)(merc.PageNumber*MAX_ITEM_IN_TRADEWINDOWS+merc.SlotPosition)))
							{
								log.Error("Loading merchant items list (" + ItemsListID + "): duplicate item at page "+merc.PageNumber+" slot : "+merc.SlotPosition+".");
							}
							
							// add item template to object member
							if(!m_cachedItemList[ItemsListID].ContainsKey((ushort)(merc.PageNumber*MAX_ITEM_IN_TRADEWINDOWS+merc.SlotPosition)) && itemtemplateDict.ContainsKey(merc.ItemTemplateID.ToLower()))
								m_cachedItemList[ItemsListID][(ushort)(merc.PageNumber*MAX_ITEM_IN_TRADEWINDOWS+merc.SlotPosition)] = itemtemplateDict[merc.ItemTemplateID.ToLower()];
						}
					}
					
					// prepare object member
					m_usedItemsTemplates = m_cachedItemList[ItemsListID];				
					
				}
				else
				{
					if (log.IsErrorEnabled)
						log.Error("Loading merchant items list (" + ItemsListID + "): no records found.");
				}
				
			}
			
		}
		#endregion

		#region Add Trade Item

		/// <summary>
		/// Adds an item to the merchant item list
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">Zero-based slot number</param>
		/// <param name="item">The item template to add</param>
		public virtual bool AddTradeItem(byte page, eMerchantWindowSlot slot, ItemTemplate item)
		{
			lock (((ICollection)m_usedItemsTemplates).SyncRoot)
			{
				if (item == null)
				{
					return false;
				}

				eMerchantWindowSlot pageSlot = GetValidSlot(page, slot);

				if (pageSlot == eMerchantWindowSlot.Invalid)
				{
					log.ErrorFormat("Invalid slot {0} specified for page {1} of TradeItemList {2}", slot, page, ItemsListID);
					return false;
				}

				m_usedItemsTemplates[(ushort)((page*MAX_ITEM_IN_TRADEWINDOWS)+(short)pageSlot)] = item;
			}

			return true;
		}

		/// <summary>
		/// Removes an item from trade window
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">Zero-based slot number</param>
		/// <returns>true if removed</returns>
		public virtual bool RemoveTradeItem(byte page, eMerchantWindowSlot slot)
		{
			slot = GetValidSlot(page, slot);
			
			if (slot == eMerchantWindowSlot.Invalid)
				return false;
			
			lock (((ICollection)m_usedItemsTemplates).SyncRoot)
			{
				
				if (!m_usedItemsTemplates.ContainsKey((ushort)((page*MAX_ITEM_IN_TRADEWINDOWS)+(short)slot))) 
					return false;
				
				return m_usedItemsTemplates.Remove((ushort)((page*MAX_ITEM_IN_TRADEWINDOWS)+(short)slot));
				
			}
		}

		#endregion

		#region Get Inventory Informations

		/// <summary>
		/// Get the list of all items in the specified page
		/// </summary>
		public virtual IDictionary GetItemsInPage(byte page)
		{
			Dictionary<ushort, ItemTemplate> result = new Dictionary<ushort, ItemTemplate>();
			
			if(m_usedItemsTemplates == null)
				return result;
			
			lock(((ICollection)m_usedItemsTemplates).SyncRoot)
			{

				foreach(ushort key in m_usedItemsTemplates.Keys)
				{
					if(key >= (MAX_ITEM_IN_TRADEWINDOWS*page) && key < (MAX_ITEM_IN_TRADEWINDOWS*(page+1)) && m_usedItemsTemplates[key] != null)
						result.Add((ushort)(key%MAX_ITEM_IN_TRADEWINDOWS), m_usedItemsTemplates[key]);
				}
			}
			
			return result;
		}

		/// <summary>
		/// Get the item in the specified page and slot
		/// </summary>
		/// <param name="page">The item page</param>
		/// <param name="slot">The item slot</param>
		/// <returns>Item template or null</returns>
		public virtual ItemTemplate GetItem(byte page, eMerchantWindowSlot slot)
		{
			slot = GetValidSlot(page, slot);
			
			if (slot == eMerchantWindowSlot.Invalid) 
				return null;

			lock (((ICollection)m_usedItemsTemplates).SyncRoot)
			{
				if(m_usedItemsTemplates.ContainsKey((ushort)((short)slot+(page*MAX_ITEM_IN_TRADEWINDOWS))))
					return m_usedItemsTemplates[(ushort)((short)slot+(page*MAX_ITEM_IN_TRADEWINDOWS))];
			}

			return null;
		}

		/// <summary>
		/// Gets a copy of all intems in trade window
		/// </summary>
		/// <returns>A list where key is the slot position and value is the ItemTemplate</returns>
		public virtual IDictionary GetAllItems()
		{
			return m_usedItemsTemplates;
		}

		/// <summary>
		/// Check if the slot is valid
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eMerchantWindowSlot.Invalid if not</returns>
		public virtual eMerchantWindowSlot GetValidSlot(byte page, eMerchantWindowSlot slot)
		{
			if (page < 0 || page >= MAX_PAGES_IN_TRADEWINDOWS) 
				return eMerchantWindowSlot.Invalid;

			if (slot == eMerchantWindowSlot.FirstEmptyInPage)
			{
				IDictionary itemsInPage = GetItemsInPage(page);
				
				for (ushort i = (ushort)eMerchantWindowSlot.FirstInPage; i < (ushort)eMerchantWindowSlot.LastInPage; i++)
				{
					if (!itemsInPage.Contains(i))
						return ((eMerchantWindowSlot)i);
				}
				return eMerchantWindowSlot.Invalid;
			}

			if (slot < eMerchantWindowSlot.FirstInPage || slot > eMerchantWindowSlot.LastInPage)
				return eMerchantWindowSlot.Invalid;

			return slot;
		}

		#endregion
	}
}