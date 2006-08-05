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
	public class BindPointDao : IBindPointDao
	{
		protected static readonly string c_rowFields = "`BindPointId`,`Radius`,`Realm`,`Region`,`X`,`Y`,`Z`";
		private readonly MySqlState m_state;

		public virtual BindPointEntity Find(int key)
		{
			BindPointEntity result = new BindPointEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `bindpoint` WHERE `BindPointId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(BindPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `bindpoint` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Radius.ToString() + "`,`" + obj.Realm.ToString() + "`,`" + obj.Region.ToString() + "`,`" + obj.X.ToString() + "`,`" + obj.Y.ToString() + "`,`" + obj.Z.ToString() + "`);");
		}

		public virtual void Update(BindPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `bindpoint` SET `BindPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Radius`='" + m_state.EscapeString(obj.Radius.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `BindPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(BindPointEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `bindpoint` WHERE `BindPointId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected virtual void FillEntityWithRow(ref BindPointEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Radius = reader.GetInt32(1);
			entity.Realm = reader.GetInt32(2);
			entity.Region = reader.GetInt32(3);
			entity.X = reader.GetInt32(4);
			entity.Y = reader.GetInt32(5);
			entity.Z = reader.GetInt32(6);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(BindPointEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `bindpoint` ("
				+"`BindPointId` int,"
				+"`Radius` int,"
				+"`Realm` int,"
				+"`Region` int,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `BindPointId` (`BindPointId`)"
			);
		}

		public BindPointDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
