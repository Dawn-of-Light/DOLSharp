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
using System.Linq;

using DOL.GS.Friends;
using DOL.Language;

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
				client.Player.SendFriendsListSnapshot();
				return;
			}
			else if (args.Length == 2 && args[1] == "window")
			{
				client.Player.SendFriendsListSocial();
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
				if (client.Player.GetFriends().Contains(name) && client.Player.RemoveFriend(name))
				{
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.Removed", name));
					return;
				}
				else
				{
					// nothing found
					DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.NotFound"));
					return;
				}
			}

			switch (result)
			{
				case 2:
					{
						// name not unique
						DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.NotUnique"));
						break;
					}
				case 3: // exact match
				case 4: // guessed name
					{
						if (fclient == client)
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.Yourself"));
							return;
						}

						name = fclient.Player.Name;
						if (client.Player.GetFriends().Contains(name) && client.Player.RemoveFriend(name))
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.Removed", name));
						}
						else if (client.Player.AddFriend(name))
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "Scripts.Players.Friend.Added", name));
						}
						break;
					}
			}
		}
	}
}