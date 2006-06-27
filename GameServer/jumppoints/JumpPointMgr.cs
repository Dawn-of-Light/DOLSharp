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

namespace DOL.GS.JumpPoints
{
	public class JumpPointMgr
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds all the jump point used in the world (id => jump point)
		/// </summary>
		public static IDictionary m_jumpPoints = new Hashtable(1);

		/// <summary>
		/// Lod all jump points from the db
		/// </summary>
		public static bool LoadAllJumpPoints()
		{
			lock(m_jumpPoints.SyncRoot)
			{
				IList allJumpPoints = GameServer.Database.SelectAllObjects(typeof(AbstractJumpPoint));
				foreach (AbstractJumpPoint currentJumpPoint in allJumpPoints)
				{
					if(m_jumpPoints.Contains(currentJumpPoint.JumpPointID))
					{	
						if(log.IsWarnEnabled)
							log.Warn("Jumppoint unique id defined twice (JumpPointID : "+currentJumpPoint.JumpPointID+")");	
					}
					else
					{
						m_jumpPoints.Add(currentJumpPoint.JumpPointID, currentJumpPoint);
					}
				}
				return true;
			}

		}

		/// <summary>
		/// Get the jump point with the given id
		/// </summary>
		/// <param name="uniqueID">The id of the jump point to get</param>
		public static AbstractJumpPoint GetJumpPoint(int uniqueID)
		{
			return (m_jumpPoints[uniqueID] as AbstractJumpPoint);
		}
		
	}
}