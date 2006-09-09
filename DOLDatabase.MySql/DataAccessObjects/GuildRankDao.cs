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
	public class GuildRankDao : IGuildRankDao
	{
		protected static readonly string c_rowFields = "`GuildRankId`,`AcHear`,`AcSpeak`,`Alli`,`Buff`,`BuyBanner`,`Claim`,`Deposit`,`Dues`,`Emblem`,`GcHear`,`GcSpeak`,`GetMission`,`GuildId`,`Invite`,`Motd`,`OcHear`,`OcSpeak`,`Promote`,`RankLevel`,`Release`,`Remove`,`SetNote`,`SummonBanner`,`Title`,`Upgrade`,`View`,`Withdraw`";
		protected readonly MySqlState m_state;

		public virtual GuildRankEntity Find(int id)
		{
			GuildRankEntity result = new GuildRankEntity();
			string command = "SELECT " + c_rowFields + " FROM `guildrank` WHERE `GuildRankId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(ref GuildRankEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `guildrank` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AcHear.ToString()) + "','" + m_state.EscapeString(obj.AcSpeak.ToString()) + "','" + m_state.EscapeString(obj.Alli.ToString()) + "','" + m_state.EscapeString(obj.Buff.ToString()) + "','" + m_state.EscapeString(obj.BuyBanner.ToString()) + "','" + m_state.EscapeString(obj.Claim.ToString()) + "','" + m_state.EscapeString(obj.Deposit.ToString()) + "','" + m_state.EscapeString(obj.Dues.ToString()) + "','" + m_state.EscapeString(obj.Emblem.ToString()) + "','" + m_state.EscapeString(obj.GcHear.ToString()) + "','" + m_state.EscapeString(obj.GcSpeak.ToString()) + "','" + m_state.EscapeString(obj.GetMission.ToString()) + "','" + m_state.EscapeString(obj.Guild.ToString()) + "','" + m_state.EscapeString(obj.Invite.ToString()) + "','" + m_state.EscapeString(obj.Motd.ToString()) + "','" + m_state.EscapeString(obj.OcHear.ToString()) + "','" + m_state.EscapeString(obj.OcSpeak.ToString()) + "','" + m_state.EscapeString(obj.Promote.ToString()) + "','" + m_state.EscapeString(obj.RankLevel.ToString()) + "','" + m_state.EscapeString(obj.Release.ToString()) + "','" + m_state.EscapeString(obj.Remove.ToString()) + "','" + m_state.EscapeString(obj.SetNote.ToString()) + "','" + m_state.EscapeString(obj.SummonBanner.ToString()) + "','" + m_state.EscapeString(obj.Title.ToString()) + "','" + m_state.EscapeString(obj.Upgrade.ToString()) + "','" + m_state.EscapeString(obj.View.ToString()) + "','" + m_state.EscapeString(obj.Withdraw.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
		}

		public virtual void Update(GuildRankEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `guildrank` SET `GuildRankId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AcHear`='" + m_state.EscapeString(obj.AcHear.ToString()) + "', `AcSpeak`='" + m_state.EscapeString(obj.AcSpeak.ToString()) + "', `Alli`='" + m_state.EscapeString(obj.Alli.ToString()) + "', `Buff`='" + m_state.EscapeString(obj.Buff.ToString()) + "', `BuyBanner`='" + m_state.EscapeString(obj.BuyBanner.ToString()) + "', `Claim`='" + m_state.EscapeString(obj.Claim.ToString()) + "', `Deposit`='" + m_state.EscapeString(obj.Deposit.ToString()) + "', `Dues`='" + m_state.EscapeString(obj.Dues.ToString()) + "', `Emblem`='" + m_state.EscapeString(obj.Emblem.ToString()) + "', `GcHear`='" + m_state.EscapeString(obj.GcHear.ToString()) + "', `GcSpeak`='" + m_state.EscapeString(obj.GcSpeak.ToString()) + "', `GetMission`='" + m_state.EscapeString(obj.GetMission.ToString()) + "', `GuildId`='" + m_state.EscapeString(obj.Guild.ToString()) + "', `Invite`='" + m_state.EscapeString(obj.Invite.ToString()) + "', `Motd`='" + m_state.EscapeString(obj.Motd.ToString()) + "', `OcHear`='" + m_state.EscapeString(obj.OcHear.ToString()) + "', `OcSpeak`='" + m_state.EscapeString(obj.OcSpeak.ToString()) + "', `Promote`='" + m_state.EscapeString(obj.Promote.ToString()) + "', `RankLevel`='" + m_state.EscapeString(obj.RankLevel.ToString()) + "', `Release`='" + m_state.EscapeString(obj.Release.ToString()) + "', `Remove`='" + m_state.EscapeString(obj.Remove.ToString()) + "', `SetNote`='" + m_state.EscapeString(obj.SetNote.ToString()) + "', `SummonBanner`='" + m_state.EscapeString(obj.SummonBanner.ToString()) + "', `Title`='" + m_state.EscapeString(obj.Title.ToString()) + "', `Upgrade`='" + m_state.EscapeString(obj.Upgrade.ToString()) + "', `View`='" + m_state.EscapeString(obj.View.ToString()) + "', `Withdraw`='" + m_state.EscapeString(obj.Withdraw.ToString()) + "' WHERE `GuildRankId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GuildRankEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `guildrank` WHERE `GuildRankId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<GuildRankEntity> SelectAll()
		{
			GuildRankEntity entity;
			List<GuildRankEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `guildrank`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GuildRankEntity>();
					while (reader.Read())
					{
						entity = new GuildRankEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `guildrank`");
		}

		protected virtual void FillEntityWithRow(ref GuildRankEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AcHear = reader.GetBoolean(1);
			entity.AcSpeak = reader.GetBoolean(2);
			entity.Alli = reader.GetBoolean(3);
			entity.Buff = reader.GetBoolean(4);
			entity.BuyBanner = reader.GetBoolean(5);
			entity.Claim = reader.GetBoolean(6);
			entity.Deposit = reader.GetBoolean(7);
			entity.Dues = reader.GetBoolean(8);
			entity.Emblem = reader.GetBoolean(9);
			entity.GcHear = reader.GetBoolean(10);
			entity.GcSpeak = reader.GetBoolean(11);
			entity.GetMission = reader.GetBoolean(12);
			entity.Guild = reader.GetInt32(13);
			entity.Invite = reader.GetBoolean(14);
			entity.Motd = reader.GetBoolean(15);
			entity.OcHear = reader.GetBoolean(16);
			entity.OcSpeak = reader.GetBoolean(17);
			entity.Promote = reader.GetBoolean(18);
			entity.RankLevel = reader.GetByte(19);
			entity.Release = reader.GetBoolean(20);
			entity.Remove = reader.GetBoolean(21);
			entity.SetNote = reader.GetBoolean(22);
			entity.SummonBanner = reader.GetBoolean(23);
			entity.Title = reader.GetString(24);
			entity.Upgrade = reader.GetBoolean(25);
			entity.View = reader.GetBoolean(26);
			entity.Withdraw = reader.GetBoolean(27);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GuildRankEntity); }
		}

		public IList<string> VerifySchema()
		{
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `guildrank` ("
				+"`GuildRankId` int NOT NULL auto_increment,"
				+"`AcHear` bit NOT NULL,"
				+"`AcSpeak` bit NOT NULL,"
				+"`Alli` bit NOT NULL,"
				+"`Buff` bit NOT NULL,"
				+"`BuyBanner` bit NOT NULL,"
				+"`Claim` bit NOT NULL,"
				+"`Deposit` bit NOT NULL,"
				+"`Dues` bit NOT NULL,"
				+"`Emblem` bit NOT NULL,"
				+"`GcHear` bit NOT NULL,"
				+"`GcSpeak` bit NOT NULL,"
				+"`GetMission` bit NOT NULL,"
				+"`GuildId` int,"
				+"`Invite` bit NOT NULL,"
				+"`Motd` bit NOT NULL,"
				+"`OcHear` bit NOT NULL,"
				+"`OcSpeak` bit NOT NULL,"
				+"`Promote` bit NOT NULL,"
				+"`RankLevel` tinyint unsigned NOT NULL,"
				+"`Release` bit NOT NULL,"
				+"`Remove` bit NOT NULL,"
				+"`SetNote` bit NOT NULL,"
				+"`SummonBanner` bit NOT NULL,"
				+"`Title` char(255) character set latin1 NOT NULL,"
				+"`Upgrade` bit NOT NULL,"
				+"`View` bit NOT NULL,"
				+"`Withdraw` bit NOT NULL"
				+", primary key `GuildRankId` (`GuildRankId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `guildrank`");
			return null;
		}

		public GuildRankDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
