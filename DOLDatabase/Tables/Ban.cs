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
	/// Bans table
	/// </summary>
	[DataTable(TableName="Ban")]
	public class DBBannedAccount : DataObject
	{
		private string	m_author;
        private string  m_type;
		private string	m_ip;
		private string	m_account;
        private DateTime m_dateban;
		private string	m_reason;

		/// <summary>
		/// Who have ban player
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string Author
		{
			get
			{
				return m_author;
			}
			set
			{
				m_author=value;
			}
		}

		/// <summary>
		/// type of ban (I=ip, A=account, B=both)
		/// </summary>
		[DataElement(AllowDbNull=false)]
        public string Type
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type=value;
			}
		}

		/// <summary>
		/// IP banned
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string Ip
		{
			get
			{
				return m_ip;
			}
			set
			{
				m_ip=value;
			}
		}

		/// <summary>
		/// Account banned
		/// </summary>
		[DataElement(AllowDbNull=false, Index=true)]
		public string Account
		{
			get
			{
				return m_account;
			}
			set
			{
				m_account=value;
			}
		}

		/// <summary>
		/// When have been ban this account/IP
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public DateTime DateBan
		{
			get
			{
				return m_dateban;
			}
			set
			{
                m_dateban = value;
			}
		}

		/// <summary>
		/// reason of ban
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string Reason
		{
			get
			{
				return m_reason;
			}
			set
			{
				m_reason=value;
			}
		}

		/// <summary>
		/// autosave ban table
		/// </summary>
		override public bool AutoSave
		{
			get
			{
				return true;
			}
			set
			{
			}
		}
	}
}
