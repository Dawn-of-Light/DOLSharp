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
using System.Timers;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// AbstractGameKeep is the keep or a tower in game in RVR
	/// </summary>
	public abstract class AbstractGameKeep : IKeep
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public FrontiersPortalStone TeleportStone;
		public KeepArea Area;

		/// <summary>
		/// The time interval in milliseconds that defines how
		/// often guild bounty points should be removed
		/// </summary>
		protected readonly int CLAIM_CALLBACK_INTERVAL = 60 * 60 * 1000;

		/// <summary>
		/// Timer to remove bounty point and add realm point to guild which own keep
		/// </summary>
		protected RegionTimer m_claimTimer;

		/// <summary>
		/// Timerto upgrade the keep level
		/// </summary>
		protected RegionTimer m_changeLevelTimer;

		protected long m_lastAttackedByEnemyTick = 0;
		public long LastAttackedByEnemyTick
		{
			get { return m_lastAttackedByEnemyTick; }
			set 
			{
				// if we aren't currently in combat then treat this attack as the beginning of combat
				if (!InCombat)
				{
					bool underAttack = false;
					foreach (GameKeepDoor door in this.Doors.Values)
					{
						if (door.State == eDoorState.Open)
						{
							underAttack = true;
							break;
						}
					}

					if (!underAttack || StartCombatTick == 0)
						StartCombatTick = value;
				}

				m_lastAttackedByEnemyTick = value;
			}
		}

		protected long m_startCombatTick = 0;
		public long StartCombatTick
		{
			get { return m_startCombatTick; }
			set { m_startCombatTick = value; }
		}

		public bool InCombat
		{
			get
			{
				if (m_lastAttackedByEnemyTick == 0)
					return false;
				return m_currentRegion.Time - m_lastAttackedByEnemyTick < (5 * 60 * 1000); 
			}
		}

		public bool IsPortalKeep
		{
			get
			{
				return ((this is GameKeepTower && this.KeepComponents.Count > 1) || this.BaseLevel >= 100);
			}
		}

		/// <summary>
		/// The Keep Type
		/// This enum holds type of keep related to shape - Repurposed this due to game changes that eliminated melee/magic... keep types
		/// </summary>
		public enum eKeepType: byte
		{
			/*
				update keep set keeptype = 0 where keeptype = 1;
				update keep set keeptype = 1 where name = 'Dun Crauchon' or name = 'Bledmeer Faste' or name = 'Caer Benowyc';
				update keep set keeptype = 2 where name = 'Dun Crimthain' or name = 'Nottmoor Faste' or name = 'Caer Berkstead';
				update keep set keeptype = 3 where name = 'Dun Bolg' or name = 'Hlidskialf Faste' or name = 'Caer Erasleigh';
				update keep set keeptype = 4 where name = 'Dun nGed' or name = 'Glenlock Faste' or name = 'Caer Boldiam';
				update keep set keeptype = 5 where name = 'Dun Da Behnn' or name = 'Blendrake Faste' or name = 'Caer Sursbrooke';
				update keep set keeptype = 6 where name = 'Dun Scathaig' or name = 'Fensalir Faste' or name = 'Caer Renaris';
				update keep set keeptype = 7 where name = 'Dun Ailinne' or name = 'Arvakr Faste' or name = 'Caer Hurbury';
			*/
			Any = 0,
			Crauchon_Bledmeer_Benowyc = 1,
			Crimthain_Nottmoor_Berkstead = 2,
			Bolg_Hlidskialf_Erasleigh = 3,
			nGed_Glenlock_Boldiam = 4,
			DaBehnn_Blendrake_Sursbrooke = 5,
			Scathaig_Fensalir_Renaris = 6,
			Ailinne_Arvakr_Hurbury = 7,
		}

		#region Properties

		/// <summary>
		/// This hold all keep components
		/// </summary>
		protected List<GameKeepComponent> m_keepComponents;

		/// <summary>
		/// Keep components ( wall, tower, gate,...)
		/// </summary>
		public List<GameKeepComponent> KeepComponents
		{
			get	{ return m_keepComponents; }
			set { m_keepComponents = value;}
		}

		/// <summary>
		/// This hold list of all keep doors
		/// </summary>
		//protected ArrayList m_doors;
		protected Hashtable m_doors;

		/// <summary>
		/// keep doors
		/// </summary>
		//public ArrayList Doors
		public Hashtable Doors
		{
			get	{ return m_doors; }
			set { m_doors = value; }
		}

		/// <summary>
		/// the keep db object
		/// </summary>
		protected DBKeep m_dbkeep;

		/// <summary>
		/// the keepdb object
		/// </summary>
		public DBKeep DBKeep
		{
			get	{ return m_dbkeep; }
			set { m_dbkeep = value;}
		}

		/// <summary>
		/// This hold list of all guards of keep
		/// </summary>
		protected Hashtable m_guards;

		/// <summary>
		/// List of all guards of keep
		/// </summary>
		public Hashtable Guards
		{
			get	{ return m_guards; }
		}

		/// <summary>
		/// List of all banners
		/// </summary>
		protected Hashtable m_banners;

		/// <summary>
		/// List of all banners
		/// </summary>
		public Hashtable Banners
		{
			get	{ return m_banners; }
			set	{ m_banners = value; }
		}

		protected Hashtable m_patrols;
		/// <summary>
		/// List of all patrols
		/// </summary>
		public Hashtable Patrols
		{
			get { return m_patrols; }
			set { m_patrols = value; }
		}

		/// <summary>
		/// region of the keep
		/// </summary>
		protected Region m_currentRegion;

		/// <summary>
		/// region of the keep
		/// </summary>
		public Region CurrentRegion
		{
			get	{ return m_currentRegion; }
			set	{ m_currentRegion = value; }
		}

		/// <summary>
		/// zone of the keep
		/// </summary>
		public Zone CurrentZone
		{
			get
			{
				if (m_currentRegion != null)
				{
					return m_currentRegion.GetZone(X, Y);
				}
				return null;
			}
		}

		/// <summary>
		/// This hold the guild which has claimed the keep
		/// </summary>
		protected Guild m_guild = null;

		/// <summary>
		/// The guild which has claimed the keep
		/// </summary>
		public Guild Guild
		{
			get	{ return m_guild; }
			set { m_guild = value; }
		}

		/// <summary>
		/// Difficulty level of keep for each realm
		/// the keep is more difficult the guild which have claimed gain more bonus
		/// </summary>
		protected int[] m_difficultyLevel = new int[3];

		/// <summary>
		/// Difficulty level of keep for each realm
		/// the keep is more difficult the guild which have claimed gain more bonus
		/// </summary>
		public int DifficultyLevel
		{
			get
			{
				return m_difficultyLevel[(int)Realm - 1];
			}
		}

		public virtual int RealmPointsValue()
		{
			if (this is GameKeepTower)
			{
				return ServerProperties.Properties.TOWER_RP_BASE;
			}
			else
			{
				return ServerProperties.Properties.KEEP_RP_BASE;
			}
		}

		/// <summary>
		/// Respawn time for the lord of this keep (milliseconds)
		/// </summary>
		public virtual int LordRespawnTime
		{
			get { return 5000; }
		}


		#region DBKeep Properties
		/// <summary>
		/// The Keep ID linked to the DBKeep
		/// </summary>
		public int KeepID
		{
			get	{ return DBKeep.KeepID; }
			set	{ DBKeep.KeepID = value; }
		}

		/// <summary>
		/// The Keep Level linked to the DBKeep (0 - 10)
		/// </summary>
		public byte Level
		{
			get	{ return DBKeep.Level; }
			set	{ DBKeep.Level = value; }
		}

		protected byte m_baseLevel = 0;

		/// <summary>
		/// The Base Keep Level, typically 50 or the BG cap level
		/// </summary>
		public byte BaseLevel
		{
			get
			{
				if (m_baseLevel == 0)
					m_baseLevel = DBKeep.BaseLevel;
				return m_baseLevel;
			}
			set
			{
				m_baseLevel = value;
				//DBKeep.BaseLevel = value;
			}
		}

		/// <summary>
		/// calculate the effective level from a keep level
		/// </summary>
		public byte EffectiveLevel(byte level)
		{
			return (byte)Math.Max(0, level - 1);
		}

		/// <summary>
		/// The Keep Name linked to the DBKeep
		/// </summary>
		public string Name
		{
			get	
			{
				string name = DBKeep.Name;

				if (ServerProperties.Properties.ENABLE_DEBUG)
				{
					name += string.Format(" KID: {0}", KeepID);
				}

				return name;

			}
			set	{ DBKeep.Name = value; }
		}

		/// <summary>
		/// The Keep Region ID linked to the DBKeep
		/// </summary>
		public ushort Region
		{
			get	{ return DBKeep.Region; }
			set	{ DBKeep.Region = value; }
		}

		/// <summary>
		/// The Keep X linked to the DBKeep
		/// </summary>
		public int X
		{
			get	{ return DBKeep.X; }
			set	{ DBKeep.X = value; }
		}

		/// <summary>
		/// The Keep Y linked to the DBKeep
		/// </summary>
		public int Y
		{
			get	{ return DBKeep.Y; }
			set	{ DBKeep.Y = value; }
		}

		/// <summary>
		/// The Keep Z linked to the DBKeep
		/// </summary>
		public int Z
		{
			get	{ return DBKeep.Z; }
			set	{ DBKeep.Z = value; }
		}

		/// <summary>
		/// The Keep Heading linked to the DBKeep
		/// </summary>
		public ushort Heading
		{
			get	{ return DBKeep.Heading; }
			set	{ DBKeep.Heading = value; }
		}

		/// <summary>
		/// The Keep Realm linked to the DBKeep
		/// </summary>
		public eRealm Realm
		{
			get	{ return (eRealm)DBKeep.Realm; }
			set	{ DBKeep.Realm = (byte)value; }
		}

		/// <summary>
		/// The Original Keep Realm linked to the DBKeep
		/// </summary>
		public eRealm OriginalRealm
		{
			get	{ return (eRealm)DBKeep.OriginalRealm; }
		}

		protected string m_InternalID;
		/// <summary>
		/// The Keep Internal ID
		/// </summary>
		public string InternalID
		{
			get { return m_InternalID; }
			set { m_InternalID = value; }
		}

		/// <summary>
		/// The Keep Type
		/// </summary>
		public eKeepType KeepType
		{
			get	{ return (eKeepType)DBKeep.KeepType; }
			set
			{
				DBKeep.KeepType = (int)value;
			}
		}

		#endregion

		#endregion

		/// <summary>
		/// AbstractGameKeep constructor
		/// </summary>
		public AbstractGameKeep()
		{
			m_guards = new Hashtable();
			m_keepComponents = new List<GameKeepComponent>();
			m_banners = new Hashtable();
			m_doors = new Hashtable();
			m_patrols = new Hashtable();
		}

		#region LOAD/UNLOAD

		/// <summary>
		/// load keep from Db object and load keep component and object of keep
		/// </summary>
		/// <param name="keep"></param>
		public virtual void Load(DBKeep keep)
		{
			CurrentRegion = WorldMgr.GetRegion((ushort)keep.Region);
			InitialiseTimers();
			LoadFromDatabase(keep);
			GameEventMgr.AddHandler(CurrentRegion, RegionEvent.PlayerEnter, new DOLEventHandler(SendKeepInit));
			KeepArea area = null;
			//see if any keep areas for this keep have already been added via DBArea
			foreach (AbstractArea a in CurrentRegion.GetAreasOfSpot(keep.X, keep.Y, keep.Z))
			{
				if (a is KeepArea && a.Description == keep.Name)
				{
					log.Debug("Found a DBArea entry for " + keep.Name + ", loading that instead of creating a new one.");
					area = a as KeepArea;
					area.Keep = this;
					break;
				}
			}

			if (area == null)
			{
				area = new KeepArea(this);
				area.CanBroadcast = true;
				area.CheckLOS = true;
				CurrentRegion.AddArea(area);
			}
			area.Keep = this;
			this.Area = area;
		}

		/// <summary>
		/// Remove a keep from the database
		/// </summary>
		/// <param name="area"></param>
		public virtual void Remove(KeepArea area)
		{
			foreach (GameKeepGuard guard in (m_guards.Clone() as Hashtable).Values)
			{
				guard.Delete();
				guard.DeleteFromDatabase();
			}

			foreach (GameKeepBanner banner in (m_banners.Clone() as Hashtable).Values)
			{
				banner.Delete();
				banner.DeleteFromDatabase();
			}

			foreach (GameKeepDoor door in (m_doors.Clone() as Hashtable).Values)
			{
				door.Delete();
				GameDoor d = new GameDoor();
				d.CurrentRegionID = door.CurrentRegionID;
				d.DoorID = door.DoorID;
				d.Heading = door.Heading;
				d.Level = door.Level;
				d.Model = door.Model;
				d.Name = "door";
				d.Realm = door.Realm;
				d.State = eDoorState.Closed;
				d.X = door.X;
				d.Y = door.Y;
				d.Z = door.Z;
				DoorMgr.RegisterDoor(door);
				d.AddToWorld();
			}

			UnloadTimers();
			GameEventMgr.RemoveHandler(CurrentRegion, RegionEvent.PlayerEnter, new DOLEventHandler(SendKeepInit));
			CurrentRegion.RemoveArea(area);
			RemoveFromDatabase();
		}

		/// <summary>
		/// load keep from DB
		/// </summary>
		/// <param name="keep"></param>
		public virtual void LoadFromDatabase(DataObject keep)
		{
			m_dbkeep = keep as DBKeep;
			InternalID = keep.ObjectId;
			m_difficultyLevel[0] = m_dbkeep.AlbionDifficultyLevel;
			m_difficultyLevel[1] = m_dbkeep.MidgardDifficultyLevel;
			m_difficultyLevel[2] = m_dbkeep.HiberniaDifficultyLevel;
			if (m_dbkeep.ClaimedGuildName != null && m_dbkeep.ClaimedGuildName != "")
			{
				Guild myguild = GuildMgr.GetGuildByName(m_dbkeep.ClaimedGuildName);
				if (myguild != null)
				{
					this.m_guild = myguild;
					this.m_guild.ClaimedKeeps.Add(this);
					StartDeductionTimer();
				}
			}
			if (Level < ServerProperties.Properties.MAX_KEEP_LEVEL && m_guild != null)
				StartChangeLevel((byte)ServerProperties.Properties.MAX_KEEP_LEVEL);
			else if (Level <= ServerProperties.Properties.MAX_KEEP_LEVEL && Level > ServerProperties.Properties.STARTING_KEEP_LEVEL && m_guild == null)
				StartChangeLevel((byte)ServerProperties.Properties.STARTING_KEEP_LEVEL);
		}

		/// <summary>
		/// remove keep from DB
		/// </summary>
		public virtual void RemoveFromDatabase()
		{
			GameServer.Database.DeleteObject(m_dbkeep);
		}

		/// <summary>
		/// save keep in DB
		/// </summary>
		public virtual void SaveIntoDatabase()
		{
			if (m_guild != null)
				m_dbkeep.ClaimedGuildName = m_guild.Name;
			else
				m_dbkeep.ClaimedGuildName = "";
			if(InternalID == null)
			{
				GameServer.Database.AddNewObject(m_dbkeep);
				InternalID = m_dbkeep.ObjectId;
			}
			else
				GameServer.Database.SaveObject(m_dbkeep);

			foreach (GameKeepComponent comp in this.KeepComponents)
				comp.SaveIntoDatabase();
		}


		#endregion

		#region Claim

		public virtual bool CheckForClaim(GamePlayer player)
		{
			if(player.Realm != this.Realm)
			{
				player.Out.SendMessage("The keep is not owned by your realm.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}

			if (this.DBKeep.BaseLevel != 50)
			{
				player.Out.SendMessage("This keep is not able to be claimed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Guild == null)
			{
				player.Out.SendMessage("You must be in a guild to claim a keep.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			if (!player.Guild.GotAccess(player, eGuildRank.Claim))
			{
				player.Out.SendMessage("You do not have permission to claim for your guild.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			if (this.Guild != null)
			{
				player.Out.SendMessage("The keep is already claimed.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			switch (ServerProperties.Properties.GUILDS_CLAIM_LIMIT)
			{
				case 0:
					{
						player.Out.SendMessage("Keep claiming is disabled!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return false;
					}
				case 1:
					{
						if (player.Guild.ClaimedKeeps.Count == 1)
						{
							player.Out.SendMessage("Your guild already owns a keep.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}
				default:
					{
						if (player.Guild.ClaimedKeeps.Count >= ServerProperties.Properties.GUILDS_CLAIM_LIMIT)
						{
							player.Out.SendMessage("Your guild already owns the limit of keeps (" + ServerProperties.Properties.GUILDS_CLAIM_LIMIT + ")", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return false;
						}
						break;
					}
			}

			if (player.Group != null)
			{
				int count = 0;
				foreach (GamePlayer p in player.Group.GetPlayersInTheGroup())
				{
					if (KeepMgr.getKeepCloseToSpot(p.CurrentRegionID, p, WorldMgr.VISIBILITY_DISTANCE) == this)
						count++;
				}

				int needed = ServerProperties.Properties.CLAIM_NUM;
				if (this is GameKeepTower)
					needed = needed / 2;
				if (player.Client.Account.PrivLevel > 1)
					needed = 0;
				if (count < needed)
				{
					player.Out.SendMessage("Not enough group members are near the keep. You have " + count + "/" + needed + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
			}
			return true;
		}

		/// <summary>
		/// claim the keep to a guild
		/// </summary>
		/// <param name="player">the player who have claim the keep</param>
		public void Claim(GamePlayer player)
		{
			this.m_guild = player.Guild;
			player.Guild.ClaimedKeeps.Add(this);
			if (ServerProperties.Properties.GUILDS_CLAIM_LIMIT > 1)
				player.Guild.SendMessageToGuildMembers("Your guild has currently claimed " + player.Guild.ClaimedKeeps.Count + " keeps of a maximum of " + ServerProperties.Properties.GUILDS_CLAIM_LIMIT, eChatType.CT_Guild, eChatLoc.CL_ChatWindow);

			ChangeLevel((byte)ServerProperties.Properties.STARTING_KEEP_CLAIM_LEVEL);

			PlayerMgr.BroadcastClaim(this);

			foreach (GameKeepGuard guard in Guards.Values)
			{
				guard.ChangeGuild();
			}

			foreach (GameKeepBanner banner in Banners.Values)
			{
				banner.ChangeGuild();
			}
			GameEventMgr.Notify(KeepEvent.KeepClaimed, this, new KeepEventArgs(this));
			StartDeductionTimer();
			this.SaveIntoDatabase();
		}

		/// <summary>
		/// Starts the deduction timer
		/// </summary>
		public void StartDeductionTimer()
		{
			m_claimTimer.Start(1);
		}

		/// <summary>
		/// Stops the deduction timer
		/// </summary>
		public void StopDeductionTimer()
		{
			m_claimTimer.Stop();
		}

		protected void InitialiseTimers()
		{
			m_changeLevelTimer = new RegionTimer(CurrentRegion.TimeManager);
			m_changeLevelTimer.Callback = new RegionTimerCallback(ChangeLevelTimerCallback);
			m_claimTimer = new RegionTimer(CurrentRegion.TimeManager);
			m_claimTimer.Callback = new RegionTimerCallback(ClaimCallBack);
			m_claimTimer.Interval = CLAIM_CALLBACK_INTERVAL;
		}

		protected void UnloadTimers()
		{
			m_changeLevelTimer.Stop();
			m_claimTimer.Stop();
		}

		/// <summary>
		/// Callback method for the claim timer, it deducts bounty points and gains realm points for a guild
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public int ClaimCallBack(RegionTimer timer)
		{
			if (Guild == null)
				return 0;

			int amount = CalculRP();
			this.Guild.RealmPoints+=amount;

			return timer.Interval;
		}

		public virtual int CalculRP()
		{
			return 0;
		}

		#endregion

		#region Release

		/// <summary>
		/// released the keep of the guild
		/// </summary>
		public void Release()
		{
			this.Guild.ClaimedKeeps.Remove(this);
			PlayerMgr.BroadcastRelease(this);
			this.m_guild = null;
			StopDeductionTimer();
			StopChangeLevelTimer();
			ChangeLevel((byte)ServerProperties.Properties.STARTING_KEEP_LEVEL);

			foreach (GameKeepGuard guard in Guards.Values)
			{
				guard.ChangeGuild();
			}

			foreach (GameKeepBanner banner in Banners.Values)
			{
				banner.ChangeGuild();
			}

			this.SaveIntoDatabase();
		}
		#endregion

		#region Upgrade

		/// <summary>
		/// upgrade keep to a target level
		/// </summary>
		/// <param name="targetLevel">the target level</param>
		public void ChangeLevel(byte targetLevel)
		{
			this.Level = targetLevel;

			foreach (GameKeepComponent comp in this.KeepComponents)
			{
				comp.UpdateLevel();
				foreach (GameClient cln in WorldMgr.GetClientsOfRegion(this.CurrentRegion.ID))
					cln.Out.SendKeepComponentDetailUpdate(comp);
				comp.FillPositions();
			}

			foreach (GameKeepGuard guard in this.Guards.Values)
			{
				SetGuardLevel(guard);
			}

			foreach (Patrol p in this.Patrols.Values)
			{
				p.ChangePatrolLevel();
			}

			foreach (GameKeepDoor door in this.Doors.Values)
			{
				door.UpdateLevel();
			}

			KeepGuildMgr.SendLevelChangeMessage(this);
			ResetPlayersOfKeep();

			this.SaveIntoDatabase();
		}


		public virtual byte GetBaseLevel(GameKeepGuard guard)
		{
			if (guard.Component == null)
			{
				if (guard is GuardLord)
					return 75;
				else
					return 65;
			}

			if (guard is GuardLord)
			{
				if (guard.Component.Keep is GameKeep)
					return (byte)(guard.Component.Keep.BaseLevel + ((guard.Component.Keep.BaseLevel / 10) + 1) * 2);
				else
					return (byte)(guard.Component.Keep.BaseLevel + 2); // flat increase for tower captains
			}

			if (guard.Component.Keep is GameKeep)
				return (byte)(guard.Component.Keep.BaseLevel + 1);

			return guard.Component.Keep.BaseLevel;
		}


		public virtual void SetGuardLevel(GameKeepGuard guard)
		{
			if (guard is FrontierHastener)
			{
				guard.Level = 1;
			}
			else
			{
				int bonusLevel = 0;
				double multiplier = ServerProperties.Properties.KEEP_GUARD_LEVEL_MULTIPLIER;

				if (guard.Component != null)
				{
					// level is usually 4 unless upgraded, BaseLevel is usually 50
					bonusLevel = guard.Component.Keep.Level;

					if (guard.Component.Keep is GameKeepTower)
						multiplier = ServerProperties.Properties.TOWER_GUARD_LEVEL_MULTIPLIER;
				}

				guard.Level = (byte)(GetBaseLevel(guard) + (bonusLevel * multiplier));
			}
		}


		/// <summary>
		/// Start changing the keeps level to a target level
		/// </summary>
		/// <param name="targetLevel">The target level</param>
		public void StartChangeLevel(byte targetLevel)
		{
			if (ServerProperties.Properties.ENABLE_KEEP_UPGRADE_TIMER)
			{
				if (this.Level == targetLevel)
					return;
				//this.TargetLevel = targetLevel;
				StartChangeLevelTimer();
				if (this.Guild != null)
					KeepGuildMgr.SendChangeLevelTimeMessage(this);
			}
		}

		/// <summary>
		/// Time remaining for the single level change
		/// </summary>
		public TimeSpan ChangeLevelTimeRemaining
		{
			get
			{
				int totalsec = m_changeLevelTimer.TimeUntilElapsed / 1000;
				int min = (totalsec/60)%60;
				int hours = (totalsec/3600)%24;
				return new TimeSpan(0, hours, min + 1, totalsec%60);
			}
		}

		/// <summary>
		/// Time remaining for total level change
		/// </summary>
		public TimeSpan TotalChangeLevelTimeRemaining
		{
			get
			{
				//TODO
				return new TimeSpan();
			}
		}

		/// <summary>
		/// Starts the Change Level Timer
		/// </summary>
		public void StartChangeLevelTimer()
		{
			int newinterval = CalculateTimeToUpgrade();

			if (m_changeLevelTimer.IsAlive)
			{
				int timeelapsed = m_changeLevelTimer.Interval - m_changeLevelTimer.TimeUntilElapsed;
				//if timer has run for more then we need, run event instantly
				if (timeelapsed > m_changeLevelTimer.Interval)
					newinterval = 1;
				//change timer to the value we need
				else if (timeelapsed < newinterval)
					newinterval = m_changeLevelTimer.Interval - timeelapsed;
				m_changeLevelTimer.Interval = newinterval;
				
			}
			m_changeLevelTimer.Stop();
			m_changeLevelTimer.Start(newinterval);
		}

		/// <summary>
		/// Stops the Change Level Timer
		/// </summary>
		public void StopChangeLevelTimer()
		{
			m_changeLevelTimer.Stop();
			m_changeLevelTimer.Interval = 1;
		}

		/// <summary>
		/// Destroys the Change Level Timer
		/// </summary>
		public void DestroyChangeLevelTimer()
		{
			if (m_changeLevelTimer != null)
			{
				m_changeLevelTimer.Stop();
				m_changeLevelTimer = null;
			}
		}

		/// <summary>
		/// Change Level Timer Callback, this method handles the the action part of the change level timer
		/// </summary>
		/// <param name="timer"></param>
		/// <returns></returns>
		public int ChangeLevelTimerCallback(RegionTimer timer)
		{
			if (this is GameKeepTower)
			{
				foreach (GameKeepComponent component in this.KeepComponents)
				{
					/*
					 *  - A realm can claim a razed tower, and may even set it to raise to level 10,
					 * but will have to wait until the tower is repaired to 75% before 
					 * it will begin upgrading normally.
					 */
					if (component.HealthPercent < 75)
						return 5 * 60 * 1000;
				}
			}
			if (Level < 10 && m_guild != null)
				ChangeLevel((byte)(this.Level + 1));
			else if (Level > 1 && m_guild == null)
				ChangeLevel((byte)(this.Level - 1));

			if (this.Level != 10 && this.Level != 1)
			{
				return CalculateTimeToUpgrade();
			}
			else
			{
				this.SaveIntoDatabase();
				return 0;
			}
		}

		/// <summary>
		/// calculate time to upgrade keep, in milliseconds
		/// </summary>
		/// <returns></returns>
		public virtual int CalculateTimeToUpgrade()
		{
			//todo : relik owner to modify formulat
			/*
			0 Relics owned - Upgrade time from level 5 to level 10 is 3.2 hours
			1 Relic owned - Upgrade time from level 5 to level 10 is 8 hours
			2 Relics owned - Upgrade time from level 5 to level 10 is 24 hours
			3 or 4 Relics owned - Upgrade time from level 5 to level 10 remains unchanged (32 hours)
			5 Relics owned - Upgrade time from level 5 to level 10 is 48 hours
			6 Relics owned - Upgrade time from level 5 to level 10 is 64 hours
			*/
			return 0;
		}
		#endregion

		#region Reset

		/// <summary>
		/// reset the realm when the lord have been killed
		/// </summary>
		/// <param name="realm"></param>
		public virtual void Reset(eRealm realm)
		{
			LastAttackedByEnemyTick = 0;
			StartCombatTick = 0;

			Realm = realm;

			PlayerMgr.BroadcastCapture(this);

            Level = (byte)ServerProperties.Properties.STARTING_KEEP_LEVEL;

			//if a guild holds the keep, we release it
			if (Guild != null)
			{
				Release();
			}
			//we repair all keep components, but not if it is a tower and is raised
			foreach (GameKeepComponent component in this.KeepComponents)
			{
				if (!component.IsRaized)
					component.Repair(component.MaxHealth - component.Health);
				foreach (GameKeepHookPoint hp in component.HookPoints.Values)
				{
					if (hp.Object != null)
						hp.Object.Die(null);
				}
			}
			//change realm
			foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegion.ID))
			{
				client.Out.SendKeepComponentUpdate(this, false);
			}
			//we reset all doors
			foreach(GameKeepDoor door in Doors.Values)
			{
				door.Reset(realm);
			}

			//we make sure all players are not in the air
			ResetPlayersOfKeep();

			//we reset the guards
			foreach (GameKeepGuard guard in Guards.Values)
			{
				if (guard is GuardLord == false)
				{
					guard.Die(guard);
				}
			}

			//we reset the banners
			foreach (GameKeepBanner banner in Banners.Values)
			{
				banner.ChangeRealm();
			}

			//update guard level for every keep
			if (!IsPortalKeep && ServerProperties.Properties.USE_KEEP_BALANCING)
				KeepMgr.UpdateBaseLevels();

			//update the counts of keeps for the bonuses
			if (ServerProperties.Properties.USE_LIVE_KEEP_BONUSES)
				KeepBonusMgr.UpdateCounts();

			SaveIntoDatabase();

			GameEventMgr.Notify(KeepEvent.KeepTaken, new KeepEventArgs(this));

		}

		/// <summary>
		/// This method is important, because players could fall through air
		/// if they are on the top of a keep when it is captured because
		/// the keep size will reset
		/// </summary>
		protected void ResetPlayersOfKeep()
		{
			ushort distance = 0;
			int id = 0;
			if (this is GameKeepTower)
			{
				distance = 750;
				id = 11;
			}
			else
			{
				distance = 1500;
				id = 10;
			}


			GameKeepComponent component = null;
			foreach (GameKeepComponent c in this.KeepComponents)
			{
				if (c.Skin == id)
				{
					component = c;
					break;
				}
			}
			if (component == null)
				return;

			GameKeepHookPoint hookpoint = component.HookPoints[97] as GameKeepHookPoint;

			if (hookpoint == null)
				return;

			//calculate target height
			int height = KeepMgr.GetHeightFromLevel(this.Level);

			//predict Z
			DBKeepHookPoint hp = (DBKeepHookPoint)GameServer.Database.SelectObject(typeof(DBKeepHookPoint), "HookPointID = '97' and Height = '" + height + "'");
			if (hp == null)
				return;
			int z = component.Z + hp.Z;

			foreach (GamePlayer player in component.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
                int d = hookpoint.GetDistance( player as IPoint2D );
				if (d > distance)
					continue;

				if (player.Z > z)
					player.MoveTo(player.CurrentRegionID, player.X, player.Y, z, player.Heading);
			}
		}


		#endregion

		/// <summary>
		/// send keep init when player enter in region
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		protected void SendKeepInit(DOLEvent e, object sender, EventArgs args)
		{
			RegionPlayerEventArgs regionPlayerEventArgs = args as RegionPlayerEventArgs;
			GamePlayer player = regionPlayerEventArgs.Player;
			player.Out.SendKeepInfo(this);
			foreach(GameKeepComponent keepComponent in this.KeepComponents)
			{
				player.Out.SendKeepComponentInfo(keepComponent);
			}
		}
	}
}
