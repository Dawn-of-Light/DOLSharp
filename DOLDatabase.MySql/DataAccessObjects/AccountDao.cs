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
using System.Collections.Generic;
using System.Data;
using DOL.Database.DataAccessInterfaces;
using DOL.Database.DataTransferObjects;
using MySql.Data.MySqlClient;

namespace DOL.Database.MySql.DataAccessObjects
{
	public class AccountDao : IAccountDao
	{
		private readonly MySqlState m_state;

		public virtual AccountEntity Find(int key)
		{
			AccountEntity result = new AccountEntity();
			m_state.ExecuteQuery(
				"SELECT `AccountId`,`AccountName`,`BanAuthor`,`BanDuration`,`BanReason`,`CreationDate`,`LastLogin`,`LastLoginIp`,`Mail`,`Password`,`PrivLevel`,`Realm` FROM `account` WHERE `AccountId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(AccountEntity obj)
		{
		}

		public virtual void Update(AccountEntity obj)
		{
		}

		public virtual void Delete(AccountEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref AccountEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AccountName = reader.GetString(1);
			entity.BanAuthor = reader.GetString(2);
			entity.BanDuration = reader.GetInt64(3);
			entity.BanReason = reader.GetString(4);
			entity.CreationDate = reader.GetDateTime(5);
			entity.LastLogin = reader.GetDateTime(6);
			entity.LastLoginIp = reader.GetString(7);
			entity.Mail = reader.GetString(8);
			entity.Password = reader.GetString(9);
			entity.PrivLevel = reader.GetByte(10);
			entity.Realm = reader.GetByte(11);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(AccountEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `account` ("
				+"`AccountId` int,"
				+"`AccountName` varchar(40) character set unicode,"
				+"`BanAuthor` varchar(510) character set unicode,"
				+"`BanDuration` bigint,"
				+"`BanReason` varchar(510) character set unicode,"
				+"`CreationDate` datetime,"
				+"`LastLogin` datetime,"
				+"`LastLoginIp` varchar(510) character set unicode,"
				+"`Mail` varchar(510) character set unicode,"
				+"`Password` varchar(510) character set unicode,"
				+"`PrivLevel` tinyint unsigned,"
				+"`Realm` tinyint unsigned"
				+", primary key `AccountId` (`AccountId`)"
			);
		}

		public AccountDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
