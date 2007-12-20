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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&gu",
		new string[] {"&guild"},
		ePrivLevel.Player,
		"Guild Chat command",
		"/gu <text>")]
	public class GuildChatCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
				client.Out.SendMessage("You don't belong to a player guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.GcSpeak))
			{
				client.Out.SendMessage("You don't have permission to speak on the on guild line.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string message = "[Guild] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			client.Player.Guild.SendMessageToGuildMembers(message, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
			return 1;
		}
	}

	[CmdAttribute(
		"&o",
		new string[] {"&osend"},
		ePrivLevel.Player,
		"Speak in officer chat (Must be a guild officer)",
		"/o <text>")]
	public class OfficerGuildChatCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
				client.Out.SendMessage("You don't belong to a player guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.OcSpeak))
			{
				client.Out.SendMessage("You don't have permission to speak on the officer line.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string message = "[Officers] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			foreach (GamePlayer ply in client.Player.Guild.ListOnlineMembers())
			{
				if (!client.Player.Guild.GotAccess(ply, eGuildRank.OcHear))
				{
					continue;
				}
				ply.Out.SendMessage(message, eChatType.CT_Officer, eChatLoc.CL_ChatWindow);
			}
			return 1;
		}
	}

	[CmdAttribute(
		"&as",
		new string[] {"&asend"},
		ePrivLevel.Player,
		"Sends a message to the alliance chat",
		"/as <text>")]
	public class AllianceGuildChatCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
				client.Out.SendMessage("You don't belong to a player guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (client.Player.Guild.alliance == null)
			{
				client.Out.SendMessage("Your guild doesn't belong to any alliance.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (!client.Player.Guild.GotAccess(client.Player, eGuildRank.AcSpeak))
			{
				client.Out.SendMessage("You can not speak on alliance chan.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string message = "[Alliance] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			foreach (Guild gui in client.Player.Guild.alliance.Guilds)
			{
				foreach (GamePlayer ply in gui.ListOnlineMembers())
				{
					if (!gui.GotAccess(ply, eGuildRank.AcHear))
					{
						continue;
					}
					ply.Out.SendMessage(message, eChatType.CT_Alliance, eChatLoc.CL_ChatWindow);
				}
			}
			return 1;
		}
	}
}