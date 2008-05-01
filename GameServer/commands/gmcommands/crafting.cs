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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&crafting",
		ePrivLevel.GM,
		"GMCommands.Crafting.Description",
		"GMCommands.Crafting.Usage.Add",
		"GMCommands.Crafting.Usage.Change",
		"GMCommands.Crafting.Usage.List")]
	public class CraftCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			#region List
			if (args[1].ToLower() == "list")
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.SkillDescription"));
				foreach (int value in Enum.GetValues(typeof(eCraftingSkill)))
					DisplayMessage(client, value + " = " + Enum.GetName(typeof(eCraftingSkill), value));
			}
			#endregion List

			GamePlayer target = null;
			if ((client.Player.TargetObject != null) && (client.Player.TargetObject is GamePlayer))
				target = client.Player.TargetObject as GamePlayer;
			else
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.NoPlayerTarget"));
				return;
			}

			switch (args[1].ToLower())
			{
				#region Add
				case "add":
					{
						eCraftingSkill craftingSkillID = eCraftingSkill.NoCrafting;
						int startLevel = 1;
						try
						{
							craftingSkillID = (eCraftingSkill)Convert.ToUInt16(args[2]);
							if (args.Length > 3)
								startLevel = Convert.ToUInt16(args[3]);

							AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(craftingSkillID);
							if (skill == null)
							{
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.InvalidSkill"));
							}
							else
							{
								if (target.AddCraftingSkill(craftingSkillID, startLevel))
								{
									target.Out.SendUpdateCraftingSkills();
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.SkillAdded", skill.Name));
								}
								else
								{
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.AlreadyHaveSkill", target.Name, skill.Name));
								}
							}
						}
						catch (Exception)
						{
							DisplaySyntax(client);
						}
						break;
					}
				#endregion Add
				#region Change
				case "change":
					{
						eCraftingSkill craftingSkillID = eCraftingSkill.NoCrafting;
						int amount = 1;
						try
						{
							craftingSkillID = (eCraftingSkill)Convert.ToUInt16(args[2]);
							if (args.Length > 3)
								amount = Convert.ToUInt16(args[3]);

							AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(craftingSkillID);
							if (skill == null)
							{
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.InvalidSkill"));
							}
							else
							{
								if (target.GetCraftingSkillValue(craftingSkillID) < 0)
								{
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.NotHaveSkillAddIt", target.Name, skill.Name));
									return;
								}

								target.GainCraftingSkill(craftingSkillID, amount);
								target.Out.SendUpdateCraftingSkills();
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.SkillChanged", skill.Name));
								DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.NowHasSkillPoints", target.Name, target.GetCraftingSkillValue(craftingSkillID)));
							}
						}
						catch (Exception)
						{
							DisplaySyntax(client);
							return;
						}
						break;
					}
				#endregion Change
				#region Default
				default:
					{
						DisplaySyntax(client);
						break;
					}
				#endregion Default
			}
		}
	}
}