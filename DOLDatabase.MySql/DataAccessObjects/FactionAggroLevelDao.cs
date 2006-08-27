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
	public class FactionAggroLevelDao : IFactionAggroLevelDao
	{
		protected static readonly string c_rowFields = "`FactionId`,`PersistantGameObjectId`,`AggroLevel`";
		protected readonly MySqlState m_state;

		public virtual FactionAggroLevelEntity Find(int factionId, int persistantGameObject)
		{
			FactionAggroLevelEntity result = new FactionAggroLevelEntity();
			string command = "SELECT " + c_rowFields + " FROM `factionaggrolevel` WHERE `FactionId`='" + m_state.EscapeString(factionId.ToString()) + "', `PersistantGameObjectId`='" + m_state.EscapeString(persistantGameObject.ToString()) + "'";

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

		public virtual void Create(FactionAggroLevelEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `factionaggrolevel` VALUES ('" + m_state.EscapeString(obj.FactionId.ToString()) + "','" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "','" + m_state.EscapeString(obj.AggroLevel.ToString()) + "');");
		}

		public virtual void Update(FactionAggroLevelEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `factionaggrolevel` SET `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "', `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "', `AggroLevel`='" + m_state.EscapeString(obj.AggroLevel.ToString()) + "' WHERE `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "'AND `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "'");
		}

		public virtual void Delete(FactionAggroLevelEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `factionaggrolevel` WHERE `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "'AND `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<FactionAggroLevelEntity> SelectAll()
		{
			FactionAggroLevelEntity entity;
			List<FactionAggroLevelEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `factionaggrolevel`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<FactionAggroLevelEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new FactionAggroLevelEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `factionaggrolevel`");
		}

		protected virtual void FillEntityWithRow(ref FactionAggroLevelEntity entity, MySqlDataReader reader)
		{
			entity.FactionId = reader.GetInt32(0);
			entity.PersistantGameObject = reader.GetInt32(1);
			entity.AggroLevel = reader.GetInt32(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(FactionAggroLevelEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `factionaggrolevel` ("
				+"`FactionId` int,"
				+"`PersistantGameObjectId` int,"
				+"`AggroLevel` int"
				+", primary key `FactionIdPersistantGameObjectId` (`FactionId`,`PersistantGameObjectId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `factionaggrolevel`");
			return null;
		}

		public FactionAggroLevelDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
