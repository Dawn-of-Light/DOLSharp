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
	public class SpawnTemplateDao : ISpawnTemplateDao
	{
		protected static readonly string c_rowFields = "`SpawnTemplateBaseId`,`Count`,`GameNPCTemplateId`,`SpawnGeneratorBaseId`,`SpawnTemplateBaseType`";
		protected readonly MySqlState m_state;

		public virtual SpawnTemplateEntity Find(int id)
		{
			SpawnTemplateEntity result = new SpawnTemplateEntity();
			string command = "SELECT " + c_rowFields + " FROM `spawntemplate` WHERE `SpawnTemplateBaseId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref SpawnTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `spawntemplate` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Count.ToString()) + "','" + m_state.EscapeString(obj.GameNPCTemplate.ToString()) + "','" + m_state.EscapeString(obj.SpawnGeneratorBase.ToString()) + "','" + m_state.EscapeString(obj.SpawnTemplateBaseType.ToString()) + "');");
		}

		public virtual void Update(SpawnTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `spawntemplate` SET `SpawnTemplateBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Count`='" + m_state.EscapeString(obj.Count.ToString()) + "', `GameNPCTemplateId`='" + m_state.EscapeString(obj.GameNPCTemplate.ToString()) + "', `SpawnGeneratorBaseId`='" + m_state.EscapeString(obj.SpawnGeneratorBase.ToString()) + "', `SpawnTemplateBaseType`='" + m_state.EscapeString(obj.SpawnTemplateBaseType.ToString()) + "' WHERE `SpawnTemplateBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(SpawnTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `spawntemplate` WHERE `SpawnTemplateBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<SpawnTemplateEntity> SelectAll()
		{
			SpawnTemplateEntity entity;
			List<SpawnTemplateEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `spawntemplate`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<SpawnTemplateEntity>();
					while (reader.Read())
					{
						entity = new SpawnTemplateEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `spawntemplate`");
		}

		protected virtual void FillEntityWithRow(ref SpawnTemplateEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Count = reader.GetInt32(1);
			entity.GameNPCTemplate = reader.GetInt32(2);
			entity.SpawnGeneratorBase = reader.GetInt32(3);
			entity.SpawnTemplateBaseType = reader.GetString(4);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpawnTemplateEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spawntemplate` ("
				+"`SpawnTemplateBaseId` int NOT NULL,"
				+"`Count` int NOT NULL,"
				+"`GameNPCTemplateId` int,"
				+"`SpawnGeneratorBaseId` int,"
				+"`SpawnTemplateBaseType` char(255) character set latin1 NOT NULL"
				+", primary key `SpawnTemplateBaseId` (`SpawnTemplateBaseId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `spawntemplate`");
			return null;
		}

		public SpawnTemplateDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
