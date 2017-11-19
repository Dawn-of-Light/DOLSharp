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
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    /// <summary>
    /// The GroupMgr holds pointers to all groups and to players
    /// looking for a group
    /// </summary>
    public static class GroupMgr
	{
		/// <summary>
		/// Dictionary of all groups in the game
		/// </summary>
		static readonly ReaderWriterDictionary<Group, bool> m_groups = new ReaderWriterDictionary<Group, bool>();
		/// <summary>
		/// ArrayList of all players looking for a group
		/// </summary>
		static readonly ReaderWriterDictionary<GamePlayer, bool> m_lfgPlayers = new ReaderWriterDictionary<GamePlayer, bool>();

		/// <summary>
		/// Adds a group to the list of groups
		/// </summary>
		/// <param name="group">The group to add</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public static bool AddGroup(Group group)
		{
			return m_groups.AddIfNotExists(group, true);
		}

		/// <summary>
		/// Removes a group from the manager
		/// </summary>
		/// <param name="group"></param>
		/// <returns></returns>
		public static bool RemoveGroup(Group group)
		{
			bool dummy;
			return m_groups.TryRemove(group, out dummy);
		}

		/// <summary>
		/// Adds a player to the looking for group list
		/// </summary>
		/// <param name="member">player to add to the list</param>
		public static void SetPlayerLooking(GamePlayer member)
		{
			if (member.LookingForGroup == false && m_lfgPlayers.AddIfNotExists(member, true))
			{
				member.LookingForGroup = true;
			}
		}

		/// <summary>
		/// Removes a player from the looking for group list
		/// </summary>
		/// <param name="member">player to remove from the list</param>
		public static void RemovePlayerLooking(GamePlayer member)
		{
			member.LookingForGroup = false;
			bool dummy;
			m_lfgPlayers.TryRemove(member, out dummy);
		}

		/// <summary>
		/// Returns a list of groups by their status
		/// </summary>
		/// <param name="status">statusbyte</param>
		/// <returns>ArrayList of groups</returns>
		public static ICollection<Group> ListGroupByStatus(byte status)
		{
			return m_groups.Keys.Where(g => g.Status == 0x0B || g.Status == status).ToArray();
		}

		/// <summary>
		/// Returns an Arraylist of all players looking for a group
		/// </summary>
		/// <returns>ArrayList of all players looking for a group</returns>
		public static ICollection<GamePlayer> LookingForGroupPlayers()
		{
			return m_lfgPlayers.Keys.Where(p => p.Group == null).ToArray();
		}
	}
}
