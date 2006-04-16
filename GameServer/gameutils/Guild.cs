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
using DOL.GS.Database;
using log4net;

namespace DOL.GS
{
	
	/// <summary>
	/// Defines the guilds permission
	/// </summary>
	public enum eGuildPerm : byte
	{
		/// <summary>
		/// First permission, for use in all arrays
		/// </summary>
		_First = 0,
		/// <summary>
		/// Hear guild chat
		/// </summary>
		GcHear = 0,
		/// <summary>
		/// Speak guild chat
		/// </summary>
		GcSpeak = 1,
		/// <summary>
		/// Hear officer guild chat
		/// </summary>
		OcHear = 2,
		/// <summary>
		/// Speak officer guild chat
		/// </summary>
		OcSpeak = 3,
		/// <summary>
		/// Hear alliance guild chat
		/// </summary>
		AcHear = 4,
		/// <summary>
		/// Speak alliance guild chat
		/// </summary>
		AcSpeak = 5,
		/// <summary>
		/// View guild informations
		/// </summary>
		View = 6,
		/// <summary>
		/// Can player wear the emblem
		/// </summary>
		Emblem = 7,
		/// <summary>
		/// Can invite players to the guild
		/// </summary>
		Invite = 8,
		/// <summary>
		/// Can remove players from the guild
		/// </summary>
		Remove = 9,
		/// <summary>
		/// Can promote guild members
		/// </summary>
		Promote = 10,
		/// <summary>
		/// Can claim a keep
		/// </summary>
		Claim = 11,
		/// <summary>
		/// Can upgrade a keep
		/// </summary>
		Upgrade = 12,
		/// <summary>
		/// Can release a keep
		/// </summary>
		Release = 13,
		/// <summary>
		/// Can create a alliance
		/// </summary>
		Alli = 14,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		Motd = 15,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		Deposit = 16,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		Withdraw = 17,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		Dues = 18,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		Buff = 19,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		GetMission = 20,
		/// <summary>
		/// Can edit the motd
		/// </summary>
		SetNote = 21,
		/// <summary>
		/// Can summon a banner
		/// </summary>
		SummonBanner = 22,
		/// <summary>
		/// Can buy a banner
		/// </summary>
		BuyBanner = 23,
		/// <summary>
		/// Last permission, for use in all arrays
		/// </summary>
		_Last = 15,
	}

	/// <summary>
	/// Summary description for a Guild
	/// </summary> 
	public class Guild
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Declaraction

		/// <summary>
		/// Max guild name length
		/// </summary>
		public const int MAX_GUILD_NAME_LENGTH = 30;

		/// <summary>
		/// The unique guild identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
		private readonly ArrayList m_guildMembers = new ArrayList();

		/// <summary>
		/// the name of the guild
		/// </summary>
		private string m_guildName;

		/// <summary>
		/// Holds the guild motd
		/// </summary>
		private string	m_motd;

		/// <summary>
		/// Holds the officier motd
		/// </summary>
		private string	m_omotd;

		/// <summary>
		/// Holds the guild emblem
		/// </summary>
		private int		m_emblem;

		/// <summary>
		/// Holds the guild realm points
		/// </summary>
		private long m_realmPoints;

		/// <summary>
		/// Holds the guild bounty points
		/// </summary>
		private long m_bountyPoints;

		/// <summary>
		/// Holds the guild merit points
		/// </summary>
		private long m_meritPoints;

		/// <summary>
		/// Holds the guild due
		/// </summary>
		private bool m_due;

		/// <summary>
		/// Holds the guild money
		/// </summary>
		private long m_totalMoney;

		/// <summary>
		/// Holds the guild lvl
		/// </summary>
		private int m_level;

		/// <summary>
		/// Holds the guild webpage
		/// </summary>
		private string m_webpage;

		/// <summary>
		/// Holds the guild email
		/// </summary>
		private string m_email;

		/// <summary>
		/// Holds all guild ranks with their permissions
		/// </summary>
		private DBGuildRank[] m_guildRanks;

		/// <summary>
		/// Holds the guild claimed keep
		/// </summary>
		//private AbstractGameKeep m_claimedKeep;

		/// <summary>
		/// Holds the alliance the guild is on
		/// </summary>
		private Alliance m_alliance;

