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
using DOL.AI.Brain;
using DOL.GS.Effects;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.Language;
using DOL.GS.Realm;
using DOL.Database;

namespace DOL.GS
{
	

	/// <summary>
	/// The Base class for all Character Classes in DOL
	/// </summary>
	public class CharacterClassBase : ICharacterClass
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// id of class in Client
		/// </summary>
		protected int m_id;

		/// <summary>
		/// Name of class
		/// </summary>
		protected string m_name;

		/// <summary>
		/// Female name of class
		/// </summary>
		protected string m_femaleName;

		/// <summary>
		/// Base of this class
		/// </summary>
		protected string m_basename;

		/// <summary>
		/// Profession of character, e.g. Defenders of Albion
		/// </summary>
		protected string m_profession;

		/// <summary>
		/// multiplier for specialization points per level in 10th
		/// </summary>
		protected int m_specializationMultiplier;

		/// <summary>
		/// BaseHP for hp calculation
		/// </summary>
		protected int m_baseHP;

		/// <summary>
		/// Stat gained every level.
		///	see eStat consts
		/// </summary>
		protected eStat m_primaryStat;

		/// <summary>
		/// Stat gained every second level.
		/// see eStat consts
		/// </summary>
		protected eStat m_secondaryStat;

		/// <summary>
		/// Stat gained every third level.
		/// see eStat consts
		/// </summary>
		protected eStat m_tertiaryStat;

		/// <summary>
		/// Stat that affects the power/mana pool.
		/// Do not set if they do not have a power pool/spells
		/// </summary>
		protected eStat m_manaStat;

		/// <summary>
		/// Weapon Skill Base value to influence weapon skill calc
		/// </summary>
		protected int m_baseWeaponSkill;

		/// <summary>
		/// Weapon Skill Base value to influence ranged weapon skill calc
		/// </summary>
		protected int m_baseWeaponSkillRanged;

		/// <summary>
		/// How many chants can be run at once
		/// </summary>
		protected ushort m_maxPulsingSpells;

		/// <summary>
		/// The GamePlayer for this character
		/// </summary>
		public GamePlayer Player { get; private set; }

		private static readonly List<string> DefaultAutoTrainableSkills = new() { };

		protected List<string> m_autotrainableSkills;

		protected List<PlayerRace> m_eligibleRaces;

		protected struct ClassOverride
		{
			public byte ClassID;
			public int SpecializationMultiplier;
			public int BaseHP;
			public int BaseWeaponSkill;
			public List<string> AutotrainableSkills;
			public List<PlayerRace> EligibleRaces;
		}

		protected static Dictionary<byte, ClassOverride> m_classOverride = new Dictionary<byte, ClassOverride>(0);

		public CharacterClassBase()
		{
			m_id = 0;
			m_name = "Unknown Class";
			m_basename = "Unknown Base Class";
			m_profession = "";
			m_specializationMultiplier = 10;
			m_baseHP = 600;
			m_primaryStat = eStat.UNDEFINED;
			m_secondaryStat = eStat.UNDEFINED;
			m_tertiaryStat = eStat.UNDEFINED;
			m_manaStat = eStat.UNDEFINED;
			m_baseWeaponSkill = 400;
			m_baseWeaponSkillRanged = 440;
			m_autotrainableSkills = DefaultAutoTrainableSkills;
			m_eligibleRaces = PlayerRace.AllRaces;
			m_maxPulsingSpells = 1;
			
			// initialize members from attributes
			Attribute[] attrs = Attribute.GetCustomAttributes(this.GetType(), typeof(CharacterClassAttribute));
			foreach (Attribute attr in attrs)
			{
				if (attr is CharacterClassAttribute)
				{
					m_id = ((CharacterClassAttribute)attr).ID;
					m_name = ((CharacterClassAttribute)attr).Name;
					m_basename = ((CharacterClassAttribute)attr).BaseName;
					if (Util.IsEmpty(((CharacterClassAttribute)attr).FemaleName) == false)
						m_femaleName = ((CharacterClassAttribute)attr).FemaleName;
					break;
				}
			}
		}

