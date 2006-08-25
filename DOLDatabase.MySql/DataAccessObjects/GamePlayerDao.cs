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
		protected static readonly string c_rowFields = "`PersistantGameObjectId`,`AccountId`,`ActiveQuiverSlot`,`ActiveWeaponSlot`,`BaseCharisma`,`BaseConstitution`,`BaseDexterity`,`BaseEmpathy`,`BaseIntelligence`,`BasePiety`,`BaseQuickness`,`BaseStrength`,`BindHeading`,`BindRegionId`,`BindX`,`BindY`,`BindZ`,`BountyPoints`,`CancelStyle`,`CapturedKeeps`,`CapturedTowers`,`CharacterClassId`,`CraftingPrimarySkill`,`CreationDate`,`CreationModel`,`CurrentTitleType`,`CustomisationStep`,`DeathCount`,`DeathTime`,`DisabledAbilities`,`DisabledSpells`,`EndurancePercent`,`Experience`,`EyeColor`,`EyeSize`,`FaceType`,`Gender`,`GuildName`,`GuildNameFlag`,`GuildRank`,`HairColor`,`HairStyle`,`Heading`,`Health`,`IsAnonymous`,`IsLevelRespecUsed`,`IsLevelSecondStage`,`KillsAlbionDeathBlows`,`KillsAlbionPlayers`,`KillsAlbionSolo`,`KillsHiberniaDeathBlows`,`KillsHiberniaPlayers`,`KillsHiberniaSolo`,`KillsMidgardDeathBlows`,`KillsMidgardPlayers`,`KillsMidgardSolo`,`LastName`,`LastPlayed`,`Level`,`LipSize`,`LotNumber`,`Mana`,`MaxSpeedBase`,`Model`,`Money`,`MoodType`,`Name`,`PlayedTime`,`Race`,`Realm`,`RealmLevel`,`RealmPoints`,`RealmSpecialtyPoints`,`RegionId`,`RespecAmountAllSkill`,`RespecAmountSingleSkill`,`RespecBought`,`SafetyFlag`,`SerializedAbilities`,`SerializedCraftingSkills`,`SerializedFriendsList`,`SerializedSpecs`,`SerializedSpellLines`,`SkillSpecialtyPoints`,`SlotPosition`,`SpellQueue`,`Styles`,`TaskDone`,`TotalConstitutionLostAtDeath`,`UsedLevelCommand`,`X`,`Y`,`Z`";
		private readonly MySqlState m_state;

		public virtual GamePlayerEntity Find(int id)
		{
			GamePlayerEntity result = new GamePlayerEntity();

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `gameplayer` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(id.ToString()) + "'",
				CommandBehavior.SingleRow,
				delegate(MySqlDataReader reader)
				{
					reader.Read();
					FillEntityWithRow(ref result, reader);
				}
			);

			return result;
		}

		public virtual IList<GamePlayerEntity> FindByAccountAndRealm(int account, byte realm)
		{
			GamePlayerEntity entity;
			List<GamePlayerEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT  " + c_rowFields + " FROM `gameplayer` WHERE `AccountId`='" + m_state.EscapeString(account.ToString()) + "' AND `Realm`='" + m_state.EscapeString(realm.ToString()) + "'",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GamePlayerEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new GamePlayerEntity();
						FillEntityWithRow(ref entity, reader);
						results.Add(entity);
					}
				}
			);

			return results;
		}

		public virtual int CountByAccountAndRealm(int account, byte realm)
		{

			return (int)m_state.ExecuteScalar(
				"SELECT  count(*) FROM `gameplayer` WHERE `AccountId`='" + m_state.EscapeString(account.ToString()) + "' AND `Realm`='" + m_state.EscapeString(realm.ToString()) + "'");

		}

		public virtual void Create(GamePlayerEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `gameplayer` VALUES (`" + obj.Id.ToString() + "`,`" + obj.Account.ToString() + "`,`" + obj.ActiveQuiverSlot.ToString() + "`,`" + obj.ActiveWeaponSlot.ToString() + "`,`" + obj.BaseCharisma.ToString() + "`,`" + obj.BaseConstitution.ToString() + "`,`" + obj.BaseDexterity.ToString() + "`,`" + obj.BaseEmpathy.ToString() + "`,`" + obj.BaseIntelligence.ToString() + "`,`" + obj.BasePiety.ToString() + "`,`" + obj.BaseQuickness.ToString() + "`,`" + obj.BaseStrength.ToString() + "`,`" + obj.BindHeading.ToString() + "`,`" + obj.BindRegionId.ToString() + "`,`" + obj.BindX.ToString() + "`,`" + obj.BindY.ToString() + "`,`" + obj.BindZ.ToString() + "`,`" + obj.BountyPoints.ToString() + "`,`" + obj.CancelStyle.ToString() + "`,`" + obj.CapturedKeeps.ToString() + "`,`" + obj.CapturedTowers.ToString() + "`,`" + obj.CharacterClassId.ToString() + "`,`" + obj.CraftingPrimarySkill.ToString() + "`,`" + obj.CreationDate.ToString() + "`,`" + obj.CreationModel.ToString() + "`,`" + obj.CurrentTitleType.ToString() + "`,`" + obj.CustomisationStep.ToString() + "`,`" + obj.DeathCount.ToString() + "`,`" + obj.DeathTime.ToString() + "`,`" + obj.DisabledAbilities.ToString() + "`,`" + obj.DisabledSpells.ToString() + "`,`" + obj.EndurancePercent.ToString() + "`,`" + obj.Experience.ToString() + "`,`" + obj.EyeColor.ToString() + "`,`" + obj.EyeSize.ToString() + "`,`" + obj.FaceType.ToString() + "`,`" + obj.Gender.ToString() + "`,`" + obj.GuildName.ToString() + "`,`" + obj.GuildNameFlag.ToString() + "`,`" + obj.GuildRank.ToString() + "`,`" + obj.HairColor.ToString() + "`,`" + obj.HairStyle.ToString() + "`,`" + obj.Heading.ToString() + "`,`" + obj.Health.ToString() + "`,`" + obj.IsAnonymous.ToString() + "`,`" + obj.IsLevelRespecUsed.ToString() + "`,`" + obj.IsLevelSecondStage.ToString() + "`,`" + obj.KillsAlbionDeathBlows.ToString() + "`,`" + obj.KillsAlbionPlayers.ToString() + "`,`" + obj.KillsAlbionSolo.ToString() + "`,`" + obj.KillsHiberniaDeathBlows.ToString() + "`,`" + obj.KillsHiberniaPlayers.ToString() + "`,`" + obj.KillsHiberniaSolo.ToString() + "`,`" + obj.KillsMidgardDeathBlows.ToString() + "`,`" + obj.KillsMidgardPlayers.ToString() + "`,`" + obj.KillsMidgardSolo.ToString() + "`,`" + obj.LastName.ToString() + "`,`" + obj.LastPlayed.ToString() + "`,`" + obj.Level.ToString() + "`,`" + obj.LipSize.ToString() + "`,`" + obj.LotNumber.ToString() + "`,`" + obj.Mana.ToString() + "`,`" + obj.MaxSpeedBase.ToString() + "`,`" + obj.Model.ToString() + "`,`" + obj.Money.ToString() + "`,`" + obj.MoodType.ToString() + "`,`" + obj.Name.ToString() + "`,`" + obj.PlayedTime.ToString() + "`,`" + obj.Race.ToString() + "`,`" + obj.Realm.ToString() + "`,`" + obj.RealmLevel.ToString() + "`,`" + obj.RealmPoints.ToString() + "`,`" + obj.RealmSpecialtyPoints.ToString() + "`,`" + obj.RegionId.ToString() + "`,`" + obj.RespecAmountAllSkill.ToString() + "`,`" + obj.RespecAmountSingleSkill.ToString() + "`,`" + obj.RespecBought.ToString() + "`,`" + obj.SafetyFlag.ToString() + "`,`" + obj.SerializedAbilities.ToString() + "`,`" + obj.SerializedCraftingSkills.ToString() + "`,`" + obj.SerializedFriendsList.ToString() + "`,`" + obj.SerializedSpecs.ToString() + "`,`" + obj.SerializedSpellLines.ToString() + "`,`" + obj.SkillSpecialtyPoints.ToString() + "`,`" + obj.SlotPosition.ToString() + "`,`" + obj.SpellQueue.ToString() + "`,`" + obj.Styles.ToString() + "`,`" + obj.TaskDone.ToString() + "`,`" + obj.TotalConstitutionLostAtDeath.ToString() + "`,`" + obj.UsedLevelCommand.ToString() + "`,`" + obj.X.ToString() + "`,`" + obj.Y.ToString() + "`,`" + obj.Z.ToString() + "`);");
		}

		public virtual void Update(GamePlayerEntity obj)
		{
			m_state.ExecuteNonQuery(
				"UPDATE `gameplayer` SET `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "', `AccountId`='" + m_state.EscapeString(obj.Account.ToString()) + "', `ActiveQuiverSlot`='" + m_state.EscapeString(obj.ActiveQuiverSlot.ToString()) + "', `ActiveWeaponSlot`='" + m_state.EscapeString(obj.ActiveWeaponSlot.ToString()) + "', `BaseCharisma`='" + m_state.EscapeString(obj.BaseCharisma.ToString()) + "', `BaseConstitution`='" + m_state.EscapeString(obj.BaseConstitution.ToString()) + "', `BaseDexterity`='" + m_state.EscapeString(obj.BaseDexterity.ToString()) + "', `BaseEmpathy`='" + m_state.EscapeString(obj.BaseEmpathy.ToString()) + "', `BaseIntelligence`='" + m_state.EscapeString(obj.BaseIntelligence.ToString()) + "', `BasePiety`='" + m_state.EscapeString(obj.BasePiety.ToString()) + "', `BaseQuickness`='" + m_state.EscapeString(obj.BaseQuickness.ToString()) + "', `BaseStrength`='" + m_state.EscapeString(obj.BaseStrength.ToString()) + "', `BindHeading`='" + m_state.EscapeString(obj.BindHeading.ToString()) + "', `BindRegionId`='" + m_state.EscapeString(obj.BindRegionId.ToString()) + "', `BindX`='" + m_state.EscapeString(obj.BindX.ToString()) + "', `BindY`='" + m_state.EscapeString(obj.BindY.ToString()) + "', `BindZ`='" + m_state.EscapeString(obj.BindZ.ToString()) + "', `BountyPoints`='" + m_state.EscapeString(obj.BountyPoints.ToString()) + "', `CancelStyle`='" + m_state.EscapeString(obj.CancelStyle.ToString()) + "', `CapturedKeeps`='" + m_state.EscapeString(obj.CapturedKeeps.ToString()) + "', `CapturedTowers`='" + m_state.EscapeString(obj.CapturedTowers.ToString()) + "', `CharacterClassId`='" + m_state.EscapeString(obj.CharacterClassId.ToString()) + "', `CraftingPrimarySkill`='" + m_state.EscapeString(obj.CraftingPrimarySkill.ToString()) + "', `CreationDate`='" + m_state.EscapeString(obj.CreationDate.ToString()) + "', `CreationModel`='" + m_state.EscapeString(obj.CreationModel.ToString()) + "', `CurrentTitleType`='" + m_state.EscapeString(obj.CurrentTitleType.ToString()) + "', `CustomisationStep`='" + m_state.EscapeString(obj.CustomisationStep.ToString()) + "', `DeathCount`='" + m_state.EscapeString(obj.DeathCount.ToString()) + "', `DeathTime`='" + m_state.EscapeString(obj.DeathTime.ToString()) + "', `DisabledAbilities`='" + m_state.EscapeString(obj.DisabledAbilities.ToString()) + "', `DisabledSpells`='" + m_state.EscapeString(obj.DisabledSpells.ToString()) + "', `EndurancePercent`='" + m_state.EscapeString(obj.EndurancePercent.ToString()) + "', `Experience`='" + m_state.EscapeString(obj.Experience.ToString()) + "', `EyeColor`='" + m_state.EscapeString(obj.EyeColor.ToString()) + "', `EyeSize`='" + m_state.EscapeString(obj.EyeSize.ToString()) + "', `FaceType`='" + m_state.EscapeString(obj.FaceType.ToString()) + "', `Gender`='" + m_state.EscapeString(obj.Gender.ToString()) + "', `GuildName`='" + m_state.EscapeString(obj.GuildName.ToString()) + "', `GuildNameFlag`='" + m_state.EscapeString(obj.GuildNameFlag.ToString()) + "', `GuildRank`='" + m_state.EscapeString(obj.GuildRank.ToString()) + "', `HairColor`='" + m_state.EscapeString(obj.HairColor.ToString()) + "', `HairStyle`='" + m_state.EscapeString(obj.HairStyle.ToString()) + "', `Heading`='" + m_state.EscapeString(obj.Heading.ToString()) + "', `Health`='" + m_state.EscapeString(obj.Health.ToString()) + "', `IsAnonymous`='" + m_state.EscapeString(obj.IsAnonymous.ToString()) + "', `IsLevelRespecUsed`='" + m_state.EscapeString(obj.IsLevelRespecUsed.ToString()) + "', `IsLevelSecondStage`='" + m_state.EscapeString(obj.IsLevelSecondStage.ToString()) + "', `KillsAlbionDeathBlows`='" + m_state.EscapeString(obj.KillsAlbionDeathBlows.ToString()) + "', `KillsAlbionPlayers`='" + m_state.EscapeString(obj.KillsAlbionPlayers.ToString()) + "', `KillsAlbionSolo`='" + m_state.EscapeString(obj.KillsAlbionSolo.ToString()) + "', `KillsHiberniaDeathBlows`='" + m_state.EscapeString(obj.KillsHiberniaDeathBlows.ToString()) + "', `KillsHiberniaPlayers`='" + m_state.EscapeString(obj.KillsHiberniaPlayers.ToString()) + "', `KillsHiberniaSolo`='" + m_state.EscapeString(obj.KillsHiberniaSolo.ToString()) + "', `KillsMidgardDeathBlows`='" + m_state.EscapeString(obj.KillsMidgardDeathBlows.ToString()) + "', `KillsMidgardPlayers`='" + m_state.EscapeString(obj.KillsMidgardPlayers.ToString()) + "', `KillsMidgardSolo`='" + m_state.EscapeString(obj.KillsMidgardSolo.ToString()) + "', `LastName`='" + m_state.EscapeString(obj.LastName.ToString()) + "', `LastPlayed`='" + m_state.EscapeString(obj.LastPlayed.ToString()) + "', `Level`='" + m_state.EscapeString(obj.Level.ToString()) + "', `LipSize`='" + m_state.EscapeString(obj.LipSize.ToString()) + "', `LotNumber`='" + m_state.EscapeString(obj.LotNumber.ToString()) + "', `Mana`='" + m_state.EscapeString(obj.Mana.ToString()) + "', `MaxSpeedBase`='" + m_state.EscapeString(obj.MaxSpeedBase.ToString()) + "', `Model`='" + m_state.EscapeString(obj.Model.ToString()) + "', `Money`='" + m_state.EscapeString(obj.Money.ToString()) + "', `MoodType`='" + m_state.EscapeString(obj.MoodType.ToString()) + "', `Name`='" + m_state.EscapeString(obj.Name.ToString()) + "', `PlayedTime`='" + m_state.EscapeString(obj.PlayedTime.ToString()) + "', `Race`='" + m_state.EscapeString(obj.Race.ToString()) + "', `Realm`='" + m_state.EscapeString(obj.Realm.ToString()) + "', `RealmLevel`='" + m_state.EscapeString(obj.RealmLevel.ToString()) + "', `RealmPoints`='" + m_state.EscapeString(obj.RealmPoints.ToString()) + "', `RealmSpecialtyPoints`='" + m_state.EscapeString(obj.RealmSpecialtyPoints.ToString()) + "', `RegionId`='" + m_state.EscapeString(obj.RegionId.ToString()) + "', `RespecAmountAllSkill`='" + m_state.EscapeString(obj.RespecAmountAllSkill.ToString()) + "', `RespecAmountSingleSkill`='" + m_state.EscapeString(obj.RespecAmountSingleSkill.ToString()) + "', `RespecBought`='" + m_state.EscapeString(obj.RespecBought.ToString()) + "', `SafetyFlag`='" + m_state.EscapeString(obj.SafetyFlag.ToString()) + "', `SerializedAbilities`='" + m_state.EscapeString(obj.SerializedAbilities.ToString()) + "', `SerializedCraftingSkills`='" + m_state.EscapeString(obj.SerializedCraftingSkills.ToString()) + "', `SerializedFriendsList`='" + m_state.EscapeString(obj.SerializedFriendsList.ToString()) + "', `SerializedSpecs`='" + m_state.EscapeString(obj.SerializedSpecs.ToString()) + "', `SerializedSpellLines`='" + m_state.EscapeString(obj.SerializedSpellLines.ToString()) + "', `SkillSpecialtyPoints`='" + m_state.EscapeString(obj.SkillSpecialtyPoints.ToString()) + "', `SlotPosition`='" + m_state.EscapeString(obj.SlotPosition.ToString()) + "', `SpellQueue`='" + m_state.EscapeString(obj.SpellQueue.ToString()) + "', `Styles`='" + m_state.EscapeString(obj.Styles.ToString()) + "', `TaskDone`='" + m_state.EscapeString(obj.TaskDone.ToString()) + "', `TotalConstitutionLostAtDeath`='" + m_state.EscapeString(obj.TotalConstitutionLostAtDeath.ToString()) + "', `UsedLevelCommand`='" + m_state.EscapeString(obj.UsedLevelCommand.ToString()) + "', `X`='" + m_state.EscapeString(obj.X.ToString()) + "', `Y`='" + m_state.EscapeString(obj.Y.ToString()) + "', `Z`='" + m_state.EscapeString(obj.Z.ToString()) + "' WHERE `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void Delete(GamePlayerEntity obj)
		{
			m_state.ExecuteNonQuery(
				"DELETE FROM `gameplayer` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(obj.Id.ToString()) + "'");
		}

		public virtual void SaveAll()
		{
			// not used by this implementation
		}

		public virtual IList<GamePlayerEntity> SelectAll()
		{
			GamePlayerEntity entity;
			List<GamePlayerEntity> results = null;

			m_state.ExecuteQuery(
				"SELECT " + c_rowFields + " FROM `gameplayer`",
				CommandBehavior.Default,
				delegate(MySqlDataReader reader)
				{
					results = new List<GamePlayerEntity>(reader.FieldCount);
					while (reader.Read())
					{
						entity = new GamePlayerEntity();
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
			"SELECT COUNT(*) FROM `gameplayer`");

		}

		protected virtual void FillEntityWithRow(ref GamePlayerEntity entity, MySqlDataReader reader)
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
			entity.CancelStyle = reader.GetBoolean(18);
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
			entity.GuildNameFlag = reader.GetBoolean(38);
			entity.GuildRank = reader.GetByte(39);
			entity.HairColor = reader.GetByte(40);
			entity.HairStyle = reader.GetByte(41);
			entity.Heading = reader.GetInt32(42);
			entity.Health = reader.GetInt32(43);
			entity.IsAnonymous = reader.GetBoolean(44);
			entity.IsLevelRespecUsed = reader.GetBoolean(45);
			entity.IsLevelSecondStage = reader.GetBoolean(46);
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
			entity.SafetyFlag = reader.GetBoolean(77);
			entity.SerializedAbilities = reader.GetString(78);
			entity.SerializedCraftingSkills = reader.GetString(79);
			entity.SerializedFriendsList = reader.GetString(80);
			entity.SerializedSpecs = reader.GetString(81);
			entity.SerializedSpellLines = reader.GetString(82);
			entity.SkillSpecialtyPoints = reader.GetInt32(83);
			entity.SlotPosition = reader.GetByte(84);
			entity.SpellQueue = reader.GetBoolean(85);
			entity.Styles = reader.GetString(86);
			entity.TaskDone = reader.GetByte(87);
			entity.TotalConstitutionLostAtDeath = reader.GetInt32(88);
			entity.UsedLevelCommand = reader.GetBoolean(89);
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
				+"`CancelStyle` bit,"
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
				+"`GuildNameFlag` bit,"
				+"`GuildRank` tinyint unsigned,"
				+"`HairColor` tinyint unsigned,"
				+"`HairStyle` tinyint unsigned,"
				+"`Heading` int,"
				+"`Health` int,"
				+"`IsAnonymous` bit,"
				+"`IsLevelRespecUsed` bit,"
				+"`IsLevelSecondStage` bit,"
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
				+"`SafetyFlag` bit,"
				+"`SerializedAbilities` varchar(510) character set unicode,"
				+"`SerializedCraftingSkills` varchar(510) character set unicode,"
				+"`SerializedFriendsList` varchar(510) character set unicode,"
				+"`SerializedSpecs` varchar(510) character set unicode,"
				+"`SerializedSpellLines` varchar(510) character set unicode,"
				+"`SkillSpecialtyPoints` int,"
				+"`SlotPosition` tinyint unsigned,"
				+"`SpellQueue` bit,"
				+"`Styles` varchar(510) character set unicode,"
				+"`TaskDone` tinyint unsigned,"
				+"`TotalConstitutionLostAtDeath` int,"
				+"`UsedLevelCommand` bit,"
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
