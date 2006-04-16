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
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using DOL.Events;
using DOL.GS.Collections;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Defines all type of region
	/// </summary>
	public enum eRegionType : byte
	{
		/// <summary>
		/// Normal region
		/// </summary>
		Normal = 0,
		/// <summary>
		/// No pvp allowed / can't summon
		/// </summary>
		Safe = 1,
		/// <summary>
		/// Safe flag useless
		/// </summary>
		Frontier
	}

	/// <summary>
	/// This class represents a region in DAOC. A region is everything where you
	/// need a loadingscreen to go there. Eg. whole Albion is one Region, Midgard and
	/// Hibernia are just one region too. Darkness Falls is a region. Each dungeon, city
	/// is a region ... you get the clue. Each Region can hold an arbitary number of
	/// Zones! Camelot Hills is one Zone, Tir na Nog is one Zone (and one Region)...
	/// </summary>
	public class Region
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Persistant data

		/// <summary>
		/// The Region ID eg. 11
		/// </summary>
		protected int m_id;

		/// <summary>
		/// The Region Description eg. "Camelot Hills"
		/// </summary>
		protected string m_description;

		/// <summary>
		/// The region expansion
		/// </summary>
		protected byte m_expansion;

		/// <summary>
		/// Store the type of the region
		/// </summary>
		protected eRegionType m_type;

		/// <summary>
		/// Is diving anabled in region
		/// </summary>
		protected bool m_isDivingEnabled;

		/// <summary>
		/// Is a dungeon
		/// </summary>
		protected bool m_isDungeon;

		/// <summary>
		/// Is a instance
		/// </summary>
		protected bool m_isInstance;

		/// <summary>
		/// Holds all the Zones inside this Region
		/// </summary>
		protected Iesi.Collections.ISet m_zones;

		/// <summary>
		/// The unique Region id eg. 21
		/// </summary>
		public int RegionID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The Region Description eg. Cursed Forest
		/// </summary>
		public string Description
		{
			get { return m_description; }
			set { m_description = value; }
		}

		/// <summary>
		/// Gets or Sets the region expansion
		/// </summary>
		public byte Expansion
		{
			get { return m_expansion; }
			set { m_expansion = value; }
		}

		/// <summary>
		/// Gets or Sets the type of the region
		/// </summary>
		public eRegionType Type
		{
			get { return m_type; }
			set { m_type = value; }
		}

		/// <summary>
		/// Gets or Sets diving flag for region
		/// </summary>
		public bool IsDivingEnabled
		{
			get { return m_isDivingEnabled; }
			set { m_isDivingEnabled = value; }
		}

		/// <summary>
		/// Gets or Sets if this region is a dungeon
		/// </summary>
		public bool IsDungeon
		{
			get { return m_isDungeon; }
			set { m_isDungeon = value; }
		}

		/// <summary>
		/// Gets or Sets if this region is a instance
		/// </summary>
		public bool IsInstance
		{
			get { return m_isInstance; }
			set { m_isInstance = value; }
		}

		/// <summary>
		/// Get or set the set of all zones within this Region
		/// </summary>
		public Iesi.Collections.ISet Zones 
		{
			get 
			{ 
				if(m_zones==null) m_zones = new Iesi.Collections.HybridSet();
				return m_zones; 
			}
			set { m_zones = value; }
		}

		#endregion

		#region Runtime data

		/// <summary>
		/// This holds all objects inside this region. Their index = their id!
		/// </summary>
		private GeometryEngineNode[] m_objects;

		/// <summary>
		/// A queue where are stored all free object id
		/// </summary>
		private Queue m_emptyObjectID;

		/// <summary>
		/// This holds a counter with the absolute count of all objects that are actually in this region
		/// </summary>
		private int m_objectsInRegion;

		/// <summary>
		/// The region time manager
		/// </summary>
		protected GameTimer.TimeManager m_timeManager;

		/// <summary>
		/// Returns the object array of this region
		/// </summary>
		public GeometryEngineNode[] Objects
		{
			get { return m_objects; }
		}

		/// <summary>
		/// Gets the region time manager
		/// </summary>
		public GameTimer.TimeManager TimeManager
		{
			get { return m_timeManager; }
			set { m_timeManager = value; }
		}

		/// <summary>
		/// Gets the curret region time in milliseconds
		/// </summary>
		public long Time
		{
			get { return m_timeManager.CurrentTime; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Starts the RegionMgr
		/// </summary>
		public void StartRegionMgr()
		{
			m_timeManager.Start();
			Notify(RegionEvent.RegionStart, this);
		}

		/// <summary>
		/// Stops the RegionMgr
		/// </summary>
		public void StopRegionMgr()
		{
			m_timeManager.Stop();
			Notify(RegionEvent.RegionStop, this);
		}

		/// <summary>
		/// Adds an object to the region and assigns the object an id
		/// </summary>
		/// <param name="obj">A GameObject to be added to the region</param>
		/// <returns>success</returns>
		internal bool AddObject(GeometryEngineNode obj)
		{
			// first we check if this object is not already enter in this region
			if(obj.ObjectState != eObjectState.Inactive)
			{
				if (obj.ObjectID < m_objects.Length && obj == m_objects[obj.ObjectID - 1]) 
				{
					if (log.IsWarnEnabled)
						log.Warn(obj+" is already in the region "+RegionID);
					return false;
				}
				if (log.IsWarnEnabled)
					log.Warn(obj+" is not inactive and can't be added to the region "+RegionID);
				return false;
			}

			// second check if the obj position is inside this region
			Zone zoneToAdd = GetZone(obj.Position);
			if (zoneToAdd == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Destination zone not found for Object "+ obj+". Coohor are not avalable in this region.");
				return false;
			}

			// check if our region is already inited
			if(m_objects == null || m_objects.Length == 0 || m_emptyObjectID == null)
			{
				m_objects = new GeometryEngineNode[256];
				m_emptyObjectID = new Queue();
				m_objectsInRegion = 0;
			}

			lock(m_objects.SyncRoot)
			{
				// check if a old object id is free
				if(m_emptyObjectID.Count > 0)
				{
					int freeObjectID = (int)m_emptyObjectID.Dequeue();
					if(m_objects[freeObjectID - 1] != null)
					{
						if (log.IsWarnEnabled)
							log.Warn(obj+" can't re-use the ObjectID "+freeObjectID+" in region "+RegionID+" because this index in the array is not free !!!");
						return false;
					}

					m_objects[freeObjectID - 1] = obj;
					zoneToAdd.ObjectEnterZone(obj);
					obj.ObjectID = freeObjectID;
					m_objectsInRegion ++;

					return true;
				}

				// check if our m_objects array is enough big to hold a new object
				if(m_objectsInRegion + 1 >= m_objects.Length)
				{
					// we need to add a certain amount to grow
					int size = (int)(m_objects.Length * 1.20);
					if (size < m_objects.Length + 256)
						size = m_objects.Length + 256;

					GeometryEngineNode[] newObjectsRef = new GeometryEngineNode[size]; // grow the array by 20%, at least 256
					Array.Copy(m_objects, newObjectsRef, m_objects.Length);
					m_objects = newObjectsRef;
				}

				// here we are sure to have a object array without hole and with enough space to add the new object
				if(m_objects[m_objectsInRegion] != null)
				{
					if (log.IsWarnEnabled)
						log.Warn(obj+" can't use the ObjectID "+m_objectsInRegion+" in region "+RegionID+" because this index in the array is not free !!!");
					return false;
				}
			
				m_objects[m_objectsInRegion] = obj;
				zoneToAdd.ObjectEnterZone(obj);
				obj.ObjectID = m_objectsInRegion + 1;
				m_objectsInRegion ++;

				/*log.Error("---------------------------");
				int i = 0;
				foreach(GeometryEngineNode node in m_objects)
				{
					GameObject objct = node as GameObject;
					if(objct != null)
					{
						string prev = objct.previous != null ? objct.previous.ObjectID.ToString() : "null";
						string next = objct.next != null ? objct.next.ObjectID.ToString() : "null";
						log.Error("m_objects["+i+"] = "+objct.Name+" (precOID ="+ prev +") (suivOID ="+ next +")");
					}
					i++;
				}
				log.Error("---------------------------");*/
			}
			return true;
		}

		/// <summary>
		/// Removes the object with the specified ID from the region
		/// </summary>
		/// <param name="obj">A GameObject to be removed from the region</param>
		internal bool RemoveObject(GeometryEngineNode obj)
		{
			// first we check if this object is always in this region
			if(obj.ObjectState != eObjectState.Active)
			{
				if (log.IsWarnEnabled)
					log.Warn(obj+" can't be removed from region "+RegionID+" because its already inactive.");
				return false;
			}

			// second check if the obj position is inside this region
			Zone zoneToRemove = GetZone(obj.Position);
			if (zoneToRemove == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("Destination zone not found for Object "+ obj+". Coohor are not available in its region.");
				return false;
			}

			lock(m_objects.SyncRoot)
			{
				GeometryEngineNode inPlace = m_objects[obj.ObjectID - 1];
				if (inPlace == null) 
				{
					if (log.IsWarnEnabled)
						log.Warn("RemoveObject conflict! "+obj+" but there is no object in that slot !!!");
					return false;
				}
				if (obj != inPlace) 
				{
					if (log.IsWarnEnabled)
						log.Warn("RemoveObject conflict! "+obj+" but there was another object in that slot ("+inPlace+")");
					return false;
				}

				m_emptyObjectID.Enqueue(obj.ObjectID);
				m_objects[obj.ObjectID - 1] = null;
				zoneToRemove.ObjectExitZone(obj);
				m_objectsInRegion --;
				obj.ObjectID = -1; // invalidate object id

				/*log.Error("---------------------------");
				int i = 0;
				foreach(GeometryEngineNode node in m_objects)
				{
					GameObject objct = node as GameObject;
					if(objct != null)
					{
						string prev = objct.previous != null ? objct.previous.ObjectID.ToString() : "null";
						string next = objct.next != null ? objct.next.ObjectID.ToString() : "null";
						log.Error("m_objects["+i+"] = "+objct.Name+" (precOID ="+ prev +") (suivOID ="+ next +")");
					}
					i++;
				}
				log.Error("---------------------------");*/
			}
			return true;
		}

		/// <summary>
		/// Gets the object with the specified ID
		/// </summary>
		/// <param name="id">The ID of the object to get</param>
		/// <returns>The object with the specified ID, null if it didn't exist</returns>
		public GeometryEngineNode GetObject(int id)
		{
			if (id < 1 || id > m_objects.Length)
				return null;
			return m_objects[id - 1];
		}
		
		/// <summary>
		/// Returns the zone that contains the specified point.
		/// </summary>
		/// <param name="pos">global position for the zone you're retrieving</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public Zone GetZone(Point pos)
		{
			lock(m_zones)
			{
				foreach(Zone currentZone in m_zones)
				{
					if (currentZone.XOffset <= pos.X && currentZone.YOffset <= pos.Y && (currentZone.XOffset + Zone.ZONE_SIZE) > pos.X && (currentZone.YOffset + Zone.ZONE_SIZE) > pos.Y)
						return currentZone;
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the zone in this region with the id.
		/// </summary>
		/// <param name="zoneID">the id to search for</param>
		/// <returns>The zone you're retrieving or null if it couldn't be found</returns>
		public Zone GetZone(ushort zoneID)
		{
			foreach(Zone currentZone in m_zones)
			{
				if (currentZone.ZoneID == zoneID)
					return currentZone;
			}
			return null;
		}

		#endregion

		#region Notify

		public virtual void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GameEventMgr.Notify(e, sender, args);
		}

		public virtual void Notify(DOLEvent e, object sender)
		{
			Notify(e, sender, null);
		}

		public virtual void Notify(DOLEvent e)
		{
			Notify(e, null, null);
		}

		public virtual void Notify(DOLEvent e, EventArgs args)
		{
			Notify(e, null, args);
		}

		#endregion

		#region Get in radius / Get all objects

		/// <summary>
		/// Gets all objects of a given type inside the zone
		/// </summary>
		/// <param name="type">the type of object to get</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public DynamicList GetAllObjects(Type type) 
		{
			DynamicList result = new DynamicList();
			if(m_objects != null)
			{
				lock(m_objects.SyncRoot)
				{
					foreach(GeometryEngineNode node in m_objects)
					{
						if(type.IsInstanceOfType(node))
						{
							result.Add(node);
						}
					}
				}
			}
			return result;
		}

		/// <summary>
		/// Gets objects in a radius around a point
		/// </summary>
		/// <param name="type">the type of object to get</param>
		/// <param name="pos">origin</param>
		/// <param name="radius">radius around origin</param>
		/// <returns>IEnumerable to be used with foreach</returns>
		public DynamicList GetInRadius(Type type, Point pos, ushort radius) 
		{
			DynamicList result = new DynamicList();
			foreach(Zone currentZone in m_zones)
			{
				if (CheckShortestDistance(currentZone, pos.X, pos.Y, radius)) 
				{
					result = currentZone.GetObjectsInRadius(type, pos, radius, result);
				}
			}
			return result;
		}


		/// <summary>
		/// get the shortest distance from a point to a zone
		/// </summary>
		/// <param name="zone">The zone to check</param>
		/// <param name="x">X value of the point</param>
		/// <param name="y">Y value of the point</param>
		/// <param name="radius">The radius to compare the distance with</param>
		/// <returns>True if the distance is shorter false either</returns>
		private static bool CheckShortestDistance(Zone zone, int x, int y, uint radius)
		{
			//  coordinates of zone borders
			int xLeft = zone.XOffset;
			int xRight = zone.XOffset + Zone.ZONE_SIZE;
			int yTop = zone.YOffset;
			int yBottom = zone.YOffset + Zone.ZONE_SIZE;
			long distance = 0;
			
			if ((y >= yTop) && (y <= yBottom))
			{
				if ((x >= xLeft) && (x <= xRight))
				{
					return true;
				}
				else
				{
					int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
					distance = (long) xdiff*xdiff;
				}
			}
			else
			{
				if ((x >= xLeft) && (x <= xRight))
				{
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long) ydiff*ydiff;
				}
				else
				{
					int xdiff = Math.Min(FastMath.Abs(x - xLeft), FastMath.Abs(x - xRight));
					int ydiff = Math.Min(FastMath.Abs(y - yTop), FastMath.Abs(y - yBottom));
					distance = (long) xdiff*xdiff + (long) ydiff*ydiff;
				}
			}

			return (distance <= (radius*radius));
		}

		#endregion	
	}
}