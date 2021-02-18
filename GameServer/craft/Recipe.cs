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
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
    public class Recipe
    {
        private static RecipeCache recipeCache = new RecipeCache();
        private Ingredient[] ingredients;

        public ItemTemplate Product { get; }
        public eCraftingSkill RequiredCraftingSkill { get; }
        public int Level { get; }
        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);

        #region transition members
        public string DatabaseID { get; private set; } = "-1";
        public bool MakeTemplated { get; private set; } = false;
        #endregion

        public Recipe(ItemTemplate product, List<Ingredient> ingredients)
        {
            this.ingredients = ingredients.ToArray();
            Product = product;
        }

        public Recipe(ItemTemplate product, List<Ingredient> ingredients, eCraftingSkill requiredSkill, int level) 
            : this(product, ingredients)
        {
            RequiredCraftingSkill = requiredSkill;
            Level = level;
        }

        public static Recipe LoadFromDB(DBCraftedItem dbRecipe)
        {
            if(recipeCache.Loaded)
            {
                recipeCache.GetRecipe(dbRecipe.CraftedItemID);
            }

            ItemTemplate product = GameServer.Database.FindObjectByKey<ItemTemplate>(dbRecipe.Id_nb);
            if (product == null) throw new ArgumentException("Product ItemTemplate " + dbRecipe.Id_nb + " for Recipe with ID " + dbRecipe.CraftedItemID +  " does not exist.");

            IList<DBCraftedXItem> rawMaterials = GameServer.Database.SelectObjects<DBCraftedXItem>("`CraftedItemId_nb` = @CraftedItemId_nb", new QueryParameter("@CraftedItemId_nb", dbRecipe.Id_nb));
            if (rawMaterials.Count == 0) throw new ArgumentException("Recipe with ID " + dbRecipe.CraftedItemID + " has no ingredients.");

            bool isRecipeValid = true;
            var errorText = "";
            var ingredients = new List<Ingredient>();
            foreach (DBCraftedXItem material in rawMaterials)
            {
                ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);

                if (template == null)
                {
                    errorText += "Cannot find raw material ItemTemplate: " + material.IngredientId_nb + ") needed for recipe: " + dbRecipe.CraftedItemID + "\n";
                    isRecipeValid = false;
                }
                ingredients.Add(new Ingredient(material.Count, template));
            }
            if (!isRecipeValid) throw new ArgumentException(errorText);

            var recipe = new Recipe(product, ingredients, (eCraftingSkill)dbRecipe.CraftingSkillType, dbRecipe.CraftingLevel);
            recipe.DatabaseID = dbRecipe.CraftedItemID;
            recipe.MakeTemplated = dbRecipe.MakeTemplated;
            return recipe;
        }

        public static Recipe LoadFromDB(ushort recipeID)
        {
            if (recipeCache.Loaded)
            {
                recipeCache.GetRecipe(recipeID.ToString());
            }

            DBCraftedItem recipe = GameServer.Database.FindObjectByKey<DBCraftedItem>(recipeID.ToString());
            if (recipe == null) throw new ArgumentException("No CraftedItem with ID " + recipeID + "exists.");
            return LoadFromDB(recipe);
        }

        public static void LoadCache()
        {
            recipeCache.Load();
        }

        public long GetIngredientCost()
        {
            long result = 0;
            foreach (var ingredient in ingredients)
            {
                result += ingredient.Cost;
            }
            return result;
        }

        public DBCraftedItem ExportDBCraftedItem()
        {
            var dbRecipe = new DBCraftedItem();
            dbRecipe.CraftedItemID = DatabaseID;
            dbRecipe.Id_nb = Product.Id_nb;
            dbRecipe.CraftingLevel = Level;
            dbRecipe.CraftingSkillType = (int)RequiredCraftingSkill;
            dbRecipe.MakeTemplated = MakeTemplated;
            return dbRecipe;
        }

        public IList<DBCraftedXItem> ExportListOfDBCraftedXItem()
        {
            var dbCraftedXItemList = new List<DBCraftedXItem>();
            foreach (var ingredient in ingredients)
            {
                var craftedXitem = new DBCraftedXItem();
                craftedXitem.Count = ingredient.Count;
                craftedXitem.CraftedItemId_nb = Product.Id_nb;
                craftedXitem.IngredientId_nb = ingredient.Material.Id_nb;
                dbCraftedXItemList.Add(craftedXitem);
            }
            return dbCraftedXItemList;
        }
    }

    public class Ingredient
    {
        public int Count { get; }
        public ItemTemplate Material { get; }

        public Ingredient(int count, ItemTemplate ingredient)
        {
            Count = count;
            Material = ingredient;
        }

        public long Cost => Count * Material.Price;
    }

    public class RecipeCache
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<string, Recipe> recipeCache = null;

        public bool Loaded { get; private set; } = false;

        public Recipe GetRecipe(string recipeDatabaseID)
        {
            if (!Loaded) throw new KeyNotFoundException("RecipeCache is not loaded.");
            return recipeCache[recipeDatabaseID];
        }

        public void Load()
        {
            recipeCache = new Dictionary<string, Recipe>();
            var allDBRecipes = GameServer.Database.SelectAllObjects<DBCraftedItem>();
            foreach(var dbRecipe in allDBRecipes)
            {
                try
                {
                    recipeCache[dbRecipe.CraftedItemID] = Recipe.LoadFromDB(dbRecipe);
                }
                catch(Exception e)
                {
                    log.Error(e.Message);
                }
            }
            Loaded = true;
        }
    }
}
