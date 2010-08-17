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
		"&send",
		new string[] { "&tell", "&t" },
		ePrivLevel.Player,
		"Sends a private message to a player",
		"Use: SEND <TARGET> <TEXT TO SEND>")]
	public class SendCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 3)
			{
				client.Out.SendMessage("Use: SEND <TARGET> <TEXT TO SEND>", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsSpammingCommand(client.Player, "send", 500))
			{
				DisplayMessage(client, "Slow down! Think before you say each word!");
				return;
			}

			string targetName = args[1];
			string message = string.Join(" ", args, 2, args.Length - 2);

			int result = 0;
			GameClient targetClient = WorldMgr.GuessClientByPlayerNameAndRealm(targetName, 0, false, out result);
			if (targetClient != null && !GameServer.ServerRules.IsAllowedToUnderstand(client.Player, targetClient.Player))
			{
				targetClient = null;
			}

			if (targetClient == null)
			{
				// nothing found
				client.Out.SendMessage(targetName + " is not in the game, or in another realm.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

            // prevent to send an anon GM a message to find him - but send the message to the GM - thx to Sumy
            if (targetClient.Player != null && targetClient.Player.IsAnonymous && targetClient.Account.PrivLevel > (uint)ePrivLevel.Player)
            {
                client.Out.SendMessage(targetName + " is not in the game, or in another realm.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                targetClient.Player.Out.SendMessage(string.Format("You're anon but {0} tried to send: {1}", client.Player.Name, message), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
                return;
            }

			switch (result)
			{
				case 2: // name not unique
					client.Out.SendMessage("Character name is not unique.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				case 3: // exact match
				case 4: // guessed name
					if (targetClient == client)
					{
						client.Out.SendMessage("You can't /send to yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						client.Player.SendPrivateMessage(targetClient.Player, message);
					}
					return;
			}
		}
	}
}