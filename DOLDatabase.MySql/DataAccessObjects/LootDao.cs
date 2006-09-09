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
	public class LootDao : ILootDao
	{
		protected static readonly string c_rowFields = "`LootId`,`Chance`,`GenericItemTemplateId`,`LootListId`,`LootType`";
		protected readonly MySqlState m_state;

		public virtual LootEntity Find(int id)
		{
			LootEntity result = new LootEntity();
			string command = "SELECT " + c_rowFields + " FROM `loot` WHERE `LootId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref LootEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `loot` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Chance.ToString()) + "','" + m_state.EscapeString(obj.GenericItemTemplate.ToString()) + "','" + m_state.EscapeString(obj.LootListId.ToString()) + "','" + m_state.EscapeString(obj.LootType.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(LootEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `loot` SET `LootId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Chance`='" + m_state.EscapeString(obj.Chance.ToString()) + "', `GenericItemTemplateId`='" + m_state.EscapeString(obj.GenericItemTemplate.ToString()) + "', `LootListId`='" + m_state.EscapeString(obj.LootListId.ToString()) + "', `LootType`='" + m_state.EscapeString(obj.LootType.ToString()) + "' WHERE `LootId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(LootEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `loot` WHERE `LootId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<LootEntity> SelectAll()
		{
			LootEntity entity;
			List<LootEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `loot`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<LootEntity>();
					while (reader.Read())
					{
						entity = new LootEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `loot`");
		}

		protected virtual void FillEntityWithRow(ref LootEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Chance = reader.GetInt32(1);
			entity.GenericItemTemplate = reader.GetString(2);
			entity.LootListId = reader.GetInt32(3);
			entity.LootType = reader.GetString(4);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(LootEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `loot` ("
				+"`LootId` int NOT NULL auto_increment,"
				+"`Chance` int NOT NULL,"
				+"`GenericItemTemplateId` char(255) character set latin1,"
				+"`LootListId` int,"
				+"`LootType` char(255) character set latin1 NOT NULL"
				+", primary key `LootId` (`LootId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `loot`");
			return null;
		}

		public LootDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
