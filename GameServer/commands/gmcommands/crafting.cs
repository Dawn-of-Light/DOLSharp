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
using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		 "&crafting",
		 ePrivLevel.GM,
		 "Change the crafting level of your target",
		 "'/crafting add <craftingSkillID> <startLevel>' to add a new crating skill to your target",
		 "'/crafting change <craftingSkillID> <amount>' to increase or decrease the crafting skill level of your target",
		 "'/crafting list' to have the list of all crafting skill with their id")]
	public class CraftCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				DisplaySyntax(client);
				return 1;
			}

			if (args[1] == "list")
			{
				client.Out.SendMessage("Crafting Skill ID description :", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				foreach (int valeur in Enum.GetValues(typeof(eCraftingSkill)))
				{
					client.Out.SendMessage(valeur + " = " + Enum.GetName(typeof(eCraftingSkill), valeur), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return 1;
			}

			GamePlayer targetPlayer = null;
			if (client.Player.TargetObject != null && client.Player.TargetObject is GamePlayer)
				targetPlayer = (GamePlayer)client.Player.TargetObject;

			if (targetPlayer == null)
			{
				if (client.Player.TargetObject != null)
				{
					client.Out.SendMessage("You can't use " + client.Player.TargetObject + " for /crafting command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
				client.Out.SendMessage("You must target a player to use this command.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (args[1])
			{
				case "add":
					{
						eCraftingSkill craftingSkillID = eCraftingSkill.NoCrafting;
						int startLevel = 1;
						try
						{
							craftingSkillID = (eCraftingSkill)Convert.ToUInt16(args[2]);
							if (args.Length > 3) startLevel = Convert.ToUInt16(args[3]);

							AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(craftingSkillID);
							if (skill == null)
							{
								client.Out.SendMessage("You must enter a valid crafting skill id, type /crafting for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								if (targetPlayer.AddCraftingSkill(craftingSkillID, startLevel))
								{
									targetPlayer.Out.SendUpdateCraftingSkills();
									client.Out.SendMessage("Crafting skill " + skill.Name + " correctly added.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									client.Out.SendMessage(targetPlayer.Name + " already have the crafting skill " + skill.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /crafting for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				case "change":
					{
						eCraftingSkill craftingSkillID = eCraftingSkill.NoCrafting;
						int amount = 1;
						try
						{
							craftingSkillID = (eCraftingSkill)Convert.ToUInt16(args[2]);
							if (args.Length > 3) amount = Convert.ToUInt16(args[3]);

							AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(craftingSkillID);
							if (skill == null)
							{
								client.Out.SendMessage("You must enter a valid crafting skill id, type /crafting for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								if (targetPlayer.GetCraftingSkillValue(craftingSkillID) < 0)
								{
									client.Out.SendMessage(targetPlayer.Name + " does not have the crafting skill " + skill.Name + ", add it first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return 1;
								}

								targetPlayer.GainCraftingSkill(craftingSkillID, amount);
								targetPlayer.Out.SendUpdateCraftingSkills();
								client.Out.SendMessage("Crafting skill " + skill.Name + " correctly changed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								client.Out.SendMessage(targetPlayer.Name + " now has " + targetPlayer.GetCraftingSkillValue(craftingSkillID) + " in " + skill.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						catch (Exception)
						{
							client.Out.SendMessage("Type /crafting for command overview.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					break;

				default:
					{
						DisplaySyntax(client);
					}
					break;
			}

			return 1;
		}
	}
}