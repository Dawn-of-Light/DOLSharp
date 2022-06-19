using NUnit.Framework;
using DOL.GS;
using DOL.UnitTests.Gameserver;
using DOL.Database;
using DOL.GS.Finance;

namespace DOL.Integration.Gameserver
{
    [TestFixture]
    class Test_Wallet
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
        public void GetBalance_Aurulite_Init_Zero()
        {
            var wallet = CreateWallet();

            var actual = wallet.GetBalance(Aurulite);

            var expected = Aurulite.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetBalance_Aurulite_WalletOwnerHasOneAuruliteInInventory_One()
        {
            var owner = CreatePlayer();
            var wallet = CreateWallet(owner);
            var currencyAmount = 1;
            owner.Inventory.AddTemplate(new GameInventoryItem(AuruliteItemTemplate), currencyAmount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            var actual = wallet.GetBalance(Aurulite);

            var expected = Aurulite.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetBalance_Aurulite_WalletOwnerHasOnlyAnotherItemInInventory_Zero()
        {
            var owner = CreatePlayer();
            var wallet = CreateWallet(owner);
            var currencyAmount = 1;
            var anotherItem = new ItemTemplate() { Id_nb = "another_item" };
            owner.Inventory.AddTemplate(new GameInventoryItem(anotherItem), currencyAmount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            var actual = wallet.GetBalance(Aurulite);

            var expected = Aurulite.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneAurulite_WalletOwnerHasOneAuruliteInInventory_GetBalanceForAuruliteIsZero()
        {
            var owner = CreatePlayer();
            var wallet = CreateWallet(owner);
            var currencyAmount = 1;
            owner.Inventory.AddTemplate(new GameInventoryItem(AuruliteItemTemplate), currencyAmount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            wallet.RemoveMoney(Aurulite.Mint(1));

            var actual = wallet.GetBalance(Aurulite);
            var expected = Aurulite.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_TwoAurulite_WalletOwnerHasOneAuruliteInInventory_GetBalanceForAuruliteIsOne()
        {
            var owner = CreatePlayer();
            var wallet = CreateWallet(owner);
            var currencyAmount = 1;
            owner.Inventory.AddTemplate(new GameInventoryItem(AuruliteItemTemplate), currencyAmount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            wallet.RemoveMoney(Aurulite.Mint(2));

            var actual = wallet.GetBalance(Aurulite);
            var expected = Aurulite.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneAurulite_Init_False()
        {
            var wallet = CreateWallet();

            var actual = wallet.RemoveMoney(Aurulite.Mint(1));

            var expected = false;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneAurulite_WalletOwnerHasOneAuruliteInInventory_True()
        {
            var owner = CreatePlayer();
            var wallet = CreateWallet(owner);
            var currencyAmount = 1;
            owner.Inventory.AddTemplate(new GameInventoryItem(AuruliteItemTemplate), currencyAmount, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            var actual = wallet.RemoveMoney(Aurulite.Mint(1));

            var expected = true;
            Assert.That(actual, Is.EqualTo(expected));
        }

        private static Currency Aurulite => Currency.Item("aurulite");
        private static ItemTemplate AuruliteItemTemplate
            = new ItemTemplate() { Name = "Aurulite Splitter", Id_nb = "aurulite_splitter", ClassType = "Currency.aurulite", MaxCount = 2000 };
        private static Wallet CreateWallet()
            => new Wallet(new MinimalGamePlayer());
        private static Wallet CreateWallet(GamePlayer owner)
            => new Wallet(owner);
        private static GamePlayer CreatePlayer() => new MinimalGamePlayer();

        private class MinimalGamePlayer : GamePlayer
        {
            public MinimalGamePlayer() : base(new GameClient(null), new DOL.Database.DOLCharacters())
            {
                Client.Out = new FakePacketLib();
                InternalID = System.Guid.NewGuid().ToString();
            }

            public override void LoadFromDatabase(DataObject obj) { } //don't exercise database
        }
    }
}
