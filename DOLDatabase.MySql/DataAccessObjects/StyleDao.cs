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
	public class StyleDao : IStyleDao
	{
		protected static readonly string c_rowFields = "`Id`,`AttackResultRequirement`,`BonusToDefense`,`BonusToHit`,`EnduranceCost`,`GrowthRate`,`KeyName`,`Name`,`OpeningRequirementType`,`OpeningRequirementValue`,`SpecialType`,`SpecialValue`,`SpecKeyName`,`SpecLevelRequirement`,`StealthRequirement`,`TwoHandAnimation`,`WeaponTypeRequirement`";
		protected readonly MySqlState m_state;

		public virtual StyleEntity Find(int id)
		{
			StyleEntity result = new StyleEntity();
			string command = "SELECT " + c_rowFields + " FROM `style` WHERE `Id`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref StyleEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `style` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AttackResultRequirement.ToString()) + "','" + m_state.EscapeString(obj.BonusToDefense.ToString()) + "','" + m_state.EscapeString(obj.BonusToHit.ToString()) + "','" + m_state.EscapeString(obj.EnduranceCost.ToString()) + "','" + m_state.EscapeString(obj.GrowthRate.ToString()) + "','" + m_state.EscapeString(obj.KeyName.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.OpeningRequirementType.ToString()) + "','" + m_state.EscapeString(obj.OpeningRequirementValue.ToString()) + "','" + m_state.EscapeString(obj.SpecialType.ToString()) + "','" + m_state.EscapeString(obj.SpecialValue.ToString()) + "','" + m_state.EscapeString(obj.SpecKeyName.ToString()) + "','" + m_state.EscapeString(obj.SpecLevelRequirement.ToString()) + "','" + m_state.EscapeString(obj.StealthRequirement.ToString()) + "','" + m_state.EscapeString(obj.TwoHandAnimation.ToString()) + "','" + m_state.EscapeString(obj.WeaponTypeRequirement.ToString()) + "');");
		}

		public virtual void Update(StyleEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `style` SET `Id`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AttackResultRequirement`='" + m_state.EscapeString(obj.AttackResultRequirement.ToString()) + "', `BonusToDefense`='" + m_state.EscapeString(obj.BonusToDefense.ToString()) + "', `BonusToHit`='" + m_state.EscapeString(obj.BonusToHit.ToString()) + "', `EnduranceCost`='" + m_state.EscapeString(obj.EnduranceCost.ToString()) + "', `GrowthRate`='" + m_state.EscapeString(obj.GrowthRate.ToString()) + "', `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `OpeningRequirementType`='" + m_state.EscapeString(obj.OpeningRequirementType.ToString()) + "', `OpeningRequirementValue`='" + m_state.EscapeString(obj.OpeningRequirementValue.ToString()) + "', `SpecialType`='" + m_state.EscapeString(obj.SpecialType.ToString()) + "', `SpecialValue`='" + m_state.EscapeString(obj.SpecialValue.ToString()) + "', `SpecKeyName`='" + m_state.EscapeString(obj.SpecKeyName.ToString()) + "', `SpecLevelRequirement`='" + m_state.EscapeString(obj.SpecLevelRequirement.ToString()) + "', `StealthRequirement`='" + m_state.EscapeString(obj.StealthRequirement.ToString()) + "', `TwoHandAnimation`='" + m_state.EscapeString(obj.TwoHandAnimation.ToString()) + "', `WeaponTypeRequirement`='" + m_state.EscapeString(obj.WeaponTypeRequirement.ToString()) + "' WHERE `Id`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(StyleEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `style` WHERE `Id`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<StyleEntity> SelectAll()
		{
			StyleEntity entity;
			List<StyleEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `style`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<StyleEntity>();
					while (reader.Read())
					{
						entity = new StyleEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `style`");
		}

		protected virtual void FillEntityWithRow(ref StyleEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AttackResultRequirement = reader.GetInt32(1);
			entity.BonusToDefense = reader.GetInt32(2);
			entity.BonusToHit = reader.GetInt32(3);
			entity.EnduranceCost = reader.GetInt32(4);
			entity.GrowthRate = reader.GetDouble(5);
			entity.KeyName = reader.GetString(6);
			entity.Name = reader.GetString(7);
			entity.OpeningRequirementType = reader.GetInt32(8);
			entity.OpeningRequirementValue = reader.GetInt32(9);
			entity.SpecialType = reader.GetInt32(10);
			entity.SpecialValue = reader.GetInt32(11);
			entity.SpecKeyName = reader.GetString(12);
			entity.SpecLevelRequirement = reader.GetInt32(13);
			entity.StealthRequirement = reader.GetBoolean(14);
			entity.TwoHandAnimation = reader.GetInt32(15);
			entity.WeaponTypeRequirement = reader.GetInt32(16);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(StyleEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `style` ("
				+"`Id` int NOT NULL,"
				+"`AttackResultRequirement` int NOT NULL,"
				+"`BonusToDefense` int NOT NULL,"
				+"`BonusToHit` int NOT NULL,"
				+"`EnduranceCost` int NOT NULL,"
				+"`GrowthRate` double NOT NULL,"
				+"`KeyName` char(255) character set latin1,"
				+"`Name` char(255) character set latin1 NOT NULL,"
				+"`OpeningRequirementType` int NOT NULL,"
				+"`OpeningRequirementValue` int NOT NULL,"
				+"`SpecialType` int NOT NULL,"
				+"`SpecialValue` int NOT NULL,"
				+"`SpecKeyName` char(255) character set latin1 NOT NULL,"
				+"`SpecLevelRequirement` int NOT NULL,"
				+"`StealthRequirement` bit NOT NULL,"
				+"`TwoHandAnimation` int NOT NULL,"
				+"`WeaponTypeRequirement` int NOT NULL"
				+", primary key `Id` (`Id`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `style`");
			return null;
		}

		public StyleDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
