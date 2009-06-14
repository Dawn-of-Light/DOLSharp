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
using System.Collections.Generic;
using System.Threading;
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Events;
using DOL.Language;
using DOL.GS.Effects;
using DOL.GS.Movement;
using DOL.GS.Quests;
using DOL.GS.PacketHandler;
using DOL.GS.Spells;
using DOL.GS.Utils;
using DOL.GS.Housing;
using DOL.GS.RealmAbilities;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class is the baseclass for all Non Player Characters like
	/// Monsters, Merchants, Guards, Steeds ...
	/// </summary>
	public class GameNPC : GameLiving
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Constant for determining if already at a point
		/// </summary>
		/// <remarks>
		/// This helps to reduce the turning of an npc while fighting or returning to a spawn
		/// </remarks>
		public const int CONST_WALKTOTOLERANCE = 5;

		#region Formations/Spacing

		//Space/Offsets used in formations
		// Normal = 1
		// Big = 2
		// Huge = 3
		private int m_FormationSpacing = 1;

		/// <summary>
		/// The Minions's x-offset from it's commander
		/// </summary>
		public int FormationSpacing
		{
			get { return m_FormationSpacing; }
			set
			{
				//BD range values vary from 1 to 3.  It is more appropriate to just ignore the
				//incorrect values than throw an error since this isn't a very important area.
				if (value > 0 && value < 4)
					m_FormationSpacing = value;
			}
		}

		/// <summary>
		/// Used for that formation type if a GameNPC has a formation
		/// </summary>
		public enum eFormationType
		{
			// M = owner
			// x = following npcs
			//Line formation
			// M x x x
			Line,
			//Triangle formation
			//		x
			// M x
			//		x
			Triangle,
			//Protect formation
			//		 x
			// x  M
			//		 x
			Protect,
		}

		private eFormationType m_formation = eFormationType.Line;
		/// <summary>
		/// How the minions line up with the commander
		/// </summary>
		public eFormationType Formation
		{
			get { return m_formation; }
			set { m_formation = value; }
		}

		#endregion
		#region Sizes/Properties
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
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendModelAndSizeChange(this, Model, value);
//					BroadcastUpdate();
				}
			}
		}

		/// <summary>
		/// Gets or sets the model of this npc
		/// </summary>
		public override ushort Model
		{
			get { return base.Model; }
			set
			{
				base.Model = value;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						player.Out.SendModelChange(this, Model);
				}
			}
		}

		/// <summary>
		/// Gets or sets the owner of this npc
		/// </summary>
		public string BoatOwnerID
		{
			get { return m_boatowner_id; }
			set
			{
				m_boatowner_id = value;
			}
		}

		/// <summary>
		/// Gets or sets the heading of this NPC
		/// </summary>
		public override ushort Heading
		{
			get { return base.Heading; }
			set
			{
				if (IsTurningDisabled)
					return;
				ushort oldHeading = base.Heading;
				base.Heading = value;
				if (base.Heading != oldHeading)
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
				byte oldlevel = base.Level;
				base.Level = value;
				CheckStats(); // make sure stats are sane

				//MaxHealth = (ushort)(value * 20 + 20);	// MaxHealth depends from mob level
				if (!InCombat)
					m_health = MaxHealth;
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if (m_inventory != null)
							player.Out.SendLivingEquipmentUpdate(this);
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
		public override eRealm Realm
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
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if (m_inventory != null)
							player.Out.SendLivingEquipmentUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		protected bool m_HealthMultiplicator = false;
		public bool HealthMultiplicator
		{
			get { return m_HealthMultiplicator; }
			set { m_HealthMultiplicator = value; }
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
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if (m_inventory != null)
							player.Out.SendLivingEquipmentUpdate(this);
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
				if (ObjectState == eObjectState.Active)
				{
					foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					{
						player.Out.SendNPCCreate(this);
						if (m_inventory != null)
							player.Out.SendLivingEquipmentUpdate(this);
					}
					BroadcastUpdate();
				}
			}
		}

		private Faction m_faction = null;
		/// <summary>
		/// Gets the Faction of the NPC
		/// </summary>
		public Faction Faction
		{
			get { return m_faction; }
			set
			{
				m_faction = value;
			}
		}

		private ArrayList m_linkedFactions;
		/// <summary>
		/// The linked factions for this NPC
		/// </summary>
		public ArrayList LinkedFactions
		{
			get { return m_linkedFactions; }
			set { m_linkedFactions = value; }
		}

		private bool m_isConfused;
		/// <summary>
		/// Is this NPC currently confused
		/// </summary>
		public bool IsConfused
		{
			get { return m_isConfused; }
			set { m_isConfused = value; }
		}

		private int m_bodyType;
		/// <summary>
		/// The NPC's body type
		/// </summary>
		public int BodyType
		{
			get { return m_bodyType; }
			set { m_bodyType = value; }
		}

		private int m_houseNumber;
		/// <summary>
		/// The NPC's current house
		/// </summary>
		public int HouseNumber
		{
			get { return m_houseNumber; }
			set { m_houseNumber = value; }
		}
		#endregion
		#region Stats

		/// <summary>
		/// Calculates the maximum health for a specific npclevel and constitution
		/// </summary>
		/// <param name="level">The level of the npc</param>
		/// <param name="constitution">The constitution of the npc</param>
		/// <returns></returns>
		public virtual int CalculateMaxHealth(int level, int constitution)
		{
			constitution -= 50;
			if (constitution < 0)
				constitution *= 2;
			int hp1 = ServerProperties.Properties.GAMENPC_BASE_HP * (20 + 20 + level);
			int hp2 = hp1 * constitution / 10000;
			return Math.Max(1, 20 + hp1 / 50 + hp2);
		}


		/// <summary>
		/// Calculates MaxHealth of an NPC
		/// </summary>
		/// <returns>maxpower of npc</returns>
		public virtual int CalculateMaxMana(int level, int manastat)
		{
			int maxpower = 0;
			maxpower = (level * (20 + 20 + 5)) + (this.Intelligence - 50);
			if (maxpower < 0)
				maxpower = 0;
			return maxpower;
		}

		/// <summary>
		/// Change a stat value
		/// (delegate to GameNPC)
		/// </summary>
		/// <param name="stat">The stat to change</param>
		/// <param name="val">The new value</param>
		public override void ChangeBaseStat(eStat stat, short val)
		{
			int oldstat = GetBaseStat(stat);
			base.ChangeBaseStat(stat, val);
			int newstat = GetBaseStat(stat);
			GameNPC npc = this;
			if (this != null && oldstat != newstat)
			{
				switch (stat)
				{
					case eStat.STR: npc.Strength = (short)newstat; break;
					case eStat.DEX: npc.Dexterity = (short)newstat; break;
					case eStat.CON: npc.Constitution = (short)newstat; break;
					case eStat.QUI: npc.Quickness = (short)newstat; break;
					case eStat.INT: npc.Intelligence = (short)newstat; break;
					case eStat.PIE: npc.Piety = (short)newstat; break;
					case eStat.EMP: npc.Empathy = (short)newstat; break;
					case eStat.CHR: npc.Charisma = (short)newstat; break;
				}
			}
		}

		/// <summary>
		/// Gets NPC's constitution
		/// </summary>
		public virtual short Constitution
		{
			get
			{
				return m_charStat[eStat.CON - eStat._First];
			}
			set { m_charStat[eStat.CON - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's dexterity
		/// </summary>
		public virtual short Dexterity
		{
			get { return m_charStat[eStat.DEX - eStat._First]; }
			set { m_charStat[eStat.DEX - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's strength
		/// </summary>
		public virtual short Strength
		{
			get { return m_charStat[eStat.STR - eStat._First]; }
			set { m_charStat[eStat.STR - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's quickness
		/// </summary>
		public virtual short Quickness
		{
			get { return m_charStat[eStat.QUI - eStat._First]; }
			set { m_charStat[eStat.QUI - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's intelligence
		/// </summary>
		public virtual short Intelligence
		{
			get { return m_charStat[eStat.INT - eStat._First]; }
			set { m_charStat[eStat.INT - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's piety
		/// </summary>
		public virtual short Piety
		{
			get { return m_charStat[eStat.PIE - eStat._First]; }
			set { m_charStat[eStat.PIE - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's empathy
		/// </summary>
		public virtual short Empathy
		{
			get { return m_charStat[eStat.EMP - eStat._First]; }
			set { m_charStat[eStat.EMP - eStat._First] = value; }
		}

		/// <summary>
		/// Gets NPC's charisma
		/// </summary>
		public virtual short Charisma
		{
			get { return m_charStat[eStat.CHR - eStat._First]; }
			set { m_charStat[eStat.CHR - eStat._First] = value; }
		}
		#endregion
		#region Flags/Position/SpawnPosition/UpdateTick/Tether
		/// <summary>
		/// Various flags for this npc
		/// </summary>
		[Flags]
		public enum eFlags : byte
		{
			/// <summary>
			/// The npc is a ghost, see through, transparent
			/// </summary>
			TRANSPARENT = 0x01,
			/// <summary>
			/// The npc is stealth (new since 1.71)
			/// </summary>
			STEALTH = 0x02,
			/// <summary>
			/// The npc doesn't show a name above its head but can be targeted
			/// </summary>
			DONTSHOWNAME = 0x04,
			/// <summary>
			/// The npc doesn't show a name above its head and can't be targeted
			/// </summary>
			CANTTARGET = 0x08,
			/// <summary>
			/// Not in nearest enemyes if different vs player realm, but can be targeted if model support this
			/// </summary>
			PEACE = 0x10,
			/// <summary>
			/// The npc is flying (z above ground permitted)
			/// </summary>
			FLYING = 0x20,
		}
		/// <summary>
		/// Holds various flags of this npc
		/// </summary>
		protected uint m_flags;
        /// <summary>
        /// Spawn point
        /// </summary>
        protected Point3D m_spawnPoint;
		/// <summary>
		/// Spawn Heading
		/// </summary>
		protected ushort m_spawnHeading;
		/// <summary>
		/// The last time this NPC sent the 0x09 update packet
		/// </summary>
		protected volatile uint m_lastUpdateTickCount = uint.MinValue;
		/// <summary>
		/// The last time this NPC was actually updated to at least one player
		/// </summary>
		protected volatile uint m_lastVisibleToPlayerTick = uint.MinValue;
		/// <summary>
		/// Gets or Sets the flags of this npc
		/// </summary>
		public virtual uint Flags
		{
			get { return m_flags; }
			set
			{
				uint oldflags = m_flags;
				m_flags = value;
				if (ObjectState == eObjectState.Active)
				{
					bool ghostChanged = (oldflags & (byte)eFlags.TRANSPARENT) != (value & (byte)eFlags.TRANSPARENT);
					bool stealthChanged = (oldflags & (byte)eFlags.STEALTH) != (value & (byte)eFlags.STEALTH);
					bool cantTargetChanged = (oldflags & (byte)eFlags.CANTTARGET) != (value & (byte)eFlags.CANTTARGET);
					bool dontShowNameChanged = (oldflags & (byte)eFlags.DONTSHOWNAME) != (value & (byte)eFlags.DONTSHOWNAME);
					bool peaceChanged = (oldflags & (byte)eFlags.PEACE) != (value & (byte)eFlags.PEACE);
					bool flyingChanged = (oldflags & (byte)eFlags.FLYING) != (value & (byte)eFlags.FLYING);
					if (ghostChanged || stealthChanged || cantTargetChanged || dontShowNameChanged || peaceChanged || flyingChanged)
					{
						foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendNPCCreate(this);
							if (m_inventory != null)
								player.Out.SendLivingEquipmentUpdate(this);
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
        public virtual Point3D SpawnPoint
        {
            get { return m_spawnPoint; }
            set { m_spawnPoint = value; }
        }

        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete( "Use GameNPC.SpawnPoint" )]
        public virtual int SpawnX
        {
            get { return m_spawnPoint.X; }
            set { m_spawnPoint.X = value; }
        }
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete( "Use GameNPC.SpawnPoint" )]
        public virtual int SpawnY
        {
            get { return m_spawnPoint.Y; }
            set { m_spawnPoint.Y = value; }
        }
        /// <summary>
        /// Gets or sets the spawnposition of this npc
        /// </summary>
        [Obsolete( "Use GameNPC.SpawnPoint" )]
        public virtual int SpawnZ
        {
            get { return m_spawnPoint.Z; }
            set { m_spawnPoint.Z = value; }
        }

		/// <summary>
		/// Gets or sets the spawnheading of this npc
		/// </summary>
		public virtual ushort SpawnHeading
		{
			get { return m_spawnHeading; }
			set { m_spawnHeading = value; }
		}

		/// <summary>
		/// Gets or sets the current speed of the npc
		/// </summary>
		public override int CurrentSpeed
		{
			set
			{
                SaveCurrentPosition();

				if (base.CurrentSpeed != value)
				{
					base.CurrentSpeed = value;
					BroadcastUpdate();
				}
			}
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
		/// Gets the current X of this living. Don't modify this property
		/// to try to change position of the mob while active. Use the
		/// MoveTo function instead
		/// </summary>
		public override int X
		{
			get
			{
				if (!IsMoving)
					return base.X;

				if (Target.X != 0 || Target.Y != 0 || Target.Z != 0)
				{
                    long expectedDistance = FastMath.Abs((long)Target.X - m_x);

                    if (expectedDistance == 0)
                        return Target.X;

					long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedX));
					
					if (expectedDistance - actualDistance < 0)
						return Target.X;
				}

				return base.X;
			}
		}

		/// <summary>
		/// Gets the current Y of this NPC. Don't modify this property
		/// to try to change position of the mob while active. Use the
		/// MoveTo function instead
		/// </summary>
		public override int Y
		{
			get
			{
				if (!IsMoving)
					return base.Y;

				if (Target.X != 0 || Target.Y != 0 || Target.Z != 0)
				{
                    long expectedDistance = FastMath.Abs((long)Target.Y - m_y);

                    if (expectedDistance == 0)
                        return Target.Y;

					long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedY));

					if (expectedDistance - actualDistance < 0)
						return Target.Y;
				}
				return base.Y;
			}
		}

		/// <summary>
		/// Gets the current Z of this NPC. Don't modify this property
		/// to try to change position of the mob while active. Use the
		/// MoveTo function instead
		/// </summary>
		public override int Z
		{
			get
			{
				if (!IsMoving)
					return base.Z;

				if (Target.X != 0 || Target.Y != 0 || Target.Z != 0)
				{
                    long expectedDistance = FastMath.Abs((long)Target.Z - m_z);

                    if (expectedDistance == 0) 
                        return Target.Z;

					long actualDistance = FastMath.Abs((long)(MovementElapsedTicks * TickSpeedZ));

					if (expectedDistance - actualDistance < 0)
						return Target.Z;
				}
				return base.Z;
			}
		}

		/// <summary>
		/// The stealth state of this NPC
		/// </summary>
		public override bool IsStealthed
		{
			get
			{
				//return (Flags & (uint)eFlags.TRANSPARENT) != 0;//TODO
				return false; // can't charm transparent mobs? that's not the right way
			}
		}

		protected int m_maxdistance;
		/// <summary>
		/// The Mob's max distance from its spawn before return automatically
		/// if MaxDistance > 0 ... the amount is the normal value
		/// if MaxDistance = 0 ... no maxdistance check
		/// if MaxDistance less than 0 ... the amount is calculated in procent of the value and the aggrorange (in StandardMobBrain)
		/// </summary>
		public int MaxDistance
		{
			get { return m_maxdistance; }
			set { m_maxdistance = value; }
		}

		protected int m_roamingRange;
		/// <summary>
		/// radius for roaming
		/// </summary>
		public int RoamingRange
		{
			get { return m_roamingRange; }
			set { m_roamingRange = value; }
		}

		protected int m_tetherRange;

		/// <summary>
		/// The mob's tether range; if mob is pulled farther than this distance
		/// it will return to its spawn point.
		/// if TetherRange > 0 ... the amount is the normal value
		/// if TetherRange less or equal 0 ... no tether check
		/// </summary>
		public int TetherRange
		{
			get { return m_tetherRange; }
			set { m_tetherRange = value; }
		}

		/// <summary>
		/// True, if NPC is out of tether range, false otherwise; if no tether
		/// range is specified, this will always return false.
		/// </summary>
		public bool IsOutOfTetherRange
		{
			get
			{
                if ( TetherRange > 0 )
                {
                    if( this.IsWithinRadius( this.SpawnPoint, TetherRange ) )
                        return false;
                    else
                        return true;
                }
                else
                {
                    return false;
                }
			}
		}

		#endregion
		#region Movement
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
		//protected CloseToTargetAction m_closeToTargetAction;
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
        /// At what health percent will npc give up range attack and rush the attacker
        /// </summary>
        protected const int MINHEALTHPERCENTFORRANGEDATTACK = 70;

		private string m_pathID;
		public string PathID
		{
			get { return m_pathID; }
			set { m_pathID = value; }
		}

        private IPoint3D m_target = new Point3D(0, 0, 0);

        /// <summary>
        /// The target position.
        /// </summary>
        public virtual IPoint3D Target 
        {
            get
            {
                return m_target;
            }

            protected set
            {
                if (value != m_target)
                {
                    SaveCurrentPosition();
                    m_target = value;
                }
            }
        }

        /// <summary>
        /// The target object.
        /// </summary>
        public override GameObject TargetObject
        {
            get
            {
                return base.TargetObject;
            }
            set
            {
                GameObject previousTarget = TargetObject;
                GameObject newTarget = value;

                base.TargetObject = newTarget;

                if (previousTarget != null && newTarget != previousTarget)
                    previousTarget.Notify(GameNPCEvent.SwitchedTarget, this,
                        new SwitchedTargetEventArgs(previousTarget, newTarget));           
            }
        }

		/// <summary>
		/// Updates the tick speed for this living.
        /// </summary>
        protected override void UpdateTickSpeed()
        {
            if (!IsMoving)
            {
                SetTickSpeed(0, 0, 0);
                return;
            }

            if (Target.X != 0 || Target.Y != 0 || Target.Z != 0)
            {
                double dist = this.GetDistanceTo(new Point3D(Target.X, Target.Y, Target.Z));

                if (dist <= 0)
                {
                    SetTickSpeed(0, 0, 0);
                    return;
                }

                double dx = (double)(Target.X - m_x) / dist;
                double dy = (double)(Target.Y - m_y) / dist;
                double dz = (double)(Target.Z - m_z) / dist;

                SetTickSpeed(dx, dy, dz, CurrentSpeed);
                return;
            }

            base.UpdateTickSpeed();
        }

		/// <summary>
		/// True if the mob is at its target position, else false.
		/// </summary>
		public bool IsAtTargetPosition
		{
            get
            {
                return (X == Target.X && Y == Target.Y && Z == Target.Z);
            }
		}

		/// <summary>
		/// Turns the npc towards a specific spot
		/// </summary>
		/// <param name="tx">Target X</param>
		/// <param name="ty">Target Y</param>
		public virtual void TurnTo(int tx, int ty)
		{
			TurnTo(tx, ty, true);
		}

		/// <summary>
		/// Turns the npc towards a specific spot
		/// optionally sends update to client
		/// </summary>
		/// <param name="tx">Target X</param>
		/// <param name="ty">Target Y</param>
		public virtual void TurnTo(int tx, int ty, bool sendUpdate)
		{
			if (IsStunned || IsMezzed) return;

			Notify(GameNPCEvent.TurnTo, this, new TurnToEventArgs(tx, ty));

            if (sendUpdate)
                Heading = GetHeading(new Point2D(tx, ty));
            else
                base.Heading = GetHeading(new Point2D(tx, ty));
		}

		/// <summary>
		/// Turns the npc towards a specific heading
		/// </summary>
		/// <param name="newHeading">the new heading</param>
		public virtual void TurnTo(ushort heading)
		{
			TurnTo(heading, true);
		}

		/// <summary>
		/// Turns the npc towards a specific heading
		/// optionally sends update to client
		/// </summary>
		/// <param name="newHeading">the new heading</param>
		public virtual void TurnTo(ushort heading, bool sendUpdate)
		{
			if (IsStunned || IsMezzed) return;

			Notify(GameNPCEvent.TurnToHeading, this, new TurnToHeadingEventArgs(heading));

			if (sendUpdate)
				if (Heading != heading) Heading = heading;
			else
				if (base.Heading != heading) base.Heading = heading;
		}

		/// <summary>
		/// Turns the NPC towards a specific gameObject
		/// which can be anything ... a player, item, mob, npc ...
		/// </summary>
		/// <param name="target">GameObject to turn towards</param>
		public virtual void TurnTo(GameObject target)
		{
			TurnTo(target, true);
		}

		/// <summary>
		/// Turns the NPC towards a specific gameObject
		/// which can be anything ... a player, item, mob, npc ...
		/// optionally sends update to client
		/// </summary>
		/// <param name="target">GameObject to turn towards</param>
		public virtual void TurnTo(GameObject target, bool sendUpdate)
		{
            if (target == null || target.CurrentRegion != CurrentRegion)
                return;

			TurnTo(target.X, target.Y, sendUpdate);
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
			if (target == null || target.CurrentRegion != CurrentRegion) 
                return;

			// Store original heading if not set already.

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
			protected readonly Point3D m_oldPosition;

			/// <summary>
			/// Creates a new TurnBackAction
			/// </summary>
			/// <param name="actionSource">The source of action</param>
			public RestoreHeadingAction(GameNPC actionSource)
				: base(actionSource)
			{
				m_oldHeading = actionSource.Heading;
				m_oldPosition = new Point3D(actionSource);
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;

				npc.TempProperties.removeProperty(RESTORE_HEADING_ACTION_PROP);

				if (npc.ObjectState != eObjectState.Active) return;
				if (!npc.IsAlive) return;
				if (npc.AttackState) return;
				if (npc.IsMoving) return;
				if (npc.Equals(m_oldPosition)) return;
				if (npc.Heading == m_oldHeading) return; // already set? oO

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
		  public ArriveAtTargetAction(GameNPC actionSource)
			: base(actionSource)
		  {
		  }

		  /// <summary>
		  /// This function is called when the Mob arrives at its target spot
		  /// It fires the ArriveAtTarget event
		  /// </summary>
          protected override void OnTick()
          {
              GameNPC npc = (GameNPC)m_actionSource;

              bool arriveAtSpawnPoint = npc.IsReturningToSpawnPoint;

              npc.StopMoving();
              npc.Notify(GameNPCEvent.ArriveAtTarget, npc);

              if (arriveAtSpawnPoint)
                  npc.Notify(GameNPCEvent.ArriveAtSpawnPoint, npc);
          }
        }

        public virtual void CancelWalkToTimer()
        {
            if (m_arriveAtTargetAction != null)
            {
                m_arriveAtTargetAction.Stop();
                m_arriveAtTargetAction = null;
            }
		}

        /// <summary>
        /// Ticks required to arrive at a given spot.
        /// </summary>
        /// <param name="target"></param>
        /// <param name="speed"></param>
        /// <returns></returns>
        private int GetTicksToArriveAt(IPoint3D target, int speed)
        {
            return GetDistanceTo(target) * 1000 / speed;
        }

        /// <summary>
        /// Make the current (calculated) position permanent.
        /// </summary>
        private void SaveCurrentPosition()
        {
            SavePosition(this);
        }

        /// <summary>
        /// Make the target position permanent.
        /// </summary>
        private void SavePosition(IPoint3D target)
        {
            X = target.X;
            Y = target.Y;
            Z = target.Z;

            MovementStartTick = Environment.TickCount;
        }

		/// <summary>
        /// Walk to a certain spot at a given speed.
		/// </summary>
		/// <param name="tx"></param>
		/// <param name="ty"></param>
		/// <param name="tz"></param>
		/// <param name="speed"></param>
		public virtual void WalkTo(int targetX, int targetY, int targetZ, int speed)
		{
            WalkTo(new Point3D(targetX, targetY, targetZ), speed);
		}

		/// <summary>
		/// Walk to a certain spot at a given speed.
		/// </summary>
		/// <param name="p"></param>
		/// <param name="speed"></param>
		public virtual void WalkTo(IPoint3D target, int speed)
		{
            if (IsTurningDisabled)
                return;

            if (speed > MaxSpeed)
                speed = MaxSpeed;

            if (speed <= 0)
                return;

            Target = target; // this also saves the current position

            if (IsWithinRadius(Target, CONST_WALKTOTOLERANCE))
            {
                // No need to start walking.

                Notify(GameNPCEvent.ArriveAtTarget, this);
                return;
            }

            CancelWalkToTimer();

            m_Heading = GetHeading(Target);
            m_currentSpeed = speed; 

            UpdateTickSpeed();
            Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(Target, speed));

            StartArriveAtTargetAction(GetTicksToArriveAt(Target, speed));
            BroadcastUpdate();
		}

        private void StartArriveAtTargetAction(int requiredTicks)
        {
            m_arriveAtTargetAction = new ArriveAtTargetAction(this);
            m_arriveAtTargetAction.Start((requiredTicks > 1) ? requiredTicks : 1);
        }

		/// <summary>
		/// Walk to the spawn point
		/// </summary>
		public virtual void WalkToSpawn()
		{
			WalkToSpawn((int) (MaxSpeed/2.5));
		}

		/// <summary>
		/// Walk to the spawn point
		/// </summary>
		public virtual void CancelWalkToSpawn()
		{
			CancelWalkToTimer();
			IsReturningHome = false;
            IsReturningToSpawnPoint = false;
		}

		/// <summary>
		/// Walk to the spawn point with specified speed
		/// </summary>
		public virtual void WalkToSpawn(int speed)
		{
			StopAttack();
			StopFollowing();

			StandardMobBrain brain = Brain as StandardMobBrain;
			//[Ganrod] Nidel: Force to clear Aggro
            //Satyr: Actually this is not the right place to forget aggro but
            //for now we may stay at this until the whole logic is redesigned.
            //Correct would be: Forget Aggro as soon as arrived at spot
            //
            // Aredhel: Actually, this brain stuff doesn't belong here in the 
            // first place, get rid of this asap.

			if(brain != null && brain.IsAggroing)
			{
				brain.ClearAggroList();
			}

			IsReturningHome = true;
            IsReturningToSpawnPoint = true;
            WalkTo( SpawnPoint, speed );
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

			CancelWalkToTimer();
            SaveCurrentPosition();
            Target.Clear();

			m_currentSpeed = speed;

			MovementStartTick = Environment.TickCount;
			UpdateTickSpeed();
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
		/// Stops the movement of the mob.
		/// </summary>
		public virtual void StopMoving()
		{
		    CancelWalkToSpawn();

			if (IsMoving)
				CurrentSpeed = 0;
		}

        /// <summary>
        /// Stops the movement of the mob and forcibly moves it to the
        /// given target position.
        /// </summary>
        public virtual void StopMovingAt(IPoint3D target)
        {
            CancelWalkToSpawn();

            if (IsMoving)
            {
                m_currentSpeed = 0;
                UpdateTickSpeed();
            }

            SavePosition(target);
            BroadcastUpdate();
        }

        private const int StickMinimumRange = 100;
        private const int StickMaximumRange = 5000;

		/// <summary>
		/// Follow given object
		/// </summary>
		/// <param name="target">Target to follow</param>
		/// <param name="minDistance">Min distance to keep to the target</param>
		/// <param name="maxDistance">Max distance to keep following</param>
		public virtual void Follow(GameObject target, int minDistance, int maxDistance)
		{
			if (m_followTimer.IsAlive)
				m_followTimer.Stop();

			if (target == null || target.ObjectState != eObjectState.Active) 
                return;

			m_followMaxDist = maxDistance;
			m_followMinDist = minDistance;
			m_followTarget.Target = target;
			m_followTimer.Start(1);
		}

		/// <summary>
		/// Stop following
		/// </summary>
		public virtual void StopFollowing()
		{
			lock (m_followTimer)
			{
				if (m_followTimer.IsAlive)
					m_followTimer.Stop();

				m_followTarget.Target = null;
				StopMoving();
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
			//sirru
			else if (m_attackers.Count == 0 && this.Spells.Count > 0 && this.TargetObject != null && GameServer.ServerRules.IsAllowedToAttack(this, (this.TargetObject as GameLiving), true))
			{
				if (TargetObject.Realm == 0 || Realm == 0)
					m_lastAttackTickPvE = m_CurrentRegion.Time;
				else m_lastAttackTickPvP = m_CurrentRegion.Time;
				if (this.CurrentRegion.Time - LastAttackedByEnemyTick > 10 * 1000)
				{
                    // Aredhel: Erm, checking for spells in a follow method, what did we create
                    // brain classes for again?

					//Check for negatively casting spells
					StandardMobBrain stanBrain = (StandardMobBrain)Brain;
					if (stanBrain != null)
						((StandardMobBrain)stanBrain).CheckSpells(StandardMobBrain.eCheckSpellType.Offensive);
				}
			}
		}

          /// <summary>
          /// Keep following a specific object at a max distance
          /// </summary>
          protected virtual int FollowTimerCallback(RegionTimer callingTimer)
          {
             if (IsCasting)
                return FOLLOWCHECKTICKS;
             bool wasInRange = m_followTimer.Properties.getProperty(FOLLOW_TARGET_IN_RANGE, false);
             m_followTimer.Properties.removeProperty(FOLLOW_TARGET_IN_RANGE);

             GameObject followTarget = (GameObject)m_followTarget.Target;
             GameLiving followLiving = followTarget as GameLiving;

             //Stop following if target living is dead
             if (followLiving != null && !followLiving.IsAlive)
             {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                return 0;
             }

             //Stop following if we have no target
             if (followTarget == null || followTarget.ObjectState != eObjectState.Active || CurrentRegionID != followTarget.CurrentRegionID)
             {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                return 0;
             }

             //Calculate the difference between our position and the players position
             float diffx = (long)followTarget.X - X;
             float diffy = (long)followTarget.Y - Y;
             float diffz = (long)followTarget.Z - Z;

             //SH: Removed Z checks when one of the two Z values is zero(on ground)
             //Tolakram: a Z of 0 does not indicate on the ground.  Z varies based on terrain  Removed 0 Z check
             float distance = (float)Math.Sqrt(diffx * diffx + diffy * diffy + diffz * diffz);

             //if distance is greater then the max follow distance, stop following and return home
             if ((int)distance > m_followMaxDist)
             {
                StopFollowing();
                Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                this.WalkToSpawn();
                return 0;
             }
             int newX, newY, newZ;

             if (this.Brain is StandardMobBrain)
             {
                StandardMobBrain brain = this.Brain as StandardMobBrain;

                //if the npc hasn't hit or been hit in a while, stop following and return home
                if (!(Brain is IControlledBrain))
                {
                   if (AttackState && brain != null && followLiving != null)
                   {
                      long seconds = 20 + ((brain.GetAggroAmountForLiving(followLiving) / (MaxHealth + 1)) * 100);
                      long lastattacked = LastAttackTick;
                      long lasthit = LastAttackedByEnemyTick;
                      if (CurrentRegion.Time - lastattacked > seconds * 1000 && CurrentRegion.Time - lasthit > seconds * 1000)
                      {
                         //StopFollow();
                         Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
                         //brain.ClearAggroList();
                         this.WalkToSpawn();
                         return 0;
                      }
                   }
                }

                //If we're part of a formation, we can get out early.
                newX = followTarget.X;
                newY = followTarget.Y;
                newZ = followTarget.Z;
                if (brain.CheckFormation(ref newX, ref newY, ref newZ))
                {
                   WalkTo(newX, newY, (ushort)newZ, MaxSpeed);
                   return FOLLOWCHECKTICKS;
                }
             }

             //Are we in range yet?  Tolakram - Distances under 100 do not calculate correctly leading to the mob always being told to walkto
             if ((int)distance <= (m_followMinDist < 100 ? 100 : m_followMinDist))
             {
                StopMoving();
                TurnTo(followTarget);
                if (!wasInRange)
                {
                   m_followTimer.Properties.setProperty(FOLLOW_TARGET_IN_RANGE, true);
                   FollowTargetInRange();
                }
                return FOLLOWCHECKTICKS;
             }

             // follow on distance
             diffx = (diffx / distance) * m_followMinDist;
             diffy = (diffy / distance) * m_followMinDist;
             diffz = (diffz / distance) * m_followMinDist;

             //Subtract the offset from the target's position to get
             //our target position
             newX = (int)(followTarget.X - diffx);
             newY = (int)(followTarget.Y - diffy);
             newZ = (int)(followTarget.Z - diffz);
             WalkTo(newX, newY, (ushort)newZ, MaxSpeed);
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
		#region Path (Movement)
		/// <summary>
		/// Gets sets the currentwaypoint that npc has to wander to
		/// </summary>
		public PathPoint CurrentWayPoint
		{
			get { return m_currentWayPoint; }
			set { m_currentWayPoint = value; }
		}

		/// <summary>
		/// Is the NPC returning home, if so, we don't want it to think
		/// </summary>
		public bool IsReturningHome
		{
			get { return m_isReturningHome; }
			set { m_isReturningHome = value; }
		}

		protected bool m_isReturningHome = false;

        /// <summary>
        /// Whether or not the NPC is on its way back to the spawn point.
        /// [Aredhel: I decided to add this property in order not to mess
        /// with SMB and IsReturningHome. Also, to prevent outside classes
        /// from interfering the setter is now protected.]
        /// </summary>
        public bool IsReturningToSpawnPoint { get; protected set; }

		/// <summary>
		/// Gets if npc moving on path
		/// </summary>
		public bool IsMovingOnPath
		{
			get { return m_IsMovingOnPath; }
		}
		/// <summary>
		/// Stores if npc moving on path
		/// </summary>
		protected bool m_IsMovingOnPath = false;

		/// <summary>
		/// let the npc travel on its path
		/// </summary>
		/// <param name="speed">Speed on path</param>
		public void MoveOnPath(int speed)
		{
			if (IsMovingOnPath)
				StopMovingOnPath();

			if (CurrentWayPoint == null)
			{
				if (log.IsWarnEnabled)
					log.Warn("No path to travel on for " + Name);
				return;
			}

			PathingNormalSpeed = speed;

            if( this.IsWithinRadius( CurrentWayPoint, 100 ) )
			{
                if (CurrentWayPoint.Type == ePathType.Path_Reverse && CurrentWayPoint.FiredFlag)
                    CurrentWayPoint = CurrentWayPoint.Prev;
                else
                {
                    if ((CurrentWayPoint.Type == ePathType.Loop) && (CurrentWayPoint.Next == null))
                        CurrentWayPoint = MovementMgr.FindFirstPathPoint(CurrentWayPoint);
                    else
                        CurrentWayPoint = CurrentWayPoint.Next;     
                }
			}

			if (CurrentWayPoint != null)
			{
                GameEventMgr.AddHandler(this, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnArriveAtWaypoint));
				WalkTo(CurrentWayPoint, Math.Min(speed, CurrentWayPoint.MaxSpeed));
				m_IsMovingOnPath = true;
				Notify(GameNPCEvent.PathMoveStarts, this);
			}
			else
			{
				StopMovingOnPath();
			}
		}

		/// <summary>
		/// Stop moving on path.
		/// </summary>
		public void StopMovingOnPath()
		{
			if (!IsMovingOnPath)
				return;

            GameEventMgr.RemoveHandler(this, GameNPCEvent.ArriveAtTarget, new DOLEventHandler(OnArriveAtWaypoint));
			Notify(GameNPCEvent.PathMoveEnds, this);
			m_IsMovingOnPath = false;
		}

        /// <summary>
        /// decides what to do on reached waypoint in path
        /// </summary>
        /// <param name="e"></param>
        /// <param name="n"></param>
        /// <param name="args"></param>
        protected void OnArriveAtWaypoint(DOLEvent e, object n, EventArgs args)
        {
            if (!IsMovingOnPath || n != this)
                return;

            if (CurrentWayPoint != null)
            {
                WaypointDelayAction waitTimer = new WaypointDelayAction(this);
                waitTimer.Start(Math.Max(1, CurrentWayPoint.WaitTime * 100));
            }
            else
                StopMovingOnPath();
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
			public WaypointDelayAction(GameObject actionSource)
				: base(actionSource)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC npc = (GameNPC)m_actionSource;
				if (!npc.IsMovingOnPath)
					return;
				PathPoint oldPathPoint = npc.CurrentWayPoint;
				PathPoint nextPathPoint = npc.CurrentWayPoint.Next;
				if ((npc.CurrentWayPoint.Type == ePathType.Path_Reverse) && (npc.CurrentWayPoint.FiredFlag))
					nextPathPoint = npc.CurrentWayPoint.Prev;

				if (nextPathPoint == null)
				{
					switch (npc.CurrentWayPoint.Type)
					{
						case ePathType.Loop:
							{
								npc.CurrentWayPoint = MovementMgr.FindFirstPathPoint(npc.CurrentWayPoint);
								npc.Notify(GameNPCEvent.PathMoveStarts, npc);
								break;
							}
						case ePathType.Once:
							npc.CurrentWayPoint = null;//to stop
							break;
						case ePathType.Path_Reverse://invert sens when go to end of path
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
					npc.StopMovingOnPath();
				}
			}
		}
		#endregion
		#region Inventory/LoadfromDB
		private NpcTemplate m_npcTemplate;
		/// <summary>
		/// The NPC's template
		/// </summary>
		public NpcTemplate NPCTemplate
		{
			get { return m_npcTemplate; }
			set { m_npcTemplate = value; }
		}
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
				if (template.LoadFromDatabase(EquipmentTemplateID))
				{
					m_inventory = template.CloseTemplate();
				}
				else
				{
					//if (log.IsDebugEnabled)
					//{
					//    //log.Warn("Error loading NPC inventory: InventoryID="+EquipmentTemplateID+", NPC name="+Name+".");
					//}
				}
				if (Inventory != null)
				{
					//if the distance slot isnt empty we use that
					//Seems to always
					if (Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
						SwitchWeapon(eActiveWeaponSlot.Distance);
					else
					{
						InventoryItem twohand = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
						InventoryItem onehand = Inventory.GetItem(eInventorySlot.RightHandWeapon);

						if (twohand != null && onehand != null)
							//Let's add some random chance
							SwitchWeapon(Util.Chance(50) ? eActiveWeaponSlot.TwoHanded : eActiveWeaponSlot.Standard);
						else if (twohand != null)
							//Hmm our right hand weapon may have been null
							SwitchWeapon(eActiveWeaponSlot.TwoHanded);
						else if (onehand != null)
							//Hmm twohand was null lets default down here
							SwitchWeapon(eActiveWeaponSlot.Standard);
					}
				}
			}
		}

		private bool m_loadedFromScript = true;
		public bool LoadedFromScript
		{
			get { return m_loadedFromScript; }
			set { m_loadedFromScript = value; }
		}


		/// <summary>
		/// Load a npc from the npc template
		/// </summary>
		/// <param name="obj">template to load from</param>
		public override void LoadFromDatabase(DataObject obj)
		{
			if (obj == null) return;
			base.LoadFromDatabase(obj);
			if (!(obj is Mob)) return;
			m_loadedFromScript = false;
			Mob npc = (Mob)obj;
			INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(npc.NPCTemplateID);
			if (npcTemplate != null)
				LoadTemplate(npcTemplate);
			Name = npc.Name;
			GuildName = npc.Guild;
			m_x = npc.X;
			m_y = npc.Y;
			m_z = npc.Z;
			m_Heading = (ushort)(npc.Heading & 0xFFF);
			m_maxSpeedBase = npc.Speed;
			m_currentSpeed = 0;
			CurrentRegionID = npc.Region;
			Realm = (eRealm)npc.Realm;
			Model = npc.Model;
			Size = npc.Size;
			Level = npc.Level;	// health changes when GameNPC.Level changes
			Flags = npc.Flags;

			// Copy stats from the table, we'll do a sanity check
			// afterwards.

			Strength = (short)npc.Strength;
			Constitution = (short)npc.Constitution;
			Dexterity = (short)npc.Dexterity;
			Quickness = (short)npc.Quickness;
			Intelligence = (short)npc.Intelligence;
			Piety = (short)npc.Piety;
			Charisma = (short)npc.Charisma;
			Empathy = (short)npc.Empathy;
			CheckStats();

			MeleeDamageType = (eDamageType)npc.MeleeDamageType;
			if (MeleeDamageType == 0)
			{
				MeleeDamageType = eDamageType.Slash;
			}
			m_activeWeaponSlot = eActiveWeaponSlot.Standard;
			ActiveQuiverSlot = eActiveQuiverSlot.None;

			m_faction = FactionMgr.GetFactionByID(npc.FactionID);
			LoadEquipmentTemplateFromDatabase(npc.EquipmentTemplateID);

			if (npc.RespawnInterval == -1)
				npc.RespawnInterval = 0;
			m_respawnInterval = npc.RespawnInterval * 1000;

			m_pathID = npc.PathID;

			if (npc.Brain != "")
			{
				ArrayList asms = new ArrayList();
				asms.Add(typeof(GameServer).Assembly);
				asms.AddRange(ScriptMgr.Scripts);
				ABrain brain = null;
				foreach (Assembly asm in asms)
				{
					brain = (ABrain)asm.CreateInstance(npc.Brain, false);
					if (brain != null)
						break;
				}
				if (brain != null)
					SetOwnBrain(brain);
			}

			IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
			if (aggroBrain != null)
			{
				if (npc.AggroRange == 0)
				{
					if (Realm == eRealm.None)
					{
						if (CurrentRegion.IsDungeon)
							aggroBrain.AggroRange = 300;
						else if (Name != Name.ToLower())
							aggroBrain.AggroRange = 500;
						else
							aggroBrain.AggroRange = 400;

						if (Name != Name.ToLower())
							aggroBrain.AggroLevel = 30;
						else
							aggroBrain.AggroLevel = (Level > 5) ? 30 : 0;
					}
					else if (Realm != eRealm.None)
					{
						aggroBrain.AggroRange = 500;
						aggroBrain.AggroLevel = 60;
					}
				}
				else
				{
					aggroBrain.AggroLevel = npc.AggroLevel;
					aggroBrain.AggroRange = npc.AggroRange;
				}
			}
			m_bodyType = npc.BodyType;
			m_houseNumber = npc.HouseNumber;
			m_maxdistance = npc.MaxDistance;
			m_roamingRange = npc.RoamingRange;
            m_isCloakHoodUp = npc.IsCloakHoodUp;

            Gender = (Gender)npc.Gender;
		}

		/// <summary>
		/// Deletes the mob from the database
		/// </summary>
		public override void DeleteFromDatabase()
		{
			if (InternalID != null)
			{
				Mob mob = (Mob)GameServer.Database.FindObjectByKey(typeof(Mob), InternalID);
				if (mob != null)
					GameServer.Database.DeleteObject(mob);
			}
		}

		/// <summary>
		/// Saves a mob into the db if it exists, it is
		/// updated, else it creates a new object in the DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			Mob mob = null;
			if (InternalID != null)
				mob = (Mob)GameServer.Database.FindObjectByKey(typeof(Mob), InternalID);

			if (mob == null)
				if (LoadedFromScript == false)
					mob = new Mob();
				else
					return;

			mob.Name = Name;
			mob.Guild = GuildName;
			mob.X = X;
			mob.Y = Y;
			mob.Z = Z;
			mob.Heading = Heading;
			mob.Speed = MaxSpeedBase;
			mob.Region = CurrentRegionID;
			mob.Realm = (byte)Realm;
			mob.Model = Model;
			mob.Size = Size;
			mob.Level = Level;

			// Stats
			mob.Constitution = Constitution;
			mob.Dexterity = Dexterity;
			mob.Strength = Strength;
			mob.Quickness = Quickness;
			mob.Intelligence = Intelligence;
			mob.Piety = Piety;
			mob.Empathy = Empathy;
			mob.Charisma = Charisma;

			mob.ClassType = this.GetType().ToString();
			mob.Flags = Flags;
			mob.Speed = MaxSpeedBase;
			mob.RespawnInterval = m_respawnInterval / 1000;
			mob.HouseNumber = HouseNumber;
			mob.RoamingRange = RoamingRange;
			if (Brain.GetType().FullName != typeof(StandardMobBrain).FullName)
				mob.Brain = Brain.GetType().FullName;
			IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
			if (aggroBrain != null)
			{
				mob.AggroLevel = aggroBrain.AggroLevel;
				mob.AggroRange = aggroBrain.AggroRange;
			}
			mob.EquipmentTemplateID = EquipmentTemplateID;
			if (m_faction != null)
				mob.FactionID = m_faction.ID;
			mob.MeleeDamageType = (int)MeleeDamageType;
			if (NPCTemplate != null)
				mob.NPCTemplateID = NPCTemplate.TemplateId;
			mob.PathID = PathID;
			mob.MaxDistance = m_maxdistance;
            mob.IsCloakHoodUp = m_isCloakHoodUp;
            mob.Gender = (byte)Gender;

			if (InternalID == null)
			{
				GameServer.Database.AddNewObject(mob);
				InternalID = mob.ObjectId;
			}
			else
				GameServer.Database.SaveObject(mob);
		}

		/// <summary>
		/// Load a NPC template onto this NPC
		/// </summary>
		/// <param name="template"></param>
		public void LoadTemplate(INpcTemplate template)
		{
			IList m_models = new ArrayList();
			IList m_sizes = new ArrayList();
			IList m_levels = new ArrayList();
			IList m_equipLoc = new ArrayList();
			Hashtable m_equipModel = new Hashtable();

			this.Name = template.Name;
			this.GuildName = template.GuildName;

			#region Models
			foreach (string str in template.Model.Split(';'))
			{
				if (str.Length == 0) continue;
				ushort i = ushort.Parse(str);
				m_models.Add(i);
			}
			this.Model = (ushort)m_models[Util.Random(m_models.Count - 1)];
			#endregion

			#region Size
			byte size = 50;
			if (!Util.IsEmpty(template.Size))
			{
				string[] splitSize = template.Size.Split(';');
				if (splitSize.Length == 1)
					size = byte.Parse(splitSize[0]);
				else size = (byte)Util.Random(int.Parse(splitSize[0]), int.Parse(splitSize[1]));
			}
			this.Size = size;
			#endregion

			#region Level
			byte level = 0;
			if (!Util.IsEmpty(template.Level))
			{
				string[] splitLevel = template.Level.Split(';');
				if (splitLevel.Length == 1)
					level = byte.Parse(splitLevel[0]);
				else level = (byte)Util.Random(int.Parse(splitLevel[0]), int.Parse(splitLevel[1]));
			}
			this.Level = level;
			#endregion

			#region Stats
			// Stats
			this.Constitution = (short)template.Constitution;
			this.Dexterity = (short)template.Dexterity;
			this.Strength = (short)template.Strength;
			this.Quickness = (short)template.Quickness;
			this.Intelligence = (short)template.Intelligence;
			this.Piety = (short)template.Piety;
			this.Empathy = (short)template.Empathy;
			this.Charisma = (short)template.Charisma;
			#endregion

			#region Misc Stats
			this.MaxDistance = template.MaxDistance;
			this.TetherRange = template.TetherRange;
			this.BodyType = template.BodyType;
			this.MaxSpeedBase = template.MaxSpeed;
			this.Flags = template.Flags;
			this.MeleeDamageType = template.MeleeDamageType;
			this.ParryChance = template.ParryChance;
			this.EvadeChance = template.EvadeChance;
			this.BlockChance = template.BlockChance;
			this.LeftHandSwingChance = template.LeftHandSwingChance;
			#endregion

			#region Inventory
			//Ok lets start loading the npc equipment - only if there is a value!
			if (!Util.IsEmpty(template.Inventory))
			{
				bool equipHasItems = false;
				GameNpcInventoryTemplate equip = new GameNpcInventoryTemplate();
				//First let's try to reach the npcequipment table and load that!
				//We use a ';' split to allow npctemplates to support more than one equipmentIDs
				string[] equipIDs = template.Inventory.Split(';');
				if (!template.Inventory.Contains(":"))
				{
					foreach (string str in equipIDs)
					{
						equipHasItems |= equip.LoadFromDatabase(str);
					}
				}

				#region Legacy Equipment Code
				//Nope, nothing in the npcequipment table, lets do the crappy parsing
				//This is legacy code
				if (!equipHasItems)
				{
					//Temp list to store our models
					List<int> tempModels = new List<int>();

					//Let's go through all of our ';' seperated slots
					foreach (string str in equipIDs)
					{
						tempModels.Clear();
						//Split the equipment into slot and model(s)
						string[] slotXModels = str.Split(':');
						//It should only be two in length SLOT : MODELS
						if (slotXModels.Length == 2)
						{
							int slot;
							//Let's try to get our slot
							if (Int32.TryParse(slotXModels[0], out slot))
							{
								//Now lets go through and add all the models to the list
								string[] models = slotXModels[1].Split('|');
								foreach (string strModel in models)
								{
									//We'll add it to the list if we successfully parse it!
									int model;
									if (Int32.TryParse(strModel, out model))
										tempModels.Add(model);
								}

								//If we found some models let's randomly pick one and add it the equipment
								if (tempModels.Count > 0)
									equipHasItems |= equip.AddNPCEquipment((eInventorySlot)slot, tempModels[Util.Random(tempModels.Count - 1)]);
							}
						}
					}
				}
				#endregion

				//We added some items - let's make it the new inventory
				if (equipHasItems)
				{
					this.Inventory = new GameNPCInventory(equip);
					if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
						this.SwitchWeapon(eActiveWeaponSlot.Distance);
				}
			}
			#endregion

			this.Spells = template.Spells;
			this.Styles = template.Styles;
			if (template.Abilities != null)
			{
				foreach (Ability ab in template.Abilities)
					m_abilities[ab.KeyName] = ab;
			}
			BuffBonusCategory4[(int)eStat.STR] += template.Strength;
			BuffBonusCategory4[(int)eStat.DEX] += template.Dexterity;
			BuffBonusCategory4[(int)eStat.CON] += template.Constitution;
			BuffBonusCategory4[(int)eStat.QUI] += template.Quickness;
			BuffBonusCategory4[(int)eStat.INT] += template.Intelligence;
			BuffBonusCategory4[(int)eStat.PIE] += template.Piety;
			BuffBonusCategory4[(int)eStat.EMP] += template.Empathy;
			BuffBonusCategory4[(int)eStat.CHR] += template.Charisma;

			m_ownBrain = (Name != null && (Name.EndsWith("retriever")
				|| Name.EndsWith("messenger"))) // Don't like this hack, but can't specify a brain in npctemplate at the moment
				? new RetrieverMobBrain()
				: new StandardMobBrain();
			m_ownBrain.Body = this;
			(m_ownBrain as StandardMobBrain).AggroLevel = template.AggroLevel;
			(m_ownBrain as StandardMobBrain).AggroRange = template.AggroRange;
			this.NPCTemplate = template as NpcTemplate;
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
			if (ObjectState == eObjectState.Active)
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendLivingEquipmentUpdate(this);
			}
		}

		#endregion
		#region Quest
		/// <summary>
		/// Holds all the quests this npc can give to players
		/// </summary>
		protected readonly ArrayList m_questListToGive = new ArrayList(1);

		/// <summary>
		/// Gets the questlist of this player
		/// </summary>
		public IList QuestListToGive
		{
			get { return m_questListToGive; }
		}

		/// <summary>
		/// Adds a quest type to the npc questlist
		/// </summary>
		/// <param name="questType">The quest type to add</param>
		/// <returns>true if added, false if the npc has already the quest!</returns>
		public void AddQuestToGive(Type questType)
		{
			lock (m_questListToGive.SyncRoot)
			{
				if (HasQuest(questType) == null)
				{
					AbstractQuest newQuest = (AbstractQuest)Activator.CreateInstance(questType);
					if (newQuest != null) m_questListToGive.Add(newQuest);
				}
			}
		}

		/// <summary>
		/// Adds a quest to the npc questlist
		/// </summary>
		/// <param name="questType">The questType to remove</param>
		/// <returns>true if added, false if the npc has already the quest!</returns>
		public bool RemoveQuestToGive(Type questType)
		{
			lock (m_questListToGive.SyncRoot)
			{
				foreach (AbstractQuest q in m_questListToGive)
				{
					if (q.GetType().Equals(questType))
					{
						m_questListToGive.Remove(q);
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check if the npc can give the specified quest to a player
		/// </summary>
		/// <param name="questType">The type of the quest</param>
		/// <param name="player">The player who search a quest</param>
		/// <returns>the number of time the quest can be done again</returns>
		public int CanGiveQuest(Type questType, GamePlayer player)
		{
			lock (m_questListToGive.SyncRoot)
			{
				foreach (AbstractQuest q in m_questListToGive)
				{
					if (q.GetType().Equals(questType) && q.CheckQuestQualification(player) && player.HasFinishedQuest(questType) < q.MaxQuestCount)
					{
						return q.MaxQuestCount - player.HasFinishedQuest(questType);
					}
				}
			}
			return 0;
		}

		/// <summary>
		/// Should the NPC show a quest indicator, this can be overriden for custom handling
		/// </summary>
		/// <param name="player"></param>
		/// <returns>True if the NPC should show quest indicator, false otherwise</returns>
		public virtual bool ShowQuestIndicator(GamePlayer player)
		{
			return CanGiveOneQuest(player);
		}

		/// <summary>
		/// Check if the npc can give one quest to a player
		/// </summary>
		/// <param name="player">The player to check</param>
		/// <returns>true if yes, false if the npc can give any quest</returns>
		public bool CanGiveOneQuest(GamePlayer player)
		{
			lock (m_questListToGive.SyncRoot)
			{
				foreach (AbstractQuest q in m_questListToGive)
				{
					Type questType = q.GetType();
					int doingQuest = (player.IsDoingQuest(questType) != null ? 1 : 0);
					if (q.CheckQuestQualification(player) && player.HasFinishedQuest(questType) + doingQuest < q.MaxQuestCount)
						return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Give a quest a to specific player
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <param name="player">The player that gets the quest</param>
		/// <param name="startStep">The starting quest step</param>
		/// <returns>true if added, false if the player do already the quest!</returns>
		public bool GiveQuest(Type questType, GamePlayer player, int startStep)
		{
			AbstractQuest quest = HasQuest(questType);
			if (quest != null)
			{
				AbstractQuest newQuest = (AbstractQuest)Activator.CreateInstance(questType, new object[] { player, startStep });
				if (newQuest != null && player.AddQuest(newQuest))
				{
					if (!CanGiveOneQuest(player))
						player.Out.SendNPCsQuestEffect(this, false);
					return true;
				}
			}
			return false;
		}

		/// <summary>
		/// Checks if this npc already have a specified quest
		/// </summary>
		/// <param name="questType">The quest type</param>
		/// <returns>the quest if the npc have the quest or null if not</returns>
		protected AbstractQuest HasQuest(Type questType)
		{
			lock (m_questListToGive.SyncRoot)
			{
				foreach (AbstractQuest q in m_questListToGive)
				{
					if (q.GetType().Equals(questType))
						return q;
				}
			}
			return null;
		}

		#endregion
		#region Riding
		//NPC's can have riders :-)
		/// <summary>
		/// Holds the rider of this NPC as weak reference
		/// </summary>
		public GamePlayer[] Riders;

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
			int exists = RiderArrayLocation(rider);
			if (exists != -1)
				return false;

			rider.MoveTo(CurrentRegionID, X, Y, Z, Heading);

			Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider, this));
			int slot = GetFreeArrayLocation();
			Riders[slot] = rider;
			rider.Steed = this;
			return true;
		}

		/// <summary>
		/// This function is called when a rider mounts this npc
		/// Since only players can ride NPC's you should use the
		/// GamePlayer.MountSteed function instead to make sure all
		/// callbacks are called correctly
		/// </summary>
		/// <param name="rider">GamePlayer that is the rider</param>
		/// <param name="forced">if true, mounting can't be prevented by handlers</param>
		/// <param name="slot">The desired slot to mount</param>
		/// <returns>true if mounted successfully</returns>
		public virtual bool RiderMount(GamePlayer rider, bool forced, int slot)
		{
			int exists = RiderArrayLocation(rider);
			if (exists != -1)
				return false;

			if (Riders[slot] != null)
				return false;

			//rider.MoveTo(CurrentRegionID, X, Y, Z, Heading);

			Notify(GameNPCEvent.RiderMount, this, new RiderMountEventArgs(rider, this));
			Riders[slot] = rider;
			rider.Steed = this;
			return true;
		}

		/// <summary>
		/// Called to dismount a rider from this npc.
		/// Since only players can ride NPC's you should use the
		/// GamePlayer.MountSteed function instead to make sure all
		/// callbacks are called correctly
		/// </summary>
		/// <param name="forced">if true, the dismounting can't be prevented by handlers</param>
		/// <param name="player">the player that is dismounting</param>
		/// <returns>true if dismounted successfully</returns>
		public virtual bool RiderDismount(bool forced, GamePlayer player)
		{
			if (Riders.Length <= 0)
				return false;

			int slot = RiderArrayLocation(player);
			if (slot < 0)
			{
				return false;
			}
			Riders[slot] = null;

			Notify(GameNPCEvent.RiderDismount, this, new RiderDismountEventArgs(player, this));
			player.Steed = null;

			return true;
		}

		/// <summary>
		/// Get a free array location on the NPC
		/// </summary>
		/// <returns></returns>
		public int GetFreeArrayLocation()
		{
			for (int i = 0; i < MAX_PASSENGERS; i++)
			{
				if (Riders[i] == null)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Get the riders array location
		/// </summary>
		/// <param name="player">the player to get location of</param>
		/// <returns></returns>
		public int RiderArrayLocation(GamePlayer player)
		{
			for (int i = 0; i < MAX_PASSENGERS; i++)
			{
				if (Riders[i] == player)
					return i;
			}
			return -1;
		}

		/// <summary>
		/// Get the riders slot on the npc
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public int RiderSlot(GamePlayer player)
		{
			int location = RiderArrayLocation(player);
			if (location == -1)
				return location;
			return location + SLOT_OFFSET;
		}

		/// <summary>
		/// The maximum passengers the NPC can take
		/// </summary>
		public virtual int MAX_PASSENGERS
		{
			get { return 1; }
		}

		/// <summary>
		/// The minimum number of passengers required to move
		/// </summary>
		public virtual int REQUIRED_PASSENGERS
		{
			get { return 1; }
		}

		/// <summary>
		/// The slot offset for this NPC
		/// </summary>
		public virtual int SLOT_OFFSET
		{
			get { return 0; }
		}

		/// <summary>
		/// Gets a list of the current riders
		/// </summary>
		public GamePlayer[] CurrentRiders
		{
			get
			{
				List<GamePlayer> list = new List<GamePlayer>(MAX_PASSENGERS);
				for (int i = 0; i < MAX_PASSENGERS; i++)
				{
					GamePlayer player = Riders[i];
					if (player != null)
						list.Add(player);
				}
				return list.ToArray();
			}
		}
		#endregion
		#region Add/Remove/Create/Remove/Update
		/// <summary>
		/// Broadcasts the npc to all players around
		/// </summary>
		public virtual void BroadcastUpdate()
		{
			if (ObjectState != eObjectState.Active) return;
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null) continue;
				player.Out.SendObjectUpdate(this);
				player.CurrentUpdateArray[ObjectID - 1] = true;
			}
			m_lastUpdateTickCount = (uint)Environment.TickCount;
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
			if (!base.AddToWorld()) return false;
			if (MAX_PASSENGERS > 0)
				Riders = new GamePlayer[MAX_PASSENGERS];

			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if (player == null) continue;
				player.Out.SendNPCCreate(this);
				if (m_inventory != null)
					player.Out.SendLivingEquipmentUpdate(this);
			}
			BroadcastUpdate();
			m_spawnPoint.X = X;
            m_spawnPoint.Y = Y;
            m_spawnPoint.Z = Z;
			m_spawnHeading = Heading;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				if (brain != null)
					brain.Start();
			}

			if (Mana <= 0 && MaxMana > 0)
				Mana = MaxMana;
			else if (Mana > 0 && MaxMana > 0)
				StartPowerRegeneration();

			//If the Mob has a Path assigned he will now walk on it!
			if (MaxSpeedBase > 0 && CurrentSpellHandler == null && !IsMoving
			    && !AttackState && !InCombat && !IsMovingOnPath && !IsReturningHome
				//Check everything otherwise the Server will crash
			    && PathID != null && PathID != "" && PathID != "NULL")
			{
				PathPoint path = MovementMgr.LoadPath(PathID);
				if(path != null)
				{
					CurrentWayPoint = path;
					MoveOnPath(path.MaxSpeed);
				}
			}

            if (m_houseNumber > 0 && !(this is Consignment))
			{
				log.Info("NPC '" + Name + "' added to house N�" + m_houseNumber);
				CurrentHouse = HouseMgr.GetHouse(m_houseNumber);
				if (CurrentHouse == null)
					log.Warn("House " + CurrentHouse + " for NPC " + Name + " doesn't exist !!!");
				else
					log.Info("Confirmed number: " + CurrentHouse.HouseNumber.ToString());
			}
      // [Ganrod] Nidel: Hack pour mettre full life au respawn.
      if (!InCombat && IsAlive && base.Health < MaxHealth)
      {
        base.Health = MaxHealth;
      }
			return true;
		}

		/// <summary>
		/// Removes the npc from the world
		/// </summary>
		/// <returns>true if the npc has been successfully removed</returns>
		public override bool RemoveFromWorld()
		{
			if (IsMovingOnPath)
				StopMovingOnPath();
			if (MAX_PASSENGERS > 0)
			{
				foreach (GamePlayer player in CurrentRiders)
				{
					player.DismountSteed(true);
				}
			}

			if (ObjectState == eObjectState.Active)
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendObjectRemove(this);
			}
			if (!base.RemoveFromWorld()) return false;
			lock (BrainSync)
			{
				ABrain brain = Brain;
				brain.Stop();
			}
			EffectList.CancelAll();
			return true;
		}

		public bool MoveTo(ushort regionID, int x, int y, int z, ushort heading, bool petMove)
		{
			if (!petMove)
				return base.MoveTo(regionID, x, y, z, heading);

			if (m_ObjectState != eObjectState.Active)
				return false;

			Region rgn = WorldMgr.GetRegion(regionID);
			if (rgn == null)
				return false;
			if (rgn.GetZone(x, y) == null)
				return false;

			Notify(GameObjectEvent.MoveTo, this, new MoveToEventArgs(regionID, x, y, z, heading));

			if (ObjectState == eObjectState.Active)
			{
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
					player.Out.SendObjectRemove(this);
			}
			base.RemoveFromWorld();
			m_x = x;
			m_y = y;
			m_z = z;
			m_Heading = heading;
			CurrentRegionID = regionID;
			return AddToWorld();
		}

		/// <summary>
		/// Gets or Sets the current Region of the Object
		/// </summary>
		public override Region CurrentRegion
		{
			get { return base.CurrentRegion; }
			set
			{
				Region oldRegion = CurrentRegion;
				base.CurrentRegion = value;
				Region newRegion = CurrentRegion;
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
			lock (m_respawnTimerLock)
			{
				if (m_respawnTimer != null)
				{
					m_respawnTimer.Stop();
					m_respawnTimer = null;
				}
			}
			lock (BrainSync)
			{
				ABrain brain = Brain;
				brain.Stop();
			}
			StopFollowing();
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
					return (ABrain)brains[brains.Count - 1];
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
				return null;
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

			// "aggressive towards you!", "hostile towards you.", "neutral towards you.", "friendly."
			// TODO: correct aggro strings
			string aggroLevelString = "";
			int aggroLevel;
			if (Faction != null)
			{
				aggroLevel = Faction.GetAggroToFaction(player);
				if (aggroLevel > 75)
					aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Aggressive1");
				else if (aggroLevel > 50)
					aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Hostile1");
				else if (aggroLevel > 25)
					aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Neutral1");
				else
					aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Friendly1");
			}
			else
			{
				IOldAggressiveBrain aggroBrain = Brain as IOldAggressiveBrain;
				if (GameServer.ServerRules.IsSameRealm(this, player, true))
				{
					if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Friendly2");
					else aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Friendly1");
				}
				else if (aggroBrain != null && aggroBrain.AggroLevel > 0)
				{
					if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Aggressive2");
					else aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Aggressive1");
				}
				else
				{
					if (firstLetterUppercase) aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Neutral2");
					else aggroLevelString = LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.Neutral1");
				}
			}
			return LanguageMgr.GetTranslation(player.Client, "GameNPC.GetAggroLevelString.TowardsYou", aggroLevelString);
		}

        /// <summary>
        /// Gets the proper pronoun including capitalization.
        /// </summary>
        /// <param name="form"></param>
        /// <param name="capitalize"></param>
        /// <returns></returns>
        public override string GetPronoun(int form, bool capitalize)
        {
            String language = ServerProperties.Properties.DB_LANGUAGE;

            switch (Gender)
            {
                case Gender.Male:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Male.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Male.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                "GameLiving.Pronoun.Male.Subjective"));
                    }

                case Gender.Female:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Female.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Female.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                "GameLiving.Pronoun.Female.Subjective"));
                    }
                default:
                    switch (form)
                    {
                        case 1:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Neutral.Possessive"));
                        case 2:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language, 
                                "GameLiving.Pronoun.Neutral.Objective"));
                        default:
                            return Capitalize(capitalize, LanguageMgr.GetTranslation(language,
                                "GameLiving.Pronoun.Neutral.Subjective"));
                    }
            }
        }

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameNPC.GetExamineMessages.YouExamine", 
                GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));

			return list;
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
		#endregion
		#region Interact/WhisperReceive/SayTo
		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player)) return false;
			if (!GameServer.ServerRules.IsSameRealm(this, player, true))
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameNPC.Interact.DirtyLook", GetName(0, true)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				Notify(GameObjectEvent.InteractFailed, this, new InteractEventArgs(player));
				return false;
			}
			if (MAX_PASSENGERS > 1)
			{
				string name = "";
				if (this is GameHorseBoat)
					name = "boat";
				if (this is GameSiegeRam)
					name = "ram";

				if (RiderSlot(player) != -1)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameNPC.Interact.AlreadyRiding", name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (GetFreeArrayLocation() == -1)
				{
					player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameNPC.Interact.IsFull", name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (player.IsRiding)
				{
					player.DismountSteed(true);
				}

				if (player.IsOnHorse)
				{
					player.IsOnHorse = false;
				}

				player.MountSteed(this, true);
			}
			return true;
		}

		/// <summary>
		/// ToDo
		/// </summary>
		/// <param name="source"></param>
		/// <param name="text"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text))
				return false;
			if (source is GamePlayer == false)
				return true;

			GamePlayer player = (GamePlayer) source;

			//TODO: Guards in rvr areas doesn't need check
			if (text == "task")
		    {
				if (source.TargetObject == null)
					return false;
				if (KillTask.CheckAvailability(player, (GameLiving) source.TargetObject))
				{
					KillTask.BuildTask(player, (GameLiving) source.TargetObject);
					return true;
				}
				else if (MoneyTask.CheckAvailability(player, (GameLiving) source.TargetObject))
				{
					MoneyTask.BuildTask(player, (GameLiving) source.TargetObject);
					return true;
				}
				else if (CraftTask.CheckAvailability(player, (GameLiving) source.TargetObject))
				{
					CraftTask.BuildTask(player, (GameLiving) source.TargetObject);
					return true;
				}
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
			if (target == null)
				return;

			TurnTo(target);
			string resultText = LanguageMgr.GetTranslation(target.Client, "GameNPC.SayTo.Says", GetName(0, true), message);
			switch (loc)
			{
				case eChatLoc.CL_PopupWindow:
					target.Out.SendMessage(resultText, eChatType.CT_System, eChatLoc.CL_PopupWindow);
					Message.ChatToArea(this, LanguageMgr.GetTranslation(target.Client, "GameNPC.SayTo.Says", GetName(0, true), target.GetName(0, false)), eChatType.CT_System, WorldMgr.SAY_DISTANCE, target);
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
		/// <param name="target">The object to attack</param>
		public override void StartAttack(GameObject target)
		{
			if (target == null)
				return;

			if (IsMovingOnPath)
				StopMovingOnPath();

            // Aredhel: More brain code that doesn't belong here. If the pet is put on
            // passive, why fire StartAttack at all?
			if (Brain is IControlledBrain)  
			{
				if ((Brain as IControlledBrain).AggressionState == eAggressionState.Passive)
					return;

				GamePlayer owner = null;

				if ((owner = ((IControlledBrain)Brain).GetPlayerOwner()) != null)
					owner.Stealth(false);
			}

			TargetObject = target;

            SetLastMeleeAttackTick();
            StartMeleeAttackTimer();

			base.StartAttack(target);

			if (AttackState)
			{
                // if we're moving we need to lock down the current position
                if (IsMoving)
                    SaveCurrentPosition();

                if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
                {
                    Follow(target, AttackRange, StickMaximumRange);
                }
                else
                {
                    Follow(target, StickMinimumRange, StickMaximumRange);
                }
			}
		}

        private void SetLastMeleeAttackTick()
        {
            if (TargetObject.Realm == 0 || Realm == 0)
                m_lastAttackTickPvE = m_CurrentRegion.Time;
            else
                m_lastAttackTickPvP = m_CurrentRegion.Time;
        }

        private void StartMeleeAttackTimer()
        {
            if (m_attackers.Count == 0)
            {
                if (SpellTimer == null)
                    SpellTimer = new SpellAction(this);

                if (!SpellTimer.IsAlive)
                    SpellTimer.Start(1);
            }
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
		/// npcs can always have mana to cast
		/// </summary>
		public override int Mana
		{
			get { return 5000; }
		}

		/// <summary>
		/// The Max Mana for this NPC
		/// </summary>
		public override int MaxMana
		{
			get { return 1000; }
		}

		/// <summary>
		/// The Concentration for this NPC
		/// </summary>
		public override int Concentration
		{
			get
			{
				return 500;
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
				if (CurrentRegion.Time - CHARMED_NOEXP_TIMEOUT < TempProperties.getLongProperty(CHARMED_TICK_PROP, 0L))
					return false;
				if (this.Brain is IControlledBrain)
					return false;
				lock (m_xpGainers.SyncRoot)
				{
					if (m_xpGainers.Keys.Count == 0) return false;
					foreach (DictionaryEntry de in m_xpGainers)
					{
						GameObject obj = (GameObject)de.Key;
						if (obj is GamePlayer)
						{
							//If a gameplayer with privlevel > 1 attacked the
							//mob, then the players won't gain xp ...
							if (((GamePlayer)obj).Client.Account.PrivLevel > 1)
								return false;
							//If a player to which we are gray killed up we
							//aren't worth anything either
							if (((GamePlayer)obj).IsObjectGreyCon(this))
								return false;
						}
						else
						{
							//If object is no gameplayer and realm is != none
							//then it means that a npc has hit this living and
							//it is not worth any xp ...
							//if(obj.Realm != (byte)eRealm.None)
							//If grey to at least one living then no exp
							if (obj is GameLiving && ((GameLiving)obj).IsObjectGreyCon(this))
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
			if(killer!=null)
			{
				if (IsWorthReward)
					DropLoot(killer);

				Message.SystemToArea(this, GetName(0, true) + " dies!", eChatType.CT_PlayerDied, killer);
				if (killer is GamePlayer)
					((GamePlayer)killer).Out.SendMessage(GetName(0, true) + " dies!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			}
			StopFollowing();

			if (Group != null)
				Group.RemoveMember(this);

			if(killer!=null)
			{
				base.Die(killer);
				// deal out exp and realm points based on server rules
				GameServer.ServerRules.OnNPCKilled(this, killer);
			}

			Delete();

			if ((Faction != null) && (killer is GamePlayer))
			{
				GamePlayer player = killer as GamePlayer;
				Faction.KillMember(player);
			}

			// remove temp properties
			TempProperties.RemoveAll();

			if (!(this is GamePet))
				StartRespawn();
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
		public override eDamageType AttackDamageType(InventoryItem weapon)
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
			get
			{
				//When npcs have two handed weapons, we don't want them to block
				if (ActiveWeaponSlot != eActiveWeaponSlot.Standard)
					return 0;
				return m_blockChance;
			}
			set
			{
				m_blockChance = value;
			}
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

		/// <summary>
		/// Method to switch the npc to Melee attacks
		/// </summary>
		/// <param name="target"></param>
		public void SwitchToMelee(GameObject target)
		{
            // Tolakram: Order is important here.  First StopAttack, then switch weapon
            StopAttack();
            StopMoving();

            InventoryItem twohand = Inventory.GetItem(eInventorySlot.TwoHandWeapon);
			InventoryItem righthand = Inventory.GetItem(eInventorySlot.RightHandWeapon);

			if (twohand != null && righthand == null)
				SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			else if (twohand != null && righthand != null)
			{
				if (Util.Chance(50))
					SwitchWeapon(eActiveWeaponSlot.TwoHanded);
				else SwitchWeapon(eActiveWeaponSlot.Standard);
			}
			else 
                SwitchWeapon(eActiveWeaponSlot.Standard);

			StartAttack(target);
		}

		/// <summary>
		/// Method to switch the guard to Ranged attacks
		/// </summary>
		/// <param name="target"></param>
		public void SwitchToRanged(GameObject target)
		{
			SwitchWeapon(eActiveWeaponSlot.Distance);
			StartAttack(target);
			StopFollowing();
		}

		/// <summary>
		/// If npcs cant move, they cant be interupted from range attack
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="attackType"></param>
		/// <returns></returns>
		protected override bool OnInterruptTick(GameLiving attacker, AttackData.eAttackType attackType)
		{
			if (this.MaxSpeedBase == 0)
			{
				if (attackType == AttackData.eAttackType.Ranged || attackType == AttackData.eAttackType.Spell)
				{
                    if( this.IsWithinRadius( attacker, 150 ) == false )
					    return false;
				}
			}

            // Experimental - this prevents interrupts from causing ranged attacks to always switch to melee
            if (AttackState && HealthPercent < MINHEALTHPERCENTFORRANGEDATTACK) 
            {
                SwitchToMelee(attacker);
            }

			return base.OnInterruptTick(attacker, attackType);
		}

		/// <summary>
		/// The time to wait before each mob respawn
		/// </summary>
		protected int m_respawnInterval;
		/// <summary>
		/// A timer that will respawn this mob
		/// </summary>
		protected RegionTimer m_respawnTimer;
		/// <summary>
		/// The sync object for respawn timer modifications
		/// </summary>
		protected readonly object m_respawnTimerLock = new object();
		/// <summary>
		/// The Respawn Interval of this mob in milliseconds
		/// </summary>
		public virtual int RespawnInterval
		{
			get
			{
				if ( m_respawnInterval > 0 || m_respawnInterval < 0 )
					return m_respawnInterval;

				//Standard 5-8 mins
				if (Level <= 65 || Realm != 0)
				{
					return Util.Random(5 * 60000) + 3 * 60000;
				}
				else
				{
					int minutes = Level - 65 + 15;
					return minutes * 60000;
				}
			}
			set
			{
				m_respawnInterval = value;
			}
		}

		/// <summary>
		/// True if NPC is alive, else false.
		/// </summary>
		public override bool IsAlive
		{
			get
			{
				bool alive = base.IsAlive;
				if (alive && IsRespawning)
					return false;
				return alive;
			}
		}

		/// <summary>
		/// True, if the mob is respawning, else false.
		/// </summary>
		public bool IsRespawning
		{
			get
			{
				if (m_respawnTimer == null)
					return false;
				return m_respawnTimer.IsAlive;
			}
		}

		/// <summary>
		/// Starts the Respawn Timer
		/// </summary>
		public virtual void StartRespawn()
		{
			if (IsAlive) return;

			if (this.Brain is IControlledBrain)
				return;

			int respawnInt = RespawnInterval;
			if (respawnInt > 0)
			{
				lock (m_respawnTimerLock)
				{
					if (m_respawnTimer == null)
					{
						m_respawnTimer = new RegionTimer(this);
						m_respawnTimer.Callback = new RegionTimerCallback(RespawnTimerCallback);
					}
					else if (m_respawnTimer.IsAlive)
					{
						m_respawnTimer.Stop();
					}
					m_respawnTimer.Start(respawnInt);
				}
			}
		}
		/// <summary>
		/// The callback that will respawn this mob
		/// </summary>
		/// <param name="respawnTimer">the timer calling this callback</param>
		/// <returns>the new interval</returns>
		protected virtual int RespawnTimerCallback(RegionTimer respawnTimer)
		{
			lock (m_respawnTimerLock)
			{
				if (m_respawnTimer != null)
				{
					m_respawnTimer.Stop();
					m_respawnTimer = null;
				}
			}

			//DOLConsole.WriteLine("respawn");
			//TODO some real respawn handling
			if (IsAlive) return 0;
			if (ObjectState == eObjectState.Active) return 0;

			//Heal this mob, move it to the spawnlocation
			Health = MaxHealth;
			Mana = MaxMana;
			Endurance = MaxEndurance;
            int origSpawnX = m_spawnPoint.X;
            int origSpawnY = m_spawnPoint.Y;
			//X=(m_spawnX+Random(750)-350); //new SpawnX = oldSpawn +- 350 coords
			//Y=(m_spawnY+Random(750)-350);	//new SpawnX = oldSpawn +- 350 coords
			X = m_spawnPoint.X;
            Y = m_spawnPoint.Y;
            Z = m_spawnPoint.Z;
			Heading = m_spawnHeading;
			AddToWorld();
            m_spawnPoint.X = origSpawnX;
            m_spawnPoint.Y = origSpawnY;
			return 0;
		}

		/// <summary>
		/// Callback timer for health regeneration
		/// </summary>
		/// <param name="selfRegenerationTimer">the regeneration timer</param>
		/// <returns>the new interval</returns>
		protected override int HealthRegenerationTimerCallback(RegionTimer selfRegenerationTimer)
		{
			int period = m_healthRegenerationPeriod;
			if (!InCombat)
			{
                int oldPercent = HealthPercent;
				period = base.HealthRegenerationTimerCallback(selfRegenerationTimer);
				if (oldPercent != HealthPercent)
					BroadcastUpdate();
			}
			return (Health < MaxHealth) ? period : 0;
		}

		/// <summary>
		/// The chance for a critical hit
		/// </summary>
		public override int AttackCriticalChance(InventoryItem weapon)
		{
			return 0;
		}

		/// <summary>
		/// Stop attacking and following, but stay in attack mode (e.g. in
		/// order to cast a spell instead).
		/// </summary>
		public virtual void HoldAttack()
		{
			if (m_attackAction != null)
				m_attackAction.Stop();
			StopFollowing();
		}

		/// <summary>
		/// Continue a previously started attack.
		/// </summary>
		public virtual void ContinueAttack(GameObject target)
		{
			if (m_attackAction != null && target != null)
			{
				Follow(target, StickMinimumRange, MaxDistance);
				m_attackAction.Start(1);
			}
		}

		/// <summary>
		/// Stops all attack actions, including following target
		/// </summary>
		public override void StopAttack()
		{
			base.StopAttack();
			StopFollowing();

            // Tolakram: If npc has a distance weapon it needs to be made active after attack is stopped
            if (Inventory != null && Inventory.GetItem(eInventorySlot.DistanceWeapon) != null && ActiveWeaponSlot != eActiveWeaponSlot.Distance)
                SwitchWeapon(eActiveWeaponSlot.Distance);
        }

		/// <summary>
		/// This method is called to drop loot after this mob dies
		/// </summary>
		/// <param name="killer">The killer</param>
		public virtual void DropLoot(GameObject killer)
		{
			// TODO: mobs drop "a small chest" sometimes
			ArrayList dropMessages = new ArrayList();
			ArrayList autolootlist = new ArrayList();
			ArrayList aplayer = new ArrayList();
			String message;
			lock (m_xpGainers.SyncRoot)
			{
				if (m_xpGainers.Keys.Count == 0) return;

				ItemTemplate[] lootTemplates = LootMgr.GetLoot(this, killer);

				foreach (ItemTemplate lootTemplate in lootTemplates)
				{
					if(lootTemplate==null) continue;
					GameStaticItem loot;
					if (GameMoney.IsItemMoney(lootTemplate.Name))
					{
						long value = lootTemplate.Value;
						if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Coin_Drop_5, (eRealm)killer.Realm))
							value += (value / 100) * 5;
						else if (Keeps.KeepBonusMgr.RealmHasBonus(DOL.GS.Keeps.eKeepBonusType.Coin_Drop_3, (eRealm)killer.Realm))
							value += (value / 100) * 3;

						//this will need to be changed when the ML for increasing money is added
						if (value != lootTemplate.Value)
						{
							GamePlayer killerPlayer = killer as GamePlayer;
							if (killerPlayer != null)
								killerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(killerPlayer.Client, "GameNPC.DropLoot.AdditionalMoney", Money.GetString(value - lootTemplate.Value)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
						}

						loot = new GameMoney(value, this);
						loot.Name = lootTemplate.Name;
						loot.Model = (ushort)lootTemplate.Model;
					}
					else if (lootTemplate.Name.StartsWith("scroll|"))
					{
						String[] scrollData = lootTemplate.Name.Split('|');
						String artifactID = scrollData[1];
						int pageNumber = UInt16.Parse(scrollData[2]);
						loot = ArtifactMgr.CreateScroll(artifactID, pageNumber);
						loot.X = X;
						loot.Y = Y;
						loot.Z = Z;
						loot.Heading = Heading;
						loot.CurrentRegion = CurrentRegion;
					}
					else
					{
						loot = new GameInventoryItem(new InventoryItem(lootTemplate));
						loot.X = X;
						loot.Y = Y;
						loot.Z = Z;
						loot.Heading = Heading;
						loot.CurrentRegion = CurrentRegion;

						// This may seem like an odd place for this code, but loot-generating code further up the line
						// is dealing strictly with ItemTemplate objects, while you need the InventoryItem in order
						// to be able to set the Count property.
						// Converts single drops of loot with PackSize > 1 (and MaxCount >= PackSize) to stacks of Count = PackSize
						if ( ( (GameInventoryItem)loot ).Item.PackSize > 1 && ( (GameInventoryItem)loot ).Item.MaxCount >= ( (GameInventoryItem)loot ).Item.PackSize )
						{
							( (GameInventoryItem)loot ).Item.Count = ( (GameInventoryItem)loot ).Item.PackSize;
						}
					}

					GamePlayer playerAttacker = null;
					foreach (GameObject gainer in m_xpGainers.Keys)
					{
						if (gainer is GamePlayer)
						{
							playerAttacker = gainer as GamePlayer;
							if (loot.Realm == 0)
								loot.Realm = ((GamePlayer)gainer).Realm;
						}
						loot.AddOwner(gainer);
						if (gainer is GameNPC)
						{
							IControlledBrain brain = ((GameNPC)gainer).Brain as IControlledBrain;
							if (brain != null)
							{
								playerAttacker = brain.GetPlayerOwner();
								loot.AddOwner(brain.GetPlayerOwner());
							}
						}
					}
					if (playerAttacker == null) return; // no loot if mob kills another mob

                    message = String.Format(LanguageMgr.GetTranslation(playerAttacker.Client, "GameNPC.DropLoot.Drops",
                        GetName(0, true), char.IsLower(loot.Name[0]) ? loot.GetName(1, false) : loot.Name));

					dropMessages.Add(message);
					loot.AddToWorld();

					foreach (GameObject gainer in m_xpGainers.Keys)
					{
						if (gainer is GamePlayer)
						{
							GamePlayer player = gainer as GamePlayer;
							if (player.Autoloot && loot.IsWithinRadius(player, 700))
							{
								if (player.Group == null || (player.Group != null && player == player.Group.Leader))
									aplayer.Add(player);
								autolootlist.Add(loot);
							}
						}
					}
				}
			}

			BroadcastLoot(dropMessages);

			if (autolootlist.Count > 0)
			{
				foreach (GameObject obj in autolootlist)
				{
					foreach (GamePlayer player in aplayer)
					{
						player.PickupObject(obj, true);
						break;
					}
				}
			}
		}

		/// <summary>
		/// The enemy is healed, so we add to the xp gainers list
		/// </summary>
		/// <param name="enemy"></param>
		/// <param name="healSource"></param>
		/// <param name="changeType"></param>
		/// <param name="healAmount"></param>
		public override void EnemyHealed(GameLiving enemy, GameObject healSource, GameLiving.eHealthChangeType changeType, int healAmount)
		{
			base.EnemyHealed(enemy, healSource, changeType, healAmount);
			if (changeType != eHealthChangeType.Spell)
				return;
			if (enemy == healSource)
				return;
			if (!IsAlive)
				return;

			GamePlayer attackerPlayer = healSource as GamePlayer;
			if (attackerPlayer == null)
				return;

			Group attackerGroup = attackerPlayer.Group;
			if (attackerGroup != null)
			{
				ArrayList xpGainers = new ArrayList(8);
				// collect "helping" group players in range
				foreach (GameLiving living in attackerGroup.GetMembersInTheGroup())
				{
					if (this.IsWithinRadius(living, WorldMgr.MAX_EXPFORKILL_DISTANCE) && living.IsAlive && living.ObjectState == eObjectState.Active)
						xpGainers.Add(living);
				}
				// add players in range for exp to exp gainers
				for (int i = 0; i < xpGainers.Count; i++)
				{
					this.AddXPGainer((GamePlayer)xpGainers[i], (float)(healAmount / xpGainers.Count));
				}
			}
			else
			{
				this.AddXPGainer(healSource, (float)healAmount);
			}
			//DealDamage needs to be called after addxpgainer!
		}

		#endregion

		#region Spell

        /// <summary>
        /// Whether or not the NPC can cast harmful spells
        /// at the moment.
        /// </summary>
        public override bool CanCastHarmfulSpells
        {
            get
            {
                if (!base.CanCastHarmfulSpells)
                    return false;

                IList<Spell> harmfulSpells = HarmfulSpells;

                foreach (Spell harmfulSpell in harmfulSpells)
                    if (harmfulSpell.CastTime == 0)
                        return true;

                return (harmfulSpells.Count > 0 && !IsBeingInterrupted);
            }
        }

        public override IList<Spell> HarmfulSpells
        {
            get
            {
                IList<Spell> harmfulSpells = new List<Spell>();

                foreach (Spell spell in Spells)
                    if (spell.IsHarmful)
                        harmfulSpells.Add(spell);

                return harmfulSpells;
            }
        }

		private IList m_spells = new ArrayList(1);
		/// <summary>
		/// property of spell array of NPC
		/// </summary>
		public IList Spells
		{
			get { return m_spells; }
			set { m_spells = value; }
		}

		private IList m_styles = new ArrayList(1);
		/// <summary>
		/// The Styles for this NPC
		/// </summary>
		public IList Styles
		{
			get { return m_styles; }
			set { m_styles = value; }
		}

		/// <summary>
		/// The Abilities for this NPC
		/// </summary>
		public Hashtable Abilities
		{
			get { return m_abilities; }
		}

		private SpellAction m_spellaction = null;
		/// <summary>
		/// The timer that controls an npc's spell casting
		/// </summary>
		public SpellAction SpellTimer
		{
			get { return m_spellaction; }
			set { m_spellaction = value; }
		}

		/// <summary>
		/// Callback after spell execution finished and next spell can be processed
		/// </summary>
		/// <param name="handler"></param>
		public override void OnAfterSpellCastSequence(ISpellHandler handler)
		{
			if (SpellTimer != null)
			{
				if (this == null || this.ObjectState != eObjectState.Active || !this.IsAlive || this.TargetObject == null || (this.TargetObject is GameLiving && this.TargetObject.ObjectState != eObjectState.Active || !(this.TargetObject as GameLiving).IsAlive))
					SpellTimer.Stop();
				else
					SpellTimer.Start(1);
			}
			if (m_runningSpellHandler != null)
			{
				//prevent from relaunch
				m_runningSpellHandler.CastingCompleteEvent -= new CastingCompleteCallback(OnAfterSpellCastSequence);
				m_runningSpellHandler = null;
			}
		}

		/// <summary>
		/// The spell action of this living
		/// </summary>
		public class SpellAction : RegionAction
		{
			/// <summary>
			/// Constructs a new attack action
			/// </summary>
			/// <param name="owner">The action source</param>
			public SpellAction(GameLiving owner)
				: base(owner)
			{
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GameNPC owner = null;
				if (m_actionSource != null && m_actionSource is GameNPC)
					owner = (GameNPC)m_actionSource;
				else
				{
					Stop();
					return;
				}

				if (owner.TargetObject == null || !owner.AttackState)
				{
					Stop();
					return;
				}

				//If we started casting a spell, stop the timer and wait for
				//GameNPC.OnAfterSpellSequenceCast to start again
				if (owner.Brain is StandardMobBrain && ((StandardMobBrain)owner.Brain).CheckSpells(StandardMobBrain.eCheckSpellType.Offensive))
				{
					Stop();
					return;
				}
				else
				{
					//If we aren't a distance NPC, lets make sure we are in range to attack the target!
					if (owner.ActiveWeaponSlot != eActiveWeaponSlot.Distance && !owner.IsWithinRadius( owner.TargetObject, StickMinimumRange ) )
						((GameNPC)owner).Follow(owner.TargetObject, StickMinimumRange, StickMaximumRange);
				}
				Interval = 500;
			}
		}

		private const string LOSTEMPCHECKER = "LOSTEMPCHECKER";
		private const string LOSCURRENTSPELL = "LOSCURRENTSPELL";
		private const string LOSCURRENTLINE = "LOSCURRENTLINE";
		private const string LOSSPELLTARGET = "LOSSPELLTARGET";

		/// <summary>
		/// Does a check for a gameplayer to start gamenpcs have LOS checks
		/// </summary>
		/// <param name="spell"></param>
		/// <param name="line"></param>
		public override void CastSpell(Spell spell, SpellLine line)
		{
            // Aredhel: Waiting on the LOS check completely f* up Facilitate
            // Painworking timing, avoid LOS checks for necro pets AT ALL!

            if (this is NecromancerPet)
            {
                base.CastSpell(spell, line);
                return;
            }

			// Let's do a few checks to make sure it doesn't just wait on the LOS check
			int tempProp = TempProperties.getIntProperty(LOSTEMPCHECKER, 0);

			if (tempProp <= 0)
			{
				GamePlayer LOSChecker = TargetObject as GamePlayer;
				if (LOSChecker == null)
				{
					foreach (GamePlayer ply in GetPlayersInRadius(300))
					{
						if (ply != null)
						{
							LOSChecker = ply;
							break;
						}
					}
				}

				if (LOSChecker == null)
				{
					TempProperties.setProperty(LOSTEMPCHECKER, 0);
					base.CastSpell(spell, line);
				}
				else
				{
					TempProperties.setProperty(LOSTEMPCHECKER, 10);
					TempProperties.setProperty(LOSCURRENTSPELL, spell);
					TempProperties.setProperty(LOSCURRENTLINE, line);
					TempProperties.setProperty(LOSSPELLTARGET, TargetObject);
					LOSChecker.Out.SendCheckLOS(LOSChecker, this, new CheckLOSResponse(PetStartSpellAttackCheckLOS));
				}
			}
			else
				TempProperties.setProperty(LOSTEMPCHECKER, tempProp - 1);
			return;

		}

		public void PetStartSpellAttackCheckLOS(GamePlayer player, ushort response, ushort targetOID)
		{
			SpellLine line = (SpellLine)TempProperties.getObjectProperty(LOSCURRENTLINE, null);
			Spell spell = (Spell)TempProperties.getObjectProperty(LOSCURRENTSPELL, null);

			if ((response & 0x100) == 0x100 && line != null && spell != null)
			{
				GameObject target = (GameObject)TempProperties.getObjectProperty(LOSSPELLTARGET, null);
				GameObject lasttarget = TargetObject;
				TargetObject = target;
				base.CastSpell(spell, line);
				TargetObject = lasttarget;
			}
			else
				Notify(GameLivingEvent.CastFailed, this, new CastFailedEventArgs(null, CastFailedEventArgs.Reasons.TargetNotInView));

			TempProperties.setProperty(LOSSPELLTARGET, null);
			TempProperties.setProperty(LOSTEMPCHECKER, null);
			TempProperties.setProperty(LOSCURRENTLINE, null);
			TempProperties.setProperty(LOSTEMPCHECKER, 0);
		}

		#endregion
		#region Notify

		/// <summary>
		/// Handle event notifications
		/// </summary>
		/// <param name="e">The event</param>
		/// <param name="sender">The sender</param>
		/// <param name="args">The arguements</param>
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			base.Notify(e, sender, args);

			ABrain brain = Brain;
			if (brain != null)
				brain.Notify(e, sender, args);

            if (e == GameNPCEvent.ArriveAtTarget)
            {
                if (IsReturningToSpawnPoint)
                {
                    TurnTo(SpawnHeading);
                    IsReturningToSpawnPoint = false;
                }
            }
		}

		#endregion
		#region ControlledNPCs

		/// <summary>
		/// Gets the controlled object of this NPC
		/// </summary>
		public override IControlledBrain ControlledNpc
		{
			get
			{
				if (m_controlledNpc == null) return null;
				return m_controlledNpc[0];
			}
		}

		/// <summary>
		/// Gets the controlled array of this NPC
		/// </summary>
		public IControlledBrain[] ControlledNpcList
		{
			get { return m_controlledNpc; }
		}

		/// <summary>
		/// Adds a pet to the current array of pets
		/// </summary>
		/// <param name="controlledNpc">The brain to add to the list</param>
		/// <returns>Whether the pet was added or not</returns>
		public virtual bool AddControlledNpc(IControlledBrain controlledNpc)
		{
			return true;
		}

		/// <summary>
		/// Removes the brain from
		/// </summary>
		/// <param name="controlledNpc">The brain to find and remove</param>
		/// <returns>Whether the pet was removed</returns>
		public virtual bool RemoveControlledNpc(IControlledBrain controlledNpc)
		{
			return true;
		}

		#endregion

		/// <summary>
		/// Whether this NPC is available to add on a fight.
		/// </summary>
		public bool IsAvailable
		{
			get { return !(Brain is IControlledBrain) && !InCombat; }
		}

		/// <summary>
		/// Whether this NPC is aggressive.
		/// </summary>
		public bool IsAggressive
		{
			get
			{
				ABrain brain = Brain;
				return (brain == null) ? false : (brain is IOldAggressiveBrain);
			}
		}

		/// <summary>
		/// Whether this NPC is a friend or not.
		/// </summary>
		/// <param name="npc">The NPC that is checked against.</param>
		/// <returns></returns>
		public bool IsFriend(GameNPC npc)
		{
			if (Faction == null || npc.Faction == null)
				return false;
			else
				return (npc.Faction == Faction || Faction.FriendFactions.Contains(npc.Faction));
		}

		/// <summary>
		/// Broadcast loot to the raid.
		/// </summary>
		/// <param name="dropMessages">List of drop messages to broadcast.</param>
		protected void BroadcastLoot(ArrayList dropMessages)
		{
			if (dropMessages.Count > 0)
			{
				String lastMessage;
				foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				{
					lastMessage = "";
					foreach (string str in dropMessages)
					{
						// Suppress identical messages (multiple item drops).

						if (str != lastMessage)
						{
							player.Out.SendMessage(str, eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
							lastMessage = str;
						}
					}
				}
			}
		}

		/// <summary>
		/// Check whether current stats for this mob are sane.
		/// </summary>
		private void CheckStats()
		{
			// Mob stats should be a minimum of 30 each and strength should
			// be on par with that of a templated player.
			// This is only changed if values are 0 or less, allowing for unusually slow mobs

			for (eStat stat = eStat._First; stat <= eStat.CHR; ++stat)
				if (m_charStat[stat - eStat._First] <= 0) m_charStat[stat - eStat._First] = 30;

			// primary weapon stat for npc's is Strength and this should be adjusted for level and added to original template
			if (m_template != null && m_template.Strength < m_template.Strength + 20 + Level * 6)
				Strength = (short)(m_template.Strength + 20 + Level * 6);
			else if (Strength < (20 + Level * 6))
				Strength = (short)(20 + Level * 6);
		}

        /// <summary>
        /// Gender of this NPC.
        /// </summary>
        public override Gender Gender { get; set; }

		private string m_boatowner_id;
		/// <summary>
		/// Constructs a NPC
		/// </summary>
		public GameNPC()
			: base()
		{
			Level = 1; // health changes when GameNPC.Level changes
			m_Realm = 0;
			m_Name = "new Mob";
			m_Model = 408;
			//Fill the living variables
			//			CurrentSpeed = 0; // cause position addition recalculation
			MaxSpeedBase = 100;
			GuildName = "";
			m_healthRegenerationPeriod = 3000;

			m_brainSync = m_brains.SyncRoot;
			m_followTarget = new WeakRef(null);

			m_size = 50; //Default size
            Target = new Point3D();
			m_followMinDist = 100;
			m_followMaxDist = 3000;
			m_flags = 0;
			m_maxdistance = 0;
			m_roamingRange = -1; // default to normal roaming mob
			m_boatowner_id = "";

            if ( m_spawnPoint == null )
                m_spawnPoint = new Point3D();

			// Mob stats should be a minimum of 30 each and strength should
			// be on par with that of a templated player.

			if (Strength <= 0)
				Strength = (short)(20 + Level * 6);

			for (eStat stat = eStat._First; stat <= eStat.CHR; ++stat)
				if (m_charStat[stat - eStat._First] <= 0) m_charStat[stat - eStat._First] = 30;


			//m_factionName = "";
			LinkedFactions = new ArrayList(1);
			if (m_ownBrain == null)
			{
				m_ownBrain = new StandardMobBrain();
				m_ownBrain.Body = this;
			}
		}

		INpcTemplate m_template = null;

		/// <summary>
		/// create npc from template
		/// </summary>
		/// <param name="template">template of generator</param>
		public GameNPC(INpcTemplate template)
			: this()
		{
			if (template == null) return;

			// save the original template so we can do calculations off the original values
			m_template = template;

			// Load template.

			LoadTemplate(template);

			// Copy stats from template, we'll do a sanity check
			// afterwards.

			Strength = (short)template.Strength;
			Constitution = (short)template.Constitution;
			Dexterity = (short)template.Dexterity;
			Quickness = (short)template.Quickness;
			Intelligence = (short)template.Intelligence;
			Piety = (short)template.Piety;
			Charisma = (short)template.Charisma;
			Empathy = (short)template.Empathy;

			CheckStats();

			m_boatowner_id = "";
		}

	}
}
