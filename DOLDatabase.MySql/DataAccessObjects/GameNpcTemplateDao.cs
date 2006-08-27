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
	public class GameNpcTemplateDao : IGameNpcTemplateDao
	{
		protected static readonly string c_rowFields = "`GameNPCTemplateId`,`BlockChance`,`EvadeChance`,`FactionId`,`Flags`,`GameNPCTemplateType`,`GuildName`,`InventoryId`,`LeftHandSwingChance`,`LootListId`,`MaxLevel`,`MaxSize`,`MaxSpeedBase`,`MeleeDamageType`,`MinLevel`,`MinSize`,`Model`,`Name`,`ParryChance`,`Realm`,`RespawnInterval`";
		protected readonly MySqlState m_state;

		public virtual GameNpcTemplateEntity Find(int id)
		{
			GameNpcTemplateEntity result = new GameNpcTemplateEntity();
			string command = "SELECT " + c_rowFields + " FROM `gamenpctemplate` WHERE `GameNPCTemplateId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(GameNpcTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `gamenpctemplate` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.BlockChance.ToString()) + "','" + m_state.EscapeString(obj.EvadeChance.ToString()) + "','" + m_state.EscapeString(obj.FactionId.ToString()) + "','" + m_state.EscapeString(obj.Flags.ToString()) + "','" + m_state.EscapeString(obj.GameNPCTemplateType.ToString()) + "','" + m_state.EscapeString(obj.GuildName.ToString()) + "','" + m_state.EscapeString(obj.InventoryId.ToString()) + "','" + m_state.EscapeString(obj.LeftHandSwingChance.ToString()) + "','" + m_state.EscapeString(obj.LootListId.ToString()) + "','" + m_state.EscapeString(obj.MaxLevel.ToString()) + "','" + m_state.EscapeString(obj.MaxSize.ToString()) + "','" + m_state.EscapeString(obj.MaxSpeedBase.ToString()) + "','" + m_state.EscapeString(obj.MeleeDamageType.ToString()) + "','" + m_state.EscapeString(obj.MinLevel.ToString()) + "','" + m_state.EscapeString(obj.MinSize.ToString()) + "','" + m_state.EscapeString(obj.Model.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.ParryChance.ToString()) + "','" + m_state.EscapeString(obj.Realm.ToString()) + "','" + m_state.EscapeString(obj.RespawnInterval.ToString()) + "');");
		}

		public virtual void Update(GameNpcTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `gamenpctemplate` SET `GameNPCTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `BlockChance`='" + m_state.EscapeString(obj.BlockChance.ToString()) + "', `EvadeChance`='" + m_state.EscapeString(obj.EvadeChance.ToString()) + "', `FactionId`='" + m_state.EscapeString(obj.FactionId.ToString()) + "', `Flags`='" + m_state.EscapeString(obj.Flags.ToString()) + "', `GameNPCTemplateType`='" + m_state.EscapeString(obj.GameNPCTemplateType.ToString()) + "', `GuildName`='" + m_state.EscapeString(obj.GuildName.ToString()) + "', `InventoryId`='" + m_state.EscapeString(obj.InventoryId.ToString()) + "', `LeftHandSwingChance`='" + m_state.EscapeString(obj.LeftHandSwingChance.ToString()) + "', `LootListId`='" + m_state.EscapeString(obj.LootListId.ToString()) + "', `MaxLevel`='" + m_state.EscapeString(obj.MaxLevel.ToString()) + "', `MaxSize`='" + m_state.EscapeString(obj.MaxSize.ToString()) + "', `MaxSpeedBase`='" + m_state.EscapeString(obj.MaxSpeedBase.ToString()) + "', `MeleeDamageType`='" + m_state.EscapeString(obj.MeleeDamageType.ToString()) + "', `MinLevel`='" + m_state.EscapeString(obj.MinLevel.ToString()) + "', `MinSize`='" + m_state.EscapeString(obj.MinSize.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `ParryChance`='" + m_state.EscapeString(obj.ParryChance.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `RespawnInterval`='" + m_state.EscapeString(obj.RespawnInterval.ToString()) + "' WHERE `GameNPCTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GameNpcTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `gamenpctemplate` WHERE `GameNPCTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<GameNpcTemplateEntity> SelectAll()
		{
			GameNpcTemplateEntity entity;
			List<GameNpcTemplateEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `gamenpctemplate`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GameNpcTemplateEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new GameNpcTemplateEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `gamenpctemplate`");
		}

		protected virtual void FillEntityWithRow(ref GameNpcTemplateEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.BlockChance = reader.GetByte(1);
			entity.EvadeChance = reader.GetByte(2);
			entity.FactionId = reader.GetInt32(3);
			entity.Flags = reader.GetByte(4);
			entity.GameNPCTemplateType = reader.GetString(5);
			entity.GuildName = reader.GetString(6);
			entity.InventoryId = reader.GetInt32(7);
			entity.LeftHandSwingChance = reader.GetByte(8);
			entity.LootListId = reader.GetInt32(9);
			entity.MaxLevel = reader.GetByte(10);
			entity.MaxSize = reader.GetByte(11);
			entity.MaxSpeedBase = reader.GetInt32(12);
			entity.MeleeDamageType = reader.GetByte(13);
			entity.MinLevel = reader.GetByte(14);
			entity.MinSize = reader.GetByte(15);
			entity.Model = reader.GetInt32(16);
			entity.Name = reader.GetString(17);
			entity.ParryChance = reader.GetByte(18);
			entity.Realm = reader.GetByte(19);
			entity.RespawnInterval = reader.GetInt32(20);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GameNpcTemplateEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gamenpctemplate` ("
				+"`GameNPCTemplateId` int,"
				+"`BlockChance` tinyint unsigned,"
				+"`EvadeChance` tinyint unsigned,"
				+"`FactionId` int,"
				+"`Flags` tinyint unsigned,"
				+"`GameNPCTemplateType` varchar(255) character set utf8,"
				+"`GuildName` varchar(255) character set utf8,"
				+"`InventoryId` int,"
				+"`LeftHandSwingChance` tinyint unsigned,"
				+"`LootListId` int,"
				+"`MaxLevel` tinyint unsigned,"
				+"`MaxSize` tinyint unsigned,"
				+"`MaxSpeedBase` int,"
				+"`MeleeDamageType` tinyint unsigned,"
				+"`MinLevel` tinyint unsigned,"
				+"`MinSize` tinyint unsigned,"
				+"`Model` int,"
				+"`Name` varchar(255) character set utf8,"
				+"`ParryChance` tinyint unsigned,"
				+"`Realm` tinyint unsigned,"
				+"`RespawnInterval` int"
				+", primary key `GameNPCTemplateId` (`GameNPCTemplateId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `gamenpctemplate`");
			return null;
		}

		public GameNpcTemplateDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
