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
using DOL.Database;
using DOL.Events;
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
		/// Holds the MovementMgr instance
		/// </summary>
		protected static MovementMgr m_instance;

		/// <summary>
		/// Constructs a new MovementMgr instance
		/// </summary>
		public MovementMgr()
		{
			m_closeToWaypointEvent = new DOLEventHandler(OnCloseToWaypoint);
		}

		/// <summary>
		/// The MovementMgr instance
		/// </summary>
		public static MovementMgr Instance
		{
			get 
			{ 
				if (m_instance==null)
				{
					m_instance = new MovementMgr();
				}
				return m_instance;
			}
		}

		/// <summary>
		/// loads a path from database
		/// </summary>
		/// <param name="pathID">path to load</param>
		/// <returns>first pathpoint of path or null if not found</returns>
		public PathPoint LoadPath(string pathID)
		{
			SortedList sorted = new SortedList();
			pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
			DBPath dbpath = (DBPath) GameServer.Database.SelectObject(typeof(DBPath), "PathID='"+GameServer.Database.Escape(pathID)+"'");
			DBPathPoint[] pathpoints = null;
			ePathType pathType = ePathType.Once;

			if (dbpath != null) {
				pathpoints = dbpath.PathPoints;	
				pathType = (ePathType)dbpath.PathType;
			}
			if (pathpoints == null) {
				pathpoints = (DBPathPoint[]) GameServer.Database.SelectObjects(typeof(DBPathPoint), "PathID='"+GameServer.Database.Escape(pathID)+"'");				
			}

			foreach (DBPathPoint point in pathpoints)
			{
				sorted.Add(point.Step, point);
			}	
			PathPoint prev = null;
			PathPoint first = null;
			for (int i=0; i<sorted.Count; i++)
			{
				DBPathPoint pp = (DBPathPoint)sorted.GetByIndex(i);
				PathPoint p = new PathPoint(pp.X, pp.Y, pp.Z, pp.MaxSpeed,pathType);
				p.WaitTime = pp.WaitTime;

				if (first==null) 
				{
					first = p;
				}
				p.Prev = prev;
				if (prev!=null) 
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
		public void SavePath(string pathID, PathPoint path)
		{
			if ( path == null )
				return;

			pathID.Replace('\'', '/'); // we must replace the ', found no other way yet
			foreach (DBPath pp in GameServer.Database.SelectObjects(typeof(DBPath), "PathID='"+GameServer.Database.Escape(pathID)+"'")) 
			{
				GameServer.Database.DeleteObject(pp);
			}

			PathPoint root = FindFirstPathPoint(path);
			
			//Set the current pathpoint to the rootpoint!
			path = root;
			DBPath dbp = new DBPath(pathID, root.Type);
			GameServer.Database.AddNewObject(dbp);

			int i=1;
			do 
			{			
				DBPathPoint dbpp = new DBPathPoint(path.X, path.Y, path.Z, path.MaxSpeed);
				dbpp.Step = i++;
				dbpp.PathID = pathID;
				dbpp.WaitTime = path.WaitTime;
				GameServer.Database.AddNewObject(dbpp);
				path = path.Next;
			} while (path!=null && path!=root);
		}

		/// <summary>
		/// Searches for the first point in the waypoints chain
		/// </summary>
		/// <param name="path">One of the pathpoints</param>
		/// <returns>The first pathpoint in the chain or null</returns>
		public PathPoint FindFirstPathPoint(PathPoint path)
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
		/// <summary>
		/// let the npc travel on its path
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="speed"></param>
		public void MoveOnPath(GameNPC npc, int speed)
		{
			if (npc.CurrentWayPoint == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("No path to travel on for "+npc.Name);
				return;
			} 
			npc.PathingNormalSpeed = speed;
			//if (Point3D.GetDistance(npc.CurrentWayPoint, npc)<100)
			//not sure because here use point3D get distance but why??
			if (WorldMgr.CheckDistance(npc.CurrentWayPoint, npc,100))
			{
				if (npc.CurrentWayPoint.Type == ePathType.Path_Reverse && npc.CurrentWayPoint.FiredFlag)
					npc.CurrentWayPoint = npc.CurrentWayPoint.Prev;
				else
					npc.CurrentWayPoint = npc.CurrentWayPoint.Next;
				if ((npc.CurrentWayPoint.Type == ePathType.Loop) && (npc.CurrentWayPoint.Next == null))
				{
					npc.CurrentWayPoint = FindFirstPathPoint(npc.CurrentWayPoint);
				}
			}
			if (npc.CurrentWayPoint != null) 
			{
				GameEventMgr.AddHandler(npc, GameNPCEvent.CloseToTarget, new DOLEventHandler(OnCloseToWaypoint));
				npc.WalkTo(npc.CurrentWayPoint, Math.Min(speed, npc.CurrentWayPoint.MaxSpeed));
			} 
			else 
			{
				npc.Notify(GameNPCEvent.PathMoveEnds, npc);
			}					
		}

		/// <summary>
		/// The OnCloseToWaypoint delegate
		/// </summary>
		private readonly DOLEventHandler m_closeToWaypointEvent;

		/// <summary>
		/// decides what to do on reached waypoint in path
		/// </summary>
		/// <param name="e"></param>
		/// <param name="n"></param>
		/// <param name="args"></param>
		protected void OnCloseToWaypoint(DOLEvent e, object n, EventArgs args) 
		{
			GameNPC npc = (GameNPC)n;
			if (npc.CurrentWayPoint != null)
			{
				WaypointDelayAction waitTimer = new WaypointDelayAction(npc);
				waitTimer.Start(Math.Max(1, npc.CurrentWayPoint.WaitTime*100));
			}
			else
			{
				GameEventMgr.RemoveHandler(npc, GameNPCEvent.CloseToTarget, m_closeToWaypointEvent);
				npc.Notify(GameNPCEvent.PathMoveEnds, npc);
			}			
		}

		/// <summary>
		/// Delays movement to the next waypoint
		/// </summary>
		protected class WaypointDelayAction : RegionAction
		{
			/// <summary>
			/// Constructs a new WaypointDelayAction
			/// </summary>
			/// <param name="actionSource"></param>
			public WaypointDelayAction(GameObject actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;
				PathPoint oldPathPoint = npc.CurrentWayPoint;
				PathPoint nextPathPoint = npc.CurrentWayPoint.Next;
				if ((npc.CurrentWayPoint.Type == ePathType.Path_Reverse) && (npc.CurrentWayPoint.FiredFlag))
					nextPathPoint = npc.CurrentWayPoint.Prev;

				if ( nextPathPoint == null)
				{
					switch(npc.CurrentWayPoint.Type)
					{
					case ePathType.Loop :
						npc.CurrentWayPoint = Instance.FindFirstPathPoint(npc.CurrentWayPoint);
						break;
					case ePathType.Once :
						npc.CurrentWayPoint = null;//to stop
						break;
					case ePathType.Path_Reverse ://invert sens when go to end of path
						if (oldPathPoint.FiredFlag)
							npc.CurrentWayPoint = npc.CurrentWayPoint.Next;
						else
							npc.CurrentWayPoint = npc.CurrentWayPoint.Prev;
						break;
					}
				}
				else
				{
					if ((npc.CurrentWayPoint.Type == ePathType.Path_Reverse) && (npc.CurrentWayPoint.FiredFlag))
						npc.CurrentWayPoint = npc.CurrentWayPoint.Prev;
					else
						npc.CurrentWayPoint = npc.CurrentWayPoint.Next;
				}
				oldPathPoint.FiredFlag = !oldPathPoint.FiredFlag;
					 
				if (npc.CurrentWayPoint != null) 
				{
					npc.WalkTo(npc.CurrentWayPoint, Math.Min(npc.PathingNormalSpeed, npc.CurrentWayPoint.MaxSpeed));
				}
				else
				{
					GameEventMgr.RemoveHandler(npc, GameNPCEvent.CloseToTarget, Instance.m_closeToWaypointEvent);
					npc.Notify(GameNPCEvent.PathMoveEnds, npc);
				}
			}
		}
	}
}
