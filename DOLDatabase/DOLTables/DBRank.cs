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
//using System.Collections;

namespace DOL.Database
{
	/// <summary>
	/// Rank rules in guild
	/// </summary>
	[DataTable(TableName="guild_rank")]
	public class DBRank : DataObject
	{
		static bool		m_autoSave;

		private uint	m_guildid;
		private string	m_title;
		private byte	m_ranklevel;
				
		private bool	m_alli;
		private bool	m_emblem;
		private bool	m_gchear;
		private bool	m_gcspeak;
		private bool	m_ochear;
		private bool	m_ocspeak;
		private bool	m_achear;
		private bool	m_acspeak ;
		private bool	m_invite;
		private bool	m_promote;
		private bool	m_remove;
		private bool	m_view;//gc info
		private bool	m_claim;
		private bool	m_upgrade;
		private bool	m_release;

		/// <summary>
		/// create rank rules
		/// </summary>
		public DBRank()
		{
			m_guildid = 0;
			m_title = "";
			m_ranklevel = 0; 
			m_autoSave=false;
			m_alli = false;
			m_emblem  = false;
			m_gchear  = false;
			m_gcspeak = false;
			m_ochear  = false;
			m_ocspeak = false;
			m_achear  = false;
			m_acspeak = false;
			m_invite  = false;
			m_promote = false;
			m_remove  = false;
		}

		/// <summary>
		/// autosave table
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
		/// ID of guild
		/// </summary>
		[DataElement(AllowDbNull = true, Index=true)]
		public uint GuildID
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
		/// Title of rank
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Title
		{
			get
			{
				return m_title;
			}
			set
			{
				Dirty = true;
				m_title = value;
			}
		}

		/// <summary>
		/// rank level between 1 and 10
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte RankLevel
		{
			get
			{
				return m_ranklevel;
			}
			set
			{
				Dirty = true;
				m_ranklevel = value;
			}
		}

		/// <summary>
		/// Is player allowed to make alliance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Alli
		{
			get
			{
				return m_alli;
			}
			set
			{
				Dirty = true;
				m_alli = value;
			}
		}

		/// <summary>
		/// is member alowed to wear alliance
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Emblem
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
		/// Can player with this rank hear guild chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool GcHear
		{
			get
			{
				return m_gchear;
			}
			set
			{
				Dirty = true;
				m_gchear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on guild chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool GcSpeak
		{
			get
			{
				return m_gcspeak;
			}
			set
			{
				Dirty = true;
				m_gcspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank hear officier chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool OcHear
		{
			get
			{
				return m_ochear;
			}
			set
			{
				Dirty = true;
				m_ochear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on officier chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool OcSpeak
		{
			get
			{
				return m_ocspeak;
			}
			set
			{
				Dirty = true;
				m_ocspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank hear alliance chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool AcHear
		{
			get
			{
				return m_achear;
			}
			set
			{
				Dirty = true;
				m_achear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on alliance chat
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool AcSpeak
		{
			get
			{
				return m_acspeak;
			}
			set
			{
				Dirty = true;
				m_acspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank invite player to join the guild
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Invite
		{
			get
			{
				return m_invite;
			}
			set
			{
				Dirty = true;
				m_invite = value;
			}
		}

		/// <summary>
		/// Can player with this rank promote player in the guild
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Promote
		{
			get
			{
				return m_promote;
			}
			set
			{
				Dirty = true;
				m_promote = value;
			}
		}

		/// <summary>
		/// Can player with this rank removed player from the guild
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Remove
		{
			get
			{
				return m_remove;
			}
			set
			{
				Dirty = true;
				m_remove = value;
			}
		}

		/// <summary>
		/// Can player with this rank view player in the guild
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool View
		{
			get
			{
				return m_view;
			}
			set
			{
				Dirty = true;
				m_view = value;
			}
		}

		/// <summary>
		/// Can player with this rank claim keep
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Claim
		{
			get
			{
				return m_claim;
			}
			set
			{
				Dirty = true;
				m_claim = value;
			}
		}

		/// <summary>
		/// Can player with this rank upgrade keep
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Upgrade
		{
			get
			{
				return m_upgrade;
			}
			set
			{
				Dirty = true;
				m_upgrade = value;
			}
		}

		/// <summary>
		/// Can player with this rank released the keep claimed
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public bool Release
		{
			get
			{
				return m_release;
			}
			set
			{
				Dirty = true;
				m_release = value;
			}
		}
	}
}