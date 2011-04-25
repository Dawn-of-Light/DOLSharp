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
using System;
using System.Collections.Generic;

namespace DOL.GS
{
	public class Alchemy : AdvancedCraftingSkill
	{
		public Alchemy()
		{
			Icon = 0x04;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                "Crafting.Name.Alchemy");
			eSkill = eCraftingSkill.Alchemy;
		}

        protected override String Profession
        {
            get 
            { 
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                    "CraftersProfession.Alchemist"); 
            }
        }

		#region Classic Crafting Overrides

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
			return base.CheckForTools(player, recipe, itemToCraft, rawMaterials);
		}

		/// <summary>
		/// Gain a point in the appropriate skills for a recipe and materials
		/// </summary>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem recipe, IList<DBCraftedXItem> rawMaterials)
		{
			if (Util.Chance( CalculateChanceToGainPoint(player, recipe)))
			{
				player.GainCraftingSkill(eCraftingSkill.Alchemy, 1);

				if (player.GetCraftingSkillValue(eCraftingSkill.HerbalCrafting) < subSkillCap)
					player.GainCraftingSkill(eCraftingSkill.HerbalCrafting, 1);

				player.Out.SendUpdateCraftingSkills();
			}
		}

		#endregion
		
		#region Requirement check

		/// <summary>
		/// This function is called when player accept the combine
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override bool IsAllowedToCombine(GamePlayer player, InventoryItem item)
		{
			if (!base.IsAllowedToCombine(player, item)) 
                return false;
			
			if (((InventoryItem)player.TradeWindow.TradeItems[0]).Object_Type != 
                (int)eObjectType.AlchemyTincture)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, 
                    "Alchemy.IsAllowedToCombine.AlchemyTinctures"), PacketHandler.eChatType.CT_System, 
                    PacketHandler.eChatLoc.CL_SystemWindow);
				
                return false;
			}

			if (player.TradeWindow.ItemsCount > 1)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client,
                    "Alchemy.IsAllowedToCombine.OneTincture"), PacketHandler.eChatType.CT_System, 
                    PacketHandler.eChatLoc.CL_SystemWindow);

				return false;
			}

			if (item.ProcSpellID != 0 || item.SpellID != 0)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, 
                    "Alchemy.IsAllowedToCombine.AlreadyImbued", item.Name), 
                    PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);

				return false;
			}

			return true;
		}

		#endregion

		#region Apply magical effect

		/// <summary>
		/// Apply all needed magical bonus to the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		protected override void ApplyMagicalEffect(GamePlayer player, InventoryItem item)
		{
			InventoryItem tincture = (InventoryItem)player.TradeWindow.TradeItems[0];

            // One item each side of the trade window.

			if (item == null || tincture == null) 
                return ;
			
			if(tincture.ProcSpellID != 0)
			{
				item.ProcSpellID = tincture.ProcSpellID;
			}
			else
			{
				item.MaxCharges = GetItemMaxCharges(item);
				item.Charges = item.MaxCharges;
				item.SpellID = tincture.SpellID;
			}

			player.Inventory.RemoveCountFromStack(tincture, 1);

			if (item.Template is ItemUnique)
			{
				GameServer.Database.SaveObject(item);
				GameServer.Database.SaveObject(item.Template as ItemUnique);
			}
			else
			{
				ChatUtil.SendErrorMessage(player, "Alchemy crafting error: Item was not an ItemUnique, crafting changes not saved to DB!");
				log.ErrorFormat("Alchemy crafting error: Item {0} was not an ItemUnique for player {1}, crafting changes not saved to DB!", item.Id_nb, player.Name);
			}
		}

		#endregion

		/// <summary>
		/// Get the maximum charge the item will have
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int GetItemMaxCharges(InventoryItem item)
		{
			if(item.Quality < 94)
			{
				return 2;
			}
			if(item.Quality >= 100)
			{
				return 10;
			}
			return item.Quality - 92;
		}
	}
}
