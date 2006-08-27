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
		protected static readonly string c_rowFields = "`SpellId`,`AmnesiaChance`,`CastTime`,`ClientEffect`,`Concentration`,`Damage`,`DamageType`,`Description`,`Duration`,`EffectGroup`,`Frequency`,`Icon`,`InstrumentRequirement`,`LifeDrainReturn`,`Message1`,`Message2`,`Message3`,`Message4`,`Name`,`Power`,`Pulse`,`PulsePower`,`Radius`,`Range`,`RecastDelay`,`ResurrectHealth`,`ResurrectMana`,`SpellGroup`,`Target`,`Type`,`Value`";
		protected readonly MySqlState m_state;

		public virtual SpellEntity Find(int id)
		{
			SpellEntity result = new SpellEntity();
			string command = "SELECT " + c_rowFields + " FROM `spell` WHERE `SpellId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual void Create(SpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `spell` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.AmnesiaChance.ToString()) + "','" + m_state.EscapeString(obj.CastTime.ToString()) + "','" + m_state.EscapeString(obj.ClientEffect.ToString()) + "','" + m_state.EscapeString(obj.Concentration.ToString()) + "','" + m_state.EscapeString(obj.Damage.ToString()) + "','" + m_state.EscapeString(obj.DamageType.ToString()) + "','" + m_state.EscapeString(obj.Description.ToString()) + "','" + m_state.EscapeString(obj.Duration.ToString()) + "','" + m_state.EscapeString(obj.EffectGroup.ToString()) + "','" + m_state.EscapeString(obj.Frequency.ToString()) + "','" + m_state.EscapeString(obj.Icon.ToString()) + "','" + m_state.EscapeString(obj.InstrumentRequirement.ToString()) + "','" + m_state.EscapeString(obj.LifeDrainReturn.ToString()) + "','" + m_state.EscapeString(obj.Message1.ToString()) + "','" + m_state.EscapeString(obj.Message2.ToString()) + "','" + m_state.EscapeString(obj.Message3.ToString()) + "','" + m_state.EscapeString(obj.Message4.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.Power.ToString()) + "','" + m_state.EscapeString(obj.Pulse.ToString()) + "','" + m_state.EscapeString(obj.PulsePower.ToString()) + "','" + m_state.EscapeString(obj.Radius.ToString()) + "','" + m_state.EscapeString(obj.Range.ToString()) + "','" + m_state.EscapeString(obj.RecastDelay.ToString()) + "','" + m_state.EscapeString(obj.ResurrectHealth.ToString()) + "','" + m_state.EscapeString(obj.ResurrectMana.ToString()) + "','" + m_state.EscapeString(obj.SpellGroup.ToString()) + "','" + m_state.EscapeString(obj.Target.ToString()) + "','" + m_state.EscapeString(obj.Type.ToString()) + "','" + m_state.EscapeString(obj.Value.ToString()) + "');");
		}

		public virtual void Update(SpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `spell` SET `SpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AmnesiaChance`='" + m_state.EscapeString(obj.AmnesiaChance.ToString()) + "', `CastTime`='" + m_state.EscapeString(obj.CastTime.ToString()) + "', `ClientEffect`='" + m_state.EscapeString(obj.ClientEffect.ToString()) + "', `Concentration`='" + m_state.EscapeString(obj.Concentration.ToString()) + "', `Damage`='" + m_state.EscapeString(obj.Damage.ToString()) + "', `DamageType`='" + m_state.EscapeString(obj.DamageType.ToString()) + "', `Description`='" + m_state.EscapeString(obj.Description.ToString()) + "', `Duration`='" + m_state.EscapeString(obj.Duration.ToString()) + "', `EffectGroup`='" + m_state.EscapeString(obj.EffectGroup.ToString()) + "', `Frequency`='" + m_state.EscapeString(obj.Frequency.ToString()) + "', `Icon`='" + m_state.EscapeString(obj.Icon.ToString()) + "', `InstrumentRequirement`='" + m_state.EscapeString(obj.InstrumentRequirement.ToString()) + "', `LifeDrainReturn`='" + m_state.EscapeString(obj.LifeDrainReturn.ToString()) + "', `Message1`='" + m_state.EscapeString(obj.Message1.ToString()) + "', `Message2`='" + m_state.EscapeString(obj.Message2.ToString()) + "', `Message3`='" + m_state.EscapeString(obj.Message3.ToString()) + "', `Message4`='" + m_state.EscapeString(obj.Message4.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `Power`='" + m_state.EscapeString(obj.Power.ToString()) + "', `Pulse`='" + m_state.EscapeString(obj.Pulse.ToString()) + "', `PulsePower`='" + m_state.EscapeString(obj.PulsePower.ToString()) + "', `Radius`='" + m_state.EscapeString(obj.Radius.ToString()) + "', `Range`='" + m_state.EscapeString(obj.Range.ToString()) + "', `RecastDelay`='" + m_state.EscapeString(obj.RecastDelay.ToString()) + "', `ResurrectHealth`='" + m_state.EscapeString(obj.ResurrectHealth.ToString()) + "', `ResurrectMana`='" + m_state.EscapeString(obj.ResurrectMana.ToString()) + "', `SpellGroup`='" + m_state.EscapeString(obj.SpellGroup.ToString()) + "', `Target`='" + m_state.EscapeString(obj.Target.ToString()) + "', `Type`='" + m_state.EscapeString(obj.Type.ToString()) + "', `Value`='" + m_state.EscapeString(obj.Value.ToString()) + "' WHERE `SpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(SpellEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `spell` WHERE `SpellId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<SpellEntity> SelectAll()
		{
			SpellEntity entity;
			List<SpellEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `spell`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<SpellEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new SpellEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `spell`");
		}

		protected virtual void FillEntityWithRow(ref SpellEntity entity, MySqlDataReader reader)
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
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `spell` ("
				+"`SpellId` int,"
				+"`AmnesiaChance` int,"
				+"`CastTime` double,"
				+"`ClientEffect` int,"
				+"`Concentration` int,"
				+"`Damage` double,"
				+"`DamageType` int,"
				+"`Description` varchar(255) character set utf8,"
				+"`Duration` int,"
				+"`EffectGroup` int,"
				+"`Frequency` int,"
				+"`Icon` int,"
				+"`InstrumentRequirement` int,"
				+"`LifeDrainReturn` int,"
				+"`Message1` varchar(255) character set utf8,"
				+"`Message2` varchar(255) character set utf8,"
				+"`Message3` varchar(255) character set utf8,"
				+"`Message4` varchar(255) character set utf8,"
				+"`Name` varchar(255) character set utf8,"
				+"`Power` int,"
				+"`Pulse` int,"
				+"`PulsePower` int,"
				+"`Radius` int,"
				+"`Range` int,"
				+"`RecastDelay` int,"
				+"`ResurrectHealth` int,"
				+"`ResurrectMana` int,"
				+"`SpellGroup` int,"
				+"`Target` varchar(255) character set utf8,"
				+"`Type` varchar(255) character set utf8,"
				+"`Value` double"
				+", primary key `SpellId` (`SpellId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `spell`");
			return null;
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
