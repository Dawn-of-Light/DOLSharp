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
	public class NpcEquipmentDao : INpcEquipmentDao
	{
		protected static readonly string c_rowFields = "`ItemId`,`Color`,`GlowEffect`,`InventoryId`,`Model`,`ModelExtension`,`NPCEquipmentType`,`SlotPosition`";
		private readonly MySqlState m_state;

		public virtual NpcEquipmentEntity Find(int key)
		{
			NpcEquipmentEntity result = new NpcEquipmentEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `npcequipment` WHERE `ItemId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(NpcEquipmentEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `npcequipment` VALUES (`" + obj.Id.ToString() + "`,`" + obj.GlowEffect.ToString() + "`,`" + obj.Inventory.ToString() + "`,`" + obj.Model.ToString() + "`,`" + obj.ModelExtension.ToString() + "`,`" + obj.NPCEquipmentType.ToString() + "`,`" + obj.or1.ToString() + "`,`" + obj.SlotPosition.ToString() + "`);");
		}

		public virtual void Update(NpcEquipmentEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `npcequipment` SET `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `GlowEffect`='" + m_state.EscapeString(obj.GlowEffect.ToString()) + "', `InventoryId`='" + m_state.EscapeString(obj.Inventory.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `ModelExtension`='" + m_state.EscapeString(obj.ModelExtension.ToString()) + "', `NPCEquipmentType`='" + m_state.EscapeString(obj.NPCEquipmentType.ToString()) + "', `Color`='" + m_state.EscapeString(obj.or1.ToString()) + "', `SlotPosition`='" + m_state.EscapeString(obj.SlotPosition.ToString()) + "' WHERE `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(NpcEquipmentEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `npcequipment` WHERE `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<NpcEquipmentEntity> SelectAll()
		{
			NpcEquipmentEntity entity;
			List<NpcEquipmentEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `npcequipment`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<NpcEquipmentEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new NpcEquipmentEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `npcequipment`");

		}

		protected virtual void FillEntityWithRow(ref NpcEquipmentEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.GlowEffect = reader.GetInt32(1);
			entity.Inventory = reader.GetInt32(2);
			entity.Model = reader.GetInt32(3);
			entity.ModelExtension = reader.GetByte(4);
			entity.NPCEquipmentType = reader.GetString(5);
			entity.or1 = reader.GetInt32(6);
			entity.SlotPosition = reader.GetInt32(7);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(NpcEquipmentEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `npcequipment` ("
				+"`ItemId` int,"
				+"`GlowEffect` int,"
				+"`InventoryId` int,"
				+"`Model` int,"
				+"`ModelExtension` tinyint unsigned,"
				+"`NPCEquipmentType` varchar(510) character set unicode,"
				+"`Color` int,"
				+"`SlotPosition` int"
				+", primary key `ItemId` (`ItemId`)"
			);
		}

		public NpcEquipmentDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
