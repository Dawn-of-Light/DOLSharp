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
		"&walk",
		ePrivLevel.GM,
		"Commands a npc to walk!",
		"'/walk <xoff> <yoff> <zoff> <speed>' to make the npc walk to x+xoff, y+yoff, z+zoff")]
	[CmdAttribute(
		"&stop",
		ePrivLevel.GM,
		"Stops the npc's movement!",
		"'/stop' to stop the target mob")]
	public class WalkCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			GameNPC targetNPC = null;
			if (client.Player.TargetObject != null && client.Player.TargetObject is GameNPC)
				targetNPC = (GameNPC) client.Player.TargetObject;

			if (args.Length == 1 && args[0] == "&stop")
			{
				if (targetNPC == null)
				{
					client.Out.SendMessage("Type /stop to stop your target npc from moving", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				targetNPC.StopMoving();
				return;
			}
			if (args.Length < 4)
			{
				client.Out.SendMessage("Usage:", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("'/walk <xoff> <yoff> <zoff> <speed>'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (targetNPC == null)
			{
				client.Out.SendMessage("Type /walk for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			int xoff = 0;
			int yoff = 0;
			int zoff = 0;
			short speed = 50;

			try
			{
				xoff = Convert.ToInt16(args[1]);
				yoff = Convert.ToInt16(args[2]);
				zoff = Convert.ToInt16(args[3]);
				speed = Convert.ToInt16(args[4]);
			}
			catch (Exception)
			{
				client.Out.SendMessage("Type /walk for command overview", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			targetNPC.WalkTo((targetNPC.X + xoff), (targetNPC.Y + yoff), (targetNPC.Z + zoff), speed);
		}
	}
}