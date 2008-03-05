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
using System.Collections;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&announce2",
        ePrivLevel.GM,
        "Broadcast something to all players in the DAOC",
        "/broadcast <message>")]
    public class AnnounceCommandHandler : ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            string message = string.Join(" ", args, 1, args.Length - 1);
            foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
            {
                IList textList = new ArrayList();
                switch (client.Account.PrivLevel)
                {
                    case 2:
                        {
                            textList.Add("[GM]\n" + client.Player.Name + " Broadcasts: ");
                        }
                        break;
                    case 3:
                        {
                            textList.Add("[Admin]\n" + client.Player.Name + " Broadcasts: ");
                        }
                        break;
                }
                textList.Add("");
                textList.Add(message);
                switch (client.Account.PrivLevel)
                {
                    case 2:
                        {
                            clientz.Player.Out.SendCustomTextWindow("Broadcast from GM " + client.Player.Name + "", textList);
                        }
                        break;
                    case 3:
                        {
                            clientz.Player.Out.SendCustomTextWindow("Broadcast from Admin " + client.Player.Name + "", textList);
                        }
                        break;
                    default: break;
                }

            }
        }
    }
}