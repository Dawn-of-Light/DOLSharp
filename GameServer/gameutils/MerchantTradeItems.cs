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
using System.Linq;
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

    public class MerchantTradeItems
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const byte MAX_ITEM_IN_TRADEWINDOWS = 30;
        public const int MAX_PAGES_IN_TRADEWINDOWS = 5;

        #region Constructor/Declaration
        public MerchantTradeItems(string itemsListId)
        {
            Catalog = MerchantCatalog.LoadFromDatabase(itemsListId);
        }

        public MerchantTradeItems(MerchantCatalog catalog)
        {
            Catalog = catalog;
        }

        public string ItemsListID => Catalog.ItemListId;

        public MerchantCatalog Catalog { get; private set; }
        #endregion

        #region Add Trade Item
        public virtual bool AddTradeItem(int page, eMerchantWindowSlot slot, ItemTemplate item)
        {
            if (item == null) throw new NullReferenceException("Null may not be added as MerchantTradeItem.");

            slot = GetValidSlot(page, slot);

            if (slot == eMerchantWindowSlot.Invalid)
            {
                log.ErrorFormat("Invalid slot {0} specified for page {1} of TradeItemList {2}", slot, page, ItemsListID);
                return false;
            }

            Catalog.GetPage(page).Add(item, (byte)slot);

            return true;
        }

        public virtual bool RemoveTradeItem(int page, eMerchantWindowSlot slot)
        {
            slot = GetValidSlot(page, slot);
            if (slot == eMerchantWindowSlot.Invalid) return false;
            return Catalog.GetPage(page).Remove((byte)slot);
        }
        #endregion

        #region Get Inventory Informations
        public virtual IDictionary GetItemsInPage(int page)
        {
            var pageEntries = Catalog.GetPage(page).GetAllEntries();
            var result = new HybridDictionary();
            foreach (var entry in pageEntries)
            {
                result.Add(entry.SlotPosition, entry.Item);
            }
            return result;
        }

        public virtual ItemTemplate GetItem(int page, eMerchantWindowSlot slot)
            => Catalog.GetPage(page).GetEntry((byte)slot).Item;

        public virtual IDictionary GetAllItems()
        {
            var items = new Hashtable();
            var catalogEntries = Catalog.GetAllEntries();
            foreach (var entry in catalogEntries)
            {
                if (items.Contains((entry.Page, entry.SlotPosition)))
                {
                    log.ErrorFormat($"two merchant items on same page/slot: listID={ItemsListID} page={entry.Page} slot={entry.SlotPosition}");
                    continue;
                }
                items.Add((entry.Page, entry.SlotPosition), entry.Item);
            }
            return items;
        }

        public virtual eMerchantWindowSlot GetValidSlot(int page, eMerchantWindowSlot slot)
        {
            var isPageInvalid = page < 0 || page >= MAX_PAGES_IN_TRADEWINDOWS;
            if (isPageInvalid) return eMerchantWindowSlot.Invalid;

            if (slot == eMerchantWindowSlot.FirstEmptyInPage)
            {
                slot = (eMerchantWindowSlot)GetNextFreeSlot(page);
            }

            var isSlotInvalid = slot < eMerchantWindowSlot.FirstInPage || slot > eMerchantWindowSlot.LastInPage;
            if (isSlotInvalid) return eMerchantWindowSlot.Invalid;

            return slot;
        }

        private int GetNextFreeSlot(int onPage)
        {
            var itemsInPage = GetItemsInPage(onPage);
            for (int i = 0; i < MAX_ITEM_IN_TRADEWINDOWS; i++)
            {
                if (!itemsInPage.Contains(i))
                    return i;
            }
            return MAX_ITEM_IN_TRADEWINDOWS;
        }

        #endregion
    }
}