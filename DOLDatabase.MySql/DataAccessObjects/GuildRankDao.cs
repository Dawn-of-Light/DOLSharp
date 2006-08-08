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
		private readonly MySqlState m_state;

		public virtual GuildRankEntity Find(int key)
		{
			GuildRankEntity result = new GuildRankEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `guildrank` WHERE `GuildRankId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual void Create(GuildRankEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `guildrank` VALUES (`" + obj.Id.ToString() + "`,`" + obj.AcHear.ToString() + "`,`" + obj.AcSpeak.ToString() + "`,`" + obj.Alli.ToString() + "`,`" + obj.Buff.ToString() + "`,`" + obj.BuyBanner.ToString() + "`,`" + obj.Claim.ToString() + "`,`" + obj.Deposit.ToString() + "`,`" + obj.Dues.ToString() + "`,`" + obj.Emblem.ToString() + "`,`" + obj.GcHear.ToString() + "`,`" + obj.GcSpeak.ToString() + "`,`" + obj.GetMission.ToString() + "`,`" + obj.Guild.ToString() + "`,`" + obj.Invite.ToString() + "`,`" + obj.Motd.ToString() + "`,`" + obj.OcHear.ToString() + "`,`" + obj.OcSpeak.ToString() + "`,`" + obj.Promote.ToString() + "`,`" + obj.RankLevel.ToString() + "`,`" + obj.Release.ToString() + "`,`" + obj.Remove.ToString() + "`,`" + obj.SetNote.ToString() + "`,`" + obj.SummonBanner.ToString() + "`,`" + obj.Title.ToString() + "`,`" + obj.Upgrade.ToString() + "`,`" + obj.View.ToString() + "`,`" + obj.Withdraw.ToString() + "`);");
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
					results = new List<GuildRankEntity>(reader.FieldCount);
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

		public virtual int CountAll()
		{
			return (int)m_state.ExecuteScalar(
			"SELECT COUNT(*) FROM `guildrank`");

		}

		protected virtual void FillEntityWithRow(ref GuildRankEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AcHear = reader.GetString(1);
			entity.AcSpeak = reader.GetString(2);
			entity.Alli = reader.GetString(3);
			entity.Buff = reader.GetString(4);
			entity.BuyBanner = reader.GetString(5);
			entity.Claim = reader.GetString(6);
			entity.Deposit = reader.GetString(7);
			entity.Dues = reader.GetString(8);
			entity.Emblem = reader.GetString(9);
			entity.GcHear = reader.GetString(10);
			entity.GcSpeak = reader.GetString(11);
			entity.GetMission = reader.GetString(12);
			entity.Guild = reader.GetInt32(13);
			entity.Invite = reader.GetString(14);
			entity.Motd = reader.GetString(15);
			entity.OcHear = reader.GetString(16);
			entity.OcSpeak = reader.GetString(17);
			entity.Promote = reader.GetString(18);
			entity.RankLevel = reader.GetByte(19);
			entity.Release = reader.GetString(20);
			entity.Remove = reader.GetString(21);
			entity.SetNote = reader.GetString(22);
			entity.SummonBanner = reader.GetString(23);
			entity.Title = reader.GetString(24);
			entity.Upgrade = reader.GetString(25);
			entity.View = reader.GetString(26);
			entity.Withdraw = reader.GetString(27);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GuildRankEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `guildrank` ("
				+"`GuildRankId` int,"
				+"`AcHear` char(1) character set ascii,"
				+"`AcSpeak` char(1) character set ascii,"
				+"`Alli` char(1) character set ascii,"
				+"`Buff` char(1) character set ascii,"
				+"`BuyBanner` char(1) character set ascii,"
				+"`Claim` char(1) character set ascii,"
				+"`Deposit` char(1) character set ascii,"
				+"`Dues` char(1) character set ascii,"
				+"`Emblem` char(1) character set ascii,"
				+"`GcHear` char(1) character set ascii,"
				+"`GcSpeak` char(1) character set ascii,"
				+"`GetMission` char(1) character set ascii,"
				+"`GuildId` int,"
				+"`Invite` char(1) character set ascii,"
				+"`Motd` char(1) character set ascii,"
				+"`OcHear` char(1) character set ascii,"
				+"`OcSpeak` char(1) character set ascii,"
				+"`Promote` char(1) character set ascii,"
				+"`RankLevel` tinyint unsigned,"
				+"`Release` char(1) character set ascii,"
				+"`Remove` char(1) character set ascii,"
				+"`SetNote` char(1) character set ascii,"
				+"`SummonBanner` char(1) character set ascii,"
				+"`Title` varchar(510) character set unicode,"
				+"`Upgrade` char(1) character set ascii,"
				+"`View` char(1) character set ascii,"
				+"`Withdraw` char(1) character set ascii"
				+", primary key `GuildRankId` (`GuildRankId`)"
			);
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
