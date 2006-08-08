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
	public class GuildDao : IGuildDao
	{
		protected static readonly string c_rowFields = "`GuildId`,`Alliance`,`BountyPoints`,`Due`,`Email`,`Emblem`,`GuildName`,`Level`,`MeritPoints`,`Motd`,`OMotd`,`RealmPoints`,`TotalMoney`,`Webpage`";
		private readonly MySqlState m_state;

		public virtual GuildEntity Find(int key)
		{
			GuildEntity result = new GuildEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `guild` WHERE `GuildId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(GuildEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `guild` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Alliance.ToString() + "`,`" + obj.BountyPoints.ToString() + "`,`" + obj.Due.ToString() + "`,`" + obj.Email.ToString() + "`,`" + obj.Emblem.ToString() + "`,`" + obj.GuildName.ToString() + "`,`" + obj.Level.ToString() + "`,`" + obj.MeritPoints.ToString() + "`,`" + obj.Motd.ToString() + "`,`" + obj.OMotd.ToString() + "`,`" + obj.RealmPoints.ToString() + "`,`" + obj.TotalMoney.ToString() + "`,`" + obj.Webpage.ToString() + "`);");
		}

		public virtual void Update(GuildEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `guild` SET `GuildId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `Alliance`='" + m_state.EscapeString(obj.Alliance.ToString()) + "', `BountyPoints`='" + m_state.EscapeString(obj.BountyPoints.ToString()) + "', `Due`='" + m_state.EscapeString(obj.Due.ToString()) + "', `Email`='" + m_state.EscapeString(obj.Email.ToString()) + "', `Emblem`='" + m_state.EscapeString(obj.Emblem.ToString()) + "', `GuildName`='" + m_state.EscapeString(obj.GuildName.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `MeritPoints`='" + m_state.EscapeString(obj.MeritPoints.ToString()) + "', `Motd`='" + m_state.EscapeString(obj.Motd.ToString()) + "', `OMotd`='" + m_state.EscapeString(obj.OMotd.ToString()) + "', `RealmPoints`='" + m_state.EscapeString(obj.RealmPoints.ToString()) + "', `TotalMoney`='" + m_state.EscapeString(obj.TotalMoney.ToString()) + "', `Webpage`='" + m_state.EscapeString(obj.Webpage.ToString()) + "' WHERE `GuildId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GuildEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `guild` WHERE `GuildId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<GuildEntity> SelectAll()
		{
			GuildEntity entity;
			List<GuildEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `guild`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GuildEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new GuildEntity();
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
			"SELECT COUNT(*) FROM `guild`");

		}

		protected virtual void FillEntityWithRow(ref GuildEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Alliance = reader.GetInt32(1);
			entity.BountyPoints = reader.GetInt64(2);
			entity.Due = reader.GetString(3);
			entity.Email = reader.GetString(4);
			entity.Emblem = reader.GetInt32(5);
			entity.GuildName = reader.GetString(6);
			entity.Level = reader.GetInt32(7);
			entity.MeritPoints = reader.GetInt64(8);
			entity.Motd = reader.GetString(9);
			entity.OMotd = reader.GetString(10);
			entity.RealmPoints = reader.GetInt64(11);
			entity.TotalMoney = reader.GetInt64(12);
			entity.Webpage = reader.GetString(13);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GuildEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `guild` ("
				+"`GuildId` int,"
				+"`Alliance` int,"
				+"`BountyPoints` bigint,"
				+"`Due` char(1) character set ascii,"
				+"`Email` varchar(510) character set unicode,"
				+"`Emblem` int,"
				+"`GuildName` varchar(510) character set unicode,"
				+"`Level` int,"
				+"`MeritPoints` bigint,"
				+"`Motd` varchar(510) character set unicode,"
				+"`OMotd` varchar(510) character set unicode,"
				+"`RealmPoints` bigint,"
				+"`TotalMoney` bigint,"
				+"`Webpage` varchar(510) character set unicode"
				+", primary key `GuildId` (`GuildId`)"
			);
		}

		public GuildDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
