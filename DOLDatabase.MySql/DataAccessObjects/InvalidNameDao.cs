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
	public class InvalidNameDao : IInvalidNameDao
	{
		protected static readonly string c_rowFields = "`InvalidNameId`,`Name`";
		private readonly MySqlState m_state;

		public virtual InvalidNameEntity Find(int id)
		{
			InvalidNameEntity result = new InvalidNameEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `invalidname` WHERE `InvalidNameId`='" + m_state.EscapeString(id.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(InvalidNameEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `invalidname` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Name.ToString() + "`);");
		}

		public virtual void Update(InvalidNameEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `invalidname` SET `InvalidNameId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "' WHERE `InvalidNameId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(InvalidNameEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `invalidname` WHERE `InvalidNameId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<InvalidNameEntity> SelectAll()
		{
			InvalidNameEntity entity;
			List<InvalidNameEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `invalidname`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<InvalidNameEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new InvalidNameEntity();
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
			"SELECT COUNT(*) FROM `invalidname`");

		}

		protected virtual void FillEntityWithRow(ref InvalidNameEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Name = reader.GetString(1);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(InvalidNameEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `invalidname` ("
				+"`InvalidNameId` int,"
				+"`Name` varchar(510) character set unicode"
				+", primary key `InvalidNameId` (`InvalidNameId`)"
			);
		}

		public InvalidNameDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
