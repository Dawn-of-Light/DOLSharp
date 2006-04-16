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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// SiegeCrafting is the crafting skill to make siege weapon like catapult, balista,...
	/// </summary>
	public class SiegeCrafting : AbstractCraftingSkill
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public SiegeCrafting() : base()
		{
			Icon = 0x03;
			Name = "Siegecraft";
			eSkill = eCraftingSkill.SiegeCrafting;
		}

		/// <summary>
		/// Check if  the player own all needed tools
		/// </summary>
		/// <param name="player">the crafting player</param>
		/// <param name="craftItemData">the object in construction</param>
		/// <returns>true if the player hold all needed tools</returns>
		public override bool CheckTool(GamePlayer player, CraftItemData craftItemData)
		{
			bool result = false;
			foreach (GameCraftTool item in player.GetInRadius(typeof(GameCraftTool), CRAFT_DISTANCE))
			{
				if(item.ToolType == eCraftingToolType.Lathe) // Lathe (model = 481)
				{
					result = true;
				}
			}

			if(result == false)
			{
				player.Out.SendMessage("You do not have the tools to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				player.Out.SendMessage("You must find a lathe!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			byte flags = 0;
			foreach (GenericItem item in player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
			{
				if(!(item is CraftingTool)) continue;

				if(((CraftingTool)item).Type == eCraftingToolType.PlaningTool)
				{
					if((flags & 0x01) == 0) flags |= 0x01;
					if(flags >= 0x07) break;
				}
				else if(((CraftingTool)item).Type == eCraftingToolType.SmithHammer)
				{
					if((flags & 0x02) == 0) flags |= 0x02;
					if(flags >= 0x07) break;
				}
				else if(((CraftingTool)item).Type == eCraftingToolType.SewingKit)
				{
					if((flags & 0x04) == 0) flags |= 0x04;
					if(flags >= 0x07) break;
				}
			}

			if(flags < 0x07)
			{
				player.Out.SendMessage("You do not have the tools to make the "+craftItemData.TemplateToCraft.Name+".",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					
				if((flags & 0x01) == 0)
				{
					player.Out.SendMessage("You must find a planing tool!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}

				if((flags & 0x02) == 0)
				{
					player.Out.SendMessage("You must find a smith tool!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
					return false;
				}

				if((flags & 0x04) == 0)
				{
					player.Out.SendMessage("You must find a sewing kit!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
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
			int maxAchivableLevel;
			switch (player.CraftingPrimarySkill)
			{
				case eCraftingSkill.WeaponCrafting:
				{
					maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.WeaponCrafting) * 0.75);
					break;
				}

				case eCraftingSkill.ArmorCrafting:
				{
					maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.ArmorCrafting) * 0.40);
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

				case eCraftingSkill.Alchemy:
				{
					maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.Alchemy) * 0.02);
					break;
				}

				case eCraftingSkill.SpellCrafting:
				{
					maxAchivableLevel = (int)(player.GetCraftingSkillValue(eCraftingSkill.SpellCrafting) * 0.02);
					break;
				}

				default:
				{
					maxAchivableLevel = 0;
					break;
				}
			}

			if(player.GetCraftingSkillValue(eCraftingSkill.SiegeCrafting) >= maxAchivableLevel)
			{
				return;
			}

			if(Util.Chance( CalculateChanceToGainPoint(player, item)))
			{
				player.IncreaseCraftingSkill(eCraftingSkill.SiegeCrafting, 1);
				player.Out.SendUpdateCraftingSkills();
			}
		}
		protected override void BuildCraftedItem(GamePlayer player, CraftItemData craftItemData)
		{
		/*	GameSiegeWeapon siegeweapon = null;
			switch ((eObjectType)craftItemData.ItemtemplateToCraft.Object_Type)
			{
				case eObjectType.SiegeBalista :
				{
					siegeweapon = new GameSiegeBallista();
				}
			break;
				case eObjectType.SiegeCatapult :
				{
					siegeweapon = new GameSiegeCatapult();
				}
				break;
				case eObjectType.SiegeCauldron :
				{
					siegeweapon = new GameSiegeCauldron();
				}
				break;
				case eObjectType.SiegeRam :
				{
					siegeweapon = new GameSiegeRam();
				}
				break;
				case eObjectType.SiegeTrebuchet :
				{
					siegeweapon = new GameSiegeTrebuchet();
				}
				break;
				default:
				{
					base.BuildCraftedItem(player,craftItemData);
					return;
				}
			}
			siegeweapon.LoadFromDatabase(craftItemData.ItemtemplateToCraft);
			siegeweapon.Region = player.Region;
			siegeweapon.Heading = player.Heading;
			siegeweapon.Position = player.Position;
			siegeweapon.AddToWorld();*/
		}
	}
}
