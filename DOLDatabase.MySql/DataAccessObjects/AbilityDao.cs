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
	public class AbilityDao : IAbilityDao
	{
		protected static readonly string c_rowFields = "`KeyName`,`Description`,`IconId`,`Name`";
		private readonly MySqlState m_state;

		public virtual AbilityEntity Find(string key)
		{
			AbilityEntity result = new AbilityEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `ability` WHERE `KeyName`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(AbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `ability` VALUES (`" + obj.KeyName.ToString() + "`,`" + obj.Description.ToString() + "`,`" + obj.IconId.ToString() + "`,`" + obj.Name.ToString() + "`);");
		}

		public virtual void Update(AbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `ability` SET `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `IconId`='" + m_state.EscapeString(obj.IconId.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "' WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void Delete(AbilityEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `ability` WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref AbilityEntity entity, MySqlDataReader reader)
		{
			entity.KeyName = reader.GetString(0);
			entity.Description = reader.GetString(1);
			entity.IconId = reader.GetInt32(2);
			entity.Name = reader.GetString(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(AbilityEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `ability` ("
				+"`KeyName` varchar(100) character set unicode,"
				+"`Description` varchar(510) character set unicode,"
				+"`IconId` int,"
				+"`Name` varchar(510) character set unicode"
				+", primary key `KeyName` (`KeyName`)"
			);
		}

		public AbilityDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
