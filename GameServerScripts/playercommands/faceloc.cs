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
 * Rev:		$Id: faceloc.cs,v 1.8 2006/01/16 16:52:20 doulbousiouf Exp $
 * 
 * Desc:	Implements /faceloc command
 * 
 */
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&faceloc",
		(uint) ePrivLevel.Player,
		"Turns and faces your character into the direction of the x, y coordinates provided (using Mythic zone coordinates).",
		"/faceloc [x] [y]")]
	public class LocFaceCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Mez || client.Player.Stun)
				return 1;

			if (args.Length < 3)
			{
				client.Out.SendMessage
					(
					"Please enter X and Y coordinates.",
					eChatType.CT_System,
					eChatLoc.CL_SystemWindow
					);
				return 1;
			}
			int x = System.Convert.ToInt32(args[1]);
			int y = System.Convert.ToInt32(args[2]);
			Zone currentZone = client.Player.Region.GetZone(client.Player.Position);
			if(currentZone == null) return 0;
			int Xoffset = currentZone.XOffset;
			int Yoffset = currentZone.YOffset;
			int glocX = Xoffset + x;
			int glocY = Yoffset + y;
			ushort direction = client.Player.Position.GetHeadingTo(new Point(glocX, glocY, 0));
			client.Player.Heading = direction;
			client.Out.SendPlayerJump(true);
			return 1;
		}
	}
}