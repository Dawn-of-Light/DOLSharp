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
using System;
using System.Reflection;
using DOL.Database;
using DOL.GS.Keeps;
using log4net;

namespace DOL.GS
{
	//-----------------------------------------------------------------------------------------------
	// GuildEntry
	//-----------------------------------------------------------------------------------------------
	public enum eGuildRank : int
	{
		Emblem,
		AcHear,
		AcSpeak,
		Demote,
		Promote,
		GcHear,
		GcSpeak,
		Invite,
		OcHear,
		OcSpeak,
		Remove,
		Leader,
		Alli,
		View,
		Claim,
		Upgrade,
		Release
	}
	/// <summary>
	/// Summary description for a Guild inside the game.
	/// </summary>
	/// 
	public class Guild
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
		protected readonly ArrayList m_guildMembers = new ArrayList();

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
		protected Alliance  m_alliance=null;


		/// <summary>
		/// This holds the DB instance of the guild
		/// </summary>
		protected DBGuild m_DBguild;

		/// <summary>
		/// the name of the guild
		/// </summary>
		protected string m_name;

		/// <summary>
		/// Holds the guild realm points
		/// </summary>
		protected long m_realmPoints;

		/// <summary>
		/// Holds the guild bounty points
		/// </summary>
		protected long m_bountyPoints;

		/// <summary>
		/// Stores claimed keep (unique)
		/// </summary>
		protected AbstractGameKeep m_claimedKeep;

		/// <summary>
		/// Stores guild unique run-time ID
		/// </summary>
		protected ushort m_id;

		/// <summary>
		/// Creates an empty Guild. Don't use this, use
		/// GuildMgr.CreateGuild() to create a guild
		/// </summary>
		public Guild()
		{
		}

		/// <summary>
		/// Gets or sets the guild db
		/// </summary>
		public DBGuild theGuildDB
		{
			get	{	return m_DBguild; }
			set	{	m_DBguild = value;	}
		}

		/// <summary>
		/// Gets or sets the guild db
		/// </summary>
		public Alliance  alliance
		{
			get	{	return m_alliance; }
			set	{	m_alliance = value;	}
		}

		/// <summary>
		/// Gets or sets the guild db
		/// </summary>
		public string Name
		{
			get	{	return m_name; }
			set	{	m_name = value; }
		}

		/// <summary>
		/// Gets or sets the guild realm points
		/// </summary>
		public long RealmPoints
		{
			get { return m_realmPoints; }
		}

		/// <summary>
		/// Gets or sets the guild realm points
		/// </summary>
		public long BountyPoints
		{
			get { return m_bountyPoints; }
		}

		/// <summary>
		/// Gets or sets the guild claimed keep
		/// </summary>
		public AbstractGameKeep ClaimedKeep
		{
			get { return m_claimedKeep; }
			set	{ m_claimedKeep = value; }
		}

		/// <summary>
		/// Returns the number of players online inside this guild
		/// </summary>
		public int MemberOnlineCount
		{
			get
			{
				return m_guildMembers.Count;
			}
		}

		/// <summary>
		/// Gets/Sets unique run-time guild ID
		/// </summary>
		public ushort ID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		public Quests.AbstractMission Mission = null;

