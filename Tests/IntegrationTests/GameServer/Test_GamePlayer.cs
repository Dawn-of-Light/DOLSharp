using NUnit.Framework;
using DOL.GS;
using DOL.UnitTests.Gameserver;
using DOL.Database;

namespace DOL.Integration.Gameserver
{
    [TestFixture]
    class Test_GamePlayer
    {
        [OneTimeSetUp]
        public void Init()
        {
            var sqliteDB = Create.TemporarySQLiteDB();
            sqliteDB.RegisterDataObject(typeof(ItemTemplate));
            sqliteDB.RegisterDataObject(typeof(InventoryItem));
            sqliteDB.RegisterDataObject(typeof(ItemUnique));

            var fakeServer = new FakeServer();
            fakeServer.SetDatabase(sqliteDB);
            GameServer.LoadTestDouble(fakeServer);
        }

        [Test]
        public void AddMoney_MinusOne_GetCurrentMoneyIsMinusOne()
        {
            var player = CreatePlayer();

            player.AddMoney(-1);

            var actual = player.GetCurrentMoney();
            var expected = -1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_MinusOne_GetCurrentMoneyIsOne()
        {
            var player = CreatePlayer();

            player.RemoveMoney(-1);

            var actual = player.GetCurrentMoney();
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItemCurrencyBalance_Aurulite_Init_Zero()
        {
            var player = CreatePlayer();
            var auruliteCurrency = Currency.Item(Aurulite);

            var actual = player.GetItemCurrencyBalance(auruliteCurrency);

            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetItemCurrencyBalance_Aurulite_AddOneAuruliteToInventory_One()
        {
            var player = CreatePlayer();
            var auruliteCurrency = Currency.Item(Aurulite);
            var inventoryItem = new GameInventoryItem(Aurulite);

            player.Inventory.AddTemplate(inventoryItem, 1, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
            var actual = player.GetItemCurrencyBalance(auruliteCurrency);

            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static GamePlayer CreatePlayer() => new MinimalGamePlayer(){InternalID = System.Guid.NewGuid().ToString()};

        private class MinimalGamePlayer : GamePlayer
        {
            public MinimalGamePlayer() : base(new GameClient(null), new DOL.Database.DOLCharacters())
            {
                Client.Out = new FakePacketLib();
            }

            public override void LoadFromDatabase(DataObject obj) { } //don't exercise database
        }
    }
}
