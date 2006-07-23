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
		private readonly MySqlState m_state;

		public virtual StyleEntity Find(int key)
		{
			StyleEntity result = new StyleEntity();
			m_state.ExecuteQuery(
				"SELECT `Id`,`AttackResultRequirement`,`BonusToDefense`,`BonusToHit`,`EnduranceCost`,`GrowthRate`,`KeyName`,`Name`,`OpeningRequirementType`,`OpeningRequirementValue`,`SpecialType`,`SpecialValue`,`SpecKeyName`,`SpecLevelRequirement`,`StealthRequirement`,`TwoHandAnimation`,`WeaponTypeRequirement` FROM `style` WHERE `Id`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(StyleEntity obj)
		{
		}

		public virtual void Update(StyleEntity obj)
		{
		}

		public virtual void Delete(StyleEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref StyleEntity entity, MySqlDataReader reader)
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
			entity.StealthRequirement = reader.GetString(14);
			entity.TwoHandAnimation = reader.GetInt32(15);
			entity.WeaponTypeRequirement = reader.GetInt32(16);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(StyleEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `style` ("
				+"`Id` int,"
				+"`AttackResultRequirement` int,"
				+"`BonusToDefense` int,"
				+"`BonusToHit` int,"
				+"`EnduranceCost` int,"
				+"`GrowthRate` double,"
				+"`KeyName` varchar(510) character set unicode,"
				+"`Name` varchar(510) character set unicode,"
				+"`OpeningRequirementType` int,"
				+"`OpeningRequirementValue` int,"
				+"`SpecialType` int,"
				+"`SpecialValue` int,"
				+"`SpecKeyName` varchar(510) character set unicode,"
				+"`SpecLevelRequirement` int,"
				+"`StealthRequirement` char(1) character set ascii,"
				+"`TwoHandAnimation` int,"
				+"`WeaponTypeRequirement` int"
				+", primary key `Id` (`Id`)"
			);
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
