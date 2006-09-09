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
	public class ActiveQuestDao : IActiveQuestDao
	{
		protected static readonly string c_rowFields = "`AbstractQuestId`,`PersistantGameObjectId`,`QuestType`,`Step`";
		protected readonly MySqlState m_state;

		public virtual ActiveQuestEntity Find(int id)
		{
			ActiveQuestEntity result = new ActiveQuestEntity();
			string command = "SELECT " + c_rowFields + " FROM `activequests` WHERE `AbstractQuestId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref ActiveQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `activequests` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "','" + m_state.EscapeString(obj.QuestType.ToString()) + "','" + m_state.EscapeString(obj.Step1.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(ActiveQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `activequests` SET `AbstractQuestId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `PersistantGameObjectId`='" + m_state.EscapeString(obj.PersistantGameObject.ToString()) + "', `QuestType`='" + m_state.EscapeString(obj.QuestType.ToString()) + "', `Step`='" + m_state.EscapeString(obj.Step1.ToString()) + "' WHERE `AbstractQuestId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(ActiveQuestEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `activequests` WHERE `AbstractQuestId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ActiveQuestEntity> SelectAll()
		{
			ActiveQuestEntity entity;
			List<ActiveQuestEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `activequests`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ActiveQuestEntity>();
					while (reader.Read())
					{
						entity = new ActiveQuestEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `activequests`");
		}

		protected virtual void FillEntityWithRow(ref ActiveQuestEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.PersistantGameObject = reader.GetInt32(1);
			entity.QuestType = reader.GetString(2);
			entity.Step1 = reader.GetByte(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ActiveQuestEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `activequests` ("
				+"`AbstractQuestId` int NOT NULL auto_increment,"
				+"`PersistantGameObjectId` int NOT NULL,"
				+"`QuestType` char(255) character set latin1 NOT NULL,"
				+"`Step` tinyint unsigned NOT NULL"
				+", primary key `AbstractQuestId` (`AbstractQuestId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `activequests`");
			return null;
		}

		public ActiveQuestDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
