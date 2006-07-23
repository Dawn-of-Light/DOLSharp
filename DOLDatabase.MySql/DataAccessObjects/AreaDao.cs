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
	public class AreaDao : IAreaDao
	{
		private readonly MySqlState m_state;

		public virtual AreaEntity Find(int key)
		{
			AreaEntity result = new AreaEntity();
			m_state.ExecuteQuery(
				"SELECT `AreaId`,`AreaType`,`Description`,`Height`,`IsBroadcastEnabled`,`Radius`,`RegionId`,`Sound`,`Width`,`X`,`Y` FROM `area` WHERE `AreaId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(AreaEntity obj)
		{
		}

		public virtual void Update(AreaEntity obj)
		{
		}

		public virtual void Delete(AreaEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref AreaEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AreaType = reader.GetString(1);
			entity.Description = reader.GetString(2);
			entity.Height = reader.GetInt32(3);
			entity.IsBroadcastEnabled = reader.GetString(4);
			entity.Radius = reader.GetInt32(5);
			entity.RegionId = reader.GetInt32(6);
			entity.Sound = reader.GetByte(7);
			entity.Width = reader.GetInt32(8);
			entity.X = reader.GetInt32(9);
			entity.Y = reader.GetInt32(10);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(AreaEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `area` ("
				+"`AreaId` int,"
				+"`AreaType` varchar(510) character set unicode,"
				+"`Description` varchar(510) character set unicode,"
				+"`Height` int,"
				+"`IsBroadcastEnabled` char(1) character set ascii,"
				+"`Radius` int,"
				+"`RegionId` int,"
				+"`Sound` tinyint unsigned,"
				+"`Width` int,"
				+"`X` int,"
				+"`Y` int"
				+", primary key `AreaId` (`AreaId`)"
			);
		}

		public AreaDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
