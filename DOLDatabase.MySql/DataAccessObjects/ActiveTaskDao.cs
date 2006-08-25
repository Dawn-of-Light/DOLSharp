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
		private readonly MySqlState m_state;

		public virtual ActiveTaskEntity Find(int abstractTask)
		{
			ActiveTaskEntity result = new ActiveTaskEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `activetasks` WHERE `AbstractTaskId`='" + m_state.EscapeString(abstractTask.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(ActiveTaskEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `activetasks` VALUES (`" + obj.AbstractTask.ToString() + "`,`" + obj.ItemName.ToString() + "`,`" + obj.RewardGiverName.ToString() + "`,`" + obj.StartingPlayedTime.ToString() + "`,`" + obj.TargetKilled.ToString() + "`,`" + obj.TargetMobName.ToString() + "`,`" + obj.TaskType.ToString() + "`);");
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

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `activetasks`");

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
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `activetasks` ("
				+"`AbstractTaskId` int,"
				+"`ItemName` varchar(510) character set unicode,"
				+"`RewardGiverName` varchar(510) character set unicode,"
				+"`StartingPlayedTime` bigint,"
				+"`TargetKilled` bit,"
				+"`TargetMobName` varchar(510) character set unicode,"
				+"`TaskType` varchar(510) character set unicode"
				+", primary key `AbstractTaskId` (`AbstractTaskId`)"
			);
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
