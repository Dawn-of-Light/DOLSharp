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
using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Housing;

namespace DOL.GS.Commands
{
	[CmdAttribute("&knock", //command to handle
		ePrivLevel.Player, //minimum privelege level
	   "Knock on a house", //command description
		"/knock")] //command usage
	public class KnockCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public const string PLAYER_KNOCKED = "player_knocked_weak";
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.CurrentHouse != null)
			{
				DisplayMessage(client, "You can't knock while your in a house!");
				return;
			}

			long KnockTick = client.Player.TempProperties.getLongProperty(PLAYER_KNOCKED, 0);
			if (KnockTick > 0 && KnockTick - client.Player.CurrentRegion.Time <= 0)
			{
				client.Player.TempProperties.removeProperty(PLAYER_KNOCKED);
			}

			long changeTime = client.Player.CurrentRegion.Time - KnockTick;
			if (changeTime < 30000 && KnockTick > 0)
			{
				client.Player.Out.SendMessage("You must wait " + ((30000 - changeTime) / 1000).ToString() + " more seconds before knocking again!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			bool done = false;
			foreach (House house in HouseMgr.getHousesCloseToSpot(client.Player.CurrentRegionID, client.Player.X, client.Player.Y, 650))
			{
				client.Player.Emote(eEmote.Knock);
				foreach (GamePlayer player in house.GetAllPlayersInHouse())
				{
					string message = client.Player.Name + " is at your door";
					player.Out.SendMessage(message + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				done = true;
			}

			if (done)
			{
				client.Out.SendMessage("You knock on the door.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Player.TempProperties.setProperty(PLAYER_KNOCKED, client.Player.CurrentRegion.Time);
			}
			else client.Out.SendMessage("You must go to the house you wish to knock on!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}