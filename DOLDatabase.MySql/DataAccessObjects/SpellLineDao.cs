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
	public class SpellLineDao : ISpellLineDao
	{
		protected static readonly string c_rowFields = "`KeyName`,`IsBaseLine`,`Name`,`Spec`";
		protected readonly MySqlState m_state;

		public virtual SpellLineEntity Find(string keyName)
		{
			SpellLineEntity result = new SpellLineEntity();
			string command = "SELECT " + c_rowFields + " FROM `spellline` WHERE `KeyName`='" + m_state.EscapeString(keyName.ToString()) + "'";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						throw new RowNotFoundException();
					}
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(SpellLineEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `spellline` VALUES ('" + m_state.EscapeString(obj.KeyName.ToString()) + "','" + m_state.EscapeString(obj.IsBaseLine.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.Spec.ToString()) + "');");
		}

		public virtual void Update(SpellLineEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `spellline` SET `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "', `IsBaseLine`='" + m_state.EscapeString(obj.IsBaseLine.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `Spec`='" + m_state.EscapeString(obj.Spec.ToString()) + "' WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void Delete(SpellLineEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `spellline` WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<SpellLineEntity> SelectAll()
		{
			SpellLineEntity entity;
			List<SpellLineEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `spellline`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<SpellLineEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new SpellLineEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `spellline`");
		}

		protected virtual void FillEntityWithRow(ref SpellLineEntity entity, MySqlDataReader reader)
		{
			entity.KeyName = reader.GetString(0);
			entity.IsBaseLine = reader.GetBoolean(1);
			entity.Name = reader.GetString(2);
			entity.Spec = reader.GetString(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpellLineEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spellline` ("
				+"`KeyName` varchar(255) character set utf8,"
				+"`IsBaseLine` bit,"
				+"`Name` varchar(255) character set utf8,"
				+"`Spec` varchar(255) character set utf8"
				+", primary key `KeyName` (`KeyName`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `spellline`");
			return null;
		}

		public SpellLineDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
