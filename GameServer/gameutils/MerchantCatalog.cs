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
using System.Linq;
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS
{
    public class MerchantCatalogEntry
    {
        public int SlotPosition { get; } = -1;
        public int Page { get; } = 0;
        public ItemTemplate Item { get; }

        public MerchantCatalogEntry(int slotPosition, int page, ItemTemplate itemTemplate)
        {
            SlotPosition = slotPosition;
            Page = page;
            Item = itemTemplate;
        }
    }

    public class MerchantCatalog
    {
        private const int FIRST_SLOT = 0;
        private const int MAX_SLOTS = 30;
        private const int FIRST_PAGE = 0;
        private const int MAX_PAGES = 5;
        private List<MerchantCatalogEntry> merchantCatalogEntries = new List<MerchantCatalogEntry>();

        public string ItemListId { get; private set; } = "NotLoadedFromDatabase";

        private MerchantCatalog() { }
        
        public static MerchantCatalog CreateEmpty() => new MerchantCatalog();

        public static MerchantCatalog LoadFromDatabase(string itemListId)
        {
            var catalog = new MerchantCatalog();
            var dbMerchantItems = DOLDB<MerchantItem>.SelectObjects(DB.Column(nameof(MerchantItem.ItemListID)).IsEqualTo(itemListId));
            foreach (var entry in dbMerchantItems)
            {
                var itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(entry.ItemTemplateID);
                if (itemTemplate == null) continue;
                var catalogEntry = new MerchantCatalogEntry(entry.SlotPosition, entry.PageNumber, itemTemplate);
                catalog.Add(catalogEntry);
            }
            catalog.ItemListId = itemListId;
            return catalog;
        }

        public IEnumerable<MerchantCatalogEntry> GetAllEntries()
            => merchantCatalogEntries.ToArray();
        
        public IEnumerable<MerchantCatalogEntry> GetAllEntriesOnPage(int page)
            => merchantCatalogEntries.Where(x => x.Page == page);

        public bool Add(MerchantCatalogEntry entry)
        {
            if (IsSlotValid(entry.Page, entry.SlotPosition))
            {
                merchantCatalogEntries.Add(entry);
                return true;
            }
            return false;
        }

        public bool AddItemToPage(ItemTemplate item, int page)
        {
            var nextSlot = GetNextFreeSlotOnPage(page);
            return Add(new MerchantCatalogEntry(nextSlot, page, item));
        }

        public bool Remove(int page, int slot)
        {
            var itemToBeRemoved = merchantCatalogEntries.Where(x => x.Page == page && x.SlotPosition == slot).FirstOrDefault();
            if (itemToBeRemoved != null)
            {
                merchantCatalogEntries.Remove(itemToBeRemoved);
            }
            return false;
        }

        public MerchantCatalogEntry GetEntry(int atPage, int atSlot)
        {
            var entry = merchantCatalogEntries
                .Where(x => x.Page == atPage && x.SlotPosition == atSlot)
                .FirstOrDefault();
            return entry != null ? entry : new MerchantCatalogEntry(-1, -1, null);
        }

        public MerchantTradeItems ConvertToMerchantTradeItems()
            => new MerchantTradeItems(this);

        private bool IsSlotValid(int page, int slot)
        {
            var isPageInvalid = page < FIRST_PAGE || page >= MAX_PAGES;
            var isSlotInvalid = slot < FIRST_SLOT || slot > MAX_SLOTS;

            if (isSlotInvalid || isSlotInvalid) return false;

            return true;
        }

        public int GetNextFreeSlotOnPage(int page)
        {
            var entriesOnPage = merchantCatalogEntries.Where(x => x.Page == page);
            foreach(var i in Enumerable.Range(FIRST_SLOT,MAX_SLOTS))
            {
                if(entriesOnPage.Where(x => x.SlotPosition == i).Any() == false)
                {
                    return i;
                }
            }
            var invalidSlot = -1;
            return invalidSlot;
        }
    }
}