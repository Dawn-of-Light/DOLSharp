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
using System.Diagnostics;

using NUnit.Framework;

using DOL.Database;
using DOL.GS;
using DOL.UnitTests.Gameserver;

namespace DOL.Integration.Performance
{
    [TestFixture, Explicit]
    class Test_GamePlayerCraftItem
    {
        private static ushort itemToCraftID = 1;

        [OneTimeSetUp]
        public void SetupFakeServer()
        {
            var sqliteDB = Create.TemporarySQLiteDB();
            sqliteDB.RegisterDataObject(typeof(DBCraftedItem));
            sqliteDB.RegisterDataObject(typeof(DBCraftedXItem));
            sqliteDB.RegisterDataObject(typeof(ItemTemplate));

            var fakeServer = new FakeServer();
            fakeServer.SetDatabase(sqliteDB);
            GameServer.LoadTestDouble(fakeServer);

            AddOneCompleteRecipeToDatabase();
        }

        [Test]
        public void CraftItem_RecipeIsComplete_AThousandTimes_()
        {
            var player = new FakePlayer();
            var repetitions = 1000;

            Stopwatch sw = Stopwatch.StartNew();
            for (int index = 0; index < repetitions; index++)
            {
                player.CraftItem(itemToCraftID);
            }
            var duration = sw.ElapsedMilliseconds;
            sw.Stop();
            Assert.Warn($"{repetitions} executions of GamePlayer.CraftItem took {duration} ms.");
        }

        private static DBCraftedItem AddOneCompleteRecipeToDatabase()
        {
            var itemToCraft = new ItemTemplate();
            itemToCraft.Id_nb = "item_to_craft";
            itemToCraft.Name = "Item To Craft";
            itemToCraft.AllowedClasses = "";
            itemToCraft.CanUseEvery = 0;
            AddDatabaseEntry(itemToCraft);

            var craftedItem = new DBCraftedItem();
            craftedItem.CraftedItemID = itemToCraftID.ToString();
            craftedItem.Id_nb = itemToCraft.Id_nb;
            craftedItem.CraftingLevel = 1;
            craftedItem.CraftingSkillType = 1;
            AddDatabaseEntry(craftedItem);

            var ingredient1 = new DBCraftedXItem();
            ingredient1.Count = 1;
            ingredient1.ObjectId = "id1";
            ingredient1.CraftedItemId_nb = craftedItem.Id_nb;
            ingredient1.IngredientId_nb = "item1_id";
            AddDatabaseEntry(ingredient1);
            var ingredient2 = new DBCraftedXItem();
            ingredient2.Count = 2;
            ingredient2.ObjectId = "id2";
            ingredient2.CraftedItemId_nb = craftedItem.Id_nb;
            ingredient2.IngredientId_nb = "item2_id";
            AddDatabaseEntry(ingredient2);

            var ingredientItem1 = new ItemTemplate();
            ingredientItem1.Id_nb = ingredient1.IngredientId_nb;
            ingredientItem1.Name = "First Ingredient Name";
            ingredientItem1.AllowedClasses = "";
            ingredientItem1.Price = 10000;
            ingredientItem1.CanUseEvery = 0;
            AddDatabaseEntry(ingredientItem1);
            var ingredientItem2 = new ItemTemplate();
            ingredientItem2.Id_nb = ingredient2.IngredientId_nb;
            ingredientItem2.Name = "Second Ingredient Name";
            ingredientItem2.AllowedClasses = "";
            ingredientItem2.CanUseEvery = 0;
            ingredientItem2.Price = 20000;
            AddDatabaseEntry(ingredientItem2);

            return craftedItem;
        }

        private static void AddDatabaseEntry(DataObject dataObject)
        {
            dataObject.AllowAdd = true;
            GameServer.Database.AddObject(dataObject);
        }
    }
}
