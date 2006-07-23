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
		private readonly MySqlState m_state;

		public virtual LineXSpellEntity Find(int key)
		{
			LineXSpellEntity result = new LineXSpellEntity();
			m_state.ExecuteQuery(
				"SELECT `LineXSpellId`,`Level`,`LineName`,`SpellId` FROM `linexspell` WHERE `LineXSpellId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(LineXSpellEntity obj)
		{
		}

		public virtual void Update(LineXSpellEntity obj)
		{
		}

		public virtual void Delete(LineXSpellEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref LineXSpellEntity entity, MySqlDataReader reader)
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
