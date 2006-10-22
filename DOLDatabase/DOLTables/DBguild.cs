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
		static bool		m_autoSave;

		private string	m_guildname;
		private string	m_motd;
		private string	m_omotd;//officier motd

		private string m_allianceID;
		private int m_emblem;
		private long m_realmPoints;
		private long m_bountyPoints;

		/// <summary>
		/// Create a guild
		/// </summary>
		public DBGuild()
		{
			m_guildname = "default guild name";
			m_autoSave=false;
			m_emblem = 0;
			Ranks = new DBRank[10];
		}

		/// <summary>
		/// Autosave table
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		/// <summary>
		/// Name of guild
		/// </summary>
		[DataElement(AllowDbNull = true, Index=true)] // can be primary too
		public string GuildName
		{
			get
			{
				return m_guildname;
			}
			set
			{
				Dirty = true;
				m_guildname = value;
			}
		}
		
		/// <summary>
		/// Message of the day of the guild
		/// </summary>
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
		
		/// <summary>
		/// officier message of the day
		/// </summary>
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
		
		/// <summary>
		/// alliance id when guild join an alliance
		/// </summary>
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

		/// <summary>
		/// emblem of guild
		/// </summary>
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

		/// <summary>
		/// realm point of guild
		/// </summary>
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

		/// <summary>
		/// bounty point of guild
		/// </summary>
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

		/// <summary>
		/// characters in guild
		/// </summary>
		[Relation(LocalField = "GuildName", RemoteField = "GuildName", AutoLoad = true, AutoDelete=false)]
		public Character[] Characters;

		/// <summary>
		/// rank rules
		/// </summary>
		[Relation(LocalField = "GuildName", RemoteField = "GuildName", AutoLoad = true, AutoDelete=true)]
		public DBRank[] Ranks;
	}
}
