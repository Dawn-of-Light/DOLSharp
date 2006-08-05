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
	public class PersistantGameObjectDao : IPersistantGameObjectDao
	{
		protected static readonly string c_rowFields = "`PersistantGameObjectId`,`BlockChance`,`DoorId`,`EvadeChance`,`FactionId`,`Flags`,`GuildName`,`Heading`,`InventoryId`,`LeftHandSwingChance`,`Level`,`LootListId`,`MaxSpeedBase`,`MeleeDamageType`,`MerchantWindowId`,`Model`,`Name`,`ParryChance`,`PersistantGameObjectType`,`Realm`,`RegionId`,`RespawnInterval`,`Size`,`ToolType`,`X`,`Y`,`Z`";
		private readonly MySqlState m_state;

		public virtual PersistantGameObjectEntity Find(int key)
		{
			PersistantGameObjectEntity result = new PersistantGameObjectEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `persistantgameobject` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(PersistantGameObjectEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `persistantgameobject` VALUES (`" + obj.Id.ToString() + "`,`" + obj.BlockChance.ToString() + "`,`" + obj.DoorId.ToString() + "`,`" + obj.EvadeChance.ToString() + "`,`" + obj.FactionId.ToString() + "`,`" + obj.Flags.ToString() + "`,`" + obj.GuildName.ToString() + "`,`" + obj.Heading.ToString() + "`,`" + obj.InventoryId.ToString() + "`,`" + obj.LeftHandSwingChance.ToString() + "`,`" + obj.Level.ToString() + "`,`" + obj.LootListId.ToString() + "`,`" + obj.MaxSpeedBase.ToString() + "`,`" + obj.MeleeDamageType.ToString() + "`,`" + obj.MerchantWindowId.ToString() + "`,`" + obj.Model.ToString() + "`,`" + obj.Name.ToString() + "`,`" + obj.ParryChance.ToString() + "`,`" + obj.PersistantGameObjectType.ToString() + "`,`" + obj.Realm.ToString() + "`,`" + obj.RegionId.ToString() + "`,`" + obj.RespawnInterval.ToString() + "`,`" + obj.Size.ToString() + "`,`" + obj.ToolType.ToString() + "`,`" + obj.X.ToString() + "`,`" + obj.Y.ToString() + "`,`" + obj.Z.ToString() + "`);");
		}

		public virtual void Update(PersistantGameObjectEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `persistantgameobject` SET `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `BlockChance`='" + m_state.EscapeString(obj.BlockChance.ToString()) + "', `DoorId`='" + m_state.EscapeString(obj.DoorId.ToString()) + "', `EvadeChance`='" + m_state.EscapeString(obj.EvadeChance.ToString()) + "', `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "', `Flags`='" + m_state.EscapeString(obj.Flags.ToString()) + "', `GuildName`='" + m_state.EscapeString(obj.GuildName.ToString()) + "', `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `InventoryId`='" + m_state.EscapeString(obj.InventoryId.ToString()) + "', `LeftHandSwingChance`='" + m_state.EscapeString(obj.LeftHandSwingChance.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `LootListId`='" + m_state.EscapeString(obj.LootListId.ToString()) + "', `MaxSpeedBase`='" + m_state.EscapeString(obj.MaxSpeedBase.ToString()) + "', `MeleeDamageType`='" + m_state.EscapeString(obj.MeleeDamageType.ToString()) + "', `MerchantWindowId`='" + m_state.EscapeString(obj.MerchantWindowId.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `ParryChance`='" + m_state.EscapeString(obj.ParryChance.ToString()) + "', `PersistantGameObjectType`='" + m_state.EscapeString(obj.PersistantGameObjectType.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.RegionId.ToString()) + "', `RespawnInterval`='" + m_state.EscapeString(obj.RespawnInterval.ToString()) + "', `Size`='" + m_state.EscapeString(obj.Size.ToString()) + "', `ToolType`='" + m_state.EscapeString(obj.ToolType.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(PersistantGameObjectEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `persistantgameobject` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref PersistantGameObjectEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.BlockChance = reader.GetByte(1);
			entity.DoorId = reader.GetInt32(2);
			entity.EvadeChance = reader.GetByte(3);
			entity.FactionId = reader.GetInt32(4);
			entity.Flags = reader.GetByte(5);
			entity.GuildName = reader.GetString(6);
			entity.Heading = reader.GetInt32(7);
			entity.InventoryId = reader.GetInt32(8);
			entity.LeftHandSwingChance = reader.GetByte(9);
			entity.Level = reader.GetByte(10);
			entity.LootListId = reader.GetInt32(11);
			entity.MaxSpeedBase = reader.GetInt32(12);
			entity.MeleeDamageType = reader.GetByte(13);
			entity.MerchantWindowId = reader.GetInt32(14);
			entity.Model = reader.GetInt32(15);
			entity.Name = reader.GetString(16);
			entity.ParryChance = reader.GetByte(17);
			entity.PersistantGameObjectType = reader.GetString(18);
			entity.Realm = reader.GetByte(19);
			entity.RegionId = reader.GetInt32(20);
			entity.RespawnInterval = reader.GetInt32(21);
			entity.Size = reader.GetByte(22);
			entity.ToolType = reader.GetByte(23);
			entity.X = reader.GetInt32(24);
			entity.Y = reader.GetInt32(25);
			entity.Z = reader.GetInt32(26);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(PersistantGameObjectEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `persistantgameobject` ("
				+"`PersistantGameObjectId` int,"
				+"`BlockChance` tinyint unsigned,"
				+"`DoorId` int,"
				+"`EvadeChance` tinyint unsigned,"
				+"`FactionId` int,"
				+"`Flags` tinyint unsigned,"
				+"`GuildName` varchar(510) character set unicode,"
				+"`Heading` int,"
				+"`InventoryId` int,"
				+"`LeftHandSwingChance` tinyint unsigned,"
				+"`Level` tinyint unsigned,"
				+"`LootListId` int,"
				+"`MaxSpeedBase` int,"
				+"`MeleeDamageType` tinyint unsigned,"
				+"`MerchantWindowId` int,"
				+"`Model` int,"
				+"`Name` varchar(510) character set unicode,"
				+"`ParryChance` tinyint unsigned,"
				+"`PersistantGameObjectType` varchar(510) character set unicode,"
				+"`Realm` tinyint unsigned,"
				+"`RegionId` int,"
				+"`RespawnInterval` int,"
				+"`Size` tinyint unsigned,"
				+"`ToolType` tinyint unsigned,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `PersistantGameObjectId` (`PersistantGameObjectId`)"
			);
		}

		public PersistantGameObjectDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
