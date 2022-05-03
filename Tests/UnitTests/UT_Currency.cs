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

            Assert.IsTrue(actual);
        }

        [Test]
        public void Equals_SameCurrencyAndDifferentValue_False()
        {
            var money1 = Currency.Copper.Mint(1);
            var money2 = Currency.Copper.Mint(2);

            var actual = money1.Equals(money2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_SameValueButDifferentCurrencyType_False()
        {
            var money1 = Currency.Copper.Mint(1);
            var money2 = Currency.BountyPoints.Mint(1);

            var actual = money1.Equals(money2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_CurrencyItemIsDifferent_False()
        {
            var itemMoney1 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var itemMoney2 = new ItemTemplate() { Id_nb = "itemCurrency2" };
            var money1 = Currency.Item(itemMoney1).Mint(1);
            var money2 = Currency.Item(itemMoney2).Mint(1);

            var actual = money1.Equals(money2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_ItemCurrencyWithSameIdNb_True()
        {
            var currencyItem1 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var currencyItem2 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var money1 = Currency.Item(currencyItem1).Mint(1);
            var money2 = Currency.Item(currencyItem2).Mint(1);

            var actual = money1.Equals(money2);

            Assert.IsTrue(actual);
        }
    }
}