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
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections.Generic;

namespace DOL.GS.Commands
{
	[CmdAttribute("&jump",
		ePrivLevel.GM,
		"GMCommands.Jump.Description",
		"GMCommands.Jump.Information",
		"GMCommands.Jump.Usage.ToPlayerName",
		"/jump to <#ClientID> ex. /jump to #10",
		"GMCommands.Jump.Usage.ToNameRealmID",
		"GMCommands.Jump.Usage.ToXYZRegionID",
		"/jump to house <myhouse | house number>",
		"GMCommands.Jump.Usage.PlayerNameToXYZ",
		"GMCommands.Jump.Usage.PlayerNameToXYZRegID",
		"GMCommands.Jump.Usage.PlayerNToPlayerN",
		"GMCommands.Jump.Usage.ToGT",
		"GMCommands.Jump.Usage.RelXYZ",
		"GMCommands.Jump.Usage.Push",
		"GMCommands.Jump.Usage.Pop",
		"/jump refresh - force a world refresh around you"
		)]
	public class JumpCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		private const string TEMP_KEY_JUMP = "JumpLocStack";

		public void OnCommand(GameClient client, string[] args)
		{
			try
			{
				#region Refresh
				if (args.Length == 2 && args[1].ToLower() == "refresh")
				{
					client.Player.LeaveHouse();
					client.Player.RefreshWorld();
					return;
				}
				#endregion Refresh
				#region Jump to GT
				if (args.Length == 3 && args[1].ToLower() == "to" && args[2].ToLower() == "gt")
				{
					client.Player.MoveTo(client.Player.CurrentRegionID, client.Player.GroundTarget.X, client.Player.GroundTarget.Y, client.Player.GroundTarget.Z, client.Player.Heading);
					return;
				}
				#endregion Jump to GT
				#region Jump to house
				else if (args.Length >= 3 && args[1].ToLower() == "to" && (args[2].ToLower() == "house" || args[2].ToLower() == "myhouse"))
				{
					House house = null;
					if (args[2] == "myhouse")
					{
						house = HouseMgr.GetHouseByPlayer(client.Player);
					}
					else
					{
						house = HouseMgr.GetHouse(Convert.ToInt32(args[3]));
					}
					if (house != null)
					{
						client.Player.MoveTo(house.OutdoorJumpPoint);
					}
					else
					{
						client.Out.SendMessage("This house number is not owned by anyone!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}
				#endregion Jump to house
				#region Jump to PlayerName or ClientID
				if (args.Length == 3 && args[1] == "to")
				{
					GameClient clientc = null;
					if (args[2].StartsWith("#"))
					{
						try
						{
							int sessionID = Convert.ToInt32(args[2].Substring(1));
							clientc = WorldMgr.GetClientFromID(sessionID);
						}
						catch
						{
						}
					}
					else
					{
						clientc = WorldMgr.GetClientByPlayerName(args[2], false, true);
					}

					if (clientc == null)
					{
						GameNPC[] npcs = WorldMgr.GetNPCsByName(args[2], eRealm.None);

						if (npcs.Length > 0)
						{
							// for multiple npc's first try to jump to the npc in the players current region
							GameNPC jumpTarget = npcs[0];

							foreach (GameNPC npc in npcs)
							{
								if (npc.CurrentRegionID == client.Player.CurrentRegionID)
								{
									jumpTarget = npc;
									break;
								}
							}

							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.JumpToX", npcs[0].CurrentRegion.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							if (jumpTarget.InHouse && jumpTarget.CurrentHouse != null)
							{
								jumpTarget.CurrentHouse.Enter(client.Player);
							}
							else
							{
								client.Player.MoveTo(jumpTarget.CurrentRegionID, jumpTarget.X, jumpTarget.Y, jumpTarget.Z, jumpTarget.Heading);
							}
							return;
						}

						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.CannotBeFound", args[2]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					if (CheckExpansion(client, clientc, clientc.Player.CurrentRegionID))
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.JumpToX", clientc.Player.CurrentRegion.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						if (clientc.Player.CurrentHouse != null && clientc.Player.InHouse)
							clientc.Player.CurrentHouse.Enter(client.Player);
						else
							client.Player.MoveTo(clientc.Player.CurrentRegionID, clientc.Player.X, clientc.Player.Y, clientc.Player.Z, client.Player.Heading);
						return;
					}

					client.Out.SendMessage("You don't have an expansion needed to jump to this location.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					return;
				}
				#endregion Jump to PlayerName
				#region Jump to Name Realm
				else if (args.Length == 4 && args[1] == "to")
				{
					GameClient clientc;
					clientc = WorldMgr.GetClientByPlayerName(args[2], false, true);
					if (clientc == null)
					{
						int realm = 0;
						int.TryParse(args[3], out realm);

						GameNPC[] npcs = WorldMgr.GetNPCsByName(args[2], (eRealm)realm);

						if (npcs.Length > 0)
						{
							// for multiple npc's first try to jump to the npc in the players current region
							GameNPC jumpTarget = npcs[0];

							foreach (GameNPC npc in npcs)
							{
								if (npc.CurrentRegionID == client.Player.CurrentRegionID)
								{
									jumpTarget = npc;
									break;
								}
							}

							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.JumpToX", npcs[0].CurrentRegion.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Player.MoveTo(jumpTarget.CurrentRegionID, jumpTarget.X, jumpTarget.Y, jumpTarget.Z, jumpTarget.Heading);
							return;
						}

						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.CannotBeFoundInRealm", args[2], realm), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (CheckExpansion(client, clientc, clientc.Player.CurrentRegionID))
					{
						if (clientc.Player.InHouse)
						{
							client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.CannotJumpToInsideHouse"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.JumpToX", clientc.Player.CurrentRegion.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						if (clientc.Player.CurrentHouse != null && clientc.Player.InHouse)
							clientc.Player.CurrentHouse.Enter(client.Player);
						else
							client.Player.MoveTo(clientc.Player.CurrentRegionID, clientc.Player.X, clientc.Player.Y, clientc.Player.Z, client.Player.Heading);
						return;
					}
					return;
				}
				#endregion Jump to Name Realm
				#region Jump to X Y Z
				else if (args.Length == 5 && args[1] == "to")
				{
					client.Player.MoveTo(client.Player.CurrentRegionID, Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]), client.Player.Heading);
					return;
				}
				#endregion Jump to X Y Z
				#region Jump rel +/-X +/-Y +/-Z
				else if (args.Length == 5 && args[1] == "rel")
				{
					client.Player.MoveTo(client.Player.CurrentRegionID,
										 client.Player.X + Convert.ToInt32(args[2]),
										 client.Player.Y + Convert.ToInt32(args[3]),
										 client.Player.Z + Convert.ToInt32(args[4]),
										 client.Player.Heading);
					return;
				}
				#endregion Jump rel +/-X +/-Y +/-Z
				#region Jump to X Y Z RegionID
				else if (args.Length == 6 && args[1] == "to")
				{
					if (CheckExpansion(client, client, (ushort)Convert.ToUInt16(args[5])))
					{
						client.Player.MoveTo(Convert.ToUInt16(args[5]), Convert.ToInt32(args[2]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]), client.Player.Heading);
						return;
					}
					return;
				}
				#endregion Jump to X Y Z RegionID
				#region Jump PlayerName to X Y Z
				else if (args.Length == 6 && args[2] == "to")
				{
					GameClient clientc;
					clientc = WorldMgr.GetClientByPlayerName(args[1], false, true);
					if (clientc == null)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.PlayerIsNotInGame", args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					clientc.Player.MoveTo(clientc.Player.CurrentRegionID, Convert.ToInt32(args[3]), Convert.ToInt32(args[4]), Convert.ToInt32(args[5]), clientc.Player.Heading);
					return;
				}
				#endregion Jump PlayerName to X Y Z
				#region Jump PlayerName to X Y Z RegionID
				else if (args.Length == 7 && args[2] == "to")
				{
					GameClient clientc;
					clientc = WorldMgr.GetClientByPlayerName(args[1], false, true);
					if (clientc == null)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.PlayerIsNotInGame", args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (CheckExpansion(clientc, clientc, (ushort)Convert.ToUInt16(args[6])))
					{
						clientc.Player.MoveTo(Convert.ToUInt16(args[6]), Convert.ToInt32(args[3]), Convert.ToInt32(args[4]), Convert.ToInt32(args[5]), clientc.Player.Heading);
						return;
					}
					return;
				}
				#endregion Jump PlayerName to X Y Z RegionID
				#region Jump PlayerName to PlayerCible
				else if (args.Length == 4 && args[2] == "to")
				{
					GameClient clientc;
					GameClient clientto;
					clientc = WorldMgr.GetClientByPlayerName(args[1], false, true);
					if (clientc == null)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.PlayerIsNotInGame", args[1]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					if (args[3] == "me")
					{
						clientto = client;
					}
					else
					{
						clientto = WorldMgr.GetClientByPlayerName(args[3], false, false);
					}

					if (clientto == null)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.PlayerIsNotInGame", args[3]), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					else
					{
						if (CheckExpansion(clientto, clientc, clientto.Player.CurrentRegionID))
						{
							if (clientto.Player.CurrentHouse != null && clientto.Player.InHouse)
								clientto.Player.CurrentHouse.Enter(clientc.Player);
							else
								clientc.Player.MoveTo(clientto.Player.CurrentRegionID, clientto.Player.X, clientto.Player.Y, clientto.Player.Z, client.Player.Heading);
							return;
						}
						return;
					}
				}
				#endregion Jump PlayerName to PlayerCible
				#region push/pop
				else if (args.Length > 1 && args[1] == "push")
				{
					Stack<GameLocation> locations;

					locations = client.Player.TempProperties.getProperty<object>(TEMP_KEY_JUMP, null) as Stack<GameLocation>;

					if (locations == null)
					{
						locations = new Stack<GameLocation>(3);
						client.Player.TempProperties.setProperty(TEMP_KEY_JUMP, locations);
					}

					locations.Push(new GameLocation("temploc", client.Player.CurrentRegionID, client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading));

					string message = LanguageMgr.GetTranslation(client, "GMCommands.Jump.Pushed");

					if (locations.Count > 1)
						message += " " + LanguageMgr.GetTranslation(client, "GMCommands.Jump.PushedTotal", locations.Count);

					message += " - " + LanguageMgr.GetTranslation(client, "GMCommands.Jump.PopInstructions");

					client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (args.Length > 1 && args[1] == "pop")
				{
					Stack<GameLocation> locations;

					locations = client.Player.TempProperties.getProperty<object>(TEMP_KEY_JUMP, null) as Stack<GameLocation>;

					if (locations == null || locations.Count < 1)
					{
						client.Out.SendMessage(LanguageMgr.GetTranslation(client, "GMCommands.Jump.NothingPushed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					GameLocation jumploc = locations.Pop();

					// slight abuse of the stack principle, but convenient to always have your last popped loc jumpable
					if (locations.Count < 1)
						locations.Push(jumploc);

					client.Player.MoveTo(jumploc.RegionID, jumploc.X, jumploc.Y, jumploc.Z, jumploc.Heading);
				}
				#endregion push/pop
				#region DisplaySyntax
				else
				{
					DisplaySyntax(client);
					return;
				}
				#endregion DisplaySyntax
			}

			catch (Exception ex)
			{
				DisplayMessage(client, ex.Message);
			}
		}

		public bool CheckExpansion(GameClient clientJumper, GameClient clientJumpee, ushort RegionID)
		{
			Region reg = WorldMgr.GetRegion(RegionID);
			if (reg != null && reg.Expansion > (int)clientJumpee.ClientType)
			{
				clientJumper.Out.SendMessage(LanguageMgr.GetTranslation(clientJumper, "GMCommands.Jump.CheckExpansion.CannotJump", clientJumpee.Player.Name, reg.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (clientJumper != clientJumpee)
					clientJumpee.Out.SendMessage(LanguageMgr.GetTranslation(clientJumpee, "GMCommands.Jump.CheckExpansion.ClientNoSup", clientJumper.Player.Name, reg.Description), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
	}
}