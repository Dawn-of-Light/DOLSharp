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
	/// Account table
	/// </summary>
	public class AccountTO
	{
		/// <summary>
		/// The unique id of the acccount.
		/// </summary>
		private int m_accountId;

		/// <summary>
		/// The name of the account (login).
		/// </summary>
		private string m_accountName = "";

		/// <summary>
		/// The password of this account encode in MD5 when starts with '##' or clear text.
		/// </summary>
		private string m_password = "";

		/// <summary>
		/// The date of creation of this account.
		/// </summary>
		private DateTime m_creationDate;

		/// <summary>
		/// The date of last login of this account.
		/// </summary>
		private DateTime m_lastLogin;

		/// <summary>
		/// The realm of this account.
		/// </summary>
		private eRealm m_realm;

		/// <summary>
		/// The privileges level of this account (admin=3, GM=2 or player=1).
		/// </summary>
		private ePrivLevel m_privLevel;

		/// <summary>
		/// The mail of this account.
		/// </summary>
		private string m_mail = "";

		/// <summary>
		/// The last IP logged onto this account.
		/// </summary>
		private string m_lastLoginIp = "";

		/// <summary>
		/// The ban duration of this account.
		/// </summary>
		private TimeSpan m_banDuration;

		/// <summary>
		/// The ban author.
		/// </summary>
		private string m_banAuthor = "";

		/// <summary>
		/// The ban reason.
		/// </summary>
		private string m_banReason = "";
		
		/// <summary>
		/// The account's characters
		/// </summary>
		private IList m_characters;

		#region All get/set

		/// <summary>
		/// The unique id of the acccount.
		/// </summary>
		public int AccountId
		{
			get { return m_accountId; }
			set { m_accountId = value; }
		}

		/// <summary>
		/// The name of the account (login).
		/// </summary>
		public string AccountName
		{
			get { return m_accountName; }
			set { m_accountName = value; }
		}

		/// <summary>
		/// The password of this account encode in MD5 when starts with '##' or clear text.
		/// </summary>
		public string Password
		{
			get { return m_password; }
			set { m_password = value; }
		}

		/// <summary>
		/// The date of creation of this account.
		/// </summary>
		public DateTime CreationDate
		{
			get { return m_creationDate; }
			set { m_creationDate = value; }
		}

		/// <summary>
		/// The date of last login of this account
		/// </summary>
		public DateTime LastLogin
		{
			get { return m_lastLogin; }
			set { m_lastLogin = value; }
		}

		/// <summary>
		/// The realm of this account.
		/// </summary>
		public eRealm Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}

		/// <summary>
		/// The privileges level of this account (admin=3, GM=2 or player=1).
		/// </summary>
		public ePrivLevel PrivLevel
		{
			get { return m_privLevel; }
			set { m_privLevel = value; }
		}

		/// <summary>
		/// The mail of this account.
		/// </summary>
		public string Mail
		{
			get { return m_mail; }
			set { m_mail = value; }
		}

		/// <summary>
		/// The last IP logged onto this account.
		/// </summary>
		public string LastLoginIp
		{
			get { return m_lastLoginIp; }
			set { m_lastLoginIp = value; }
		}

		/// <summary>
		/// The ban duration of this account.
		/// </summary>
		public TimeSpan BanDuration
		{
			get { return m_banDuration; }
			set { m_banDuration = value; }
		}

		/// <summary>
		/// The ban author.
		/// </summary>
		public string BanAuthor
		{
			get { return m_banAuthor; }
			set { m_banAuthor = value; }
		}

		/// <summary>
		/// The ban reason.
		/// </summary>
		public string BanReason
		{
			get { return m_banReason; }
			set { m_banReason = value; }
		}

		/// <summary>
		/// The list of all the characters in the actual realm.
		/// </summary>
		public IList Characters
		{
			get
			{
				if (m_characters == null)
				{
					m_characters = new ArrayList(1);
				}
				return m_characters;
			}
			set { m_characters = value; }
		}

		#endregion
	}
}