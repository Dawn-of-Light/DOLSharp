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
using System.Collections.Generic;
using System.Text;
using DOL.GS.PacketHandler;
using DOL.Events;

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
		protected List<GameLiving> m_groupMembers = new List<GameLiving>(8);

		public Group(GamePlayer leader)
		{
			m_leader = leader;
		}

		/// <summary>
		/// Gets/sets the group leader
		/// </summary>
		GamePlayer m_leader = null;
		public GamePlayer Leader
		{
			get { return m_leader; }
			set { m_leader = value; }
		}


		private Quests.AbstractMission m_mission = null;
		public Quests.AbstractMission Mission
		{
			get { return m_mission; }
			set
			{
				m_mission = value;
				lock (m_groupMembers)
				{
					foreach (GameLiving living in this.m_groupMembers)
					{
						if (living is GamePlayer == false) continue;
						GamePlayer player = living as GamePlayer;
						player.Out.SendQuestListUpdate();
						if (value != null) player.Out.SendMessage(m_mission.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the group's autosplit loot flag
		/// </summary>
		bool m_autosplitLoot = true;
		public bool AutosplitLoot
		{
			get { return m_autosplitLoot; }
			set { m_autosplitLoot = value; }
		}

		/// <summary>
		/// Gets or sets the group's autosplit coins flag
		/// </summary>
		bool m_autosplitCoins = true;
		public bool AutosplitCoins
		{
			get { return m_autosplitCoins; }
			set { m_autosplitCoins = value; }
		}

		/// <summary>
		/// Group size limit
		/// </summary>
		public const int MAX_GROUP_SIZE = 8;

		/// <summary>
		/// This holds the status of the group
		/// eg. looking for members etc ...
		/// </summary>
		protected byte m_status = 0x0A;

		/// <summary>
		/// Gets or sets the status of this group
		/// </summary>
		public byte Status
		{
			get { return m_status; }
			set { m_status = value; }
		}

		/// <summary>
		/// Adds a player to the group
		/// </summary>
		/// <param name="player">GamePlayer to be added to the group</param>
		/// <returns>true if added successfully</returns>
		public virtual bool AddMember(GameLiving living) 
		{
			if (this.MemberCount >= MAX_GROUP_SIZE)
				return false;

			lock (m_groupMembers)
			{
				if (m_groupMembers.Contains(living))
					return false;
				m_groupMembers.Add(living);
			}

			living.Group = this;
			living.GroupIndex = MemberCount - 1;

			UpdateGroupWindow();
			// update icons of joined player to everyone in the group
			UpdateMember(living, true, false);
			// updae all icons for just joined player
			if (living is GamePlayer)
				((GamePlayer)living).Out.SendGroupMembersUpdate(true);

			SendMessageToGroupMembers(living.Name + " has joined the group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			GameEventMgr.Notify(GroupEvent.MemberJoined, this, new MemberJoinedEventArgs(living));
			return true;
		}

		/// <summary>
		/// Gets all members of the group
		/// </summary>
		/// <returns>Array of GamePlayers in this group</returns>
		public ICollection<GameLiving> GetMembersInTheGroup()
		{
			List<GameLiving> livings = new List<GameLiving>();
			lock (m_groupMembers)
			{
				foreach (GameLiving living in m_groupMembers)
				{
					livings.Add(living);
				}
			}
			return livings;
		}

		/// <summary>
		/// Gets all players of the group
		/// </summary>
		/// <returns>Array of GamePlayers in this group</returns>
		public ICollection<GamePlayer> GetPlayersInTheGroup()
		{
			List<GamePlayer> players = new List<GamePlayer>();
			lock (m_groupMembers)
			{
				foreach (GameLiving living in m_groupMembers)
				{
					if (living is GamePlayer == false) continue;
					players.Add(living as GamePlayer);
				}
			}
			return players;
		}

		/// <summary>
		/// Removes a player from the group
		/// </summary>
		/// <param name="player">GamePlayer to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public virtual bool RemoveMember(GameLiving living)
		{
			lock (m_groupMembers)
			{
				if (!m_groupMembers.Contains(living))
					return false;

				m_groupMembers.Remove(living);

				if (m_groupMembers.Count <= 0)
				{
					GroupMgr.RemoveGroup(this);
				}
			}

			living.Group = null;
			living.GroupIndex = -1;
			if (living is GamePlayer)
			{
				((GamePlayer)living).Out.SendGroupWindowUpdate();
				((GamePlayer)living).Out.SendQuestListUpdate();
			}
			UpdateGroupWindow();
			if (living is GamePlayer)
				((GamePlayer)living).Out.SendMessage("You leave your group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			SendMessageToGroupMembers(living.Name + " has left the group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			// only one player left?
			if (MemberCount == 1)
			{
				// RR4: Group is disbanded, ending mission group if any
				if (Mission != null)
					Mission.ExpireMission();
				RemoveMember(m_groupMembers[0]);
			}
			else if (Leader == living && MemberCount > 0)
			{
				//only allow players to be leaders
				for (int i = 0; i < m_groupMembers.Count; i++)
				{
					if (m_groupMembers[i] is GamePlayer)
					{
						Leader = m_groupMembers[i] as GamePlayer;
						SendMessageToGroupMembers(Leader.Name + " is the new group leader.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						break;
					}
				}
				//if the leader is still the same, disband group, only npcs left
				if (Leader == living)
				{
					lock (m_groupMembers)
					{
						foreach (GameLiving member in m_groupMembers)
						{
							member.Group = null;
							member.GroupIndex = -1;
							GameEventMgr.Notify(GroupEvent.MemberDisbanded, this, new MemberDisbandedEventArgs(member));
						}
					}
					m_groupMembers.Clear();
				}
			}
			UpdateGroupIndexes();
			GameEventMgr.Notify(GroupEvent.MemberDisbanded, this, new MemberDisbandedEventArgs(living));

			return true;
		}



		/// <summary>
		/// Updates player indexes
		/// </summary>
		private void UpdateGroupIndexes()
		{
			lock (m_groupMembers)
			{
				for (int i = 0; i < m_groupMembers.Count; i++)
				{
					((GameLiving)m_groupMembers[i]).GroupIndex = i;
				}
			}
		}

		
		/// <summary>
		/// Sends a message to all group members 
		/// </summary>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public virtual void SendMessageToGroupMembers(GameLiving from, string msg, eChatType type, eChatLoc loc)
		{
			string message = msg;
			if (from != null)
			{
				message = "[Party] " + from.GetName(0, true) + ": \"" + message + "\"";
			}
			else
			{
				message = "[Party] " + message;
			}

			SendMessageToGroupMembers(message, type, loc);
		}

		public virtual void SendMessageToGroupMembers(string msg, eChatType type, eChatLoc loc)
		{
			lock (m_groupMembers)
			{
				foreach (GameLiving member in m_groupMembers)
				{
					if (member is GamePlayer == false) continue;
					((GamePlayer)member).Out.SendMessage(msg, type, loc);
				}
			}
		}

		/// <summary>
		/// Checks if a player is inside the group
		/// </summary>
		/// <param name="player">GamePlayer to check</param>
		/// <returns>true if the player is in the group</returns>
		public virtual bool IsInTheGroup(GameLiving living)
		{
			lock (m_groupMembers)
			{	
				return m_groupMembers.Contains(living);
			}
		}

		/// <summary>
		/// Updates a group member to all other players in the group
		/// </summary>
		/// <param name="player">player to update</param>
		/// <param name="updateIcons">Do icons need an update</param>
		/// <param name="updateOtherRegions">Should updates be sent to players in other regions</param>
		public void UpdateMember(GameLiving living, bool updateIcons, bool updateOtherRegions)
		{
			lock (m_groupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (living.Group != this)
					return;

				foreach (GameLiving grpLiving in m_groupMembers)
				{
					if (grpLiving is GamePlayer == false) continue;
					GamePlayer member = grpLiving as GamePlayer;
					if (updateOtherRegions || member.CurrentRegion == living.CurrentRegion)
					{
						member.Out.SendGroupMemberUpdate(updateIcons, living);
					}
				}
			}
		}

		/// <summary>
		/// Updates the group window to all players
		/// </summary>
		public void UpdateGroupWindow()
		{
			lock (m_groupMembers)
			{
				foreach (GameLiving living in m_groupMembers)
				{
					if (living is GamePlayer)
						((GamePlayer)living).Out.SendGroupWindowUpdate();
				}
			}
		}

		/// <summary>
		/// If at least one player is in combat group is in combat
		/// </summary>
		/// <returns>true if group in combat</returns>
		public bool IsGroupInCombat()
		{
			lock (m_groupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				foreach (GameLiving living in m_groupMembers)
				{
					if (living.InCombat)
						return true;
				}
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
			lock (m_groupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (!m_groupMembers.Contains(player))
					return false;

				int ind;

				ind = player.GroupIndex;
				m_groupMembers[ind] = m_groupMembers[0];
				m_groupMembers[0] = player;
				Leader = player;
				((GameLiving)m_groupMembers[0]).GroupIndex = 0;
				((GameLiving)m_groupMembers[ind]).GroupIndex = ind;
			}
			UpdateGroupWindow();
			return true;
		}

		/// <summary>
		/// Updates all group members to one member
		/// </summary>
		/// <param name="player">The player that should receive updates</param>
		/// <param name="updateIcons">Do icons need an update</param>
		/// <param name="updateOtherRegions">Should updates be sent to players in other regions</param>
		public void UpdateAllToMember(GamePlayer player, bool updateIcons, bool updateOtherRegions)
		{
			lock (m_groupMembers) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (player.Group != this)
					return;

				foreach (GameLiving living in m_groupMembers)
				{
					if (updateOtherRegions || living.CurrentRegion == player.CurrentRegion)
					{
						player.Out.SendGroupMemberUpdate(updateIcons, living);
					}
				}
			}
		}




		/// <summary>
		///  This is NOT to be used outside of Battelgroup code.
		/// </summary>
		/// <param name="player">Input from battlegroups</param>
		/// <returns>A string of group members</returns>
		public string GroupMemberString(GamePlayer player)
		{
			lock (m_groupMembers)
			{
				StringBuilder text = new StringBuilder(64); //create the string builder
				text.Length = 0;
				BattleGroup mybattlegroup = (BattleGroup)player.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
				foreach (GamePlayer plr in m_groupMembers)
				{
					if (mybattlegroup.IsInTheBattleGroup(plr))
					{
						if ((bool)mybattlegroup.Members[plr] == true)
						{
							text.Append("<Leader> ");
						}
						text.Append("(I)");
					}
					text.Append(plr.Name + " ");
				}
				return text.ToString();
			}
		}



		/// <summary>
		///  This is NOT to be used outside of Battelgroup code.
		/// </summary>
		/// <param name="player">Input from battlegroups</param>
		/// <returns>A string of group members</returns>
		public string GroupMemberClassString(GamePlayer player)
		{
			lock (m_groupMembers)
			{
				StringBuilder text = new StringBuilder(64); //create the string builder
				text.Length = 0;
				BattleGroup mybattlegroup = (BattleGroup)player.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
				foreach (GamePlayer plr in m_groupMembers)
				{
					if (mybattlegroup.IsInTheBattleGroup(plr))
					{
						if ((bool)mybattlegroup.Members[plr] == true)
						{
							text.Append("<Leader> ");
						}
					}
					text.Append("(" + plr.CharacterClass.Name + ")");
					text.Append(plr.Name + " ");
				}
				return text.ToString();
			}
		}

		/// <summary>
		/// Returns the number of players inside this group
		/// </summary>
		public int MemberCount
		{
			get { return m_groupMembers.Count; }
		}

		#region IEnumerable Members

		public IEnumerator GetEnumerator()
		{
			return m_groupMembers.GetEnumerator();
		}

		#endregion
	}
}
