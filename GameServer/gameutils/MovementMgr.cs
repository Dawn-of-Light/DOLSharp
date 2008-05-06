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
using DOL.Database2;
using log4net;

namespace DOL.GS.Movement
{
    /// <summary>
    /// TODO: instead movement manager we need AI when npc should travel on path and attack 
    /// enemies if they are near and after that return to pathing for example.
    /// this current implementation is incomplete but usable for horses
    /// </summary>
    public class MovementMgr
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// loads a path from database
        /// </summary>
        /// <param name="pathID">path to load</param>
        /// <returns>first pathpoint of path or null if not found</returns>
        public static PathPoint LoadPath(string pathID)
        {
            SortedList sorted = new SortedList();
            pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
            DBPath dbpath = (DBPath)GameServer.Database.SelectObject(typeof(DBPath),"PathID",pathID);
            DBPathPoint[] pathpoints = null;
            ePathType pathType = ePathType.Once;

            if (dbpath != null)
            {
                pathpoints = dbpath.PathPoints;
                pathType = (ePathType)dbpath.PathType;
            }
            if (pathpoints == null)
            {
                pathpoints = (DBPathPoint[])GameServer.Database.SelectObjects(typeof(DBPathPoint), "PathID",pathID);
            }

            foreach (DBPathPoint point in pathpoints)
            {
                sorted.Add(point.Step, point);
            }
            PathPoint prev = null;
            PathPoint first = null;
            for (int i = 0; i < sorted.Count; i++)
            {
                DBPathPoint pp = (DBPathPoint)sorted.GetByIndex(i);
                PathPoint p = new PathPoint(pp.X, pp.Y, pp.Z, pp.MaxSpeed, pathType);
                p.WaitTime = pp.WaitTime;

                if (first == null)
                {
                    first = p;
                }
                p.Prev = prev;
                if (prev != null)
                {
                    prev.Next = p;
                }
                prev = p;
            }
            /*if (pathType == ePathType.Loop )
            {
                prev.Next = first;
                first.Prev = prev;
            }*/
            return first;
        }

        /// <summary>
        /// Saves the path into the database
        /// </summary>
        /// <param name="pathID">The path ID</param>
        /// <param name="path">The path waypoint</param>
        public static void SavePath(string pathID, PathPoint path)
        {
            if (path == null)
                return;

            pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
            foreach (DBPath pp in GameServer.Database.SelectObjects(typeof(DBPath), pathID))
            {
                GameServer.Database.DeleteObject(pp);
            }

            PathPoint root = FindFirstPathPoint(path);

            //Set the current pathpoint to the rootpoint!
            path = root;
            DBPath dbp = new DBPath(pathID, root.Type);
            GameServer.Database.AddNewObject(dbp);

            int i = 1;
            do
            {
                DBPathPoint dbpp = new DBPathPoint(path.X, path.Y, path.Z, path.MaxSpeed);
                dbpp.Step = i++;
                dbpp.PathID = pathID;
                dbpp.WaitTime = path.WaitTime;
                GameServer.Database.AddNewObject(dbpp);
                path = path.Next;
            } while (path != null && path != root);
        }

        /// <summary>
        /// Searches for the first point in the waypoints chain
        /// </summary>
        /// <param name="path">One of the pathpoints</param>
        /// <returns>The first pathpoint in the chain or null</returns>
        public static PathPoint FindFirstPathPoint(PathPoint path)
        {
            PathPoint root = path;
            // avoid circularity
            int iteration = 50000;
            while (path.Prev != null && path.Prev != root)
            {
                path = path.Prev;
                iteration--;
                if (iteration <= 0)
                {
                    if (log.IsErrorEnabled)
                        log.Error("Path cannot be saved, it seems endless");
                    return null;
                }
            }
            return path;
        }
    }
}