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
		private readonly MySqlState m_state;

		public virtual SpellLineEntity Find(string key)
		{
			SpellLineEntity result = new SpellLineEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `spellline` WHERE `KeyName`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(SpellLineEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `spellline` VALUES (`" + obj.KeyName.ToString() + "`,`" + obj.IsBaseLine.ToString() + "`,`" + obj.Name.ToString() + "`,`" + obj.Spec.ToString() + "`);");
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

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `spellline`");

		}

		protected virtual void FillEntityWithRow(ref SpellLineEntity entity, MySqlDataReader reader)
		{
			entity.KeyName = reader.GetString(0);
			entity.IsBaseLine = reader.GetString(1);
			entity.Name = reader.GetString(2);
			entity.Spec = reader.GetString(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpellLineEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spellline` ("
				+"`KeyName` varchar(510) character set unicode,"
				+"`IsBaseLine` char(1) character set ascii,"
				+"`Name` varchar(510) character set unicode,"
				+"`Spec` varchar(510) character set unicode"
				+", primary key `KeyName` (`KeyName`)"
			);
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
