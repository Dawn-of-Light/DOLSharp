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

using log4net;

using DOL.Database;
using DOL.GS.ServerProperties;

namespace DOL.GS
{
    public class Recipe
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Ingredient[] ingredients;

        public ItemTemplate Product { get; }
        public eCraftingSkill RequiredCraftingSkill { get; }
        public int Level { get; }
        public List<Ingredient> Ingredients => new List<Ingredient>(ingredients);

        #region transition members
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

        public Recipe(ItemTemplate product, List<Ingredient> ingredients, eCraftingSkill requiredSkill, int level, bool makeTemplated)
            :this(product,ingredients,requiredSkill,level)
        {
            MakeTemplated = makeTemplated;
        }

        public long CostToCraft
        {
            get
            {
                long result = 0;
                foreach (var ingredient in ingredients)
                {
                    result += ingredient.Cost;
                }
                return result;
            }
        }

        public void SetRecommendedProductPriceInDB()
        {
            var product = Product;
            var totalPrice = CostToCraft;
            bool updatePrice = true;

            if (product.Name.EndsWith("metal bars") ||
                product.Name.EndsWith("leather square") ||
                product.Name.EndsWith("cloth square") ||
                product.Name.EndsWith("wooden boards"))
                updatePrice = false;

            if (product.PackageID.Contains("NoPriceUpdate"))
                updatePrice = false;

            if (updatePrice)
            {
                long pricetoset;
                var secondaryCraftingSkills = new List<eCraftingSkill>() { 
                    eCraftingSkill.MetalWorking, eCraftingSkill.LeatherCrafting, eCraftingSkill.ClothWorking, eCraftingSkill.WoodWorking
                };

                if (secondaryCraftingSkills.Contains(RequiredCraftingSkill))
                    pricetoset = Math.Abs((long)(totalPrice * 2 * Properties.CRAFTING_SECONDARYCRAFT_SELLBACK_PERCENT) / 100);
                else
                    pricetoset = Math.Abs(totalPrice * 2 * Properties.CRAFTING_SELLBACK_PERCENT / 100);

                if (pricetoset > 0 && product.Price != pricetoset)
                {
                    long currentPrice = product.Price;
                    product.Price = pricetoset;
                    product.AllowUpdate = true;
                    product.Dirty = true;
                    product.Id_nb = product.Id_nb.ToLower();
                    if (GameServer.Database.SaveObject(product))
                        log.Error("Craft Price Correction: " + product.Id_nb + " rawmaterials price= " + totalPrice + " Actual Price= " + currentPrice + ". Corrected price to= " + pricetoset);
                    else
                        log.Error("Craft Price Correction Not SAVED: " + product.Id_nb + " rawmaterials price= " + totalPrice + " Actual Price= " + currentPrice + ". Corrected price to= " + pricetoset);
                    GameServer.Database.UpdateInCache<ItemTemplate>(product.Id_nb);
                    product.Dirty = false;
                    product.AllowUpdate = false;
                }
            }
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

    public class RecipeDB
    {
        protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static Dictionary<ushort, Recipe> recipeCache = new Dictionary<ushort, Recipe>();

        public static Recipe FindBy(ushort recipeDatabaseID)
        {
            Recipe recipe;
            recipeCache.TryGetValue(recipeDatabaseID, out recipe);
            if (recipe != null)
            {
                //avoid repeated DB access for invalid recipes
                if (recipe.Product != null) return recipeCache[recipeDatabaseID];
                else throw new KeyNotFoundException("Recipe is marked as invalid. Check your logs for Recipe with ID " + recipeDatabaseID + ".");
            }

            try
            {
                recipe = LoadFromDB(recipeDatabaseID);
                return recipe;
            }
            catch (Exception e)
            {
                log.Error(e);
                recipe = NullRecipe;
                return recipe;
            }
            finally
            {
                if (Properties.CRAFTING_ADJUST_PRODUCT_PRICE)
                    recipe.SetRecommendedProductPriceInDB();
                recipeCache[recipeDatabaseID] = recipe;
            }

        }

        private static Recipe NullRecipe => new Recipe(null, null);

        private static Recipe LoadFromDB(ushort recipeDatabaseID)
        {
            var dbRecipe = GameServer.Database.FindObjectByKey<DBCraftedItem>(recipeDatabaseID.ToString());
            if (dbRecipe == null) throw new ArgumentException("No DBCraftedItem with ID " + recipeDatabaseID + "exists.");

            ItemTemplate product = GameServer.Database.FindObjectByKey<ItemTemplate>(dbRecipe.Id_nb);
            if (product == null) throw new ArgumentException("Product ItemTemplate " + dbRecipe.Id_nb + " for Recipe with ID " + dbRecipe.CraftedItemID + " does not exist.");

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

            var recipe = new Recipe(product, ingredients, (eCraftingSkill)dbRecipe.CraftingSkillType, dbRecipe.CraftingLevel, dbRecipe.MakeTemplated);
            return recipe;
        }
    }
}
