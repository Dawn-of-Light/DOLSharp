using DOL.Database;
using DOL.GS;
using DOL.GS.Finance;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_Wallet
    {
        [Test]
        public void Balance_Copper_Init_Zero()
        {
            var wallet = new Wallet();

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneCopperToEmptyWallet_CopperBalanceIsOne()
        {
            var wallet = new Wallet();
            var oneCopper = Currency.Copper.Mint(1);

            wallet.AddMoney(oneCopper);

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneBountyPointToEmptyWallet_CopperBalanceRemainsZero()
        {
            var wallet = new Wallet();
            var oneBp = Currency.BountyPoints.Mint(1);

            wallet.AddMoney(oneBp);

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneBountyPointToEmptyWallet_BountyPointBalanceIsOne()
        {
            var wallet = new Wallet();
            var oneBp = Currency.BountyPoints.Mint(1);

            wallet.AddMoney(oneBp);

            var actual = wallet.GetBalance(Currency.BountyPoints);
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneCopper_AddOneCopperTwiceBefore_CopperBalanceIsOne()
        {
            var wallet = new Wallet();
            var oneCopper = Currency.Copper.Mint(1);

            wallet.AddMoney(oneCopper);
            wallet.AddMoney(oneCopper);
            wallet.RemoveMoney(oneCopper);

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetBalance_Aurulite_AuruliteBalanceIsZero()
        {
            var player = new FakePlayer();
            var wallet = new Wallet(player);
            var aurulite = Currency.Item(new ItemTemplate(){Id_nb="aurulite"});

            var actual = wallet.GetBalance(aurulite);
            var expected = 0;
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}