		/// <summary>
		/// Adds a player to the guild
		/// </summary>
		/// <param name="member">GamePlayer to be added to the guild</param>
		/// <returns>true if added successfully</returns>
		public bool AddOnlineMember(GamePlayer member) 
		{
			lock(m_guildMembers.SyncRoot)
			{
				if(!m_guildMembers.Contains(member))
				{
					m_guildMembers.Add(member);
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes a player from the guild
		/// </summary>
		/// <param name="member">GamePlayer to be removed</param>
		/// <returns>true if removed, false if not</returns>
		public bool RemoveOnlineMember(GamePlayer member)
		{
			lock(m_guildMembers.SyncRoot)
			{
				if(m_guildMembers.Contains(member))
				{
					m_guildMembers.Remove(member);

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Remove all Members from memory
		/// </summary>
		public void RemoveAllMembers()
		{
			lock(m_guildMembers.SyncRoot)
			{
				m_guildMembers.Clear();
			}
		}

		/// <summary>
		/// Returns a guild according to the matching membername
		/// </summary>
		/// <returns>GuildMemberEntry</returns>
		public GamePlayer GetMemberByName(string memberName)
		{
			lock(m_guildMembers.SyncRoot)
			{
				foreach(GamePlayer member in m_guildMembers)
				{
					if(member.Name == memberName)
					{
						return member;
					}
				}
			}
			return null;
		}

		/// <summary>
		/// Add's a player to this Guild
		/// </summary>
		/// <param name="addeePlayer">the player beeing added</param>
		/// <returns>true or false</returns>
		public bool AddPlayer(GamePlayer addeePlayer)
		{
			if (log.IsDebugEnabled)
				log.Debug("Adding player to the guild, guild name=\""+Name+"\"; player name="+addeePlayer.Name);
			if (addeePlayer == null)
				return false;

			// guild name can't be null, it's set to "" if no guild
			//if (addeePlayer.GuildName != null) // Hey this should have been tested by the guild script!
			//	return false;

			try 
			{
				AddOnlineMember(addeePlayer);
				addeePlayer.GuildName = Name;
				addeePlayer.GuildRank = GetRankByID(9);
				addeePlayer.Guild = this;
				addeePlayer.SaveIntoDatabase();
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("AddPlayer", e);
				return false;
			}

			// No errors
			return true;
		}
		
		/// <summary>
		/// Delete's a member from this Guild
		/// </summary>
		/// <param name="removername">the player (client) removing</param>
		/// <param name="member">the player named beeing remove</param>
		/// <returns>true or false</returns>
		public bool RemovePlayer(string removername, GamePlayer member)
		{
			try
			{
				RemoveOnlineMember(member);
				member.GuildName = "";
				member.GuildRank = null;
				member.Guild = null;
				member.SaveIntoDatabase();

				// Send message to removerClient about successful removal
				member.Out.SendMessage(removername + " remove you from " + theGuildDB.GuildName, PacketHandler.eChatType.CT_System, PacketHandler.eChatLoc.CL_SystemWindow);
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("RemovePlayer", e);
				return false;
			}

			return true;
		}

		/// <summary>
		/// Looks up if a given client have access for the specific command in this guild
		/// </summary>
		/// <returns>true or false</returns>
		public bool GotAccess(GamePlayer member, eGuildRank rankneededforcommand)
		{
			try
			{
				// Is the player in the guild at all?
				if (!m_guildMembers.Contains(member))
					return false;

				// If player have a privlevel above 1, it has access enough
				if (member.Client.Account.PrivLevel > 1)
					return true;
				else // No guild leader, lets check if user rank is high enough in the guild
				{
					if (member.GuildRank == null)
					{
						if (log.IsWarnEnabled)
							log.Warn("Rank not in db for player "+member.Name);
						return false;
					}

					switch(rankneededforcommand)
					{
						case eGuildRank.Emblem: 
						{
							return member.GuildRank.Emblem;
						} 
						case eGuildRank.AcHear: 
						{
							return member.GuildRank.AcHear;
						} 
						case eGuildRank.AcSpeak: 
						{
							return member.GuildRank.AcSpeak;
						} 
						case eGuildRank.Demote: 
						{
							return member.GuildRank.Promote;
						} 
						case eGuildRank.Promote: 
						{
							return member.GuildRank.Promote;
						} 
						case eGuildRank.GcHear: 
						{
							return member.GuildRank.GcHear;
						} 
						case eGuildRank.GcSpeak: 
						{
							return member.GuildRank.GcSpeak;
						} 
						case eGuildRank.Invite: 
						{
							return member.GuildRank.Invite;
						} 
						case eGuildRank.OcHear: 
						{
							return member.GuildRank.OcHear;
						} 
						case eGuildRank.OcSpeak: 
						{
							return member.GuildRank.OcSpeak;
						} 
						case eGuildRank.Remove: 
						{
							return member.GuildRank.Remove;
						} 
						case eGuildRank.Alli: 
						{
							return member.GuildRank.Alli;
						} 
						case eGuildRank.View: 
						{
							return member.GuildRank.View;
						} 
						case eGuildRank.Claim: 
						{
							return member.GuildRank.Claim;
						}
						case eGuildRank.Release: 
						{
							return member.GuildRank.Release;
						} 
						case eGuildRank.Upgrade: 
						{
							return member.GuildRank.Upgrade;
						} 
						case eGuildRank.Leader: 
						{
							return (member.GuildRank.RankLevel == 0);
						} 
						default : 
						{
							if (log.IsWarnEnabled)
								log.Warn("Required rank not in the DB: "+rankneededforcommand);
							return false;
						}
					}
				}
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("GotAccess", e);
				return false;
			}
		}

		/// <summary>
		/// get rank by level
		/// </summary>
		/// <param name="index">the index of rank</param>
		/// <returns>the dbrank</returns>
		public DBRank GetRankByID(int index)
		{
			try
			{
				foreach (DBRank rank in theGuildDB.Ranks)
				{
					if ( rank.RankLevel == index )
						return rank;

				}
				return null;
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("GetRankByID", e);
				return null;
			}
		}

		/// <summary>
		/// Returns a list of members by their status
		/// </summary>
		/// <returns>ArrayList of members</returns>
		public ArrayList ListOnlineMembers()
		{
			return m_guildMembers;
		}

		/// <summary>
		/// Sends a message to all guild members 
		/// </summary>
		/// <param name="msg">message string</param>
		/// <param name="type">message type</param>
		/// <param name="loc">message location</param>
		public void SendMessageToGuildMembers(string msg, PacketHandler.eChatType type, PacketHandler.eChatLoc loc)
		{
			lock(m_guildMembers.SyncRoot)
			{
				foreach(GamePlayer pl in m_guildMembers)
				{
					if (!GotAccess(pl, eGuildRank.GcHear))
					{
						continue;
					}
					pl.Out.SendMessage(msg, type, loc);
				}
			}
		}

		/// <summary>
		/// Checks if a player is in the guild
		/// </summary>
		/// <param name="memberName">GamePlayer to check</param>
		/// <returns>true if the player is in the guild</returns>
		public bool IsInTheGuild(string memberName)
		{
			lock(m_guildMembers.SyncRoot)
			{
				foreach(GamePlayer member in m_guildMembers)
				{
					if(member.Name == memberName)
					{
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Called when this guild gains realm points
		/// </summary>
		/// <param name="amount">The amount of realm points gained</param>
		public virtual void GainRealmPoints(long amount)
		{
			lock (this)
			{
				m_realmPoints += amount;
				m_DBguild.RealmPoints = m_realmPoints;
			}
		}

		/// <summary>
		/// Called when this guild gains bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual void GainBountyPoints(long amount)
		{
			lock (this)
			{
				m_bountyPoints += amount;
				m_DBguild.BountyPoints = m_bountyPoints;
			}
		}
		/// <summary>
		/// Called when this guild loose bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual bool RemoveBountyPoints(long amount)
		{
			lock (this)
			{
				if (amount > m_bountyPoints) return false;
				m_bountyPoints -= amount;
				m_DBguild.BountyPoints = m_bountyPoints;
				return true;
			}
		}

		/// <summary>
		/// Loads this guild from a guild table
		/// </summary>
		/// <param name="obj"></param>
		public void LoadFromDatabase(DataObject obj)
		{
			if(!(obj is DBGuild))
				return;

			m_DBguild = (DBGuild)obj;
			m_name = m_DBguild.GuildName;
			m_realmPoints = m_DBguild.RealmPoints;
			m_bountyPoints = m_DBguild.BountyPoints;
		}

		/// <summary>
		/// Saves this guild to database
		/// </summary>
		public void SaveIntoDatabase()
		{
			GameServer.Database.SaveObject(theGuildDB);
		}
	}
}
