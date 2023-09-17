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

//Instance devised by Dinberg 
//     - there will probably be questions, direct them to dinberg_darktouch@hotmail.co.uk ;)
using System;
using System.Text;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using log4net;

using DOL.GS;
using DOL.Database;
using DOL.GS.Geometry;

namespace DOL.GS
{
    /// <summary>
    /// The instance object is a dynamic region that is designed to be adventured by a select few.
    /// Instances can share the same skins as other regions, so they look the same, but in reality
    /// are an entirely unique region, often with different mobs. Two players, standing in the same
    /// spot, on top of the merchant's tent in Jordheim, if in two instances, will not be able to
    /// interact in any way.
    ///</summary>
    public class BaseInstance : Region
    {
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
        /// Creates an instance object. This shouldn't be used directly - Please use WorldMgr.CreateInstance
        /// to create an instance.
        /// </summary>
        public BaseInstance(ushort ID, GameTimer.TimeManager time, RegionData data) :base(time, data)
        {
            m_regionID = ID;
            m_skinID = data.Id;
            
            //Notify we've created an instance.
            log.Warn("An instance is created! " + Name + ", RegionID: " + ID + ", SkinID: " + Skin);
        }

		/// <summary>
		/// Called as last step in Instance creation.
		/// </summary>
		public virtual void Start()
		{
			StartRegionMgr();
			BeginAutoClosureCountdown(10);
			
			foreach (Zone z in m_zones)
			{
				m_zoneSkinMap.Add(z.ZoneSkinID, z);
			}
		}

        #region Inheritance and Region

        private ushort m_regionID;

        /// <summary>
        /// The unique region ID of this region.
        /// </summary>
        public override ushort ID
        { get { return m_regionID; } }


        public override string Description
        {
            get
            {
                return base.Description + " (Instance)";
            }
        }

		private ushort m_skinID;
		/// <summary>
        /// Gets the SkinID of the instance - the 'look' of the instance.
        /// </summary>
        public override ushort Skin
        { get { return m_skinID; } }

        /// <summary>
        /// Gets the name of the region this instance copies, + " (Instance)"
        /// </summary>
        public override string Name
        { get { return base.Name + " (Instance)"; } }

        public override bool IsInstance
        { get { return true; } }


		private bool m_destroyWhenEmpty = true;
		private bool m_persistent = false;

		/// <summary>
		/// If this is true the instance will be destroyed as soon as the last player leaves.
		/// </summary>
		public bool DestroyWhenEmpty
		{
			get { return m_destroyWhenEmpty; }
			set 
			{ 
				m_destroyWhenEmpty = value;

				// Instance will be destroyed as soon as all players leave
				if (m_destroyWhenEmpty)
				{
					if (m_autoCloseRegionTimer != null)
					{
						m_autoCloseRegionTimer.Stop();
						m_autoCloseRegionTimer = null;
					}

					if (m_delayCloseRegionTimer != null)
					{
						m_delayCloseRegionTimer.Stop();
						m_delayCloseRegionTimer = null;
					}
				}

				//If no more players remain, remove and clean up the instance...
				if (m_destroyWhenEmpty && m_playersInInstance == 0)
				{
					log.Info("Instance is empty, destroying instance " + Description + ", ID: " + ID + ".");
					WorldMgr.RemoveInstance(this);
				}
			}
		}

		/// <summary>
		/// Persistent instances never close
		/// </summary>
		public bool Persistent
		{
			get { return m_persistent; }

			set
			{
				m_persistent = value;

				// This instance is persistent, stop all close timers
				if (m_persistent)
				{
					DestroyWhenEmpty = false;

					if (m_autoCloseRegionTimer != null)
					{
						m_autoCloseRegionTimer.Stop();
						m_autoCloseRegionTimer = null;
					}

					if (m_delayCloseRegionTimer != null)
					{
						m_delayCloseRegionTimer.Stop();
						m_delayCloseRegionTimer = null;
					}
				}
				else
				{
					DestroyWhenEmpty = true;
				}
			}
		}

        #endregion

        #region Instance specific

        //I know there are commands I could use already for these, but I want to make a clear distinction
        //between instance and region. These two commands are called when AddToWorld() is invoked on a player,
        //and RemoveFromWorld(), and that players current zone is an Instance.

        private int m_playersInInstance;

		protected int PlayersInInstance
		{
			get { return m_playersInInstance; }
		}

 		/// <summary>
		/// Event handler for player entering instance
		/// </summary>
        public virtual void OnPlayerEnterInstance(GamePlayer player)
        { 
        //Increment the amount of players.
            m_playersInInstance++;

            //Stop the timer to prevent the region's removal.
			if (m_autoCloseRegionTimer != null)
			{
				m_autoCloseRegionTimer.Stop();
				m_autoCloseRegionTimer = null;
			}
        }

