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
using DOL.Language;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&nohelp",
		(uint) ePrivLevel.Player,
		"Toggle nohelp on or off, to stop receiving help from  your realm", "/afk <text>")]
	public class NoHelpCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			client.Player.NoHelp = !client.Player.NoHelp;

			if (client.Player.NoHelp)
			{
				client.Out.SendMessage("You will no longer receive help from members of your realm, type /nohelp again to receive help again.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Out.SendMessage("You will once again receive help from members of your realm, type /nohelp again to stop receiving help.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			return 1;
		}
	}
}
