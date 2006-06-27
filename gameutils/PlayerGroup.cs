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
using System.Reflection;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for PlayerGroup.
	/// </summary>
	public class PlayerGroup : Group
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Group size limit
		/// </summary>
		public const int MAX_GROUP_SIZE = 8;

		GamePlayer m_leader = null;
		bool m_autosplitLoot = true;
		bool m_autosplitCoins = true;

		/// <summary>
		/// Constructs a new PlayerGroup
		/// </summary>
		/// <param name="leader">The group leader</param>
		public PlayerGroup(GamePlayer leader)
		{
			m_leader = leader;
		}

		/// <summary>
		/// Gets/sets the group moderator
		/// </summary>
		public GamePlayer Leader
		{
			get { return m_leader; }
			set { m_leader = value; }
		}
		
		/// <summary>
		/// Adds a player to the group
		/// </summary>
		/// <param name="player">The player to be added</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public override bool AddPlayer(GamePlayer player)
		{
			if (this.PlayerCount >= MAX_GROUP_SIZE || !base.AddPlayer(player))
				return false;

			player.PlayerGroup = this;
			player.PlayerGroupIndex = PlayerCount - 1;

			UpdateGroupWindow();
			// update icons of joined player to everyone in the group
			UpdateMember(player, true, false);
			// updae all icons for just joined player
			player.Out.SendGroupMembersUpdate(true);

			SendMessageToGroupMembers(player.Name + " has joined the group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			GameEventMgr.Notify(PlayerGroupEvent.PlayerJoined, this, new PlayerJoinedEventArgs(player));
			return true;
		}

		/// <summary>
		/// Removes a player from the group
		/// </summary>
		/// <param name="player">The player to remove</param>
		/// <returns>True if the function succeeded, otherwise false</returns>
		public override bool RemovePlayer(GamePlayer player)
		{
			if(base.RemovePlayer(player))
			{				
				player.PlayerGroup = null;
				player.PlayerGroupIndex = -1;
				player.Out.SendGroupWindowUpdate();
				UpdateGroupWindow();
				player.Out.SendMessage("You leave your group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				SendMessageToGroupMembers(player.Name+" has left the group.",eChatType.CT_System,eChatLoc.CL_SystemWindow);

				// only one player left?
				if (PlayerCount==1)
				{
					RemovePlayer((GamePlayer)m_groupMembers[0]);
				}
				else if(Leader == player && PlayerCount > 0)
				{
					Leader = (GamePlayer)m_groupMembers[0];
					SendMessageToGroupMembers(null, Leader.Name + " is the new group leader.");
				}
				UpdatePlayerIndexes();
				GameEventMgr.Notify(PlayerGroupEvent.PlayerDisbanded, this, new PlayerDisbandedEventArgs(player));				

				return true;
			}
			return false;
		}

		/// <summary>
		/// Makes player current leader of the group
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool MakeLeader(GamePlayer player)
		{
			if(!m_groupMembers.Contains(player))
				return false;

			int ind;
			lock(this)
			{
				ind = player.PlayerGroupIndex;
				m_groupMembers[ind] = m_groupMembers[0];
				m_groupMembers[0] = player;
				Leader = player;
			}	
			((GamePlayer)m_groupMembers[0]).PlayerGroupIndex = 0;
			((GamePlayer)m_groupMembers[ind]).PlayerGroupIndex = ind;
			UpdateGroupWindow();
			return true;
		}

		/// <summary>
		/// Updates player indexes
		/// </summary>
		private void UpdatePlayerIndexes()
		{
			for (int i = 0; i < m_groupMembers.Count; i++)
			{
				((GamePlayer)m_groupMembers[i]).PlayerGroupIndex = i;
			}
		}

		/// <summary>
		/// Updates the group window to all players
		/// </summary>
		public void UpdateGroupWindow()
		{
			lock (this)
			{
				for(int i = 0; i < m_groupMembers.Count; ++i)
				{
					((GamePlayer)m_groupMembers[i]).Out.SendGroupWindowUpdate();
				}
			}
		}

		/// <summary>
		/// Updates a group member to all other players in the group
		/// </summary>
		/// <param name="player">player to update</param>
		/// <param name="updateIcons">Do icons need an update</param>
		/// <param name="updateOtherRegions">Should updates be sent to players in other regions</param>
		public void UpdateMember(GamePlayer player, bool updateIcons, bool updateOtherRegions)
		{
			lock(this)
			{
				if (player.PlayerGroup != this)
					return;

				for(int i=0; i < m_groupMembers.Count; i++)
				{
					GamePlayer member = (GamePlayer)m_groupMembers[i];
					if (updateOtherRegions || member.CurrentRegion == player.CurrentRegion)
					{
						member.Out.SendGroupMemberUpdate(updateIcons, player);
					}
				}
			}
		}

		/// <summary>
		/// Updates all group members to one member
		/// </summary>
		/// <param name="player">The player that should receive updates</param>
		/// <param name="updateIcons">Do icons need an update</param>
		/// <param name="updateOtherRegions">Should updates be sent to players in other regions</param>
		public void UpdateAllToMember(GamePlayer player, bool updateIcons, bool updateOtherRegions)
		{
			lock (this)
			{
				if (player.PlayerGroup != this)
					return;

				for(int i=0; i < m_groupMembers.Count; i++)
				{
					GamePlayer member = (GamePlayer)m_groupMembers[i];
					if (updateOtherRegions || member.CurrentRegion == player.CurrentRegion)
					{
						player.Out.SendGroupMemberUpdate(updateIcons, member);
					}
				}
			}
		}

		/// <summary>
		/// Sends a message to the group
		/// </summary>
		/// <param name="from">The living the message comes from</param>
		/// <param name="msg">The message to be sent</param>
		public void SendMessageToGroupMembers(GameLiving from, string msg)
		{
			string message = msg;
			if (from!=null) {
				message = "[Party] "+from.GetName(0, true)+": \""+message+"\"";
			} else {
				message = "[Party] "+message;
			}
			base.SendMessageToGroupMembers(message, eChatType.CT_Group, eChatLoc.CL_ChatWindow);
		}

		/// <summary>
		/// If at least one player is in combat group is in combat
		/// </summary>
		/// <returns>true if group in combat</returns>
		public bool IsGroupInCombat()
		{
			lock (this)
			{
				foreach(GamePlayer player in m_groupMembers)
				{
					if(player.InCombat)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Gets or sets the group's autosplit loot flag
		/// </summary>
		public bool AutosplitLoot
		{
			get { return m_autosplitLoot; }
			set { m_autosplitLoot = value; }
		}

		/// <summary>
		/// Gets or sets the group's autosplit coins flag
		/// </summary>
		public bool AutosplitCoins
		{
			get { return m_autosplitCoins; }
			set { m_autosplitCoins = value; }
		}
	}
}
