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
using System.Collections;
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a Group inside the game
	/// </summary>
	public class Group : IEnumerable
	{
		/// <summary>
		/// This holds all players inside the group
		/// </summary>
		protected ArrayList m_groupMembers = new ArrayList(8);
		/// <summary>
		/// This holds the status of the group
		/// eg. looking for members etc ...
		/// </summary>
		protected byte m_status = 0x0A;

		/// <summary>
		/// Creates an empty Group. Don't use this, use
		/// GroupMgr.CreateGroup() to create a group
		/// </summary>
		public Group()
		{
		}

		/// <summary>
		/// Adds a player to the group
		/// </summary>
		/// <param name="player">GamePlayer to be added to the group</param>
		/// <returns>true if added successfully</returns>
		public virtual bool AddPlayer(GamePlayer player) 
		{
			lock(this)
			{
				if (m_groupMembers.Contains(player))
					return false;
				m_groupMembers.Add(player);
			}
			return true;
		}

		/// <summary>
		/// Gets all GameClients inside this group
		/// </summary>
		/// <returns>GameClient Array of all group members</returns>
		public GameClient[] GetClientsInTheGroup()
		{
			ArrayList clients = new ArrayList(8);
			lock(this)
			{
				for(int i=0; i < m_groupMembers.Count; i++)
				{
					clients.Add(((GamePlayer)m_groupMembers[i]).Client);
				}
			}
			return (GameClient[])clients.ToArray(typeof(GameClient));
		}

		/// <summary>
		/// Gets all GamePlayers in this group
		/// </summary>
		/// <returns>Array of GamePlayers in this group</returns>
		public GamePlayer[] GetPlayersInTheGroup()
		{
			ArrayList players = new ArrayList(8);
			lock(this)
			{
				for(int i=0; i < m_groupMembers.Count; i++)
				{
					players.Add((GamePlayer)m_groupMembers[i]);
				}
			}
			return (GamePlayer[])players.ToArray(typeof(GamePlayer));
		}

		/// <summary>
		/// Removes a player from the group
		/// </summary>
		/// <param name="player">GamePlayer to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public virtual bool RemovePlayer(GamePlayer player)
		{
			lock(this)
			{
				if (!m_groupMembers.Contains(player))
					return false;

				m_groupMembers.Remove(player);

				if (m_groupMembers.Count<=0) 
				{
					GroupMgr.RemoveGroup(this);
				}					
				return true;
			}
		}

		
		/// <summary>
		/// Sends a message to all group members 
		/// </summary>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public virtual void SendMessageToGroupMembers(string msg, eChatType type, eChatLoc loc)
		{
			lock(this)
			{
				foreach(GamePlayer player in m_groupMembers)
				{
					player.Out.SendMessage(msg,type,loc);
				}
			}
		}

		/// <summary>
		/// Checks if a player is inside the group
		/// </summary>
		/// <param name="player">GamePlayer to check</param>
		/// <returns>true if the player is in the group</returns>
		public virtual bool IsInTheGroup(GamePlayer player)
		{
			lock(this)
			{	
				return m_groupMembers.Contains(player);
			}
		}

		/// <summary>
		/// Returns the number of players inside this group
		/// </summary>
		public int PlayerCount
		{
			get { return m_groupMembers.Count; }
		}

		/// <summary>
		/// Gets or sets the status of this group
		/// </summary>
		public byte Status
		{
			get { return m_status; }
			set { m_status=value; }
		}

		/// <summary>
		/// Gets group member by index
		/// </summary>
		public GamePlayer this[int index]
		{
			get { return (GamePlayer)m_groupMembers[index]; }
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return m_groupMembers.GetEnumerator();
		}

		#endregion
	}
}