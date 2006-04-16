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
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class holds all information that
	/// EVERY MovingGameStaticItem in the game world needs!
	/// MovingGameStaticItem are ambient rat, butterfly ... (all moving non targetable ambient objects)
	/// </summary>
	public class MovingGameStaticItem : PersistantGameObject, IMovingGameObject
	{
		#region Constant
		/// <summary>
		/// The max distance where the object will move to the spanw point
		/// </summary>
		public const int MAX_WANDER_DISTANCE = 100;

		/// <summary>
		/// The movment speed of the object
		/// </summary>
		public const int WANDER_SPEED = 200;

		/// <summary>
		/// The mx wait time between two movment
		/// </summary>
		public const int MAX_WANDER_WAIT_TIME = 10000;
		#endregion

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
				if(IsMoving) return m_movementAction.TargetPosition;
				return Point.Zero;
			}
			set
			{
				if(IsMoving)
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
				if(IsMoving) return m_movementAction.Position;
				return base.Position;
			}
			set
			{
				if(ObjectState == eObjectState.Active)
				{
					if(IsMoving) return;
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
				if(IsMoving) return m_movementAction.Heading;
				return base.Heading;
			}
			set
			{
				if(IsMoving) return;
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
				if(IsMoving) return m_movementAction.CurrentSpeed;
				return 0;
			}
			set
			{
				if(IsMoving) m_movementAction.CurrentSpeed = value;
			}
		}

		/// <summary>
		/// Returns if the object is moving or not
		/// </summary>
		public virtual bool IsMoving
		{
			get { return m_movementAction != null; }
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
			StopMoving();

			m_movementAction = new MovementAction(this, walkTarget, speed);
			BroadcastUpdate(); // broadcast update
		}

		/// <summary>
		/// Stops the movement of the object
		/// </summary>
		public virtual void StopMoving()
		{
			if(IsMoving) m_movementAction.StopMoving();
		}
		#endregion

		#region SpawnPosition / SpawnHeading / Wander

		/// <summary>
		/// Spawn position.
		/// </summary>
		protected Point m_spawnPosition;

		/// <summary>
		/// Spawn Heading
		/// </summary>
		protected ushort m_spawnHeading;

		/// <summary>
		/// Gets or sets the spawnposition of this npc
		/// </summary>
		public Point SpawnPosition
		{
			get { return m_spawnPosition; }
			set { m_spawnPosition = value; }
		}

		/// <summary>
		/// Gets or sets the spawnheading of this npc
		/// </summary>
		public virtual ushort SpawnHeading
		{
			get { return m_spawnHeading; }
			set { m_spawnHeading = (ushort) (value&0xFFF); }
		}

		/// <summary>
		/// The timer that will be started each this object will need to move
		/// </summary>
		protected StartWanderAction m_wanderAction;

		/// <summary>
		/// The timed pray action
		/// </summary>
		protected class StartWanderAction : RegionAction
		{
			/// <summary>
			/// Constructs a new pray action
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public StartWanderAction(MovingGameStaticItem actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// Callback method for the wander movment
			/// </summary>
			protected override void OnTick()
			{
				MovingGameStaticItem obj = (MovingGameStaticItem)m_actionSource;
				int range = Util.Random(MAX_WANDER_DISTANCE / 2, MAX_WANDER_DISTANCE);
				double angle = Util.RandomDouble() * 2 * Math.PI;
				obj.WalkTo(new Point(obj.SpawnPosition.X + (int)(range * Math.Cos(angle)), obj.SpawnPosition.Y + (int)(range * Math.Sin(angle)), 0), Util.Random(WANDER_SPEED/2, WANDER_SPEED));
			}
		}

		/// <summary>
		/// Called each time the steed arrive to a new route point
		/// </summary>
		public void OnObjectAtTarget(DOLEvent e, object o, EventArgs args)
		{
			MovingGameStaticItem obj = o as MovingGameStaticItem;
			if(obj != null)
			{
				m_wanderAction.Start(Util.Random(2000, MAX_WANDER_WAIT_TIME));
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

		#region BroadcastCreate / BroadcastRemove / BroadcastUpdate

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendNPCCreate(this);
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
		#endregion

		#region AddToWorld / RemoveFromWorld
		/// <summary>
		/// Creates the item in the world
		/// </summary>
		/// <returns>true if object was created</returns>
		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;

			m_spawnPosition = Position;
			m_spawnHeading = (ushort)Heading;
			
			m_wanderAction = new StartWanderAction(this);
			m_wanderAction.Start(MAX_WANDER_WAIT_TIME);
			GameEventMgr.AddHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnObjectAtTarget));
			
			return true;
		}

		/// <summary>
		/// Removes the item from the world
		/// </summary>
		public override bool RemoveFromWorld()
		{
			StopMoving();

			if (!base.RemoveFromWorld()) return false;
			
			GameEventMgr.RemoveHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnObjectAtTarget));
			if(m_wanderAction != null)
			{
				m_wanderAction.Stop();
				m_wanderAction = null;
			}

			return true;
		}
		#endregion
	}
}
