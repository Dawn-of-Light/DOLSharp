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
	public class ItemTemplateDao : IItemTemplateDao
	{
		protected static readonly string c_rowFields = "`ItemTemplateId`,`ArmorFactor`,`ArmorLevel`,`Bonus`,`BonusType`,`Charge`,`ChargeEffectType`,`ChargeSpellId`,`Color`,`Condition`,`Damage`,`DamagePerSecond`,`DamageType`,`Durability`,`GlowEffect`,`HandNeeded`,`Heading`,`IsDropable`,`IsSaleable`,`IsTradable`,`ItemTemplateType`,`Level`,`MaterialLevel`,`MaxCharge`,`MaxCount`,`Model`,`ModelExtension`,`Name`,`PackSize`,`Precision`,`ProcEffectType`,`ProcSpellId`,`Quality`,`Range`,`Realm`,`Region`,`RespecType`,`Size`,`Speed`,`SpellId`,`TripPathId`,`Type`,`Value`,`WeaponRange`,`Weight`,`X`,`Y`,`Z`";
		private readonly MySqlState m_state;

		public virtual ItemTemplateEntity Find(string id)
		{
			ItemTemplateEntity result = new ItemTemplateEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `itemtemplate` WHERE `ItemTemplateId`='" + m_state.EscapeString(id.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(ItemTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `itemtemplate` VALUES (`" + obj.Id.ToString() + "`,`" + obj.ArmorFactor.ToString() + "`,`" + obj.ArmorLevel.ToString() + "`,`" + obj.Bonus.ToString() + "`,`" + obj.BonusType.ToString() + "`,`" + obj.Charge.ToString() + "`,`" + obj.ChargeEffectType.ToString() + "`,`" + obj.ChargeSpellId.ToString() + "`,`" + obj.Condition.ToString() + "`,`" + obj.Damage.ToString() + "`,`" + obj.DamagePerSecond.ToString() + "`,`" + obj.DamageType.ToString() + "`,`" + obj.Durability.ToString() + "`,`" + obj.GlowEffect.ToString() + "`,`" + obj.HandNeeded.ToString() + "`,`" + obj.Heading.ToString() + "`,`" + obj.IsDropable.ToString() + "`,`" + obj.IsSaleable.ToString() + "`,`" + obj.IsTradable.ToString() + "`,`" + obj.ItemTemplateType.ToString() + "`,`" + obj.Level.ToString() + "`,`" + obj.MaterialLevel.ToString() + "`,`" + obj.MaxCharge.ToString() + "`,`" + obj.MaxCount.ToString() + "`,`" + obj.Model.ToString() + "`,`" + obj.ModelExtension.ToString() + "`,`" + obj.Name.ToString() + "`,`" + obj.or1.ToString() + "`,`" + obj.PackSize.ToString() + "`,`" + obj.Precision.ToString() + "`,`" + obj.ProcEffectType.ToString() + "`,`" + obj.ProcSpellId.ToString() + "`,`" + obj.Quality.ToString() + "`,`" + obj.Range.ToString() + "`,`" + obj.Realm.ToString() + "`,`" + obj.Region.ToString() + "`,`" + obj.RespecType.ToString() + "`,`" + obj.Size.ToString() + "`,`" + obj.Speed.ToString() + "`,`" + obj.SpellId.ToString() + "`,`" + obj.TripPathId.ToString() + "`,`" + obj.Type.ToString() + "`,`" + obj.Value.ToString() + "`,`" + obj.WeaponRange.ToString() + "`,`" + obj.Weight.ToString() + "`,`" + obj.X.ToString() + "`,`" + obj.Y.ToString() + "`,`" + obj.Z.ToString() + "`);");
		}

		public virtual void Update(ItemTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `itemtemplate` SET `ItemTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `ArmorFactor`='" + m_state.EscapeString(obj.ArmorFactor.ToString()) + "', `ArmorLevel`='" + m_state.EscapeString(obj.ArmorLevel.ToString()) + "', `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "', `Charge`='" + m_state.EscapeString(obj.Charge.ToString()) + "', `ChargeEffectType`='" + m_state.EscapeString(obj.ChargeEffectType.ToString()) + "', `ChargeSpellId`='" + m_state.EscapeString(obj.ChargeSpellId.ToString()) + "', `Condition`='" + m_state.EscapeString(obj.Condition.ToString()) + "', `Damage`='" + m_state.EscapeString(obj.Damage.ToString()) + "', `DamagePerSecond`='" + m_state.EscapeString(obj.DamagePerSecond.ToString()) + "', `DamageType`='" + m_state.EscapeString(obj.DamageType.ToString()) + "', `Durability`='" + m_state.EscapeString(obj.Durability.ToString()) + "', `GlowEffect`='" + m_state.EscapeString(obj.GlowEffect.ToString()) + "', `HandNeeded`='" + m_state.EscapeString(obj.HandNeeded.ToString()) + "', `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `IsDropable`='" + m_state.EscapeString(obj.IsDropable.ToString()) + "', `IsSaleable`='" + m_state.EscapeString(obj.IsSaleable.ToString()) + "', `IsTradable`='" + m_state.EscapeString(obj.IsTradable.ToString()) + "', `ItemTemplateType`='" + m_state.EscapeString(obj.ItemTemplateType.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `MaterialLevel`='" + m_state.EscapeString(obj.MaterialLevel.ToString()) + "', `MaxCharge`='" + m_state.EscapeString(obj.MaxCharge.ToString()) + "', `MaxCount`='" + m_state.EscapeString(obj.MaxCount.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `ModelExtension`='" + m_state.EscapeString(obj.ModelExtension.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `Color`='" + m_state.EscapeString(obj.or1.ToString()) + "', `PackSize`='" + m_state.EscapeString(obj.PackSize.ToString()) + "', `Precision`='" + m_state.EscapeString(obj.Precision.ToString()) + "', `ProcEffectType`='" + m_state.EscapeString(obj.ProcEffectType.ToString()) + "', `ProcSpellId`='" + m_state.EscapeString(obj.ProcSpellId.ToString()) + "', `Quality`='" + m_state.EscapeString(obj.Quality.ToString()) + "', `Range`='" + m_state.EscapeString(obj.Range.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "', `RespecType`='" + m_state.EscapeString(obj.RespecType.ToString()) + "', `Size`='" + m_state.EscapeString(obj.Size.ToString()) + "', `Speed`='" + m_state.EscapeString(obj.Speed.ToString()) + "', `SpellId`='" + m_state.EscapeString(obj.SpellId.ToString()) + "', `TripPathId`='" + m_state.EscapeString(obj.TripPathId.ToString()) + "', `Type`='" + m_state.EscapeString(obj.Type.ToString()) + "', `Value`='" + m_state.EscapeString(obj.Value.ToString()) + "', `WeaponRange`='" + m_state.EscapeString(obj.WeaponRange.ToString()) + "', `Weight`='" + m_state.EscapeString(obj.Weight.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `ItemTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(ItemTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `itemtemplate` WHERE `ItemTemplateId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ItemTemplateEntity> SelectAll()
		{
			ItemTemplateEntity entity;
			List<ItemTemplateEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `itemtemplate`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ItemTemplateEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new ItemTemplateEntity();
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
			"SELECT COUNT(*) FROM `itemtemplate`");

		}

		protected virtual void FillEntityWithRow(ref ItemTemplateEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetString(0);
			entity.ArmorFactor = reader.GetByte(1);
			entity.ArmorLevel = reader.GetByte(2);
			entity.Bonus = reader.GetInt32(3);
			entity.BonusType = reader.GetByte(4);
			entity.Charge = reader.GetByte(5);
			entity.ChargeEffectType = reader.GetByte(6);
			entity.ChargeSpellId = reader.GetInt32(7);
			entity.Condition = reader.GetDouble(8);
			entity.Damage = reader.GetByte(9);
			entity.DamagePerSecond = reader.GetByte(10);
			entity.DamageType = reader.GetByte(11);
			entity.Durability = reader.GetByte(12);
			entity.GlowEffect = reader.GetInt32(13);
			entity.HandNeeded = reader.GetByte(14);
			entity.Heading = reader.GetInt32(15);
			entity.IsDropable = reader.GetBoolean(16);
			entity.IsSaleable = reader.GetBoolean(17);
			entity.IsTradable = reader.GetBoolean(18);
			entity.ItemTemplateType = reader.GetString(19);
			entity.Level = reader.GetByte(20);
			entity.MaterialLevel = reader.GetByte(21);
			entity.MaxCharge = reader.GetByte(22);
			entity.MaxCount = reader.GetInt32(23);
			entity.Model = reader.GetInt32(24);
			entity.ModelExtension = reader.GetByte(25);
			entity.Name = reader.GetString(26);
			entity.or1 = reader.GetInt32(27);
			entity.PackSize = reader.GetInt32(28);
			entity.Precision = reader.GetByte(29);
			entity.ProcEffectType = reader.GetByte(30);
			entity.ProcSpellId = reader.GetInt32(31);
			entity.Quality = reader.GetInt32(32);
			entity.Range = reader.GetByte(33);
			entity.Realm = reader.GetByte(34);
			entity.Region = reader.GetInt32(35);
			entity.RespecType = reader.GetByte(36);
			entity.Size = reader.GetByte(37);
			entity.Speed = reader.GetInt32(38);
			entity.SpellId = reader.GetInt32(39);
			entity.TripPathId = reader.GetInt32(40);
			entity.Type = reader.GetByte(41);
			entity.Value = reader.GetInt64(42);
			entity.WeaponRange = reader.GetInt32(43);
			entity.Weight = reader.GetInt32(44);
			entity.X = reader.GetInt32(45);
			entity.Y = reader.GetInt32(46);
			entity.Z = reader.GetInt32(47);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ItemTemplateEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `itemtemplate` ("
				+"`ItemTemplateId` varchar(510) character set unicode,"
				+"`ArmorFactor` tinyint unsigned,"
				+"`ArmorLevel` tinyint unsigned,"
				+"`Bonus` int,"
				+"`BonusType` tinyint unsigned,"
				+"`Charge` tinyint unsigned,"
				+"`ChargeEffectType` tinyint unsigned,"
				+"`ChargeSpellId` int,"
				+"`Condition` double,"
				+"`Damage` tinyint unsigned,"
				+"`DamagePerSecond` tinyint unsigned,"
				+"`DamageType` tinyint unsigned,"
				+"`Durability` tinyint unsigned,"
				+"`GlowEffect` int,"
				+"`HandNeeded` tinyint unsigned,"
				+"`Heading` int,"
				+"`IsDropable` bit,"
				+"`IsSaleable` bit,"
				+"`IsTradable` bit,"
				+"`ItemTemplateType` varchar(510) character set unicode,"
				+"`Level` tinyint unsigned,"
				+"`MaterialLevel` tinyint unsigned,"
				+"`MaxCharge` tinyint unsigned,"
				+"`MaxCount` int,"
				+"`Model` int,"
				+"`ModelExtension` tinyint unsigned,"
				+"`Name` varchar(510) character set unicode,"
				+"`Color` int,"
				+"`PackSize` int,"
				+"`Precision` tinyint unsigned,"
				+"`ProcEffectType` tinyint unsigned,"
				+"`ProcSpellId` int,"
				+"`Quality` int,"
				+"`Range` tinyint unsigned,"
				+"`Realm` tinyint unsigned,"
				+"`Region` int,"
				+"`RespecType` tinyint unsigned,"
				+"`Size` tinyint unsigned,"
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
				+", primary key `ItemTemplateId` (`ItemTemplateId`)"
			);
		}

		public ItemTemplateDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
