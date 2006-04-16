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
using System.Reflection;
using DOL.GS.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// DoorMgr is manager of all door regular door and keep door
	/// </summary>		
	public sealed class DoorMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// this hash store all IDoors of the game by door unique id
		/// </summary>
		private static Hashtable m_doors = new Hashtable();

		/// <summary>
		/// This method add a new door to the door manadger
		/// </summary>	
		public static bool AddDoor(IDoor newDoor)
		{
			if(!m_doors.Contains(newDoor.DoorID))
			{
				m_doors.Add(newDoor.DoorID, newDoor);
				return true;
			}
			else
			{
				if (log.IsWarnEnabled)
					log.Warn("Door with DoorID: " + newDoor.DoorID + " defined twice in the db !!!");
				return false;
			}	
		}

		/// <summary>
		/// This method return the door with the given DoorID
		/// </summary>
		/// <returns>return the door</returns>
		public static IDoor GetDoor(int doorID)
		{
			return m_doors[doorID] as IDoor;
		}
	}
}
