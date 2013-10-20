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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System;

using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// The GroupMgr holds pointers to all groups and to players
	/// looking for a group
	/// </summary>
	public sealed class GroupMgr
	{
		/// <summary>
		/// ArrayList of all groups in the game
		/// </summary>
		static readonly Dictionary<object, Group> m_groups = new Dictionary<object, Group>();
		/// <summary>
		/// ArrayList of all players looking for a group
		/// </summary>
		static readonly List<GamePlayer> m_lfgPlayers = new List<GamePlayer>();

		/// <summary>
		/// Adds a group to the list of groups
		/// </summary>
		/// <param name="key"></param>
		/// <param name="group">The group to add</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public static bool AddGroup(object key, Group group)
		{
			lock(((ICollection)m_groups).SyncRoot)
			{
				if(!m_groups.ContainsKey(key))
				{
					m_groups.Add(key, group);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes a group from the manager
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		public static bool RemoveGroup(object key)
		{
			Group group = null;
			lock (((ICollection)m_groups).SyncRoot)
			{
				if (!m_groups.ContainsKey(key) || group == null)
				{
					return false;
				}
				m_groups.Remove(key);
			}

			foreach (GameLiving living in group.GetMembersInTheGroup())
			{
				group.RemoveMember(living);
			}

			return true;
		}

		/// <summary>
		/// Adds a player to the looking for group list
		/// </summary>
		/// <param name="member">player to add to the list</param>
		public static void SetPlayerLooking(GamePlayer member)
		{
			lock(((ICollection)m_lfgPlayers).SyncRoot)
			{
				if(!m_lfgPlayers.Contains(member) && member.LookingForGroup==false)
				{
					member.LookingForGroup=true;
					m_lfgPlayers.Add(member);
				}
			}
		}

		/// <summary>
		/// Removes a player from the looking for group list
		/// </summary>
		/// <param name="member">player to remove from the list</param>
		public static void RemovePlayerLooking(GamePlayer member)
		{
			lock(((ICollection)m_lfgPlayers).SyncRoot)
			{
				member.LookingForGroup=false;
				if(m_lfgPlayers.Contains(member))
					m_lfgPlayers.Remove(member);
			}
		}

		/// <summary>
		/// Returns a list of groups by their status
		/// </summary>
		/// <param name="status">statusbyte</param>
		/// <returns>ArrayList of groups</returns>
		public static List<Group> ListGroupByStatus(byte status)
		{
			List<Group> groupList = new List<Group>();

			lock(((ICollection)m_groups).SyncRoot)
			{
				foreach (Group group in m_groups.Values)
				{
					if (group.Status == 10) continue; // not looking for members, ignore
					if (group.Status == status || group.Status == 0x0B)
					{
						groupList.Add(group);
					}
				}
			}
			return groupList;
		}

		/// <summary>
		/// Returns an Arraylist of all players looking for a group
		/// </summary>
		/// <returns>ArrayList of all players looking for a group</returns>
		public static List<GamePlayer> LookingForGroupPlayers()
		{
			List<GamePlayer> lookingPlayers = new List<GamePlayer>();
			lock(((ICollection)m_lfgPlayers).SyncRoot)
			{
				foreach (GamePlayer player in m_lfgPlayers)
				{
					if(player.Group==null)
					{
						lookingPlayers.Add(player);
					}
				}
			}
			return lookingPlayers;
		}
	}
}
