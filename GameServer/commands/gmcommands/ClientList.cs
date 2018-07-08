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
using System.Collections.Generic;

namespace DOL.GS.Commands
{
    [Cmd(
        "&clientlist",
        ePrivLevel.GM,
        "Usage: /clientlist [full] - full option includes IP's and accounts",
        "Show a list of currently playing clients and their ID's")]
    public class ClientListCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            var clients = WorldMgr.GetAllPlayingClients();
            var message = new List<string>();

            foreach (GameClient gc in clients)
            {
                if (gc.Player != null)
                {
                    if (args.Length > 1 && args[1].ToLower() == "full")
                    {
                        message.Add("(" + gc.SessionID + ") " + gc.TcpEndpointAddress + ", " + gc.Account.Name + ", " + gc.Player.Name + " " + gc.Version);
                    }
                    else
                    {
                        message.Add("(" + gc.SessionID + ") " + gc.Player.Name);
                    }
                }
            }

            client.Out.SendCustomTextWindow("[ Playing Client List ]", message);
            return;
        }
    }
}
