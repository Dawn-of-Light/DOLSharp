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
using System.Collections.Specialized;
using System.Reflection;
using System.Collections;
using System.Text;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.Events;
using DOL.GS.Database;
using DOL.GS.Effects;
using DOL.GS.PacketHandler.v168;
using DOL.GS.PlayerTitles;
using DOL.GS.PropertyCalc;
using DOL.GS.Quests;
using DOL.GS.Scripts;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using NHibernate.Expression;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a player inside the game
	/// </summary>
	public class GamePlayer : GameLiving
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Name/LastName

		/// <summary>
		/// The last name of this character
		/// </summary>
		private string m_lastName;		
		
		/// <summary>
		/// The lastname of this player
		/// </summary>
		public virtual string LastName
		{
			get { return m_lastName; }
			set
			{
				m_lastName = value;
				//update last name for all players if client is playing
				if(ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					BroadcastCreate();
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of the player
		/// </summary>
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				//update name for all players if client is playing
				if(ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					if(PlayerGroup != null)
						Out.SendGroupWindowUpdate();
					BroadcastCreate();
				}
			}
		}

		#endregion

		#region CreationDate/LastPlayedDate/PlayedTime

		/// <summary>
		/// The datetime when the player has been created
		/// </summary>
		private DateTime m_creationDate;

		/// <summary>
		/// The datetime when the player was last played
		/// </summary>
		private DateTime m_lastPlayed;

		/// <summary>
		/// The played time of this char
		/// </summary>
		private long m_playedTime;  // character /played in seconds.

		/// <summary>
		/// The creation date of this character
		/// </summary>
		public DateTime CreationDate
		{
			get { return m_creationDate; }
			set { m_creationDate = value; }
		}

		/// <summary>
		/// The last time this character have been played
		/// </summary>
		public DateTime LastPlayed
		{
			get { return m_lastPlayed; }
			set { m_lastPlayed = value; }
		}

		/// <summary>
		/// Gets/sets the characters /played time
		/// </summary>
		public long PlayedTime
		{
			get 
			{
				DateTime rightNow = DateTime.Now;
				DateTime oldLast = LastPlayed;
				// Get the total amount of time played between now and lastplayed
				// This is safe as lastPlayed is updated on char load.
				TimeSpan playaPlayed = rightNow.Subtract(oldLast);
				TimeSpan newPlayed = playaPlayed + TimeSpan.FromSeconds(m_playedTime);
				return (long) newPlayed.TotalSeconds; // for the /played command.
			}
			set { m_playedTime = value; }
		}

		#endregion
		
		#region Encumberance

		/// <summary>
		/// Gets the total possible Encumberance
		/// </summary>
		public virtual int MaxEncumberance
		{
			get { return GetModified(eProperty.Strength); }
		}

		/// <summary>
		/// Gets the current Encumberance
		/// </summary>
		public virtual int Encumberance
		{
			get { return Inventory.InventoryWeight; }
		}

		/// <summary>
		/// The Encumberance state of this player
		/// </summary>
		protected bool m_overencumbered=true;

		/// <summary>
		/// Gets/Set the players Encumberance state
		/// </summary>
		public bool IsOverencumbered
		{
			get { return m_overencumbered; }
			set { m_overencumbered=value; }
		}

		/// <summary>
		/// Updates Encumberance and its effects
		/// </summary>
		public  void UpdateEncumberance()
		{
			if(Inventory.InventoryWeight > MaxEncumberance)
			{
				if(IsOverencumbered == false)
				{
					IsOverencumbered=true;
					Out.SendMessage("You are encumbered and move more slowly.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage("You are encumbered!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				Out.SendUpdateMaxSpeed();
			}
			else if (IsOverencumbered)
			{
				IsOverencumbered=false;
				Out.SendUpdateMaxSpeed();
			}
			Out.SendEncumberance();
		}

		#endregion
		
		#region Race/Gender/CreationModel
		/// <summary>
		/// Holds the player's race id
		/// </summary>
		protected int m_race;
		
		/// <summary>
		/// Gets or sets this player's race id
		/// </summary>
		public int Race
		{
			get { return m_race; }
			set { m_race = value; }
		}

		/// <summary>
		/// Holds the player's gender
		/// </summary>
		private int m_gender;
		
		/// <summary>
		/// Male or female character
		/// </summary>
		public int Gender
		{
			get { return m_gender; }
			set { m_gender = value; }
		}

		/// <summary>
		/// The creation model of this player
		/// </summary>
		protected int m_creationModel;

		/// <summary>
		/// Gets or Sets the creation Model of the player
		/// </summary>
		public virtual int CreationModel
		{
			get { return m_creationModel; }
			set { m_creationModel = value; }
		}
		#endregion

		#region FaceCustomisation
		/// <summary>
		/// Hold the customisation step of this player (1 = char older than cata ; 2 = disable config button ; 3 = enable config button)
		/// </summary>
		private byte m_customisationStep = 1;

		/// <summary>
		/// The face customisation step
		/// </summary>
		public byte CustomisationStep
		{
			get { return m_customisationStep; }
			set { m_customisationStep = value; }
		}

		/// <summary>
		/// Hold the character eye size
		/// </summary>
		private byte m_eyesize = 0;

		/// <summary>
		/// Hold the character lip size
		/// </summary>
		private byte m_lipsize = 0;

		/// <summary>
		/// Hold the character eye color
		/// </summary>
		private byte m_eyecolor = 0;

		/// <summary>
		/// Hold the character hair color
		/// </summary>
		private byte m_hairColor = 0;

		/// <summary>
		/// Hold the character face type
		/// </summary>
		private byte m_facetype = 0;

		/// <summary>
		/// Hold the character hair style
		/// </summary>
		private byte m_hairstyle = 0;

		/// <summary>
		/// Hold the character mood type
		/// </summary>
		private byte m_moodtype = 0;

		/// <summary>
		/// Gets/sets character EyeSize
		/// </summary>
		public byte EyeSize
		{
			get { return m_eyesize; }
			set { m_eyesize = value; }
		}

		/// <summary>
		/// Gets/sets character LipSize
		/// </summary>
		public byte LipSize
		{
			get { return m_lipsize; }
			set { m_lipsize = value; }
		}
		
		/// <summary>
		/// Gets/sets character EyeColor
		/// </summary>
		public byte EyeColor
		{
			get { return m_eyecolor; }
			set { m_eyecolor = value; }
		}
		
		/// <summary>
		/// Gets/sets character HairColor
		/// </summary>
		public byte HairColor
		{
			get { return m_hairColor; }
			set { m_hairColor = value; }
		}
		
		/// <summary>
		/// Gets/sets character FaceType
		/// </summary>
		public byte FaceType
		{
			get { return m_facetype; }
			set { m_facetype = value; }
		}
		
		/// <summary>
		/// Gets/sets character HairStyle
		/// </summary>
		public byte HairStyle
		{
			get { return m_hairstyle; }
			set { m_hairstyle = value; }
		}

		/// <summary>
		/// Gets/sets character MoodType
		/// </summary>
		public byte MoodType
		{
			get { return m_moodtype; }
			set { m_moodtype = value; }
		}
		#endregion

		#region Base Stats

		/// <summary>
		/// get a unmodified char stat value
		/// </summary>
		/// <param name="stat"></param>
		/// <returns></returns>
		public int GetBaseStat(eStat stat)
		{
			switch(stat)
			{
				case eStat.STR: return m_baseStrength;
				case eStat.DEX: return m_baseDexterity;
				case eStat.CON: return m_baseConstitution;
				case eStat.QUI: return m_baseQuickness;
				case eStat.INT: return m_baseIntelligence;
				case eStat.PIE: return m_basePiety;
				case eStat.EMP: return m_baseEmpathy;
				default:		return m_baseCharisma;
			}
		}

		/// <summary>
		/// base consitution values for char stats
		/// </summary>
		private int m_baseConstitution = 0;

		/// <summary>
		/// base Dexterity values for char stats
		/// </summary>
		private int m_baseDexterity = 0;

		/// <summary>
		/// base Strength values for char stats
		/// </summary>
		private int m_baseStrength = 0;

		/// <summary>
		/// base Quickness values for char stats
		/// </summary>
		private int m_baseQuickness = 0;

		/// <summary>
		/// base Intelligence values for char stats
		/// </summary>
		private int m_baseIntelligence = 0;

		/// <summary>
		/// base Piety values for char stats
		/// </summary>
		private int m_basePiety = 0;

		/// <summary>
		/// base Empathy values for char stats
		/// </summary>
		private int m_baseEmpathy = 0;

		/// <summary>
		/// base Charisma values for char stats
		/// </summary>
		private int m_baseCharisma = 0;

		/// <summary>
		/// Gets/sets character constitution
		/// </summary>
		public int BaseConstitution
		{
			get { return m_baseConstitution; }
			set { m_baseConstitution = value; }
		}

		/// <summary>
		/// Gets/sets character dexterity
		/// </summary>
		public int BaseDexterity
		{
			get { return m_baseDexterity; }
			set { m_baseDexterity = value; }
		}

		/// <summary>
		/// Gets/sets character strength
		/// </summary>
		public int BaseStrength
		{
			get { return m_baseStrength; }
			set { m_baseStrength = value; }
		}

		/// <summary>
		/// Gets/sets character quickness
		/// </summary>
		public int BaseQuickness
		{
			get { return m_baseQuickness; }
			set { m_baseQuickness = value; }
		}

		/// <summary>
		/// Gets/sets character intelligence
		/// </summary>
		public int BaseIntelligence
		{
			get { return m_baseIntelligence; }
			set { m_baseIntelligence = value; }
		}

		/// <summary>
		/// Gets/sets character piety
		/// </summary>
		public int BasePiety
		{
			get { return m_basePiety; }
			set { m_basePiety = value; }
		}

		/// <summary>
		/// Gets/sets character empathy
		/// </summary>
		public int BaseEmpathy
		{
			get { return m_baseEmpathy; }
			set { m_baseEmpathy = value; }
		}

		/// <summary>
		/// Gets/sets character charisma
		/// </summary>
		public int BaseCharisma
		{
			get { return m_baseCharisma; }
			set { m_baseCharisma = value; }
		}
		#endregion

		#region Heading/PLAYER_BASE_SPEED

		/// <summary>
		/// Gets or sets the heading of this player
		/// </summary>
		public override int Heading
		{
			get { return base.Heading; }
			set
			{
				base.Heading = value;
				
				if(AttackState && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
				{
					AttackData ad = TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
					if(ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
					{
						//Does the target can be attacked ?
						if(ad.Target != null && IsObjectInFront(ad.Target, 120) && Position.CheckDistance(ad.Target.Position, AttackRange) && m_attackAction != null)
						{
							m_attackAction.Start(1);
						}
					}
				}
			}
		}

		
		/// <summary>
		/// The base speed of the player
		/// </summary>
		public const int PLAYER_BASE_SPEED = 191;


		#endregion
	
		#region DeathTime/ConLostAtDeath/DeathCount
		/// <summary>
		/// Holds the total amount of constitution lost at deaths
		/// </summary>
		protected int m_totalConLostAtDeath = 0;

		/// <summary>
		/// Gets/sets the player efficacy percent
		/// </summary>
		public int TotalConstitutionLostAtDeath
		{
			get { return m_totalConLostAtDeath; }
			set { m_totalConLostAtDeath = value; }
		}

		/// <summary>
		/// Holds the time when the player was killed
		/// </summary>
		protected long m_deathTime = 0;

		/// <summary>
		/// Gets/sets the player death time
		/// </summary>
		public long DeathTime
		{
			get { return m_deathTime; }
			set { m_deathTime = value; }
		}

		/// <summary>
		/// Holds how much time the player die this level
		/// </summary>
		protected byte m_deathCount = 0;

		/// <summary>
		/// Gets/sets the player death time
		/// </summary>
		public byte DeathCount
		{
			get { return m_deathCount; }
			set { m_deathCount = value; }
		}
		#endregion

		#region Class/SlotPosition/AccountId
		/// <summary>
		/// Players class
		/// </summary>
		protected IClassSpec m_class;
		
		private int m_characterClassID;
		
		public int CharacterClassID
		{
			get { return m_characterClassID; }
			set
			{
				m_characterClassID = value;
				if(m_class == null)
				{
					SetCharacterClass(m_characterClassID);	
				}
			}
		}

		/// <summary>
		/// Gets the player's character class
		/// </summary>
		public IClassSpec CharacterClass
		{
			get { return m_class; }
		}

		/// <summary>
		/// Set the character class to a specific one
		/// </summary>
		/// <param name="id">id of the character class</param>
		/// <returns>success</returns>
		public bool SetCharacterClass(int id)
		{
			//Try to find the class from scripts first
			IClassSpec cl = null;
			foreach (Assembly asm in ScriptMgr.Scripts)
			{
				cl = ScriptMgr.FindClassSpec(id, asm);
				if (cl != null)
					break;
			}

			//If it can't be found via script directory, try in gameserver
			if (cl == null)
				cl = ScriptMgr.FindClassSpec(id, Assembly.GetExecutingAssembly());

			if (cl == null)
			{
				if (log.IsErrorEnabled)
					log.Error("No CharacterClass with ID " + id + " found");
				return false;
			}
			m_class = cl;
			CharacterClassID = m_class.ID;
			return true;
		}

		/// <summary>
		/// Hold the slot position of the character
		/// </summary>
		private byte m_slotPosition;	

		/// <summary>
		/// Hold the account unique id of this caracter
		/// </summary>
		private int m_accountID;	

		/// <summary>
		/// The slot of character in account
		/// </summary>
		public byte SlotPosition
		{
			get { return m_slotPosition; }
			set { m_slotPosition = value; }
		}

		/// <summary>
		/// The account id of this character
		/// </summary>
		public int AccountId
		{
			get { return m_accountID; }
			set { m_accountID = value; }
		}
		#endregion
	
		#region Health/Mana/Endurance/Regeneration

		/// <summary>
		/// Starts the health regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StartHealthRegeneration()
		{
			if (ObjectState != eObjectState.Active) return;
			if (m_healthRegenerationTimer.IsAlive) return;
			m_healthRegenerationTimer.Start(HealthRegenerationPeriod);
		}
		/// <summary>
		/// Starts the power regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StartPowerRegeneration()
		{
			if (ObjectState != eObjectState.Active) return;
			if (m_powerRegenerationTimer.IsAlive) return;
			m_powerRegenerationTimer.Start(PowerRegenerationPeriod);
		}
		/// <summary>
		/// Starts the endurance regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
			if (ObjectState != eObjectState.Active) return;
			if (m_enduRegenerationTimer.IsAlive) return;
			m_enduRegenerationTimer.Start(EnduRegenerationPeriod);
		}
		/// <summary>
		/// Stop the health regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StopHealthRegeneration()
		{
			if (m_healthRegenerationTimer == null) return;
			m_healthRegenerationTimer.Stop();
		}
		/// <summary>
		/// Stop the power regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StopPowerRegeneration()
		{
			if (m_powerRegenerationTimer == null) return;
			m_powerRegenerationTimer.Stop();
		}
		/// <summary>
		/// Stop the endurance regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StopEnduranceRegeneration()
		{
			if (m_enduRegenerationTimer == null) return;
			m_enduRegenerationTimer.Stop();
		}

		/// <summary>
		/// Override HealthRegenTimer because if we are not connected anymore
		/// we DON'T regenerate health, even if we are not garbage collected yet!
		/// </summary>
		/// <param name="callingTimer">the timer</param>
		/// <returns>the new time</returns>
		protected override int HealthRegenerationTimerCallback(RegionTimer callingTimer)
		{
			if(Client.ClientState!=GameClient.eClientState.Playing)
				return HealthRegenerationPeriod;
			return base.HealthRegenerationTimerCallback(callingTimer);
		}

		/// <summary>
		/// Override PowerRegenTimer because if we are not connected anymore
		/// we DON'T regenerate mana, even if we are not garbage collected yet!
		/// </summary>
		/// <param name="selfRegenerationTimer">the timer</param>
		/// <returns>the new time</returns>
		protected override int PowerRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if(Client.ClientState != GameClient.eClientState.Playing)
				return PowerRegenerationPeriod;
			return base.PowerRegenerationTimerCallback(selfRegenerationTimer);
		}

		/// <summary>
		/// Override EnduranceRegenTimer because if we are not connected anymore
		/// we DON'T regenerate endurance, even if we are not garbage collected yet!
		/// </summary>
		/// <param name="selfRegenerationTimer">the timer</param>
		/// <returns>the new time</returns>
		protected override int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if(Client.ClientState!=GameClient.eClientState.Playing)
				return EnduRegenerationPeriod;
			return base.EnduranceRegenerationTimerCallback(selfRegenerationTimer);
		}

		/// <summary>
		/// Changes the health
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="changeAmount">the change amount</param>
		/// <param name="generateAggro">does it must generate aggro to changeSource</param>
		/// <returns>the amount really changed</returns>
		public override int ChangeHealth(GameLiving changeSource, int changeAmount, bool generateAggro)
		{
			int oldPercent = HealthPercent;
			int healthChanged = base.ChangeHealth(changeSource, changeAmount, generateAggro);
			if (oldPercent != HealthPercent)
			{
				if (PlayerGroup != null)
					PlayerGroup.UpdateMember(this, false, false);

				UpdatePlayerStatus();
			}
			return healthChanged;
		}

		/// <summary>
		/// Calculates the maximum health for a specific playerlevel and constitution
		/// </summary>
		/// <param name="level">The level of the player</param>
		/// <param name="constitution">The constitution of the player</param>
		/// <returns></returns>
		public virtual int CalculateMaxHealth(int level, int constitution)
		{
			constitution -= 50;
			if (constitution < 0)
				constitution *= 2;
			int hp1 = CharacterClass.BaseHP * level;
			int hp2 = hp1*constitution/10000;
			return Math.Max(1, 20 + hp1/50 + hp2);
		}


		/// <summary>
		/// Changes the mana
		/// </summary>
		/// <param name="changeSource">the source that inited the changes</param>
		/// <param name="changeAmount">the change amount</param>
		/// <returns>the amount really changed</returns>
		public override int ChangeMana(GameObject changeSource, int changeAmount)
		{
			int oldPercent = ManaPercent;
			int manaChanged = base.ChangeMana(changeSource, changeAmount);
			if (oldPercent != ManaPercent)
			{
				if (PlayerGroup != null)
					PlayerGroup.UpdateMember(this, false, false);

				UpdatePlayerStatus();
			}
			return manaChanged;
		}

		/// <summary>
		/// Calculates MaxHealth
		/// </summary>
		/// <returns></returns>
		public virtual int CalculateMaxMana(int level, int manastat)
		{
			int maxpower = 0;
			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				maxpower = (level*5) + (manastat - 50);
			}
			if (maxpower < 0)
				maxpower = 0;
			return maxpower;
		}


		/// <summary>
		/// Gets and set the endurance in percent
		/// </summary>
		public override byte EndurancePercent
		{
			get { return base.EndurancePercent; }
			set
			{
				byte oldPercent = EndurancePercent;
				base.EndurancePercent = value;

				if (oldPercent != EndurancePercent)
				{
					if (PlayerGroup != null )
						PlayerGroup.UpdateMember(this, false, false);

					UpdatePlayerStatus();
				}
			}
		}

		#endregion
	
		#region Level/Experience/LevelSecondStage

		/// <summary>
		/// The maximum level a player can reach
		/// </summary>
		public const int MAX_LEVEL = 50;

		/// <summary>
		/// A table that holds the required XP/Level
		/// </summary>
		public static readonly long[] XPLevel =
			{
				//http://forums.jeuxonline.info/showthread.php?t=441747
				0, // xp to level 1
				50, // xp to level 2
				200, // xp to level 3
				600, // xp to level 4
				1450, // xp to level 5
				4050, // xp to level 6
				9650, // xp to level 7
				22000, // xp to level 8
				51000, // xp to level 9
				115000, // xp to level 10
				256000, // xp to level 11
				380000, // xp to level 12
				560000, // xp to level 13
				800000, // xp to level 14
				1200000, // xp to level 15
				1800000, // xp to level 16
				2700000, // xp to level 17
				3900000, // xp to level 18
				5700000, // xp to level 19
				8400000, // xp to level 20
				12300000, // xp to level 21
				16500000, // xp to level 22
				23000000, // xp to level 23
				30000000, // xp to level 24
				40000000, // xp to level 25
				53000000, // xp to level 26
				70000000, // xp to level 27
				90000000, // xp to level 28
				120000000, // xp to level 29
				160000000, // xp to level 30
				210000000, // xp to level 31
				270000000, // xp to level 32
				350000000, // xp to level 33
				460000000, // xp to level 34
				600000000, // xp to level 35
				790000000, // xp to level 36
				980000000, // xp to level 37
				1200000000, // xp to level 38
				1400000000, // xp to level 39
				1700000000, // xp to level 40
				4300000000, // xp to level 41
				7800000000, // xp to level 42
				9300000000, // xp to level 43
				10800000000, // xp to level 44
				13200000000, // xp to level 45
				15600000000, // xp to level 46
				18900000000, // xp to level 47
				22500000000, // xp to level 48
				25000000000, // xp to level 49
				32000000000, // xp to level 50
				300000000000, // xp to level 51
		};

		/// <summary>
		/// Holds how many XP this player has
		/// </summary>
		protected long m_currentXP;

		/// <summary>
		/// Gets the current xp of this player
		/// The set is only here for Hibernate and must not be used, use GainExperience function instead !
		/// </summary>
		public virtual long Experience
		{
			get { return m_currentXP; } 
			set { m_currentXP = value; }
		}

		/// <summary>
		/// Returns the xp that are needed for the next level
		/// </summary>
		public virtual long ExperienceForNextLevel
		{
			get
			{
				return GameServer.ServerRules.GetExperienceForLevel(Level+1);
			}
		}

		/// <summary>
		/// Returns the xp that were needed for the current level
		/// </summary>
		public virtual long ExperienceForCurrentLevel
		{
			get
			{
				return GameServer.ServerRules.GetExperienceForLevel(Level);
			}
		}

		/// <summary>
		/// Returns the xp that is needed for the second stage of current level
		/// </summary>
		public virtual long ExperienceForCurrentLevelSecondStage
		{
			get { return 1 + ExperienceForCurrentLevel + (ExperienceForNextLevel - ExperienceForCurrentLevel)/2; }
		}

		/// <summary>
		/// Returns how far into the level we have progressed
		/// A value between 0 and 1000 (1 bubble = 100)
		/// </summary>
		public virtual ushort LevelPermill
		{
			get
			{
				//No progress if we haven't even reached current level!
				if (Experience < ExperienceForCurrentLevel)
					return 0;
				//No progess after maximum level
				if (Level > MAX_LEVEL) // needed to get exp after 50
					return 0;
				return (ushort) (1000*(Experience - ExperienceForCurrentLevel)/(ExperienceForNextLevel - ExperienceForCurrentLevel));
			}
		}

		/// <summary>
		/// Gets or sets the level of the player
		/// </summary>
		public override byte Level
		{
			get { return base.Level; }
			set
			{
				int oldLevel = Level;
				base.Level = value;
				if (oldLevel > 0 && Level > oldLevel)
				{
					OnLevelUp(oldLevel);
					Notify(GamePlayerEvent.LevelUp, this);
				}
			}
		}

		/// <summary>
		/// Holds second stage of current level flag
		/// </summary>
		private bool m_isLevelSecondStage;

		/// <summary>
		/// Is this player in second stage of current level
		/// </summary>
		public virtual bool IsLevelSecondStage
		{
			get { return m_isLevelSecondStage; }
			set { m_isLevelSecondStage = value; }
		}

		/// <summary>
		/// Called when the living is gaining experience
		/// </summary>
		/// <param name="exp">base amount of xp to gain</param>
		public void GainExperience(long exp)
		{
			GainExperience(exp, 0, 0, true);
		}

		/// <summary>
		/// Called whenever this player gains experience
		/// </summary>
		/// <param name="expBase">base amount of xp to gain</param>
		/// <param name="expCampBonus">camp bonus to base exp</param>
		/// <param name="expGroupBonus">group bonus to base exp</param>
		/// <param name="sendMessage">should exp gain message be sent</param>
		public virtual void GainExperience(long expBase, long expCampBonus, long expGroupBonus, bool sendMessage)
		{
			Notify(GamePlayerEvent.GainedExperience, this, new GainedExperienceEventArgs(expBase, expCampBonus, expGroupBonus, sendMessage));

			long totalExp = expBase + expCampBonus + expGroupBonus;

			if (IsLevelSecondStage)
			{
				if (Experience + totalExp < ExperienceForCurrentLevelSecondStage)
				{
					totalExp = ExperienceForCurrentLevelSecondStage - m_currentXP;
				}
			}
			else if (Experience + totalExp < ExperienceForCurrentLevel)
			{
				totalExp = ExperienceForCurrentLevel - m_currentXP;
			}

			if (sendMessage && totalExp > 0)
			{
				System.Globalization.NumberFormatInfo format = System.Globalization.NumberFormatInfo.InvariantInfo;
				string totalExpStr = totalExp.ToString("N0", format);
				string expCampBonusStr = "";
				string expGroupBonusStr = "";

				if(expCampBonus > 0)
				{
					expCampBonusStr = " (" + expCampBonus.ToString("N0", format) + " camp bonus)";
				}
				if(expGroupBonus > 0)
				{
					expGroupBonusStr = " (" + expGroupBonus.ToString("N0", format) + " group bonus)";
				}

				Out.SendMessage("You get " + totalExpStr + " experience points." + expCampBonusStr + expGroupBonusStr, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			//DOLConsole.WriteLine("XP="+Experience);
			m_currentXP += totalExp; // force usage of this method, Experience property cannot be set
			//DOLConsole.WriteLine("XP="+Experience+" NL="+ExperienceForNextLevel+" LP="+LevelPermill);

			//Level up
			if (Level >= 5 && CharacterClass.BaseName == CharacterClass.Name)
			{
				if (totalExp > 0)
				{
					Out.SendMessage("You cannot raise to the 6th level until you join an advanced guild!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					Out.SendMessage("Talk to your trainer for more information.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
			else if (Level >= 40 && Level < MAX_LEVEL && !IsLevelSecondStage && Experience >= ExperienceForCurrentLevelSecondStage)
			{
				OnLevelSecondStage ();
				Notify(GamePlayerEvent.LevelSecondStage, this);
				SaveIntoDatabase (); // save char on levelup
			}
			else if (Level < MAX_LEVEL && Experience >= ExperienceForNextLevel)
			{
				Level++;
				SaveIntoDatabase (); // save char on levelup
			}
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Called when this player levels
		/// </summary>
		/// <param name="previouslevel"></param>
		public virtual void OnLevelUp(int previouslevel)
		{
			IsLevelSecondStage = false;

			Out.SendMessage("You raise to level " + Level + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			Out.SendMessage("You have achieved level " + Level + "!", eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);

			if(IsLevelRespecUsed == false) // remove level respec if any
			{
				RespecAmountSingleSkill--;
				IsLevelRespecUsed = true;
			}

			switch(Level)
			{
				// full respec on level 5 since 1.70
				case 5:
					RespecAmountAllSkill++;
					break;

					// single line respec
				case 20:
				case 40:
					RespecAmountSingleSkill++; // Give character their free respecs at 20 and 40
					IsLevelRespecUsed = false;
					break;
			}

			// old hp
			int oldhp = CalculateMaxHealth(previouslevel, BaseConstitution);

			// old power
			int oldpow = 0;
			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				oldpow = CalculateMaxMana(previouslevel, GetBaseStat(CharacterClass.ManaStat));
			}

			// Adjust stats
			bool statsChanged = false;
			for (int i = Level; i > previouslevel; i--)
			{
				if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
				{
					switch(CharacterClass.PrimaryStat)
					{
						case eStat.STR: BaseStrength ++; break;
						case eStat.DEX: BaseDexterity ++; break;
						case eStat.CON: BaseConstitution ++; break;
						case eStat.QUI: BaseQuickness ++; break;
						case eStat.INT: BaseIntelligence ++; break;
						case eStat.PIE: BasePiety ++; break;
						case eStat.EMP: BaseEmpathy ++; break;
						default:		BaseCharisma ++; break;
					}
					statsChanged=true;
				}
				if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6)%2 == 0))
				{ // base level to start adding stats is 6
					switch(CharacterClass.SecondaryStat)
					{
						case eStat.STR: BaseStrength ++; break;
						case eStat.DEX: BaseDexterity ++; break;
						case eStat.CON: BaseConstitution ++; break;
						case eStat.QUI: BaseQuickness ++; break;
						case eStat.INT: BaseIntelligence ++; break;
						case eStat.PIE: BasePiety ++; break;
						case eStat.EMP: BaseEmpathy ++; break;
						default:		BaseCharisma ++; break;
					}
					statsChanged=true;
				}
				if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6)%3 == 0))
				{ // base level to start adding stats is 6
					switch(CharacterClass.TertiaryStat)
					{
						case eStat.STR: BaseStrength ++; break;
						case eStat.DEX: BaseDexterity ++; break;
						case eStat.CON: BaseConstitution ++; break;
						case eStat.QUI: BaseQuickness ++; break;
						case eStat.INT: BaseIntelligence ++; break;
						case eStat.PIE: BasePiety ++; break;
						case eStat.EMP: BaseEmpathy ++; break;
						default:		BaseCharisma ++; break;
					}
					statsChanged=true;
				}
			}

			CharacterClass.OnLevelUp(this);

			// hp upgrade
			int newhp = CalculateMaxHealth(Level, BaseConstitution);
			if (oldhp > 0 && oldhp < newhp)
			{
				Out.SendMessage("Your hits raise by " + (newhp - oldhp) + " points.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			// power upgrade
			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				int newpow = CalculateMaxMana(Level, GetBaseStat(CharacterClass.ManaStat));
				if (newpow > 0 && oldpow < newpow)
				{
					Out.SendMessage("Your power raises by " + (newpow - oldpow) + " points.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}

			if(statsChanged)
			{
				Out.SendMessage("Your stats raise!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			// workaround for starting regeneration
			StartHealthRegeneration();
			StartPowerRegeneration();

			UpdateSpellLineLevels(true);
			RefreshSpecDependendSkills(true);

			// Echostorm - Code for display of new title on level up
			// Get old and current rank titles
			string oldtitle = CharacterClass.GetTitle(previouslevel);
			string currenttitle = CharacterClass.GetTitle(Level);

			// check for difference
			if (oldtitle != currenttitle)
			{
				// Inform player of new title.
				Out.SendMessage("You have attained the rank of " + currenttitle + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			// spec points
			int specpoints = 0;
			for (int i = Level; i > previouslevel; i--)
			{
				specpoints += CharacterClass.SpecPointsMultiplier*i/10;
			}
			if (specpoints > 0)
			{
				Out.SendMessage("You get " + specpoints + " more Specialization Points to spend at this level!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			SkillSpecialtyPoints += specpoints;

			DeathCount = 0;

			if (PlayerGroup != null)
			{
				PlayerGroup.UpdateGroupWindow();
			}
			Out.SendUpdatePlayer(); // Update player level
			Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
			Out.SendCharResistsUpdate();
			Out.SendUpdatePlayerSkills();
			Out.SendUpdatePoints();
			UpdatePlayerStatus();

			// not sure what package this is, but it triggers the mob color update
			Out.SendLevelUpSound();

			// update color on levelup
			if(ObjectState == eObjectState.Active)
				foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendEmoteAnimation(this, eEmote.LvlUp);
				}

			// Reset taskDone per level.
			TaskDone = 0;
		}

		/// <summary>
		/// Called when this player reaches second stage of the current level
		/// </summary>
		public virtual void OnLevelSecondStage ()
		{
			IsLevelSecondStage = true;

			Out.SendMessage ("You raise to level " + Level + " Stage 2!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

			// spec points
			int specpoints = CharacterClass.SpecPointsMultiplier*Level/20;
			if (specpoints > 0)
			{
				Out.SendMessage ("You get " + specpoints + " more Specialization Points to spend at this level!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			SkillSpecialtyPoints += specpoints;
			DeathCount = 0; // ?

			if (PlayerGroup != null)
			{
				PlayerGroup.UpdateGroupWindow ();
			}
			Out.SendUpdatePlayer (); // Update player level
			Out.SendCharStatsUpdate (); // Update Stats and MaxHitpoints
			Out.SendUpdatePlayerSkills ();
			Out.SendUpdatePoints ();
			UpdatePlayerStatus ();

			// not sure what package this is, but it triggers the mob color update
			Out.SendLevelUpSound();

			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendEmoteAnimation (this, eEmote.LvlUp);
			}
		}

		#endregion

		#region RespecAll/RespecLine/IsRespecUsed

		/// <summary>
		/// A table that holds the cost of a respec line in copper
		/// </summary>
		//https://faq.camelot-europe.com/cgi-bin/rightnow_fr.cfg/php/enduser/std_adp.php?p_sid=y4CdTwQh&p_lva=&p_faqid=1564
		private static readonly long [ , ] respecCostTable =
		{
			{1000, -1, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, -1, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, -1, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, -1, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, -1, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, -1, -1, -1, -1, -1, -1, -1, -1},
			{1000, 1000, 2000, -1, -1, -1, -1, -1, -1, -1},
			{1000, 2000, 3000, -1, -1, -1, -1, -1, -1, -1},
			{2000, 5000, 9000, -1, -1, -1, -1, -1, -1, -1},
			{3000, 9000, 17000, -1, -1, -1, -1, -1, -1, -1},
			{6000, 16000, 30000, -1, -1, -1, -1, -1, -1, -1},
			{10000, 26000, 48000, 75000, -1, -1, -1, -1, -1, -1},
			{16000, 40000, 72000, 112000, -1, -1, -1, -1, -1, -1},
			{22000, 56000, 102000, 159000, -1, -1, -1, -1, -1, -1},
			{31000, 78000, 140000, 218000, -1, -1, -1, -1, -1, -1},
			{41000, 103000, 187000, 291000, -1, -1, -1, -1, -1, -1},
			{54000, 135000, 243000, 378000, -1, -1, -1, -1, -1, -1},
			{68000, 171000, 308000, 480000, 652000, -1, -1, -1, -1, -1},
			{85000, 214000, 385000, 600000, 814000, -1, -1, -1, -1, -1},
			{105000, 263000, 474000, 738000, 1001000, -1, -1, -1, -1, -1},
			{128000, 320000, 576000, 896000, 1216000, -1, -1, -1, -1, -1},
			{153000, 383000, 690000, 1074000, 1458000, -1, -1, -1, -1, -1},
			{182000, 455000, 820000, 1275000, 1731000, 2278000, -1, -1, -1, -1},
			{214000, 535000, 964000, 1500000, 2036000, 2679000, -1, -1, -1, -1},
			{250000, 625000, 1125000, 1750000, 2375000, 3125000, -1, -1, -1, -1},
			{289000, 723000, 1302000, 2025000, 2749000, 3617000, -1, -1, -1, -1},
			{332000, 831000, 1497000, 2329000, 3161000, 4159000, -1, -1, -1, -1},
			{380000, 950000, 1710000, 2661000, 3612000, 4752000, -1, -1, -1, -1},
			{432000, 1080000, 1944000, 3024000, 4104000, 5400000, 6696000, -1, -1, -1},
			{488000, 1220000, 2197000, 3417000, 4638000, 6103000, 7568000, -1, -1, -1},
			{549000, 1373000, 2471000, 3844000, 5217000, 6865000, 8513000, -1, -1, -1},
			{615000, 1537000, 3767000, 4305000, 5843000, 7688000, 9533000, -1, -1, -1},
			{686000, 1715000, 3087000, 4802000, 6517000, 8575000, 10633000, -1, -1, -1},
			{762000, 1905000, 3429000, 5335000, 7240000, 9526000, 11813000, 14099000, -1, -1},
			{843000, 2109000, 3796000, 5906000, 8015000, 10546000, 13078000, 15609000, -1, -1},
			{930000, 2327000, 4189000, 6516000, 8844000, 11637000, 14430000, 17222000, -1, -1},
			{1024000, 2560000, 4608000, 7168000, 9728000, 12800000, 15872000, 18944000, -1, -1},
			{1123000, 2807000, 5053000, 7861000, 10668000, 14037000, 17406000, 20776000, -1, -1},
			{1228000, 3070000, 5527000, 8597000, 11668000, 15353000, 19037000, 22722000, -1, -1},
			{1339000, 3349000, 6029000, 9378000, 12728000, 16748000, 20767000, 24787000, 28806000, -1},
			{1458000, 3645000, 6561000, 10206000, 13851000, 18225000, 22599000, 26973000, 31347000, -1},
			{1582000, 3957000, 7123000, 11080000, 15037000, 19786000, 24535000, 29283000, 34032000, -1},
			{1714000, 4286000, 7716000, 12003000, 16290000, 21434000, 26578000, 31722000, 36867000, -1},
			{1853000, 4634000, 8341000, 12976000, 17610000, 23171000, 28732000, 34293000, 39854000, -1},
			{2000000, 5000000, 9000000, 14000000, 19000000, 25000000, 31000000, 37000000, 43000000, 50000000},
		};

		/// <summary>
		/// Calculate how much a respec cost to buy
		/// </summary>
		/// <param name="respecBought">how many respec have ever been bought by the player</param>
		/// <param name="level">the player level</param>
		/// <returns>cost in copper</returns>
		public static long CalculateRespecCost(int respecBought, int level)
		{
			if (level > 50) level = 50;

			int maxRespec = 1 + ((int) (level / 11)) * 2;
			if(level % 11 > 5) maxRespec ++;
			
			respecBought = Math.Min(maxRespec, respecBought);
			
			return respecCostTable[level - 1, respecBought - 1];
		} 

		/// <summary>
		/// Holds amount of full skill respecs
		/// </summary>
		protected int m_respecAmountAllSkill;

		/// <summary>
		/// Holds amount of single-line skill respecs
		/// </summary>
		protected int m_respecAmountSingleSkill;
		
		/// <summary>
		/// Holds level respec usage flag
		/// </summary>
		protected bool m_isLevelRespecUsed;

		/// <summary>
		/// Holds how many respec this player has bought
		/// </summary>
		protected int m_respecBought;

		/// <summary>
		/// Gets/Sets amount of full skill respecs
		/// </summary>
		public int RespecAmountAllSkill
		{
			get { return m_respecAmountAllSkill; }
			set { m_respecAmountAllSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of single-line respecs
		/// </summary>
		public int RespecAmountSingleSkill
		{
			get { return m_respecAmountSingleSkill; }
			set { m_respecAmountSingleSkill = value; }
		}

		/// <summary>
		/// Gets/Sets level respec usage flag
		/// </summary>
		public bool IsLevelRespecUsed
		{
			get { return m_isLevelRespecUsed; }
			set { m_isLevelRespecUsed = value; }
		}

		/// <summary>
		/// Gets/Sets how many respec this player has bought
		/// </summary>
		public int RespecBought
		{
			get { return m_respecBought; }
			set { m_respecBought = value; }
		}
		#endregion
		
		#region Bind/Release/Pray
		/// <summary>
		/// Property that holds tick when the player bind last time
		/// </summary>
		public const string LAST_BIND_TICK = "LastBindTick";

		/// <summary>
		/// The property that holds coor when the player bind last time
		/// </summary>
		protected Point m_bindPosition;

		/// <summary>
		/// Property that holds region when the player bind last time
		/// </summary>
		private Region	m_bindRegion;

		/// <summary>
		/// Property that holds heading when the player bind last time
		/// </summary>
		private int	m_bindHeading;

		/// <summary>
		/// Gets or sets the DB bind position.
		/// NHibernate does not work with struct so we 
		/// use a warper class to save and load the Point struct ...
		/// </summary>
		/// <value>The DB position.</value>
		private DBPoint DBBindPosition
		{
			get { return new DBPoint(m_bindPosition); }
			set { m_bindPosition = new Point(value); }
		}

		/// <summary>
		/// In the db we save the regionID but we must
		/// link the object with the region instance
		/// stored in the regionMgr
		///(it is only used internaly by NHibernate)
		/// </summary>
		/// <value>The region id</value>
		private int BindRegionID
		{
			get { return m_bindRegion != null ? m_bindRegion.RegionID : 0; }
			set { m_bindRegion = WorldMgr.GetRegion(value); }
		}

		/// <summary>
		/// Gets or sets the coor when the player bind last time
		/// </summary>
		public virtual Point BindPosition
		{
			get { return m_bindPosition; }
			set { m_bindPosition = value; }
		}

		/// <summary>
		/// The bind region position of character
		/// </summary>
		public Region BindRegion
		{
			get { return m_bindRegion; }
			set { m_bindRegion = value; }
		}

		/// <summary>
		/// The bind heading position of character
		/// </summary>
		public int BindHeading
		{
			get { return m_bindHeading; }
			set { m_bindHeading = value; }
		}

		/// <summary>
		/// Binds this player to the current location
		/// </summary>
		/// <param name="forced">if true, can bind anywhere</param>
		public virtual void Bind(bool forced)
		{
			Point curPos = Position;
			if (forced)
			{
				BindRegion = Region;
				BindHeading = Heading;
				BindPosition = curPos;
				return;
			}

			if(BindRegion != null)
			{
				String sentence = BindRegion.Description; // Camelot Hills
				IList currentAreas = AreaMgr.GetAreasOfSpot(BindRegion.GetZone(Position), Position);
				if(currentAreas != null && currentAreas.Count >= 1)
				{
					sentence.Insert(0, ((AbstractArea)currentAreas[0]).Description + " in "); // Prydwen Keep in Camelot Hills
				}

				Out.SendMessage("Last Bind Point : "+sentence+".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			if (!Alive)
			{
				Out.SendMessage("You can not bind while dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			long lastBindTick = TempProperties.getLongProperty(LAST_BIND_TICK, 0L);
			long changeTime = Region.Time - lastBindTick;
			if (changeTime < 60000) //60 second rebind timer
			{
				Out.SendMessage("You must wait " + (1 + (60000 - changeTime) / 1000) + " seconds to bind again!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			IList objs = GameServer.Database.SelectObjects(typeof (BindPoint), Expression.Eq("Region",Region.RegionID));
			bool bound = false;
			if (objs.Count > 0)
			{
				foreach (BindPoint bp in objs)
				{
					if (!GameServer.ServerRules.IsAllowedToBind(this, bp)) continue;
					if (curPos.CheckDistance(new Point(bp.X, bp.Y, bp.Z), bp.Radius))
					{
						TempProperties.setProperty(LAST_BIND_TICK, Region.Time);

						bound = true;
						BindRegion = Region;
						BindHeading = Heading;
						BindPosition = curPos;
						break;
					}
				}
			}
			if (bound)
			{
				if (!IsMoving)
				{
					foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendEmoteAnimation(this, eEmote.Bind);
				}
				Out.SendMessage("You are now bound to this location.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage("You cannot bind here!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// tick when player is died
		/// </summary>
		protected int m_deathTick;

		/// <summary>
		/// choosed the player to release as soon as possible?
		/// </summary>
		protected bool m_automaticRelease = false;

		/// <summary>
		/// The release timer for this player
		/// </summary>
		protected RegionTimer m_releaseTimer;

		/// <summary>
		/// Stops release timer and closes timer window
		/// </summary>
		public void StopReleaseTimer ()
		{
			Out.SendCloseTimerWindow();
			if (m_releaseTimer != null)
			{
				m_releaseTimer.Stop();
				m_releaseTimer = null;
			}
		}

		/// <summary>
		/// minimum time to wait before release is possible in seconds
		/// </summary>
		protected const int RELEASE_MINIMUM_WAIT = 10;

		/// <summary>
		/// max time before auto release in seconds
		/// </summary>
		protected const int RELEASE_TIME = 900;

		/// <summary>
		/// The property name that is set when releasing to another region
		/// </summary>
		public const string RELEASING_PROPERTY = "releasing";

		/// <summary>
		/// The current after-death player release type
		/// </summary>
		public enum eReleaseType
		{
			/// <summary>
			/// Normal release to the bind point using /release command and 10sec delay after death
			/// </summary>
			Normal,
			/// <summary>
			/// Release to the players home city
			/// </summary>
			City,
			/// <summary>
			/// Release to the current location
			/// </summary>
			Duel,
		}

		/// <summary>
		/// The current release type
		/// </summary>
		protected eReleaseType m_releaseType = eReleaseType.Normal;

		/// <summary>
		/// Gets the player's current release type.
		/// </summary>
		public eReleaseType ReleaseType
		{
			get { return m_releaseType; }
		}

		/// <summary>
		/// Releases this player after death ... subtracts xp etc etc...
		/// </summary>
		/// <param name="releaseCommand">The type of release used for this player</param>
		/// <param name="forced">if true, will release even if not dead</param>
		public virtual void Release(eReleaseType releaseCommand, bool forced)
		{
			if (Alive)
			{
				Out.SendMessage("You are not dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!forced)
			{
				if (m_releaseType == eReleaseType.Duel)
				{
					Out.SendMessage("You can't alter your release on a duel death!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				m_releaseType = releaseCommand;
				// we use realtime, because timer window is realtime
				int diff = m_deathTick - Environment.TickCount + RELEASE_MINIMUM_WAIT*1000;
				if (diff >= 1000)
				{
					if (m_automaticRelease)
					{
						m_automaticRelease = false;
						m_releaseType = eReleaseType.Normal;
						Out.SendMessage("You will no longer release automatically. (" + diff/1000 + " more seconds)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					m_automaticRelease = true;
					switch (releaseCommand)
					{
						default:
						{
							Out.SendMessage("You will now release automatically in " + diff/1000 + " more seconds!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
						case eReleaseType.City:
						{
							Out.SendMessage("You will now release automatically to your home city in " + diff/1000 + " more seconds!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return;
						}
					}
				}
			}
			else
			{
				m_releaseType = releaseCommand;
			}

			Region relRegion;
			Point relPos;
			int relHeading;
			switch (m_releaseType)
			{
				case eReleaseType.Duel:
					relRegion = Region;
					relPos = new Point(Position.X, Position.Y, Position.Z);
					relHeading = 2048;
					break;

				case eReleaseType.City:
					if (Realm == (byte)eRealm.Hibernia)
					{
						relRegion = WorldMgr.GetRegion(201); // Tir Na Nog
						relPos = new Point(8192 + 15780, 8192 + 22727, 7060);
					}
					else if (Realm == (byte)eRealm.Midgard)
					{
						relRegion = WorldMgr.GetRegion(101); // Jordheim
						relPos = new Point(8192 + 24664, 8192 + 21402, 8759);
					}
					else
					{
						relRegion = WorldMgr.GetRegion(10); // City of Camelot
						relPos = new Point(8192 + 26315, 8192 + 21177, 8256);
					}
					relHeading = 2048;
					break;

				default:
					relRegion = BindRegion;
					relPos = BindPosition;
					relHeading = BindHeading;
					break;
			}

			Out.SendMessage("You release your corpse unto death.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
			Out.SendCloseTimerWindow();
			if (m_releaseTimer != null)
			{
				m_releaseTimer.Stop();
				m_releaseTimer = null;
			}

			if (Realm != (byte)eRealm.None && Level > 5)
			{
				// actual lost exp, needed for 2nd stage deaths
				long lostExp = Experience;
				long lastDeathExpLoss = TempProperties.getLongProperty(DEATH_EXP_LOSS_PROPERTY, 0);
				TempProperties.removeProperty(DEATH_EXP_LOSS_PROPERTY);

				GainExperience(-lastDeathExpLoss, 0, 0, false);
				lostExp -= Experience;

				// raise only the gravestone if xp has to be stored in it
				if (lostExp > 0)
				{
					// find old gravestone of player and remove it
					GameGravestone oldgrave = GravestoneMgr.GetPlayerGravestone(this);
					if (oldgrave != null)
					{
						oldgrave.XPValue = 0;
						oldgrave.RemoveFromWorld();
					}
					
					GameGravestone gravestone = GravestoneMgr.CreateGravestone(this, lostExp);
					gravestone.AddToWorld();

					Out.SendMessage("A grave was erected where you were slain.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					Out.SendMessage("Return to /pray at your grave to regain experience.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
				}
			}

			int deathConLoss = TempProperties.getIntProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, 0); // get back constitution lost at death
			if(deathConLoss > 0)
			{
				TotalConstitutionLostAtDeath += deathConLoss;
				Out.SendCharStatsUpdate();
				Out.SendMessage("You've lost some constitution, go to a healer to have it restored!", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
			}

			//Update health&sit state first!
			ChangeHealth(this, MaxHealth, false);
			StartPowerRegeneration();
			StartEnduranceRegeneration();

			Region region = null;
			if (BindRegion != null && BindRegion.GetZone(BindPosition) != null)
			{
				Out.SendMessage("Your surroundings suddenly change!", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage("You have no valid bindpoint! Releasing here instead!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				Bind(true);
			}

			int oldRegionID = Region.RegionID;

			//Call MoveTo after new GameGravestone(this...
			//or the GraveStone will be located at the player's bindpoint
			MoveTo(relRegion, relPos, (ushort)relHeading);
			//It is enough if we revive the player on this client only here
			//because for other players the player will be removed in the MoveTo
			//method and added back again (if in view) with full health ... so no
			//revive needed for others...
			Out.SendPlayerRevive(this);
			//			Out.SendUpdatePlayer();
			Out.SendUpdatePoints();

			//Set property indicating that we are releasing to another region; used for Released event
			if(oldRegionID != BindRegionID)
				TempProperties.setProperty(RELEASING_PROPERTY, true);
			else
			{
				// fire the player revive event
				Notify(GamePlayerEvent.Revive, this);
				Notify(GamePlayerEvent.Released, this);
			}

			TempProperties.removeProperty(DEATH_CONSTITUTION_LOSS_PROPERTY);

			//Reset last valide position array to prevent /stuck avec /release
			lock (m_lastUniqueLocations)
			{
				for(int i = 0; i < m_lastUniqueLocations.Length; i++)
				{
					GameLocation loc = m_lastUniqueLocations[i];
					loc.Position = Position;
					loc.Heading = (ushort)Heading;
					loc.Region = Region;
				}
			}
		}

		/// <summary>
		/// helper state var for different release phases
		/// </summary>
		private int m_releasePhase = 0;

		/// <summary>
		/// callback every second to control realtime release
		/// </summary>
		/// <param name="callingTimer"></param>
		/// <returns></returns>
		protected virtual int ReleaseTimerCallback(RegionTimer callingTimer)
		{
			if (Alive)
				return 0;
			int diffToRelease = Environment.TickCount - m_deathTick;
			if (m_automaticRelease && diffToRelease > RELEASE_MINIMUM_WAIT*1000)
			{
				Release(m_releaseType, true);
				return 0;
			}
			diffToRelease = (RELEASE_TIME*1000 - diffToRelease)/1000;
			if (diffToRelease <= 0)
			{
				Release(m_releaseType, true);
				return 0;
			}
			if (m_releasePhase <= 1 && diffToRelease <= 10 && diffToRelease >= 8)
			{
				Out.SendMessage("You will autorelease in 10 seconds.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_releasePhase = 2;
			}
			if (m_releasePhase == 0 && diffToRelease <= 30 && diffToRelease >= 28)
			{
				Out.SendMessage("You will autorelease in 30 seconds.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_releasePhase = 1;
			}
			return 1000;
		}

		/// <summary>
		/// The timer that will be started when the player wants to pray
		/// </summary>
		protected PrayAction m_prayAction;
		/// <summary>
		/// The delay to wait until xp is regained, in milliseconds
		/// </summary>
		protected const int PRAY_DELAY = 5000;
		/// <summary>
		/// Property that saves experience lost on last death
		/// </summary>
		public const string DEATH_EXP_LOSS_PROPERTY = "death_exp_loss";
		/// <summary>
		/// Property that saves condition lost on last death
		/// </summary>
		public const string DEATH_CONSTITUTION_LOSS_PROPERTY = "death_con_loss";

		/// <summary>
		/// Gets the praying-state of this living
		/// </summary>
		public virtual bool PrayState
		{
			get { return m_prayAction != null && m_prayAction.IsAlive; }
		}

		/// <summary>
		/// Prays on a gravestone for XP!
		/// </summary>
		public virtual void Pray()
		{
			if (!Alive)
			{
				Out.SendMessage("You can't pray now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			GameGravestone gravestone = TargetObject as GameGravestone;
			if (gravestone == null)
			{
				Out.SendMessage("You need to target a grave at which to pray!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (gravestone != GravestoneMgr.GetPlayerGravestone(this))
			{
				Out.SendMessage("Select your gravestone to pray!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!Position.CheckSquareDistance(gravestone.Position, 2000*2000))
			{
				Out.SendMessage("You must get closer to your grave and sit to pray!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (IsMoving)
			{
				Out.SendMessage("You must be standing still to pray.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (PrayState)
			{
				Out.SendMessage("You are already praying!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (m_prayAction != null)
				m_prayAction.Stop();
			m_prayAction = new PrayAction(this, gravestone);
			m_prayAction.Start(PRAY_DELAY);

			Sit(true);
			Out.SendMessage("You begin your prayers!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendEmoteAnimation(this, eEmote.Pray);
		}

		/// <summary>
		/// Stop praying; used when player changes target
		/// </summary>
		public void PrayTimerStop()
		{
			if (!PrayState)
				return;
			m_prayAction.Stop();
			m_prayAction = null;
		}

		/// <summary>
		/// The timed pray action
		/// </summary>
		protected class PrayAction : RegionAction
		{
			/// <summary>
			/// The gravestone player is plraying at
			/// </summary>
			protected readonly GameGravestone m_gravestone;

			/// <summary>
			/// Constructs a new pray action
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="grave">The pray grave stone</param>
			public PrayAction(GamePlayer actionSource, GameGravestone grave) : base(actionSource)
			{
				if (grave == null)
					throw new ArgumentNullException("grave");
				m_gravestone = grave;
			}

			/// <summary>
			/// Callback method for the pray-timer
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				long xp = m_gravestone.XPValue;
				m_gravestone.XPValue = 0;

				if (xp > 0)
				{
					player.Out.SendMessage("You pray at your grave and gain back experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					player.GainExperience(xp, 0, 0, false);
				}
				m_gravestone.RemoveFromWorld();
			}
		}

		/// <summary>
		/// Called when player revive
		/// </summary>
		public virtual void OnRevive(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if(player.Level > 5)
			{
				SpellLine Line = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
				if (Line == null) return;
				IList spells = SkillBase.GetSpellList(Line.KeyName);
				if (spells == null) return;
				foreach (Spell spell in spells)
				{
					if (spell.Name == "Resurrection Illness")
					{
						ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(player, spell, Line);
						if (spellHandler == null)
							player.Out.SendMessage(spell.Name + " not implemented yet (" + spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else
							spellHandler.StartSpell(player);
						break;
					}
				}
			}
			GameEventMgr.RemoveHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
		}

		#endregion
	
		#region Realm-/Bount-/Skillpoints...

		/// <summary>
		/// Character's bounty points
		/// </summary>
		protected long m_bntyPts;
		/// <summary>
		/// Character's realm points
		/// </summary>
		protected long m_realmPts;
		/// <summary>
		/// Character's skill points
		/// </summary>
		protected int m_skillSpecPts;
		/// <summary>
		/// Character's realm special points
		/// </summary>
		protected int m_realmSpecPts;
		/// <summary>
		/// Character's realm rank
		/// </summary>
		protected int m_realmLevel;

		/// <summary>
		/// Gets/sets player bounty points
		/// </summary>
		public long BountyPoints
		{
			get { return m_bntyPts; }
			set { m_bntyPts = value; }
		}

		/// <summary>
		/// Gets/sets player realm points
		/// </summary>
		public long RealmPoints
		{
			get { return m_realmPts; }
			set { m_realmPts = value; }
		}

		/// <summary>
		/// Gets/sets player skill specialty points
		/// </summary>
		public int SkillSpecialtyPoints
		{
			get { return m_skillSpecPts; }
			set { m_skillSpecPts = value; }
		}

		/// <summary>
		/// Gets/sets player realm specialty points
		/// </summary>
		public int RealmSpecialtyPoints
		{
			get { return m_realmSpecPts; }
			set { m_realmSpecPts = value; }
		}

		/// <summary>
		/// Gets/sets player realm rank
		/// </summary>
		public int RealmLevel
		{
			get { return m_realmLevel; }
			set { m_realmLevel = value; }
		}

		/// <summary>
		/// Holds all realm rank names
		/// </summary>
		public static readonly string[][] REALM_RANK_NAMES =
			{
				new string[]
				{
					"realm0"
				},
				// alb
				new string[]
				{
					"Guardian",
					"Warder",
					"Myrmidon",
					"Gryphon Knight",
					"Eagle Knight",
					"Phoenix Knight",
					"Alerion Knight",
					"Unicorn Knight",
					"Lion Knight",
					"Dragon Knight",
			},
				// Mid
				new string[]
				{
					"Skiltvakten",
					"Isen Vakten",
					"Flammen Vakten",
					"Elding Vakten",
					"Stormur Vakten",
					"Isen Herra",
					"Flammen Herra",
					"Elding Herra",
					"Stormur Herra",
					"Einherjar",
			},
				// Hib
				new string[]
				{
					"Savant",
					"Cosantoir",
					"Brehon",
					"Grove Protector",
					"Raven Ardent",
					"Silver Hand",
					"Thunderer",
					"Gilded Spear",
					"Tiarna",
					"Emerald Ridere",
			},
		};

		/// <summary>
		/// Gets player realm rank name
		/// </summary>
		public string RealmTitle
		{
			get
			{
				if(Realm >= REALM_RANK_NAMES.Length)
					return "unknown realm";

				string[] rankNames = REALM_RANK_NAMES[Realm];
				int rank = m_realmLevel / 10;
				if(rank >= rankNames.Length)
					rank = rankNames.Length - 1;

				return rankNames[rank];
			}
		}

		/// <summary>
		/// Called when this living gains realm points
		/// </summary>
		/// <param name="amount">The amount of realm points gained</param>
		public virtual void GainRealmPoints(long amount)
		{
			Notify(GamePlayerEvent.GainedRealmPoints, this, new GainedRealmPointsEventArgs(amount));
		
			RealmPoints += amount;
			if (Guild != null) Guild.RealmPoints += amount;

			Out.SendMessage("You get " + amount.ToString() + " realm points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			//"You earn 4 extra realm points!"

			if(RealmPoints >= CalculateRPsFromRealmLevel(m_realmLevel+1) && m_realmLevel < 99)
			{
				RealmLevel++;
				RealmSpecialtyPoints++;
				Out.SendUpdatePlayer();
				Out.SendMessage("You have gained a realm level!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if(m_realmLevel % 10 == 0)
				{
					Out.SendUpdatePlayerSkills();
					Out.SendMessage("You have gained a new rank and a new realm title!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					Out.SendMessage("Your new realm title is " + RealmTitle + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					Out.SendMessage("You gain a +" + m_realmLevel / 10 + " bonus to all specializations!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					foreach (GamePlayer plr in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
						plr.Out.SendLivingDataUpdate(this, true);
				}
			}
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount">The amount of realm points loosed</param>
		public bool RemoveBountyPoints(long amount)
		{
			return RemoveBountyPoints(amount,null);
		}

		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount">The amount of realm points loosed</param>
		public bool RemoveBountyPoints(long amount,string str)
		{
			return RemoveBountyPoints(amount, str,eChatType.CT_Say,eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount">The amount of realm points loosed</param>
		public bool RemoveBountyPoints(long amount,string str,eChatType type,eChatLoc loc)
		{
			if (BountyPoints < amount)
				return false;
			BountyPoints -= amount;
			Out.SendUpdatePoints();
			if (str != null && amount != 0)
				Out.SendMessage(str,type,loc);
			return true;
		}

		/// <summary>
		/// Called when this living gains bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public virtual void GainBountyPoints(long amount)
		{
			BountyPoints += amount;
			if (Guild != null) Guild.BountyPoints += amount;
			Out.SendMessage("You get " + amount.ToString() + " bounty points!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Holds realm points needed for special realm level
		/// </summary>
		protected static readonly long[] REALMPOINTS_FOR_LEVEL =
			{
				0,	// for level 0
				0,	// for level 1
				25,	// for level 2
				125,	// for level 3
				350,	// for level 4
				750,	// for level 5
				1375,	// for level 6
				2275,	// for level 7
				3500,	// for level 8
				5100,	// for level 9
				7125,	// for level 10
				9625,	// for level 11
				12650,	// for level 12
				16250,	// for level 13
				20475,	// for level 14
				25375,	// for level 15
				31000,	// for level 16
				37400,	// for level 17
				44625,	// for level 18
				52725,	// for level 19
				61750,	// for level 20
				71750,	// for level 21
				82775,	// for level 22
				94875,	// for level 23
				108100,	// for level 24
				122500,	// for level 25
				138125,	// for level 26
				155025,	// for level 27
				173250,	// for level 28
				192850,	// for level 29
				213875,	// for level 30
				236375,	// for level 31
				260400,	// for level 32
				286000,	// for level 33
				313225,	// for level 34
				342125,	// for level 35
				372750,	// for level 36
				405150,	// for level 37
				439375,	// for level 38
				475475,	// for level 39
				513500,	// for level 40
				553500,	// for level 41
				595525,	// for level 42
				639625,	// for level 43
				685850,	// for level 44
				734250,	// for level 45
				784875,	// for level 46
				837775,	// for level 47
				893000,	// for level 48
				950600,	// for level 49
				1010625,	// for level 50
				1073125,	// for level 51
				1138150,	// for level 52
				1205750,	// for level 53
				1275975,	// for level 54
				1348875,	// for level 55
				1424500,	// for level 56
				1502900,	// for level 57
				1584125,	// for level 58
				1668225,	// for level 59
				1755250,	// for level 60
				1845250,	// for level 61
				1938275,	// for level 62
				2034375,	// for level 63
				2133600,	// for level 64
				2236000,	// for level 65
				2341625,	// for level 66
				2450525,	// for level 67
				2562750,	// for level 68
				2678350,	// for level 69
				2797375,	// for level 70
				2919875,	// for level 71
				3045900,	// for level 72
				3175500,	// for level 73
				3308725,	// for level 74
				3445625,	// for level 75
				3586250,	// for level 76
				3730650,	// for level 77
				3878875,	// for level 78
				4030975,	// for level 79
				4187000,	// for level 80
				4347000,	// for level 81
				4511025,	// for level 82
				4679125,	// for level 83
				4851350,	// for level 84
				5027750,	// for level 85
				5208375,	// for level 86
				5393275,	// for level 87
				5582500,	// for level 88
				5776100,	// for level 89
				5974125,	// for level 90
				6176625,	// for level 91
				6383650,	// for level 92
				6595250,	// for level 93
				6811475,	// for level 94
				7032375,	// for level 95
				7258000,	// for level 96
				7488400,	// for level 97
				7723625,	// for level 98
				7963725,	// for level 99
				8208750,	// for level 100
		};

		/// <summary>
		/// Calculates amount of RealmPoints needed for special realm level
		/// </summary>
		/// <param name="realmLevel">realm level</param>
		/// <returns>amount of realm points</returns>
		protected virtual long CalculateRPsFromRealmLevel(int realmLevel)
		{
			if(realmLevel < REALMPOINTS_FOR_LEVEL.Length)
				return REALMPOINTS_FOR_LEVEL[realmLevel];

			// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
			return (long)(25.0/3.0*(realmLevel*realmLevel*realmLevel) - 25.0/2.0*(realmLevel*realmLevel) + 25.0/6.0*realmLevel);
		}

		/// <summary>
		/// Calculates realm level from realm points. SLOW.
		/// </summary>
		/// <param name="realmPoints">amount of realm points</param>
		/// <returns>realm level: RR5L3 = 43, RR1L2 = 2; capped at 99</returns>
		protected virtual int CalculateRealmLevelFromRPs(long realmPoints)
		{
			if(realmPoints == 0)
				return 0;

			int i = REALMPOINTS_FOR_LEVEL.Length-1;
			for(; i > 0; i--)
			{
				if(REALMPOINTS_FOR_LEVEL[i] <= realmPoints)
					break;
			}

			if(i > 99)
				return 99;
			return i;


			// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=30
			//			double z = Math.Pow(1620.0 * realmPoints + 15.0 * Math.Sqrt(-1875.0 + 11664.0 * realmPoints*realmPoints), 1.0/3.0);
			//			double rr = z / 30.0 + 5.0 / 2.0 / z + 0.5;
			//			return Math.Min(99, (int)rr);
		}

		/// <summary>
		/// Realm point value of this player
		/// </summary>
		public virtual int RealmPointsValue
		{
			// TODO: correct formula!
			get
			{
				int level = Math.Max(0, Level-20);
				if(level == 0)
					return Math.Max(1, RealmLevel);

				return Math.Max(1, level * level * level / 30 + RealmLevel);
			}
		}

		/// <summary>
		/// Bounty point value of this player
		/// </summary>
		public virtual int BountyPointsValue
		{
			// TODO: correct formula!
			get { return (int)(1 + Level*0.6); }
		}

		#endregion
	
		#region Guild

		/// <summary>
		/// Gets or sets the guildname of this living (NHibernate fix)
		/// </summary>
		public override string GuildName
		{
			get { return m_guild != null ? m_guild.GuildName : ""; }
			set { m_guild = GuildMgr.GetGuildByName(value); }
		}

		/// <summary>
		/// Hold the player's guild
		/// </summary>
		private Guild m_guild;
		
		/// <summary>
		/// Gets / set the player's guild
		/// </summary>
		public Guild Guild
		{
			get { return m_guild; }
			set
			{
				m_guild = value;
				
				//update guild name for all players if client is playing
				if(ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					BroadcastCreate();
				}	
			}
		}

		/// <summary>
		/// Hold the player's guild rank (0 to 9)
		/// </summary>
		private byte m_guildRank;

		/// <summary>
		/// Gets or sets the player's guild rank
		/// </summary>
		public byte GuildRank
		{
			get { return m_guildRank; }
			set { m_guildRank = value; }
		}

		/// <summary>
		/// Hold the player's guild flag status
		/// </summary>
		private bool m_flagGuildName = false;
		
		/// <summary>
		/// Gets or sets the player's guild flag
		/// </summary>
		public bool GuildNameFlag
		{
			get { return m_flagGuildName; }
			set { m_flagGuildName = value; }
		}
		#endregion
	
		#region Money/UseSlot/ApplyPoison

		/// <summary>
		/// The money of the player in copper
		/// </summary>
		private long m_money;

		/// <summary>
		/// Money in copper
		/// </summary>
		public long Money
		{
			get { return m_money; }
			set { m_money = value; }
		}

		/// <summary>
		/// Called when this player receives a trade item
		/// </summary>
		/// <param name="source">the source of the item</param>
		/// <param name="item">the item</param>
		/// <returns>true to accept, false to deny the item</returns>
		public virtual bool ReceiveTradeItem(GamePlayer source, GenericItem item)
		{
			if (source == null || item == null || source == this)
				return false;
			lock (this)
			{
				lock (source)
				{
					if((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
					{
						if(TradeWindow != null)
						{
							GamePlayer partner = TradeWindow.Partner;
							if(partner == null)
							{
								source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else if(source.TradeWindow != null)
						{
							GamePlayer sourceTradePartner = source.TradeWindow.Partner;
							if(sourceTradePartner == null)
							{
								source.Out.SendMessage("You are still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						return false;
					}

					if (!source.TradeWindow.AddItemToTrade(item))
					{
						source.Out.SendMessage("You can't trade this item!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return true;
				}
			}
		}

		/// <summary>
		/// Called when the player receives trade money
		/// </summary>
		/// <param name="source">the source</param>
		/// <param name="money">the money value</param>
		/// <returns>true to accept, false to deny</returns>
		public virtual bool ReceiveTradeMoney(GamePlayer source, long money)
		{
			if (source == null || source == this || money == 0)
				return false;

			lock (this)
			{
				lock (source)
				{
					if((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
					{
						if(TradeWindow != null)
						{
							GamePlayer partner = TradeWindow.Partner;
							if(partner == null)
							{
								source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else if(source.TradeWindow != null)
						{
							GamePlayer sourceTradePartner = source.TradeWindow.Partner;
							if(sourceTradePartner == null)
							{
								source.Out.SendMessage("You are still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						return false;
					}

					source.TradeWindow.AddMoneyToTrade(money);
					return true;
				}
			}
		}

		/// <summary>
		/// Adds money to this player
		/// </summary>
		/// <param name="money">money to add</param>
		public virtual void AddMoney(long money)
		{
			AddMoney(money, null, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Adds money to this player
		/// </summary>
		/// <param name="money">money to add</param>
		/// <param name="messageFormat">null if no message or "text {0} text"</param>
		public virtual void AddMoney(long money, string messageFormat)
		{
			AddMoney(money, messageFormat, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Adds money to this player
		/// </summary>
		/// <param name="money">money to add</param>
		/// <param name="messageFormat">null if no message or "text {0} text"</param>
		/// <param name="ct">message chat type</param>
		/// <param name="cl">message chat location</param>
		public virtual void AddMoney(long money, string messageFormat, eChatType ct, eChatLoc cl)
		{
			Money += money;

			Out.SendUpdateMoney();

			if (messageFormat != null && money != 0)
			{
				Out.SendMessage(string.Format(messageFormat, DOL.GS.Money.GetString(Money)), ct, cl);
			}
		}

		/// <summary>
		/// Removes money from the player
		/// </summary>
		/// <param name="money">money value to subtract</param>
		/// <returns>true if successfull, false if player doesn't have enough money</returns>
		public virtual bool RemoveMoney(long money)
		{
			return RemoveMoney(money, null, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Removes money from the player
		/// </summary>
		/// <param name="money">money value to subtract</param>
		/// <param name="messageFormat">null if no message or "text {0} text"</param>
		/// <returns>true if successfull, false if player doesn't have enough money</returns>
		public virtual bool RemoveMoney(long money, string messageFormat)
		{
			return RemoveMoney(money, messageFormat, eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Removes money from the player
		/// </summary>
		/// <param name="money">money value to subtract</param>
		/// <param name="messageFormat">null if no message or "text {0} text"</param>
		/// <param name="ct">message chat type</param>
		/// <param name="cl">message chat location</param>
		/// <returns>true if successfull, false if player doesn't have enough money</returns>
		public virtual bool RemoveMoney(long money, string messageFormat, eChatType ct, eChatLoc cl)
		{
			if (money > Money)
				return false;

			Money -= money;

			Out.SendUpdateMoney();

			if (messageFormat != null && money != 0)
			{
				Out.SendMessage(string.Format(messageFormat, DOL.GS.Money.GetString(Money)), ct, cl);
			}
			return true;
		}

		/// <summary>
		/// Called when the player uses an inventory in a slot
		/// eg. by clicking on the icon in the qickbar dragged from a slot
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="type">Which /use command was used (0=simple click on icon, 1=use, 2=/use2)</param>
		public virtual void UseSlot(int slot, int type)
		{	
			lock(Inventory)
			{
				GenericItem useItem = Inventory.GetItem((eInventorySlot)slot) as GenericItem;
				if(useItem == null)
				{
					if((slot >= Slot.FIRSTQUIVER) && (slot <= Slot.FOURTHQUIVER))
					{
						Out.SendMessage("The quiver slot "+ (slot-(Slot.FIRSTQUIVER)+1) +" is empty!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						// don't allow using empty slots
						Out.SendMessage("Illegal source object. Readied "+slot, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}

				useItem.OnItemUsed((byte)type);

				// notify event handlers about used slot
				Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
			}
		}

		/// <summary>
		/// Apply poison to weapon
		/// </summary>
		/// <param name="poisonPotion"></param>
		/// <param name="toItem"></param>
		/// <returns>true if applied</returns>
		public bool ApplyPoison(Poison poisonPotion, Weapon toItem)
		{
			if(poisonPotion == null || toItem == null) return false;
			int envenomSpec = GetModifiedSpecLevel(Specs.Envenom);
			if(envenomSpec < 1)
			{
				Out.SendMessage("You have no skill to use this poisons.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if(!HasAbilityToUseItem(toItem) )
			{
				Out.SendMessage("You can't poison this weapon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if(envenomSpec < poisonPotion.Level)
			{
				Out.SendMessage("You can't use this poison.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if(InCombat)
			{
				Out.SendMessage("You have been in combat recently and can't apply a poison yet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if(toItem.ChargeEffectType != eMagicalEffectType.NoEffect && toItem.ChargeEffectType != eMagicalEffectType.PoisonEffect)
			{
				Out.SendMessage(string.Format("You can't poison your {0}!", toItem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			//			Apply poison effect to weapon
			toItem.ChargeEffectType = eMagicalEffectType.PoisonEffect;
			
            //removed due to poisons don't have charges
            //toItem.Charge = poisonPotion.Charge;
			//toItem.MaxCharge = poisonPotion.MaxCharge;
			toItem.ChargeSpellID = poisonPotion.SpellID;
			((GamePlayerInventory)Inventory).RemoveCountFromStack(poisonPotion, 1);
			Out.SendMessage(string.Format("You apply {0} to {1}.", poisonPotion.Name, toItem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return true;
		}

		#endregion
		
		#region Serialized skills

		private string m_abilitiesString="";	// comma separated string of ability keynames and levels eg "sprint,0,evade,1"
		private string m_specsString="";			// comma separated string of spec keynames and levels like "earth_magic,5,slash,10"
		private string m_spellLinesString="";		// serialized string of spell lines and levels like "Spirit Animation|5;Vivification|7"
		private string m_stylesString="";			// comma separated string of style id's
		private string m_disabledSpellsString="";
		private string m_disabledAbilitiesString="";

		/// <summary>
		/// The abilities of character
		/// </summary>
		public string SerializedAbilities
		{
			get { return m_abilitiesString; }
			set { m_abilitiesString = value; }
		}

		/// <summary>
		/// The specs of character
		/// </summary>
		public string SerializedSpecs
		{
			get { return m_specsString; }
			set { m_specsString = value; }
		}

		/// <summary>
		/// the spell lines of character
		/// </summary>
		public string SerializedSpellLines
		{
			get { return m_spellLinesString; }
			set { m_spellLinesString = value; }
		}

		/// <summary>
		/// The Styles of character
		/// </summary>
		public string Styles
		{
			get { return m_stylesString; }
			set { m_stylesString = value; }
		}

		/// <summary>
		/// The spells unallowed to character
		/// </summary>
		public string DisabledSpells
		{
			get { return m_disabledSpellsString; }
			set { m_disabledSpellsString = value; }
		}

		/// <summary>
		/// The abilities unallowed to character
		/// </summary>
		public string DisabledAbilities
		{
			get { return m_disabledAbilitiesString; }
			set { m_disabledAbilitiesString = value; }
		}

		/// <summary>
		/// Saves the player's skills
		/// </summary>
		protected virtual void SaveSkillsToCharacter()
		{
			string ab = "";
			string sp = "";
			string styleList = "";
			lock (m_skillList.SyncRoot)
			{
				foreach (Skill skill in m_skillList)
				{
					if (skill is Ability)
					{
						if (ab.Length > 0)
						{
							ab += ";";
						}
						ab += ((Ability) skill).KeyName + "|" + skill.Level;
					}
				}
			}
			lock (m_specList.SyncRoot)
			{
				foreach (Specialization spec in m_specList)
				{
					if (sp.Length > 0)
					{
						sp += ";";
					}
					sp += spec.KeyName + "|" + spec.Level;
				}
			}
			lock (m_styles.SyncRoot)
			{
				foreach (Style style in m_styles)
				{
					if (styleList.Length > 0)
					{
						styleList += ";";
					}
					styleList += style.ID;
				}
			}
			string disabledSpells = "";
			string disabledAbilities = "";
			ICollection disabledSkills = GetAllDisabledSkills();
			foreach (Skill skill in disabledSkills)
			{
				int duration = GetSkillDisabledDuration(skill);
				if (duration <= 0) continue;
				if (skill is Spell)
				{
					Spell spl = (Spell)skill;
					if (disabledSpells.Length > 0)
						disabledSpells += ";";
					disabledSpells += spl.ID + "|" + duration;
				}
				else if (skill is Ability)
				{
					Ability ability = (Ability)skill;
					if (disabledAbilities.Length > 0)
						disabledAbilities += ";";
					disabledAbilities += ability.KeyName + "|" + duration;
				}
				else
				{
					if (log.IsWarnEnabled)
						log.Warn(Name + ": Can't save disabled skill "+skill.GetType().ToString());
				}
			}
			StringBuilder spellLines = new StringBuilder();
			lock (m_spelllines.SyncRoot)
			{
				foreach (SpellLine line in m_spelllines)
				{
					if (spellLines.Length > 0)
						spellLines.Append(';');
					spellLines.AppendFormat("{0}|{1}", line.KeyName, line.Level);
				}
			}
			SerializedAbilities = ab;
			SerializedSpecs = sp;
			Styles = styleList;
			SerializedSpellLines = spellLines.ToString();
			DisabledSpells = disabledSpells;
			DisabledAbilities = disabledAbilities;

		}

		/// <summary>
		/// Loads the Skills from the Character
		/// Called after the default skills / level have been set!
		/// </summary>
		protected virtual void LoadSkillsFromCharacter()
		{
			Hashtable disabledAbilities = new Hashtable();
			Hashtable disabledSpells = new Hashtable();
			if (DisabledAbilities != null && DisabledAbilities.Length > 0)
			{
				try
				{
					foreach (string str in DisabledAbilities.Split(';'))
					{
						string[] values = str.Split('|');
						if (values.Length < 2) continue;
						disabledAbilities.Add(values[0], int.Parse(values[1]));
					}
				}
				catch(Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(Name+": error in loading disabled abilities => '"+DisabledAbilities+"'", e);
				}
			}
			if (DisabledSpells != null && DisabledSpells.Length > 0)
			{
				try
				{
					foreach (string str in DisabledSpells.Split(';'))
					{
						string[] values = str.Split('|');
						if (values.Length < 2) continue;
						disabledSpells.Add(ushort.Parse(values[0]), int.Parse(values[1]));
					}
				}
				catch(Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error(Name+": error in loading disabled spells => '"+DisabledSpells+"'", e);
				}
			}
			lock (m_skillList.SyncRoot)
			{
				if (SerializedAbilities != null && SerializedAbilities.Length > 0)
				{
					try
					{
						string[] abilities = SerializedAbilities.Split(';');
						foreach (string ability in abilities)
						{
							string[] values = ability.Split('|');
							if (values.Length < 2) continue;
							if (!HasAbility(values[0]))
								AddAbility(SkillBase.GetAbility(values[0], int.Parse(values[1])));
						}
						foreach (Skill skill in m_skillList)
						{
							Ability ab = skill as Ability;
							if (ab == null) continue;
							foreach (string ability in abilities)
							{
								string[] values = ability.Split('|');
								if (ab.KeyName.Equals(values[0]))
								{
									skill.Level = Convert.ToInt32(values[1]);
									//DOLConsole.WriteLine("Setting Level for Ability "+skill.Name+" to level "+skill.Level);
									break;
								}
							}

							try
							{
								foreach(DictionaryEntry de in disabledAbilities)
								{
									if (ab.KeyName != (string)de.Key) continue;
									DisableSkill(ab, (int)de.Value);
									break;
								}
							}
							catch(Exception e)
							{
								if (log.IsErrorEnabled)
									log.Error("Disabling abilities '" + DisabledAbilities+"'", e);
							}
						}
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error(Name+": error in loading abilities => '"+SerializedAbilities+"'", e);
					}
				}

			}
			lock (m_specList.SyncRoot)
			{
				if (SerializedSpecs != null && SerializedSpecs.Length > 0)
				{
					try
					{
						string[] specs = SerializedSpecs.Split(';');
						foreach (string spec in specs)
						{
							string[] values = spec.Split('|');
							if (values.Length < 2) continue;
							if (!HasSpecialization(values[0]))
								AddSpecialization(SkillBase.GetSpecialization(values[0]));
						}
						foreach (string spec in specs)
						{
							string[] values = spec.Split('|');
							foreach (Specialization cspec in m_specList)
							{
								if (cspec.KeyName.Equals(values[0]))
								{
									cspec.Level = Convert.ToInt32(values[1]);
									CharacterClass.OnSkillTrained(this, cspec);
									//DOLConsole.WriteLine("Setting Level for Specialization "+cspec.Name+" to level "+cspec.Level);
								}
							}
						}
					}
					catch(Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error(Name+": error in loading specs => '"+SerializedSpecs+"'", e);
					}
				}
			}
			lock (m_styles.SyncRoot)
			{
				if (Styles != null &&Styles.Length > 0)
				{
					m_styles.Clear();
					string[] ids = Styles.Split(';');
					for (int i=0; i<ids.Length; i++)
					{
						try
						{
							if (ids[i].Trim().Length == 0) continue;
							int id = int.Parse(ids[i]);
							Style style = SkillBase.GetStyleByID(id, CharacterClass.ID);
							if (style != null)
							{
								m_styles.Add(style);
							}
							else
							{
								if (log.IsErrorEnabled)
									log.Error("Cant find style "+id+" for character "+Name+"!");
							}
						}
						catch (Exception e)
						{
							if (log.IsErrorEnabled)
								log.Error("Error loading some style from character "+Name, e);
						}
					}
				}
			}
			lock (m_spelllines.SyncRoot)
			{
				m_spelllines.Clear();
				foreach (string serializedSpellLine in SerializedSpellLines.Split(';'))
				{
					try
					{
						string[] values = serializedSpellLine.Split('|');
						if (values.Length < 2) continue;
						SpellLine splLine = SkillBase.GetSpellLine(values[0]);
						splLine.Level = int.Parse(values[1]);
						AddSpellLine(splLine);
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Error loading SpellLine '"+serializedSpellLine+"' from character '"+Name+"'", e);
					}
				}
			}
			CharacterClass.OnLevelUp(this); // load all skills from DB first to keep the order
			RefreshSpecDependendSkills(false);
			UpdateSpellLineLevels(false);
			try
			{
				IList lines = GetSpellLines();
				lock (lines.SyncRoot)
				{
					foreach (SpellLine line in lines)
						foreach (Spell spl in SkillBase.GetSpellList(line.KeyName))
							foreach(DictionaryEntry de in disabledSpells)
							{
								if (spl.ID != (ushort)de.Key) continue;
								DisableSkill(spl, (int)de.Value);
								break;
							}
				}
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Disabling spells (" + DisabledSpells + ")", e);
			}
		}
		#endregion
	
		#region Friends

		private string m_friendList=""; //comma seperated string of friends
		
		public string SerializedFriendsList
		{
			get { return m_friendList; }
			set { m_friendList = value; }
		}

		/// <summary>
		/// Gets or sets the friends of this player
		/// </summary>
		public ArrayList Friends
		{
			get
			{
				if (SerializedFriendsList != null)
					return new ArrayList(SerializedFriendsList.Split(','));
				return new ArrayList(0);
			}
			set
			{
				if (value == null)
					SerializedFriendsList = "";
				else
					SerializedFriendsList = String.Join(",", (string[]) value.ToArray(typeof (string)));
			}
		}

		/// <summary>
		/// Modifies the friend list of this player
		/// </summary>
		/// <param name="friendName">the friend name</param>
		/// <param name="remove">true to remove this friend, false to add it</param>
		public void ModifyFriend(string friendName, bool remove)
		{
			ArrayList currentFriends = Friends;
			if (remove && currentFriends != null)
			{
				if (currentFriends.Contains(friendName))
				{
					currentFriends.Remove(friendName);
					Friends = currentFriends;
				}
			}
			else
			{
				if (!currentFriends.Contains(friendName))
				{
					currentFriends.Add(friendName);
					Friends = currentFriends;
				}
			}
		}

		#endregion

		#region Crafting

		/// <summary>
		/// Store all player crafting skill and their value (eCraftingSkill => Value)
		/// </summary>
		private Hashtable craftingSkills = new Hashtable(6);

		/// <summary>
		/// Store the player primary crafting skill
		/// </summary>
		private eCraftingSkill m_craftingPrimarySkill = 0;

		/// <summary>
		/// Get all player crafting skill and their value
		/// </summary>
		public Hashtable CraftingSkills
		{
			get { return craftingSkills; }
		}

		/// <summary>
		/// Store the player primary crafting skill
		/// </summary>
		public eCraftingSkill CraftingPrimarySkill
		{
			get { return m_craftingPrimarySkill; }
			set { m_craftingPrimarySkill = value; }
		}

		private string m_craftingSkills="";// crafting skills
		
		public string SerializedCraftingSkills
		{
			get { return m_craftingSkills; }
			set { m_craftingSkills = value; }
		}

		/// <summary>
		/// Get the specified player crafting skill value
		/// </summary>
		/// <param name="skill">The crafting skill to get value</param>
		/// <returns>the level in the specified crafting if valid and -1 if not</returns>
		public virtual int GetCraftingSkillValue(eCraftingSkill skill)
		{
			lock (craftingSkills.SyncRoot)
			{
				if(craftingSkills[skill] == null) return -1;
				return Convert.ToInt32(craftingSkills[skill]);
			}
		}

		/// <summary>
		/// Increase the specified player crafting skill
		/// </summary>
		/// <param name="skill">Crafting skill to increase</param>
		/// <param name="count">How much increase or decrase</param>
		/// <returns>true if the skill is valid and -1 if not</returns>
		public virtual bool IncreaseCraftingSkill(eCraftingSkill skill, int count)
		{
			if (skill == eCraftingSkill.NoCrafting) return false;

			lock (craftingSkills.SyncRoot)
			{
				AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
				if(craftingSkill != null)
				{
					craftingSkills[skill] = count + Convert.ToInt32(craftingSkills[skill]);
					Out.SendMessage("You gain skill in "+craftingSkill.Name+"! ("+craftingSkills[skill]+").",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
				}
				return true;
			}
		}

		/// <summary>
		/// Add a new crafting skill to the player
		/// </summary>
		/// <param name="skill">the crafting skill to add</param>
		/// <returns>true if the skill correctly added and false if not</returns>
		public virtual bool AddCraftingSkill(eCraftingSkill skill, int startValue)
		{
			if (skill == eCraftingSkill.NoCrafting) return false;

			lock (craftingSkills.SyncRoot)
			{
				if(! craftingSkills.ContainsKey(skill))
				{
					AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
					if(craftingSkill != null)
					{
						craftingSkills.Add(skill, startValue);
						Out.SendMessage("You gain skill in "+craftingSkill.Name+"! ("+startValue+").",eChatType.CT_Important,eChatLoc.CL_SystemWindow);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// This is the timer used to count time when a player craft
		/// </summary>
		private RegionTimer m_crafttimer;

		/// <summary>
		/// Get and set the craft timer
		/// </summary>
		public RegionTimer CraftTimer
		{
			get {return m_crafttimer;}
			set	{m_crafttimer=value;}
		}

		/// <summary>
		/// Does the player is crafting
		/// </summary>
		public bool IsCrafting
		{
			get {return ( m_crafttimer != null && m_crafttimer.IsAlive);}
		}

		/// <summary>
		/// Get the craft title string of the player
		/// </summary>
		public string CraftTitle
		{
			get
			{
				if (CraftingPrimarySkill == eCraftingSkill.NoCrafting || !craftingSkills.ContainsKey((int)CraftingPrimarySkill))
				{
					return "";
				}

				return GlobalConstants.CraftLevelToCraftTitle((int)craftingSkills[(int)CraftingPrimarySkill]);
			}
		}

		/// <summary>
		/// This function save all player crafting skill in the db
		/// </summary>
		protected void SaveCraftingSkills()
		{
			//m_character.CraftingPrimarySkill = (byte)CraftingPrimarySkill;

			string cs = "";

			if(CraftingPrimarySkill != eCraftingSkill.NoCrafting)
			{
				lock (craftingSkills.SyncRoot)
				{
					foreach (DictionaryEntry de in craftingSkills)
					{
						//						eCraftingSkill skill = (eCraftingSkill)de.Key;
						//						int valeur = Convert.ToInt32(de.Value);

						if (cs.Length > 0) cs += ";";

						cs += Convert.ToInt32(de.Key) + "|" + Convert.ToInt32(de.Value);
					}
				}
			}

			SerializedCraftingSkills = cs ;
		}

		/// <summary>
		/// This function load all player crafting skill from the db
		/// </summary>
		protected void LoadCraftingSkills()
		{
			if (SerializedCraftingSkills == "" || CraftingPrimarySkill == 0)
			{
				CraftingPrimarySkill = eCraftingSkill.NoCrafting;
				return;
			}
			try
			{
				//CraftingPrimarySkill = (eCraftingSkill)m_character.CraftingPrimarySkill;

				lock (craftingSkills.SyncRoot)
				{
					string[] craftingSkill = SerializedCraftingSkills.Split(';');
					foreach (string skill in craftingSkill)
					{
						string[] values = skill.Split('|');
						if(! craftingSkills.ContainsKey(Convert.ToInt32(values[0])))
						{
							craftingSkills.Add(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
						}
					}
				}
			}
			catch(Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error(Name+": error in loading playerCraftingSkills => "+SerializedCraftingSkills, e);
			}
		}

		/// <summary>
		/// This function is called each time a player try to make a item
		/// </summary>
		public virtual void CraftItem(ushort itemID)
		{
			CraftItemData data = (CraftItemData) GameServer.Database.FindObjectByKey(typeof(CraftItemData), itemID);
			if (data != null && data.TemplateToCraft != null && data.RawMaterials.Count >= 1)
			{
				AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum(data.CraftingSkill);
				if (skill != null)
				{
					skill.CraftItem(data, this);
				}
				else
				{
					Out.SendMessage("You do not have the ability to make this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				Out.SendMessage("Craft item ("+itemID+") not implemented yet.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// This function is called each time a player try to salvage a item
		/// </summary>
		public virtual void SalvageItem(GenericItem item)
		{
			if(item is EquipableItem)
			{
				Salvage.BeginWork(this, (EquipableItem)item);
			}
			else
			{
				Out.SendMessage("You can't salvage this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// This function is called each time a player try to repair a item
		/// </summary>
		public virtual void RepairItem(GenericItem item)
		{
			if(item is EquipableItem)
			{
				Repair.BeginWork(this, (EquipableItem)item);
			}
			else
			{
				Out.SendMessage("You can't repair this item.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Various flags (SafetyFlag/CloakHoodUp/SpellQueue/CancelStyle/Anonymous/UsedLevelCommand)

		/// <summary>
		/// Stores PvP safety flag
		/// </summary>
		protected bool m_safetyFlag;

		/// <summary>
		/// Gets/Sets safety flag
		/// </summary>
		public bool SafetyFlag
		{
			get { return m_safetyFlag; }
			set { m_safetyFlag = value; }
		}

		/// <summary>
		/// Holds the SpellQueue flag
		/// </summary>
		private bool m_spellQueue = false;
		/// <summary>
		/// Gets or sets the players SpellQueue option
		/// </summary>
		public virtual bool SpellQueue
		{
			get { return m_spellQueue; }
			set { m_spellQueue = value; }
		}

		/// <summary>
		/// Holds the cancel style flag
		/// </summary>
		protected bool m_cancelStyle;
		
		/// <summary>
		/// Gets or Sets the cancel style flag
		/// </summary>
		public bool CancelStyle
		{
			get { return m_cancelStyle; }
			set { m_cancelStyle = value; }
		}

		/// <summary>
		/// Holds the anonymous flag for this player
		/// </summary>
		private bool m_isAnonymous;

		/// <summary>
		/// Gets or sets the anonymous flag for this player
		/// </summary>
		public bool IsAnonymous
		{
			get { return m_isAnonymous; }
			set { m_isAnonymous = value; }
		}

		/// <summary>
		/// Holds if the player used the /level
		/// </summary>
		private bool m_usedLevelCommand;

		/// <summary>
		/// Gets or sets the used /level command flag
		/// </summary>
		public bool UsedLevelCommand
		{
			get { return m_usedLevelCommand; }
			set { m_usedLevelCommand = value; }
		}

		#endregion
	
		#region Player Titles
		
		/// <summary>
		/// Holds all players titles.
		/// </summary>
		protected ArrayList m_titles;
		
		/// <summary>
		/// Holds current selected title.
		/// </summary>
		protected IPlayerTitle m_currentTitle = PlayerTitleMgr.ClearTitle;
		
		/// <summary>
		/// Adds the title to player.
		/// </summary>
		/// <param name="title">The title to add.</param>
		/// <returns>true if added.</returns>
		public virtual bool AddTitle(IPlayerTitle title)
		{
			if (m_titles.Contains(title))
				return false;
			m_titles.Add(title);
			title.OnTitleGained(this);
			return true;
		}
		
		/// <summary>
		/// Removes the title from player.
		/// </summary>
		/// <param name="title">The title to remove.</param>
		/// <returns>true if removed.</returns>
		public virtual bool RemoveTitle(IPlayerTitle title)
		{
			if (!m_titles.Contains(title))
				return false;
			if (CurrentTitle == title)
				CurrentTitle = PlayerTitleMgr.ClearTitle;
			m_titles.Remove(title);
			title.OnTitleLost(this);
			return true;
		}
		
		/// <summary>
		/// Gets all player's titles.
		/// </summary>
		public virtual IList Titles
		{
			get { return m_titles; }
		}

		private string m_currentTitleType = "";

		public string CurrentTitleType
		{
			get { return m_currentTitleType; }
			set { m_currentTitleType = value; }
		}
		
		/// <summary>
		/// Gets/sets currently selected/active player title.
		/// </summary>
		public virtual IPlayerTitle CurrentTitle
		{
			get { return m_currentTitle; }
			set
			{
				if (value == null)
					value = PlayerTitleMgr.ClearTitle;
				m_currentTitle = value;
				CurrentTitleType = value.GetType().FullName;

				//update newTitle for all players if client is playing
				if(ObjectState == eObjectState.Active)
				{
					if (value == PlayerTitleMgr.ClearTitle)
						Out.SendMessage("Your title has been cleared.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					else
						Out.SendMessage("Your title has been set to " + value.GetDescription(this) + '.', eChatType.CT_System, eChatLoc.CL_SystemWindow);

					foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
					{
						if(player != this)
						{
							player.Out.SendPlayerTitleUpdate(this);
						}
					}
					Out.SendUpdatePlayer();
				}
			}
		}

		#endregion
		
		#region Statistics
		
		/// <summary>
		/// Stores the count of albion players killed.
		/// </summary>
		private int m_killsAlbionPlayers;
		/// <summary>
		/// Stores the count of midgard players killed.
		/// </summary>
		private int m_killsMidgardPlayers;
		/// <summary>
		/// Stores the count of hibernia players killed.
		/// </summary>
		private int m_killsHiberniaPlayers;
		/// <summary>
		/// Stores the count of death blows on albion players.
		/// </summary>
		private int m_killsAlbionDeathBlows;
		/// <summary>
		/// Stores the count of death blows on midgard players.
		/// </summary>
		private int m_killsMidgardDeathBlows;
		/// <summary>
		/// Stores the count of death blows on hibernia players.
		/// </summary>
		private int m_killsHiberniaDeathBlows;
		/// <summary>
		/// Stores the count of killed solo albion players.
		/// </summary>
		private int m_killsAlbionSolo;
		/// <summary>
		/// Stores the count of killed solo midgard players.
		/// </summary>
		private int m_killsMidgardSolo;
		/// <summary>
		/// Stores the count of killed solo hibernia players.
		/// </summary>
		private int m_killsHiberniaSolo;
		/// <summary>
		/// Stores the count of captured keeps.
		/// </summary>
		private int m_capturedKeeps;
		/// <summary>
		/// Stores the count of captured towers.
		/// </summary>
		private int m_capturedTowers;
		
		/// <summary>
		/// Gets or sets the count of albion players killed.
		/// </summary>
		public int KillsAlbionPlayers
		{
			get { return m_killsAlbionPlayers; }
			set
			{
				m_killsAlbionPlayers = value;
				Notify(GamePlayerEvent.KillsAlbionPlayersChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of midgard players killed.
		/// </summary>
		public int KillsMidgardPlayers
		{
			get { return m_killsMidgardPlayers; }
			set
			{
				m_killsMidgardPlayers = value;
				Notify(GamePlayerEvent.KillsMidgardPlayersChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of hibernia players killed.
		/// </summary>
		public int KillsHiberniaPlayers
		{
			get { return m_killsHiberniaPlayers; }
			set
			{
				m_killsHiberniaPlayers = value;
				Notify(GamePlayerEvent.KillsHiberniaPlayersChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of death blows on albion players.
		/// </summary>
		public int KillsAlbionDeathBlows
		{
			get { return m_killsAlbionDeathBlows; }
			set
			{
				m_killsAlbionDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of death blows on midgard players.
		/// </summary>
		public int KillsMidgardDeathBlows
		{
			get { return m_killsMidgardDeathBlows; }
			set
			{
				m_killsMidgardDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of death blows on hibernia players.
		/// </summary>
		public int KillsHiberniaDeathBlows
		{
			get { return m_killsHiberniaDeathBlows; }
			set
			{
				m_killsHiberniaDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of killed solo albion players.
		/// </summary>
		public int KillsAlbionSolo
		{
			get { return m_killsAlbionSolo; }
			set
			{
				m_killsAlbionSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of killed solo midgard players.
		/// </summary>
		public int KillsMidgardSolo
		{
			get { return m_killsMidgardSolo; }
			set
			{
				m_killsMidgardSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of killed solo hibernia players.
		/// </summary>
		public int KillsHiberniaSolo
		{
			get { return m_killsHiberniaSolo; }
			set
			{
				m_killsHiberniaSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}
		
		/// <summary>
		/// Gets or sets the count of captured keeps.
		/// </summary>
		public int CapturedKeeps
		{
			get { return m_capturedKeeps; }
			set { m_capturedKeeps = value; }
		}
		
		/// <summary>
		/// Gets or sets the count of captured towers.
		/// </summary>
		public int CapturedTowers
		{
			get { return m_capturedTowers; }
			set { m_capturedTowers = value; }
		}
		
		#endregion
		
		#region Housing

		private int m_lotNumber;

		public int LotNumber
		{
			get { return m_lotNumber; }
			set { m_lotNumber = value; }
		}
		#endregion
		

		#region Client

		/// <summary>
		/// This is our gameclient!
		/// </summary>
		protected GameClient m_client;
		/// <summary>
		/// Has this player entered the game, will be
		/// true after the first time the char enters
		/// the world
		/// </summary>
		protected bool m_enteredGame;
		/// <summary>
		/// Holds the objects that need update
		/// </summary>
		protected BitArray[] m_objectUpdates;
		/// <summary>
		/// Holds the index into the last update array
		/// </summary>
		protected byte m_lastUpdateArray = 0;
		/// <summary>
		/// Holds the tickcount when the npcs around this player
		/// were checked the last time for new npcs. Will be done
		/// every 250ms in WorldMgr.
		/// </summary>
		protected uint m_lastNPCUpdate;

		/// <summary>
		/// true if the targetObject is visible
		/// </summary>
		protected bool m_targetInView;

		/// <summary>
		/// Property for the optional away from keyboard message.
		/// </summary>
		public static readonly string AFK_MESSAGE="afk_message";

		/// <summary>
		/// Property for the optional away from keyboard message.
		/// </summary>
		public static readonly string QUICK_CAST_CHANGE_TICK="quick_cast_change_tick";

		/// <summary>
		/// Gets or sets the targetObject's visibility
		/// </summary>
		public override bool TargetInView
		{
			get { return m_targetInView; }
			set { m_targetInView = value; }
		}

		/// <summary>
		/// Holds the ground target visibility flag
		/// </summary>
		protected bool m_groundtargetInView;

		/// <summary>
		/// Gets or sets the GroundTargetObject's visibility
		/// </summary>
		public override bool GroundTargetInView
		{
			get { return m_groundtargetInView; }
			set { m_groundtargetInView = value; }
		}

		/// <summary>
		/// Returns the Object update array that was used the last time
		/// </summary>
		public BitArray CurrentUpdateArray
		{
			get
			{
				if (m_lastUpdateArray == 0)
					return m_objectUpdates[0];
				return m_objectUpdates[1];
			}
		}

		/// <summary>
		/// Returns the Object update array that will be used next time
		/// </summary>
		public BitArray NewUpdateArray
		{
			get
			{
				if (m_lastUpdateArray == 0)
					return m_objectUpdates[1];
				return m_objectUpdates[0];
			}
		}

		/// <summary>
		/// Switches the update arrays
		/// </summary>
		public void SwitchUpdateArrays()
		{
			if (m_lastUpdateArray == 0)
				m_lastUpdateArray = 1;
			else
				m_lastUpdateArray = 0;
		}

		/// <summary>
		/// Returns the GameClient of this Player
		/// </summary>
		public GameClient Client
		{
			get { return m_client; }
			set { m_client = value; }
		}

		/// <summary>
		/// Returns the PacketSender for this player
		/// </summary>
		public IPacketLib Out
		{
			get { return Client.Out; }
		}

		/// <summary>
		/// Has this player entered the game for the first
		/// time after logging on (not Zoning!)
		/// </summary>
		public bool EnteredGame
		{
			get { return m_enteredGame; }
			set { m_enteredGame = value; }
		}

		/// <summary>
		/// quit timer
		/// </summary>
		protected RegionTimer m_quitTimer;

		/// <summary>
		/// Timer callback for quit
		/// </summary>
		/// <param name="callingTimer">the calling timer</param>
		/// <returns>the new intervall</returns>
		protected int QuitTimerCallback(RegionTimer callingTimer)
		{
			if (!Alive || ObjectState != eObjectState.Active)
			{
				m_quitTimer = null;
				return 0;
			}

			//Gms can quit instantly
			if(Client.Account.PrivLevel == ePrivLevel.Player)
			{
				long lastCombatAction = LastAttackedByEnemyTick;
				if (lastCombatAction < LastAttackTick)
				{
					lastCombatAction = LastAttackTick;
				}
				long secondsleft = 60 - (Region.Time - lastCombatAction + 500)/1000; // 500 is for rounding
				if (secondsleft > 0)
				{
					if (secondsleft == 15 || secondsleft == 10 || secondsleft == 5)
					{
						Out.SendMessage("You will quit in " + secondsleft + " seconds.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					return 1000;
				}
			}

			Out.SendPlayerQuit(false);
			Quit(true);
			m_quitTimer = null;
			return 0;
		}

		/// <summary>
		/// Gets the amount of time the player must wait before quit, in seconds
		/// </summary>
		public virtual int QuitTime
		{
			get
			{
				if (m_quitTimer == null)
				{
					if(Region.Time - LastAttackTick > 40000)
						LastAttackTick = Region.Time - 40000; // dirty trick ;-) (20sec min quit time)
				}
				long lastCombatAction = LastAttackTick;
				if (lastCombatAction < LastAttackedByEnemyTick)
				{
					lastCombatAction = LastAttackedByEnemyTick;
				}
				return (int) (60 - (Region.Time - lastCombatAction + 500)/1000); // 500 is for rounding
			}
			set
			{}
		}

		/// <summary>
		/// Callback method, called when the player went linkdead and now he is
		/// allowed to be disconnected
		/// </summary>
		/// <param name="callingTimer">the timer</param>
		/// <returns>0</returns>
		protected int LinkdeathTimerCallback(RegionTimer callingTimer)
		{
			//If we died during our callback time we release
			if(!Alive)
			{
				Release(m_releaseType, true);
				if (log.IsInfoEnabled)
					log.Info("Linkdead player "+Name+"("+Client.Account.AccountName+") was auto-released from death!");
			}

			Client.Quit();
			return 0;
		}

		public void OnLinkdeath()
		{
			//DOLConsole.WriteSystem("OnLinkdeath "+Client.ClientState.ToString());
			if (log.IsInfoEnabled)
				log.Info("Player "+Name+"("+Client.Account.AccountName+") went linkdead!");

			if(m_quitTimer!=null)
			{
				m_quitTimer.Stop();
				m_quitTimer = null;
			}

			int secondsToQuit = QuitTime;
			if (log.IsInfoEnabled)
				log.Info("Linkdead player "+Name+"("+Client.Account.AccountName+") will quit in "+secondsToQuit);
			RegionTimer timer = new RegionTimer(this); // make sure it is not stopped!
			timer.Callback = new RegionTimerCallback(LinkdeathTimerCallback);
			timer.Start(1+secondsToQuit*1000);

			lock (this)
			{
				if (TradeWindow != null)
					TradeWindow.CloseTrade();
			}

			//Notify players in close proximity!
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.INFO_DISTANCE))
				if(GameServer.ServerRules.IsAllowedToUnderstand(this,player))
					player.Out.SendMessage(Name+" went linkdead!",eChatType.CT_Important,eChatLoc.CL_SystemWindow);

			//Notify other group members of this linkdead
			if(PlayerGroup!=null)
				PlayerGroup.UpdateMember(this, false, false);

			//Notify our event handlers (if any)
			Notify(GamePlayerEvent.Linkdeath, this);
		}

		/// <summary>
		/// This function saves the character and sends a message to all others
		/// that the player has quit the game!
		/// </summary>
		/// <param name="forced">true if Quit can not be prevented!</param>
		public virtual bool Quit(bool forced)
		{
			if (!forced)
			{
				if (!Alive)
				{
					Out.SendMessage("You can't quit now, you're dead.  Type '/release' to release your corpse.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (Steed != null)
				{
					Out.SendMessage("You have to dismount before you can quit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (IsMoving)
				{
					Out.SendMessage("You must be standing still to quit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				/*if (InHouse && CurrentHouse != null)
				{
					Out.SendMessage("You can't quit while being in a house.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}*/
				if (!Sitting)
				{
					Sit(true);
				}
				int secondsleft = QuitTime;

				if(m_quitTimer==null)
				{
					m_quitTimer = new RegionTimer(this);
					m_quitTimer.Callback = new RegionTimerCallback(QuitTimerCallback);
					m_quitTimer.Start(1);
				}

				if (secondsleft > 20)
					Out.SendMessage("You were recently in combat and must wait longer to quit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			
				Out.SendMessage("You will quit after sitting for " + secondsleft + " seconds.  Type '/stand' or move if you don't want to quit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				//Notify our event handlers (if any)
				Notify(GamePlayerEvent.Quit, this);

				// save the player
				SaveIntoDatabase();

				//Cleanup stuff
				CleanupOnDisconnect();

				//Remove the player from the geometry engine
				RemoveFromWorld();
			}
			return true;
		}

		/// <summary>
		/// The last time we did update the NPCs around us
		/// </summary>
		public uint LastNPCUpdate
		{
			get { return m_lastNPCUpdate; }
			set { m_lastNPCUpdate = value; }
		}

		#endregion

		#region Spells/Skills/Abilities/Effects

		/// <summary>
		/// Holds the player specializable skills and style lines
		/// (KeyName -> Specialization)
		/// </summary>
		protected readonly Hashtable m_specialization = new Hashtable();
		/// <summary>
		/// Holds the players specs again but ordered
		/// </summary>
		protected readonly ArrayList m_specList = new ArrayList();
		/// <summary>
		/// Holds all abilities of the player (KeyName -> Ability)
		/// </summary>
		protected readonly Hashtable m_abilities = new Hashtable();
		/// <summary>
		/// Holds the Spell lines the player can use
		/// </summary>
		protected readonly ArrayList m_spelllines = new ArrayList();
		/// <summary>
		/// Holds the Spells without spell lines behind aka Songs
		/// </summary>
		//protected ArrayList m_spells = new ArrayList();
		/// <summary>
		/// Holds all styles of the player
		/// </summary>
		protected readonly ArrayList m_styles = new ArrayList();
		/// <summary>
		/// Holds all non trainable skills in determined order without styles
		/// </summary>
		protected readonly ArrayList m_skillList = new ArrayList();

		
		public virtual void WipeAllSkills() 
		{
			m_styles.Clear();
			m_specialization.Clear();
			m_abilities.Clear();
			m_disabledSkills.Clear();
			m_spelllines.Clear();
			m_skillList.Clear();
		}

		/// <summary>
		/// give player a new Specialization
		/// </summary>
		/// <param name="skill"></param>
		public void AddSpecialization(Specialization skill)
		{
			if (skill == null)
				return;
			Specialization oldskill = m_specialization[skill.KeyName] as Specialization;
			if (oldskill == null)
			{
				//DOLConsole.WriteLine("Spec "+skill.Name+" added");
				m_specialization[skill.KeyName] = skill;
				lock (m_specialization.SyncRoot)
				{
					lock (m_specList.SyncRoot)
					{
						m_specialization[skill.KeyName] = skill;
						m_specList.Add(skill);
					}
				}
				Out.SendMessage("You learn the " + skill.Name + " skill!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (oldskill.Level < skill.Level)
				{
					oldskill.Level = skill.Level;
				}
			}
		}

		/// <summary>
		/// Removes the existing specialization from the player
		/// </summary>
		/// <param name="specKeyName">The spec keyname to remove</param>
		/// <returns>true if removed</returns>
		public bool RemoveSpecialization(string specKeyName)
		{
			Specialization playerSpec = null;
			lock (m_specialization.SyncRoot)
			{
				lock (m_specList.SyncRoot)
				{
					playerSpec = (Specialization)m_specialization[specKeyName];
					if (playerSpec == null)
						return false;
					m_specList.Remove(playerSpec);
					m_specialization.Remove(specKeyName);
				}
			}
			Out.SendMessage("You lose the "+playerSpec+" skill!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return true;
		}

		/// <summary>
		/// returns a list with all specializations
		/// in the order they were added
		/// be careful when iterating this list, it has to be
		/// synced via SyncRoot before any foreach loop
		/// because its a reference to the player internal list of specs
		/// that can change at any time
		/// </summary>
		/// <returns>list of Spec's</returns>
		public IList GetSpecList()
		{
			return m_specList;
		}

		/// <summary>
		/// returns a list with all non trainable skills without styles
		/// in the order they were added
		/// be careful when iterating this list, it has to be
		/// synced via SyncRoot before any foreach loop
		/// because its a reference to the player internal list of skills
		/// that can change at any time
		/// </summary>
		/// <returns>list of Skill's</returns>
		public IList GetNonTrainableSkillList()
		{
			return m_skillList;
		}

		/// <summary>
		/// Retrieves a specific specialization by key name
		/// </summary>
		/// <param name="keyName">the key name</param>
		/// <returns>the found specialization or null</returns>
		public Specialization GetSpecialization(string keyName)
		{
			return m_specialization[keyName] as Specialization;
		}

		/// <summary>
		/// Retrives a specific specialization by name
		/// </summary>
		/// <param name="name">the name of the specialization line</param>
		/// <param name="caseSensitive">false for case-insensitive compare</param>
		/// <returns>found specialization or null</returns>
		public Specialization GetSpecializationByName(string name, bool caseSensitive)
		{
			lock (m_specList.SyncRoot)
			{
				if(caseSensitive)
				{
					foreach (Specialization spec in m_specList)
						if (spec.Name == name)
							return spec;
				}
				else
				{
					name = name.ToLower();
					foreach (Specialization spec in m_specList)
						if (spec.Name.ToLower() == name)
							return spec;
				}
			}
			return null;
		}

		/// <summary>
		/// Adds a new Ability to the player
		/// </summary>
		/// <param name="ability"></param>
		public void AddAbility(Ability ability)
		{
			if (ability == null)
				return;
			bool newAbility = false;
			lock(m_abilities.SyncRoot)
			{
				Ability oldability = (Ability) m_abilities[ability.KeyName];
				lock (m_skillList.SyncRoot)
				{
					if (oldability == null)
					{
						newAbility = true;
						m_abilities[ability.KeyName] = ability;
						m_skillList.Add(ability);
					}
					else if (oldability.Level < ability.Level)
					{
						newAbility = true;
						m_abilities[ability.KeyName] = ability;
						m_skillList[m_skillList.IndexOf(oldability)] = ability;
					}
					if (newAbility) {
						Out.SendMessage("You learn the " + ability.Name + " ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
			}
		}

		/// <summary>
		/// Removes the existing ability from the player
		/// </summary>
		/// <param name="abilityKeyName">The ability keyname to remove</param>
		/// <returns>true if removed</returns>
		public bool RemoveAbility(string abilityKeyName)
		{
			Ability ability = null;
			lock (m_abilities.SyncRoot)
			{
				lock (m_skillList.SyncRoot)
				{
					ability = (Ability)m_abilities[abilityKeyName];
					if (ability == null)
						return false;
					m_abilities.Remove(ability.KeyName);
					m_skillList.Remove(ability);
				}
			}
			Out.SendMessage("You lose the "+ability.Name+" ability!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return true;
		}

		/// <summary>
		/// Asks for existence of specific ability
		/// </summary>
		/// <param name="keyName">KeyName of ability</param>
		/// <returns>Has player this ability</returns>
		public bool HasAbility(string keyName)
		{
			return m_abilities[keyName] is Ability;
		}

		/// <summary>
		/// Asks for existance of specific specialization
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public bool HasSpecialization(string keyName)
		{
			return m_specialization[keyName] is Specialization;
		}

		/// <summary>
		/// Checks if player has ability to use items of this type
		/// </summary>
		/// <param name="item"></param>
		/// <returns>true if player has ability to use item</returns>
		public virtual bool HasAbilityToUseItem(EquipableItem item)
		{
			return GameServer.ServerRules.CheckAbilityToUseItem(this, item);
		}

		/// <summary>
		/// Checks whether Living has ability to use lefthanded weapons
		/// </summary>
		public override bool CanUseLefthandedWeapon
		{
			get
			{
				return CharacterClass.CanUseLefthandedWeapon(this);
			}
		}

		/// <summary>
		/// Calculates how many times left hand swings
		/// </summary>
		public override int CalculateLeftHandSwingCount()
		{
			if (CanUseLefthandedWeapon == false)
				return 0;

			if (GetBaseSpecLevel(Specs.Left_Axe) > 0)
				return 1; // always use left axe

			// DW chance
			int specLevel = Math.Max(GetModifiedSpecLevel(Specs.Celtic_Dual), GetModifiedSpecLevel(Specs.Dual_Wield));
			if (specLevel > 0)
			{
				return Util.Chance(25+(specLevel-1)*68/100) ? 1 : 0;
			}

			// HtH chance
			specLevel = GetModifiedSpecLevel(Specs.HandToHand);
			if (specLevel > 0)
			{
				specLevel--;
				int randomChance = Util.Random(99);
				int hitChance = specLevel>>1;
				if (randomChance < hitChance)
					return 1; // 1 hit = spec/2

				hitChance += specLevel>>2;
				if (randomChance < hitChance)
					return 2; // 2 hits = spec/4

				hitChance += specLevel>>4;
				if (randomChance < hitChance)
					return 3; // 3 hits = spec/16

				return 0;
			}

			return 0;
		}

		/// <summary>
		/// returns the level of ability
		/// if 0 is returned, the ability is non existent on player
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public int GetAbilityLevel(string keyName)
		{
			Ability ab = m_abilities[keyName] as Ability;
			if (ab == null)
				return 0;
			if (ab.Level == 0)
				return 1; // at least level 1 if ab has level 0
			return ab.Level;
		}

		/// <summary>
		/// returns the level of a specialization
		/// if 0 is returned, the spec is non existent on player
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public override int GetBaseSpecLevel(string keyName)
		{
			Specialization spec = m_specialization[keyName] as Specialization;
			if (spec == null)
				return 0;
			return spec.Level; // TODO: bonus
		}

		/// <summary>
		/// returns the level of a specialization + bonuses from RR and Items
		/// if 0 is returned, the spec is non existent on the player
		/// </summary>
		/// <param name="keyName"></param>
		/// <returns></returns>
		public override int GetModifiedSpecLevel(string keyName)
		{
			Specialization spec = m_specialization[keyName] as Specialization;
			if (spec == null)
			{
				if( keyName == GlobalSpellsLines.Combat_Styles_Effect)
				{
					if(CharacterClass.ID == (int)eCharacterClass.Reaver || CharacterClass.ID == (int)eCharacterClass.Heretic)
						return GetModifiedSpecLevel(Specs.Flexible);
					if(CharacterClass.ID == (int)eCharacterClass.Valewalker)
						return GetModifiedSpecLevel(Specs.Scythe);
					if(CharacterClass.ID == (int)eCharacterClass.Savage)
						return GetModifiedSpecLevel(Specs.Savagery);
				}

				return 0;
			}
			int res = spec.Level;
			// TODO: should be all in calculator later, right now
			// needs specKey -> eProperty conversion to find calculator and then
			// needs eProperty -> specKey conversion to find how much points player has spent
			eProperty skillProp = SkillBase.SpecToSkill(keyName);
			if (skillProp != eProperty.Undefined)
				res += GetModified(skillProp);
			return res;
		}

		/// <summary>
		/// Adds a spell line to the player
		/// </summary>
		/// <param name="line"></param>
		public void AddSpellLine(SpellLine line)
		{
			if (line == null)
				return;
			SpellLine oldline = GetSpellLine(line.KeyName);
			if (oldline == null)
			{
				lock (m_spelllines.SyncRoot)
				{
					m_spelllines.Add(line);
				}
				Out.SendMessage("You learn the " + line.Name + " spell list!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (oldline.Level < line.Level)
				{
					oldline.Level = line.Level;
				}
			}
		}

		/// <summary>
		/// return a list of spell lines in the order they were added
		/// iterate only with locking SyncRoot on the list!
		/// </summary>
		/// <returns></returns>
		public IList GetSpellLines()
		{
			return m_spelllines;
		}

		/// <summary>
		/// find a spell line on player and return them
		/// </summary>
		/// <param name="keyname"></param>
		/// <returns></returns>
		public SpellLine GetSpellLine(string keyname)
		{
			lock (m_spelllines.SyncRoot)
			{
				foreach (SpellLine line in m_spelllines)
				{
					if (line.KeyName == keyname)
						return line;
				}
			}
			return null;
		}

		/// <summary>
		/// gets a list of available styles
		/// lock SyncRoot to iterate in the list!
		/// </summary>
		public IList GetStyleList()
		{
			return m_styles;
		}

		/// <summary>
		/// Return the amount of spell the player can use
		/// </summary>
		/// <returns></returns>
		public int GetAmountOfSpell()
		{
			int spellcount = 0;
			lock (m_spelllines.SyncRoot)
			{
				foreach(SpellLine line in m_spelllines)
				{
					spellcount += GetUsableSpellsOfLine(line).Count;
				}
			}
			return spellcount;
		}

		/// <summary>
		/// Return a list of spells usable in the specified SpellLine
		/// </summary>
		/// <param name="line">the line of spell</param>
		/// <returns>list of Spells</returns>
		public IList GetUsableSpellsOfLine(SpellLine line)
		{
			IList spells = new ArrayList();
			Hashtable table_spells = new Hashtable();
			foreach (Spell spell in SkillBase.GetSpellList(line.KeyName))
			{
				if (spell.Level <= line.Level)
				{
					object key;
					if (spell.Group == 0)
						key = spell.SpellType+"+"+spell.Target+"+"+spell.CastTime+"+"+spell.RecastDelay;
					else
						key = spell.Group;

					if(!table_spells.ContainsKey(key))
					{
						table_spells.Add(key,spell);
					}
					else
					{
						Spell oldspell=(Spell)table_spells[key];
						if(spell.Level > oldspell.Level)
						{
							table_spells[key]=spell;
						}
					}
				}
			}

			foreach (DictionaryEntry spell in table_spells)
			{
				try
				{
					spells.Add(spell.Value);
				}
				catch(Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("GetUsableSpellsOfLine", e);
				}
			}
			return spells;
		}
		/// <summary>
		/// updates the list of available styles
		/// </summary>
		/// <param="sendMessages">sends "you learn" messages if true</param="sendMessages">
		public void RefreshSpecDependendSkills(bool sendMessages)
		{
			IList newStyles = new ArrayList();
			lock (m_styles.SyncRoot)
			{
				lock (m_specList.SyncRoot)
				{
					foreach (Specialization spec in m_specList)
					{
						// check styles
						IList styles = SkillBase.GetStyleList(spec.KeyName, CharacterClass.ID);
						foreach (Style style in styles)
						{
							if (style.SpecLevelRequirement <= spec.Level)
							{
								if (!m_styles.Contains(style))
								{
									newStyles.Add(style);
									m_styles.Add(style);
								}
							}
						}

						// check abilities
						IList abilities = SkillBase.GetSpecAbilityList(spec.KeyName);
						foreach (Ability ability in abilities)
						{
							if (ability.SpecLevelRequirement <= spec.Level) {
								AddAbility(ability);	// add ability cares about all
							}
						}
					}
				}
			}

			if (sendMessages)
			{
				foreach(Style style in newStyles)
				{
					Out.SendMessage("You learn the "+style.Name+" combat style!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					string message = null;
					if(Style.eOpening.Offensive == style.OpeningRequirementType)
					{
						switch(style.AttackResultRequirement)
						{
							case Style.eAttackResult.Style:
							case Style.eAttackResult.Hit: // TODO: make own message for hit after styles DB is updated
								message = "This style can only be used after you execute the ";
								Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, CharacterClass.ID);
								if(reqStyle == null)
									message += "(style " + style.OpeningRequirementValue + " not found) style.";
								else message += reqStyle.Name + " style.";
								break;
							case Style.eAttackResult.Miss:   message = "This style is best used after your last attack is missed."; break;
							case Style.eAttackResult.Parry:  message = "This style is best used after your last attack is parried."; break;
							case Style.eAttackResult.Block:  message = "This style is best used after your last attack is blocked."; break;
							case Style.eAttackResult.Evade:  message = "This style is best used after your last attack is evaded."; break;
							case Style.eAttackResult.Fumble: message = "This style is best used after your last attack fumbles."; break;
						}
					}
					else if(Style.eOpening.Defensive == style.OpeningRequirementType)
					{
						switch(style.AttackResultRequirement)
						{
							case Style.eAttackResult.Miss:   message = "This style is best used after your target's last attack misses."; break;
							case Style.eAttackResult.Hit:    message = "This style is best used after your target's last attack hits."; break;
							case Style.eAttackResult.Parry:  message = "This style is best used after your target's last attack is parried."; break;
							case Style.eAttackResult.Block:  message = "This style is best used after your target's last attack is blocked."; break;
							case Style.eAttackResult.Evade:  message = "This style is best used after your target's last attack is evaded."; break;
							case Style.eAttackResult.Fumble: message = "This style is best used after your target's last attack fumbles."; break;
							case Style.eAttackResult.Style:  message = "This style is best used after your target's last attack is style."; break;
						}
					}

					if(message != null)
						Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// updates the levels of all spell lines
		/// specialized spell lines depend from spec levels
		/// base lines depend from player level
		/// </summary>
		/// <param="sendMessages">sends "You gain power" messages if true</param="sendMessages">
		public virtual void UpdateSpellLineLevels(bool sendMessages)
		{
			lock (GetSpellLines().SyncRoot)
			{
				foreach (SpellLine line in GetSpellLines())
				{
					if (line.IsBaseLine)
					{
						line.Level = Level;
					}
					else
					{
						int newSpec = GetBaseSpecLevel(line.Spec);
						if (newSpec > 0)
						{
							if(sendMessages && line.Level < newSpec)
								Out.SendMessage("You gain power in the " + line.Name + " spell list!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							line.Level = newSpec;
						}
					}
				}
			}
		}

		/// <summary>
		/// Called by trainer when specialization points were added to a skill
		/// </summary>
		/// <param name="skill"></param>
		public void OnSkillTrained(Specialization skill)
		{
			Out.SendMessage("You spend " + skill.Level + " points and specialize further in " + skill.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			Out.SendMessage("You have " + SkillSpecialtyPoints + " specialization points left this level.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			Message.SystemToOthers(this, GetName(0, true) + " trains in various specializations.", eChatType.CT_System);
			CharacterClass.OnSkillTrained(this, skill);
			RefreshSpecDependendSkills(true);
			UpdateSpellLineLevels(true);

			Out.SendUpdatePlayerSkills();
		}

		/// <summary>
		/// effectiveness of the player (resurrection illness)
		/// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration formula
		/// </summary>
		protected double m_playereffectiveness = 1.0;

		/// <summary>
		/// get / set the player's effectiveness.
		/// Effectiveness is used in physical/magic damage (exept dot), in weapon skill and max concentration
		/// </summary>
		public double PlayerEffectiveness
		{
			get { return m_playereffectiveness; }
			set { m_playereffectiveness = value; }
		}

		/// <summary>
		/// Creates new effects list for this living.
		/// </summary>
		/// <returns>New effects list instance</returns>
		protected override GameEffectList CreateEffectsList()
		{
			return new GameEffectPlayerList(this);
		}

		#endregion

		#region Combat
		/// <summary>
		/// The time someone can hold a ranged attack before tiring
		/// </summary>
		internal const string RANGE_ATTACK_HOLD_START =" RangeAttackHoldStart";
		/// <summary>
		/// Endurance used for normal range attack
		/// </summary>
		public const int RANGE_ATTACK_ENDURANCE = 5;

		/// <summary>
		/// Holds the Style that this player wants to use next
		/// </summary>
		protected Style m_nextCombatStyle;
		/// <summary>
		/// Holds the backup style for the style that the player wants to use next
		/// </summary>
		protected Style m_nextCombatBackupStyle;
		/// <summary>
		/// Gets or Sets the next combat style to use
		/// </summary>
		public Style NextCombatStyle
		{
			get { return m_nextCombatStyle; }
			set { m_nextCombatStyle = value; }
		}
		/// <summary>
		/// Gets or Sets the next combat backup style to use
		/// </summary>
		public Style NextCombatBackupStyle
		{
			get { return m_nextCombatBackupStyle; }
			set { m_nextCombatBackupStyle = value; }
		}

		/// <summary>
		/// Decides which style living will use in this moment
		/// </summary>
		/// <returns>Style to use or null if none</returns>
		protected override Style GetStyleToUse()
		{
			Weapon weapon;
			if (NextCombatStyle == null) return null;
			if (NextCombatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
				weapon = Inventory.GetItem(eInventorySlot.LeftHandWeapon) as Weapon;
			else 
				weapon = AttackWeapon;

			if (StyleProcessor.CanUseStyle(this, NextCombatStyle, weapon))
				return NextCombatStyle;

			if (NextCombatBackupStyle == null) return NextCombatStyle;

			return NextCombatBackupStyle;
		}

		/// <summary>
		/// Switches the active weapon to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public override void SwitchWeapon(eActiveWeaponSlot slot)
		{
			//When switching weapons, attackmode is removed!
			if(AttackState)
				StopAttack();

			if(CurrentSpellHandler != null)
			{
				Out.SendMessage("Your spell is cancelled!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				StopCurrentSpellcast();
			}

			switch(slot)
			{
				case eActiveWeaponSlot.Standard:
					// remove endurance remove handler
					if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
						GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
					break;

				case eActiveWeaponSlot.TwoHanded:
					// remove endurance remove handler
					if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
						GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
					break;

				case eActiveWeaponSlot.Distance:
					// add endurance remove handler
					if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
						GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
					break;
			}



			VisibleEquipment[] oldActiveSlots = new VisibleEquipment[4];
			VisibleEquipment[] newActiveSlots = new VisibleEquipment[4];
			VisibleEquipment rightHandSlot = (VisibleEquipment)Inventory.GetItem(eInventorySlot.RightHandWeapon);
			VisibleEquipment leftHandSlot  = (VisibleEquipment)Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			VisibleEquipment twoHandSlot   = (VisibleEquipment)Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			VisibleEquipment distanceSlot  = (VisibleEquipment)Inventory.GetItem(eInventorySlot.DistanceWeapon);

			// save old active weapons
			// simple active slot logic:
			// 0=right hand, 1=left hand, 2=two-hand, 3=range, F=none
			switch (VisibleActiveWeaponSlots & 0x0F)
			{
				case 0: oldActiveSlots[0] = rightHandSlot; break;
				case 2: oldActiveSlots[2] = twoHandSlot; break;
				case 3: oldActiveSlots[3] = distanceSlot; break;
			}
			if ((VisibleActiveWeaponSlots & 0xF0) == 0x10) oldActiveSlots[1] = leftHandSlot;


			base.SwitchWeapon(slot);


			// save new active slots
			switch (VisibleActiveWeaponSlots & 0x0F)
			{
				case 0: newActiveSlots[0] = rightHandSlot; break;
				case 2: newActiveSlots[2] = twoHandSlot; break;
				case 3: newActiveSlots[3] = distanceSlot; break;
			}
			if ((VisibleActiveWeaponSlots & 0xF0) == 0x10) newActiveSlots[1] = leftHandSlot;

			// unequip changed items
			for(int i=0; i<4; i++)
			{
				if (oldActiveSlots[i] != null && newActiveSlots[i] == null)
				{
					oldActiveSlots[i].OnItemUnequipped(oldActiveSlots[i].SlotPosition);
				}
			}

			// equip new active items
			for(int i=0; i<4; i++)
			{
				if (newActiveSlots[i] != null && oldActiveSlots[i] == null)
				{
					newActiveSlots[i].OnItemEquipped();
				}
			}

			if (ObjectState == eObjectState.Active)
			{
				//Send new wield info, no items updated
				Out.SendInventorySlotsUpdate(null);
				// Update active weapon appearence (has to be done with all
				// equipment in the packet else player is naked)
				UpdateEquipementAppearance();
				//Send new weapon stats
				Out.SendUpdateWeaponAndArmorStats();
			}
		}

		/// <summary>
		/// Removes ammo and endurance on range attack
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void RangeAttackHandler(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackFinishedEventArgs args = arguments as AttackFinishedEventArgs;
			if(args == null) return;

			switch(args.AttackData.AttackResult)
			{
				case eAttackResult.HitUnstyled:
				case eAttackResult.Missed:
				case eAttackResult.Blocked:
				case eAttackResult.Parried:
				case eAttackResult.Evaded:
				case eAttackResult.HitStyle:
				case eAttackResult.Fumbled:
					
					IStackableItem ammo = null;
					if(AttackWeapon is ThrownWeapon)
					{
						ammo = Inventory.GetItem(eInventorySlot.DistanceWeapon) as IStackableItem;
					}
					else
					{
						ammo = RangeAttackAmmo;
					}

					if(ammo != null)
					{
						((GamePlayerInventory)Inventory).RemoveCountFromStack(ammo, 1);
					}
					
					if(RangeAttackType == eRangeAttackType.Critical || RangeAttackType == eRangeAttackType.RapidFire && GetAbilityLevel(Abilities.RapidFire) == 1)
					{
						EndurancePercent -= 2 * RANGE_ATTACK_ENDURANCE;
					}
					else
					{
						EndurancePercent -= RANGE_ATTACK_ENDURANCE;
					}
					break;
			}
		}

		/// <summary>
		/// Starts a melee attack with this player
		/// </summary>
		/// <param name="attackTarget">the target to attack</param>
		public override void StartAttack(GameObject attackTarget)
		{

			if (!Alive)
			{
				Out.SendMessage("You can't enter combat mode while lying down!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

//			if(IsShade)
//			{
//				Out.SendMessage("You cannot enter combat mode in shade mode!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//				return;
//			}
			if (Stun)
			{
				Out.SendMessage("You can't attack when you are stunned!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
			if (Mez)
			{
				Out.SendMessage("You can't attack when you are mesmerized!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			if (Sitting)
			{
				Sit(false);
			}
			if (AttackWeapon == null)
			{
				Out.SendMessage("You cannot enter combat mode without a weapon!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
//			if (AttackWeapon is Instrument)
//			{
//				Out.SendMessage("You cannot enter melee combat mode with an instrument!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//				return;
//			}
//			if(attackTarget!=null && attackTarget is GamePlayer && ((GamePlayer)attackTarget).IsShade)
//			{
//				Out.SendMessage("You cannot attack shaded player!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
//				return;
//			}
			if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				// Check arrows for ranged attack
				if(RangeAttackAmmo == null && !(AttackWeapon is ThrownWeapon))
				{
					Out.SendMessage("You must select a quiver slot to draw from!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}
				// Check if selected ammo is compatible for ranged attack
				if(!CheckRangedAmmoCompatibilityWithActiveWeapon())
				{
					Out.SendMessage("You can't use the selected quiver ammo with your weapon!", eChatType.CT_YouHit,eChatLoc.CL_SystemWindow);
					return;
				}

				lock (EffectList)
				{
					foreach (IGameEffect effect in EffectList) // switch to the correct range attack type
					{
						if (effect is SureShotEffect)
						{
							RangeAttackType = eRangeAttackType.SureShot;
							break;
						}

						if(effect is RapidFireEffect)
						{
							RangeAttackType = eRangeAttackType.RapidFire;
							break;
						}
					}
				}

				if((RangeAttackType == eRangeAttackType.Critical
				||(RangeAttackType == eRangeAttackType.RapidFire && GetAbilityLevel(Abilities.RapidFire) == 1))
				&& EndurancePercent < 2 * RANGE_ATTACK_ENDURANCE)
				{
					Out.SendMessage("You're too tired to perform a critical shot!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}
				else if(EndurancePercent < RANGE_ATTACK_ENDURANCE)
				{
					Out.SendMessage("You're too tired to use your " + AttackWeapon.Name + "!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}

				if(IsStealthed)
				{
					/*
					 * -Chance to unstealth while nocking an arrow = stealth spec / level
					 * -Chance to unstealth nocking a crit = stealth / level – 0.20
					 */
					int stealthSpec = GetModifiedSpecLevel(Specs.Stealth);
					int stayStealthed = stealthSpec * 100 / Level;
					if(RangeAttackType == eRangeAttackType.Critical)
						stayStealthed -= 20;

					if(!Util.Chance(stayStealthed))
						Stealth(false);
				}
			}
			else
			{
				if (attackTarget == null)
					Out.SendMessage("You enter combat mode but have no target!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				else
					Out.SendMessage("You enter combat mode and target [" + attackTarget.GetName(0, false) + "]", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}

			base.StartAttack(attackTarget);

			if (m_runningSpellHandler!=null && m_runningSpellHandler.IsCasting)
			{
				StopCurrentSpellcast();
				Out.SendMessage("Your spell is cancelled!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			//Clear styles
			NextCombatStyle = null;
			NextCombatBackupStyle = null;

			if(ActiveWeaponSlot != eActiveWeaponSlot.Distance)
			{
				Out.SendAttackMode(AttackState);
			}
			else
			{
				TempProperties.setProperty(RANGE_ATTACK_HOLD_START, 0L);

				string typeMsg="shot";
				if(AttackWeapon is ThrownWeapon)
					typeMsg="throw";

				string targetMsg="";
				if(attackTarget!=null)
				{
					if(Position.CheckDistance(attackTarget.Position, AttackRange))
						targetMsg=", target is in range";
					else
						targetMsg=", target is out of range";
				}

				int speed = AttackSpeed(AttackWeapon)/100;
				Out.SendMessage(string.Format("You prepare your {0} ({1}.{2}s to fire{3})", typeMsg, speed/10, speed%10, targetMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Stops all attacks this player is making
		/// </summary>
		public override void StopAttack()
		{
			NextCombatStyle = null;
			NextCombatBackupStyle = null;
			base.StopAttack();
			if (Alive)
				Out.SendAttackMode(AttackState);
		}


		/// <summary>
		/// Switches the active quiver slot to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public virtual void SwitchQuiver(eActiveQuiverSlot slot, bool forced)
		{
			if(slot != eActiveQuiverSlot.None)
			{
				eInventorySlot updatedSlot = eInventorySlot.Invalid;
				if ((slot & eActiveQuiverSlot.Fourth) > 0)
					updatedSlot = eInventorySlot.FourthQuiver;
				else if ((slot & eActiveQuiverSlot.Third) > 0)
					updatedSlot = eInventorySlot.ThirdQuiver;
				else if ((slot & eActiveQuiverSlot.Second) > 0)
					updatedSlot = eInventorySlot.SecondQuiver;
				else if ((slot & eActiveQuiverSlot.First) > 0)
					updatedSlot = eInventorySlot.FirstQuiver;

				GenericItem item = Inventory.GetItem(updatedSlot) as GenericItem;
				if(item != null && (ActiveQuiverSlot != slot || forced))
				{
					ActiveQuiverSlot = slot;
					Out.SendMessage("You will shoot with: "+ item.Name +".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					ActiveQuiverSlot = eActiveQuiverSlot.None;
					Out.SendMessage("You have no more ammo in your quiver!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				Out.SendInventorySlotsUpdate(new int[] { (int)updatedSlot });
			}
			else
			{
				if(Inventory.GetItem(eInventorySlot.FirstQuiver)!=null)
					SwitchQuiver(eActiveQuiverSlot.First, true);
				else if (Inventory.GetItem(eInventorySlot.SecondQuiver)!=null)
					SwitchQuiver(eActiveQuiverSlot.Second, true);
				else if (Inventory.GetItem(eInventorySlot.ThirdQuiver)!=null)
					SwitchQuiver(eActiveQuiverSlot.Third, true);
				else if (Inventory.GetItem(eInventorySlot.FourthQuiver)!=null)
					SwitchQuiver(eActiveQuiverSlot.Fourth, true);
				else
				{
					ActiveQuiverSlot = eActiveQuiverSlot.None;
					Out.SendMessage("You will not use your quiver.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				Out.SendInventorySlotsUpdate(null);
			}
		}

		/// <summary>
		/// Check the selected range ammo and decides if it's compatible with select weapon
		/// </summary>
		/// <returns>True if compatible, false if not</returns>
		protected bool CheckRangedAmmoCompatibilityWithActiveWeapon()
		{
			Weapon weapon = AttackWeapon;
			if(weapon != null && weapon is RangedWeapon && !(weapon is ThrownWeapon))
			{
				if (ActiveQuiverSlot != eActiveQuiverSlot.None)
				{
					Ammunition ammo = null;
					switch (ActiveQuiverSlot)
					{
						case eActiveQuiverSlot.Fourth : ammo = Inventory.GetItem(eInventorySlot.FourthQuiver) as Ammunition; break;
						case eActiveQuiverSlot.Third  : ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver) as Ammunition; break;
						case eActiveQuiverSlot.Second : ammo = Inventory.GetItem(eInventorySlot.SecondQuiver) as Ammunition; break;
						case eActiveQuiverSlot.First  : ammo = Inventory.GetItem(eInventorySlot.FirstQuiver) as Ammunition; break;
					}

					if (ammo == null) return false;

					if (weapon is Crossbow) return ammo is Bolt;
					return ammo is Arrow;	
				}
			}
			return true;
		}

		/// <summary>
		/// Holds the arrows for next range attack
		/// </summary>
		protected WeakReference m_rangeAttackAmmo;

		/// <summary>
		/// Gets/Sets the item that is used for ranged attack
		/// </summary>
		/// <returns>Item that will be used for range/accuracy/damage modifications</returns>
		protected override Ammunition RangeAttackAmmo
		{
			get
			{
				//TODO: ammo should be saved on start of every range attack and used here
				Ammunition ammo = null;//(InventoryItem)m_rangeAttackArrows.Target;

				Weapon weapon = AttackWeapon;
				if(weapon!=null && weapon is RangedWeapon && !(weapon is ThrownWeapon))
				{
					switch(ActiveQuiverSlot)
					{
						case eActiveQuiverSlot.First  : ammo = Inventory.GetItem(eInventorySlot.FirstQuiver) as Ammunition; break;
						case eActiveQuiverSlot.Second : ammo = Inventory.GetItem(eInventorySlot.SecondQuiver) as Ammunition; break;
						case eActiveQuiverSlot.Third  : ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver) as Ammunition; break;
						case eActiveQuiverSlot.Fourth : ammo = Inventory.GetItem(eInventorySlot.FourthQuiver) as Ammunition; break;
						case eActiveQuiverSlot.None:
							foreach(GenericItem item in Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
							{
								if(weapon is Crossbow && item is Bolt || !(weapon is Crossbow) && item is Arrow)
								{
									ammo = (Ammunition)item;
									break;
								}
							}
							break;
					}
				}
				return ammo;
			}
			set { m_rangeAttackAmmo.Target = value; }
		}

		/// <summary>
		/// Holds the target for next range attack
		/// </summary>
		protected WeakReference m_rangeAttackTarget;

		/// <summary>
		/// Gets/Sets the target for current ranged attack
		/// </summary>
		/// <returns></returns>
		public override GameObject RangeAttackTarget
		{
			get
			{
				GameObject target = (GameObject)m_rangeAttackTarget.Target;
				if(target == null || target.ObjectState != eObjectState.Active)
					target = TargetObject;
				return target;
			}
			set { m_rangeAttackTarget.Target = value; }
		}

		/// <summary>
		/// Check the range attack state and decides what to do
		/// Called inside the AttackTimerCallback
		/// </summary>
		/// <returns></returns>
		protected override eCheckRangeAttackStateResult CheckRangeAttackState(GameObject target)
		{
			long holdStart = TempProperties.getLongProperty(RANGE_ATTACK_HOLD_START, 0L);
			if(holdStart==0)
			{
				holdStart = Region.Time;
				TempProperties.setProperty(RANGE_ATTACK_HOLD_START, holdStart);
			}
			//DOLConsole.WriteLine("Holding.... ("+holdStart+") "+(Environment.TickCount - holdStart));
			if((Region.Time - holdStart) > 15000 && !(AttackWeapon is Crossbow))
			{
				Out.SendMessage("You are too tired to hold your shot any longer!", eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return eCheckRangeAttackStateResult.Stop; //Stop the attack
			}

			//This state is set when the player wants to fire!
			if(RangeAttackState==eRangeAttackState.Fire
			   || RangeAttackState==eRangeAttackState.AimFire
			   || RangeAttackState==eRangeAttackState.AimFireReload)
			{
				RangeAttackTarget = null; // clean the RangeAttackTarget at the first shot try even if failed

				if(target==null || !(target is GameLiving))
				{
					Out.SendMessage("You must select a target!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				}
				else if(!Position.CheckDistance(target.Position, AttackRange))
				{
					Out.SendMessage(target.GetName(0, true) + " is too far away to attack!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if(!TargetInView)  // TODO : wrong, must be checked with the target parameter and not with the targetObject
				{
					Out.SendMessage("You can't see your target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if(!IsObjectInFront(target, 90))
				{
					Out.SendMessage(target.GetName(0, true) + " is not in view!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if(RangeAttackAmmo == null && !(AttackWeapon is ThrownWeapon))
				{
					//another check for ammo just before firing
					Out.SendMessage("You must select a quiver slot to draw from!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				else if(! CheckRangedAmmoCompatibilityWithActiveWeapon() )
				{
					Out.SendMessage("You can't use the selected quiver ammo with your weapon!", eChatType.CT_YouHit,eChatLoc.CL_SystemWindow);
				}
				else if(GameServer.ServerRules.IsAllowedToAttack(this, (GameLiving)target, false))
				{
					GameLiving living = target as GameLiving;
					if(RangeAttackType == eRangeAttackType.Critical && living != null
					   && (living.CurrentSpeed > 90 //walk speed == 85, hope that's what they mean
					       || (living.AttackState && living.InCombat) //maybe not 100% correct
					       || SpellHandler.FindEffectOnTarget(living, "Mesmerize")!=null
					      ))
					{
						/*
						 * http://rothwellhome.org/guides/archery.htm
						 * Please note that critical shot will work against targets that are:
						 * sitting, standing still (which includes standing in combat mode but
						 * not actively swinging at something), walking, moving backwards,
						 * strafing, or casting a spell. Critical shot will not work against
						 * targets that are: running, in active combat (swinging at something),
						 * or mezzed. Stunned targets may be critical shot once any timers from
						 * active combat have expired if they are not yet free to act; i.e.:
						 * they may not be critical shot until their weapon delay timer has run
						 * out after their last attack, they may be critical shot during the
						 * period between the weapon delay running out and the stun wearing off,
						 * and they may not be critical shot once they have begun swinging again.
						 * If the target was in melee with an archer, the critical shot may not
						 * be drawn against them until after their weapon delay has run out or it
						 * will be interrupted.  This means that the scout's shield stun is much
						 * less effective against large weapon wielders (who have longer weapon
						 * delays) than against fast piercing/thrusting weapon wielders.
						 */

						// TODO: more checks?
						Out.SendMessage("You can't get a critical shot on your target, you switch to a standard shot.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						RangeAttackType = eRangeAttackType.Normal;
					}
					return eCheckRangeAttackStateResult.Fire;
				}

				RangeAttackState = eRangeAttackState.ReadyToFire;
				return eCheckRangeAttackStateResult.Hold;
			}

			//Player is aiming
			if(RangeAttackState==eRangeAttackState.Aim)
			{
				Out.SendMessage("You are ready to fire!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				RangeAttackState = eRangeAttackState.ReadyToFire;
				return eCheckRangeAttackStateResult.Hold;
			}
			else if(RangeAttackState==eRangeAttackState.ReadyToFire)
			{
				return eCheckRangeAttackStateResult.Hold; //Hold the shot
			}
			return eCheckRangeAttackStateResult.Fire;
		}

		/// <summary>
		/// Called whenever a single attack strike is made
		/// </summary>
		/// <param name="target">the target of attack</param>
		/// <param name="weapon">the weapon to use for attack</param>
		/// <param name="style">the style to use for attack</param>
		/// <param name="effectiveness">damage effectiveness (0..1)</param>
		/// <returns>the object where we collect and modifiy all parameters about the attack</returns>
		protected override AttackData MakeAttack(GameObject target, Weapon weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
		{
			AttackData ad = base.MakeAttack(target, weapon, style, effectiveness*PlayerEffectiveness, interruptDuration, dualWield);

			//Clear the styles for the next round!
			NextCombatStyle = null;
			NextCombatBackupStyle = null;

			switch (ad.AttackResult)
			{
				case eAttackResult.TargetNotVisible : Out.SendMessage(ad.Target.GetName(0, true) + " is not in view!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.OutOfRange       : Out.SendMessage(ad.Target.GetName(0, true) + " is too far away to attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.TargetDead       : Out.SendMessage(ad.Target.GetName(0, true) + " is already dead!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Blocked          : Out.SendMessage(ad.Target.GetName(0, true) + " blocks your attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Parried          : Out.SendMessage(ad.Target.GetName(0, true) + " parries your attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Evaded           : Out.SendMessage(ad.Target.GetName(0, true) + " evades your attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.NoTarget         : Out.SendMessage("You need to select a target to attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.NoValidTarget    : Out.SendMessage("This can't be attacked!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Missed           : Out.SendMessage("You miss!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Fumbled			: Out.SendMessage("You fumble the attack and take time to recover!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
					{
						if (Strafing && Util.Chance(30)) // 30% chance to miss in any case
						{
							ad.AttackResult = eAttackResult.Missed;
							Out.SendMessage("You were strafing in combat and miss!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							break;
						}

						string modmessage = "";
						if (ad.Modifier > 0) modmessage = " (+"+ad.Modifier+")";
						if (ad.Modifier < 0) modmessage = " ("+ad.Modifier+")";

						string hitWeapon = "";
						switch (weapon.ObjectType)
						{
							case eObjectType.Staff: hitWeapon = "staff"; break;
						}

						if (hitWeapon.Length > 0)
						{
							hitWeapon = " with your " + hitWeapon;
						}

						string attackTypeMsg = "attack";
						if(ActiveWeaponSlot==eActiveWeaponSlot.Distance)
							attackTypeMsg = "shot";

						// intercept messages
						if (target != null && target != ad.Target)
						{
							Out.SendMessage(string.Format("{0} steps in front of {1} and takes the blow!", ad.Target.GetName(0, true), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							Out.SendMessage(string.Format("You {0} {1}{2} but hit {3} for {4}{5} damage!", attackTypeMsg, target.GetName(0, false), hitWeapon, ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
						else
						{
							Out.SendMessage(string.Format("You {0} {1}{2} and hit for {3}{4} damage!", attackTypeMsg, ad.Target.GetName(0, false), hitWeapon, ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
						if (ad.CriticalDamage > 0)
							Out.SendMessage("You critical hit " + ad.Target.GetName(0, false) + " for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						break;
					}
			}
			return ad;
		}

		/// <summary>
		/// Calculates melee critical damage of this player
		/// </summary>
		/// <param name="ad">The attack data</param>
		/// <param name="weapon">The weapon used</param>
		/// <returns>The amount of critical damage</returns>
		public override int CalculateCriticalDamage(AttackData ad, Weapon weapon)
		{
			if(Util.Chance(AttackCriticalChance(weapon)))
			{
				int critMin;
				int critMax;
				BerserkEffect berserk = (BerserkEffect) EffectList.GetOfType(typeof(BerserkEffect));

				if (berserk!=null)
				{
					int level = GetAbilityLevel(Abilities.Berserk);
					// According to : http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
					// Zerk 1 = 1-25%
					// Zerk 2 = 1-50%
					// Zerk 3 = 1-75%
					// Zerk 4 = 1-99%
					critMin = (int) (0.01 * ad.Damage);
					critMax = (int) (Math.Min(0.99,(level *0.25)) * ad.Damage);
				}
				else
				{
					//think min crit dmage is 10% of damage
					critMin = ad.Damage/10;
					// Critical damage to players is 50%, low limit should be around 20% but not sure
					// zerkers in Berserk do up to 99%
					if(ad.Target is GamePlayer)
						critMax = ad.Damage>>1;
					else
						critMax = ad.Damage;
				}
				return Util.Random(critMin, critMax);
			}
			return 0;
		}

		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		public override void OnAttackedByEnemy(AttackData ad, GameObject originalTarget)
		{
			switch (ad.AttackResult)
			{
				case eAttackResult.Parried : Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you parry the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Evaded  : Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you evade the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Fumbled : Out.SendMessage(ad.Attacker.GetName(0, true) + " fumbled!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Missed  : Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and misses!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Blocked :
				{
					// send message to guard target and guard source
					if (originalTarget != null && originalTarget != this)
					{
						if (originalTarget is GamePlayer) ((GamePlayer)originalTarget).Out.SendMessage(string.Format("{0} blocks you from {1}'s attack!", GetName(0, true), ad.Attacker.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
						Out.SendMessage(string.Format("You block {0}'s attack against {1}!", ad.Attacker.GetName(0, false), originalTarget.GetName(0, false)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					}
					else
					{
						Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you block the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
					}

					// decrease condition and start reactive effect of hitted armor piece
					Shield shield = Inventory.GetItem(eInventorySlot.LeftHandWeapon) as Shield;
					if (shield != null)
					{
						shield.OnItemHit(ad.Attacker, true);
					}
					break;
				}
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
					{
						// send message to intercept target and intercept source
						if (originalTarget != null && originalTarget != this)
						{
							if (originalTarget is GamePlayer) ((GamePlayer)originalTarget).Out.SendMessage(GetName(0, true) + " steps in front of you and takes the blow!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							Out.SendMessage("You step in front of " + originalTarget.GetName(0, false) + " and take the blow!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}

						string hitLocName = null;
						switch (ad.ArmorHitLocation)
						{
							case eArmorSlot.TORSO : hitLocName = "torso"; break;
							case eArmorSlot.ARMS  : hitLocName = "arm"; break;
							case eArmorSlot.HEAD  : hitLocName = "head"; break;
							case eArmorSlot.LEGS  : hitLocName = "leg"; break;
							case eArmorSlot.HAND  : hitLocName = "hand"; break;
							case eArmorSlot.FEET  : hitLocName = "feet"; break;
						}
						string modmessage = "";
						if (ad.Attacker is GamePlayer == false) // if attacked by player, don't show resists (?)
						{
							if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
							if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
						}

						if (hitLocName != null)
						{
							Out.SendMessage(ad.Attacker.GetName(0, true) + " hits your " + hitLocName + " for " + ad.Damage + modmessage + " damage!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
						}
						else
						{
							Out.SendMessage(ad.Attacker.GetName(0, true) + " hits you for " + ad.Damage + modmessage + " damage!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
						}
						if (ad.CriticalDamage > 0)
						{
							Out.SendMessage(ad.Attacker.GetName(0, true) + " critical hits you for an additional " + ad.CriticalDamage + " damage!", eChatType.CT_YouWereHit, eChatLoc.CL_SystemWindow);
						}

						// decrease condition ans start reactive effect of hitted armor piece
						if (ad.ArmorHitLocation != eArmorSlot.UNKNOWN)
						{
							Armor item = Inventory.GetItem((eInventorySlot) ad.ArmorHitLocation) as Armor;
							if (item != null)
							{
								item.OnItemHit(ad.Attacker, true);
							}
						}
						break;
					}
			}
			base.OnAttackedByEnemy(ad, originalTarget);
		}

		/// <summary>
		/// Does needed interrupt checks and interrupts if needed
		/// </summary>
		/// <param name="attacker">the attacker that is interrupting</param>
		/// <returns>true if interrupted successfully</returns>
		protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if (base.OnInterruptTick(attacker, attackType))
			{
				if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					string attackTypeMsg="shot";
					if(AttackWeapon != null && AttackWeapon is ThrownWeapon)
						attackTypeMsg="throw";
					Out.SendMessage(attacker.GetName(0, true) + " is attacking you and your "+attackTypeMsg+" is interrupted!",eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the effective AF of this living
		/// </summary>
		public override int EffectiveOverallAF
		{
			get
			{
				int eaf = 0;
				foreach (Armor item in Inventory.ArmorItems)
				{
					int itemAFCap = Level << 1;
					if(RealmLevel > 39) itemAFCap += 2;
					if(item.ArmorLevel == eArmorLevel.VeryLow) itemAFCap >>= 1;

					int af = Math.Min(itemAFCap, item.ArmorFactor);
					double pieceEAF = af*item.Quality/100.0*item.Condition/100.0*(1 + item.Absorbtion/100.0);

					if(item is TorsoArmor)
					{
						eaf += (int) (pieceEAF * 2.2);
					}
					else if(item is LegsArmor)
					{
						eaf += (int) (pieceEAF * 1.3);
					}
					else if(item is ArmsArmor)
					{
						eaf += (int) (pieceEAF * 0.75);
					}
					else if (item is HeadArmor)
					{
						eaf += (int) (pieceEAF * 0.5);
					}
					else if(item is HandsArmor)
					{
						eaf += (int) (pieceEAF * 0.25);
					}
					else // feet
					{
						eaf += (int) (pieceEAF * 0.25);
					}
				}

				// Overall AF CAP = 10 * level * (1 + abs%/100)
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
				int abs;
				switch (bestLevel)
				{
					default: abs = 0; break; // cloth etc
					case (int)eArmorLevel.Low : abs = 10; break;
					case (int)eArmorLevel.Medium: abs = 19; break;
					case (int)eArmorLevel.High  : abs = 27; break;
					case (int)eArmorLevel.VeryHigh  : abs = 34; break;
				}

				eaf += BuffBonusCategory1[(int)eProperty.ArmorFactor]; // base buff before cap
				int eafcap = (int) (10*Level*(1 + abs*0.01));
				eaf = Math.Min(eaf, eafcap);
				
				eaf += (int)Math.Min(Level*1.875, BuffBonusCategory2[(int)eProperty.ArmorFactor])
				       -BuffBonusCategory3[(int)eProperty.ArmorFactor]
				       +BuffBonusCategory4[(int)eProperty.ArmorFactor]
				       +Math.Min(Level, ItemBonus[(int)eProperty.ArmorFactor]);

				return eaf;
			}
		}

		/// <summary>
		/// Calc Armor hit location when player is hit by enemy
		/// </summary>
		/// <returns>slotnumber where enemy hits</returns>
		public virtual eArmorSlot CalculateArmorHitLocation()
		{
			int chancehit = Util.Random(1, 100);
			if (chancehit <= 40)
			{
				return eArmorSlot.TORSO;
			}
			else if (chancehit <= 65)
			{
				return eArmorSlot.LEGS;
			}
			else if (chancehit <= 80)
			{
				return eArmorSlot.ARMS;
			}
			else if (chancehit <= 90)
			{
				return eArmorSlot.HEAD;
			}
			else if (chancehit <= 95)
			{
				return eArmorSlot.HAND;
			}
			else
			{
				return eArmorSlot.FEET;
			}
		}

		/// <summary>
		/// determines current weaponspeclevel
		/// </summary>
		public override int WeaponSpecLevel(Weapon weapon)
		{
			if (weapon == null)
				return 0;
			// use axe spec if left hand axe is not in the left hand slot
			if (weapon is LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
				return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.Axe);
			// use left axe spec if axe is in the left hand slot
			if (weapon.SlotPosition == Slot.LEFTHAND
			    && (weapon is Axe
			        || weapon is Sword
			        || weapon is Hammer))
				return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.LeftAxe);
			return GameServer.ServerRules.GetObjectSpecLevel(this, weapon.ObjectType);
		}

		/// <summary>
		/// Gets the weaponskill of weapon
		/// </summary>
		/// <param name="weapon"></param>
		public override double GetWeaponSkill(Weapon weapon)
		{
			if (weapon == null) {
				return 0;
			}
			double classbase =
				(weapon.SlotPosition == (int)eInventorySlot.DistanceWeapon
				 	? CharacterClass.WeaponSkillRangedBase
				 	: CharacterClass.WeaponSkillBase);
			return ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50)*0.005))*PlayerEffectiveness);
		}

		/// <summary>
		/// calculates weapon stat
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override int GetWeaponStat(Weapon weapon)
		{
			if (weapon != null)
			{
				switch (weapon.ObjectType)
				{
						// DEX modifier
					case eObjectType.Staff:
					case eObjectType.ShortBow:
					case eObjectType.Longbow:
					case eObjectType.Crossbow:
					case eObjectType.CompositeBow:
					case eObjectType.RecurvedBow:
					case eObjectType.ThrownWeapon:
					case eObjectType.Shield:
						return GetModified(eProperty.Dexterity);

						// STR+DEX modifier
					case eObjectType.ThrustWeapon:
					case eObjectType.Piercing:
					case eObjectType.Spear:
					case eObjectType.FlexibleWeapon:
					case eObjectType.HandToHand:
						return (GetModified(eProperty.Strength) + GetModified(eProperty.Dexterity))>>1;
				}
			}
			// STR modifier for others
			return GetModified(eProperty.Strength);
		}

		/// <summary>
		/// calculate item armor factor influenced by quality, con and duration
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public override double GetArmorAF(eArmorSlot slot)
		{
			if (slot == eArmorSlot.UNKNOWN) return 0;
			Armor item = Inventory.GetItem((eInventorySlot)slot) as Armor;
			if (item == null) return 0;
			double eaf = item.ArmorFactor + BuffBonusCategory1[(int)eProperty.ArmorFactor]; // base AF buff

			int itemAFcap = Level;
			if (RealmLevel > 39)
				itemAFcap++;
			if (item.ArmorLevel != eArmorLevel.VeryLow)
			{
				itemAFcap <<= 1;
			}

			eaf = Math.Min(eaf, itemAFcap);
			eaf *= 4.67; // compensate *4.67 in damage formula

			// my test shows that qual is added after AF buff
			eaf *= item.Quality * 0.01 * item.Condition / 100;

			eaf += base.GetArmorAF(slot);

			return eaf;
		}

		/// <summary>
		/// Calculates keep bonuses
		/// </summary>
		/// <returns></returns>
		public override double GetKeepBonuses()
		{
			//TODO : Get bonus on NF
			//todo : type of bonus
			/*
			///keeps
			8 keeps : +3% money
			9 keeps : +3% xp
			10 keeps : +3% rp
			11 keeps : -3% crafting time
			12 keeps : +5% money
			13 keeps : +5% xp
			14 keeps : +5% rp
			15 keeps : -5% crafting time
			16 keeps : +5% mana
			17 keeps : +3% endu
			18 keeps : +??% regen mana
			19 keeps : +??% life regen
			20 keeps : +5% critik fight
			21 keeps : +5% spell critik
			///reliks
			own relik strength nohing
			1 relik strenght +10%
			2 relik strenght +20%

			own relik mananohing
			1 relik mana +10%
			2 relik mana +20%
			*/
			/*int bonus = 0;
			bonus += KeepMgr.GetKeepConqueredByRealmCount((eRealm)this.Realm);
			if ( this.Guild != null)
				if (this.Guild.ClaimedKeep != null)
				{
					bonus += 2* this.Guild.ClaimedKeep.DifficultyLevel;
					if ( this.Guild.ClaimedKeep.CurrentRegion == this.CurrentRegion)
						bonus += 3* this.Guild.ClaimedKeep.DifficultyLevel;
				}
			return bonus * 0.01;*/
			return 0;
		}

		/// <summary>
		/// Calculates armor absorb level
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			if (slot == eArmorSlot.UNKNOWN) return 0;
			Armor item = Inventory.GetItem((eInventorySlot)slot) as Armor;
			if (item == null) return 0;

			return (item.Absorbtion + GetModified(eProperty.ArmorAbsorbtion)) * 0.01;
		}

		/// <summary>
		/// Weaponskill thats shown to the player
		/// </summary>
		public virtual int DisplayedWeaponSkill
		{
			get
			{
				// better formula from LeBron:
				// double m = 1.56 + item / 70;
				// (1 + (spec+rr+item*m)*0.01)
//				int itemBonus = ItemBonus[]
				int weaponSpec = WeaponSpecLevel(AttackWeapon);
				return (int)(GetWeaponSkill(AttackWeapon) * (1 + weaponSpec*0.01));
			}
		}

		/// <summary>
		/// Gets the weapondamage of currently used weapon
		/// Used to display weapon damage in stats, 16.5dps = 1650
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		public override double WeaponDamage(Weapon weapon)
		{
			if (weapon != null)
			{
				//TODO if attackweapon is ranged -> attackdamage is arrow damage
				int DPS = (int)weapon.DamagePerSecond;

				// apply damage cap before quality
				// http://www.classesofcamelot.com/faq.asp?mode=view&cat=10
				int cap = 12 + 3 * Level;
				if (RealmLevel > 39)
					cap += 3;
				if (DPS > cap)
				{
					DPS = cap;
				}
				// beware to use always ConditionPercent, because Condition is abolute value
//				return (int) ((DPS/10.0)*(weapon.Quality/100.0)*(weapon.Condition/(double)weapon.MaxCondition)*100.0);
				return (0.001 * DPS * weapon.Quality * weapon.Condition) / 100;
			}
			else
			{
				return 0;
			}
		}

		/// <summary>
		/// Max. Damage possible without style
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override double UnstyledDamageCap(Weapon weapon)
		{
			if (weapon!=null)
			{
				int DPS = (int)weapon.DamagePerSecond;
				int cap = 12 + 3*Level;
				if (RealmLevel > 39)
					cap += 3;
				if (DPS > cap)
					DPS = cap;
//				double result = (DPS*0.1 * weapon.SPD_ABS*0.1 * 3 * (1 + (weapon.SPD_ABS*0.1 - 2) * .03));
				double result = DPS * weapon.Speed * 0.03 * (0.94 + 0.003*weapon.Speed);

				// TODO: ToA damage bonus
				if (weapon.HandNeeded == eHandNeeded.TwoHands) //2h
				{
					result *= 1.1 + (WeaponSpecLevel(weapon)-1)*0.005;
					if (weapon is RangedWeapon)
					{
						Ammunition ammo = RangeAttackAmmo;
						// http://home.comcast.net/~shadowspawn3/bowdmg.html
						//ammo damage bonus
						if(ammo != null) //null with thrown weapon
						{
							switch(ammo.Damage)
							{
								case eDamageLevel.Light: result *= 0.85; break; //Blunt       (light) -15%
								case eDamageLevel.Medium: break; //Bodkin     (medium)   0%
								case eDamageLevel.Heavy: result *= 1.15;	break; //doesn't exist on live
								case eDamageLevel.XHeavy: result *= 1.25; break; //Broadhead (X-heavy) +25%
							}
						}
					}
				}

				return result;
			}
			else
			{ // TODO: whats the damage cap without weapon?
				return AttackDamage(weapon) * 3 * (1 + (AttackSpeed(weapon)*0.001 - 2) * .03);
			}
		}

		/// <summary>
		/// The chance for a critical hit
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override int AttackCriticalChance(Weapon weapon)
		{
			if (weapon != null && weapon is RangedWeapon && RangeAttackType == eRangeAttackType.Critical)
				return 0; // no crit damage for crit shots

			// no berserk for ranged weapons
			if (weapon!=null && !(weapon is RangedWeapon))
			{
				BerserkEffect berserk = (BerserkEffect) EffectList.GetOfType(typeof(BerserkEffect));
				if (berserk!=null)
				{
					return 100;
				}
			}

			// base 10% chance of critical for all with melee weapons
			return 10;
		}

		/// <summary>
		/// Returns the chance for a critical hit with a spell
		/// </summary>
		public override int SpellCriticalChance
		{
			get
			{
				if (CharacterClass.ClassType == eClassType.ListCaster)
					return 10;
				return 0;
			}
		}

		/// <summary>
		/// Returns the damage type of the current attack
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override eDamageType AttackDamageType(Weapon weapon)
		{
			if (weapon == null)
				return eDamageType.Natural;

			if(weapon is RangedWeapon)
			{
				Ammunition ammo = RangeAttackAmmo;
				if (ammo != null) return ammo.DamageType;
			}
			
			return weapon.DamageType;
		}

		/// <summary>
		/// Returns the AttackRange of this living
		/// </summary>
		public override int AttackRange
		{ /*
		tested with:
		staff					= 125-130
		sword			   		= 126-128.06
		shield (Numb style)		= 127-129
		polearm	(Impale style)	= 127-130
		mace (Daze style)		= 127.5-128.7

		Think it's safe to say that it never changes; different with mobs.
		*/
			get
			{
				GameLiving livingTarget = TargetObject as GameLiving;

				if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					RangedWeapon weapon = AttackWeapon as RangedWeapon;
					if(weapon == null)
						return 0;

					/*switch((eObjectType)weapon.Object_Type)
					{
						case eObjectType.Longbow:      range = 1760; break;
						case eObjectType.RecurvedBow:  range = 1680; break;
						case eObjectType.CompositeBow: range = 1600; break;
						default:                       range = 1200; break; // shortbow, xbow, throwing
					}*/

					double range = Math.Max(32, weapon.WeaponRange * GetModified(eProperty.ArcheryRange) * 0.01);

					Ammunition ammo = RangeAttackAmmo;
					if(ammo != null)
					{
						switch(ammo.Range)
						{
							case eRange.Short: range *= 0.85; break; //Clout -15%
							case eRange.Medium: break;
							case eRange.Long: range *= 1.15; break; //doesn't exist on live
							case eRange.XLong: range *= 1.25; break; //Flight +25%
						}
					}

					if(livingTarget != null && livingTarget.Position.Z > 0 && Position.Z > 0)
					{
						range += (Position.Z - livingTarget.Position.Z) * 0.5;
					}

					return (int)Math.Max(range, 32);
				}

				int meleerange = 128;
				/*GameKeepComponent keepcomponent = livingTarget as GameKeepComponent; // TODO better component melee attack range check
				if (keepcomponent != null)
				{
					meleerange += 150;
				}
				else*/
				{
					if(livingTarget!=null && livingTarget.IsMoving)
						meleerange += 32;
					if(IsMoving)
						meleerange += 32;
				}

				return meleerange;
			}
		}

		/// <summary>
		/// Gets the current attackspeed of this living in milliseconds
		/// </summary>
		/// <param name="weapons">attack weapons</param>
		/// <returns>effective speed of the attack. average if more than one weapon.</returns>
		public override int AttackSpeed(params Weapon[] weapons)
		{
			if (weapons == null || weapons.Length < 1)
				return 3400;

			int count = 0;
			double speed = 0;
			bool rangedWeapon = false;
			if(weapons[0] is RangedWeapon)
			{
				count = 1;
				speed = weapons[0].Speed / 100;
				rangedWeapon = true;
			}
			else
			{
				for (int i=0; i<weapons.Length; i++)
				{
					if (weapons[i] != null)
					{
						speed += weapons[i].Speed / 100;
						count++;
					}
				}
			}

			if (count < 1)
				return 0;

			speed /= count;

			int qui = Math.Min(250, GetModified(eProperty.Quickness)); //250 soft cap on quickness
			if(rangedWeapon)
			{
				//Draw Time formulas, there are very many ...
				//Formula 2: y = iBowDelay * ((100 - ((iQuickness - 50) / 5 + iMasteryofArcheryLevel * 3)) / 100)
				//Formula 1: x = (1 - ((iQuickness - 60) / 500 + (iMasteryofArcheryLevel * 3) / 100)) * iBowDelay
				//Table a: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * ((1-MoA*0.03) - (archeryspeedbonus/100))
				//Table b: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * (1-MoA*0.03) - ((archeryspeedbonus/100 * basebowspeed))

				//For now use the standard weapon formula, later add ranger haste etc.
				speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.ArcherySpeed);
				if(RangeAttackType==eRangeAttackType.Critical)
					speed = speed * 2 - (GetAbilityLevel(Abilities.Critical_Shot)-1) * speed / 10;
			}
			else
			{
				// TODO use haste
				//Weapon Speed*(1-(Quickness-60)/500]*(1-Haste)
				speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.MeleeSpeed);
			}

			// apply speed cap
			if (speed < 15)
			{
				speed = 15;
			}

			return (int)(speed * 100);
		}

		/// <summary>
		/// Gets the attack damage
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		/// <returns>the weapon damage</returns>
		public override double AttackDamage(Weapon weapon)
		{
			if (weapon == null)
				return 0;

			double damage = WeaponDamage(weapon)*weapon.DamagePerSecond*0.1;

			if (weapon.HandNeeded == eHandNeeded.TwoHands) // two-hand
			{
				// twohanded used weapons get 2H-Bonus = 10% + (Skill / 2)%
				int spec = WeaponSpecLevel(weapon)-1;
				damage *= 1.1+spec*0.005;
			}

			if (weapon is RangedWeapon)
			{
				Ammunition ammo = RangeAttackAmmo;
				//ammo damage bonus
				if(ammo != null)
				{
					switch(ammo.Damage)
					{
						case eDamageLevel.Light: damage *= 0.85; break; //Blunt       (light) -15%
						case eDamageLevel.Medium: break;
						case eDamageLevel.Heavy: damage *= 1.15;	break; //doesn't exist on live
						case eDamageLevel.XHeavy: damage *= 1.25; break; //Broadhead (X-heavy) +25%
					}
				}

				//Ranged damage buff and debuff
				damage = damage*GetModified(eProperty.RangedDamage)*0.01;
			}
			else
			{
				//Melee damage buff and debuff
				damage = damage*GetModified(eProperty.MeleeDamage)*0.01;
			}

			return damage;
		}

		/// <summary>
		/// Stores the amount of realm points gained by other players on last death
		/// </summary>
		protected long m_lastDeathRealmPoints;

		/// <summary>
		/// Gets/sets the amount of realm points gained by other players on last death
		/// </summary>
		public long LastDeathRealmPoints
		{
			get { return m_lastDeathRealmPoints; }
			set { m_lastDeathRealmPoints = value; }
		}

		/// <summary>
		/// Called when the player dies
		/// </summary>
		/// <param name="killer">the killer</param>
		public override void Die(GameLiving killer)
		{
			TargetObject = null;

			// cancel task if active
			if (Task != null) Task.ExpireTask();

			string message;
			ushort messageDistance = WorldMgr.DEATH_MESSAGE_DISTANCE;
			m_releaseType = eReleaseType.Normal;
			if(killer == null)
			{
				message = GetName(0, true) + " was just killed!";
			}
			else
			{
				if (DuelTarget == killer)
				{
					m_releaseType = eReleaseType.Duel;
					messageDistance = WorldMgr.YELL_DISTANCE;
					message = GetName(0, true) + " was just defeated in a duel by " + killer.GetName(1, false) + "!";
				}
				else
				{
					message = GetName(0, true) + " was just killed by " + killer.GetName(1, false) + ".";
				}
			}

			DuelStop();

			/*
			1.65 Release Notes
			- We now, by default, display RvR combat deaths in the color of the realm scoring the kill.

			Example:

			(green text)Midchar was just killed by Hibchar!
			(blue text)Albchar was just killed by Midchar!
			(red text) HibChar was just killed by Albion Keep Lord!
			*/

			eChatType messageType;
			if(m_releaseType == eReleaseType.Duel)
				messageType = eChatType.CT_Emote;
			else if(killer==null)
			{
				messageType = eChatType.CT_PlayerDied;
			}
			else
			{
				switch((eRealm)killer.Realm)
				{
					case eRealm.Albion:   messageType = eChatType.CT_KilledByAlb; break;
					case eRealm.Midgard:  messageType = eChatType.CT_KilledByMid; break;
					case eRealm.Hibernia: messageType = eChatType.CT_KilledByHib; break;
					default:              messageType = eChatType.CT_PlayerDied;  break; // killed by mob
				}
			}

			if(killer is GamePlayer && killer != this)
			{
				((GamePlayer)killer).Out.SendMessage("You just killed "+ GetName(0, false) +"!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			}

			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), messageDistance))
			{
				// on normal server type send messages only to the killer and dead players realm
				// check for gameplayer is needed because killers realm don't see deaths by guards
				if (
					(player != killer) && (
					                      	(killer != null && killer is GamePlayer && GameServer.ServerRules.IsSameRealm((GamePlayer)killer, player, true))
					                      	|| (GameServer.ServerRules.IsSameRealm(this, player, true)))
					)
					player.Out.SendMessage(message, messageType, eChatLoc.CL_SystemWindow);
			}

			//Dead ppl. dismount ...
			if (Steed != null) DismountSteed();

			//Dead ppl. don't sit ...
			if (Sitting)
			{
				Sitting = false;
				UpdatePlayerStatus();
			}

			DivingTimerStop();

			// then buffs drop messages
			base.Die(killer);

			lock (this)
			{
				if (m_releaseTimer != null)
				{
					m_releaseTimer.Stop();
					m_releaseTimer = null;
				}
				if (m_quitTimer != null)
				{
					m_quitTimer.Stop();
					m_quitTimer = null;
				}
				m_automaticRelease = m_releaseType == eReleaseType.Duel;
				m_releasePhase = 0;
				m_deathTick = Environment.TickCount; // we use realtime, because timer window is realtime
				//UpdatePlayerStatus();

				Out.SendTimerWindow("Release Timer", (m_automaticRelease ? RELEASE_MINIMUM_WAIT : RELEASE_TIME));
				m_releaseTimer = new RegionTimer(this);
				m_releaseTimer.Callback = new RegionTimerCallback(ReleaseTimerCallback);
				m_releaseTimer.Start(1000);

				Out.SendMessage("You have died.  Type /release to return to your last bindpoint.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);

				// clear target object so no more actions can used on this target, spells, styles, attacks...
				TargetObject=null;

				foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendPlayerDied(this, killer);

				// first penalty is 5% of expforlevel, second penalty comes from release
				int xpLossPercent;
				if (Level < 40)
				{
					xpLossPercent = MAX_LEVEL - Level;
				}
				else
				{
					xpLossPercent = MAX_LEVEL - 40;
				}

				if (killer != null && killer.Realm != (byte)eRealm.None)
				{
					Out.SendMessage("You died fighting for your realm and lose no experience!", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					xpLossPercent = 0;
				}
				else if (Level > 5) // under level 5 there is no penalty
				{
					Out.SendMessage("You lose some experience!", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					// if this is the first death in level, you lose only half the penalty
					switch (DeathCount)
					{
						case 0:
							Out.SendMessage("This is your first death for this level.  Your experience and constitution losses are greatly reduced.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
							xpLossPercent /= 3;
							break;

						case 1:
							Out.SendMessage("This is your second death for this level.  Your experience and constitution losses are reduced.", eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
							xpLossPercent = xpLossPercent * 2 / 3;
							break;
					}

					DeathCount++;

					long xpLoss = (ExperienceForNextLevel - ExperienceForCurrentLevel)*xpLossPercent/1000;
					GainExperience(-xpLoss, 0, 0, false);
					TempProperties.setProperty(DEATH_EXP_LOSS_PROPERTY, xpLoss);

					int conLoss = DeathCount;
					if (conLoss > 3)
						conLoss = 3;
					else if (conLoss < 1)
						conLoss=1;
					TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conLoss);
				}
				GameEventMgr.AddHandler(this,GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
			}

			CommandNpcRelease();

			// sent after buffs drop
			Message.SystemToOthers(this, GetName(0, true) + " just died.  " + GetPronoun(1, true) + " corpse lies on the ground.", eChatType.CT_PlayerDied);
			if (m_releaseType == eReleaseType.Duel)
			{
				Message.SystemToOthers(this, killer.Name + " wins the duel!", eChatType.CT_Emote);
			}

			// deal out exp and realm points based on server rules
			// no other way to keep correct message order...
			GameServer.ServerRules.OnPlayerKilled(this, killer);
			ClearXPGainer();

			DeathTime = PlayedTime;

			// DOLConsole.WriteLine("Die();");
		}


		/// <summary>
		/// Check this flag to see wether this living is involved in combat
		/// </summary>
		public override bool InCombat
		{
			get
			{
				IControlledBrain npc = ControlledNpc;
				if(npc != null && npc.Body.InCombat)
					return true;
				return base.InCombat;
			}
		}

		#endregion

		#region Duel

		/// <summary>
		/// The duel target of this player
		/// </summary>
		protected GamePlayer m_duelTarget;

		/// <summary>
		/// Gets the duel target of this player
		/// </summary>
		public GamePlayer DuelTarget
		{
			get { return m_duelTarget; }
		}

		/// <summary>
		/// Starts the duel
		/// </summary>
		/// <param name="duelTarget">The duel target</param>
		public void DuelStart(GamePlayer duelTarget)
		{
			if (DuelTarget != null)
				return;

			GameEventMgr.AddHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
			GameEventMgr.AddHandler(this, GameLivingBaseEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
			GameEventMgr.AddHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
			m_duelTarget = duelTarget;
			duelTarget.DuelStart(this);
		}

		/// <summary>
		/// Stops the duel if it is running
		/// </summary>
		public void DuelStop()
		{
			GamePlayer target = DuelTarget;
			if (target == null)
				return;

			foreach (GameSpellEffect effect in EffectList.GetAllOfType(typeof(GameSpellEffect)))
			{
				if (effect.SpellHandler.Caster == target && !effect.SpellHandler.HasPositiveEffect)
					effect.Cancel(false);
			}
			m_duelTarget = null;
			GameEventMgr.RemoveHandler(this, GamePlayerEvent.Quit, new DOLEventHandler(DuelOnPlayerQuit));
			GameEventMgr.RemoveHandler(this, GameLivingBaseEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
			
			ClearXPGainer();
			Out.SendMessage("Your duel ends!", eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
			target.DuelStop();
		}

		/// <summary>
		/// Stops the duel if player attack or is attacked by anything other that duel target
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void DuelOnAttack(DOLEvent e, object sender, EventArgs arguments)
		{
			AttackData ad = null;
			GameLivingBase target = null;
			if (arguments is AttackFinishedEventArgs)
			{
				ad = ((AttackFinishedEventArgs)arguments).AttackData;
				target = ad.Target;
			}
			else if (arguments is AttackedByEnemyEventArgs)
			{
				ad = ((AttackedByEnemyEventArgs)arguments).AttackData;
				target = ad.Attacker;
			}

			if (ad == null)
				return;

			// check pets owner for my and enemy attacks
			GameNPC npc = target as GameNPC;
			if (npc != null)
			{
				IControlledBrain brain = npc.Brain as IControlledBrain;
				if (brain != null)
					target = brain.Owner;
			}

			switch (ad.AttackResult)
			{
				case eAttackResult.Blocked:
				case eAttackResult.Evaded:
				case eAttackResult.Fumbled:
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
				case eAttackResult.Missed:
				case eAttackResult.Parried:
					if (target != DuelTarget)
						DuelStop();
					break;
			}
		}

		/// <summary>
		/// Stops the duel on quit/link death
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		private void DuelOnPlayerQuit(DOLEvent e, object sender, EventArgs arguments)
		{
			DuelStop();
		}

		#endregion

		#region Spell cast

		/// <summary>
		/// The time someone can not cast
		/// </summary>
		protected long m_disabledCastingTimeout=0;
		/// <summary>
		/// Time when casting is allowed again (after interrupt from enemy attack)
		/// </summary>
		public long DisabledCastingTimeout
		{
			get { return m_disabledCastingTimeout; }
			set { m_disabledCastingTimeout = value; }
		}

		/// <summary>
		/// Table of skills currently disabled
		/// skill => disabletimeout (ticks) or 0 when endless
		/// </summary>
		protected readonly Hashtable m_disabledSkills = new Hashtable();

		/// <summary>
		/// Gets the time left for disabling this skill in milliseconds
		/// </summary>
		/// <param name="skill"></param>
		/// <returns>milliseconds left for disable</returns>
		public virtual int GetSkillDisabledDuration(Skill skill)
		{
			lock (m_disabledSkills.SyncRoot)
			{
				object time = m_disabledSkills[skill];
				if (time != null)
				{
					long timeout = (long)time;
					long left = timeout - Region.Time;
					if (left <= 0)
					{
						left = 0;
						m_disabledSkills.Remove(skill);
					}
					return (int)left;
				}
			}
			return 0;
		}

		/// <summary>
		/// Gets a copy of all disabled skills
		/// </summary>
		/// <returns></returns>
		public virtual ICollection GetAllDisabledSkills()
		{
			lock (m_disabledSkills.SyncRoot)
			{
				return ((Hashtable)m_disabledSkills.Clone()).Keys;
			}
		}

		/// <summary>
		/// Grey out some skills on client for specified duration
		/// </summary>
		/// <param name="skill">the skill to disable</param>
		/// <param name="duration">duration of disable in milliseconds</param>
		public virtual void DisableSkill(Skill skill, int duration)
		{
			lock (m_disabledSkills.SyncRoot)
			{
				if (duration > 0)
				{
					m_disabledSkills[skill] = Region.Time + duration;
				}
				else
				{
					m_disabledSkills.Remove(skill);
					duration = 0;
				}
			}
			Out.SendDisableSkill(skill, duration/1000+1);
		}

		/// <summary>
		/// Updates all disabled skills to player
		/// </summary>
		public virtual void UpdateDisabledSkills()
		{
			foreach (Skill skl in GetAllDisabledSkills())
			{
				Out.SendDisableSkill(skl, GetSkillDisabledDuration(skl)/1000+1);
			}
		}

		/// <summary>
		/// The next spell
		/// </summary>
		protected Spell m_nextSpell;
		/// <summary>
		/// The next spell line
		/// </summary>
		protected SpellLine m_nextSpellLine;
		/// <summary>
		/// A lock for the spellqueue
		/// </summary>
		protected object m_spellQueueAccessMonitor = new object();

		/// <summary>
		/// Clears the spell queue when a player is interrupted
		/// </summary>
		public void ClearSpellQueue()
		{
			lock (m_spellQueueAccessMonitor)
			{
				m_nextSpell = null;
				m_nextSpellLine = null;
			}
		}

		/// <summary>
		/// Callback after spell execution finished and next spell can be processed
		/// </summary>
		/// <param name="handler"></param>
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			lock (m_spellQueueAccessMonitor)
			{
				Spell nextSpell = m_nextSpell;
				SpellLine nextSpellLine = m_nextSpellLine;

				m_runningSpellHandler = null;
				m_nextSpell = null;			// avoid restarting nextspell by reentrance from spellhandler
				m_nextSpellLine = null;

				if(nextSpell!=null)
					m_runningSpellHandler = ScriptMgr.CreateSpellHandler(this, nextSpell, nextSpellLine);
			}
			if(m_runningSpellHandler!=null)
			{
				m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler.CastSpell();
			}
		}

		/// <summary>
		/// Cast a specific spell from given spell line
		/// </summary>
		/// <param name="spell">spell to cast</param>
		/// <param name="line">Spell line of the spell (for bonus calculations)</param>
		public override void CastSpell(Spell spell, SpellLine line)
		{
			if(Stun)
			{
				Out.SendMessage("You can't cast while stunned!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			if(Mez)
			{
				Out.SendMessage("You can't cast while mesmerized!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}

			lock (m_spellQueueAccessMonitor)
			{
				if (m_runningSpellHandler!=null && spell.CastTime>0)
				{
					if (m_runningSpellHandler.Spell.InstrumentRequirement != 0)
					{
						Out.SendMessage("You are already playing a song!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
						return;
					}
					if (SpellQueue)
					{
						Out.SendMessage("You are already casting a spell!  You prepare this spell as a follow up!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
						m_nextSpell = spell;
						m_nextSpellLine = line;
					}
					else Out.SendMessage("You are already casting a spell, to enable spell queueing use '/noqueue'!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					return;
				}
			}

			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
			if (spellhandler != null)
			{
				if (spell.CastTime > 0)
				{
					m_runningSpellHandler = spellhandler;
					m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
					spellhandler.CastSpell();
				}
				else
				{
					// insta cast no queue and no callback
					spellhandler.CastSpell();
				}
			}
			else
			{
				Out.SendMessage(spell.Name + " not implemented yet (" + spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Send/Say/Yell/Whisper

		/// <summary>
		/// Delegate to be called when this player receives a text
		/// by someone sending something
		/// </summary>
		public delegate bool SendReceiveHandler(GamePlayer source, GamePlayer receiver, string str);

		/// <summary>
		/// Event that is fired when the Player receives a Send text
		/// </summary>
		public event SendReceiveHandler OnSendReceive;

		/// <summary>
		/// Clears all OnSendReceive event handlers
		/// </summary>
		public void ClearOnSendReceive()
		{
			OnSendReceive = null;
		}

		/// <summary>
		/// This function is called when the Player receives a sent text
		/// </summary>
		/// <param name="source">GamePlayer that was sending</param>
		/// <param name="str">string that was sent</param>
		/// <returns>true if the string was received successfully, false if it was not received</returns>
		public virtual bool SendReceive(GamePlayer source, string str)
		{
			if (OnSendReceive != null && !OnSendReceive(source, this, str))
				return false;
			if(GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(source.Name + LanguageMgr.GetString("GamePlayer.SendReceive.sends"," sends, ") + "\"" + str + "\"", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
			else
			{
				Out.SendMessage(source.Name + " sends something in a language you don't understand.", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				return true;
			}

			string afkmessage = TempProperties.getProperty(AFK_MESSAGE, null);
			if(afkmessage != null)
			{
				if(afkmessage == "")
				{
					source.Out.SendMessage(Name + " is currently AFK.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					source.Out.SendMessage("<AFK> " + Name + " sends, " + "\"" + afkmessage + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
				}
			}

			return true;
		}

		/// <summary>
		/// Delegate to be called when this player is about to send a text
		/// </summary>
		public delegate bool SendHandler(GamePlayer source, GamePlayer receiver, string str);

		/// <summary>
		/// Event that is fired when the Player is about to send a text
		/// </summary>
		public event SendHandler OnSend;
		/// <summary>
		/// Clears all send event handlers
		/// </summary>
		public void ClearOnSend()
		{
			OnSend = null;
		}

		/// <summary>
		/// Sends a text to a target
		/// </summary>
		/// <param name="target">The target of the send</param>
		/// <param name="str">string to send (without any "xxx sends:" in front!!!)</param>
		/// <returns>true if text was sent successfully</returns>
		public virtual bool Send(GamePlayer target, string str)
		{
			if (target == null || str == null)
				return false;
			if (OnSend != null && !OnSend(this, target, str))
				return false;
			if (!target.SendReceive(this, str))
			{
				Out.SendMessage(target.Name + " doesn't seem to understand you!", eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				return false;
			}
			else
			{
				Out.SendMessage("You send, " + "\"" + str + "\" to " + target.Name, eChatType.CT_Send, eChatLoc.CL_ChatWindow);
			}
			return true;
		}

		/// <summary>
		/// This function is called when the Player receives a Say text!
		/// </summary>
		/// <param name="source">The source living saying something</param>
		/// <param name="str">the text that was said</param>
		/// <returns>true if received successfully</returns>
		public override bool SayReceive(GameLiving source, string str)
		{
			if (!base.SayReceive(source, str))
				return false;
			if(GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(source.GetName(0, false) + " says, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			else
				Out.SendMessage(source.GetName(0, false) + " says something in a language you don't understand.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// Call this function to make the player say something
		/// </summary>
		/// <param name="str">string to say</param>
		/// <returns>true if said successfully</returns>
		public override bool Say(string str)
		{
			if(!GameServer.ServerRules.IsAllowedToSpeak(this, "talk"))
				return false;
			if (!base.Say(str))
				return false;
			Out.SendMessage("You say, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// This function is called when the player hears a yell
		/// </summary>
		/// <param name="source">the source living yelling</param>
		/// <param name="str">string that was yelled</param>
		/// <returns>true if received successfully</returns>
		public override bool YellReceive(GameLiving source, string str)
		{
			if (!base.YellReceive(source, str))
				return false;
			if(GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(source.GetName(0, false) + " yells, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			else
				Out.SendMessage(source.GetName(0, false) + " yells something in a language you don't understand.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// Call this function to make the player yell something
		/// </summary>
		/// <param name="str">string to yell</param>
		/// <returns>true if yelled successfully</returns>
		public override bool Yell(string str)
		{
			if(!GameServer.ServerRules.IsAllowedToSpeak(this, "yell"))
				return false;
			if (!base.Yell(str))
				return false;
			Out.SendMessage("You yell, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// This function is called when the player hears a whisper
		/// from some living
		/// </summary>
		/// <param name="source">Source that was living</param>
		/// <param name="str">string that was whispered</param>
		/// <returns>true if whisper was received successfully</returns>
		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;
			if(GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(source.GetName(0, false) + " whispers to you, \"" + str + "\"", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			else
				Out.SendMessage(source.GetName(0, false) + " whispers something in a language you don't understand.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// Call this function to make the player whisper to someone
		/// </summary>
		/// <param name="target">GameLiving to whisper to</param>
		/// <param name="str">string to whisper</param>
		/// <returns>true if whispered successfully</returns>
		public override bool Whisper(GameLiving target, string str)
		{
			if (target == null)
			{
				Out.SendMessage("Select a target to whisper to!", eChatType.CT_System, eChatLoc.CL_ChatWindow);
				return false;
			}
			if(!GameServer.ServerRules.IsAllowedToSpeak(this, "whisper"))
				return false;
			if (!base.Whisper(target, str))
				return false;
			if (target is GamePlayer)
				Out.SendMessage("You whisper, \"" + str + "\" to " + target.GetName(0, false), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		#endregion

		#region Steed

		/// <summary>
		/// Holds the GameSteed that is the steed of this player as weakreference
		/// </summary>
		protected WeakReference m_steed;

		/// <summary>
		/// Holds the Steed of this player
		/// </summary>
		public GameSteed Steed
		{
			get { return m_steed.Target as GameSteed; }
			set { m_steed.Target = value; }
		}

		/// <summary>
		/// Mounts the player onto a steed
		/// </summary>
		/// <param name="steed">the steed to mount</param>
		/// <returns>true if mounted successfully or false if not</returns>
		public virtual void MountSteed(GameSteed steed)
		{
			if (Steed != null) DismountSteed();
			
			//	Notify(Game¨PlayerEvent.SteedMount, this, new SteedMountEventArgs(this, steed));
			
			Steed = steed;
			Steed.Rider = this;

			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRiding(this, Steed, false);
			}
		}

		/// <summary>
		/// Dismounts the player from it's steed.
		/// </summary>
		/// <returns>true if the dismount was successful, false if not</returns>
		public virtual void DismountSteed()
		{
			if (Steed == null) return;

		//	Notify(GamPlayerEvent.SteedDismount, this, new SteedDismountEventArgs(this, Steed));
			
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRiding(this, Steed, true);
			}

			Steed.Rider = null;
			Steed = null;
		}

		#endregion

		#region Add/Move/Remove/BroadcastCreate/BroadcastRemove/UpdateEquipementAppearance/UpdatePlayerStatus

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player != this)
				{
					player.Out.SendPlayerCreate(this);
					player.Out.SendLivingEquipementUpdate(this);
				}
			}
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player != this)
				{
					player.Out.SendRemoveObject(this, eRemoveType.Kill);
				}
			}
		}

		/// <summary>
		/// Updates the appearance of the equipment this player is using to all players around
		/// </summary>
		public virtual void UpdateEquipementAppearance()
		{
			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player != this)
					player.Out.SendLivingEquipementUpdate(this);
			}
		}

		/// <summary>
		/// Updates Health, Mana, Sitting, Endurance, Concentration and Alive status to client
		/// </summary>
		public void UpdatePlayerStatus()
		{
			//DOLConsole.WriteLine("StatusUpdate: Health="+HealthPercent+" Mana="+ManaPercent+" Sitting="+Sitting+" Endu="+EndurancePercent+" Conc="+ConcentrationPercent+" Alive="+Alive);
			if (ObjectState != eObjectState.Active) return;
			Out.SendStatusUpdate();
		}

		/// <summary>
		/// Called to create an player in the world and send the other
		/// players around this player an update
		/// </summary>
		/// <returns>true if created, false if creation failed</returns>
		public override bool AddToWorld()
		{
			//DOLConsole.WriteLine("add to world "+Name);
			if (!base.AddToWorld())
				return false;
		    
			m_pvpInvulnerabilityTick = 0;
			m_healthRegenerationTimer = new RegionTimer(this);
			m_powerRegenerationTimer = new RegionTimer(this);
			m_enduRegenerationTimer = new RegionTimer(this);
			m_healthRegenerationTimer.Callback = new RegionTimerCallback(HealthRegenerationTimerCallback);
			m_powerRegenerationTimer.Callback = new RegionTimerCallback(PowerRegenerationTimerCallback);
			m_enduRegenerationTimer.Callback = new RegionTimerCallback(EnduranceRegenerationTimerCallback);

			m_maxLastZ = int.MinValue; //make sure we don't take any falling damage when changing zone
			
		    if(m_task != null)
		    {
                m_task.TaskExpireTimer = new RegionTimer(this);
                m_task.TaskExpireTimer.Callback = new RegionTimerCallback(m_task.TaskExpireTimerCallback);
		    }
		       
			return true;
		}

		/// <summary>
		/// Called to remove the item from the world. Also removes the
		/// player visibly from all other players around this one
		/// </summary>
		/// <returns>true if removed, false if removing failed</returns>
		public override bool RemoveFromWorld()
		{
			DismountSteed();

			foreach(AbstractArea area in CurrentAreas)
			{
				area.OnPlayerLeave(this);
			}

			CommandNpcRelease();

			PrayTimerStop();
			DivingTimerStop();

			if (!base.RemoveFromWorld()) return false;
			if (m_pvpInvulnerabilityTimer != null)
			{
				m_pvpInvulnerabilityTimer.Stop();
				m_pvpInvulnerabilityTimer = null;
			}

            if (m_task != null) m_task.StopTaskExpireTimer();
		    
			return true;
		}

		/// <summary>
		/// The property to save debug mode on region change
		/// </summary>
		public const string DEBUG_MODE_PROPERTY = "Player.DebugMode";

		/// <summary>
		/// This function moves a player to a specific region and
		/// specific coordinates.
		/// </summary>
		/// <param name="targetRegion">Region to move to</param>
		/// <param name="newPosition">target position</param>
		/// <param name="heading">Target heading</param>
		/// <returns>true if move succeeded, false if failed</returns>
		public override bool MoveTo(Region targetRegion, Point newPosition, ushort heading)
		{
			if (m_region != targetRegion) //If the destination is in another region
			{
				if (!RemoveFromWorld()) return false;
				m_position = newPosition;
				m_heading = heading;
				m_region = targetRegion;
				
				Out.SendRegionChanged(); //Send the region update packet, the rest will be handled by the packethandlers

				return true;
			}

			DismountSteed();

			foreach(AbstractArea area in CurrentAreas)
			{
				area.OnPlayerLeave(this);
			}

			CommandNpcRelease();

			if(!base.MoveTo(targetRegion, newPosition, heading)) return false;
		
			//Add the player to the new coordinates
			Out.SendPlayerJump(false);
			CurrentUpdateArray.SetAll(false);
			foreach (GameNPC npc in GetInRadius(typeof(GameNPC), WorldMgr.VISIBILITY_DISTANCE))
			{
				Out.SendNPCCreate(npc);
				if (npc.Inventory != null)
					Out.SendLivingEquipementUpdate(npc);
				//Send health update only if mob-health is not 100%
				if (npc.HealthPercent != 100)
					Out.SendNPCUpdate(npc);
				CurrentUpdateArray[npc.ObjectID-1] = true;
			}
			
			return true;
		}

		/// <summary>
		/// Holds unique locations array
		/// </summary>
		protected GameLocation[] m_lastUniqueLocations;
		
		/// <summary>
		/// This method is called each time update the position of the player
		/// Nerver call it outside of the PlayerPositionUpdateHandler !!!
		/// </summary>
		public void UpdateInternalPosition(Point newPosition)
		{
			m_position = newPosition;
		
			//Moving too far cancels trades
			if (TradeWindow != null)
			{
				if (!m_position.CheckDistance(TradeWindow.Partner.Position, WorldMgr.GIVE_ITEM_DISTANCE))
				{
					Out.SendMessage("You move too far from your trade partner and cancel the trade!,", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					TradeWindow.Partner.Out.SendMessage("Your trade partner moves too far away and cancels the trade!,", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					TradeWindow.CloseTrade();
				}
			}

			lock (m_lastUniqueLocations)
			{
				GameLocation loc = m_lastUniqueLocations[0];
				if (loc.Position != newPosition || loc.Region != Region)
				{
					loc = m_lastUniqueLocations[m_lastUniqueLocations.Length - 1];
					Array.Copy(m_lastUniqueLocations, 0, m_lastUniqueLocations, 1, m_lastUniqueLocations.Length - 1);
					m_lastUniqueLocations[0] = loc;
					loc.Position = newPosition;
					loc.Heading = (ushort)Heading;
					loc.Region = Region;
				}
			}
		}


		#endregion

		#region Group

		/// <summary>
		/// Holds the group of this player
		/// </summary>
		protected PlayerGroup m_playerGroup;
		/// <summary>
		/// Holds the index of this player inside of the group
		/// </summary>
		protected int m_playerGroupIndex;
		/// <summary>
		/// true if this player is looking for a group
		/// </summary>
		protected bool m_lookingForGroup;
		/// <summary>
		/// true if this player want to receive loot with autosplit between members of group
		/// </summary>
		protected bool m_autoSplitLoot = true;
		/// <summary>
		/// Gets or sets the player's group
		/// </summary>
		public PlayerGroup PlayerGroup
		{
			get { return m_playerGroup; }
			set { m_playerGroup = value; }
		}

		/// <summary>
		/// Gets or sets the index of this player inside of the group
		/// </summary>
		public int PlayerGroupIndex
		{
			get { return m_playerGroupIndex; }
			set { m_playerGroupIndex = value; }
		}

		/// <summary>
		/// Gets or sets the LookingForGroup flag in this player
		/// </summary>
		public bool LookingForGroup
		{
			get { return m_lookingForGroup; }
			set { m_lookingForGroup = value; }
		}
		/// <summary>
		/// Gets/sets the autosplit for loot
		/// </summary>
		public bool AutoSplitLoot
		{
			get { return m_autoSplitLoot; }
			set { m_autoSplitLoot = value; }
		}

		#endregion

		#region CurrentSpeed / Area / Sitting / Straffing / Sprint / GroundTarget / isSwimming / isDiving

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		private IList m_currentAreas;
		
		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		public IList CurrentAreas
		{
			get { return m_currentAreas; }
			set { m_currentAreas = value; }
		}

		/// <summary>
		/// The tick when the areas should be updated
		/// </summary>
		public long m_areaUpdateTick = 0;

		/// <summary>
		/// Gets the tick when the areas should be updated
		/// </summary>
		public long AreaUpdateTick
		{
			get { return m_areaUpdateTick; }
			set { m_areaUpdateTick = value; }
		}

		/// <summary>
		/// The current speed of this player
		/// </summary>
		protected int m_currentSpeed;

		/// <summary>
		/// Gets or sets the current speed of this player
		/// </summary>
		public override int CurrentSpeed
		{
			get { return m_currentSpeed; }
			set
			{
				m_currentSpeed = value;
				if(m_currentSpeed != 0)
				{
					if(Sitting)
					{
						Sit(false);
					}
					// copy/pasted to Strafing property
					if(m_runningSpellHandler!=null && m_runningSpellHandler.IsCasting)
					{
						m_runningSpellHandler.CasterMoves();
					}
					if (IsCrafting)
					{
						Out.SendMessage("You move and interrupt your crafting.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						CraftTimer.Stop();
						CraftTimer = null;
						Out.SendCloseTimerWindow();
					}
					if(AttackState)
					{
						if(ActiveWeaponSlot == eActiveWeaponSlot.Distance)
						{
							string attackTypeMsg = (AttackWeapon is ThrownWeapon ? "throw" : "shot");
							Out.SendMessage("You move and interrupt your "+attackTypeMsg+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
							StopAttack();
						}
						else
						{
							AttackData ad = TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
							if(ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
							{
								//Does the target can be attacked ?
								if(ad.Target != null && IsObjectInFront(ad.Target, 120) && Position.CheckDistance(ad.Target.Position, AttackRange) && m_attackAction != null)
								{
									m_attackAction.Start(1);
								}
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Holds the players max Z for fall damage
		/// </summary>
		private int m_maxLastZ;

		/// <summary>
		/// Gets or sets the players max Z for fall damage
		/// </summary>
		public int MaxLastZ
		{
			get { return m_maxLastZ; }
			set { m_maxLastZ = value; }
		}

		/// <summary>
		/// The suck state of this player
		/// </summary>
		private bool m_stuckFlag = false;

		/// <summary>
		/// Gets/sets the current stuck state
		/// </summary>
		public virtual bool Stuck
		{
			get { return m_stuckFlag; }
			set { m_stuckFlag = value; }
		}

		/// <summary>
		/// The sitting state of this player
		/// </summary>
		protected bool m_sitting;
		/// <summary>
		/// Gets/sets the current sit state
		/// </summary>
		public override bool Sitting
		{
			get { return m_sitting; }
			set
			{
				m_sitting = value;
				if (value)
				{
					if(m_runningSpellHandler!=null && m_runningSpellHandler.IsCasting)
						m_runningSpellHandler.CasterMoves();
					if(AttackState && ActiveWeaponSlot==eActiveWeaponSlot.Distance)
					{
						string attackTypeMsg="shot";
						if(AttackWeapon is ThrownWeapon)
							attackTypeMsg="throw";
						Out.SendMessage("You move and interrupt your "+attackTypeMsg+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						StopAttack();
					}
				}
			}
		}


		/// <summary>
		/// the moving state of this player
		/// </summary>
		public override bool IsMoving
		{
			get { return CurrentSpeed > 0 || Strafing; }
		}
		/// <summary>
		/// The sprint effect of this player
		/// </summary>
		protected SprintEffect m_sprintEffect = null;

		/// <summary>
		/// Gets sprinting flag
		/// </summary>
		public bool IsSprinting
		{
			get { return m_sprintEffect != null; }
		}
		/// <summary>
		/// Change sprint state of this player
		/// </summary>
		/// <param name="state">new state</param>
		/// <returns>sprint state after command</returns>
		public virtual bool Sprint(bool state)
		{
			if(state == IsSprinting)
				return state;

			if (state)
			{
				// can't start sprinting with 10 endurance on 1.68 server
				if(EndurancePercent <= 10)
				{
					Out.SendMessage("You are too fatigued to sprint!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if(IsStealthed)
				{
					Out.SendMessage("You can't sprint while you are hidden!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (!Alive)
				{
					Out.SendMessage("You can't sprint when dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				m_sprintEffect = new SprintEffect();
				m_sprintEffect.Start(this);
				Out.SendUpdateMaxSpeed();
				Out.SendMessage("You prepare to sprint!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			else
			{
				m_sprintEffect.Stop();
				m_sprintEffect = null;
				Out.SendUpdateMaxSpeed();
				Out.SendMessage("You are no longer ready to sprint.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
		}

		/// <summary>
		/// The strafe state of this player
		/// </summary>
		protected bool m_strafing;

		/// <summary>
		/// Gets/sets the current strafing mode
		/// </summary>
		public virtual bool Strafing
		{
			set
			{
				m_strafing = value;
				if(value)
				{
					// copy/pasted to CurrentSpeed property
					if(m_runningSpellHandler!=null && m_runningSpellHandler.IsCasting)
					{
						m_runningSpellHandler.CasterMoves();
					}
					if (IsCrafting)
					{
						Out.SendMessage("You move and interrupt your crafting.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						CraftTimer.Stop();
						CraftTimer = null;
						Out.SendCloseTimerWindow();
					}
					if(AttackState && ActiveWeaponSlot==eActiveWeaponSlot.Distance)
					{
						string attackTypeMsg="shot";
						if(AttackWeapon is ThrownWeapon)
							attackTypeMsg="throw";
						Out.SendMessage("You move and interrupt your "+attackTypeMsg+"!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
						StopAttack();
					}
				}
			}
			get { return m_strafing; }
		}

		/// <summary>
		/// Sits/Stands the player
		/// </summary>
		/// <param name="sit">True if sitting, otherwise false</param>
		public virtual void Sit(bool sit)
		{
			if (Sitting == sit)
			{
				if (sit)
					Out.SendMessage ("You are already sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (!sit)
					Out.SendMessage ("You are not sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return; // already done
			}

			if (!Alive)
			{
				Out.SendMessage("You can't sit down now, you're dead!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if(Stun)
			{
				Out.SendMessage("You can't rest when you are stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if(Mez)
			{
				Out.SendMessage("You can't rest when you are mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (sit && CurrentSpeed > 0)
			{
				Out.SendMessage("You must be standing still to sit down.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (Steed != null)
			{
				Out.SendMessage("You must dismount first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			if (sit)
			{
				Out.SendMessage("You sit down.  Type '/stand' or move to stand up.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage("You stand up.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}

			//Stop attack if you sit down while attacking
			if (sit && AttackState)
			{
				StopAttack();
			}

			if (!sit)
			{
				//Stop quit sequence if you stand up
				if (m_quitTimer != null)
				{
					m_quitTimer.Stop();
					m_quitTimer = null;
					Stuck = false;
					Out.SendMessage("You are no longer waiting to quit.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				//Stop praying if the player stands up...
				PrayTimerStop();
			}
			//Update the client
			Sitting = sit;
			UpdatePlayerStatus();
		}


		/// <summary>
		/// Gets or sets the Living's ground-target Coordinate inside the current Region
		/// </summary>
		public override Point GroundTarget
		{
			get { return base.GroundTarget; }
			set
			{
				base.GroundTarget = value;
				Out.SendMessage(String.Format("You ground-target {0},{1},{2}", value.X, value.Y, value.Z), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			//	if (SiegeWeapon != null)
			//		SiegeWeapon.GroundTarget = value;
			}
		}

		/// <summary>
		/// Does the player is swimming
		/// </summary>
		protected bool m_isSwimming;

		/// <summary>
		/// Gets or sets if the player is swimming (!= diving)
		/// </summary>
		public override bool IsSwimming
		{
			get { return m_isSwimming; }
			set { m_isSwimming = value; }
		}

		/// <summary>
		/// Gets or sets if the player is diving (!= swimming)
		/// </summary>
		public override bool IsDiving
		{
			get { return DivingState; }
			set
			{
				if(value == false)
				{
					DivingTimerStop();
				}
				else if(Alive && !DivingState)
				{
					if (m_divingAction != null)
						m_divingAction.Stop();

					m_divingAction = new DivingAction(this);
					m_divingAction.Start(30000); // 30 s Holding Breath

					Out.SendTimerWindow("Holding Breath", 30);
				}
			}
		}

		/// <summary>
		/// Gets the diving-state of this living
		/// </summary>
		public virtual bool DivingState
		{
			get { return m_divingAction != null && m_divingAction.IsAlive; }
		}

		/// <summary>
		/// The timer that will be started when the player start diving
		/// </summary>
		protected DivingAction m_divingAction;

		/// <summary>
		/// Stop diving; used when player die / removefromworld 
		/// </summary>
		public void DivingTimerStop()
		{
			if (!DivingState)
				return;
			m_divingAction.Stop();
			m_divingAction = null;

			Out.SendCloseTimerWindow();
		}

		/// <summary>
		/// The timed diving action
		/// </summary>
		protected class DivingAction : RegionAction
		{
			/// <summary>
			/// Store the tick when the payer start to be underwater
			/// </summary>
			protected readonly uint m_startDivingTick;

			/// <summary>
			/// Constructs a new diving action
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public DivingAction(GamePlayer actionSource) : base(actionSource)
			{
				m_startDivingTick = (uint)Environment.TickCount; // we use realtime, because timer window is realtime
			}

			/// <summary>
			/// Callback method for the diving-timer
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				if (Environment.TickCount - m_startDivingTick >= 90000) // 30 sec Holding Breath + 60 sec Drowning
				{
					player.Out.SendMessage("You take 100% of your max hits in damage.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
				
					player.ChangeHealth(null, -player.MaxHealth, false);

					Interval = 0;
				}
				else
				{
					player.Out.SendMessage("You cannot breathe underwater and take damage!", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
					player.Out.SendMessage("You take 5% of your max hits in damage.", eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
				
					player.ChangeHealth(null, -(player.MaxHealth / 20), false);
					
					if(Interval != 3000)
					{
						player.Out.SendTimerWindow("Drowning", 60);
						Interval = 3000;
					}
				}
			}
		}

		#endregion

		#region ReceiveItem/DropItem/PickupObject
		/// <summary>
		/// Receive an item from another living
		/// </summary>
		/// <param name="source"></param>
		/// <param name="item"></param>
		/// <returns>true if player took the item</returns>
		public override bool ReceiveItem(GameLiving source, GenericItem item)
		{
			if (item == null)	return false;

			Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item);

			if(source == null)
			{
				Out.SendMessage("You receive the " + item.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage("You receive " + item.Name + " from " + source.GetName(0, false) + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			
			GamePlayer sourcePlayer = source as GamePlayer;
			if (sourcePlayer != null)
			{
				ePrivLevel privLevel1 = Client.Account.PrivLevel;
				ePrivLevel privLevel2 = sourcePlayer.Client.Account.PrivLevel;
				if(privLevel1 != privLevel2 && (privLevel1 > ePrivLevel.Player || privLevel2 > ePrivLevel.Player) && (privLevel1 == ePrivLevel.Player || privLevel2 == ePrivLevel.Player))
				{
					GameServer.Instance.LogGMAction("   Item: "+source.Name+"("+sourcePlayer.Client.Account.AccountName+") -> "+Name+"("+Client.Account.AccountName+") : "+item.Name+"("+item.ItemID+")");
				}
			}
			return true;
		}

		/// <summary>
		/// Drop a item from the inventory to the ground
		/// </summary>
		/// <param name="item">the item to drop</param>
		/// <returns>true on item dropped</returns>
		public bool DropItem(GenericItem item)
		{
			if (item == null) return false;
			
			if (item.SlotPosition < (ushort)eInventorySlot.RightHandWeapon || item.SlotPosition > (ushort)eInventorySlot.LastBackpack)
			{
				Out.SendMessage("You can not drop a item from this slot!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			if(!item.IsDropable)
			{
				Out.SendMessage("You can not drop this item!",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			if(!Inventory.RemoveItem(item)) return false;

			CreateItemOnTheGround(item);

			return true;
		}

		/// <summary>
		/// Called when the player picks up an item from the ground
		/// </summary>
		/// <param name="floorObject">GameItem on the floor</param>
		/// <returns>true if picked up</returns>
		public bool PickupObject(GameObject floorObject)
		{
			if (floorObject == null)
			{
				Out.SendMessage("You must have a target to get something!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (floorObject.ObjectState != eObjectState.Active)
				return false;

			if (floorObject is GameObjectTimed && !((GameObjectTimed)floorObject).IsOwner(this))
			{
				Out.SendMessage("That loot doesn't belong to you - you can't pick it up.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (!Position.CheckSquareDistance(floorObject.Position, (uint)(WorldMgr.PICKUP_DISTANCE*WorldMgr.PICKUP_DISTANCE)))
			{
				Out.SendMessage("The " + floorObject.Name + " is too far away to pick up!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (floorObject is GameInventoryItem)
			{
				GameInventoryItem floorItem = floorObject as GameInventoryItem;
				if (floorItem.Item == null)
				{
					Out.SendMessage("You can't get that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				lock(floorItem)
				{
					GamePlayer eligibleMember;

					PlayerGroup group = PlayerGroup;
					if (group != null && group.AutosplitLoot)
					{
						ArrayList eligibleMembers = new ArrayList(8);
						lock (group)
						{
							foreach (GamePlayer ply in group)
							{
								if (ply.Alive
								   && (ply.Region == Region)
								   && (Position.CheckSquareDistance(ply.Position, (uint)(WorldMgr.MAX_EXPFORKILL_DISTANCE*WorldMgr.MAX_EXPFORKILL_DISTANCE)))
								   && (ply.ObjectState == eObjectState.Active)
								   && (ply.AutoSplitLoot))
								{
									eligibleMembers.Add(ply);
								}
							}
						}
						if (eligibleMembers.Count <= 0)
						{
							Out.SendMessage("No one in group wants the " + floorItem.Name + "." , eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}

						int i = Util.Random(0, eligibleMembers.Count - 1);
						eligibleMember = eligibleMembers[i] as GamePlayer;
					}
					else
					{
						eligibleMember = this;
					}

					if (eligibleMember != null)
					{
						int slotNeeded = 1;

						IStackableItem stack = floorItem.Item as IStackableItem;
						if(stack != null)
						{
							int countToAdd = stack.Count;
							foreach (GenericItem item in eligibleMember.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
							{
								IStackableItem toItem = item as IStackableItem;
								if (toItem != null && toItem.CanStackWith(stack))
								{
									countToAdd -= (toItem.MaxCount - toItem.Count);
									if(countToAdd <= 0)
									{
										break;
									}
								}
							}
				
							if(countToAdd > 0)
							{
								slotNeeded = countToAdd / stack.MaxCount;
							}
							else
							{	
								slotNeeded = 0;
							}
						}

						if(!eligibleMember.Inventory.IsSlotsFree(slotNeeded, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
						{
							eligibleMember.Out.SendMessage("Your backpack is full. You must drop something first.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}
						
						eligibleMember.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

						if(group != null)
						{
							group.SendMessageToGroupMembers("(Autosplit) " + floorItem.Item.Name + " goes to " + eligibleMember.Name, eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							Out.SendMessage("You get " + floorItem.Item.Name + " and put it in your backpack.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						Message.SystemToOthers(this, Name + " picks up " + floorItem.Item.Name, eChatType.CT_System);	
					}
					floorItem.RemoveFromWorld();
				}
				return true;
			}
			else if (floorObject is GameMoney)
			{
				GameMoney moneyObject = floorObject as GameMoney;
				lock (moneyObject)
				{
					if (moneyObject.ObjectState!=eObjectState.Active)
						return false;

					PlayerGroup group = PlayerGroup;
					if (group != null && group.AutosplitCoins)
					{
						//Spread the money in the group
						ArrayList eligibleMembers = new ArrayList(8);
						lock (group)
						{
							foreach (GamePlayer ply in group)
							{
								if(ply.Alive
								   && (ply.Region == Region)
								   && (ply.ObjectState == eObjectState.Active))
								{
									eligibleMembers.Add(ply);
								}
							}
						}
						if (eligibleMembers.Count <= 0)
						{
							Out.SendMessage("No one in group wants the money.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}

						foreach (GamePlayer eligibleMember in eligibleMembers)
						{
							eligibleMember.AddMoney(moneyObject.TotalCopper / eligibleMembers.Count, "Your share of the loot is {0}.");
						}
					}
					else
					{
						//Add money only to picking player
						AddMoney(moneyObject.TotalCopper, "You pick up {0}.");
					}
					moneyObject.RemoveFromWorld();
					return true;
				}
			}
			else
			{
				Out.SendMessage("You can't get that!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
		}

		/// <summary>
		/// called to make an item on the ground with owner is player
		/// </summary>
		/// <param name="item">the item to create on the ground</param>
		/// <returns>the GameInventoryItem on the ground</returns>
		public GameInventoryItem CreateItemOnTheGround(GenericItem item)
		{
			GameInventoryItem gameItem = new GameInventoryItem(item);

			Point pos = GetSpotFromHeading(30);
			gameItem.Position = pos;
			gameItem.Heading = Heading;
			gameItem.Region = Region;

			gameItem.AddOwner(this);
			gameItem.AddToWorld();

			return gameItem;
		}

		#endregion

		#region CleanupOnDisconnect/InitOnConnect/Database

		/// <summary>
		/// Stop all timers, events and remove player from everywhere (group/guild/chat)
		/// </summary>
		public virtual void CleanupOnDisconnect()
		{
			StopAttack();
			// remove all stealth handlers
			Stealth(false);
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));

			if(PlayerGroup != null)
				PlayerGroup.RemovePlayer(this);

			lock (this)
			{
				if (TradeWindow != null)
					TradeWindow.CloseTrade();
			}

			if (Guild != null && !Guild.RemoveOnlineMember(this))
			{
				if (log.IsWarnEnabled)
					log.Warn("Do not succeed to remove " + Name + " in GamePlayer.CleanupOnDisconnect()");
			}

			ChatGroup mychatgroup = (ChatGroup)TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
			if (mychatgroup != null)
				mychatgroup.RemovePlayer(this);

			GroupMgr.RemovePlayerLooking(this);

			CommandNpcRelease();

			// cancel all effects until saving of running effects is done
			EffectList.CancelAll();

			string[] friendList = new string[]
				{
					Name
				};
			foreach (GameClient clientp in WorldMgr.GetAllPlayingClients())
			{
				if (clientp.Player.Friends.Contains(Name))
					clientp.Out.SendRemoveFriends(friendList);
			}
		}

		/// <summary>
		/// Load everything needed before add this player to the world
		/// </summary>
		public virtual void InitOnConnect()
		{
			m_steed = new WeakRef(null);
			m_rangeAttackAmmo = new WeakRef(null);
			m_rangeAttackTarget = new WeakRef(null);
			m_buff1Bonus = new PropertyIndexer((int)eProperty.MaxProperty); // set up a fixed indexer for players
			m_buff2Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_buff3Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_buff4Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_itemBonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_objectUpdates = new BitArray[2];
			m_objectUpdates[0] = new BitArray(30000);
			m_objectUpdates[1] = new BitArray(30000);
			m_lastUniqueLocations = new GameLocation[4];
			m_currentAreas = new ArrayList(1);
			m_lastNPCUpdate = (uint) Environment.TickCount;
			m_enteredGame = false;
			m_customDialogCallback = null;
			m_sitting = false;
			m_class = new DefaultCharacterClass();
			m_playerGroupIndex = -1;
			m_titles = new ArrayList();

			// Has to be updated on load to ensure time offline isn't
			// added to character /played.
			LastPlayed = DateTime.Now;

			if(m_guild != null) m_guild.AddOnlineMember(this);

			if (Region == null || Region.GetZone(m_position) == null) // check if the player is in a valid position
			{
				log.WarnFormat("Invalid region/zone on char load (" + ToString() + "); moving to bind point.");
				Position = BindPosition;
				Heading = (ushort) BindHeading;
				Region = BindRegion;

				if(Region == null || Region.GetZone(m_position) == null) // check if the player is in a valid position
				{
					log.WarnFormat("Invalid bind point region/zone on char load (" + ToString() + "); moving to realm starting location.");

					if(Realm == (byte)eRealm.Albion) //Albion SI start point
					{
						Position = new Point(526252, 542415, 3165);
						Heading = 5286;
						Region = WorldMgr.GetRegion(51);
					}
					else if (Realm == (byte)eRealm.Midgard) //Midgard SI start point
					{
						Position = new Point(293720, 356408, 3488);
						Heading = 6670;
						Region = WorldMgr.GetRegion(151);
					}
					else //Hibernia SI start point
					{
						Position = new Point(426483, 440626, 5952);
						Heading = 2403;
						Region = WorldMgr.GetRegion(181);
					}
				}
			}

			for (int i = 0; i < m_lastUniqueLocations.Length; i++)
			{
				m_lastUniqueLocations[i] = new GameLocation(null, (ushort)Region.RegionID, new Point(Position.X, Position.Y, Position.Z));
			}

			SetCharacterClass(CharacterClassID);

			// notify handlers that the item was just equipped
			foreach(eInventorySlot slot in GameLivingInventory.EQUIP_SLOTS )
			{
				// skip weapons. only active weapons should fire equip event, done in player.SwitchWeapon
				if (slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.DistanceWeapon) continue;
				EquipableItem item = Inventory.GetItem(slot) as EquipableItem;
				if (item != null) item.OnItemEquipped();
			}

			SwitchWeapon(ActiveWeaponSlot);

			if (Health == 0) // at the first load health == 0
			{
				Health = MaxHealth;
				Mana = MaxMana;
				EndurancePercent = 100;
				PlayedTime = 0;
			}

			if (m_realmLevel == 0)
			{
				m_realmLevel = CalculateRealmLevelFromRPs(m_realmPts);
			}

			LoadSkillsFromCharacter();
			LoadCraftingSkills();

			m_titles.AddRange(PlayerTitleMgr.GetPlayerTitles(this));
			IPlayerTitle t = PlayerTitleMgr.GetTitleByTypeName(CurrentTitleType);
			if (t == null)
				t = PlayerTitleMgr.ClearTitle;
			m_currentTitle = t;
		}

		/// <summary>
		/// This flag is used to not save player when deco/ld and autosave it at the same time
		/// </summary>
		protected bool m_isSaving = false;

		/// <summary>
		/// Save the player into the database
		/// </summary>
		public virtual void SaveIntoDatabase()
		{
			if(m_isSaving) return; // save in progress
			m_isSaving = true;
				
			try
			{
				SaveSkillsToCharacter();
				SaveCraftingSkills();
                m_playedTime = PlayedTime;  //We have to set the PlayedTime on the character before setting the LastPlayed
				LastPlayed = DateTime.Now;
				if(m_stuckFlag)
				{
					lock (m_lastUniqueLocations)
					{
						GameLocation loc = m_lastUniqueLocations[m_lastUniqueLocations.Length-1];
//						if (log.IsDebugEnabled)
//						{
//							log.Debug(string.Format("current pos={0} {1} {2}", m_character.Xpos, m_character.Ypos, m_character.Zpos));
//							log.Debug(string.Format("setting pos={0} {1} {2}", loc.X, loc.Y, loc.Z));
//						}
						Position = loc.Position;
						Region = loc.Region;
						Heading = loc.Heading;
					}
				}
				GameServer.Database.SaveObject(this);
				if (log.IsInfoEnabled)
					log.Info(Name + " saved!");
				Out.SendMessage("Your character has been saved.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error saving player "+Name+"!", e);
			}
			finally
			{
				m_isSaving = false;
			}
		}

		#endregion

		#region CustomDialog

		/// <summary>
		/// Holds the delegates that calls
		/// </summary>
		public CustomDialogResponse m_customDialogCallback;

		/// <summary>
		/// Gets/sets the custom dialog callback
		/// </summary>
		public CustomDialogResponse CustomDialogCallback
		{
			get { return m_customDialogCallback; }
			set { m_customDialogCallback = value; }
		}

		#endregion

		#region GetPronoun/GetExamineMessages

		/// <summary>
		/// Pronoun of this player in case you need to refer it in 3rd person
		/// http://webster.commnet.edu/grammar/cases.htm
		/// </summary>
		/// <param name="firstLetterUppercase"></param>
		/// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
		/// <returns>pronoun of this object</returns>
		public override string GetPronoun(int form, bool firstLetterUppercase)
		{
			if (Gender == 0) // male
				switch (form)
				{
					default:
						// Subjective
						if (firstLetterUppercase)
							return "He";
						else
							return "he";
					case 1:
						// Possessive
						if (firstLetterUppercase)
							return "His";
						else
							return "his";
					case 2:
						// Objective
						if (firstLetterUppercase)
							return "Him";
						else
							return "him";
				}
			else
				// female
				switch (form)
				{
					default:
						// Subjective
						if (firstLetterUppercase)
							return "She";
						else
							return "she";
					case 1:
						// Possessive
						if (firstLetterUppercase)
							return "Her";
						else
							return "her";
					case 2:
						// Objective
						if (firstLetterUppercase)
							return "Her";
						else
							return "her";
				}
		}

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			// TODO: PvP & PvE messages
			IList list = base.GetExamineMessages(player);

			string message = "";
			switch (GameServer.Instance.Configuration.ServerType)
			{
				case eGameServerType.GST_Normal:
					if (Realm == player.Realm)
					{
						message = string.Format("You examine {0}.  {1} is a member of the {2} class in your realm.", Name, GetPronoun(0, true), CharacterClass.Name);
					}
					else
					{
						message = string.Format("You examine {0}.  {1} is a member of an enemy realm!", Name, GetPronoun(0, true));
					}
					break;

				case eGameServerType.GST_PvP:
					if (Guild == null)
					{
						message = string.Format("You examine {0}.  {1} is a neutral member with no guild allegiance.", Name, GetPronoun(0, true));
					}
					else if (Guild == player.Guild)
					{
						message = string.Format("You examine {0}.  {1} is a member of the {2} class in your guild.", Name, GetPronoun(0, true), CharacterClass.Name);
					}
					else
					{
						message = string.Format("You examine {0}.  {1} is a member of the {2} guild.", Name, GetPronoun(0, true), Guild.GuildName);
					}
					break;

				default:
					message = "You examine " + Name + ".";
					break;
			}

			list.Add(message);
			return list;
		}

		#endregion

		#region Stealth

		/// <summary>
		/// Property that holds tick when stealth state was changed last time
		/// </summary>
		public const string STEALTH_CHANGE_TICK = "StealthChangeTick";
		/// <summary>
		/// Holds the stealth effect
		/// </summary>
		protected StealthEffect m_stealthEffect = null;
		/// <summary>
		/// The stealth state of this player
		/// </summary>
		public override bool IsStealthed
		{
			get { return m_stealthEffect != null; }
		}
		public static void Unstealth(DOLEvent ev, object sender, EventArgs args)
		{
			AttackedByEnemyEventArgs atkArgs = args as AttackedByEnemyEventArgs;
			GamePlayer player = sender as GamePlayer;
			if(player == null || atkArgs == null) return;
			if(atkArgs.AttackData.AttackResult != eAttackResult.HitUnstyled && atkArgs.AttackData.AttackResult != eAttackResult.HitStyle) return;

			player.Stealth(false);
		}
		/// <summary>
		/// Set player's stealth state
		/// </summary>
		/// <param name="newState">stealth state</param>
		public virtual void Stealth(bool newState)
		{
			if(IsStealthed == newState)
				return;

			UncoverStealthAction action = (UncoverStealthAction)TempProperties.getObjectProperty(UNCOVER_STEALTH_ACTION_PROP, null);
			if(newState)
			{
				//start the uncover timer
				if (action == null)
					action = new UncoverStealthAction(this);
				action.Interval = 2000;
				action.Start(2000);
				TempProperties.setProperty(UNCOVER_STEALTH_ACTION_PROP, action);

				if(ObjectState == eObjectState.Active)
					Out.SendMessage("You are now hidden!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendPlayerModelTypeChange(this, 3);
				m_stealthEffect = new StealthEffect();
				m_stealthEffect.Start(this);
				Sprint(false);
				GameEventMgr.AddHandler(this,GameLivingBaseEvent.AttackedByEnemy,new DOLEventHandler(Unstealth));
				foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					if(player == this) continue;
					if(!player.CanDetect(this))
						player.Out.SendObjectDelete(this);
				}
			}
			else
			{
				//stop the uncover timer
				if (action != null)
				{
					action.Stop();
					TempProperties.removeProperty(UNCOVER_STEALTH_ACTION_PROP);
				}

				if(ObjectState == eObjectState.Active)
					Out.SendMessage("You are no longer hidden!", eChatType.CT_System, eChatLoc.CL_SystemWindow);

				Out.SendPlayerModelTypeChange(this, 2);
				m_stealthEffect.Stop();
				m_stealthEffect = null;
				GameEventMgr.RemoveHandler(this,GameLivingBaseEvent.AttackedByEnemy,new DOLEventHandler(Unstealth));
				foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				{
					//TODO: more correct way to do it
					if(player == this) continue;
					player.Out.SendPlayerCreate(this);
					player.Out.SendLivingEquipementUpdate(this);
				}
			}
			Notify(GamePlayerEvent.StealthStateChanged, this, null);
			Out.SendUpdateMaxSpeed();
		}

		/// <summary>
		/// The temp property that stores the uncover stealth action
		/// </summary>
		protected const string UNCOVER_STEALTH_ACTION_PROP = "UncoverStealthAction";

		/// <summary>
		/// Uncovers the player if a mob is too close
		/// </summary>
		protected class UncoverStealthAction : RegionAction
		{
			/// <summary>
			/// Constructs a new uncover stealth action
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public UncoverStealthAction(GamePlayer actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;

				foreach(GameNPC npc in player.GetInRadius(typeof(GameNPC), 2048))
				{
					// Friendly mobs do not uncover stealthed players
					if(!GameServer.ServerRules.IsAllowedToAttack(npc, player, true)) continue;

					double npcLevel = Math.Max(npc.Level, 1.0);
					double stealthLevel = player.GetModifiedSpecLevel(Specs.Stealth);
					//  if(npc.hasDetectHidden)
					//    detectRadius = 2048f - (1792.0 * stealthLevel / npcLevel);
					//  else
					double detectRadius = 1024f - (896.0 * stealthLevel / npcLevel);

					//Don't check for radius <= 0
					if(detectRadius <= 0) continue;

					double distanceToPlayer = npc.Position.GetDistance(player.Position);
					//If player is out of detection distance, continue
					if(distanceToPlayer > detectRadius) continue;

					double fieldOfView   = 90.0;  //90 degrees  = standard FOV
					double fieldOfListen = 120.0; //120 degrees = standard field of listening

					//NPC's with Level > 50 get some bonuses!
					if(npcLevel > 50)
					{
						fieldOfView = 4050.0 / 2048.0 * npc.Level; //=oldFOV*npc.Level*45/2048
						fieldOfListen = 5400.0 / 2048.0 * npc.Level; //=oldFOL*npc.Level*45/2048
					}

					double angle = npc.GetAngleToSpot(player.Position);
					//player in front
					fieldOfView *= 0.5;
					bool canSeePlayer = (angle >= 360-fieldOfView || angle < fieldOfView);

//					DOLConsole.WriteLine(npc.Name + ": angle="+angle+"; distance="+distanceToPlayer+"; radius="+detectRadius+"; canSee="+canSeePlayer);

					//If npc can not see nor hear the player, continue the loop
					fieldOfListen /= 2.0;
					if(canSeePlayer == false &&
					   !(angle >= (45+60)-fieldOfListen && angle < (45+60)+fieldOfListen) &&
					   !(angle >= (360-45-60)-fieldOfListen && angle < (360-45-60)+fieldOfListen))
						continue;

					double chanceMod = 1.0;
					//Chance to detect player decreases after 128 coordinates!
					if(distanceToPlayer > 128)
						chanceMod = 1f - (distanceToPlayer-128.0)/(detectRadius-128.0);

					double chanceToUncover = (npc.Level * 10.0 + 100.0) / (stealthLevel + 100.0) * chanceMod;

					//Mobs above 50 have a higher chance to uncover/turn towards players
					if(npcLevel > 50)
						chanceToUncover *= (npc.Level - 40.0) / 10.0;
					else
						chanceToUncover /= 10.0;

//					DOLConsole.WriteLine(npc.Name + ": chance="+chanceToUncover.ToString("R"));
					if(Util.ChanceDouble(chanceToUncover))
					{
						if(canSeePlayer)
						{
							player.Out.SendMessage(npc.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							player.Stealth(false);
							break; //No more detecting needed, since uncovered already!
						}
						else
						{
							//On live server, npc turns to player only for 5 seconds (?)
							npc.TurnTo(player, 10000);
						}
					}
				}
			}
		}

		/// <summary>
		/// Checks whether this player can detect stealthed enemy
		/// </summary>
		/// <param name="enemy"></param>
		/// <returns>true if enemy can be detected</returns>
		public virtual bool CanDetect(GamePlayer enemy)
		{
			if(enemy.Region != Region)
				return false;
			if(!Alive)
				return false;

			/*
			 * http://www.critshot.com/forums/showthread.php?threadid=3142
			 * The person doing the looking has a chance to find them based on their level, minus the stealthed person's stealth spec.
			 *
			 * -Normal detection range = (enemy lvl – your stealth spec) * 20 + 125
			 * -Detect Hidden Range = (enemy lvl – your stealth spec) * 50 + 250
			 * -See Hidden range = 2700 - (38 * your stealth spec)
			 */

			int levelDiff = Level - enemy.GetModifiedSpecLevel(Specs.Stealth);
			if (levelDiff < 0) levelDiff = 0;

			int range;
			if(HasAbility(Abilities.DetectHidden) && !enemy.HasAbility(Abilities.DetectHidden))
			{
				// we have detect hidden and enemy don't = higher range
				range = levelDiff * 50 + 250;
			}
			else
			{
				// normal detect range
				range = levelDiff * 20 + 125;
			}
			return Position.CheckDistance(enemy.Position, range);
		}

		#endregion

		#region Task

		/// <summary>
		/// Holding tasks of player
		/// </summary>
		protected AbstractTask m_task = null;

		/// <summary>
		/// Gets the tasklist of this player
		/// </summary>
		public AbstractTask Task
		{
			get { return m_task; }
			set { m_task = value;}
		}

        /// <summary>
        /// Holding the number of task already sone this level
        /// </summary>
        protected byte m_taskDone;

        /// <summary>
        /// Gets the number of task already sone this level
        /// </summary>
        public byte TaskDone
        {
            get { return m_taskDone; }
            set { m_taskDone = value; }
        }
		#endregion

		#region Quest

		/// <summary>
		/// Holds all the quests currently active on this player
		/// </summary>
		protected Iesi.Collections.ISet m_activeQuests;

		/// <summary>
		/// Gets or sets the list of all active quests of this player
		/// </summary>
		public Iesi.Collections.ISet ActiveQuests
		{
			get
			{
				if(m_activeQuests == null) m_activeQuests = new Iesi.Collections.HybridSet();
				return m_activeQuests;
			}
			set	{ m_activeQuests = value; }
		}

		/// <summary>
		/// Holds all already finished quests off this player
		/// </summary>
		protected Iesi.Collections.ISet m_finishedQuests;

		/// <summary>
		/// Gets or sets the list of all finished quests of this player
		/// </summary>
		public Iesi.Collections.ISet FinishedQuests
		{
			get
			{
				if(m_finishedQuests == null) m_finishedQuests = new Iesi.Collections.HybridSet();
				return m_finishedQuests;
			}
			set	{ m_finishedQuests = value; }
		}

		/// <summary>
		/// Adds a quest to the players questlist
		/// </summary>
		/// <param name="quest">The quest to add</param>
		/// <returns>true if added, false if player is already doing the quest!</returns>
		public bool AddQuest(AbstractQuest quest)
		{
			lock(m_activeQuests)
			{
				if(IsDoingQuest(quest.GetType())!=null)
					return false;

				m_activeQuests.Add(quest);
			}
			Out.SendQuestUpdate(quest);
			return true;
		}

		/// <summary>
		/// Checks if a player has done a specific quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the number of times the player did this quest</returns>
		public int HasFinishedQuest(Type questType)
		{
			lock(m_finishedQuests)
			{
				foreach(FinishedQuest q in m_finishedQuests)
				{
					//DOLConsole.WriteLine("HasFinished: "+q.GetType().FullName+" Step="+q.Step);
					if(q.FinishedQuestType.Equals(questType))
						return q.Count;
				}
			}
			return 0;
		}

		/// <summary>
		/// Checks if this player is currently doing the specified quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the quest if player is doing the quest or null if not</returns>
		public AbstractQuest IsDoingQuest(Type questType)
		{
			lock(m_activeQuests)
			{
				foreach(AbstractQuest q in m_activeQuests)
				{
					if(q.GetType().Equals(questType))
						return q;
				}
			}
			return null;
		}

		/// <summary>
		/// Checks if this player is currently doing the specified quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the quest if player is doing the quest or null if not</returns>
		public void AbortQuest(Type questType, byte response)
		{
			lock(m_activeQuests)
			{
				foreach(AbstractQuest q in m_activeQuests)
				{
					if(q.GetType().Equals(questType))
					{
						q.CheckPlayerAbortQuest(this, response);
						return;
					}
				}
			}
		}

		#endregion

		#region FactionAggroLevel

		/// <summary>
		/// The list of all FactionID => AggroLevel
		/// </summary>
		protected IDictionary m_factionAggroLevel;

		/// <summary>
		/// Gets or sets the list of all FactionID => AggroLevel
		/// </summary>
		public IDictionary FactionAggroLevel
		{
			get
			{
				if(m_factionAggroLevel == null) m_factionAggroLevel = new Hashtable();
				return m_factionAggroLevel;
			}
			set	{ m_factionAggroLevel = value; }
		}

		/// <summary>
		/// Gets the aggro level of the faction again the player (-150 .. 100)
		/// </summary>
		public int GetFactionAggroLevel(Faction faction)
		{
			if(!m_factionAggroLevel.Contains(faction.FactionID)) return 0;
			return (int)m_factionAggroLevel[faction.FactionID] / 10;
		}

		/// <summary>
		/// Increase the aggrolevel of the faction again this player
		/// </summary>
		public void IncreaseAggroLevel(Faction fac)
		{
			if(fac == null) return;

			if (!m_factionAggroLevel.Contains(fac.FactionID)) m_factionAggroLevel.Add(fac.FactionID, 0);

			if((int)m_factionAggroLevel[fac.FactionID] < FactionMgr.MAX_FACTION_AGGROLEVEL_VALUE)
			{
				m_factionAggroLevel[fac.FactionID] = (int)m_factionAggroLevel[fac.FactionID] + 1;
			}
			
			Out.SendMessage("Your relationship with "+fac.Name+" has decreased.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		/// <summary>
		/// Decrease the aggrolevel of the faction again this player
		/// </summary>
		public void DecreaseAggroLevel(Faction fac)
		{
			if(fac == null) return;

			if (!m_factionAggroLevel.Contains(fac.FactionID)) m_factionAggroLevel.Add(fac.FactionID, 0);

			if((int)m_factionAggroLevel[fac.FactionID] > FactionMgr.MIN_FACTION_AGGROLEVEL_VALUE)
			{
				m_factionAggroLevel[fac.FactionID] = (int)m_factionAggroLevel[fac.FactionID] - 1;
			}
			
			Out.SendMessage("Your relationship with "+fac.Name+" has increased.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
		}

		#endregion

		#region Notify
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e,sender,args);

			// events will only fire for currently active quests
			foreach(AbstractQuest q in (Iesi.Collections.ISet)ActiveQuests.Clone())
				q.Notify(e, sender, args);

			if (Task!=null)
				Task.Notify(e,sender,args);

		}

		public override void Notify(DOLEvent e, object sender)
		{
			Notify(e,sender,null);
		}

		public override void Notify(DOLEvent e)
		{
			Notify(e,null,null);
		}

		public override void Notify(DOLEvent e, EventArgs args)
		{
			Notify(e,null,args);
		}
		#endregion

		#region Housing
		/*private House m_currentHouse;
		public House CurrentHouse
		{
			get { return m_currentHouse;  }
			set { m_currentHouse = value; }
		}
		private bool m_inHouse;
		public bool InHouse
		{
			get { return m_inHouse; }
			set { m_inHouse = value; }
		}

		/// <summary>
		/// Holds the houses that need a update
		/// </summary>
		private BitArray m_housingUpdateArray;

		/// <summary>
		/// Returns the Housing Update Array
		/// </summary>
		public BitArray HousingUpdateArray
		{
			get { return m_housingUpdateArray; }
			set { m_housingUpdateArray = value; }
		}

		/// <summary>
		/// Jumps the player out of the house he is in
		/// </summary>
		public void LeaveHouse()
		{
			if (CurrentHouse == null || (!InHouse))
			{
				InHouse = false;
				return;
			}

			House house = CurrentHouse;
			InHouse = false;
			CurrentHouse = null;

			house.Exit(this,false);
		}*/

		#endregion

		#region Trade

		/// <summary>
		/// Holds the trade window object
		/// </summary>
		protected ITradeWindow m_tradeWindow;

		/// <summary>
		/// Gets or sets the player trade windows
		/// </summary>
		public ITradeWindow TradeWindow
		{
			get { return m_tradeWindow; }
			set { m_tradeWindow=value; }
		}

		/// <summary>
		/// Opens the trade between two players
		/// </summary>
		/// <param name="tradePartner">GamePlayer to trade with</param>
		/// <returns>true if trade has started</returns>
		public bool OpenTrade(GamePlayer tradePartner)
		{
			lock(this)
			{
				lock(tradePartner)
				{
					if (tradePartner.TradeWindow != null)
						return false;

					object sync = new object();
					PlayerTradeWindow initiantWindow = new PlayerTradeWindow(tradePartner, false, sync);
					PlayerTradeWindow recipientWindow = new PlayerTradeWindow(this, true, sync);

					tradePartner.TradeWindow = initiantWindow;
					TradeWindow = recipientWindow;

					initiantWindow.PartnerWindow = recipientWindow;
					recipientWindow.PartnerWindow = initiantWindow;

					return true;
				}
			}
		}

		/// <summary>
		/// Opens the trade between two players
		/// </summary>
		/// <param name="item">The item to spell craft</param>
		/// <returns>true if trade has started</returns>
		public bool OpenSelfCraft(GenericItem item)
		{
			if(item == null) return false;

			lock(this)
			{
				if (TradeWindow != null)
				{
					GamePlayer sourceTradePartner = TradeWindow.Partner;
					if(sourceTradePartner == null)
					{
						Out.SendMessage("You are already selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return false;
				}

				if(item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
				{
					Out.SendMessage("You can only self-craft items in your backpack!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				TradeWindow = new SelfCraftWindow(this, item);
				TradeWindow.TradeUpdate();

				return true;
			}
		}

		#endregion

		#region ControlledNpc

		/// <summary>
		/// Holds the controlled object
		/// </summary>
		private IControlledBrain m_controlledNpc;

		/// <summary>
		/// Gets the controlled object of this player
		/// </summary>
		public virtual IControlledBrain ControlledNpc
		{
			get { return m_controlledNpc; }
		}

		/// <summary>
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public virtual void SetControlledNpc(IControlledBrain controlledNpc)
		{
			if (controlledNpc == ControlledNpc) return;
			if (controlledNpc == null)
			{
				Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
				Out.SendMessage("You release control of your controlled target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (controlledNpc.Owner != this)
					throw new ArgumentException("ControlledNpc with wrong owner is set (player="+Name+", owner="+controlledNpc.Owner.Name+")", "controlledNpc");
				Out.SendPetWindow(controlledNpc.Body, ePetWindowAction.Open, controlledNpc.AggressionState, controlledNpc.WalkState);
			}
			m_controlledNpc = controlledNpc;
		}

		/// <summary>
		/// Checks if player controls a brain and send any needed messages
		/// </summary>
		/// <param name="npc">The Npc from local var to avoid changes but other threads</param>
		/// <returns>success</returns>
		private bool CheckControlledNpc(IControlledBrain npc)
		{
			return npc != null;
		}

		/// <summary>
		/// Commands controlled object to attack
		/// </summary>
		public virtual void CommandNpcAttack()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			GameLiving target = TargetObject as GameLiving;
			if (!GameServer.ServerRules.IsAllowedToAttack(this, target, false))
				return;
			if(Position.CheckSquareDistance(target.Position, 1500*1500))
			{
				Out.SendMessage("Your target is too far away to attack by your pet!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to kill your target!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Attack(TargetObject);
		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		public virtual void CommandNpcRelease()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Notify(GamePlayerEvent.CommandNpcRelease, this);
		}

		/// <summary>
		/// Commands controlled object to follow
		/// </summary>
		public virtual void CommandNpcFollow()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to follow you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Follow(this);
		}

		/// <summary>
		/// Commands controlled object to stay where it is
		/// </summary>
		public virtual void CommandNpcStay()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to stay in this position!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Stay();
		}

		/// <summary>
		/// Commands controlled object to go to players location
		/// </summary>
		public virtual void CommandNpcComeHere()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to come to you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.ComeHere();
		}

		/// <summary>
		/// Commands controlled object to go to target
		/// </summary>
		public virtual void CommandNpcGoTarget()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			GameObject target = TargetObject;
			if (target == null)
			{
				Out.SendMessage("You must select a destination for your controlled creature.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to go to your target.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Goto(target);
		}

		/// <summary>
		/// Changes controlled object state to passive
		/// </summary>
		public virtual void CommandNpcPassive()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to disengage from combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.AggressionState = eAggressionState.Passive;
			npc.Body.StopAttack();
			npc.Body.StopCurrentSpellcast();
		}

		/// <summary>
		/// Changes controlled object state to aggressive
		/// </summary>
		public virtual void CommandNpcAgressive()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to attack all enemies!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.AggressionState = eAggressionState.Aggressive;
		}

		/// <summary>
		/// Changes controlled object state to defensive
		/// </summary>
		public virtual void CommandNpcDefensive()
		{
			IControlledBrain npc = ControlledNpc;
			if (!CheckControlledNpc(npc))
				return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to defend you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.AggressionState = eAggressionState.Defensive;
		}

		/// <summary>
		/// The shade effect of this player
		/// </summary>
		protected ShadeEffect m_ShadeEffect = null;

		/// <summary>
		/// Gets flag indication whether player is in shade mode
		/// </summary>
		public bool IsShade
		{
			get { return m_ShadeEffect != null; }
		}

		/// <summary>
		/// Changes shade state of the player
		/// </summary>
		/// <param name="state">The new state</param>
		public virtual void Shade(bool state)
		{
			if(IsShade==state)
			{
				if(state && (ObjectState == eObjectState.Active))
					Out.SendMessage("You are already a shade!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return;
			}
			if(state)
			{
				if(ObjectState == eObjectState.Active)
				{
					Model=822;// Shade
//					Out.SendMessage("You are already a shade!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					m_ShadeEffect = new ShadeEffect();
					m_ShadeEffect.Start(this);
					ArrayList temp = (ArrayList) m_attackers.Clone();
					GameNPC pet=null;
					if(ControlledNpc != null && ControlledNpc.Body != null)
						pet = ControlledNpc.Body;
					if(pet !=null)
					{
						foreach(GameLiving obj in temp)
							if(obj is GameNPC)
							{
								GameNPC npc=(GameNPC)obj;
								if (npc.TargetObject==this && npc.AttackState)
								{
									Out.SendDebugMessage("Reaggro "+npc.Name+" on "+pet.Name);
									IAggressiveBrain brain = npc.Brain as IAggressiveBrain;
									if(brain != null)
									{
										(npc).AddAttacker(pet);
										npc.StopAttack();
										brain.AddToAggroList(pet,(int)(brain.GetAggroAmountForLiving(this)+1));
									}
								}
							}
					}
				}
			}
			else
			{
				bool looseHP=false;

				m_ShadeEffect.Stop();
				m_ShadeEffect = null;
				Model = (ushort)CreationModel;
				if(ObjectState == eObjectState.Active)
					Out.SendMessage("You are no longer a shade!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

				if(ControlledNpc != null)
					CommandNpcRelease();

				foreach (GameLiving living in GetInRadius(typeof(GameLiving), WorldMgr.VISIBILITY_DISTANCE))
				{
					if (GameServer.ServerRules.IsAllowedToAttack(living, this, true))
					{
						if(living is GameNPC)
						{
							IAggressiveBrain aggroBrain = ((GameNPC)living).Brain as IAggressiveBrain;
							if (aggroBrain != null && Position.CheckDistance(living.Position, aggroBrain.AggroRange * 3))
							{
								looseHP = true;
								break;
							}
						}
						else
						{
							looseHP=true;
							break;
						}
					}
				}

				if(looseHP)
				{
					ChangeHealth(this, -9*Health/10, true); // only stay 1/10 of life
				}
			}
		}

		#endregion

		#region Siege Weapon
	/*	private GameSiegeWeapon m_siegeWeapon;

		public GameSiegeWeapon SiegeWeapon
		{
			get { return m_siegeWeapon; }
			set { m_siegeWeapon = value; }
		}
		public void RepairSiegeWeapon(GameSiegeWeapon siegeWeapon)
		{
			Repair.BeginWork(this, siegeWeapon);
		}
		public void SalvageSiegeWeapon(GameSiegeWeapon siegeWeapon)
		{
			//TODO
			//Salvage.BeginWork(this,siegeWeapon);
		}*/
		#endregion

		#region PvP Invulnerability

		/// <summary>
		/// The delegate for invulnerability expire callbacks
		/// </summary>
		public delegate void InvulnerabilityExpiredCallback(GamePlayer player);
		/// <summary>
		/// Holds the invulnerability timer
		/// </summary>
		protected InvulnerabilityTimer m_pvpInvulnerabilityTimer;
		/// <summary>
		/// Holds the invulnerability expiration tick
		/// </summary>
		protected long m_pvpInvulnerabilityTick;

		/// <summary>
		/// Sets the PvP invulnerability
		/// </summary>
		/// <param name="duration">The invulnerability duration in milliseconds</param>
		/// <param name="callback">
		/// The callback for when invulnerability expires;
		/// not guaranteed to be called if overwriten by another invulnerability
		/// </param>
		/// <returns>true if invulnerability was set (smaller than old invulnerability)</returns>
		public virtual bool SetPvPInvulnerability(int duration, InvulnerabilityExpiredCallback callback)
		{
			if (duration < 1)
				throw new ArgumentOutOfRangeException("duration", duration, "Immunity duration cannot be less than 1ms");

			long newTick = Region.Time + duration;
			if (newTick < m_pvpInvulnerabilityTick)
				return false;

			m_pvpInvulnerabilityTick = newTick;
			if (m_pvpInvulnerabilityTimer != null)
				m_pvpInvulnerabilityTimer.Stop();

			if (callback != null)
			{
				m_pvpInvulnerabilityTimer = new InvulnerabilityTimer(this, callback);
				m_pvpInvulnerabilityTimer.Start(duration);
			}
			else
			{
				m_pvpInvulnerabilityTimer = null;
			}

			return true;
		}

		/// <summary>
		/// True if player is invulnerable to PvP attacks
		/// </summary>
		public virtual bool IsPvPInvulnerability
		{
			get { return m_pvpInvulnerabilityTick > Region.Time; }
		}

		/// <summary>
		/// The timer to call invulnerability expired callbacks
		/// </summary>
		protected class InvulnerabilityTimer : RegionAction
		{
			/// <summary>
			/// Defines a logger for this class.
			/// </summary>
			private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

			/// <summary>
			/// Holds the callback
			/// </summary>
			private readonly InvulnerabilityExpiredCallback m_callback;

			/// <summary>
			/// Constructs a new InvulnerabilityTimer
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public InvulnerabilityTimer(GamePlayer actionSource, InvulnerabilityExpiredCallback callback) : base(actionSource)
			{
				if (callback == null)
					throw new ArgumentNullException("callback");
				m_callback = callback;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				try
				{
					m_callback((GamePlayer)m_actionSource);
				}
				catch (Exception e)
				{
					log.Error("InvulnerabilityTimer callback", e);
				}
			}
		}

		#endregion
		
		/// <summary>
		/// Returns the string representation of the GamePlayer
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" class=").Append(CharacterClass.Name)
				.Append('(').Append(CharacterClass.ID.ToString()).Append(')')
				.ToString();
		}
	}
}
