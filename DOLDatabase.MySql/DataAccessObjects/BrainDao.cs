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
	public class BrainDao : IBrainDao
	{
		protected static readonly string c_rowFields = "`ABrainId`,`ABrainType`,`AggroLevel`,`AggroRange`";
		private readonly MySqlState m_state;

		public virtual BrainEntity Find(int aBrain)
		{
			BrainEntity result = new BrainEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `brain` WHERE `ABrainId`='" + m_state.EscapeString(aBrain.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(BrainEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `brain` VALUES (`" + obj.ABrain.ToString() + "`,`" + obj.ABrainType.ToString() + "`,`" + obj.AggroLevel.ToString() + "`,`" + obj.AggroRange.ToString() + "`);");
		}

		public virtual void Update(BrainEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `brain` SET `ABrainId`='" + m_state.EscapeString(obj.ABrain.ToString()) + "', `ABrainType`='" + m_state.EscapeString(obj.ABrainType.ToString()) + "', `AggroLevel`='" + m_state.EscapeString(obj.AggroLevel.ToString()) + "', `AggroRange`='" + m_state.EscapeString(obj.AggroRange.ToString()) + "' WHERE `ABrainId`='" + m_state.EscapeString(obj.ABrain.ToString()) + "'");
		}

		public virtual void Delete(BrainEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `brain` WHERE `ABrainId`='" + m_state.EscapeString(obj.ABrain.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<BrainEntity> SelectAll()
		{
			BrainEntity entity;
			List<BrainEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `brain`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<BrainEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new BrainEntity();
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
			"SELECT COUNT(*) FROM `brain`");

		}

		protected virtual void FillEntityWithRow(ref BrainEntity entity, MySqlDataReader reader)
		{
			entity.ABrain = reader.GetInt32(0);
			entity.ABrainType = reader.GetString(1);
			entity.AggroLevel = reader.GetInt32(2);
			entity.AggroRange = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(BrainEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `brain` ("
				+"`ABrainId` int,"
				+"`ABrainType` varchar(510) character set unicode,"
				+"`AggroLevel` int,"
				+"`AggroRange` int"
				+", primary key `ABrainId` (`ABrainId`)"
			);
		}

		public BrainDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
