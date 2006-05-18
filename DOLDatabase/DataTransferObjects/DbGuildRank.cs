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
using System.Collections;

namespace DOL.Database.DataTransferObjects
{
	/// <summary>
	/// Rank rules in guild
	/// </summary>
	public class DbGuildRank
	{
		private int		m_id;
		private byte	m_ranklevel;
		private string	m_title;
				
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
		private bool	m_motd;
		private bool	m_deposit;
		private bool	m_withdraw;
		private bool	m_dues;
		private bool	m_buff;
		private bool	m_getMission;
		private bool	m_setNote;
		private bool	m_summonBanner;
		private bool	m_buyBanner;

		public int GuildRankID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}

		/// <summary>
		/// rank level between 1 and 10
		/// </summary>
		public byte RankLevel
		{
			get
			{
				return m_ranklevel;
			}
			set
			{
				m_ranklevel = value;
			}
		}

		/// <summary>
		/// Title of rank
		/// </summary>
		public string Title
		{
			get
			{
				return m_title;
			}
			set
			{
				m_title = value;
			}
		}

		/// <summary>
		/// Is player allowed to make alliance
		/// </summary>
		public bool Alli
		{
			get
			{
				return m_alli;
			}
			set
			{
				m_alli = value;
			}
		}

		/// <summary>
		/// is member alowed to wear alliance
		/// </summary>
		public bool Emblem
		{
			get
			{
				return m_emblem;
			}
			set
			{
				m_emblem = value;
			}
		}

		/// <summary>
		/// Can player with this rank hear guild chat
		/// </summary>
		public bool GcHear
		{
			get
			{
				return m_gchear;
			}
			set
			{
				m_gchear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on guild chat
		/// </summary>
		public bool GcSpeak
		{
			get
			{
				return m_gcspeak;
			}
			set
			{
				m_gcspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank hear officier chat
		/// </summary>
		public bool OcHear
		{
			get
			{
				return m_ochear;
			}
			set
			{
				m_ochear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on officier chat
		/// </summary>
		public bool OcSpeak
		{
			get
			{
				return m_ocspeak;
			}
			set
			{
				m_ocspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank hear alliance chat
		/// </summary>
		public bool AcHear
		{
			get
			{
				return m_achear;
			}
			set
			{
				m_achear = value;
			}
		}

		/// <summary>
		/// Can player with this rank talk on alliance chat
		/// </summary>
		public bool AcSpeak
		{
			get
			{
				return m_acspeak;
			}
			set
			{
				m_acspeak = value;
			}
		}

		/// <summary>
		/// Can player with this rank invite player to join the guild
		/// </summary>
		public bool Invite
		{
			get
			{
				return m_invite;
			}
			set
			{
				m_invite = value;
			}
		}

		/// <summary>
		/// Can player with this rank promote player in the guild
		/// </summary>
		public bool Promote
		{
			get
			{
				return m_promote;
			}
			set
			{
				m_promote = value;
			}
		}

		/// <summary>
		/// Can player with this rank removed player from the guild
		/// </summary>
		public bool Remove
		{
			get
			{
				return m_remove;
			}
			set
			{
				m_remove = value;
			}
		}

		/// <summary>
		/// Can player with this rank view player in the guild
		/// </summary>
		public bool View
		{
			get
			{
				return m_view;
			}
			set
			{
				m_view = value;
			}
		}

		/// <summary>
		/// Can player with this rank claim keep
		/// </summary>
		public bool Claim
		{
			get
			{
				return m_claim;
			}
			set
			{
				m_claim = value;
			}
		}

		/// <summary>
		/// Can player with this rank upgrade keep
		/// </summary>
		public bool Upgrade
		{
			get
			{
				return m_upgrade;
			}
			set
			{
				m_upgrade = value;
			}
		}

		/// <summary>
		/// Can player with this rank released the keep claimed
		/// </summary>
		public bool Release
		{
			get
			{
				return m_release;
			}
			set
			{
				m_release = value;
			}
		}

		/// <summary>
		/// Can player edit the motd
		/// </summary>
		public bool Motd
		{
			get
			{
				return m_motd;
			}
			set
			{
				m_motd = value;
			}
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool Deposit
		{
			get { return m_deposit; }
			set { m_deposit = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool Withdraw
		{
			get { return m_withdraw; }
			set { m_withdraw = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool Dues
		{
			get { return m_dues; }
			set { m_dues = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool Buff
		{
			get { return m_buff; }
			set { m_buff = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool GetMission
		{
			get { return m_getMission; }
			set { m_getMission = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool SetNote
		{
			get { return m_setNote; }
			set { m_setNote = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool SummonBanner
		{
			get { return m_summonBanner; }
			set { m_summonBanner = value; }
		}

		/// <summary>
		/// Gets or sets the guild level
		/// </summary>
		public bool BuyBanner
		{
			get { return m_buyBanner; }
			set { m_buyBanner = value; }
		}
	}
}