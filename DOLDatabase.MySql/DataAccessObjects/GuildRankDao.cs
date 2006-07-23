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
		private readonly MySqlState m_state;

		public virtual GuildRankEntity Find(int key)
		{
			GuildRankEntity result = new GuildRankEntity();
			m_state.ExecuteQuery(
				"SELECT `GuildRankId`,`AcHear`,`AcSpeak`,`Alli`,`Buff`,`BuyBanner`,`Claim`,`Deposit`,`Dues`,`Emblem`,`GcHear`,`GcSpeak`,`GetMission`,`GuildId`,`Invite`,`Motd`,`OcHear`,`OcSpeak`,`Promote`,`RankLevel`,`Release`,`Remove`,`SetNote`,`SummonBanner`,`Title`,`Upgrade`,`View`,`Withdraw` FROM `guildrank` WHERE `GuildRankId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(GuildRankEntity obj)
		{
		}

		public virtual void Update(GuildRankEntity obj)
		{
		}

		public virtual void Delete(GuildRankEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref GuildRankEntity entity, MySqlDataReader reader)
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
