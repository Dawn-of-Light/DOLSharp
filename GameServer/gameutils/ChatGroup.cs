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
using DOL.GS.PacketHandler;
namespace DOL.GS
{
	/// <summary>
	/// Description résumée de ChatGroup.
	/// </summary>
	public class ChatGroup
	{
		public const string CHATGROUP_PROPERTY="chatgroup";
		/// <summary>
		/// This holds all players inside the chatgroup
		/// </summary>
		protected HybridDictionary m_chatgroupMembers = new HybridDictionary();

		/// <summary>
		/// constructor of chat group
		/// </summary>
		public ChatGroup()
		{
		}
		public HybridDictionary Members
		{
			get{return m_chatgroupMembers;}
			set{m_chatgroupMembers=value;}
		}
		private bool listen=false;
		public bool Listen
		{
			get{return listen;}
			set{listen = value;}
		}
		private bool ispublic=true;
		public bool IsPublic
		{
			get{return ispublic;}
			set{ispublic = value;}
		}
		private string password="";
		public string Password
		{
			get{return password;}
			set{password = value;}
		}

		/// <summary>
		/// Adds a player to the chatgroup
		/// </summary>
		/// <param name="player">GamePlayer to be added to the group</param>
		/// <returns>true if added successfully</returns>
		public virtual bool AddPlayer(GamePlayer player,bool leader) 
		{
			if (player == null) return false;
			lock (m_chatgroupMembers)
			{
				if (m_chatgroupMembers.Contains(player))
					return false;
				player.TempProperties.setProperty(CHATGROUP_PROPERTY, this);
				player.Out.SendMessage("You join the chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach(GamePlayer member in Members.Keys)
				{
					member.Out.SendMessage(player.Name+" has joined the chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				m_chatgroupMembers.Add(player,leader);
			}
			return true;
		}

		/// <summary>
		/// Removes a player from the group
		/// </summary>
		/// <param name="player">GamePlayer to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public virtual bool RemovePlayer(GamePlayer player)
		{
			if (player == null) return false;
			lock (m_chatgroupMembers)
			{
				if (!m_chatgroupMembers.Contains(player))
					return false;
				m_chatgroupMembers.Remove(player);
				player.TempProperties.removeProperty(CHATGROUP_PROPERTY);
				player.Out.SendMessage("You leave the chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach(GamePlayer member in Members.Keys)
				{
					member.Out.SendMessage(player.Name+" has left the chat group.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				if (m_chatgroupMembers.Count == 1)
				{
					ArrayList lastPlayers = new ArrayList(m_chatgroupMembers.Count);
					lastPlayers.AddRange(m_chatgroupMembers.Keys);
					foreach (GamePlayer plr in lastPlayers)
					{
						RemovePlayer(plr);
					}
				}
			}
			return true;
		}
	}
}
