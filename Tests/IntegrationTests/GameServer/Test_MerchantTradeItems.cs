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
using System.IO;
using System.Collections.Generic;
using DOL.Database;
using DOL.Database.Connection;
using DOL.GS;
using DOL.UnitTests.Gameserver;
using NUnit.Framework;
using System.Linq;

namespace DOL.Integration.Gameserver
{
    [TestFixture]
    public class Test_MerchantTradeItems
    {
        private string databaseFileName = "dol-tests-only.sqlite3s.db";

        [SetUp]
        public void SetUp()
        {
            var fakeServer = new FakeServer();
            var connectionString = $"Data Source={databaseFileName}";
            var db = ObjectDatabase.GetObjectDatabase(ConnectionType.DATABASE_SQLITE, connectionString);
            fakeServer.SetDatabase(db);
            GameServer.LoadTestDouble(fakeServer);

            GameServer.Database.RegisterDataObject(typeof(MerchantItem));
            GameServer.Database.RegisterDataObject(typeof(ItemTemplate));
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(databaseFileName);
        }

        [Test]
        public void GetAllItems_AddedMerchantItemWithCorrespondingItemtemplate_CountIsOne()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList" });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var allItems = tradeItems.GetAllItems();

            var actual = allItems.Count;
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetAllItems_AddedMerchantItemButItemTemplateIsMissing_CountIsZero()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var allItems = tradeItems.GetAllItems();

            var actual = allItems.Count;
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetAllItems_AddTwoItems_CountIsOne()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0 });
            AddObject(new MerchantItem() { ItemTemplateID = "item2", ItemListID = "merchantList", SlotPosition = 1 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            AddObject(new ItemTemplate() { Id_nb = "item2" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var allItems = tradeItems.GetAllItems();

            var actual = allItems.Count;
            var expected = 2;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetAllItems_AddTwoItemsButAtSameSlot_CountIsOne()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0 });
            AddObject(new MerchantItem() { ItemTemplateID = "item2", ItemListID = "merchantList", SlotPosition = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            AddObject(new ItemTemplate() { Id_nb = "item2" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var allItems = tradeItems.GetAllItems();

            var actual = allItems.Count;
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetAllItems_AddTwoItemsAtSameSlotButDifferentPage_CountIsTwo()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 1 });
            AddObject(new MerchantItem() { ItemTemplateID = "item2", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 2 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            AddObject(new ItemTemplate() { Id_nb = "item2" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var allItems = tradeItems.GetAllItems();

            var actual = allItems.Count;
            var expected = 2;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItem_ItemTemplateDoesNotExist_Null()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = tradeItems.GetItem(0, (eMerchantWindowSlot)0);

            Assert.That(actual, Is.Null);
        }

        [Test]
        public void GetItem_AtPageAndSlotThatExists_NotNull()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = tradeItems.GetItem(0, (eMerchantWindowSlot)0);

            Assert.That(actual, Is.Not.Null);
        }

        [Test]
        public void GetItem_AtSlot31_Null()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 31, PageNumber = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = tradeItems.GetItem(0, (eMerchantWindowSlot)31);

            Assert.That(actual, Is.Null);
        }

        [Test]
        public void GetItemsInPage_EmptyList_CountIsZero()
        {
            var tradeItems = new MerchantTradeItems("merchantList");

            var pageResult = tradeItems.GetItemsInPage(0);

            var actual = pageResult.Count;
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItemsInPage_PageZeroHasExistingItem_CountIsOne()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var pageResult = tradeItems.GetItemsInPage(0);

            var actual = pageResult.Count;
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItemsInPage_PageZeroHasItemThatDoesNotExist_CountIsZero()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            var tradeItems = new MerchantTradeItems("merchantList");

            var pageResult = tradeItems.GetItemsInPage(0);

            var actual = pageResult.Count;
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_PageZeroSlotZero_Empty_Zero()
        {
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = (int)tradeItems.GetValidSlot(0, 0);

            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_PageZeroSlotOne_Empty_One()
        {
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = (int)tradeItems.GetValidSlot(0, (eMerchantWindowSlot)1);

            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_SlotAlreadyTaken_Zero()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = (int)tradeItems.GetValidSlot(0, 0);

            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_SlotThirtyOne_eMerchantWindowSlotInvalid()
        {
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = tradeItems.GetValidSlot(0, (eMerchantWindowSlot)31);

            var expected = eMerchantWindowSlot.Invalid;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_SlotIsFirstEmptyInPage_Zero()
        {
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = (int)tradeItems.GetValidSlot(0, eMerchantWindowSlot.FirstEmptyInPage);

            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_SlotIsFirstEmptyInPage_AllSlotsTaken_eMerchantWindowSlotInvalid()
        {
            foreach (var i in Enumerable.Range(0, 30))
            {
                AddObject(new MerchantItem() { ItemTemplateID = $"item{i + 1}", ItemListID = "merchantList", SlotPosition = i, PageNumber = 0 });
                AddObject(new ItemTemplate() { Id_nb = $"item{i + 1}" });
            }
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = tradeItems.GetValidSlot(0, eMerchantWindowSlot.FirstEmptyInPage);

            var expected = eMerchantWindowSlot.Invalid;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetValidSlot_SlotIsFirstEmptyInPage_FirstSlotTaken_One()
        {
            AddObject(new MerchantItem() { ItemTemplateID = "item1", ItemListID = "merchantList", SlotPosition = 0, PageNumber = 0 });
            AddObject(new ItemTemplate() { Id_nb = "item1" });
            var tradeItems = new MerchantTradeItems("merchantList");

            var actual = (int)tradeItems.GetValidSlot(0, eMerchantWindowSlot.FirstEmptyInPage);

            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Add_SlotIsFirstEmptyInPage_AllItemsCountIsOne()
        {
            var item = new ItemTemplate() { Id_nb = "item1" };
            var tradeItems = new MerchantTradeItems("merchantList");

            tradeItems.AddTradeItem(0,eMerchantWindowSlot.FirstEmptyInPage, item);

            var actual = tradeItems.GetAllItems().Count;
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        private void AddObject(DataObject dataObject)
        {
            GameServer.Database.AddObject(dataObject);
        }
    }
}