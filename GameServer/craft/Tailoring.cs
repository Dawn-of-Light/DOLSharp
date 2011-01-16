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
using DOL.Language;
using DOL.GS.PacketHandler;
using System;
using System.Collections.Generic;

namespace DOL.GS
{
    public class Tailoring : AbstractProfession
    {
        public Tailoring()
        {
            Icon = 0x0B;
            Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Tailoring");
            eSkill = eCraftingSkill.Tailoring;
        }

        protected override String Profession
        {
            get
            {
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,
                    "CraftersProfession.Tailor");
            }
        }

		/// <summary>
		/// Check if the player is near the needed tools (forge, lathe, etc)
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="recipe">the recipe being used</param>
		/// <param name="itemToCraft">the item to make</param>
		/// <param name="rawMaterials">a list of raw materials needed to create this item</param>
		/// <returns>true if required tools are found</returns>
		protected override bool CheckForTools(GamePlayer player, DBCraftedItem recipe, ItemTemplate itemToCraft, IList<DBCraftedXItem> rawMaterials)
		{
            bool needForge = false;

            foreach (DBCraftedXItem material in rawMaterials)
            {
				ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material.IngredientId_nb);
                if (template != null && template.Model == 519) // metal bar
                {
                    needForge = true;
                    break;
                }
            }

            if (needForge)
            {
                foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
                {
                    if (item.Name == "forge" || item.Model == 478) // Forge
                        return true;
                }

                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.FindForge"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				if (player.Client.Account.PrivLevel > 1)
					return true;

				return false;
            }

            return true;
        }

        /// <summary>
        /// Calculate the minumum needed secondary crafting skill level to make the item
        /// </summary>
        public override int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem recipe, ItemTemplate itemToCraft)
        {
            switch (itemToCraft.Object_Type)
            {
                case (int)eObjectType.Cloth:
                case (int)eObjectType.Leather:
                case (int)eObjectType.Studded:
                    return recipe.CraftingLevel - 30;
            }

            return base.GetSecondaryCraftingSkillMinimumLevel(recipe, itemToCraft);
        }

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
            if (Util.Chance(CalculateChanceToGainPoint(player, recipe)))
            {
                player.GainCraftingSkill(eCraftingSkill.Tailoring, 1);
                base.GainCraftingSkillPoints(player, recipe, rawMaterials);
                player.Out.SendUpdateCraftingSkills();
            }
        }
    }
}
