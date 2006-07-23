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

namespace DOL.Database.DataTransferObjects
{
	[Serializable]
	public struct GamePlayerEntity
	{
		private int m_id;
		private int m_account;
		private byte m_activeQuiverSlot;
		private byte m_activeWeaponSlot;
		private int m_baseCharisma;
		private int m_baseConstitution;
		private int m_baseDexterity;
		private int m_baseEmpathy;
		private int m_baseIntelligence;
		private int m_basePiety;
		private int m_baseQuickness;
		private int m_baseStrength;
		private int m_bindHeading;
		private int m_bindRegionId;
		private int m_bindX;
		private int m_bindY;
		private int m_bindZ;
		private long m_bountyPoints;
		private string m_cancelStyle;
		private int m_capturedKeeps;
		private int m_capturedTowers;
		private int m_characterClassId;
		private int m_craftingPrimarySkill;
		private DateTime m_creationDate;
		private int m_creationModel;
		private string m_currentTitleType;
		private byte m_customisationStep;
		private byte m_deathCount;
		private long m_deathTime;
		private string m_disabledAbilities;
		private string m_disabledSpells;
		private byte m_endurancePercent;
		private long m_experience;
		private byte m_eyeColor;
		private byte m_eyeSize;
		private byte m_faceType;
		private int m_gender;
		private string m_guildName;
		private string m_guildNameFlag;
		private byte m_guildRank;
		private byte m_hairColor;
		private byte m_hairStyle;
		private int m_heading;
		private int m_health;
		private string m_isAnonymous;
		private string m_isLevelRespecUsed;
		private string m_isLevelSecondStage;
		private int m_killsAlbionDeathBlows;
		private int m_killsAlbionPlayers;
		private int m_killsAlbionSolo;
		private int m_killsHiberniaDeathBlows;
		private int m_killsHiberniaPlayers;
		private int m_killsHiberniaSolo;
		private int m_killsMidgardDeathBlows;
		private int m_killsMidgardPlayers;
		private int m_killsMidgardSolo;
		private string m_lastName;
		private DateTime m_lastPlayed;
		private byte m_level;
		private byte m_lipSize;
		private int m_lotNumber;
		private int m_mana;
		private int m_maxSpeedBase;
		private int m_model;
		private long m_money;
		private byte m_moodType;
		private string m_name;
		private long m_playedTime;
		private int m_race;
		private byte m_realm;
		private int m_realmLevel;
		private long m_realmPoints;
		private int m_realmSpecialtyPoints;
		private int m_regionId;
		private int m_respecAmountAllSkill;
		private int m_respecAmountSingleSkill;
		private int m_respecBought;
		private string m_safetyFlag;
		private string m_serializedAbilities;
		private string m_serializedCraftingSkills;
		private string m_serializedFriendsList;
		private string m_serializedSpecs;
		private string m_serializedSpellLines;
		private int m_skillSpecialtyPoints;
		private byte m_slotPosition;
		private string m_spellQueue;
		private string m_styles;
		private byte m_taskDone;
		private int m_totalConstitutionLostAtDeath;
		private string m_usedLevelCommand;
		private int m_x;
		private int m_y;
		private int m_z;

