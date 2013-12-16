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
 */
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    /// <summary>
    /// Command handler for the /ignore command
    /// </summary>
    [CmdAttribute(
        "&ignore",
        ePrivLevel.Player,
        "Adds/Removes a player to/from your Ignorelist!",
        "/ignore <playerName>")]
    public class IgnoreCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        /// <summary>
        /// Method to handle the command and any arguments
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                string[] ignores = client.Player.DBCharacter.SerializedIgnoreList.Split(',');
                client.Out.SendCustomTextWindow("Ignore List (snapshot)", ignores);
                return;
            }

            string name = string.Join(" ", args, 1, args.Length - 1);

            int result = 0;
            GameClient fclient = WorldMgr.GuessClientByPlayerNameAndRealm(name, 0, false, out result);

            if (fclient == null)
            {
                name = args[1];
                if (client.Player.IgnoreList.Contains(name))
                {
                    client.Player.ModifyIgnoreList(name, true);
                    return;
                }
                else
                {
                    // nothing found
                    DisplayMessage(client, "No players online with that name.");
                    return;
                }
            }

            switch (result)
            {
                case 2:
                    {
                        // name not unique
                        DisplayMessage(client, "Character name is not unique.");
                        break;
                    }
                case 3: // exact match
                case 4: // guessed name
                    {
                        if (fclient == client)
                        {
                            DisplayMessage(client, "You can't add yourself!");
                            return;
                        }

                        name = fclient.Player.Name;
                        if (client.Player.IgnoreList.Contains(name))
                        {
                           client.Player.ModifyIgnoreList(name, true);
                        }
                        else
                        {
                            client.Player.ModifyIgnoreList(name, false);
                        }
                        break;
                    }
            }
        }
    }
}