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
using System;

namespace DOL.GS
{
	public class Alchemy : AdvancedCraftingSkill
	{
		public Alchemy()
		{
			Icon = 0x04;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.Alchemy");
			eSkill = eCraftingSkill.Alchemy;
		}
		public override string CRAFTER_TITLE_PREFIX
		{
			get
			{
				return "Alchemist's";
			}
		}

        protected override String Profession
        {
            get 
            { 
                return LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, 
                    "CraftersProfession.Alchemist"); 
            }
        }

		#region Classic craft functions
		protected override bool CheckTool(GamePlayer player, DBCraftedItem craftItemData)
		{
            return base.CheckTool(player, craftItemData);
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
			if(Util.Chance( CalculateChanceToGainPoint(player, item)))
			{
				player.GainCraftingSkill(eCraftingSkill.Alchemy, 1);
                // one of the raw materials gains the point for main skill, thats why we item.RawMaterials.Length - 1
                for (int ii = 0; ii < item.RawMaterials.Length - 1; ii++)
                {
                    if (player.GetCraftingSkillValue(eCraftingSkill.HerbalCrafting) < subSkillCap)
                    {
                        player.GainCraftingSkill(eCraftingSkill.HerbalCrafting, 1);
                    }
                }
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
			if(! base.IsAllowedToCombine(player, item)) return false;
			
			if(((InventoryItem)player.TradeWindow.TradeItems[0]).Object_Type != (int)eObjectType.AlchemyTincture)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Alchemy.IsAllowedToCombine.AlchemyTinctures"), PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
				return false;
			}

			if(player.TradeWindow.ItemsCount > 1)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Alchemy.IsAllowedToCombine.OneTincture"), PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
				return false;
			}

			if(item.ProcSpellID != 0 || item.SpellID != 0)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Alchemy.IsAllowedToCombine.AlreadyImbued", item.Name), PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
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
			if(item == null || tincture == null) return ; // be sure at least one item in each side
			
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
		}

		#endregion

		#region Calcul functions
		/// <summary>
		/// Get the maximum charge the item will have
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public int GetItemMaxCharges(InventoryItem item)
		{
			if (item.Quality <= 94) return 2;
			else if (item.Quality <= 95) return 3;
			else if (item.Quality <= 96) return 4;
			else if (item.Quality <= 97) return 5;
			else if (item.Quality <= 98) return 6;
			else if (item.Quality <= 99) return 7;
			else return 10;
		}

		#endregion

	}
}
