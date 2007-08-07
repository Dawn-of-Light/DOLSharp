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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	public enum eMerchantWindowSlot : int
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
		public const byte MAX_ITEM_IN_TRADEWINDOWS = 30;

		/// <summary>
		/// The maximum number of pages supported by clients
		/// </summary>
		public const int MAX_PAGES_IN_TRADEWINDOWS = 5;

		#region Constructor/Declaration

		// for client one page is 30 items, just need to use scrollbar to see them all
		// item30 will be on page 1
		// item31 will be on page 2

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="itemsListId"></param>
		public MerchantTradeItems(uint itemsListId)
		{
			m_itemsListID = itemsListId;
		}

		/// <summary>
		/// Item list id
		/// </summary>
		protected uint m_itemsListID;

		/// <summary>
		/// Item list id
		/// </summary>
		public uint ItemsListID
		{
			get { return m_itemsListID; }
		}

		/// <summary>
		/// Holds item template instances defined with script
		/// </summary>
		protected HybridDictionary m_usedItemsTemplates = new HybridDictionary();

		#endregion

		#region Add Trade Item

		/// <summary>
		/// Adds an item to the merchant item list
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">Zero-based slot number</param>
		/// <param name="item">The item template to add</param>
		public virtual bool AddTradeItem(int page, eMerchantWindowSlot slot, ItemTemplate item)
		{
			lock (m_usedItemsTemplates.SyncRoot)
			{
				slot = GetValidSlot(page, slot);
				if (slot == eMerchantWindowSlot.Invalid || item == null) return false;
				m_usedItemsTemplates[(int)slot] = item;
			}
			return true;
		}

		/// <summary>
		/// Removes an item from trade window
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">Zero-based slot number</param>
		/// <returns>true if removed</returns>
		public virtual bool RemoveTradeItem(int page, eMerchantWindowSlot slot)
		{
			lock (m_usedItemsTemplates.SyncRoot)
			{
				slot = GetValidSlot(page, slot);
				if (slot == eMerchantWindowSlot.Invalid) return false;
				if (!m_usedItemsTemplates.Contains((int)slot)) return false;
				m_usedItemsTemplates.Remove((int)slot);
				return true;
			}
		}

		#endregion

		#region Get Inventory Informations

		/// <summary>
		/// Get the list of all items in the specified page
		/// </summary>
		public virtual IDictionary GetItemsInPage(int page)
		{
			try
			{
				HybridDictionary itemsInPage = new HybridDictionary(MAX_ITEM_IN_TRADEWINDOWS);
				if (m_itemsListID != 0)
				{
					DataObject[] itemList = GameServer.Database.SelectObjects(typeof(MerchantItem), "ItemListID = '" + m_itemsListID + "' AND PageNumber = '" + page + "'");
					foreach (MerchantItem merchantitem in itemList)
					{
						ItemTemplate item = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), merchantitem.ItemTemplateID);
						if (item != null)
						{
							ItemTemplate slotItem = (ItemTemplate)itemsInPage[merchantitem.SlotPosition];
							if (slotItem == null)
							{
								itemsInPage.Add(merchantitem.SlotPosition, item);
							}
							else
							{
								log.ErrorFormat("two merchant items on same page/slot: listID={0} page={1} slot={2}", m_itemsListID, page, merchantitem.SlotPosition);
							}
						}
					}
				}
				lock (m_usedItemsTemplates.SyncRoot)
				{
					foreach (DictionaryEntry de in m_usedItemsTemplates)
					{
						if ((int)de.Key/MAX_ITEM_IN_TRADEWINDOWS == page)
							itemsInPage[(int)de.Key] = (ItemTemplate)de.Value;
					}
				}
				return itemsInPage;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Loading merchant items list (" + m_itemsListID + ") page (" + page + "): ", e);
				return new HybridDictionary();
			}
		}

		/// <summary>
		/// Get the item in the specified page and slot
		/// </summary>
		/// <param name="page">The item page</param>
		/// <param name="slot">The item slot</param>
		/// <returns>Item template or null</returns>
		public virtual ItemTemplate GetItem(int page, eMerchantWindowSlot slot)
		{
			try
			{
				slot = GetValidSlot(page, slot);
				if (slot == eMerchantWindowSlot.Invalid) return null;

				ItemTemplate item;
				lock (m_usedItemsTemplates.SyncRoot)
				{
					item = m_usedItemsTemplates[(int)slot] as ItemTemplate;
					if (item != null) return item;
				}

				if (m_itemsListID != 0)
				{
					MerchantItem itemToFind = (MerchantItem)GameServer.Database.SelectObject(typeof (MerchantItem), "ItemListID = '" + m_itemsListID + "' AND PageNumber = '" + page + "' AND SlotPosition = '" + (int)slot + "'");
					if (itemToFind != null)
					{
						item = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof (ItemTemplate), itemToFind.ItemTemplateID);
					}
				}
				return item;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Loading merchant items list (" + m_itemsListID + ") page (" + page + ") slot (" + slot + "): ", e);
				return null;
			}
		}

		/// <summary>
		/// Gets a copy of all intems in trade window
		/// </summary>
		/// <returns>A list where key is the slot position and value is the ItemTemplate</returns>
		public virtual IDictionary GetAllItems()
		{
			try
			{
				Hashtable allItems = new Hashtable();
				if (m_itemsListID != null)
				{
					DataObject[] itemList = GameServer.Database.SelectObjects(typeof(MerchantItem), "ItemListID = '" + m_itemsListID + "'");
					foreach (MerchantItem merchantitem in itemList)
					{
						ItemTemplate item = (ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), merchantitem.ItemTemplateID);
						if (item != null)
						{
							ItemTemplate slotItem = (ItemTemplate)allItems[merchantitem.SlotPosition];
							if (slotItem == null)
							{
								allItems.Add(merchantitem.SlotPosition, item);
							}
							else
							{
								log.ErrorFormat("two merchant items on same page/slot: listID={0} page={1} slot={2}", m_itemsListID, merchantitem.PageNumber, merchantitem.SlotPosition);
							}
						}
					}
				}

				lock (m_usedItemsTemplates.SyncRoot)
				{
					foreach (DictionaryEntry de in m_usedItemsTemplates)
					{
						allItems[(int)de.Key] = (ItemTemplate)de.Value;
					}
				}
				return allItems;
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Loading merchant items list (" + m_itemsListID + "):", e);
				return new HybridDictionary();
			}
		}

		/// <summary>
		/// Check if the slot is valid
		/// </summary>
		/// <param name="page">Zero-based page number</param>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eMerchantWindowSlot.Invalid if not</returns>
		public virtual eMerchantWindowSlot GetValidSlot(int page, eMerchantWindowSlot slot)
		{
			if (page < 0 || page >= MAX_PAGES_IN_TRADEWINDOWS) return eMerchantWindowSlot.Invalid;

			if (slot == eMerchantWindowSlot.FirstEmptyInPage)
			{
				IDictionary itemsInPage = GetItemsInPage(page);
				for (int i = (int)eMerchantWindowSlot.FirstInPage; i < (int)eMerchantWindowSlot.LastInPage; i++)
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