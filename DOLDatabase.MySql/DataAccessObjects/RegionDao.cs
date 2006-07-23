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
	public class RegionDao : IRegionDao
	{
		private readonly MySqlState m_state;

		public virtual RegionEntity Find(int key)
		{
			RegionEntity result = new RegionEntity();
			m_state.ExecuteQuery(
				"SELECT `RegionId`,`Description`,`Expansion`,`IsDivingEnabled`,`IsDungeon`,`IsInstance`,`Type` FROM `region` WHERE `RegionId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(RegionEntity obj)
		{
		}

		public virtual void Update(RegionEntity obj)
		{
		}

		public virtual void Delete(RegionEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref RegionEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Description = reader.GetString(1);
			entity.Expansion = reader.GetByte(2);
			entity.IsDivingEnabled = reader.GetString(3);
			entity.IsDungeon = reader.GetString(4);
			entity.IsInstance = reader.GetString(5);
			entity.Type = reader.GetByte(6);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(RegionEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `region` ("
				+"`RegionId` int,"
				+"`Description` varchar(510) character set unicode,"
				+"`Expansion` tinyint unsigned,"
				+"`IsDivingEnabled` char(1) character set ascii,"
				+"`IsDungeon` char(1) character set ascii,"
				+"`IsInstance` char(1) character set ascii,"
				+"`Type` tinyint unsigned"
				+", primary key `RegionId` (`RegionId`)"
			);
		}

		public RegionDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
