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
using System.Collections.Specialized;
using System.Collections;
using System.Reflection;
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using log4net;
using System;

namespace DOL.GS
{
	/// <summary>
	/// AdvancedCraftingSkill is the skill for alchemy and spellcrafting whitch add all combine system
	/// </summary>
	public abstract class AdvancedCraftingSkill : AbstractProfession
    {
        #region Classic craft function

        /// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		protected override bool CheckForTools(GamePlayer player, DBCraftedItem craftItemData)
		{
			foreach (GameStaticItem item in player.GetItemsInRadius(CRAFT_DISTANCE))
			{
                if (item.Name == "alchemy table" || item.Model == 820) // Alchemy Table
                {
					return true;
				}
			}

			player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.CheckTool.FindAlchemyTable"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			
			return false;
		}
		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
            ;
		}

		#endregion

		#region Advanced craft function

		#region First call function
		
		/// <summary>
		/// Called when player accept to combine items
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CombineItems(GamePlayer player)
		{
			if(player.TradeWindow.PartnerTradeItems == null || player.TradeWindow.PartnerItemsCount != 1)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.CombineItems.OnlyCombine"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			InventoryItem itemToCombine = (InventoryItem)player.TradeWindow.PartnerTradeItems[0];
			if(!IsAllowedToCombine(player, itemToCombine)) return false;

			ApplyMagicalEffect(player, itemToCombine);

			return true;
		}

		#endregion

		#region Requirement check

		/// <summary>
        /// Check if the player can enchant the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		/// <returns></returns>
		public virtual bool IsAllowedToCombine(GamePlayer player, InventoryItem item)
		{
			if(item == null) return false;
			
			if(player.TradeWindow.ItemsCount <= 0)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.Imbue", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;	
			}

			if(!item.IsCrafted)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.CraftedItems"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}


            InventoryItem itemToCombine = (InventoryItem)player.TradeWindow.TradeItems[0];

            if (itemToCombine.Object_Type == (int)eObjectType.AlchemyTincture)
            {
                switch (itemToCombine.Type_Damage)
                {
                    case 0: //Type damage 0 = armors
                        if (!GlobalConstants.IsArmor(item.Object_Type))
                        {
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.NoGoodCombine"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        break;
                    case 1: //Type damage 1 = weapons
                        if (!GlobalConstants.IsWeapon(item.Object_Type))
                        {
                            player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.NoGoodCombine"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return false;
                        }
                        break;
                    default:
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.ProblemCombine"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return false;
                }
            }

			if (!GlobalConstants.IsArmor(item.Object_Type) && !GlobalConstants.IsWeapon(item.Object_Type))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "AdvancedCraftingSkill.IsAllowedToCombine.NoEnchanted"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;	
			}

			return true;
		}
		
		#endregion
		
		#region Apply magical effect

		/// <summary>
        /// Apply the magical bonus to the item
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		protected abstract void ApplyMagicalEffect(GamePlayer player, InventoryItem item);
		
		#endregion

		#endregion

	}
}