		/// <summary>
		/// Gets or sets the unique guild identifier
		/// </summary>
		public int GuildID
		{
			get	{ return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or sets the guild db
		/// </summary>
		public string GuildName
		{
			get	{	return m_guildName; }
			set	{	m_guildName = value; }
		}

		/// <summary>
		/// Gets or sets the guild motd
		/// </summary>
		public string Motd
		{
			get { return m_motd; }
			set	{ m_motd = value; }
		}

		/// <summary>
		/// Gets or sets the guild officer motd
		/// </summary>
		public string OMotd
		{
			get { return m_omotd; }
			set	{ m_omotd = value; }
		}

		/// <summary>
		/// Gets or sets the guild officer motd
		/// </summary>
		public int Emblem
		{
			get { return m_emblem; }
			set	{ m_emblem = value; }
		}

		/// <summary>
		/// Gets or sets the guild realm points
		/// </summary>
		public long RealmPoints
		{
			get { return m_realmPoints; }
			set	{ m_realmPoints = value; }
		}

		/// <summary>
		/// Gets or sets the guild realm points
		/// </summary>
		public long BountyPoints
		{
			get { return m_bountyPoints; }
			set	{ m_bountyPoints = value; }
		}

		/// <summary>
		/// Gets or sets the guild realm points
		/// </summary>
		public long MeritPoints
		{
			get { return m_meritPoints; }
			set { m_meritPoints = value; }
		}

		/// <summary>
		/// Gets or sets the guild due
		/// </summary>
		public bool Due
		{
			get { return m_due; }
			set { m_due = value; }
		}

		/// <summary>
		/// Gets or sets the guild total money
		/// </summary>
		public long TotalMoney
		{
			get { return m_totalMoney; }
			set { m_totalMoney = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public int Level
		{
			get { return m_level; }
			set { m_level = value; }
		}

		/// <summary>
		/// Gets or sets the guild webpage
		/// </summary>
		public string Webpage
		{
			get { return m_webpage; }
			set { m_webpage = value; }
		}

		/// <summary>
		/// Gets or sets the guild email
		/// </summary>
		public string Email
		{
			get { return m_email; }
			set { m_email = value; }
		}

		/// <summary>
		/// Returns a list of online members inside this guild
		/// </summary>
		/// <returns>ArrayList of members</returns>
		public IList ListOnlineMembers
		{
			get { return m_guildMembers; }
		}

		/// <summary>
		/// Returns a array of all guild ranks with their permissions
		/// </summary>
		/// <returns>ArrayList of members</returns>
		public DBGuildRank[] GuildRanks
		{
			get
			{
				if(m_guildRanks == null)
				{
					// create default guild ranks perm
					m_guildRanks =  new DBGuildRank[10];
					for (byte i = 0 ; i < m_guildRanks.Length ; i++)
					{
						DBGuildRank rank = new DBGuildRank();
						rank.RankLevel = i;
						rank.Title = "";
						
						rank.GcHear = true;
						if(i < 9) rank.GcSpeak = true;	else rank.GcSpeak = false;
						if(i < 9) rank.View = true;		else rank.View = false;
						if(i < 8) rank.Emblem = true;	else rank.Emblem = false;
						if(i < 6) rank.AcSpeak = true;	else rank.AcSpeak = false;
						if(i < 7) rank.AcHear = true;	else rank.AcHear = false;
						if(i < 5) rank.OcHear = true;	else rank.OcHear = false;
						if(i < 4) rank.OcSpeak = true;	else rank.OcSpeak = false;
						if(i < 3) rank.Invite = true;	else rank.Invite = false;
						if(i < 3) rank.Promote = true;	else rank.Promote = false;
						if(i < 2) rank.Release = true;	else rank.Release = false;
						if(i < 2) rank.Upgrade = true;	else rank.Upgrade = false;
						if(i < 2) rank.Claim = true;	else rank.Claim = false;
						if(i < 1) rank.Remove = true;	else rank.Remove = false;
						if(i < 1) rank.Alli = true;		else rank.Alli = false;
						if(i < 1) rank.Motd = true;		else rank.Motd = false;
						if(i < 1) rank.Deposit = true;	else rank.Deposit = false;
						if(i < 1) rank.Withdraw = true; else rank.Withdraw = false;
						if(i < 1) rank.Dues = true;		else rank.Dues = false;
						if(i < 1) rank.Buff = true;		else rank.Buff = false;
						if(i < 1) rank.GetMission = true;	else rank.GetMission = false;
						if(i < 1) rank.SetNote = true;	else rank.SetNote = false;
						if(i < 1) rank.SummonBanner = true;	else rank.SummonBanner = false;
						if(i < 1) rank.BuyBanner = true;	else rank.BuyBanner = false;
						m_guildRanks[rank.RankLevel] = rank;
						
						GameServer.Database.AddNewObject(rank);
					}
				}
				return m_guildRanks;
			}
			set	{ m_guildRanks = value; }
		}
		
		/// <summary>
		/// Gets or sets the guild claimed keep
		/// </summary>
		/*public AbstractGameKeep ClaimedKeep
		{
			get { return m_claimedKeep; }
			set	{ m_claimedKeep = value; }
		}*/

		/// <summary>
		/// Gets or sets the alliance the guild is on
		/// </summary>
		public Alliance Alliance
		{
			get { return m_alliance; }
			set	{ m_alliance = value; }
		}
		#endregion

		#region AddGuildMember / RemoveOnlineMember

		/// <summary>
		/// Add a online member
		/// </summary>
		/// <param name="member">the player named beeing added</param>
		/// <returns>true or false</returns>
		public bool AddOnlineMember(GamePlayer member)
		{
			lock(m_guildMembers.SyncRoot)
			{
				if(!m_guildMembers.Contains(member))
				{
					m_guildMembers.Add(member);
					member.Guild = this;

					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Remove a online member
		/// </summary>
		/// <param name="member">the player named beeing remove</param>
		/// <returns>true or false</returns>
		public bool RemoveOnlineMember(GamePlayer member)
		{
			lock(m_guildMembers.SyncRoot)
			{
				if(m_guildMembers.Contains(member))
				{
					m_guildMembers.Remove(member);
					member.Guild = null;

					return true;
				}
			}
			return false;
		}
		
		/// <summary>
		/// Add's a player to this Guild (the player must not be in another guild !!!
		/// </summary>
		/// <param name="player">the player beeing added</param>
		/// <returns>true or false</returns>
		public bool AddGuildMember(GamePlayer player, byte startingRank)
		{
			if(AddOnlineMember(player))
			{
				player.Guild = this;
				player.GuildRank = startingRank;

				if (log.IsInfoEnabled)
					log.Info("Adding player ("+player.Name+") to the guild ("+GuildName+")");

				return true;
			}
			return false;
		}

		/// <summary>
		/// Delete's a member from this Guild
		/// </summary>
		/// <param name="player">the player named beeing remove</param>
		/// <returns>true or false</returns>
		public bool RemoveGuildMember(GamePlayer player)
		{
			if (RemoveOnlineMember(player) == true)
			{
				player.Guild = null;
				player.GuildRank = 9;

				if (log.IsInfoEnabled)
					log.Info("Removing player ("+player.Name+") from the guild ("+GuildName+")");
			
				return true;
			}
			return false;
		}
		#endregion

		#region CheckGuildPermission / SendMessageToGuildMembers
		/// <summary>
		/// Looks up if a given client have access for the specific command in this guild
		/// </summary>
		/// <returns>true or false</returns>
		public bool CheckGuildPermission(GamePlayer member, eGuildPerm permToCheck)
		{
			// If player have a privlevel above 1, it has access enough
			if (member.Client.Account.PrivLevel > ePrivLevel.Player) return true;

			// Is the player in the guild at all?
			if (!m_guildMembers.Contains(member)) return false;

			switch(permToCheck)
			{
				case eGuildPerm.GcHear: 
				{
					return GuildRanks[member.GuildRank].GcHear;
				} 
				case eGuildPerm.GcSpeak: 
				{
					return GuildRanks[member.GuildRank].GcSpeak;
				} 
				case eGuildPerm.OcHear: 
				{
					return GuildRanks[member.GuildRank].OcHear;
				} 
				case eGuildPerm.OcSpeak: 
				{
					return GuildRanks[member.GuildRank].OcSpeak;
				} 
				case eGuildPerm.AcHear: 
				{
					return GuildRanks[member.GuildRank].AcHear;
				} 
				case eGuildPerm.AcSpeak: 
				{
					return GuildRanks[member.GuildRank].AcSpeak;
				} 
				case eGuildPerm.View: 
				{
					return GuildRanks[member.GuildRank].View;
				} 
				case eGuildPerm.Emblem: 
				{
					return GuildRanks[member.GuildRank].Emblem;
				} 
				case eGuildPerm.Invite: 
				{
					return GuildRanks[member.GuildRank].Invite;
				} 
				case eGuildPerm.Remove: 
				{
					return GuildRanks[member.GuildRank].Remove;
				}
				case eGuildPerm.Promote: 
				{
					return GuildRanks[member.GuildRank].Promote;
				}
				case eGuildPerm.Claim: 
				{
					return GuildRanks[member.GuildRank].Claim;
				} 
				case eGuildPerm.Upgrade: 
				{
					return GuildRanks[member.GuildRank].Upgrade;
				}
				case eGuildPerm.Release: 
				{
					return GuildRanks[member.GuildRank].Release;
				} 
				case eGuildPerm.Alli: 
				{
					return GuildRanks[member.GuildRank].Alli;
				} 
				case eGuildPerm.Motd:
				{
					return GuildRanks[member.GuildRank].Motd;
				}
				case eGuildPerm.Deposit:
				{
					return GuildRanks[member.GuildRank].Deposit;
				}
				case eGuildPerm.Withdraw:
				{
					return GuildRanks[member.GuildRank].Withdraw;
				}
				case eGuildPerm.Dues:
				{
					return GuildRanks[member.GuildRank].Dues;
				}
				case eGuildPerm.Buff:
				{
					return GuildRanks[member.GuildRank].Buff;
				}
				case eGuildPerm.GetMission:
				{
					return GuildRanks[member.GuildRank].GetMission;
				}
				case eGuildPerm.SetNote:
				{
					return GuildRanks[member.GuildRank].SetNote;
				}
				case eGuildPerm.SummonBanner:
				{
					return GuildRanks[member.GuildRank].SummonBanner;
				}
				case eGuildPerm.BuyBanner:
				{
					return GuildRanks[member.GuildRank].BuyBanner;
				} 
			}
			return false;
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
				foreach(GamePlayer player in m_guildMembers)
				{
					if (player.Client.IsPlaying)
						player.Out.SendMessage(msg, type, loc);
				}
			}
		}
		#endregion
	}
}
