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
using System.Reflection;
using DOL.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS.Keeps
{
	/// <summary>
	/// KeepManager
	/// The manager that keeps track of the keeps and stuff.. in the future.
	/// Right now it just has some utilities.
	/// </summary>
	public sealed class KeepMgr
	{
		/// <summary>
		/// list of all keeps
		/// </summary>
		private static readonly Hashtable m_keeps = new Hashtable();
		private static readonly List<Battleground> m_battlegrounds = new List<Battleground>();

		public const int NEW_FRONTIERS = 163;

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// load all keep from DB
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			ClothingMgr.LoadTemplates();

            //Dinberg - moved this here, battlegrounds must be loaded before keepcomponents are.
            LoadBattlegroundCaps();

			if (!ServerProperties.Properties.LOAD_KEEPS)
				return true;

			lock (m_keeps.SyncRoot)
			{
				m_keeps.Clear();

				DataObject[] keeps = GameServer.Database.SelectAllObjects(typeof(DBKeep));
				foreach (DBKeep datakeep in keeps)
				{
					if (WorldMgr.GetRegion((ushort)datakeep.Region) == null)
						continue;

                    //Dinberg - checking whether the keep is old or new.
                    //The only way to do this is to examine the database entries for hookpoints and thus determine
                    //in this manner whether the keep is old, new or 'both'. A keep will be 'both' if it is found to
                    //have components of both sets, which is possible.

                    //'town' keeps have no components, and can always be loaded.
                    bool isOld = false;
                    bool isNew = false;

                    //I don't want to touch the loading order of hookpoints, as i think they may depend on the
                    //assumption keeps and towers are linked before population. So we will settle for a second
                    //query. It's on server start, so it wont impact running performance.

                    DBKeepComponent[] currentKeepComponents = (DBKeepComponent[])GameServer.Database.SelectObjects(typeof(DBKeepComponent), "`KeepID` = '" + datakeep.KeepID + "'");
				
                    //Pass through, and depending on the outcome of the components, determine the 'age' of the keep.
                    foreach (DBKeepComponent dum in currentKeepComponents)
                    {
                        if (dum.Skin >= 0 && dum.Skin <= 20) //these are the min/max ids for old keeps.
                            isOld = true;
                        if (dum.Skin > 20) //any skinID greater than this are ids for new keeps.
                            isNew = true;
                    }

                    //Now, consult server properties to decide our plan!

                    //Quote: ServerProperties.cs
                    //"use_new_keeps", "Keeps to load. 0 for Old Keeps, 1 for new keeps, 2 for both.", 2

                    if (ServerProperties.Properties.USE_NEW_KEEPS == 0 && isNew)
                        continue;

                    if (ServerProperties.Properties.USE_NEW_KEEPS == 1 && isOld)
                        continue;

                    //If we've got this far, we are permitted to load as per normal!

					AbstractGameKeep keep;
					if ((datakeep.KeepID >> 8) != 0)
					{
						keep = new GameKeepTower();
					}
					else
					{
						keep = new GameKeep();
					}

					keep.Load(datakeep);
					m_keeps.Add(datakeep.KeepID, keep);
				}

				// This adds owner keeps to towers / portal keeps
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					GameKeepTower tower = keep as GameKeepTower;
					if (tower != null)
					{
						int index = tower.KeepID & 0xFF;
						GameKeep ownerKeep = getKeepByID(index) as GameKeep;
						if (ownerKeep != null)
						{
							ownerKeep.AddTower(tower);
						}
						tower.Keep = ownerKeep;
					}
				}

				DBKeepComponent[] keepcomponents = (DBKeepComponent[])GameServer.Database.SelectAllObjects(typeof(DBKeepComponent));
				foreach (DBKeepComponent component in keepcomponents)
				{
					// if use old keeps don't try to load new components
					if (ServerProperties.Properties.USE_NEW_KEEPS == 0 && IsNewKeepComponent(component.Skin))
						continue;
					
					// if use new keeps don't try and load old components
					if (ServerProperties.Properties.USE_NEW_KEEPS == 1 && !IsNewKeepComponent(component.Skin))
						continue;

					AbstractGameKeep keep = getKeepByID(component.KeepID);
					if (keep == null)
					{
						if (Logger.IsWarnEnabled)
						{
							Logger.WarnFormat("Keep with ID {0} not found while loading component with ID {1}, possibly old/new keeptype; see server properties", component.KeepID, component.ID);
						}
						continue;
					}
					GameKeepComponent gamecomponent = new GameKeepComponent();
					gamecomponent.LoadFromDatabase(component, keep);
					keep.KeepComponents.Add(gamecomponent);
				}
				if (m_keeps.Count != 0)
				{
					foreach (AbstractGameKeep keep in m_keeps.Values)
					{
						if (keep.KeepComponents.Count != 0)
							keep.KeepComponents.Sort();
					}
				}
				LoadHookPoints();

				Logger.Info("Loaded " + m_keeps.Count + " keeps successfully");
			}

			if (ServerProperties.Properties.USE_KEEP_BALANCING)
				UpdateBaseLevels();

			if (ServerProperties.Properties.USE_LIVE_KEEP_BONUSES)
				KeepBonusMgr.UpdateCounts();

			return true;
		}


		public static bool IsNewKeepComponent(int skin)
		{
			if (skin > 20) 
				return true;

			return false;
		}


		private static void LoadHookPoints()
		{
			if (!ServerProperties.Properties.LOAD_KEEPS)
				return;

			Hashtable hookPointList = new Hashtable();

			DBKeepHookPoint[] dbkeepHookPoints = (DBKeepHookPoint[])GameServer.Database.SelectAllObjects(typeof(DBKeepHookPoint));
			foreach (DBKeepHookPoint dbhookPoint in dbkeepHookPoints)
			{
				ArrayList currentArray;
				string key = dbhookPoint.KeepComponentSkinID + "H:" + dbhookPoint.Height;
				if (!hookPointList.ContainsKey(key))
				{
					hookPointList.Add(key, new ArrayList());
				}
				currentArray = (ArrayList)hookPointList[key];
				currentArray.Add(dbhookPoint);
			}
			foreach (AbstractGameKeep keep in m_keeps.Values)
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					string key = component.Skin + "H:" + component.Height;
					if ((hookPointList.ContainsKey(key)))
					{
						ArrayList HPlist = hookPointList[key] as ArrayList;
						if ((HPlist != null) && (HPlist.Count != 0))
						{
							foreach (DBKeepHookPoint dbhookPoint in (ArrayList)hookPointList[key])
							{
								GameKeepHookPoint myhookPoint = new GameKeepHookPoint(dbhookPoint, component);
								component.HookPoints.Add(dbhookPoint.HookPointID, myhookPoint);
							}
							continue;
						}
					}
					//add this to keep hookpoint system until DB is not full
					for (int i = 0; i < 38; i++)
						component.HookPoints.Add(i, new GameKeepHookPoint(i, component));

					component.HookPoints.Add(65, new GameKeepHookPoint(0x41, component));
					component.HookPoints.Add(97, new GameKeepHookPoint(0x61, component));
					component.HookPoints.Add(129, new GameKeepHookPoint(0x81, component));
				}
			}

			Logger.Info("Loading HookPoint items");

			//fill existing hookpoints with objects
			foreach (AbstractGameKeep keep in m_keeps.Values)
			{
				foreach (GameKeepComponent component in keep.KeepComponents)
				{
					foreach (GameKeepHookPoint hp in component.HookPoints.Values)
					{
						DBKeepHookPointItem item = (DBKeepHookPointItem)GameServer.Database.SelectObject(typeof(DBKeepHookPointItem), "KeepID = '" + component.Keep.KeepID + "' AND ComponentID = '" + component.ID + "' AND HookPointID = '" + hp.ID + "'");
						if (item != null)
							HookPointItem.Invoke(component.HookPoints[hp.ID] as GameKeepHookPoint, item.ClassType);
					}
				}
			}
		}

		/// <summary>
		/// get keep by ID
		/// </summary>
		/// <param name="id">id of keep</param>
		/// <returns> Game keep object with keepid = id</returns>
		public static AbstractGameKeep getKeepByID(int id)
		{
			return m_keeps[id] as AbstractGameKeep;
		}

		/// <summary>
		/// get list of keep close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="point3d"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static IEnumerable getKeepsCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
		{
			return getKeepsCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
		}

		/// <summary>
		/// get the keep with minimum distance close to spot
		/// </summary>
		/// <param name="regionid"></param>
		/// <param name="point3d"></param>
		/// <param name="radius"></param>
		/// <returns></returns>
		public static AbstractGameKeep getKeepCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
		{
			return getKeepCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius);
		}

		/// <summary>
		/// Gets all keeps by a realm map /rw
		/// </summary>
		/// <param name="map"></param>
		/// <returns></returns>
		public static ICollection<AbstractGameKeep> getKeepsByRealmMap(int map)
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			SortedList keepsByID = new SortedList();
			foreach (AbstractGameKeep keep in m_keeps.Values)
			{
				if (keep.CurrentRegion.ID != NEW_FRONTIERS)
					continue;
				if (((keep.KeepID & 0xFF) / 25 - 1) == map)
					keepsByID.Add(keep.KeepID, keep);
			}
			foreach (AbstractGameKeep keep in keepsByID.Values)
				myKeeps.Add(keep);
			return myKeeps;
		}

		/// <summary>
		/// Get the battleground portal keep for a player
		/// </summary>
		/// <param name="player">The player</param>
		/// <returns>The battleground portal keep as AbstractGameKeep or null</returns>
		public static AbstractGameKeep GetBGPK(GamePlayer player)
		{
			//the temporary keep variable for use in this method
			AbstractGameKeep tempKeep = null;

			//iterate through keeps and find all those which we aren't capped out for
			foreach (AbstractGameKeep keep in m_keeps.Values)
			{
				// find keeps in the battlegrounds that arent portal keeps
				if (keep.Region != NEW_FRONTIERS && !keep.IsPortalKeep) continue;
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

		public static ICollection<AbstractGameKeep> GetNFKeeps()
		{
			return GetKeepsOfRegion(NEW_FRONTIERS);
		}

		public static ICollection<AbstractGameKeep> GetKeepsOfRegion(ushort region)
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			foreach (AbstractGameKeep keep in m_keeps.Values)
			{
				if (keep.CurrentRegion.ID != region)
					continue;
				myKeeps.Add(keep);
			}
			return myKeeps;
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
		public static ICollection<AbstractGameKeep> getKeepsCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			long radiussqrt = radius * radius;
			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.CurrentRegion.ID != regionid)
						continue;
					long xdiff = keep.DBKeep.X - x;
					long ydiff = keep.DBKeep.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff;
					if (range < radiussqrt)
						myKeeps.Add(keep);
				}
			}
			return myKeeps;
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
		public static AbstractGameKeep getKeepCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			AbstractGameKeep myKeep = null;
			lock (m_keeps.SyncRoot)
			{
				long radiussqrt = radius * radius;
				long myKeepRange = radiussqrt;
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.DBKeep.Region != regionid)
						continue;
					long xdiff = keep.DBKeep.X - x;
					long ydiff = keep.DBKeep.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff;
					if (range > radiussqrt)
						continue;
					if (myKeep == null || range <= myKeepRange)
					{
						myKeep = keep;
						myKeepRange = range;
					}
				}
			}
			return myKeep;
		}

		/// <summary>
		/// get keep count controlled by realm to calculate keep bonus
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static int GetTowerCountByRealm(eRealm realm)
		{
			int index = 0;
			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region != NEW_FRONTIERS) continue;
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
		public static Dictionary<eRealm, int> GetTowerCountAllRealm()
		{
			Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm,int>(3);
			realmXTower.Add(eRealm.Albion, 0);
			realmXTower.Add(eRealm.Hibernia, 0);
			realmXTower.Add(eRealm.Midgard, 0);

			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region == NEW_FRONTIERS && keep is GameKeepTower)
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
		public static Dictionary<eRealm, int> GetTowerCountFromZones(List<int> zones)
		{
			Dictionary<eRealm, int> realmXTower = new Dictionary<eRealm, int>(4);
			realmXTower.Add(eRealm.Albion, 0);
			realmXTower.Add(eRealm.Hibernia, 0);
			realmXTower.Add(eRealm.Midgard, 0);
			realmXTower.Add(eRealm.None, 0);

			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region == NEW_FRONTIERS && keep is GameKeepTower && zones.Contains(keep.CurrentZone.ID))
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
		public static int GetKeepCountByRealm(eRealm realm)
		{
			int index = 0;
			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region != NEW_FRONTIERS) continue;
					if (((eRealm)keep.Realm == realm) && (keep is GameKeep))
						index++;
				}
			}
			return index;
		}

		public static ICollection<AbstractGameKeep> GetAllKeeps()
		{
			List<AbstractGameKeep> myKeeps = new List<AbstractGameKeep>();
			foreach (AbstractGameKeep keep in m_keeps.Values)
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
		public static bool IsEnemy(AbstractGameKeep keep, GamePlayer target, bool checkGroup)
		{
			if (target.Client.Account.PrivLevel != 1)
				return false;

			switch (GameServer.Instance.Configuration.ServerType)
			{
				case eGameServerType.GST_Normal:
					return keep.Realm != target.Realm;
				case eGameServerType.GST_PvP:
					{
						if (keep.Guild == null)
							return false;
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
					return !(target is GamePlayer);
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
		public static bool IsEnemy(AbstractGameKeep keep, GamePlayer target)
		{
			return IsEnemy(keep, target, true);
		}

		/// <summary>
		/// Checks if a keep guard is an enemy of the player
		/// </summary>
		/// <param name="checker">The guard checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the guard</returns>
		public static bool IsEnemy(GameKeepGuard checker, GamePlayer target)
		{
			if (checker.Component == null || checker.Component.Keep == null)
				return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
			return IsEnemy(checker.Component.Keep, target);
		}
		public static bool IsEnemy(GameKeepGuard checker, GamePlayer target, bool checkGroup)
		{
			if (checker.Component == null || checker.Component.Keep == null)
				return GameServer.ServerRules.IsAllowedToAttack(checker, target, true);
			return IsEnemy(checker.Component.Keep, target, checkGroup);
		}

		/// <summary>
		/// Checks if a keep door is an enemy of the player
		/// </summary>
		/// <param name="checker">The door checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the door</returns>
		public static bool IsEnemy(GameKeepDoor checker, GamePlayer target)
		{
			return IsEnemy(checker.Component.Keep, target);
		}

		/// <summary>
		/// Checks if a keep component is an enemy of the player
		/// </summary>
		/// <param name="checker">The component checker</param>
		/// <param name="target">The player target</param>
		/// <returns>true if the player is an enemy of the component</returns>
		public static bool IsEnemy(GameKeepComponent checker, GamePlayer target)
		{
			return IsEnemy(checker.Keep, target);
		}

		/// <summary>
		/// Gets a component height from a level
		/// </summary>
		/// <param name="level">The level</param>
		/// <returns>The height</returns>
		public static byte GetHeightFromLevel(byte level)
		{
			if (level > 7)
				return 3;
			else if (level > 4)
				return 2;
			else if (level > 1)
				return 1;
			else
				return 0;
		}

		public static void GetBorderKeepLocation(int keepid, out int x, out int y, out int z, out ushort heading)
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

		public static int GetRealmKeepBonusLevel(eRealm realm)
		{
			int keep = 7 - GetKeepCountByRealm(realm);
			return (int)(keep * ServerProperties.Properties.KEEP_BALANCE_MULTIPLIER);
		}

		public static int GetRealmTowerBonusLevel(eRealm realm)
		{
			int tower = 28 - GetTowerCountByRealm(realm);
			return (int)(tower * ServerProperties.Properties.TOWER_BALANCE_MULTIPLIER);
		}

		public static void UpdateBaseLevels()
		{
			lock (m_keeps.SyncRoot)
			{
				foreach (AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region != NEW_FRONTIERS) 
						continue;

					if (ServerProperties.Properties.BALANCE_TOWERS_SEPARATE)
					{
						if (keep is GameKeepTower)
							keep.BaseLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmTowerBonusLevel((eRealm)keep.Realm));
						else
							keep.BaseLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmKeepBonusLevel((eRealm)keep.Realm));
					}
					else
					{
						keep.BaseLevel = (byte)(keep.DBKeep.BaseLevel + KeepMgr.GetRealmKeepBonusLevel((eRealm)keep.Realm) + KeepMgr.GetRealmTowerBonusLevel((eRealm)keep.Realm));
					}


					foreach (GameKeepGuard guard in keep.Guards.Values)
					{
						TemplateMgr.SetGuardLevel(guard);
					}
				}
			}
		}

		private static void LoadBattlegroundCaps()
		{
			Battleground[] bgs = (Battleground[])GameServer.Database.SelectAllObjects(typeof(Battleground));
			foreach (Battleground bg in bgs)
				m_battlegrounds.Add(bg);
		}

		public static Battleground GetBattleground(ushort region)
		{
			foreach (Battleground bg in m_battlegrounds)
			{
				if (bg.RegionID == region)
					return bg;
			}
			return null;
		}

		public static void ExitBattleground(GamePlayer player)
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
				Teleport t = (Teleport)GameServer.Database.SelectObject(typeof(Teleport), "`TeleportID` = '" + location + "'");
				if (t != null)
					player.MoveTo((ushort)t.RegionID, t.X, t.Y, t.Z, (ushort)t.Heading);
			}
		}
	}
}
