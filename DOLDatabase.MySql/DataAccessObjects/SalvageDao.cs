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
	public class SalvageDao : ISalvageDao
	{
		private readonly MySqlState m_state;

		public virtual SalvageEntity Find(int key)
		{
			SalvageEntity result = new SalvageEntity();
			m_state.ExecuteQuery(
				"SELECT `SalvageId`,`MaterialItemtemplate`,`ObjectType`,`SalvageLevel` FROM `salvage` WHERE `SalvageId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(SalvageEntity obj)
		{
		}

		public virtual void Update(SalvageEntity obj)
		{
		}

		public virtual void Delete(SalvageEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref SalvageEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.MaterialItemtemplate = reader.GetString(1);
			entity.ObjectType = reader.GetInt32(2);
			entity.SalvageLevel = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SalvageEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `salvage` ("
				+"`SalvageId` int,"
				+"`MaterialItemtemplate` varchar(510) character set unicode,"
				+"`ObjectType` int,"
				+"`SalvageLevel` int"
				+", primary key `SalvageId` (`SalvageId`)"
			);
		}

		public SalvageDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
