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
using System.Timers;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
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

		/// <summary>
		/// The time interval in milliseconds that defines how
		/// often guild bounty points should be removed
		/// </summary>
		private readonly int CLAIM_CALLBACK_INTERVAL = 60*60*1000;

		/// <summary>
		/// Timer to remove bounty point and add realm point to guild which own keep
		/// </summary>
		private RegionTimer m_claimTimer;

		/// <summary>
		/// Timerto upgrade the keep level
		/// </summary>
		private RegionTimer m_changeLevelTimer;

		/// <summary>
		///
		/// </summary>
		public enum eKeepType: byte
		{
			/// <summary>
			/// default type when not claimed, it is like melee
			/// </summary>
			Generic = 0x00,
			/// <summary>
			///  some guard inside the keep are Armsman (when claimed)
			/// </summary>
			Melee = 0x01,
			/// <summary>
			///  some guard inside the keep are wizard (when claimed)
			/// </summary>
			Magic = 0x02,
			/// <summary>
			///  some guard inside the keep are scout (when claimed)
			/// </summary>
			Stealth = 0x04,
		}

		#region properties

		/// <summary>
		/// This hold all keep components
		/// </summary>
		private ArrayList m_keepComponents;

		/// <summary>
		/// Keep components ( wall, tower, gate,...)
		/// </summary>
		public ArrayList KeepComponents
		{
			get	{ return m_keepComponents; }
			set { m_keepComponents = value;}
		}

		/// <summary>
		/// This hold list of all keep doors
		/// </summary>
		private ArrayList m_doors;

		/// <summary>
		/// keep doors
		/// </summary>
		public ArrayList Doors
		{
			get	{ return m_doors; }
			set { m_doors = value; }
		}

		/// <summary>
		/// the keep db object
		/// </summary>
		private DBKeep m_dbkeep;

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
		private ArrayList m_guards;

		/// <summary>
		/// List of all guards of keep
		/// </summary>
		public ArrayList Guards
		{
			get	{ return m_guards; }
		}

		/// <summary>
		/// This lord of keep
		/// </summary>
		private GameKeepGuard m_lord;

		/// <summary>
		/// Lord of keep or captain of tower which must be killed to take control of keep
		/// </summary>
		public GameKeepGuard Lord
		{
			get	{ return m_lord; }
			set	{ m_lord = value; }
		}

		/// <summary>
		/// List of all banners
		/// </summary>
		private ArrayList m_banners;

		/// <summary>
		/// List of all banners
		/// </summary>
		public ArrayList Banners
		{
			get	{ return m_banners; }
			set	{ m_banners = value; }
		}

		/// <summary>
		/// region of the keep
		/// </summary>
		private Region m_currentRegion;

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
		/// This hold the guild which have claimed the keep
		/// </summary>
		private Guild m_guild = null;

		/// <summary>
		/// The guild which have claimed the keep
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
		private int[] m_difficultyLevel = new int[3];

		/// <summary>
		/// Difficulty level of keep for each realm
		/// the keep is more difficult the guild which have claimed gain more bonus
		/// </summary>
		public int DifficultyLevel
		{
			get
			{
				return m_difficultyLevel[Realm-1];
			}
		}
		private int m_targetLevel;
		public int TargetLevel
		{
			get
			{
				return m_targetLevel;
			}
			set
			{
				m_targetLevel = value;
			}
		}
		#region DBkeep properties

		public int KeepID
		{
			get	{ return DBKeep.KeepID; }
			set	{ DBKeep.KeepID = value; }
		}
		public byte Level
		{
			get	{ return DBKeep.Level; }
			set	{ DBKeep.Level = value; }
		}
		public string Name
		{
			get	{ return DBKeep.Name; }
			set	{ DBKeep.Name = value; }
		}

		public int Region
		{
			get	{ return DBKeep.Region; }
			set	{ DBKeep.Region = value; }
		}

		public int X
		{
			get	{ return DBKeep.X; }
			set	{ DBKeep.X = value; }
		}

		public int Y
		{
			get	{ return DBKeep.Y; }
			set	{ DBKeep.Y = value; }
		}
		public int Z
		{
			get	{ return DBKeep.Z; }
			set	{ DBKeep.Z = value; }
		}
		public int Heading
		{
			get	{ return DBKeep.Heading; }
			set	{ DBKeep.Heading = value; }
		}
		public byte Realm
		{
			get	{ return DBKeep.Realm; }
			set	{ DBKeep.Realm = value; }
		}
		public eRealm OriginalRealm
		{
			get	{ return (eRealm)DBKeep.OriginalRealm; }
		}
		private string m_InternalID;
		public string InternalID
		{
			get { return m_InternalID; }
			set { m_InternalID = value; }
		}
		public eKeepType KeepType
		{
			get	{ return (eKeepType)DBKeep.KeepType; }
			set
			{
				DBKeep.KeepType = (int)value;
				//todo : update all guard
			}
		}

		#endregion

		#endregion

		public AbstractGameKeep()
		{
			m_guards = new ArrayList(1);
			m_keepComponents = new ArrayList(1);
			m_banners = new ArrayList(1);
			m_doors = new ArrayList(1);
		}

		#region LOAD/UNLOAD

		/// <summary>
		/// load keep from Db object and load keep component and object of keep
		/// </summary>
		/// <param name="keep"></param>
		public void Load(DBKeep keep)
		{
			KeepMgr.Logger.Info("Loading: " + keep.Name);
			CurrentRegion = WorldMgr.GetRegion((ushort)keep.Region);
			InitialiseTimers();
			LoadFromDatabase(keep);
			GameEventMgr.AddHandler(CurrentRegion, RegionEvent.PlayerEnter, new DOLEventHandler(SendKeepInit));
			int radius;
			if (this is GameKeep)
			{
				radius = 3000;
			}
			else
			{
				radius = 1500;
			}
			Area.Circle circle = new Area.Circle(this.Name, this.X, this.Y, 0, radius);
			circle.CanBroadcast = true;
			CurrentRegion.AddArea(circle);
		}

		/// <summary>
		/// load keep from DB
		/// </summary>
		/// <param name="keep"></param>
		public void LoadFromDatabase(DataObject keep)
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
					this.m_guild.ClaimedKeep = this;
					StartDeductionTimer();
				}
			}
			if (m_targetLevel< Level)
				m_targetLevel = Level;
		}

		/// <summary>
		/// save keep in DB
		/// </summary>
		public void SaveIntoDatabase()
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

		/// <summary>
		/// load the object of keep which must inherit of GameObject
		/// This can be banner, guard, static item like catapult,...
		/// </summary>
		public void LoadObjects()
		{
			foreach (GameKeepComponent component in this.KeepComponents)
			{
				DBKeepPosition[] Positions = (DBKeepPosition[])GameServer.Database.SelectObjects(typeof(DBKeepPosition), "ComponentSkin = '" + component.Skin + "' and Height <= " + component.Height + " order by Height desc, TemplateID");
				Hashtable UsedPositions = new Hashtable();
				ArrayList UsablePositions = new ArrayList();
				foreach (DBKeepPosition pos in Positions)
				{
					if (UsedPositions[pos.TemplateID] == null)
					{
						UsedPositions.Add(pos.TemplateID, pos);
						UsablePositions.Add(pos);
					}
				}
				foreach (DBKeepPosition position in UsablePositions)
				{
					if (position.Height > component.Height) continue;
					Assembly asm = Assembly.GetExecutingAssembly();
					GameObject obj = (GameObject)asm.CreateInstance(position.ClassType, true);

					if (obj is GameKeepBanner)
					{
						(obj as GameKeepBanner).LoadFromPosition(position, component);
					}
					else if (obj is GameKeepGuard)
					{
						(obj as GameKeepGuard).LoadFromPosition(position, component);
					}
				}
			}
		}


		#endregion

		#region claim

		/// <summary>
		/// table of claim bounty point take from guild each cycle
		/// </summary>
		public static readonly int[] ClaimBountyPointCost =
		{
			0,
			50,
			50,
			50,
			50,
			100,
			200,
			300,
			400,
			500,
			1000,
		};

		public virtual bool CheckForClaim(GamePlayer player)
		{
			if((int)player.Realm != this.Realm)
			{
				player.Out.SendMessage("The keep is not owned by your realm.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
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
			if (player.Guild.BountyPoints < 500)
			{
				player.Out.SendMessage("Your guild must have at least 500 guild bounty points to claim.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			if (this.Guild != null)
			{
				player.Out.SendMessage("The keep is already claimed.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
			}
			if (player.Guild.ClaimedKeep != null)
			{
				player.Out.SendMessage("Your guild already owns a keep.",eChatType.CT_System,eChatLoc.CL_SystemWindow);
				return false;
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
			player.Guild.ClaimedKeep = this;

			ChangeLevel(1);

			PlayerMgr.BroadcastClaim(this);

			foreach (GameKeepGuard guard in Guards)
			{
				guard.ChangeGuild();
			}

			foreach (GameKeepBanner banner in Banners)
			{
				banner.ChangeGuild();
			}
			/*			foreach(GamePlayer plr in player.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
						{
							if (plr == null)
								continue;
							plr.Out.SendKeepClaim(this);
						}*/
			GameEventMgr.Notify(KeepEvent.KeepClaimed, this, new KeepEventArgs(player));
			StartDeductionTimer();
			this.SaveIntoDatabase();

		}

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

		private void InitialiseTimers()
		{
			m_changeLevelTimer = new RegionTimer(CurrentRegion.TimeManager);
			m_changeLevelTimer.Callback = new RegionTimerCallback(ChangeLevelTimerCallback);
			m_claimTimer = new RegionTimer(CurrentRegion.TimeManager);
			m_claimTimer.Callback = new RegionTimerCallback(ClaimCallBack);
		}

		public int ClaimCallBack(RegionTimer timer)
		{
			if (Guild == null)
				return 0;
			if (this.Guild.BountyPoints < 50 * this.Level)
			{
				this.Release();
				return 0;
			}
			int amount = CalculRP();
			this.Guild.GainRealmPoints(amount);

			int cost = ClaimBountyPointCost[this.Level];
			this.Guild.GainBountyPoints(-cost);
			return timer.Interval;
		}

		public virtual int CalculRP()
		{
			return 0;
		}

		#endregion

		#region release

		/// <summary>
		/// released the keep of the guild
		/// </summary>
		public void Release()
		{
			this.Guild.ClaimedKeep = null;
			PlayerMgr.BroadcastRelease(this);
			this.m_guild = null;
			StopDeductionTimer();
			StopChangeLevelTimer();
			ChangeLevel(0);
			this.SaveIntoDatabase();
		}
		#endregion
		#region upgrade

		/// <summary>
		/// upgrade keep to a target level
		/// </summary>
		/// <param name="targetLevel">the target level</param>
		public void ChangeLevel(byte targetLevel)
		{
			this.Level = targetLevel;
			foreach (GameKeepGuard guard in this.Guards)
			{
				TemplateMgr.RefreshTemplate(guard);
			}
			foreach (GameKeepComponent comp in this.KeepComponents)
			{
				comp.Update();
				foreach (GameClient cln in WorldMgr.GetClientsOfRegion(this.CurrentRegion.ID))
					cln.Out.SendKeepComponentDetailUpdate(comp);
			}
			KeepGuildMgr.SendLevelChangeMessage(this);
			ResetPlayersOfKeep();
			this.SaveIntoDatabase();
		}

		public void StartChangeLevel(int targetLevel)
		{
			if (this.Level == targetLevel)
				return;
			this.TargetLevel = targetLevel;
			StartChangeLevelTimer();
			if (this.Guild != null)
				KeepGuildMgr.SendChangeLevelTimeMessage(this);
		}

		public TimeSpan ChangeLevelTimeRemaining
		{
			get
			{
				return new TimeSpan(m_changeLevelTimer.TimeUntilElapsed);
			}
		}

		/// <summary>
		/// Starts the Change Level Timer
		/// </summary>
		public void StartChangeLevelTimer()
		{
			int modifier = 1;
			if (TargetLevel < Level)
				modifier = -1;

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

		public int ChangeLevelTimerCallback(RegionTimer timer)
		{
			if (TargetLevel > Level)
				ChangeLevel((byte)(this.Level + 1));
			else
				ChangeLevel((byte)(this.Level - 1));

			if (this.Level != this.TargetLevel)
			{
				return CalculateTimeToUpgrade();
			}
			else
			{
				this.TargetLevel = 0;
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

		#region reset

		/// <summary>
		/// reset the realm when the lord have been killed
		/// </summary>
		/// <param name="realm"></param>
		public void Reset(eRealm realm)
		{
			string realmstr = GlobalConstants.RealmToName(realm);
			foreach (GameClient cl in WorldMgr.GetAllPlayingClients())
			{
				cl.Player.Out.SendMessage("The Forces of " + realmstr + " have captured " + this.Name + "!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			Realm = (byte)realm;
			Level = 0;
			KeepType = eKeepType.Melee;
			if (Guild != null)
			{
				Release();
			}
			foreach(GameKeepDoor door in Doors)
			{
				door.Reset(realm);
			}
			foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegion.ID))
			{
				client.Player.Out.SendKeepComponentUpdate(this,false);
			}

			ResetPlayersOfKeep();

			foreach (GameKeepGuard guard in Guards)
			{
				TemplateMgr.RefreshTemplate(guard);
			}

			foreach (GameKeepBanner banner in Banners)
			{
				banner.ChangeRealm();
			}
			SaveIntoDatabase();
		}

		private void ResetPlayersOfKeep()
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
				int d = WorldMgr.GetDistance(player.X, player.Y, 0, hookpoint.X, hookpoint.Y, 0, 0);
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
//todo : patrol
//TODO : keep bonus link to server rules