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
using System.Collections.Generic;

using DOL.Database;
using DOL.GS;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    class UT_Recipe
    {
        [OneTimeSetUp]
        public void SetupServer()
        {
            FakeServer.Load();
        }

        [Test]
        public void GetIngredientCosts_OneIngredientWithPrice2_2()
        {
            var item = new ItemTemplate();
            item.Price = 2;
            var ingredient = new Ingredient(1, item);
            var recipe = new Recipe(null, new List<Ingredient>() { ingredient});

            var actual = recipe.CostToCraft;

            var expected = 2;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void GetIngredientCosts_OneIngredientWithPrice2AndOneWithPrice4_6()
        {
            var item = new ItemTemplate() { Price = 2 };
            var ingredient1 = new Ingredient(1, item);
            var ingredient2 = new Ingredient(2, item);
            var recipe = new Recipe(null, new List<Ingredient>() { ingredient1, ingredient2 });

            var actual = recipe.CostToCraft;

            var expected = 6;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SetRecommendedProductPriceInDB_ProductWithPrice2AndNoIngredients_ProductPriceIs2()
        {
            var product = new ItemTemplate() { Price = 2 };
            var ingredients = new List<Ingredient>() { };
            var recipe = new Recipe(product, ingredients);

            recipe.SetRecommendedProductPriceInDB();

            var actual = product.Price;
            var expected = 2;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void SetRecommendedProductPriceInDB_ProductWithPrice2AndIngredientCostIs100_ProductPriceIs190()
        {
            var product = new ItemTemplate() { Price = 2 };
            var count = 1;
            var material = new ItemTemplate() { Price = 100 };
            var ingredients = new List<Ingredient>() { new Ingredient(count, material) };
            var recipe = new Recipe(product, ingredients);
            GS.ServerProperties.Properties.CRAFTING_SELLBACK_PERCENT = 95;

            recipe.SetRecommendedProductPriceInDB();

            var actual = product.Price;
            var expected = 95 * 2;
            Assert.That(actual, Is.EqualTo(expected));
        }
    }

    [TestFixture]
    public class UT_Ingredient
    {
        [Test]
        public void Cost_CountIsOneItemPriceIsOne_One()
        {
            var item = new ItemTemplate() { Price = 1 };
            var ingredient = new Ingredient(1, item);

            var actual = ingredient.Cost;

            var expected = 1;
            Assert.That(actual, Is.EqualTo(expected));
        }

        [Test]
        public void Cost_2ItemsWithPriceOne_2()
        {
            var item = new ItemTemplate() { Price = 1 };
            var ingredient = new Ingredient(2, item);

            var actual = ingredient.Cost;

            var expected = 2;
            Assert.That(actual, Is.EqualTo(expected));
        }
    }
}
