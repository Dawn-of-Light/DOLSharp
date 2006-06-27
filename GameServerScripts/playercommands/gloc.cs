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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&gloc", //command to handle
		(uint) ePrivLevel.Player, //minimum privelege level
		"Show the current coordinates", //command description
		"/gloc")] //command usage
		public class GlocCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			client.Out.SendMessage(
				"You are at Pos:" + client.Player.Position.ToString()
					+ " Heading:" + client.Player.Heading
					+ " Region:" + client.Player.RegionId,
				eChatType.CT_System,
				eChatLoc.CL_SystemWindow);
			//Command handled fine
			return 1;
		}
	}
}