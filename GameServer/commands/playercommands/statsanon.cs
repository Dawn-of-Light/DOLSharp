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

// By Daeli
using System;
using log4net;

using DOL.GS;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	"&stats",
	ePrivLevel.Player,
	"Hides your statistics",
	"/statsanon")]
	public class StatsAnonHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client == null)
				return;

			string msg;

			client.Player.StatsAnonFlag = !client.Player.StatsAnonFlag;
			if (client.Player.StatsAnonFlag)
				msg = "Your stats are no longer visible to other players.";
			else
				msg = "Your stats are now visible to other players.";

			client.Player.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_ChatWindow);
		}
	}
}