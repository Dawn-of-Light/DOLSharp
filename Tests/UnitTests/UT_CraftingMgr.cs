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
using NUnit.Framework;

using DOL.Database;
using DOL.GS;
using System.Collections.Generic;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_CraftingMgr
    {
        [OneTimeSetUp]
        public void SetupServer()
        {
            FakeServer.LoadAndReturn();
        }

        [Test]
        public void UpdateSellBackPrice_RecipeWithSomeSkillTypeIDAndItemWithPrice2AndTotalPriceZero_2()
        {
            var someSkillTypeID = -1;
            var item = new ItemTemplate() { Price = 2 };
            var totalPrice = 0;

            CraftingMgr.UpdateSellBackPrice(someSkillTypeID, item, totalPrice);

            var actual = item.Price;
            var expected = 2;
            Assert.AreEqual(expected, actual);
        }

        [Test]
        public void UpdateSellBackPrice_RecipeWithSomeSkillTypeIDAndItemWithPrice2AndTotalPriceIsHundred_SellBackPercentIs95_190()
        {
            var someSkillTypeID = -1;
            var item = new ItemTemplate() { Price = 2 };
            var totalPrice = 100;
            GS.ServerProperties.Properties.CRAFTING_SELLBACK_PERCENT = 95;

            CraftingMgr.UpdateSellBackPrice(someSkillTypeID, item, totalPrice);

            var actual = item.Price;
            var expected = 95 * 2;
            Assert.AreEqual(expected, actual);
        }
    }
}
