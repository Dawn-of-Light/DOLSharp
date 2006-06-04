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
using System.Text;
using DOL.Events;
using DOL.GS.Collections;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Holds the current state of the object
	/// </summary>
	public enum eObjectState : byte
	{
		/// <summary>
		/// Active, visibly in world (active in the geometry engine) 
		/// </summary>
		Active,

		/// <summary>
		/// Inactive, currently not in the geometry engine (use only active object in game)
		/// </summary>
		Inactive
	}

	/// <summary>
	/// Holds the current state of the object
	/// </summary>
	public enum eRemoveType : byte
	{
		/// <summary>
		/// Play the kill animation, wait a minute and then remove the object 
		/// </summary>
		KillAndDisappear	= 0,

		/// <summary>
		/// Play no animation, only remove the object 
		/// </summary>
		Disappear			= 1,

		/// <summary>
		/// Play the kill animation, and do nothing else
		/// </summary>
		Kill				= 2
	}

	/// <summary>
	/// This class represent a base object handled in the geometry system
	/// </summary>
	/// </summary>
	public abstract class GeometryEngineNode : IWorldPosition
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Geometry engine / State

		/// <summary>
		/// The next element in the subZone of this object
		/// </summary>
		public GeometryEngineNode next = null;

		/// <summary>
		/// The next preview in the subZone of this object
		/// </summary>
		public GeometryEngineNode previous = null;

		/// <summary>
		/// The objectID. This is -1 as long as the object is not added to a region!
		/// </summary>
		protected int m_ObjectID = -1;

		/// <summary>
		/// Returns the current geometry engine state of the object.
		/// </summary>
		public eObjectState ObjectState
		{
			get { return m_ObjectID == -1 ? eObjectState.Inactive : eObjectState.Active; }
		}

		/// <summary>
		/// Gets or Sets the current ObjectID of the Object
		/// This is done automatically by the Region and should
		/// not be done manually!!!
		/// </summary>
		public int ObjectID
		{
			get { return m_ObjectID; }
			set { m_ObjectID = value; }
		}

		/// <summary>
		/// Insert a node in the first position in the list
		/// This is done automatically by the geometry engine and must
		/// not be done manually!!!
		/// </summary>
		/// <param name="newElem">The node to insert</param>
		public GeometryEngineNode AddToElementsList(GeometryEngineNode newElem)
		{
			if(newElem.next != null || newElem.previous != null)
			{
				if(log.IsErrorEnabled)
					log.Error("GeometryEngineNode.AddToElementsList try to add a object already in a other list !!!");
			
				return this;
			}
			
			newElem.next = this;
			previous = newElem;

			return newElem;
		}


		/// <summary>
		/// Remove this node from the subZone list
		/// This is done automatically by the geometry engine and must
		/// not be done manually!!!
		/// </summary>
		public GeometryEngineNode RemoveFromElementsList(GeometryEngineNode remElem)
		{
			if(remElem.previous == null) // remElem is the first in the list
			{
				if(remElem.next == null) //remElem is alone in the list
				{
					remElem.previous = remElem.next = null;
					return null;
				}
				else
				{
					GeometryEngineNode newFirst = remElem.next;
					newFirst.previous = null;

					remElem.previous = remElem.next = null;
					return newFirst;
				}
			} 
			else 
			{
				if(remElem.next == null) // remElem last in the list
				{
					remElem.previous.next = null;
				}
				else
				{
					remElem.next.previous = remElem.previous;
					remElem.previous.next = remElem.next;
				}

				remElem.previous = remElem.next = null;
				return this;
			}
		}

		#endregion

		#region Position

		/// <summary>
		/// The object's position in the current region.
		/// </summary>
		protected Point m_position;
		
		/// <summary>
		/// The object's current region.
		/// </summary>
		protected Region m_region;

		/// <summary>
		/// The direction the Object is facing
		/// </summary>
		protected ushort m_heading;

		/// <summary>
		/// Gets or sets the object's position in the region.
		/// </summary>
		public virtual Point Position
		{
			get { return m_position; }
			set
			{
				if(ObjectState == eObjectState.Active) return;
				m_position = value;
			}
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public virtual Region Region
		{
			get { return m_region; }
			set
			{
				if(ObjectState == eObjectState.Active) return;
				m_region = value;
			}
		}

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public virtual int Heading
		{
			get { return m_heading; }
			set { m_heading = (ushort)(value&0xFFF); }
		}

		/// <summary>
		/// Checks if object is swimming
		/// </summary>
		public virtual bool IsSwimming
		{
			get { return false; } // dol need to know the world to say that
			set {}
		}

		/// <summary>
		/// Checks if object is swimming
		/// </summary>
		public virtual bool IsDiving
		{
			get { return false; } // dol need to know the world to say that
			set {}
		}

		/// <summary>
		/// Returns the angle towards a target spot in degrees, clockwise
		/// </summary>
		/// <param name="point">target point</param>
		/// <returns>the angle towards the spot</returns>
		public float GetAngleToSpot(Point point)
		{
			float headingDifference = (Position.GetHeadingTo(point) - Heading) & 0xFFF;
			return (headingDifference*360.0f/4096.0f);
		}

		/// <summary>
		/// Calculates a spot into the heading direction
		/// </summary>
		/// <param name="distance">the distance to the spot</param>
		/// <returns>the result position</returns>
		public Point GetSpotFromHeading(int distance)
		{
			Point pos = Position;
			double angle = Heading/Point.HEADING_CONST;
			double x = pos.X - Math.Sin(angle)*distance;
			double y = pos.Y + Math.Cos(angle)*distance;
			
			pos.X = (x > 0 ? (int) x : 0);
			pos.Y = (y > 0 ? (int) y : 0);
			pos.Z = 0;
			return pos;
		}

		/// <summary>
		/// determines wether a target object is front
		/// in front is defined as north +- viewangle/2
		/// </summary>
		/// <param name="target"></param>
		/// <param name="viewangle"></param>
		/// <returns></returns>
		public virtual bool IsObjectInFront(GeometryEngineNode target, double viewangle)
		{
			if (target == null)
				return false;
			float angle = GetAngleToSpot(target.Position);
			viewangle *= 0.5;
			if (angle >= 360.0 - viewangle || angle < viewangle)
				return true;
			// if target is closer than 32 units it is considered always in view
			// tested and works this way for noraml evade, parry, block (in 1.69)
			return Position.CheckSquareDistance(target.Position, 32*32);
		}

		#endregion

		#region AddToWorld / RemoveFromWorld / MoveTo

		/// <summary>
		/// Register this object in the geometry engine
		/// The object is added in teh world at its current Position in its curent Region
		/// The registering process in the geometry engine do :
		/// - assign a valid object unique id
		/// - add the object in the positionnal engine (allow getInRadius query)
		/// NOTA: to be visible in the world a object must be registered !!!
		/// </summary>
		/// <returns>true if object has been added correctly</returns>
		public virtual bool AddToWorld()
		{
			bool result = false;
			if(m_region != null) result = m_region.AddObject(this);

			if(result == true)
			{
				Notify(GeometryEngineNodeEvent.AddToWorld, this);
				BroadcastCreate();
			}
			return result;
		}

		/// <summary>
		/// Unregister this object from the geometry engine
		/// The object will be no longer visible in the world,
		/// and its ObjectID will be -1
		/// </summary>
		public virtual bool RemoveFromWorld()
		{
			if(m_region == null) return false;
			
			Notify(GeometryEngineNodeEvent.RemoveFromWorld, this);

			BroadcastRemove();	

			m_objectInRadiusCache.Clear();
			return m_region.RemoveObject(this);
		}

		/// <summary>
		/// Moves the object from one spot to another spot, possible even
		/// over region boundaries.
		/// </summary>
		/// <param name="targetRegion">the target region</param>
		/// <param name="newPosition">The new position.</param>
		/// <param name="heading">new heading</param>
		/// <returns>true if moved</returns>
		public virtual bool MoveTo(Region targetRegion, Point newPosition, ushort heading)
		{
			Notify(GeometryEngineNodeEvent.MoveTo, this, new MoveToEventArgs(targetRegion, newPosition, heading));

			if(targetRegion != m_region)
			{
				if (!RemoveFromWorld()) return false;
				m_position = newPosition;
				m_heading = heading;
				m_region = targetRegion;
				return AddToWorld(); // assign a new objectID
			}
			
			Zone startingZone = m_region.GetZone(m_position);
			Zone targetZone = targetRegion.GetZone(newPosition);
			if(startingZone != null && targetZone != null)
			{
				SubZone fromSubZone = startingZone.GetSubZone(m_position);
				SubZone toSubZone = targetZone.GetSubZone(newPosition);
			
				if(toSubZone != null && fromSubZone != null && toSubZone != fromSubZone)
				{
					if (log.IsDebugEnabled)
						log.Debug("(Zone "+startingZone.ZoneID+") Remove object from SubZone ("+m_position.X+","+m_position.Y+")");
					startingZone.ObjectExitSubZone(this, fromSubZone);

					if (log.IsDebugEnabled)
						log.Debug("(Zone "+targetZone.ZoneID+") Add object to SubZone ("+newPosition.X+","+newPosition.Y+")");
					targetZone.ObjectEnterSubZone(this, toSubZone);
				}

				BroadcastRemove();

				m_position = newPosition;
				m_heading = heading;

				BroadcastCreate();

				return true;
			}
		
			return false;
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public abstract void BroadcastCreate();

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public abstract void BroadcastRemove();

		#endregion

		#region Geometry engine cache

		/// <summary>
		/// This constant define the valid cache time when alive in a given radius for a given type
		/// </summary>
		private const short UPDATE_RATE_ALIVE = 100;

		/// <summary>
		/// This constant define the valid cache time when dead in a given radius for a given type
		/// </summary>
		private const short UPDATE_RATE_DEAD = 500;

		/// <summary>
		/// This constant define the minumum distance before request fresh datas
		/// </summary>
		private const short MAX_DISTANCE_BEFORE_UPDATE = 45;

		/// <summary>
		/// This custom hashtable hold all getInRadius cached datas
		/// </summary>
		private readonly DOL.GS.Collections.Hashtable m_objectInRadiusCache = new DOL.GS.Collections.Hashtable();

		/// <summary>
		/// Gets all object of a given type close to this object inside a certain radius
		/// </summary>
		/// <param name="typeToCheck">the object type to request</param>
		/// <param name="radiusToCheck">the radius to check</param>
		/// <returns>A DynamicList</returns>
		public DynamicList GetInRadius(Type typeToCheck, ushort radiusToCheck)
		{
			if(ObjectState == eObjectState.Active)
			{
				string requestKey = String.Concat(radiusToCheck, typeToCheck);
				OIRElement curElem = (OIRElement)m_objectInRadiusCache[requestKey];
			
				// if nothing found create and set a new OIRElement
				if(curElem == null) 
				{
					curElem = new OIRElement();
					m_objectInRadiusCache[requestKey] = curElem;
				}

				Point currentPosition = Position;
				Point curElemRequestCenter = curElem.RequestCenter;
				uint currentTick = (uint) Environment.TickCount;
				short updateDelay = UPDATE_RATE_ALIVE;
				
				if(this is GameLivingBase && !((GameLivingBase)this).Alive) updateDelay = UPDATE_RATE_DEAD;
				
				if ((currentTick - curElem.DataCacheTick) >= updateDelay // its a long time we didn't update
				|| FastMath.Abs(curElemRequestCenter.X - currentPosition.X) > MAX_DISTANCE_BEFORE_UPDATE
				|| FastMath.Abs(curElemRequestCenter.Y - currentPosition.Y) > MAX_DISTANCE_BEFORE_UPDATE) // we have moved too much
				{
					// here we request a new DynamicList to the geometry engine
					curElem.CacheData = m_region.GetInRadius(typeToCheck, currentPosition, radiusToCheck);
					curElem.DataCacheTick = currentTick;
					curElem.RequestCenter = currentPosition;
				}
				else
				{
					// here we use the cache
					curElem.CacheData.Reset();
				}

				return curElem.CacheData;
			}
			return new DynamicList(0);
		}

		/// <summary>
		/// The class constraining the cached data structure
		/// </summary>
		private sealed class OIRElement
		{
			private uint m_dataCacheTick; 

			public uint DataCacheTick
			{
				get { return m_dataCacheTick; }
				set { m_dataCacheTick = value; }
			}

			private Point m_requestCenter = Point.Zero;

			public Point RequestCenter
			{
				get { return m_requestCenter; }
				set { m_requestCenter = value; }
			}
            
			private DynamicList m_cacheData;

			public DynamicList CacheData
			{
				get { return m_cacheData; }
				set { m_cacheData = value; }
			}
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

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" ObjectID=").Append(ObjectID)
				.Append(" state=").Append(ObjectState)
				.Append(" reg=").Append(m_region == null ? "null" : m_region.RegionID.ToString())
				.Append(" heading=").Append(m_heading)
				.Append(" loc=").Append(Position.ToString())
				.ToString();
		}
	}
}
