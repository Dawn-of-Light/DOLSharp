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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	public class BasicCrafting : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public BasicCrafting()
		{
			Icon = 0x0F;
            Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.BasicCrafting");
            eSkill = eCraftingSkill.BasicCrafting;
		}

		public override string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "Crafter's";
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
			// TODO : implement tool checks based on recipes
			return true;
		}

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			if (Util.Chance(CalculateChanceToGainPoint(player, recipe)))
			{
				player.GainCraftingSkill(eCraftingSkill.BasicCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
