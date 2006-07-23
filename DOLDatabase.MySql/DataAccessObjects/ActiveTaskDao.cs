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
		private readonly MySqlState m_state;

		public virtual ActiveTaskEntity Find(int key)
		{
			ActiveTaskEntity result = new ActiveTaskEntity();
			m_state.ExecuteQuery(
				"SELECT `AbstractTaskId`,`ItemName`,`RewardGiverName`,`StartingPlayedTime`,`TargetKilled`,`TargetMobName`,`TaskType` FROM `activetasks` WHERE `AbstractTaskId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(ActiveTaskEntity obj)
		{
		}

		public virtual void Update(ActiveTaskEntity obj)
		{
		}

		public virtual void Delete(ActiveTaskEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref ActiveTaskEntity entity, MySqlDataReader reader)
		{
			entity.AbstractTask = reader.GetInt32(0);
			entity.ItemName = reader.GetString(1);
			entity.RewardGiverName = reader.GetString(2);
			entity.StartingPlayedTime = reader.GetInt64(3);
			entity.TargetKilled = reader.GetString(4);
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
				+"`TargetKilled` char(1) character set ascii,"
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
