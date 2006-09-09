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
	public class LineXSpellDao : ILineXSpellDao
	{
		protected static readonly string c_rowFields = "`LineXSpellId`,`Level`,`LineName`,`SpellId`";
		protected readonly MySqlState m_state;

		public virtual LineXSpellEntity Find(int id)
		{
			LineXSpellEntity result = new LineXSpellEntity();
			string command = "SELECT " + c_rowFields + " FROM `linexspell` WHERE `LineXSpellId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref LineXSpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `linexspell` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Level.ToString()) + "','" + m_state.EscapeString(obj.LineName.ToString()) + "','" + m_state.EscapeString(obj.SpellId.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(LineXSpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `linexspell` SET `LineXSpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `LineName`='" + m_state.EscapeString(obj.LineName.ToString()) + "', `SpellId`='" + m_state.EscapeString(obj.SpellId.ToString()) + "' WHERE `LineXSpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(LineXSpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `linexspell` WHERE `LineXSpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<LineXSpellEntity> SelectAll()
		{
			LineXSpellEntity entity;
			List<LineXSpellEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `linexspell`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<LineXSpellEntity>();
					while (reader.Read())
					{
						entity = new LineXSpellEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `linexspell`");
		}

		protected virtual void FillEntityWithRow(ref LineXSpellEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Level = reader.GetInt32(1);
			entity.LineName = reader.GetString(2);
			entity.SpellId = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(LineXSpellEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `linexspell` ("
				+"`LineXSpellId` int NOT NULL auto_increment,"
				+"`Level` int NOT NULL,"
				+"`LineName` char(255) character set latin1 NOT NULL,"
				+"`SpellId` int NOT NULL"
				+", primary key `LineXSpellId` (`LineXSpellId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `linexspell`");
			return null;
		}

		public LineXSpellDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
