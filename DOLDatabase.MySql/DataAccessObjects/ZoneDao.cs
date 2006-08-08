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
	public class ZoneDao : IZoneDao
	{
		protected static readonly string c_rowFields = "`ZoneId`,`Description`,`RegionId`,`XOffset`,`YOffset`";
		private readonly MySqlState m_state;

		public virtual ZoneEntity Find(int key)
		{
			ZoneEntity result = new ZoneEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `zone` WHERE `ZoneId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(ZoneEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `zone` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Description.ToString() + "`,`" + obj.Region.ToString() + "`,`" + obj.XOffset.ToString() + "`,`" + obj.YOffset.ToString() + "`);");
		}

		public virtual void Update(ZoneEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `zone` SET `ZoneId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.Region.ToString()) + "', `XOffset`='" + m_state.EscapeString(obj.XOffset.ToString()) + "', `YOffset`='" + m_state.EscapeString(obj.YOffset.ToString()) + "' WHERE `ZoneId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(ZoneEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `zone` WHERE `ZoneId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ZoneEntity> SelectAll()
		{
			ZoneEntity entity;
			List<ZoneEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `zone`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ZoneEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new ZoneEntity();
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
			"SELECT COUNT(*) FROM `zone`");

		}

		protected virtual void FillEntityWithRow(ref ZoneEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Description = reader.GetString(1);
			entity.Region = reader.GetInt32(2);
			entity.XOffset = reader.GetInt32(3);
			entity.YOffset = reader.GetInt32(4);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ZoneEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `zone` ("
				+"`ZoneId` int,"
				+"`Description` varchar(510) character set unicode,"
				+"`RegionId` int,"
				+"`XOffset` int,"
				+"`YOffset` int"
				+", primary key `ZoneId` (`ZoneId`)"
			);
		}

		public ZoneDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
