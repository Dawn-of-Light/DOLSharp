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
using DOL.Events;
using DOL.GS.Database;
using DOL.GS.Movement;

namespace DOL.GS
{
	/// <summary>
	/// GameDoor is class for regular door
	/// </summary>		
	public class GameSteed : GameObject, IMovingGameObject
	{
		#region Movement
		
		/// <summary>
		/// The movment action of this object
		/// </summary>
		private MovementAction m_movementAction;

		/// <summary>
		/// Gets the movement action of this object
		/// Never use it yourself !!!!
		/// </summary>
		public MovementAction MovementAction
		{
			get { return m_movementAction; }
			set { m_movementAction = value; }
		}

		/// <summary>
		/// Gets the movement target position.
		/// </summary>
		public Point TargetPosition
		{
			get
			{
				if(m_movementAction != null) return m_movementAction.TargetPosition;
				return Point.Zero;
			}
			set
			{
				if(m_movementAction != null)
				{
					if(m_region.GetZone(value) == null) return; // the target point is not valid in the current region
					m_movementAction.TargetPosition = value;
				}
			}
		}

		/// <summary>
		/// Gets the current position of this living. Don't modify this property
		/// to try to change position of the mob while active. Use the
		/// MoveTo function instead
		/// </summary>
		public override Point Position
		{
			get
			{
				if(m_movementAction != null) return m_movementAction.Position;
				return base.Position;
			}
			set
			{
				if(ObjectState == eObjectState.Active)
				{
					if(m_movementAction != null) return;
					m_position = value; // used by MovementAction at the end of the movment !
				}
				else
				{
					base.Position = value;
				}
			}
		}

		/// <summary>
		/// Gets the current direction the Object is facing
		/// </summary>
		public override int Heading
		{
			get
			{
				if(m_movementAction != null) return m_movementAction.Heading;
				return base.Heading;
			}
			set
			{
				if(m_movementAction != null) return;
				base.Heading = value;
			}
		}

		/// <summary>
		/// Gets or sets the current speed of this object
		/// </summary>
		public virtual int CurrentSpeed
		{
			get
			{
				if(m_movementAction != null) return m_movementAction.CurrentSpeed;
				return 0;
			}
			set
			{
				if(m_movementAction != null) m_movementAction.CurrentSpeed = value;
			}
		}

		/// <summary>
		/// This function is used to make the object move towards
		/// a certain target spot within this region. The target
		/// spot should be in the same or an adjacent Zone of the
		/// object
		/// </summary>
		/// <param name="walkTarget">target point</param>
		/// <param name="speed">walk speed</param>
		public virtual void WalkTo(Point walkTarget, int speed)
		{
			if(m_movementAction != null) m_movementAction.StopMoving();

			m_movementAction = new MovementAction(this, walkTarget, speed);
			BroadcastUpdate(); // broadcast update
		}

		/// <summary>
		/// Stops the movement of the object
		/// </summary>
		public virtual void StopMoving()
		{
			if(m_movementAction != null) m_movementAction.StopMoving();
		}
		#endregion

		#region Path point movment
		/// <summary>
		/// Run until the end of the horse route
		/// </summary>
		public virtual void MoveOnPath(PathPoint nextPoint)
		{
			Notify(GameSteedEvent.PathMoveStarts, this);
			m_nextPoint = nextPoint;
			new SteedRideAction(this).Start(4000); // don't start to run instantly
			GameEventMgr.AddHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnSteedAtNextPoint));
		}

		/// <summary>
		/// Handles delayed horse ride actions
		/// </summary>
		protected class SteedRideAction : RegionAction
		{
			/// <summary>
			/// Constructs a new HorseStartAction
			/// </summary>
			/// <param name="actionSource"></param>
			public SteedRideAction(GameSteed actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{		
				GameSteed steed = (GameSteed)m_actionSource;
				steed.Notify(MovingGameObjectEvent.ArriveAtTarget, steed); // really start to run now
			}
		}

		/// <summary>
		/// Store the next point in the path
		/// </summary>
		private PathPoint m_nextPoint;

		/// <summary>
		/// Called each time the steed arrive to a new route point
		/// </summary>
		public void OnSteedAtNextPoint(DOLEvent e, object o, EventArgs args)
		{
			if(m_nextPoint != null)
			{
				WalkTo(m_nextPoint.Position, m_nextPoint.Speed);
				m_nextPoint = m_nextPoint.NextPoint;
			}
			else
			{
				GameEventMgr.RemoveHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnSteedAtNextPoint));
				Notify(GameSteedEvent.PathMoveEnds, this);
			}
		}

		#endregion

		#region Level / Size
		/// <summary>
		/// The level of the Object
		/// </summary>
		protected byte m_Level;

		/// <summary>
		/// Gets or Sets the current level of the Object
		/// </summary>
		public virtual byte Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		/// <summary>
		/// Holds the size of the NPC
		/// </summary>
		protected byte m_size;

		/// <summary>
		/// Gets or sets the size of the npc
		/// </summary>
		public byte Size
		{
			get { return m_size; }
			set { m_size = value; }
		}
		#endregion

		#region Broadcats create / remove / update
		/// <summary>
		/// The last time this NPC sent the 0x09 update packet
		/// </summary>
		protected volatile uint  m_lastUpdateTickCount = uint.MinValue;
		
		/// <summary>
		/// Gets the last time this mob was updated
		/// </summary>
		public uint LastUpdateTickCount
		{
			get { return m_lastUpdateTickCount; }
		}

		/// <summary>
		/// Broadcasts the npc to all players around
		/// </summary>
		public virtual void BroadcastUpdate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendNPCUpdate(this);
				player.CurrentUpdateArray[ObjectID-1]=true;
			}
			m_lastUpdateTickCount=(uint)Environment.TickCount;
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendNPCCreate(this);
			}
		}

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendRemoveObject(this, eRemoveType.Disappear);
		}

		/// <summary>
		/// Removes the npc from the world
		/// </summary>
		/// <returns>true if the npc has been successfully removed</returns>
		public override bool RemoveFromWorld()
		{
			StopMoving();
			if (!base.RemoveFromWorld()) return false;
			
			return true;
		}
		#endregion

		#region Rider
		/// <summary>
		/// Holds the rider as weak reference
		/// </summary>
		protected WeakReference	m_rider = new WeakRef(null);

		/// <summary>
		/// Gets the rider of this mob
		/// </summary>
		public GamePlayer Rider
		{
			get { return m_rider.Target as GamePlayer; }
			set { m_rider.Target = value; }
		}
		#endregion
	}
}