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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&gu",
		new string[] {"&guild"},
		ePrivLevel.Player,
		"Guild Chat command",
		"/gu <text>")]
	public class GuildChatCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoGuild"));
                return;
			}

			if (!client.Player.Guild.HasRank(client.Player, Guild.eRank.GcSpeak))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoGuildPermission"));
                return;
			}

			if (IsSpammingCommand(client.Player, "guildchat", 500))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.Spamming.Say"));
                return;
			}

			string message = "[Guild] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			client.Player.Guild.SendMessageToGuildMembers(message, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
		}
	}

	[CmdAttribute(
		"&o",
		new string[] {"&osend"},
		ePrivLevel.Player,
		"Speak in officer chat (Must be a guild officer)",
		"/o <text>")]
	public class OfficerGuildChatCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoGuild"));
                return;
			}

			if (!client.Player.Guild.HasRank(client.Player, Guild.eRank.OcSpeak))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoOfficerPermission"));
                return;
			}

			if (IsSpammingCommand(client.Player, "osend", 500))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "GamePlayer.Spamming.Say"));
                return;
			}

			string message = "[Officers] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			foreach (GamePlayer ply in client.Player.Guild.GetListOfOnlineMembers())
			{
				if (!client.Player.Guild.HasRank(ply, Guild.eRank.OcHear))
				{
					continue;
				}
				ply.Out.SendMessage(message, eChatType.CT_Officer, eChatLoc.CL_ChatWindow);
			}
		}
	}

	[CmdAttribute(
		"&as",
		new string[] {"&asend"},
		ePrivLevel.Player,
		"Sends a message to the alliance chat",
		"/as <text>")]
	public class AllianceGuildChatCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Guild == null)
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoGuild"));
				return;
			}

			if (client.Player.Guild.alliance == null)
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoAlliance"));
                return;
			}

			if (!client.Player.Guild.HasRank(client.Player, Guild.eRank.AcSpeak))
			{
                DisplayMessage(client, LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.NoAlliancePermission"));
                return;
			}

			if (client.Player.IsMuted)
			{
                client.Player.Out.SendMessage(LanguageMgr.GetTranslation(client.Account.Language, "Scripts.Players.Guildchat.AllianceMuted"), eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
                return;
			}

			string message = "[Alliance] " + client.Player.Name + ": \"" + string.Join(" ", args, 1, args.Length - 1) + "\"";
			foreach (Guild gui in client.Player.Guild.alliance.Guilds)
			{
				foreach (GamePlayer ply in gui.GetListOfOnlineMembers())
				{
					if (!gui.HasRank(ply, Guild.eRank.AcHear))
					{
						continue;
					}
					ply.Out.SendMessage(message, eChatType.CT_Alliance, eChatLoc.CL_ChatWindow);
				}
			}
		}
	}
}