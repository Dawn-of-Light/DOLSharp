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
	public class AllianceDao : IAllianceDao
	{
		protected static readonly string c_rowFields = "`AllianceId`,`AllianceLeader`,`AMotd`";
		protected readonly MySqlState m_state;

		public virtual AllianceEntity Find(int id)
		{
			AllianceEntity result = new AllianceEntity();
			string command = "SELECT " + c_rowFields + " FROM `alliance` WHERE `AllianceId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref AllianceEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `alliance` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AllianceLeader.ToString()) + "','" + m_state.EscapeString(obj.AMotd.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(AllianceEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `alliance` SET `AllianceId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AllianceLeader`='" + m_state.EscapeString(obj.AllianceLeader.ToString()) + "', `AMotd`='" + m_state.EscapeString(obj.AMotd.ToString()) + "' WHERE `AllianceId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(AllianceEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `alliance` WHERE `AllianceId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<AllianceEntity> SelectAll()
		{
			AllianceEntity entity;
			List<AllianceEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `alliance`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<AllianceEntity>();
					while (reader.Read())
					{
						entity = new AllianceEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `alliance`");
		}

		protected virtual void FillEntityWithRow(ref AllianceEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AllianceLeader = reader.GetInt32(1);
			entity.AMotd = reader.GetString(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(AllianceEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `alliance` ("
				+"`AllianceId` int NOT NULL auto_increment,"
				+"`AllianceLeader` int,"
				+"`AMotd` char(255) character set latin1 NOT NULL"
				+", primary key `AllianceId` (`AllianceId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `alliance`");
			return null;
		}

		public AllianceDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
