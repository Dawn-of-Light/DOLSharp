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
		protected static readonly string c_rowFields = "`ItemId`,`ArmorFactor`,`ArmorLevel`,`Bonus`,`BonusType`,`Charge`,`ChargeEffectType`,`ChargeSpellId`,`Color`,`Condition`,`Count`,`CrafterName`,`Damage`,`DamagePerSecond`,`DamageType`,`Durability`,`GenericItemType`,`GlowEffect`,`HandNeeded`,`Heading`,`InventoryId`,`IsDropable`,`IsSaleable`,`IsTradable`,`Level`,`MaterialLevel`,`MaxCharge`,`MaxCount`,`Model`,`ModelExtension`,`Name`,`Owner`,`Precision`,`ProcEffectType`,`ProcSpellId`,`Quality`,`QuestName`,`Range`,`Realm`,`Region`,`RespecType`,`Size`,`SlotPosition`,`Speed`,`SpellId`,`TripPathId`,`Type`,`Value`,`WeaponRange`,`Weight`,`X`,`Y`,`Z`";
		protected readonly MySqlState m_state;

		public virtual ItemEntity Find(int id)
		{
			ItemEntity result = new ItemEntity();
			string command = "SELECT " + c_rowFields + " FROM `item` WHERE `ItemId`='" + m_state.EscapeString(id.ToString()) + "'";

			m_state.ExecuteQuery(
				command,
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					if (!reader.Read())
					{
						result = null;
					}
					else
					{
						FillEntityWithRow(ref result, reader);
					}
				}
			);

			return result;
		}

		public virtual void Create(ref ItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `item` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.ArmorFactor.ToString()) + "','" + m_state.EscapeString(obj.ArmorLevel.ToString()) + "','" + m_state.EscapeString(obj.Bonus.ToString()) + "','" + m_state.EscapeString(obj.BonusType.ToString()) + "','" + m_state.EscapeString(obj.Charge.ToString()) + "','" + m_state.EscapeString(obj.ChargeEffectType.ToString()) + "','" + m_state.EscapeString(obj.ChargeSpellId.ToString()) + "','" + m_state.EscapeString(obj.Condition.ToString()) + "','" + m_state.EscapeString(obj.Count.ToString()) + "','" + m_state.EscapeString(obj.CrafterName.ToString()) + "','" + m_state.EscapeString(obj.Damage.ToString()) + "','" + m_state.EscapeString(obj.DamagePerSecond.ToString()) + "','" + m_state.EscapeString(obj.DamageType.ToString()) + "','" + m_state.EscapeString(obj.Durability.ToString()) + "','" + m_state.EscapeString(obj.GenericItemType.ToString()) + "','" + m_state.EscapeString(obj.GlowEffect.ToString()) + "','" + m_state.EscapeString(obj.HandNeeded.ToString()) + "','" + m_state.EscapeString(obj.Heading.ToString()) + "','" + m_state.EscapeString(obj.IsDropable.ToString()) + "','" + m_state.EscapeString(obj.IsSaleable.ToString()) + "','" + m_state.EscapeString(obj.IsTradable.ToString()) + "','" + m_state.EscapeString(obj.Level.ToString()) + "','" + m_state.EscapeString(obj.MaterialLevel.ToString()) + "','" + m_state.EscapeString(obj.MaxCharge.ToString()) + "','" + m_state.EscapeString(obj.MaxCount.ToString()) + "','" + m_state.EscapeString(obj.Model.ToString()) + "','" + m_state.EscapeString(obj.ModelExtension.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.or1.ToString()) + "','" + m_state.EscapeString(obj.Precision.ToString()) + "','" + m_state.EscapeString(obj.ProcEffectType.ToString()) + "','" + m_state.EscapeString(obj.ProcSpellId.ToString()) + "','" + m_state.EscapeString(obj.Quality.ToString()) + "','" + m_state.EscapeString(obj.QuestName.ToString()) + "','" + m_state.EscapeString(obj.Range.ToString()) + "','" + m_state.EscapeString(obj.Realm.ToString()) + "','" + m_state.EscapeString(obj.Region.ToString()) + "','" + m_state.EscapeString(obj.RespecType.ToString()) + "','" + m_state.EscapeString(obj.Size.ToString()) + "','" + m_state.EscapeString(obj.SlotPosition.ToString()) + "','" + m_state.EscapeString(obj.Speed.ToString()) + "','" + m_state.EscapeString(obj.SpellId.ToString()) + "','" + m_state.EscapeString(obj.TripPathId.ToString()) + "','" + m_state.EscapeString(obj.Type.ToString()) + "','" + m_state.EscapeString(obj.Value.ToString()) + "','" + m_state.EscapeString(obj.WeaponRange.ToString()) + "','" + m_state.EscapeString(obj.Weight.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "','" + m_state.EscapeString(obj.Z.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(ItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `item` SET `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `ArmorFactor`='" + m_state.EscapeString(obj.ArmorFactor.ToString()) + "', `ArmorLevel`='" + m_state.EscapeString(obj.ArmorLevel.ToString()) + "', `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "', `Charge`='" + m_state.EscapeString(obj.Charge.ToString()) + "', `ChargeEffectType`='" + m_state.EscapeString(obj.ChargeEffectType.ToString()) + "', `ChargeSpellId`='" + m_state.EscapeString(obj.ChargeSpellId.ToString()) + "', `Condition`='" + m_state.EscapeString(obj.Condition.ToString()) + "', `Count`='" + m_state.EscapeString(obj.Count.ToString()) + "', `CrafterName`='" + m_state.EscapeString(obj.CrafterName.ToString()) + "', `Damage`='" + m_state.EscapeString(obj.Damage.ToString()) + "', `DamagePerSecond`='" + m_state.EscapeString(obj.DamagePerSecond.ToString()) + "', `DamageType`='" + m_state.EscapeString(obj.DamageType.ToString()) + "', `Durability`='" + m_state.EscapeString(obj.Durability.ToString()) + "', `GenericItemType`='" + m_state.EscapeString(obj.GenericItemType.ToString()) + "', `GlowEffect`='" + m_state.EscapeString(obj.GlowEffect.ToString()) + "', `HandNeeded`='" + m_state.EscapeString(obj.HandNeeded.ToString()) + "', `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `IsDropable`='" + m_state.EscapeString(obj.IsDropable.ToString()) + "', `IsSaleable`='" + m_state.EscapeString(obj.IsSaleable.ToString()) + "', `IsTradable`='" + m_state.EscapeString(obj.IsTradable.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `MaterialLevel`='" + m_state.EscapeString(obj.MaterialLevel.ToString()) + "', `MaxCharge`='" + m_state.EscapeString(obj.MaxCharge.ToString()) + "', `MaxCount`='" + m_state.EscapeString(obj.MaxCount.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `ModelExtension`='" + m_state.EscapeString(obj.ModelExtension.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `Color`='" + m_state.EscapeString(obj.or1.ToString()) + "', `Precision`='" + m_state.EscapeString(obj.Precision.ToString()) + "', `ProcEffectType`='" + m_state.EscapeString(obj.ProcEffectType.ToString()) + "', `ProcSpellId`='" + m_state.EscapeString(obj.ProcSpellId.ToString()) + "', `Quality`='" + m_state.EscapeString(obj.Quality.ToString()) + "', `QuestName`='" + m_state.EscapeString(obj.QuestName.ToString()) + "', `Range`='" + m_state.EscapeString(obj.Range.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "', `RespecType`='" + m_state.EscapeString(obj.RespecType.ToString()) + "', `Size`='" + m_state.EscapeString(obj.Size.ToString()) + "', `SlotPosition`='" + m_state.EscapeString(obj.SlotPosition.ToString()) + "', `Speed`='" + m_state.EscapeString(obj.Speed.ToString()) + "', `SpellId`='" + m_state.EscapeString(obj.SpellId.ToString()) + "', `TripPathId`='" + m_state.EscapeString(obj.TripPathId.ToString()) + "', `Type`='" + m_state.EscapeString(obj.Type.ToString()) + "', `Value`='" + m_state.EscapeString(obj.Value.ToString()) + "', `WeaponRange`='" + m_state.EscapeString(obj.WeaponRange.ToString()) + "', `Weight`='" + m_state.EscapeString(obj.Weight.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(ItemEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `item` WHERE `ItemId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ItemEntity> SelectAll()
		{
			ItemEntity entity;
			List<ItemEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `item`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ItemEntity>();
					while (reader.Read())
					{
						entity = new ItemEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `item`");
		}

		protected virtual void FillEntityWithRow(ref ItemEntity entity, MySqlDataReader reader)
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
			entity.IsDropable = reader.GetBoolean(19);
			entity.IsSaleable = reader.GetBoolean(20);
			entity.IsTradable = reader.GetBoolean(21);
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
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `item` ("
				+"`ItemId` int NOT NULL auto_increment,"
				+"`ArmorFactor` tinyint unsigned NOT NULL,"
				+"`ArmorLevel` tinyint unsigned NOT NULL,"
				+"`Bonus` int NOT NULL,"
				+"`BonusType` tinyint unsigned NOT NULL,"
				+"`Charge` tinyint unsigned NOT NULL,"
				+"`ChargeEffectType` tinyint unsigned NOT NULL,"
				+"`ChargeSpellId` int NOT NULL,"
				+"`Condition` double NOT NULL,"
				+"`Count` int NOT NULL,"
				+"`CrafterName` char(255) character set latin1 NOT NULL,"
				+"`Damage` tinyint unsigned NOT NULL,"
				+"`DamagePerSecond` tinyint unsigned NOT NULL,"
				+"`DamageType` tinyint unsigned NOT NULL,"
				+"`Durability` tinyint unsigned NOT NULL,"
				+"`GenericItemType` char(255) character set latin1 NOT NULL,"
				+"`GlowEffect` int NOT NULL,"
				+"`HandNeeded` tinyint unsigned NOT NULL,"
				+"`Heading` int NOT NULL,"
				+"`IsDropable` bit NOT NULL,"
				+"`IsSaleable` bit NOT NULL,"
				+"`IsTradable` bit NOT NULL,"
				+"`Level` tinyint unsigned NOT NULL,"
				+"`MaterialLevel` tinyint unsigned NOT NULL,"
				+"`MaxCharge` tinyint unsigned NOT NULL,"
				+"`MaxCount` int NOT NULL,"
				+"`Model` int NOT NULL,"
				+"`ModelExtension` tinyint unsigned NOT NULL,"
				+"`Name` char(255) character set latin1 NOT NULL,"
				+"`Color` int NOT NULL,"
				+"`Precision` tinyint unsigned NOT NULL,"
				+"`ProcEffectType` tinyint unsigned NOT NULL,"
				+"`ProcSpellId` int NOT NULL,"
				+"`Quality` int NOT NULL,"
				+"`QuestName` char(255) character set latin1 NOT NULL,"
				+"`Range` tinyint unsigned NOT NULL,"
				+"`Realm` tinyint unsigned NOT NULL,"
				+"`Region` int NOT NULL,"
				+"`RespecType` tinyint unsigned NOT NULL,"
				+"`Size` tinyint unsigned NOT NULL,"
				+"`SlotPosition` int NOT NULL,"
				+"`Speed` int NOT NULL,"
				+"`SpellId` int NOT NULL,"
				+"`TripPathId` int NOT NULL,"
				+"`Type` tinyint unsigned NOT NULL,"
				+"`Value` bigint NOT NULL,"
				+"`WeaponRange` int NOT NULL,"
				+"`Weight` int NOT NULL,"
				+"`X` int NOT NULL,"
				+"`Y` int NOT NULL,"
				+"`Z` int NOT NULL"
				+", primary key `ItemId` (`ItemId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `item`");
			return null;
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
