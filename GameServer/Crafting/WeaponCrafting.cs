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
	/// Crafting skill to make weapon
	/// </summary>
	public class WeaponCrafting : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public WeaponCrafting()
		{
			Icon = 0x01;
			Name = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "Crafting.Name.Weaponcraft");
			eSkill = eCraftingSkill.WeaponCrafting;
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
				player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.FindForge"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

            // Luhz Crafting Update: 
            // Crafting no longer requires hand-held tools!
            /*
			if (player.Inventory.GetFirstItemByName(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.SmithsHammer"), eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == null)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.CheckTool.NotHaveTools", craftItemData.ItemTemplate.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Out.SendMessage(LanguageMgr.GetTranslation(ServerProperties.Properties.DB_LANGUAGE, "Crafting.CheckTool.FindSmithTool"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
            */

			return true;
		}

		/// <summary>
		/// Calculate the minumum needed secondary crafting skill level to make the item
		/// </summary>
		public override int CalculateSecondCraftingSkillMinimumLevel(DBCraftedItem item)
		{
			switch(item.ItemTemplate.Object_Type)
			{
				case (int)eObjectType.CrushingWeapon:
				case (int)eObjectType.SlashingWeapon:
				case (int)eObjectType.ThrustWeapon:
				case (int)eObjectType.TwoHandedWeapon:
				case (int)eObjectType.PolearmWeapon:
				case (int)eObjectType.Flexible:
				case (int)eObjectType.Sword:
				case (int)eObjectType.Hammer:
				case (int)eObjectType.Axe:
				case (int)eObjectType.Spear:
				case (int)eObjectType.HandToHand:
				case (int)eObjectType.Blades:
				case (int)eObjectType.Blunt:
				case (int)eObjectType.Piercing:
				case (int)eObjectType.LargeWeapons:
				case (int)eObjectType.CelticSpear:
				case (int)eObjectType.Scythe:
					return item.CraftingLevel - 60;
			}

			return base.CalculateSecondCraftingSkillMinimumLevel(item);
		}

		/// <summary>
		/// Select craft to gain point and increase it
		/// </summary>
		/// <param name="player"></param>
		/// <param name="item"></param>
		public override void GainCraftingSkillPoints(GamePlayer player, DBCraftedItem item)
		{
			base.GainCraftingSkillPoints(player, item);

            // Luhz Crafting Update:
            // "Secondary" tradeskills are no longer limited by "Primary" tradeskills - Patch 1.87
            /*
            if(player.CraftingPrimarySkill == eCraftingSkill.WeaponCrafting)
			{
				if(player.GetCraftingSkillValue(eCraftingSkill.WeaponCrafting)%100 == 99)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Crafting.GainCraftingSkillPoints.RaiseWeaponcraft"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			else
			{
				int maxAchivableLevel;
				switch (player.CraftingPrimarySkill)
				{
					case eCraftingSkill.ArmorCrafting:
					{
						maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.ArmorCrafting) * 0.75);
						break;
					}

					case eCraftingSkill.Tailoring:
					{
						maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.Tailoring) * 0.40);
						break;
					}

					case eCraftingSkill.Fletching:
					{
						maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.Fletching) * 0.75);
						break;
					}

					default:
					{
						maxAchivableLevel = 0;
						break;
					}
				}

				if(player.GetCraftingSkillValue(eCraftingSkill.WeaponCrafting) >= maxAchivableLevel)
				{
					return;
				}
			}
            */
			if(Util.Chance( CalculateChanceToGainPoint(player, item)))
			{
				player.GainCraftingSkill(eCraftingSkill.WeaponCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}
	}
}
