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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public class GuildRankEntity
	{
		private int m_id;
		private bool m_acHear;
		private bool m_acSpeak;
		private bool m_alli;
		private bool m_buff;
		private bool m_buyBanner;
		private bool m_claim;
		private bool m_deposit;
		private bool m_dues;
		private bool m_emblem;
		private bool m_gcHear;
		private bool m_gcSpeak;
		private bool m_getMission;
		private int m_guild;
		private bool m_invite;
		private bool m_motd;
		private bool m_ocHear;
		private bool m_ocSpeak;
		private bool m_promote;
		private byte m_rankLevel;
		private bool m_release;
		private bool m_remove;
		private bool m_setNote;
		private bool m_summonBanner;
		private string m_title;
		private bool m_upgrade;
		private bool m_view;
		private bool m_withdraw;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public bool AcHear
		{
			get { return m_acHear; }
			set { m_acHear = value; }
		}
		public bool AcSpeak
		{
			get { return m_acSpeak; }
			set { m_acSpeak = value; }
		}
		public bool Alli
		{
			get { return m_alli; }
			set { m_alli = value; }
		}
		public bool Buff
		{
			get { return m_buff; }
			set { m_buff = value; }
		}
		public bool BuyBanner
		{
			get { return m_buyBanner; }
			set { m_buyBanner = value; }
		}
		public bool Claim
		{
			get { return m_claim; }
			set { m_claim = value; }
		}
		public bool Deposit
		{
			get { return m_deposit; }
			set { m_deposit = value; }
		}
		public bool Dues
		{
			get { return m_dues; }
			set { m_dues = value; }
		}
		public bool Emblem
		{
			get { return m_emblem; }
			set { m_emblem = value; }
		}
		public bool GcHear
		{
			get { return m_gcHear; }
			set { m_gcHear = value; }
		}
		public bool GcSpeak
		{
			get { return m_gcSpeak; }
			set { m_gcSpeak = value; }
		}
		public bool GetMission
		{
			get { return m_getMission; }
			set { m_getMission = value; }
		}
		public int Guild
		{
			get { return m_guild; }
			set { m_guild = value; }
		}
		public bool Invite
		{
			get { return m_invite; }
			set { m_invite = value; }
		}
		public bool Motd
		{
			get { return m_motd; }
			set { m_motd = value; }
		}
		public bool OcHear
		{
			get { return m_ocHear; }
			set { m_ocHear = value; }
		}
		public bool OcSpeak
		{
			get { return m_ocSpeak; }
			set { m_ocSpeak = value; }
		}
		public bool Promote
		{
			get { return m_promote; }
			set { m_promote = value; }
		}
		public byte RankLevel
		{
			get { return m_rankLevel; }
			set { m_rankLevel = value; }
		}
		public bool Release
		{
			get { return m_release; }
			set { m_release = value; }
		}
		public bool Remove
		{
			get { return m_remove; }
			set { m_remove = value; }
		}
		public bool SetNote
		{
			get { return m_setNote; }
			set { m_setNote = value; }
		}
		public bool SummonBanner
		{
			get { return m_summonBanner; }
			set { m_summonBanner = value; }
		}
		public string Title
		{
			get { return m_title; }
			set { m_title = value; }
		}
		public bool Upgrade
		{
			get { return m_upgrade; }
			set { m_upgrade = value; }
		}
		public bool View
		{
			get { return m_view; }
			set { m_view = value; }
		}
		public bool Withdraw
		{
			get { return m_withdraw; }
			set { m_withdraw = value; }
		}
	}
}
