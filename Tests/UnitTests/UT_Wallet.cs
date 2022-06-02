using DOL.GS.Finance;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_Wallet
    {
        [Test]
        public void GetBalance_OfCopper_Init_ZeroCopper()
        {
            var wallet = CreateWallet();

            var actual = wallet.GetBalance(Currency.Copper);

            var expected = Currency.Copper.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneCopperToEmptyWallet_CopperBalanceIsOne()
        {
            var wallet = CreateWallet();
            var oneCopper = Currency.Copper.Mint(1);

            wallet.AddMoney(oneCopper);

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = Currency.Copper.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneBountyPointToEmptyWallet_CopperBalanceRemainsZero()
        {
            var wallet = CreateWallet();
            var oneBp = Currency.BountyPoints.Mint(1);

            wallet.AddMoney(oneBp);

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = Currency.Copper.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void AddMoney_OneBountyPointToEmptyWallet_BountyPointBalanceIsOne()
        {
            var wallet = CreateWallet();
            var oneBp = Currency.BountyPoints.Mint(1);

            wallet.AddMoney(oneBp);

            var actual = wallet.GetBalance(Currency.BountyPoints);
            var expected = Currency.BountyPoints.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneCopper_Add2CopperPrior_CopperBalanceIsOne()
        {
            var wallet = CreateWallet();
            wallet.AddMoney(Currency.Copper.Mint(2));

            wallet.RemoveMoney(Currency.Copper.Mint(1));

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = Currency.Copper.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneCopper_Init_CopperBalanceIsZero()
        {
            var wallet = CreateWallet();

            wallet.RemoveMoney(Currency.Copper.Mint(1));

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = Currency.Copper.Mint(0);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneCopper_Init_False()
        {
            var wallet = CreateWallet();

            var actual = wallet.RemoveMoney(Currency.Copper.Mint(1));

            var expected = false;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_TwoCopper_AddOneCopperPrior_CopperBalanceIsOne()
        {
            var wallet = CreateWallet();
            wallet.AddMoney(Currency.Copper.Mint(1));

            wallet.RemoveMoney(Currency.Copper.Mint(2));

            var actual = wallet.GetBalance(Currency.Copper);
            var expected = Currency.Copper.Mint(1);
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void RemoveMoney_OneCopper_AddOneCopperPrior_True()
        {
            var wallet = CreateWallet();
            wallet.AddMoney(Currency.Copper.Mint(1));

            var actual = wallet.RemoveMoney(Currency.Copper.Mint(1));

            var expected = true;
            Assert.That(actual, Is.EqualTo(expected));
        }

        private Wallet CreateWallet() => new Wallet();
    }
}