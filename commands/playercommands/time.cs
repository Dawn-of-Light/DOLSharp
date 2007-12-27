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
	[CmdAttribute(
		"&time",
		ePrivLevel.Player,
		"time in game",
		"/time")]
	public class TimeCommandHandler : ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			// starts a new day with 'speed' at 'time'(1/1000)
			// /time speed time
			if (client.Account.PrivLevel > 1)
			{
				if (args.Length == 3)
				{
					uint speed = 0;
					uint time = 0;
					try
					{
						speed = Convert.ToUInt32(args[1]);
						time = Convert.ToUInt32(args[2]);
					}
					catch
					{
						client.Out.SendMessage(
							"Usage: /time <speed> <time>",
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					WorldMgr.StartDay(speed, time * (77760000 / 1000));
					return;
					//78643200 > 
					//77929920 >
					//myTimer = new Timer(77760000/timeadd);
				}
			}
			//1000/60/54 to be like 77760 = 54*60*24
			// so for mythic day contain 24 hours each hours contain 60 minutes and each minute contain 54 seconde ;)
			uint hour = WorldMgr.GetCurrentDayTime() / 1000 / 60 / 60;
			uint minute = WorldMgr.GetCurrentDayTime() / 1000 / 60 % 60;
			uint seconde = WorldMgr.GetCurrentDayTime() / 1000 % 60;

			client.Out.SendMessage("it is " + hour.ToString() + "H" + minute.ToString() + "min" + seconde.ToString() + "sec",
								   eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}