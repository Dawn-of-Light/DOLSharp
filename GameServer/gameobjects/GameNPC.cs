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
using System.Text;
using System.Threading;
using DOL.GS;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
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
	public abstract class GameNPC : GameLiving, IMovingGameObject
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region Sizes/Level/Realm/Name/GuildName/Faction
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
				BroadcastCreate();
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
				m_health = MaxHealth; // MaxHealth depends from mob level
				m_mana = MaxMana;
				m_endurancePercent = 100;
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
				BroadcastCreate();
				BroadcastUpdate();
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
				BroadcastCreate();
				BroadcastUpdate();
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
				BroadcastCreate();
				BroadcastUpdate();
			}
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
		/// Holds the GameTick of when this object was added to the world
		/// </summary>
		protected long m_spawnTick = 0;

		/// <summary>
		/// Holds various flags of this npc
		/// </summary>
		protected byte m_flags;
		
		/// <summary>
		/// The last time this NPC was actually updated to at least one player
		/// </summary>
		protected volatile uint  m_lastVisibleToPlayerTick = uint.MinValue;

		/// <summary>
		/// Gets the last this this NPC was actually update to at least one player.
		/// </summary>
		public uint LastVisibleToPlayersTickCount
		{
			get { return m_lastVisibleToPlayerTick; }
		}

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
						BroadcastCreate();
						BroadcastUpdate();
					}
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
		/// Gets the GameTick of when this object was added to the world
		/// </summary>
		public long SpawnTick
		{
			get { return m_spawnTick; }
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
		public override int CurrentSpeed
		{
			get
			{
				if(IsMoving) return m_movementAction.CurrentSpeed;
				return 0;
			}
			set
			{
				if(IsMoving) m_movementAction.CurrentSpeed = Math.Min(value, MaxSpeed);
			}
		}

		/// <summary>
		/// Returns if the object is moving or not
		/// </summary>
		public override bool IsMoving
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

			m_movementAction = new MovementAction(this, walkTarget, Math.Min(speed, MaxSpeed));
			BroadcastUpdate(); // broadcast update
		}

		/// <summary>
		/// Walk to the spawn point
		/// </summary>
		public virtual void WalkToSpawn()
		{
			StopAttack(); // call stopFollow
			
			WalkTo(SpawnPosition, (int)(MaxSpeed/2.5));
			GameEventMgr.AddHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnNPCArriveAtSpawnPoint));
		}

		/// <summary>
		/// Restore the spawn heading when a npc come back to its spawn point
		/// </summary>
		/// <param name="e"></param>
		/// <param name="sender"></param>
		/// <param name="arguments"></param>
		protected virtual void OnNPCArriveAtSpawnPoint(DOLEvent e, object sender, EventArgs arguments)
		{
			if (Position == SpawnPosition)
			{
				TurnTo(SpawnHeading);
			}
			GameEventMgr.RemoveHandler(this, MovingGameObjectEvent.ArriveAtTarget, new DOLEventHandler(OnNPCArriveAtSpawnPoint));
		}

		/// <summary>
		/// Stops the movement of the object
		/// </summary>
		public virtual void StopMoving()
		{
			if(IsMoving) m_movementAction.StopMoving();
		}

		#endregion
		#region HealthRegeneration
		/// <summary>
		/// Callback timer for health regeneration
		/// </summary>
		/// <param name="selfRegenerationTimer">the regeneration timer</param>
		/// <returns>the new interval</returns>
		protected override int HealthRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			int oldHealthPercent = HealthPercent;
			int period = base.HealthRegenerationTimerCallback(selfRegenerationTimer);
			if(oldHealthPercent != HealthPercent)
			{
				BroadcastUpdate();
			}
			return period;
		}
		#endregion
		#region Follow

		/// <summary>
		/// The interval between follow checks, in milliseconds
		/// </summary>
		protected const int FOLLOWCHECKTICKS = 500;

		/// <summary>
		/// Timer fired when the npc follow something
		/// </summary>
		protected FollowTargetAction m_followTargetAction;

		/// <summary>
		/// Gets the NPC current follow target
		/// </summary>
		public GameObject CurrentFollowTarget
		{
			get 
			{ 
				if(m_followTargetAction != null) return m_followTargetAction.FollowTarget;
				return null;
			}
		}

		/// <summary>
		/// Follow given object
		/// </summary>
		/// <param name="followTarget">target to follow</param>
		/// <param name="minDistance">min distance to keep to the target</param>
		/// <param name="maxDistance">max distance to keep following</param>
		public virtual void Follow(GameLivingBase followTarget, int minDistance, int maxDistance) 
		{
			if(followTarget==null || followTarget.ObjectState!=eObjectState.Active) return;

			if(m_followTargetAction != null) m_followTargetAction.Stop();

			m_followTargetAction = new FollowTargetAction(this, followTarget, minDistance, maxDistance);
			m_followTargetAction.Start(1);
		}

		/// <summary>
		/// Delayed action that fires an event when an NPC arrives at its next movment step
		/// </summary>
		protected class FollowTargetAction : RegionAction
		{
			/// <summary>
			/// Max range to keep following
			/// </summary>
			protected int m_followMaxDist;

			/// <summary>
			/// Min range to keep to the target
			/// </summary>
			protected int m_followMinDist;

			/// <summary>
			/// Object that this npc is following as weakreference
			/// </summary>
			protected WeakReference m_followTarget = new WeakRef(null);

			/// <summary>
			/// Gets the NPC current follow target
			/// </summary>
			public GameLivingBase FollowTarget
			{
				get { return m_followTarget.Target as GameLivingBase; }
			}

			/// <summary>
			/// Constructs a new ArriveAtTargetAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			public FollowTargetAction(GameNPC actionSource, GameLivingBase followTarget, int followMinDist, int followMaxDist) : base(actionSource)
			{
				m_followTarget.Target = followTarget;
				m_followMaxDist = followMaxDist;
				m_followMinDist = followMinDist;
			}

			/// <summary>
			/// This function is called when the Mob follow something
			/// </summary>
			protected override void OnTick()
			{
				GameNPC source = (GameNPC)m_actionSource;
				
				GameLivingBase followTarget = FollowTarget;
				
				//Stop following if we have no target
				if(followTarget == null 
				|| followTarget.ObjectState != eObjectState.Active
				|| followTarget.Region != source.Region
				|| !followTarget.Alive)
				{
					source.Notify(GameNPCEvent.FollowLostTarget, source, new FollowLostTargetEventArgs(followTarget));
					Stop();
					return;
				}

				Point followPos = followTarget.Position;
				Point myPos = source.Position;

				//check if we are already moving to a correct target position
				if(source.TargetPosition.CheckDistance(followPos, m_followMinDist))
				{
					Interval = FOLLOWCHECKTICKS;
					return;
				}
				
				//Calculate the difference between our position and the players position
				int distance = myPos.GetDistance(followPos);
				if(distance > m_followMaxDist)
				{
					source.Notify(GameNPCEvent.FollowLostTarget, source, new FollowLostTargetEventArgs(followTarget));
					Stop();
					return;
				}
				else if(distance <= m_followMinDist)
				{
					source.TurnTo(followPos);
					Interval = FOLLOWCHECKTICKS;
					return;
				}

				// follow on distance
				double angle = myPos.GetHeadingTo(followPos)/Point.HEADING_CONST;
				int newX = (int)(myPos.X - Math.Sin(angle) * (distance - (m_followMinDist - 5)));
				int newY = (int)(myPos.Y + Math.Cos(angle) * (distance - (m_followMinDist - 5)));
				int newZ = 0;

				source.WalkTo(new Point(newX, newY, newZ), source.MaxSpeed);
				Interval = FOLLOWCHECKTICKS;
			}
		}

		/// <summary>
		/// Stop following
		/// </summary>
		public virtual void StopFollow() 
		{
			if (m_followTargetAction != null) m_followTargetAction.Stop();
			m_followTargetAction = null;
			
			StopMoving();
		}

		/// <summary>
		/// Gets the stunned flag of this living
		/// </summary>
		public override bool Stun
		{
			set
			{
				bool oldStun = Stun;
				base.Stun = value;

				if(Mez == false && oldStun != Stun)
				{
					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets the mesmerized flag of this living
		/// </summary>
		public override bool Mez
		{
			set 
			{ 
				bool oldMez = Mez;
				base.Mez = value;

				if(Stun == false && oldMez != Mez)
				{
					BroadcastUpdate();
				}
			}
		}

		#endregion
		#region TurnTo

		/// <summary>
		/// Turns the object towards a specific object.
		/// </summary>
		/// <param name="obj">Target object to turn to.</param>
		public virtual void TurnTo(GeometryEngineNode obj) 
		{
			TurnTo(obj.Position);
		}

		/// <summary>
		/// Turns the object towards a specific point.
		/// </summary>
		/// <param name="point">Target point to turn to.</param>
		public virtual void TurnTo(Point point) 
		{
			TurnTo(Position.GetHeadingTo(point));
		}

		/// <summary>
		/// Turns the object towards a specific heading
		/// </summary>
		/// <param name="newHeading">the new heading</param>
		public virtual void TurnTo(ushort newHeading)
		{
			if(!IsMoving && Alive && !IsTurningDisabled && Heading != newHeading)
			{
				Notify(GameNPCEvent.TurnToHeading, this, new TurnToHeadingEventArgs(newHeading));
				Heading = newHeading;
				
				BroadcastUpdate();
			}
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

			TurnTo(target);
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
				
				npc.TurnTo(m_oldHeading);
			}
		}
		#endregion
		#region Inventory / SwitchWeapon

		/// <summary>
		/// In the db we save the inventoryID but we must
		/// link the object with the inventory instance
		/// stored in the NPCInventoryMgr
		///(it is only used internaly by NHIbernate)
		/// </summary>
		/// <value>The faction id</value>
		private int InventoryID
		{
			get { return m_inventory != null ? m_inventory.InventoryID : 0; }
			set
			{
				m_inventory = NPCInventoryMgr.GetNPCInventory(value);
				if (m_inventory != null)
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
		/// Updates the items on a character
		/// </summary>
		public void UpdateNPCEquipmentAppearance()
		{
			if(ObjectState==eObjectState.Active)
			{
				foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendLivingEquipementUpdate(this);
			}
		}

		#endregion
		#region AddToWorld/RemoveFromWorld/BroadcastCreate/BroadcastUpdate
		
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
				if(m_inventory != null)
					player.Out.SendLivingEquipementUpdate(this);
			}
		}

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendRemoveObject(this, eRemoveType.KillAndDisappear);
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
				APlayerVicinityBrain brain = Brain as APlayerVicinityBrain;
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
			if(OwnBrain == null)
			{
				if(log.IsErrorEnabled)
					log.Error("A GameNPC without brain can't be added to the world !"+ToString()+")");

				return false;
			}

			if(!base.AddToWorld()) return false;
			m_spawnTick = Region.Time;
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
			if (!base.RemoveFromWorld()) return false;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				if(brain != null) brain.Stop();
			}
			StopFollow();
			StopMoving();
			return true;
		}

		#endregion
		#region AI

		/// <summary>
		/// Holds the own NPC brain
		/// </summary>
		protected ABrain m_ownBrain;

		/// <summary>
		/// Gets or set the own brain of this NPC (NHibernate)
		/// </summary>
		public ABrain OwnBrain
		{
			get { return m_ownBrain; }
			set { m_ownBrain = value; }
		}

		/// <summary>
		/// Holds the all added to this npc brains
		/// </summary>
		private ArrayList m_brains = new ArrayList(1);

		/// <summary>
		/// Gets the brain sync object
		/// </summary>
		public object BrainSync
		{
			get { return m_brains.SyncRoot; }
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

			if(GameServer.ServerRules.IsSameRealm(this, player, true))
			{
				if(firstLetterUppercase) return "Friendly";
				else return "friendly";
			}

			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if(aggroBrain != null && aggroBrain.AggroLevel > 0)
			{
				if(firstLetterUppercase) return "Aggressive";
				else return "aggressive";
			}

			if(firstLetterUppercase) return "Neutral";
			else return "neutral";
		}

