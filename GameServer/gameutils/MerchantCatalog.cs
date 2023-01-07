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
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Finance;

namespace DOL.GS.Profession
{
    public class MerchantCatalogEntry
    {
        public byte SlotPosition { get; } = 0;
        public byte Page { get; } = 0;
        public ItemTemplate Item { get; }
        public long CurrencyAmount { get; } = 0;

        public MerchantCatalogEntry(byte slotPosition, byte page, ItemTemplate itemTemplate, long currencyAmount)
        {
            SlotPosition = slotPosition;
            Page = page;
            Item = itemTemplate;
            CurrencyAmount = currencyAmount;
        }
    }

    public class MerchantCatalogPage
    {
        private const byte FIRST_SLOT = 0;
        private const byte MAX_SLOTS = 30;

        private List<MerchantCatalogEntry> entries = new List<MerchantCatalogEntry>();

        public byte Number { get; }
        public Currency Currency { get; private set; } = Currency.Copper;

        public MerchantCatalogPage(byte number)
        {
            Number = number;
        }

        public bool AddItem(ItemTemplate item, byte slotPosition, long currencyAmount)
        {
            var isSlotInvalid = slotPosition < FIRST_SLOT || slotPosition > MAX_SLOTS;
            if (isSlotInvalid) return false;
            entries.Add(new MerchantCatalogEntry(slotPosition, Number, item, currencyAmount));
            return true;
        }

        public bool AddItemWithDefaultPrice(ItemTemplate item, byte slotPosition)
        {
            return AddItem(item, slotPosition, item.Price);
        }

        public bool AddItemToNextFreeSlot(ItemTemplate item, long currencyAmount)
        {
            var nextSlot = GetNextFreeSlot();
            return AddItem(item, nextSlot, currencyAmount);
        }

        public bool AddItemToNextFreeSlotWithDefaultPrice(ItemTemplate item)
        {
            return AddItemToNextFreeSlot(item, item.Price);
        }

        public bool Remove(byte slotPosition)
        {
            var entry = entries.Where(x => x.SlotPosition == slotPosition);
            if (!entry.Any()) return false;
            return entries.Remove(entry.First());
        }

        public void SetCurrency(Currency currency)
        {
            Currency = currency;
        }

        public IEnumerable<MerchantCatalogEntry> GetAllEntries()
            => entries.ToArray();

        public MerchantCatalogEntry GetEntry(byte slotPosition)
        {
            var entry = entries.Where(x => x.SlotPosition == slotPosition).FirstOrDefault();
            return entry != null ? entry : new MerchantCatalogEntry(255, 255, null, -1);
        }

        public byte GetNextFreeSlot()
        {
            foreach (byte i in Enumerable.Range(FIRST_SLOT, MAX_SLOTS))
            {
                if (entries.Where(x => x.SlotPosition == i).Any() == false)
                {
                    return i;
                }
            }
            byte invalidSlot = 255;
            return invalidSlot;
        }

        public int EntryCount => entries.Count;
    }

    public class MerchantCatalog
    {
        private const int FIRST_PAGE = 0;
        private const int MAX_PAGES = 5;
        private SortedList<byte, MerchantCatalogPage> merchantPages = new SortedList<byte, MerchantCatalogPage>();

        public string ItemListId { get; private set; }

        private MerchantCatalog() { }

        public static MerchantCatalog Create() => new MerchantCatalog();

        public static MerchantCatalog Create(string itemListId) => new MerchantCatalog() { ItemListId = itemListId };

        public static MerchantCatalog LoadFromDatabase(string itemListId)
        {
            var currencySlotPosition = -1;
            var catalog = new MerchantCatalog();
            var dbMerchantItems = DOLDB<MerchantItem>.SelectObjects(DB.Column(nameof(MerchantItem.ItemListID)).IsEqualTo(itemListId));
            foreach (var dbMerchantItem in dbMerchantItems)
            {
                var page = catalog.GetPage(dbMerchantItem.PageNumber);
                if (dbMerchantItem.SlotPosition == currencySlotPosition)
                {
                    page.SetCurrency(GetCurrencyFrom(dbMerchantItem));
                }
                var itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(dbMerchantItem.ItemTemplateID);
                if (itemTemplate == null) continue;
                var currencyAmount = dbMerchantItem.Price != 0 ? dbMerchantItem.Price : itemTemplate.Price;
                catalog.GetPage(dbMerchantItem.PageNumber).AddItem(itemTemplate, (byte)dbMerchantItem.SlotPosition, currencyAmount);
            }
            catalog.ItemListId = itemListId;
            return catalog;
        }

        private static Currency GetCurrencyFrom(MerchantItem merchantItem)
        {
            var currencyId = (byte)merchantItem.Price;
            var itemCurrencyId = merchantItem.ItemTemplateID;
            switch (currencyId)
            {
                case 1: return Currency.Copper;
                case 2: 
                    var itemTemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(itemCurrencyId);
                    if (itemTemplate == null) return Currency.Item(itemCurrencyId, $"units of {itemCurrencyId}");
                    else return Currency.Item(itemCurrencyId, itemTemplate.Name);
                case 3: return Currency.BountyPoints;
                case 4: return Currency.Mithril;
                default: throw new System.NotImplementedException($"Currency with id {currencyId} is not implemented.");
            }
        }

        public IEnumerable<MerchantCatalogEntry> GetAllEntries()
            => merchantPages.Select(x => x.Value.GetAllEntries()).SelectMany(x => x);

        public MerchantCatalogPage GetPage(int pageNumber)
        {
            var pageIsInvalid = pageNumber < FIRST_PAGE || pageNumber >= MAX_PAGES;
            if (pageIsInvalid) throw new ArgumentException($"PageNumber {pageNumber} is invalid. PageNumber must be in the range of 0-4.");
            if (merchantPages.TryGetValue((byte)pageNumber, out var page) == false)
            {
                merchantPages.Add((byte)pageNumber, new MerchantCatalogPage((byte)pageNumber));
            }
            return merchantPages[(byte)pageNumber];
        }

        public IEnumerable<MerchantCatalogPage> GetAllPages()
            => merchantPages.Values;

        public MerchantCatalogEntry GetEntry(int atPage, int atSlot)
            => GetPage(atPage).GetEntry((byte)atSlot);

        public bool IsEmpty
            => GetAllEntries().Any() == false;

        public MerchantTradeItems ConvertToMerchantTradeItems()
            => new MerchantTradeItems(this);
    }
}