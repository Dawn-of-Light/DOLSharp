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
	public class RawMaterialDao : IRawMaterialDao
	{
		protected static readonly string c_rowFields = "`CountNeeded`,`CraftItemDataId`,`MaterialTemplate`";
		protected readonly MySqlState m_state;

		public virtual RawMaterialEntity Find(byte countNeeded, int craftItemData, string materialTemplate)
		{
			RawMaterialEntity result = new RawMaterialEntity();
			string command = "SELECT " + c_rowFields + " FROM `rawmaterials` WHERE `CountNeeded`='" + m_state.EscapeString(countNeeded.ToString()) + "', `CraftItemDataId`='" + m_state.EscapeString(craftItemData.ToString()) + "', `MaterialTemplate`='" + m_state.EscapeString(materialTemplate.ToString()) + "'";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						throw new RowNotFoundException();
					}
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(RawMaterialEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `rawmaterials` VALUES ('" + m_state.EscapeString(obj.CountNeeded.ToString()) + "','" + m_state.EscapeString(obj.CraftItemData.ToString()) + "','" + m_state.EscapeString(obj.MaterialTemplate.ToString()) + "');");
		}

		public virtual void Update(RawMaterialEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `rawmaterials` SET `CountNeeded`='" + m_state.EscapeString(obj.CountNeeded.ToString()) + "', `CraftItemDataId`='" + m_state.EscapeString(obj.CraftItemData.ToString()) + "', `MaterialTemplate`='" + m_state.EscapeString(obj.MaterialTemplate.ToString()) + "' WHERE `CountNeeded`='" + m_state.EscapeString(obj.CountNeeded.ToString()) + "'AND `CraftItemDataId`='" + m_state.EscapeString(obj.CraftItemData.ToString()) + "'AND `MaterialTemplate`='" + m_state.EscapeString(obj.MaterialTemplate.ToString()) + "'");
		}

		public virtual void Delete(RawMaterialEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `rawmaterials` WHERE `CountNeeded`='" + m_state.EscapeString(obj.CountNeeded.ToString()) + "'AND `CraftItemDataId`='" + m_state.EscapeString(obj.CraftItemData.ToString()) + "'AND `MaterialTemplate`='" + m_state.EscapeString(obj.MaterialTemplate.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<RawMaterialEntity> SelectAll()
		{
			RawMaterialEntity entity;
			List<RawMaterialEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `rawmaterials`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<RawMaterialEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new RawMaterialEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `rawmaterials`");
		}

		protected virtual void FillEntityWithRow(ref RawMaterialEntity entity, MySqlDataReader reader)
		{
			entity.CountNeeded = reader.GetByte(0);
			entity.CraftItemData = reader.GetInt32(1);
			entity.MaterialTemplate = reader.GetString(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(RawMaterialEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `rawmaterials` ("
				+"`CountNeeded` tinyint unsigned,"
				+"`CraftItemDataId` int,"
				+"`MaterialTemplate` varchar(255) character set utf8"
				+", primary key `CountNeededCraftItemDataIdMaterialTemplate` (`CountNeeded`,`CraftItemDataId`,`MaterialTemplate`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `rawmaterials`");
			return null;
		}

		public RawMaterialDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
