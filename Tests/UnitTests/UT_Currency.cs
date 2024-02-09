using DOL.Database;
using DOL.GS.Finance;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_Currency
    {
        [Test]
        public void Equals_SameCurrencyAndSameValue_True()
        {
            var money1 = Currency.Copper.Mint(1);
            var money2 = Currency.Copper.Mint(1);

            var actual = money1.Equals(money2);

            Assert.That(actual, Is.True);
        }

        [Test]
        public void Equals_SameCurrencyAndDifferentValue_False()
        {
            var money1 = Currency.Copper.Mint(1);
            var money2 = Currency.Copper.Mint(2);

            var actual = money1.Equals(money2);

            Assert.That(actual, Is.False);
        }

        [Test]
        public void Equals_SameValueButDifferentCurrencyType_False()
        {
            var money1 = Currency.Copper.Mint(1);
            var money2 = Currency.BountyPoints.Mint(1);

            var actual = money1.Equals(money2);

            Assert.That(actual, Is.False);
        }

        [Test]
        public void Equals_ItemCurrenciesWithDifferentId_False()
        {
            var firstItemCurrencyId = "itemCurrency1";
            var secondItemCurrencyId = "itemCurrency2";
            var money1 = Currency.Item(firstItemCurrencyId).Mint(1);
            var money2 = Currency.Item(secondItemCurrencyId).Mint(1);

            var actual = money1.Equals(money2);

            Assert.That(actual, Is.False);
        }

        [Test]
        public void Equals_ItemCurrenciesWithSameId_True()
        {
            var firstItemCurrencyId = "itemCurrency1";
            var secondItemCurrencyId = "itemCurrency1";
            var money1 = Currency.Item(firstItemCurrencyId).Mint(1);
            var money2 = Currency.Item(secondItemCurrencyId).Mint(1);

            var actual = money1.Equals(money2);

            Assert.That(actual, Is.True);
        }
    }
}