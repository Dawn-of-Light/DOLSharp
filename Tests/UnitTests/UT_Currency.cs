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
using DOL.GS;
using DOL.Database;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_Currency
    {
        [Test]
        public void Equals_SameTypeAndSameValue_True()
        {
            var currency1 = Money.Copper.Create(1);
            var currency2 = Money.Copper.Create(1);

            var actual = currency1.Equals(currency2);

            Assert.IsTrue(actual);
        }

        [Test]
        public void Equals_SameTypeAndDifferentValue_False()
        {
            var currency1 = Money.Copper.Create(1);
            var currency2 = Money.Copper.Create(2);

            var actual = currency1.Equals(currency2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_SameValueButDifferentCurrencyType_False()
        {
            var currency1 = Money.Copper.Create(1);
            var currency2 = Money.BP.Create(1);

            var actual = currency1.Equals(currency2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_ItemCurrencyDifferent_False()
        {
            var currencyItem1 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var currencyItem2 = new ItemTemplate() { Id_nb = "itemCurrency2" };
            var currency1 = Money.Item(currencyItem1).Create(1);
            var currency2 = Money.Item(currencyItem2).Create(1);

            var actual = currency1.Equals(currency2);

            Assert.IsFalse(actual);
        }

        [Test]
        public void Equals_ItemCurrencyWithSameIdNb_True()
        {
            var currencyItem1 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var currencyItem2 = new ItemTemplate() { Id_nb = "itemCurrency1" };
            var currency1 = Money.Item(currencyItem1).Create(1);
            var currency2 = Money.Item(currencyItem2).Create(1);

            var actual = currency1.Equals(currency2);

            Assert.IsTrue(actual);
        }
    }
}