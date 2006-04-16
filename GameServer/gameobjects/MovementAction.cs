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
using DOL.Events;
using DOL.GS.Utils;

namespace DOL.GS
{
	/// <summary>
	/// Description résumée de MovmentAction.
	/// </summary>
	public class MovementAction
	{
		/// <summary>
		/// The current speed of this object
		/// </summary>
		protected int m_currentSpeed;

		/// <summary>
		/// Gets / set the current speed of this object.
		/// </summary>
		public int CurrentSpeed
		{
			get { return m_currentSpeed; }
			set
			{
				if(m_currentSpeed != value)
				{
					m_currentSpeed = value;
					if(m_currentSpeed <= 0) // can be mezz / stun ect ...
					{
						StopMoving();
					}
					else
					{
						owner.Notify(MovingGameObjectEvent.WalkTo, owner, new WalkToEventArgs(m_targetPosition, m_currentSpeed));
			
						PauseMoving();
						RecalculatePostionAddition();
						UpdateInternalTimersAndStartMoving();

						owner.BroadcastUpdate(); // broadcast update
					}
				}
			}
		}

		/// <summary>
		/// The heading direction to move to
		/// </summary>
		protected int m_heading;

		/// <summary>
		/// Gets the current heading of this object.
		/// </summary>
		public int Heading
		{
			get { return m_heading; }
		}

		/// <summary>
		/// The object's position cache.
		/// </summary>
		protected Point m_positionCache;
		
		/// <summary>
		/// The position cache time stamp.
		/// </summary>
		protected uint m_positionCacheTick = 0;

		/// <summary>
		/// Starting position to walk from.
		/// </summary>
		protected Point m_startPosition;

		/// <summary>
		/// Gets the current position of this object.
		/// </summary>
		public Point Position
		{
			get
			{
				if(m_movementStartTick > 0)
				{
					uint tick = (uint)Environment.TickCount;
					if (tick == m_positionCacheTick) return m_positionCache;
					m_positionCacheTick = tick;

					double timeSinceMoved = tick - m_movementStartTick;
					m_positionCache.X = m_startPosition.X + (int) (timeSinceMoved*m_xAddition);
					m_positionCache.Y = m_startPosition.Y + (int) (timeSinceMoved*m_yAddition);
					m_positionCache.Z = m_startPosition.Z + (int) (timeSinceMoved*m_zAddition);
					return m_positionCache;
				}
				else
				{
					return m_startPosition;
				}
			}
		}

		/// <summary>
		/// Target position to walk to.
		/// </summary>
		protected Point m_targetPosition;

		/// <summary>
		/// Gets the target position of this object.
		/// </summary>
		public Point TargetPosition
		{
			get { return m_targetPosition; }
			set
			{
				if(m_targetPosition != value)
				{
					m_targetPosition = value;
					m_heading = m_startPosition.GetHeadingTo(m_targetPosition);

					owner.Notify(MovingGameObjectEvent.WalkTo, owner, new WalkToEventArgs(m_targetPosition, m_currentSpeed));
			
					PauseMoving();
					RecalculatePostionAddition();
					UpdateInternalTimersAndStartMoving();

					owner.BroadcastUpdate(); // broadcast update
				}
			}
		}

		/// <summary>
		/// The X addition per coordinate of forward movement
		/// </summary>
		protected double m_xAddition;
		
		/// <summary>
		/// The Y addition per coordinate of forward movement
		/// </summary>
		protected double m_yAddition;
		
		/// <summary>
		/// The Z addition per coordinate of forward movement
		/// </summary>
		protected double m_zAddition;

		/// <summary>
		/// Holds when the movement started
		/// </summary>
		protected uint  m_movementStartTick = 0;

		/// <summary>
		/// Timer fired each time the object arrive to a next step
		/// (each time it change of subZone and when it arrive at target)
		/// </summary>
		protected ArriveAtNextMovmentStepAction m_arriveAtNextMovmentStepAction;

		/// <summary>
		/// The object to move
		/// </summary>
		protected IMovingGameObject owner;

		/// <summary>
		/// Consructor
		/// </summary>
		public MovementAction(IMovingGameObject obj, Point targetPosition, int speed)
		{
			if(speed <= 0) return; // can be mezz / stun ect ...
			if(obj.Region.GetZone(targetPosition) == null) return; // the target point is not valid in the current region
			
			owner = obj;
			m_startPosition = obj.Position;
			m_targetPosition = targetPosition;
			m_currentSpeed = speed;
			m_heading = m_startPosition.GetHeadingTo(m_targetPosition);

			owner.Notify(MovingGameObjectEvent.WalkTo, owner, new WalkToEventArgs(m_targetPosition, m_currentSpeed));
			
			RecalculatePostionAddition();
			UpdateInternalTimersAndStartMoving();
		}

