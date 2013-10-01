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
		"&group",
		new string[] {"&g"},
		ePrivLevel.Player,
		"Say something to other chat group players",
		"/g <message>")]
	public class GCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (client.Player.Group == null)
			{
				DisplayMessage(client, "You are not part of a group");
				return;
			}

			if (IsSpammingCommand(client.Player, "group", 500))
			{
				DisplayMessage(client, "Slow down! Think before you say each word!");
				return;
			}

			if (args.Length >= 2)
			{
				string msg = "";
				for (int i = 1; i < args.Length; ++i)
				{
					msg += args[i] + " ";
				}

				client.Player.Group.SendMessageToGroupMembers(client.Player, msg, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
			}
			else
			{
				DisplaySyntax(client);
			}
		}
	}
}