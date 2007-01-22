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
	[CmdAttribute(
		"&autoloot",
		(uint)ePrivLevel.Player,
		"automaticly pick up any loot that drops in your area",
		"/autoloot <on/off>")]
	public class AutolootCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return 0;
			}

			if (args[1].ToLower().Equals("on"))
			{
				client.Player.Autoloot = true;
				client.Out.SendMessage("Your autoloot is on. You will now attempt to automaticly loot anything that drops in your area.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			else if (args[1].ToLower().Equals("off"))
			{
				client.Player.Autoloot = false;
				client.Out.SendMessage("Your autoloot is off. You will no longer attempt to automaticly loot anything that drops in your area.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}