 		/// <summary>
		/// Event handler for player leaving instance
		/// </summary>
        public virtual void OnPlayerLeaveInstance(GamePlayer player)
        {
            //Decrease the amount of players
            m_playersInInstance--;

            //If no more players remain, remove and clean up the instance...
            if (m_playersInInstance < 1 && DestroyWhenEmpty)
            {
                log.Warn("Instance now empty, destroying instance " + Description + ", ID: " + ID + ", type=" + GetType().ToString() + ".");
                WorldMgr.RemoveInstance(this);
            }
        }

        //Below we have a void I've added. By default, it will do nothing, but its intention is for inherited
        //classes.

        //This concerns instances that have a door or other zone-method within them. If you recall, the players
        //regionID.Skin is the region the player thinks its in. As such, it will always call the zone point
        //of the same region, regardless of the instance the client is in.

        //This gives us a problem here: Lets say TaskDungeon1 wants to zone out to mularn, wherethe player entered.
        //Thats all dandy, and the database is set up that this is the case - as it most often will be. BUT,
        //another quest, 'Find Dinberg's Hat', uses the same regionID and wants the door not to even zone back to
        //mularn, but actually wants the door to go to a castle, in another instanced region.

        //The below method can be thus overriden to return false when the default method should NOT be used.
        //moving of the player can be handled inside this void.

        /// <summary>
        /// Invoked when the player attempts to use a door or zone point inside an instance.
        /// </summary>
        /// <returns>True if the player should be moved to default locations.</returns>
        public virtual bool OnInstanceDoor(GamePlayer player, ZonePoint zonePoint)
        {
            //zone point ID is also used for larger instances, eg Jordheim, with multiple exits.
            return true;
        }

        /// <summary>
        /// What to do when the region collapses.
        /// Examples of use: Expire task on task dungeons.
        /// </summary>
        public override void OnCollapse()
        {
			base.OnCollapse();

			if (m_autoCloseRegionTimer != null)
			{
				m_autoCloseRegionTimer.Stop();
				m_autoCloseRegionTimer = null;
			}

			if (m_delayCloseRegionTimer != null)
			{
				m_delayCloseRegionTimer.Stop();
				m_delayCloseRegionTimer = null;
			}

			DOL.Events.GameEventMgr.RemoveAllHandlersForObject(this);
			
			m_zoneSkinMap.Clear();

			Areas.Clear();
		}

		~BaseInstance()
		{
			log.Debug("BaseInstance destructor called for " + Description);
		}

        private AutoCloseRegionTimer m_autoCloseRegionTimer;
		private DelayCloseRegionTimer m_delayCloseRegionTimer;

		/// <summary>
		/// Setting this will ensure the instance stays around x minutes.  After that the region will be destroyed when empty
		/// </summary>
		/// <param name="minutes"></param>
        public void BeginAutoClosureCountdown(int minutes)
        {
			if (m_autoCloseRegionTimer != null)
			{
				m_autoCloseRegionTimer.Stop();
				m_autoCloseRegionTimer = null;
			}

            m_autoCloseRegionTimer = new AutoCloseRegionTimer(TimeManager, this);
            m_autoCloseRegionTimer.Interval = minutes * 60000;
            m_autoCloseRegionTimer.Start(minutes * 60000);
        }

		/// <summary>
		/// Setting this will ensure the instance stays around x minutes.  After that the region will be destroyed when empty
		/// </summary>
		/// <param name="minutes"></param>
		public void BeginDelayCloseCountdown(int minutes)
		{
			DestroyWhenEmpty = false;

			if (m_autoCloseRegionTimer != null)
			{
				m_autoCloseRegionTimer.Stop();
				m_autoCloseRegionTimer = null;
			}

			if (m_delayCloseRegionTimer != null)
			{
				m_delayCloseRegionTimer.Stop();
				m_delayCloseRegionTimer = null;
			}

			m_delayCloseRegionTimer = new DelayCloseRegionTimer(TimeManager, this);
			m_delayCloseRegionTimer.Interval = minutes * 60000;
			m_delayCloseRegionTimer.Start(minutes * 60000);
		}

		/// <summary>
		/// Automated Closing Timer for Instances
		/// </summary>
		protected class AutoCloseRegionTimer : GameTimer
        {
            public AutoCloseRegionTimer(TimeManager time, BaseInstance i)
                : base(time)
            {
                m_instance = i;
            }

            //The instance to remove...
            BaseInstance m_instance;

            //When the timer ticks, it means there are no players in the region.
            //This, we remove the instance.
            protected override void OnTick()
            {
                if (m_instance == null)
                {
                    log.Warn("RegionRemovalTimer is not being stopped once the instance is destroyed!");
                    Stop();
                    return;
                }
                //If there are players, someone has callously forgotten to include
                //a base in one of their OnPlayerEnter/Exit overrides.
                //When this is a case, keep the timer ticking - we will eventually have it cleanup the instance,
                //it just wont be runnign at optimum speed.

                if (WorldMgr.GetClientsOfRegion(m_instance.ID).Count > 0)
                    log.Warn("Players were still in the region on AutoRemoveregionTimer Tick! Please check the overridden voids OnPlayerEnter/Exit to ensure that a 'base.OnPlayerEnter/Exit' is included!");
                else
                {
                    //Collapse the zone!
                    //Thats my favourite bit ;)
                    log.Info(m_instance.Name + " (ID: " + m_instance.ID + ") just reached the timeout for the removal timer. The region is empty, and will now be demolished and removed from the world. Entering OnCollapse!");
                    Stop();
                    WorldMgr.RemoveInstance(m_instance);
                }                
            }

        }

