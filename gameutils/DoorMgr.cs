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
using System;
using System.Reflection;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// DoorMgr is manager of all door regular door and keep door
	/// </summary>		
	public sealed class DoorMgr
	{
		private static Hashtable m_doors;

		/// <summary>
		/// this function load all door from DB
		/// </summary>	
		public static bool Init()
		{
			m_doors = new Hashtable();
			DataObject[] dbdoors =	GameServer.Database.SelectAllObjects(typeof(DBDoor));
			foreach(DBDoor door in dbdoors)
			{
				GameObject mydoor;
				if (door.KeepID != 0) 
				{
					AbstractGameKeep keep = KeepMgr.getKeepByID(door.KeepID);
					mydoor = new GameKeepDoor(keep);
				}
				else
					mydoor = new GameDoor();
				mydoor.LoadFromDatabase(door);
				m_doors.Add(door.InternalID,mydoor);
			}
			return true;
		}

		/// <summary>
		/// This function get the door object by door index
		/// </summary>
		/// <returns>return the door with the index</returns>
		public static IDoor getDoorByID(int id)
		{
			return m_doors[id] as IDoor;
		}

		/// <summary>
		/// This function get the door close to spot
		/// </summary>
		/// <returns>array of door</returns>
		public static IEnumerable getDoorsCloseToSpot(ushort regionid, IPoint3D point3d, int radius)
		{
			return getDoorsCloseToSpot(regionid, point3d.X, point3d.Y, point3d.Z, radius); 
		}

		/// <summary>
		/// This function get the door close to spot
		/// </summary>
		/// <returns>array of door</returns>
		public static IEnumerable getDoorsCloseToSpot(ushort regionid, int x, int y, int z, int radius)
		{
			ArrayList mydoors = new ArrayList();
			int radiussqrt = radius * radius;
			lock (m_doors.SyncRoot)
			{
				foreach(GameObject door in m_doors.Values)//door inerite from GameObject and IDoor
				{
					if (door.CurrentRegionID != regionid)
						continue;
					int xdiff = door.X - x;
					int ydiff = door.Y - y;
					int range = xdiff * xdiff + ydiff * ydiff ;
					if (range < radiussqrt)
						mydoors.Add(door);
				}
			}
			return mydoors;
		}
	}
}
