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
//Jump script, created by Crystal!

namespace DOL.GS.Scripts
{
	[CmdAttribute("&jump", //command to handle
		(uint) ePrivLevel.GM, //minimum privelege level
		"Teleports a player or yourself to the specified location xp", //command description
		//Usage
		"/jump to <PlayerName>",
		"/jump to <Name> <RealmID>",
		"/jump to <x> <y> <z>",
		"/jump to <x> <y> <z> <RegionID>",
		"/jump <PlayerName> to <x> <y> <z>",
		"/jump <PlayerName> to <x> <y> <z> <RegionID>",
		"/jump <PlayerName> to <PlayerName>",
		"/jump <PlayerName> to me",
		"/jump to GT",
		"/jump rel <x> <y> <z>")]
	public class OnJump : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 3 && args[1] == "to" && (args[2] == "GT" || args[2] == "gt")) // /Jump to GT
			{
				client.Player.MoveTo((ushort)client.Player.RegionId,
				                     client.Player.GroundTarget,
				                     (ushort)client.Player.Heading);
				return 1;
			}
			if (args.Length == 3 && args[1] == "to") // /Jump to PlayerName
			{
				GameClient clientc;
				clientc = WorldMgr.GetClientByPlayerName(args[2], false);
				if (clientc == null)
				{
					client.Out.SendMessage(args[2] + " cannot be found.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				if (CheckExpansion(client, clientc, (ushort)clientc.Player.RegionId))
				{
					client.Out.SendMessage("/Jump to " + clientc.Player.Region, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Player.MoveTo((ushort)clientc.Player.RegionId, clientc.Player.Position, (ushort)client.Player.Heading);
					return 1;
				}
				return 1;
			}
			else if (args.Length == 4 && args[1] == "to") // /Jump to Name Realm
			{
				GameClient clientc;
				clientc = WorldMgr.GetClientByPlayerName(args[2], false);
				if (clientc == null)
				{
					int realm = int.Parse(args[3]);

					GameNPC[] npcs = WorldMgr.GetNPCsByName(args[2], (eRealm) realm);
					if (npcs.Length > 0)
					{
						client.Out.SendMessage("/Jump to " + npcs[0].Region.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Player.MoveTo((ushort)npcs[0].RegionId, npcs[0].Position, (ushort)npcs[0].Heading);
						return 1;
					}

					client.Out.SendMessage(args[2] + " cannot be found in realm " + realm + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				if (CheckExpansion(client, clientc, (ushort)clientc.Player.RegionId))
				{
					client.Out.SendMessage("/Jump to " + clientc.Player.Region, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Player.MoveTo((ushort)clientc.Player.RegionId, clientc.Player.Position, (ushort)client.Player.Heading);
					return 1;
				}
				return 1;
			}
			else if (args.Length == 5 && args[1] == "to") // /Jump to X Y Z
			{
				Point targetPos = new Point(int.Parse(args[2]), int.Parse(args[3]), int.Parse(args[4]));
				client.Player.MoveTo((ushort)client.Player.RegionId, targetPos, (ushort)client.Player.Heading);
				return 1;
			}
			else if (args.Length == 5 && args[1] == "rel") // /Jump rel +/-X +/-Y +/-Z
			{
				Point pos = client.Player.Position;
				pos.X += int.Parse(args[2]);
				pos.Y += int.Parse(args[3]);
				pos.Z += int.Parse(args[4]);
				client.Player.MoveTo((ushort)client.Player.RegionId, pos, (ushort)client.Player.Heading);
				return 1;
			}
			else if (args.Length == 6 && args[1] == "to") // /Jump to X Y Z RegionID
			{
				if (CheckExpansion(client, client, ushort.Parse(args[5])))
				{
					Point targetPos = new Point(int.Parse(args[2]), int.Parse(args[3]), int.Parse(args[4]));
					client.Player.MoveTo((ushort)ushort.Parse(args[5]), targetPos, (ushort)client.Player.Heading);
					return 1;
				}
				return 0;
			}
			else if (args.Length == 6 && args[2] == "to") // /Jump PlayerName to X Y Z
			{
				GameClient clientc;
				clientc = WorldMgr.GetClientByPlayerName(args[1], false);
				if (clientc == null)
				{
					client.Out.SendMessage(args[1] + " is not in the game.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				Point targetPos = new Point(int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]));
				clientc.Player.MoveTo((ushort)clientc.Player.RegionId, targetPos, (ushort)clientc.Player.Heading);
				return 1;
			}
			else if (args.Length == 7 && args[2] == "to") // /Jump PlayerName to X Y Z RegionID
			{
				GameClient clientc;
				clientc = WorldMgr.GetClientByPlayerName(args[1], false);
				if (clientc == null)
				{
					client.Out.SendMessage(args[1] + " is not in the game.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				if (CheckExpansion(clientc, clientc, ushort.Parse(args[6])))
				{
					Point targetPos = new Point(int.Parse(args[3]), int.Parse(args[4]), int.Parse(args[5]));
					clientc.Player.MoveTo((ushort)Convert.ToUInt16(args[6]), targetPos, (ushort)clientc.Player.Heading);
					return 1;
				}
				return 0;
			}
			else if (args.Length == 4 && args[2] == "to") // /Jump PlayerName to PlayerCible
			{
				GameClient clientc;
				GameClient clientto;
				clientc = WorldMgr.GetClientByPlayerName(args[1], false);
				if (clientc == null)
				{
					client.Out.SendMessage(args[1] + " is not in the game.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				if (args[3] == "me") // /Jump PlayerName to me
				{
					clientto = client;
				}
				else
				{
					clientto = WorldMgr.GetClientByPlayerName(args[3], false);
				}

				if (clientto == null)
				{
					client.Out.SendMessage(args[3] + " is not in the game.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 0;
				}
				else
				{
					if (CheckExpansion(clientto, clientc, (ushort)clientto.Player.RegionId))
					{
						clientc.Player.MoveTo((ushort)clientto.Player.RegionId, clientto.Player.Position, (ushort)client.Player.Heading);
						return 1;
					}
					return 0;
				}
			}
			else
			{
				client.Out.SendMessage("Usage : /Jump to PlayerName", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump to <Name> <RealmID>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump to X Y Z", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump to X Y Z RegionID", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump rel [-]X [-]Y [-]Z", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump to GT", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump PlayerName to PlayerCible", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump PlayerName to X Y Z", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("Usage : /Jump PlayerName to X Y Z RegionID", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("PlayerCible can be [me].", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
		}

		public bool CheckExpansion(GameClient clientJumper, GameClient clientJumpee, ushort RegionID)
		{
			Region reg = WorldMgr.GetRegion(RegionID);
			if (reg != null && reg.Expansion >= clientJumpee.ClientType)
			{
				clientJumper.Out.SendMessage(clientJumpee.Player.Name + " cannot jump to Destination region (" + reg.Description + ") because it is not supported by his/her client type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (clientJumper != clientJumpee)
					clientJumpee.Out.SendMessage(clientJumper.Player.Name + " tried to jump you to Destination region (" + reg.Description + ") but it is not supported by your client type.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}
	}
}