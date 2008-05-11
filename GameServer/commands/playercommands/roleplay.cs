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

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&roleplay",
		ePrivLevel.Player,
	   "Flags a player with an  tag to indicate the player is a role player.",
	   "/roleplay on/off")]
	public class RolePlayCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			if (args[1].ToLower().Equals("on"))
			{
				client.Player.RPFlag = true;
				client.Out.SendMessage("Your roleplay flag is ON. You will now be flagged as a roleplayer. Use '/roleplay off' to turn the flag off.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			else if (args[1].ToLower().Equals("off"))
			{
				client.Player.RPFlag = false;
				client.Out.SendMessage("Your roleplay flag is OFF. You will be flagged as a roleplayer. Use '/roleplay on' to turn the flag back on.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
		}
	}
}