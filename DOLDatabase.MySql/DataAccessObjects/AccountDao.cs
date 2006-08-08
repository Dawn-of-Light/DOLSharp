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
		protected static readonly string c_rowFields = "`AccountId`,`AccountName`,`BanAuthor`,`BanDuration`,`BanReason`,`CreationDate`,`LastLogin`,`LastLoginIp`,`Mail`,`Password`,`PrivLevel`,`Realm`";
		private readonly MySqlState m_state;

		public virtual AccountEntity Find(int key)
		{
			AccountEntity result = new AccountEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `account` WHERE `AccountId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(AccountEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `account` VALUES (`" + obj.Id.ToString() + "`,`" + obj.AccountName.ToString() + "`,`" + obj.BanAuthor.ToString() + "`,`" + obj.BanDuration.ToString() + "`,`" + obj.BanReason.ToString() + "`,`" + obj.CreationDate.ToString() + "`,`" + obj.LastLogin.ToString() + "`,`" + obj.LastLoginIp.ToString() + "`,`" + obj.Mail.ToString() + "`,`" + obj.Password.ToString() + "`,`" + obj.PrivLevel.ToString() + "`,`" + obj.Realm.ToString() + "`);");
		}

		public virtual void Update(AccountEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `account` SET `AccountId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AccountName`='" + m_state.EscapeString(obj.AccountName.ToString()) + "', `BanAuthor`='" + m_state.EscapeString(obj.BanAuthor.ToString()) + "', `BanDuration`='" + m_state.EscapeString(obj.BanDuration.ToString()) + "', `BanReason`='" + m_state.EscapeString(obj.BanReason.ToString()) + "', `CreationDate`='" + m_state.EscapeString(obj.CreationDate.ToString()) + "', `LastLogin`='" + m_state.EscapeString(obj.LastLogin.ToString()) + "', `LastLoginIp`='" + m_state.EscapeString(obj.LastLoginIp.ToString()) + "', `Mail`='" + m_state.EscapeString(obj.Mail.ToString()) + "', `Password`='" + m_state.EscapeString(obj.Password.ToString()) + "', `PrivLevel`='" + m_state.EscapeString(obj.PrivLevel.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "' WHERE `AccountId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(AccountEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `account` WHERE `AccountId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<AccountEntity> SelectAll()
		{
			AccountEntity entity;
			List<AccountEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `account`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<AccountEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new AccountEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `account`");

		}

		protected virtual void FillEntityWithRow(ref AccountEntity entity, MySqlDataReader reader)
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