		/// <summary>
		/// Load class override data from the DB
		/// </summary>
		static public void LoadClassOverrideDictionary()
		{
			// Make sure we aren't using a FakeDatabase used in testing
			if (GameServer.Instance == null) return;
			if (GameServer.Database is not ObjectDatabase) return;

			var dbClassOverrideTable = DOLDB<DBCharacterClass>.SelectAllObjects();

            foreach (var dbClassOverride in dbClassOverrideTable)
            {
                if (dbClassOverride.ClassID < 0 || dbClassOverride.ClassID > byte.MaxValue
                    || !Enum.IsDefined(typeof(eCharacterClass), (byte)dbClassOverride.ClassID))
                {
                    log.Error($"LoadClassOverrideDictionary(): ClassID {dbClassOverride.ClassID} in table CharacterClass is out of range ({eCharacterClass.Unknown}-{eCharacterClass.MaulerHib}");
                    continue;
                }

                ClassOverride classOverride = new()
                {
                    ClassID = (byte)dbClassOverride.ClassID,
                    SpecializationMultiplier = dbClassOverride.SpecializationMultiplier,
                    BaseHP = dbClassOverride.BaseHP,
                    BaseWeaponSkill = dbClassOverride.BaseWeaponSkill
                };

                classOverride.AutotrainableSkills = new(0);
                if (!String.IsNullOrEmpty(dbClassOverride.AutoTrainSkills))
                    foreach (string s in dbClassOverride.AutoTrainSkills.Split(';', ','))
                        if (!String.IsNullOrEmpty(s))
                            classOverride.AutotrainableSkills.Add(s);

                classOverride.EligibleRaces = new(0);
                if (!String.IsNullOrEmpty(dbClassOverride.EligibleRaces))
                    foreach (string s in dbClassOverride.EligibleRaces.Split(';', ','))
                        if (!String.IsNullOrEmpty(s) && Enum.TryParse<eRace>(s, out eRace race) && PlayerRace.TryGetRace(race, out PlayerRace pRace))
                            classOverride.EligibleRaces.Add(pRace);

                m_classOverride.Add(classOverride.ClassID, classOverride);
            }
		}

		/// <summary>
		/// Replace class data with values in the DB
		/// </summary>
		public void LoadClassOverride(eCharacterClass charClass)
		{
			if (m_classOverride.TryGetValue((byte)charClass, out ClassOverride cOverride))
			{
				if (cOverride.SpecializationMultiplier > 0)
					m_specializationMultiplier = cOverride.SpecializationMultiplier;

				if (cOverride.BaseHP > 0)
					m_baseHP = cOverride.BaseHP;

				if (cOverride.BaseWeaponSkill > 0)
					m_baseWeaponSkill = cOverride.BaseWeaponSkill;

				if (cOverride.AutotrainableSkills.Count > 0)
					m_autotrainableSkills = cOverride.AutotrainableSkills;

				if (cOverride.EligibleRaces.Count > 0)
					m_eligibleRaces = cOverride.EligibleRaces;
			}
		}

		public virtual void Init(GamePlayer player)
		{
			// TODO : Should Throw Exception Here.
			if (Player != null && log.IsWarnEnabled)
				log.WarnFormat("Character Class initializing Player when it was already initialized ! Old Player : {0} New Player : {1}", Player, player);
			
			Player = player;
		}

		public List<PlayerRace> EligibleRaces => m_eligibleRaces;

		public string FemaleName
		{
			get { return m_femaleName; }
		}

		public int BaseHP
		{
			get { return m_baseHP; }
		}

		public int ID
		{
			get { return m_id; }
		}

		public string Name
		{
			get { return (Player != null && Player.Gender == eGender.Female && !Util.IsEmpty(m_femaleName)) ? m_femaleName : m_name; }
		}

		public string BaseName
		{
			get { return m_basename; }
		}

		/// <summary>
		/// Return Translated Profession
		/// </summary>
		public string Profession
		{
			get
			{
				return LanguageMgr.TryTranslateOrDefault(Player, m_profession, m_profession);
			}
		}

		public int SpecPointsMultiplier
		{
			get { return m_specializationMultiplier; }
		}

		/// <summary>
		/// This is specifically used for adjusting spec points as needed for new training window
		/// For standard DOL classes this will simply return the standard spec multiplier
		/// </summary>
		public int AdjustedSpecPointsMultiplier
		{
			get { return m_specializationMultiplier; }
		}

		public eStat PrimaryStat
		{
			get { return m_primaryStat; }
		}

		public eStat SecondaryStat
		{
			get { return m_secondaryStat; }
		}

		public eStat TertiaryStat
		{
			get { return m_tertiaryStat; }
		}

		public eStat ManaStat
		{
			get { return m_manaStat; }
		}

		public int WeaponSkillBase
		{
			get { return m_baseWeaponSkill; }
		}

		public int WeaponSkillRangedBase
		{
			get { return m_baseWeaponSkillRanged; }
		}

		/// <summary>
		/// Maximum number of pulsing spells that can be active simultaneously
		/// </summary>
		public ushort MaxPulsingSpells
		{
			get { return m_maxPulsingSpells; }
		}

		public virtual string GetTitle(GamePlayer player, int level)
		{
			
			// Clamp level in 5 by 5 steps - 50 is the max available translation for now
			int clamplevel = Math.Min(50, (level / 5) * 5);
			
			string none = LanguageMgr.TryTranslateOrDefault(player, "!None!", "PlayerClass.GetTitle.none");
			
			if (clamplevel > 0)
				return LanguageMgr.TryTranslateOrDefault(player, string.Format("!{0}!", m_name), string.Format("PlayerClass.{0}.GetTitle.{1}", m_name, clamplevel));

			return none;
		}

		public virtual eClassType ClassType
		{
			get { return eClassType.ListCaster; }
		}

		/// <summary>
		/// Return the base list of Realm abilities that the class
		/// can train in.  Added by Echostorm for RAs
		/// </summary>
		/// <returns></returns>
		public IList<string> GetAutotrainableSkills()
		{
			return m_autotrainableSkills;
		}

