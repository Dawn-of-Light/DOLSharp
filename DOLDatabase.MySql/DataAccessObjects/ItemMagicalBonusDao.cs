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
	public class ItemMagicalBonusDao : IItemMagicalBonusDao
	{
		protected static readonly string c_rowFields = "`Bonus`,`BonusType`,`ItemId`";
		protected readonly MySqlState m_state;

		public virtual ItemMagicalBonusEntity Find(short bonus, byte bonusType, int item)
		{
			ItemMagicalBonusEntity result = new ItemMagicalBonusEntity();
			string command = "SELECT " + c_rowFields + " FROM `itemmagicalbonus` WHERE `Bonus`='" + m_state.EscapeString(bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(bonusType.ToString()) + "', `ItemId`='" + m_state.EscapeString(item.ToString()) + "'";

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

		public virtual void Create(ref ItemMagicalBonusEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `itemmagicalbonus` VALUES ('" + m_state.EscapeString(obj.Bonus.ToString()) + "','" + m_state.EscapeString(obj.BonusType.ToString()) + "','" + m_state.EscapeString(obj.Item.ToString()) + "');");
		}

		public virtual void Update(ItemMagicalBonusEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `itemmagicalbonus` SET `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "', `ItemId`='" + m_state.EscapeString(obj.Item.ToString()) + "' WHERE `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "'AND `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "'AND `ItemId`='" + m_state.EscapeString(obj.Item.ToString()) + "'");
		}

		public virtual void Delete(ItemMagicalBonusEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `itemmagicalbonus` WHERE `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "'AND `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "'AND `ItemId`='" + m_state.EscapeString(obj.Item.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ItemMagicalBonusEntity> SelectAll()
		{
			ItemMagicalBonusEntity entity;
			List<ItemMagicalBonusEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `itemmagicalbonus`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ItemMagicalBonusEntity>();
					while (reader.Read())
					{
						entity = new ItemMagicalBonusEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `itemmagicalbonus`");
		}

		protected virtual void FillEntityWithRow(ref ItemMagicalBonusEntity entity, MySqlDataReader reader)
		{
			entity.Bonus = reader.GetInt16(0);
			entity.BonusType = reader.GetByte(1);
			entity.Item = reader.GetInt32(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ItemMagicalBonusEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `itemmagicalbonus` ("
				+"`Bonus` smallint NOT NULL,"
				+"`BonusType` tinyint unsigned NOT NULL,"
				+"`ItemId` int NOT NULL"
				+", primary key `BonusBonusTypeItemId` (`Bonus`,`BonusType`,`ItemId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `itemmagicalbonus`");
			return null;
		}

		public ItemMagicalBonusDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
