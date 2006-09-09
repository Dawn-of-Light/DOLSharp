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
	public class AccountEntity
	{
		private int m_id;
		private string m_accountName;
		private string m_banAuthor;
		private long m_banDuration;
		private string m_banReason;
		private DateTime m_creationDate;
		private DateTime m_lastLogin;
		private string m_lastLoginIp;
		private string m_mail;
		private string m_password;
		private byte m_privLevel;
		private byte m_realm;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public string AccountName
		{
			get { return m_accountName; }
			set { m_accountName = value; }
		}
		public string BanAuthor
		{
			get { return m_banAuthor; }
			set { m_banAuthor = value; }
		}
		public long BanDuration
		{
			get { return m_banDuration; }
			set { m_banDuration = value; }
		}
		public string BanReason
		{
			get { return m_banReason; }
			set { m_banReason = value; }
		}
		public DateTime CreationDate
		{
			get { return m_creationDate; }
			set { m_creationDate = value; }
		}
		public DateTime LastLogin
		{
			get { return m_lastLogin; }
			set { m_lastLogin = value; }
		}
		public string LastLoginIp
		{
			get { return m_lastLoginIp; }
			set { m_lastLoginIp = value; }
		}
		public string Mail
		{
			get { return m_mail; }
			set { m_mail = value; }
		}
		public string Password
		{
			get { return m_password; }
			set { m_password = value; }
		}
		public byte PrivLevel
		{
			get { return m_privLevel; }
			set { m_privLevel = value; }
		}
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
	}
}
