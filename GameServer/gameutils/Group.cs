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
using System.Linq;
using System.Collections.Generic;
using System.Text;
using DOL.GS.PacketHandler;
using DOL.Events;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a Group inside the game
	/// </summary>
	public class Group
	{
		#region constructor and members
		/// <summary>
		/// Default Constructor with GamePlayer Leader.
		/// </summary>
		/// <param name="leader"></param>
		public Group(GamePlayer leader)
			: this((GameLiving)leader)
		{
		}
		
		/// <summary>
		/// Default Constructor with GameLiving Leader.
		/// </summary>
		/// <param name="leader"></param>
		public Group(GameLiving leader)
		{
			LivingLeader = leader;
			m_groupMembers = new ReaderWriterList<GameLiving>(ServerProperties.Properties.GROUP_MAX_MEMBER);
		}
		
		/// <summary>
		/// This holds all players inside the group
		/// </summary>
		protected readonly ReaderWriterList<GameLiving> m_groupMembers;
		#endregion

		#region Leader / Member
		
		/// <summary>
		/// Gets/sets the group Player leader
		/// </summary>
		public GamePlayer Leader
		{
			get { return LivingLeader as GamePlayer; }
			private set { LivingLeader = value; }
		}

		/// <summary>
		/// Gets/sets the group Living leader
		/// </summary>
		public GameLiving LivingLeader { get; protected set; }
		
		/// <summary>
		/// Returns the number of players inside this group
		/// </summary>
		public byte MemberCount
		{
			get { return (byte)m_groupMembers.Count; }
		}
		#endregion
		
		#region mission
		/// <summary>
		/// This Group Mission.
		/// </summary>
		private Quests.AbstractMission m_mission = null;
		
		/// <summary>
		/// Group Mission
		/// </summary>
		public Quests.AbstractMission Mission
		{
			get { return m_mission; }
			set
			{
				m_mission = value;
				foreach (GamePlayer player in m_groupMembers.OfType<GamePlayer>())
				{
					player.Out.SendQuestListUpdate();
					if (value != null)
						player.Out.SendMessage(m_mission.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}
		#endregion

		#region autosplit
		/// <summary>
		/// Gets or sets the group's autosplit loot flag
		/// </summary>
		protected bool m_autosplitLoot = true;
		
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
		protected bool m_autosplitCoins = true;

		/// <summary>
		/// Gets or sets the group's autosplit coins flag
		/// </summary>
		public bool AutosplitCoins
		{
			get { return m_autosplitCoins; }
			set { m_autosplitCoins = value; }
		}
		#endregion

		#region lfg status
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
		#endregion

		#region managing members
		/// <summary>
		/// Gets all members of the group
		/// </summary>
		/// <returns>Array of GameLiving in this group</returns>
		public ICollection<GameLiving> GetMembersInTheGroup()
		{
			return m_groupMembers.ToArray();
		}
		
		/// <summary>
		/// Gets all players of the group
		/// </summary>
		/// <returns>Array of GamePlayers in this group</returns>
		public ICollection<GamePlayer> GetPlayersInTheGroup()
		{
			return m_groupMembers.OfType<GamePlayer>().ToArray();
		}
		
		/// <summary>
		/// Adds a living to the group
		/// </summary>
		/// <param name="living">GameLiving to be added to the group</param>
		/// <returns>true if added successfully</returns>
		public virtual bool AddMember(GameLiving living) 
        {
			if (!m_groupMembers.FreezeWhile<bool>(l => {
			                                      	if (l.Count >= ServerProperties.Properties.GROUP_MAX_MEMBER || l.Count >= (byte.MaxValue - 1))
			                                      		return false;
			                                      	
			                                      	if (l.Contains(living))
			                                      		return false;
			                                      	
			                                      	l.Add(living);
			                                      	living.Group = this;
			                                      	living.GroupIndex = (byte)(l.Count - 1);
			                                      	return true;
			                                      }))
				return false;

			UpdateGroupWindow();
			// update icons of joined player to everyone in the group
			UpdateMember(living, true, false);
			
			// update all icons for just joined player
			var player = living as GamePlayer;
			if (player != null)
				player.Out.SendGroupMembersUpdate(true, true);

			SendMessageToGroupMembers(string.Format("{0} has joined the group.", living.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			GameEventMgr.Notify(GroupEvent.MemberJoined, this, new MemberJoinedEventArgs(living));
			return true;
		}
		
		/// <summary>
		/// Removes a living from the group
		/// </summary>
		/// <param name="living">GameLiving to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public virtual bool RemoveMember(GameLiving living)
		{
			if (!m_groupMembers.TryRemove(living))
				return false;

			if (MemberCount < 1)
				DisbandGroup();
			
			living.Group = null;
			living.GroupIndex = 0xFF;
			// Update Player.
			var player = living as GamePlayer;
			if (player != null)
			{
				player.Out.SendGroupWindowUpdate();
				player.Out.SendQuestListUpdate();
			}
			
			UpdateGroupWindow();
			
            if (player != null)
            {
                player.Out.SendMessage("You leave your group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                player.Notify(GamePlayerEvent.LeaveGroup, player);
            }
            
            SendMessageToGroupMembers(string.Format("{0} has left the group.", living.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			
            
            // only one member left?
			if (MemberCount == 1)
			{
				// RR4: Group is disbanded, ending mission group if any
				RemoveMember(m_groupMembers.First());
			}
			
            // Update all members
			if (MemberCount > 1 && LivingLeader == living)
			{
				var newLeader = m_groupMembers.OfType<GamePlayer>().First();
				
				if (newLeader != null)
				{
					LivingLeader = newLeader;
					SendMessageToGroupMembers(string.Format("{0} is the new group leader.", Leader.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					// Set aother Living Leader.
					LivingLeader = m_groupMembers.First();
				}
			}
			
			UpdateGroupIndexes();
			GameEventMgr.Notify(GroupEvent.MemberDisbanded, this, new MemberDisbandedEventArgs(living));

			return true;
		}
		
		/// <summary>
		/// Clear this group
		/// </summary>
		public void DisbandGroup()
		{
			GroupMgr.RemoveGroup(this);
			
			if (Mission != null)
				Mission.ExpireMission();
			
			LivingLeader = null;
			m_groupMembers.Clear();
		}
		
		/// <summary>
		/// Updates player indexes
		/// </summary>
		private void UpdateGroupIndexes()
		{
			m_groupMembers.FreezeWhile(l => {
			                           	for (byte ind = 0; ind < l.Count; ind++)
			                           		l[ind].GroupIndex = ind;
			                           });
		}
		
		/// <summary>
		/// Makes living current leader of the group
		/// </summary>
		/// <param name="living"></param>
		/// <returns></returns>
		public bool MakeLeader(GameLiving living)
		{
			bool allOk = m_groupMembers.FreezeWhile<bool>(l => {
			                                        	if (!l.Contains(living))
			                                        		return false;
			                                        	
			                                        	byte ind = living.GroupIndex;
			                                        	var oldLeader = l[0];
			                                        	l[ind] = oldLeader;
			                                        	l[0] = living;
			                                        	LivingLeader = living;
			                                        	living.GroupIndex = 0;
			                                        	oldLeader.GroupIndex = ind;
			                                        	
			                                        	return true;
			                                        });
			if (allOk)
			{
				// all went ok
				UpdateGroupWindow();
				SendMessageToGroupMembers(string.Format("{0} is the new group leader.", Leader.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			return allOk;
		}
		#endregion
		
		#region messaging
		/// <summary>
		/// Sends a message to all group members with an object from
		/// </summary>
		/// <param name="from">GameLiving source of the message</param>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public virtual void SendMessageToGroupMembers(GameLiving from, string msg, eChatType type, eChatLoc loc)
		{
			string message;
			if (from != null)
			{
				message = string.Format("[Party] {0}: \"{1}\"", from.GetName(0, true), msg);
			}
			else
			{
				message = string.Format("[Party] {0}", msg);
			}

			SendMessageToGroupMembers(message, type, loc);
		}

		/// <summary>
		/// Send Raw Message to all group members.
		/// </summary>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public virtual void SendMessageToGroupMembers(string msg, eChatType type, eChatLoc loc)
		{
			foreach (GamePlayer player in GetPlayersInTheGroup())
				player.Out.SendMessage(msg, type, loc);
		}
		#endregion
		
		#region update group
		/// <summary>
		/// Updates a group member to all other living in the group
		/// </summary>
		/// <param name="living">living to update</param>
		/// <param name="updateIcons">Do icons need an update</param>
		/// <param name="updateOtherRegions">Should updates be sent to players in other regions</param>
		public void UpdateMember(GameLiving living, bool updateIcons, bool updateOtherRegions)
		{
			if (living.Group != this)
				return;
			
			foreach (var player in GetPlayersInTheGroup())
			{
				if (updateOtherRegions || player.CurrentRegion == living.CurrentRegion)
					player.Out.SendGroupMemberUpdate(updateIcons, true, living);
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
			if (player.Group != this)
				return;
			
			foreach (GameLiving living in m_groupMembers)
			{
				if (updateOtherRegions || living.CurrentRegion == player.CurrentRegion)
				{
					player.Out.SendGroupMemberUpdate(updateIcons, true, living);
				}
			}
		}

		/// <summary>
		/// Updates the group window to all players
		/// </summary>
		public void UpdateGroupWindow()
		{
			foreach (GamePlayer player in GetPlayersInTheGroup())
				player.Out.SendGroupWindowUpdate();
		}
		#endregion

		#region utils
		/// <summary>
		/// If at least one player is in combat group is in combat
		/// </summary>
		/// <returns>true if group in combat</returns>
		public bool IsGroupInCombat()
		{
			return m_groupMembers.Any(m => m.InCombat);
		}

		/// <summary>
		/// Checks if a living is inside the group
		/// </summary>
		/// <param name="living">GameLiving to check</param>
		/// <returns>true if the player is in the group</returns>
		public virtual bool IsInTheGroup(GameLiving living)
		{
			return m_groupMembers.Contains(living);
		}
		#endregion

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
				BattleGroup mybattlegroup = (BattleGroup)player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
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
				BattleGroup mybattlegroup = (BattleGroup)player.TempProperties.getProperty<object>(BattleGroup.BATTLEGROUP_PROPERTY, null);
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
	}
}
