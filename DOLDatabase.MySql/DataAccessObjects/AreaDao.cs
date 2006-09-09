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
	public class AreaDao : IAreaDao
	{
		protected static readonly string c_rowFields = "`AreaId`,`AreaType`,`Description`,`Height`,`IsBroadcastEnabled`,`Radius`,`RegionId`,`Sound`,`Width`,`X`,`Y`";
		protected readonly MySqlState m_state;

		public virtual AreaEntity Find(int id)
		{
			AreaEntity result = new AreaEntity();
			string command = "SELECT " + c_rowFields + " FROM `area` WHERE `AreaId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref AreaEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `area` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AreaType.ToString()) + "','" + m_state.EscapeString(obj.Description.ToString()) + "','" + m_state.EscapeString(obj.Height.ToString()) + "','" + m_state.EscapeString(obj.IsBroadcastEnabled.ToString()) + "','" + m_state.EscapeString(obj.Radius.ToString()) + "','" + m_state.EscapeString(obj.RegionId.ToString()) + "','" + m_state.EscapeString(obj.Sound.ToString()) + "','" + m_state.EscapeString(obj.Width.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(AreaEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `area` SET `AreaId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AreaType`='" + m_state.EscapeString(obj.AreaType.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `Height`='" + m_state.EscapeString(obj.Height.ToString()) + "', `IsBroadcastEnabled`='" + m_state.EscapeString(obj.IsBroadcastEnabled.ToString()) + "', `Radius`='" + m_state.EscapeString(obj.Radius.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.RegionId.ToString()) + "', `Sound`='" + m_state.EscapeString(obj.Sound.ToString()) + "', `Width`='" + m_state.EscapeString(obj.Width.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "' WHERE `AreaId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(AreaEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `area` WHERE `AreaId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<AreaEntity> SelectAll()
		{
			AreaEntity entity;
			List<AreaEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `area`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<AreaEntity>();
					while (reader.Read())
					{
						entity = new AreaEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `area`");
		}

		protected virtual void FillEntityWithRow(ref AreaEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AreaType = reader.GetString(1);
			entity.Description = reader.GetString(2);
			entity.Height = reader.GetInt32(3);
			entity.IsBroadcastEnabled = reader.GetBoolean(4);
			entity.Radius = reader.GetInt32(5);
			entity.RegionId = reader.GetInt32(6);
			entity.Sound = reader.GetByte(7);
			entity.Width = reader.GetInt32(8);
			entity.X = reader.GetInt32(9);
			entity.Y = reader.GetInt32(10);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(AreaEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `area` ("
				+"`AreaId` int NOT NULL auto_increment,"
				+"`AreaType` char(255) character set latin1 NOT NULL,"
				+"`Description` char(255) character set latin1 NOT NULL,"
				+"`Height` int NOT NULL,"
				+"`IsBroadcastEnabled` bit NOT NULL,"
				+"`Radius` int NOT NULL,"
				+"`RegionId` int NOT NULL,"
				+"`Sound` tinyint unsigned NOT NULL,"
				+"`Width` int NOT NULL,"
				+"`X` int NOT NULL,"
				+"`Y` int NOT NULL"
				+", primary key `AreaId` (`AreaId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `area`");
			return null;
		}

		public AreaDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
