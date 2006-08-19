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
	public class FriendFactionDao : IFriendFactionDao
	{
		protected static readonly string c_rowFields = "`FactionId`,`FriendFactionID`";
		private readonly MySqlState m_state;

		public virtual FriendFactionEntity Find()
		{
			FriendFactionEntity result = new FriendFactionEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `friendfactions` WHERE ",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(FriendFactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `friendfactions` VALUES (`" + obj.FactionId.ToString() + "`,`" + obj.FriendFactionId.ToString() + "`);");
		}

		public virtual void Update(FriendFactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `friendfactions` SET `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "', `FriendFactionID`='" + m_state.EscapeString(obj.FriendFactionId.ToString()) + "' WHERE ");
		}

		public virtual void Delete(FriendFactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `friendfactions` WHERE ");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<FriendFactionEntity> SelectAll()
		{
			FriendFactionEntity entity;
			List<FriendFactionEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `friendfactions`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<FriendFactionEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new FriendFactionEntity();
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
			"SELECT COUNT(*) FROM `friendfactions`");

		}

		protected virtual void FillEntityWithRow(ref FriendFactionEntity entity, MySqlDataReader reader)
		{
			entity.FactionId = reader.GetInt32(0);
			entity.FriendFactionId = reader.GetInt32(1);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(FriendFactionEntity); }
		}

		public IList<string> VerifySchema()
		{
//			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `friendfactions` ("
				+"`FactionId` int,"
				+"`FriendFactionID` int)"

			);
			return null;
		}

		public FriendFactionDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
