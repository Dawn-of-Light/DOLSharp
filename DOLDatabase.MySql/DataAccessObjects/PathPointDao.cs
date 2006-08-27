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
	public class PathPointDao : IPathPointDao
	{
		protected static readonly string c_rowFields = "`PathPointId`,`NextPoint`,`Speed`,`X`,`Y`,`Z`";
		protected readonly MySqlState m_state;

		public virtual PathPointEntity Find(int id)
		{
			PathPointEntity result = new PathPointEntity();
			string command = "SELECT " + c_rowFields + " FROM `pathpoint` WHERE `PathPointId`='" + m_state.EscapeString(id.ToString()) + "'";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						throw new RowNotFoundException();
					}
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(PathPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `pathpoint` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.NextPoint.ToString()) + "','" + m_state.EscapeString(obj.Speed.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "','" + m_state.EscapeString(obj.Z.ToString()) + "');");
		}

		public virtual void Update(PathPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `pathpoint` SET `PathPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `NextPoint`='" + m_state.EscapeString(obj.NextPoint.ToString()) + "', `Speed`='" + m_state.EscapeString(obj.Speed.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `PathPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(PathPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `pathpoint` WHERE `PathPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<PathPointEntity> SelectAll()
		{
			PathPointEntity entity;
			List<PathPointEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `pathpoint`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<PathPointEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new PathPointEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `pathpoint`");
		}

		protected virtual void FillEntityWithRow(ref PathPointEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.NextPoint = reader.GetInt32(1);
			entity.Speed = reader.GetInt32(2);
			entity.X = reader.GetInt32(3);
			entity.Y = reader.GetInt32(4);
			entity.Z = reader.GetInt32(5);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(PathPointEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `pathpoint` ("
				+"`PathPointId` int,"
				+"`NextPoint` int,"
				+"`Speed` int,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `PathPointId` (`PathPointId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `pathpoint`");
			return null;
		}

		public PathPointDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
