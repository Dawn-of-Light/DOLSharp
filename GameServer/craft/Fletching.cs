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
	public class Fletching : AbstractProfession
	{
        protected override String Profession
        {
            get
            {
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE,
                    "CraftersProfession.Fletcher");
            }
        }

		public Fletching()
		{
			Icon = 0x0C;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Fletching");
			eSkill = eCraftingSkill.Fletching;
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
			if (itemToCraft.Object_Type != (int)eObjectType.Arrow &&
				itemToCraft.Object_Type != (int)eObjectType.Bolt)
			{
				foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
				{
					if (item.Name.ToLower() == "lathe" || item.Model == 481) // Lathe
						return true;
				}

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", itemToCraft.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.FindLathe"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				if (player.Client.Account.PrivLevel > 1)
					return true;

				return false;
			}

			return true;
		}

		public override int GetSecondaryCraftingSkillMinimumLevel(DBCraftedItem recipe, ItemTemplate itemToCraft)
		{
			switch (itemToCraft.Object_Type)
			{
				case (int)eObjectType.Fired:  //tested
				case (int)eObjectType.Longbow: //tested
				case (int)eObjectType.Crossbow: //tested
				case (int)eObjectType.Instrument: //tested
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.CompositeBow:
					return recipe.CraftingLevel - 20;

				case (int)eObjectType.Arrow: //tested
				case (int)eObjectType.Bolt: //tested
				case (int)eObjectType.Thrown:
					return recipe.CraftingLevel - 15;

				case (int)eObjectType.Staff: //tested
					return recipe.CraftingLevel - 35;
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
				player.GainCraftingSkill(eCraftingSkill.Fletching, 1);
				base.GainCraftingSkillPoints(player, recipe, rawMaterials);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
