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
		/// <summary>
		/// The character the account own. it store GamePlayer in DB
		/// </summary>
		[DataTable(TableName = "DOLCharacters")]
		public class Character : DataObject
		{
			private string m_accountName;
			private int m_accountSlot;

			private DateTime m_creationDate;
			private DateTime m_lastPlayed;

			//The following structure was directly taken
			//from the 0x57/0x55 Packets
			//
			private string m_name;			//24 Bytes
			//0x00					//24 bytes empty 
			//Locationstring		//24 bytes empty when sent
			private string m_guildid;		//not sent in 0x55/0x57		
			private string m_lastName;		//not sent in 0x55/0x57		
			private int m_race;
			private int m_gender;
			private int m_level;				//01 byte
			private int m_class;				//01 byte
			private int m_realm;				//01 byte
			private int m_creationModel;		//02 byte
			private int m_region;				//01 byte
			private int m_maxEndurance;
			private int m_health;
			private int m_mana;
			private int m_concentration;
			private int m_endurance;
			private long m_exp;
			private long m_bntyPts;
			private long m_realmPts;
			private int m_skillSpecPts;
			private int m_realmSpecPts;
			private int m_realmLevel;
			//0x00					//01 byte
			//int mUnk2;			//04 byte
			//int mStr;				//01 byte
			//int mDex;				//01 byte
			//int mCon;				//01 byte
			//int mQui;				//01 byte
			//int mInt;				//01 byte
			//int mPie;				//01 byte
			//int mEmp;				//01 byte
			//int mChr;				//01 byte
			//0x00					//44 bytes for inventory and stuff

			private byte m_activeWeaponSlot;
			private bool m_isCloakHoodUp;
			private bool m_isCloakInvisible;
			private bool m_isHelmInvisible;
			private bool m_spellQueue;
			private int m_copper;
			private int m_silver;
			private int m_gold;
			private int m_platinum;
			private int m_mithril;
			private int m_currentModel;

			private int m_constitution = 0;
			private int m_dexterity = 0;
			private int m_strength = 0;
			private int m_quickness = 0;
			private int m_intelligence = 0;
			private int m_piety = 0;
			private int m_empathy = 0;
			private int m_charisma = 0;

			//This needs to be uint and ushort!
			private int m_xpos;
			private int m_ypos;
			private int m_zpos;

			//bind position
			private int m_bindxpos;
			private int m_bindypos;
			private int m_bindzpos;
			private int m_bindregion;
			private int m_bindheading;
			private byte m_deathCount;
			private int m_conLostAtDeath;

			private bool m_hasGravestone;
			private int m_gravestoneRegion;

			private int m_direction;
			private int m_maxSpeed;

			private bool m_isLevelSecondStage;
			private bool m_usedLevelCommand;

			// here are skills stored. loading and saving skills of player is done automatically and 
			// these fields should NOT manipulated or used otherwise
			// instead use the skill access methods on GamePlayer
			private string m_abilities = "";	// comma separated string of ability keynames and levels eg "sprint,0,evade,1"
			private string m_specs = "";			// comma separated string of spec keynames and levels like "earth_magic,5,slash,10"
			private string m_spellLines = "";		// serialized string of spell lines and levels like "Spirit Animation|5;Vivification|7"
			private string m_realmAbilities = ""; // for later use
			private string m_craftingSkills = "";// crafting skills
			private string m_disabledSpells = "";
			private string m_disabledAbilities = "";

			private string m_friendList = ""; //comma seperated string of friends
			private string m_playerTitleType = "";

			private bool m_flagClassName = true;
			private ushort m_guildRank;

			private long m_playedTime;  // character /played in seconds.
			private long m_deathTime; // character /played death time

			private int m_respecAmountAllSkill;  // full Respecs.
			private int m_respecAmountSingleSkill; // Single-Line Respecs
			private int m_respecAmountRealmSkill; //realm respecs
			private int m_respecAmountDOL; // Patch 1.84 /respec Mythic
			private int m_respecAmountChampionSkill; // CL Respecs
			private bool m_isLevelRespecUsed;
			private bool m_safetyFlag;
			private int m_craftingPrimarySkill = 0;
			private bool m_cancelStyle;
			private bool m_isAnonymous;

			private byte m_customisationStep = 1;

			private byte m_eyesize = 0;
			private byte m_lipsize = 0;
			private byte m_eyecolor = 0;
			private byte m_hairColor = 0;
			private byte m_facetype = 0;
			private byte m_hairstyle = 0;
			private byte m_moodtype = 0;

			private bool m_gainXP;
			private bool m_gainRP;
			private bool m_autoloot;
			private int m_lastfreeLevel;
			private DateTime m_lastfreeleveled;
			private bool m_showXFireInfo;
			private bool m_noHelp;
			private bool m_showGuildLogins;

			private string m_guildNote = "";
			
            //CLs
            private bool m_cl;
            private long m_clExperience;
            private int m_clLevel;
            private int m_clSpecPoints;
            private string m_clSpells;
			
            // MLs
            private byte m_ml;
            private long m_mlExperience;
            private int m_mlLevel;
            private int m_mlStep;
            private bool m_mlGranted;
			
			static bool m_autoSave;

			/// <summary>
			/// Create the character row in table
			/// </summary>
			public Character()
			{
				m_creationDate = DateTime.Now;
				m_autoSave = false;
				m_concentration = 100;
				m_exp = 0;
				m_bntyPts = 0;
				m_realmPts = 0;
				m_skillSpecPts = 0;
				m_realmSpecPts = 0;

				m_lastPlayed = DateTime.Now; // Prevent /played crash.
				m_playedTime = 0;  // /played startup
				m_deathTime = long.MinValue;
				m_respecAmountAllSkill = 0;
				m_respecAmountSingleSkill = 0;
				m_respecAmountRealmSkill = 0;
				m_respecAmountDOL = 0;
				
				m_isLevelRespecUsed = true;
				m_safetyFlag = true;
				m_craftingPrimarySkill = 0;
				m_usedLevelCommand = false;
				m_spellQueue = true;
				m_gainXP = true;
				m_gainRP = true;
				m_autoloot = true;
				m_showXFireInfo = false;
				m_noHelp = false;
				m_showGuildLogins = false;
			}

			/// <summary>
			/// Gets/sets if this character has xp in a gravestone
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool HasGravestone
			{
				get
				{
					return m_hasGravestone;
				}
				set
				{
					m_hasGravestone = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets the region id where the gravestone of the player is located
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int GravestoneRegion
			{
				get
				{
					return m_gravestoneRegion;
				}
				set
				{
					m_gravestoneRegion = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character constitution
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Constitution
			{
				get
				{
					return m_constitution;
				}
				set
				{
					m_constitution = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character dexterity
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Dexterity
			{
				get
				{
					return m_dexterity;
				}
				set
				{
					m_dexterity = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character strength
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Strength
			{
				get
				{
					return m_strength;
				}
				set
				{
					m_strength = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character quickness
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Quickness
			{
				get
				{
					return m_quickness;
				}
				set
				{
					m_quickness = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character intelligence
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Intelligence
			{
				get
				{
					return m_intelligence;
				}
				set
				{
					m_intelligence = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character piety
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Piety
			{
				get
				{
					return m_piety;
				}
				set
				{
					m_piety = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character empathy
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Empathy
			{
				get
				{
					return m_empathy;
				}
				set
				{
					m_empathy = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character charisma
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Charisma
			{
				get
				{
					return m_charisma;
				}
				set
				{
					m_charisma = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets chracter bounty points
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public long BountyPoints
			{
				get
				{
					return m_bntyPts;
				}
				set
				{
					m_bntyPts = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character realm points
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public long RealmPoints
			{
				get
				{
					return m_realmPts;
				}
				set
				{
					m_realmPts = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character skill specialty points
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int SkillSpecialtyPoints
			{
				get
				{
					return m_skillSpecPts;
				}
				set
				{
					m_skillSpecPts = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets realm specialty points
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RealmSpecialtyPoints
			{
				get
				{
					return m_realmSpecPts;
				}
				set
				{
					m_realmSpecPts = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets realm rank
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RealmLevel
			{
				get
				{
					return m_realmLevel;
				}
				set
				{
					m_realmLevel = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets experience
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public long Experience
			{
				get
				{
					return m_exp;
				}
				set
				{
					m_exp = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets max endurance
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int MaxEndurance
			{
				get
				{
					return m_maxEndurance;
				}
				set
				{
					m_maxEndurance = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets maximum health
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Health
			{
				get
				{
					return m_health;
				}
				set
				{
					m_health = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets max mana
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Mana
			{
				get
				{
					return m_mana;
				}
				set
				{
					m_mana = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets max endurance
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Endurance
			{
				get
				{
					return m_endurance;
				}
				set
				{
					m_endurance = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets the object concentration
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Concentration
			{
				get
				{
					return m_concentration;
				}
				set
				{
					m_concentration = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Auto save the character
			/// </summary>
			override public bool AutoSave
			{
				get
				{
					return m_autoSave;
				}
				set
				{
					m_autoSave = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Account name of account which own this character
			/// </summary>
			[DataElement(AllowDbNull = false, Index = true)]
			public string AccountName
			{
				get
				{
					return m_accountName;
				}
				set
				{
					Dirty = true;
					m_accountName = value;
				}
			}

			/// <summary>
			/// The slot of character in account
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int AccountSlot
			{
				get
				{
					return m_accountSlot;
				}
				set
				{
					Dirty = true;
					m_accountSlot = value;
				}
			}

			/// <summary>
			/// The creation date of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public DateTime CreationDate
			{
				get
				{
					return m_creationDate;
				}
				set
				{
					Dirty = true;
					m_creationDate = value;
				}
			}

			/// <summary>
			/// The last time this character have been played
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public DateTime LastPlayed
			{
				get
				{
					return m_lastPlayed;
				}
				set
				{
					Dirty = true;
					m_lastPlayed = value;
				}
			}

			/// <summary>
			/// Name of this character. all name of character is unique
			/// </summary>
			[DataElement(AllowDbNull = false, Unique = true)]
			public string Name
			{
				get
				{
					return m_name;
				}
				set
				{
					Dirty = true;
					m_name = value;
				}
			}

			/// <summary>
			/// Lastname of this character. You can have family ;)
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string LastName
			{
				get
				{
					return m_lastName;
				}
				set
				{
					Dirty = true;
					m_lastName = value;
				}
			}

			/// <summary>
			/// ID of the guild this character is in
			/// </summary>
			[DataElement(AllowDbNull = true, Index = true)]
			public string GuildID
			{
				get
				{
					return m_guildid;
				}
				set
				{
					Dirty = true;
					m_guildid = value;
				}
			}

			/// <summary>
			/// Male or female character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Gender
			{
				get
				{
					return m_gender;
				}
				set
				{
					Dirty = true;
					m_gender = value;
				}
			}

			/// <summary>
			/// Race of character (viking,troll,...)
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Race
			{
				get
				{
					return m_race;
				}
				set
				{
					Dirty = true;
					m_race = value;
				}
			}

			/// <summary>
			/// Level of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Level
			{
				get
				{
					return m_level;
				}
				set
				{
					Dirty = true;
					m_level = value;
				}
			}

			/// <summary>
			/// class of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Class
			{
				get
				{
					return m_class;
				}
				set
				{
					Dirty = true;
					m_class = value;
				}
			}

			/// <summary>
			/// Realm of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Realm
			{
				get
				{
					return m_realm;
				}
				set
				{
					Dirty = true;
					m_realm = value;
				}
			}

			/// <summary>
			/// The model of character when created
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int CreationModel
			{
				get
				{
					return m_creationModel;
				}
				set
				{
					Dirty = true;
					m_creationModel = value;
				}
			}

			/// <summary>
			/// The region of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Region
			{
				get
				{
					return m_region;
				}
				set
				{
					Dirty = true;
					m_region = value;
				}
			}

			/// <summary>
			/// the weapon active to show
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public byte ActiveWeaponSlot
			{
				get
				{
					return m_activeWeaponSlot;
				}
				set
				{
					m_activeWeaponSlot = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// The model used actually in character (main time same than CreationModel)
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int CurrentModel
			{
				get
				{
					return m_currentModel;
				}
				set
				{
					Dirty = true;
					m_currentModel = value;
				}
			}

			/// <summary>
			/// The X position of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Xpos
			{
				get
				{
					return m_xpos;
				}
				set
				{
					Dirty = true;
					m_xpos = value;
				}
			}

			/// <summary>
			/// The Y position of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Ypos
			{
				get
				{
					return m_ypos;
				}
				set
				{
					Dirty = true;
					m_ypos = value;
				}
			}

			/// <summary>
			/// The Z position of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Zpos
			{
				get
				{
					return m_zpos;
				}
				set
				{
					Dirty = true;
					m_zpos = value;
				}
			}

			/// <summary>
			/// The bind X position of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int BindXpos
			{
				get
				{
					return m_bindxpos;
				}
				set
				{
					Dirty = true;
					m_bindxpos = value;
				}
			}

			/// <summary>
			/// The bind Y position of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int BindYpos
			{
				get
				{
					return m_bindypos;
				}
				set
				{
					Dirty = true;
					m_bindypos = value;
				}
			}

			/// <summary>
			/// The bind Z position of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int BindZpos
			{
				get
				{
					return m_bindzpos;
				}
				set
				{
					Dirty = true;
					m_bindzpos = value;
				}
			}

			/// <summary>
			/// The bind region position of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int BindRegion
			{
				get
				{
					return m_bindregion;
				}
				set
				{
					Dirty = true;
					m_bindregion = value;
				}
			}

			/// <summary>
			/// The bind heading position of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int BindHeading
			{
				get
				{
					return m_bindheading;
				}
				set
				{
					Dirty = true;
					m_bindheading = value;
				}
			}

			/// <summary>
			/// The number of chacter is dead at this level
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte DeathCount
			{
				get
				{
					return m_deathCount;
				}
				set
				{
					Dirty = true;
					m_deathCount = value;
				}
			}

			/// <summary>
			/// Constitution lost at death
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int ConLostAtDeath
			{
				get
				{
					return m_conLostAtDeath;
				}
				set
				{
					Dirty = true;
					m_conLostAtDeath = value;
				}
			}

			/// <summary>
			/// Heading of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int Direction
			{
				get
				{
					return m_direction;
				}
				set
				{
					Dirty = true;
					m_direction = value;
				}
			}

			/// <summary>
			/// The max speed of character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int MaxSpeed
			{
				get
				{
					return m_maxSpeed;
				}
				set
				{
					Dirty = true;
					m_maxSpeed = value;
				}
			}

			/// <summary>
			/// Money copper part player own
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Copper
			{
				get
				{
					return m_copper;
				}
				set
				{
					Dirty = true;
					m_copper = value;
				}
			}

			/// <summary>
			/// Money silver part player own
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Silver
			{
				get
				{
					return m_silver;
				}
				set
				{
					Dirty = true;
					m_silver = value;
				}
			}

			/// <summary>
			/// Money gold part player own
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Gold
			{
				get
				{
					return m_gold;
				}
				set
				{
					Dirty = true;
					m_gold = value;
				}
			}

			/// <summary>
			/// Money platinum part player own
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Platinum
			{
				get
				{
					return m_platinum;
				}
				set
				{
					Dirty = true;
					m_platinum = value;
				}
			}

			/// <summary>
			/// Money mithril part player own
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int Mithril
			{
				get
				{
					return m_mithril;
				}
				set
				{
					Dirty = true;
					m_mithril = value;
				}
			}

			/// <summary>
			/// The crafting skills of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedCraftingSkills
			{
				get { return m_craftingSkills; }
				set
				{
					Dirty = true;
					m_craftingSkills = value;
				}
			}

			/// <summary>
			/// The abilities of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedAbilities
			{
				get { return m_abilities; }
				set
				{
					Dirty = true;
					m_abilities = value;
				}
			}

			/// <summary>
			/// The specs of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedSpecs
			{
				get { return m_specs; }
				set
				{
					Dirty = true;
					m_specs = value;
				}
			}

			/// <summary>
			/// the spell lines of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedSpellLines
			{
				get { return m_spellLines; }
				set
				{
					m_spellLines = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// the realm abilities of character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedRealmAbilities
			{
				get { return m_realmAbilities; }
				set
				{
					Dirty = true;
					m_realmAbilities = value;
				}
			}

			/// <summary>
			/// The spells unallowed to character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string DisabledSpells
			{
				get { return m_disabledSpells; }
				set { m_disabledSpells = value; }
			}

			/// <summary>
			/// The abilities unallowed to character
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string DisabledAbilities
			{
				get { return m_disabledAbilities; }
				set { m_disabledAbilities = value; }
			}

			/// <summary>
			/// The Friend list
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string SerializedFriendsList
			{
				get { return m_friendList; }
				set
				{
					Dirty = true;
					m_friendList = value;
				}
			}

			/// <summary>
			/// Is cloak hood up
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool IsCloakHoodUp
			{
				get { return m_isCloakHoodUp; }
				set
				{
					m_isCloakHoodUp = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Is cloak hood up
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public bool IsCloakInvisible
			{
				get { return m_isCloakInvisible; }
				set
				{
					m_isCloakInvisible = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Is cloak hood up
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public bool IsHelmInvisible
			{
				get { return m_isHelmInvisible; }
				set
				{
					m_isHelmInvisible = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Spell queue flag
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool SpellQueue
			{
				get { return m_spellQueue; }
				set
				{
					m_spellQueue = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets half-level flag
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool IsLevelSecondStage
			{
				get
				{
					return m_isLevelSecondStage;
				}
				set
				{
					m_isLevelSecondStage = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets guildname flag to print guildname or crafting title
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool FlagClassName
			{
				get
				{
					return m_flagClassName;
				}
				set
				{
					m_flagClassName = value;
					Dirty = true;
				}
			}

			private bool m_advisor = false;
			/// <summary>
			/// Is the character an advisor
			/// </summary>
			[DataElement(AllowDbNull=true)]
			public bool Advisor
			{
				get { return m_advisor; }
				set { Dirty = true; m_advisor = value; }
			}

			/// <summary>
			/// Gets/sets guild rank in the guild
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public ushort GuildRank
			{
				get
				{
					return m_guildRank;
				}
				set
				{
					m_guildRank = value;
					Dirty = true;
				}
			}
			/// <summary>
			/// Gets/sets the characters /played time
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public long PlayedTime
			{
				get
				{
					return m_playedTime;
				}
				set
				{
					Dirty = true;
					m_playedTime = value;
				}
			}
			/// <summary>
			/// Gets/sets the characters death /played time
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public long DeathTime
			{
				get { return m_deathTime; }
				set
				{
					Dirty = true;
					m_deathTime = value;
				}
			}
			/// <summary>
			/// Gets/sets the characters full skill respecs available
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RespecAmountAllSkill
			{
				get
				{
					return m_respecAmountAllSkill;
				}
				set
				{
					Dirty = true;
					m_respecAmountAllSkill = value;
				}
			}
			/// <summary>
			/// Gets/sets the characters single-line respecs available
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RespecAmountSingleSkill
			{
				get
				{
					return m_respecAmountSingleSkill;
				}
				set
				{
					Dirty = true;
					m_respecAmountSingleSkill = value;
				}
			}

			/// <summary>
			/// Gets/Sets the characters realm respecs available
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RespecAmountRealmSkill
			{
				get
				{
					return m_respecAmountRealmSkill;
				}
				set
				{
					Dirty = true;
					m_respecAmountRealmSkill = value;
				}
			}
			
			/// <summary>
			/// Gets/Sets the characters DOL respecs available
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RespecAmountDOL
			{
				get
				{
					return m_respecAmountDOL;
				}
				set
				{
					Dirty = true;
					m_respecAmountDOL = value;
				}
			}
			
			/// <summary>
			/// Gets/sets the characters single-line respecs available
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int RespecAmountChampionSkill
			{
				get
				{
					return m_respecAmountChampionSkill;
				}
				set
				{
					Dirty = true;
					m_respecAmountChampionSkill = value;
				}
			}
			/// <summary>
			/// Gets/Sets level respec flag
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool IsLevelRespecUsed
			{
				get { return m_isLevelRespecUsed; }
				set
				{
					Dirty = true;
					m_isLevelRespecUsed = value;
				}
			}
			/// <summary>
			/// Gets/sets the characters safety flag
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool SafetyFlag
			{
				get { return m_safetyFlag; }
				set
				{
					Dirty = true;
					m_safetyFlag = value;
				}
			}

			/// <summary>
			/// Gets/sets the characters safety flag
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int CraftingPrimarySkill
			{
				get { return m_craftingPrimarySkill; }
				set
				{
					Dirty = true;
					m_craftingPrimarySkill = value;
				}
			}

			/// <summary>
			/// the cancel style
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool CancelStyle
			{
				get { return m_cancelStyle; }
				set
				{
					Dirty = true;
					m_cancelStyle = value;
				}
			}

			/// <summary>
			/// is anonymous( can not seen him in /who and some other things
			/// /anon to toggle
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool IsAnonymous
			{
				get { return m_isAnonymous; }
				set
				{
					Dirty = true;
					m_isAnonymous = value;
				}
			}

			/// <summary>
			/// the face customisation step
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte CustomisationStep
			{
				get { return m_customisationStep; }
				set
				{
					Dirty = true;
					m_customisationStep = value;
				}
			}

			/// <summary>
			/// Gets/sets character EyeSize
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte EyeSize
			{
				get
				{
					return m_eyesize;
				}
				set
				{
					m_eyesize = value;
					Dirty = true;
				}
			}
			/// <summary>
			/// Gets/sets character LipSize
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte LipSize
			{
				get
				{
					return m_lipsize;
				}
				set
				{
					m_lipsize = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character EyeColor
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte EyeColor
			{
				get
				{
					return m_eyecolor;
				}
				set
				{
					m_eyecolor = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character HairColor
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte HairColor
			{
				get
				{
					return m_hairColor;
				}
				set
				{
					m_hairColor = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character FaceType
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte FaceType
			{
				get
				{
					return m_facetype;
				}
				set
				{
					m_facetype = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character HairStyle
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte HairStyle
			{
				get
				{
					return m_hairstyle;
				}
				set
				{
					m_hairstyle = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets character MoodType
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public byte MoodType
			{
				get
				{
					return m_moodtype;
				}
				set
				{
					m_moodtype = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets weather a character has used /level
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool UsedLevelCommand
			{
				get
				{
					return m_usedLevelCommand;
				}
				set
				{
					m_usedLevelCommand = value;
					Dirty = true;
				}
			}

			/// <summary>
			/// Gets/sets selected player title type
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public string CurrentTitleType
			{
				get { return m_playerTitleType; }
				set { m_playerTitleType = value; Dirty = true; }
			}

			#region Statistics

			private int m_killsAlbionPlayers;
			private int m_killsMidgardPlayers;
			private int m_killsHiberniaPlayers;
			private int m_killsAlbionDeathBlows;
			private int m_killsMidgardDeathBlows;
			private int m_killsHiberniaDeathBlows;
			private int m_killsAlbionSolo;
			private int m_killsMidgardSolo;
			private int m_killsHiberniaSolo;
			private int m_capturedKeeps;
			private int m_capturedTowers;
			private int m_capturedRelics;
			private int m_killsDragon;
			private int m_deathsPvP;

			/// <summary>
			/// Amount of Albion Players Killed
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsAlbionPlayers
			{
				get { return m_killsAlbionPlayers; }
				set { m_killsAlbionPlayers = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Midgard Players Killed
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsMidgardPlayers
			{
				get { return m_killsMidgardPlayers; }
				set { m_killsMidgardPlayers = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Hibernia Players Killed
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsHiberniaPlayers
			{
				get { return m_killsHiberniaPlayers; }
				set { m_killsHiberniaPlayers = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Death Blows on Albion Players
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsAlbionDeathBlows
			{
				get { return m_killsAlbionDeathBlows; }
				set { m_killsAlbionDeathBlows = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Death Blows on Midgard Players
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsMidgardDeathBlows
			{
				get { return m_killsMidgardDeathBlows; }
				set { m_killsMidgardDeathBlows = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Death Blows on Hibernia Players
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsHiberniaDeathBlows
			{
				get { return m_killsHiberniaDeathBlows; }
				set { m_killsHiberniaDeathBlows = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Solo Albion Kills
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsAlbionSolo
			{
				get { return m_killsAlbionSolo; }
				set { m_killsAlbionSolo = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Solo Midgard Kills
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsMidgardSolo
			{
				get { return m_killsMidgardSolo; }
				set { m_killsMidgardSolo = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Solo Hibernia Kills
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsHiberniaSolo
			{
				get { return m_killsHiberniaSolo; }
				set { m_killsHiberniaSolo = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Keeps Captured
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int CapturedKeeps
			{
				get { return m_capturedKeeps; }
				set { m_capturedKeeps = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Towers Captured
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int CapturedTowers
			{
				get { return m_capturedTowers; }
				set { m_capturedTowers = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of Relics Captured
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int CapturedRelics
			{
				get { return m_capturedRelics; }
				set { m_capturedRelics = value; Dirty = true; }
			}
			
			/// <summary>
			/// Amount of Dragons Killed
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsDragon
			{
				get { return m_killsDragon; }
				set { m_killsDragon = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of PvP deaths
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int DeathsPvP
			{
				get { return m_deathsPvP; }
				set { m_deathsPvP = value; Dirty = true; }
			}

			private int m_killsLegion;
			private int m_killsEpicBoss;


			/// <summary>
			/// Amount of killed Legions
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsLegion
			{
				get { return m_killsLegion; }
				set { m_killsLegion = value; Dirty = true; }
			}

			/// <summary>
			/// Amount of killed EpicDungeon Boss
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int KillsEpicBoss
			{
				get { return m_killsEpicBoss; }
				set { m_killsEpicBoss = value; Dirty = true; }
			} 

			#endregion

			/// <summary>
			/// can gain experience points
			/// /xp to toggle
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool GainXP
			{
				get { return m_gainXP; }
				set
				{
					Dirty = true;
					m_gainXP = value;
				}
			}

			/// <summary>
			/// can gain realm points
			/// /rp to toggle
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool GainRP
			{
				get { return m_gainRP; }
				set
				{
					Dirty = true;
					m_gainRP = value;
				}
			}
			/// <summary>
			/// autoloot
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public bool Autoloot
			{
				get { return m_autoloot; }
				set
				{
					Dirty = true;
					m_autoloot = value;
				}
			}
			/// <summary>
			/// Last Date for FreeLevel
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public DateTime LastFreeLeveled
			{
				get
				{
					if (m_lastfreeleveled == DateTime.MinValue)
						m_lastfreeleveled = DateTime.Now;
					return m_lastfreeleveled;
				}
				set
				{
					Dirty = true;
					m_lastfreeleveled = value;

				}
			}

			/// <summary>
			/// Last Level for FreeLevel
			/// </summary>
			[DataElement(AllowDbNull = true)]
			public int LastFreeLevel
			{
				get
				{
					return m_lastfreeLevel;

				}
				set
				{
					Dirty = true;
					m_lastfreeLevel = value;

				}
			}

			[DataElement(AllowDbNull = true)]
			public string GuildNote
			{
				get
				{
					return m_guildNote;
				}
				set
				{
					Dirty = true;
					m_guildNote = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public bool ShowXFireInfo
			{
				get
				{
					return m_showXFireInfo;
				}
				set
				{
					Dirty = true;
					m_showXFireInfo = value;
				}
			}

			[DataElement(AllowDbNull = true)]
			public bool NoHelp
			{
				get { return m_noHelp; }
				set { Dirty = true; m_noHelp = value; }
			}

			[DataElement(AllowDbNull = true)]
			public bool ShowGuildLogins
			{
				get { return m_showGuildLogins; }
				set { Dirty = true; m_showGuildLogins = value; }
			}	
			
			/// <summary>
			/// Is Champion level activated
			/// </summary>	           
            [DataElement(AllowDbNull = true)]
            public bool Champion
            {
                get
                {
                    return m_cl;
                }
                set
                {
                    m_cl = value;
                    Dirty = true;
                }
            } 
 			/// <summary>
			/// Champion level
			/// </summary>		           
            [DataElement(AllowDbNull = true)]
            public int ChampionLevel
            {
                get
                {
                    return m_clLevel;
                }
                set
                {
                    m_clLevel = value;
                    Dirty = true;
                }
            }             
			/// <summary>
			/// Champion Available speciality points
			/// </summary>			
            [DataElement(AllowDbNull = true)]
            public int ChampionSpecialtyPoints
            {
                get
                {
                    return m_clSpecPoints;
                }
                set
                {
                    m_clSpecPoints = value;
                    Dirty = true;
                }
            }
 			/// <summary>
			/// Champion Experience
			/// </summary>		           
            [DataElement(AllowDbNull = true)]
            public long ChampionExperience
            { 
                get
                {
                    return m_clExperience;
                }
                set
                {
                    m_clExperience = value;
                    Dirty = true;
                }
            }   
			/// <summary>
			/// Champion Spells
			/// </summary>		           
            [DataElement(AllowDbNull = true)]
            public string ChampionSpells
            { 
                get
                {
                    return m_clSpells;
                }
                set
                {
                    m_clSpells = value;
                    Dirty = true;
                }
            }               
 			/// <summary>
			/// ML Line
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public byte ML
			{
				get
				{
					return m_ml;
				}
				set
				{
					Dirty = true;
					m_ml = value;
				}
			}
			/// <summary>
			/// ML Experience of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public long MLExperience
			{
				get
				{
					return m_mlExperience;
				}
				set
				{
					Dirty = true;
					m_mlExperience = value;
				}
			}
			/// <summary>
			/// ML Level of this character
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public int MLLevel
			{
				get
				{
					return m_mlLevel;
				}
				set
				{
					Dirty = true;
					m_mlLevel = value;
				}
			}		
			/// <summary>
			/// ML can be validated to next level
			/// </summary>
			[DataElement(AllowDbNull = false)]
			public bool MLGranted
			{
				get
				{
					return m_mlGranted;
				}
				set
				{
					Dirty = true;
					m_mlGranted = value;
				}
			}  	
		}
	}
}
