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
	public class ItemDao : IItemDao
	{
		private readonly MySqlState m_state;

		public virtual ItemEntity Find(int key)
		{
			ItemEntity result = new ItemEntity();
			m_state.ExecuteQuery(
				"SELECT `ItemId`,`ArmorFactor`,`ArmorLevel`,`Bonus`,`BonusType`,`Charge`,`ChargeEffectType`,`ChargeSpellId`,`Color`,`Condition`,`Count`,`CrafterName`,`Damage`,`DamagePerSecond`,`DamageType`,`Durability`,`GenericItemType`,`GlowEffect`,`HandNeeded`,`Heading`,`InventoryId`,`IsDropable`,`IsSaleable`,`IsTradable`,`Level`,`MaterialLevel`,`MaxCharge`,`MaxCount`,`Model`,`ModelExtension`,`Name`,`Owner`,`Precision`,`ProcEffectType`,`ProcSpellId`,`Quality`,`QuestName`,`Range`,`Realm`,`Region`,`RespecType`,`Size`,`SlotPosition`,`Speed`,`SpellId`,`TripPathId`,`Type`,`Value`,`WeaponRange`,`Weight`,`X`,`Y`,`Z` FROM `item` WHERE `ItemId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(ItemEntity obj)
		{
		}

		public virtual void Update(ItemEntity obj)
		{
		}

		public virtual void Delete(ItemEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref ItemEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.ArmorFactor = reader.GetByte(1);
			entity.ArmorLevel = reader.GetByte(2);
			entity.Bonus = reader.GetInt32(3);
			entity.BonusType = reader.GetByte(4);
			entity.Charge = reader.GetByte(5);
			entity.ChargeEffectType = reader.GetByte(6);
			entity.ChargeSpellId = reader.GetInt32(7);
			entity.Condition = reader.GetDouble(8);
			entity.Count = reader.GetInt32(9);
			entity.CrafterName = reader.GetString(10);
			entity.Damage = reader.GetByte(11);
			entity.DamagePerSecond = reader.GetByte(12);
			entity.DamageType = reader.GetByte(13);
			entity.Durability = reader.GetByte(14);
			entity.GenericItemType = reader.GetString(15);
			entity.GlowEffect = reader.GetInt32(16);
			entity.HandNeeded = reader.GetByte(17);
			entity.Heading = reader.GetInt32(18);
			entity.IsDropable = reader.GetString(19);
			entity.IsSaleable = reader.GetString(20);
			entity.IsTradable = reader.GetString(21);
			entity.Level = reader.GetByte(22);
			entity.MaterialLevel = reader.GetByte(23);
			entity.MaxCharge = reader.GetByte(24);
			entity.MaxCount = reader.GetInt32(25);
			entity.Model = reader.GetInt32(26);
			entity.ModelExtension = reader.GetByte(27);
			entity.Name = reader.GetString(28);
			entity.or1 = reader.GetInt32(29);
			entity.Precision = reader.GetByte(30);
			entity.ProcEffectType = reader.GetByte(31);
			entity.ProcSpellId = reader.GetInt32(32);
			entity.Quality = reader.GetInt32(33);
			entity.QuestName = reader.GetString(34);
			entity.Range = reader.GetByte(35);
			entity.Realm = reader.GetByte(36);
			entity.Region = reader.GetInt32(37);
			entity.RespecType = reader.GetByte(38);
			entity.Size = reader.GetByte(39);
			entity.SlotPosition = reader.GetInt32(40);
			entity.Speed = reader.GetInt32(41);
			entity.SpellId = reader.GetInt32(42);
			entity.TripPathId = reader.GetInt32(43);
			entity.Type = reader.GetByte(44);
			entity.Value = reader.GetInt64(45);
			entity.WeaponRange = reader.GetInt32(46);
			entity.Weight = reader.GetInt32(47);
			entity.X = reader.GetInt32(48);
			entity.Y = reader.GetInt32(49);
			entity.Z = reader.GetInt32(50);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ItemEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `item` ("
				+"`ItemId` int,"
				+"`ArmorFactor` tinyint unsigned,"
				+"`ArmorLevel` tinyint unsigned,"
				+"`Bonus` int,"
				+"`BonusType` tinyint unsigned,"
				+"`Charge` tinyint unsigned,"
				+"`ChargeEffectType` tinyint unsigned,"
				+"`ChargeSpellId` int,"
				+"`Condition` double,"
				+"`Count` int,"
				+"`CrafterName` varchar(510) character set unicode,"
				+"`Damage` tinyint unsigned,"
				+"`DamagePerSecond` tinyint unsigned,"
				+"`DamageType` tinyint unsigned,"
				+"`Durability` tinyint unsigned,"
				+"`GenericItemType` varchar(510) character set unicode,"
				+"`GlowEffect` int,"
				+"`HandNeeded` tinyint unsigned,"
				+"`Heading` int,"
				+"`IsDropable` char(1) character set ascii,"
				+"`IsSaleable` char(1) character set ascii,"
				+"`IsTradable` char(1) character set ascii,"
				+"`Level` tinyint unsigned,"
				+"`MaterialLevel` tinyint unsigned,"
				+"`MaxCharge` tinyint unsigned,"
				+"`MaxCount` int,"
				+"`Model` int,"
				+"`ModelExtension` tinyint unsigned,"
				+"`Name` varchar(510) character set unicode,"
				+"`Color` int,"
				+"`Precision` tinyint unsigned,"
				+"`ProcEffectType` tinyint unsigned,"
				+"`ProcSpellId` int,"
				+"`Quality` int,"
				+"`QuestName` varchar(510) character set unicode,"
				+"`Range` tinyint unsigned,"
				+"`Realm` tinyint unsigned,"
				+"`Region` int,"
				+"`RespecType` tinyint unsigned,"
				+"`Size` tinyint unsigned,"
				+"`SlotPosition` int,"
				+"`Speed` int,"
				+"`SpellId` int,"
				+"`TripPathId` int,"
				+"`Type` tinyint unsigned,"
				+"`Value` bigint,"
				+"`WeaponRange` int,"
				+"`Weight` int,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `ItemId` (`ItemId`)"
			);
		}

		public ItemDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
