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
using System.Reflection;
using System.Collections;
using System.Threading;
using DOL.GS;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.Movement;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Utils;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class is the baseclass for all Non Player Characters like
	/// Monsters, Merchants, Guards, Steeds ...
	/// </summary>
	public abstract class GameNPC : GameLiving
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Sizes/Properties/template
		
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
			set
			{
				m_size = value;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if(m_inventory!=null)
							player.Out.SendLivingEquipementUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets or sets the model of this npc
		/// </summary>
		public override int Model
		{
			get { return base.Model; }
			set
			{
				base.Model = value;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendModelChange(this,(ushort)Model);
				}
			}
		}

		/// <summary>
		/// Gets or sets the heading of this NPC
		/// </summary>
		public override int Heading
		{
			get { return base.Heading; }
			set
			{
				if (IsTurningDisabled)
					return;
				base.Heading=value;
				BroadcastUpdate();
			}
		}
		
		/// <summary>
		/// Gets or sets the level of this NPC
		/// </summary>
		public override byte Level
		{
			get { return base.Level; }
			set
			{
				base.Level = value;
				//MaxHealth = (ushort)(value * 20 + 20);	// MaxHealth depends from mob level
				m_health = MaxHealth;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if(m_inventory!=null)
							player.Out.SendLivingEquipementUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets or Sets the effective level of the Object
		/// </summary>
		public override int EffectiveLevel
		{
			get
			{
				IControlledBrain brain = Brain as IControlledBrain;
				if (brain != null)
					return brain.Owner.Level;
				return base.Level;
			}
		}

		/// <summary>
		/// Gets or sets the Realm of this NPC
		/// </summary>
		public override byte Realm
		{
			get
			{
				IControlledBrain brain = Brain as IControlledBrain;
				if (brain != null)
					return brain.Owner.Realm; // always realm of the owner
				return base.Realm;
			}
			set
			{
				base.Realm = value;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if(m_inventory!=null)
							player.Out.SendLivingEquipementUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets or sets the name of this npc
		/// </summary>
		public override string Name
		{
			get { return base.Name; }
			set
			{
				base.Name = value;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if(m_inventory!=null)
							player.Out.SendLivingEquipementUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets or sets the guild name
		/// </summary>
		public override string GuildName
		{
			get { return base.GuildName; }
			set
			{
				base.GuildName = value;
				if(ObjectState==eObjectState.Active)
				{
					foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if(m_inventory!=null)
							player.Out.SendLivingEquipementUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

        /// <summary>
        /// mob faction
        /// </summary>
        protected  Faction m_faction;

		/// <summary>
		/// Gets the Faction of the NPC
		/// </summary>
		public Faction Faction
		{
			get { return m_faction; }
            set { m_faction = value; }
		}

		#endregion
		#region Flags/Position/SpawnPosition/UpdateTick
		/// <summary>
		/// Various flags for this npc
		/// </summary>
		[Flags]
		public enum eFlags : byte
		{
			/// <summary>
			/// The npc is a ghost, see through, transparent
			/// </summary>
			GHOST			= 0x01,
			/// <summary>
			/// The npc is stealth (new since 1.71)
			/// </summary>
			STEALTH			= 0x02,
			/// <summary>
			/// The npc doesn't show a name above its head but can be targeted
			/// </summary>
			DONTSHOWNAME	= 0x04,
			/// <summary>
			/// The npc doesn't show a name above its head and can't be targeted
			/// </summary>
			CANTTARGET		= 0x08,
			/// <summary>
			/// Not in nearest enemyes if different vs player realm, but can be targeted if model support this
			/// </summary>
			PEACE			= 0x10,
			/// <summary>
			/// The npc is flying (z above ground permitted)
			/// </summary>
			FLYING			= 0x20,
		}
		/// <summary>
		/// Spawn position.
		/// </summary>
		protected Point m_spawnPosition;

		/// <summary>
		/// Spawn Heading
		/// </summary>
		protected ushort m_spawnHeading;
		/// <summary>
		/// Holds various flags of this npc
		/// </summary>
		protected byte m_flags;
		/// <summary>
		/// The last time this NPC sent the 0x09 update packet
		/// </summary>
		protected volatile uint  m_lastUpdateTickCount = uint.MinValue;
		/// <summary>
		/// The last time this NPC was actually updated to at least one player
		/// </summary>
		protected volatile uint  m_lastVisibleToPlayerTick = uint.MinValue;
		/// <summary>
		/// Gets or Sets the flags of this npc
		/// </summary>
		public virtual byte Flags
		{
			get { return m_flags; }
			set
			{
				byte oldflags = m_flags;
				m_flags = value;
				if(ObjectState == eObjectState.Active)
				{
					bool ghostChanged = (oldflags & (byte)eFlags.GHOST) != (value & (byte)eFlags.GHOST);
					bool cantTargetChanged = (oldflags & (byte)eFlags.CANTTARGET) != (value & (byte)eFlags.CANTTARGET);
					bool dontShowNameChanged = (oldflags & (byte)eFlags.DONTSHOWNAME) != (value & (byte)eFlags.DONTSHOWNAME);
					bool flyingChanged = (oldflags & (byte)eFlags.FLYING) != (value & (byte)eFlags.FLYING);

					if(ghostChanged || cantTargetChanged || dontShowNameChanged || flyingChanged)
					{
						foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendNPCCreate(this);
							if(m_inventory != null)
								player.Out.SendLivingEquipementUpdate(this);
						}
					}
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Shows wether any player sees that mob
		/// we dont need to calculate things like AI if mob is in no way
		/// visible to at least one player
		/// </summary>
		public virtual bool IsVisibleToPlayers
		{
			get { return (uint)Environment.TickCount - m_lastVisibleToPlayerTick < 60000; } 
		}

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
		/// Gets or sets the current speed of the npc
		/// </summary>
		public override int CurrentSpeed
		{
			set
			{
				//Update the position before changing speed!
				m_position = Position;
				base.CurrentSpeed = value;
				BroadcastUpdate();
			}
		}

		/// <summary>
		/// Gets sets the currentwaypoint that npc has to wander to
		/// </summary>
		public PathPoint CurrentWayPoint
		{
			get { return m_currentWayPoint; }
			set { m_currentWayPoint = value; }
		}
		/// <summary>
		/// Stores the currentwaypoint that npc has to wander to
		/// </summary>
		protected PathPoint m_currentWayPoint = null;

		/// <summary>
		/// Gets sets the speed for traveling on path
		/// </summary>
		public int PathingNormalSpeed
		{
			get { return m_pathingNormalSpeed; }
			set { m_pathingNormalSpeed = value; }
		}
		/// <summary>
		/// Stores the speed for traveling on path
		/// </summary>
		protected int m_pathingNormalSpeed;

		/// <summary>
		/// Gets the current position of this living. Don't modify this property
		/// to try to change position of the mob while active. Use the
		/// MoveTo function instead
		/// </summary>
		public override Point Position
		{
			get
			{
				if (m_targetPosition.X == 0 && m_targetPosition.Y == 0 && m_targetPosition.Z == 0)
					return base.Position;
				
				uint timeWalking = (uint)Environment.TickCount - (uint)MovementStartTick;
				if (m_timeToTarget > timeWalking)
					return base.Position;
				
				return m_targetPosition;
			}
			set { base.Position = value; }
		}

		/// <summary>
		/// The stealth state of this NPC
		/// </summary>
		public override bool IsStealthed
		{
			get
			{
				return (Flags & (uint)eFlags.GHOST) != 0;//TODO
			}
		}

		#endregion
		#region Movement
		/// <summary>
		/// Target position to walk to.
		/// </summary>
		protected Point m_targetPosition;
		/// <summary>
		/// Time needed to get to target position, in milliseconds.
		/// </summary>
		protected uint m_timeToTarget;
		/// <summary>
		/// Timer to be set if an OnArriveAtTarget
		/// handler is set before calling the WalkTo function
		/// </summary>
		protected ArriveAtTargetAction m_arriveAtTargetAction;
		/// <summary>
		/// The interval between follow checks, in milliseconds
		/// </summary>
		protected const int FOLLOWCHECKTICKS = 500;
		/// <summary>
		/// Timer to be set if an OnCloseToTarget
		/// handler is set before calling the WalkTo function
		/// </summary>
		protected CloseToTargetAction m_closeToTargetAction;
		/// <summary>
		/// Object that this npc is following as weakreference
		/// </summary>
		protected WeakReference m_followTarget;
		/// <summary>
		/// Max range to keep following
		/// </summary>
		protected int m_followMaxDist;
		/// <summary>
		/// Min range to keep to the target
		/// </summary>
		protected int m_followMinDist;
		/// <summary>
		/// Timer with purpose of follow updating
		/// </summary>
		protected RegionTimer m_followTimer;
		/// <summary>
		/// Property entry on follow timer, wether the follow target is in range
		/// </summary>
		protected static readonly string FOLLOW_TARGET_IN_RANGE = "FollowTargetInRange";

		/// <summary>
		/// Recalculates position addition values of this living
		/// </summary>
		protected override void RecalculatePostionAddition()
		{
			m_positionCacheExpired = true;
			int speed = CurrentSpeed;
			if(speed == 0)
			{
//				log.ErrorFormat("{0} is not moving\n{1}", Name, Environment.StackTrace);
				m_xAddition = m_yAddition = m_zAddition = 0f;
				return;
			}

			if(m_targetPosition.X != 0 || m_targetPosition.Y != 0 || m_targetPosition.Z != 0)
			{
				int diffx = m_targetPosition.X - m_position.X;
				int diffy = m_targetPosition.Y - m_position.Y;
				int diffz = m_targetPosition.Z - m_position.Z;
				long sum = (long) FastMath.Abs(diffx) + FastMath.Abs(diffy) + FastMath.Abs(diffz);
				if (sum <= 0)
				{
					m_xAddition = m_yAddition = m_zAddition = 0f;
				}
				else
				{
					double scaleFactor = 0.001 * speed / sum;
					m_xAddition = (float) (diffx*scaleFactor);
					m_yAddition = (float) (diffy*scaleFactor);
					m_zAddition = (float) (diffz*scaleFactor);
//					log.WarnFormat("{0} is moving to target: sum = {1}, add = ({2:r} {3:r} {4:r}), scale = {5:r}", Name, sum, m_xAddition, m_yAddition, m_zAddition, scaleFactor);
				}
			}
			else
			{
//				log.WarnFormat("{0} is moving but target is 0", Name);
				base.RecalculatePostionAddition();
			}
		}

		/// <summary>
		/// Gets the movement target position.
		/// </summary>
		public Point TargetPosition
		{
			get { return m_targetPosition; }
		}

		/// <summary>
		/// Turns the npc towards a specific object.
		/// </summary>
		/// <param name="obj">Target object to turn to.</param>
		public virtual void TurnTo(GameObject obj) 
		{
			TurnTo(obj.Position);
		}

		/// <summary>
		/// Turns the npc towards a specific point.
		/// </summary>
		/// <param name="point">Target point to turn to.</param>
		public virtual void TurnTo(Point point) 
		{
			if (Stun || Mez) return;
			Notify(GameNPCEvent.TurnTo, this, new TurnToEventArgs(point));
			Heading = Position.GetHeadingTo(point);
		}

		/// <summary>
		/// Turns the npc towards a specific heading
		/// </summary>
		/// <param name="newHeading">the new heading</param>
		public virtual void TurnTo(ushort newHeading)
		{
			if (Stun || Mez) return;
			Notify(GameNPCEvent.TurnToHeading, this, new TurnToHeadingEventArgs(newHeading));
			Heading = newHeading;
		}
		/// <summary>
		/// Turns the NPC towards a specific gameObject
		/// which can be anything ... a player, item, mob, npc ...
		/// and turn back after specified duration
		/// </summary>
		/// <param name="target">GameObject to turn towards</param>
		/// <param name="duration">restore heading after this duration</param>
		public virtual void TurnTo(GameObject target, int duration) 
		{
			if(target==null) return;
			if(target.Region != Region) return;
			// store original heading if not set already
			RestoreHeadingAction restore = (RestoreHeadingAction)TempProperties.getObjectProperty(RESTORE_HEADING_ACTION_PROP, null);
			if (restore == null)
			{
				restore = new RestoreHeadingAction(this);
				TempProperties.setProperty(RESTORE_HEADING_ACTION_PROP, restore);
			}
			TurnTo(target.Position);
			restore.Start(duration);
		}

		/// <summary>
		/// The property used to store the NPC heading restore action
		/// </summary>
		protected const string RESTORE_HEADING_ACTION_PROP = "NpcRestoreHeadingAction";

		/// <summary>
		/// Restores the NPC heading after some time
		/// </summary>
		protected class RestoreHeadingAction : RegionAction
		{
			/// <summary>
			/// The NPCs old heading
			/// </summary>
			protected readonly ushort m_oldHeading;

			/// <summary>
			/// The NPCs old position
			/// </summary>
			protected readonly Point m_oldPosition;

			/// <summary>
			/// Creates a new TurnBackAction
			/// </summary>
			/// <param name="actionSource">The source of action</param>
			public RestoreHeadingAction(GameNPC actionSource) : base(actionSource)
			{
				m_oldHeading = (ushort)actionSource.Heading;
				m_oldPosition = actionSource.Position;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;

				npc.TempProperties.removeProperty(RESTORE_HEADING_ACTION_PROP);

				if(npc.ObjectState != eObjectState.Active) return;
				if(!npc.Alive) return;
				if(npc.AttackState) return;
				if(npc.IsMoving) return;
				if(npc.Position != m_oldPosition) return;
				if(npc.Heading == m_oldHeading) return; // already set?

				npc.TurnTo(m_oldHeading);
			}
		}

		/// <summary>
		/// Gets the last time this mob was updated
		/// </summary>
		public uint LastUpdateTickCount
		{
			get { return m_lastUpdateTickCount; }
		}

		/// <summary>
		/// Gets the last this this NPC was actually update to at least one player.
		/// </summary>
		public uint LastVisibleToPlayersTickCount
		{
			get { return m_lastVisibleToPlayerTick; }
		}

		/// <summary>
		/// Delayed action that fires an event when an NPC arrives at its target
		/// </summary>
		protected class ArriveAtTargetAction : RegionAction
		{
			/// <summary>
			/// Constructs a new ArriveAtTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public ArriveAtTargetAction(GameNPC actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// This function is called when the Mob arrives at its target spot
			/// It fires the ArriveAtTarget event
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;
				npc.m_arriveAtTargetAction = null;
				npc.m_timeToTarget = 0;
				npc.m_currentSpeed = 0;
				npc.m_position = npc.m_targetPosition;
				npc.m_targetPosition = Point.Zero;
				npc.RecalculatePostionAddition();
				npc.Notify(GameNPCEvent.ArriveAtTarget, npc);
			}
		}

		/// <summary>
		/// Delayed action that fires an event when an NPC is 200ms away from its target
		/// </summary>
		protected class CloseToTargetAction : RegionAction
		{
			/// <summary>
			/// Constructs a new CloseToTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public CloseToTargetAction(GameNPC actionSource) : base(actionSource)
			{
			}

			/// <summary>
			/// This function is called when the npc is close to its target
			/// It will fire the CloseToTarget event
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;
				npc.m_closeToTargetAction = null;
				npc.Notify(GameNPCEvent.CloseToTarget, npc);
			}
		}

		/// <summary>
		/// This function is used to make the npc move towards
		/// a certain target spot within this region. The target
		/// spot should be in the same or an adjacent Zone of the
		/// npc
		/// </summary>
		/// <param name="walkTarget">target point</param>
		/// <param name="speed">walk speed</param>
		public virtual void WalkTo(Point walkTarget, int speed)
		{
			if (IsTurningDisabled)
				return; // can't walk when turning is disabled
			//Slow mobs down when they are hurt!
			int maxSpeed = MaxSpeed;
			if (speed > maxSpeed)
				speed = maxSpeed;

			Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(walkTarget, speed));
			//Set our current position
			m_position = Position;
			m_Heading = m_position.GetHeadingTo(walkTarget);
			m_currentSpeed=speed;
			m_targetPosition = walkTarget;
			RecalculatePostionAddition();

			//ARGHL!!!! the following took me 2 days to find out!
			//The mobs in the database have a Z value set ... but normally when
			//we call WalkTo, we set TargetZ to Zero, this results in a HUGE
			//distance bug, makeing the mobs move totally weird!
			//So we have to test if our targetZ == 0
			//duff answer : ever test in get distance so do not need it!
			double dist = m_position.GetDistance(m_targetPosition);

			//DOLConsole.WriteLine("DX="+diffx+" DY="+diffy+" DIST="+dist);
			int timeToTarget = 0;
			if(speed > 0) {
				timeToTarget = (int)(dist*1000/speed);
			}
			if (timeToTarget < 1)
				timeToTarget = 1;
			m_timeToTarget = (uint)timeToTarget;

			if(m_arriveAtTargetAction != null) {
				m_arriveAtTargetAction.Stop();
			}

			m_arriveAtTargetAction = new ArriveAtTargetAction(this);
			m_arriveAtTargetAction.Start(timeToTarget);

			if(m_closeToTargetAction != null) {
				m_closeToTargetAction.Stop();
			}

			m_closeToTargetAction = new CloseToTargetAction(this);
			if(timeToTarget > 200) {
				m_closeToTargetAction.Start(timeToTarget - 200); //200ms before target is close
			} else {
				m_closeToTargetAction.Start(1);
			}

			MovementStartTick = Environment.TickCount;
			BroadcastUpdate();
		}

		/// <summary>
		/// Walk to the spawn point
		/// </summary>
		public virtual void WalkToSpawn()
		{
			StopAttack();
			StopFollow();
//			WalkTo(SpawnX+Random(750)-350, SpawnY+Random(750)-350, SpawnZ, MaxSpeed/3);
			WalkTo(SpawnPosition, (int)(MaxSpeed/2.5));
		}
	
		/// <summary>
		/// This function is used to start the mob walking. It will
		/// walk in the heading direction until the StopMovement function
		/// is called
		/// </summary>
		/// <param name="speed">walk speed</param>
		public virtual void Walk(int speed)
		{
			Notify(GameNPCEvent.Walk, this, new WalkEventArgs(speed));

			if(m_arriveAtTargetAction!=null)
			{
				m_arriveAtTargetAction.Stop();
				m_arriveAtTargetAction=null;
			}
			if(m_closeToTargetAction!=null)
			{
				m_closeToTargetAction.Stop();
				m_closeToTargetAction=null;
			}

			m_position = Position;
			m_targetPosition = Point.Zero;
			m_currentSpeed = speed;
			
			MovementStartTick = Environment.TickCount;
			RecalculatePostionAddition();
			BroadcastUpdate();
		}

		/// <summary>
		/// Gets the NPC current follow target
		/// </summary>
		public GameObject CurrentFollowTarget
		{
			get { return m_followTarget.Target as GameObject; }
		}

		/// <summary>
		/// Stops the movement of the mob
		/// </summary>
		public virtual void StopMoving()
		{
			//if(!IsMoving) return;
			if(m_arriveAtTargetAction!=null)
			{
				m_arriveAtTargetAction.Stop();
				m_arriveAtTargetAction=null;
			}
			if(m_closeToTargetAction!=null)
			{
				m_closeToTargetAction.Stop();
				m_closeToTargetAction=null;
			}
			//This broadcasts an update!
			CurrentSpeed=0;
		}
		/// <summary>
		/// Follow given object
		/// </summary>
		/// <param name="followTarget">target to follow</param>
		/// <param name="minDistance">min distance to keep to the target</param>
		/// <param name="maxDistance">max distance to keep following</param>
		public virtual void Follow(GameObject followTarget, int minDistance, int maxDistance) 
		{
//			string targName = followTarget==null ? "(null)" : followTarget.Name;
//			log.Debug(this.Name+": Follow("+targName+","+minDistance+","+maxDistance+")");
			//First stop the active timer
			if(m_followTimer.IsAlive)
				m_followTimer.Stop();
			if(followTarget==null || followTarget.ObjectState!=eObjectState.Active) return;
			//Set the new values
			m_followMaxDist = maxDistance;
			m_followMinDist = minDistance;
			m_followTarget.Target = followTarget;
			//Start the timer again
			m_followTimer.Start(1);
			//TODO fire event OnFollow
		}

		/// <summary>
		/// Stop following
		/// </summary>
		public virtual void StopFollow() 
		{
			lock(m_followTimer) 
			{
				if(m_followTimer.IsAlive)
					m_followTimer.Stop();
				m_followTarget.Target=null;
				StopMoving();
				//TODO fire event OnStopFollow
			}
		}

		/// <summary>
		/// Will be called if follow mode is active 
		/// and we reached the follow target
		/// </summary>
		public virtual void FollowTargetInRange()
		{
			if (AttackState)
			{
				// if in last attack the enemy was out of range, we can attack him now immediately
				AttackData ad = (AttackData)TempProperties.getObjectProperty(LAST_ATTACK_DATA, null);
				if (ad != null && ad.AttackResult == eAttackResult.OutOfRange)
				{
					m_attackAction.Start(1);// schedule for next tick
				}
			}
		}

		/// <summary>
		/// Keep following a specific object at a max distance
		/// </summary>
		protected virtual int FollowTimerCallback(RegionTimer callingTimer) 
		{
//			log.Debug(this.Name+":FollowTimerCallback("+followTimer+")");
			bool wasInRange = m_followTimer.Properties.getProperty(FOLLOW_TARGET_IN_RANGE, false);
			m_followTimer.Properties.removeProperty(FOLLOW_TARGET_IN_RANGE);

			GameObject followTarget = (GameObject)m_followTarget.Target;
			GameLiving followLiving = followTarget as GameLiving;
			if(followLiving!=null && !followLiving.Alive)
			{
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				return 0;
			}

			//Stop following if we have no target
			if(followTarget==null || followTarget.ObjectState!=eObjectState.Active || Region!=followTarget.Region)
			{
				//DOLConsole.WriteLine("Target not active or region doesn't match ... stop following!");
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				return 0;
			}

			//Calculate the difference between our position and the players position
			Point followPos = followTarget.Position;
			Point myPos = Position;
			double diffx = (long)followPos.X - myPos.X;
			double diffy = (long)followPos.Y - myPos.Y;
			double diffz = (long)followPos.Z - myPos.Z;

			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			double distance = 0;
			if(followPos.Z==0 || myPos.Z==0)
				distance = Math.Sqrt(diffx*diffx + diffy*diffy);
			else
				distance = Math.Sqrt(diffx*diffx + diffy*diffy + diffz*diffz);

//			log.Debug(this.Name+": Follow -> DX="+diffx+" DY="+diffy+" D="+distance+" MXD="+m_followMaxDist+" MID="+m_followMinDist);
			if(distance > m_followMaxDist)
			{
//				log.Debug("Distance>MaxDistance ... stop following!");
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				return 0;
			}

			//Are we in range yet?
			if(distance <= m_followMinDist)
			{
				//StopMoving();
				TurnTo(followPos);
				if (!wasInRange) 
				{
					m_followTimer.Properties.setProperty(FOLLOW_TARGET_IN_RANGE, true);
					FollowTargetInRange();
				}
				return FOLLOWCHECKTICKS;
			}

//			if(followLiving!=null && followLiving.CurrentSpeed>0)
//			{
//				//If the target is moving, directly follow closer
//				diffx=(diffx/distance)*m_followMinDist/4;
//				diffy=(diffy/distance)*m_followMinDist/4;
//				diffz=(diffz/distance)*m_followMinDist/4;
//			}
//			else
//			{
//				//Calculate the offset to the target we will be walking to
//				//Our spot will be mindistance coordinates from the target, 
//				//so we calculate how much x and how much y we need to 
//				//subtract from the player to get the right x and y to walk to 
//				diffx=(diffx/distance)*m_followMinDist/2;
//				diffy=(diffy/distance)*m_followMinDist/2;
//				diffz=(diffz/distance)*m_followMinDist/2;
//			}

			// follow on distance
			diffx=(diffx/distance)*m_followMinDist;
			diffy=(diffy/distance)*m_followMinDist;
			diffz=(diffz/distance)*m_followMinDist;

			//Subtract the offset from the target's position to get
			//our target position
			int newX = (int)(followPos.X - diffx);
			int newY = (int)(followPos.Y - diffy);
			int newZ = (int)(followPos.Z - diffz);
			WalkTo(new Point(newX, newY, newZ), MaxSpeed);
			return FOLLOWCHECKTICKS;
		}

		/// <summary>
		/// Disables the turning for this living
		/// </summary>
		/// <param name="add"></param>
		public override void DisableTurning(bool add)
		{
			bool old = IsTurningDisabled;
			base.DisableTurning(add);
			if (old != IsTurningDisabled)
				BroadcastUpdate();
		}

		#endregion
		#region Inventory/LoadfromDB/ParseParams

		/// <summary>
		/// Loads the equipment template of this npc
		/// </summary>
		/// <param name="equipmentTemplateID">The template id</param>
		public virtual void LoadEquipmentTemplateFromDatabase(string equipmentTemplateID)
		{
			EquipmentTemplateID = equipmentTemplateID;
			if (EquipmentTemplateID != null && EquipmentTemplateID.Length > 0)
			{
				GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
				if (template.LoadFromDatabase(EquipmentTemplateID)) {
					m_inventory = template.CloseTemplate();
				} else {
					if (log.IsDebugEnabled) {
						//log.Warn("Error loading NPC inventory: InventoryID="+EquipmentTemplateID+", NPC name="+Name+".");
					}
				}
				if (Inventory != null)
				{
					// if the two handed slot isnt empty we use that
					// or if the distance slot isnt empty we use that
					if (Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
						SwitchWeapon(eActiveWeaponSlot.TwoHanded);
					else if (Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
						SwitchWeapon(eActiveWeaponSlot.Distance);
					else SwitchWeapon(eActiveWeaponSlot.Standard); // sets visible left and right hand slots
				}
			}			
		}


		/// <summary>
		/// Load a npc from the npc template
		/// </summary>
		/// <param name="obj">template to load from</param>
		public override void LoadFromDatabase(object obj)
		{
			if(!(obj is Mob)) return;
			Mob npc = (Mob) obj;
			InternalID = npc.MobID.ToString();
			Name=npc.Name;
			GuildName=npc.Guild;
			m_position = new Point(npc.X, npc.Y, npc.Z);
			m_Heading=(ushort)(npc.Heading&0xFFF);
			m_maxSpeedBase=npc.Speed;	// TODO db has currntly senseless information here, mob type db required
			if(m_maxSpeedBase == 0)
				m_maxSpeedBase = 191;
			m_currentSpeed=0;
			RegionId=(ushort)npc.Region;
			Realm=npc.Realm;
			Model=(ushort)npc.Model;
			Size=npc.Size;
			Level=npc.Level;	// health changes when GameNPC.Level changes
			Flags=npc.Flags;
			MeleeDamageType = (eDamageType)npc.MeleeDamageType;
			if (MeleeDamageType == 0) 
			{
				MeleeDamageType = eDamageType.Slash;
			}
			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if (aggroBrain != null)
			{
				if (npc.AggroRange==0)
				{
					if (Char.IsLower(Name[0])) 
					{ // make some default on basic mobs
						aggroBrain.AggroRange = 450;
						aggroBrain.AggroLevel = (Level > 3) ? 30 : 0;
					} 
					else if (Realm != 0) 
					{
						aggroBrain.AggroRange = 500;
						aggroBrain.AggroLevel = 60;						
					}
				}
				else
				{
					aggroBrain.AggroLevel=npc.AggroLevel;
					aggroBrain.AggroRange=npc.AggroRange;
				}
			}
			m_activeWeaponSlot=eActiveWeaponSlot.Standard;
			ActiveQuiverSlot=eActiveQuiverSlot.None;
			
			Faction = FactionMgr.GetFactionByID(npc.FactionID);
			LoadEquipmentTemplateFromDatabase(npc.EquipmentTemplateID);
		}
        /// <summary>
        /// parse params
        /// </summary>
        /// <param name="strparams"></param>
        public void ParseParam(string strparams)
        {
            string[] strparam = strparams.Split(',');
            foreach (string str in strparam)
            {
                if (str == null || str == "")
                    continue;
                LoadParam(str);
            }
        }
        /// <summary>
        /// Loads each param.
        /// </summary>
        /// <param name="str">The STR.</param>
        protected virtual void LoadParam(string str)
        {
            str = str.Replace(" ", "");//remove space
            string[] param = str.Split('=');
            switch (param[0])
            {
                default:
                    {
                        if (log.IsErrorEnabled)
                            log.Error("Could not recognize param :" + param[0] + ".");
                    } break;
            }
        }
		/// <summary>
		/// Switches the active weapon to another one
		/// </summary>
		/// <param name="slot">the new eActiveWeaponSlot</param>
		public override void SwitchWeapon(eActiveWeaponSlot slot)
		{
			base.SwitchWeapon(slot);
			if (ObjectState == eObjectState.Active)
			{
				// Update active weapon appearence
				UpdateNPCEquipmentAppearance();
			}
		}
		/// <summary>
		/// Equipment templateID
		/// </summary>
		protected string m_equipmentTemplateID;
		/// <summary>
		/// The equipment template id of this npc
		/// </summary>
		public string EquipmentTemplateID
		{
			get { return m_equipmentTemplateID; }
			set { m_equipmentTemplateID = value; }
		}
		/// <summary>
		/// Updates the items on a character
		/// </summary>
		public void UpdateNPCEquipmentAppearance()
		{
			if(ObjectState==eObjectState.Active)
			{
				foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendLivingEquipementUpdate(this);
			}
		}

		#endregion
		#region Riding
		//NPC's can have riders :-)
		/// <summary>
		/// Holds the rider of this NPC as weak reference
		/// </summary>
		protected WeakReference	m_rider;
		/// <summary>
		/// Gets the rider of this mob
		/// </summary>
		public GamePlayer Rider
		{
			get
			{
				return m_rider.Target as GamePlayer;
			}
		}

		/// <summary>
		/// This function is called when a rider mounts this npc
		/// Since only players can ride NPC's you should use the
		/// GamePlayer.MountSteed function instead to make sure all
		/// callbacks are called correctly
		/// </summary>
		/// <param name="rider">GamePlayer that is the rider</param>
		/// <param name="forced">if true, mounting can't be prevented by handlers</param>
		/// <returns>true if mounted successfully</returns>
		public virtual bool RiderMount(GamePlayer rider, bool forced)
		{
			if(Rider!=null) return false;
			Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider,this));
			m_rider.Target = rider;
			Rider.Steed = this;
			return true;
		}

		/// <summary>
		/// Called to dismount a rider from this npc. 
		/// Since only players can ride NPC's you should use the
		/// GamePlayer.MountSteed function instead to make sure all
		/// callbacks are called correctly
		/// </summary>
		/// <param name="forced">if true, the dismounting can't be prevented by handlers</param>
		/// <returns>true if dismounted successfully</returns>
		public virtual bool RiderDismount(bool forced)
		{
			if(Rider==null) return false;
			Notify(GameNPCEvent.RiderDismount, this, new RiderDismountEventArgs(Rider,this));
			Rider.Steed = null;
			m_rider.Target = null;
			return true;
		}
		#endregion
		#region Add/Remove/Create/Remove/Update
		/// <summary>
		/// Broadcasts the npc to all players around
		/// </summary>
		public virtual void BroadcastUpdate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendNPCUpdate(this);
				player.CurrentUpdateArray[ObjectID-1]=true;
			}
			m_lastUpdateTickCount=(uint)Environment.TickCount;
		}

		/// <summary>
		/// callback that npc was updated to the world
		/// so it must be visible to at least one player
		/// </summary>
		public void NPCUpdatedCallback()
		{
			m_lastVisibleToPlayerTick = (uint)Environment.TickCount;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				if (brain != null)
					brain.Start();
			}
		}
		/// <summary>
		/// Adds the npc to the world
		/// </summary>
		/// <returns>true if the npc has been successfully added</returns>
		public override bool AddToWorld()
		{
			if(!base.AddToWorld()) return false;
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendNPCCreate(this);
				if(m_inventory != null)
					player.Out.SendLivingEquipementUpdate(this);
			}
			BroadcastUpdate();
			m_spawnPosition = Position;
			m_spawnHeading = (ushort)Heading;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				if (brain != null)
					brain.Start();
			}
			return true;
		}

		/// <summary>
		/// Removes the npc from the world
		/// </summary>
		/// <returns>true if the npc has been successfully removed</returns>
		public override bool RemoveFromWorld()
		{
			if (ObjectState == eObjectState.Active)
			{
				foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendRemoveObject(this);
			}
			if (!base.RemoveFromWorld()) return false;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				brain.Stop();
			}
			return true;
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public override Region Region
		{
			get{ return base.Region; }
			set
			{
				Region oldRegion = Region;
				base.Region = value;
				Region newRegion = Region;
				if (oldRegion != newRegion && newRegion != null)
				{
					if (m_followTimer != null) m_followTimer.Stop();
					m_followTimer = new RegionTimer(this);
					m_followTimer.Callback = new RegionTimerCallback(FollowTimerCallback);
				}
			}
		}

		/// <summary>
		/// Marks this object as deleted!
		/// </summary>
		public override void Delete()
		{
			lock (BrainSync)
			{
				ABrain brain = Brain;
				brain.Stop();
			}
			StopFollow();
			TempProperties.removeProperty(CHARMED_TICK_PROP);
			base.Delete();
		}

		#endregion
		#region AI

		/// <summary>
		/// Holds the own NPC brain
		/// </summary>
		protected ABrain m_ownBrain;

		/// <summary>
		/// Holds the all added to this npc brains
		/// </summary>
		private ArrayList m_brains = new ArrayList(1);

		/// <summary>
		/// The sync object for brain changes
		/// </summary>
		private readonly object m_brainSync = new object();

		/// <summary>
		/// Gets the brain sync object
		/// </summary>
		public object BrainSync
		{
			get { return m_brainSync; }
		}

		/// <summary>
		/// Gets the current brain of this NPC
		/// </summary>
		public ABrain Brain
		{
			get
			{
				ArrayList brains = m_brains;
				if (brains.Count > 0)
					return (ABrain)brains[brains.Count-1];
				return m_ownBrain;
			}
		}

		/// <summary>
		/// Sets the NPC own brain
		/// </summary>
		/// <param name="brain">The new brain</param>
		/// <returns>The old own brain</returns>
		public virtual ABrain SetOwnBrain(ABrain brain)
		{
			if (brain == null)
				throw new ArgumentNullException("brain");
			if (brain.IsActive)
				throw new ArgumentException("The new brain is already active.", "brain");

			lock (BrainSync)
			{
				ABrain oldBrain = m_ownBrain;
				bool activate = oldBrain.IsActive;
				if (activate)
					oldBrain.Stop();
				m_ownBrain = brain;
				m_ownBrain.Body = this;
				if (activate)
					m_ownBrain.Start();

				return oldBrain;
			}
		}

		/// <summary>
		/// Adds a temporary brain to Npc, last added brain is active
		/// </summary>
		/// <param name="newBrain"></param>
		public virtual void AddBrain(ABrain newBrain)
		{
			if (newBrain == null)
				throw new ArgumentNullException("newBrain");
			if (newBrain.IsActive)
				throw new ArgumentException("The new brain is already active.", "newBrain");

			lock (BrainSync)
			{
				Brain.Stop();
				ArrayList brains = new ArrayList(m_brains);
				brains.Add(newBrain);
				m_brains = brains; // make new array list to avoid locks in the Brain property
				newBrain.Body = this;
				newBrain.Start();
			}
		}

		/// <summary>
		/// Removes a temporary brain from Npc
		/// </summary>
		/// <param name="removeBrain">The brain to remove</param>
		/// <returns>True if brain was found</returns>
		public virtual bool RemoveBrain(ABrain removeBrain)
		{
			if (removeBrain == null) return false;

			lock (BrainSync)
			{
				ArrayList brains = new ArrayList(m_brains);
				int index = brains.IndexOf(removeBrain);
				if (index < 0) return false;
				bool active = brains[index] == Brain;
				if (active)
					removeBrain.Stop();
				brains.RemoveAt(index);
				m_brains = brains;
				if (active)
					Brain.Start();

				return true;
			}
		}

		/// <summary>
		/// The ai timer of this npc
		/// </summary>
//		protected GameTimer m_AITimer;
		/// <summary>
		/// Calculate AI of this npc. This method needs to be as fast as possible!
		/// The overall performance of the server depends heavily on this method
		/// </summary>
//		public virtual uint AITimerCallback(GameTimer timer) 
//		{
//			GameEventMgr.Notify(GameNPCEvent.OnAICallback, this);
//			int interval = 20 + Random(5);
//			return (uint)interval;	// 2 seconds by default
//		}
		#endregion
		#region GetAggroLevelString

		/// <summary>
		/// How friendly this NPC is to player
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <param name="firstLetterUppercase"></param>
		/// <returns>aggro state as string</returns>
		public virtual string GetAggroLevelString(GamePlayer player, bool firstLetterUppercase)
		{
			// "aggressive", "hostile", "neutral", "friendly"
			// TODO: correct aggro strings
			// TODO: some merchants can be aggressive to players even in same realm
			// TODO: findout if trainers can be aggro at all

			//int aggro = CalculateAggroLevelToTarget(player);

			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if(GameServer.ServerRules.IsSameRealm(this, player, true))
			{
				if(firstLetterUppercase) return "Friendly";
				else return "friendly";
			}
			if(aggroBrain != null && aggroBrain.AggroLevel > 0)
			{
				if(firstLetterUppercase) return "Aggressive";
				else return "aggressive";
			}

			if(firstLetterUppercase) return "Neutral";
			else return "neutral";

		}
		#endregion
		#region Interact/SayTo
		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if(!base.Interact(player)) return false;
			if(!GameServer.ServerRules.IsSameRealm(this, player, true)) 
			{
				player.Out.SendMessage(GetName(0, true) + " gives you a dirty look.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}

		/// <summary>
		/// Format "say" message and send it to target in popup window
		/// </summary>
		/// <param name="target"></param>
		/// <param name="message"></param>
		public virtual void SayTo(GamePlayer target, string message)
		{
			SayTo(target, eChatLoc.CL_PopupWindow, message);
		}

		/// <summary>
		/// Format "say" message and send it to target
		/// </summary>
		/// <param name="target"></param>
		/// <param name="loc">chat location of the message</param>
		/// <param name="message"></param>
		public virtual void SayTo(GamePlayer target, eChatLoc loc, string message)
		{
			string resultText = GetName(0, true) + " says, \"" + message + "\"";

			switch(loc)
			{
				case eChatLoc.CL_PopupWindow:
					target.Out.SendMessage(resultText, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					Message.ChatToArea(this, GetName(0, true) + " speaks to " + target.GetName(0, false), eChatType.CT_System, WorldMgr.SAY_DISTANCE, target);
					break;
				case eChatLoc.CL_ChatWindow:
					target.Out.SendMessage(resultText, eChatType.CT_Say, eChatLoc.CL_ChatWindow);
					break;
				case eChatLoc.CL_SystemWindow:
					target.Out.SendMessage(resultText, eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;
			}
		}
		#endregion
		#region Combat
		
		/// <summary>
		/// The property that holds charmed tick if any
		/// </summary>
		public const string CHARMED_TICK_PROP = "CharmedTick";

		/// <summary>
		/// The duration of no exp after charmed, in game ticks
		/// </summary>
		public const int CHARMED_NOEXP_TIMEOUT = 60000;

		/// <summary>
		/// Starts a melee attack on a target
		/// </summary>
		/// <param name="attackTarget">The object to attack</param>
		public override void StartAttack(GameObject attackTarget)
		{
			TargetObject = attackTarget;
			base.StartAttack(attackTarget);
		}

		/// <summary>
		/// Gets/sets the object health
		/// </summary>
		public override int Health
		{
			get
			{
				return base.Health;
			}
			set
			{
				base.Health = value;
				//Slow mobs down when they are hurt!
				int maxSpeed = MaxSpeed;
				if (CurrentSpeed > maxSpeed)
					CurrentSpeed = maxSpeed;
			}
		}

		/// <summary>
		/// Tests if this MOB should give XP and loot based on the XPGainers
		/// </summary>
		/// <returns>true if it should deal XP and give loot</returns>
		public virtual bool IsWorthReward
		{
			get
			{
				if (Region.Time - CHARMED_NOEXP_TIMEOUT < TempProperties.getLongProperty(CHARMED_TICK_PROP, long.MinValue))
					return false;
				lock(m_xpGainers.SyncRoot)
				{
					if(m_xpGainers.Keys.Count==0) return false;
					foreach(DictionaryEntry de in m_xpGainers)
					{
						GameObject obj = (GameObject)de.Key;
						if(obj is GamePlayer)
						{
							//If a gameplayer with privlevel > 1 attacked the
							//mob, then the players won't gain xp ...
							if(((GamePlayer)obj).Client.Account.PrivLevel > ePrivLevel.Player)
								return false;
							//If a player to which we are gray killed up we
							//aren't worth anything either
							if( ((GamePlayer)obj).IsObjectGreyCon(this)) 
								return false;
						}
						else
						{
							//If object is no gameplayer and realm is != none
							//then it means that a npc has hit this living and
							//it is not worth any xp ...
							//if(obj.Realm != (byte)eRealm.None)
							//If grey to at least one living then no exp
							if(obj is GameLiving && ((GameLiving)obj).IsObjectGreyCon(this))
								return false;
						}
					}
					return true;
				}
			}
			set
			{
			}
		}

		/// <summary>
		/// Called when this living dies
		/// </summary>
		public override void Die(GameObject killer)
		{
			Message.SystemToArea(this, GetName(0, true)+" dies!", eChatType.CT_PlayerDied, killer);
			if (killer is GamePlayer)
				((GamePlayer)killer).Out.SendMessage(GetName(0, true)+" dies!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			StopFollow();
			base.Die(killer);

			// deal out exp and realm points based on server rules
			GameServer.ServerRules.OnNPCKilled(this, killer);

			Delete();
		}

		/// <summary>
		/// Stores the melee damage type of this NPC
		/// </summary>
		protected eDamageType m_meleeDamageType = eDamageType.Slash;

		/// <summary>
		/// Gets or sets the melee damage type of this NPC
		/// </summary>
		public virtual eDamageType MeleeDamageType
		{
			get { return m_meleeDamageType; }
			set { m_meleeDamageType = value; }
		}

		/// <summary>
		/// Returns the damage type of the current attack
		/// </summary>
		/// <param name="weapon">attack weapon</param>
		public override eDamageType AttackDamageType(Weapon weapon)
		{
			return m_meleeDamageType;
		}

		/// <summary>
		/// Stores the NPC evade chance
		/// </summary>
		protected byte m_evadeChance;
		/// <summary>
		/// Stores the NPC block chance
		/// </summary>
		protected byte m_blockChance;
		/// <summary>
		/// Stores the NPC parry chance
		/// </summary>
		protected byte m_parryChance;
		/// <summary>
		/// Stores the NPC left hand swing chance
		/// </summary>
		protected byte m_leftHandSwingChance;

		/// <summary>
		/// Gets or sets the NPC evade chance
		/// </summary>
		public virtual byte EvadeChance
		{
			get { return m_evadeChance; }
			set { m_evadeChance = value; }
		}

		/// <summary>
		/// Gets or sets the NPC block chance
		/// </summary>
		public virtual byte BlockChance
		{
			get { return m_blockChance; }
			set { m_blockChance = value; }
		}

		/// <summary>
		/// Gets or sets the NPC parry chance
		/// </summary>
		public virtual byte ParryChance
		{
			get { return m_parryChance; }
			set { m_parryChance = value; }
		}

		/// <summary>
		/// Gets or sets the NPC left hand swing chance
		/// </summary>
		public byte LeftHandSwingChance
		{
			get { return m_leftHandSwingChance; }
			set { m_leftHandSwingChance = value; }
		}

		/// <summary>
		/// Calculates how many times left hand swings
		/// </summary>
		/// <returns></returns>
		public override int CalculateLeftHandSwingCount()
		{
			if (Util.Chance(m_leftHandSwingChance))
				return 1;
			return 0;
		}

		/// <summary>
		/// Checks whether Living has ability to use lefthanded weapons
		/// </summary>
		public override bool CanUseLefthandedWeapon
		{
			get { return m_leftHandSwingChance > 0; }
		}

		#endregion
		#region Spell
		/// <summary>
		/// array of spell
		/// </summary>
		private IList m_spells = new ArrayList(1);

		/// <summary>
		/// property of spell array of NPC
		/// </summary>
		public IList Spells
		{
			get{return m_spells;}
			set{m_spells=value;}
		}

		/// <summary>
		/// start to cast spell attack in continue until takken melee damage
		/// </summary>
		/// <param name="attackTarget"></param>
		/// <returns></returns>
		public virtual bool StartSpellAttack(GameObject attackTarget)
		{
			if (!Position.CheckDistance(attackTarget.Position, AttackRange))
			{
				if (this.Spells != null)
				{
					foreach (Spell spell in this.Spells)
					{
						if (spell.SpellType == "DirectDamage")
						{
							SpellLine spellline = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
							this.CastSpell(spell,spellline);
							return true;
						}
					}
				}
			}
			return false;
		}

		/// <summary>
		/// stop the spell attack
		/// </summary>
		public virtual void StopSpellAttack()
		{
			if (m_runningSpellHandler != null && m_runningSpellHandler.Spell.SpellType == "DirectDamage")
			{
				//prevent from relaunch
				m_runningSpellHandler.CastingCompleteEvent -= new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler = null;
			}
		}

		/// <summary>
		/// Callback after spell execution finished and next spell can be processed
		/// </summary>
		/// <param name="handler"></param>
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			if (handler.Spell.SpellType == "DirectDamage")
			{
				m_runningSpellHandler = handler;
				handler.CastSpell();
			}			
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="body"></param>
		public void Buff(GameNPC body)
		{
			if (this.Spells != null)
			{
				foreach (Spell spell in this.Spells)
				{
					//todo find a way to get it by inherit of PropertyChangingSpell or not
					switch(spell.SpellType)
					{
						case "StrengthConstitutionBuff":
						case "DexterityQuicknessBuff":
						case "StrengthBuff":
						case "DexterityBuff":
						case "ConstitutionBuff":
						case "ArmorFactorBuff":
						case "ArmorAbsorbtionBuff":
						case "CombatSpeedBuff":
						case "MeleeDamageBuff":
						case "AcuityBuff" :
						case "BodyResistBuff":
						case "ColdResistBuff":
						case "EnergyResistBuff":
						case "HeatResistBuff":
						case "MatterResistBuff":
						case "SpiritResistBuff":
						case "BodySpiritEnergyBuff":
						case "HeatColdMatterBuff":
						case "CrushSlashThrustBuff":
						{
							SpellLine spellline = SkillBase.GetSpellLine(GlobalSpellsLines.Mob_Spells);
							this.CastSpell(spell,spellline);
							return;
						}
					}
				}
			}
		}
		#endregion
		#region Notify

		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			ABrain brain = Brain;
			if (brain != null)
				brain.Notify(e, sender, args);
		}

		#endregion

		/// <summary>
		/// Constructs a NPC
		/// </summary>
		public GameNPC() : base()
		{
			m_brainSync = m_brains.SyncRoot;
			m_followTarget = new WeakRef(null);
			m_rider = new WeakRef(null);

			m_size = 50; //Default size 
			m_followMinDist=100;
			m_followMaxDist=3000;
			m_flags=0;
			m_ownBrain = new StandardMobBrain();
			m_ownBrain.Body = this;
		}
        public GameNPC(INpcTemplate template) : this()
		{
            //TODO : maxhealth and styles
            this.Gender = (int)template.Gender;
            this.Race = template.Race;
            this.Faction = template.Faction;
            this.Realm = (byte)template.Realm;
            ABrain mybrain = Activator.CreateInstance(template.BrainType) as ABrain;
            if (mybrain is IAggressiveBrain)
            {
                ((IAggressiveBrain)mybrain).AggroLevel = template.AggroLevel;
                ((IAggressiveBrain)mybrain).AggroRange = template.AggroRange;
            }
            this.SetOwnBrain(mybrain);
           
            this.Level = (byte)template.Level;
			this.Name = template.Name;
			this.GuildName = template.GuildName;
			this.Model = template.Model;
			this.Size = template.Size;
			this.MaxSpeedBase = template.MaxSpeed;
            this.Flags = (byte)((byte)(template.IsStealth ? eFlags.STEALTH : 0) + (byte)(template.IsFlying ? eFlags.FLYING : 0) + (byte)(template.IsGhost ? eFlags.GHOST : 0) + (byte)(template.IsNameHidden ? eFlags.DONTSHOWNAME : 0) + (byte)(template.IsTargetable ? eFlags.CANTTARGET : 0));
            this.Inventory = template.Inventory;
			this.MeleeDamageType = template.MeleeDamageType;
			this.ParryChance = template.ParryChance;
			this.EvadeChance = template.EvadeChance;
			this.BlockChance = template.BlockChance;
			this.LeftHandSwingChance = template.LeftHandSwingChance;
			if (this.Inventory != null)
			{
				if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
					this.SwitchWeapon(eActiveWeaponSlot.Distance);
				else if (Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
					this.SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			}
			this.Spells = template.Spells;
		}
    }
}
