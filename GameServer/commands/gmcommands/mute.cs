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
	[Cmd(
		"&mute",
		ePrivLevel.GM,
		"Command to mute annoying players.  Player mutes are temporary, account mutes are set until another account mute command turns it off.",
        "/mute [account] <playername>")]
    public class MuteCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
				DisplaySyntax(client);
                return;
            }

			GameClient playerClient = null;
			bool mutedAccount = false;

			if (args[1].ToLower() == "account")
			{
				if (args.Length < 3)
				{
					DisplaySyntax(client);
					return;
				}

				playerClient = WorldMgr.GetClientByPlayerName(args[2], true, false);

				if (playerClient != null)
				{
					playerClient.Account.IsMuted = !playerClient.Account.IsMuted;
					playerClient.Player.IsMuted = playerClient.Account.IsMuted;
					GameServer.Database.SaveObject(playerClient.Account);
					mutedAccount = true;
				}
			}
			else
			{
				playerClient = WorldMgr.GetClientByPlayerName(args[1], true, false);
				if (playerClient != null)
				{
					if (playerClient.Account.IsMuted)
					{
						DisplayMessage(client, "This player has an account mute which must be removed first.");
						return;
					}

					playerClient.Player.IsMuted = !playerClient.Player.IsMuted;
				}
			}

			if (playerClient == null)
			{
				DisplayMessage(client, "No player found for name '" + args[1] + "'");
				return;
			}

			if (playerClient.Player.IsMuted)
			{
				playerClient.Player.Out.SendMessage("You have been muted from public channels by staff member " + client.Player.Name + "!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				client.Player.Out.SendMessage("You have muted player " + playerClient.Player.Name + " from public channels!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				if (mutedAccount)
				{
					playerClient.Player.Out.SendMessage("This mute has been placed on all characters for this account.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
					client.Player.Out.SendMessage("This action was done to the players account.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				playerClient.Player.Out.SendMessage("You have been unmuted from public channels by staff member " + client.Player.Name + "!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				client.Player.Out.SendMessage("You have unmuted player " + playerClient.Player.Name + " from public channels!", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				if (mutedAccount)
				{
					client.Player.Out.SendMessage("This action was done to the players account.", eChatType.CT_Staff, eChatLoc.CL_SystemWindow);
				}
			}
            return;
        }
    }
}
