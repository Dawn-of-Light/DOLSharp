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
	public class ItemMagicalBonusTemplateDao : IItemMagicalBonusTemplateDao
	{
		protected static readonly string c_rowFields = "`Bonus`,`BonusType`,`ItemTemplateId`";
		protected readonly MySqlState m_state;

		public virtual ItemMagicalBonusTemplateEntity Find(short bonus, byte bonusType, string itemTemplate)
		{
			ItemMagicalBonusTemplateEntity result = new ItemMagicalBonusTemplateEntity();
			string command = "SELECT " + c_rowFields + " FROM `itemmagicalbonustemplate` WHERE `Bonus`='" + m_state.EscapeString(bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(bonusType.ToString()) + "', `ItemTemplateId`='" + m_state.EscapeString(itemTemplate.ToString()) + "'";

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

		public virtual void Create(ItemMagicalBonusTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `itemmagicalbonustemplate` VALUES ('" + m_state.EscapeString(obj.Bonus.ToString()) + "','" + m_state.EscapeString(obj.BonusType.ToString()) + "','" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "');");
		}

		public virtual void Update(ItemMagicalBonusTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `itemmagicalbonustemplate` SET `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "', `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "', `ItemTemplateId`='" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "' WHERE `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "'AND `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "'AND `ItemTemplateId`='" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "'");
		}

		public virtual void Delete(ItemMagicalBonusTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `itemmagicalbonustemplate` WHERE `Bonus`='" + m_state.EscapeString(obj.Bonus.ToString()) + "'AND `BonusType`='" + m_state.EscapeString(obj.BonusType.ToString()) + "'AND `ItemTemplateId`='" + m_state.EscapeString(obj.ItemTemplate.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<ItemMagicalBonusTemplateEntity> SelectAll()
		{
			ItemMagicalBonusTemplateEntity entity;
			List<ItemMagicalBonusTemplateEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `itemmagicalbonustemplate`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<ItemMagicalBonusTemplateEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new ItemMagicalBonusTemplateEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `itemmagicalbonustemplate`");
		}

		protected virtual void FillEntityWithRow(ref ItemMagicalBonusTemplateEntity entity, MySqlDataReader reader)
		{
			entity.Bonus = reader.GetInt16(0);
			entity.BonusType = reader.GetByte(1);
			entity.ItemTemplate = reader.GetString(2);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(ItemMagicalBonusTemplateEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `itemmagicalbonustemplate` ("
				+"`Bonus` smallint,"
				+"`BonusType` tinyint unsigned,"
				+"`ItemTemplateId` varchar(255) character set utf8"
				+", primary key `BonusBonusTypeItemTemplateId` (`Bonus`,`BonusType`,`ItemTemplateId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `itemmagicalbonustemplate`");
			return null;
		}

		public ItemMagicalBonusTemplateDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
