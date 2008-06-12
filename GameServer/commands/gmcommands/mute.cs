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
		"Command to mute annoying players.",
        "/mute <playername>")]
    public class MuteCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
				DisplaySyntax(client);
                return;
            }
			GameClient clientc = WorldMgr.GetClientByPlayerName(args[1], true, false);

			if (clientc == null)
			{
				DisplayMessage(client, "No player found for name '" + args[1] + "'");
				return;
			}

			clientc.Player.IsMuted = !clientc.Player.IsMuted;
			if (clientc.Player.IsMuted)
			{
				clientc.Player.Out.SendMessage("You have been muted from public channels by staff member " + client.Player.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.Out.SendMessage("You have muted player " + clientc.Player.Name + " from public channels!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				clientc.Player.Out.SendMessage("You have been unmuted from public channels by staff member " + client.Player.Name + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.Out.SendMessage("You have unmuted player " + clientc.Player.Name + " from public channels!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
            return;
        }
    }
}
