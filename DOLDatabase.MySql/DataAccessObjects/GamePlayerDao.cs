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
		protected readonly MySqlState m_state;

		public virtual GamePlayerEntity Find(int id)
		{
			GamePlayerEntity result = new GamePlayerEntity();
			string command = "SELECT " + c_rowFields + " FROM `gameplayer` WHERE `PersistantGameObjectId`='" + m_state.EscapeString(id.ToString()) + "'";

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

		public virtual long CountByAccountAndRealm(int account, byte realm)
		{

			return (long) m_state.ExecuteScalar(
				"SELECT  count(*) FROM `gameplayer` WHERE `AccountId`='" + m_state.EscapeString(account.ToString()) + "' AND `Realm`='" + m_state.EscapeString(realm.ToString()) + "'");

		}

		public virtual void Create(ref GamePlayerEntity obj)
		{
			m_state.ExecuteNonQuery(
				"INSERT INTO `gameplayer` VALUES ('" + m_state.EscapeString(obj.Id.ToString()) + "','" + m_state.EscapeString(obj.Account.ToString()) + "','" + m_state.EscapeString(obj.ActiveQuiverSlot.ToString()) + "','" + m_state.EscapeString(obj.ActiveWeaponSlot.ToString()) + "','" + m_state.EscapeString(obj.BaseCharisma.ToString()) + "','" + m_state.EscapeString(obj.BaseConstitution.ToString()) + "','" + m_state.EscapeString(obj.BaseDexterity.ToString()) + "','" + m_state.EscapeString(obj.BaseEmpathy.ToString()) + "','" + m_state.EscapeString(obj.BaseIntelligence.ToString()) + "','" + m_state.EscapeString(obj.BasePiety.ToString()) + "','" + m_state.EscapeString(obj.BaseQuickness.ToString()) + "','" + m_state.EscapeString(obj.BaseStrength.ToString()) + "','" + m_state.EscapeString(obj.BindHeading.ToString()) + "','" + m_state.EscapeString(obj.BindRegionId.ToString()) + "','" + m_state.EscapeString(obj.BindX.ToString()) + "','" + m_state.EscapeString(obj.BindY.ToString()) + "','" + m_state.EscapeString(obj.BindZ.ToString()) + "','" + m_state.EscapeString(obj.BountyPoints.ToString()) + "','" + m_state.EscapeString(obj.CancelStyle.ToString()) + "','" + m_state.EscapeString(obj.CapturedKeeps.ToString()) + "','" + m_state.EscapeString(obj.CapturedTowers.ToString()) + "','" + m_state.EscapeString(obj.CharacterClassId.ToString()) + "','" + m_state.EscapeString(obj.CraftingPrimarySkill.ToString()) + "','" + m_state.EscapeString(obj.CreationDate.ToString()) + "','" + m_state.EscapeString(obj.CreationModel.ToString()) + "','" + m_state.EscapeString(obj.CurrentTitleType.ToString()) + "','" + m_state.EscapeString(obj.CustomisationStep.ToString()) + "','" + m_state.EscapeString(obj.DeathCount.ToString()) + "','" + m_state.EscapeString(obj.DeathTime.ToString()) + "','" + m_state.EscapeString(obj.DisabledAbilities.ToString()) + "','" + m_state.EscapeString(obj.DisabledSpells.ToString()) + "','" + m_state.EscapeString(obj.EndurancePercent.ToString()) + "','" + m_state.EscapeString(obj.Experience.ToString()) + "','" + m_state.EscapeString(obj.EyeColor.ToString()) + "','" + m_state.EscapeString(obj.EyeSize.ToString()) + "','" + m_state.EscapeString(obj.FaceType.ToString()) + "','" + m_state.EscapeString(obj.Gender.ToString()) + "','" + m_state.EscapeString(obj.GuildName.ToString()) + "','" + m_state.EscapeString(obj.GuildNameFlag.ToString()) + "','" + m_state.EscapeString(obj.GuildRank.ToString()) + "','" + m_state.EscapeString(obj.HairColor.ToString()) + "','" + m_state.EscapeString(obj.HairStyle.ToString()) + "','" + m_state.EscapeString(obj.Heading.ToString()) + "','" + m_state.EscapeString(obj.Health.ToString()) + "','" + m_state.EscapeString(obj.IsAnonymous.ToString()) + "','" + m_state.EscapeString(obj.IsLevelRespecUsed.ToString()) + "','" + m_state.EscapeString(obj.IsLevelSecondStage.ToString()) + "','" + m_state.EscapeString(obj.KillsAlbionDeathBlows.ToString()) + "','" + m_state.EscapeString(obj.KillsAlbionPlayers.ToString()) + "','" + m_state.EscapeString(obj.KillsAlbionSolo.ToString()) + "','" + m_state.EscapeString(obj.KillsHiberniaDeathBlows.ToString()) + "','" + m_state.EscapeString(obj.KillsHiberniaPlayers.ToString()) + "','" + m_state.EscapeString(obj.KillsHiberniaSolo.ToString()) + "','" + m_state.EscapeString(obj.KillsMidgardDeathBlows.ToString()) + "','" + m_state.EscapeString(obj.KillsMidgardPlayers.ToString()) + "','" + m_state.EscapeString(obj.KillsMidgardSolo.ToString()) + "','" + m_state.EscapeString(obj.LastName.ToString()) + "','" + m_state.EscapeString(obj.LastPlayed.ToString()) + "','" + m_state.EscapeString(obj.Level.ToString()) + "','" + m_state.EscapeString(obj.LipSize.ToString()) + "','" + m_state.EscapeString(obj.LotNumber.ToString()) + "','" + m_state.EscapeString(obj.Mana.ToString()) + "','" + m_state.EscapeString(obj.MaxSpeedBase.ToString()) + "','" + m_state.EscapeString(obj.Model.ToString()) + "','" + m_state.EscapeString(obj.Money.ToString()) + "','" + m_state.EscapeString(obj.MoodType.ToString()) + "','" + m_state.EscapeString(obj.Name.ToString()) + "','" + m_state.EscapeString(obj.PlayedTime.ToString()) + "','" + m_state.EscapeString(obj.Race.ToString()) + "','" + m_state.EscapeString(obj.Realm.ToString()) + "','" + m_state.EscapeString(obj.RealmLevel.ToString()) + "','" + m_state.EscapeString(obj.RealmPoints.ToString()) + "','" + m_state.EscapeString(obj.RealmSpecialtyPoints.ToString()) + "','" + m_state.EscapeString(obj.RegionId.ToString()) + "','" + m_state.EscapeString(obj.RespecAmountAllSkill.ToString()) + "','" + m_state.EscapeString(obj.RespecAmountSingleSkill.ToString()) + "','" + m_state.EscapeString(obj.RespecBought.ToString()) + "','" + m_state.EscapeString(obj.SafetyFlag.ToString()) + "','" + m_state.EscapeString(obj.SerializedAbilities.ToString()) + "','" + m_state.EscapeString(obj.SerializedCraftingSkills.ToString()) + "','" + m_state.EscapeString(obj.SerializedFriendsList.ToString()) + "','" + m_state.EscapeString(obj.SerializedSpecs.ToString()) + "','" + m_state.EscapeString(obj.SerializedSpellLines.ToString()) + "','" + m_state.EscapeString(obj.SkillSpecialtyPoints.ToString()) + "','" + m_state.EscapeString(obj.SlotPosition.ToString()) + "','" + m_state.EscapeString(obj.SpellQueue.ToString()) + "','" + m_state.EscapeString(obj.Styles.ToString()) + "','" + m_state.EscapeString(obj.TaskDone.ToString()) + "','" + m_state.EscapeString(obj.TotalConstitutionLostAtDeath.ToString()) + "','" + m_state.EscapeString(obj.UsedLevelCommand.ToString()) + "','" + m_state.EscapeString(obj.X.ToString()) + "','" + m_state.EscapeString(obj.Y.ToString()) + "','" + m_state.EscapeString(obj.Z.ToString()) + "');");
			object insertedId = m_state.ExecuteScalar("SELECT LAST_INSERT_ID();");
			obj.Id = (int) (long) insertedId;
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
					results = new List<GamePlayerEntity>();
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

		public virtual long CountAll()
		{
			return (long) m_state.ExecuteScalar("SELECT COUNT(*) FROM `gameplayer`");
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
			m_state.ExecuteNonQuery("CREATE TABLE IF NOT EXISTS `gameplayer` ("
				+"`PersistantGameObjectId` int NOT NULL auto_increment,"
				+"`AccountId` int NOT NULL,"
				+"`ActiveQuiverSlot` tinyint unsigned NOT NULL,"
				+"`ActiveWeaponSlot` tinyint unsigned NOT NULL,"
				+"`BaseCharisma` int NOT NULL,"
				+"`BaseConstitution` int NOT NULL,"
				+"`BaseDexterity` int NOT NULL,"
				+"`BaseEmpathy` int NOT NULL,"
				+"`BaseIntelligence` int NOT NULL,"
				+"`BasePiety` int NOT NULL,"
				+"`BaseQuickness` int NOT NULL,"
				+"`BaseStrength` int NOT NULL,"
				+"`BindHeading` int NOT NULL,"
				+"`BindRegionId` int NOT NULL,"
				+"`BindX` int NOT NULL,"
				+"`BindY` int NOT NULL,"
				+"`BindZ` int NOT NULL,"
				+"`BountyPoints` bigint NOT NULL,"
				+"`CancelStyle` bit NOT NULL,"
				+"`CapturedKeeps` int NOT NULL,"
				+"`CapturedTowers` int NOT NULL,"
				+"`CharacterClassId` int NOT NULL,"
				+"`CraftingPrimarySkill` int NOT NULL,"
				+"`CreationDate` datetime NOT NULL,"
				+"`CreationModel` int NOT NULL,"
				+"`CurrentTitleType` char(255) character set latin1,"
				+"`CustomisationStep` tinyint unsigned NOT NULL,"
				+"`DeathCount` tinyint unsigned NOT NULL,"
				+"`DeathTime` bigint NOT NULL,"
				+"`DisabledAbilities` char(255) character set latin1,"
				+"`DisabledSpells` char(255) character set latin1,"
				+"`EndurancePercent` tinyint unsigned NOT NULL,"
				+"`Experience` bigint NOT NULL,"
				+"`EyeColor` tinyint unsigned NOT NULL,"
				+"`EyeSize` tinyint unsigned NOT NULL,"
				+"`FaceType` tinyint unsigned NOT NULL,"
				+"`Gender` int NOT NULL,"
				+"`GuildName` char(255) character set latin1 NOT NULL,"
				+"`GuildNameFlag` bit NOT NULL,"
				+"`GuildRank` tinyint unsigned NOT NULL,"
				+"`HairColor` tinyint unsigned NOT NULL,"
				+"`HairStyle` tinyint unsigned NOT NULL,"
				+"`Heading` int NOT NULL,"
				+"`Health` int NOT NULL,"
				+"`IsAnonymous` bit NOT NULL,"
				+"`IsLevelRespecUsed` bit NOT NULL,"
				+"`IsLevelSecondStage` bit NOT NULL,"
				+"`KillsAlbionDeathBlows` int NOT NULL,"
				+"`KillsAlbionPlayers` int NOT NULL,"
				+"`KillsAlbionSolo` int NOT NULL,"
				+"`KillsHiberniaDeathBlows` int NOT NULL,"
				+"`KillsHiberniaPlayers` int NOT NULL,"
				+"`KillsHiberniaSolo` int NOT NULL,"
				+"`KillsMidgardDeathBlows` int NOT NULL,"
				+"`KillsMidgardPlayers` int NOT NULL,"
				+"`KillsMidgardSolo` int NOT NULL,"
				+"`LastName` char(46) character set latin1,"
				+"`LastPlayed` datetime NOT NULL,"
				+"`Level` tinyint unsigned NOT NULL,"
				+"`LipSize` tinyint unsigned NOT NULL,"
				+"`LotNumber` int NOT NULL,"
				+"`Mana` int NOT NULL,"
				+"`MaxSpeedBase` int NOT NULL,"
				+"`Model` int NOT NULL,"
				+"`Money` bigint NOT NULL,"
				+"`MoodType` tinyint unsigned NOT NULL,"
				+"`Name` char(255) character set latin1 NOT NULL,"
				+"`PlayedTime` bigint NOT NULL,"
				+"`Race` int NOT NULL,"
				+"`Realm` tinyint unsigned NOT NULL,"
				+"`RealmLevel` int NOT NULL,"
				+"`RealmPoints` bigint NOT NULL,"
				+"`RealmSpecialtyPoints` int NOT NULL,"
				+"`RegionId` int NOT NULL,"
				+"`RespecAmountAllSkill` int NOT NULL,"
				+"`RespecAmountSingleSkill` int NOT NULL,"
				+"`RespecBought` int NOT NULL,"
				+"`SafetyFlag` bit NOT NULL,"
				+"`SerializedAbilities` char(255) character set latin1,"
				+"`SerializedCraftingSkills` char(255) character set latin1,"
				+"`SerializedFriendsList` char(255) character set latin1,"
				+"`SerializedSpecs` char(255) character set latin1,"
				+"`SerializedSpellLines` char(255) character set latin1,"
				+"`SkillSpecialtyPoints` int NOT NULL,"
				+"`SlotPosition` tinyint unsigned NOT NULL,"
				+"`SpellQueue` bit NOT NULL,"
				+"`Styles` char(255) character set latin1,"
				+"`TaskDone` tinyint unsigned NOT NULL,"
				+"`TotalConstitutionLostAtDeath` int NOT NULL,"
				+"`UsedLevelCommand` bit NOT NULL,"
				+"`X` int NOT NULL,"
				+"`Y` int NOT NULL,"
				+"`Z` int NOT NULL"
				+", primary key `PersistantGameObjectId` (`PersistantGameObjectId`)"
				+")"
			);
			m_state.ExecuteNonQuery("OPTIMIZE TABLE `gameplayer`");
			return null;
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
