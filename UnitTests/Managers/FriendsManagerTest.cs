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

using DOL.GS;
using DOL.GS.Friends;
using DOL.Database;
using DOLGameServerConsole;
using NUnit.Framework;

namespace DOL.Server.Tests
{
	/// <summary>
	/// FriendsManager Unit Tests.
	/// </summary>
	[TestFixture]
	public class FriendsManagerTest
	{		
		public FriendsManagerTest()
		{
		}
		
		[Test]
		public void FriendsManager_StartUp_NotNull()
		{
			Assert.IsNotNull(GameServer.Instance.PlayerManager.Friends);
		}
		
		[Test]
		public void FriendsManager_NoAdd_RetrieveEmptyGamePlayerList()
		{
			CollectionAssert.IsEmpty(GameServer.Instance.PlayerManager.Friends[null]);
		}
		
		[Test]
		public void FriendsManager_AddPlayer_RetrieveFriendsList()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			
			CollectionAssert.IsEmpty(gameplayer.GetFriends());
		}
		
		[Test]
		public void FriendsManager_AddPlayerWithFriend_RetrieveFriendsList()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() { SerializedFriendsList = "buddy" } } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { client.Account.Characters[0].SerializedFriendsList });
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy" });
		}
		
		[Test]
		public void FriendsManager_AddPlayerWithFriends_RetrieveFriendsList()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() { SerializedFriendsList = "buddy , mate , someone" } } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), gameplayer.SerializedFriendsList);
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy", "mate", "someone" });
		}
		
		[Test]
		public void FriendsManager_AddFriend_RetrieveFriend()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");

			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { client.Account.Characters[0].SerializedFriendsList });
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy" });
		}
		
		[Test]
		public void FriendsManager_AddFriends_RetrieveFriend()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");
			gameplayer.AddFriend("mate");
			gameplayer.AddFriend("someone");

			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), gameplayer.SerializedFriendsList);
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy", "mate", "someone" });
		}
		
		[Test]
		public void FriendsManager_AddDuplicate_RetrieveOnlyOne()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");
			Assert.IsFalse(gameplayer.AddFriend("buddy"));
			gameplayer.AddFriend("someone");

			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), gameplayer.SerializedFriendsList);
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy", "someone" });
		}
		
		[Test]
		public void FriendsManager_RemoveFriend_RetrieveEmpty()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");
			CollectionAssert.IsNotEmpty(gameplayer.GetFriends());
			gameplayer.RemoveFriend("buddy");
			CollectionAssert.IsEmpty(gameplayer.GetFriends());
		}
		
		[Test]
		public void FriendsManager_RemoveFriends_RetrieveEmpty()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");
			gameplayer.AddFriend("mate");
			gameplayer.AddFriend("someone");
			CollectionAssert.IsNotEmpty(gameplayer.GetFriends());
			gameplayer.RemoveFriend("buddy");
			gameplayer.RemoveFriend("mate");
			gameplayer.RemoveFriend("someone");
			CollectionAssert.IsEmpty(gameplayer.GetFriends());
		}
		
		[Test]
		public void FriendsManager_RemoveNonExisting_RetrieveOne()
		{
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(gameplayer);
			gameplayer.AddFriend("buddy");
			CollectionAssert.IsNotEmpty(gameplayer.GetFriends());
			Assert.IsFalse(gameplayer.RemoveFriend("mate"));
			CollectionAssert.AreEquivalent(gameplayer.GetFriends(), new [] { "buddy" });
		}
		
		[Test]
		public void FriendsManager_WrongMethodArguments_ThrowsException()
		{
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.AddPlayerFriendsListToCache(null));
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.RemovePlayerFriendsListFromCache(null));
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.AddFriendToPlayerList(null, null));
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.RemoveFriendFromPlayerList(null, null));
			
			var client = new GameClient(GameServer.Instance) {
				Account = new Account() { Characters = new [] { new DOLCharacters() } },
				Out = new ConsolePacketLib()
			};
			var gameplayer = new GamePlayer(client, client.Account.Characters[0]);
			
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.AddFriendToPlayerList(gameplayer, null));
			Assert.Throws(typeof(ArgumentNullException), () => GameServer.Instance.PlayerManager.Friends.RemoveFriendFromPlayerList(gameplayer, null));
			Assert.Throws(typeof(ArgumentException), () => GameServer.Instance.PlayerManager.Friends.AddFriendToPlayerList(gameplayer, string.Empty));
			Assert.Throws(typeof(ArgumentException), () => GameServer.Instance.PlayerManager.Friends.RemoveFriendFromPlayerList(gameplayer, string.Empty));
			Assert.Throws(typeof(ArgumentException), () => GameServer.Instance.PlayerManager.Friends.AddFriendToPlayerList(gameplayer, " "));
			Assert.Throws(typeof(ArgumentException), () => GameServer.Instance.PlayerManager.Friends.RemoveFriendFromPlayerList(gameplayer, " "));
		}
	}
}
