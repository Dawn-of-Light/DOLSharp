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
		protected readonly MySqlState m_state;

		public virtual FriendFactionEntity Find()
		{
			FriendFactionEntity result = new FriendFactionEntity();
			string command = "SELECT " + c_rowFields + " FROM `friendfactions` WHERE ";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						result = null;
					}
					else
					{
						FillEntityWithRow(ref result, reader);
					}
				}
			);

			return result;
		}

		public virtual void Create(ref FriendFactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `friendfactions` VALUES ('" + m_state.EscapeString(obj.FactionId.ToString()) + "','" + m_state.EscapeString(obj.FriendFactionId.ToString()) + "');");
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
					results = new List<FriendFactionEntity>();
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

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `friendfactions`");
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
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `friendfactions` ("
				+"`FactionId` int,"
				+"`FriendFactionID` int"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `friendfactions`");
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