		public int Id
		{
			get { return m_id; }
			set { m_id = value; }
		}
		public int Account
		{
			get { return m_account; }
			set { m_account = value; }
		}
		public byte ActiveQuiverSlot
		{
			get { return m_activeQuiverSlot; }
			set { m_activeQuiverSlot = value; }
		}
		public byte ActiveWeaponSlot
		{
			get { return m_activeWeaponSlot; }
			set { m_activeWeaponSlot = value; }
		}
		public int BaseCharisma
		{
			get { return m_baseCharisma; }
			set { m_baseCharisma = value; }
		}
		public int BaseConstitution
		{
			get { return m_baseConstitution; }
			set { m_baseConstitution = value; }
		}
		public int BaseDexterity
		{
			get { return m_baseDexterity; }
			set { m_baseDexterity = value; }
		}
		public int BaseEmpathy
		{
			get { return m_baseEmpathy; }
			set { m_baseEmpathy = value; }
		}
		public int BaseIntelligence
		{
			get { return m_baseIntelligence; }
			set { m_baseIntelligence = value; }
		}
		public int BasePiety
		{
			get { return m_basePiety; }
			set { m_basePiety = value; }
		}
		public int BaseQuickness
		{
			get { return m_baseQuickness; }
			set { m_baseQuickness = value; }
		}
		public int BaseStrength
		{
			get { return m_baseStrength; }
			set { m_baseStrength = value; }
		}
		public int BindHeading
		{
			get { return m_bindHeading; }
			set { m_bindHeading = value; }
		}
		public int BindRegionId
		{
			get { return m_bindRegionId; }
			set { m_bindRegionId = value; }
		}
		public int BindX
		{
			get { return m_bindX; }
			set { m_bindX = value; }
		}
		public int BindY
		{
			get { return m_bindY; }
			set { m_bindY = value; }
		}
		public int BindZ
		{
			get { return m_bindZ; }
			set { m_bindZ = value; }
		}
		public long BountyPoints
		{
			get { return m_bountyPoints; }
			set { m_bountyPoints = value; }
		}
		public string CancelStyle
		{
			get { return m_cancelStyle; }
			set { m_cancelStyle = value; }
		}
		public int CapturedKeeps
		{
			get { return m_capturedKeeps; }
			set { m_capturedKeeps = value; }
		}
		public int CapturedTowers
		{
			get { return m_capturedTowers; }
			set { m_capturedTowers = value; }
		}
		public int CharacterClassId
		{
			get { return m_characterClassId; }
			set { m_characterClassId = value; }
		}
		public int CraftingPrimarySkill
		{
			get { return m_craftingPrimarySkill; }
			set { m_craftingPrimarySkill = value; }
		}
		public DateTime CreationDate
		{
			get { return m_creationDate; }
			set { m_creationDate = value; }
		}
		public int CreationModel
		{
			get { return m_creationModel; }
			set { m_creationModel = value; }
		}
		public string CurrentTitleType
		{
			get { return m_currentTitleType; }
			set { m_currentTitleType = value; }
		}
		public byte CustomisationStep
		{
			get { return m_customisationStep; }
			set { m_customisationStep = value; }
		}
		public byte DeathCount
		{
			get { return m_deathCount; }
			set { m_deathCount = value; }
		}
		public long DeathTime
		{
			get { return m_deathTime; }
			set { m_deathTime = value; }
		}
		public string DisabledAbilities
		{
			get { return m_disabledAbilities; }
			set { m_disabledAbilities = value; }
		}
		public string DisabledSpells
		{
			get { return m_disabledSpells; }
			set { m_disabledSpells = value; }
		}
		public byte EndurancePercent
		{
			get { return m_endurancePercent; }
			set { m_endurancePercent = value; }
		}
		public long Experience
		{
			get { return m_experience; }
			set { m_experience = value; }
		}
		public byte EyeColor
		{
			get { return m_eyeColor; }
			set { m_eyeColor = value; }
		}
		public byte EyeSize
		{
			get { return m_eyeSize; }
			set { m_eyeSize = value; }
		}
		public byte FaceType
		{
			get { return m_faceType; }
			set { m_faceType = value; }
		}
		public int Gender
		{
			get { return m_gender; }
			set { m_gender = value; }
		}
		public string GuildName
		{
			get { return m_guildName; }
			set { m_guildName = value; }
		}
		public string GuildNameFlag
		{
			get { return m_guildNameFlag; }
			set { m_guildNameFlag = value; }
		}
		public byte GuildRank
		{
			get { return m_guildRank; }
			set { m_guildRank = value; }
		}
		public byte HairColor
		{
			get { return m_hairColor; }
			set { m_hairColor = value; }
		}
		public byte HairStyle
		{
			get { return m_hairStyle; }
			set { m_hairStyle = value; }
		}
		public int Heading
		{
			get { return m_heading; }
			set { m_heading = value; }
		}
		public int Health
		{
			get { return m_health; }
			set { m_health = value; }
		}
		public string IsAnonymous
		{
			get { return m_isAnonymous; }
			set { m_isAnonymous = value; }
		}
		public string IsLevelRespecUsed
		{
			get { return m_isLevelRespecUsed; }
			set { m_isLevelRespecUsed = value; }
		}
		public string IsLevelSecondStage
		{
			get { return m_isLevelSecondStage; }
			set { m_isLevelSecondStage = value; }
		}
		public int KillsAlbionDeathBlows
		{
			get { return m_killsAlbionDeathBlows; }
			set { m_killsAlbionDeathBlows = value; }
		}
		public int KillsAlbionPlayers
		{
			get { return m_killsAlbionPlayers; }
			set { m_killsAlbionPlayers = value; }
		}
		public int KillsAlbionSolo
		{
			get { return m_killsAlbionSolo; }
			set { m_killsAlbionSolo = value; }
		}
		public int KillsHiberniaDeathBlows
		{
			get { return m_killsHiberniaDeathBlows; }
			set { m_killsHiberniaDeathBlows = value; }
		}
		public int KillsHiberniaPlayers
		{
			get { return m_killsHiberniaPlayers; }
			set { m_killsHiberniaPlayers = value; }
		}
		public int KillsHiberniaSolo
		{
			get { return m_killsHiberniaSolo; }
			set { m_killsHiberniaSolo = value; }
		}
		public int KillsMidgardDeathBlows
		{
			get { return m_killsMidgardDeathBlows; }
			set { m_killsMidgardDeathBlows = value; }
		}
		public int KillsMidgardPlayers
		{
			get { return m_killsMidgardPlayers; }
			set { m_killsMidgardPlayers = value; }
		}
		public int KillsMidgardSolo
		{
			get { return m_killsMidgardSolo; }
			set { m_killsMidgardSolo = value; }
		}
		public string LastName
		{
			get { return m_lastName; }
			set { m_lastName = value; }
		}
		public DateTime LastPlayed
		{
			get { return m_lastPlayed; }
			set { m_lastPlayed = value; }
		}
		public byte Level
		{
			get { return m_level; }
			set { m_level = value; }
		}
		public byte LipSize
		{
			get { return m_lipSize; }
			set { m_lipSize = value; }
		}
		public int LotNumber
		{
			get { return m_lotNumber; }
			set { m_lotNumber = value; }
		}
		public int Mana
		{
			get { return m_mana; }
			set { m_mana = value; }
		}
		public int MaxSpeedBase
		{
			get { return m_maxSpeedBase; }
			set { m_maxSpeedBase = value; }
		}
		public int Model
		{
			get { return m_model; }
			set { m_model = value; }
		}
		public long Money
		{
			get { return m_money; }
			set { m_money = value; }
		}
		public byte MoodType
		{
			get { return m_moodType; }
			set { m_moodType = value; }
		}
		public string Name
		{
			get { return m_name; }
			set { m_name = value; }
		}
		public long PlayedTime
		{
			get { return m_playedTime; }
			set { m_playedTime = value; }
		}
		public int Race
		{
			get { return m_race; }
			set { m_race = value; }
		}
		public byte Realm
		{
			get { return m_realm; }
			set { m_realm = value; }
		}
		public int RealmLevel
		{
			get { return m_realmLevel; }
			set { m_realmLevel = value; }
		}
		public long RealmPoints
		{
			get { return m_realmPoints; }
			set { m_realmPoints = value; }
		}
		public int RealmSpecialtyPoints
		{
			get { return m_realmSpecialtyPoints; }
			set { m_realmSpecialtyPoints = value; }
		}
		public int RegionId
		{
			get { return m_regionId; }
			set { m_regionId = value; }
		}
		public int RespecAmountAllSkill
		{
			get { return m_respecAmountAllSkill; }
			set { m_respecAmountAllSkill = value; }
		}
		public int RespecAmountSingleSkill
		{
			get { return m_respecAmountSingleSkill; }
			set { m_respecAmountSingleSkill = value; }
		}
		public int RespecBought
		{
			get { return m_respecBought; }
			set { m_respecBought = value; }
		}
		public string SafetyFlag
		{
			get { return m_safetyFlag; }
			set { m_safetyFlag = value; }
		}
		public string SerializedAbilities
		{
			get { return m_serializedAbilities; }
			set { m_serializedAbilities = value; }
		}
		public string SerializedCraftingSkills
		{
			get { return m_serializedCraftingSkills; }
			set { m_serializedCraftingSkills = value; }
		}
		public string SerializedFriendsList
		{
			get { return m_serializedFriendsList; }
			set { m_serializedFriendsList = value; }
		}
		public string SerializedSpecs
		{
			get { return m_serializedSpecs; }
			set { m_serializedSpecs = value; }
		}
		public string SerializedSpellLines
		{
			get { return m_serializedSpellLines; }
			set { m_serializedSpellLines = value; }
		}
		public int SkillSpecialtyPoints
		{
			get { return m_skillSpecialtyPoints; }
			set { m_skillSpecialtyPoints = value; }
		}
		public byte SlotPosition
		{
			get { return m_slotPosition; }
			set { m_slotPosition = value; }
		}
		public string SpellQueue
		{
			get { return m_spellQueue; }
			set { m_spellQueue = value; }
		}
		public string Styles
		{
			get { return m_styles; }
			set { m_styles = value; }
		}
		public byte TaskDone
		{
			get { return m_taskDone; }
			set { m_taskDone = value; }
		}
		public int TotalConstitutionLostAtDeath
		{
			get { return m_totalConstitutionLostAtDeath; }
			set { m_totalConstitutionLostAtDeath = value; }
		}
		public string UsedLevelCommand
		{
			get { return m_usedLevelCommand; }
			set { m_usedLevelCommand = value; }
		}
		public int X
		{
			get { return m_x; }
			set { m_x = value; }
		}
		public int Y
		{
			get { return m_y; }
			set { m_y = value; }
		}
		public int Z
		{
			get { return m_z; }
			set { m_z = value; }
		}
	}
}
