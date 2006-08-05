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
		private readonly MySqlState m_state;

		public virtual PathEntity Find(int key)
		{
			PathEntity result = new PathEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `path` WHERE `PathId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(PathEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `path` VALUES (`" + obj.Id.ToString() + "`,`" + obj.PathType.ToString() + "`,`" + obj.RegionId.ToString() + "`,`" + obj.StartingPoint.ToString() + "`,`" + obj.SteedModel.ToString() + "`,`" + obj.SteedName.ToString() + "`);");
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

		public virtual int CountAll()
		{
			return -1;
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
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `path` ("
				+"`PathId` int,"
				+"`PathType` varchar(510) character set unicode,"
				+"`RegionId` int,"
				+"`StartingPoint` int,"
				+"`SteedModel` int,"
				+"`SteedName` varchar(510) character set unicode"
				+", primary key `PathId` (`PathId`)"
			);
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
