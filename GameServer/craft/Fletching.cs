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
                return "CraftersProfession.Fletcher";
            }
        }

		public Fletching()
		{
			Icon = 0x0C;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Fletching");
			eSkill = eCraftingSkill.Fletching;
		}

		protected override bool CheckForTools(GamePlayer player, Recipe recipe)
		{
			if (recipe.Product.Object_Type != (int)eObjectType.Arrow &&
				recipe.Product.Object_Type != (int)eObjectType.Bolt)
			{
				foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
				{
					if (item.Name.ToLower() == "lathe" || item.Model == 481) // Lathe
						return true;
				}

				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "Crafting.CheckTool.NotHaveTools", recipe.Product.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.FindLathe"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				if (player.Client.Account.PrivLevel > 1)
					return true;

				return false;
			}

			return true;
		}

		public override int GetSecondaryCraftingSkillMinimumLevel(Recipe recipe)
		{
			switch (recipe.Product.Object_Type)
			{
				case (int)eObjectType.Fired:  //tested
				case (int)eObjectType.Longbow: //tested
				case (int)eObjectType.Crossbow: //tested
				case (int)eObjectType.Instrument: //tested
				case (int)eObjectType.RecurvedBow:
				case (int)eObjectType.CompositeBow:
					return recipe.Level - 20;

				case (int)eObjectType.Arrow: //tested
				case (int)eObjectType.Bolt: //tested
				case (int)eObjectType.Thrown:
					return recipe.Level - 15;

				case (int)eObjectType.Staff: //tested
					return recipe.Level - 35;
			}

			return base.GetSecondaryCraftingSkillMinimumLevel(recipe);
		}

		public override void GainCraftingSkillPoints(GamePlayer player, Recipe recipe)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, recipe.Level)))
			{
				player.GainCraftingSkill(eCraftingSkill.Fletching, 1);
				base.GainCraftingSkillPoints(player, recipe);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
