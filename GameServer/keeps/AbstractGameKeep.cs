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
using System.Threading;
using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS
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
		private readonly Timer m_claimTimer;

		/// <summary>
		/// Timerto upgrade the keep level
		/// </summary>
		private readonly Timer m_upgradeTimer;

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
		public int Level
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
		public int Realm
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
			m_claimTimer = new Timer(new TimerCallback(ClaimCallBack), null, Timeout.Infinite, Timeout.Infinite);
			m_upgradeTimer = new Timer(new TimerCallback(UpgradeTimerCallback), null, Timeout.Infinite, Timeout.Infinite);
		}

		#region LOAD/UNLOAD

		/// <summary>
		/// load keep from Db object and load keep component and object of keep
		/// </summary>
		/// <param name="keep"></param>
		public void Load(DBKeep keep)
		{
			LoadFromDatabase(keep);
			LoadObjects();
			GameEventMgr.AddHandler(CurrentRegion,RegionEvent.PlayerEnter,new DOLEventHandler(SendKeepInit));
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
					m_claimTimer.Change(CLAIM_CALLBACK_INTERVAL, CLAIM_CALLBACK_INTERVAL);
				}
			}
		}

		/// <summary>
		/// save keep in DB
		/// </summary>
		public void SaveIntoDatabase()
		{
			if (m_guild != null)
				m_dbkeep.ClaimedGuildName = m_guild.Name;
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
			eRealm realm = (eRealm)this.DBKeep.Realm;
			DataObject[] objs = GameServer.Database.SelectObjects(typeof(DBKeepObject),"KeepID = " + this.KeepID + " AND Realm = " + (int)realm + " AND KeepType = " + (int)KeepType);
			CurrentRegion = WorldMgr.GetRegion((ushort)this.DBKeep.Region);
			
			GameObject gameObject = null;
			foreach(DBKeepObject dbkeepObject in objs)
			{
				if (dbkeepObject.ClassType != null && dbkeepObject.ClassType != "")
				{
					Type keepObjType = null;
					foreach (Assembly asm in ScriptMgr.Scripts)
					{
						keepObjType = asm.GetType(dbkeepObject.ClassType);
						if (keepObjType != null)
							break;
					}
					if(keepObjType==null)
						keepObjType = Assembly.GetAssembly(typeof(GameServer)).GetType(dbkeepObject.ClassType);
					if(keepObjType==null)
					{
						if (log.IsErrorEnabled)
							log.Error("Could not find keepobject: "+dbkeepObject.ClassType+"!!!");
						continue;
					}
					try
					{
						gameObject = (GameObject) Activator.CreateInstance(keepObjType,new object[] {this});
					}
					catch
					{
						try
						{
							gameObject = (GameObject) Activator.CreateInstance(keepObjType);//with no param else
						}
						catch
						{

						}
					}

					if (gameObject == null)
					{
						if (log.IsWarnEnabled)
							log.Warn("DBKeepObject have a wrong class type which must inherite from GameObject");
						continue;
					}
				}
				else
				{
					if (log.IsWarnEnabled)
						log.Warn("DBKeepObject have a wrong class type which must inherite from GameObject");
					continue;
				}

				if (gameObject != null)
				{
					try
					{
						gameObject.LoadFromDatabase(dbkeepObject);
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.Error("Failed: " + gameObject.GetType().FullName + ":LoadFromDatabase(DBKeepObject);", e);
						throw e;
					}
					gameObject.AddToWorld();
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
			if(player.Guild == null)
			{
				return;
			}
			if((int)player.Realm != this.Realm)
			{
				return;
			}
			player.Out.SendCustomDialog("Do you want to claim " + this.Name, new CustomDialogResponse(ClaimDialogResponse));
			
		}

		//TODO : check if keep is underattack if yes keep can not been claim

		/// <summary>
		/// this function is called when the player hit yes or no on dialogue which ask for claim
		/// </summary>
		/// <param name="player"></param>
		/// <param name="response"></param>
		private void ClaimDialogResponse(GamePlayer player, byte response)
		{
			if (response == 0x00)return;

			this.m_guild = player.Guild;
			player.Guild.ClaimedKeep = this;
			Lord.Say(Lord.Name + " has accepted your request to claim the outpost.");
			foreach(GameClient client in WorldMgr.GetClientsOfRealm(player.Realm))
			{
				client.Out.SendMessage(player.GuildName+" has taken control of "+this.DBKeep.Name+"!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			foreach( GameKeepGuard guard in Guards)
			{
				guard.ChangeGuild(player.Guild);
			}
			foreach(GameKeepBanner banner in Banners)
			{
				banner.ChangeGuild(player.Guild);
			}
			foreach(GamePlayer currentPlayer in player.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				currentPlayer.Out.SendKeepClaim(this);
			}
			GameEventMgr.Notify(KeepEvent.KeepClaimed, this, new KeepEventArgs(player));
			m_claimTimer.Change(1, CLAIM_CALLBACK_INTERVAL);
			this.SaveIntoDatabase();
		}

		/// <summary>
		/// take cost of claim to guild
		/// </summary>
		/// <param name="state"></param>
		public void ClaimCallBack(object state)
		{
			if (this.Guild.BountyPoints < 50*this.Level)
			{
				this.Release();
				return;
			}
			int amount = CalculRP();
			this.Guild.GainRealmPoints(amount);

			int cost = ClaimBountyPointCost[this.Level-1];
			this.Guild.GainBountyPoints(-cost);
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
			this.Guild.SendMessageToGuildMembers("You loose the claim of the keep :" + this.Name,eChatType.CT_Guild,eChatLoc.CL_SystemWindow);
			this.m_guild = null;
			m_claimTimer.Change(Timeout.Infinite, Timeout.Infinite);
			this.Level = 1;
			this.SaveIntoDatabase();
		}
		#endregion

		#region upgrade

		/// <summary>
		/// upgrade keep to a target level
		/// </summary>
		/// <param name="targetLevel">the target level</param>
		public void Upgrade(int targetLevel)
		{
			TargetLevel = targetLevel;
			this.Level++ ;
			m_upgradeTimer.Change(CalculateTimeToUpgrade(), Timeout.Infinite);
			//todo : refresh guard
			this.Guild.SendMessageToGuildMembers(this.Name + " is being upgraded to level " + targetLevel,eChatType.CT_Guild,eChatLoc.CL_SystemWindow);
			this.SaveIntoDatabase();
		}
		
		/// <summary>
		/// call back called to increase the level by one
		/// </summary>
		/// <param name="state"></param>
		public void UpgradeTimerCallback(object state)
		{
			if ((TargetLevel < 1) || (TargetLevel > 10))
			{
				return ;
			}

			this.Level++;
			this.SaveIntoDatabase();
			//refresh guard level

			foreach(GameClient client in WorldMgr.GetAllPlayingClients())
			{
				client.Out.SendMessage("The keep "+this.Name+" is upgraded to level "+this.Level+"!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}

			foreach(GameKeepComponent comp in this.KeepComponents)
			{
				comp.Update();
			}
			foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegion.ID))
			{
				client.Out.SendKeepComponentUpdate(this,true);
			}
			if(this.Level >= TargetLevel)
			{
				return ;
			}
			else
			{
				m_upgradeTimer.Change(this.CalculateTimeToUpgrade(), Timeout.Infinite);
				return ;
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
		/// delete all guard of keep
		/// </summary>
		public void DeleteAllGuard()
		{
			foreach( GameKeepGuard guard in Guards)
			{
				guard.Delete();
			}
			Lord.Delete();
		}

		/// <summary>
		/// delete all banner of keep
		/// </summary>
		public void DeleteAllBanner()
		{
			foreach( GameStaticItem banner in Banners)
			{
				banner.Delete();
			}
		}

		/// <summary>
		/// reset the realm when the lord have been killed
		/// </summary>
		/// <param name="realm"></param>
		public void Reset(eRealm realm)
		{
			DBKeep.Realm = (int)realm;
			this.Level = 1;
			this.KeepType = eKeepType.Melee;
			if (this.Guild != null)
			{
				this.Release();
			}
			m_claimTimer.Change(Timeout.Infinite, Timeout.Infinite);
			DeleteAllGuard();
			DeleteAllBanner();
			LoadObjects();
			foreach(GameKeepDoor door in Doors)
			{
				door.Reset(realm);
			}
			foreach (GameClient client in WorldMgr.GetClientsOfRegion(this.CurrentRegion.ID))
			{
				client.Player.Out.SendKeepComponentUpdate(this,false);
			}
			this.SaveIntoDatabase();
		}
		
		/// <summary>
		/// function called when lord is killed
		/// to warn that the loard have been taken
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="args"></param>
		public void LordKilled(DOLEvent e, object sender, EventArgs args)
		{
			DyingEventArgs dyingarg = args as DyingEventArgs;


			string realm = GlobalConstants.RealmToName((eRealm)dyingarg.Killer.Realm);
			foreach (GameClient cl in WorldMgr.GetAllPlayingClients())
			{
				for (int i = 0; i < 3; i++)
				{
					cl.Player.Out.SendMessage("The Forces of " + realm + " have captured " + this.Name, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				}
			}
			GameEventMgr.RemoveHandler(Lord,GameNPCEvent.Dying,new DOLEventHandler(LordKilled));
			GameEventMgr.Notify(KeepEvent.KeepTaken,this,new KeepEventArgs(dyingarg.Killer as GamePlayer));

			Reset((eRealm)dyingarg.Killer.Realm);
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