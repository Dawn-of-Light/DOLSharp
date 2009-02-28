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
        "Manage teleport destinations",
        "'/teleport add <ID> <type>' add a teleport destination")]
    public class TeleportCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        /// <summary>
        /// Handle command.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="args"></param>
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
                            client.Out.SendMessage("You must specify a teleport ID.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                            return;
                        }

                        AddTeleport(client, args[2], args[3]);
                    }
                    break;
                default:
                    DisplaySyntax(client);
                    break;
            }    
        }

        /// <summary>
        /// Add a new teleport destination in memory and save to database, if
        /// successful.
        /// </summary>
        /// <param name="client"></param>
        /// <param name="teleportID"></param>
        /// <param name="type"></param>
        private void AddTeleport(GameClient client, String teleportID, String type)
        {
            GamePlayer player = client.Player;
            eRealm realm = player.Realm;

            if (WorldMgr.GetTeleportLocation(realm, String.Format("{0}:{1}", type, teleportID)) != null)
            {
                client.Out.SendMessage(String.Format("Teleport ID [{0}] already exists!", teleportID), 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            Teleport teleport = new Teleport();
            teleport.TeleportID = teleportID;
            teleport.Realm = (int)realm;
            teleport.RegionID = player.CurrentRegion.ID;
            teleport.X = player.X;
            teleport.Y = player.Y;
            teleport.Z = player.Z;
            teleport.Heading = player.Heading;
            teleport.Type = type;

            if (!WorldMgr.AddTeleportLocation(teleport))
            {
                client.Out.SendMessage(String.Format("Failed to add teleport ID [{0}] in memory!", teleportID), 
                    eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return;
            }

            GameServer.Database.AddNewObject(teleport);
        }
    }
}
