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
		private readonly MySqlState m_state;

		public virtual NpcEquipmentEntity Find(int key)
		{
			NpcEquipmentEntity result = new NpcEquipmentEntity();
			m_state.ExecuteQuery(
				"SELECT `ItemId`,`Color`,`GlowEffect`,`InventoryId`,`Model`,`ModelExtension`,`NPCEquipmentType`,`SlotPosition` FROM `npcequipment` WHERE `ItemId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(NpcEquipmentEntity obj)
		{
		}

		public virtual void Update(NpcEquipmentEntity obj)
		{
		}

		public virtual void Delete(NpcEquipmentEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref NpcEquipmentEntity entity, MySqlDataReader reader)
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
