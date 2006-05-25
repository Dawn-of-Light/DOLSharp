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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&speed",
		(uint) ePrivLevel.GM,
		"Change speed",
		"/speed <newSpeed>")]
	public class SpeedCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /speed <number>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}
			ushort speed;
			try
			{
				speed = Convert.ToUInt16(args[1]);

				GamePlayer obj = client.Player.TargetObject as GamePlayer;

				if (obj != null)
				{
					obj.MaxSpeedBase = speed;
					obj.Out.SendUpdateMaxSpeed();
					obj.Out.SendMessage((client.Player == obj ? "Your" : obj.Name) + " maximum speed is now " + speed.ToString(),
					                    eChatType.CT_System,
					                    eChatLoc.CL_SystemWindow);
				}
				else
				{
					client.Out.SendMessage("You have not selected a valid target", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage("Usage: /speed <number>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}