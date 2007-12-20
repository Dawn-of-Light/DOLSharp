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
	[CmdAttribute("&Noqueue", //command to handle
	ePrivLevel.Player, //minimum privelege level
	"Allows you to disable/enable queuing", "/Noqueue")] //usage
	public class NoqueueCommandHandler : ICommandHandler
	{

		public int OnCommand(GameClient client, string[] args)
		{
			client.Player.SpellQueue = !client.Player.SpellQueue;

			if (client.Player.SpellQueue)
			{
				client.Out.SendMessage("You are now using the queuing option! To disable queuing use '/noqueue'.",
					   eChatType.CT_System,
					   eChatLoc.CL_SystemWindow);
			}
			else
			{
				client.Out.SendMessage("You are no longer using the queuing option! To enable queuing use '/noqueue'.",
					   eChatType.CT_System,
					   eChatLoc.CL_SystemWindow);

			}
			return 1;
		}
	}
}