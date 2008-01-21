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
using System.Runtime.Serialization;
using DOL.Database2;
using DOL.Database2;
using DOL.Language;
using DOL.GS.Keeps;
using log4net;
using DOL.GS.Housing;
using DOL.Regiment;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	//-----------------------------------------------------------------------------------------------
	// GuildEntry
	//-----------------------------------------------------------------------------------------------
	[Serializable]
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
		Release,
        Buff,
        Dues,
        Withdraw
	}
	/// <summary>
	/// Summary description for a Guild inside the game.
	/// </summary>
	/// 
    [Serializable]
    
	public class Guild :DatabaseObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
        [NonSerialized]
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
		protected readonly ArrayList m_guildMembers = new ArrayList();

		/// <summary>
		/// This holds all players inside the guild
		/// </summary>
        [NonSerialized]
		protected Alliance  m_alliance=null;

        /// <summary>
        /// Alliance Id
        /// </summary>
        protected UInt64 m_alliance_id;

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
        [NonSerialized]
		protected AbstractGameKeep m_claimedKeep;

        protected UInt64 m_claimedKeepId;

        protected double m_guildBank;
        protected bool guildDues;
        protected long guildDuesPercent;
        protected bool haveGuildHouse;
        protected int GuildHouseNumber;

        public DBRank[] Ranks;

        public ushort RuntimeID;
        public int GetGuildHouseNumber()
        {
            return GuildHouseNumber;
        }
        public bool GuildOwnsHouse()
        {
            return haveGuildHouse;
        }
        public double GetGuildBank()
        {
            return m_guildBank;
        }
        public bool IsGuildDuesOn()
        {
            return guildDues;
        }
        public long GetGuildDuesPercent()
        {
            return guildDuesPercent;
        }

        public void SetGuildHouseNumber(int num)
        {
            GuildHouseNumber = num;
        }
        public void SetGuildHouse(bool owns)
        {
            haveGuildHouse = owns;
        }
        public void SetGuildDues(bool dues)
        {
            if (dues == true)
            {
                guildDues = true;
            }
            else
            {
                guildDues = false;
            }
        }
        public void SetGuildDuesPercent(long dues)
        {
            if (IsGuildDuesOn() == true)
            {
                guildDuesPercent = dues;
            }
            else
            {
                guildDuesPercent = 0;
            }
        }
        /// <summary>
        /// Set guild bank command 
        /// </summary>
        /// <param name="donating"></param>
        /// <param name="ammount"></param>
        /// <returns></returns>
        public void SetGuildBank(GamePlayer donating, double ammount)
        {
            if (ammount < 0)
            {
                donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Client, "Scripts.Player.Guild.DepositInvalid"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                return;
            }
            else if (donating.Guild.GetGuildBank() >= 1000000001)
            {
                donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Client, "Scripts.Player.Guild.DepositFull"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                return;
            }

            donating.Out.SendMessage(LanguageMgr.GetTranslation(donating.Client, "Scripts.Player.Guild.DepositAmmount", ammount), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            if (m_guildBank == 0)
                m_guildBank = ammount;
            else
                m_guildBank = m_guildBank + ammount;

            donating.Guild.UpdateGuildWindow();
            return;
        }
        public void WithdrawGuildBank(GamePlayer withdraw, double ammount)
        {
            if (ammount < 0)
            {
                withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Client, "Scripts.Player.Guild.WithdrawInvalid"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                return;
            }
            else if (withdraw.Guild.GetGuildBank() >= 1000000001)
            {
                withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Client, "Scripts.Player.Guild.WithdrawTooMuch"), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
                return;
            }

            withdraw.Out.SendMessage(LanguageMgr.GetTranslation(withdraw.Client, "Scripts.Player.Guild.WithdrawAmmount", ammount), eChatType.CT_Guild, eChatLoc.CL_SystemWindow);
            m_guildBank = m_guildBank - ammount;
            withdraw.Guild.UpdateGuildWindow();
            return;
        }
        /// <summary>
		/// Creates an empty Guild. Don't use this, use
		/// GuildMgr.CreateGuild() to create a guild
		/// </summary>
		public Guild()
		{
		}

        protected bool m_guildBanner;

        public bool GuildBanner
        {
            get { return m_guildBanner; }
            set
            {
                m_guildBanner = value;
            }
        }


		/// <summary>
		/// Gets or sets the guild alliance
		/// </summary>
		public Alliance  alliance
		{
			get	
            {
                //runtime initialization
                if(m_alliance ==null)
                if (m_alliance_id != null)
                {
                    if (!DatabaseLayer.Instance.DatabaseObjects.TryGetValue(m_alliance_id, m_alliance))
                    {
                        log.Error("Could not get Alliance of Guild " + m_name + "with key" + m_alliance_id);
                    }
                }
                return m_alliance; 
            }
            set 
            { 
                m_alliance = value;
                if (value != null)
                    m_alliance_id = value.ID;
                else
                    m_alliance_id = null;
            }
		}

		/// <summary>
		/// Gets or sets the constant guild id
		/// </summary>
		public UInt64 GuildID
		{
			get	{	return ID ; }
		}



		/// <summary>
		/// Gets or sets the guild name
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
			get {
                //runtime initialization

                if(m_claimedKeepId != null)
                {
                    if (!DatabaseLayer.Instance.DatabaseObjects.TryGetValue(m_claimedKeepId, m_claimedKeep))
                    {
                        log.Error("Could not get Keep of Guild " + m_name + "with key" + m_claimedKeepId);
                    }
                }
                return m_claimedKeep; }
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
					if (!member.IsAnonymous)
						NotifyGuildMembers(member);
					m_guildMembers.Add(member);
					return true;
				}
			}

			return false;
		}

		private void NotifyGuildMembers(GamePlayer member)
		{
			foreach (GamePlayer player in m_guildMembers)
			{
				if (player == member) continue;
				if (player.ShowGuildLogins)
					player.Out.SendMessage("Guild member " + member.Name + " has logged in!", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
			}
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
				addeePlayer.GuildID = GuildID;
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
				member.GuildNote = "";
				member.GuildID = "";
				member.GuildRank = null;
				member.Guild = null;
				member.SaveIntoDatabase();

				member.Out.SendObjectGuildID(member, member.Guild);
				// Send message to removerClient about successful removal
				if (removername == member.Name)
					member.Out.SendMessage("You leave the guild.", DOL.GS.PacketHandler.eChatType.CT_System, DOL.GS.PacketHandler.eChatLoc.CL_SystemWindow);
				else
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
                        case eGuildRank.Dues:
                        {
                            return member.GuildRank.Dues;
                        }
                        case eGuildRank.Withdraw:
                        {
                            return member.GuildRank.Withdraw;
                        }
						case eGuildRank.Leader: 
						{
							return (member.GuildRank.RankLevel == 0);
						}
                        case eGuildRank.Buff:
                        {
                            return member.GuildRank.Buff;
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
				foreach (DBRank rank in Ranks)
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
			m_realmPoints += amount;
		}

		/// <summary>
		/// Called when this guild gains bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual void GainBountyPoints(long amount)
		{
			m_bountyPoints += amount;
		}

		/// <summary>
		/// Called when this guild loose bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual bool RemoveBountyPoints(long amount)
		{
			if (amount > m_bountyPoints)
				amount = m_bountyPoints;

			m_bountyPoints -= amount;
			return true;
		}

		/// <summary>
		/// Loads this guild from a guild table
		/// </summary>
		/// <param name="obj"></param>
		/*public void LoadFromDatabase(DatabaseObject obj)
		{
			if(!(obj is DBGuild))
				return;

			m_DBguild = (DBGuild)obj;
			m_guildid = m_DBguild.GuildID;			
			m_name = m_DBguild.GuildName;
			m_realmPoints = m_DBguild.RealmPoints;
			m_bountyPoints = m_DBguild.BountyPoints;
            m_BuffTime = m_DBguild.BuffTime;
            m_BuffType = m_DBguild.BuffType;
            m_GuildLevel = m_DBguild.GuildLevel;
            guildDues = m_DBguild.Dues;
            m_guildBank = m_DBguild.Bank;
            m_meritPoints = m_DBguild.MeritPoints;
            guildDuesPercent = m_DBguild.DuesPercent;
            bannerStatus = "None";
		}*/

        /// <summary>
        /// Gets or sets the guild merit points
        /// </summary>
        public long MeritPoints
        {
            get { return m_meritPoints; }
            set { m_meritPoints = value; }
        }

        /// <summary>
        /// Gets or sets the guildlevel
        /// </summary>
        public long GuildLevel
        {
            get { return m_GuildLevel; }
            set { m_GuildLevel = value; }
        }

        /// <summary>
        /// Gets or sets the guild buff type
        /// </summary>
        public long BuffType
        {
            get { return m_BuffType; }
            set { m_BuffType = value; }
        }

        //First time run this QRY -> update guild set BuffTime=NOW(); to set the bufftime properly
        /// <summary>
        /// Gets or sets the guild buff time
        /// </summary>
        public DateTime BuffTime
        {
            get { return m_BuffTime; }
            set { m_BuffTime = value; }
        }

        /// <summary>
        /// Called when this guild gains merit points
        /// </summary>
        /// <param name="amount">The amount of bounty points gained</param>
        public virtual void GainMeritPoints(long amount)
        {
            MeritPoints += amount;
            UpdateGuildWindow();
        }

        /// <summary>
        /// Called when this guild loose bounty points
        /// </summary>
        /// <param name="amount">The amount of bounty points gained</param>
        public virtual bool RemoveMeritPoints(long amount)
        {
            if (amount > MeritPoints)
                amount = MeritPoints;

            MeritPoints -= amount;
            UpdateGuildWindow();
            return true;
        }

        /// <summary>
        /// Called when this guild gains a level
        /// </summary>
        /// <param name="amount">The amount of bounty points gained</param>
        public virtual void GainGuildLevel(int amount)
        {
            GuildLevel += amount;
            UpdateGuildWindow();
        }

        /// <summary>
        /// Holds the guild merit points
        /// </summary>
        protected long m_meritPoints;

        /// <summary>
        /// Holds the guild level
        /// </summary>
        private long m_GuildLevel;

        /// <summary>
        /// Holds the guild buff type
        /// </summary>
        private long m_BuffType;

        /// <summary>
        /// Holds the guild buff time
        /// </summary>
        private DateTime m_BuffTime;

        public GamePlayer GetGuildLeader(GamePlayer plr)
        {
    		if (!m_guildMembers.Contains(plr))
	    		return null;
            
            for (int i = 0; i < m_guildMembers.Count; i++)
            {
                if (((GamePlayer)m_guildMembers[i]).Guild.GotAccess(((GamePlayer)m_guildMembers[i]), eGuildRank.Leader) && ((GamePlayer)m_guildMembers[i]).IsAlive)
                    return ((GamePlayer)m_guildMembers[i]);
            }
            return null;
        }

        private string bannerStatus;
        public string GuildBannerStatus(GamePlayer player)
        {
            if (player.Guild != null)
            {
                if (player.Guild.GuildBanner)
                {
                    foreach (GamePlayer plr in player.Guild.ListOnlineMembers())
                    {
                        if (plr.IsCarryingGuildBanner)
                        {
                            bannerStatus = "Summoned";
                        }
                    }
                    if (bannerStatus == "None")
                    {
                        bannerStatus = "Not Summoned";
                    }
                    return bannerStatus;
                }
            }
            return bannerStatus;
        }
        public void UpdateMember(GamePlayer player)
        {
            if (player.Guild != this)
                return;
            int housenum;
            if (player.Guild.GuildOwnsHouse())
            {
                housenum = player.Guild.GetGuildHouseNumber();
            }
            else
                housenum = 0;

            string mes = "I";
            mes += ',' + player.Guild.GuildLevel.ToString(); // Guild Level
            mes += ',' + player.Guild.GetGuildBank().ToString(); // Guild Bank money
            mes += ',' + player.Guild.GetGuildDuesPercent().ToString(); // Guild Dues enable/disable
            mes += ',' + player.Guild.BountyPoints.ToString(); // Guild Bounty
            mes += ',' + player.Guild.RealmPoints.ToString(); // Guild Experience
            mes += ',' + player.Guild.MeritPoints.ToString(); // Guild Merit Points
            mes += ',' + housenum.ToString(); // Guild houseLot ?
            mes += ',' + (player.Guild.MemberOnlineCount + 1).ToString(); // online Guild member ?
            mes += ',' + player.Guild.GuildBannerStatus(player); //"Banner available for purchase", "Missing banner buying permissions"
            mes += ",\"" + player.Guild.theGuildDB.Motd + '\"'; // Guild Motd
            mes += ",\"" + player.Guild.theGuildDB.oMotd + '\"'; // Guild oMotd
            player.Out.SendMessage(mes, eChatType.CT_SocialInterface, eChatLoc.CL_SystemWindow);
            player.Guild.SaveIntoDatabase();
        }

        public void UpdateGuildWindow()
        {
            lock (m_guildMembers)
            {
                long newgLevel;
                if (GuildLevel < SocialEventHandler.REALMPOINTS_FOR_GUILDLEVEL.Length - 1)
                {
                    newgLevel = CalculateGuildLevelFromRPs(RealmPoints);

                    if (newgLevel > 0 || newgLevel < 120)
                    {
                        if (newgLevel > GuildLevel)
                        {
                            GuildLevel = newgLevel;
                            foreach (GamePlayer player in ListOnlineMembers())
                            {
                                player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Scripts.Player.Guild.LevelUp", GuildLevel), eChatType.CT_Guild, eChatLoc.CL_ChatWindow);
                            }
                        }
                    }
                    else if (newgLevel > 120)
                    {
                        GuildLevel = 120;
                    }
                }

                foreach (GamePlayer player in m_guildMembers)
                {
                    player.Guild.UpdateMember(player);
                }
            }
        }

        public virtual long CalculateRPsFromGuildLevel(long realmLevel)
        {
            if (realmLevel < SocialEventHandler.REALMPOINTS_FOR_GUILDLEVEL.Length)
                return SocialEventHandler.REALMPOINTS_FOR_GUILDLEVEL[realmLevel];

            // thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
            return (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);
        }

        public virtual long CalculateGuildLevelFromRPs(long rps)
        {
            if (rps == 0)
                return 0;

            int i = SocialEventHandler.REALMPOINTS_FOR_GUILDLEVEL.Length - 1;
            for (; i > 0; i--)
            {
                if (SocialEventHandler.REALMPOINTS_FOR_GUILDLEVEL[i] <= rps)
                    break;
            }

            if (i > 120)
                return 120;
            return i;


            // thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=30
            //			double z = Math.Pow(1620.0 * realmPoints + 15.0 * Math.Sqrt(-1875.0 + 11664.0 * realmPoints*realmPoints), 1.0/3.0);
            //			double rr = z / 30.0 + 5.0 / 2.0 / z + 0.5;
            //			return Math.Min(99, (int)rr);
        }
    }
}
