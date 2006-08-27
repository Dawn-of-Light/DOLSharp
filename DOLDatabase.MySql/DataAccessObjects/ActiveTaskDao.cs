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
	public class ActiveTaskDao : IActiveTaskDao
	{
		protected static readonly string c_rowFields = "`AbstractTaskId`,`ItemName`,`RewardGiverName`,`StartingPlayedTime`,`TargetKilled`,`TargetMobName`,`TaskType`";
		protected readonly MySqlState m_state;

		public virtual ActiveTaskEntity Find(int abstractTask)
		{
			ActiveTaskEntity result = new ActiveTaskEntity();
			string command = "SELECT " + c_rowFields + " FROM `activetasks` WHERE `AbstractTaskId`='" + m_state.EscapeString(abstractTask.ToString()) + "'";

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

		public virtual void Create(ActiveTaskEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `activetasks` VALUES ('" + m_state.EscapeString(obj.AbstractTask.ToString()) + "','" + m_state.EscapeString(obj.ItemName.ToString()) + "','" + m_state.EscapeString(obj.RewardGiverName.ToString()) + "','" + m_state.EscapeString(obj.StartingPlayedTime.ToString()) + "','" + m_state.EscapeString(obj.TargetKilled.ToString()) + "','" + m_state.EscapeString(obj.TargetMobName.ToString()) + "','" + m_state.EscapeString(obj.TaskType.ToString()) + "');");
		}

		public virtual void Update(ActiveTaskEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `activetasks` SET `AbstractTaskId`='" + m_state.EscapeString(obj.AbstractTask.ToString()) + "', `ItemName`='" + m_state.EscapeString(obj.ItemName.ToString()) + "', `RewardGiverName`='" + m_state.EscapeString(obj.RewardGiverName.ToString()) + "', `StartingPlayedTime`='" + m_state.EscapeString(obj.StartingPlayedTime.ToString()) + "', `TargetKilled`='" + m_state.EscapeString(obj.TargetKilled.ToString()) + "', `TargetMobName`='" + m_state.EscapeString(obj.TargetMobName.ToString()) + "', `TaskType`='" + m_state.EscapeString(obj.TaskType.ToString()) + "' WHERE `AbstractTaskId`='" + m_state.EscapeString(obj.AbstractTask.ToString()) + "'");
		}

		public virtual void Delete(ActiveTaskEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `activetasks` WHERE `AbstractTaskId`='" + m_state.EscapeString(obj.AbstractTask.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ActiveTaskEntity> SelectAll()
		{
			ActiveTaskEntity entity;
			List<ActiveTaskEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `activetasks`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ActiveTaskEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new ActiveTaskEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `activetasks`");
		}

		protected virtual void FillEntityWithRow(ref ActiveTaskEntity entity, MySqlDataReader reader)
		{
			entity.AbstractTask = reader.GetInt32(0);
			entity.ItemName = reader.GetString(1);
			entity.RewardGiverName = reader.GetString(2);
			entity.StartingPlayedTime = reader.GetInt64(3);
			entity.TargetKilled = reader.GetBoolean(4);
			entity.TargetMobName = reader.GetString(5);
			entity.TaskType = reader.GetString(6);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ActiveTaskEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `activetasks` ("
				+"`AbstractTaskId` int,"
				+"`ItemName` varchar(255) character set utf8,"
				+"`RewardGiverName` varchar(255) character set utf8,"
				+"`StartingPlayedTime` bigint,"
				+"`TargetKilled` bit,"
				+"`TargetMobName` varchar(255) character set utf8,"
				+"`TaskType` varchar(255) character set utf8"
				+", primary key `AbstractTaskId` (`AbstractTaskId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `activetasks`");
			return null;
		}

		public ActiveTaskDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
