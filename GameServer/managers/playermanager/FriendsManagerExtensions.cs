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

namespace DOL.GS.Friends
{
    /// <summary>
    /// Game Player Friends List Manager Extensions
    /// </summary>
    public static class FriendsManagerExtensions
    {
        /// <summary>
        /// Get This Player Friends List
        /// </summary>
        /// <param name="player">Player to retrieve Friends List from</param>
        /// <returns>String array of Player's friends</returns>
        public static string[] GetFriends(this GamePlayer player)
        {
            return GameServer.Instance.PlayerManager.Friends[player];
        }

        /// <summary>
        /// Remove a Friend from Player Friends List
        /// </summary>
        /// <param name="player">Player to remove Friend from</param>
        /// <param name="friendName">Friend's Name to be removed</param>
        /// <returns>True if friend removed successfully.</returns>
        public static bool RemoveFriend(this GamePlayer player, string friendName)
        {
            return GameServer.Instance.PlayerManager.Friends.RemoveFriendFromPlayerList(player, friendName);
        }

        /// <summary>
        /// Add a Friend to Player Friends List
        /// </summary>
        /// <param name="player">Player to add Friend to</param>
        /// <param name="friendName">Friend's Name to be added</param>
        /// <returns>True if friend added successfully</returns>
        public static bool AddFriend(this GamePlayer player, string friendName)
        {
            return GameServer.Instance.PlayerManager.Friends.AddFriendToPlayerList(player, friendName);
        }

        /// <summary>
        /// Send Player Friends List Snapshot
        /// </summary>
        /// <param name="player">Player to send Snapshot to</param>
        public static void SendFriendsListSnapshot(this GamePlayer player)
        {
            GameServer.Instance.PlayerManager.Friends.SendPlayerFriendsSnapshot(player);
        }

        /// <summary>
        /// Send Player Friends List Social Window
        /// </summary>
        /// <param name="player">Player to send Social Window to</param>
        public static void SendFriendsListSocial(this GamePlayer player)
        {
            GameServer.Instance.PlayerManager.Friends.SendPlayerFriendsSocial(player);
        }
    }
}