		/// <summary>
		/// Delay Closing Timer for Instances
		/// </summary>
		protected class DelayCloseRegionTimer : GameTimer
		{
			public DelayCloseRegionTimer(TimeManager time, BaseInstance i)
				: base(time)
			{
				m_instance = i;
			}

			//The instance to remove...
			BaseInstance m_instance;

			protected override void OnTick()
			{
				if (m_instance == null)
				{
					log.Warn("DelayCloseRegionTimer is not being stopped once the instance is destroyed!");
					Stop();
					return;
				}

				Stop();
				m_instance.DestroyWhenEmpty = true;
			}

		}

        #endregion

        
		#region Area

		/// <summary>
		/// Zone Mapping for Instances
		/// Update Leodagan : moved from Instance to BaseInstance to make Areas work !
		/// </summary>
		protected Dictionary<int, Zone> m_zoneSkinMap = new Dictionary<int, Zone>();

		/// <summary>
		/// Gets the areas for a certain spot
		/// </summary>
		/// <returns></returns>
		public override IList<IArea> GetAreasOfZone(Zone zone, Coordinate loc, bool checkZ)
		{
			Zone checkZone = zone;
			var areas = new List<IArea>();

			if (checkZone == null)
			{
				return areas;
			}

			// Players will always request the skinned zone so map it to the actual instance zone
			if (m_zoneSkinMap.ContainsKey(zone.ID))
			{
				checkZone = m_zoneSkinMap[zone.ID];
			}

			int zoneIndex = Zones.IndexOf(checkZone);

			if (zoneIndex >= 0)
			{
				lock (m_lockAreas)
				{
					try
					{
						for (int i = 0; i < m_ZoneAreasCount[zoneIndex]; i++)
						{
							IArea area = (IArea)Areas[m_ZoneAreas[zoneIndex][i]];
							if (area.IsContaining(loc, ignoreZ: !checkZ))
							{
								areas.Add(area);
							}
						}
					}
					catch (Exception e)
					{
						log.Error("GetAreaOfZone: Caught exception for Zone " + zone.Description + ", Area count " + m_ZoneAreasCount[zoneIndex] + ".", e);
					}
				}
			}

			return areas;
		}

		/// <summary>
		/// Gets the areas for a certain spot
		/// </summary>
		/// <param name="zone"></param>
		/// <param name="p"></param>
		/// <param name="checkZ"></param>
		/// <returns></returns>
		public override IList<IArea> GetAreasOfZone(Zone zone, Coordinate loc)
		{
			Zone checkZone = zone;
			var areas = new List<IArea>();

			if (checkZone == null)
			{
				return areas;
			}

			// Players will always request the skinned zone so map it to the actual instance zone
			if (m_zoneSkinMap.ContainsKey(zone.ID))
			{
				checkZone = m_zoneSkinMap[zone.ID];
			}

			int zoneIndex = Zones.IndexOf(checkZone);

			if (zoneIndex >= 0)
			{
				lock (m_lockAreas)
				{
					try
					{
						for (int i = 0; i < m_ZoneAreasCount[zoneIndex]; i++)
						{
							IArea area = (IArea)Areas[m_ZoneAreas[zoneIndex][i]];
							if (area.IsContaining(loc))
								areas.Add(area);
						}
					}
					catch (Exception e)
					{
						log.Error("GetArea exception.Area count " + m_ZoneAreasCount[zoneIndex], e);
					}
				}
			}

			return areas;
		}

		#endregion

		#region mobcount

		/// <summary>
		/// Get an Enumerable of Mobs inside instance, Meant for mini-quest finished conditions.
		/// Can be used to update all mobs in area depending on player levels or other conditions.
		/// </summary>
		/// <param name="alive">Return Alive mobs or all mobs</param>
		/// <returns>List of Mobs</returns>
		public IEnumerable<GameNPC> GetMobsInsideInstance(bool alive)
		{
			lock(ObjectsSyncLock)
			{
				if(alive)
				{
					return new List<GameNPC>(from regionObjects in this.Objects where (regionObjects is GameNPC) && (!((GameNPC)regionObjects).IsPeaceful) && ((GameNPC)regionObjects).IsAlive select (GameNPC)regionObjects);
				}
				else
				{
					return new List<GameNPC>(from regionObjects in this.Objects where (regionObjects is GameNPC) && (!((GameNPC)regionObjects).IsPeaceful) select (GameNPC)regionObjects);
				}
			}
		}
		
		#endregion
    }
}
