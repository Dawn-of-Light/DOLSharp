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
	public class SalvageDao : ISalvageDao
	{
		protected static readonly string c_rowFields = "`SalvageId`,`MaterialItemtemplate`,`ObjectType`,`SalvageLevel`";
		protected readonly MySqlState m_state;

		public virtual SalvageEntity Find(int id)
		{
			SalvageEntity result = new SalvageEntity();
			string command = "SELECT " + c_rowFields + " FROM `salvage` WHERE `SalvageId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref SalvageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `salvage` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.MaterialItemtemplate.ToString()) + "','" + m_state.EscapeString(obj.ObjectType.ToString()) + "','" + m_state.EscapeString(obj.SalvageLevel.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(SalvageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `salvage` SET `SalvageId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `MaterialItemtemplate`='" + m_state.EscapeString(obj.MaterialItemtemplate.ToString()) + "', `ObjectType`='" + m_state.EscapeString(obj.ObjectType.ToString()) + "', `SalvageLevel`='" + m_state.EscapeString(obj.SalvageLevel.ToString()) + "' WHERE `SalvageId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(SalvageEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `salvage` WHERE `SalvageId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<SalvageEntity> SelectAll()
		{
			SalvageEntity entity;
			List<SalvageEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `salvage`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<SalvageEntity>();
					while (reader.Read())
					{
						entity = new SalvageEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `salvage`");
		}

		protected virtual void FillEntityWithRow(ref SalvageEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.MaterialItemtemplate = reader.GetString(1);
			entity.ObjectType = reader.GetInt32(2);
			entity.SalvageLevel = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SalvageEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `salvage` ("
				+"`SalvageId` int NOT NULL auto_increment,"
				+"`MaterialItemtemplate` char(255) character set latin1 NOT NULL,"
				+"`ObjectType` int NOT NULL,"
				+"`SalvageLevel` int NOT NULL"
				+", primary key `SalvageId` (`SalvageId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `salvage`");
			return null;
		}

		public SalvageDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
