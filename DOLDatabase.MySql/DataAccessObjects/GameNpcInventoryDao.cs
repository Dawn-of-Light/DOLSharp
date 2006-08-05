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
	public class GameNpcInventoryDao : IGameNpcInventoryDao
	{
		protected static readonly string c_rowFields = "`InventoryId`,`IsCloakHoodUp`";
		private readonly MySqlState m_state;

		public virtual GameNpcInventoryEntity Find(int key)
		{
			GameNpcInventoryEntity result = new GameNpcInventoryEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `gamenpcinventory` WHERE `InventoryId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `gamenpcinventory` VALUES (`" + obj.Id.ToString() + "`,`" + obj.IsCloakHoodUp.ToString() + "`);");
		}

		public virtual void Update(GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `gamenpcinventory` SET `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `IsCloakHoodUp`='" + m_state.EscapeString(obj.IsCloakHoodUp.ToString()) + "' WHERE `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GameNpcInventoryEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `gamenpcinventory` WHERE `InventoryId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref GameNpcInventoryEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.IsCloakHoodUp = reader.GetString(1);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GameNpcInventoryEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gamenpcinventory` ("
				+"`InventoryId` int,"
				+"`IsCloakHoodUp` char(1) character set ascii"
				+", primary key `InventoryId` (`InventoryId`)"
			);
		}

		public GameNpcInventoryDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
