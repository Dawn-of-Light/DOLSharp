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
using DOL.GS.Database;
using System.Collections;
using System;
using log4net;
using System.Reflection;

namespace DOL.GS.Movement
{
	public class PathMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all the paths used in the world (id => path)
		/// </summary>
		public static IDictionary m_paths = new Hashtable(1);

		/// <summary>
		/// Get the path with the given id
		/// </summary>
		/// <param name="uniqueID">The id of the jump point to get</param>
		public static Path GetPath(int uniqueID)
		{
			if(uniqueID < 0)
			{
				return m_paths[uniqueID] as Path;
			}
			else
			{
				return GameServer.Database.FindObjectByKey(typeof(Path), uniqueID) as Path;
			}
		}

		/// <summary>
		/// Add a new path to the PathMgr
		/// This method is used by quests in order to add new path with scipts
		/// (the PathID of the new path must be less than 0 !!!)
		/// </summary>
		/// <param name="newPath">The new path to add</param>
		public static bool AddPath(Path newPath)
		{
			if(newPath.PathID > 0 || m_paths.Contains(newPath.PathID)) return false;
			
			m_paths.Add(newPath.PathID, newPath);
			return true;
		}
		
	}
}