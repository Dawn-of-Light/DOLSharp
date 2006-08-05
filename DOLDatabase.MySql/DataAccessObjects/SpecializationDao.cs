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
	public class SpecializationDao : ISpecializationDao
	{
		protected static readonly string c_rowFields = "`KeyName`,`Description`,`Icon`,`Name`";
		private readonly MySqlState m_state;

		public virtual SpecializationEntity Find(string key)
		{
			SpecializationEntity result = new SpecializationEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `specialization` WHERE `KeyName`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(SpecializationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `specialization` VALUES (`" + obj.KeyName.ToString() + "`,`" + obj.Description.ToString() + "`,`" + obj.Icon.ToString() + "`,`" + obj.Name.ToString() + "`);");
		}

		public virtual void Update(SpecializationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `specialization` SET `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `Icon`='" + m_state.EscapeString(obj.Icon.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "' WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void Delete(SpecializationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `specialization` WHERE `KeyName`='" + m_state.EscapeString(obj.KeyName.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref SpecializationEntity entity, MySqlDataReader reader)
		{
			entity.KeyName = reader.GetString(0);
			entity.Description = reader.GetString(1);
			entity.Icon = reader.GetInt32(2);
			entity.Name = reader.GetString(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpecializationEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `specialization` ("
				+"`KeyName` varchar(510) character set unicode,"
				+"`Description` varchar(510) character set unicode,"
				+"`Icon` int,"
				+"`Name` varchar(510) character set unicode"
				+", primary key `KeyName` (`KeyName`)"
			);
		}

		public SpecializationDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
