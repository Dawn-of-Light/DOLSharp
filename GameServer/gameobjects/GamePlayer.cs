/*
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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Text;

using DOL.AI.Brain;
using DOL.GS;
using DOL.Events;
using DOL.Database;
using DOL.GS.Effects;
using DOL.GS.Housing;
using DOL.GS.Keeps;
using DOL.GS.PacketHandler.Client.v168;
using DOL.GS.PlayerTitles;
using DOL.GS.PropertyCalc;
using DOL.GS.Quests;
using DOL.GS.RealmAbilities;
using DOL.GS.SkillHandler;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.Language;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a player inside the game
	/// </summary>
	public class GamePlayer : GameLiving
	{
		private readonly object m_LockObject = new object();
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Client/Character/VariousFlags


		/// <summary>
		/// This is our gameclient!
		/// </summary>
		protected readonly GameClient m_client;
		/// <summary>
		/// This holds the character this player is
		/// based on!
		/// (renamed and private, cause if derive is needed overwrite PlayerCharacter)
		/// </summary>
		private Character my_character;
		/// <summary>
		/// The database id this character belong to
		/// </summary>
		protected string m_guildid;
		/// <summary>
		/// Char spec points checked on load
		/// </summary>
		protected bool SpecPointsOk = true;
		/// <summary>
		/// Has this player entered the game, will be
		/// true after the first time the char enters
		/// the world
		/// </summary>
		protected bool m_enteredGame;
		/// <summary>
		/// Holds the objects that need update
		/// </summary>
		protected readonly BitArray[] m_objectUpdates;
		/// <summary>
		/// Holds the index into the last update array
		/// </summary>
		protected byte m_lastUpdateArray;
		/// <summary>
		/// Holds the tickcount when the npcs around this player
		/// were checked the last time for new npcs. Will be done
		/// every 250ms in WorldMgr.
		/// </summary>
		protected int m_lastNPCUpdate;

		/// <summary>
		/// true if the targetObject is visible
		/// </summary>
		protected bool m_targetInView;

		/// <summary>
		/// Property for the optional away from keyboard message.
		/// </summary>
		public static readonly string AFK_MESSAGE = "afk_message";

		/// <summary>
		/// Property for the optional away from keyboard message.
		/// </summary>
		public static readonly string QUICK_CAST_CHANGE_TICK = "quick_cast_change_tick";

		/// <summary>
		/// Array that stores ML step completition
		/// </summary>		
		private ArrayList m_mlsteps = new ArrayList();

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
        /// Player is in BG ?
        /// </summary>
        protected bool m_isInBG;
        public bool isInBG
        {
            get { return m_isInBG; }
            set { m_isInBG = value; }
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
		}

		/// <summary>
		/// Returns the PacketSender for this player
		/// </summary>
		public IPacketLib Out
		{
			get { return Client.Out; }
		}

		/// <summary>
		/// The character the player is based on
		/// </summary>
		public Character PlayerCharacter
		{
			get { return my_character; }
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
		/// Gets or sets the anonymous flag for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public bool IsAnonymous
		{
			get { return PlayerCharacter != null ? PlayerCharacter.IsAnonymous && (ServerProperties.Properties.ANON_MODIFIER != -1) : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.IsAnonymous = value; }
		}

		/// <summary>
		/// Gets or sets the no help flag for this player
		/// </summary>
		public bool NoHelp
		{
			get { return PlayerCharacter != null ? PlayerCharacter.NoHelp : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.NoHelp = value; }
		}

		/// <summary>
		/// Gets or sets the show guild logins flag for this player
		/// </summary>
		public bool ShowGuildLogins
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ShowGuildLogins : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.ShowGuildLogins = value; }
		}

		/// <summary>
		/// Gets or sets the gain XP flag for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public bool GainXP
		{
			get { return PlayerCharacter != null ? PlayerCharacter.GainXP : true; }
			set { if (PlayerCharacter != null) PlayerCharacter.GainXP = value; }
		}

		/// <summary>
		/// Gets or sets the gain RP flag for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public bool GainRP
		{
			get { return (PlayerCharacter != null ? PlayerCharacter.GainRP : true); }
			set { if (PlayerCharacter != null) PlayerCharacter.GainRP = value; }
		}

		/// <summary>
		/// gets or sets the guildnote for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public string GuildNote
		{
			get { return PlayerCharacter != null ? PlayerCharacter.GuildNote : String.Empty; }
			set { if (PlayerCharacter != null) PlayerCharacter.GuildNote = value; }
		}

		/// <summary>
		/// Gets or sets the autoloot flag for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public bool Autoloot
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Autoloot : true; }
			set { if (PlayerCharacter != null) PlayerCharacter.Autoloot = value; }
		}

		/// <summary>
		/// Gets or sets the advisor flag for this player
		/// (delegate to property in PlayerCharacter)
		/// </summary>
		public bool Advisor
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Advisor : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.Advisor = value; }
		}

		private bool m_canUseSlashLevel = false;
		public bool CanUseSlashLevel
		{
			get { return m_canUseSlashLevel; }
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
			if (!IsAlive || ObjectState != eObjectState.Active)
			{
				m_quitTimer = null;
				return 0;
			}

			//Gms can quit instantly
			if (Client.Account.PrivLevel == 1)
			{
				if (CraftTimer != null && CraftTimer.IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.CantQuitCrafting"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					m_quitTimer = null;
					return 0;
				}

				long lastCombatAction = LastAttackedByEnemyTick;
				if (lastCombatAction < LastAttackTick)
				{
					lastCombatAction = LastAttackTick;
				}
				long secondsleft = 60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000; // 500 is for rounding
				if (secondsleft > 0)
				{
					if (secondsleft == 15 || secondsleft == 10 || secondsleft == 5)
					{
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.YouWillQuit1", secondsleft), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					return 1000;
				}
			}

			Out.SendPlayerQuit(false);
			Quit(true);
			SaveIntoDatabase();
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
					// dirty trick ;-) (20sec min quit time)
					if (CurrentRegion.Time - LastAttackTickPvP > 40000)
						LastAttackTickPvP = CurrentRegion.Time - 40000;
					if (CurrentRegion.Time - LastAttackTickPvE > 40000)
						LastAttackTickPvE = CurrentRegion.Time - 40000;
				}
				long lastCombatAction = LastAttackTick;
				if (lastCombatAction < LastAttackedByEnemyTick)
				{
					lastCombatAction = LastAttackedByEnemyTick;
				}
				return (int)(60 - (CurrentRegion.Time - lastCombatAction + 500) / 1000); // 500 is for rounding
			}
			set
			{ }
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
			try
			{
				if (!IsAlive)
				{
					Release(m_releaseType, true);
					if (log.IsInfoEnabled)
						log.Info("Linkdead player " + Name + "(" + Client.Account.Name + ") was auto-released from death!");
				}

				SaveIntoDatabase();
			}
			finally
			{
				Client.Quit();
			}

			return 0;
		}


		#region Combat timer
		RegionTimer noCombatTimer = null;

		public override long LastAttackedByEnemyTickPvE
		{
			set
			{
				bool wasInCombat = InCombat;
				base.LastAttackedByEnemyTickPvE = value;
				if (!wasInCombat && InCombat)
				{
					Out.SendUpdateMaxSpeed();
				}
				ResetInCombatTimer();
			}
		}

		public override long LastAttackTickPvE
		{
			set
			{
				bool wasInCombat = InCombat;
				base.LastAttackTickPvE = value;
				if (!wasInCombat && InCombat)
				{
					Out.SendUpdateMaxSpeed();
				}
				ResetInCombatTimer();
			}
		}

		protected void ResetInCombatTimer()
		{
			if (noCombatTimer == null)
			{
				noCombatTimer = new RegionTimer(this, new RegionTimerCallback(InCombatTimerExpired));
			}
			noCombatTimer.Stop();
			noCombatTimer.Start(11000);
		}

		public int InCombatTimerExpired(RegionTimer timer)
		{
			Out.SendUpdateMaxSpeed();
			return 0;
		}
		#endregion

		public void OnLinkdeath()
		{
			if (log.IsInfoEnabled)
				log.Info("Player " + Name + "(" + Client.Account.Name + ") went linkdead!");

			// Dead link-dead players release on live servers
			if (!IsAlive)
			{
				Release(m_releaseType, true);
				if (log.IsInfoEnabled)
					log.Info("Linkdead player " + Name + "(" + Client.Account.Name + ") was auto-released from death!");
				SaveIntoDatabase();
				Client.Quit();
				return;
			}

			//Stop player if he's running....
			CurrentSpeed = 0;
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				//Maybe there is a better solution?
				player.Out.SendObjectRemove(this);
				player.Out.SendPlayerCreate(this);
			}

			UpdateEquipmentAppearance();

			SaveIntoDatabase();

			if (m_quitTimer != null)
			{
				m_quitTimer.Stop();
				m_quitTimer = null;
			}

			int secondsToQuit = QuitTime;
			if (log.IsInfoEnabled)
				log.Info("Linkdead player " + Name + "(" + Client.Account.Name + ") will quit in " + secondsToQuit);
			RegionTimer timer = new RegionTimer(this); // make sure it is not stopped!
			timer.Callback = new RegionTimerCallback(LinkdeathTimerCallback);
			timer.Start(1 + secondsToQuit * 1000);

			if (TradeWindow != null)
				TradeWindow.CloseTrade();

			//Notify players in close proximity!
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
			{
				if (GameServer.ServerRules.IsAllowedToUnderstand(this, player))
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.OnLinkdeath.Linkdead", Name), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			//Notify other group members of this linkdead
			if (Group != null)
				Group.UpdateMember(this, false, false);

			//Notify our event handlers (if any)
			Notify(GamePlayerEvent.Linkdeath, this);
		}

		/// <summary>
		/// Stop all timers, events and remove player from everywhere (group/guild/chat)
		/// </summary>
		public virtual void CleanupOnDisconnect()
		{
			StopAttack();
			// remove all stealth handlers
			Stealth(false);
			if (IsOnHorse)
				IsOnHorse = false;
			if (InWraithForm)
				InWraithForm = false;
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(RangeAttackHandler));
			GameEventMgr.RemoveHandler(m_inventory, PlayerInventoryEvent.ItemEquipped, new DOLEventHandler(OnItemEquipped));
			GameEventMgr.RemoveHandler(m_inventory, PlayerInventoryEvent.ItemUnequipped, new DOLEventHandler(OnItemUnequipped));
			GameEventMgr.RemoveHandler(m_inventory, PlayerInventoryEvent.ItemBonusChanged, new DOLEventHandler(OnItemBonusChanged));

			if (Group != null)
				Group.RemoveMember(this);

			BattleGroup mybattlegroup = (BattleGroup)this.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
			if (mybattlegroup != null)
				mybattlegroup.RemoveBattlePlayer(this);

			if (TradeWindow != null)
				TradeWindow.CloseTrade();

			if (m_guild != null)
				m_guild.RemoveOnlineMember(this);

			// RR4: expire personal mission, if any
			if (Mission != null)
				Mission.ExpireMission();

			ChatGroup mychatgroup = (ChatGroup)TempProperties.getObjectProperty(ChatGroup.CHATGROUP_PROPERTY, null);
			if (mychatgroup != null)
				mychatgroup.RemovePlayer(this);

			CommandNpcRelease();

			if (SiegeWeapon != null)
				SiegeWeapon.ReleaseControl();

			if (InHouse)
				CurrentHouse.Exit(this, true);

			if (CurrentRegion.IsInstance)
				MoveTo((ushort)PlayerCharacter.BindRegion, PlayerCharacter.BindXpos, PlayerCharacter.BindYpos, PlayerCharacter.BindZpos, (ushort)PlayerCharacter.BindHeading);

			//check for battleground caps
			Battleground bg = KeepMgr.GetBattleground(CurrentRegionID);
			if (bg != null)
			{
				if (Level > bg.MaxLevel || RealmLevel >= bg.MaxRealmLevel)
				{
					KeepMgr.ExitBattleground(this);
				}
			}

			// Remove champion dedicated spell line
			SkillBase.UnRegisterSpellLine("Champion Abilities" + Name);

			// cancel all effects until saving of running effects is done
			try
			{
				EffectList.SaveAllEffects();
				CancelAllConcentrationEffects();
				EffectList.CancelAll();
			}
			catch (Exception e)
			{
				log.Error("Cannot cancel all effects, " + e.ToString());
			}
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
				if (!IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.CantQuitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (Steed != null || IsOnHorse)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.CantQuitMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (IsMoving)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.CantQuitStanding"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (CraftTimer != null && CraftTimer.IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.CantQuitCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (CurrentRegion.IsInstance)
				{
					Out.SendMessage("You cannot quit in an instance!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				string stats = PlayerStatistic.GetStatsMessage(this);
				if (stats != "")
					Out.SendMessage(stats, eChatType.CT_System, eChatLoc.CL_SystemWindow);

				if (!IsSitting)
				{
					Sit(true);
				}
				int secondsleft = QuitTime;

				if (m_quitTimer == null)
				{
					m_quitTimer = new RegionTimer(this);
					m_quitTimer.Callback = new RegionTimerCallback(QuitTimerCallback);
					m_quitTimer.Start(1);
				}

				if (secondsleft > 20)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.RecentlyInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Quit.YouWillQuit2", secondsleft), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				//Notify our event handlers (if any)
				Notify(GamePlayerEvent.Quit, this);

				//Cleanup stuff
				CleanupOnDisconnect();
				Delete();
			}
			return true;
		}

		/// <summary>
		/// Updates Health, Mana, Sitting, Endurance, Concentration and Alive status to client
		/// </summary>
		public void UpdatePlayerStatus()
		{
			Out.SendStatusUpdate();
		}

		/// <summary>
		/// The last time we did update the NPCs around us
		/// </summary>
		public int LastNPCUpdate
		{
			get { return m_lastNPCUpdate; }
			set { m_lastNPCUpdate = value; }
		}

		#endregion

		#region release/bind/pray
		/// <summary>
		/// Property that holds tick when the player bind last time
		/// </summary>
		public const string LAST_BIND_TICK = "LastBindTick";

		/// <summary>
		/// Binds this player to the current location
		/// </summary>
		/// <param name="forced">if true, can bind anywhere</param>
		public virtual void Bind(bool forced)
		{
			Character character = PlayerCharacter; // if property will be derived with cost-intensive code, so its only getted one time
			if (character == null) return;

			if (forced)
			{
				character.BindRegion = CurrentRegionID;
				character.BindHeading = Heading;
				character.BindXpos = X;
				character.BindYpos = Y;
				character.BindZpos = Z;
				GameServer.Database.SaveObject(character);
				return;
			}

			string description = "";

			Region reg = WorldMgr.GetRegion((ushort)character.BindRegion);
			Zone zon = null;
			if (reg != null)
				zon = reg.GetZone(character.BindXpos, character.BindYpos);
			if (zon != null)
			{
				IList areas = zon.GetAreasOfSpot(character.BindXpos, character.BindYpos, character.BindZpos);

				foreach (AbstractArea area in areas)
				{
					if (!area.DisplayMessage) continue;
					description = area.Description;
					break;
				}
			}
			if (description == "")
			{
				if (zon != null)
					description = zon.Description;
			}
			else
			{
				description += " in " + zon.Description;
			}
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Bind.LastBindPoint", description), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Bind.CantBindDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			long lastBindTick = TempProperties.getLongProperty(LAST_BIND_TICK, 0L);
			long changeTime = CurrentRegion.Time - lastBindTick;
			if (Client.Account.PrivLevel == 1 && changeTime < 60000) //60 second rebind timer
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Bind.MustWait", (1 + (60000 - changeTime) / 1000)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			bool bound = false;
			lock (CurrentAreas.SyncRoot)
			{
				foreach (AbstractArea area in CurrentAreas)
				{
					if (area is Area.BindArea)
					{
						if (!GameServer.ServerRules.IsAllowedToBind(this, (area as Area.BindArea).BindPoint)) continue;
						TempProperties.setProperty(LAST_BIND_TICK, CurrentRegion.Time);

						bound = true;
						character.BindRegion = CurrentRegionID;
						character.BindHeading = Heading;
						character.BindXpos = X;
						character.BindYpos = Y;
						character.BindZpos = Z;
						GameServer.Database.SaveObject(character);
						break;
					}
				}
			}
			if (bound)
			{
				if (!IsMoving)
				{
					eEmote bindEmote = eEmote.Bind;
					switch (this.Realm)
					{
						case eRealm.Albion: bindEmote = eEmote.BindAlb; break;
						case eRealm.Midgard: bindEmote = eEmote.BindMid; break;
						case eRealm.Hibernia: bindEmote = eEmote.BindHib; break;
					}
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if ((int)player.Client.Version < (int)GameClient.eClientVersion.Version187)
							player.Out.SendEmoteAnimation(this, eEmote.Bind);
						else player.Out.SendEmoteAnimation(this, bindEmote);
					}
				}
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Bind.Bound"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Bind.CantHere"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
		public void StopReleaseTimer()
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
			/// <summary>
			/// Release to your bind point
			/// </summary>
			Bind,
			/// <summary>
			/// Release in a battleground or the frontiers
			/// </summary>
			RvR,
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
			Character character = PlayerCharacter;
			if (character == null) return;

			//battlegrounds caps
			Battleground bg = KeepMgr.GetBattleground(CurrentRegionID);
			if (bg != null && releaseCommand == eReleaseType.RvR)
			{
				if (Level > bg.MaxLevel)
					releaseCommand = eReleaseType.Normal;
			}

			if (IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.NotDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!forced)
			{
				if (m_releaseType == eReleaseType.Duel)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.CantReleaseDuel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}
				m_releaseType = releaseCommand;
				// we use realtime, because timer window is realtime
				int diff = m_deathTick - Environment.TickCount + RELEASE_MINIMUM_WAIT * 1000;
				if (diff >= 1000)
				{
					if (m_automaticRelease)
					{
						m_automaticRelease = false;
						m_releaseType = eReleaseType.Normal;
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.NoLongerReleaseAuto", diff / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					m_automaticRelease = true;
					switch (releaseCommand)
					{
						default:
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.WillReleaseAuto", diff / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						case eReleaseType.City:
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.WillReleaseAutoCity", diff / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
						case eReleaseType.RvR:
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.ReleaseToPortalKeep", diff / 1000), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
					}
				}
			}
			else
			{
				m_releaseType = releaseCommand;
			}

			int relX = 0, relY = 0, relZ = 0;
			ushort relRegion = 0, relHeading = 0;
			switch (m_releaseType)
			{
				case eReleaseType.Duel:
					{
						relRegion = (ushort)character.Region;
						relX = character.Xpos;
						relY = character.Ypos;
						relZ = character.Zpos;
						relHeading = 2048;
						break;
					}
				case eReleaseType.City:
					{
						if (Realm == eRealm.Hibernia)
						{
							relRegion = 201; // Tir Na Nog
							relX = 8192 + 15780;
							relY = 8192 + 22727;
							relZ = 7060;
						}
						else if (Realm == eRealm.Midgard)
						{
							relRegion = 101; // Jordheim
							relX = 8192 + 24664;
							relY = 8192 + 21402;
							relZ = 8759;
						}
						else
						{
							relRegion = 10; // City of Camelot
							relX = 8192 + 26315;
							relY = 8192 + 21177;
							relZ = 8256;
						}
						relHeading = 2048;
						break;
					}
				case eReleaseType.RvR:
					{
						foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
						{
							if (keep.IsPortalKeep && keep.OriginalRealm == Realm)
							{
								relRegion = keep.CurrentRegion.ID;
								relX = keep.X;
								relY = keep.Y;
								relZ = keep.Z;
							}
						}

						//if we aren't releasing anywhere, release to the border keeps
						if (relX == 0)
						{
							relRegion = CurrentRegion.ID;
							KeepMgr.GetBorderKeepLocation(((byte)Realm * 2) / 1, out relX, out relY, out relZ, out relHeading);
						}
						break;
					}
				default:
					{
						if (!ServerProperties.Properties.DISABLE_TUTORIAL)
						{
							//Tutorial
							if (character.BindRegion == 27)
							{
								switch (Realm)
								{
									case eRealm.Albion:
										{
											relRegion = 1; // Cotswold
											relX = 8192 + 553251;
											relY = 8192 + 502936;
											relZ = 2280;
											break;
										}
									case eRealm.Midgard:
										{
											relRegion = 100; // Mularn
											relX = 8192 + 795621;
											relY = 8192 + 719590;
											relZ = 4680;
											break;
										}
									case eRealm.Hibernia:
										{
											relRegion = 200; // MagMell
											relX = 8192 + 338652;
											relY = 8192 + 482335;
											relZ = 5200;
											break;
										}
								}
								break;
							}
						}
						switch (CurrentRegionID)
						{
							//battlegrounds
							case 234:
							case 235:
							case 236:
							case 237:
							case 238:
							case 239:
							case 240:
							case 241:
							case 242:
								{
									//get the bg cap
									byte cap = 50;
									foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
									{
										if (keep.DBKeep.BaseLevel < cap)
										{
											cap = keep.DBKeep.BaseLevel;
											break;
										}
									}
									//get the portal location
									foreach (AbstractGameKeep keep in KeepMgr.GetKeepsOfRegion(CurrentRegionID))
									{
										if (keep.DBKeep.BaseLevel > 50 && keep.Realm == Realm)
										{
											relRegion = (ushort)keep.Region;
											relX = keep.X;
											relY = keep.Y;
											relZ = keep.Z;
											break;
										}
									}
									break;
								}
							//nf
							case 163:
								{
									if (character.BindRegion != 163)
									{
										relRegion = 163;
										switch (Realm)
										{
											case eRealm.Albion:
												{
													KeepMgr.GetBorderKeepLocation(1, out relX, out relY, out relZ, out relHeading);
													break;
												}
											case eRealm.Midgard:
												{
													KeepMgr.GetBorderKeepLocation(3, out relX, out relY, out relZ, out relHeading);
													break;
												}
											case eRealm.Hibernia:
												{
													KeepMgr.GetBorderKeepLocation(5, out relX, out relY, out relZ, out relHeading);
													break;
												}
										}
										break;
									}
									else
									{
										relRegion = (ushort)character.BindRegion;
										relX = character.BindXpos;
										relY = character.BindYpos;
										relZ = character.BindZpos;
										relHeading = (ushort)character.BindHeading;
									}
									break;
								}/*
								//bg45-49
							case 165:
								{
									break;
								}*/
							default:
								{
									relRegion = (ushort)character.BindRegion;
									relX = character.BindXpos;
									relY = character.BindYpos;
									relZ = character.BindZpos;
									relHeading = (ushort)character.BindHeading;
									break;
								}
						}
						break;
					}
			}

			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.YouRelease"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
			Out.SendCloseTimerWindow();
			if (m_releaseTimer != null)
			{
				m_releaseTimer.Stop();
				m_releaseTimer = null;
			}

			if (Realm != eRealm.None)
			{
				if (Level > 5)
				{
					// actual lost exp, needed for 2nd stage deaths
					long lostExp = Experience;
					long lastDeathExpLoss = TempProperties.getLongProperty(DEATH_EXP_LOSS_PROPERTY, 0);
					TempProperties.removeProperty(DEATH_EXP_LOSS_PROPERTY);

					GainExperience(-lastDeathExpLoss);
					lostExp -= Experience;

					// raise only the gravestone if xp has to be stored in it
					if (lostExp > 0)
					{
						// find old gravestone of player and remove it
						if (character.HasGravestone)
						{
							Region reg = WorldMgr.GetRegion((ushort)character.GravestoneRegion);
							if (reg != null)
							{
								GameGravestone oldgrave = reg.FindGraveStone(this);
								if (oldgrave != null)
								{
									oldgrave.Delete();
								}
							}
							character.HasGravestone = false;
						}

						GameGravestone gravestone = new GameGravestone(this, lostExp);
						gravestone.AddToWorld();
						character.GravestoneRegion = gravestone.CurrentRegionID;
						character.HasGravestone = true;
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.GraveErected"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.ReturnToPray"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					}
				}
			}

			if (Level > 10)
			{
				int deathConLoss = TempProperties.getIntProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, 0); // get back constitution lost at death
				if (deathConLoss > 0)
				{
					TotalConstitutionLostAtDeath += deathConLoss;
					Out.SendCharStatsUpdate();
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.LostConstitution"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
				}
			}

			//Update health&sit state first!
			Health = MaxHealth;
			StartPowerRegeneration();
			StartEnduranceRegeneration();

			Region region = null;
			if ((region = WorldMgr.GetRegion((ushort)character.BindRegion)) != null && region.GetZone(character.BindXpos, character.BindYpos) != null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.SurroundingChange"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.NoValidBindpoint"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				Bind(true);
			}

			int oldRegion = CurrentRegionID;

			//Call MoveTo after new GameGravestone(this...
			//or the GraveStone will be located at the player's bindpoint
			MoveTo(relRegion, relX, relY, relZ, relHeading);
			//It is enough if we revive the player on this client only here
			//because for other players the player will be removed in the MoveTo
			//method and added back again (if in view) with full health ... so no
			//revive needed for others...
			Out.SendPlayerRevive(this);
			//			Out.SendUpdatePlayer();
			Out.SendUpdatePoints();

			//Set property indicating that we are releasing to another region; used for Released event
			if (oldRegion != character.BindRegion)
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
				for (int i = 0; i < m_lastUniqueLocations.Length; i++)
				{
					GameLocation loc = m_lastUniqueLocations[i];
					loc.X = X;
					loc.Y = Y;
					loc.Z = Z;
					loc.Heading = Heading;
					loc.RegionID = CurrentRegionID;
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
			if (IsAlive)
				return 0;
			int diffToRelease = Environment.TickCount - m_deathTick;
			if (m_automaticRelease && diffToRelease > RELEASE_MINIMUM_WAIT * 1000)
			{
				Release(m_releaseType, true);
				return 0;
			}
			diffToRelease = (RELEASE_TIME * 1000 - diffToRelease) / 1000;
			if (diffToRelease <= 0)
			{
				Release(m_releaseType, true);
				return 0;
			}
			if (m_releasePhase <= 1 && diffToRelease <= 10 && diffToRelease >= 8)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.WillReleaseIn", 10), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				m_releasePhase = 2;
			}
			if (m_releasePhase == 0 && diffToRelease <= 30 && diffToRelease >= 28)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Release.WillReleaseIn", 30), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
		/// Property that saves the gravestone in the pray timer
		/// </summary>
		protected const string GRAVESTONE_PROPERTY = "gravestone";
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
			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.CantPrayNow"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsRiding)
			{
				Out.SendMessage("You can't pray while riding a horse!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			GameGravestone gravestone = TargetObject as GameGravestone;
			if (gravestone == null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.NeedTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!gravestone.InternalID.Equals(InternalID))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.SelectGrave"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (!WorldMgr.CheckDistance(this, gravestone, 2000))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.MustGetCloser"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}
			if (IsMoving)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.MustStandingStill"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (PrayState)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.AlreadyPraying"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (m_prayAction != null)
				m_prayAction.Stop();
			m_prayAction = new PrayAction(this, gravestone);
			m_prayAction.Start(PRAY_DELAY);

			Sit(true);
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Pray.Begin"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
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
			public PrayAction(GamePlayer actionSource, GameGravestone grave)
				: base(actionSource)
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
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GamePlayer.Pray.GainBack"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					player.GainExperience(xp);
				}
				m_gravestone.Delete();
			}
		}

		/// <summary>
		/// Called when player revive
		/// </summary>
		public virtual void OnRevive(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = (GamePlayer)sender;
			if (player.Level > 5)
			{
				SpellLine Line = SkillBase.GetSpellLine(GlobalSpellsLines.Reserved_Spells);
				if (Line == null) return;
				ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(player, GlobalSpells.PvERezIllness, Line);
				spellHandler.StartSpell(player);
			}
			GameEventMgr.RemoveHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
		}

		#endregion

		#region Name/LastName/GuildName/Model

		/// <summary>
		/// The lastname of this player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual string LastName
		{
			get { return PlayerCharacter != null ? PlayerCharacter.LastName : string.Empty; }
			set
			{
				if (PlayerCharacter == null) return;
				PlayerCharacter.LastName = value;
				//update last name for all players if client is playing
				if (ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player != this)
						{
							player.Out.SendObjectRemove(this);
							player.Out.SendPlayerCreate(this);
							player.Out.SendLivingEquipmentUpdate(this);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the guildname of this player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override string GuildName
		{
			get
			{
				if (m_guild == null)
					return "";
				else return m_guild.Name;
			}
			set
			{ }
		}

		/// <summary>
		/// Gets or sets the name of the player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override string Name
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Name : string.Empty; }
			set
			{
				if (PlayerCharacter == null) return;
				PlayerCharacter.Name = value;
				//base.Name = value; // only need if there will be some special code in base-property in future
				//update name for all players if client is playing
				if (ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					if (Group != null)
						Out.SendGroupWindowUpdate();
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player != this)
						{
							player.Out.SendObjectRemove(this);
							player.Out.SendPlayerCreate(this);
							player.Out.SendLivingEquipmentUpdate(this);
						}
					}
				}
			}
		}

		/// <summary>
		/// The prefix for RR 12/13 players
		/// </summary>
		public string PrefixName
		{
			get
			{
				if (RealmLevel >= 110)
					return RealmTitle;
				else
					return string.Empty;
			}
		}

		/// <summary>
		/// Sets or gets the model of the player. If the player is
		/// active in the world, the modelchange will be visible
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override ushort Model
		{
			get { return PlayerCharacter != null ? (ushort)PlayerCharacter.CurrentModel : base.Model; }
			set
			{
				if (PlayerCharacter == null || PlayerCharacter.CurrentModel == value) return;

				PlayerCharacter.CurrentModel = value;
				//base.Model; // only need if there will be some special code in base-property in future
				if (ObjectState == eObjectState.Active)
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendModelChange(this, Model);
			}
		}

		#endregion

		#region Stats

		/// <summary>
		/// Holds if the player can gain a FreeLevel
		/// </summary>
		public byte FreeLevelState
		{
			get
			{
				if (ServerProperties.Properties.FREELEVEL_DAYS == -1)
					return 1;
				//flag 1 = above level, 2 = elligable, 3= time until, 4 = level and time until, 5 = level until
				if (Level >= 48)
					return 1;
				TimeSpan t = new TimeSpan((long)(DateTime.Now.Ticks - PlayerCharacter.LastFreeLeveled.Ticks));
				if (t.Days >= ServerProperties.Properties.FREELEVEL_DAYS)
				{
					if (Level >= PlayerCharacter.LastFreeLevel + 2)
						return 2;
					else return 5;
				}
				else
				{
					if (Level >= PlayerCharacter.LastFreeLevel + 2)
						return 3;
					else return 4;
				}
			}
		}

		/// <summary>
		/// Gets/sets the player efficacy percent
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int TotalConstitutionLostAtDeath
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ConLostAtDeath : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.ConLostAtDeath = value; }
		}

        /// <summary>
        /// Change a stat value
        /// (delegate to PlayerCharacter)
        /// </summary>
        /// <param name="stat">The stat to change</param>
        /// <param name="val">The new value</param>
        public override void ChangeBaseStat(eStat stat, short val)
        {
            int oldstat = GetBaseStat(stat);
            base.ChangeBaseStat(stat, val);
            int newstat = GetBaseStat(stat);
            Character character = PlayerCharacter; // to call it only once, if in future there will be some special code to get the character
            // Graveen: always positive and not null. This allows /player stats to substract values safely
            if (newstat < 1) newstat = 1;
            if (character != null && oldstat != newstat)
            {
                switch (stat)
                {
                    case eStat.STR: character.Strength = newstat; break;
                    case eStat.DEX: character.Dexterity = newstat; break;
                    case eStat.CON: character.Constitution = newstat; break;
                    case eStat.QUI: character.Quickness = newstat; break;
                    case eStat.INT: character.Intelligence = newstat; break;
                    case eStat.PIE: character.Piety = newstat; break;
                    case eStat.EMP: character.Empathy = newstat; break;
                    case eStat.CHR: character.Charisma = newstat; break;
                }
            }
        }

		/// <summary>
		/// Gets player's constitution
		/// </summary>
		public int Constitution
		{
			get { return GetModified(eProperty.Constitution); }
		}

		/// <summary>
		/// Gets player's dexterity
		/// </summary>
		public int Dexterity
		{
			get { return GetModified(eProperty.Dexterity); }
		}

		/// <summary>
		/// Gets player's strength
		/// </summary>
		public int Strength
		{
			get { return GetModified(eProperty.Strength); }
		}

		/// <summary>
		/// Gets player's quickness
		/// </summary>
		public int Quickness
		{
			get { return GetModified(eProperty.Quickness); }
		}

		/// <summary>
		/// Gets player's intelligence
		/// </summary>
		public int Intelligence
		{
			get { return GetModified(eProperty.Intelligence); }
		}

		/// <summary>
		/// Gets player's piety
		/// </summary>
		public int Piety
		{
			get { return GetModified(eProperty.Piety); }
		}

		/// <summary>
		/// Gets player's empathy
		/// </summary>
		public int Empathy
		{
			get { return GetModified(eProperty.Empathy); }
		}

		/// <summary>
		/// Gets player's charisma
		/// </summary>
		public int Charisma
		{
			get { return GetModified(eProperty.Charisma); }
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
			m_healthRegenerationTimer.Start(m_healthRegenerationPeriod);
		}
		/// <summary>
		/// Starts the power regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StartPowerRegeneration()
		{
			if (ObjectState != eObjectState.Active) return;
			if (m_powerRegenerationTimer.IsAlive) return;
			m_powerRegenerationTimer.Start(m_powerRegenerationPeriod);
		}
		/// <summary>
		/// Starts the endurance regeneration.
		/// Overriden. No lazy timers for GamePlayers.
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
			if (ObjectState != eObjectState.Active) return;
			if (m_enduRegenerationTimer.IsAlive) return;
			m_enduRegenerationTimer.Start(m_enduRegenerationPeriod);
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
			if (Client.ClientState != GameClient.eClientState.Playing)
				return m_healthRegenerationPeriod;
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
			if (Client.ClientState != GameClient.eClientState.Playing)
				return m_powerRegenerationPeriod;
			int interval = base.PowerRegenerationTimerCallback(selfRegenerationTimer);
			return interval;
		}

		/// <summary>
		/// Override EnduranceRegenTimer because if we are not connected anymore
		/// we DON'T regenerate endurance, even if we are not garbage collected yet!
		/// </summary>
		/// <param name="selfRegenerationTimer">the timer</param>
		/// <returns>the new time</returns>
		protected override int EnduranceRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			if (Client.ClientState != GameClient.eClientState.Playing)
				return m_enduRegenerationPeriod;
			return base.EnduranceRegenerationTimerCallback(selfRegenerationTimer);
		}

		/// <summary>
		/// Gets/sets the object health
		/// </summary>
		public override int Health
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Health : base.Health; }
			set
			{
				//DOLConsole.WriteSystem("Health="+value);
				value = Math.Min(value, MaxHealth);
				value = Math.Max(value, 0);
				//If it is already set, don't do anything
				if (Health == value)
				{
					base.Health = value; //needed to start regeneration
					return;
				}

				int oldPercent = HealthPercent;
				base.Health = value;
				if (PlayerCharacter != null)
					PlayerCharacter.Health = value;
				if (oldPercent != HealthPercent)
				{
					if (Group != null)
						Group.UpdateMember(this, false, false);
					UpdatePlayerStatus();
				}
			}
		}

		/// <summary>
		/// Gets/sets the object max health
		/// </summary>
		public override int MaxHealth
		{
			get { return base.MaxHealth; }
			//			set
			//			{
			//				//If it is already set, don't do anything
			//				if (MaxHealth == value)
			//					return;
			//				base.MaxHealth = value;
			//				m_character.MaxHealth = base.MaxHealth;
			//				UpdatePlayerStatus();
			//			}
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
			int hp2 = hp1 * constitution / 10000;
			return Math.Max(1, 20 + hp1 / 50 + hp2);
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
				maxpower = (level * 5) + (manastat - 50);
			}
			if (maxpower < 0)
				maxpower = 0;
			return maxpower;
		}


		/// <summary>
		/// Gets/sets the object mana
		/// </summary>
		public override int Mana
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Mana : base.Mana; }
			set
			{
				value = Math.Min(value, MaxMana);
				value = Math.Max(value, 0);
				//If it is already set, don't do anything
				if (Mana == value)
				{
					base.Mana = value; //needed to start regeneration
					return;
				}
				int oldPercent = ManaPercent;
				base.Mana = value;
				if (PlayerCharacter != null)
					PlayerCharacter.Mana = value;
				if (oldPercent != ManaPercent)
				{
					if (Group != null)
						Group.UpdateMember(this, false, false);
					UpdatePlayerStatus();
				}
			}
		}

		/// <summary>
		/// Gets/sets the object max mana
		/// </summary>
		public override int MaxMana
		{
			get { return GetModified(eProperty.MaxMana); }
			//			set
			//			{
			//				//If it is already set, don't do anything
			//				if (MaxMana == value)
			//					return;
			//				base.MaxMana = value;
			//				m_character.MaxMana = base.m_maxMana;
			//				UpdatePlayerStatus();
			//			}
		}

		/// <summary>
		/// Gets/sets the object endurance
		/// </summary>
		public override int Endurance
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Endurance : base.Endurance; }
			set
			{
				value = Math.Min(value, MaxEndurance);
				value = Math.Max(value, 0);
				//If it is already set, don't do anything
				if (Endurance == value)
				{
					base.Endurance = value; //needed to start regeneration
					return;
				}
				else if (IsAlive && value < MaxEndurance)
					StartEnduranceRegeneration();
				int oldPercent = EndurancePercent;
				base.Endurance = value;
				if (PlayerCharacter != null)
					PlayerCharacter.Endurance = value;
				if (oldPercent != EndurancePercent)
				{
					//ogre: 1.69+ endurance is displayed on group window
					if (Group != null)
						Group.UpdateMember(this, false, false);
					//end ogre
					UpdatePlayerStatus();
				}
			}
		}

		/// <summary>
		/// Gets/sets the objects maximum endurance
		/// </summary>
		public override int MaxEndurance
		{
			get { return PlayerCharacter != null ? PlayerCharacter.MaxEndurance : base.MaxEndurance; }
			set
			{
				//If it is already set, don't do anything
				if (MaxEndurance == value)
					return;
				base.MaxEndurance = value;
				if (PlayerCharacter != null)
					PlayerCharacter.MaxEndurance = value;
				UpdatePlayerStatus();
			}
		}

		/// <summary>
		/// Gets the concentration left
		/// </summary>
		public override int Concentration
		{
			get { return MaxConcentration - ConcentrationEffects.UsedConcentration; }
		}

		/// <summary>
		/// Gets the maximum concentration for this player
		/// </summary>
		public override int MaxConcentration
		{
			get { return GetModified(eProperty.MaxConcentration); }
		}

		#endregion

		#region Class/Race

		/// <summary>
		/// All possible player races
		/// </summary>
		public static readonly string[] RACENAMES = new string[]
			{
				"Unknown",
				"Briton",
				"Avalonian",
				"Highlander",
				"Saracen",
				"Norseman",
				"Troll",
				"Dwarf",
				"Kobold",
				"Celt",
				"Firbolg",
				"Elf",
				"Lurikeen",
				"Inconnu",
				"Valkyn",
				"Sylvan",
				"Half Ogre",
				"Frostalf",
				"Shar",
				"Korazh",//albion minotaur
				"Deifang",//midgard minotaur
				"Graoch"//hibernia minotaur
			};

		/// <summary>
		/// Players class
		/// </summary>
		protected IClassSpec m_class;
		/// <summary>
		/// Gets/sets the player's race name
		/// </summary>
		public string RaceName
		{
			get { return RACENAMES[Race]; }
		}

		/// <summary>
		/// Gets or sets this player's race id
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int Race
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Race : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.Race = value; }
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
			IClassSpec cl = ScriptMgr.FindClassSpec(id);

			if (cl == null)
			{
				if (log.IsErrorEnabled)
					log.Error("No CharacterClass with ID " + id + " found");
				return false;
			}
			m_class = cl;
			if (Util.IsEmpty(m_class.FemaleName) == false && PlayerCharacter.Gender == 1)
				m_class.SwitchToFemaleName();
			PlayerCharacter.Class = m_class.ID;

            if (Group != null)
			{
				Group.UpdateMember(this, false, true);
			}
			return true;
		}

		/// <summary>
		/// Hold all player face custom attibutes
		/// </summary>
		protected byte[] m_customFaceAttributes = new byte[(int)eCharFacePart._Last + 1];

		/// <summary>
		/// Get the character face attribute you want
		/// </summary>
		/// <param name="part">face part</param>
		/// <returns>attribute</returns>
		public byte GetFaceAttribute(eCharFacePart part)
		{
			return m_customFaceAttributes[(int)part];
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
		/// Holds the Spell lines the player can use
		/// </summary>
		protected readonly ArrayList m_spelllines = new ArrayList();

		/// <summary>
		/// Holds all styles of the player
		/// </summary>
		protected readonly ArrayList m_styles = new ArrayList();

		/// <summary>
		/// Holds all non trainable skills in determined order without styles
		/// </summary>
		protected readonly ArrayList m_skillList = new ArrayList();

		/// <summary>
		/// Temporary Stats Boni
		/// </summary>
		protected readonly int[] m_statBonus = new int[8];

		/// <summary>
		/// Temporary Stats Boni in percent
		/// </summary>
		protected readonly int[] m_statBonusPercent = new int[8];

		/// <summary>
		/// Gets/Sets amount of full skill respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RespecAmountAllSkill
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecAmountAllSkill : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecAmountAllSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of single-line respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RespecAmountSingleSkill
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecAmountSingleSkill : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecAmountSingleSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of realm skill respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RespecAmountRealmSkill
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecAmountRealmSkill : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecAmountRealmSkill = value; }
		}

		/// <summary>
		/// Gets/Sets amount of DOL respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RespecAmountDOL
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecAmountDOL : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecAmountDOL = value; }
		}

		/// <summary>
		/// Gets/Sets level respec usage flag
		/// (delegate to PlayerCharacter)
		/// </summary>
		public bool IsLevelRespecUsed
		{
			get { return PlayerCharacter != null ? PlayerCharacter.IsLevelRespecUsed : true; }
			set { if (PlayerCharacter != null) PlayerCharacter.IsLevelRespecUsed = value; }
		}

		/// <summary>
		/// Gets/Sets amount of bought respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RespecBought
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecBought : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecBought = value; }
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
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.AddSpecialisation.YouLearn", skill.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.RemoveSpecialization.YouLose", playerSpec), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return true;
		}

		/// <summary>
		/// Removes the existing specialization from the player, the line instance should be called with GamePlayer.GetSpellLine ONLY and NEVER SkillBase.GetSpellLine!!!!!
		/// </summary>
		/// <param name="line">The spell line to remove</param>
		/// <returns>true if removed</returns>
		private bool RemoveSpellLine(SpellLine line)
		{
			if (!m_spelllines.Contains(line))
				return false;
			lock (m_spelllines.SyncRoot)
			{
				m_spelllines.Remove(line);
			}
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.RemoveSpellLine.YouLose", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			return true;
		}

		/// <summary>
		/// Removes the existing specialization from the player
		/// </summary>
		/// <param name="lineKeyName">The spell line keyname to remove</param>
		/// <returns>true if removed</returns>
		public bool RemoveSpellLine(string lineKeyName)
		{
			SpellLine line = GetSpellLine(lineKeyName);
			if (line == null)
				return false;
			return RemoveSpellLine(line);
		}

		public int RespecAll()
		{
			int specPoints = RespecAllLines(); // Wipe skills and styles.

			RespecAmountAllSkill--; // Decriment players respecs available.

			if (Level == 5)
				IsLevelRespecUsed = true;
			return specPoints;
		}

		public int RespecDOL()
		{
			int specPoints = RespecAllLines(); // Wipe skills and styles.

			RespecAmountDOL--; // Decriment players respecs available.

            return specPoints;
		}

		public int RespecSingle(Specialization specLine)
		{
			int specPoints = RespecSingleLine(specLine); // Wipe skills and styles.
			RespecAmountSingleSkill--; // Decriment players respecs available.
			if (Level == 20 || Level == 40)
			{
				IsLevelRespecUsed = true;
			}
			return specPoints;
		}

		public int RespecRealm()
		{
			int respecPoints = 0;
			foreach (Ability ab in GetAllAbilities())
			{
				if (ab is RealmAbility && ab is RR5RealmAbility == false)
				{
					for (int i = 0; i < ab.Level; i++)
					{
						respecPoints += ((RealmAbility)ab).CostForUpgrade(i);
					}
					RemoveAbility(ab.KeyName);
				}
			}
			RespecAmountRealmSkill--;
			return respecPoints;
		}

		private int RespecAllLines()
		{
			int specPoints = 0;
			IList specList = GetSpecList();
			lock (specList.SyncRoot)
			{
				foreach (Specialization cspec in specList)
				{
					if (cspec.Level < 2)
						continue;
					specPoints += RespecSingleLine(cspec);
				}
			}
			return specPoints;
		}

		/// <summary>
		/// Respec single line
		/// </summary>
		/// <param name="specLine">spec line being respec'd</param>
		/// <returns>Amount of points spent in that line</returns>
		private int RespecSingleLine(Specialization specLine)
		{
			int specPoints = (specLine.Level * (specLine.Level + 1) - 2) / 2;
			// Graveen - autotrain 1.87
			specPoints -= GetAutoTrainPoints(specLine, 0);
			specLine.Level = 1;

			return specPoints;
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
				if (caseSensitive)
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
		/// The best armor level this player can use.
		/// </summary>
		public int BestArmorLevel
		{
			get
			{
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
				return bestLevel;
			}
		}

		/// <summary>
		/// Adds a new Ability to the player
		/// </summary>
		/// <param name="ability"></param>
		/// <param name="sendUpdates"></param>
		public override void AddAbility(Ability ability, bool sendUpdates)
		{
			if (ability == null)
				return;

			if (CharacterClass.ID != 49 &&
				CharacterClass.ID != 23 &&
				CharacterClass.ID != 9 &&
				ability.KeyName == Abilities.DetectHidden)
			{
				return;
			}

			if (CharacterClass.ID != 4
					&& CharacterClass.ID != 9
					&& CharacterClass.ID != 49
					&& CharacterClass.ID != 23
					&& CharacterClass.ID != 58
					&& ability.KeyName == Abilities.Climbing)
			{
				return;
			}

			if ((eCharacterClass)CharacterClass.ID == eCharacterClass.Infiltrator &&
				ability.KeyName == Abilities.Snapshot)
			{
				return;
			}

			Ability oldability = (Ability)m_abilities[ability.KeyName];
			if (oldability == null || oldability.Level < ability.Level)
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.AddAbility.YouLearn", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			base.AddAbility(ability, sendUpdates);
			lock (m_skillList)
			{
				bool found = false;
				foreach (Skill skill in m_skillList)
				{
					if (skill is Ability && (skill as Ability).KeyName == ability.KeyName)
					{
						skill.Level = ability.Level;
						found = true;
					}
				}
				if (!found)
					m_skillList.Add(ability);
			}
		}

		/// <summary>
		/// Removes the existing ability from the player
		/// </summary>
		/// <param name="abilityKeyName">The ability keyname to remove</param>
		/// <returns>true if removed</returns>
		public override bool RemoveAbility(string abilityKeyName)
		{
			if (!base.RemoveAbility(abilityKeyName))
				return false;

			Ability ability = (Ability)m_abilities[abilityKeyName];
			if (ability != null)
			{
				lock (m_skillList.SyncRoot)
				{
					m_skillList.Remove(ability);
				}
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.RemoveAbility.YouLose", ability.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return true;
		}

		public void RemoveAllSkills()
		{
			ArrayList skills = new ArrayList();
			lock (m_skillList.SyncRoot)
			{
				foreach (Skill skill in m_skillList)
					skills.Add(skill);
			}
			foreach (NamedSkill skill in skills)
			{
				m_skillList.Remove(skill);
				m_abilities.Remove(skill.KeyName);
			}
		}

		public void RemoveAllSpecs()
		{
			ArrayList specs = new ArrayList();
			lock (m_specList.SyncRoot)
			{
				foreach (Specialization spec in m_specList)
					specs.Add(spec);
			}
			foreach (Specialization spec in specs)
			{
				m_specList.Remove(spec);
				m_specialization[spec.KeyName] = null;
			}
		}

		public void RemoveAllSpellLines()
		{
			ArrayList lines = new ArrayList();
			lock (m_spelllines.SyncRoot)
			{
				foreach (SpellLine line in m_spelllines)
				{
					lines.Add(line);
				}

				foreach (SpellLine line in lines)
				{
					m_spelllines.Remove(line);
				}
			}
		}

		public void RemoveAllStyles()
		{
			ArrayList styles = new ArrayList();
			lock (m_styles.SyncRoot)
			{
				foreach (Style style in m_styles)
					styles.Add(style);

				foreach (Style style in styles)
					m_styles.Remove(style);
			}
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

			// DW/FW chance
			int specLevel = Math.Max(GetModifiedSpecLevel(Specs.Celtic_Dual), GetModifiedSpecLevel(Specs.Dual_Wield));
			if (specLevel > 0 || GetBaseSpecLevel(Specs.Fist_Wraps) > 0)
			{
				return Util.Chance(25 + (specLevel - 1) * 68 / 100) ? 1 : 0;
			}

			// HtH chance
			specLevel = GetModifiedSpecLevel(Specs.HandToHand);
			InventoryItem attackWeapon = AttackWeapon;
			InventoryItem leftWeapon = (Inventory == null) ? null : Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			if (specLevel > 0 && ActiveWeaponSlot == eActiveWeaponSlot.Standard
				&& attackWeapon != null && attackWeapon.Object_Type == (int)eObjectType.HandToHand &&
				leftWeapon != null && leftWeapon.Object_Type == (int)eObjectType.HandToHand)
			{
				specLevel--;
				int randomChance = Util.Random(99);
				int hitChance = specLevel >> 1;
				if (randomChance < hitChance)
					return 1; // 1 hit = spec/2

				hitChance += specLevel >> 2;
				if (randomChance < hitChance)
					return 2; // 2 hits = spec/4

				hitChance += specLevel >> 4;
				if (randomChance < hitChance)
					return 3; // 3 hits = spec/16

				return 0;
			}

			return 0;
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
			return spec.Level;
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
				if (keyName == GlobalSpellsLines.Combat_Styles_Effect)
				{
					if (CharacterClass.ID == (int)eCharacterClass.Reaver || CharacterClass.ID == (int)eCharacterClass.Heretic)
						return GetModifiedSpecLevel(Specs.Flexible);
					if (CharacterClass.ID == (int)eCharacterClass.Valewalker)
						return GetModifiedSpecLevel(Specs.Scythe);
					if (CharacterClass.ID == (int)eCharacterClass.Savage)
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
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.AddSpellLine.YouLearn", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
				foreach (SpellLine line in m_spelllines)
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
		public virtual IList GetUsableSpellsOfLine(SpellLine line)
		{
			IList spells = new ArrayList();
			Hashtable table_spells = new Hashtable();
			foreach (Spell spell in SkillBase.GetSpellList(line.KeyName))
			{
				if (spell.Level <= line.Level)
				{
					object key;
					if (spell.Group == 0)
					{
						//Give out different versions of spreadheal
						if (spell.SpellType == "SpreadHeal")
							key = spell.SpellType + "+" + spell.Target + "+" + spell.CastTime + "+" + spell.RecastDelay + spell.Radius + spell.Level;
						else
							key = spell.SpellType + "+" + spell.Target + "+" + spell.CastTime + "+" + spell.RecastDelay + spell.Radius;
						if (spell.Radius > 0)
						{
							key = key + "+AOE";
						}
					}
					else
						key = spell.Group;

					if (!table_spells.ContainsKey(key))
					{
						table_spells.Add(key, spell);
					}
					else
					{
						Spell oldspell = (Spell)table_spells[key];
						if (spell.Level > oldspell.Level)
						{
							table_spells[key] = spell;
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
				catch (Exception e)
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
		/// <param name="sendMessages">sends "you learn" messages if true</param>
		public virtual void RefreshSpecDependantSkills(bool sendMessages)
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
						if (styles != null)
						{
							foreach (Style style in styles)
							{
								if (style == null)
									continue;
								if (style.SpecLevelRequirement <= spec.Level)
								{
									if (!m_styles.Contains(style))
									{
										newStyles.Add(style);
										m_styles.Add(style);
									}
								}
							}
						}

						// check abilities
						IList abilities = SkillBase.GetSpecAbilityList(spec.KeyName);
						foreach (Ability ability in abilities)
						{
							if (ability.SpecLevelRequirement <= spec.Level)
							{
								AddAbility(ability);	// add ability cares about all
							}
						}
					}
				}
			}

			if (sendMessages)
			{
				foreach (Style style in newStyles)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.YouLearn", style.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

					string message = null;
					if (Style.eOpening.Offensive == style.OpeningRequirementType)
					{
						switch (style.AttackResultRequirement)
						{
							case Style.eAttackResult.Style:
							case Style.eAttackResult.Hit: // TODO: make own message for hit after styles DB is updated

								Style reqStyle = SkillBase.GetStyleByID(style.OpeningRequirementValue, CharacterClass.ID);
								if (reqStyle == null)
									message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterStyle", "(style " + style.OpeningRequirementValue + " not found)");
								else message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterStyle", reqStyle.Name);

								break;
							case Style.eAttackResult.Miss: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterMissed"); break;
							case Style.eAttackResult.Parry: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterParried"); break;
							case Style.eAttackResult.Block: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterBlocked"); break;
							case Style.eAttackResult.Evade: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterEvaded"); break;
							case Style.eAttackResult.Fumble: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.AfterFumbles"); break;
						}
					}
					else if (Style.eOpening.Defensive == style.OpeningRequirementType)
					{
						switch (style.AttackResultRequirement)
						{
							case Style.eAttackResult.Miss: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetMisses"); break;
							case Style.eAttackResult.Hit: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetHits"); break;
							case Style.eAttackResult.Parry: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetParried"); break;
							case Style.eAttackResult.Block: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetBlocked"); break;
							case Style.eAttackResult.Evade: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetEvaded"); break;
							case Style.eAttackResult.Fumble: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetFumbles"); break;
							case Style.eAttackResult.Style: message = LanguageMgr.GetTranslation(Client, "GamePlayer.RefreshSpec.TargetStyle"); break;
						}
					}

					if (message != null)
						Out.SendMessage(message, eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// updates the levels of all spell lines
		/// specialized spell lines depend from spec levels
		/// base lines depend from player level
		/// </summary>
		/// <param name="sendMessages">sends "You gain power" messages if true</param>
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
							if (sendMessages && line.Level < newSpec)
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UpdateSpellLine.GainPower", line.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnSkillTrained.YouSpend", skill.Level, skill.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnSkillTrained.YouHave", SkillSpecialtyPoints), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client, "GamePlayer.OnSkillTrained.TrainsInVarious", GetName(0, true)), eChatType.CT_System);
			CharacterClass.OnSkillTrained(this, skill);
			RefreshSpecDependantSkills(true);
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
		public override double Effectiveness
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

		#region Realm-/Region-/Bount-/Skillpoints...

		/// <summary>
		/// Gets/sets player bounty points
		/// (delegate to PlayerCharacter)
		/// </summary>
		public long BountyPoints
		{
			get { return PlayerCharacter != null ? PlayerCharacter.BountyPoints : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.BountyPoints = value; }
		}

		/// <summary>
		/// Gets/sets player realm points
		/// (delegate to PlayerCharacter)
		/// </summary>
		public long RealmPoints
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RealmPoints : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RealmPoints = value; }
		}

		/// <summary>
		/// Gets/sets player skill specialty points
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int SkillSpecialtyPoints
		{
			get { return PlayerCharacter != null ? PlayerCharacter.SkillSpecialtyPoints : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.SkillSpecialtyPoints = value; }
		}

		/// <summary>
		/// Gets/sets player realm specialty points
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int RealmSpecialtyPoints
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RealmSpecialtyPoints : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RealmSpecialtyPoints = value; }
		}

		/// <summary>
		/// Gets/sets player realm rank
		/// </summary>
		public int RealmLevel
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RealmLevel : 0; }
			set
			{
				if (PlayerCharacter != null)
					PlayerCharacter.RealmLevel = value;
				CharacterClass.OnRealmLevelUp(this);
			}
		}

		/// <summary>
		/// Holds all realm rank names
		/// sirru mod 20.11.06
		/// </summary>
		public static string[, ,] REALM_RANK_NAMES = new string[,,]
        {
            // Albion
            {
                // Male
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
                    "Lord",
                    "Baronet",
                    "Baron"
                }
                ,
                // Female
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
                    "Lady",
                    "Baronetess",
                    "Baroness"
                }
            }
            ,
            // Midgard
            {
                // Male
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
                    "Herra",
                    "Hersir",
                    "Cian"
                }
                ,
                // Female
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
                    "Fru",
                    "Baronsfru",
                    "Cath"
                }
            }
            ,
            // Hibernia
            {
                // Male
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
                    "Barun",
                    "Ard Tiarna",
                    "Vicomte"
                }
                ,
                // Female
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
                    "Banbharun",
                    "Ard Bantiarna",
                    "Vicomtessa"
                }
            }
        };

		/// <summary>
		/// Gets player realm rank name
		/// sirru mod 20.11.06
		/// </summary>
		public string RealmTitle
		{
			get
			{
				if (Realm == eRealm.None)
					return LanguageMgr.GetTranslation(Client, "GamePlayer.RealmTitle.UnknownRealm");

				int m_RR = PlayerCharacter.RealmLevel / 10;
				return REALM_RANK_NAMES[(int)Realm - 1, PlayerCharacter.Gender, m_RR];
			}
		}

		/// <summary>
		/// Called when this living gains realm points
		/// </summary>
		/// <param name="amount">The amount of realm points gained</param>
		/// <param name="modify">Should we apply the rp modifer</param>
		public void GainRealmPoints(long amount, bool modify)
		{
			if (!GainRP)
				return;

			if (modify)
			{
				//rp rate modifier
				double modifier = ServerProperties.Properties.RP_RATE;
				if (modifier != -1)
					amount = (long)((double)amount * modifier);
			}

			base.GainRealmPoints(amount);
			RealmPoints += amount;
			if (m_guild != null && Client.Account.PrivLevel == 1)
				m_guild.GainRealmPoints(amount);

			if (amount > 0)
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			//"You earn 4 extra realm points!"

			while (RealmPoints >= CalculateRPsFromRealmLevel(RealmLevel + 1) && RealmLevel < 120)
			{
				RealmLevel++;
				RealmSpecialtyPoints++;
				Out.SendUpdatePlayer();
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.GainedLevel"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (RealmLevel % 10 == 0)
				{
					Out.SendUpdatePlayerSkills();
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.GainedRank"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.NewRealmTitle", RealmTitle), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.GainBonus", RealmLevel / 10), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						plr.Out.SendLivingDataUpdate(this, true);
					Notify(GamePlayerEvent.RRLevelUp, this);
				}
				else
					Notify(GamePlayerEvent.RLLevelUp, this);
				if (GameServer.ServerRules.CanGenerateNews(this) && ((RealmLevel >= 20 && RealmLevel % 10 == 0) || RealmLevel >= 60))
				{
					string message = LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.ReachedRank", Name, RealmLevel + 10, LastPositionUpdateZone.Description);
					string newsmessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.GainRealmPoints.ReachedRank", Name, RealmLevel + 10, LastPositionUpdateZone.Description);
					NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
				}
				if (GameServer.ServerRules.CanGenerateNews(this) && RealmPoints >= 1000000 && RealmPoints - amount < 1000000)
				{
					string message = LanguageMgr.GetTranslation(Client, "GamePlayer.GainRealmPoints.Earned", Name, LastPositionUpdateZone.Description);
					string newsmessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.GainRealmPoints.Earned", Name, LastPositionUpdateZone.Description);
					NewsMgr.CreateNews(newsmessage, this.Realm, eNewsType.RvRLocal, true);
				}
			}
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Called when this living gains realm points
		/// </summary>
		/// <param name="amount">The amount of realm points gained</param>
		public override void GainRealmPoints(long amount)
		{
			GainRealmPoints(amount, true);
		}

		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount">The amount of realm points loosed</param>
		public bool RemoveBountyPoints(long amount)
		{
			return RemoveBountyPoints(amount, null);
		}
		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount"></param>
		/// <param name="str"></param>
		/// <returns></returns>
		public bool RemoveBountyPoints(long amount, string str)
		{
			return RemoveBountyPoints(amount, str, eChatType.CT_Say, eChatLoc.CL_SystemWindow);
		}
		/// <summary>
		/// Called when this living buy something with realm points
		/// </summary>
		/// <param name="amount">The amount of realm points loosed</param>
		/// <param name="loc">The chat location</param>
		/// <param name="str">The message</param>
		/// <param name="type">The chat type</param>
		public bool RemoveBountyPoints(long amount, string str, eChatType type, eChatLoc loc)
		{
			if (BountyPoints < amount)
				return false;
			BountyPoints -= amount;
			Out.SendUpdatePoints();
			if (str != null && amount != 0)
				Out.SendMessage(str, type, loc);
			return true;
		}
		/// <summary>
		/// Called when this living gains bounty points
		/// </summary>
		/// <param name="amount">The amount of bounty points gained</param>
		public override void GainBountyPoints(long amount)
		{
			//bp rate modifier
			double modifier = ServerProperties.Properties.BP_RATE;
			if (modifier != -1)
				amount = (long)((double)amount * modifier);

			base.GainBountyPoints(amount);
			BountyPoints += amount;
			if (m_guild != null && Client.Account.PrivLevel == 1)
				m_guild.GainBountyPoints(amount);
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainBountyPoints.YouGet", amount.ToString()), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Holds realm points needed for special realm level
		/// </summary>
		public static readonly long[] REALMPOINTS_FOR_LEVEL =
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
				9111713,	// for level 101
				10114001,	// for level 102
				11226541,	// for level 103
				12461460,	// for level 104
				13832221,	// for level 105
				15353765,	// for level 106
				17042680,	// for level 107
				18917374,	// for level 108
				20998286,	// for level 109
				23308097,	// for level 110
				25871988,	// for level 111
				28717906,	// for level 112
				31876876,	// for level 113
				35383333,	// for level 114
				39275499,	// for level 115
				43595804,	// for level 116
				48391343,	// for level 117
				53714390,	// for level 118
				59622973,	// for level 119
				66181501,	// for level 120
			};

		/// <summary>
		/// Calculates amount of RealmPoints needed for special realm level
		/// </summary>
		/// <param name="realmLevel">realm level</param>
		/// <returns>amount of realm points</returns>
		protected virtual long CalculateRPsFromRealmLevel(int realmLevel)
		{
			if (realmLevel < REALMPOINTS_FOR_LEVEL.Length)
				return REALMPOINTS_FOR_LEVEL[realmLevel];

			// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=0
			return (long)(25.0 / 3.0 * (realmLevel * realmLevel * realmLevel) - 25.0 / 2.0 * (realmLevel * realmLevel) + 25.0 / 6.0 * realmLevel);
		}

		/// <summary>
		/// Calculates realm level from realm points. SLOW.
		/// </summary>
		/// <param name="realmPoints">amount of realm points</param>
		/// <returns>realm level: RR5L3 = 43, RR1L2 = 2; capped at 99</returns>
		protected virtual int CalculateRealmLevelFromRPs(long realmPoints)
		{
			if (realmPoints == 0)
				return 0;

			int i = REALMPOINTS_FOR_LEVEL.Length - 1;
			for (; i > 0; i--)
			{
				if (REALMPOINTS_FOR_LEVEL[i] <= realmPoints)
					break;
			}

			if (i > 120)
				return 120;
			return i;


			// thanks to Linulo from http://daoc.foren.4players.de/viewtopic.php?t=40839&postdays=0&postorder=asc&start=30
			//			double z = Math.Pow(1620.0 * realmPoints + 15.0 * Math.Sqrt(-1875.0 + 11664.0 * realmPoints*realmPoints), 1.0/3.0);
			//			double rr = z / 30.0 + 5.0 / 2.0 / z + 0.5;
			//			return Math.Min(99, (int)rr);
		}

		/// <summary>
		/// Realm point value of this player
		/// </summary>
		public override int RealmPointsValue
		{
			get
			{
				// http://www.camelotherald.com/more/2275.shtml
				// new 1.81D formula
				// Realm point value = (level - 20)squared + (realm rank level x 5) + (champion level x 10) + (master level (squared)x 5)
				//we use realm level 1L0 = 0, mythic uses 1L0 = 10, so we + 10 the realm level
				int level = Math.Max(0, Level - 20);
				if (level == 0)
					return Math.Max(1, (RealmLevel + 10) * 5);

				return Math.Max(1, level * level + (RealmLevel + 10) * 5);
			}
		}

		/// <summary>
		/// Bounty point value of this player
		/// </summary>
		public override int BountyPointsValue
		{
			// TODO: correct formula!
			get { return (int)(1 + Level * 0.6); }
		}

		/// <summary>
		/// Returns the amount of experience this player is worth
		/// </summary>
		public override long ExperienceValue
		{
			get
			{
				return base.ExperienceValue * 4;
			}
		}

		public static readonly int[] prcRestore =
		{
			// http://www.silicondragon.com/Gaming/DAoC/Misc/XPs.htm
			0,//0
			0,//1
			0,//2
			0,//3
			0,//4
			0,//5
			33,//6
			53,//7
			82,//8
			125,//9
			188,//10
			278,//11
			352,//12
			443,//13
			553,//14
			688,//15
			851,//16
			1048,//17
			1288,//18
			1578,//19
			1926,//20
			2347,//21
			2721,//22
			3146,//23
			3633,//24
			4187,//25
			4820,//26
			5537,//27
			6356,//28
			7281,//29
			8337,//30
			9532,//31 - from logs
			10886,//32 - from logs
			12421,//33 - from logs
			14161,//34
			16131,//35
			18360,//36 - recheck
			19965,//37 - guessed
			21857,//38
			23821,//39
			25928,//40 - guessed
			28244,//41
			30731,//42
			33411,//43
			36308,//44
			39438,//45
			42812,//46
			46454,//47
			50385,//48
			54625,//49
			59195,//50
		};

		/// <summary>
		/// Money value of this player
		/// </summary>
		public override long MoneyValue
		{
			get
			{
				return 3 * prcRestore[Level < GamePlayer.prcRestore.Length ? Level : GamePlayer.prcRestore.Length - 1];
			}
		}

		#endregion

		#region Level/Experience

		/// <summary>
		/// The maximum level a player can reach
		/// </summary>
		public const int MAX_LEVEL = 50;
		/// <summary>
		/// A table that holds the required XP/Level
		/// </summary>
		public static readonly long[] XPLevel =
			{
				0, // xp to level 1
				50, // xp to level 2
				200, // xp to level 3
				850, // xp to level 4
				2300, // xp to level 5
				6350, // xp to level 6
				16000, // xp to level 7
				38000, // xp to level 8
				89000, // xp to level 9
				204000, // xp to level 10
				460000, // xp to level 11
				834806, // xp to level 12
				1383554, // xp to level 13
				2186968, // xp to level 14
				3363235, // xp to level 15
				5085391, // xp to level 16
				7606775, // xp to level 17
				11298297, // xp to level 18
				16703001, // xp to level 19
				24615952, // xp to level 20
				36201189, // xp to level 21
				53162970, // xp to level 22
				75544173, // xp to level 23
				105076348, // xp to level 24
				144044287, // xp to level 25
				195462790, // xp to level 26
				263309912, // xp to level 27
				352834727, // xp to level 28
				470963427, // xp to level 29
				626835182, // xp to level 30
				832509196, // xp to level 31
				1103897684, // xp to level 32
				1461996941, // xp to level 33
				1934511744, // xp to level 34
				2557998764, // xp to level 35
				3352670771, // xp to level 36
				4365528423, // xp to level 37
				5656476907, // xp to level 38
				7301869018, // xp to level 39
				9399021018, // xp to level 40
				12925996216, // xp to level 41
				18857637817, // xp to level 42
				28833430136, // xp to level 43
				40292606362, // xp to level 44
				53919940262, // xp to level 45
				70125662695, // xp to level 46
				89397623115, // xp to level 47
				112315975568, // xp to level 48
				139570643368, // xp to level 49
				171982088233, // xp to level 50
				210526009074, // xp to level 51
			};

		/// <summary>
		/// Holds how many XP this player has
		/// </summary>
		protected long m_currentXP;

		/// <summary>
		/// Gets or sets the current xp of this player
		/// (TODO: Exp is setted in GainExp() and therefore NOT delegated to PlayerCharacter)  (VaNaTiC)
		/// </summary>
		public virtual long Experience
		{
			get { return m_currentXP; }
			//			set
			//			{
			//				m_currentXP=value;
			//				m_character.Experience=m_currentXP;
			//			}
		}

		/// <summary>
		/// Returns the xp that are needed for the next level
		/// </summary>
		public virtual long ExperienceForNextLevel
		{
			get
			{
				return GameServer.ServerRules.GetExperienceForLevel(Level + 1);
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
			get { return 1 + ExperienceForCurrentLevel + (ExperienceForNextLevel - ExperienceForCurrentLevel) / 2; }
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
				return (ushort)(1000 * (Experience - ExperienceForCurrentLevel) / (ExperienceForNextLevel - ExperienceForCurrentLevel));
			}
		}

		/// <summary>
		/// Called whenever this player gains experience
		/// </summary>
		/// <param name="expTotal"></param>
		/// <param name="expCampBonus"></param>
		/// <param name="expGroupBonus"></param>
		/// <param name="expOutpostBonus"></param>
		/// <param name="sendMessage"></param>
		public void GainExperience(long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage)
		{
			GainExperience(expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, true);
		}

		/// <summary>
		/// Called whenever this player gains experience
		/// </summary>
		/// <param name="expTotal"></param>
		/// <param name="expCampBonus"></param>
		/// <param name="expGroupBonus"></param>
		/// <param name="expOutpostBonus"></param>
		/// <param name="sendMessage"></param>
		/// <param name="allowMultiply"></param>
		public override void GainExperience(long expTotal, long expCampBonus, long expGroupBonus, long expOutpostBonus, bool sendMessage, bool allowMultiply)
		{
			if (!GainXP && expTotal > 0)
				return;

			//xp rate modifier
			if (allowMultiply)
			{
				//we only want to modify the base rate, not the group or camp bonus
				expTotal -= expGroupBonus;
				expTotal -= expCampBonus;
				expTotal -= expOutpostBonus;
				if (this.CurrentRegion.IsRvR)
					expTotal = (long)(expTotal * ServerProperties.Properties.RvR_XP_RATE);
				else
					expTotal = (long)(expTotal * ServerProperties.Properties.XP_RATE);
				expTotal += expOutpostBonus;
				expTotal += expGroupBonus;
				expTotal += expCampBonus;
			}

			// Get Champion Experience too
			GainChampionExperience(expTotal);

			//catacombs characters get 50% boost if they are elligable for slash level
			switch ((eCharacterClass)CharacterClass.ID)
			{
				case eCharacterClass.Heretic:
				case eCharacterClass.Valkyrie:
				case eCharacterClass.Warlock:
				case eCharacterClass.Bainshee:
				case eCharacterClass.Vampiir:
				case eCharacterClass.Mauler_Alb:
				case eCharacterClass.Mauler_Hib:
				case eCharacterClass.Mauler_Mid:
					{
						//we don't want to allow catacombs classes to use free levels and
						//have a 50% bonus
						if (!ServerProperties.Properties.ALLOW_CATA_SLASH_LEVEL && CanUseSlashLevel && Level < 20)
						{
							expTotal = (long)(expTotal * 1.5);
						}
						break;
					}
			}

			base.GainExperience(expTotal, expCampBonus, expGroupBonus, expOutpostBonus, sendMessage, allowMultiply);

			if (IsLevelSecondStage)
			{
				if (Experience + expTotal < ExperienceForCurrentLevelSecondStage)
				{
					expTotal = ExperienceForCurrentLevelSecondStage - m_currentXP;
				}
			}
			else if (Experience + expTotal < ExperienceForCurrentLevel)
			{
				expTotal = ExperienceForCurrentLevel - m_currentXP;
			}

			if (sendMessage && expTotal > 0)
			{
				System.Globalization.NumberFormatInfo format = System.Globalization.NumberFormatInfo.InvariantInfo;
				string totalExpStr = expTotal.ToString("N0", format);
				string expCampBonusStr = "";
				string expGroupBonusStr = "";
				string expOutpostBonusStr = "";

				if (expCampBonus > 0)
				{
					expCampBonusStr = LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.CampBonus", expCampBonus.ToString("N0", format));
				}
				if (expGroupBonus > 0)
				{
					expGroupBonusStr = LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.GroupBonus", expGroupBonus.ToString("N0", format));
				}
				if (expOutpostBonus > 0)
				{
					expOutpostBonusStr = LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.OutpostBonus", expOutpostBonus.ToString("N0", format));
				}

				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.YouGet", totalExpStr) + expCampBonusStr + expGroupBonusStr, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			m_currentXP += expTotal; // force usage of this method, Experience property cannot be set
			PlayerCharacter.Experience = m_currentXP;

			if (expTotal >= 0)
			{
				//Level up
				if (Level >= 5 && CharacterClass.BaseName == CharacterClass.Name)
				{
					if (expTotal > 0)
					{
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.CannotRaise"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainExperience.TalkToTrainer"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}
				else if (Level >= 40 && Level < MAX_LEVEL && !IsLevelSecondStage && Experience >= ExperienceForCurrentLevelSecondStage)
				{
					OnLevelSecondStage();
					Notify(GamePlayerEvent.LevelSecondStage, this);
					SaveIntoDatabase(); // save char on levelup
				}
				else if (Level < MAX_LEVEL && Experience >= ExperienceForNextLevel)
				{
					Level++;
					SaveIntoDatabase(); // save char on levelup
				}
			}
			Out.SendUpdatePoints();
		}

		/// <summary>
		/// Gets or sets the level of the player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override byte Level
		{
			get { return PlayerCharacter != null ? (byte)PlayerCharacter.Level : base.Level; }
			set
			{
				int oldLevel = Level;
				base.Level = value;
				if (PlayerCharacter != null)
					PlayerCharacter.Level = value;
				if (oldLevel > 0)
				{
					if (value > oldLevel)
					{
						OnLevelUp(oldLevel);
						Notify(GamePlayerEvent.LevelUp, this);
					}
					else
					{
						//update the mob colours
						Out.SendLevelUpSound();
					}
				}
			}
		}

		/// <summary>
		/// Is this player in second stage of current level
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual bool IsLevelSecondStage
		{
			get { return PlayerCharacter != null ? PlayerCharacter.IsLevelSecondStage : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.IsLevelSecondStage = value; }
		}

		/// <summary>
		/// Called when this player levels
		/// </summary>
		/// <param name="previouslevel"></param>
		public virtual void OnLevelUp(int previouslevel)
		{
			IsLevelSecondStage = false;

			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.YouRaise", Level), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			if (Experience < GameServer.ServerRules.GetExperienceForLevel(Level + 1))
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.YouAchived", Level), eChatType.CT_ScreenCenter, eChatLoc.CL_SystemWindow);
			Out.SendPlayerFreeLevelUpdate();
			if (FreeLevelState == 2)
				Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.FreeLevelEligible"));

			switch (Level)
			{
				// full respec on level 5 since 1.70
				case 5:
					RespecAmountAllSkill++;
					IsLevelRespecUsed = false;
					break;
				case 6:
					if (IsLevelRespecUsed) break;
					RespecAmountAllSkill--;
					break;

				// single line respec
				case 20:
				case 40:
					{
						RespecAmountSingleSkill++; // Give character their free respecs at 20 and 40
						IsLevelRespecUsed = false;
						break;
					}
				case 21:
				case 41:
					{
						if (IsLevelRespecUsed) break;
						RespecAmountSingleSkill--; // Remove free respecs if it wasn't used
						break;
					}
				case 50:
					{
						if (GameServer.ServerRules.CanGenerateNews(this))
						{
							string message = LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.Reached", Name, Level, LastPositionUpdateZone.Description);
							string newsmessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.OnLevelUp.Reached", Name, Level, LastPositionUpdateZone.Description);
							NewsMgr.CreateNews(newsmessage, Realm, eNewsType.PvE, true);
						}
						break;
					}
			}

            // Graveen: give a DOL respec on the GIVE_DOL_RESPEC_ON_LEVELS levels
            Byte level_respec;
            foreach (string str in ServerProperties.Properties.GIVE_DOL_RESPEC_AT_LEVEL.Split(';'))
            {
                try
                {
                    level_respec = Convert.ToByte(str);
                }
                catch (Exception)
                {
                    level_respec = 0;
                }

                if (Level == level_respec)
                {
                    RespecAmountDOL++;
                    Out.SendMessage("As you reached level " + Level + ", you are awarded a DOL (full) respec!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                }
            }
        
            //level 20 changes realm title and gives 1 realm skill point
            if (Level == 20)
                  GainRealmPoints(0);

            // Adjust stats
			bool statsChanged = false;
			for (int i = Level; i > previouslevel; i--)
			{
				if (CharacterClass.PrimaryStat != eStat.UNDEFINED)
				{
					ChangeBaseStat(CharacterClass.PrimaryStat, 1);
					statsChanged = true;
				}
				if (CharacterClass.SecondaryStat != eStat.UNDEFINED && ((i - 6) % 2 == 0))
				{ // base level to start adding stats is 6
					ChangeBaseStat(CharacterClass.SecondaryStat, 1);
					statsChanged = true;
				}
				if (CharacterClass.TertiaryStat != eStat.UNDEFINED && ((i - 6) % 3 == 0))
				{ // base level to start adding stats is 6
					ChangeBaseStat(CharacterClass.TertiaryStat, 1);
					statsChanged = true;
				}
			}

			if (statsChanged)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.StatRaise"), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			CharacterClass.OnLevelUp(this);
			UpdateSpellLineLevels(true);
			RefreshSpecDependantSkills(true);

			// Echostorm - Code for display of new title on level up
			// Get old and current rank titles
			string currenttitle = CharacterClass.GetTitle(Level);

			// check for difference
			if (CharacterClass.GetTitle(previouslevel) != currenttitle)
			{
				// Inform player of new title.
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.AttainedRank", currenttitle), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			// spec points 
			int specpoints = 0;
			for (int i = Level; i > previouslevel; i--)
			{
				if (i <= 5) specpoints += i; //start levels
				else specpoints += CharacterClass.SpecPointsMultiplier * i / 10; //spec levels
			}
			if (specpoints > 0)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.YouGetSpec", specpoints), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			SkillSpecialtyPoints += specpoints;

			// graveen 1.87 Autotrain
			foreach (Specialization spec in GetSpecList())
				SkillSpecialtyPoints += GetAutoTrainPoints(spec, 1);

			// old hp
			int oldhp = CalculateMaxHealth(previouslevel, GetBaseStat(eStat.CON));

			// old power
			int oldpow = 0;
			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				oldpow = CalculateMaxMana(previouslevel, GetBaseStat(CharacterClass.ManaStat));
			}

			// hp upgrade
			int newhp = CalculateMaxHealth(Level, GetBaseStat(eStat.CON));
			if (oldhp > 0 && oldhp < newhp)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.HitsRaise", (newhp - oldhp)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			// power upgrade
			if (CharacterClass.ManaStat != eStat.UNDEFINED)
			{
				int newpow = CalculateMaxMana(Level, GetBaseStat(CharacterClass.ManaStat));
				if (newpow > 0 && oldpow < newpow)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.PowerRaise", (newpow - oldpow)), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}

			if (IsAlive)
			{
				// workaround for starting regeneration
				StartHealthRegeneration();
				StartPowerRegeneration();
			}

			if (PlayerCharacter != null)
				PlayerCharacter.DeathCount = 0;

			if (Group != null)
			{
				Group.UpdateGroupWindow();
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
			if (ObjectState == eObjectState.Active)
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					player.Out.SendEmoteAnimation(this, eEmote.LvlUp);
				}
			}

			// Reset taskDone per level.
			if (Task != null)
			{
				Task.TasksDone = 0;
				Task.SaveIntoDatabase();
			}
		}

		/// <summary>
		/// Called when this player reaches second stage of the current level
		/// </summary>
		public virtual void OnLevelSecondStage()
		{
			IsLevelSecondStage = true;

			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.SecondStage", Level), eChatType.CT_Important, eChatLoc.CL_SystemWindow);

			// spec points
			int specpoints = CharacterClass.SpecPointsMultiplier * Level / 20;
			if (specpoints > 0)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnLevelUp.YouGetSpec", specpoints), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			SkillSpecialtyPoints += specpoints;
			//death penalty reset on mini-ding
			if (PlayerCharacter != null)
				PlayerCharacter.DeathCount = 0;

			if (Group != null)
			{
				Group.UpdateGroupWindow();
			}
			Out.SendUpdatePlayer(); // Update player level
			Out.SendCharStatsUpdate(); // Update Stats and MaxHitpoints
			Out.SendUpdatePlayerSkills();
			Out.SendUpdatePoints();
			UpdatePlayerStatus();
		}

		/// <summary>
		/// Calculate the Autotrain points.
		/// </summary>
		/// <param name="spec">Specialization</param>
		/// <param name="mode">various AT related calculations (amount of points, level of AT...)</param>
		public virtual int GetAutoTrainPoints(Specialization spec, int Mode)
		{
			int max_autotrain = Level / 4;
			if (max_autotrain == 0) max_autotrain = 1;

			foreach (string autotrainKey in CharacterClass.AutoTrainableSkills())
			{
				if (autotrainKey == spec.KeyName)
					switch (Mode)
					{
						case 0:// return sum of all AT points in the spec
							{
								int pts_to_refund = Math.Min(max_autotrain, spec.Level);
								return ((pts_to_refund * (pts_to_refund + 1) - 2) / 2);
							}
						case 1: // return max AT level + message
							{
								if (Level % 4 == 0)
									if (spec.Level >= max_autotrain)
										return max_autotrain;
									else
										Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, true, LanguageMgr.GetTranslation(Client, "PlayerClass.OnLevelUp.Autotrain", spec.Name, max_autotrain));
								return 0;
							}
                        case 2: // return next free points due to AT change on levelup
							{
								if (spec.Level < max_autotrain)
									return (spec.Level + 1);
								else
									return 0;
							}
                        case 3: // return sum of all free AT points 
                            {
                                if (spec.Level < max_autotrain) 
                                    return (((max_autotrain * ( max_autotrain + 1) - 2) / 2) - ((spec.Level  * ( spec.Level  + 1) - 2) / 2));
                                else
                                    return ((max_autotrain * ( max_autotrain + 1) - 2) / 2);
                            }
                        case 4: // spec is autotrainable
                            {
                                return 1;
                            }
					}
			}
			return 0;
		}
		#endregion

		#region Combat
		/// <summary>
		/// The time someone can hold a ranged attack before tiring
		/// </summary>
		internal const string RANGE_ATTACK_HOLD_START = " RangeAttackHoldStart";
		/// <summary>
		/// Endurance used for normal range attack
		/// </summary>
		public const int RANGE_ATTACK_ENDURANCE = 5;
		/// <summary>
		/// Endurance used for critical shot
		/// </summary>
		public const int CRITICAL_SHOT_ENDURANCE = 10;

		/// <summary>
		/// Holds the Style that this player wants to use next
		/// </summary>
		protected Style m_nextCombatStyle;
		/// <summary>
		/// Holds the backup style for the style that the player wants to use next
		/// </summary>
		protected Style m_nextCombatBackupStyle;
		/// <summary>
		/// Holds the cancel style flag
		/// </summary>
		protected bool m_cancelStyle;
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
		/// Gets or Sets the cancel style flag
		/// (delegate to PlayerCharacter)
		/// </summary>
		public bool CancelStyle
		{
			get { return PlayerCharacter != null ? PlayerCharacter.CancelStyle : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.CancelStyle = value; }
		}
		/// <summary>
		/// Decides which style living will use in this moment
		/// </summary>
		/// <returns>Style to use or null if none</returns>
		protected override Style GetStyleToUse()
		{
			InventoryItem weapon;
			if (NextCombatStyle == null) return null;
			if (NextCombatStyle.WeaponTypeRequirement == (int)eObjectType.Shield)
				weapon = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			else weapon = AttackWeapon;

			if (StyleProcessor.CanUseStyle(this, NextCombatStyle, weapon))
				return NextCombatStyle;

			if (NextCombatBackupStyle == null) return NextCombatStyle;

			return NextCombatBackupStyle;
		}

		/// <summary>
		/// Gets/Sets safety flag
		/// (delegate to PlayerCharacter)
		/// </summary>
		public bool SafetyFlag
		{
			get { return PlayerCharacter != null ? PlayerCharacter.SafetyFlag : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.SafetyFlag = value; }
		}

		/// <summary>
		/// Sets/gets the living's cloak hood state
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override bool IsCloakHoodUp
		{
			get { return PlayerCharacter != null ? PlayerCharacter.IsCloakHoodUp : base.IsCloakHoodUp; }
			set
			{
				//base.IsCloakHoodUp = value; // only needed if some special code will be added in base-property in future
				PlayerCharacter.IsCloakHoodUp = value;

				Out.SendInventoryItemsUpdate(null);
				UpdateEquipmentAppearance();

				if (value)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsCloakHoodUp.NowWear"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsCloakHoodUp.NoLongerWear"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// Sets/gets the living's cloak visible state
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override bool IsCloakInvisible
		{
			get
			{
				return PlayerCharacter != null ? PlayerCharacter.IsCloakInvisible : base.IsCloakInvisible;
			}
			set
			{
				PlayerCharacter.IsCloakInvisible = value;

				Out.SendInventoryItemsUpdate(null);
				UpdateEquipmentAppearance();

				if (value)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsCloakInvisible.Invisible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsCloakInvisible.Visible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// Sets/gets the living's helm visible state
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override bool IsHelmInvisible
		{
			get
			{
				return PlayerCharacter != null ? PlayerCharacter.IsHelmInvisible : base.IsHelmInvisible;
			}
			set
			{
				PlayerCharacter.IsHelmInvisible = value;

				Out.SendInventoryItemsUpdate(null);
				UpdateEquipmentAppearance();

				if (value)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsHelmInvisible.Invisible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsHelmInvisible.Visible"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		/// <summary>
		/// Gets or sets the players SpellQueue option
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual bool SpellQueue
		{
			get { return PlayerCharacter != null ? PlayerCharacter.SpellQueue : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.SpellQueue = value; }
		}


		/// <summary>
		/// Switches the active weapon to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public override void SwitchWeapon(eActiveWeaponSlot slot)
		{
			//When switching weapons, attackmode is removed!
			if (AttackState)
				StopAttack();

			if (CurrentSpellHandler != null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SwitchWeapon.SpellCancelled"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				StopCurrentSpellcast();
			}

			switch (slot)
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



			InventoryItem[] oldActiveSlots = new InventoryItem[4];
			InventoryItem[] newActiveSlots = new InventoryItem[4];
			InventoryItem rightHandSlot = Inventory.GetItem(eInventorySlot.RightHandWeapon);
			InventoryItem leftHandSlot = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
			InventoryItem twoHandSlot = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			InventoryItem distanceSlot = Inventory.GetItem(eInventorySlot.DistanceWeapon);

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
			for (int i = 0; i < 4; i++)
			{
				if (oldActiveSlots[i] != null && newActiveSlots[i] == null)
					Notify(PlayerInventoryEvent.ItemUnequipped, Inventory, new ItemUnequippedArgs(oldActiveSlots[i], oldActiveSlots[i].SlotPosition));
			}

			// equip new active items
			for (int i = 0; i < 4; i++)
			{
				if (newActiveSlots[i] != null && oldActiveSlots[i] == null)
					Notify(PlayerInventoryEvent.ItemEquipped, Inventory, new ItemEquippedArgs(newActiveSlots[i], newActiveSlots[i].SlotPosition));
			}

			if (ObjectState == eObjectState.Active)
			{
				//Send new wield info, no items updated
				Out.SendInventorySlotsUpdate(null);
				// Update active weapon appearence (has to be done with all
				// equipment in the packet else player is naked)
				UpdateEquipmentAppearance();
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
			if (args == null) return;

			switch (args.AttackData.AttackResult)
			{
				case eAttackResult.HitUnstyled:
				case eAttackResult.Missed:
				case eAttackResult.Blocked:
				case eAttackResult.Parried:
				case eAttackResult.Evaded:
				case eAttackResult.HitStyle:
				case eAttackResult.Fumbled:
					// remove an arrow and endurance
					ItemTemplate ammoTemplate = RangeAttackAmmo;
					if (ammoTemplate is InventoryItem)
					{
						Inventory.RemoveCountFromStack((InventoryItem)ammoTemplate, 1);
					}
					if (RangeAttackType == eRangeAttackType.Critical)
						Endurance -= CRITICAL_SHOT_ENDURANCE;
					else if (RangeAttackType == eRangeAttackType.RapidFire && GetAbilityLevel(Abilities.RapidFire) == 1)
						Endurance -= 2 * RANGE_ATTACK_ENDURANCE;
					else Endurance -= RANGE_ATTACK_ENDURANCE;
					break;
			}
		}

		/// <summary>
		/// Starts a melee attack with this player
		/// </summary>
		/// <param name="attackTarget">the target to attack</param>
		public override void StartAttack(GameObject attackTarget)
		{
			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.YouCantCombat"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			// Necromancer with summoned pet cannot attack
			if (ControlledNpc != null)
				if (ControlledNpc.Body != null)
					if (ControlledNpc.Body is NecromancerPet)
					{
						Out.SendMessage("You cannot enter combat in shade mode!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						return;
					}

			//			if(IsShade)
			//			{
			//				Out.SendMessage("You cannot enter combat mode in shade mode!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			//				return;
			//			}
			if (IsStunned)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CantAttackStunned"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
			if (IsMezzed)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CantAttackmesmerized"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			long vanishTimeout = TempProperties.getLongProperty(VanishEffect.VANISH_BLOCK_ATTACK_TIME_KEY, 0);
			if (vanishTimeout > 0 && vanishTimeout > CurrentRegion.Time)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.YouMustWaitAgain", (vanishTimeout - CurrentRegion.Time + 1000) / 1000), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			long VanishTick = this.TempProperties.getLongProperty(VanishEffect.VANISH_BLOCK_ATTACK_TIME_KEY, 0);
			long changeTime = this.CurrentRegion.Time - VanishTick;
			if (changeTime < 30000 && VanishTick > 0)
			{
				this.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.YouMustWait", ((30000 - changeTime) / 1000).ToString()), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsOnHorse)
				IsOnHorse = false;

			if (IsDisarmed)
			{
				Out.SendMessage("You are disarmed and cannot attack!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsSitting)
			{
				Sit(false);
			}
			if (AttackWeapon == null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CannotWithoutWeapon"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
			if (AttackWeapon.Object_Type == (int)eObjectType.Instrument)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CannotMelee"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				return;
			}
			//			if(attackTarget!=null && attackTarget is GamePlayer && ((GamePlayer)attackTarget).IsShade)
			//			{
			//				Out.SendMessage("You cannot attack shaded player!", eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			//				return;
			//			}
			if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
			{
				// Check arrows for ranged attack
				if (RangeAttackAmmo == null)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.SelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}
				// Check if selected ammo is compatible for ranged attack
				if (!CheckRangedAmmoCompatibilityWithActiveWeapon())
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
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

						if (effect is RapidFireEffect)
						{
							RangeAttackType = eRangeAttackType.RapidFire;
							break;
						}

						if (effect is TrueshotEffect)
						{
							RangeAttackType = eRangeAttackType.Long;
							break;
						}
					}
				}

				if (RangeAttackType == eRangeAttackType.Critical && Endurance < CRITICAL_SHOT_ENDURANCE)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.TiredShot"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}

				if (Endurance < RANGE_ATTACK_ENDURANCE)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.TiredUse", AttackWeapon.Name), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
					return;
				}

				if (IsStealthed)
				{
					/*
					 * -Chance to unstealth while nocking an arrow = stealth spec / level
					 * -Chance to unstealth nocking a crit = stealth / level  0.20
					 */
					int stealthSpec = GetModifiedSpecLevel(Specs.Stealth);
					int stayStealthed = stealthSpec * 100 / Level;
					if (RangeAttackType == eRangeAttackType.Critical)
						stayStealthed -= 20;

					if (!Util.Chance(stayStealthed))
						Stealth(false);
				}
			}
			else
			{
				if (attackTarget == null)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CombatNoTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				else
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.CombatTarget", attackTarget.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
			}
			// vampiir
			if (CharacterClass is PlayerClass.ClassVampiir)
			{
				GameSpellEffect removeEffect = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
				if (removeEffect != null)
					removeEffect.Cancel(false);
			}
			else
			{
				// Bard RR5 ability must drop when the player starts a melee attack
				IGameEffect DreamweaverRR5 = EffectList.GetOfType(typeof(DreamweaverEffect));
				if (DreamweaverRR5 != null)
					DreamweaverRR5.Cancel(false);
			}
			base.StartAttack(attackTarget);

			if (IsCasting && !m_runningSpellHandler.Spell.Uninterruptible)
			{
				StopCurrentSpellcast();
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.SpellCancelled"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
			}

			//Clear styles
			NextCombatStyle = null;
			NextCombatBackupStyle = null;

			if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
			{
				Out.SendAttackMode(AttackState);
			}
			else
			{
				TempProperties.setProperty(RANGE_ATTACK_HOLD_START, 0L);

				string typeMsg = "shot";
				if (AttackWeapon.Object_Type == (int)eObjectType.Thrown)
					typeMsg = "throw";

				string targetMsg = "";
				if (attackTarget != null)
				{
					if (WorldMgr.CheckDistance(this, attackTarget, AttackRange))
						targetMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.TargetInRange");
					else
						targetMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.TargetOutOfRange");
				}

				int speed = AttackSpeed(AttackWeapon) / 100;
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.StartAttack.YouPrepare", typeMsg, speed / 10, speed % 10, targetMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
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
			if (IsAlive)
				Out.SendAttackMode(AttackState);
		}


		/// <summary>
		/// Switches the active quiver slot to another one
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="forced"></param>
		public virtual void SwitchQuiver(eActiveQuiverSlot slot, bool forced)
		{
			if (slot != eActiveQuiverSlot.None)
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

				if (Inventory.GetItem(updatedSlot) != null && (ActiveQuiverSlot != slot || forced))
				{
					ActiveQuiverSlot = slot;
					//GamePlayer.SwitchQuiver.ShootWith:		You will shoot with: {0}.
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SwitchQuiver.ShootWith", Inventory.GetItem(updatedSlot).GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					ActiveQuiverSlot = eActiveQuiverSlot.None;
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SwitchQuiver.NoMoreAmmo"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				Out.SendInventorySlotsUpdate(new int[] { (int)updatedSlot });
			}
			else
			{
				if (Inventory.GetItem(eInventorySlot.FirstQuiver) != null)
					SwitchQuiver(eActiveQuiverSlot.First, true);
				else if (Inventory.GetItem(eInventorySlot.SecondQuiver) != null)
					SwitchQuiver(eActiveQuiverSlot.Second, true);
				else if (Inventory.GetItem(eInventorySlot.ThirdQuiver) != null)
					SwitchQuiver(eActiveQuiverSlot.Third, true);
				else if (Inventory.GetItem(eInventorySlot.FourthQuiver) != null)
					SwitchQuiver(eActiveQuiverSlot.Fourth, true);
				else
				{
					ActiveQuiverSlot = eActiveQuiverSlot.None;
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SwitchQuiver.NotUseQuiver"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			InventoryItem weapon = AttackWeapon;
			if (weapon != null)
			{
				switch ((eObjectType)weapon.Object_Type)
				{
					case eObjectType.Crossbow:
					case eObjectType.Longbow:
					case eObjectType.CompositeBow:
					case eObjectType.RecurvedBow:
					case eObjectType.Fired:
						{
							if (ActiveQuiverSlot != eActiveQuiverSlot.None)
							{
								InventoryItem ammo = null;
								switch (ActiveQuiverSlot)
								{
									case eActiveQuiverSlot.Fourth: ammo = Inventory.GetItem(eInventorySlot.FourthQuiver); break;
									case eActiveQuiverSlot.Third: ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
									case eActiveQuiverSlot.Second: ammo = Inventory.GetItem(eInventorySlot.SecondQuiver); break;
									case eActiveQuiverSlot.First: ammo = Inventory.GetItem(eInventorySlot.FirstQuiver); break;
								}

								if (ammo == null) return false;

								if (weapon.Object_Type == (int)eObjectType.Crossbow)
									return ammo.Object_Type == (int)eObjectType.Bolt;
								return ammo.Object_Type == (int)eObjectType.Arrow;
							}
						} break;
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
		protected override ItemTemplate RangeAttackAmmo
		{
			get
			{
				//TODO: ammo should be saved on start of every range attack and used here
				InventoryItem ammo = null;//(InventoryItem)m_rangeAttackArrows.Target;

				InventoryItem weapon = AttackWeapon;
				if (weapon != null)
				{
					switch (weapon.Object_Type)
					{
						case (int)eObjectType.Thrown: ammo = Inventory.GetItem(eInventorySlot.DistanceWeapon); break;
						case (int)eObjectType.Crossbow:
						case (int)eObjectType.Longbow:
						case (int)eObjectType.CompositeBow:
						case (int)eObjectType.RecurvedBow:
						case (int)eObjectType.Fired:
							{
								switch (ActiveQuiverSlot)
								{
									case eActiveQuiverSlot.First: ammo = Inventory.GetItem(eInventorySlot.FirstQuiver); break;
									case eActiveQuiverSlot.Second: ammo = Inventory.GetItem(eInventorySlot.SecondQuiver); break;
									case eActiveQuiverSlot.Third: ammo = Inventory.GetItem(eInventorySlot.ThirdQuiver); break;
									case eActiveQuiverSlot.Fourth: ammo = Inventory.GetItem(eInventorySlot.FourthQuiver); break;
									case eActiveQuiverSlot.None:
										eObjectType findType = eObjectType.Arrow;
										if (weapon.Object_Type == (int)eObjectType.Crossbow)
											findType = eObjectType.Bolt;

										ammo = Inventory.GetFirstItemByObjectType((int)findType, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

										break;
								}
							} break;
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
		protected override GameObject RangeAttackTarget
		{
			get
			{
				GameObject target = (GameObject)m_rangeAttackTarget.Target;
				if (target == null || target.ObjectState != eObjectState.Active)
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
			if (holdStart == 0)
			{
				holdStart = CurrentRegion.Time;
				TempProperties.setProperty(RANGE_ATTACK_HOLD_START, holdStart);
			}
			//DOLConsole.WriteLine("Holding.... ("+holdStart+") "+(Environment.TickCount - holdStart));
			if ((CurrentRegion.Time - holdStart) > 15000 && AttackWeapon.Object_Type != (int)eObjectType.Crossbow)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.TooTired"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return eCheckRangeAttackStateResult.Stop; //Stop the attack
			}

			//This state is set when the player wants to fire!
			if (RangeAttackState == eRangeAttackState.Fire
			   || RangeAttackState == eRangeAttackState.AimFire
			   || RangeAttackState == eRangeAttackState.AimFireReload)
			{
				RangeAttackTarget = null; // clean the RangeAttackTarget at the first shot try even if failed

				if (target == null || !(target is GameLiving))
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "System.MustSelectTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (!WorldMgr.CheckDistance(this, target, AttackRange))
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.TooFarAway", target.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (!TargetInView)  // TODO : wrong, must be checked with the target parameter and not with the targetObject
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CantSeeTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (!IsObjectInFront(target, 90))
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NotInView", target.GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (RangeAttackAmmo == null)
				{
					//another check for ammo just before firing
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.MustSelectQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				else if (!CheckRangedAmmoCompatibilityWithActiveWeapon())
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CantUseQuiver"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				else if (GameServer.ServerRules.IsAllowedToAttack(this, (GameLiving)target, false))
				{
					GameLiving living = target as GameLiving;
					if (RangeAttackType == eRangeAttackType.Critical && living != null
					   && (living.CurrentSpeed > 90 //walk speed == 85, hope that's what they mean
						   || (living.AttackState && living.InCombat) //maybe not 100% correct
						   || SpellHandler.FindEffectOnTarget(living, "Mesmerize") != null
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
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CantCritical"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						RangeAttackType = eRangeAttackType.Normal;
					}
					return eCheckRangeAttackStateResult.Fire;
				}

				RangeAttackState = eRangeAttackState.ReadyToFire;
				return eCheckRangeAttackStateResult.Hold;
			}

			//Player is aiming
			if (RangeAttackState == eRangeAttackState.Aim)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.ReadyToFire"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				RangeAttackState = eRangeAttackState.ReadyToFire;
				return eCheckRangeAttackStateResult.Hold;
			}
			else if (RangeAttackState == eRangeAttackState.ReadyToFire)
			{
				return eCheckRangeAttackStateResult.Hold; //Hold the shot
			}
			return eCheckRangeAttackStateResult.Fire;
		}

		/// <summary>
		/// Called whenever a single attack strike is made
		/// </summary>
		/// <param name="target"></param>
		/// <param name="weapon"></param>
		/// <param name="style"></param>
		/// <param name="effectiveness"></param>
		/// <param name="interruptDuration"></param>
		/// <param name="dualWield"></param>
		/// <returns></returns>
		protected override AttackData MakeAttack(GameObject target, InventoryItem weapon, Style style, double effectiveness, int interruptDuration, bool dualWield)
		{
			AttackData ad = base.MakeAttack(target, weapon, style, effectiveness * Effectiveness, interruptDuration, dualWield);

			//Clear the styles for the next round!
			NextCombatStyle = null;
			NextCombatBackupStyle = null;

			switch (ad.AttackResult)
			{
				case eAttackResult.TargetNotVisible: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NotInView", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.OutOfRange: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.TooFarAway", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.TargetDead: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.AlreadyDead", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Blocked: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Blocked", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Parried: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Parried", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Evaded: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Evaded", ad.Target.GetName(0, true)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.NoTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NeedTarget"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.NoValidTarget: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CantBeAttacked"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Missed: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Miss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Fumbled: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Fumble"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
					{
						// vampiir
						if (CharacterClass is PlayerClass.ClassVampiir
							&& target is GameKeepComponent == false
							&& target is GameKeepDoor == false
							&& target is GameSiegeWeapon == false)
						{
                            int perc = ad.Damage + ad.CriticalDamage;
                            perc = (perc < 1) ? 1 : ((perc > 12) ? 12 : perc);
                            this.Mana += Convert.ToInt32(Math.Ceiling(((Decimal)(perc * this.MaxMana) / 100)));
						}

						//only miss when strafing when attacking a player
						//30% chance to miss
						if (IsStrafing && ad.Target is GamePlayer && Util.Chance(30))
						{
							ad.AttackResult = eAttackResult.Missed;
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.StrafMiss"), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							break;
						}

						string modmessage = "";
						if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
						if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";

						string hitWeapon = "";

						switch (ServerProperties.Properties.SERV_LANGUAGE)
						{
							case "EN":
								if (weapon != null)
									hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
								break;
							case "DE":
								if (weapon != null)
									hitWeapon = weapon.Name;
								break;
							default:
								if (weapon != null)
									hitWeapon = GlobalConstants.NameToShortName(weapon.Name);
								break;
						}

						if (hitWeapon.Length > 0)
							hitWeapon = " " + LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.WithYour") + " " + hitWeapon;

						string attackTypeMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.YouAttack");
						if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
							attackTypeMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.YouShot");

						// intercept messages
						if (target != null && target != ad.Target)
						{
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Intercepted", ad.Target.GetName(0, true), target.GetName(0, false)), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.InterceptedHit", attackTypeMsg, target.GetName(0, false), hitWeapon, ad.Target.GetName(0, false), ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						}
						else
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.InterceptHit", attackTypeMsg, ad.Target.GetName(0, false), hitWeapon, ad.Damage, modmessage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);

						// critical hit
						if (ad.CriticalDamage > 0)
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Critical", ad.Target.GetName(0, false), ad.CriticalDamage), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
						break;
					}
			}

			switch (ad.AttackResult)
			{
				case eAttackResult.Blocked:
				case eAttackResult.Fumbled:
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
				case eAttackResult.Missed:
				case eAttackResult.Parried:
					//Condition percent can reach 70%
					//durability percent can reach zero
					// if item durability reachs 0, item is useless and become broken item

					if (weapon != null && weapon.ConditionPercent > 70 && Util.Chance(15))
					{
						int oldPercent = weapon.ConditionPercent;
						double con = GetConLevel(Level, weapon.Level);
						if (con < -3.0)
							con = -3.0;
						int sub = (int)(con + 4);
						if (oldPercent < 91)
						{
							sub *= 2;
						}

						// Subtract condition
						weapon.Condition -= sub;
						if (weapon.Condition < 0)
							weapon.Condition = 0;

						// Update displayed AF only if condition changed
						if (weapon.ConditionPercent != oldPercent)
						{
							// stats and max hits can't change, why update with every hit?
							// item 's buff do not depend of condition
							// Out.SendCharStatsUpdate();
							if (weapon.ConditionPercent == 90)
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CouldRepair", weapon.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							else if (weapon.ConditionPercent == 80)
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NeedRepair", weapon.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							else if (weapon.ConditionPercent == 70)
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NeedRepairDire", weapon.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							Out.SendUpdateWeaponAndArmorStats();
							Out.SendInventorySlotsUpdate(new int[] { weapon.SlotPosition });
						}
					}
					//Camouflage
					if (target is GamePlayer && HasAbility(Abilities.Camouflage))
					{
						CamouflageEffect camouflage = (CamouflageEffect)EffectList.GetOfType(typeof(CamouflageEffect));
						if (camouflage != null)
						{
							DisableSkill(SkillBase.GetAbility(Abilities.Camouflage), CamouflageSpecHandler.DISABLE_DURATION);
							camouflage.Cancel(false);
						}
					}
					//Savagery targets
					if (ad.AttackResult == eAttackResult.HitStyle)
					{
						byte targetToHit = 0;
						int random;
						IList targets = new ArrayList(1);
						IList list = new ArrayList(1);
						InventoryItem attackWeapon = AttackWeapon;
						InventoryItem leftWeapon = (Inventory == null) ? null : Inventory.GetItem(eInventorySlot.LeftHandWeapon);
						switch (style.ID)
						{
							case 374: targetToHit = 1; break; //Tribal Assault:	Hits 2 targets
							case 377: targetToHit = 1; break; //Clan's Might:		Hits 2 targets
							case 379: targetToHit = 2; break; //Totemic Wrath:		Hits 3 targets
							case 384: targetToHit = 3; break; //Totemic Sacrifice:	Hits 4 targets
							default: targetToHit = 0; break; //For others;
						}
						if (targetToHit > 0)
						{
							foreach (GamePlayer pl in GetPlayersInRadius(false, (ushort)AttackRange))
							{
								if (GameServer.ServerRules.IsAllowedToAttack(this, pl, true))
								{
									list.Add(pl);
								}
							}
							foreach (GameNPC npc in GetNPCsInRadius(false, (ushort)AttackRange))
							{
								if (GameServer.ServerRules.IsAllowedToAttack(this, npc, true))
								{
									list.Add(npc);
								}
							}
							list.Remove(target);
							if (list.Count > 1)
								while (targets.Count < targetToHit)
								{
									random = Util.Random(list.Count - 1);
									if (!targets.Contains(list[random]))
										targets.Add(list[random] as GameObject);
								}
							foreach (GameObject obj in targets)
							{
								if (obj is GamePlayer && ((GamePlayer)obj).IsSitting)
								{
									effectiveness *= 2;
								}
								new WeaponOnTargetAction(this, obj as GameObject, attackWeapon, leftWeapon, CalculateLeftHandSwingCount(), effectiveness, AttackSpeed(attackWeapon), null).Start(1);  // really start the attack
							}
						}
					}
					break;
			}
			return ad;
		}

		/// <summary>
		/// Calculates melee critical damage of this player
		/// </summary>
		/// <param name="ad">The attack data</param>
		/// <param name="weapon">The weapon used</param>
		/// <returns>The amount of critical damage</returns>
		public override int CalculateCriticalDamage(AttackData ad, InventoryItem weapon)
		{
			if (Util.Chance(AttackCriticalChance(weapon)))
			{
				// triple wield prevents critical hits
				if (ad.Target.EffectList.GetOfType(typeof(TripleWieldEffect)) != null) return 0;

				int critMin;
				int critMax;
				BerserkEffect berserk = (BerserkEffect)EffectList.GetOfType(typeof(BerserkEffect));

				if (berserk != null)
				{
					int level = GetAbilityLevel(Abilities.Berserk);
					// According to : http://daoc.catacombs.com/forum.cfm?ThreadKey=10833&DefMessage=922046&forum=37
					// Zerk 1 = 1-25%
					// Zerk 2 = 1-50%
					// Zerk 3 = 1-75%
					// Zerk 4 = 1-99%
					critMin = (int)(0.01 * ad.Damage);
					critMax = (int)(Math.Min(0.99, (level * 0.25)) * ad.Damage);
				}
				else
				{
					//think min crit dmage is 10% of damage
					critMin = ad.Damage / 10;
					// Critical damage to players is 50%, low limit should be around 20% but not sure
					// zerkers in Berserk do up to 99%
					if (ad.Target is GamePlayer)
						critMax = ad.Damage >> 1;
					else
						critMax = ad.Damage;
				}
				critMin = Math.Max(critMin, 0);
				critMax = Math.Max(critMin, critMax);

				return Util.Random(critMin, critMax);
			}
			return 0;
		}

		/// <summary>
		/// This method is called at the end of the attack sequence to
		/// notify objects if they have been attacked/hit by an attack
		/// </summary>
		/// <param name="ad">information about the attack</param>
		public override void OnAttackedByEnemy(AttackData ad)
		{
			if (IsOnHorse && ad.IsHit)
				IsOnHorse = false;

			switch (ad.AttackResult)
			{
				// is done in game living because of guard
				//			case eAttackResult.Blocked : Out.SendMessage(ad.Attacker.GetName(0, true) + " attacks you and you block the blow!", eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Parried: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Parry", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Evaded: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Evade", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Fumbled: Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Fumbled", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow); break;
				case eAttackResult.Missed:
					if (ad.AttackType == AttackData.eAttackType.Spell)
						break;
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Missed", ad.Attacker.GetName(0, true)), eChatType.CT_Missed, eChatLoc.CL_SystemWindow);
					break;
				case eAttackResult.HitStyle:
				case eAttackResult.HitUnstyled:
					{
						if (ad.Damage == -1)
							break;
						string hitLocName = null;
						switch (ad.ArmorHitLocation)
						{
							//GamePlayer.Attack.Location.Feet:	feet
							// LanguageMgr.GetTranslation(Client, "", ad.Attacker.GetName(0, true))
							case eArmorSlot.TORSO: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Torso"); break;
							case eArmorSlot.ARMS: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Arm"); break;
							case eArmorSlot.HEAD: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Head"); break;
							case eArmorSlot.LEGS: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Leg"); break;
							case eArmorSlot.HAND: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Hand"); break;
							case eArmorSlot.FEET: hitLocName = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Location.Foot"); break;
						}
						string modmessage = "";
						if (ad.Attacker is GamePlayer == false) // if attacked by player, don't show resists (?)
						{
							if (ad.Modifier > 0) modmessage = " (+" + ad.Modifier + ")";
							if (ad.Modifier < 0) modmessage = " (" + ad.Modifier + ")";
						}

						if (hitLocName != null)
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.HitsYour", ad.Attacker.GetName(0, true), hitLocName, ad.Damage, modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
						else
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.HitsYou", ad.Attacker.IsAlive ? ad.Attacker.GetName(0, true) : "A dead enemy", ad.Damage, modmessage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

						if (ad.CriticalDamage > 0)
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.HitsYouCritical", ad.Attacker.GetName(0, true), ad.CriticalDamage), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);

						// decrease condition of hitted armor piece
						if (ad.ArmorHitLocation != eArmorSlot.UNKNOWN)
						{
							InventoryItem item = Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

							//Condition percent can reach 70%
							//durability percent can reach zero
							// if item durability reachs 0, item is useless and become broken item

							// TODO: Random = quick hack in order to find good formula later
							// maybe because database have wrong max condition so must be increase
							//like condition
							if (item != null && item.ConditionPercent > 70)
							{
								int oldPercent = item.ConditionPercent;
								double con = GetConLevel(Level, item.Level);
								if (con < -3.0)
									con = -3.0;
								int sub = (int)(con + 4);
								if (oldPercent < 91)
								{
									sub *= 2;
								}

								// Subtract condition
								item.Condition -= sub;
								if (item.Condition < 0)
									item.Condition = 0;

								// Update displayed AF only if condition changed
								if (item.ConditionPercent != oldPercent)
								{
									// stats and max hits can't change, why update with every hit?
									// item 's buff do not depend of condition
									// Out.SendCharStatsUpdate();
									if (item.ConditionPercent == 90)
										Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.CouldRepair", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									else if (item.ConditionPercent == 80)
										Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NeedRepair", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									else if (item.ConditionPercent == 70)
										Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.NeedRepairDire", item.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									Out.SendUpdateWeaponAndArmorStats();
									Out.SendInventorySlotsUpdate(new int[] { item.SlotPosition });
								}
							}
						}

						//reactive effect
						if (ad.ArmorHitLocation != eArmorSlot.UNKNOWN)
						{
							InventoryItem reactiveitem = Inventory.GetItem((eInventorySlot)ad.ArmorHitLocation);

							if (reactiveitem != null && reactiveitem.ProcSpellID != 0 && Util.Chance(10))
							{
								// reactive effect on shield only proc again player
								if (reactiveitem.Object_Type != (int)eObjectType.Shield || (reactiveitem.Object_Type == (int)eObjectType.Shield && ad.Attacker is GamePlayer))
								{

									SpellLine reactiveEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
									if (reactiveEffectLine != null)
									{
										IList spells = SkillBase.GetSpellList(reactiveEffectLine.KeyName);
										if (spells != null)
										{
											foreach (Spell spell in spells)
											{
												if (spell.ID == reactiveitem.ProcSpellID)
												{
													if (spell.Level <= Level)
													{
														ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, reactiveEffectLine);
														if (spellHandler != null)
															spellHandler.StartSpell(ad.Attacker);
														else
															Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Spell.Proc.NotImplemented", reactiveitem.ProcSpellID), eChatType.CT_System, eChatLoc.CL_SystemWindow);
													}
													else
														Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Spell.NotEnoughPowerful"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
													break;
												}
											}
										}
									}
								}
							}
							if (reactiveitem != null && reactiveitem.ProcSpellID1 != 0 && Util.Chance(10))
							{
								// reactive effect on shield only proc again player
								if (reactiveitem.Object_Type != (int)eObjectType.Shield || (reactiveitem.Object_Type == (int)eObjectType.Shield && ad.Attacker is GamePlayer))
								{

									SpellLine reactiveEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);
									if (reactiveEffectLine != null)
									{
										IList spells = SkillBase.GetSpellList(reactiveEffectLine.KeyName);
										if (spells != null)
										{
											foreach (Spell spell in spells)
											{
												if (spell.ID == reactiveitem.ProcSpellID1)
												{
													if (spell.Level <= Level)
													{
														ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, reactiveEffectLine);
														if (spellHandler != null)
														{
															spellHandler.StartSpell(ad.Attacker);
														}
														else
														{
															Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Spell.Proc.NotImplemented", reactiveitem.ProcSpellID), eChatType.CT_System, eChatLoc.CL_SystemWindow);
														}
													}
													else
														Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Spell.NotEnoughPowerful"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
													break;
												}
											}
										}
									}
								}
							}
						}
						break;
					}
			}
			// Mauler
			if (CharacterClass.ID > 59 && CharacterClass.ID < 63)
			{
				this.Mana += (7 * this.MaxMana) / 100;
			}

			// vampiir
			if (CharacterClass is PlayerClass.ClassVampiir)
			{
				GameSpellEffect removeEffect = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
				if (removeEffect != null)
					removeEffect.Cancel(false);
			}
			base.OnAttackedByEnemy(ad);
		}

		/// <summary>
		/// Does needed interrupt checks and interrupts if needed
		/// </summary>
		/// <param name="attacker">the attacker that is interrupting</param>
		/// <param name="attackType">The attack type</param>
		/// <returns>true if interrupted successfully</returns>
		protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if (base.OnInterruptTick(attacker, attackType))
			{
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					string attackTypeMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Type.Shot");
					if (AttackWeapon != null && AttackWeapon.Object_Type == (int)eObjectType.Thrown)
						attackTypeMsg = LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Type.Throw");
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Attack.Interrupted", attacker.GetName(0, true), attackTypeMsg), eChatType.CT_YouHit, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the effective AF of this living
		/// </summary>
		public override int EffectiveOverallAF
		{ // TODO: consider player level against armor level
			get
			{
				int eaf = 0;
				int abs = 0;
				foreach (InventoryItem item in Inventory.VisibleItems)
				{
					double factor = 0;
					switch (item.Item_Type)
					{
						case Slot.TORSO:
							factor = 2.2;
							break;
						case Slot.LEGS:
							factor = 1.3;
							break;
						case Slot.ARMS:
							factor = 0.75;
							break;
						case Slot.HELM:
							factor = 0.5;
							break;
						case Slot.HANDS:
							factor = 0.25;
							break;
						case Slot.FEET:
							factor = 0.25;
							break;
					}

					int itemAFCap = Level << 1;
					if (RealmLevel > 39)
						itemAFCap += 2;
					switch ((eObjectType)item.Object_Type)
					{
						case eObjectType.Cloth:
							abs = 0;
							itemAFCap >>= 1;
							break;
						case eObjectType.Leather:
							abs = 10;
							break;
						case eObjectType.Reinforced:
							abs = 19;
							break;
						case eObjectType.Studded:
							abs = 19;
							break;
						case eObjectType.Scale:
							abs = 27;
							break;
						case eObjectType.Chain:
							abs = 27;
							break;
						case eObjectType.Plate:
							abs = 34;
							break;
					}

					if (factor > 0)
					{
						int af = item.DPS_AF;
						if (af > itemAFCap)
							af = itemAFCap;
						double piece_eaf = af * item.Quality / 100.0 * item.ConditionPercent / 100.0 * (1 + abs / 100.0);
						eaf += (int)(piece_eaf * factor);
					}
				}

				// Overall AF CAP = 10 * level * (1 + abs%/100)
				int bestLevel = -1;
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.AlbArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.HibArmor));
				bestLevel = Math.Max(bestLevel, GetAbilityLevel(Abilities.MidArmor));
				switch (bestLevel)
				{
					default: abs = 0; break; // cloth etc
					case ArmorLevel.Leather: abs = 10; break;
					case ArmorLevel.Studded: abs = 19; break;
					case ArmorLevel.Chain: abs = 27; break;
					case ArmorLevel.Plate: abs = 34; break;
				}

				eaf += BuffBonusCategory1[(int)eProperty.ArmorFactor]; // base buff before cap
				int eafcap = (int)(10 * Level * (1 + abs * 0.01));
				if (eaf > eafcap)
					eaf = eafcap;
				eaf += (int)Math.Min(Level * 1.875, BuffBonusCategory2[(int)eProperty.ArmorFactor])
					   - BuffBonusCategory3[(int)eProperty.ArmorFactor]
					   + BuffBonusCategory4[(int)eProperty.ArmorFactor]
					   + Math.Min(Level, ItemBonus[(int)eProperty.ArmorFactor]);

				eaf = (int)(eaf * BuffBonusMultCategory1.Get((int)eProperty.ArmorFactor));

				return eaf;
			}
		}
		/// <summary>
		/// Calc Armor hit location when player is hit by enemy
		/// </summary>
		/// <returns>slotnumber where enemy hits</returns>
		/// attackdata(ad) changed 
		public virtual eArmorSlot CalculateArmorHitLocation(AttackData ad)
		{
			if (ad.Style != null)
			{
				if (ad.Style.ArmorHitLocation != eArmorSlot.UNKNOWN)
					return ad.Style.ArmorHitLocation;
			}
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
		public override int WeaponSpecLevel(InventoryItem weapon)
		{
			if (weapon == null)
				return 0;
			// use axe spec if left hand axe is not in the left hand slot
			if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
				return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.Axe);
			// use left axe spec if axe is in the left hand slot
			if (weapon.SlotPosition == Slot.LEFTHAND
				&& (weapon.Object_Type == (int)eObjectType.Axe
					|| weapon.Object_Type == (int)eObjectType.Sword
					|| weapon.Object_Type == (int)eObjectType.Hammer))
				return GameServer.ServerRules.GetObjectSpecLevel(this, eObjectType.LeftAxe);
			return GameServer.ServerRules.GetObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
		}

		public virtual String GetWeaponSpec(InventoryItem weapon)
		{
			if (weapon == null)
				return null;
			// use axe spec if left hand axe is not in the left hand slot
			if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
				return SkillBase.ObjectTypeToSpec(eObjectType.Axe);
			// use left axe spec if axe is in the left hand slot
			if (weapon.SlotPosition == Slot.LEFTHAND
				&& (weapon.Object_Type == (int)eObjectType.Axe
					|| weapon.Object_Type == (int)eObjectType.Sword
					|| weapon.Object_Type == (int)eObjectType.Hammer))
				return SkillBase.ObjectTypeToSpec(eObjectType.LeftAxe);
			return SkillBase.ObjectTypeToSpec((eObjectType)weapon.Object_Type);
		}

		/// <summary>
		/// determines current weaponspeclevel
		/// </summary>
		public int WeaponBaseSpecLevel(InventoryItem weapon)
		{
			if (weapon == null)
				return 0;
			// use axe spec if left hand axe is not in the left hand slot
			if (weapon.Object_Type == (int)eObjectType.LeftAxe && weapon.SlotPosition != Slot.LEFTHAND)
				return GameServer.ServerRules.GetBaseObjectSpecLevel(this, eObjectType.Axe);
			// use left axe spec if axe is in the left hand slot
			if (weapon.SlotPosition == Slot.LEFTHAND
				&& (weapon.Object_Type == (int)eObjectType.Axe
				|| weapon.Object_Type == (int)eObjectType.Sword
				|| weapon.Object_Type == (int)eObjectType.Hammer))
				return GameServer.ServerRules.GetBaseObjectSpecLevel(this, eObjectType.LeftAxe);
			return GameServer.ServerRules.GetBaseObjectSpecLevel(this, (eObjectType)weapon.Object_Type);
		}

		/// <summary>
		/// Gets the weaponskill of weapon
		/// </summary>
		/// <param name="weapon"></param>
		public override double GetWeaponSkill(InventoryItem weapon)
		{
			if (weapon == null)
			{
				return 0;
			}
			double classbase =
				(weapon.SlotPosition == (int)eInventorySlot.DistanceWeapon
					? CharacterClass.WeaponSkillRangedBase
					: CharacterClass.WeaponSkillBase);

			//added for WS Poisons
			double preBuff = ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * Effectiveness);

			//return ((Level * classbase * 0.02 * (1 + (GetWeaponStat(weapon) - 50) * 0.005)) * PlayerEffectiveness);
			return Math.Max(0, preBuff * GetModified(eProperty.WeaponSkill) * 0.01);
		}

		/// <summary>
		/// calculates weapon stat
		/// </summary>
		/// <param name="weapon"></param>
		/// <returns></returns>
		public override int GetWeaponStat(InventoryItem weapon)
		{
			if (weapon != null)
			{
				switch ((eObjectType)weapon.Object_Type)
				{
					// DEX modifier
					case eObjectType.Staff:
					case eObjectType.Fired:
					case eObjectType.Longbow:
					case eObjectType.Crossbow:
					case eObjectType.CompositeBow:
					case eObjectType.RecurvedBow:
					case eObjectType.Thrown:
					case eObjectType.Shield:
					case eObjectType.FistWraps:
						return GetModified(eProperty.Dexterity);

					// STR+DEX modifier
					case eObjectType.ThrustWeapon:
					case eObjectType.Piercing:
					case eObjectType.Spear:
					case eObjectType.Flexible:
					case eObjectType.HandToHand:
					case eObjectType.MaulerStaff:
						return (GetModified(eProperty.Strength) + GetModified(eProperty.Dexterity)) >> 1;
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
			InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
			if (item == null) return 0;
			double eaf = item.DPS_AF + BuffBonusCategory1[(int)eProperty.ArmorFactor]; // base AF buff

			int itemAFcap = Level;
			if (RealmLevel > 39)
				itemAFcap++;
			if (item.Object_Type != (int)eObjectType.Cloth)
			{
				itemAFcap <<= 1;
			}

			eaf = Math.Min(eaf, itemAFcap);
			eaf *= 4.67; // compensate *4.67 in damage formula

			// my test shows that qual is added after AF buff
			eaf *= item.Quality * 0.01 * item.Condition / item.MaxCondition;

			eaf += GetModified(eProperty.ArmorFactor);

			/*GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, typeof(VampiirArmorDebuff));
			if (effect != null && slot == (effect.SpellHandler as VampiirArmorDebuff).Slot)
			{
				eaf -= (int)(effect.SpellHandler as VampiirArmorDebuff).Spell.Value;
			}*/
			return eaf;
		}

		/// <summary>
		/// Calculates armor absorb level
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		public override double GetArmorAbsorb(eArmorSlot slot)
		{
			if (slot == eArmorSlot.UNKNOWN) return 0;
			InventoryItem item = Inventory.GetItem((eInventorySlot)slot);
			if (item == null) return 0;
			// vampiir random armor debuff change ~
			double eaf = (item.SPD_ABS + GetModified(eProperty.ArmorAbsorbtion)) * 0.01;
			/*GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, typeof(VampiirArmorDebuff));
			if (effect != null && slot == (effect.SpellHandler as VampiirArmorDebuff).Slot)
			{
				eaf -= (int)(effect.SpellHandler as VampiirArmorDebuff).Spell.Value;
			}*/
			return eaf;
		}

		/// <summary>
		/// Weaponskill thats shown to the player
		/// </summary>
		public virtual int DisplayedWeaponSkill
		{
			get
			{
				int itemBonus = WeaponSpecLevel(AttackWeapon) - WeaponBaseSpecLevel(AttackWeapon) - RealmLevel / 10;
				double m = 0.56 + itemBonus / 70.0;
				double weaponSpec = WeaponSpecLevel(AttackWeapon) + itemBonus * m;
				return (int)(GetWeaponSkill(AttackWeapon) * (1.00 + weaponSpec * 0.01));
			}
		}

		/// <summary>
		/// Gets the weapondamage of currently used weapon
		/// Used to display weapon damage in stats, 16.5dps = 1650
		/// </summary>
		/// <param name="weapon">the weapon used for attack</param>
		public override double WeaponDamage(InventoryItem weapon)
		{
			if (weapon != null)
			{
				//TODO if attackweapon is ranged -> attackdamage is arrow damage
				int DPS = weapon.DPS_AF;

				// apply damage cap before quality
				// http://www.classesofcamelot.com/faq.asp?mode=view&cat=10
				int cap = 12 + 3 * Level;
				if (RealmLevel > 39)
					cap += 3;
				if (DPS > cap)
				{
					DPS = cap;
				}
				//(1.0 + BuffBonusCategory1[(int)eProperty.DPS]/100.0 - BuffBonusCategory3[(int)eProperty.DPS]/100.0)
				DPS = (int)(DPS * (1 + (GetModified(eProperty.DPS) * 0.01)));
				// beware to use always ConditionPercent, because Condition is abolute value
				//				return (int) ((DPS/10.0)*(weapon.Quality/100.0)*(weapon.Condition/(double)weapon.MaxCondition)*100.0);
				double wdamage = (0.001 * DPS * weapon.Quality * weapon.Condition) / weapon.MaxCondition;
				return wdamage;
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
		public override double UnstyledDamageCap(InventoryItem weapon)
		{
			if (weapon != null)
			{
				int DPS = weapon.DPS_AF;
				int cap = 12 + 3 * Level;
				if (RealmLevel > 39)
					cap += 3;
				if (DPS > cap)
					DPS = cap;
				//				double result = (DPS*0.1 * weapon.SPD_ABS*0.1 * 3 * (1 + (weapon.SPD_ABS*0.1 - 2) * .03));
				double result = DPS * weapon.SPD_ABS * 0.03 * (0.94 + 0.003 * weapon.SPD_ABS);

				// TODO: ToA damage bonus
				if (weapon.Hand == 1) //2h
				{
					result *= 1.1 + (WeaponSpecLevel(weapon) - 1) * 0.005;
					if (weapon.Item_Type == Slot.RANGED)
					{
						// http://home.comcast.net/~shadowspawn3/bowdmg.html
						//ammo damage bonus
						double ammoDamageBonus = 1;
						if (RangeAttackAmmo != null)
						{
							switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
							{
								case 0: ammoDamageBonus = 0.85; break; //Blunt       (light) -15%
								case 1: ammoDamageBonus = 1; break; //Bodkin     (medium)   0%
								case 2: ammoDamageBonus = 1.15; break; //doesn't exist on live
								case 3: ammoDamageBonus = 1.25; break; //Broadhead (X-heavy) +25%
							}
						}
						result *= ammoDamageBonus;
					}
				}

				return result;
			}
			else
			{ // TODO: whats the damage cap without weapon?
				return AttackDamage(weapon) * 3 * (1 + (AttackSpeed(weapon) * 0.001 - 2) * .03);
			}
		}

		/// <summary>
		/// The chance for a critical hit
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override int AttackCriticalChance(InventoryItem weapon)
		{
			if (weapon != null && weapon.Item_Type == Slot.RANGED && RangeAttackType == eRangeAttackType.Critical)
				return 0; // no crit damage for crit shots

			// check for melee attack
			if (weapon != null && weapon.Item_Type != Slot.RANGED)
			{
				return GetModified(eProperty.CriticalMeleeHitChance);
			}

			// check for ranged attack
			if (weapon != null && weapon.Item_Type == Slot.RANGED)
			{
				return GetModified(eProperty.CriticalArcheryHitChance);
			}

			// base 10% chance of critical for all with melee weapons
			return 10;
		}

		/// <summary>
		/// Returns the damage type of the current attack
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override eDamageType AttackDamageType(InventoryItem weapon)
		{
			if (weapon == null)
				return eDamageType.Natural;
			switch ((eObjectType)weapon.Object_Type)
			{
				case eObjectType.Crossbow:
				case eObjectType.Longbow:
				case eObjectType.CompositeBow:
				case eObjectType.RecurvedBow:
				case eObjectType.Fired:
					ItemTemplate ammo = RangeAttackAmmo;
					if (ammo == null)
						return (eDamageType)weapon.Type_Damage;
					return (eDamageType)ammo.Type_Damage;
				case eObjectType.Shield:
					return eDamageType.Crush; // TODO: shields do crush damage (!) best is if Type_Damage is used properly
				default:
					return (eDamageType)weapon.Type_Damage;
			}
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

				//TODO change to real distance of bows!
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					InventoryItem weapon = AttackWeapon;
					if (weapon == null)
						return 0;

					double range;
					ItemTemplate ammo = RangeAttackAmmo;

					switch ((eObjectType)weapon.Object_Type)
					{
						case eObjectType.Longbow: range = 1760; break;
						case eObjectType.RecurvedBow: range = 1680; break;
						case eObjectType.CompositeBow: range = 1600; break;
						default: range = 1200; break; // shortbow, xbow, throwing
					}

					range = Math.Max(32, range * GetModified(eProperty.ArcheryRange) * 0.01);

					if (ammo != null)
						switch ((ammo.SPD_ABS >> 2) & 0x3)
						{
							case 0: range *= 0.85; break; //Clout -15%
							//						case 1:                break; //(none) 0%
							case 2: range *= 1.15; break; //doesn't exist on live
							case 3: range *= 1.25; break; //Flight +25%
						}
					if (livingTarget != null) range += Math.Min((Z - livingTarget.Z) / 2.0, 500);
					if (range < 32) range = 32;

					return (int)(range);
				}

				int meleerange = 128;
				GameKeepComponent keepcomponent = livingTarget as GameKeepComponent; // TODO better component melee attack range check
				if (keepcomponent != null)
					meleerange += 150;
				else
				{
					if (livingTarget != null && livingTarget.IsMoving)
						meleerange += 32;
					if (IsMoving)
						meleerange += 32;
				}
				return meleerange;
			}
			set { }
		}

		/// <summary>
		/// Gets the current attackspeed of this living in milliseconds
		/// </summary>
		/// <param name="weapons">attack weapons</param>
		/// <returns>effective speed of the attack. average if more than one weapon.</returns>
		public override int AttackSpeed(params InventoryItem[] weapons)
		{
			if (weapons == null || weapons.Length < 1)
				return 0;

			int count = 0;
			double speed = 0;
			bool bowWeapon = true;
			for (int i = 0; i < weapons.Length; i++)
			{
				if (weapons[i] != null)
				{
					speed += weapons[i].SPD_ABS;
					count++;
					switch (weapons[i].Object_Type)
					{
						case (int)eObjectType.Fired:
						case (int)eObjectType.Longbow:
						case (int)eObjectType.Crossbow:
						case (int)eObjectType.RecurvedBow:
						case (int)eObjectType.CompositeBow: break;
						default: bowWeapon = false; break;
					}
				}
			}

			if (count < 1)
				return 0;

			speed /= count;

			int qui = Math.Min(250, Quickness); //250 soft cap on quickness
			if (bowWeapon)
			{
				//Draw Time formulas, there are very many ...
				//Formula 2: y = iBowDelay * ((100 - ((iQuickness - 50) / 5 + iMasteryofArcheryLevel * 3)) / 100)
				//Formula 1: x = (1 - ((iQuickness - 60) / 500 + (iMasteryofArcheryLevel * 3) / 100)) * iBowDelay
				//Table a: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * ((1-MoA*0.03) - (archeryspeedbonus/100))
				//Table b: Formula used: drawspeed = bowspeed * (1-(quickness - 50)*0.002) * (1-MoA*0.03) - ((archeryspeedbonus/100 * basebowspeed))

				//For now use the standard weapon formula, later add ranger haste etc.
				speed *= (1.0 - (qui - 60) * 0.002) * 0.01 * GetModified(eProperty.ArcherySpeed);
				if (RangeAttackType == eRangeAttackType.Critical)
					speed = speed * 2 - (GetAbilityLevel(Abilities.Critical_Shot) - 1) * speed / 10;
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
		public override double AttackDamage(InventoryItem weapon)
		{
			if (weapon == null)
				return 0;

			double effectiveness = 1.00;
			double damage = WeaponDamage(weapon) * weapon.SPD_ABS * 0.1;

			if (weapon.Hand == 1) // two-hand
			{
				// twohanded used weapons get 2H-Bonus = 10% + (Skill / 2)%
				int spec = WeaponSpecLevel(weapon) - 1;
				damage *= 1.1 + spec * 0.005;
			}

			if (weapon.Item_Type == Slot.RANGED)
			{
				//ammo damage bonus
				if (RangeAttackAmmo != null)
				{
					switch ((RangeAttackAmmo.SPD_ABS) & 0x3)
					{
						case 0: damage *= 0.85; break; //Blunt       (light) -15%
						//case 1: damage *= 1;	break; //Bodkin     (medium)   0%
						case 2: damage *= 1.15; break; //doesn't exist on live
						case 3: damage *= 1.25; break; //Broadhead (X-heavy) +25%
					}
				}
				//Ranged damage buff,debuff,Relic,RA
				effectiveness += GetModified(eProperty.RangedDamage) * 0.01;
			}
			else if (weapon.Item_Type == Slot.RIGHTHAND || weapon.Item_Type == Slot.LEFTHAND || weapon.Item_Type == Slot.TWOHAND)
			{
				//Melee damage buff,debuff,Relic,RA
				effectiveness += GetModified(eProperty.MeleeDamage) * 0.01;
			}
			damage *= effectiveness;
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
		public override void Die(GameObject killer)
		{
			bool realmDeath = killer != null && killer.Realm != eRealm.None;

			TargetObject = null;
			Diving(waterBreath.Normal);
			if (IsOnHorse)
				IsOnHorse = false;

			// cancel task if active
			if (Task != null && Task.TaskActive)
				Task.ExpireTask();

			string playerMessage;
			string publicMessage;
			ushort messageDistance = WorldMgr.DEATH_MESSAGE_DISTANCE;
			m_releaseType = eReleaseType.Normal;

			string location = "";
			if (CurrentAreas.Count > 0)
				location = (CurrentAreas[0] as AbstractArea).Description;
			else
				location = CurrentZone.Description;

			if (killer == null)
			{
				if (realmDeath)
				{
					playerMessage = LanguageMgr.GetTranslation(Client, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
					publicMessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.Die.KilledLocation", GetName(0, true), location);
				}
				else
				{
					playerMessage = LanguageMgr.GetTranslation(Client, "GamePlayer.Die.Killed", GetName(0, true));
					publicMessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.Die.Killed", GetName(0, true));
				}
			}
			else
			{
				if (DuelTarget == killer)
				{
					m_releaseType = eReleaseType.Duel;
					messageDistance = WorldMgr.YELL_DISTANCE;
					playerMessage = LanguageMgr.GetTranslation(Client, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
					publicMessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.Die.DuelDefeated", GetName(0, true), killer.GetName(1, false));
				}
				else
				{
					messageDistance = 0;
					if (realmDeath)
					{
						playerMessage = LanguageMgr.GetTranslation(Client, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
						publicMessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.Die.KilledByLocation", GetName(0, true), killer.GetName(1, false), location);
					}
					else
					{
						playerMessage = LanguageMgr.GetTranslation(Client, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
						publicMessage = LanguageMgr.GetTranslation(ServerProperties.Properties.SERV_LANGUAGE, "GamePlayer.Die.KilledBy", GetName(0, true), killer.GetName(1, false));
					}
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
			if (m_releaseType == eReleaseType.Duel)
				messageType = eChatType.CT_Emote;
			else if (killer == null)
			{
				messageType = eChatType.CT_PlayerDied;
			}
			else
			{
				switch ((eRealm)killer.Realm)
				{
					case eRealm.Albion: messageType = eChatType.CT_KilledByAlb; break;
					case eRealm.Midgard: messageType = eChatType.CT_KilledByMid; break;
					case eRealm.Hibernia: messageType = eChatType.CT_KilledByHib; break;
					default: messageType = eChatType.CT_PlayerDied; break; // killed by mob
				}
			}

			if (killer is GamePlayer && killer != this)
			{
				((GamePlayer)killer).Out.SendMessage(LanguageMgr.GetTranslation(((GamePlayer)killer).Client, "GamePlayer.Die.YouKilled", GetName(0, false)), eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			}

			ArrayList players = new ArrayList();
			if (messageDistance == 0)
			{
				foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
				{
					players.Add(client.Player);
				}
			}
			else
			{
				foreach (GamePlayer player in GetPlayersInRadius(messageDistance))
				{
					players.Add(player);
				}
			}

			foreach (GamePlayer player in players)
			{
				// on normal server type send messages only to the killer and dead players realm
				// check for gameplayer is needed because killers realm don't see deaths by guards
				if (
					(player != killer) && (
											(killer != null && killer is GamePlayer && GameServer.ServerRules.IsSameRealm((GamePlayer)killer, player, true))
											|| (GameServer.ServerRules.IsSameRealm(this, player, true))
											|| ServerProperties.Properties.DEATH_MESSAGES_ALL_REALMS)
					)
					if (player == this)
						player.Out.SendMessage(playerMessage, messageType, eChatLoc.CL_SystemWindow);
					else player.Out.SendMessage(publicMessage, messageType, eChatLoc.CL_SystemWindow);
			}

			//Dead ppl. dismount ...
			if (Steed != null)
				DismountSteed(true);
			//Dead ppl. don't sit ...
			if (IsSitting)
			{
				IsSitting = false;
				UpdatePlayerStatus();
			}

			// then buffs drop messages
			base.Die(killer);

			lock (m_LockObject)
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

				Out.SendTimerWindow(LanguageMgr.GetTranslation(Client, "System.ReleaseTimer"), (m_automaticRelease ? RELEASE_MINIMUM_WAIT : RELEASE_TIME));
				m_releaseTimer = new RegionTimer(this);
				m_releaseTimer.Callback = new RegionTimerCallback(ReleaseTimerCallback);
				m_releaseTimer.Start(1000);

				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Die.ReleaseToReturn"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);

				// clear target object so no more actions can used on this target, spells, styles, attacks...
				TargetObject = null;

				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
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

				if (realmDeath)
				{
					// 
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Die.DeadRVR"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					xpLossPercent = 0;
				}
				else if (Level > 5) // under level 5 there is no penalty
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Die.LoseExperience"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
					// if this is the first death in level, you lose only half the penalty
					switch (PlayerCharacter.DeathCount)
					{
						case 0:
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Die.DeathN1"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
							xpLossPercent /= 3;
							break;
						//FIXME: And if DeathCount > 1 ??? (VaNaTiC)
						case 1:
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Die.DeathN2"), eChatType.CT_YouDied, eChatLoc.CL_SystemWindow);
							xpLossPercent = xpLossPercent * 2 / 3;
							break;
					}

					PlayerCharacter.DeathCount++;

					long xpLoss = (ExperienceForNextLevel - ExperienceForCurrentLevel) * xpLossPercent / 1000;
					GainExperience(-xpLoss, 0, 0, 0, false, true);
					TempProperties.setProperty(DEATH_EXP_LOSS_PROPERTY, xpLoss);

					int conLoss = PlayerCharacter.DeathCount;
					if (conLoss > 3)
						conLoss = 3;
					else if (conLoss < 1)
						conLoss = 1;
					TempProperties.setProperty(DEATH_CONSTITUTION_LOSS_PROPERTY, conLoss);
				}
				GameEventMgr.AddHandler(this, GamePlayerEvent.Revive, new DOLEventHandler(OnRevive));
			}

			CommandNpcRelease();
			if (this.SiegeWeapon != null)
				SiegeWeapon.ReleaseControl();

			// sent after buffs drop
			// GamePlayer.Die.CorpseLies:		{0} just died. {1} corpse lies on the ground.
			//Message.SystemToOthers(this, GetName(0, true) + " just died.  " + GetPronoun(1, true) + " corpse lies on the ground.", eChatType.CT_PlayerDied);
			Message.SystemToOthers2(this, eChatType.CT_PlayerDied, "GamePlayer.Die.CorpseLies", GetName(0, true), GetPronoun(this.Client, 1, true));
			if (m_releaseType == eReleaseType.Duel)
			{
				Message.SystemToOthers(this, killer.Name + " wins the duel!", eChatType.CT_Emote);
			}

			// deal out exp and realm points based on server rules
			// no other way to keep correct message order...
			GameServer.ServerRules.OnPlayerKilled(this, killer);
			if (m_releaseType != eReleaseType.Duel)
				PlayerCharacter.DeathTime = PlayedTime;
		}

		public override void EnemyKilled(GameLiving enemy)
		{
			if (Group != null)
			{
				foreach (GamePlayer player in Group.GetPlayersInTheGroup())
				{
					if (player == this) continue;
					if (enemy.Attackers.Contains(player)) continue;
					if (WorldMgr.CheckDistance(this, player, WorldMgr.MAX_EXPFORKILL_DISTANCE))
					{
						Notify(GameLivingEvent.EnemyKilled, player, new EnemyKilledEventArgs(enemy));
					}

					if (player.Attackers.Contains(enemy))
						player.RemoveAttacker(enemy);

					if (player.ControlledNpc != null && player.ControlledNpc.Body.Attackers.Contains(enemy))
						player.ControlledNpc.Body.RemoveAttacker(enemy);
				}
			}

			if (ControlledNpc != null && ControlledNpc.Body.Attackers.Contains(enemy))
				ControlledNpc.Body.RemoveAttacker(enemy);

			base.EnemyKilled(enemy);
		}


		/// <summary>
		/// Check this flag to see wether this living is involved in combat
		/// </summary>
		public override bool InCombat
		{
			get
			{
				IControlledBrain npc = ControlledNpc;
				if (npc != null && npc.Body.InCombat)
					return true;
				return base.InCombat;
			}
		}

		/// <summary>
		/// Easy method to get the resist of a certain damage type
		/// Good for when we add RAs
		/// </summary>
		/// <param name="property"></param>
		/// <returns></returns>
		public override int GetDamageResist(eProperty property)
		{
			int res = 0;
			int classResist = 0;
			int secondResist = 0;

			//Q: Do the Magic resist bonuses from Bedazzling Aura and Empty Mind stack with each other?
			//A: Nope.
			switch ((eResist)property)
			{
				case eResist.Body:
				case eResist.Cold:
				case eResist.Energy:
				case eResist.Heat:
				case eResist.Matter:
				case eResist.Spirit:
					res += BuffBonusCategory1[(int)eProperty.MagicAbsorbtion];
					break;
				default:
					break;
			}
			return (int)((res + classResist) - 0.01 * secondResist * (res + classResist) + secondResist);
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
			GameEventMgr.AddHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
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
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(DuelOnAttack));
			GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackFinished, new DOLEventHandler(DuelOnAttack));
			lock (m_xpGainers.SyncRoot)
			{
				m_xpGainers.Clear();
			}
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.DuelStop.DuelEnds"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
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
			GameLiving target = null;
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
		protected long m_disabledCastingTimeout = 0;
		/// <summary>
		/// Time when casting is allowed again (after interrupt from enemy attack)
		/// </summary>
		public long DisabledCastingTimeout
		{
			get { return m_disabledCastingTimeout; }
			set { m_disabledCastingTimeout = value; }
		}

		/// <summary>
		/// Grey out some skills on client for specified duration
		/// </summary>
		/// <param name="skill">the skill to disable</param>
		/// <param name="duration">duration of disable in milliseconds</param>
		public override void DisableSkill(Skill skill, int duration)
		{
			if (this.Client.Account.PrivLevel > 1)
				return;

			base.DisableSkill(skill, duration);

			Out.SendDisableSkill(skill, duration / 1000 + 1);
		}

		/// <summary>
		/// Updates all disabled skills to player
		/// </summary>
		public virtual void UpdateDisabledSkills()
		{
			foreach (Skill skl in GetAllDisabledSkills())
			{
				Out.SendDisableSkill(skl, GetSkillDisabledDuration(skl) / 1000 + 1);
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
				// warlock
				if (nextSpell != null)
				{
					if (nextSpell.IsSecondary)
					{
						GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Powerless");
						if (effect == null)
							effect = SpellHandler.FindEffectOnTarget(this, "Range");
						if (effect == null)
							effect = SpellHandler.FindEffectOnTarget(this, "Uninterruptable");

						if (effect != null)
							effect.Cancel(false);
					}

				}
				m_runningSpellHandler = null;
				m_nextSpell = null;			// avoid restarting nextspell by reentrance from spellhandler
				m_nextSpellLine = null;

				if (nextSpell != null)
					m_runningSpellHandler = ScriptMgr.CreateSpellHandler(this, nextSpell, nextSpellLine);
			}
			if (m_runningSpellHandler != null)
			{
				m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler.CastSpell();
			}
		}

		public override void CastSpell(Spell spell, SpellLine line)
		{
			if (spell.SpellType == "StyleHandler")
			{
				Style style = SkillBase.GetStyleByID((int)spell.Value, CharacterClass.ID);
				if (style != null)
				{
					StyleProcessor.TryToUseStyle(this, style);
				}
				else { Out.SendMessage("That style is not implemented!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow); }
			}
			else if (spell.SpellType == "BodyguardHandler")
			{
				Ability ab = SkillBase.GetAbility("Bodyguard");
				IAbilityActionHandler handler = SkillBase.GetAbilityActionHandler(ab.KeyName);
				if (handler != null)
				{
					handler.Execute(ab, this);
					return;
				}
			}
			else
			{
				if (IsStunned)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CastSpell.CantCastStunned"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					return;
				}
				if (IsMezzed)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CastSpell.CantCastMezzed"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
					return;
				}
				/*
				if (IsSilenced)
				{
					Out.SendMessage("You are fumbling for your words, and cannot cast!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
					return;
				}
				*/
				double fumbleChance = GetModified(eProperty.SpellFumbleChance);
				fumbleChance *= 0.01;
				if (fumbleChance > 0)
				{
					if (Util.ChanceDouble(fumbleChance))
					{
						Out.SendMessage("You are fumbling for your words, and cannot cast!", eChatType.CT_Spell, eChatLoc.CL_SystemWindow);
						return;
					}
				}

				lock (m_spellQueueAccessMonitor)
				{
					// warlock queue stuff
					if (m_runningSpellHandler != null)
					{
						if (spell.CastTime > 0 && !(m_runningSpellHandler is ChamberSpellHandler) && spell.SpellType != "Chamber")
						{
							if (m_runningSpellHandler.Spell.InstrumentRequirement != 0)
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CastSpell.AlreadyPlaySong"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
								return;
							}
							if (SpellQueue)
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CastSpell.AlreadyCastFollow"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
								m_nextSpell = spell;
								m_nextSpellLine = line;
							}
							else Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CastSpell.AlreadyCastNoQueue"), eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
							return;
						}
						else if (m_runningSpellHandler is PrimerSpellHandler)
						{
							if (!spell.IsSecondary)
							{
								Out.SendMessage("Only a secondary spell can follow up the spell your are casting!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
							}
							else
							{
								if (SpellQueue && !(m_runningSpellHandler is ChamberSpellHandler))
								{
									Spell cloneSpell = null;
									if (m_runningSpellHandler is PowerlessSpellHandler)
									{
										cloneSpell = spell.Copy();
										cloneSpell.CostPower = false;
										m_nextSpell = cloneSpell;
										m_nextSpellLine = line;
									}
									else if (m_runningSpellHandler is RangeSpellHandler)
									{
										cloneSpell = spell.Copy();
										cloneSpell.CostPower = false;
										cloneSpell.OverrideRange = m_runningSpellHandler.Spell.Range;
										m_nextSpell = cloneSpell;
										m_nextSpellLine = line;
									}
									else if (m_runningSpellHandler is UninterruptableSpellHandler)
									{
										cloneSpell = spell.Copy();
										cloneSpell.CostPower = false;
										m_nextSpell = cloneSpell;
										m_nextSpellLine = line;
									}
									Out.SendMessage("You prepare a secondary spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
								}
								return;
							}
						}
						else if (m_runningSpellHandler is ChamberSpellHandler)
						{
							ChamberSpellHandler chamber = (ChamberSpellHandler)m_runningSpellHandler;
							if (IsMoving || IsStrafing)
							{
								m_runningSpellHandler = null;
								return;
							}
							if (spell.IsPrimary)
							{
								if (spell.SpellType == "Bolt" && !chamber.Spell.AllowBolt)
								{
									Out.SendMessage("This spell cannot be stored in this chamber.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
									return;
								}
								if (chamber.PrimarySpell == null)
								{
									Spell cloneSpell = spell.Copy();
									cloneSpell.InChamber = true;
									cloneSpell.CostPower = false;
									chamber.PrimarySpell = cloneSpell;
									chamber.PrimarySpellLine = line;
									Out.SendMessage("You load " + spell.Name + " into your " + ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									Out.SendMessage("Select the second spell for your " + ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									Out.SendMessage("This spell cannot be stored in this chamber.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
								}
							}
							else if (spell.IsSecondary)
							{
								if (chamber.PrimarySpell != null)
								{
									if (chamber.SecondarySpell == null)
									{
										Spell cloneSpell = spell.Copy();
										cloneSpell.CostPower = false;
										cloneSpell.InChamber = true;
										cloneSpell.OverrideRange = chamber.PrimarySpell.Range;
										chamber.SecondarySpell = cloneSpell;
										chamber.SecondarySpellLine = line;
										Out.SendMessage("You load " + spell.Name + " into your " + ((ChamberSpellHandler)m_runningSpellHandler).Spell.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

									}
									else
									{
										Out.SendMessage("You have already chosen your spells for this chamber.", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

									}
								}
								else
								{
									Out.SendMessage("You must store a primary spell first!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);

								}
							}

						}
						else if (!(m_runningSpellHandler is ChamberSpellHandler) && spell.SpellType == "Chamber")
						{
							Out.SendMessage("You may not ready this spell as a followup!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
							return;
						}
					}
				}
				ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, spell, line);
				if (spellhandler != null)
				{
					if (spell.CastTime > 0)
					{
						GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Chamber", spell.Name);

						if (effect != null && spell.Name == effect.Spell.Name)
						{
							spellhandler.CastSpell();
						}
						else
						{
							if (spellhandler is ChamberSpellHandler && m_runningSpellHandler == null)
							{
								((ChamberSpellHandler)spellhandler).EffectSlot = ChamberSpellHandler.GetEffectSlot(spellhandler.Spell.Name);
								m_runningSpellHandler = spellhandler;
								m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
								spellhandler.CastSpell();
							}
							else if (m_runningSpellHandler == null)
							{
								m_runningSpellHandler = spellhandler;
								m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
								spellhandler.CastSpell();
							}
						}
					}
					else
					{
						if (spell.IsSecondary)
						{
							GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "Powerless");
							if (effect == null)
								effect = SpellHandler.FindEffectOnTarget(this, "Range");
							if (effect == null)
								effect = SpellHandler.FindEffectOnTarget(this, "Uninterruptable");

							if (m_runningSpellHandler == null && effect == null)
								Out.SendMessage("You cannot cast this spell directly!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
							else if (m_runningSpellHandler != null)
							{
								if (m_runningSpellHandler.Spell.IsPrimary)
								{
									lock (m_spellQueueAccessMonitor)
									{
										if (SpellQueue && !(m_runningSpellHandler is ChamberSpellHandler))
										{
											Out.SendMessage("You prepare this as a secondary spell!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
											m_nextSpell = spell;
											m_nextSpellLine = line;
										}
									}
								}
								else if (!(m_runningSpellHandler is ChamberSpellHandler))
									Out.SendMessage("You cannot cast this spell directly!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
							}
							else if (effect != null)
							{
								Spell cloneSpell = null;
								if (effect.SpellHandler is PowerlessSpellHandler)
								{
									cloneSpell = spell.Copy();
									cloneSpell.CostPower = false;
									spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
									spellhandler.CastSpell();
									effect.Cancel(false);
								}
								else if (effect.SpellHandler is RangeSpellHandler)
								{
									cloneSpell = spell.Copy();
									cloneSpell.CostPower = false;
									cloneSpell.OverrideRange = effect.Spell.Range;
									spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
									spellhandler.CastSpell();
									effect.Cancel(false);
								}
								else if (effect.SpellHandler is UninterruptableSpellHandler)
								{
									cloneSpell = spell.Copy();
									cloneSpell.CostPower = false;
									spellhandler = ScriptMgr.CreateSpellHandler(this, cloneSpell, line);
									spellhandler.CastSpell();
									effect.Cancel(false);
								}
							}
						}
						else
							spellhandler.CastSpell();
					}
				}
				else
				{
					Out.SendMessage(spell.Name + " not implemented yet (" + spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
		}

		public void CastSpell(SpellCastingAbilityHandler ab)
		{
			ISpellHandler spellhandler = ScriptMgr.CreateSpellHandler(this, ab.Spell, ab.SpellLine);
			if (spellhandler != null)
			{
				m_runningSpellHandler = spellhandler;
				m_runningSpellHandler.CastingCompleteEvent += new CastingCompleteCallback(OnAfterSpellCastSequence);
				spellhandler.Ability = ab;
				spellhandler.CastSpell();
			}
			else
			{
				Out.SendMessage(ab.Spell.Name + " not implemented yet (" + ab.Spell.SpellType + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Realm Abilities
		/// <summary>
		/// This is the timer used to count time when a player casts a RA
		/// </summary>
		private RegionTimer m_realmAbilityCastTimer;

		/// <summary>
		/// Get and set the RA cast timer
		/// </summary>
		public RegionTimer RealmAbilityCastTimer
		{
			get { return m_realmAbilityCastTimer; }
			set { m_realmAbilityCastTimer = value; }
		}

		/// <summary>
		/// Does the player is casting a realm ability
		/// </summary>
		public bool IsCastingRealmAbility
		{
			get { return (m_realmAbilityCastTimer != null && m_realmAbilityCastTimer.IsAlive); }
		}
		#endregion

		#region Vault/Money/Items/Trading/UseSlot/ApplyPoison

		private GameHouseVault m_activeVault;

		/// <summary>
		/// The currently active house vault.
		/// </summary>
		public GameHouseVault ActiveVault
		{
			get { return m_activeVault; }
			set { m_activeVault = value; }
		}

		/// <summary>
		/// Property that holds tick when charged item was used last time
		/// </summary>
		public const string LAST_CHARGED_ITEM_USE_TICK = "LastChargedItemUsedTick";
		public const string ITEM_USE_DELAY = "ItemUseDelay";
		public const string LAST_POTION_ITEM_USE_TICK = "LastPotionItemUsedTick";

		/// <summary>
		/// Called when this player receives a trade item
		/// </summary>
		/// <param name="source">the source of the item</param>
		/// <param name="item">the item</param>
		/// <returns>true to accept, false to deny the item</returns>
		public virtual bool ReceiveTradeItem(GamePlayer source, InventoryItem item)
		{
			if (source == null || item == null || source == this)
				return false;

			lock (m_LockObject)
			{
				lock (source)
				{
					if ((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
					{
						if (TradeWindow != null)
						{
							GamePlayer partner = TradeWindow.Partner;
							if (partner == null)
							{
								source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else if (source.TradeWindow != null)
						{
							GamePlayer sourceTradePartner = source.TradeWindow.Partner;
							if (sourceTradePartner == null)
							{
								source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveTradeItem.StillSelfcrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						return false;
					}
					if (item.IsTradable == false && source.Client.Account.PrivLevel < 2 && TradeWindow.Partner.Client.Account.PrivLevel < 2)
					{
						source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveTradeItem.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return false;
					}

					if (!source.TradeWindow.AddItemToTrade(item))
					{
						source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveTradeItem.CantTrade"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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

			lock (m_LockObject)
			{
				lock (source)
				{
					if ((TradeWindow != null && source != TradeWindow.Partner) || (TradeWindow == null && !OpenTrade(source)))
					{
						if (TradeWindow != null)
						{
							GamePlayer partner = TradeWindow.Partner;
							if (partner == null)
							{
								source.Out.SendMessage(Name + " is still selfcrafting.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								source.Out.SendMessage(Name + " is still trading with " + partner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						else if (source.TradeWindow != null)
						{
							GamePlayer sourceTradePartner = source.TradeWindow.Partner;
							if (sourceTradePartner == null)
							{
								source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveTradeItem.StillSelfcrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
		/// Gets the money value this player owns
		/// </summary>
		/// <returns></returns>
		public virtual long GetCurrentMoney()
		{
			return Money.GetMoney(PlayerCharacter.Mithril, PlayerCharacter.Platinum, PlayerCharacter.Gold, PlayerCharacter.Silver, PlayerCharacter.Copper);
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
			long newMoney = GetCurrentMoney() + money;

			PlayerCharacter.Copper = Money.GetCopper(newMoney);
			PlayerCharacter.Silver = Money.GetSilver(newMoney);
			PlayerCharacter.Gold = Money.GetGold(newMoney);
			PlayerCharacter.Platinum = Money.GetPlatinum(newMoney);
			PlayerCharacter.Mithril = Money.GetMithril(newMoney);

			Out.SendUpdateMoney();

			if (messageFormat != null)
			{
				Out.SendMessage(string.Format(messageFormat, Money.GetString(money)), ct, cl);
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
			if (money > GetCurrentMoney())
				return false;

			long newMoney = GetCurrentMoney() - money;

			PlayerCharacter.Mithril = Money.GetMithril(newMoney);
			PlayerCharacter.Platinum = Money.GetPlatinum(newMoney);
			PlayerCharacter.Gold = Money.GetGold(newMoney);
			PlayerCharacter.Silver = Money.GetSilver(newMoney);
			PlayerCharacter.Copper = Money.GetCopper(newMoney);

			Out.SendUpdateMoney();

			if (messageFormat != null && money != 0)
			{
				Out.SendMessage(string.Format(messageFormat, Money.GetString(money)), ct, cl);
			}
			return true;
		}

		private InventoryItem m_useItem;

		/// <summary>
		/// The item the player is trying to use.
		/// </summary>
		public InventoryItem UseItem
		{
			get { return m_useItem; }
			set { m_useItem = value; }
		}

		/// <summary>
		/// Called when the player uses an inventory in a slot
		/// eg. by clicking on the icon in the qickbar dragged from a slot
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="type">Which /use command was used (0=simple click on icon, 1=use, 2=/use2)</param>
		public virtual void UseSlot(int slot, int type)
		{
			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantFire"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			lock (Inventory)
			{
				InventoryItem useItem = Inventory.GetItem((eInventorySlot)slot);
				UseItem = useItem;

				if (useItem == null)
				{
					if ((slot >= Slot.FIRSTQUIVER) && (slot <= Slot.FOURTHQUIVER))
					{
						Out.SendMessage("The quiver slot " + (slot - (Slot.FIRSTQUIVER) + 1) + " is empty!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						// don't allow using empty slots
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.IllegalSourceObject", slot), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return;
				}
				if (useItem.Item_Type != Slot.RANGED && (slot != Slot.HORSE || type != 0))
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.AttemptToUse", useItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}

				#region Non-backpack/vault slots

				switch (slot)
				{
					case Slot.HORSEARMOR:
					case Slot.HORSEBARDING:
						return;
					case Slot.HORSE:
						if (type == 0)
						{
							if (IsOnHorse)
								IsOnHorse = false;
							else
							{
								if (Level < useItem.Level)
								{
									Out.SendMessage("You must have " + useItem.Level + " level for summon this horse", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (!IsAlive)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantMountWhileDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (Steed != null)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.MustDismountBefore"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (CurrentZone.IsDungeon)
								{
									Out.SendMessage("You cannot summon a mount in a dungeon!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (CurrentRegion.IsRvR && !ActiveHorse.IsSummonRvR)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantSummonRvR"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (IsMoving)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantMountMoving"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (IsSitting)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantCallMountSeated"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (IsStealthed)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantMountSteath"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (InCombat)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantMountCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								if (IsSummoningMount)
								{
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.StopCallingMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
									StopWhistleTimers();
									return;
								}
								Out.SendTimerWindow("Summoning Mount", 5);
								foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
									plr.Out.SendEmoteAnimation(this, eEmote.Horse_whistle);
								// vampiir ~
								GameSpellEffect effects = SpellHandler.FindEffectOnTarget(this, "VampiirSpeedEnhancement");
								GameSpellEffect effect = SpellHandler.FindEffectOnTarget(this, "SpeedEnhancement");
								if (effects != null)
									effects.Cancel(false);
								if (effect != null)
									effect.Cancel(false);
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.WhistleMount"), eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
								m_whistleMountTimer = new RegionTimer(this);
								m_whistleMountTimer.Callback = new RegionTimerCallback(WhistleMountTimerCallback);
								m_whistleMountTimer.Start(5000);
							}
						}
						break;
					case Slot.RIGHTHAND:
					case Slot.LEFTHAND:
						if (ActiveWeaponSlot == eActiveWeaponSlot.Standard)
							break;
						SwitchWeapon(eActiveWeaponSlot.Standard);
						Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
						return;

					case Slot.TWOHAND:
						if (ActiveWeaponSlot == eActiveWeaponSlot.TwoHanded)
							break;
						SwitchWeapon(eActiveWeaponSlot.TwoHanded);
						Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
						return;

					case Slot.RANGED:
						bool newAttack = false;
						if (ActiveWeaponSlot != eActiveWeaponSlot.Distance)
						{
							SwitchWeapon(eActiveWeaponSlot.Distance);
						}
						else if (!AttackState)
						{
							StartAttack(TargetObject);
							newAttack = true;
						}

						//Clean up range attack state/type if we are not in combat mode
						//anymore
						if (!AttackState)
						{
							RangeAttackState = eRangeAttackState.None;
							RangeAttackType = eRangeAttackType.Normal;
						}
						if (!newAttack && RangeAttackState != eRangeAttackState.None)
						{
							if (RangeAttackState == eRangeAttackState.ReadyToFire)
							{
								RangeAttackState = eRangeAttackState.Fire;
								m_attackAction.Start(1);
							}
							else if (RangeAttackState == eRangeAttackState.Aim)
							{
								if (!TargetInView)
								{
									// Don't store last target if it's not visible
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantSeeTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									if (m_rangeAttackTarget.Target == null)
									{
										//set new target only if there was no target before
										RangeAttackTarget = TargetObject;
									}

									RangeAttackState = eRangeAttackState.AimFire;
									Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.AutoReleaseShot"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
							}
							else if (RangeAttackState == eRangeAttackState.AimFire)
							{
								RangeAttackState = eRangeAttackState.AimFireReload;
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.AutoReleaseShotReload"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else if (RangeAttackState == eRangeAttackState.AimFireReload)
							{
								RangeAttackState = eRangeAttackState.Aim;
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.NoAutoReleaseShotReload"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
						break;
					case Slot.FIRSTQUIVER: SwitchQuiver(eActiveQuiverSlot.First, false); break;
					case Slot.SECONDQUIVER: SwitchQuiver(eActiveQuiverSlot.Second, false); break;
					case Slot.THIRDQUIVER: SwitchQuiver(eActiveQuiverSlot.Third, false); break;
					case Slot.FOURTHQUIVER: SwitchQuiver(eActiveQuiverSlot.Fourth, false); break;
				}

				#endregion

				if (useItem.SpellID != 0 || useItem.SpellID1 != 0 || useItem.PoisonSpellID != 0) // don't return without firing events
				{
					if (IsSitting)
					{
						Out.SendMessage("You can't use an item while sitting!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}

					// Item with a non-charge ability.

					if (useItem.Object_Type == (int)eObjectType.Magical
						&& useItem.Item_Type == (int)eInventorySlot.FirstBackpack
						&& useItem.SpellID > 0
						&& useItem.MaxCharges == 0)
					{
						int cooldown = useItem.CanUseAgainIn;
						if (cooldown > 0)
						{
							int minutes = cooldown / 60;
							int seconds = cooldown % 60;
							Out.SendMessage(String.Format("You must wait {0} to discharge this item!",
								(minutes <= 0)
									? String.Format("{0} more seconds", seconds)
									: String.Format("{0} more minutes and {1} seconds",
										minutes, seconds)),
								eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							if (UseMagicalItem(useItem, type))
								useItem.CanUseAgainIn = useItem.CanUseEvery;
						}
						return;
					}

					// Artifacts don't require charges.

					if ((type < 2 && useItem.SpellID > 0 && useItem.Charges < 1 && !(useItem is InventoryArtifact)) ||
						(type == 2 && useItem.SpellID1 > 0 && useItem.Charges1 < 1 && !(useItem is InventoryArtifact)) ||
						(useItem.PoisonSpellID > 0 && useItem.PoisonCharges < 1))
					{
						Out.SendMessage("The " + useItem.Name + " is out of charges.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return;
					}
					else
					{
						if (useItem.Object_Type == (int)eObjectType.Poison)
						{
							InventoryItem mainHand = AttackWeapon;
							InventoryItem leftHand = Inventory.GetItem(eInventorySlot.LeftHandWeapon);
							if (mainHand != null && mainHand.PoisonSpellID == 0)
							{
								ApplyPoison(useItem, mainHand);
							}
							else if (leftHand != null && leftHand.PoisonSpellID == 0)
							{
								ApplyPoison(useItem, leftHand);
							}
						}
						else if (useItem.Object_Type == (int)eObjectType.Magical &&
							(useItem.Item_Type == (int)eInventorySlot.FirstBackpack || useItem.Item_Type == 41))
						{
							long lastPotionItemUseTick = TempProperties.getLongProperty(LAST_POTION_ITEM_USE_TICK, 0L);
							long changeTime = CurrentRegion.Time - lastPotionItemUseTick;
							if (Client.Account.PrivLevel == 1 && changeTime < 60000) //1 minutes reuse timer
							{
								Out.SendMessage("You must wait " + (60000 - changeTime) / 1000 + " more second before use potion!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return;
							}
							else
							{
								SpellLine potionEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Potions_Effects);
								if (useItem.Item_Type == 41)
									potionEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);

								if (potionEffectLine != null)
								{
									IList spells = SkillBase.GetSpellList(potionEffectLine.KeyName);
									if (spells != null)
									{
										foreach (Spell spell in spells)
										{
											if (spell.ID == useItem.SpellID)
											{
												if (spell.Level <= Level)
												{
													if (spell.CastTime > 0 && AttackState)
													{
														Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.CantUseInCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
													}
													else
													{
														ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, potionEffectLine);
														if (spellHandler != null)
														{
															Stealth(false);
															if (useItem.Item_Type == 40)
																Emote(eEmote.Drink);
															spellHandler.StartSpell(TargetObject as GameLiving);
															if (useItem.Count > 1)
																Inventory.RemoveCountFromStack(useItem, 1);
															else
															{
																useItem.Charges--;
																if (useItem.Charges < 1) Inventory.RemoveCountFromStack(useItem, 1);
															}
															Out.SendMessage(useItem.GetName(0, false) + " has been used.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
															TempProperties.setProperty(LAST_POTION_ITEM_USE_TICK, CurrentRegion.Time);
														}
														else
														{
															Out.SendMessage("Potion effect ID " + spell.ID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
														}
													}
												}
												else
													Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.NotEnouthPower"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
												break;
											}
										}
									}
								}
							}
						}
						else if (type > 0)
						{
							if (!(new ArrayList(Inventory.EquippedItems).Contains(useItem)))
							{
								Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UseSlot.Can'tUseFromBackpack"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
							else
							{
								long lastChargedItemUseTick = TempProperties.getLongProperty(LAST_CHARGED_ITEM_USE_TICK, 0L);
								long changeTime = CurrentRegion.Time - lastChargedItemUseTick;
								long delay = TempProperties.getLongProperty(ITEM_USE_DELAY, 0L);
								if (Client.Account.PrivLevel == 1 && changeTime < delay) //2 minutes reuse timer
								{
									Out.SendMessage("You must wait " + (delay - changeTime) / 1000 + " more second before discharge another object!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
									return;
								}
								else
								{
									SpellLine chargeEffectLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Effects);

									if (chargeEffectLine != null)
									{
										if (type == 1) //use1
										{
											Spell spell = SkillBase.GetSpellByID(useItem.SpellID);
											if (spell.Level <= Level)
											{
												ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, chargeEffectLine);
												if (spellHandler != null)
												{
													if (IsOnHorse && !spellHandler.HasPositiveEffect)
														IsOnHorse = false;
													Stealth(false);
													spellHandler.CastSpell();
													if (useItem.MaxCharges > 0)
														useItem.Charges--;
													TempProperties.setProperty(LAST_CHARGED_ITEM_USE_TICK, CurrentRegion.Time);
													TempProperties.setProperty(ITEM_USE_DELAY, (long)(60000 * 2));
												}
												else
												{
													Out.SendMessage("Charge effect ID " + spell.ID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												}
											}
											else
												Out.SendMessage("You are not powerful enough to use this item's spell.", eChatType.CT_System, eChatLoc.CL_SystemWindow);


										}
										else if (type == 2) //use2
										{
											Spell spell = SkillBase.GetSpellByID(useItem.SpellID1);
											if (spell.Level <= Level)
											{
												ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, chargeEffectLine);
												if (spellHandler != null)
												{
													if (IsOnHorse && !spellHandler.HasPositiveEffect)
														IsOnHorse = false;
													Stealth(false);
													spellHandler.CastSpell();
													if (useItem.MaxCharges1 > 0)
														useItem.Charges1--;
													TempProperties.setProperty(LAST_CHARGED_ITEM_USE_TICK, CurrentRegion.Time);
													TempProperties.setProperty(ITEM_USE_DELAY, (long)(60000 * 2));
												}
												else
												{
													Out.SendMessage("Charge effect ID " + spell.ID + " is not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
												}
											}
											else
												Out.SendMessage("You are not powerful enough to use this item's spell.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
										}
									}
								}
							}
						}
					}
				}
				// notify event handlers about used slot
				Notify(GamePlayerEvent.UseSlot, this, new UseSlotEventArgs(slot, type));
			}
		}

		/// <summary>
		/// Use a magical item's spell.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="type"></param>
		protected bool UseMagicalItem(InventoryItem item, int type)
		{
			SpellLine itemSpellLine = SkillBase.GetSpellLine(GlobalSpellsLines.Item_Spells);
			if (itemSpellLine == null)
				return false;

			if (type == 1 || type == 0) //use1
			{
				Spell spell = SkillBase.GetSpellByID(item.SpellID);
				if (spell.Level > Level)
				{
					Out.SendMessage("You are not powerful enough to use this item's spell.",
						eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				Out.SendMessage(String.Format("You use {0}.", item.GetName(0, false)),
					eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

				ISpellHandler spellHandler = ScriptMgr.CreateSpellHandler(this, spell, itemSpellLine);
				if (spellHandler == null)
					return false;

				if (IsOnHorse && !spellHandler.HasPositiveEffect)
					IsOnHorse = false;
				Stealth(false);
				if (spellHandler.CheckBeginCast(TargetObject as GameLiving))
					spellHandler.CastSpell();
				return true;
			}
			return false;
		}

		/// <summary>
		/// Apply poison to weapon
		/// </summary>
		/// <param name="poisonPotion"></param>
		/// <param name="toItem"></param>
		/// <returns>true if applied</returns>
		public bool ApplyPoison(InventoryItem poisonPotion, InventoryItem toItem)
		{
			if (poisonPotion == null || toItem == null) return false;
			int envenomSpec = GetModifiedSpecLevel(Specs.Envenom);
			if (envenomSpec < 1)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ApplyPoison.CantUsePoisons"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (!GlobalConstants.IsWeapon(toItem.Object_Type))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ApplyPoison.PoisonsAppliedWeapons"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (!HasAbilityToUseItem(toItem))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ApplyPoison.CantPoisonWeapon"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (envenomSpec < poisonPotion.Level)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ApplyPoison.CantUsePoison"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (InCombat)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.ApplyPoison.CantApplyRecentCombat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (toItem.PoisonSpellID != 0)
			{
				bool canApply = false;
				SpellLine poisonLine = SkillBase.GetSpellLine(GlobalSpellsLines.Mundane_Poisons);
				if (poisonLine != null)
				{
					IList spells = SkillBase.GetSpellList(poisonLine.KeyName);
					if (spells != null)
					{
						foreach (Spell spl in spells)
						{
							if (spl.ID == toItem.PoisonSpellID)
							{
								canApply = true;
								break;
							}
						}
					}
				}
				if (canApply == false)
				{
					Out.SendMessage(string.Format("You can't poison your {0}!", toItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
			}

			//			Apply poison effect to weapon
			toItem.PoisonCharges = poisonPotion.PoisonCharges;
			toItem.PoisonMaxCharges = poisonPotion.PoisonMaxCharges;
			toItem.PoisonSpellID = poisonPotion.PoisonSpellID;
			Inventory.RemoveCountFromStack(poisonPotion, 1);
			Out.SendMessage(string.Format("You apply {0} to {1}.", poisonPotion.GetName(0, false), toItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);

			return true;
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

			eChatType type = eChatType.CT_Send;
			if (source.Client.Account.PrivLevel > 1)
				type = eChatType.CT_Staff;

			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SendReceive.Sends", source.Name, str), type, eChatLoc.CL_ChatWindow);
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SendReceive.FalseLanguage", source.Name), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				return true;
			}

			string afkmessage = TempProperties.getProperty(AFK_MESSAGE, null);
			if (afkmessage != null)
			{
				if (afkmessage == "")
				{
					source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SendReceive.Afk", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					source.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SendReceive.AfkMessage", Name, afkmessage), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
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
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Send.target.DontUnderstandYou", target.Name), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
				return false;
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Send.YouSendTo", str, target.Name), eChatType.CT_Send, eChatLoc.CL_ChatWindow);
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
			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SayReceive.Says", source.GetName(0, false), str), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			else
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SayReceive.FalseLanguage", source.GetName(0, false)), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
			return true;
		}

		/// <summary>
		/// Call this function to make the player say something
		/// </summary>
		/// <param name="str">string to say</param>
		/// <returns>true if said successfully</returns>
		public override bool Say(string str)
		{
			if (!GameServer.ServerRules.IsAllowedToSpeak(this, "talk"))
				return false;
			if (!base.Say(str))
				return false;
			Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Say.YouSay", str), eChatType.CT_Say, eChatLoc.CL_ChatWindow);
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
			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
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
			if (!GameServer.ServerRules.IsAllowedToSpeak(this, "yell"))
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
			if (GameServer.ServerRules.IsAllowedToUnderstand(source, this))
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
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Whisper.SelectTarget"), eChatType.CT_System, eChatLoc.CL_ChatWindow);
				return false;
			}
			if (!GameServer.ServerRules.IsAllowedToSpeak(this, "whisper"))
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
		/// Holds the GameLiving that is the steed of this player as weakreference
		/// </summary>
		protected WeakReference m_steed;
		/// <summary>
		/// Holds the Steed of this player
		/// </summary>
		public GameNPC Steed
		{
			get { return m_steed.Target as GameNPC; }
			set { m_steed.Target = value; }
		}

		/// <summary>
		/// Delegate callback to be called when the player
		/// tries to mount a steed
		/// </summary>
		public delegate bool MountSteedHandler(GamePlayer rider, GameNPC steed, bool forced);

		/// <summary>
		/// Event will be fired whenever the player tries to
		/// mount a steed
		/// </summary>
		public event MountSteedHandler OnMountSteed;
		/// <summary>
		/// Clears all MountSteed handlers
		/// </summary>
		public void ClearMountSteedHandlers()
		{
			OnMountSteed = null;
		}

		/// <summary>
		/// Mounts the player onto a steed
		/// </summary>
		/// <param name="steed">the steed to mount</param>
		/// <param name="forced">true if the mounting can not be prevented by handlers</param>
		/// <returns>true if mounted successfully or false if not</returns>
		public virtual bool MountSteed(GameNPC steed, bool forced)
		{
			if (Steed != null)
			{
				if (!DismountSteed(forced))
					return false;
			}

			if (OnMountSteed != null && !OnMountSteed(this, steed, forced) && !forced)
				return false;

			if (!steed.RiderMount(this, forced) && !forced)
				return false;

			if (IsOnHorse)
				IsOnHorse = false;

			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRiding(this, steed, false);
			}
			return true;
		}

		/// <summary>
		/// Delegate callback to be called whenever the player tries
		/// to dismount from a steed
		/// </summary>
		public delegate bool DismountSteedHandler(GamePlayer rider, GameLiving steed, bool forced);

		/// <summary>
		/// Event will be fired whenever the player tries to dismount
		/// from a steed
		/// </summary>
		public event DismountSteedHandler OnDismountSteed;
		/// <summary>
		/// Clears all DismountSteed handlers
		/// </summary>
		public void ClearDismountSteedHandlers()
		{
			OnDismountSteed = null;
		}

		/// <summary>
		/// Dismounts the player from it's steed.
		/// </summary>
		/// <param name="forced">true if the dismounting should not be prevented by handlers</param>
		/// <returns>true if the dismount was successful, false if not</returns>
		public virtual bool DismountSteed(bool forced)
		{
			if (Steed == null)
				return false;
			if (OnDismountSteed != null && !OnDismountSteed(this, Steed, forced) && !forced)
				return false;
			GameObject steed = Steed;
			if (!Steed.RiderDismount(forced, this) && !forced)
				return false;

			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRiding(this, steed, true);
			}
			return true;
		}

		/// <summary>
		/// Returns if the player is riding or not
		/// </summary>
		/// <returns>true if on a steed, false if not</returns>
		public virtual bool IsRiding
		{
			get { return Steed != null; }
		}

		public void SwitchSeat(int slot)
		{
			if (Steed == null)
				return;

			if (Steed.Riders[slot] != null)
				return;

			Out.SendMessage("You switch to seat " + slot + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);

			GameNPC steed = Steed;
			steed.RiderDismount(true, this);
			steed.RiderMount(this, true, slot);
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendRiding(this, steed, false);
			}
		}

		#endregion

		#region Add/Move/Remove

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
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player != this)
					player.Out.SendPlayerCreate(this);
			}
			UpdateEquipmentAppearance();

			// display message
			if (SpecPointsOk == false)
			{
				Out.SendMessage("Your Specs points total was incorrect. Now corrected, please train again.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				SpecPointsOk = true;
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
			if (ObjectState == eObjectState.Active)
			{
				DismountSteed(true);
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != this)
						player.Out.SendObjectRemove(this);
				}
			}
			if (!base.RemoveFromWorld()) return false;
			if (m_pvpInvulnerabilityTimer != null)
			{
				m_pvpInvulnerabilityTimer.Stop();
				m_pvpInvulnerabilityTimer = null;
			}
			Diving(waterBreath.Normal);
			if (IsOnHorse)
				IsOnHorse = false;
			return true;
		}

		/// <summary>
		/// Marks this player as deleted
		/// </summary>
		public override void Delete()
		{
			string[] friendList = new string[]
				{
					Name
				};
			foreach (GameClient clientp in WorldMgr.GetAllPlayingClients())
			{
				if (clientp.Player.Friends.Contains(Name))
					clientp.Out.SendRemoveFriends(friendList);
			}
			if (Group != null)
			{
				Group.RemoveMember(this);
			}
			BattleGroup mybattlegroup = (BattleGroup)this.TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
			if (mybattlegroup != null)
			{
				mybattlegroup.RemoveBattlePlayer(this);
			}
			if (m_guild != null)
			{
				m_guild.RemoveOnlineMember(this);
			}
			GroupMgr.RemovePlayerLooking(this);
			if (log.IsDebugEnabled)
				log.Debug("(" + Name + ") player.Delete");
			base.Delete();
		}

		/// <summary>
		/// The property to save debug mode on region change
		/// </summary>
		public const string DEBUG_MODE_PROPERTY = "Player.DebugMode";

		/// <summary>
		/// This function moves a player to a specific region and
		/// specific coordinates.
		/// </summary>
		/// <param name="regionID">RegionID to move to</param>
		/// <param name="x">X target coordinate</param>
		/// <param name="y">Y target coordinate</param>
		/// <param name="z">Z target coordinate (0 to put player on floor)</param>
		/// <param name="heading">Target heading</param>
		/// <returns>true if move succeeded, false if failed</returns>
		public override bool MoveTo(ushort regionID, int x, int y, int z, ushort heading)
		{
			//if we are jumping somewhere away from our house not using house.Exit
			//we need to make the server know we have left the house
			if (CurrentHouse != null && CurrentHouse.RegionID != regionID)
			{
				InHouse = false;
				CurrentHouse = null;
			}
			//if we send a jump, we get off the horse
			if (IsOnHorse)
				IsOnHorse = false;
			//Get the destination region based on the ID
			Region rgn = WorldMgr.GetRegion(regionID);
			//If the region doesn't exist, return false or if they aren't allowed to zone here
			if (rgn == null || !GameServer.ServerRules.IsAllowedToZone(this, rgn))
				return false;
			//If the x,y inside this region don't point to a zone
			//return false
			if (rgn.GetZone(x, y) == null)
				return false;

			Diving(waterBreath.Normal);

			if (SiegeWeapon != null)
				SiegeWeapon.ReleaseControl();

			if (regionID != CurrentRegionID)
			{
				GameEventMgr.Notify(GamePlayerEvent.RegionChanging, this);
				if (!RemoveFromWorld())
					return false;
				//notify event
				CurrentRegion.Notify(RegionEvent.PlayerLeave, CurrentRegion, new RegionPlayerEventArgs(this));

				CancelAllConcentrationEffects(true);
				CommandNpcRelease();
			}
			else
			{
				//Just remove the player visible, but leave his OID intact!
				//If player doesn't change region
				DismountSteed(true);
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != this)
					{
						player.Out.SendObjectRemove(this);
					}
				}
			}

			//Remove the last update tick property, to prevent speedhack messages during zoning and teleporting!
			TempProperties.removeProperty(PlayerPositionUpdateHandler.LASTUPDATETICK);

			//Set the new destination
			//Current Speed = 0 when moved ... else X,Y,Z continue to be modified
			CurrentSpeed = 0;
			MovementStartTick = Environment.TickCount;
			int m_originalX = X;
			int m_originalY = Y;
			int m_originalZ = Z;
			X = x;
			Y = y;
			Z = z;
			Heading = heading;

			//If the destination is in another region
			if (regionID != CurrentRegionID)
			{
				//Set our new region
				CurrentRegionID = regionID;
				//Send the region update packet, the rest will be handled
				//by the packethandlers
				Out.SendRegionChanged();
			}
			else
			{
				//Add the player to the new coordinates
				Out.SendPlayerJump(false);
				LastNPCUpdate = Environment.TickCount;
				CurrentUpdateArray.SetAll(false);
				foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (WorldMgr.GetDistance(npc.X, npc.Y, npc.Z, m_originalX, m_originalY, m_originalZ) <= WorldMgr.VISIBILITY_DISTANCE)
						continue;

					Out.SendNPCCreate(npc);
					if (npc.Inventory != null)
						Out.SendLivingEquipmentUpdate(npc);
					//Send health update only if mob-health is not 100%
					if (npc.HealthPercent != 100)
						Out.SendObjectUpdate(npc);
					CurrentUpdateArray[npc.ObjectID - 1] = true;
				}
				//Create player visible to all others
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != this)
					{
						player.Out.SendPlayerCreate(this);
					}
				}
				UpdateEquipmentAppearance();

				if (this.IsUnderwater)
					this.IsDiving = true;

				if (ControlledNpc != null &&
					ControlledNpc.WalkState != eWalkState.Stay
					&& WorldMgr.CheckDistance(this, ControlledNpc.Body, 1000))
				{
					int tx, ty;
					GetSpotFromHeading(64, out tx, out ty);
					ControlledNpc.Body.MoveTo(CurrentRegionID, tx, ty, Z, (ushort)((this.Heading + 2048) % 4096), true);
				}
			}
			return true;
		}

		public bool MoveToInstance(Region rgn, int x, int y, int z, ushort heading)
		{
			if (IsOnHorse)
				IsOnHorse = false;
			//If the region doesn't exist, return false
			if (rgn == null)
			{
				log.Error("rgn is null in MoveToInstance");
				return false;
			}
			//If the x,y inside this region don't point to a zone
			//return false
			if (rgn.GetZone(x, y) == null)
			{
				log.Error("rgn.GetZone is null in MoveToInstance");
				return false;
			}

			Diving(waterBreath.Normal);

			if (SiegeWeapon != null)
				SiegeWeapon.ReleaseControl();

			if (rgn.ID != CurrentRegionID)
			{
				if (!RemoveFromWorld())
					return false;
				//notify event
				CurrentRegion.Notify(RegionEvent.PlayerLeave, CurrentRegion, new RegionPlayerEventArgs(this));

				CancelAllConcentrationEffects(true);
				CommandNpcRelease();
			}
			else
			{
				//Just remove the player visible, but leave his OID intact!
				//If player doesn't change region
				DismountSteed(true);
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != this)
					{
						player.Out.SendObjectRemove(this);
					}
				}
			}

			//Remove the last update tick property, to prevent speedhack messages during zoning and teleporting!
			TempProperties.removeProperty(PlayerPositionUpdateHandler.LASTUPDATETICK);

			//Set the new destination
			//Current Speed = 0 when moved ... else X,Y,Z continue to be modified
			CurrentSpeed = 0;
			MovementStartTick = Environment.TickCount;
			int m_originalX = X;
			int m_originalY = Y;
			int m_originalZ = Z;
			X = x;
			Y = y;
			Z = z;
			Heading = heading;

			//If the destination is in another region
			if (rgn != CurrentRegion)
			{
				//Set our new region
				CurrentRegion = rgn;
				//Send the region update packet, the rest will be handled
				//by the packethandlers
				Out.SendRegionChanged();
			}
			else
			{
				//Add the player to the new coordinates
				Out.SendPlayerJump(false);
				LastNPCUpdate = Environment.TickCount;
				CurrentUpdateArray.SetAll(false);
				foreach (GameNPC npc in GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (WorldMgr.GetDistance(npc.X, npc.Y, npc.Z, m_originalX, m_originalY, m_originalZ) <= WorldMgr.VISIBILITY_DISTANCE)
						continue;

					Out.SendNPCCreate(npc);
					if (npc.Inventory != null)
						Out.SendLivingEquipmentUpdate(npc);
					//Send health update only if mob-health is not 100%
					if (npc.HealthPercent != 100)
						Out.SendObjectUpdate(npc);
					CurrentUpdateArray[npc.ObjectID - 1] = true;
				}
				//Create player visible to all others
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player != this)
					{
						player.Out.SendPlayerCreate(this);
					}
				}
				UpdateEquipmentAppearance();

				if (this.IsUnderwater)
					this.IsDiving = true;
			}
			return true;
		}

		#endregion

		#region Group/Friendlist/guild

		private Guild m_guild;
		private DBRank m_guildRank;

		/// <summary>
		/// Gets or sets the player's guild
		/// </summary>
		public Guild Guild
		{
			get { return m_guild; }
			set
			{
				if (m_guild != null)
				{
					m_guild.RemoveOnlineMember(this);
				}
				m_guild = value;

				//update guild name for all players if client is playing
				if (ObjectState == eObjectState.Active)
				{
					Out.SendUpdatePlayer();
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						if (player != this)
						{
							player.Out.SendObjectRemove(this);
							player.Out.SendPlayerCreate(this);
							player.Out.SendLivingEquipmentUpdate(this);
						}
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the player's guild rank
		/// </summary>
		public DBRank GuildRank
		{
			get { return m_guildRank; }
			set
			{
				m_guildRank = value;
				if (value != null)
					PlayerCharacter.GuildRank = value.RankLevel;//maybe mistake here and need to change and make an index var
			}
		}

		/// <summary>
		/// Gets or sets the database guildid of this player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public string GuildID
		{
			get { return PlayerCharacter != null ? PlayerCharacter.GuildID : string.Empty; }
			set { if (PlayerCharacter != null) PlayerCharacter.GuildID = value; }
		}

		/// <summary>
		/// Gets or sets the player's guild flag
		/// (delegate to PlayerCharacter)
		/// </summary>
		public bool ClassNameFlag
		{
			get { return PlayerCharacter.FlagClassName; }
			set { PlayerCharacter.FlagClassName = value; }
		}

		/// <summary>
		/// true if this player is looking for a group
		/// </summary>
		protected bool m_lookingForGroup;
		/// <summary>
		/// true if this player want to receive loot with autosplit between members of group
		/// </summary>
		protected bool m_autoSplitLoot = true;

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

		/// <summary>
		/// Gets or sets the friends of this player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public ArrayList Friends
		{
			get
			{
				if (PlayerCharacter != null && PlayerCharacter.SerializedFriendsList != null)
					return new ArrayList(PlayerCharacter.SerializedFriendsList.Split(','));
				return new ArrayList(0);
			}
			set
			{
				if (PlayerCharacter == null) return;
				if (value == null)
					PlayerCharacter.SerializedFriendsList = "";
				else
					PlayerCharacter.SerializedFriendsList = String.Join(",", (string[])value.ToArray(typeof(string)));
				GameServer.Database.SaveObject(PlayerCharacter);
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

		#region X/Y/Z/Region/Realm/Position...

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		private IList m_currentAreas;

		/// <summary>
		/// Holds all areas this player is currently within
		/// </summary>
		public override IList CurrentAreas
		{
			get { return m_currentAreas; }
			set { m_currentAreas = value; }
		}

		/// <summary>
		/// Property that saves last maximum Z value
		/// </summary>
		public const string MAX_LAST_Z = "max_last_z";

		/// <summary>
		/// Property that saves zone on last postion update
		/// </summary>
		public const string LAST_POSITION_UPDATE_ZONE = "LastPositionUpdateZone";

		/// <summary>
		/// The base speed of the player
		/// </summary>
		public const int PLAYER_BASE_SPEED = 191;

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
		/// Gets the current position of this player
		/// </summary>
		public override int X
		{
			set
			{
				base.X = value;
				PlayerCharacter.Xpos = base.X;
			}
		}

		/// <summary>
		/// Gets the current position of this player
		/// </summary>
		public override int Y
		{
			set
			{
				base.Y = value;
				PlayerCharacter.Ypos = base.Y;
			}
		}

		/// <summary>
		/// Gets the current position of this player
		/// </summary>
		public override int Z
		{
			set
			{
				base.Z = value;
				PlayerCharacter.Zpos = base.Z;
			}
		}

		/// <summary>
		/// Gets or sets the current speed of this player
		/// </summary>
		public override int CurrentSpeed
		{
			set
			{
				base.CurrentSpeed = value;
				if (value != 0)
				{
					OnPlayerMove();
				}
			}
		}

		/// <summary>
		/// Gets or sets the region of this player
		/// </summary>
		public override Region CurrentRegion
		{
			set
			{
				base.CurrentRegion = value;
				PlayerCharacter.Region = CurrentRegionID;
			}
		}

		/// <summary>
		/// Holds the zone player was in after last position update
		/// </summary>
		private Zone m_lastPositionUpdateZone;

		/// <summary>
		/// Gets or sets the zone after last position update
		/// </summary>
		public Zone LastPositionUpdateZone
		{
			get { return m_lastPositionUpdateZone; }
			set { m_lastPositionUpdateZone = value; }
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
		/// Gets or sets the realm of this player
		/// </summary>
		public override eRealm Realm
		{
			get { return PlayerCharacter != null ? (eRealm)PlayerCharacter.Realm : base.Realm; }
			set
			{
				base.Realm = value;
				PlayerCharacter.Realm = (byte)value;
			}
		}

		/// <summary>
		/// Gets or sets the heading of this player
		/// </summary>
		public override ushort Heading
		{
			set
			{
				base.Heading = value;
				PlayerCharacter.Direction = value;

				if (AttackState && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
				{
					AttackData ad = TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
					if (ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
					{
						//Does the target can be attacked ?
						if (ad.Target != null && IsObjectInFront(ad.Target, 120) && WorldMgr.CheckDistance(this, ad.Target, AttackRange) && m_attackAction != null)
						{
							m_attackAction.Start(1);
						}
					}
				}
			}
		}

		protected bool m_climbing;
		/// <summary>
		/// Gets/sets the current climbing state
		/// </summary>
		public bool IsClimbing
		{
			get { return m_climbing; }
			set
			{
				if (value == m_climbing) return;
				m_climbing = value;
			}
		}

		protected bool m_swimming;
		/// <summary>
		/// Gets/sets the current swimming state
		/// </summary>
		public bool IsSwimming
		{
			get { return m_swimming; }
			set
			{
				if (value == m_swimming) return;
				m_swimming = value;
				Notify(GamePlayerEvent.SwimmingStatus, this);
			}
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
			set
			{
				if (value == m_stuckFlag) return;
				m_stuckFlag = value;
			}
		}


		public enum waterBreath : byte
		{
			Normal = 0,
			Holding = 1,
			Drowning = 2,
		}

		protected long m_beginDrowningTick;
		protected waterBreath m_currentWaterBreathState;

		protected int DrowningTimerCallback(RegionTimer callingTimer)
		{
			if (!IsAlive || ObjectState != eObjectState.Active)
				return 0;
			if (this.Client.Account.PrivLevel == 1)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.DrowningTimerCallback.CannotBreath"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.DrowningTimerCallback.Take5%Damage"), eChatType.CT_Damaged, eChatLoc.CL_SystemWindow);
				if (CurrentRegion.Time - m_beginDrowningTick > 15000) // 15 sec
				{
					TakeDamage(null, eDamageType.Natural, MaxHealth, 0);
					Out.SendCloseTimerWindow();
					return 0;
				}
				else
					TakeDamage(null, eDamageType.Natural, MaxHealth / 20, 0);
			}
			return 3000;
		}

		protected int HoldingBreathTimerCallback(RegionTimer callingTimer)
		{
			m_holdBreathTimer = null;
			Diving(waterBreath.Drowning);
			return 0;
		}

		protected RegionTimer m_drowningTimer;
		protected RegionTimer m_holdBreathTimer;
		/// <summary>
		/// The diving state of this player
		/// </summary>
		protected bool m_diving;
		/// <summary>
		/// Gets/sets the current diving state
		/// </summary>
		public bool IsDiving
		{
			get { return m_diving; }
			set
			{
				if (m_diving != value)
					if (value && !CanBreathUnderWater)
						Diving(waterBreath.Holding);
					else
						Diving(waterBreath.Normal);
				m_diving = value;
			}
		}

		protected bool m_canBreathUnderwater;
		public bool CanBreathUnderWater
		{
			get { return m_canBreathUnderwater; }
			set
			{
				m_canBreathUnderwater = value;
				if (!value && IsDiving)
					Diving(waterBreath.Holding);
				else
					Diving(waterBreath.Normal);
			}
		}

		public void Diving(waterBreath state)
		{
			//bool changeSpeed = false;
			if (m_currentWaterBreathState != state)
			{
				//changeSpeed = true;
				Out.SendCloseTimerWindow();
			}

			if (m_holdBreathTimer != null)
			{
				m_holdBreathTimer.Stop();
				m_holdBreathTimer = null;
			}
			if (m_drowningTimer != null)
			{
				m_drowningTimer.Stop();
				m_drowningTimer = null;
			}
			switch (state)
			{
				case waterBreath.Normal:
					break;
				case waterBreath.Holding:
					if (m_holdBreathTimer == null)
					{
						Out.SendTimerWindow("Holding Breath", 30);
						m_holdBreathTimer = new RegionTimer(this);
						m_holdBreathTimer.Callback = new RegionTimerCallback(HoldingBreathTimerCallback);
						m_holdBreathTimer.Start(30001);
					}
					break;
				case waterBreath.Drowning:
					m_beginDrowningTick = CurrentRegion.Time;
					if (m_drowningTimer == null)
					{
						Out.SendTimerWindow("Drowning", 15);
						m_drowningTimer = new RegionTimer(this);
						m_drowningTimer.Callback = new RegionTimerCallback(DrowningTimerCallback);
						m_drowningTimer.Start(1);
					}
					break;
			}
			m_currentWaterBreathState = state;
			//if (changeSpeed)
			//	Out.SendUpdateMaxSpeed();
		}

		/// <summary>
		/// The sitting state of this player
		/// </summary>
		protected bool m_sitting;
		/// <summary>
		/// Gets/sets the current sit state
		/// </summary>
		public override bool IsSitting
		{
			get { return m_sitting; }
			set
			{
				m_sitting = value;
				if (value)
				{
					if (IsCasting)
						m_runningSpellHandler.CasterMoves();
					if (AttackState && ActiveWeaponSlot == eActiveWeaponSlot.Distance)
					{
						string attackTypeMsg = "shot";
						if (AttackWeapon.Object_Type == (int)eObjectType.Thrown)
							attackTypeMsg = "throw";
						Out.SendMessage("You move and interrupt your " + attackTypeMsg + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						StopAttack();
					}
				}
			}
		}

		/// <summary>
		/// Gets or sets the max speed of this player
		/// (delegate to PlayerCharacter)
		/// </summary>
		public override int MaxSpeedBase
		{
			get { return PlayerCharacter != null ? PlayerCharacter.MaxSpeed : base.MaxSpeedBase; }
			set
			{
				base.MaxSpeedBase = value;
				PlayerCharacter.MaxSpeed = value;
			}
		}

		/// <summary>
		/// the moving state of this player
		/// </summary>
		public override bool IsMoving
		{
			get { return base.IsMoving || IsStrafing; }
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
			if (state == IsSprinting)
				return state;

			if (state)
			{
				// can't start sprinting with 10 endurance on 1.68 server
				if (Endurance <= 10)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sprint.TooFatigued"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (IsStealthed)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sprint.CantSprintHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				if (!IsAlive)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sprint.CantSprintDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				m_sprintEffect = new SprintEffect();
				m_sprintEffect.Start(this);
				Out.SendUpdateMaxSpeed();
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sprint.PrepareSprint"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return true;
			}
			else
			{
				m_sprintEffect.Stop();
				m_sprintEffect = null;
				Out.SendUpdateMaxSpeed();
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sprint.NoLongerReady"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
		public override bool IsStrafing
		{
			set
			{
				m_strafing = value;
				if (value)
				{
					OnPlayerMove();
				}
			}
			get { return m_strafing; }
		}

		public virtual void OnPlayerMove()
		{
			if (IsSitting)
			{
				Sit(false);
			}
			if (IsCasting)
			{
				m_runningSpellHandler.CasterMoves();
			}
			if (IsCastingRealmAbility)
			{
				Out.SendInterruptAnimation(this);
				Out.SendMessage("You move and interrupt your spellcast!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				RealmAbilityCastTimer.Stop();
				RealmAbilityCastTimer = null;
			}
			if (IsCrafting)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnPlayerMove.InterruptCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				CraftTimer.Stop();
				CraftTimer = null;
				Out.SendCloseTimerWindow();
			}
			if (IsSummoningMount)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OnPlayerMove.CannotCallMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				StopWhistleTimers();
			}
			if (AttackState)
			{
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					string attackTypeMsg = (AttackWeapon.Object_Type == (int)eObjectType.Thrown ? "throw" : "shot");
					Out.SendMessage("You move and interrupt your " + attackTypeMsg + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					StopAttack();
				}
				else
				{
					AttackData ad = TempProperties.getObjectProperty(LAST_ATTACK_DATA, null) as AttackData;
					if (ad != null && ad.IsMeleeAttack && (ad.AttackResult == eAttackResult.TargetNotVisible || ad.AttackResult == eAttackResult.OutOfRange))
					{
						//Does the target can be attacked ?
						if (ad.Target != null && IsObjectInFront(ad.Target, 120) && WorldMgr.CheckDistance(this, ad.Target, AttackRange) && m_attackAction != null)
						{
							m_attackAction.Start(1);
						}
					}
				}
			}
			//Notify the GameEventMgr of the moving player
			GameEventMgr.Notify(GamePlayerEvent.Moving, this);
		}

		/// <summary>
		/// Sits/Stands the player
		/// </summary>
		/// <param name="sit">True if sitting, otherwise false</param>
		public virtual void Sit(bool sit)
		{
			Sprint(false);
			if (IsSummoningMount)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.InterruptCallMount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				StopWhistleTimers();
			}
			if (IsSitting == sit)
			{
				if (sit)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.AlreadySitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				if (!sit)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.NotSitting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return; // already done
			}

			if (!IsAlive)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.CantSitDead"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsStunned)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.CantSitStunned"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (IsMezzed)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.CantSitMezzed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (sit && CurrentSpeed > 0)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.MustStandingStill"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (Steed != null || IsOnHorse)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.MustDismount"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (sit)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.YouSitDown"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.YouStandUp"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Sit.NoLongerWaitingQuit"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				//Stop praying if the player stands up...
				if (PrayState)
				{
					m_prayAction.Stop();
				}
			}
			//Update the client
			IsSitting = sit;
			UpdatePlayerStatus();
		}

		/// <summary>
		/// Sets the Living's ground-target Coordinates inside the current Region
		/// </summary>
		public override void SetGroundTarget(int groundX, int groundY, int groundZ)
		{
			base.SetGroundTarget(groundX, groundY, groundZ);
			Out.SendMessage(String.Format("You ground-target {0},{1},{2}", groundX, groundY, groundZ), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			if (SiegeWeapon != null)
				SiegeWeapon.SetGroundTarget(groundX, groundY, groundZ);
		}

		/// <summary>
		/// Holds unique locations array
		/// </summary>
		protected readonly GameLocation[] m_lastUniqueLocations;

		/// <summary>
		/// Gets unique locations array
		/// </summary>
		public GameLocation[] LastUniqueLocations
		{
			get { return m_lastUniqueLocations; }
		}

		#endregion

		#region Equipment/Encumberance

		/// <summary>
		/// Gets the total possible Encumberance
		/// </summary>
		public virtual int MaxEncumberance
		{
			get
			{
				double enc = (double)Strength;
				RAPropertyEnhancer ab = GetAbility(typeof(LifterAbility)) as RAPropertyEnhancer;
				if (ab != null)
					enc *= 1 + ((double)ab.Amount / 100);

				// Apply Sojourner ability
				if (this.GetSpellLine("Sojourner") != null)
				{
					enc *= 1.25;
				}

				// Apply Walord effect
				GameSpellEffect iBaneLordEffect = SpellHandler.FindEffectOnTarget((GameLiving)this, "Oppression");
				if (iBaneLordEffect != null)
					enc *= 1.00 - (iBaneLordEffect.Spell.Value * 0.01);

				return (int)enc;
			}
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
		protected bool m_overencumbered = true;

		/// <summary>
		/// Gets/Set the players Encumberance state
		/// </summary>
		public bool IsOverencumbered
		{
			get { return m_overencumbered; }
			set { m_overencumbered = value; }
		}

		/// <summary>
		/// Updates the appearance of the equipment this player is using
		/// </summary>
		public virtual void UpdateEquipmentAppearance()
		{
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player != this)
					player.Out.SendLivingEquipmentUpdate(this);
			}
		}

		/// <summary>
		/// Updates Encumberance and its effects
		/// </summary>
		public void UpdateEncumberance()
		{
			if (Inventory.InventoryWeight > MaxEncumberance)
			{
				if (IsOverencumbered == false)
				{
					IsOverencumbered = true;
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UpdateEncumberance.EncumberedMoveSlowly"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.UpdateEncumberance.Encumbered"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				Out.SendUpdateMaxSpeed();
			}
			else if (IsOverencumbered)
			{
				IsOverencumbered = false;
				Out.SendUpdateMaxSpeed();
			}
			Out.SendEncumberance();
		}

		/// <summary>
		/// Get the bonus names
		/// </summary>
		public string ItemBonusName(int BonusType)
		{
			string BonusName = "";

			if (BonusType == 1) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus1");//Strength
			if (BonusType == 2) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus2");//Dextery
			if (BonusType == 3) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus3");//Constitution
			if (BonusType == 4) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus4");//Quickness
			if (BonusType == 5) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus5");//Intelligence
			if (BonusType == 6) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus6");//Piety
			if (BonusType == 7) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus7");//Empathy
			if (BonusType == 8) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus8");//Charisma
			if (BonusType == 9) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus9");//Power
			if (BonusType == 10) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus10");//Hits
			if (BonusType == 11) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus11");//Body
			if (BonusType == 12) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus12");//Cold
			if (BonusType == 13) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus13");//Crush
			if (BonusType == 14) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus14");//Energy
			if (BonusType == 15) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus15");//Heat
			if (BonusType == 16) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus16");//Matter
			if (BonusType == 17) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus17");//Slash
			if (BonusType == 18) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus18");//Spirit
			if (BonusType == 19) BonusName = LanguageMgr.GetTranslation(Client, "GamePlayer.ItemBonusName.Bonus19");//Thrust
			return BonusName;
		}

		/// <summary>
		/// Adds magical bonuses whenever item was equipped
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">inventory</param>
		/// <param name="arguments"></param>
		protected virtual void OnItemEquipped(DOLEvent e, object sender, EventArgs arguments)
		{
			if (arguments is ItemEquippedArgs == false) return;
			InventoryItem item = ((ItemEquippedArgs)arguments).Item;
			if (item == null) return;
			if (item.Item_Type >= Slot.RIGHTHAND && item.Item_Type <= Slot.RANGED)
			{
				if (item.Hand == 1) // 2h
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.WieldBothHands", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (item.SlotPosition == Slot.LEFTHAND)
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.WieldLeftHand", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.WieldRightHand", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}

			if (item.Item_Type == (int)eInventorySlot.Horse)
			{
				if (item.SlotPosition == Slot.HORSE)
				{
					ActiveHorse.ID = (byte)(item.SPD_ABS == 0 ? 1 : item.SPD_ABS);
					ActiveHorse.Name = item.CrafterName;
				}
				return;
			}
			else if (item.Item_Type == (int)eInventorySlot.HorseArmor)
			{
				if (item.SlotPosition == Slot.HORSEARMOR)
				{
					//					Out.SendDebugMessage("Try apply horse armor.");
					ActiveHorse.Saddle = (byte)(item.DPS_AF);
				}
				return;
			}
			else if (item.Item_Type == (int)eInventorySlot.HorseBarding)
			{
				if (item.SlotPosition == Slot.HORSEBARDING)
				{
					//					Out.SendDebugMessage("Try apply horse barding.");
					ActiveHorse.Barding = (byte)(item.DPS_AF);
				}
				return;
			}

			if (!item.IsMagical) return;

			Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Magic", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

			if (item.Bonus1 != 0)
			{
				ItemBonus[item.Bonus1Type] += item.Bonus1;
				if (item.Bonus1Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus1Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus2 != 0)
			{
				ItemBonus[item.Bonus2Type] += item.Bonus2;
				if (item.Bonus2Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus2Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus3 != 0)
			{
				ItemBonus[item.Bonus3Type] += item.Bonus3;
				if (item.Bonus3Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus3Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus4 != 0)
			{
				ItemBonus[item.Bonus4Type] += item.Bonus4;
				if (item.Bonus4Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus4Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus5 != 0)
			{
				ItemBonus[item.Bonus5Type] += item.Bonus5;
				if (item.Bonus5Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus5Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus6 != 0)
			{
				ItemBonus[item.Bonus6Type] += item.Bonus6;
				if (item.Bonus6Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus6Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus7 != 0)
			{
				ItemBonus[item.Bonus7Type] += item.Bonus7;
				if (item.Bonus7Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus7Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus8 != 0)
			{
				ItemBonus[item.Bonus8Type] += item.Bonus8;
				if (item.Bonus8Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus8Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus9 != 0)
			{
				ItemBonus[item.Bonus9Type] += item.Bonus9;
				if (item.Bonus9Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus9Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus10 != 0)
			{
				ItemBonus[item.Bonus10Type] += item.Bonus10;
				if (item.Bonus10Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.Bonus10Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.ExtraBonus != 0)
			{
				ItemBonus[item.ExtraBonusType] += item.ExtraBonus;
				//if (item.ExtraBonusType < 20)
				//Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemEquipped.Increased", ItemBonusName(item.ExtraBonusType))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}

			if (ObjectState == eObjectState.Active)
			{
				// TODO: remove when properties system is finished
				Out.SendCharStatsUpdate();
				Out.SendCharResistsUpdate();
				Out.SendUpdateWeaponAndArmorStats();
				Out.SendEncumberance();
				Out.SendUpdatePlayerSkills();
				UpdatePlayerStatus();

				if (IsAlive)
				{
					if (Health < MaxHealth) StartHealthRegeneration();
					else if (Health > MaxHealth) Health = MaxHealth;

					if (Mana < MaxMana) StartPowerRegeneration();
					else if (Mana > MaxMana) Mana = MaxMana;

					if (Endurance < MaxEndurance) StartEnduranceRegeneration();
					else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
				}
			}
		}

		/// <summary>
		/// Removes magical bonuses whenever item was unequipped
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender">inventory</param>
		/// <param name="arguments"></param>
		protected virtual void OnItemUnequipped(DOLEvent e, object sender, EventArgs arguments)
		{
			if (arguments is ItemUnequippedArgs == false) return;
			InventoryItem item = ((ItemUnequippedArgs)arguments).Item;
			int prevSlot = ((ItemUnequippedArgs)arguments).PreviousSlotPosition;
			if (item == null) return;

			//			DOLConsole.WriteLine("unequipped item '" + item.Name + "' !");

			if (item.Item_Type >= Slot.RIGHTHAND && item.Item_Type <= Slot.RANGED)
			{
				if (item.Hand == 1) // 2h
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.BothHandsFree", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else if (prevSlot == Slot.LEFTHAND)
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.LeftHandFree", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.RightHandFree", item.GetName(0, false))), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}

			if (item.Item_Type == (int)eInventorySlot.Horse)
			{
				if (IsOnHorse)
					IsOnHorse = false;
				ActiveHorse.ID = 0;
				ActiveHorse.Name = "";
				//				Out.SendDebugMessage("Try unapply horse.");
				return;
			}
			else if (item.Item_Type == (int)eInventorySlot.HorseArmor)
			{
				ActiveHorse.Saddle = 0;
				//				Out.SendDebugMessage("Try unapply saddle.");
				return;
			}
			else if (item.Item_Type == (int)eInventorySlot.HorseBarding)
			{
				ActiveHorse.Barding = 0;
				//				Out.SendDebugMessage("Try unapply barding.");
				return;
			}

			if (!item.IsMagical) return;

			if (item.Bonus1 != 0)
			{
				ItemBonus[item.Bonus1Type] -= item.Bonus1;
				if (item.Bonus1Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus1Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus2 != 0)
			{
				ItemBonus[item.Bonus2Type] -= item.Bonus2;
				if (item.Bonus2Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus2Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus3 != 0)
			{
				ItemBonus[item.Bonus3Type] -= item.Bonus3;
				if (item.Bonus3Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus3Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus4 != 0)
			{
				ItemBonus[item.Bonus4Type] -= item.Bonus4;
				if (item.Bonus4Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus4Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus5 != 0)
			{
				ItemBonus[item.Bonus5Type] -= item.Bonus5;
				if (item.Bonus5Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus5Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus6 != 0)
			{
				ItemBonus[item.Bonus6Type] -= item.Bonus6;
				if (item.Bonus6Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus6Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus7 != 0)
			{
				ItemBonus[item.Bonus7Type] -= item.Bonus7;
				if (item.Bonus7Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus7Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus8 != 0)
			{
				ItemBonus[item.Bonus8Type] -= item.Bonus8;
				if (item.Bonus8Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus8Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus9 != 0)
			{
				ItemBonus[item.Bonus9Type] -= item.Bonus9;
				if (item.Bonus9Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus9Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.Bonus10 != 0)
			{
				ItemBonus[item.Bonus10Type] -= item.Bonus10;
				if (item.Bonus10Type < 20)
					Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.Bonus10Type))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			if (item.ExtraBonus != 0)
			{
				ItemBonus[item.ExtraBonusType] -= item.ExtraBonus;
				//if (item.ExtraBonusType < 20)
				//Out.SendMessage(string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.OnItemUnequipped.Decreased", ItemBonusName(item.ExtraBonusType))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}

			if (ObjectState == eObjectState.Active)
			{
				// TODO: remove when properties system is finished
				Out.SendCharStatsUpdate();
				Out.SendCharResistsUpdate();
				Out.SendUpdateWeaponAndArmorStats();
				Out.SendEncumberance();
				Out.SendUpdatePlayerSkills();
				UpdatePlayerStatus();

				if (IsAlive)
				{
					if (Health < MaxHealth) StartHealthRegeneration();
					else if (Health > MaxHealth) Health = MaxHealth;

					if (Mana < MaxMana) StartPowerRegeneration();
					else if (Mana > MaxMana) Mana = MaxMana;

					if (Endurance < MaxEndurance) StartEnduranceRegeneration();
					else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
				}
			}
		}

		/// <summary>
		/// Handles a bonus change on an item.
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected virtual void OnItemBonusChanged(DOLEvent e, object sender, EventArgs args)
		{
			ItemBonusChangedEventArgs changeArgs = args as ItemBonusChangedEventArgs;
			if (changeArgs == null || changeArgs.BonusType == 0 || changeArgs.BonusAmount == 0)
				return;

			ItemBonus[changeArgs.BonusType] += changeArgs.BonusAmount;

			if (ObjectState == eObjectState.Active)
			{
				Out.SendCharStatsUpdate();
				Out.SendCharResistsUpdate();
				Out.SendUpdateWeaponAndArmorStats();
				Out.SendUpdatePlayerSkills();
				UpdatePlayerStatus();

				if (IsAlive)
				{
					if (Health < MaxHealth) StartHealthRegeneration();
					else if (Health > MaxHealth) Health = MaxHealth;

					if (Mana < MaxMana) StartPowerRegeneration();
					else if (Mana > MaxMana) Mana = MaxMana;

					if (Endurance < MaxEndurance) StartEnduranceRegeneration();
					else if (Endurance > MaxEndurance) Endurance = MaxEndurance;
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
		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (item == null) return false;

			if (!Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
				return false;

			if (source == null)
			{
				Out.SendMessage(String.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveItem.Receive", item.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}
			else
			{
				Out.SendMessage(String.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.ReceiveItem.ReceiveFrom", item.GetName(0, false), source.GetName(0, false))), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
			}

			if (source is GamePlayer)
			{
				GamePlayer sourcePlayer = source as GamePlayer;
				if (sourcePlayer != null)
				{
					uint privLevel1 = Client.Account.PrivLevel;
					uint privLevel2 = sourcePlayer.Client.Account.PrivLevel;
					if (privLevel1 != privLevel2
						&& (privLevel1 > 1 || privLevel2 > 1)
						&& (privLevel1 == 1 || privLevel2 == 1))
					{
						GameServer.Instance.LogGMAction("   Item: " + source.Name + "(" + sourcePlayer.Client.Account.Name + ") -> " + Name + "(" + Client.Account.Name + ") : " + item.Name + "(" + item.Id_nb + ")");
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Called to drop an Item from the Inventory to the floor
		/// </summary>
		/// <param name="slot_pos">SlotPosition to drop</param>
		/// <returns>true if dropped</returns>
		public virtual bool DropItem(eInventorySlot slot_pos)
		{
			GameInventoryItem tempItem;
			return DropItem(slot_pos, out tempItem);
		}

		/// <summary>
		/// Called to drop an item from the Inventory to the floor
		/// and return the GameInventoryItem that is created on the floor
		/// </summary>
		/// <param name="slot_pos">SlotPosition to drop</param>
		/// <param name="droppedItem">out GameItem that was created</param>
		/// <returns>true if dropped</returns>
		public virtual bool DropItem(eInventorySlot slot_pos, out GameInventoryItem droppedItem)
		{
			droppedItem = null;
			if (slot_pos >= eInventorySlot.FirstBackpack && slot_pos <= eInventorySlot.LastBackpack)
			{
				lock (Inventory)
				{
					InventoryItem item = Inventory.GetItem(slot_pos);
					if (!item.IsDropable)
					{
						Out.SendMessage(item.GetName(0, true) + " can not be dropped!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return false;
					}

					if (!Inventory.RemoveItem(item)) return false;

					droppedItem = CreateItemOnTheGround(item);
					Notify(PlayerInventoryEvent.ItemDropped, this, new ItemDroppedEventArgs(item, droppedItem));
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// called to make an item on the ground with owner is player
		/// </summary>
		/// <param name="item">the item to create on the ground</param>
		/// <returns>the GameInventoryItem on the ground</returns>
		public virtual GameInventoryItem CreateItemOnTheGround(InventoryItem item)
		{
			GameInventoryItem gameItem = new GameInventoryItem(item); // fixed

			int x, y;
			GetSpotFromHeading(30, out x, out y);
			gameItem.X = x;
			gameItem.Y = y;
			gameItem.Z = Z;
			gameItem.Heading = Heading;
			gameItem.CurrentRegionID = CurrentRegionID;

			gameItem.AddOwner(this);
			gameItem.AddToWorld();

			return gameItem;
		}
		/// <summary>
		/// Called when the player picks up an item from the ground
		/// </summary>
		/// <param name="floorObject"></param>
		/// <param name="checkRange"></param>
		/// <returns></returns>
		public virtual bool PickupObject(GameObject floorObject, bool checkRange)
		{
			if (floorObject == null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.MustHaveTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (floorObject.ObjectState != eObjectState.Active)
				return false;

			if (floorObject is GameStaticItemTimed && !((GameStaticItemTimed)floorObject).IsOwner(this))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.LootDoesntBelongYou"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if ((floorObject is GameBoat == false) && !checkRange && !WorldMgr.CheckDistance(floorObject, this, WorldMgr.PICKUP_DISTANCE))
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.ObjectTooFarAway", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (floorObject is GameInventoryItem)
			{
				GameInventoryItem floorItem = floorObject as GameInventoryItem;

				lock (floorItem)
				{
					if (floorItem.ObjectState != eObjectState.Active)
						return false;

					if (floorItem.Item == null || floorItem.Item.IsPickable == false)
					{
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.CantGetThat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return false;
					}
					Group group = Group;
					BattleGroup mybattlegroup = (BattleGroup)TempProperties.getObjectProperty(BattleGroup.BATTLEGROUP_PROPERTY, null);
					if (mybattlegroup != null
						&& mybattlegroup.GetBGLootType() == true)
					{
						GamePlayer theTreasurer = mybattlegroup.GetBGTreasurer();
						if (theTreasurer.CanSeeObject(mybattlegroup.GetBGTreasurer(), floorObject))
						{
							bool good = false;
							if (floorItem.Item.IsStackable)
								good = theTreasurer.Inventory.AddTemplate(floorItem.Item, floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
							else
								good = theTreasurer.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

							if (!good)
							{
								theTreasurer.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							theTreasurer.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YouGet", floorItem.Item.GetName(1, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.GroupMemberPicksUp", Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);
						}
						else
						{
							mybattlegroup.SendMessageToBattleGroupMembers(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.NoOneWantsThis", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					else if (group != null && group.AutosplitLoot)
					{
						ArrayList eligibleMembers = new ArrayList(8);
						foreach (GamePlayer ply in group.GetPlayersInTheGroup())
						{
							if (ply.IsAlive
								&& ply.CanSeeObject(ply, floorObject)
								&& (WorldMgr.GetDistance(this, ply) < WorldMgr.MAX_EXPFORKILL_DISTANCE)
								&& (ply.ObjectState == eObjectState.Active)
								&& (ply.AutoSplitLoot)
								&& (ply.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) != eInventorySlot.Invalid))
							{
								eligibleMembers.Add(ply);
							}
						}

						if (eligibleMembers.Count <= 0)
						{
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.NoOneWantsThis", floorObject.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}

						int i = Util.Random(0, eligibleMembers.Count - 1);
						GamePlayer eligibleMember = eligibleMembers[i] as GamePlayer;
						if (eligibleMember != null)
						{
							bool good = false;
							if (floorItem.Item.IsStackable) // poison ID is lost here
								good = eligibleMember.Inventory.AddTemplate(floorItem.Item, floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
							else
								good = eligibleMember.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

							if (!good)
							{
								eligibleMember.Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return false;
							}
							Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.GroupMemberPicksUp", Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);
							group.SendMessageToGroupMembers(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.Autosplit", floorItem.Item.GetName(1, true), eligibleMember.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
					}
					else
					{
						bool good = false;
						if (floorItem.Item.IsStackable)
							good = Inventory.AddTemplate(floorItem.Item, floorItem.Item.Count, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);
						else
							good = Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, floorItem.Item);

						if (!good)
						{
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.BackpackFull"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YouGet", floorItem.Item.GetName(1, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						Message.SystemToOthers(this, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.GroupMemberPicksUp", Name, floorItem.Item.GetName(1, false)), eChatType.CT_System);
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
					if (moneyObject.ObjectState != eObjectState.Active)
						return false;

					Group group = Group;
					if (group != null && group.AutosplitCoins)
					{
						//Spread the money in the group
						ArrayList eligibleMembers = new ArrayList(8);

						foreach (GamePlayer ply in group.GetPlayersInTheGroup())
						{
							if (ply.IsAlive
								&& ply.CanSeeObject(ply, floorObject)
								&& (ply.ObjectState == eObjectState.Active))
							{
								eligibleMembers.Add(ply);
							}
						}
						if (eligibleMembers.Count <= 0)
						{
							Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.NoOneGroupWantsMoney"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}

						foreach (GamePlayer eligibleMember in eligibleMembers)
						{
							if (eligibleMember.Guild != null && eligibleMember.Guild.IsGuildDuesOn())
							{
								double moneyToGuild = moneyObject.TotalCopper / eligibleMembers.Count * eligibleMember.Guild.GetGuildDuesPercent() / 100;
								long moneyToPlayer = (moneyObject.TotalCopper / eligibleMembers.Count) - (long)moneyToGuild;
								eligibleMember.Guild.SetGuildBank(eligibleMember, moneyToGuild);
								eligibleMember.AddMoney(moneyToPlayer, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyObject.TotalCopper / eligibleMembers.Count)));
							}
							else
								eligibleMember.AddMoney(moneyObject.TotalCopper / eligibleMembers.Count, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YourLootShare", Money.GetString(moneyObject.TotalCopper / eligibleMembers.Count)));
						}
					}
					else
					{
						//Add money only to picking player
						if (this != null && Guild != null && Guild.IsGuildDuesOn())
						{
							double moneyToGuild = moneyObject.TotalCopper * Client.Player.Guild.GetGuildDuesPercent() / 100;
							long MoneyToPlayer = moneyObject.TotalCopper - (long)moneyToGuild;
							Client.Player.Guild.SetGuildBank(Client.Player, moneyToGuild);
							Client.Player.AddMoney(MoneyToPlayer, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YourLootShare", MoneyToPlayer.ToString()));
						}
						else
							AddMoney(moneyObject.TotalCopper, LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.YouPickUp", Money.GetString(moneyObject.TotalCopper)));
					}
					moneyObject.Delete();
					return true;
				}
			}
			else if (floorObject is GameBoat)
			{
				if (!WorldMgr.CheckDistance(this, floorObject, 1000))
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.TooFarFromBoat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (!InCombat)
					MountSteed(floorObject as GameBoat, false);

				return true;
			}
			else if (floorObject is GameHouseVault && floorObject.CurrentHouse != null)
			{
				GameHouseVault houseVault = floorObject as GameHouseVault;
				if (houseVault.Detach())
				{
					ItemTemplate template =
						(ItemTemplate)GameServer.Database.FindObjectByKey(typeof(ItemTemplate), houseVault.TemplateID);
					Inventory.AddItem(eInventorySlot.FirstEmptyBackpack,
						new InventoryItem(template));
				}
				return true;
			}
			else if ((floorObject is GameNPC || floorObject is GameStaticItem) && floorObject.CurrentHouse != null)
			{
				floorObject.CurrentHouse.EmptyHookpoint(this, floorObject);
				return true;
			}
			else
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.PickupObject.CantGetThat"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
		}

		/// <summary>
		/// Checks to see if an object is viewable from the players perspective
		/// </summary>
		/// <param name="player">The Player that can see</param>
		/// <param name="obj">The Object to be seen</param>
		/// <returns>True/False</returns>
		public bool CanSeeObject(GamePlayer player, GameObject obj)
		{
			foreach (GamePlayer plr in obj.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (plr == player)
					return true;
			}
			return false;
		}

		#endregion

		#region Database

		/// <summary>
		/// Subtracts the current time from the last time the character was saved
		/// and adds it in the form of seconds to player.PlayedTime
		/// for the /played command.
		/// </summary>
		public long PlayedTime
		{
			get
			{
				DateTime rightNow = DateTime.Now;
				DateTime oldLast = PlayerCharacter.LastPlayed;
				// Get the total amount of time played between now and lastplayed
				// This is safe as lastPlayed is updated on char load.
				TimeSpan playaPlayed = rightNow.Subtract(oldLast);
				TimeSpan newPlayed = playaPlayed + TimeSpan.FromSeconds(PlayerCharacter.PlayedTime);
				return (long)newPlayed.TotalSeconds;
			}
		}

		/// <summary>
		/// Saves the player's skills
		/// </summary>
		protected virtual void SaveSkillsToCharacter()
		{
			string ab = "";
			string sp = "";
			string styleList = "";
			lock (m_skillList)
			{
				foreach(Skill skill in m_skillList)
				{
					Ability ability = skill as Ability;
					if (ability != null)
					{
						if (ab.Length > 0)
						{
							ab += ";";
						}
						ab += ability.KeyName + "|" + ability.Level;
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
						log.Warn(Name + ": Can't save disabled skill " + skill.GetType().ToString());
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
			PlayerCharacter.SerializedAbilities = ab;
			PlayerCharacter.SerializedSpecs = sp;
			PlayerCharacter.SerializedSpellLines = spellLines.ToString();
			PlayerCharacter.DisabledSpells = disabledSpells;
			PlayerCharacter.DisabledAbilities = disabledAbilities;

		}

		/// <summary>
		/// Loads the Skills from the Character
		/// Called after the default skills / level have been set!
		/// </summary>
		protected virtual void LoadSkillsFromCharacter()
		{
			Character character = PlayerCharacter; // if its derived and filled with some code
			if (character == null) return; // no character => exit

			//Lohx - champion abilities
			LoadChampionSpells();
			string tmpStr;

			#region Load Abilities
			//Load up the player skills
			//1. Load all abilities
			//2. Disable appropriate abilities
			lock (m_skillList)
			{
				tmpStr = character.SerializedAbilities;
				if (tmpStr != null && tmpStr.Length > 0)
				{
					string[] abilities = tmpStr.Split(';');

					//Add the abilities to our skill list!
					foreach (string ability in abilities)
					{
						string[] values = ability.Split('|');
						if (values.Length >= 2 && !HasAbility(values[0]))
						{
							string keyname = values[0];
							int level;
							if (int.TryParse(values[1], out level))
								AddAbility(SkillBase.GetAbility(keyname, level), true);
						}
					}

					//Since we added all the abilities that this character has, let's now disable the disabled ones!
					tmpStr = character.DisabledAbilities;
					if (tmpStr != null && tmpStr.Length > 0)
					{
						foreach (string str in tmpStr.Split(';'))
						{
							string[] values = str.Split('|');
							if (values.Length >= 2)
							{
								string keyname = values[0];
								int duration;
								if (int.TryParse(values[1], out duration) && HasAbility(keyname))
									DisableSkill(GetAbility(keyname), duration);
								else if (log.IsErrorEnabled)
									log.Error(Name + ": error in loading disabled abilities => '" + tmpStr + "'");
							}
						}
					}
				}
			}
			#endregion

			#region Load Specs
			lock (m_specList.SyncRoot)
			{
				tmpStr = character.SerializedSpecs;
				if (tmpStr != null && tmpStr.Length > 0)
				{
					string[] specs = tmpStr.Split(';');
					foreach (string spec in specs)
					{
						string[] values = spec.Split('|');
						if (values.Length >= 2)
						{
							Specialization tempspec = SkillBase.GetSpecialization(values[0]);
							int level;
							if (int.TryParse(values[1], out level))
							{
								tempspec.Level = level;
								if (!HasSpecialization(values[0]))
									AddSpecialization(tempspec);
								CharacterClass.OnSkillTrained(this, tempspec);
							}
							else if (log.IsErrorEnabled)
								log.Error(Name + ": error in loading specs => '" + tmpStr + "'");
						}
					}
				}
			}
			#endregion

			#region Load Spell Lines
			Hashtable disabledSpells = new Hashtable();

			//Load the disabled spells
			tmpStr = character.DisabledSpells;
			if (tmpStr != null && tmpStr.Length > 0)
			{
				foreach (string str in tmpStr.Split(';'))
				{
					string[] values = str.Split('|');
					ushort spellid;
					int duration;
					if (values.Length >= 2 && ushort.TryParse(values[0], out spellid) && int.TryParse(values[1], out duration))
						disabledSpells.Add(spellid, duration);
					else if (log.IsErrorEnabled)
						log.Error(Name + ": error in loading disabled spells => '" + tmpStr + "'");
				}
			}

			//Load the spell lines and then check to see if an spells in the spell lines should be disabled
			lock (m_spelllines.SyncRoot)
			{
				m_spelllines.Clear();
				tmpStr = character.SerializedSpellLines;
				if (tmpStr != null && tmpStr.Length > 0)
				{
					foreach (string serializedSpellLine in tmpStr.Split(';'))
					{
						string[] values = serializedSpellLine.Split('|');
						if (values.Length >= 2)
						{
							SpellLine splLine = SkillBase.GetSpellLine(values[0]);
							int level;
							if (int.TryParse(values[1], out level))
							{
								splLine.Level = level;
								AddSpellLine(splLine);

								foreach (Spell spell in SkillBase.GetSpellList(splLine.KeyName))
								{
									if (disabledSpells.ContainsKey(spell.ID))
										DisableSkill(spell, (int)disabledSpells[spell.ID]);
								}
							}
							else if (log.IsErrorEnabled)
								log.Error("Error loading SpellLine '" + serializedSpellLine + "' from character '" + character.Name + "'");
						}
					}
				}
			}

			#endregion

			CharacterClass.OnLevelUp(this); // load all skills from DB first to keep the order
			CharacterClass.OnRealmLevelUp(this);
			RefreshSpecDependantSkills(false);
			UpdateSpellLineLevels(false);
		}

		// seems to never be used
		/*public virtual void WipeAllSkills()
		{
			m_styles.Clear();
			m_specialization.Clear();
			m_abilities.Clear();
			m_disabledSkills.Clear();
			m_spelllines.Clear();
			m_skillList.Clear();
		}*/

		/// <summary>
		/// Loads this player from a character table slot
		/// </summary>
		/// <param name="obj">DOLCharacter</param>
		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			if (!(obj is Character))
				return;
			my_character = (Character)obj;

			m_customFaceAttributes[(int)eCharFacePart.EyeSize] = my_character.EyeSize;
			m_customFaceAttributes[(int)eCharFacePart.LipSize] = my_character.LipSize;
			m_customFaceAttributes[(int)eCharFacePart.EyeColor] = my_character.EyeColor;
			m_customFaceAttributes[(int)eCharFacePart.HairColor] = my_character.HairColor;
			m_customFaceAttributes[(int)eCharFacePart.FaceType] = my_character.FaceType;
			m_customFaceAttributes[(int)eCharFacePart.HairStyle] = my_character.HairStyle;
			m_customFaceAttributes[(int)eCharFacePart.MoodType] = my_character.MoodType;

			#region guild handling
			//TODO: overwork guild handling (VaNaTiC)
			m_guildid = my_character.GuildID;
			if (m_guildid != null)
				m_guild = GuildMgr.GetGuildByGuildID(m_guildid);
			else
				m_guild = null;

			if (m_guild != null)
			{
				foreach (DBRank rank in m_guild.theGuildDB.Ranks)
				{
					if (rank == null) continue;
					if (rank.RankLevel == my_character.GuildRank)
					{
						m_guildRank = rank;
						break;
					}
				}

				m_guildName = m_guild.Name;
				m_guild.AddOnlineMember(this);
				//m_guildNote = m_character.GuildNote; // OBSOLETE (VaNaTiC)
			}
			#endregion

			#region setting world-init-position (delegate to PlayerCharacter dont make sense)
			m_X = my_character.Xpos;
			m_Y = my_character.Ypos;
			m_Z = my_character.Zpos;
			m_Heading = (ushort)my_character.Direction;
			//important, use CurrentRegion property
			//instead because it sets the Region too
			CurrentRegionID = (ushort)my_character.Region;
			if (CurrentRegion == null || CurrentRegion.GetZone(m_X, m_Y) == null)
			{
				log.WarnFormat("Invalid region/zone on char load ({0}): x={1} y={2} z={3} reg={4}; moving to bind point.", my_character.Name, m_X, m_Y, m_Z, my_character.Region);
				m_X = my_character.BindXpos;
				m_Y = my_character.BindYpos;
				m_Z = my_character.BindZpos;
				m_Heading = (ushort)my_character.BindHeading;
				CurrentRegionID = (ushort)my_character.BindRegion;
			}

			for (int i = 0; i < m_lastUniqueLocations.Length; i++)
			{
				//FIXME: Whats this if the normal pos not used and the bind-pos is used???
				//m_lastUniqueLocations[i] = new GameLocation(null, (ushort)m_character.Region, m_character.Xpos, m_character.Ypos, m_character.Zpos);
				m_lastUniqueLocations[i] = new GameLocation(null, CurrentRegionID, m_X, m_Y, m_Z);
			}
			#endregion

			// stats first
			m_charStat[eStat.STR - eStat._First] = (short)my_character.Strength;
			m_charStat[eStat.DEX - eStat._First] = (short)my_character.Dexterity;
			m_charStat[eStat.CON - eStat._First] = (short)my_character.Constitution;
			m_charStat[eStat.QUI - eStat._First] = (short)my_character.Quickness;
			m_charStat[eStat.INT - eStat._First] = (short)my_character.Intelligence;
			m_charStat[eStat.PIE - eStat._First] = (short)my_character.Piety;
			m_charStat[eStat.EMP - eStat._First] = (short)my_character.Empathy;
			m_charStat[eStat.CHR - eStat._First] = (short)my_character.Charisma;

			SetCharacterClass(my_character.Class);

			m_currentSpeed = 0;
			if (MaxSpeedBase == 0)
				MaxSpeedBase = PLAYER_BASE_SPEED;

			m_currentXP = my_character.Experience;
			m_inventory.LoadFromDatabase(InternalID);

			SwitchQuiver((eActiveQuiverSlot)(my_character.ActiveWeaponSlot & 0xF0), false);
			SwitchWeapon((eActiveWeaponSlot)(my_character.ActiveWeaponSlot & 0x0F));

			if (my_character.PlayedTime == 0)
			{
				Health = MaxHealth;
				Mana = MaxMana;
				Endurance = MaxEndurance; // has to be set after max, same applies to other values with max properties
			}
			else
			{
				Health = my_character.Health;
				Mana = my_character.Mana;
				Endurance = my_character.Endurance; // has to be set after max, same applies to other values with max properties
			}

			if (Health <= 0)
			{ // after death we have max health
				// TODO: instead of this here, we have to handle release if linkdead
				// and prevent quit after dying ...
				Health = MaxHealth;
			}

			if (RealmLevel == 0)
				RealmLevel = CalculateRealmLevelFromRPs(RealmPoints);

			//Need to load the skills at the end, so the stored values modify the
			//existing skill levels for this player
			LoadSkillsFromCharacter();
			LoadCraftingSkills();

			# region SkillSpecialtypoints coherence check
			// calc normal spec points for the level & classe
			int allpoints = -1;
			for (int i = 1; i <= Level; i++)
			{
				if (i <= 5) allpoints += i; //start levels
				if (i > 5) allpoints += CharacterClass.SpecPointsMultiplier * i / 10; //normal levels
				if (i > 40) allpoints += CharacterClass.SpecPointsMultiplier * (i - 1) / 20; //half levels
			}
            if (IsLevelSecondStage == true && Level != 50)
                allpoints += CharacterClass.SpecPointsMultiplier * Level  / 20; // add current half level

			// calc spec points player have (autotrain is not anymore processed here - 1.87 livelike)
			int mypoints = SkillSpecialtyPoints;
			foreach (Specialization spec in GetSpecList())
			{
				mypoints += (spec.Level * (spec.Level + 1) - 2) / 2;
				mypoints -= GetAutoTrainPoints(spec, 0);
			}

            // check if correct, if not respec. Not applicable to GMs
            SpecPointsOk = true;
            if (allpoints != mypoints && Client.Account.PrivLevel == 1)
            {
                log.WarnFormat("Spec points for {0} is incorrect, should be {1} but is {2}", Name, allpoints, mypoints);
                mypoints = RespecAllLines();
                SkillSpecialtyPoints = allpoints;
                SpecPointsOk = false;
            }

			#endregion

			//Load the quests for this player
			DBQuest[] quests = (DBQuest[])GameServer.Database.SelectObjects(typeof(DBQuest), "CharName ='" + GameServer.Database.Escape(Name) + "'");
			foreach (DBQuest dbquest in quests)
			{
				AbstractQuest quest = AbstractQuest.LoadFromDatabase(this, dbquest);
				if (quest != null)
				{
					if (quest.Step == -1)
						m_questListFinished.Add(quest);
					else
						m_questList.Add(quest);
				}
			}

			// Load Task object of player ...
			DBTask[] tasks = (DBTask[])GameServer.Database.SelectObjects(typeof(DBTask), "CharName ='" + GameServer.Database.Escape(Name) + "'");
			if (tasks.Length == 1)
			{
				m_task = AbstractTask.LoadFromDatabase(this, tasks[0]);
			}
			else if (tasks.Length > 1)
			{
				if (log.IsErrorEnabled)
					log.Error("More than one DBTask Object found for player " + Name);
			}

			// Load ML steps of player ...
			DBCharacterXMasterLevel[] mlsteps = (DBCharacterXMasterLevel[])GameServer.Database.SelectObjects(typeof(DBCharacterXMasterLevel), "CharName ='" + GameServer.Database.Escape(Name) + "'");
			if (mlsteps.Length > 0)
			{
				foreach (DBCharacterXMasterLevel mlstep in mlsteps)
					m_mlsteps.Add(mlstep);
			}

			// Has to be updated on load to ensure time offline isn't
			// added to character /played.
			my_character.LastPlayed = DateTime.Now;

			m_titles.Clear();
			m_titles.AddRange(PlayerTitleMgr.GetPlayerTitles(this));
			IPlayerTitle t = PlayerTitleMgr.GetTitleByTypeName(my_character.CurrentTitleType);
			if (t == null)
				t = PlayerTitleMgr.ClearTitle;
			m_currentTitle = t;

			//let's only check if we can use /level once shall we, 
			//this is nice because i want to check the property often for the new catacombs classes

			//find all characters in the database
			foreach (Character plr in Client.Account.Characters)
			{
				//where the level of one of the characters if 50
				if (plr.Level == ServerProperties.Properties.SLASH_LEVEL_REQUIREMENT && GameServer.ServerRules.CountsTowardsSlashLevel(plr))
				{
					m_canUseSlashLevel = true;
					break;
				}
			}
		}

		/// <summary>
		/// Save the player into the database
		/// </summary>
		public override void SaveIntoDatabase()
		{
			try
			{
				SaveSkillsToCharacter();
				SaveCraftingSkills();
				my_character.PlayedTime = PlayedTime;  //We have to set the PlayedTime on the character before setting the LastPlayed
				my_character.LastPlayed = DateTime.Now;
				my_character.ActiveWeaponSlot = (byte)((byte)ActiveWeaponSlot | (byte)ActiveQuiverSlot);
				if (m_stuckFlag)
				{
					lock (m_lastUniqueLocations)
					{
						GameLocation loc = m_lastUniqueLocations[m_lastUniqueLocations.Length - 1];
						my_character.Xpos = loc.X;
						my_character.Ypos = loc.Y;
						my_character.Zpos = loc.Z;
						my_character.Region = loc.RegionID;
						my_character.Direction = loc.Heading;
					}
				}
				GameServer.Database.SaveObject(my_character);
				Inventory.SaveIntoDatabase(InternalID);

				if (m_mlsteps != null)
				{
					foreach (DBCharacterXMasterLevel mlstep in m_mlsteps)
						if (mlstep != null)
							GameServer.Database.SaveObject(mlstep);
				}

				if (log.IsInfoEnabled)
					log.Info(my_character.Name + " saved!");
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SaveIntoDatabase.CharacterSaved"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error saving player " + Name + "!", e);
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
			if (PlayerCharacter.Gender == 0) // male
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

		public string SetFirstUppercase(bool firstLetterUppercase, string text)
		{
			if (!firstLetterUppercase) return text;

			string result = "";
			if (text == null || text.Length <= 0) return result;
			result = text[0].ToString().ToUpper();
			if (text.Length > 1) result += text.Substring(1, text.Length - 1);
			return result;
		}

		public string GetPronoun(GameClient Client, int form, bool firstLetterUppercase)
		{
			if (PlayerCharacter.Gender == 0)
				switch (form)
				{
					default:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Male.Subjective"));
					case 1:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Male.Possessive"));
					case 2:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Male.Objective"));
				}
			else
				switch (form)
				{
					default:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Female.Subjective"));
					case 1:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Female.Possessive"));
					case 2:
						return SetFirstUppercase(firstLetterUppercase, LanguageMgr.GetTranslation(Client, "GamePlayer.Pronoun.Female.Objective"));
				}
		}

		public string GetName(GamePlayer target)
		{
			return GameServer.ServerRules.GetPlayerName(this, target);
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
			{//FIXME: Better extract this to a new function in ServerRules !!! (VaNaTiC)
				case eGameServerType.GST_Normal:
					{
						if (Realm == player.Realm || Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.RealmMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name));
						else
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.EnemyRealmMember", player.GetName(this), GetPronoun(Client, 0, true)));
						break;
					}

				case eGameServerType.GST_PvP:
					{
						if (Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.YourGuildMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name));
						else if (Guild == null)
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.NeutralMember", player.GetName(this), GetPronoun(Client, 0, true)));
						else if (Guild == player.Guild || Client.Account.PrivLevel > 1 || player.Client.Account.PrivLevel > 1)
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.YourGuildMember", player.GetName(this), GetPronoun(Client, 0, true), CharacterClass.Name));
						else
							message = string.Format(LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.OtherGuildMember", player.GetName(this), GetPronoun(Client, 0, true), GuildName));
						break;
					}

				default:
					{
						message = LanguageMgr.GetTranslation(player.Client, "GamePlayer.GetExamineMessages.YouExamine", player.GetName(this));
						break;
					}
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
			if (player == null || atkArgs == null) return;
			if (atkArgs.AttackData.AttackResult != eAttackResult.HitUnstyled && atkArgs.AttackData.AttackResult != eAttackResult.HitStyle) return;
			if (atkArgs.AttackData.Damage == -1) return;

			player.Stealth(false);
		}
		/// <summary>
		/// Set player's stealth state
		/// </summary>
		/// <param name="newState">stealth state</param>
		public virtual void Stealth(bool newState)
		{
			if (IsStealthed == newState)
				return;

			if (IsOnHorse)
				IsOnHorse = false;

			UncoverStealthAction action = (UncoverStealthAction)TempProperties.getObjectProperty(UNCOVER_STEALTH_ACTION_PROP, null);
			if (newState)
			{
				//start the uncover timer
				if (action == null)
					action = new UncoverStealthAction(this);
				action.Interval = 2000;
				action.Start(2000);
				TempProperties.setProperty(UNCOVER_STEALTH_ACTION_PROP, action);

				if (ObjectState == eObjectState.Active)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Stealth.NowHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendPlayerModelTypeChange(this, 3);
				m_stealthEffect = new StealthEffect();
				m_stealthEffect.Start(this);
				Sprint(false);
				GameEventMgr.AddHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(Unstealth));
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (player == this) continue;
					if (!player.CanDetect(this))
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

				if (ObjectState == eObjectState.Active)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Stealth.NoLongerHidden"), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				CamouflageEffect cam = (CamouflageEffect)EffectList.GetOfType(typeof(CamouflageEffect));
				if (cam != null)
				{
					cam.Stop();
				}

				Out.SendPlayerModelTypeChange(this, 2);
				m_stealthEffect.Stop();
				m_stealthEffect = null;
				GameEventMgr.RemoveHandler(this, GameLivingEvent.AttackedByEnemy, new DOLEventHandler(Unstealth));
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					//TODO: more correct way to do it
					if (player == this) continue;
					//if a player could see us stealthed, we just update our model to avoid untargetting.
					if (player.CanDetect(this))
						player.Out.SendPlayerModelTypeChange(this, 2);
					else
						player.Out.SendPlayerCreate(this);
					player.Out.SendLivingEquipmentUpdate(this);
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
			public UncoverStealthAction(GamePlayer actionSource)
				: base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				if (player.Client.Account.PrivLevel > 1) return;

				bool checklos = false;
				foreach (AbstractArea area in player.CurrentAreas)
				{
					if (area.CheckLOS)
					{
						checklos = true;
						break;
					}
				}

				foreach (GameNPC npc in player.GetNPCsInRadius(1024))
				{
					// Friendly mobs do not uncover stealthed players
					if (!GameServer.ServerRules.IsAllowedToAttack(npc, player, true)) continue;

					double npcLevel = Math.Max(npc.Level, 1.0);
					double stealthLevel = player.GetModifiedSpecLevel(Specs.Stealth);
					double detectRadius = 125.0 + ((npcLevel - stealthLevel) * 20.0);

					// we have detect hidden and enemy don't = higher range
					if (npc.HasAbility(Abilities.DetectHidden) && player.EffectList.GetOfType(typeof(CamouflageEffect)) == null)
					{
						detectRadius += 125;
					}

					if (detectRadius < 126) detectRadius = 126;

					double distanceToPlayer = WorldMgr.GetDistance(npc, player);
					if (distanceToPlayer > detectRadius) continue;

					double fieldOfView = 90.0;  //90 degrees  = standard FOV
					double fieldOfListen = 120.0; //120 degrees = standard field of listening
					if (npc.Level > 50)
					{
						fieldOfListen += (npc.Level - player.Level) * 3;
					}
					double angle = npc.GetAngleToTarget(player);

					//player in front
					fieldOfView /= 2.0;
					bool canSeePlayer = (angle >= 360 - fieldOfView || angle < fieldOfView);

					//If npc can not see nor hear the player, continue the loop
					fieldOfListen /= 2.0;
					if (canSeePlayer == false &&
					   !(angle >= (45 + 60) - fieldOfListen && angle < (45 + 60) + fieldOfListen) &&
					   !(angle >= (360 - 45 - 60) - fieldOfListen && angle < (360 - 45 - 60) + fieldOfListen))
						continue;

					double chanceMod = 1.0;

					//Chance to detect player decreases after 125 coordinates!
					if (distanceToPlayer > 125)
						chanceMod = 1f - (distanceToPlayer - 125.0) / (detectRadius - 125.0);

					double chanceToUncover = 0.1 + (npc.Level - stealthLevel) * 0.03 * chanceMod;
					if (chanceToUncover < 0.01) chanceToUncover = 0.01;

					if (Util.ChanceDouble(chanceToUncover))
					{
						if (canSeePlayer)
						{
							if (checklos)
							{
								player.Out.SendCheckLOS(player, npc, new CheckLOSResponse(player.UncoverLOSHandler));
							}
							else
							{
								player.Out.SendMessage(npc.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								player.Stealth(false);
								break;
							}
						}
						else
						{
							npc.TurnTo(player, 10000);
						}
					}
				}
			}
		}
		/// <summary>
		/// This handler is called by the unstealth check of mobs
		/// </summary>   
		public void UncoverLOSHandler(GamePlayer player, ushort response, ushort targetOID)
		{
			GameObject target = CurrentRegion.GetObject(targetOID);

			if ((target == null) || (player.IsStealthed == false)) return;

			if ((response & 0x100) == 0x100)
			{
				player.Out.SendMessage(target.GetName(0, true) + " uncovers you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				player.Stealth(false);
			}
		}

		/// <summary>
		/// Checks whether this player can detect stealthed enemy
		/// </summary>
		/// <param name="enemy"></param>
		/// <returns>true if enemy can be detected</returns>
		public virtual bool CanDetect(GamePlayer enemy)
		{
			if (enemy.CurrentRegionID != CurrentRegionID)
				return false;
			if (!IsAlive)
				return false;
			if (enemy.EffectList.GetOfType(typeof(VanishEffect)) != null)
				return false;
			if (this.Client.Account.PrivLevel > 1)
				return true;
			if (enemy.Client.Account.PrivLevel > 1)
				return false;

			/*
			 * http://www.critshot.com/forums/showthread.php?threadid=3142
			 * The person doing the looking has a chance to find them based on their level, minus the stealthed person's stealth spec.
			 *
			 * -Normal detection range = (enemy lvl  your stealth spec) * 20 + 125
			 * -Detect Hidden Range = (enemy lvl  your stealth spec) * 50 + 250
			 * -See Hidden range = 2700 - (38 * your stealth spec)
			 */

			int EnemyStealthLevel = enemy.GetModifiedSpecLevel(Specs.Stealth);
			if (EnemyStealthLevel > 50)
				EnemyStealthLevel = 50;
			int levelDiff = this.Level - EnemyStealthLevel;
			if (levelDiff < 0) levelDiff = 0;

			int range;
			bool enemyHasCamouflage = enemy.EffectList.GetOfType(typeof(CamouflageEffect)) != null;
			if (HasAbility(Abilities.DetectHidden) && !enemy.HasAbility(Abilities.DetectHidden) && !enemyHasCamouflage)
			{
				// we have detect hidden and enemy don't = higher range
				range = levelDiff * 50 + 250; // Detect Hidden advantage
			}
			else
			{
				range = levelDiff * 20 + 125; // Normal detection range
			}

			// Mastery of Stealth Bonus
			RAPropertyEnhancer mos = GetAbility(typeof(MasteryOfStealthAbility)) as RAPropertyEnhancer;
			if (mos != null && !enemyHasCamouflage)
			{
				range += mos.GetAmountForLevel(mos.Level);
			}

			range += BuffBonusCategory1[(int)eProperty.Skill_Stealth];

			// Apply Blanket of camouflage effect
			GameSpellEffect iSpymasterEffect1 = SpellHandler.FindEffectOnTarget((GameLiving)enemy, "BlanketOfCamouflage");
			if (iSpymasterEffect1 != null)
			{
				range -= (int)iSpymasterEffect1.Spell.Value;
				if (range < 0) range = 0;
			}

			// Apply Lookout effect
			GameSpellEffect iSpymasterEffect2 = SpellHandler.FindEffectOnTarget((GameLiving)this, "Loockout");
			if (iSpymasterEffect2 != null)
				range += (int)iSpymasterEffect2.Spell.Value;

			// Apply Prescience node effect
			GameSpellEffect iConvokerEffect = SpellHandler.FindEffectOnTarget((GameLiving)enemy, "Prescience");
			if (iConvokerEffect != null)
				range += (int)iConvokerEffect.Spell.Value;

			//Hard cap is 1900
			if (range > 1900)
				range = 1900;
			//Possibilité de voir les membres du groupes fufutés .... pour tout le monde
			else if (enemy.Group != null && Group != null && enemy.Group == Group)
			{
				range = 2500;
			}

			// Fin
			// vampiir stealth range, uncomment when add eproperty stealthrange i suppose
			return WorldMgr.CheckDistance(this, enemy, range);
		}

		#endregion

		#region Task

		/// <summary>
		/// Holding tasks of player
		/// </summary>
		AbstractTask m_task = null;

		/// <summary>
		/// Gets the tasklist of this player
		/// </summary>
		public AbstractTask Task
		{
			get { return m_task; }
			set { m_task = value; }
		}

		#endregion

		#region Mission

		private AbstractMission m_mission = null;

		/// <summary>
		/// Gets the personal mission
		/// </summary>
		public AbstractMission Mission
		{
			get { return m_mission; }
			set
			{
				m_mission = value;
				this.Out.SendQuestListUpdate();
				if (value != null) Out.SendMessage(m_mission.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Quest

		/// <summary>
		/// Holds all the quests currently active on this player
		/// </summary>
		protected readonly ArrayList m_questList = new ArrayList(1);

		/// <summary>
		/// Holds all already finished quests off this player
		/// </summary>
		protected readonly ArrayList m_questListFinished = new ArrayList(1);

		/// <summary>
		/// Gets the questlist of this player
		/// </summary>
		public IList QuestList
		{
			get { return m_questList; }
		}

		/// <summary>
		/// Gets the questlist of this player
		/// </summary>
		public IList QuestListFinished
		{
			get { return m_questListFinished; }
		}

		/// <summary>
		/// Adds a quest to the players questlist
		/// </summary>
		/// <param name="quest">The quest to add</param>
		/// <returns>true if added, false if player is already doing the quest!</returns>
		public bool AddQuest(AbstractQuest quest)
		{
			lock (m_questList)
			{
				if (IsDoingQuest(quest.GetType()) != null)
					return false;

				m_questList.Add(quest);
				quest.OnQuestAssigned(this);
			}
			Out.SendQuestUpdate(quest);
			return true;
		}

		/// <summary>
		/// Remove credit for this type of encounter.
		/// </summary>
		/// <param name="questType"></param>
		/// <returns></returns>
		public bool RemoveEncounterCredit(Type questType)
		{
			if (questType == null)
				return false;

			lock (m_questListFinished)
			{
				foreach (AbstractQuest q in m_questListFinished)
				{
					if (q.GetType().Equals(questType) && q.Step == -1)
					{
						m_questListFinished.Remove(q);
						q.DeleteFromDatabase();
						return true;
					}
				}
			}

			return false;
		}

		/// <summary>
		/// Checks if a player has done a specific quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the number of times the player did this quest</returns>
		public int HasFinishedQuest(Type questType)
		{
			int counter = 0;
			lock (m_questListFinished)
			{
				foreach (AbstractQuest q in m_questListFinished)
				{
					//DOLConsole.WriteLine("HasFinished: "+q.GetType().FullName+" Step="+q.Step);
					if (q.GetType().Equals(questType) && q.Step == -1)
						counter++;
				}
			}
			return counter;
		}

		/// <summary>
		/// Checks if this player is currently doing the specified quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the quest if player is doing the quest or null if not</returns>
		public AbstractQuest IsDoingQuest(Type questType)
		{
			lock (m_questList)
			{
				foreach (AbstractQuest q in m_questList)
				{
					if (q.GetType().Equals(questType) && q.Step != -1)
						return q;
				}
			}
			return null;
		}

		#endregion

		#region Notify
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			// events will only fire for currently active quests.
			foreach (AbstractQuest q in (ArrayList)m_questList.Clone())
				q.Notify(e, sender, args);

			if (Task != null)
				Task.Notify(e, sender, args);

			if (Mission != null)
				Mission.Notify(e, sender, args);

			if (Group != null && Group.Mission != null)
				Group.Mission.Notify(e, sender, args);

			//Realm mission will be handled on the capture event args
		}

		public override void Notify(DOLEvent e, object sender)
		{
			Notify(e, sender, null);
		}

		public override void Notify(DOLEvent e)
		{
			Notify(e, null, null);
		}

		public override void Notify(DOLEvent e, EventArgs args)
		{
			Notify(e, null, args);
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

		/// <summary>
		/// Get the specified player crafting skill value
		/// </summary>
		/// <param name="skill">The crafting skill to get value</param>
		/// <returns>the level in the specified crafting if valid and -1 if not</returns>
		public virtual int GetCraftingSkillValue(eCraftingSkill skill)
		{
			lock (craftingSkills.SyncRoot)
			{
				if (craftingSkills[(int)skill] == null) return -1;
				return Convert.ToInt32(craftingSkills[(int)skill]);
			}
		}

		/// <summary>
		/// Increase the specified player crafting skill
		/// </summary>
		/// <param name="skill">Crafting skill to increase</param>
		/// <param name="count">How much increase or decrase</param>
		/// <returns>true if the skill is valid and -1 if not</returns>
		public virtual bool GainCraftingSkill(eCraftingSkill skill, int count)
		{
			if (skill == eCraftingSkill.NoCrafting) return false;

			lock (craftingSkills.SyncRoot)
			{
				AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
				if (craftingSkill != null)
				{
					craftingSkills[(int)skill] = count + Convert.ToInt32(craftingSkills[(int)skill]);
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.GainCraftingSkill.GainSkill", craftingSkill.Name, craftingSkills[(int)skill]), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					int amount = GetCraftingSkillValue(skill);
					if (GameServer.ServerRules.CanGenerateNews(this) && amount >= 1000 && amount - count < 1000)
					{
						string message = string.Format(LanguageMgr.GetTranslation(Client, "GamePlayer.GainCraftingSkill.ReachedSkill", Name, craftingSkill.Name));
						NewsMgr.CreateNews(message, Realm, eNewsType.PvE, true);
					}
				}
				return true;
			}
		}

		/// <summary>
		/// Add a new crafting skill to the player
		/// </summary>
		/// <param name="skill"></param>
		/// <param name="startValue"></param>
		/// <returns></returns>
		public virtual bool AddCraftingSkill(eCraftingSkill skill, int startValue)
		{
			if (skill == eCraftingSkill.NoCrafting) return false;

			lock (craftingSkills.SyncRoot)
			{
				if (!craftingSkills.ContainsKey((int)skill))
				{
					AbstractCraftingSkill craftingSkill = CraftingMgr.getSkillbyEnum(skill);
					if (craftingSkill != null)
					{
						craftingSkills.Add(Convert.ToInt32(skill), startValue);
						Out.SendMessage("You gain skill in " + craftingSkill.Name + "! (" + startValue + ").", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
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
			get { return m_crafttimer; }
			set { m_crafttimer = value; }
		}

		/// <summary>
		/// Does the player is crafting
		/// </summary>
		public bool IsCrafting
		{
			get { return (m_crafttimer != null && m_crafttimer.IsAlive); }
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
			PlayerCharacter.CraftingPrimarySkill = (byte)CraftingPrimarySkill;

			string cs = "";

			if (CraftingPrimarySkill != eCraftingSkill.NoCrafting)
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

			PlayerCharacter.SerializedCraftingSkills = cs;
		}

		/// <summary>
		/// This function load all player crafting skill from the db
		/// </summary>
		protected void LoadCraftingSkills()
		{
			if (PlayerCharacter.SerializedCraftingSkills == "" || PlayerCharacter.CraftingPrimarySkill == 0)
			{
				CraftingPrimarySkill = eCraftingSkill.NoCrafting;
				return;
			}
			try
			{
				CraftingPrimarySkill = (eCraftingSkill)PlayerCharacter.CraftingPrimarySkill;

				lock (craftingSkills.SyncRoot)
				{
					string[] craftingSkill = PlayerCharacter.SerializedCraftingSkills.Split(';');
					foreach (string skill in craftingSkill)
					{
						string[] values = skill.Split('|');
						//Load by crafting skill name
						if (values[0].Length > 3)
						{
							int i = 0;
							switch (values[0])
							{
								case "WeaponCrafting": i = 1; break;
								case "ArmorCrafting": i = 2; break;
								case "SiegeCrafting": i = 3; break;
								case "Alchemy": i = 4; break;
								case "Jewellery": i = 5; break;
								case "MetalWorking": i = 6; break;
								case "LeatherCrafting": i = 7; break;
								case "ClothWorking": i = 8; break;
								case "GemCutting": i = 9; break;
								case "HerbalCrafting": i = 10; break;
								case "Tailoring": i = 11; break;
								case "Fletching": i = 12; break;
								case "SpellCrafting": i = 13; break;
								case "WoodWorking": i = 14; break;

							}
							if (!craftingSkills.ContainsKey(i))
							{
								craftingSkills.Add(i, Convert.ToInt32(values[1]));
							}

						}
						//Load by number
						else if (!craftingSkills.ContainsKey(Convert.ToInt32(values[0])))
						{
							craftingSkills.Add(Convert.ToInt32(values[0]), Convert.ToInt32(values[1]));
						}
					}
				}
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error(Name + ": error in loading playerCraftingSkills => " + PlayerCharacter.SerializedCraftingSkills, e);
			}
		}

		/// <summary>
		/// This function is called each time a player try to make a item
		/// </summary>
		public void CraftItem(ushort itemID)
		{
			DBCraftedItem craftitem = (DBCraftedItem)GameServer.Database.SelectObject(typeof(DBCraftedItem), "CraftedItemID ='" + GameServer.Database.Escape(itemID.ToString()) + "'");
			if (craftitem != null && craftitem.ItemTemplate != null && craftitem.RawMaterials != null)
			{
				AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum((eCraftingSkill)craftitem.CraftingSkillType);
				if (skill != null)
				{
					skill.CraftItem(craftitem, this);
				}
				else
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CraftItem.DontHaveAbilityMake"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				Out.SendMessage("Craft item (" + itemID + ") not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		/// <summary>
		/// This function is called each time a player try to salvage a item
		/// </summary>
		public void SalvageItem(InventoryItem item)
		{
			Salvage.BeginWork(this, item);
		}

		/// <summary>
		/// This function is called each time a player try to repair a item
		/// </summary>
		public void RepairItem(InventoryItem item)
		{
			Repair.BeginWork(this, item);
		}

		#endregion

		#region Housing

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

			house.Exit(this, false);
		}


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
			set { m_tradeWindow = value; }
		}

		/// <summary>
		/// Opens the trade between two players
		/// </summary>
		/// <param name="tradePartner">GamePlayer to trade with</param>
		/// <returns>true if trade has started</returns>
		public bool OpenTrade(GamePlayer tradePartner)
		{
			lock (m_LockObject)
			{
				lock (tradePartner)
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
		public bool OpenSelfCraft(InventoryItem item)
		{
			if (item == null) return false;

			lock (m_LockObject)
			{
				if (TradeWindow != null)
				{
					GamePlayer sourceTradePartner = TradeWindow.Partner;
					if (sourceTradePartner == null)
					{
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OpenSelfCraft.AlreadySelfCrafting"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						Out.SendMessage("You are still trading with " + sourceTradePartner.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					return false;
				}

				if (item.SlotPosition < (int)eInventorySlot.FirstBackpack || item.SlotPosition > (int)eInventorySlot.LastBackpack)
				{
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.OpenSelfCraft.CanOnlyCraftBackpack"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
		/// Sets the controlled object for this player
		/// </summary>
		/// <param name="controlledNpc"></param>
		public override void SetControlledNpc(IControlledBrain controlledNpc)
		{
			if (controlledNpc == ControlledNpc) return;
			if (controlledNpc == null)
			{
				Out.SendPetWindow(null, ePetWindowAction.Close, 0, 0);
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SetControlledNpc.ReleaseTarget2", ControlledNpc.Body.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.SetControlledNpc.ReleaseTarget"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			else
			{
				if (controlledNpc.Owner != this)
					throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");
				if (m_controlledNpc == null)
					InitControlledNpc(1);
				Out.SendPetWindow(controlledNpc.Body, ePetWindowAction.Open, controlledNpc.AggressionState, controlledNpc.WalkState);
			}
			m_controlledNpc[0] = controlledNpc;
		}

		/// <summary>
		/// Commands controlled object to attack
		/// </summary>
		public virtual void CommandNpcAttack()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null || npc.Body.IsConfused || !GameServer.ServerRules.IsAllowedToAttack(this, TargetObject as GameLiving, false))
				return;

			if (WorldMgr.GetDistance(TargetObject, this) > 1500)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CommandNpcAttack.TooFarAwayForPet"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (!TargetInView)
			{
				Out.SendMessage("You can't see your target!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
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
			if (npc == null)
				return;
			BDPet subpet = TargetObject as BDPet;
			if (subpet != null && subpet.Brain is BDPetBrain && ControlledNpc is CommanderBrain && (ControlledNpc as CommanderBrain).FindPet(subpet.Brain as IControlledBrain))
			{
				Notify(GameLivingEvent.PetReleased, subpet);
				return;
			}
			Notify(GameLivingEvent.PetReleased, ControlledNpc.Body);
		}

		/// <summary>
		/// Commands controlled object to follow
		/// </summary>
		public virtual void CommandNpcFollow()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to follow you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Follow(this);
		}

		/// <summary>
		/// Commands controlled object to stay where it is
		/// </summary>
		public virtual void CommandNpcStay()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to stay in this position!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.Stay();
		}

		/// <summary>
		/// Commands controlled object to go to players location
		/// </summary>
		public virtual void CommandNpcComeHere()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to come to you.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.ComeHere();
		}

		/// <summary>
		/// Commands controlled object to go to target
		/// </summary>
		public virtual void CommandNpcGoTarget()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			GameObject target = TargetObject;
			if (target == null)
			{
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CommandNpcGoTarget.MustSelectDestination"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to disengage from combat!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.SetAggressionState(eAggressionState.Passive);
			npc.Body.StopAttack();
			npc.Body.StopCurrentSpellcast();
			npc.Follow(this);
		}

		/// <summary>
		/// Changes controlled object state to aggressive
		/// </summary>
		public virtual void CommandNpcAgressive()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to attack all enemies!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.SetAggressionState(eAggressionState.Aggressive);
		}

		/// <summary>
		/// Changes controlled object state to defensive
		/// </summary>
		public virtual void CommandNpcDefensive()
		{
			IControlledBrain npc = ControlledNpc;
			if (npc == null)
				return;

			if (npc.Body.IsConfused) return;

			Out.SendMessage("You command " + npc.Body.GetName(0, false) + " to defend you!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			npc.SetAggressionState(eAggressionState.Defensive);
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
		/// Create a shade effect for this player.
		/// </summary>
		/// <returns></returns>
		protected virtual ShadeEffect CreateShadeEffect()
		{
			return new ShadeEffect();
		}

		/// <summary>
		/// Changes shade state of the player.
		/// </summary>
		/// <param name="state">The new state.</param>
		public virtual void Shade(bool state)
		{
			if (IsShade == state)
			{
				if (state && (ObjectState == eObjectState.Active))
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Shade.AlreadyShade"),
						eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return;
			}

			if (state)
			{
				// Turn into a shade.

				Model = 822;
				m_ShadeEffect = CreateShadeEffect();
				m_ShadeEffect.Start(this);
			}
			else
			{
				// Drop shade form.

				m_ShadeEffect.Stop();
				m_ShadeEffect = null;
				Model = (ushort)m_client.Account.Characters[m_client.ActiveCharIndex].CreationModel;
				Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.Shade.NoLongerShade"),
					eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}

		#endregion

		#region Siege Weapon
		private GameSiegeWeapon m_siegeWeapon;

		public GameSiegeWeapon SiegeWeapon
		{
			get { return m_siegeWeapon; }
			set { m_siegeWeapon = value; }
		}
		public void SalvageSiegeWeapon(GameSiegeWeapon siegeWeapon)
		{
			Salvage.BeginWork(this, siegeWeapon);
		}
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

			long newTick = CurrentRegion.Time + duration;
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
			get { return m_pvpInvulnerabilityTick > CurrentRegion.Time; }
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
			/// <param name="actionSource"></param>
			/// <param name="callback"></param>
			public InvulnerabilityTimer(GamePlayer actionSource, InvulnerabilityExpiredCallback callback)
				: base(actionSource)
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

		#region Player Titles

		/// <summary>
		/// Holds all players titles.
		/// </summary>
		protected readonly ArrayList m_titles = new ArrayList();

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
				PlayerCharacter.CurrentTitleType = value.GetType().FullName;

				//update newTitle for all players if client is playing
				if (ObjectState == eObjectState.Active)
				{
					if (value == PlayerTitleMgr.ClearTitle)
						Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.CurrentTitle.TitleCleared"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					else
						Out.SendMessage("Your title has been set to " + value.GetDescription(this) + '.', eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				UpdateCurrentTitle();
			}
		}

		/// <summary>
		/// Updates player's current title to him and everyone around.
		/// </summary>
		public virtual void UpdateCurrentTitle()
		{
			if (ObjectState == eObjectState.Active)
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					if (player != this)
					{
						//						player.Out.SendRemoveObject(this);
						//						player.Out.SendPlayerCreate(this);
						//						player.Out.SendLivingEquipementUpdate(this);
						player.Out.SendPlayerTitleUpdate(this);
					}
				Out.SendUpdatePlayer();
			}
		}

		#endregion

		#region Statistics

		/// <summary>
		/// Gets or sets the count of albion players killed.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsAlbionPlayers
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsAlbionPlayers : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsAlbionPlayers = value;
				Notify(GamePlayerEvent.KillsAlbionPlayersChanged, this);
				Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of midgard players killed.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsMidgardPlayers
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsMidgardPlayers : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsMidgardPlayers = value;
				Notify(GamePlayerEvent.KillsMidgardPlayersChanged, this);
				Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of hibernia players killed.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsHiberniaPlayers
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsHiberniaPlayers : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsHiberniaPlayers = value;
				Notify(GamePlayerEvent.KillsHiberniaPlayersChanged, this);
				Notify(GamePlayerEvent.KillsTotalPlayersChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of death blows on albion players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsAlbionDeathBlows
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsAlbionDeathBlows : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsAlbionDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of death blows on midgard players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsMidgardDeathBlows
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsMidgardDeathBlows : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsMidgardDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of death blows on hibernia players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsHiberniaDeathBlows
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsHiberniaDeathBlows : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsHiberniaDeathBlows = value;
				Notify(GamePlayerEvent.KillsTotalDeathBlowsChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of killed solo albion players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsAlbionSolo
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsAlbionSolo : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsAlbionSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of killed solo midgard players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsMidgardSolo
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsMidgardSolo : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsMidgardSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of killed solo hibernia players.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsHiberniaSolo
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsHiberniaSolo : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsHiberniaSolo = value;
				Notify(GamePlayerEvent.KillsTotalSoloChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of captured keeps.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int CapturedKeeps
		{
			get { return PlayerCharacter != null ? PlayerCharacter.CapturedKeeps : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.CapturedKeeps = value;
				Notify(GamePlayerEvent.CapturedKeepsChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of captured towers.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int CapturedTowers
		{
			get { return PlayerCharacter != null ? PlayerCharacter.CapturedTowers : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.CapturedTowers = value;
				Notify(GamePlayerEvent.CapturedTowersChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of captured relics.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int CapturedRelics
		{
			get { return PlayerCharacter != null ? PlayerCharacter.CapturedRelics : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.CapturedRelics = value;
				Notify(GamePlayerEvent.CapturedRelicsChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of dragons killed.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsDragon
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsDragon : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsDragon = value;
				Notify(GamePlayerEvent.KillsDragonChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the pvp deaths
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int DeathsPvP
		{
			get { return PlayerCharacter != null ? PlayerCharacter.DeathsPvP : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.DeathsPvP = value; }
		}

		/// <summary>
		/// Gets or sets the count of killed Legions.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsLegion
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsLegion : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsLegion = value;
				Notify(GamePlayerEvent.KillsLegionChanged, this);
			}
		}

		/// <summary>
		/// Gets or sets the count of killed Epic Boss.
		/// (delegate to PlayerCharacter)
		/// </summary>
		public int KillsEpicBoss
		{
			get { return PlayerCharacter != null ? PlayerCharacter.KillsEpicBoss : 0; }
			set
			{
				if (PlayerCharacter != null) PlayerCharacter.KillsEpicBoss = value;
				Notify(GamePlayerEvent.KillsEpicBossChanged, this);
			}
		}
		#endregion

		#region Controlled Mount

		protected RegionTimer m_whistleMountTimer;
		protected ControlledHorse m_controlledHorse;

		public bool HasHorse
		{
			get
			{
				if (ActiveHorse == null || ActiveHorse.ID == 0)
					return false;
				return true;
			}
		}

		public bool IsSummoningMount
		{
			get { return m_whistleMountTimer != null && m_whistleMountTimer.IsAlive; }
		}

		protected bool m_isOnHorse;
		public bool IsOnHorse
		{
			get { return m_isOnHorse; }
			set
			{
				if (m_whistleMountTimer != null)
					StopWhistleTimers();
				m_isOnHorse = value;
				Out.SendControlledHorse(this, value); // fix very rare bug when this player not in GetPlayersInRadius;
				foreach (GamePlayer plr in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				{
					if (plr == this)
						continue;
					plr.Out.SendControlledHorse(this, value);
				}
				if (m_isOnHorse)
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsOnHorse.MountSteed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				else
					Out.SendMessage(LanguageMgr.GetTranslation(Client, "GamePlayer.IsOnHorse.DismountSteed"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Out.SendUpdateMaxSpeed();
			}
		}

		protected void StopWhistleTimers()
		{
			if (m_whistleMountTimer != null)
			{
				m_whistleMountTimer.Stop();
				Out.SendCloseTimerWindow();
			}
			m_whistleMountTimer = null;
		}

		protected int WhistleMountTimerCallback(RegionTimer callingTimer)
		{
			StopWhistleTimers();
			IsOnHorse = true;
			return 0;
		}

		public ControlledHorse ActiveHorse
		{
			get { return m_controlledHorse; }
		}

		public class ControlledHorse
		{
			protected byte h_id;
			protected byte h_bardingId;
			protected ushort h_bardingColor;
			protected byte h_saddleId;
			protected byte h_saddleColor;
			protected byte h_slots;
			protected byte h_armor;
			protected string h_name;
			protected int m_level;
			protected GamePlayer h_player;

			public ControlledHorse(GamePlayer player)
			{
				h_name = "";
				h_player = player;
			}

			public byte ID
			{
				get { return h_id; }
				set
				{
					h_id = value;
					InventoryItem item = h_player.Inventory.GetItem(eInventorySlot.Horse);
					if (item != null)
						m_level = item.Level;
					else
						m_level = 35;//base horse by default
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public byte Barding
			{
				get
				{
					InventoryItem barding = h_player.Inventory.GetItem(eInventorySlot.HorseBarding);
					if (barding != null)
						return (byte)barding.DPS_AF;
					return h_bardingId;
				}
				set
				{
					h_bardingId = value;
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public ushort BardingColor
			{
				get
				{
					InventoryItem barding = h_player.Inventory.GetItem(eInventorySlot.HorseBarding);
					if (barding != null)
						return (ushort)barding.Color;
					return h_bardingColor;
				}
				set
				{
					h_bardingColor = value;
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public byte Saddle
			{
				get
				{
					InventoryItem armor = h_player.Inventory.GetItem(eInventorySlot.HorseArmor);
					if (armor != null)
						return (byte)armor.DPS_AF;
					return h_saddleId;
				}
				set
				{
					h_saddleId = value;
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public byte SaddleColor
			{
				get
				{
					InventoryItem armor = h_player.Inventory.GetItem(eInventorySlot.HorseArmor);
					if (armor != null)
						return (byte)armor.Color;
					return h_saddleColor;
				}
				set
				{
					h_saddleColor = value;
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public byte Slots
			{
				get { return h_slots; }
			}

			public byte Armor
			{
				get { return h_armor; }
			}

			public string Name
			{
				get { return h_name; }
				set
				{
					h_name = value;
					InventoryItem item = h_player.Inventory.GetItem(eInventorySlot.Horse);
					if (item != null)
						item.CrafterName = Name;
					h_player.Out.SendSetControlledHorse(h_player);
				}
			}

			public short Speed
			{
				get
				{
					if (m_level <= 35)
						return 135;
					else
						return 145;
				}
			}

			public bool IsSummonRvR
			{
				get
				{
					if (m_level <= 35)
						return false;
					else
						return true;
				}
			}

			public bool IsCombatHorse
			{
				get
				{
					return false;
				}
			}
		}
		#endregion

		#region GuildBanner
		protected bool m_isCarryingGuildBanner = false;

		/// <summary>
		/// Gets/Sets the visibility of the carryable RvrGuildBanner. Wont work if the player has no guild.
		/// </summary>
		public bool IsCarryingGuildBanner
		{
			get { return m_isCarryingGuildBanner; }
			set
			{
				//cant send guildbanner for players without guild.
				if (value == true && Guild == null)
					return;
				if (value != m_isCarryingGuildBanner)
				{
					foreach (GamePlayer playerToUpdate in GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
					{
						if (playerToUpdate != null && playerToUpdate.Client.IsPlaying)
							playerToUpdate.Out.SendRvRGuildBanner(this, value);
					}
					m_isCarryingGuildBanner = value;
				}
			}
		}

		#endregion

		#region Bainshee
		protected bool m_InWraithForm = false;

		public bool InWraithForm
		{
			get { return m_InWraithForm; }
			set
			{
				m_InWraithForm = value;
				if (m_InWraithForm)
				{
					switch (Race)
					{
						case 9: Model = 1883; break; //Celt   
						case 11: Model = 1885; break; //Elf
						case 12: Model = 1884; break; //Lurikeen

					}
					WraithFormTime();
				}
				else
				{
					Model = (ushort)m_client.Account.Characters[m_client.ActiveCharIndex].CreationModel;
				}
			}
		}

		protected RegionTimer WraithTimer;

		protected virtual int WraithForm(RegionTimer timer)
		{
			InWraithForm = false;

			timer.Stop();
			timer = null;
			return 0;
		}


		protected void WraithFormTime()
		{
			if (WraithTimer != null)
			{
				WraithTimer.Stop();
			}
			WraithTimer = null;
			WraithTimer = new RegionTimer(this, new RegionTimerCallback(WraithForm), 30000);
		}
		#endregion

		#region Champion Levels
		/// <summary> 
		/// The maximum champion level a player can reach 
		/// </summary> 
		public const int CL_MAX_LEVEL = 10;
		/// <summary> 
		/// A table that holds the required XP/Level 
		/// </summary> 
		public static readonly long[] CLXPLevel = 
        { 
            0, //xp tp level 0 
            32000, //xp to level 1 
         	64000, // xp to level 2 
         	96000, // xp to level 3 
         	128000, // xp to level 4 
         	160000, // xp to level 5 
         	192000, // xp to level 6 
         	224000, // xp to level 7 
         	256000, // xp to level 8 
         	288000, // xp to level 9 
         	320000, // xp to level 10 
        };
		/// <summary>
		/// Get the CL title string of the player
		/// </summary>
		public string CLTitle
		{
			get
			{
				if (Champion && ChampionLevel > 0)
					return LanguageMgr.GetTranslation(Client, String.Format("Titles.CL.Level{0}", ChampionLevel));
				else
					return "None";
			}
		}
		/// <summary>
		/// Is Champion level activated
		/// </summary>	        
		public virtual bool Champion
		{
			get { return PlayerCharacter != null ? PlayerCharacter.Champion : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.Champion = value; }
		}
		/// <summary>
		/// Champion level
		/// </summary>			
		public virtual int ChampionLevel
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ChampionLevel : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.ChampionLevel = value; }
		}

		/// <summary>
		/// Max champion level for the player
		/// </summary>
		public virtual int ChampionMaxLevel
		{
			get { return CL_MAX_LEVEL; }
		}
		/// <summary>
		/// Champion Experience
		/// </summary>		
		public virtual long ChampionExperience
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ChampionExperience : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.ChampionExperience = value; }
		}
		/// <summary>
		/// Champion Available speciality points
		/// </summary>		
		public virtual int ChampionSpecialtyPoints
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ChampionSpecialtyPoints : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.ChampionSpecialtyPoints = value; }
		}
		/// <summary>
		/// Serialised Champion spells
		/// </summary>		
		public virtual string ChampionSpells
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ChampionSpells : null; }
			set { if (PlayerCharacter != null) PlayerCharacter.ChampionSpells = value; }
		}
		/// <summary> 
		/// Returns how far into the champion level we have progressed 
		/// A value between 0 and 1000 (1 bubble = 100) 
		/// </summary> 
		public virtual ushort ChampionLevelPermill
		{
			get
			{
				//No progress if we haven't even reached current level!
				if (ChampionExperience <= ChampionExperienceForCurrentLevel)
					return 0;
				//No progess after maximum level 
				if (ChampionLevel > CL_MAX_LEVEL) // needed to get exp after 10 
					return 0;
				return (ushort)(1000 * (ChampionExperience - ChampionExperienceForCurrentLevel) / (ChampionExperienceForNextLevel - ChampionExperienceForCurrentLevel));

			}
		}
		/// <summary> 
		/// Returns the xp that are needed for the next level 
		/// </summary> 
		public virtual long ChampionExperienceForNextLevel
		{
			get { return GetChampionExperienceForLevel(ChampionLevel + 1); }
		}
		/// <summary> 
		/// Returns the xp that were needed for the current level 
		/// </summary> 
		public virtual long ChampionExperienceForCurrentLevel
		{
			get { return GetChampionExperienceForLevel(ChampionLevel); }
		}
		/// <summary>
		/// Gets/Sets amount of champion skills respecs
		/// (delegate to PlayerCharacter)
		/// </summary>
		public virtual int RespecAmountChampionSkill
		{
			get { return PlayerCharacter != null ? PlayerCharacter.RespecAmountChampionSkill : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.RespecAmountChampionSkill = value; }
		}
		/// <summary>
		/// Returns the xp that are needed for the specified level 
		/// </summary>         
		public virtual long GetChampionExperienceForLevel(int level)
		{
			if (level > CL_MAX_LEVEL)
				return CLXPLevel[GamePlayer.CL_MAX_LEVEL]; // exp for level 51, needed to get exp after 50 
			if (level <= 0)
				return CLXPLevel[0];
			return CLXPLevel[level];
		}
		/// <summary> 
		/// The process that gains exp 
		/// </summary> 
		/// <param name="experience">Amount of Experience</param> 
		public virtual void GainChampionExperience(long experience)
		{
			// Do not gain experience if champion not activated or if champion max level reached
			if (!Champion || ChampionLevel == CL_MAX_LEVEL)
				return;

			if (experience > 0)
			{
				double modifier = ServerProperties.Properties.XP_RATE;
				// 1 CLXP point per 333K normal XP
				if (this.CurrentRegion.IsRvR)
					experience = (long)((double)experience * modifier / 333000);
				else // 1 CLXP point per 2 Million normal XP
					experience = (long)((double)experience * modifier / 2000000);
			}
			System.Globalization.NumberFormatInfo format = System.Globalization.NumberFormatInfo.InvariantInfo;
			string totalexp = experience.ToString("N0", format);

			// Wtf this screws up level 0 
			if (ChampionExperience + experience < ChampionExperienceForCurrentLevel)
				experience = ChampionExperienceForCurrentLevel - ChampionExperience;

			if (experience > 0)
			{
				System.Globalization.NumberFormatInfo format2 = System.Globalization.NumberFormatInfo.InvariantInfo;
				string totalXP = experience.ToString("N0", format2);

				Out.SendMessage("You get " + totalXP + " champion experience points.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			ChampionExperience += experience; // force usage of this method, Experience property cannot be set 

			Out.SendUpdatePoints();
		}
		/// <summary> 
		/// Holds what happens when your champion level goes up; 
		/// </summary> 
		public virtual void ChampionLevelUp()
		{
			ChampionLevel++;
			ChampionSpecialtyPoints++;
			/*
			//Code for w/e happens when your CL goes up... 
			if (ChampionLevel == 3) 
			{ 
				switch (Realm) 
				{ 
				   case 1: 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Slashing)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Thrusting)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Crushing)); 
					  break; 
				   case 2: 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Axes)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Hammers)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Swords)); 
					  break; 
				   case 3: 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blades)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Blunt)); 
					  AddAbility(SkillBase.GetAbility(Abilities.Weapon_Piercing)); 
					  break; 
				} 
				AddAbility(SkillBase.GetAbility(Abilities.Shield, ShieldLevel.Small)); 
			} 
			*/
			Notify(GamePlayerEvent.ChampionLevelUp, this);
			Out.SendMessage("You have gained one champion level!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			Out.SendUpdatePlayer();
			Out.SendUpdatePoints();
			UpdatePlayerStatus();
		}
		/// <summary> 
		/// Load champion spells of this player 
		/// </summary>  
		protected virtual void LoadChampionSpells()
		{
			string championSpells = ChampionSpells;
			Hashtable championSpellsh = new Hashtable();
			SpellLine line = new SpellLine("Champion Abilities" + Name, "Champion Abilities", "Champion Abilities", true);
			line.Level = 50;
			SkillBase.RegisterSpellLine(line);
			if (championSpells != null && championSpells.Length > 0)
			{
				foreach (string cSpell in championSpells.Split(';'))
				{
					string[] cSpellProp = cSpell.Split('|');
					if (cSpellProp.Length < 2) continue;
					championSpellsh.Add(cSpellProp[0], int.Parse(cSpellProp[0]));
				}
			}
			if (championSpellsh != null)
			{
				foreach (DictionaryEntry de in championSpellsh)
				{
					SkillBase.AddSpellToList("Champion Abilities" + Name, (int)de.Value);
				}
				AddSpellLine(line);
			}
			championSpellsh = null;
		}
		/// <summary> 
		/// Checks if player has this champion spell 
		/// </summary>         
		public virtual bool HaveChampionSpell(int spellid)
		{
			string championSpells = ChampionSpells;
			if (championSpells != null && championSpells.Length > 0)
			{
				foreach (string cSpell in championSpells.Split(';'))
				{
					string[] cSpellProp = cSpell.Split('|');
					if (cSpellProp.Length < 2) continue;
					if (int.Parse(cSpellProp[0]) == spellid) return true;
				}
			}
			return false;
		}
		/// <summary> 
		/// Returns if spell is available (for trainer window)
		/// </summary> 
		public virtual bool IsCSAvailable(int idline, int skillindex, int index)
		{
			// TODO : this has to be reviewed. Original code has some problem with cross lines etc.
			// Andraste - reviewed by Vico
			ChampSpec spec = null;
			ChampSpec specA = null;
			ChampSpec specB = null;
			if (index == 1) return true;
			else
			{
				spec = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex, index - 1);
				if (spec == null)
				{
					specA = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex - 1, index - 1);
					specB = ChampSpecMgr.GetAbilityFromIndex(idline, skillindex + 1, index - 1);
					if ((specA != null && HaveChampionSpell(specA.SpellID)) || (specB != null && HaveChampionSpell(specB.SpellID))) return true;
					else return false;
				}
				else return HaveChampionSpell(spec.SpellID);
			}
		}
		#endregion

		#region Master levels
		/// <summary> 
		/// The maximum ML level a player can reach 
		/// </summary>
		public const int ML_MAX_LEVEL = 10;

		/// <summary> 
		/// Amount of MLXP required for ML validation. MLXP reset at every ML.
		/// </summary>	
		public static readonly long[] MLXPLevel = 
        { 
            0, //xp tp level 0 
            32000, //xp to level 1 
         	32000, // xp to level 2 
         	32000, // xp to level 3 
         	32000, // xp to level 4 
         	32000, // xp to level 5 
         	32000, // xp to level 6 
         	32000, // xp to level 7 
         	32000, // xp to level 8 
         	32000, // xp to level 9 
         	32000, // xp to level 10 
        };
		/// <summary>
		/// Get the ML title string of the player
		/// </summary>
		public string MLTitle
		{
			get
			{
				if (ML > 0)
					return LanguageMgr.GetTranslation(Client, String.Format("Titles.ML.Line{0}", (int)ML));
				else
					return "None";
			}
		}
		/// <summary> 
		/// Holds the ml line 
		/// </summary> 
		public virtual byte ML
		{
			get { return PlayerCharacter != null ? PlayerCharacter.ML : (byte)0; }
			set { if (PlayerCharacter != null) PlayerCharacter.ML = value; }
		}
		/// <summary> 
		/// Gets and sets the ML Level of this character 
		/// </summary> 
		public virtual int MLLevel
		{
			get { return PlayerCharacter != null ? PlayerCharacter.MLLevel : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.MLLevel = value; }
		}
		/// <summary> 
		/// Gets and sets ML Experience 
		/// </summary> 
		public virtual long MLExperience
		{
			get { return PlayerCharacter != null ? PlayerCharacter.MLExperience : 0; }
			set { if (PlayerCharacter != null) PlayerCharacter.MLExperience = value; }
		}
		/// <summary> 
		/// Gets and sets the ML validated flag. This allow to get new ML to arbiter
		/// </summary> 
		public virtual bool MLGranted
		{
			get { return PlayerCharacter != null ? PlayerCharacter.MLGranted : false; }
			set { if (PlayerCharacter != null) PlayerCharacter.MLGranted = value; }
		}
		/// <summary>
		/// Check ML step completition
		/// </summary>   
		public bool HasFinishedMLStep(int mllevel, int step)
		{
			// No steps registered so false
			if (m_mlsteps == null) return false;
			// Current ML Level >= required ML, so true
			if (MLLevel >= mllevel) return true;

			// Check current registered steps
			foreach (DBCharacterXMasterLevel mlstep in m_mlsteps)
			{
				// Found so return value
				if (mlstep.MLLevel == mllevel && mlstep.MLStep == step)
					return mlstep.StepCompleted;
			}

			// Not found so false
			return false;
		}
		/// <summary>
		/// Set ML step completition
		/// </summary>   
		public void SetFinishedMLStep(int mllevel, int step)
		{
			// Check current registered steps in case of previous GM rollback command
			if (m_mlsteps != null)
			{
				foreach (DBCharacterXMasterLevel mlstep in m_mlsteps)
				{
					if (mlstep.MLLevel == mllevel && mlstep.MLStep == step)
					{
						mlstep.StepCompleted = true;
						return;
					}
				}
			}

			// Register new step
			DBCharacterXMasterLevel newstep = new DBCharacterXMasterLevel();
			newstep.CharName = Name;
			newstep.MLLevel = mllevel;
			newstep.MLStep = step;
			newstep.StepCompleted = true;
			newstep.ValidationDate = DateTime.Now;
			m_mlsteps.Add(newstep);

			// Add it in DB
			try
			{
				GameServer.Database.AddNewObject(newstep);
			}
			catch (Exception e)
			{
				if (log.IsErrorEnabled)
					log.Error("Error adding player " + Name + " ml step!", e);
			}

			// Refresh Window
			Out.SendMasterLevelWindow((byte)mllevel);
		}
		/// <summary>
		/// Returns the xp that are needed for the specified level 
		/// </summary>         
		public virtual long GetMLExperienceForLevel(int level)
		{
			if (level >= ML_MAX_LEVEL)
				return MLXPLevel[GamePlayer.ML_MAX_LEVEL - 1]; // exp for level 9, needed to get exp after 9 
			if (level <= 0)
				return MLXPLevel[0];
			return MLXPLevel[level];
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

		/// <summary>
		/// Creates a new player
		/// </summary>
		/// <param name="client">The GameClient for this player</param>
		/// <param name="theChar">The character for this player</param>
		public GamePlayer(GameClient client, Character theChar)
			: base()
		{
			m_steed = new WeakRef(null);
			m_rangeAttackAmmo = new WeakRef(null);
			m_rangeAttackTarget = new WeakRef(null);
			m_client = client;
			my_character = theChar;
			m_controlledHorse = new ControlledHorse(this);
			m_buff1Bonus = new PropertyIndexer((int)eProperty.MaxProperty); // set up a fixed indexer for players
			m_buff2Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_buff3Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_buff4Bonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_itemBonus = new PropertyIndexer((int)eProperty.MaxProperty);
			m_lastUniqueLocations = new GameLocation[4];
			m_objectUpdates = new BitArray[2];
			m_objectUpdates[0] = new BitArray(Region.MAXOBJECTS);
			m_objectUpdates[1] = new BitArray(Region.MAXOBJECTS);
			m_housingUpdateArray = null;
			m_lastUpdateArray = 0;
			m_lastNPCUpdate = Environment.TickCount;
			m_inventory = new GamePlayerInventory(this);
			GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemEquipped, new DOLEventHandler(OnItemEquipped));
			GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemUnequipped, new DOLEventHandler(OnItemUnequipped));
			GameEventMgr.AddHandler(m_inventory, PlayerInventoryEvent.ItemBonusChanged, new DOLEventHandler(OnItemBonusChanged));
			m_enteredGame = false;
			m_customDialogCallback = null;
			m_sitting = false;
			m_saveInDB = true; // always save char data in db
			m_class = new DefaultCharacterClass();
			m_groupIndex = -1;
			LoadFromDatabase(theChar);
			m_currentAreas = new ArrayList(1);
		}
	}
}
