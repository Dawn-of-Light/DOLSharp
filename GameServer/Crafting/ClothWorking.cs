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
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// The cloth crafting skill
	/// </summary>
	public class ClothWorking : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ClothWorking()
		{
			Icon = 0x08;
			Name = "Clothworking";
			eSkill = eCraftingSkill.ClothWorking;
		}

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, CraftItemData craftItemData)
		{
			bool sewingKitFound = false;
			foreach (GenericItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				if(!(item is CraftingTool)) continue;

				if(((CraftingTool)item).Type == eCraftingToolType.SewingKit)
				{
					sewingKitFound = true;
					break;
				}
			}

			if(sewingKitFound == false)
			{
				player.Out.SendMessage("You do not have the tools to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendMessage("You must find a sewing kit!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, CraftItemData item)
		{
			if (player.GetCraftingSkillValue(eCraftingSkill.ClothWorking) < player.GetCraftingSkillValue(player.CraftingPrimarySkill)) // max secondary skill cap == primary skill
			{
				if(Util.Chance( CalculateChanceToGainPoint(player, item)))
				{
					player.IncreaseCraftingSkill(eCraftingSkill.ClothWorking, 1);
					player.Out.SendUpdateCraftingSkills();
				}
			}
		}
	}
}
