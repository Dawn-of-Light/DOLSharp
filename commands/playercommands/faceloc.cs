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
 * Rev:		$Id: faceloc.cs,v 1.6 2005/05/10 13:36:38 noret Exp $
 *
 * Desc:	Implements /faceloc command
 *
 */
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&faceloc",
		ePrivLevel.Player,
		"Turns and faces your character into the direction of the x, y coordinates provided (using Mythic zone coordinates).",
		"/faceloc [x] [y]")]
	public class LocFaceCommandHandler : AbstractCommandHandler,ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (client.Player.IsTurningDisabled)
			{
				DisplayError(client, "You can't use this command now!", new object[] { });
				return 0;
			}
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
			int x = 0;
			int y = 0;
			try
			{
				x = System.Convert.ToInt32(args[1]);
				y = System.Convert.ToInt32(args[2]);
			}
			catch {}
			int Xoffset = client.Player.CurrentZone.XOffset;
			int Yoffset = client.Player.CurrentZone.YOffset;
			int glocX = Xoffset + x;
			int glocY = Yoffset + y;
			ushort direction = client.Player.GetHeadingToSpot(glocX, glocY);
			client.Player.Heading = direction;
			client.Out.SendPlayerJump(true);
			return 1;
		}
	}
}