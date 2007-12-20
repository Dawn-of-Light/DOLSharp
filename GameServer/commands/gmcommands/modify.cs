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
		"&modify",
		ePrivLevel.GM,
		"Modifies the targeted object",
		"/modify <args>")]
	public class ModifyCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /modify <args>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}

			try
			{
				GameObject obj = (GameObject) client.Player.TargetObject;

				if (obj != null)
				{
					obj.Mod(args);
				}
				else
				{
					client.Out.SendMessage("You have not selected a valid target", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception e)
			{
				client.Out.SendMessage(e.Message,
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				client.Out.SendMessage(e.StackTrace,
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}

			return 1;
		}
	}
}