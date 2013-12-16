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
using DOL.Database;
using DOL.Database.Attributes;


namespace DOL.Database
{
	/// <summary>
	/// Guild table
	/// </summary>
	[DataTable(TableName="Guild")]
	public class DBGuild : DataObject
	{
		private string m_guildid; //Unique id for this guild
		private string	m_guildname;
		private string	m_motd;
		private string	m_omotd;//officier motd
		private byte m_realm;

		private string m_allianceID;
		private int m_emblem;
		private long m_realmPoints;
		private long m_bountyPoints;

		private string m_webpage;
		private string m_email;
		private bool m_guildBanner;
		private DateTime m_guildBannerLostTime;
		private bool m_guildDues;
		private double m_guildBank;
		private long m_guildDuesPercent;
		private bool m_guildHouse;
		private int m_guildHouseNumber;

		private long m_guildLevel;
		private byte m_bonusType;
		private DateTime m_bonusStartTime;
		private long m_meritPoints;

		public DBGuild()
		{
			m_guildname = "default guild name";
			m_realm = 0;
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
			m_meritPoints = 0;
			m_bonusType = 0;
			m_bonusStartTime = new DateTime(2000, 1, 1);
			m_guildBannerLostTime = new DateTime(2000, 1, 1);
		}


		/// <summary>
		/// A unique ID for the guild
		/// </summary>
		[DataElement(AllowDbNull = true, Index=true, Unique=true)]
		public string GuildID
		{
			get
			{
				return m_guildid;
			}
			set
			{
				Dirty = true;
				m_guildid = value;
			}
		}

		/// <summary>
		/// Name of the Guild.  This is readonly after initial creation.
		/// </summary>
		[ReadOnly]
		[DataElement(AllowDbNull = true)]
		public string GuildName
		{
			get
			{
				return m_guildname;
			}
			set
			{
				m_guildname = value;
			}
		}

		[ReadOnly]
		[DataElement(AllowDbNull = true)]
		public byte Realm
		{
			get
			{
				return m_realm;
			}
			set
			{
				m_realm = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool GuildBanner
		{
			get
			{
				return m_guildBanner;
			}
			set
			{
				Dirty = true;
				m_guildBanner = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public DateTime GuildBannerLostTime
		{
			get
			{
				return m_guildBannerLostTime;
			}
			set
			{
				Dirty = true;
				m_guildBannerLostTime = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Motd
		{
			get
			{
				return m_motd;
			}
			set
			{
				Dirty = true;
				m_motd = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string oMotd
		{
			get
			{
				return m_omotd;
			}
			set
			{
				Dirty = true;
				m_omotd = value;
			}
		}

		[DataElement(AllowDbNull = true, Index=true)]
		public string AllianceID
		{
			get
			{
				return m_allianceID;
			}
			set
			{
				Dirty = true;
				m_allianceID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				Dirty = true;
				m_emblem = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public long RealmPoints
		{
			get
			{
				return m_realmPoints;
			}
			set
			{
				Dirty = true;
				m_realmPoints = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public long BountyPoints
		{
			get
			{
				return m_bountyPoints;
			}
			set
			{
				Dirty = true;
				m_bountyPoints = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Webpage
		{
			get { return m_webpage; }
			set
			{
				Dirty = true;
				m_webpage = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string Email
		{
			get { return m_email; }
			set
			{
				Dirty = true;
				m_email = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public bool Dues
		{
			get { return m_guildDues; }
			set
			{
				Dirty = true;
				m_guildDues = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public double Bank
		{
			get { return m_guildBank; }
			set
			{
				Dirty = true;
				m_guildBank = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public long DuesPercent
		{
			get { return m_guildDuesPercent; }
			set
			{
				Dirty = true;
				m_guildDuesPercent = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public bool HaveGuildHouse
		{
			get { return m_guildHouse; }
			set
			{
				Dirty = true;
				m_guildHouse = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int GuildHouseNumber
		{
			get { return m_guildHouseNumber; }
			set
			{
				Dirty = true;
				m_guildHouseNumber = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public long GuildLevel
		{
			get
			{
				return m_guildLevel;
			}
			set
			{
				Dirty = true;
				m_guildLevel = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte BonusType
		{
			get
			{
				return m_bonusType;
			}
			set
			{
				Dirty = true;
				m_bonusType = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public DateTime BonusStartTime
		{
			get
			{
				return m_bonusStartTime;
			}
			set
			{
				Dirty = true;
				m_bonusStartTime = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public long MeritPoints
		{
			get
			{
				return m_meritPoints;
			}
			set
			{
				Dirty = true;
				m_meritPoints = value;
			}
		}

		/// <summary>
		/// rank rules
		/// </summary>
		[Relation(LocalField = "GuildID", RemoteField = "GuildID", AutoLoad = true, AutoDelete=true)]
		public DBRank[] Ranks;
	}
}
