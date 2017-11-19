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
using System.Linq;

using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "DOLCharactersBackup")]
	public class DOLCharactersBackup : DOLCharacters
	{
		string m_dolCharactersID = string.Empty;
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

            AccountName = character.AccountName;
            AccountSlot = character.AccountSlot;
            ActiveWeaponSlot = character.ActiveWeaponSlot;
            ActiveSaddleBags = character.ActiveSaddleBags;
            Advisor = character.Advisor;
            Autoloot = character.Autoloot;
            BindHeading = character.BindHeading;
            BindHouseHeading = character.BindHouseHeading;
            BindHouseRegion = character.BindHouseRegion;
            BindHouseXpos = character.BindHouseXpos;
            BindHouseYpos = character.BindHouseYpos;
            BindHouseZpos = character.BindHouseZpos;
            BindRegion = character.BindRegion;
            BindXpos = character.BindXpos;
            BindYpos = character.BindYpos;
            BindZpos = character.BindZpos;
            BountyPoints = character.BountyPoints;
            CancelStyle = character.CancelStyle;
            CapturedKeeps = character.CapturedKeeps;
            CapturedRelics = character.CapturedRelics;
            CapturedTowers = character.CapturedTowers;
            Champion = character.Champion;
            ChampionExperience = character.ChampionExperience;
            ChampionLevel = character.ChampionLevel;
            Charisma = character.Charisma;
            Class = character.Class;
            Concentration = character.Concentration;
            ConLostAtDeath = character.ConLostAtDeath;
            Constitution = character.Constitution;
            Copper = character.Copper;
            CraftingPrimarySkill = character.CraftingPrimarySkill;
            CreationDate = character.CreationDate;
            CreationModel = character.CreationModel;
            CurrentModel = character.CurrentModel;
            CurrentTitleType = character.CurrentTitleType;
            CustomisationStep = character.CustomisationStep;
            DeathCount = character.DeathCount;
            DeathsPvP = character.DeathsPvP;
            DeathTime = character.DeathTime;
            Dexterity = character.Dexterity;
            Direction = character.Direction;
            DisabledAbilities = character.DisabledAbilities;
            DisabledSpells = character.DisabledSpells;
            Empathy = character.Empathy;
            Endurance = character.Endurance;
            Experience = character.Experience;
            EyeColor = character.EyeColor;
            EyeSize = character.EyeSize;
            FaceType = character.FaceType;
            FlagClassName = character.FlagClassName;
            GainRP = character.GainRP;
            GainXP = character.GainXP;
            Gender = character.Gender;
            Gold = character.Gold;
            GravestoneRegion = character.GravestoneRegion;
            GuildID = character.GuildID;
            GuildNote = character.GuildNote;
            GuildRank = character.GuildRank;
            HairColor = character.HairColor;
            HairStyle = character.HairStyle;
            HasGravestone = character.HasGravestone;
            Health = character.Health;
            IgnoreStatistics = character.IgnoreStatistics;
            Intelligence = character.Intelligence;
            IsAnonymous = character.IsAnonymous;
            IsCloakHoodUp = character.IsCloakHoodUp;
            IsCloakInvisible = character.IsCloakInvisible;
            IsHelmInvisible = character.IsHelmInvisible;
            IsLevelRespecUsed = character.IsLevelRespecUsed;
            IsLevelSecondStage = character.IsLevelSecondStage;
            KillsAlbionDeathBlows = character.KillsAlbionDeathBlows;
            KillsAlbionPlayers = character.KillsAlbionPlayers;
            KillsAlbionSolo = character.KillsAlbionSolo;
            KillsDragon = character.KillsDragon;
            KillsEpicBoss = character.KillsEpicBoss;
            KillsHiberniaDeathBlows = character.KillsHiberniaDeathBlows;
            KillsHiberniaPlayers = character.KillsHiberniaPlayers;
            KillsHiberniaSolo = character.KillsHiberniaSolo;
            KillsLegion = character.KillsLegion;
            KillsMidgardDeathBlows = character.KillsMidgardDeathBlows;
            KillsMidgardPlayers = character.KillsMidgardPlayers;
            KillsMidgardSolo = character.KillsMidgardSolo;
            LastFreeLevel = character.LastFreeLevel;
            LastFreeLeveled = character.LastFreeLeveled;
            LastName = character.LastName;
            LastPlayed = character.LastPlayed;
            Level = character.Level;
            LipSize = character.LipSize;
            Mana = character.Mana;
            MaxEndurance = character.MaxEndurance;
            MaxSpeed = character.MaxSpeed;
            Mithril = character.Mithril;
            ML = character.ML;
            MLExperience = character.MLExperience;
            MLGranted = character.MLGranted;
            MLLevel = character.MLLevel;
            MoodType = character.MoodType;
            Name = character.Name;
            NoHelp = character.NoHelp;
            NotDisplayedInHerald = character.NotDisplayedInHerald;
            Piety = character.Piety;
            Platinum = character.Platinum;
            PlayedTime = character.PlayedTime;
            Quickness = character.Quickness;
            Race = character.Race;
            Realm = character.Realm;
            RealmLevel = character.RealmLevel;
            RealmPoints = character.RealmPoints;
            Region = character.Region;
            RespecAmountAllSkill = character.RespecAmountAllSkill;
            RespecAmountChampionSkill = character.RespecAmountChampionSkill;
            RespecAmountDOL = character.RespecAmountDOL;
            RespecAmountRealmSkill = character.RespecAmountRealmSkill;
            RespecAmountSingleSkill = character.RespecAmountSingleSkill;
            RespecBought = character.RespecBought;
            RPFlag = character.RPFlag;
            SafetyFlag = character.SafetyFlag;
            SerializedAbilities = character.SerializedAbilities;
            SerializedCraftingSkills = character.SerializedCraftingSkills;
            SerializedFriendsList = character.SerializedFriendsList;
            SerializedIgnoreList = character.SerializedIgnoreList;
            SerializedRealmAbilities = character.SerializedRealmAbilities;
            SerializedSpecs = character.SerializedSpecs;
            ShowGuildLogins = character.ShowGuildLogins;
            ShowXFireInfo = character.ShowXFireInfo;
            Silver = character.Silver;
            SpellQueue = character.SpellQueue;
            Strength = character.Strength;
            UsedLevelCommand = character.UsedLevelCommand;
            Xpos = character.Xpos;
            Ypos = character.Ypos;
            Zpos = character.Zpos;
            // Copy Custom Params
            CustomParams = character.CustomParams != null
				? character.CustomParams.Select(param => new DOLCharactersBackupXCustomParam(param.DOLCharactersObjectId, param.KeyName, param.Value)).ToArray()
				: new DOLCharactersBackupXCustomParam[] { } ;
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
        /// Name of this character - indexed but not unique for backups
        /// </summary>
        [DataElement(AllowDbNull = false, Index = true)]
        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
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
		
		/// <summary>
		/// List of Custom Params for this Character Backup
		/// </summary>
		[Relation(LocalField = "DOLCharacters_ID", RemoteField = "DOLCharactersObjectId", AutoLoad = true, AutoDelete = true)]
		public new DOLCharactersBackupXCustomParam[] CustomParams;
	}
	
	/// <summary>
	/// DOL Characters Backup Custom Params linked to Character Backup Entry
	/// </summary>
	[DataTable(TableName = "DOLCharactersBackupXCustomParam")]
	public class DOLCharactersBackupXCustomParam : DOLCharactersXCustomParam
	{
		/// <summary>
		/// Create new instance of <see cref="DOLCharactersBackupXCustomParam"/> linked to Backup'd Character ObjectId
		/// </summary>
		/// <param name="DOLCharactersObjectId">DOLCharacters ObjectId</param>
		/// <param name="KeyName">Key Name</param>
		/// <param name="Value">Value</param>
		public DOLCharactersBackupXCustomParam(string DOLCharactersObjectId, string KeyName, string Value)
			: base(DOLCharactersObjectId, KeyName, Value)
		{
		}

		/// <summary>
		/// Create new instance of <see cref="DOLCharactersBackupXCustomParam"/>
		/// </summary>
		public DOLCharactersBackupXCustomParam()
		{
		}
	}
}