		/// <summary>
		/// Delayed action that fires an event when an NPC arrives at its next movment step
		/// </summary>
		protected class ArriveAtNextMovmentStepAction : RegionAction
		{
			/// <summary>
			/// Is it the final step ?
			/// </summary>
			protected readonly bool m_finalStep;

			/// <summary>
			/// Constructs a new ArriveAtTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public ArriveAtNextMovmentStepAction(IMovingGameObject actionSource, bool finalStep) : base((GeometryEngineNode)actionSource)
			{
				m_finalStep = finalStep;
			}

			/// <summary>
			/// This function is called when the Mob arrives at its target spot
			/// It fires the ArriveAtTarget event
			/// </summary>
			protected override void OnTick()
			{
				IMovingGameObject obj = (IMovingGameObject)m_actionSource;
				
				if(m_finalStep)
				{
					obj.MovementAction.StopMovingAtTarget();
				}
				else
				{
					obj.MovementAction.UpdatePosition();
				}
			}
		}

		/// <summary>
		/// Update the position of the object in the geometry engine
		/// This method is called when a object walk from a subZone to another one
		/// </summary>
		public virtual void UpdatePosition()
		{
			Region currentRegion = owner.Region;
			Zone startingZone = currentRegion.GetZone(m_startPosition);
			Zone targetZone = currentRegion.GetZone(Position);

			if(startingZone != null && targetZone != null)
			{
				SubZone fromSubZone = startingZone.GetSubZone(m_startPosition);
				SubZone toSubZone = targetZone.GetSubZone(Position);
				
				if(toSubZone != null && fromSubZone != null && toSubZone != fromSubZone)
				{
					System.Console.WriteLine("(Zone "+startingZone.ZoneID+") Remove object from SubZone ("+m_startPosition.X+","+m_startPosition.Y+")");
					startingZone.ObjectExitSubZone((GeometryEngineNode)owner, fromSubZone);

					System.Console.WriteLine("(Zone "+targetZone.ZoneID+") Add object to SubZone ("+Position.X+","+Position.Y+")");
					targetZone.ObjectEnterSubZone((GeometryEngineNode)owner, toSubZone);
				}
			}

			PauseMoving();
			UpdateInternalTimersAndStartMoving();
		}

		/// <summary>
		/// Stops the movement of the object
		/// </summary>
		public virtual void StopMoving()
		{
			PauseMoving();
			owner.MovementAction = null;
			owner.Position = m_startPosition;
			owner.Heading = m_heading;
		}

		/// <summary>
		/// Stops the movement of the object when it is at its targetPos
		/// </summary>
		public virtual void StopMovingAtTarget()
		{
			owner.MovementAction = null;
			owner.Position = m_targetPosition;
			owner.Heading = m_heading;

			owner.Notify(MovingGameObjectEvent.ArriveAtTarget, owner);	
		}

		#region private methods

		/// <summary>
		/// Pause the movement in order to update all internal datas
		/// </summary>
		private void PauseMoving()
		{
			if(m_arriveAtNextMovmentStepAction != null)
			{
				m_arriveAtNextMovmentStepAction.Stop();
				m_arriveAtNextMovmentStepAction = null;
			}

			m_startPosition = Position;
			m_movementStartTick = 0; // stop moving
		}

		/// <summary>
		/// Recalculates position addition values of this living
		/// </summary>
		private void RecalculatePostionAddition()
		{
			double angle = m_heading / Point.HEADING_CONST;
			m_xAddition = -Math.Sin(angle) * 0.001 * m_currentSpeed;
			m_yAddition = Math.Cos(angle) * 0.001 * m_currentSpeed;
			m_zAddition = 0f;
		}