/*		/// <summary>
		/// Pronoun of this NPC in case you need to refer it in 3rd person
		/// http://webster.commnet.edu/grammar/cases.htm
		/// </summary>
		/// <param name="firstLetterUppercase"></param>
		/// <param name="form">0=Subjective, 1=Possessive, 2=Objective</param>
		/// <returns>pronoun of this object</returns>
		public override string GetPronoun(bool firstLetterUppercase, int form)
		{
			// TODO: when mobs will get gender
			if(PlayerCharacter.Gender == 0)
				// male
				switch(form)
				{
					default: // Subjective
						if(firstLetterUppercase) return "He"; else return "he";
					case 1:	// Possessive
						if(firstLetterUppercase) return "His"; else return "his";
					case 2:	// Objective
						if(firstLetterUppercase) return "Him"; else return "him";
				}
			else
				// female
				switch(form)
				{
					default: // Subjective
						if(firstLetterUppercase) return "She"; else return "she";
					case 1:	// Possessive
						if(firstLetterUppercase) return "Her"; else return "her";
					case 2:	// Objective
						if(firstLetterUppercase) return "Her"; else return "her";
				}

			// it
			switch(form)
			{
				// Subjective
				default: if(firstLetterUppercase) return "It"; else return "it";
				// Possessive
				case 1:	if(firstLetterUppercase) return "Its"; else return "its";
				// Objective
				case 2: if(firstLetterUppercase) return "It"; else return "it";
			}
		}*/

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false)+" toward you.");
			return list;
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
		/// Starts a melee attack on a target
		/// </summary>
		/// <param name="attackTarget">The object to attack</param>
		public override void StartAttack(GameObject attackTarget)
		{
			TargetObject = attackTarget;
			base.StartAttack(attackTarget);
			if (AttackState) Follow((GameLivingBase)attackTarget, 90, 2500);	// follow at stickrange
		}

		/// <summary>
		/// Stops all attack actions, including following target
		/// </summary>
		public override void StopAttack()
		{
			base.StopAttack ();
			StopFollow();
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
		/// Called when this living dies
		/// </summary>
		public override void Die(GameLiving killer)
		{
			Message.SystemToArea(this, GetName(0, true)+" dies!", eChatType.CT_PlayerDied, killer);
			if (killer is GamePlayer)
				((GamePlayer)killer).Out.SendMessage(GetName(0, true)+" dies!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			StopFollow();
			StopMoving();
			base.Die(killer);

			// deal out exp + realm points based on server rules and loots
			GameServer.ServerRules.OnNPCKilled(this, killer);
			ClearXPGainer();

			RemoveFromWorld();
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
			{
				brain.Notify(e, sender, args);
			}
		}

		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" size=").Append(Size)
				.Append(" meleeDamageType=").Append(MeleeDamageType)
				.Append(" evadeChance=").Append(EvadeChance)
				.Append(" blockChance=").Append(BlockChance)
				.Append(" parryChance=").Append(ParryChance)
				.Append(" leftHandSwingChance=").Append(LeftHandSwingChance)
				.Append(" AI=").Append(OwnBrain != null ? OwnBrain.GetType().FullName : null)
				.ToString();
		}
	}
}
