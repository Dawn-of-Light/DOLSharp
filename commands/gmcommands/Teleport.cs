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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Utils;
using DOL.GS.Quests;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS.Commands
{
    /// <summary>
    /// A command to manage teleport destinations.
    /// </summary>
    /// <author>Aredhel</author>
	[CmdAttribute(
		"&teleport",
		ePrivLevel.GM,
        "Manage the teleport destinations",
        "'/teleport add' add a teleport destination")]
    public class TeleportCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
            if (args.Length < 2)
            {
                DisplaySyntax(client);
                return;
            }

            switch (args[1].ToLower())
            {
                case "add":
                    {
                        if (args.Length != 4)
                        {
                            DisplaySyntax(client);
                            return;
                        }

                        if (args[2] == "")
                        {
                            client.Out.SendMessage("You must specify a destination ID.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        String result;

                        if (AddTeleport(args[2], client.Player.Realm, client.Player.CurrentRegion.ID,
                            client.Player.X, client.Player.Y, client.Player.Z, client.Player.Heading,
                            args[3]))
                            result = String.Format("Teleport [{0}] added to the database.", args[2]);
                        else
                            result = String.Format("Failed to add [{0}] to the database!", args[2]);

                        client.Out.SendMessage(result, eChatType.CT_System, eChatLoc.CL_SystemWindow);

                    }
                    break;
                default:
                    DisplaySyntax(client);
                    break;
            }    
        }

        private bool AddTeleport(String teleportID, eRealm realm, ushort regionID, int x, int y, int z,
            ushort heading, String type)
        {
            Teleport teleport = new Teleport();
            teleport.TeleportID = teleportID;
            teleport.Realm = (int)realm;
            teleport.RegionID = regionID;
            teleport.X = x;
            teleport.Y = y;
            teleport.Z = z;
            teleport.Heading = heading;
            teleport.Type = type;

            GameServer.Database.AddNewObject(teleport);
            return true;
        }
    }
}