		/// <summary>
		/// Update all internal data of this object and restart moving
		/// </summary>
		private void UpdateInternalTimersAndStartMoving()
		{
			int startingSubZoneXIndex = m_startPosition.X / Zone.SUBZONE_SIZE;
			int startingSubZoneYIndex = m_startPosition.Y / Zone.SUBZONE_SIZE;
				
			int targetSubZoneXIndex = m_targetPosition.X / Zone.SUBZONE_SIZE;
			int targetSubZoneYIndex = m_targetPosition.Y / Zone.SUBZONE_SIZE;

			if(startingSubZoneXIndex == targetSubZoneXIndex && startingSubZoneYIndex == targetSubZoneYIndex)
			{
				// the target in the same subZone
				int timeX = (int)Math.Ceiling(FastMath.Abs((m_targetPosition.X - m_startPosition.X) / m_xAddition));
				int timeY = (int)Math.Ceiling(FastMath.Abs((m_targetPosition.Y - m_startPosition.Y) / m_yAddition));
			
				int timeToTarget = Math.Max(timeX, timeY);
				if(timeToTarget < 1) timeToTarget = 1;
			
				m_arriveAtNextMovmentStepAction = new ArriveAtNextMovmentStepAction(owner, true);
				m_arriveAtNextMovmentStepAction.Start(timeToTarget);
			}
			else if(startingSubZoneXIndex == targetSubZoneXIndex)
			{
				// moving nord or sud
				int targetY = startingSubZoneYIndex < targetSubZoneYIndex ? (1 + startingSubZoneYIndex) * Zone.SUBZONE_SIZE + 1 : startingSubZoneYIndex * Zone.SUBZONE_SIZE - 1;
				int timeToNextSubZone = (int)Math.Ceiling(FastMath.Abs((targetY - m_startPosition.Y) / m_yAddition));

				m_arriveAtNextMovmentStepAction = new ArriveAtNextMovmentStepAction(owner, false);
				m_arriveAtNextMovmentStepAction.Start(timeToNextSubZone);
				
			}
			else if(startingSubZoneYIndex == targetSubZoneYIndex)
			{
				// moving est or ouest
				int targetX = startingSubZoneXIndex < targetSubZoneXIndex ? (1 + startingSubZoneXIndex) * Zone.SUBZONE_SIZE + 1 : startingSubZoneXIndex * Zone.SUBZONE_SIZE - 1;
				int timeToNextSubZone = (int)Math.Ceiling(FastMath.Abs((targetX - m_startPosition.X) / m_xAddition));
			
				m_arriveAtNextMovmentStepAction = new ArriveAtNextMovmentStepAction(owner, false);
				m_arriveAtNextMovmentStepAction.Start(timeToNextSubZone);
			}
			else
			{
				int timeToNextSubZoneY;
				int timeToNextSubZoneX;

				if(m_startPosition.X < m_targetPosition.X)
				{
					if(m_startPosition.Y > m_targetPosition.Y)
					{
						// moving nord east
						int targetY = startingSubZoneYIndex * Zone.SUBZONE_SIZE - 1;
						timeToNextSubZoneY = (int)Math.Ceiling(FastMath.Abs((targetY - m_startPosition.Y) / m_yAddition));
			
						int targetX = (1 + startingSubZoneXIndex) * Zone.SUBZONE_SIZE + 1;
						timeToNextSubZoneX = (int)Math.Ceiling(FastMath.Abs((targetX - m_startPosition.X) / m_xAddition));
					}
					else
					{
						// moving sud east
						int targetY = (1 + startingSubZoneYIndex) * Zone.SUBZONE_SIZE + 1;
						timeToNextSubZoneY = (int)Math.Ceiling(FastMath.Abs((targetY - m_startPosition.Y) / m_yAddition));
			
						int targetX = (1 + startingSubZoneXIndex) * Zone.SUBZONE_SIZE + 1;
						timeToNextSubZoneX = (int)Math.Ceiling(FastMath.Abs((targetX - m_startPosition.X) / m_xAddition));
					}
				}
				else 
				{
					if(m_startPosition.Y > m_targetPosition.Y)
					{
						// moving nord ouest
						int targetY = startingSubZoneYIndex * Zone.SUBZONE_SIZE - 1;
						timeToNextSubZoneY = (int)Math.Ceiling(FastMath.Abs((targetY - m_startPosition.Y) / m_yAddition));
		
						int targetX = startingSubZoneXIndex * Zone.SUBZONE_SIZE - 1;
						timeToNextSubZoneX = (int)Math.Ceiling(FastMath.Abs((targetX - m_startPosition.X) / m_xAddition));
					}
					else
					{
						// moving sud ouest
						int targetY = (1 + startingSubZoneYIndex) * Zone.SUBZONE_SIZE + 1;
						timeToNextSubZoneY = (int)Math.Ceiling(FastMath.Abs((targetY - m_startPosition.Y) / m_yAddition));
		
						int targetX = startingSubZoneXIndex * Zone.SUBZONE_SIZE - 1;
						timeToNextSubZoneX = (int)Math.Ceiling(FastMath.Abs((targetX - m_startPosition.X) / m_xAddition));
					}
				}

				m_arriveAtNextMovmentStepAction = new ArriveAtNextMovmentStepAction(owner, false);
				m_arriveAtNextMovmentStepAction.Start(Math.Min(timeToNextSubZoneY, timeToNextSubZoneX));	
			}

			m_movementStartTick = (uint)Environment.TickCount; // start moving
		}

		#endregion
	}
}
