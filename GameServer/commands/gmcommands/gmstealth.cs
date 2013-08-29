﻿/*
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
using System.Collections.Generic;
using DOL.GS.Commands;
using DOL.GS;
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
    [CmdAttribute(
        "&gmstealth",
        ePrivLevel.GM,
        "Grants the ability to stealth to a gm/admin character",
        "/gmstealth on : turns the command on",
        "/gmstealth off : turns the command off")]
    public class GMStealthCommandHandler : AbstractCommandHandler, ICommandHandler
    {
        public void OnCommand(GameClient client, string[] args)
        {
        	if (args.Length != 2) {
        		DisplaySyntax(client);
        	}
        	else if (args[1].ToLower().Equals("on")) {

                if (client.Player.IsStealthed != true)
                {
                   client.Player.Stealth(true);
                }
        	}
            else if (args[1].ToLower().Equals("off"))
            {
                    client.Player.Stealth(false);
            }
        }
    }
}
