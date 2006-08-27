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
	public class JumpPointMultiTargetLocationDao : IJumpPointMultiTargetLocationDao
	{
		protected static readonly string c_rowFields = "`Heading`,`JumpPointId`,`Realm`,`Region`,`X`,`Y`,`Z`";
		protected readonly MySqlState m_state;

		public virtual JumpPointMultiTargetLocationEntity Find(int heading, int jumpPoint, byte realm, int region, int x, int y, int z)
		{
			JumpPointMultiTargetLocationEntity result = new JumpPointMultiTargetLocationEntity();
			string command = "SELECT " + c_rowFields + " FROM `jumppointmultitargetlocation` WHERE `Heading`='" + m_state.EscapeString(heading.ToString()) + "', `JumpPointId`='" + m_state.EscapeString(jumpPoint.ToString()) + "', `Realm`='" + m_state.EscapeString(realm.ToString()) + "', `Region`='" + m_state.EscapeString(region.ToString()) + "', `X`='" + m_state.EscapeString(x.ToString()) + "', `Y`='" + m_state.EscapeString(y.ToString()) + "', `Z`='" + m_state.EscapeString(z.ToString()) + "'";

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

		public virtual void Create(JumpPointMultiTargetLocationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `jumppointmultitargetlocation` VALUES ('" + m_state.EscapeString(obj.Heading.ToString()) + "','" + m_state.EscapeString(obj.JumpPoint.ToString()) + "','" + m_state.EscapeString(obj.Realm.ToString()) + "','" + m_state.EscapeString(obj.Region.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "','" + m_state.EscapeString(obj.Z.ToString()) + "');");
		}

		public virtual void Update(JumpPointMultiTargetLocationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `jumppointmultitargetlocation` SET `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `JumpPointId`='" + m_state.EscapeString(obj.JumpPoint.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "'AND `JumpPointId`='" + m_state.EscapeString(obj.JumpPoint.ToString()) + "'AND `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "'AND `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "'AND `X`='" + m_state.EscapeString(obj.X.ToString()) + "'AND `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "'AND `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "'");
		}

		public virtual void Delete(JumpPointMultiTargetLocationEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `jumppointmultitargetlocation` WHERE `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "'AND `JumpPointId`='" + m_state.EscapeString(obj.JumpPoint.ToString()) + "'AND `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "'AND `Region`='" + m_state.EscapeString(obj.Region.ToString()) + "'AND `X`='" + m_state.EscapeString(obj.X.ToString()) + "'AND `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "'AND `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<JumpPointMultiTargetLocationEntity> SelectAll()
		{
			JumpPointMultiTargetLocationEntity entity;
			List<JumpPointMultiTargetLocationEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `jumppointmultitargetlocation`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<JumpPointMultiTargetLocationEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new JumpPointMultiTargetLocationEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `jumppointmultitargetlocation`");
		}

		protected virtual void FillEntityWithRow(ref JumpPointMultiTargetLocationEntity entity, MySqlDataReader reader)
		{
			entity.Heading = reader.GetInt32(0);
			entity.JumpPoint = reader.GetInt32(1);
			entity.Realm = reader.GetByte(2);
			entity.Region = reader.GetInt32(3);
			entity.X = reader.GetInt32(4);
			entity.Y = reader.GetInt32(5);
			entity.Z = reader.GetInt32(6);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(JumpPointMultiTargetLocationEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `jumppointmultitargetlocation` ("
				+"`Heading` int,"
				+"`JumpPointId` int,"
				+"`Realm` tinyint unsigned,"
				+"`Region` int,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `HeadingJumpPointIdRealmRegionXYZ` (`Heading`,`JumpPointId`,`Realm`,`Region`,`X`,`Y`,`Z`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `jumppointmultitargetlocation`");
			return null;
		}

		public JumpPointMultiTargetLocationDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
