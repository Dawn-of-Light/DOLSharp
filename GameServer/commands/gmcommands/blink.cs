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
		"&blink",
		ePrivLevel.GM,
		"Make me blink",
		"/blink <id>: type /blink for a list of possible IDs")]
	public class BlinkCommandHandler : ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			GamePlayer player = client.Player;

			if (args.Length > 1)
			{
				byte value;
				if (byte.TryParse(args[1].ToLower(), out value))
				{
					if (Enum.IsDefined(typeof(ePanel), value))
					{
						client.Out.SendMessage("Start blinking UI part: " + Enum.GetName(typeof(ePanel), value), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						player.Out.SendBlinkPanel(player, value);
					}
				}
			}
			else
			{
				Usage(client);
			}
		}

		private void Usage(GameClient client)
		{
			String visualEffectList = "";

			visualEffectList += "You must specify a value!\nID: Name\n";

			int count = 0;

			foreach (string panelID in Enum.GetNames(typeof(ePanel)))
			{
				visualEffectList += count + ": " + panelID + "\n";
				count++;
			}
			client.Out.SendMessage(visualEffectList, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}
	}
}