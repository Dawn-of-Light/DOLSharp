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
	public class BrainTemplateDao : IBrainTemplateDao
	{
		protected static readonly string c_rowFields = "`ABrainTemplateId`,`ABrainTemplateType`,`AggroLevel`,`AggroRange`";
		protected readonly MySqlState m_state;

		public virtual BrainTemplateEntity Find(int aBrainTemplate)
		{
			BrainTemplateEntity result = new BrainTemplateEntity();
			string command = "SELECT " + c_rowFields + " FROM `braintemplate` WHERE `ABrainTemplateId`='" + m_state.EscapeString(aBrainTemplate.ToString()) + "'";

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

		public virtual void Create(ref BrainTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `braintemplate` VALUES ('" + m_state.EscapeString(obj.ABrainTemplate.ToString()) + "','" + m_state.EscapeString(obj.ABrainTemplateType.ToString()) + "','" + m_state.EscapeString(obj.AggroLevel.ToString()) + "','" + m_state.EscapeString(obj.AggroRange.ToString()) + "');");
		}

		public virtual void Update(BrainTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `braintemplate` SET `ABrainTemplateId`='" + m_state.EscapeString(obj.ABrainTemplate.ToString()) + "', `ABrainTemplateType`='" + m_state.EscapeString(obj.ABrainTemplateType.ToString()) + "', `AggroLevel`='" + m_state.EscapeString(obj.AggroLevel.ToString()) + "', `AggroRange`='" + m_state.EscapeString(obj.AggroRange.ToString()) + "' WHERE `ABrainTemplateId`='" + m_state.EscapeString(obj.ABrainTemplate.ToString()) + "'");
		}

		public virtual void Delete(BrainTemplateEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `braintemplate` WHERE `ABrainTemplateId`='" + m_state.EscapeString(obj.ABrainTemplate.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<BrainTemplateEntity> SelectAll()
		{
			BrainTemplateEntity entity;
			List<BrainTemplateEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `braintemplate`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<BrainTemplateEntity>();
					while (reader.Read())
					{
						entity = new BrainTemplateEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `braintemplate`");
		}

		protected virtual void FillEntityWithRow(ref BrainTemplateEntity entity, MySqlDataReader reader)
		{
			entity.ABrainTemplate = reader.GetInt32(0);
			entity.ABrainTemplateType = reader.GetString(1);
			entity.AggroLevel = reader.GetInt32(2);
			entity.AggroRange = reader.GetInt32(3);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(BrainTemplateEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `braintemplate` ("
				+"`ABrainTemplateId` int NOT NULL,"
				+"`ABrainTemplateType` char(255) character set latin1 NOT NULL,"
				+"`AggroLevel` int NOT NULL,"
				+"`AggroRange` int NOT NULL"
				+", primary key `ABrainTemplateId` (`ABrainTemplateId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `braintemplate`");
			return null;
		}

		public BrainTemplateDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