		/// <summary>
		/// What Champion trainer does this class use?
		/// </summary>
		/// <returns></returns>
		public virtual GameTrainer.eChampionTrainerType ChampionTrainerType()
		{
			return GameTrainer.eChampionTrainerType.None;
		}

		/// <summary>
		/// Add things that are required for current level
		/// Skills and other things are handled through player specs... (on Refresh Specs)
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="previousLevel">the previous level of the player</param>
		public virtual void OnLevelUp(GamePlayer player, int previousLevel)
		{
		}

		/// <summary>
		/// Add various skills as the player levels his realm rank up
		/// </summary>
		/// <param name="player">player to modify</param>
		public virtual void OnRealmLevelUp(GamePlayer player)
		{
			//we dont want to add things when players arent using their advanced class
			if (player.CharacterClass.BaseName == player.CharacterClass.Name)
				return;
		}

		/// <summary>
		/// Add all spell-lines and other things that are new when this skill is trained
		/// </summary>
		/// <param name="player">player to modify</param>
		/// <param name="skill">The skill that is trained</param>
		public virtual void OnSkillTrained(GamePlayer player, Specialization skill)
		{
		}

		/// <summary>
		/// Checks whether player has ability to use lefthanded weapons
		/// </summary>
		public virtual bool CanUseLefthandedWeapon
		{
			get { return false; }
		}

		public virtual bool HasAdvancedFromBaseClass()
		{
			return true;
		}

		public virtual void SetControlledBrain(IControlledBrain controlledBrain)
		{
			if (controlledBrain == Player.ControlledBrain) return;
			if (controlledBrain == null)
			{
				Player.Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget2", Player.ControlledBrain.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.SetControlledNpc.ReleaseTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (controlledBrain.Owner != Player)
					throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Player.Name + ", owner=" + controlledBrain.Owner.Name + ")", "controlledNpc");
				if (Player.ControlledBrain == null)
					Player.InitControlledBrainArray(1);
				Player.Out.SendPetWindow(controlledBrain.Body, ePetWindowAction.Open, controlledBrain.AggressionState, controlledBrain.WalkState);
				if (controlledBrain.Body != null)
				{
					Player.Out.SendNPCCreate(controlledBrain.Body); // after open pet window again send creation NPC packet
					if (controlledBrain.Body.Inventory != null)
						Player.Out.SendLivingEquipmentUpdate(controlledBrain.Body);
				}
			}

			Player.ControlledBrain = controlledBrain;

		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		public virtual void CommandNpcRelease()
		{
			IControlledBrain controlledBrain = Player.ControlledBrain;
			if (controlledBrain == null)
				return;

			GameNPC npc = controlledBrain.Body;
			if (npc == null)
				return;
			
			if (npc is GamePet pet)
				pet.StripBuffs();

			Player.Notify(GameLivingEvent.PetReleased, npc);
		}

		/// <summary>
		/// Invoked when pet is released.
		/// </summary>
		public virtual void OnPetReleased()
		{
		}

		/// <summary>
		/// Can this character start an attack?
		/// </summary>
		/// <param name="attackTarget"></param>
		/// <returns></returns>
		public virtual bool StartAttack(GameObject attackTarget)
		{
			return true;
		}


		/// <summary>
		/// Return the health percent of this character
		/// </summary>
		public virtual byte HealthPercentGroupWindow
		{
			get
			{
				return Player.HealthPercent;
			}
		}


		/// <summary>
		/// Create a shade effect for this player.
		/// </summary>
		/// <returns></returns>
		public virtual ShadeEffect CreateShadeEffect()
		{
			return new ShadeEffect();
		}

		/// <summary>
		/// Changes shade state of the player.
		/// </summary>
		/// <param name="state">The new state.</param>
		public virtual void Shade(bool makeShade)
		{
			if (Player.IsShade == makeShade)
			{
				if (makeShade && (Player.ObjectState == GameObject.eObjectState.Active))
					Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.Shade.AlreadyShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (makeShade)
			{
				// Turn into a shade.
				Player.Model = Player.ShadeModel;
				Player.ShadeEffect = CreateShadeEffect();
				Player.ShadeEffect.Start(Player);
			}
			else
			{
				if (Player.ShadeEffect != null)
				{
					// Drop shade form.
					Player.ShadeEffect.Stop();
					Player.ShadeEffect = null;
				}
				// Drop shade form.
				Player.Model = Player.CreationModel;
				Player.Out.SendMessage(LanguageMgr.GetTranslation(Player.Client.Account.Language, "GamePlayer.Shade.NoLongerShade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// Called when player is removed from world.
		/// </summary>
		/// <returns></returns>
		public virtual bool RemoveFromWorld()
		{
			return true;
		}

		/// <summary>
		/// What to do when this character dies
		/// </summary>
		/// <param name="killer"></param>
		public virtual void Die(GameObject killer)
		{
		}

		public virtual void Notify(DOLEvent e, object sender, EventArgs args)
		{
		}

		public virtual bool CanChangeCastingSpeed(SpellLine line, Spell spell)
		{
			return true;
		}
	}
}
