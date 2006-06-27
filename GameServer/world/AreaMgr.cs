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

using DOL.Events;
using DOL.GS.Database;
using System.Collections;
using System;
using log4net;
using System.Reflection;

namespace DOL.GS
{
	public class AreaMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all the areas used in the world (zone => list of areas inside)
		/// </summary>
		public static IDictionary m_areas = new Hashtable(1);

		/// <summary>
		/// Register a new area
		/// </summary>
		/// <param name="newArea">The new area to add</param>
		public static bool RegisterArea(AbstractArea newArea)
		{
			lock(m_areas.SyncRoot)
			{
				Region regionToCheck = WorldMgr.GetRegion((ushort)newArea.RegionID);
				if(regionToCheck == null || regionToCheck.Zones == null)
					return false;

				foreach(Zone currentZone in regionToCheck.Zones)
				{
					IList areasInZone = (IList)m_areas[currentZone];
					if(areasInZone != null)
					{
						foreach(AbstractArea currentArea in areasInZone)
						{
							if(currentArea.IsEqual(newArea))
								return false;
						}
					}
				}

				bool added = false;
				foreach(Zone currentZone in regionToCheck.Zones)
				{
					if(newArea.IsIntersectingZone(currentZone))
					{
						IList areasOfZone = (IList)m_areas[currentZone];
						if(areasOfZone == null)
						{	
							areasOfZone = new ArrayList(1);
							m_areas.Add(currentZone, areasOfZone);
						}
						areasOfZone.Add(newArea);
						added = true;
					}
				}

				return added;
			}
		}

		/// <summary>
		/// Unregister a old area
		/// </summary>
		/// <param name="oldArea">The new area to add</param>
		public static bool UnregisterArea(AbstractArea oldArea)
		{
			lock(m_areas.SyncRoot)
			{
				Region regionToCheck = WorldMgr.GetRegion((ushort)oldArea.RegionID);
				if(regionToCheck == null || regionToCheck.Zones == null)
					return false;

				bool deleted = false;
				foreach(Zone currentZone in regionToCheck.Zones)
				{
					IList areasInZone = (IList)m_areas[currentZone];
					if(areasInZone != null && areasInZone.Contains(oldArea))
					{
						deleted = true;
						areasInZone.Remove(oldArea);
					}
				}
				return deleted;
			}
		}

		/// <summary>
		/// Get all areas registered in the zone constraining the spot
		/// </summary>
		/// <param name="zoneToCheck">The zone to check</param>
		/// <param name="spot">The point to search</param>
		public static IList GetAreasOfSpot(Zone zoneToCheck, Point spot)
		{
			lock(m_areas.SyncRoot)
			{
				IList finalAreaList = new ArrayList(1);
				
				IList areasInZone = (IList)m_areas[zoneToCheck];
				if(areasInZone == null) 
					return finalAreaList;

				foreach(AbstractArea currentArea in areasInZone)
				{
					if(currentArea.IsContaining(spot))
					{
						finalAreaList.Add(currentArea);
					}
				}
				return finalAreaList;
			}
		}

		/// <summary>
		/// Load all areas from the db and register it
		/// </summary>
		public static bool LoadAllAreas()
		{
			IList allAreasSavedInDB = GameServer.Database.SelectAllObjects(typeof(AbstractArea));
			foreach (AbstractArea currentArea in allAreasSavedInDB)
			{
				if(RegisterArea(currentArea) == true)
				{
					if(log.IsDebugEnabled)
						log.Debug("Area loaded :"+ currentArea.Description +" , Region :"+currentArea.RegionID);
				}
			}
			return true;
		}
	}
}