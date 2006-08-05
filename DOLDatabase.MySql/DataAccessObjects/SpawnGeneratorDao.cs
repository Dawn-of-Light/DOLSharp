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
	public class SpawnGeneratorDao : ISpawnGeneratorDao
	{
		protected static readonly string c_rowFields = "`SpawnGeneratorBaseId`,`Height`,`Radius`,`RegionId`,`SpawnGeneratorBaseType`,`Width`,`X`,`Y`";
		private readonly MySqlState m_state;

		public virtual SpawnGeneratorEntity Find(int key)
		{
			SpawnGeneratorEntity result = new SpawnGeneratorEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `spawngenerator` WHERE `SpawnGeneratorBaseId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(SpawnGeneratorEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `spawngenerator` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Height.ToString() + "`,`" + obj.Radius.ToString() + "`,`" + obj.RegionId.ToString() + "`,`" + obj.SpawnGeneratorBaseType.ToString() + "`,`" + obj.Width.ToString() + "`,`" + obj.X.ToString() + "`,`" + obj.Y.ToString() + "`);");
		}

		public virtual void Update(SpawnGeneratorEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `spawngenerator` SET `SpawnGeneratorBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Height`='" + m_state.EscapeString(obj.Height.ToString()) + "', `Radius`='" + m_state.EscapeString(obj.Radius.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.RegionId.ToString()) + "', `SpawnGeneratorBaseType`='" + m_state.EscapeString(obj.SpawnGeneratorBaseType.ToString()) + "', `Width`='" + m_state.EscapeString(obj.Width.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "' WHERE `SpawnGeneratorBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(SpawnGeneratorEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `spawngenerator` WHERE `SpawnGeneratorBaseId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref SpawnGeneratorEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Height = reader.GetInt32(1);
			entity.Radius = reader.GetInt32(2);
			entity.RegionId = reader.GetInt32(3);
			entity.SpawnGeneratorBaseType = reader.GetString(4);
			entity.Width = reader.GetInt32(5);
			entity.X = reader.GetInt32(6);
			entity.Y = reader.GetInt32(7);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpawnGeneratorEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spawngenerator` ("
				+"`SpawnGeneratorBaseId` int,"
				+"`Height` int,"
				+"`Radius` int,"
				+"`RegionId` int,"
				+"`SpawnGeneratorBaseType` varchar(510) character set unicode,"
				+"`Width` int,"
				+"`X` int,"
				+"`Y` int"
				+", primary key `SpawnGeneratorBaseId` (`SpawnGeneratorBaseId`)"
			);
		}

		public SpawnGeneratorDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
