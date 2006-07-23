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
	public struct GuildRankEntity
	{
		private int m_id;
		private string m_acHear;
		private string m_acSpeak;
		private string m_alli;
		private string m_buff;
		private string m_buyBanner;
		private string m_claim;
		private string m_deposit;
		private string m_dues;
		private string m_emblem;
		private string m_gcHear;
		private string m_gcSpeak;
		private string m_getMission;
		private int m_guild;
		private string m_invite;
		private string m_motd;
		private string m_ocHear;
		private string m_ocSpeak;
		private string m_promote;
		private byte m_rankLevel;
		private string m_release;
		private string m_remove;
		private string m_setNote;
		private string m_summonBanner;
		private string m_title;
		private string m_upgrade;
		private string m_view;
		private string m_withdraw;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string AcHear
		{
			get { return m_acHear; }
			set { m_acHear = value; }
		}
		public string AcSpeak
		{
			get { return m_acSpeak; }
			set { m_acSpeak = value; }
		}
		public string Alli
		{
			get { return m_alli; }
			set { m_alli = value; }
		}
		public string Buff
		{
			get { return m_buff; }
			set { m_buff = value; }
		}
		public string BuyBanner
		{
			get { return m_buyBanner; }
			set { m_buyBanner = value; }
		}
		public string Claim
		{
			get { return m_claim; }
			set { m_claim = value; }
		}
		public string Deposit
		{
			get { return m_deposit; }
			set { m_deposit = value; }
		}
		public string Dues
		{
			get { return m_dues; }
			set { m_dues = value; }
		}
		public string Emblem
		{
			get { return m_emblem; }
			set { m_emblem = value; }
		}
		public string GcHear
		{
			get { return m_gcHear; }
			set { m_gcHear = value; }
		}
		public string GcSpeak
		{
			get { return m_gcSpeak; }
			set { m_gcSpeak = value; }
		}
		public string GetMission
		{
			get { return m_getMission; }
			set { m_getMission = value; }
		}
		public int Guild
		{
			get { return m_guild; }
			set { m_guild = value; }
		}
		public string Invite
		{
			get { return m_invite; }
			set { m_invite = value; }
		}
		public string Motd
		{
			get { return m_motd; }
			set { m_motd = value; }
		}
		public string OcHear
		{
			get { return m_ocHear; }
			set { m_ocHear = value; }
		}
		public string OcSpeak
		{
			get { return m_ocSpeak; }
			set { m_ocSpeak = value; }
		}
		public string Promote
		{
			get { return m_promote; }
			set { m_promote = value; }
		}
		public byte RankLevel
		{
			get { return m_rankLevel; }
			set { m_rankLevel = value; }
		}
		public string Release
		{
			get { return m_release; }
			set { m_release = value; }
		}
		public string Remove
		{
			get { return m_remove; }
			set { m_remove = value; }
		}
		public string SetNote
		{
			get { return m_setNote; }
			set { m_setNote = value; }
		}
		public string SummonBanner
		{
			get { return m_summonBanner; }
			set { m_summonBanner = value; }
		}
		public string Title
		{
			get { return m_title; }
			set { m_title = value; }
		}
		public string Upgrade
		{
			get { return m_upgrade; }
			set { m_upgrade = value; }
		}
		public string View
		{
			get { return m_view; }
			set { m_view = value; }
		}
		public string Withdraw
		{
			get { return m_withdraw; }
			set { m_withdraw = value; }
		}
	}
}
