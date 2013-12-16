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
/* The /played command
      /played returns the total time a character has been
      played.  Script and core changes by Echostorm 
	  with thanks to Smallhorse for guidance.*/

using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&played",
		ePrivLevel.Player,
		"Returns the age of the character",
		"/played")]
	public class PlayedCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "played"))
				return;

			int yearsPlayed = 0;
			int monthsPlayed = 0;
			TimeSpan showPlayed = TimeSpan.FromSeconds(client.Player.PlayedTime);
			int daysPlayed = showPlayed.Days;
			// Figure Years
			if (showPlayed.Days >= 365)
			{
				yearsPlayed = daysPlayed/365;
				daysPlayed -= yearsPlayed*365;
			}
			// Figure Months (roughly)
			if (showPlayed.Days >= 30)
			{
				monthsPlayed = daysPlayed/30;
				daysPlayed -= monthsPlayed*30;
			}

			client.Out.SendMessage("You have played for " + yearsPlayed + " Years, " + monthsPlayed + " Months, " + daysPlayed + " Days, " + showPlayed.Hours + " Hours and " + showPlayed.Minutes + " Minutes.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}