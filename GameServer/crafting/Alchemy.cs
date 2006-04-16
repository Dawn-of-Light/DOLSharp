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
	/// Alchemy is the advanced crafting skill to add proc and reactive effect on item
	/// </summary>
	public class Alchemy : AdvancedCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public Alchemy()
		{
			Icon = 0x04;
			Name = "Alchemy";
			eSkill = eCraftingSkill.Alchemy;
		}

		#region Classic craft functions

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, CraftItemData craftItemData)
		{
			if(! base.CheckTool(player, craftItemData)) return false;

			byte flags = 0;
			foreach (GenericItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				if(!(item is CraftingTool)) continue;

				if(((CraftingTool)item).Type == eCraftingToolType.AlchemyKit)
				{
					if((flags & 0x01) == 0) flags |= 0x01;
					if(flags >= 0x03) break;
				}
				else if(((CraftingTool)item).Type == eCraftingToolType.MortarAndPestle)
				{
					if((flags & 0x02) == 0) flags |= 0x02;
					if(flags >= 0x03) break;
				}
			}

			if(flags < 0x03)
			{
				player.Out.SendMessage("You do not have the tools to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					
				if((flags & 0x01) == 0)
				{
					player.Out.SendMessage("You must find a alchemy kit!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}
				else
				{
					player.Out.SendMessage("You must find a mortar and pestle!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}	
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
			base.GainCraftingSkillPoints(player, item);

			if(player.CraftingPrimarySkill == eCraftingSkill.Alchemy)
			{
				if(player.GetCraftingSkillValue(eCraftingSkill.Alchemy)%100 == 99)
				{
					player.Out.SendMessage("You must see your trainer to raise your Alchemy further!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
					return;
				}
			}
			else
			{
				int maxAchivableLevel;
				switch (player.CraftingPrimarySkill)
				{
					case eCraftingSkill.SpellCrafting:
					{
						maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting) * 0.45);
						break;
					}

					default:
					{
						maxAchivableLevel = 0;
						break;
					}
				}

				if(player.GetCraftingSkillValue(eCraftingSkill.Alchemy) >= maxAchivableLevel)
				{
					return;
				}
			}

			if(Util.Chance( CalculateChanceToGainPoint(player, item)))
			{
				player.IncreaseCraftingSkill(eCraftingSkill.Alchemy, 1);
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
		public override bool IsAllowedToCombine(GamePlayer player, GenericItem item)
		{
			if(! base.IsAllowedToCombine(player, item)) return false;
			
			if(!(player.TradeWindow.TradeItems[0] is AlchemieTincture))
			{
				player.Out.SendMessage("You can only combine alchemy tinctures!",PacketHandler.eChatType.CT_System,PacketHandler.eChatLoc.CL_SystemWindow);
				return false;
			}

			if(player.TradeWindow.ItemsCount > 1)
			{
				player.Out.SendMessage("You can only combine one tincture on one item!",PacketHandler.eChatType.CT_System,PacketHandler.eChatLoc.CL_SystemWindow);
				return false;
			}

			if(((EquipableItem)item).ProcEffectType != eMagicalEffectType.NoEffect || ((EquipableItem)item).ChargeEffectType == eMagicalEffectType.ChargeEffect)
			{
				player.Out.SendMessage("The "+item.Name+" is already imbued with a magical effect!",PacketHandler.eChatType.CT_System,PacketHandler.eChatLoc.CL_SystemWindow);
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
		protected override void ApplyMagicalEffect(GamePlayer player, EquipableItem item)
		{
			AlchemieTincture tincture = (AlchemieTincture)player.TradeWindow.TradeItems[0];
			if(item == null || tincture == null) return ; // be sure at least one item in each side
			
			if(tincture is ActiveTincture)
			{
				item.ProcSpellID = ((ActiveTincture)tincture).SpellID;
				item.ProcEffectType = eMagicalEffectType.ActiveEffect;
			}
			else if(tincture is ReactiveTincture)
			{
				item.ProcSpellID = ((ReactiveTincture)tincture).SpellID;
				item.ProcEffectType = eMagicalEffectType.ReactiveEffect;
			}
			else if(tincture is StableTincture)
			{
				item.ChargeSpellID = ((StableTincture)tincture).SpellID;
				item.MaxCharge = GetItemMaxCharges(item);
				item.Charge = item.MaxCharge;
				item.ChargeEffectType = eMagicalEffectType.ChargeEffect;
			}

			player.Inventory.RemoveItem(tincture);
		}

		#endregion

		#region Calcul functions
		/// <summary>
		/// Get the maximum charge the item will have
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public byte GetItemMaxCharges(EquipableItem item)
		{
			if (item.Quality <= 94)			return 2;
			else if (item.Quality <= 95)	return 3;
			else if (item.Quality <= 96)	return 4;
			else if (item.Quality <= 97)	return 5;
			else if (item.Quality <= 98)	return 6;
			else if (item.Quality <= 99)	return 7;
			else	return 10;
		}

		#endregion

	}
}
