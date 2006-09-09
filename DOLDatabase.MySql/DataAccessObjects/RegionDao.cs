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
	public class RegionDao : IRegionDao
	{
		protected static readonly string c_rowFields = "`RegionId`,`Description`,`Expansion`,`IsDivingEnabled`,`IsDungeon`,`IsInstance`,`Type`";
		protected readonly MySqlState m_state;

		public virtual RegionEntity Find(int id)
		{
			RegionEntity result = new RegionEntity();
			string command = "SELECT " + c_rowFields + " FROM `region` WHERE `RegionId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref RegionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `region` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Description.ToString()) + "','" + m_state.EscapeString(obj.Expansion.ToString()) + "','" + m_state.EscapeString(obj.IsDivingEnabled.ToString()) + "','" + m_state.EscapeString(obj.IsDungeon.ToString()) + "','" + m_state.EscapeString(obj.IsInstance.ToString()) + "','" + m_state.EscapeString(obj.Type.ToString()) + "');");
		}

		public virtual void Update(RegionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `region` SET `RegionId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `Expansion`='" + m_state.EscapeString(obj.Expansion.ToString()) + "', `IsDivingEnabled`='" + m_state.EscapeString(obj.IsDivingEnabled.ToString()) + "', `IsDungeon`='" + m_state.EscapeString(obj.IsDungeon.ToString()) + "', `IsInstance`='" + m_state.EscapeString(obj.IsInstance.ToString()) + "', `Type`='" + m_state.EscapeString(obj.Type.ToString()) + "' WHERE `RegionId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(RegionEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `region` WHERE `RegionId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<RegionEntity> SelectAll()
		{
			RegionEntity entity;
			List<RegionEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `region`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<RegionEntity>();
					while (reader.Read())
					{
						entity = new RegionEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `region`");
		}

		protected virtual void FillEntityWithRow(ref RegionEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Description = reader.GetString(1);
			entity.Expansion = reader.GetByte(2);
			entity.IsDivingEnabled = reader.GetBoolean(3);
			entity.IsDungeon = reader.GetBoolean(4);
			entity.IsInstance = reader.GetBoolean(5);
			entity.Type = reader.GetByte(6);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(RegionEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `region` ("
				+"`RegionId` int NOT NULL,"
				+"`Description` char(255) character set latin1 NOT NULL,"
				+"`Expansion` tinyint unsigned NOT NULL,"
				+"`IsDivingEnabled` bit NOT NULL,"
				+"`IsDungeon` bit NOT NULL,"
				+"`IsInstance` bit NOT NULL,"
				+"`Type` tinyint unsigned NOT NULL"
				+", primary key `RegionId` (`RegionId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `region`");
			return null;
		}

		public RegionDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
