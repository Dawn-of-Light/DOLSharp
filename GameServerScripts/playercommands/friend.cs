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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&friend",
		(uint) ePrivLevel.Player,
		"Adds/Removes a player to/from your friendlist!",
		"/friend <playerName>")]
	public class FriendCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				string[] friends = client.Player.PlayerCharacter.SerializedFriendsList.Split(',');
				client.Out.SendCustomTextWindow("Friends (snapshot)", friends);
				return 1;
			}
			string name = string.Join(" ", args, 1, args.Length - 1);

			int result = 0;
			GameClient fclient = WorldMgr.GuessClientByPlayerNameAndRealm(name, 0, out result);
			if (fclient != null && !GameServer.ServerRules.IsSameRealm(fclient.Player, client.Player, true))
			{
				fclient = null;
			}

			if (fclient == null)
			{
				name = args[1];
				if (client.Player.Friends.Contains(name))
				{
					client.Out.SendMessage(name + " was removed from your friend list!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Player.ModifyFriend(name, true);
					client.Out.SendRemoveFriends(new string[] {name});
					return 1;
				}
				else
				{
					// nothing found
					client.Out.SendMessage("No players online with that name.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}
			}

			switch (result)
			{
				case 2: // name not unique
					client.Out.SendMessage("Character name is not unique.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				case 3: // exact match
				case 4: // guessed name
					if (fclient == client)
					{
						client.Out.SendMessage("You can't add yourself!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}

					name = fclient.Player.Name;
					if (client.Player.Friends.Contains(name))
					{
						client.Out.SendMessage(name + " was removed from your friend list!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Player.ModifyFriend(name, true);
						client.Out.SendRemoveFriends(new string[] {name});
					}
					else
					{
						client.Out.SendMessage(name + " was added to your friend list!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Player.ModifyFriend(name, false);
						client.Out.SendAddFriends(new string[] {name});
					}
					return 1;
			}
			return 0;
		}
	}
}