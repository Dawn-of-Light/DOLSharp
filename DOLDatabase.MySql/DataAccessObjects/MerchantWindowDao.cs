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
	public class MerchantWindowDao : IMerchantWindowDao
	{
		protected static readonly string c_rowFields = "`MerchantWindowId`";
		private readonly MySqlState m_state;

		public virtual MerchantWindowEntity Find(int key)
		{
			MerchantWindowEntity result = new MerchantWindowEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `merchantwindow` WHERE `MerchantWindowId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(MerchantWindowEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `merchantwindow` VALUES (`" + obj.Id.ToString() + "`);");
		}

		public virtual void Update(MerchantWindowEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `merchantwindow` SET `MerchantWindowId`='" + m_state.EscapeString(obj.Id.ToString()) + "' WHERE `MerchantWindowId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(MerchantWindowEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `merchantwindow` WHERE `MerchantWindowId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<MerchantWindowEntity> SelectAll()
		{
			MerchantWindowEntity entity;
			List<MerchantWindowEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `merchantwindow`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<MerchantWindowEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new MerchantWindowEntity();
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
			"SELECT COUNT(*) FROM `merchantwindow`");

		}

		protected virtual void FillEntityWithRow(ref MerchantWindowEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(MerchantWindowEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `merchantwindow` ("
				+"`MerchantWindowId` int"
				+", primary key `MerchantWindowId` (`MerchantWindowId`)"
			);
		}

		public MerchantWindowDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
