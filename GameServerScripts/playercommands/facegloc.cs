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
/*
 * Author:	Ogre <ogre@videogasm.com>
 * Rev:		$Id: facegloc.cs,v 1.10 2006/01/16 16:52:20 doulbousiouf Exp $
 * 
 * Desc:	Implements /facegloc command
 * 
 */
using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&facegloc",
		(uint) ePrivLevel.Player,
		"Turns and faces your character into the direction of the x, y coordinates provided (using DOL region global coordinates).",
		"/facegloc [x] [y]")]
	public class GLocFaceCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Mez || client.Player.Stun)
				return 1;

			if (args.Length < 3)
			{
				client.Out.SendMessage("Please enter X and Y coordinates.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			Point pos;
			try
			{
				pos = new Point(int.Parse(args[1]), int.Parse(args[2]), 0);
			}
			catch (Exception)
			{
				client.Out.SendMessage("Please enter X and Y coordinates.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			ushort direction = client.Player.Position.GetHeadingTo(pos);
			client.Player.Heading = direction;
			client.Out.SendPlayerJump(true);
			return 1;
		}
	}
}