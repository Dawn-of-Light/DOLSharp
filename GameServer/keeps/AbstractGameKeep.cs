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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using NHibernate.Expression;
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

        private int m_albionDifficultyLevel;
        private int m_midgardDifficultyLevel;
        private int m_hiberniaDifficultyLevel;
	    
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
		/// The keep's position.
		/// </summary>
		private Point m_position;

		/// <summary>
		/// Gets or sets the keep's postion.
		/// </summary>
		public Point Position
		{
			get { return m_position; }
			set { m_position = value; }
		}

		/// <summary>
		/// region of the keep
		/// </summary>
		private Region m_region;

		/// <summary>
		/// region of the keep
		/// </summary>
		public Region Region
		{
			get	{ return m_region; }
			set	{ m_region = value; }
		}

		/// <summary>
		/// zone of the keep
		/// </summary>
		public Zone CurrentZone
		{
			get 
			{
				if (m_region != null) 
				{
					return m_region.GetZone(Position);
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
            set	{ m_guild = value; }
		}

        /// <summary>
        /// Albion difficulty level
        /// </summary>
        public int AlbionDifficultyLevel
        {
            get
            {
                return m_albionDifficultyLevel;
            }
            set
            {
                m_albionDifficultyLevel = value;
            }
        }

        /// <summary>
        /// Midgard difficulty level
        /// </summary>
        public int MidgardDifficultyLevel
        {
            get
            {
                return m_midgardDifficultyLevel;
            }
            set
            {
                m_midgardDifficultyLevel = value;
            }
        }

        /// <summary>
        /// Hibernia difficulty level
        /// </summary>
        public int HiberniaDifficultyLevel
        {
            get
            {
                return m_hiberniaDifficultyLevel;
            }
            set
            {
                m_hiberniaDifficultyLevel = value;
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
        private int m_keepID;
		public virtual int KeepID
		{
			get	{ return m_keepID; }
            set { m_keepID = value; }
		}
        private int m_level;
		public int Level
		{
			get	{ return m_level; }
            set { m_level = value; }
		}
        private string m_name;
		public string Name
		{
			get	{ return m_name; }
            set { m_name = value; }
		}
        private int m_heading;
		public int Heading
		{
			get	{ return m_heading; }
            set { m_heading = value; }
		}	
        private int m_realm;
		public int Realm
		{
			get	{ return m_realm; }
			set	{ m_realm = value; }
		}
        private eRealm m_originalRealm;
		public eRealm OriginalRealm
		{
            get { return m_originalRealm; }
            set { m_originalRealm = value; }
		}

	    private eKeepType m_keepType;
		public eKeepType KeepType
		{
			get	{ return m_keepType; }
			set	
			{
                m_keepType = value; 
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
		public void Load()
		{
            if (this.Guild != null)
            {
                m_claimTimer.Change(CLAIM_CALLBACK_INTERVAL, CLAIM_CALLBACK_INTERVAL);
            }
			LoadObjects();
			GameEventMgr.AddHandler(Region,RegionEvent.PlayerEnter,new DOLEventHandler(SendKeepInit));
		}


		/// <summary>
		/// save keep in DB
		/// </summary>
		public void SaveIntoDatabase()
		{
			GameServer.Database.SaveObject(this);
			foreach (GameKeepComponent comp in this.KeepComponents)
				comp.SaveIntoDatabase();
		}

		/// <summary>
		/// load the object of keep which must inherit of GameObject
		/// This can be banner, guard, static item like catapult,...
		/// </summary>
		public void LoadObjects()
		{			
			eRealm realm = (eRealm)this.Realm;
			IList objs = GameServer.Database.SelectObjects(typeof(DBKeepObject), Expression.And(Expression.And(Expression.Eq("KeepID",KeepID),Expression.Eq("Realm",(int)realm)),Expression.Eq("KeepType",(int)KeepType)));
			//Region = WorldMgr.GetRegion(this.Regionid);
			
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
			if (!player.Guild.CheckGuildPermission(player, eGuildPerm.Claim))
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
				client.Out.SendMessage(player.GuildName+" has taken control of "+this.Name+"!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
			}
			foreach( GameKeepGuard guard in Guards)
			{
				guard.ChangeGuild(player.Guild);
			}
			foreach(GameKeepBanner banner in Banners)
			{
				banner.ChangeGuild(player.Guild);
			}
			foreach(GameClient client in player.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
			{
				client.Out.SendKeepClaim(this);
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
			this.Guild.RealmPoints += amount;

			int cost = ClaimBountyPointCost[this.Level-1];
			this.Guild.BountyPoints -= cost;
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
			foreach (GameClient client in WorldMgr.GetClientsOfRegion((ushort)Region.RegionID))
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
			this.Realm = (int)realm;
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
			foreach (GameClient client in WorldMgr.GetClientsOfRegion((ushort)Region.RegionID))
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

	    public int DifficultyLevel()
	    {
	        switch ((eRealm)Realm)
	        {
	            case eRealm.Albion :
	                return AlbionDifficultyLevel;
	            case eRealm.Midgard :
	                return MidgardDifficultyLevel;
	            case eRealm.Hibernia :
	                return HiberniaDifficultyLevel;
                default :
	                    return 1;
	        }
	    }

        public void AddKeepComponents(GameKeepComponent comp)
        {
            this.KeepComponents.Add(comp);
            comp.Keep = this;
        }
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