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
// Give Respec script by Echostorm
// Utility for GMs to set target players respecs.

using System;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&giverespec",
		ePrivLevel.GM,
		"Gives your target respecs",
		"/giverespec <full|single> <amount>")]
	public class GiveRespecCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				client.Out.SendMessage("Usage: /giverespec <full|single> <amount>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}


			try
			{
				int amount = Convert.ToInt32(args[2]);
				if (amount < 1)
				{
					client.Out.SendMessage("Amount can't be less than 1.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				GamePlayer obj = client.Player.TargetObject as GamePlayer;
				if (obj == null)
				{
					client.Out.SendMessage("You have not selected a valid target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				switch (args[1].ToLower())
				{
					case "full":
						obj.RespecAmountAllSkill += amount;
						client.Out.SendMessage("You have given " + amount + " full skill respecs to " + obj.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						obj.Out.SendMessage(client.Player.Name + " grants you " + amount + " full skill respecs.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					case "single":
						obj.RespecAmountSingleSkill += amount;
						client.Out.SendMessage("You have given " + amount + " single-line skill respecs to " + obj.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						obj.Out.SendMessage(client.Player.Name + " grants you " + amount + " single-line skill respecs.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					default:
						client.Out.SendMessage(args[1] + " respec type is not known.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
				}
			}
			catch (Exception)
			{
				client.Out.SendMessage("Usage: /giverespec <full|single> <amount>",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
			}
			return 1;
		}
	}
}