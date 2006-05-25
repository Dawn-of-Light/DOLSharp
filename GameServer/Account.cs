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
using DOL.Database;
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;

namespace DOL.GS
{
	/// <summary>
	/// Provides access to the client's account information.
	/// </summary>
	public class Account
	{
		#region AccountTO delegation properties
		
		/// <summary>
		/// Holds the account data transfer object.
		/// </summary>
		private readonly AccountTO m_account;

		/// <summary>
		/// The unique id of the acccount.
		/// </summary>
		public int AccountId
		{
			get { return m_account.AccountId; }
		}

		/// <summary>
		/// The name of the account (login).
		/// </summary>
		public string AccountName
		{
			get { return m_account.AccountName; }
			set { m_account.AccountName = value; }
		}

		/// <summary>
		/// The password of this account encode in MD5 when starts with '##' or clear text.
		/// </summary>
		public string Password
		{
			get { return m_account.Password; }
			set { m_account.Password = value; }
		}

		/// <summary>
		/// The date of creation of this account.
		/// </summary>
		public DateTime CreationDate
		{
			get { return m_account.CreationDate; }
			set { m_account.CreationDate = value; }
		}

		/// <summary>
		/// The date of last login of this account
		/// </summary>
		public DateTime LastLogin
		{
			get { return m_account.LastLogin; }
			set { m_account.LastLogin = value; }
		}

		/// <summary>
		/// The realm of this account.
		/// </summary>
		public eRealm Realm
		{
			get { return m_account.Realm; }
			set { m_account.Realm = value; }
		}

		/// <summary>
		/// The privileges level of this account (admin=3, GM=2 or player=1).
		/// </summary>
		public ePrivLevel PrivLevel
		{
			get { return m_account.PrivLevel; }
			set { m_account.PrivLevel = value; }
		}

		/// <summary>
		/// The mail of this account.
		/// </summary>
		public string Mail
		{
			get { return m_account.Mail; }
			set { m_account.Mail = value; }
		}

		/// <summary>
		/// The last IP logged onto this account.
		/// </summary>
		public string LastLoginIp
		{
			get { return m_account.LastLoginIp; }
			set { m_account.LastLoginIp = value; }
		}

		/// <summary>
		/// The ban duration of this account.
		/// </summary>
		public TimeSpan BanDuration
		{
			get { return m_account.BanDuration; }
			set { m_account.BanDuration = value; }
		}

		/// <summary>
		/// The ban author.
		/// </summary>
		public string BanAuthor
		{
			get { return m_account.BanAuthor; }
			set { m_account.BanAuthor = value; }
		}

		/// <summary>
		/// The ban reason.
		/// </summary>
		public string BanReason
		{
			get { return m_account.BanReason; }
			set { m_account.BanReason = value; }
		}

		/// <summary>
		/// The list of all the characters in the actual realm.
		/// </summary>
		public IList CharactersInSelectedRealm
		{
			get { return m_account.CharactersInSelectedRealm; }
			set { m_account.CharactersInSelectedRealm = value; }
		}

		#endregion

		/// <summary>
		/// Updates the account data in a database with current values.
		/// </summary>
		public void UpdateDatabase()
		{
			GameServer.DatabaseNew.Using<IAccountDao>().Update(m_account);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Account"/> class.
		/// </summary>
		/// <param name="account">The account data transfer object.</param>
		public Account(AccountTO account)
		{
			if (account == null)
			{
				throw new ArgumentNullException("account");
			}
			m_account = account;
		}
	}
}
