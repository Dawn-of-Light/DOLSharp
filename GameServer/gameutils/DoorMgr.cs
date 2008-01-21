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

using DOL.Database2;
using DOL.GS.Keeps;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// DoorMgr is manager of all door regular door and keep door
	/// </summary>		
	public sealed class DoorMgr
	{
		private static Hashtable m_doors = new Hashtable();

		public static Hashtable Doors
		{
			get { return m_doors; }
		}

		/// <summary>
		/// this function load all door from DB
		/// </summary>	
		public static bool Init()
		{
			DatabaseObject[] dbdoors = GameServer.Database.SelectAllObjects(typeof(DBDoor));
			foreach (DBDoor door in dbdoors)
			{
				if (m_doors[door.InternalID] == null)
				{
					bool loaded = false;
					ushort zone = (ushort)(door.InternalID / 1000000);
					foreach (AbstractArea area in WorldMgr.GetZone(zone).GetAreasOfSpot(door.X, door.Y, door.Z))
					{
						if (area is KeepArea)
						{
							GameKeepDoor mydoor = new GameKeepDoor();
							mydoor.LoadFromDatabase(door);
							loaded = true;
							break;
						}
					}
					if (!loaded)
					{
						GameDoor mydoor = new GameDoor();
						mydoor.LoadFromDatabase(door);
					}
				}
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
	}
}
