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
		private readonly MySqlState m_state;

		public virtual JumpPointEntity Find(int key)
		{
			JumpPointEntity result = new JumpPointEntity();
			m_state.ExecuteQuery(
				"SELECT `JumpPointId`,`AllowedRealm`,`Heading`,`JumpPointType`,`Region`,`X`,`Y`,`Z` FROM `jumppoint` WHERE `JumpPointId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(JumpPointEntity obj)
		{
		}

		public virtual void Update(JumpPointEntity obj)
		{
		}

		public virtual void Delete(JumpPointEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref JumpPointEntity entity, MySqlDataReader reader)
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
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `jumppoint` ("
				+"`JumpPointId` int,"
				+"`AllowedRealm` tinyint unsigned,"
				+"`Heading` int,"
				+"`JumpPointType` varchar(510) character set unicode,"
				+"`Region` int,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `JumpPointId` (`JumpPointId`)"
			);
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
