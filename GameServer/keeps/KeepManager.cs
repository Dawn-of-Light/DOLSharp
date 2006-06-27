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
using System.Collections;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
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

		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		
		/// <summary>
		/// load all keep from DB
		/// </summary>
		/// <returns></returns>
		public static bool Load()
		{
			lock (m_keeps.SyncRoot)
			{
				m_keeps.Clear();

				IList keeps = GameServer.Database.SelectAllObjects(typeof(GameKeep));
                foreach (GameKeep keep in keeps)
				{
					m_keeps.Add(keep.KeepID, keep);
				}
                IList towers = GameServer.Database.SelectAllObjects(typeof(GameKeepTower));
                foreach (GameKeepTower tower in towers)
                {
                    m_keeps.Add(tower.KeepID, tower);
                }
				foreach(AbstractGameKeep keep in  m_keeps.Values)
				{
                    keep.Load();
				}
				//get with one command is more quick even if we look for keep in hashtable
				IList keepcomponents= GameServer.Database.SelectAllObjects(typeof(GameKeepComponent));
				foreach(GameKeepComponent component in keepcomponents)
				{
                    component.Load();
				}
				if (m_keeps.Count != 0)
				{
					foreach (AbstractGameKeep keep in m_keeps.Values)
					{
						if ( keep.KeepComponents.Count != 0)
							keep.KeepComponents.Sort();
					}
				}
				LoadHookPoints();
				//todo :to think : maybe done a better system with keep mgr in each region instanciate and list only in each region
				//GameEventMgr.AddHandler(RegionEvent.RegionStart,new DOLEventHandler(LoadKeep));
				//GamePlayerEvent.RegionChanged
			}
			return true;
		}

		private static void LoadHookPoints()
		{
			Hashtable hookPointList = new Hashtable();

			IList dbkeepHookPoints = GameServer.Database.SelectAllObjects(typeof(DBKeepHookPoint));
			foreach(DBKeepHookPoint dbhookPoint in dbkeepHookPoints)
			{
				ArrayList currentArray;
				string key = dbhookPoint.KeepComponentSkinID + "H:" + dbhookPoint.Height;
				if (!hookPointList.ContainsKey(key))
				{
					hookPointList.Add(key,new ArrayList());
				}
				currentArray = (ArrayList)hookPointList[key];
				currentArray.Add(dbhookPoint);
			}
			foreach(AbstractGameKeep keep in  m_keeps.Values)
			{
				foreach(GameKeepComponent component in keep.KeepComponents )
				{
					string key = component.Skin+"H:"+component.Height;
					if ((hookPointList.ContainsKey(key)))
					{
						ArrayList HPlist = hookPointList[key] as ArrayList;
						if ((HPlist != null) && (HPlist.Count != 0))
						{
							foreach(DBKeepHookPoint dbhookPoint in (ArrayList)hookPointList[key])
							{
								GameKeepHookPoint myhookPoint = new GameKeepHookPoint(dbhookPoint,component);
								component.HookPoints.Add(dbhookPoint.HookPointID,myhookPoint);
							}
							continue;
						}
					}
					//add this to keep hookpoint system until DB is not full
					for (int i = 0;i<38;i++)
						component.HookPoints.Add(i,new GameKeepHookPoint(i,component));

					component.HookPoints.Add(65,new GameKeepHookPoint(0x41,component));
					component.HookPoints.Add(97,new GameKeepHookPoint(0x61,component));
					component.HookPoints.Add(129,new GameKeepHookPoint(0x81,component));
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
		public static IEnumerable getKeepsCloseToSpot(ushort regionid, Point point3d, int radius)
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
		public static AbstractGameKeep getKeepCloseToSpot(ushort regionid, Point point3d, int radius)
		{
			return getKeepCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius); 
		}

		public static IList getKeepsByRealmMap(int map)
		{
			ArrayList myKeeps = new ArrayList();
			SortedList keepsByID = new SortedList();
			foreach(AbstractGameKeep keep in m_keeps.Values)
			{
				if (keep.Region.RegionID != 163)
					continue;
				if (((keep.KeepID & 0xFF) / 25 - 1) == map)
					keepsByID.Add(keep.KeepID,keep);
			}
			foreach(AbstractGameKeep keep in keepsByID.Values)
				myKeeps.Add(keep);
			return myKeeps;
		}
		public static IList getNFKeeps()
		{
			ArrayList myKeeps = new ArrayList();
			foreach(AbstractGameKeep keep in m_keeps.Values)
			{
				if (keep.Region.RegionID != 163)
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
		public static IEnumerable getKeepsCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			ArrayList myKeeps = new ArrayList();
			long radiussqrt = radius * radius;
			lock (m_keeps.SyncRoot)
			{
				foreach(AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region.RegionID != regionid)
						continue;
                    long xdiff = keep.Position.X - x;
                    long ydiff = keep.Position.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff ;
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
				foreach(AbstractGameKeep keep in m_keeps.Values)
				{
					if (keep.Region.RegionID != regionid)
						continue;
                    long xdiff = keep.Position.X - x;
                    long ydiff = keep.Position.Y - y;
					long range = xdiff * xdiff + ydiff * ydiff ;
					if (range > radiussqrt)
						continue;
					if ( myKeep == null || range <= myKeepRange )
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
			int index =0;
			lock (m_keeps.SyncRoot)
			{
				foreach(AbstractGameKeep keep in m_keeps.Values)
				{
					if (((eRealm)keep.Realm == realm) && (keep is GameKeepTower))
						index++;
				}
			}
			return index;
		}

		/// <summary>
		/// get keep count by realm
		/// </summary>
		/// <param name="realm"></param>
		/// <returns></returns>
		public static int GetKeepCountByRealm(eRealm realm)
		{
			int index =0;
			lock (m_keeps.SyncRoot)
			{
				foreach(AbstractGameKeep keep in m_keeps.Values)
				{
					if (((eRealm)keep.Realm == realm) && (keep is GameKeep))
						index++;
				}
			}
			return index;
		}

		/// <summary>
		/// Gets a copy of the current keeps table
		/// </summary>
		/// <returns></returns>
		public static Hashtable GetAllKeeps()
		{
			lock (m_keeps.SyncRoot)
			{
				return (Hashtable)m_keeps.Clone();
			}
		}
	}
}
