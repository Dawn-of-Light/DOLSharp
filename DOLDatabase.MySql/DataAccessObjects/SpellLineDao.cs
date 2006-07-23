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
		private readonly MySqlState m_state;

		public virtual SpellLineEntity Find(string key)
		{
			SpellLineEntity result = new SpellLineEntity();
			m_state.ExecuteQuery(
				"SELECT `KeyName`,`IsBaseLine`,`Name`,`Spec` FROM `spellline` WHERE `KeyName`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(SpellLineEntity obj)
		{
		}

		public virtual void Update(SpellLineEntity obj)
		{
		}

		public virtual void Delete(SpellLineEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref SpellLineEntity entity, MySqlDataReader reader)
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
