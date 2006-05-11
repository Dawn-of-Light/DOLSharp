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

namespace DOL.Database.TransferObjects
{
	/// <summary>
	/// Account table
	/// </summary>
	public class DbAccount
	{
		private int		m_id;
		private string m_accountName;
		private string m_password;
		private DateTime m_creationDate;
		private DateTime m_lastLogin;
		private eRealm m_realm;
		private ePrivLevel m_plvl;
		private string m_mail;
		private string m_lastLoginIP;
		private TimeSpan m_banDuration;
		private string m_banAuthor;
		private string m_banReason;
		private IList m_charactersInSelectedRealm;

		#region All get/set
		/// <summary>
		/// The unique id of the acccount
		/// </summary>
		public int AccountID
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
		/// The name of the account (login)
		/// </summary>
		public string AccountName
		{
			get
			{
				return m_accountName;
			}
			set
			{   
				m_accountName = value;
			}
		}

		/// <summary>
		/// The password of this account encode in MD5 or clear when start with ##
		/// </summary>
		public string Password
		{
			get
			{
				return m_password;
			}
			set
			{   
				m_password = value;
			}
		}

		/// <summary>
		/// The date of creation of this account
		/// </summary>
		public DateTime CreationDate
		{
			get
			{
				return m_creationDate;
			}
			set
			{   
				m_creationDate = value;
			}
		}

		/// <summary>
		/// The date of last login of this account
		/// </summary>
		public DateTime LastLogin
		{
			get
			{
				return m_lastLogin;
			}
			set
			{   
				m_lastLogin = value;
			}
		}

		/// <summary>
		/// The realm of this account
		/// </summary>
		public eRealm Realm
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

		/// <summary>
		/// The private level of this account (admin=3, GM=2 or player=1)
		/// </summary>
		public ePrivLevel PrivLevel
		{
			get
			{
				return m_plvl;
			}
			set
			{
				m_plvl = value;
			}
		}

		/// <summary>
		/// The mail of this account
		/// </summary>
		public string Mail
		{
			get
			{ 
				return m_mail; 
			}
			set 
			{ 
				m_mail = value;
			}
		}

		/// <summary>
		/// The last IP logged onto this account
		/// </summary>
		public string LastLoginIP
		{
			get
			{ 
				return m_lastLoginIP; 
			}
			set 
			{ 
				m_lastLoginIP = value; 
			}
		}

		/// <summary>
		/// The ban duration of this account
		/// </summary>
		public TimeSpan BanDuration
		{
			get
			{ 
				return m_banDuration; 
			}
			set 
			{ 
				m_banDuration = value; 
			}
		}

		/// <summary>
		/// The ban author
		/// </summary>
		public string BanAuthor
		{
			get
			{ 
				return m_banAuthor; 
			}
			set 
			{ 
				m_banAuthor = value; 
			}
		}

		/// <summary>
		/// The ban author
		/// </summary>
		public string BanReason
		{
			get
			{ 
				return m_banReason; 
			}
			set 
			{ 
				m_banReason = value; 
			}
		}
		#endregion

		/// <summary>
		/// The list of all the characters in the actual realm
		/// </summary>
		public IList CharactersInSelectedRealm
		{
			get
			{
				if(m_charactersInSelectedRealm == null) m_charactersInSelectedRealm = new ArrayList(1);
				return m_charactersInSelectedRealm;
			}
			set
			{
				m_charactersInSelectedRealm=value;
			}
		}
	}
}
