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
using System.Collections.Specialized;
using System.Reflection;
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The metalworking crafting skill
	/// </summary>
	public class MetalWorking : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public MetalWorking()
		{
			Icon = 0x06;
			Name = "Metalworking";
			eSkill = eCraftingSkill.MetalWorking;
		}

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, DBCraftedItem craftItemData)
		{
			bool result = false;
			foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
			{
                if (item.Name == "forge" || item.Model == 478) // Forge
                {
					result = true;
					break;
				}
			}

			if(result == false)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.FindForge"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Inventory.GetFirstItemByName(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.SmithsHammer"), eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.FindSmithTool"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
			if (player.GetCraftingSkillValue(eCraftingSkill.MetalWorking) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill cap == primary skill
			{
				if(Util.Chance( CalculateChanceToGainPoint(player, item)))
				{
					player.GainCraftingSkill(eCraftingSkill.MetalWorking, 1);
					player.Out.SendUpdateCraftingSkills();
				}
			}
		}
	}
}
