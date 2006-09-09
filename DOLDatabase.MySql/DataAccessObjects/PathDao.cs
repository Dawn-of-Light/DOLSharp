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
	public class PathDao : IPathDao
	{
		protected static readonly string c_rowFields = "`PathId`,`PathType`,`RegionId`,`StartingPoint`,`SteedModel`,`SteedName`";
		protected readonly MySqlState m_state;

		public virtual PathEntity Find(int id)
		{
			PathEntity result = new PathEntity();
			string command = "SELECT " + c_rowFields + " FROM `path` WHERE `PathId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref PathEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `path` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.PathType.ToString()) + "','" + m_state.EscapeString(obj.RegionId.ToString()) + "','" + m_state.EscapeString(obj.StartingPoint.ToString()) + "','" + m_state.EscapeString(obj.SteedModel.ToString()) + "','" + m_state.EscapeString(obj.SteedName.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(PathEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `path` SET `PathId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `PathType`='" + m_state.EscapeString(obj.PathType.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.RegionId.ToString()) + "', `StartingPoint`='" + m_state.EscapeString(obj.StartingPoint.ToString()) + "', `SteedModel`='" + m_state.EscapeString(obj.SteedModel.ToString()) + "', `SteedName`='" + m_state.EscapeString(obj.SteedName.ToString()) + "' WHERE `PathId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(PathEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `path` WHERE `PathId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<PathEntity> SelectAll()
		{
			PathEntity entity;
			List<PathEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `path`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<PathEntity>();
					while (reader.Read())
					{
						entity = new PathEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `path`");
		}

		protected virtual void FillEntityWithRow(ref PathEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.PathType = reader.GetString(1);
			entity.RegionId = reader.GetInt32(2);
			entity.StartingPoint = reader.GetInt32(3);
			entity.SteedModel = reader.GetInt32(4);
			entity.SteedName = reader.GetString(5);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(PathEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `path` ("
				+"`PathId` int NOT NULL auto_increment,"
				+"`PathType` char(255) character set latin1 NOT NULL,"
				+"`RegionId` int NOT NULL,"
				+"`StartingPoint` int,"
				+"`SteedModel` int NOT NULL,"
				+"`SteedName` char(255) character set latin1 NOT NULL"
				+", primary key `PathId` (`PathId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `path`");
			return null;
		}

		public PathDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
