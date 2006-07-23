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
	public class SpellDao : ISpellDao
	{
		private readonly MySqlState m_state;

		public virtual SpellEntity Find(int key)
		{
			SpellEntity result = new SpellEntity();
			m_state.ExecuteQuery(
				"SELECT `SpellId`,`AmnesiaChance`,`CastTime`,`ClientEffect`,`Concentration`,`Damage`,`DamageType`,`Description`,`Duration`,`EffectGroup`,`Frequency`,`Icon`,`InstrumentRequirement`,`LifeDrainReturn`,`Message1`,`Message2`,`Message3`,`Message4`,`Name`,`Power`,`Pulse`,`PulsePower`,`Radius`,`Range`,`RecastDelay`,`ResurrectHealth`,`ResurrectMana`,`SpellGroup`,`Target`,`Type`,`Value` FROM `spell` WHERE `SpellId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual void Create(SpellEntity obj)
		{
		}

		public virtual void Update(SpellEntity obj)
		{
		}

		public virtual void Delete(SpellEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref SpellEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.AmnesiaChance = reader.GetInt32(1);
			entity.CastTime = reader.GetDouble(2);
			entity.ClientEffect = reader.GetInt32(3);
			entity.Concentration = reader.GetInt32(4);
			entity.Damage = reader.GetDouble(5);
			entity.DamageType = reader.GetInt32(6);
			entity.Description = reader.GetString(7);
			entity.Duration = reader.GetInt32(8);
			entity.EffectGroup = reader.GetInt32(9);
			entity.Frequency = reader.GetInt32(10);
			entity.Icon = reader.GetInt32(11);
			entity.InstrumentRequirement = reader.GetInt32(12);
			entity.LifeDrainReturn = reader.GetInt32(13);
			entity.Message1 = reader.GetString(14);
			entity.Message2 = reader.GetString(15);
			entity.Message3 = reader.GetString(16);
			entity.Message4 = reader.GetString(17);
			entity.Name = reader.GetString(18);
			entity.Power = reader.GetInt32(19);
			entity.Pulse = reader.GetInt32(20);
			entity.PulsePower = reader.GetInt32(21);
			entity.Radius = reader.GetInt32(22);
			entity.Range = reader.GetInt32(23);
			entity.RecastDelay = reader.GetInt32(24);
			entity.ResurrectHealth = reader.GetInt32(25);
			entity.ResurrectMana = reader.GetInt32(26);
			entity.SpellGroup = reader.GetInt32(27);
			entity.Target = reader.GetString(28);
			entity.Type = reader.GetString(29);
			entity.Value = reader.GetDouble(30);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(SpellEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spell` ("
				+"`SpellId` int,"
				+"`AmnesiaChance` int,"
				+"`CastTime` double,"
				+"`ClientEffect` int,"
				+"`Concentration` int,"
				+"`Damage` double,"
				+"`DamageType` int,"
				+"`Description` varchar(510) character set unicode,"
				+"`Duration` int,"
				+"`EffectGroup` int,"
				+"`Frequency` int,"
				+"`Icon` int,"
				+"`InstrumentRequirement` int,"
				+"`LifeDrainReturn` int,"
				+"`Message1` varchar(510) character set unicode,"
				+"`Message2` varchar(510) character set unicode,"
				+"`Message3` varchar(510) character set unicode,"
				+"`Message4` varchar(510) character set unicode,"
				+"`Name` varchar(510) character set unicode,"
				+"`Power` int,"
				+"`Pulse` int,"
				+"`PulsePower` int,"
				+"`Radius` int,"
				+"`Range` int,"
				+"`RecastDelay` int,"
				+"`ResurrectHealth` int,"
				+"`ResurrectMana` int,"
				+"`SpellGroup` int,"
				+"`Target` varchar(510) character set unicode,"
				+"`Type` varchar(510) character set unicode,"
				+"`Value` double"
				+", primary key `SpellId` (`SpellId`)"
			);
		}

		public SpellDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
