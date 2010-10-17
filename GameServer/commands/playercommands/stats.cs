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

// Eden - Darwin 06/10/2008 - Complete /stats
// Tolakram - moved most code to PlayerStatistics to enable custom stats display

using System;
using System.Reflection;
using System.Collections.Generic;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL;
using System.Collections;
using DOL.Database;
using log4net;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&stats",
		ePrivLevel.Player,
		"Displays player statistics")]

	public class StatsCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "stats"))
				return;

			if (client == null) return;

            if (args.Length > 1)
            {
                string playerName = "";

                if (args[1].ToLower() == "player")
                {
                    if (args.Length > 2)
                    {
                        playerName = args[2];
                    }
                    else
                    {
                        // try and get player name from target
                        if (client.Player.TargetObject != null && client.Player.TargetObject is GamePlayer)
                        {
                            playerName = client.Player.TargetObject.Name;
                        }
                    }
                }

                client.Player.Statistics.DisplayServerStatistics(client, args[1].ToLower(), playerName);
            }
            else
            {
                DisplayMessage(client, client.Player.Statistics.GetStatisticsMessage());
            }
		}
	}
}