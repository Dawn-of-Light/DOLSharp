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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&send",
		new string[] {"&tell", "&t"},
		(uint) ePrivLevel.Player,
		"Sends a private message to a player",
		"Use: SEND <TARGET> <TEXT TO SEND>")]
	public class SendCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				client.Out.SendMessage("Use: SEND <TARGET> <TEXT TO SEND>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}
			string targetName = args[1];
			string message = string.Join(" ", args, 2, args.Length - 2);

			int result = 0;
			GameClient targetClient = WorldMgr.GetClientByPlayerName(targetName, false);
			if (targetClient != null && !GameServer.ServerRules.IsAllowedToUnderstand(client.Player, targetClient.Player))
			{
				targetClient = null;
			}

			if (targetClient == null)
			{
				// nothing found
				client.Out.SendMessage(targetName + " is not in the game, or in another realm.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return 1;
			}

			switch (result)
			{
				case 2: // name not unique
					client.Out.SendMessage("Character name is not unique.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				case 3: // exact match
				case 4: // guessed name
					if (targetClient == client)
					{
						client.Out.SendMessage("You can't /send to yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						client.Player.Send(targetClient.Player, message);
					}
					return 1;
			}
			return 0;
		}
	}
}