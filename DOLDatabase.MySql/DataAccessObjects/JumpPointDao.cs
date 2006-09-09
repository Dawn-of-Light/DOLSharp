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
	public class JumpPointDao : IJumpPointDao
	{
		protected static readonly string c_rowFields = "`JumpPointId`,`AllowedRealm`,`Heading`,`JumpPointType`,`Region`,`X`,`Y`,`Z`";
		protected readonly MySqlState m_state;

		public virtual JumpPointEntity Find(int id)
		{
			JumpPointEntity result = new JumpPointEntity();
			string command = "SELECT " + c_rowFields + " FROM `jumppoint` WHERE `JumpPointId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref JumpPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `jumppoint` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AllowedRealm.ToString()) + "','" + m_state.EscapeString(obj.Heading.ToString()) + "','" + m_state.EscapeString(obj.JumpPointType.ToString()) + "','" + m_state.EscapeString(obj.Region.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "','" + m_state.EscapeString(obj.Z.ToString()) + "');");
		}

		public virtual void Update(JumpPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `jumppoint` SET `JumpPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AllowedRealm`='" + m_state.EscapeString(obj.AllowedRealm.ToString()) + "', `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `JumpPointType`='" + m_state.EscapeString(obj.JumpPointType.ToString()) + "', `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `JumpPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(JumpPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `jumppoint` WHERE `JumpPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<JumpPointEntity> SelectAll()
		{
			JumpPointEntity entity;
			List<JumpPointEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `jumppoint`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<JumpPointEntity>();
					while (reader.Read())
					{
						entity = new JumpPointEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `jumppoint`");
		}

		protected virtual void FillEntityWithRow(ref JumpPointEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AllowedRealm = reader.GetByte(1);
			entity.Heading = reader.GetInt32(2);
			entity.JumpPointType = reader.GetString(3);
			entity.Region = reader.GetInt32(4);
			entity.X = reader.GetInt32(5);
			entity.Y = reader.GetInt32(6);
			entity.Z = reader.GetInt32(7);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(JumpPointEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `jumppoint` ("
				+"`JumpPointId` int NOT NULL,"
				+"`AllowedRealm` tinyint unsigned NOT NULL,"
				+"`Heading` int NOT NULL,"
				+"`JumpPointType` char(255) character set latin1 NOT NULL,"
				+"`Region` int NOT NULL,"
				+"`X` int NOT NULL,"
				+"`Y` int NOT NULL,"
				+"`Z` int NOT NULL"
				+", primary key `JumpPointId` (`JumpPointId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `jumppoint`");
			return null;
		}

		public JumpPointDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
