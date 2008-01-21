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
using System.Collections.Specialized;
using System;

using DOL.Database2;

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
		static readonly HybridDictionary m_groups = new HybridDictionary();
		/// <summary>
		/// ArrayList of all players looking for a group
		/// </summary>
		static readonly HybridDictionary m_lfgPlayers = new HybridDictionary();

		/// <summary>
		/// Adds a group to the list of groups
		/// </summary>
		/// <param name="key"></param>
		/// <param name="group">The group to add</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public static bool AddGroup(object key, Group group)
		{
			lock(m_groups)
			{
				if(!m_groups.Contains(key))
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
			lock (m_groups)
			{
				group = (Group)m_groups[key];
				if(group == null)
				{
					return false;
				}
				m_groups.Remove(key);
			}

			lock (group)
			{
				foreach(GameLiving living in group)
				{
					group.RemoveMember(living);
				}
			}

			return true;
		}

		/// <summary>
		/// Adds a player to the looking for group list
		/// </summary>
		/// <param name="member">player to add to the list</param>
		public static void SetPlayerLooking(GamePlayer member)
		{
			lock(m_lfgPlayers)
			{
				if(!m_lfgPlayers.Contains(member) && member.LookingForGroup==false)
				{
					member.LookingForGroup=true;
					m_lfgPlayers.Add(member, null);
				}
			}
		}

		/// <summary>
		/// Removes a player from the looking for group list
		/// </summary>
		/// <param name="member">player to remove from the list</param>
		public static void RemovePlayerLooking(GamePlayer member)
		{
			lock(m_lfgPlayers)
			{
				member.LookingForGroup=false;
				m_lfgPlayers.Remove(member);
			}
		}

		/// <summary>
		/// Returns a list of groups by their status
		/// </summary>
		/// <param name="status">statusbyte</param>
		/// <returns>ArrayList of groups</returns>
		public static ArrayList ListGroupByStatus(byte status)
		{
			ArrayList groupList = new ArrayList();

			lock(m_groups)
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
		public static ArrayList LookingForGroupPlayers()
		{
			ArrayList lookingPlayers = new ArrayList();
			lock(m_lfgPlayers)
			{
				foreach (GamePlayer player in m_lfgPlayers.Keys)
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
