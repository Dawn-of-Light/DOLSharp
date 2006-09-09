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
	public class FactionDao : IFactionDao
	{
		protected static readonly string c_rowFields = "`FactionId`,`Name`";
		protected readonly MySqlState m_state;

		public virtual FactionEntity Find(int factionId)
		{
			FactionEntity result = new FactionEntity();
			string command = "SELECT " + c_rowFields + " FROM `faction` WHERE `FactionId`='" + m_state.EscapeString(factionId.ToString()) + "'";

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

		public virtual void Create(ref FactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `faction` VALUES ('" + m_state.EscapeString(obj.FactionId.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.FactionId = (int) (long) insertedId;
		}

		public virtual void Update(FactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `faction` SET `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "' WHERE `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "'");
		}

		public virtual void Delete(FactionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `faction` WHERE `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<FactionEntity> SelectAll()
		{
			FactionEntity entity;
			List<FactionEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `faction`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<FactionEntity>();
					while (reader.Read())
					{
						entity = new FactionEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `faction`");
		}

		protected virtual void FillEntityWithRow(ref FactionEntity entity, MySqlDataReader reader)
		{
			entity.FactionId = reader.GetInt32(0);
			entity.Name = reader.GetString(1);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(FactionEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `faction` ("
				+"`FactionId` int NOT NULL auto_increment,"
				+"`Name` char(255) character set latin1 NOT NULL"
				+", primary key `FactionId` (`FactionId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `faction`");
			return null;
		}

		public FactionDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
