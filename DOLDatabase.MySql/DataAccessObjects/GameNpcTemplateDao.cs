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
		private readonly MySqlState m_state;

		public virtual GameNpcTemplateEntity Find(int key)
		{
			GameNpcTemplateEntity result = new GameNpcTemplateEntity();
			m_state.ExecuteQuery(
				"SELECT `GameNPCTemplateId`,`BlockChance`,`EvadeChance`,`FactionId`,`Flags`,`GameNPCTemplateType`,`GuildName`,`InventoryId`,`LeftHandSwingChance`,`LootListId`,`MaxLevel`,`MaxSize`,`MaxSpeedBase`,`MeleeDamageType`,`MinLevel`,`MinSize`,`Model`,`Name`,`ParryChance`,`Realm`,`RespawnInterval` FROM `gamenpctemplate` WHERE `GameNPCTemplateId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(GameNpcTemplateEntity obj)
		{
		}

		public virtual void Update(GameNpcTemplateEntity obj)
		{
		}

		public virtual void Delete(GameNpcTemplateEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref GameNpcTemplateEntity entity, MySqlDataReader reader)
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
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gamenpctemplate` ("
				+"`GameNPCTemplateId` int,"
				+"`BlockChance` tinyint unsigned,"
				+"`EvadeChance` tinyint unsigned,"
				+"`FactionId` int,"
				+"`Flags` tinyint unsigned,"
				+"`GameNPCTemplateType` varchar(510) character set unicode,"
				+"`GuildName` varchar(510) character set unicode,"
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
				+"`Name` varchar(510) character set unicode,"
				+"`ParryChance` tinyint unsigned,"
				+"`Realm` tinyint unsigned,"
				+"`RespawnInterval` int"
				+", primary key `GameNPCTemplateId` (`GameNPCTemplateId`)"
			);
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
