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
	public class SinglePermissionDao : ISinglePermissionDao
	{
		private readonly MySqlState m_state;

		public virtual SinglePermissionEntity Find(int key)
		{
			SinglePermissionEntity result = new SinglePermissionEntity();
			m_state.ExecuteQuery(
				"SELECT `SinglePermissionId`,`Command`,`PlayerId` FROM `singlepermission` WHERE `SinglePermissionId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(SinglePermissionEntity obj)
		{
		}

		public virtual void Update(SinglePermissionEntity obj)
		{
		}

		public virtual void Delete(SinglePermissionEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref SinglePermissionEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Command = reader.GetString(1);
			entity.PlayerId = reader.GetString(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SinglePermissionEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `singlepermission` ("
				+"`SinglePermissionId` int,"
				+"`Command` varchar(510) character set unicode,"
				+"`PlayerId` varchar(510) character set unicode"
				+", primary key `SinglePermissionId` (`SinglePermissionId`)"
			);
		}

		public SinglePermissionDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
