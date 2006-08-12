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
	public class FinishedQuestDao : IFinishedQuestDao
	{
		protected static readonly string c_rowFields = "`Count`,`FinishedQuestType`,`PersistantGameObjectId`";
		private readonly MySqlState m_state;

		public virtual FinishedQuestEntity Find(byte count, string finishedQuestType, int persistantGameObject)
		{
			FinishedQuestEntity result = new FinishedQuestEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `finishedquests` WHERE `Count`='" + m_state.EscapeString(count.ToString()) + "', `FinishedQuestType`='" + m_state.EscapeString(finishedQuestType.ToString()) + "', `PersistantGameObjectId`='" + m_state.EscapeString(persistantGameObject.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(FinishedQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `finishedquests` VALUES (`" + obj.Count.ToString() + "`,`" + obj.FinishedQuestType.ToString() + "`,`" + obj.PersistantGameObject.ToString() + "`);");
		}

		public virtual void Update(FinishedQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `finishedquests` SET `Count`='" + m_state.EscapeString(obj.Count.ToString()) + "', `FinishedQuestType`='" + m_state.EscapeString(obj.FinishedQuestType.ToString()) + "', `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "' WHERE `Count`='" + m_state.EscapeString(obj.Count.ToString()) + "'AND `FinishedQuestType`='" + m_state.EscapeString(obj.FinishedQuestType.ToString()) + "'AND `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "'");
		}

		public virtual void Delete(FinishedQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `finishedquests` WHERE `Count`='" + m_state.EscapeString(obj.Count.ToString()) + "'AND `FinishedQuestType`='" + m_state.EscapeString(obj.FinishedQuestType.ToString()) + "'AND `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<FinishedQuestEntity> SelectAll()
		{
			FinishedQuestEntity entity;
			List<FinishedQuestEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `finishedquests`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<FinishedQuestEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new FinishedQuestEntity();
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
			"SELECT COUNT(*) FROM `finishedquests`");

		}

		protected virtual void FillEntityWithRow(ref FinishedQuestEntity entity, MySqlDataReader reader)
		{
			entity.Count = reader.GetByte(0);
			entity.FinishedQuestType = reader.GetString(1);
			entity.PersistantGameObject = reader.GetInt32(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(FinishedQuestEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `finishedquests` ("
				+"`Count` tinyint unsigned,"
				+"`FinishedQuestType` varchar(510) character set unicode,"
				+"`PersistantGameObjectId` int"
				+", primary key `Count` (`Count`,`FinishedQuestType`,`PersistantGameObjectId`)"
			);
		}

		public FinishedQuestDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
