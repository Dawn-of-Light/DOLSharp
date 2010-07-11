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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL
{
	namespace Database
	{
		[DataTable(TableName = "DOLCharactersBackup")]
		public class DOLCharactersBackup : DOLCharacters
		{
			string m_dolCharactersID = "";
			private DateTime m_deleteDate;

			public DOLCharactersBackup()
				: base()
			{
				m_deleteDate = DateTime.Now;
			}

			public DOLCharactersBackup(DOLCharacters character)
				: base()
			{
				DOLCharacters_ID = character.ObjectId;
				DeleteDate = DateTime.Now;

				this.AccountName = character.AccountName;
				this.AccountSlot = character.AccountSlot;
				this.ActiveWeaponSlot = character.ActiveWeaponSlot;
				this.Advisor = character.Advisor;
				this.Autoloot = character.Autoloot;
				this.BindHeading = character.BindHeading;
				this.BindHouseHeading = character.BindHouseHeading;
				this.BindHouseRegion = character.BindHouseRegion;
				this.BindHouseXpos = character.BindHouseXpos;
				this.BindHouseYpos = character.BindHouseYpos;
				this.BindHouseZpos = character.BindHouseZpos;
				this.BindRegion = character.BindRegion;
				this.BindXpos = character.BindXpos;
				this.BindYpos = character.BindYpos;
				this.BindZpos = character.BindZpos;
				this.BountyPoints = character.BountyPoints;
				this.CancelStyle = character.CancelStyle;
				this.CapturedKeeps = character.CapturedKeeps;
				this.CapturedRelics = character.CapturedRelics;
				this.CapturedTowers = character.CapturedTowers;
				this.Champion = character.Champion;
				this.ChampionExperience = character.ChampionExperience;
				this.ChampionLevel = character.ChampionLevel;
				this.ChampionSpecialtyPoints = character.ChampionSpecialtyPoints;
				this.ChampionSpells = character.ChampionSpells;
				this.Charisma = character.Charisma;
				this.Class = character.Class;
				this.Concentration = character.Concentration;
				this.ConLostAtDeath = character.ConLostAtDeath;
				this.Constitution = character.Constitution;
				this.Copper = character.Copper;
				this.CraftingPrimarySkill = character.CraftingPrimarySkill;
				this.CreationDate = character.CreationDate;
				this.CreationModel = character.CreationModel;
				this.CurrentModel = character.CurrentModel;
				this.CurrentTitleType = character.CurrentTitleType;
				this.CustomisationStep = character.CustomisationStep;
				this.DeathCount = character.DeathCount;
				this.DeathsPvP = character.DeathsPvP;
				this.DeathTime = character.DeathTime;
				this.Dexterity = character.Dexterity;
				this.Direction = character.Direction;
				this.DisabledAbilities = character.DisabledAbilities;
				this.DisabledSpells = character.DisabledSpells;
				this.Empathy = character.Empathy;
				this.Endurance = character.Endurance;
				this.Experience = character.Experience;
				this.EyeColor = character.EyeColor;
				this.EyeSize = character.EyeSize;
				this.FaceType = character.FaceType;
				this.FlagClassName = character.FlagClassName;
				this.GainRP = character.GainRP;
				this.GainXP = character.GainXP;
				this.Gender = character.Gender;
				this.Gold = character.Gold;
				this.GravestoneRegion = character.GravestoneRegion;
				this.GuildID = character.GuildID;
				this.GuildNote = character.GuildNote;
				this.GuildRank = character.GuildRank;
				this.HairColor = character.HairColor;
				this.HairStyle = character.HairStyle;
				this.HasGravestone = character.HasGravestone;
				this.Health = character.Health;
				this.Intelligence = character.Intelligence;
				this.IsAnonymous = character.IsAnonymous;
				this.IsCloakHoodUp = character.IsCloakHoodUp;
				this.IsCloakInvisible = character.IsCloakInvisible;
				this.IsHelmInvisible = character.IsHelmInvisible;
				this.IsLevelRespecUsed = character.IsLevelRespecUsed;
				this.IsLevelSecondStage = character.IsLevelSecondStage;
				this.KillsAlbionDeathBlows = character.KillsAlbionDeathBlows;
				this.KillsAlbionPlayers = character.KillsAlbionPlayers;
				this.KillsAlbionSolo = character.KillsAlbionSolo;
				this.KillsDragon = character.KillsDragon;
				this.KillsEpicBoss = character.KillsEpicBoss;
				this.KillsHiberniaDeathBlows = character.KillsHiberniaDeathBlows;
				this.KillsHiberniaPlayers = character.KillsHiberniaPlayers;
				this.KillsHiberniaSolo = character.KillsHiberniaSolo;
				this.KillsLegion = character.KillsLegion;
				this.KillsMidgardDeathBlows = character.KillsMidgardDeathBlows;
				this.KillsMidgardPlayers = character.KillsMidgardPlayers;
				this.KillsMidgardSolo = character.KillsMidgardSolo;
				this.LastFreeLevel = character.LastFreeLevel;
				this.LastFreeLeveled = character.LastFreeLeveled;
				this.LastName = character.LastName;
				this.LastPlayed = character.LastPlayed;
				this.Level = character.Level;
				this.LipSize = character.LipSize;
				this.Mana = character.Mana;
				this.MaxEndurance = character.MaxEndurance;
				this.MaxSpeed = character.MaxSpeed;
				this.Mithril = character.Mithril;
				this.ML = character.ML;
				this.MLExperience = character.MLExperience;
				this.MLGranted = character.MLGranted;
				this.MLLevel = character.MLLevel;
				this.MoodType = character.MoodType;
				this.Name = character.Name;
				this.NoHelp = character.NoHelp;
				this.Piety = character.Piety;
				this.Platinum = character.Platinum;
				this.PlayedTime = character.PlayedTime;
				this.Quickness = character.Quickness;
				this.Race = character.Race;
				this.Realm = character.Realm;
				this.RealmLevel = character.RealmLevel;
				this.RealmPoints = character.RealmPoints;
				this.RealmSpecialtyPoints = character.RealmSpecialtyPoints;
				this.Region = character.Region;
				this.RespecAmountAllSkill = character.RespecAmountAllSkill;
				this.RespecAmountChampionSkill = character.RespecAmountChampionSkill;
				this.RespecAmountDOL = character.RespecAmountDOL;
				this.RespecAmountRealmSkill = character.RespecAmountRealmSkill;
				this.RespecAmountSingleSkill = character.RespecAmountSingleSkill;
				this.RespecBought = character.RespecBought;
				this.RPFlag = character.RPFlag;
				this.SafetyFlag = character.SafetyFlag;
				this.SerializedAbilities = character.SerializedAbilities;
				this.SerializedCraftingSkills = character.SerializedCraftingSkills;
				this.SerializedFriendsList = character.SerializedFriendsList;
				this.SerializedIgnoreList = character.SerializedIgnoreList;
				this.SerializedRealmAbilities = character.SerializedRealmAbilities;
				this.SerializedSpecs = character.SerializedSpecs;
				this.SerializedSpellLines = character.SerializedSpellLines;
				this.ShowGuildLogins = character.ShowGuildLogins;
				this.ShowXFireInfo = character.ShowXFireInfo;
				this.Silver = character.Silver;
				this.SkillSpecialtyPoints = character.SkillSpecialtyPoints;
				this.SpellQueue = character.SpellQueue;
				this.Strength = character.Strength;
				this.UsedLevelCommand = character.UsedLevelCommand;
				this.Xpos = character.Xpos;
				this.Ypos = character.Ypos;
				this.Zpos = character.Zpos;

			}

			/// <summary>
			/// The old character ID
			/// </summary>
			[DataElement(AllowDbNull = false, Varchar=255)]
			public string DOLCharacters_ID
			{
				get
				{
					return m_dolCharactersID;
				}
				set
				{
					Dirty = true;
					m_dolCharactersID = value;
				}
			}

			/// <summary>
			/// The deletion date of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public DateTime DeleteDate
			{
				get
				{
					return m_deleteDate;
				}
				set
				{
					Dirty = true;
					m_deleteDate = value;
				}
			}

		}

	}
}
