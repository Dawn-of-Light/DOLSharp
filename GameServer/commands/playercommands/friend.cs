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
		"&friend",
		ePrivLevel.Player,
		"Adds/Removes a player to/from your friendlist!",
		"/friend <playerName>")]
	public class FriendCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				string[] friends = client.Player.DBCharacter.SerializedFriendsList.Split(',');
				client.Out.SendCustomTextWindow("Friends (snapshot)", friends);
				return;
			}
			else if (args.Length == 2 && args[1] == "window")
			{
				// "TF" - clear friend list in social
				client.Out.SendMessage("TF", eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
				byte ind = 0;
				foreach (string friendName in client.Player.Friends)
				{
					GameClient friendClient = WorldMgr.GetClientByPlayerName(friendName, true, true);
					if (friendClient == null || friendClient.Player == null || friendClient.Player.IsAnonymous) continue;
					client.Out.SendMessage(string.Format("F,{0},{1},{2},{3},\"{4}\"",
						ind++, friendClient.Player.Name, friendClient.Player.Level, friendClient.Player.CharacterClass.ID, (friendClient.Player.CurrentZone == null ? "" : friendClient.Player.CurrentZone.Description)), eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
				}
				return;
			}
			string name = string.Join(" ", args, 1, args.Length - 1);

			int result = 0;
			GameClient fclient = WorldMgr.GuessClientByPlayerNameAndRealm(name, 0, false, out result);
			if (fclient != null && !GameServer.ServerRules.IsSameRealm(fclient.Player, client.Player, true))
			{
				fclient = null;
			}

			if (fclient == null)
			{
				name = args[1];
				if (client.Player.Friends.Contains(name))
				{
					DisplayMessage(client, name + " was removed from your friend list!");
					client.Player.ModifyFriend(name, true);
					client.Out.SendRemoveFriends(new string[] {name});
					return;
				}
				else
				{
					// nothing found
					DisplayMessage(client, "No players online with that name.");
					return;
				}
			}

			switch (result)
			{
				case 2:
					{
						// name not unique
						DisplayMessage(client, "Character name is not unique.");
						break;
					}
				case 3: // exact match
				case 4: // guessed name
					{
						if (fclient == client)
						{
							DisplayMessage(client, "You can't add yourself!");
							return;
						}

						name = fclient.Player.Name;
						if (client.Player.Friends.Contains(name))
						{
							DisplayMessage(client, name + " was removed from your friend list!");
							client.Player.ModifyFriend(name, true);
							client.Out.SendRemoveFriends(new string[] { name });
						}
						else
						{
							DisplayMessage(client, name + " was added to your friend list!");
							client.Player.ModifyFriend(name, false);
							client.Out.SendAddFriends(new string[] { name });
						}
						break;
					}
			}
		}
	}
}