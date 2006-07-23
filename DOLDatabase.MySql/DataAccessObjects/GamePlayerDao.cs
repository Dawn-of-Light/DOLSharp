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
	public class GamePlayerDao : IGamePlayerDao
	{
		private readonly MySqlState m_state;

		public virtual GamePlayerEntity Find(int key)
		{
			GamePlayerEntity result = new GamePlayerEntity();
			m_state.ExecuteQuery(
				"SELECT `PersistantGameObjectId`,`AccountId`,`ActiveQuiverSlot`,`ActiveWeaponSlot`,`BaseCharisma`,`BaseConstitution`,`BaseDexterity`,`BaseEmpathy`,`BaseIntelligence`,`BasePiety`,`BaseQuickness`,`BaseStrength`,`BindHeading`,`BindRegionId`,`BindX`,`BindY`,`BindZ`,`BountyPoints`,`CancelStyle`,`CapturedKeeps`,`CapturedTowers`,`CharacterClassId`,`CraftingPrimarySkill`,`CreationDate`,`CreationModel`,`CurrentTitleType`,`CustomisationStep`,`DeathCount`,`DeathTime`,`DisabledAbilities`,`DisabledSpells`,`EndurancePercent`,`Experience`,`EyeColor`,`EyeSize`,`FaceType`,`Gender`,`GuildName`,`GuildNameFlag`,`GuildRank`,`HairColor`,`HairStyle`,`Heading`,`Health`,`IsAnonymous`,`IsLevelRespecUsed`,`IsLevelSecondStage`,`KillsAlbionDeathBlows`,`KillsAlbionPlayers`,`KillsAlbionSolo`,`KillsHiberniaDeathBlows`,`KillsHiberniaPlayers`,`KillsHiberniaSolo`,`KillsMidgardDeathBlows`,`KillsMidgardPlayers`,`KillsMidgardSolo`,`LastName`,`LastPlayed`,`Level`,`LipSize`,`LotNumber`,`Mana`,`MaxSpeedBase`,`Model`,`Money`,`MoodType`,`Name`,`PlayedTime`,`Race`,`Realm`,`RealmLevel`,`RealmPoints`,`RealmSpecialtyPoints`,`RegionId`,`RespecAmountAllSkill`,`RespecAmountSingleSkill`,`RespecBought`,`SafetyFlag`,`SerializedAbilities`,`SerializedCraftingSkills`,`SerializedFriendsList`,`SerializedSpecs`,`SerializedSpellLines`,`SkillSpecialtyPoints`,`SlotPosition`,`SpellQueue`,`Styles`,`TaskDone`,`TotalConstitutionLostAtDeath`,`UsedLevelCommand`,`X`,`Y`,`Z` FROM `gameplayer` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(key.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					FillEntityWithRow(ref result, reader);
				}
			);
			return result;
		}

		public virtual GamePlayerEntity FindByAccountAndRealm(int account, byte realm)
		{
			return new GamePlayerEntity();
		}

		public virtual void Create(GamePlayerEntity obj)
		{
		}

		public virtual void Update(GamePlayerEntity obj)
		{
		}

		public virtual void Delete(GamePlayerEntity obj)
		{
		}

		public virtual void SaveAll()
		{
		}

		public virtual int CountAll()
		{
			return -1;
		}

		protected void FillEntityWithRow(ref GamePlayerEntity entity, MySqlDataReader reader)
		{
			entity.Id = reader.GetInt32(0);
			entity.Account = reader.GetInt32(1);
			entity.ActiveQuiverSlot = reader.GetByte(2);
			entity.ActiveWeaponSlot = reader.GetByte(3);
			entity.BaseCharisma = reader.GetInt32(4);
			entity.BaseConstitution = reader.GetInt32(5);
			entity.BaseDexterity = reader.GetInt32(6);
			entity.BaseEmpathy = reader.GetInt32(7);
			entity.BaseIntelligence = reader.GetInt32(8);
			entity.BasePiety = reader.GetInt32(9);
			entity.BaseQuickness = reader.GetInt32(10);
			entity.BaseStrength = reader.GetInt32(11);
			entity.BindHeading = reader.GetInt32(12);
			entity.BindRegionId = reader.GetInt32(13);
			entity.BindX = reader.GetInt32(14);
			entity.BindY = reader.GetInt32(15);
			entity.BindZ = reader.GetInt32(16);
			entity.BountyPoints = reader.GetInt64(17);
			entity.CancelStyle = reader.GetString(18);
			entity.CapturedKeeps = reader.GetInt32(19);
			entity.CapturedTowers = reader.GetInt32(20);
			entity.CharacterClassId = reader.GetInt32(21);
			entity.CraftingPrimarySkill = reader.GetInt32(22);
			entity.CreationDate = reader.GetDateTime(23);
			entity.CreationModel = reader.GetInt32(24);
			entity.CurrentTitleType = reader.GetString(25);
			entity.CustomisationStep = reader.GetByte(26);
			entity.DeathCount = reader.GetByte(27);
			entity.DeathTime = reader.GetInt64(28);
			entity.DisabledAbilities = reader.GetString(29);
			entity.DisabledSpells = reader.GetString(30);
			entity.EndurancePercent = reader.GetByte(31);
			entity.Experience = reader.GetInt64(32);
			entity.EyeColor = reader.GetByte(33);
			entity.EyeSize = reader.GetByte(34);
			entity.FaceType = reader.GetByte(35);
			entity.Gender = reader.GetInt32(36);
			entity.GuildName = reader.GetString(37);
			entity.GuildNameFlag = reader.GetString(38);
			entity.GuildRank = reader.GetByte(39);
			entity.HairColor = reader.GetByte(40);
			entity.HairStyle = reader.GetByte(41);
			entity.Heading = reader.GetInt32(42);
			entity.Health = reader.GetInt32(43);
			entity.IsAnonymous = reader.GetString(44);
			entity.IsLevelRespecUsed = reader.GetString(45);
			entity.IsLevelSecondStage = reader.GetString(46);
			entity.KillsAlbionDeathBlows = reader.GetInt32(47);
			entity.KillsAlbionPlayers = reader.GetInt32(48);
			entity.KillsAlbionSolo = reader.GetInt32(49);
			entity.KillsHiberniaDeathBlows = reader.GetInt32(50);
			entity.KillsHiberniaPlayers = reader.GetInt32(51);
			entity.KillsHiberniaSolo = reader.GetInt32(52);
			entity.KillsMidgardDeathBlows = reader.GetInt32(53);
			entity.KillsMidgardPlayers = reader.GetInt32(54);
			entity.KillsMidgardSolo = reader.GetInt32(55);
			entity.LastName = reader.GetString(56);
			entity.LastPlayed = reader.GetDateTime(57);
			entity.Level = reader.GetByte(58);
			entity.LipSize = reader.GetByte(59);
			entity.LotNumber = reader.GetInt32(60);
			entity.Mana = reader.GetInt32(61);
			entity.MaxSpeedBase = reader.GetInt32(62);
			entity.Model = reader.GetInt32(63);
			entity.Money = reader.GetInt64(64);
			entity.MoodType = reader.GetByte(65);
			entity.Name = reader.GetString(66);
			entity.PlayedTime = reader.GetInt64(67);
			entity.Race = reader.GetInt32(68);
			entity.Realm = reader.GetByte(69);
			entity.RealmLevel = reader.GetInt32(70);
			entity.RealmPoints = reader.GetInt64(71);
			entity.RealmSpecialtyPoints = reader.GetInt32(72);
			entity.RegionId = reader.GetInt32(73);
			entity.RespecAmountAllSkill = reader.GetInt32(74);
			entity.RespecAmountSingleSkill = reader.GetInt32(75);
			entity.RespecBought = reader.GetInt32(76);
			entity.SafetyFlag = reader.GetString(77);
			entity.SerializedAbilities = reader.GetString(78);
			entity.SerializedCraftingSkills = reader.GetString(79);
			entity.SerializedFriendsList = reader.GetString(80);
			entity.SerializedSpecs = reader.GetString(81);
			entity.SerializedSpellLines = reader.GetString(82);
			entity.SkillSpecialtyPoints = reader.GetInt32(83);
			entity.SlotPosition = reader.GetByte(84);
			entity.SpellQueue = reader.GetString(85);
			entity.Styles = reader.GetString(86);
			entity.TaskDone = reader.GetByte(87);
			entity.TotalConstitutionLostAtDeath = reader.GetInt32(88);
			entity.UsedLevelCommand = reader.GetString(89);
			entity.X = reader.GetInt32(90);
			entity.Y = reader.GetInt32(91);
			entity.Z = reader.GetInt32(92);
		}

		public virtual Type TransferObjectType
		{
			get { return typeof(GamePlayerEntity); }
		}

		public IList<string> VerifySchema()
		{
			return null;
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gameplayer` ("
				+"`PersistantGameObjectId` int,"
				+"`AccountId` int,"
				+"`ActiveQuiverSlot` tinyint unsigned,"
				+"`ActiveWeaponSlot` tinyint unsigned,"
				+"`BaseCharisma` int,"
				+"`BaseConstitution` int,"
				+"`BaseDexterity` int,"
				+"`BaseEmpathy` int,"
				+"`BaseIntelligence` int,"
				+"`BasePiety` int,"
				+"`BaseQuickness` int,"
				+"`BaseStrength` int,"
				+"`BindHeading` int,"
				+"`BindRegionId` int,"
				+"`BindX` int,"
				+"`BindY` int,"
				+"`BindZ` int,"
				+"`BountyPoints` bigint,"
				+"`CancelStyle` char(1) character set ascii,"
				+"`CapturedKeeps` int,"
				+"`CapturedTowers` int,"
				+"`CharacterClassId` int,"
				+"`CraftingPrimarySkill` int,"
				+"`CreationDate` datetime,"
				+"`CreationModel` int,"
				+"`CurrentTitleType` varchar(510) character set unicode,"
				+"`CustomisationStep` tinyint unsigned,"
				+"`DeathCount` tinyint unsigned,"
				+"`DeathTime` bigint,"
				+"`DisabledAbilities` varchar(510) character set unicode,"
				+"`DisabledSpells` varchar(510) character set unicode,"
				+"`EndurancePercent` tinyint unsigned,"
				+"`Experience` bigint,"
				+"`EyeColor` tinyint unsigned,"
				+"`EyeSize` tinyint unsigned,"
				+"`FaceType` tinyint unsigned,"
				+"`Gender` int,"
				+"`GuildName` varchar(510) character set unicode,"
				+"`GuildNameFlag` char(1) character set ascii,"
				+"`GuildRank` tinyint unsigned,"
				+"`HairColor` tinyint unsigned,"
				+"`HairStyle` tinyint unsigned,"
				+"`Heading` int,"
				+"`Health` int,"
				+"`IsAnonymous` char(1) character set ascii,"
				+"`IsLevelRespecUsed` char(1) character set ascii,"
				+"`IsLevelSecondStage` char(1) character set ascii,"
				+"`KillsAlbionDeathBlows` int,"
				+"`KillsAlbionPlayers` int,"
				+"`KillsAlbionSolo` int,"
				+"`KillsHiberniaDeathBlows` int,"
				+"`KillsHiberniaPlayers` int,"
				+"`KillsHiberniaSolo` int,"
				+"`KillsMidgardDeathBlows` int,"
				+"`KillsMidgardPlayers` int,"
				+"`KillsMidgardSolo` int,"
				+"`LastName` varchar(46) character set unicode,"
				+"`LastPlayed` datetime,"
				+"`Level` tinyint unsigned,"
				+"`LipSize` tinyint unsigned,"
				+"`LotNumber` int,"
				+"`Mana` int,"
				+"`MaxSpeedBase` int,"
				+"`Model` int,"
				+"`Money` bigint,"
				+"`MoodType` tinyint unsigned,"
				+"`Name` varchar(510) character set unicode,"
				+"`PlayedTime` bigint,"
				+"`Race` int,"
				+"`Realm` tinyint unsigned,"
				+"`RealmLevel` int,"
				+"`RealmPoints` bigint,"
				+"`RealmSpecialtyPoints` int,"
				+"`RegionId` int,"
				+"`RespecAmountAllSkill` int,"
				+"`RespecAmountSingleSkill` int,"
				+"`RespecBought` int,"
				+"`SafetyFlag` char(1) character set ascii,"
				+"`SerializedAbilities` varchar(510) character set unicode,"
				+"`SerializedCraftingSkills` varchar(510) character set unicode,"
				+"`SerializedFriendsList` varchar(510) character set unicode,"
				+"`SerializedSpecs` varchar(510) character set unicode,"
				+"`SerializedSpellLines` varchar(510) character set unicode,"
				+"`SkillSpecialtyPoints` int,"
				+"`SlotPosition` tinyint unsigned,"
				+"`SpellQueue` char(1) character set ascii,"
				+"`Styles` varchar(510) character set unicode,"
				+"`TaskDone` tinyint unsigned,"
				+"`TotalConstitutionLostAtDeath` int,"
				+"`UsedLevelCommand` char(1) character set ascii,"
				+"`X` int,"
				+"`Y` int,"
				+"`Z` int"
				+", primary key `PersistantGameObjectId` (`PersistantGameObjectId`)"
			);
		}

		public GamePlayerDao(MySqlState state)
		{
			if (state == null)
			{
				throw new ArgumentNullException("state");
			}
			m_state = state;
		}
	}
}
