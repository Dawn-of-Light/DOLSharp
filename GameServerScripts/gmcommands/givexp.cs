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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&givexp",
		(uint) ePrivLevel.GM,
		"Gives XP to your target",
		"/givexp <ammount>")]
	public class GiveXPCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /givexp <amount>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}

			long amount;

			try
			{
				amount = long.Parse(args[1]);
				GamePlayer obj = client.Player.TargetObject as GamePlayer;
				if (obj != null)
				{
					obj.GainExperience(amount, 0, 0, true);
				}
				else
				{
					client.Out.SendMessage("You have not selected a valid target", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage("Usage: /givexp <amount>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}