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
using DOL.Database2;



namespace DOL.Database2
{
	/// <summary>
	/// Guild table
	/// </summary>
	[Serializable]//TableName="Guild")]
	public class DBGuild : DatabaseObject
	{
		static bool		m_autoSave;

		private string m_guildid; //Unique id for this guild
		private string	m_guildname;
		private string	m_motd;
		private string	m_omotd;//officier motd

		private UInt64 m_allianceID;
		private int m_emblem;
		private long m_realmPoints;
		private long m_bountyPoints;

		private string m_webpage;
		private string m_email;
        private bool m_guildBanner;
        private bool m_guildDues;
        private double m_guildBank;
        private long m_guildDuesPercent;
        private bool m_guildHouse;
        private int m_guildHouseNumber;

		/// <summary>
		/// Create a guild
		/// </summary>
        public DBGuild()
            : base()
		{
			m_guildname = "default guild name";
			m_autoSave=false;
			m_emblem = 0;
			Ranks = new DBRank[10];
			m_webpage = "";
			m_email = "";
            m_guildBanner = false;
            m_guildDues = false;
            m_guildBank = 0;
            m_guildDuesPercent = 0;
            m_guildHouse = false;
            m_guildHouseNumber = 0;
        }

        #region guild level/buff
        private long m_GuildLevel;
        private long m_BuffType;
        private DateTime m_BuffTime;
        private long m_meritPoints;
        /// <summary>
        /// Guild level
        /// </summary>
        //[DataElement(AllowDbNull=true)]
        public long GuildLevel
        {
            get
            {
                return m_GuildLevel;
            }
            set
            {
                m_Dirty = true;
                m_GuildLevel = value;
            }
        }

        /// <summary>
        /// Buff flag of guild // BUFFTYPE
        /// </summary>
        //[DataElement(AllowDbNull=true)]
        public long BuffType
        {
            get
            {
                return m_BuffType;
            }
            set
            {
                m_Dirty = true;
                m_BuffType = value;
            }
        }

        /// <summary>
        /// Buff flag of guild // BUFFTIME
        /// </summary>
        
        public DateTime BuffTime
        {
            get
            {
                return m_BuffTime;
            }
            set
            {
                m_Dirty = true;
                m_BuffTime = value;
            }
        }
        
        public long MeritPoints
        {
            get
            {
                return m_meritPoints;
            }
            set
            {
                m_Dirty = true;
                m_meritPoints = value;
            }
        }
        #endregion


		/// <summary>
		/// Name of guild
		/// </summary>
		//[DataElement(AllowDbNull=true)] // can be primary too
		public string GuildName
		{
			get
			{
				return m_guildname;
			}
			set
			{
				m_Dirty = true;
				m_guildname = value;
			}
		}

        //[DataElement(AllowDbNull=true)] // can be primary too
        public bool GuildBanner
        {
            get
            {
                return m_guildBanner;
            }
            set
            {
                m_Dirty = true;
                m_guildBanner = value;
            }
        }
        /// <summary>
		/// Message of the day of the guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public string Motd
		{
			get
			{
				return m_motd;
			}
			set
			{
				m_Dirty = true;
				m_motd = value;
			}
		}
		
		/// <summary>
		/// officier message of the day
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public string oMotd
		{
			get
			{
				return m_omotd;
			}
			set
			{
				m_Dirty = true;
				m_omotd = value;
			}
		}
		
		/// <summary>
		/// alliance id when guild join an alliance
		/// </summary>
		//[DataElement(AllowDbNull = true, Index=true)]
		public UInt64 AllianceID
		{
			get
			{
				return m_allianceID;
			}
			set
			{
				m_Dirty = true;
				m_allianceID = value;
			}
		}

		/// <summary>
		/// emblem of guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				m_Dirty = true;
				m_emblem = value;
			}
		}

		/// <summary>
		/// realm point of guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public long RealmPoints
		{
			get
			{
				return m_realmPoints;
			}
			set
			{
				m_Dirty = true;
				m_realmPoints = value;
			}
		}

		/// <summary>
		/// bounty point of guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public long BountyPoints
		{
			get
			{
				return m_bountyPoints;
			}
			set
			{
				m_Dirty = true;
				m_bountyPoints = value;
			}
		}

		/// <summary>
		/// Webpage for the guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public string Webpage
		{
			get { return m_webpage; }
			set
			{
				m_Dirty = true;
				m_webpage = value;
			}
		}

		/// <summary>
		/// Email for the guild
		/// </summary>
		//[DataElement(AllowDbNull=true)]
		public string Email
		{
			get { return m_email; }
			set
			{
				m_Dirty = true;
				m_email = value;
			}
		}

        //[DataElement(AllowDbNull=true)]
        public bool Dues
        {
            get { return m_guildDues; }
            set
            {
                m_Dirty = true;
                m_guildDues = value;
            }
        }
        //[DataElement(AllowDbNull=true)]
        public double Bank
        {
            get { return m_guildBank; }
            set
            {
                m_Dirty = true;
                m_guildBank = value;
            }
        }
        //[DataElement(AllowDbNull=true)]
        public long DuesPercent
        {
            get { return m_guildDuesPercent; }
            set
            {
                m_Dirty = true;
                m_guildDuesPercent = value;
            }
        }
        
        public bool HaveGuildHouse
        {
            get { return m_guildHouse; }
            set
            {
                m_Dirty = true;
                m_guildHouse = value;
            }
        }
        
        public int GuildHouseNumber
        {
            get { return m_guildHouseNumber; }
            set
            {
                m_Dirty = true;
                m_guildHouseNumber = value;
            }
        }        
        /*
		/// <summary>
		/// characters in guild
		/// </summary>
		[Relation(LocalField = "guildID", RemoteField = "guildID", AutoLoad = true, AutoDelete=false)]
		public Character[] Characters;
		 */

		/// <summary>
		/// rank rules
		/// </summary>
		//[Relation(LocalField = "guildID", RemoteField = "guildID", AutoLoad = true, AutoDelete=true)]
		public DBRank[] Ranks;
        public override void FillObjectRelations()
        {
            Ranks = DatabaseLayer.Instance.SelectObjects<DBRank>("guildID", ID).ToArray();
            base.FillObjectRelations();
        }
	}
}
