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
using DOL.GS.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Alliance are the alliance between guild in game
	/// </summary>
	public class Alliance
	{
		#region Declaraction
		/// <summary>
		/// The unique alliance identifier
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Holds the alliance motd
		/// </summary>
		private string	m_amotd;

		/// <summary>
		/// Holds all alliance guilds
		/// </summary>
		protected IList m_allianceGuilds;

		/// <summary>
		/// Holds the alliance leader
		/// </summary>
		protected Guild m_allianceLeader;

		/// <summary>
		/// Gets or sets the unique alliance identifier
		/// </summary>
		public int AllianceID
		{
			get	{ return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or sets the alliance motd
		/// </summary>
		public string AMotd
		{
			get { return m_amotd; }
			set	{ m_amotd = value; }
		}

		/// <summary>
		/// Gets or sets all alliances guilds
		/// </summary>
		public IList AllianceGuilds
		{
			get
			{
				if(m_allianceGuilds == null) m_allianceGuilds = new ArrayList();
				return m_allianceGuilds;
			}
			set
			{
				m_allianceGuilds = value;
			}
		}

		/// <summary>
		/// Gets or sets the alliance leader
		/// </summary>
		public Guild AllianceLeader
		{
			get
			{
				return m_allianceLeader;
			}
			set
			{
				m_allianceLeader = value;
			}
		}
		#endregion

		#region AddGuildToAlliance / RemoveGuildFromAlliance
		/// <summary>
		/// Add a guild to the alliance
		/// </summary>
		/// <param name="guildToAdd">the guild to add</param>
		public bool AddGuildToAlliance(Guild guildToAdd)
		{
			if(guildToAdd.Alliance.AllianceLeader != guildToAdd || guildToAdd.Alliance.AllianceGuilds.Count > 1) return false; //can't join a alliance if already in another

			if(m_allianceGuilds.Contains(guildToAdd)) return false; // already a member of this alliance
			
			GameServer.Database.DeleteObject(guildToAdd.Alliance);  //delete the empty alliance

			guildToAdd.Alliance = this;
			lock(m_allianceGuilds.SyncRoot)
			{
				m_allianceGuilds.Add(guildToAdd);
			}
			GameServer.Database.SaveObject(guildToAdd);
			return true;
		}

		/// <summary>
		/// Remove a guild to the alliance
		/// </summary>
		/// <param name="guildToRemove">the guild to remove</param>
		public bool RemoveGuildFromAlliance(Guild guildToRemove)
		{
			if(m_allianceLeader == guildToRemove) return false; //can't leave your own alliance

			lock(m_allianceGuilds.SyncRoot)
			{
				if(!m_allianceGuilds.Contains(guildToRemove)) return false;// guild not in the alliance
			
				m_allianceGuilds.Remove(guildToRemove);
			}

			guildToRemove.Alliance = new Alliance();
			guildToRemove.Alliance.AMotd = "Your guild have no alliances.";
			guildToRemove.Alliance.AllianceLeader = guildToRemove;
			guildToRemove.Alliance.AllianceGuilds.Add(guildToRemove);
			
			GameServer.Database.SaveObject(guildToRemove);	// save the guild in a new empty alliance
			
			if(m_allianceGuilds.Count <= 1) // only the leader guild stay in the alliance => empty alliance
			{
				m_amotd = "Your guild have no alliances.";

				SendMessageToAllianceMembers("Your alliance has been deleted!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				GameServer.Database.SaveObject(this);
			}
				
			return true;
		}

		#endregion
		
		#region SendMessageToAllianceMembers
		/// <summary>
		/// send message to all member of alliance
		/// </summary>
		public void SendMessageToAllianceMembers(string msg, eChatType type, eChatLoc loc)
		{
			lock (m_allianceGuilds)
			{
				foreach(Guild allie in m_allianceGuilds)
				{
					allie.SendMessageToGuildMembers(msg, type, loc);	// send the message to all others allies
				}
			}
		}
		#endregion
	}
}
