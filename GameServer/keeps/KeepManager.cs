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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// The default KeepManager
	/// The manager that keeps track of the keeps and stuff.. in the future.
	/// Right now it just has some utilities.
	/// </summary>
	public class DefaultKeepManager : IKeepManager
	{
		/// <summary>
		/// list of all keeps
		/// </summary>
		protected Hashtable m_keepList = new Hashtable();

		public virtual Hashtable Keeps
		{
			get { return m_keepList; }
		}

		protected List<Battleground> m_battlegrounds = new List<Battleground>();

		public const int DEFAULT_FRONTIERS_REGION = 163; // New Frontiers

		public List<uint> m_frontierRegionsList = new List<uint>();

		public virtual List<uint> FrontierRegionsList
		{
			get { return m_frontierRegionsList; }
		}

		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public ILog Log
		{
			get { return log; }
		}

		/// <summary>
		/// load all keeps from the DB
		/// </summary>
		/// <returns></returns>
		public virtual bool Load()
		{
			// first check the regions we manage
			foreach (Region r in WorldMgr.Regions.Values)
			{
				if (r.IsFrontier)
				{
					m_frontierRegionsList.Add(r.ID);
				}
			}

			// default to NF if no frontier regions found
			if (m_frontierRegionsList.Count == 0)
			{
				m_frontierRegionsList.Add(DEFAULT_FRONTIERS_REGION);
			}

			ClothingMgr.LoadTemplates();

            //Dinberg - moved this here, battlegrounds must be loaded before keepcomponents are.
            LoadBattlegroundCaps();

			if (!ServerProperties.Properties.LOAD_KEEPS)
				return true;

			lock (m_keepList.SyncRoot)
			{
				m_keepList.Clear();

				var keeps = GameServer.Database.SelectAllObjects<DBKeep>();
				foreach (DBKeep datakeep in keeps)
				{
					Region keepRegion = WorldMgr.GetRegion(datakeep.Region);
					if (keepRegion == null)
						continue;

                    var currentKeepComponents = GameServer.Database.SelectObjects<DBKeepComponent>("`KeepID` = '" + datakeep.KeepID + "'");
				
					AbstractGameKeep keep;
					if ((datakeep.KeepID >> 8) != 0 || ((datakeep.KeepID & 0xFF) > 150))
					{
						keep = keepRegion.CreateGameKeepTower();
					}
					else
					{
						keep = keepRegion.CreateGameKeep();
					}

					keep.Load(datakeep);
                    RegisterKeep(datakeep.KeepID, keep);
				}

				// This adds owner keeps to towers / portal keeps
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					GameKeepTower tower = keep as GameKeepTower;
					if (tower != null)
					{
						int index = tower.KeepID & 0xFF;
						GameKeep ownerKeep = GetKeepByID(index) as GameKeep;
						if (ownerKeep != null)
						{
							ownerKeep.AddTower(tower);
						}
						tower.Keep = ownerKeep;
						tower.OwnerKeepID = index;

						if (tower.OwnerKeepID < 10)
						{
							log.WarnFormat("Tower.OwnerKeepID < 10 for KeepID {0}. Doors on this tower will not be targetable! ({0} & 0xFF < 10). Choose a different KeepID to correct this issue.", tower.KeepID);
						}
					}
				}
                if (ServerProperties.Properties.USE_NEW_KEEPS == 2) log.ErrorFormat("ServerProperty USE_NEW_KEEPS is actually set to 2 but it is no longer used. Loading as if he were 0 but please set to 0 or 1 !");
				    
				var keepcomponents = default(IList<DBKeepComponent>);

                if (ServerProperties.Properties.USE_NEW_KEEPS == 0 || ServerProperties.Properties.USE_NEW_KEEPS == 2) keepcomponents = GameServer.Database.SelectObjects<DBKeepComponent>("Skin < 20");
                else if (ServerProperties.Properties.USE_NEW_KEEPS == 1) keepcomponents = GameServer.Database.SelectObjects<DBKeepComponent>("Skin > 20");

				foreach (DBKeepComponent component in keepcomponents)
				{
					AbstractGameKeep keep = GetKeepByID(component.KeepID);
					if (keep == null)
					{
						//missingKeeps = true;
						continue;
					} 

					GameKeepComponent gamecomponent = keep.CurrentRegion.CreateGameKeepComponent();
					gamecomponent.LoadFromDatabase(component, keep);
					keep.KeepComponents.Add(gamecomponent);
				}

				/*if (missingKeeps && log.IsWarnEnabled)
				{
					log.WarnFormat("Some keeps not found while loading components, possibly old/new keeptypes.");
				}*/

				if (m_keepList.Count != 0)
				{
					foreach (AbstractGameKeep keep in m_keepList.Values)
					{
						if (keep.KeepComponents.Count != 0)
							keep.KeepComponents.Sort();
					}
				}
				LoadHookPoints();

				log.Info("Loaded " + m_keepList.Count + " keeps successfully");
			}

			if (ServerProperties.Properties.USE_KEEP_BALANCING)
				UpdateBaseLevels();

			if (ServerProperties.Properties.USE_LIVE_KEEP_BONUSES)
				KeepBonusMgr.UpdateCounts();

			return true;
		}


		public virtual bool IsNewKeepComponent(int skin)
		{
			if (skin > 20) 
				return true;

			return false;
		}


		protected virtual void LoadHookPoints()
		{
			if (!ServerProperties.Properties.LOAD_KEEPS || !ServerProperties.Properties.LOAD_HOOKPOINTS)
				return;

			Dictionary<string, List<DBKeepHookPoint>> hookPointList = new Dictionary<string, List<DBKeepHookPoint>>();

			var dbkeepHookPoints = GameServer.Database.SelectAllObjects<DBKeepHookPoint>();
			foreach (DBKeepHookPoint dbhookPoint in dbkeepHookPoints)
			{
				List<DBKeepHookPoint> currentArray;
				string key = dbhookPoint.KeepComponentSkinID + "H:" + dbhookPoint.Height;
				if (!hookPointList.ContainsKey(key))
					hookPointList.Add(key, currentArray = new List<DBKeepHookPoint>());
				else
					currentArray = hookPointList[key];
				currentArray.Add(dbhookPoint);
			}
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					string key = component.Skin + "H:" + component.Height;
					if ((hookPointList.ContainsKey(key)))
					{
						List<DBKeepHookPoint> HPlist = hookPointList[key];
						if ((HPlist != null) && (HPlist.Count != 0))
						{
							foreach (DBKeepHookPoint dbhookPoint in hookPointList[key])
							{
								GameKeepHookPoint myhookPoint = new GameKeepHookPoint(dbhookPoint, component);
								component.KeepHookPoints.Add(dbhookPoint.HookPointID, myhookPoint);
							}
							continue;
						}
					}
					//add this to keep hookpoint system until DB is not full
					for (int i = 0; i < 38; i++)
						component.KeepHookPoints.Add(i, new GameKeepHookPoint(i, component));

					component.KeepHookPoints.Add(65, new GameKeepHookPoint(0x41, component));
					component.KeepHookPoints.Add(97, new GameKeepHookPoint(0x61, component));
					component.KeepHookPoints.Add(129, new GameKeepHookPoint(0x81, component));
				}
			}

			log.Info("Loading HookPoint items");

			//fill existing hookpoints with objects
			IList<DBKeepHookPointItem> items = GameServer.Database.SelectAllObjects<DBKeepHookPointItem>();
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					foreach (GameKeepHookPoint hp in component.KeepHookPoints.Values)
					{
						var item = items.FirstOrDefault(
							it => it.KeepID == component.AbstractKeep.KeepID && it.ComponentID == component.ID && it.HookPointID == hp.ID);
						if (item != null)
							HookPointItem.Invoke(component.KeepHookPoints[hp.ID] as GameKeepHookPoint, item.ClassType);
					}
				}
			}
		}

        public virtual void RegisterKeep(int keepID, AbstractGameKeep keep)
        {
            m_keepList.Add(keepID, keep);
            log.Info("Registered Keep: " + keep.Name);
        }

        /// <summary>
		/// get keep by ID
		/// </summary>
		/// <param name="id">id of keep</param>
		/// <returns> Game keep object with keepid = id</returns>
		public virtual AbstractGameKeep GetKeepByID(int id)
		{
			return m_keepList[id] as AbstractGameKeep;
		}

		/// <summary>
		/// get list of keep close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="point3d"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public virtual IEnumerable GetKeepsCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
		{
			return GetKeepsCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
		}

		/// <summary>
		/// get the keep with minimum distance close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="point3d"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public virtual AbstractGameKeep GetKeepCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
		{
			return GetKeepCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
		}

		/// <summary>
		/// Gets all keeps by a realm map /rw
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
		public virtual ICollection<IGameKeep> GetKeepsByRealmMap(int map)
		{
			List<IGameKeep> myKeeps = new List<IGameKeep>();
			SortedList keepsByID = new SortedList();
			foreach (IGameKeep keep in m_keepList.Values)
			{
				if (m_frontierRegionsList.Contains(keep.CurrentRegion.ID) == false)
					continue;
				
				if (((keep.KeepID & 0xFF) / 25 - 1) == map)
				{
					keepsByID.Add(keep.KeepID, keep);
				}
				else if (((keep.KeepID & 0xFF) > 150) && ((keep.KeepID & 0xFF) / 25 - 2) == map)
				{
					keepsByID.Add(keep.KeepID, keep);
				}
			}
			
			foreach (IGameKeep keep in keepsByID.Values)
				myKeeps.Add(keep);
			
			return myKeeps;
		}

		/// <summary>
		/// Get the battleground portal keep for a player
		/// </summary>
		/// <param name="player">The player</param>
		/// <returns>The battleground portal keep as AbstractGameKeep or null</returns>
		public virtual AbstractGameKeep GetBGPK(GamePlayer player)
		{
			//the temporary keep variable for use in this method
			AbstractGameKeep tempKeep = null;

			//iterate through keeps and find all those which we aren't capped out for
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				// find keeps in the battlegrounds that arent portal keeps
				if (m_frontierRegionsList.Contains(keep.Region) == false && keep.IsPortalKeep == false) continue;
				Battleground bg = GetBattleground(keep.Region);
				if (bg == null) continue;
				if (player.Level >= bg.MinLevel &&
					player.Level <= bg.MaxLevel &&
					(bg.MaxRealmLevel == 0 || player.RealmLevel < bg.MaxRealmLevel))
					tempKeep = keep;
			}

			//if we haven't found a CK, we're not going to find a PK
			if (tempKeep == null)
				return null;

			//we now use the central keep we found, to find the portal keeps
			foreach (AbstractGameKeep keep in GetKeepsOfRegion((ushort)tempKeep.Region))
			{
				//match the region keeps to a portal keep, and realm
				if (keep.IsPortalKeep && keep.Realm == player.Realm)
					return keep;
			}

			return null;
		}

		public virtual ICollection<AbstractGameKeep> GetFrontierKeeps()
		{
			List<AbstractGameKeep> keepList = new List<AbstractGameKeep>();

			foreach (ushort regionID in m_frontierRegionsList)
			{
				keepList.AddRange(GetKeepsOfRegion(regionID));
			}

			return keepList;
		}

		public virtual ICollection<AbstractGameKeep> GetKeepsOfRegion(ushort region)
		{
			List<AbstractGameKeep> regionKeeps = new List<AbstractGameKeep>();
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				if (keep.CurrentRegion.ID != region)
					continue;

				regionKeeps.Add(keep);
			}

			return regionKeeps;
		}

		/// <summary>
		///  get list of keep close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public virtual ICollection<AbstractGameKeep> GetKeepsCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			List<AbstractGameKeep> closeKeeps = new List<AbstractGameKeep>();
			long radiussqrt = radius * radius;

			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (keep.DBKeep == null || keep.CurrentRegion.ID != regionid)
						continue;

					long xdiff = keep.DBKeep.X - x;
					long ydiff = keep.DBKeep.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff;
					if (range < radiussqrt)
					{
						closeKeeps.Add(keep);
					}
				}
			}

			return closeKeeps;
		}

		/// <summary>
		/// get the keep with minimum distance close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="z"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public virtual AbstractGameKeep GetKeepCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			AbstractGameKeep closestKeep = null;

			lock (m_keepList.SyncRoot)
			{
				long radiussqrt = radius * radius;
				long lastKeepDistance = radiussqrt;

				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (keep == null || keep.DBKeep == null || keep.DBKeep.Region != regionid)
						continue;

					long xdiff = keep.DBKeep.X - x;
					long ydiff = keep.DBKeep.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff;

					if (range > radiussqrt)
						continue;

					if (closestKeep == null || range <= lastKeepDistance)
					{
						closestKeep = keep;
						lastKeepDistance = range;
					}
				}
			}

			return closestKeep;
		}

		/// <summary>
		/// get keep count controlled by realm to calculate keep bonus
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public virtual int GetTowerCountByRealm(eRealm realm)
		{
			int index = 0;
			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (m_frontierRegionsList.Contains(keep.Region) == false) continue;
					if (((eRealm)keep.Realm == realm) && (keep is GameKeepTower))
						index++;
				}
			}
			return index;
		}

		/// <summary>
		/// Get the tower count of each realm
		/// </summary>
		/// <returns></returns>
		public virtual Dictionary<eRealm, int> GetTowerCountAllRealm()
		{
			Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm,int>(3);
			realmXTower.Add(eRealm.Albion, 0);
			realmXTower.Add(eRealm.Hibernia, 0);
			realmXTower.Add(eRealm.Midgard, 0);

			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (m_frontierRegionsList.Contains(keep.Region) && keep is GameKeepTower)
					{
						realmXTower[keep.Realm] += 1;
					}
				}
			}

			return realmXTower;
		}

		/// <summary>
		/// Get the tower count of each realm
		/// </summary>
		/// <returns></returns>
		public virtual Dictionary<eRealm, int> GetTowerCountFromZones(List<int> zones)
		{
			Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm, int>(4);
			realmXTower.Add(eRealm.Albion, 0);
			realmXTower.Add(eRealm.Hibernia, 0);
			realmXTower.Add(eRealm.Midgard, 0);
			realmXTower.Add(eRealm.None, 0);

			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (m_frontierRegionsList.Contains(keep.Region) && keep is GameKeepTower && zones.Contains(keep.CurrentZone.ID))
					{
						realmXTower[keep.Realm] += 1;
					}
				}
			}

			return realmXTower;
		}

		/// <summary>
		/// get keep count by realm
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public virtual int GetKeepCountByRealm(eRealm realm)
		{
			int index = 0;
			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (m_frontierRegionsList.Contains(keep.Region) == false) continue;
					if (((eRealm)keep.Realm == realm) && (keep is GameKeep))
						index++;
				}
			}
			return index;
		}

		public virtual ICollection<AbstractGameKeep> GetAllKeeps()
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			foreach (AbstractGameKeep keep in m_keepList.Values)
			{
				myKeeps.Add(keep);
			}
			return myKeeps;
		}

		/// <summary>
		/// Main checking method to see if a player is an enemy of the keep
		/// </summary>
		/// <param name="keep">The keep checking</param>
		/// <param name="target">The target player</param>
		/// <param name="checkGroup">Do we check the players group for a friend</param>
		/// <returns>true if the player is an enemy of the keep</returns>
		public virtual bool IsEnemy(AbstractGameKeep keep, GamePlayer target, bool checkGroup)
		{
			if (target.Client.Account.PrivLevel != 1)
				return false;

			switch (GameServer.Instance.Configuration.ServerType)
			{
				case eGameServerType.GST_Normal:
					{
						return keep.Realm != target.Realm;
					}
				case eGameServerType.GST_PvP:
					{
						if (keep.Guild == null)
							return ServerProperties.Properties.PVP_UNCLAIMED_KEEPS_ENEMY;

						//friendly player in group
						if (checkGroup && target.Group != null)
						{
							foreach (GamePlayer player in target.Group.GetPlayersInTheGroup())
							{
								if (!IsEnemy(keep, target, false))
									return false;
							}
						}

						//guild alliance
						if (keep.Guild != null && keep.Guild.alliance != null)
						{
							if (keep.Guild.alliance.Guilds.Contains(target.Guild))
								return false;
						}

						return keep.Guild != target.Guild;
					}
				case eGameServerType.GST_PvE:
					{
						return !(target is GamePlayer);
					}
			}
			return true;
		}

		/// <summary>
		/// Convinience method for checking if a player is an enemy of a keep
		/// This sets checkGroup to true in the main method
		/// </summary>
		/// <param name="keep">The keep checking</param>
		/// <param name="target">The target player</param>
		/// <returns>true if the player is an enemy of the keep</returns>
		public virtual bool IsEnemy(AbstractGameKeep keep, GamePlayer target)
		{
			return IsEnemy(keep, target, true);
		}

		/// <summary>
		/// Checks if a keep guard is an enemy of the player
		/// </summary>
		/// <param name="checker">The guard checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the guard</returns>
		public virtual bool IsEnemy(GameKeepGuard checker, GamePlayer target)
		{
			if (checker.Component == null || checker.Component.AbstractKeep == null)
				return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
			return IsEnemy(checker.Component.AbstractKeep, target);
		}

		public virtual bool IsEnemy(GameKeepGuard checker, GamePlayer target, bool checkGroup)
		{
			if (checker.Component == null || checker.Component.AbstractKeep == null)
				return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
			return IsEnemy(checker.Component.AbstractKeep, target, checkGroup);
		}

		/// <summary>
		/// Checks if a keep door is an enemy of the player
		/// </summary>
		/// <param name="checker">The door checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the door</returns>
		public virtual bool IsEnemy(GameKeepDoor checker, GamePlayer target)
		{
			return IsEnemy(checker.Component.AbstractKeep, target);
		}

		/// <summary>
		/// Checks if a keep component is an enemy of the player
		/// </summary>
		/// <param name="checker">The component checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the component</returns>
		public virtual bool IsEnemy(GameKeepComponent checker, GamePlayer target)
		{
			return IsEnemy(checker.AbstractKeep, target);
		}

		/// <summary>
		/// Gets a component height from a level
		/// </summary>
		/// <param name="level">The level</param>
		/// <returns>The height</returns>
		public virtual byte GetHeightFromLevel(byte level)
		{
			if (level > 15)
				return 5;
			if (level > 10)
				return 4;
			if (level > 7)
				return 3;
			if (level > 4)
				return 2;
			if (level > 1)
				return 1;
			
			return 0;
		}

		public virtual void GetBorderKeepLocation(int keepid, out int x, out int y, out int z, out ushort heading)
		{
			x = 0;
			y = 0;
			z = 0;
			heading = 0;
			switch (keepid)
			{
				//sauvage
				case 1:
					{
						x = 653811;
						y = 616998;
						z = 9560;
						heading = 2040;
						break;
					}
				//snowdonia
				case 2:
					{
						x = 616149;
						y = 679042;
						z = 9560;
						heading = 1611;
						break;
					}
				//svas
				case 3:
					{
						x = 651460;
						y = 313758;
						z = 9432;
						heading = 1004;
						break;
					}
				//vind
				case 4:
					{
						x = 715179;
						y = 365101;
						z = 9432;
						heading = 314;
						break;
					}
				//ligen
				case 5:
					{
						x = 396519;
						y = 618017;
						z = 9838;
						heading = 2159;
						break;
					}
				//cain
				case 6:
					{
						x = 432841;
						y = 680032;
						z = 9747;
						heading = 2585;
						break;
					}
			}
		}

		public virtual int GetRealmKeepBonusLevel(eRealm realm)
		{
			int keep = 7 - GetKeepCountByRealm(realm);
			return (int)(keep * ServerProperties.Properties.KEEP_BALANCE_MULTIPLIER);
		}

		public virtual int GetRealmTowerBonusLevel(eRealm realm)
		{
			int tower = 28 - GetTowerCountByRealm(realm);
			return (int)(tower * ServerProperties.Properties.TOWER_BALANCE_MULTIPLIER);
		}

		public virtual void UpdateBaseLevels()
		{
			lock (m_keepList.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keepList.Values)
				{
					if (m_frontierRegionsList.Contains(keep.Region) == false) 
						continue;

					byte newLevel = keep.BaseLevel;

					if (ServerProperties.Properties.BALANCE_TOWERS_SEPARATE)
					{
						if (keep is GameKeepTower)
							newLevel = (byte)(keep.DBKeep.BaseLevel + GameServer.KeepManager.GetRealmTowerBonusLevel((eRealm)keep.Realm));
						else
							newLevel = (byte)(keep.DBKeep.BaseLevel + GameServer.KeepManager.GetRealmKeepBonusLevel((eRealm)keep.Realm));
					}
					else
					{
						newLevel = (byte)(keep.DBKeep.BaseLevel + GameServer.KeepManager.GetRealmKeepBonusLevel((eRealm)keep.Realm) + GameServer.KeepManager.GetRealmTowerBonusLevel((eRealm)keep.Realm));
					}

					if (keep.BaseLevel != newLevel)
					{
						keep.BaseLevel = newLevel;

						foreach (GameKeepGuard guard in keep.Guards.Values)
						{
							keep.TemplateManager.GetMethod("SetGuardLevel").Invoke(null, new object[] { guard });
						}
					}
				}
			}
		}

		protected virtual void LoadBattlegroundCaps()
		{
			m_battlegrounds.AddRange(GameServer.Database.SelectAllObjects<Battleground>());
		}

		public virtual Battleground GetBattleground(ushort region)
		{
			foreach (Battleground bg in m_battlegrounds)
			{
				if (bg.RegionID == region)
					return bg;
			}
			return null;
		}

		public virtual void ExitBattleground(GamePlayer player)
		{
			string location = "";
			switch (player.Realm)
			{
				case eRealm.Albion: location = "Castle Sauvage"; break;
				case eRealm.Midgard: location = "Svasudheim Faste"; break;
				case eRealm.Hibernia: location = "Druim Ligen"; break;
			}

			if (location != "")
			{
				Teleport t = GameServer.Database.SelectObject<Teleport>("`TeleportID` = '" + location + "'");
				if (t != null)
					player.MoveTo((ushort)t.RegionID, t.X, t.Y, t.Z, (ushort)t.Heading);
			}
		}
	}
}
