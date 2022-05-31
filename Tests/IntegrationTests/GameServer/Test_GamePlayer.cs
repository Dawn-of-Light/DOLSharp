using NUnit.Framework;
using DOL.GS;
using DOL.UnitTests.Gameserver;
using DOL.Database;
using DOL.GS.Finance;

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
        public void AddMoney_MinusOne_CopperBalanceIsMinusOne()
        {
            var player = CreatePlayer();

            player.AddMoney(Currency.Copper.Mint(-1));

            var actual = player.CopperBalance;
            var expected = -1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_MinusOne_CopperBalanceMoneyIsOne()
        {
            var player = CreatePlayer();

            player.RemoveMoney(Currency.Copper.Mint(-1));

            var actual = player.CopperBalance;
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
