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
using System.Collections.Generic;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.Database;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&crafting",
		ePrivLevel.GM,
		"GMCommands.Crafting.Description",
		"GMCommands.Crafting.Usage.Add",
		"GMCommands.Crafting.Usage.Change",
		"/crafting salvageadd <SalvageYieldID (0 for next free)> <MaterialId_nb> <Count> [Realm] [PackageID]",
		"/crafting salvageupdate <SalvageYieldID> <MaterialId_nb> <Count> <Realm> [PackageID]",
		"/crafting salvageinfo <SalvageYieldID>",
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

			try
			{
				#region List
				if (args[1].ToLower() == "list")
				{
					List<string> list = new List<string>();
					int count = 0;
					foreach (int value in Enum.GetValues(typeof(eCraftingSkill)))
					{
						if (++count < 16) // get rid of duplicate due to _Last
							list.Add(value + " = " + Enum.GetName(typeof(eCraftingSkill), value));
					}

					client.Out.SendCustomTextWindow(LanguageMgr.GetTranslation(client, "GMCommands.Crafting.SkillDescription"), list);
					return;
				}
				#endregion List

				#region Salvage

				if (args[1].ToLower() == "salvageinfo")
				{
					List<string> list = new List<string>();
					int salvageID = Convert.ToInt32(args[2]);
					SalvageYield salvage = GameServer.Database.SelectObject<SalvageYield>("ID=" + salvageID);

					if (salvage == null)
					{
						DisplayMessage(client, "SalvageYield ID not found!");
						return;
					}

					ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(salvage.MaterialId_nb);
					string materialName = "Not Found!";

					if (template != null)
					{
						materialName = template.Name;
					}

					list.Add("SalvageYield ID: " + salvageID);
					list.Add("     ObjectType: " + (salvage.ObjectType == 0 ? "Unused" : salvage.ObjectType.ToString()));
					list.Add("   SalvageLevel: " + (salvage.SalvageLevel == 0 ? "Unused" : salvage.SalvageLevel.ToString()));
					list.Add("       Material: " + materialName + " (" + salvage.MaterialId_nb + ")");
					list.Add("          Count: " + (salvage.Count == 0 ? "Calculated" : salvage.Count.ToString()));
					list.Add("          Realm: " + (salvage.Realm == 0 ? "Any" : GlobalConstants.RealmToName((eRealm)salvage.Realm)));
					list.Add("      PackageID: " + salvage.PackageID);

					client.Out.SendCustomTextWindow("SalvageYield ID " + salvageID, list);
					return;
				}

				if (args[1].ToLower() == "salvageadd" || args[1].ToLower() == "salvageupdate")
				{
					try
					{
						int salvageID = Convert.ToInt32(args[2]);
						string material = args[3];
						int count = Convert.ToInt32(args[4]);
						byte realm = 0;
						string package = "";

						if (args.Length > 5)
							realm = Convert.ToByte(args[5]);

						if (args.Length > 6)
							package = args[6];

						ItemTemplate template = GameServer.Database.FindObjectByKey<ItemTemplate>(material);

						if (template == null)
						{
							DisplayMessage(client, "Material id_nb " + material + " not found!");
							return;
						}

						SalvageYield salvage = GameServer.Database.SelectObject<SalvageYield>("ID=" + salvageID);

						if (args[1].ToLower() == "salvageadd")
						{
							if (salvage != null)
							{
								DisplayMessage(client, "This SalvageYield ID already exists, use salvageupdate to change it.");
								return;
							}

							salvage = new SalvageYield();
							if (salvageID > 0)
								salvage.ID = salvageID;

							salvage.MaterialId_nb = material;
							salvage.Count = Math.Max(1, count);
							salvage.Realm = realm;

							if (package == "")
							{
								package = client.Player.Name;
							}

							salvage.PackageID = package;

							GameServer.Database.AddObject(salvage);

							DisplayMessage(client, string.Format("Created SalvageYield ID: {0}, Material: {1}, Count: {2}, Realm: {3}, PackageID: {4}",
																	salvage.ID, salvage.MaterialId_nb, salvage.Count, salvage.Realm, salvage.PackageID));
						}
						else
						{
							if (salvage == null)
							{
								DisplayMessage(client, "SalvageID not found!");
								return;
							}

							if (salvage.PackageID == SalvageYield.LEGACY_SALVAGE_ID)
							{
								DisplayMessage(client, "This SalvageYield ID is used for legacy salvage support and can not be updated.");
								return;
							}

							salvage.MaterialId_nb = material;
							salvage.Count = Math.Max(1, count);
							salvage.Realm = realm;

							if (string.IsNullOrEmpty(salvage.PackageID) && package == "")
							{
								package = client.Player.Name;
							}

							if (package != "")
							{
								salvage.PackageID = package;
							}

							GameServer.Database.SaveObject(salvage);

							DisplayMessage(client, string.Format("Updated SalvageYield ID: {0}, Material: {1}, Count: {2}, Realm: {3}, PackageID: {4}",
																	salvage.ID, salvage.MaterialId_nb, salvage.Count, salvage.Realm, salvage.PackageID));
						}

					}
					catch
					{
						DisplaySyntax(client);
					}

					return;
				}

				#endregion Salvage

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
										target.SaveIntoDatabase();
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
									target.SaveIntoDatabase();
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.SkillChanged", skill.Name));
									DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Crafting.NowHasSkillPoints", target.Name, target.GetCraftingSkillValue(craftingSkillID), (eCraftingSkill)craftingSkillID));
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
			catch
			{
				DisplaySyntax(client);
			}
		}
	}
}