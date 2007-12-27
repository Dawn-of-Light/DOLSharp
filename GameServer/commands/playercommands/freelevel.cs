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
	[CmdAttribute("&freelevel", //command to handle
		ePrivLevel.Player, //minimum privelege level
		"Display state of FreeLevel", //command description
		"/freelevel")] //command usage
	public class FreelevelCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			//flag 1 = above level, 2 = elligable, 3= time until, 4 = level and time until, 5 = level until
			byte state = client.Player.FreeLevelState;
			string message = "";

			if (args.Length == 2 && args[1] == "decline")
			{
				if (state == 2)
				{
					// NOT SURE FOR THIS MESSAGE
					message = "Your FreeLevel has been removed.";
					// we decline THIS ONE, but next level, we will gain another freelevel !!
					client.Player.PlayerCharacter.LastFreeLevel = client.Player.Level - 1;
					client.Player.Out.SendPlayerFreeLevelUpdate();
				}
				else
				{
					// NOT SURE FOR THIS MESSAGE
					message = "You don't have any FreeLevel !";
				}
				client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			switch (state)
			{
				case 1:
					message = "You are above the maximum level to obtain a free level.";
					break;
				case 2:
					message = "You are eligible for a free level! Click on your trainer to receive it (or type /freelevel decline to discard your free level).";
					break;
				case 3:
					TimeSpan t = (client.Player.PlayerCharacter.LastFreeLeveled.AddDays(7) - DateTime.Now);
					// NOT SURE FOR THIS MESSAGE
					message = "You will be eligible for a free level in : " + t.Days + " days " + t.Hours + " hours " + t.Minutes + " minutes.";
					break;
				case 4:
					TimeSpan t2 = (client.Player.PlayerCharacter.LastFreeLeveled.AddDays(7) - DateTime.Now);
					// NOT SURE FOR THIS MESSAGE
					message = "You will be eligible for a free level after your next level, and  in : " + t2.Days + " days " + t2.Hours + " hours " + t2.Minutes + " minutes.";
					break;
				case 5:
					message = "You will be eligible for a free level as soon as you obtain a level.";
					break;

			}
			client.Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}