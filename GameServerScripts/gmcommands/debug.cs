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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&debug",
		(uint) ePrivLevel.GM,
		"activate or deactivate debug command",
		"/debug <on/off>")]
	public class DebugCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				client.Out.SendMessage("Usage: /debug {on/off}", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 0;
			}
			if (args[1].ToLower().Equals("on"))
			{
				client.Player.TempProperties.setProperty(GamePlayer.DEBUG_MODE_PROPERTY, this);
				client.Out.SendDebugMode(true);
				client.Out.SendMessage("debug mode ON", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else if (args[1].ToLower().Equals("off"))
			{
				client.Player.TempProperties.removeProperty(GamePlayer.DEBUG_MODE_PROPERTY);
				client.Out.SendDebugMode(false);
				client.Out.SendMessage("debug mode OFF", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}