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
		private readonly MySqlState m_state;

		public virtual LineXSpellEntity Find(int key)
		{
			LineXSpellEntity result = new LineXSpellEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `linexspell` WHERE `LineXSpellId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(LineXSpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `linexspell` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Level.ToString() + "`,`" + obj.LineName.ToString() + "`,`" + obj.SpellId.ToString() + "`);");
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
					results = new List<LineXSpellEntity>(reader.FieldCount);
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

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `linexspell`");

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
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `linexspell` ("
				+"`LineXSpellId` int,"
				+"`Level` int,"
				+"`LineName` varchar(500) character set unicode,"
				+"`SpellId` int"
				+", primary key `LineXSpellId` (`LineXSpellId`)"
			);
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
