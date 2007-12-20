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
		"&morph", //command to handle
		ePrivLevel.GM, //minimum privelege level
		"Changes the players model", //command description
		"'/morph <modelID>' to change into <modelID>",
		"'/morph reset' to change back to normal")] //usage
		public class MorphCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length == 1)
			{
				client.Out.SendMessage("Usage: /morph modelid",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}
			if (args[1] == "reset")
			{
				client.Player.Model = (ushort) client.Account.Characters[client.ActiveCharIndex].CreationModel;
				client.Out.SendMessage("You change back to your normal form!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			ushort model;
			try
			{
				model = Convert.ToUInt16(args[1]);
			}
			catch (Exception)
			{
				client.Out.SendMessage("Usage: /morph modelid",
				                       eChatType.CT_System,
				                       eChatLoc.CL_SystemWindow);
				return 1;
			}
			client.Player.Model = model;
			client.Out.SendMessage("You change into a new form!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return 1;
		}
	}
}