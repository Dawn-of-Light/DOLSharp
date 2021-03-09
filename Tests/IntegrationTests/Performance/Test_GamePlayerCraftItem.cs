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
using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using DOL.Database;
using DOL.Database.Connection;
using DOL.GS;
using DOL.UnitTests.Gameserver;
using NUnit.Framework;

namespace DOL.Integration.Performance
{
    [TestFixture, Explicit]
    class Test_GamePlayerCraftItem
    {
        private static ushort itemToCraftID = 1;

        [OneTimeSetUp]
        public void SetupFakeServer()
        {
            var fakeServer = new FakeServerWithSQLLiteDB();
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
            Assert.Fail("{0} executions of GamePlayer.CraftItem took {1} ms.", repetitions, duration);
        }

        private static DBCraftedItem AddOneCompleteRecipeToDatabase()
        {
            var itemToCraft = new ItemTemplate();
            itemToCraft.Id_nb = "item_to_craft";
            itemToCraft.Name = "Item To Craft";
            itemToCraft.AllowedClasses = "";
            itemToCraft.CanUseEvery = 0;
            AddOrUpdateDatabaseEntry(itemToCraft);

            var craftedItem = new DBCraftedItem();
            craftedItem.CraftedItemID = itemToCraftID.ToString();
            craftedItem.Id_nb = itemToCraft.Id_nb;
            craftedItem.CraftingLevel = 1;
            craftedItem.CraftingSkillType = 1;
            AddOrUpdateDatabaseEntry(craftedItem);

            var ingredient1 = new DBCraftedXItem();
            ingredient1.Count = 1;
            ingredient1.ObjectId = "id1";
            ingredient1.CraftedItemId_nb = craftedItem.Id_nb;
            ingredient1.IngredientId_nb = "item1_id";
            AddOrUpdateDatabaseEntry(ingredient1);
            var ingredient2 = new DBCraftedXItem();
            ingredient2.Count = 2;
            ingredient2.ObjectId = "id2";
            ingredient2.CraftedItemId_nb = craftedItem.Id_nb;
            ingredient2.IngredientId_nb = "item2_id";
            AddOrUpdateDatabaseEntry(ingredient2);

            var ingredientItem1 = new ItemTemplate();
            ingredientItem1.Id_nb = ingredient1.IngredientId_nb;
            ingredientItem1.Name = "First Ingredient Name";
            ingredientItem1.AllowedClasses = "";
            ingredientItem1.Price = 10000;
            ingredientItem1.CanUseEvery = 0;
            AddOrUpdateDatabaseEntry(ingredientItem1);
            var ingredientItem2 = new ItemTemplate();
            ingredientItem2.Id_nb = ingredient2.IngredientId_nb;
            ingredientItem2.Name = "Second Ingredient Name";
            ingredientItem2.AllowedClasses = "";
            ingredientItem2.CanUseEvery = 0;
            ingredientItem2.Price = 20000;
            AddOrUpdateDatabaseEntry(ingredientItem2);

            return craftedItem;
        }

        private static bool AddOrUpdateDatabaseEntry(DataObject dataObject)
        {
            dataObject.AllowAdd = true;
            bool successfullyAddedOrUpdated = GameServer.Database.AddObject(dataObject);
            if (!successfullyAddedOrUpdated)
            {
                successfullyAddedOrUpdated = GameServer.Database.SaveObject(dataObject);
            }
            return successfullyAddedOrUpdated;
        }

        private class FakeServerWithSQLLiteDB : FakeServer
        {
            private IObjectDatabase realDB;

            protected override IObjectDatabase DataBaseImpl => realDB;

            public FakeServerWithSQLLiteDB()
            {
                SetupDatabase();
            }

            private void SetupDatabase()
            {
                var configDBType = ConnectionType.DATABASE_SQLITE;
                DirectoryInfo CodeBase = new FileInfo(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath).Directory;
                DirectoryInfo FakeRoot = CodeBase.Parent;
                var configRootDirectory = FakeRoot.FullName;
                var configDBConnectionString = string.Format("Data Source={0};Version=3;Pooling=False;Cache Size=1073741824;Journal Mode=Off;Synchronous=Off;Foreign Keys=True;Default Timeout=60",
                                                 Path.Combine(configRootDirectory, "performance.sqlite3.db"));
                realDB = ObjectDatabase.GetObjectDatabase(configDBType, configDBConnectionString);
                realDB.RegisterDataObject(typeof(DBCraftedItem));
                realDB.RegisterDataObject(typeof(DBCraftedXItem));
                realDB.RegisterDataObject(typeof(ItemTemplate));
            }
        }
    }
}
