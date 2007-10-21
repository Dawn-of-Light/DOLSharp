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
				base.Level = value;
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
        public short Constitution
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
        public short Dexterity
        {
            get { return m_charStat[eStat.DEX - eStat._First]; }
            set { m_charStat[eStat.DEX - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's strength
        /// </summary>
        public short Strength
        {
            get { return m_charStat[eStat.STR - eStat._First]; }
            set { m_charStat[eStat.STR - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's quickness
        /// </summary>
        public short Quickness
        {
            get { return m_charStat[eStat.QUI - eStat._First]; }
            set { m_charStat[eStat.QUI - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's intelligence
        /// </summary>
        public short Intelligence
        {
            get { return m_charStat[eStat.INT - eStat._First]; }
            set { m_charStat[eStat.INT - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's piety
        /// </summary>
        public short Piety
        {
            get { return m_charStat[eStat.PIE - eStat._First]; }
            set { m_charStat[eStat.PIE - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's empathy
        /// </summary>
        public short Empathy
        {
            get { return m_charStat[eStat.EMP - eStat._First]; }
            set { m_charStat[eStat.EMP - eStat._First] = value; }
        }

        /// <summary>
        /// Gets NPC's charisma
        /// </summary>
        public short Charisma
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
		/// Spawn X coordinate
		/// </summary>
		protected int m_spawnX;
		/// <summary>
		/// Spawn Y coordinate
		/// </summary>
		protected int m_spawnY;
		/// <summary>
		/// Spawn Z coordinate
		/// </summary>
		protected int m_spawnZ;
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
		public virtual int SpawnX
		{
			get { return m_spawnX; }
			set { m_spawnX = value; }
		}
		/// <summary>
		/// Gets or sets the spawnposition of this npc
		/// </summary>
		public virtual int SpawnY
		{
			get { return m_spawnY; }
			set { m_spawnY = value; }
		}
		/// <summary>
		/// Gets or sets the spawnposition of this npc
		/// </summary>
		public virtual int SpawnZ
		{
			get { return m_spawnZ; }
			set { m_spawnZ = value; }
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
				//Update the position before changing speed!
				m_X = X;
				m_Y = Y;
				m_Z = Z;
				base.CurrentSpeed = value;
				BroadcastUpdate();
			}
		}
        /*
		/// <summary>
		/// Gets sets the currentwaypoint that npc has to wander to
		/// </summary>
		public PathPoint CurrentWayPoint
		{
			get { return m_currentWayPoint; }
			set { m_currentWayPoint = value; }
		}
        */
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

				if (TargetX != 0 || TargetY != 0 || TargetZ != 0)
				{
					long diffnow = (long)FastMath.Abs((Environment.TickCount - MovementStartTick) * m_xAddition);
					long diffshould = FastMath.Abs((long)m_targetX - m_X);
					if (diffshould == 0) return m_targetX;

					if (diffshould - diffnow < 0)
					{
						return TargetX;
					}
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

				if (TargetX != 0 || TargetY != 0 || TargetZ != 0)
				{
					long diffnow = (long)FastMath.Abs((Environment.TickCount - MovementStartTick) * m_yAddition);
					long diffshould = FastMath.Abs((long)TargetY - m_Y);
					if (diffshould == 0) return TargetY;

					if (diffshould - diffnow < 0)
					{
						return TargetY;
					}
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

				if (TargetX != 0 || TargetY != 0 || TargetZ != 0)
				{
					long diffnow = (long)FastMath.Abs((Environment.TickCount - MovementStartTick) * m_zAddition);
					long diffshould = FastMath.Abs((long)TargetZ - m_Z);
					if (diffshould == 0) return TargetZ;

					if (diffshould - diffnow < 0)
					{
						return TargetZ;
					}
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
				return (Flags & (uint)eFlags.TRANSPARENT) != 0;//TODO
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
				return (TetherRange > 0)
					? !WorldMgr.CheckDistance(this, SpawnX, SpawnY, SpawnZ, TetherRange)
					: false;
			}
		}

		#endregion
		#region Movement
		/// <summary>
		/// Target X coordinate to walk to
		/// </summary>
		protected int m_targetX;
		/// <summary>
		/// Target Y coordinate to walk to
		/// </summary>
		protected int m_targetY;
		/// <summary>
		/// Target Z coordinate to walk to
		/// </summary>
		protected int m_targetZ;
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
		
		private string m_pathID;
		public string PathID
		{
			get { return m_pathID; }
			set { m_pathID = value; }
		}

		/// <summary>
		/// Recalculates position addition values of this living
		/// </summary>
		protected override void RecalculatePostionAddition()
		{
			if (!IsMoving)
			{
				//				log.ErrorFormat("{0} is not moving\n{1}", Name, Environment.StackTrace);
				m_xAddition = m_yAddition = m_zAddition = 0f;
				return;
			}

			if (TargetX != 0 || TargetY != 0 || TargetZ != 0)
			{
				float dist = WorldMgr.GetDistance(m_X, m_Y, m_Z, m_targetX, m_targetY, m_targetZ);
				if (dist <= 0)
				{
					m_xAddition = m_yAddition = m_zAddition = 0f;
					return;
				}
				float speed = CurrentSpeed;
				float diffx = (long)TargetX - m_X;
				float diffy = (long)TargetY - m_Y;
				float diffz = (long)TargetZ - m_Z;
				m_xAddition = (diffx * speed / dist) * 0.001f;
				m_yAddition = (diffy * speed / dist) * 0.001f;
				m_zAddition = (diffz * speed / dist) * 0.001f;
				//				log.WarnFormat("{0} is moving to target, dist = {1}, add = ({2} {3} {4})", Name, dist, m_xAddition, m_yAddition, m_zAddition);
			}
			else
			{
				//				log.WarnFormat("{0} is moving but target is 0", Name);
				base.RecalculatePostionAddition();
			}
		}

		/// <summary>
		/// Returns if the mob has arrived on its target
		/// </summary>
		/// <returns>true if on target</returns>
		public bool IsOnTarget()
		{
			if (X == TargetX && Y == TargetY && Z == TargetZ) return true;
			return false;
		}
		/// <summary>
		/// Gets or sets the TargetX
		/// </summary>
		public virtual int TargetX
		{
			get
			{
				return m_targetX;
			}
		}
		/// <summary>
		/// Gets or sets the TargetY
		/// </summary>
		public virtual int TargetY
		{
			get
			{
				return m_targetY;
			}
		}
		/// <summary>
		/// Gets or sets the TargetZ
		/// </summary>
		public virtual int TargetZ
		{
			get
			{
				return m_targetZ;
			}
		}

		/// <summary>
		/// Turns the npc towards a specific spot
		/// </summary>
		/// <param name="tx">Target X</param>
		/// <param name="ty">Target Y</param>
		public virtual void TurnTo(int tx, int ty)
		{
			if (this.IsStunned || this.IsMezzed) return;
			Notify(GameNPCEvent.TurnTo, this, new TurnToEventArgs(tx, ty));
			Heading = GetHeadingToSpot(tx, ty);
		}

		/// <summary>
		/// Turns the npc towards a specific heading
		/// </summary>
		/// <param name="newHeading">the new heading</param>
		public virtual void TurnTo(ushort newHeading)
		{
			if (this.IsStunned || this.IsMezzed) return;
			Notify(GameNPCEvent.TurnToHeading, this, new TurnToHeadingEventArgs(newHeading));
			Heading = newHeading;
		}
		/// <summary>
		/// Turns the NPC towards a specific gameObject
		/// which can be anything ... a player, item, mob, npc ...
		/// </summary>
		/// <param name="target">GameObject to turn towards</param>
		public virtual void TurnTo(GameObject target)
		{
			if (target == null) return;
			if (target.CurrentRegion != CurrentRegion) return;
			TurnTo(target.X, target.Y);
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
			if (target == null) return;
			if (target.CurrentRegion != CurrentRegion) return;
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
				npc.m_arriveAtTargetAction = null;
				npc.StopMoving();
				npc.Notify(GameNPCEvent.ArriveAtTarget, npc);
				if (npc.IsReturningHome
					&& npc.X == npc.SpawnX
					&& npc.Y == npc.SpawnY
					&& npc.Z == npc.SpawnZ)
					npc.IsReturningHome = false;
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
			public CloseToTargetAction(GameNPC actionSource)
				: base(actionSource)
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
		/// <param name="tx">target x</param>
		/// <param name="ty">target y</param>
		/// <param name="tz">target z (or 0 to put the mob on the ground)</param>
		/// <param name="speed">walk speed</param>
		public virtual void WalkTo(int tx, int ty, int tz, int speed)
		{
            // Walking to the spot we're already at will only get us into trouble.
            if (tx == X && ty == Y && tz == Z)
                return;

			if (IsTurningDisabled)
				return; // can't walk when turning is disabled

			if (IsMoving)
				StopMoving();

			//Slow mobs down when they are hurt!
			int maxSpeed = MaxSpeed;
			if (speed > maxSpeed)
				speed = maxSpeed;

			Notify(GameNPCEvent.WalkTo, this, new WalkToEventArgs(tx, ty, tz, speed));
			//Set our current position
			m_X = X;
			m_Y = Y;
			m_Z = Z;
			m_Heading = GetHeadingToSpot(tx, ty);
			m_currentSpeed = speed;
			m_targetX = tx;
			m_targetY = ty;
			m_targetZ = tz;
			RecalculatePostionAddition();

			//ARGHL!!!! the following took me 2 days to find out!
			//The mobs in the database have a Z value set ... but normally when
			//we call WalkTo, we set TargetZ to Zero, this results in a HUGE
			//distance bug, makeing the mobs move totally weird!
			//So we have to test if our targetZ == 0
			//duff answer : already test in get distance so do not need it!
			double dist = WorldMgr.GetDistance(m_X, m_Y, m_Z, m_targetX, m_targetY, m_targetZ);

			int timeToTarget = 0;
			if (speed > 0)
			{
				timeToTarget = (int)(dist * 1000 / speed);
			}

			if (m_arriveAtTargetAction != null)
			{
				m_arriveAtTargetAction.Stop();
			}

			m_arriveAtTargetAction = new ArriveAtTargetAction(this);
			if (timeToTarget > 1)
			{
				m_arriveAtTargetAction.Start(timeToTarget);
			}
			else
			{
				m_arriveAtTargetAction.Start(1);
			}

			if (m_closeToTargetAction != null)
			{
				m_closeToTargetAction.Stop();
			}

			m_closeToTargetAction = new CloseToTargetAction(this);
			if (timeToTarget > 200)
			{
				m_closeToTargetAction.Start(timeToTarget - 200); //200ms before target is close
			}
			else
			{
				m_closeToTargetAction.Start(1);
			}

			MovementStartTick = Environment.TickCount;
			BroadcastUpdate();
		}

		/// <summary>
		/// Walk to a certain point with given speed
		/// </summary>
		/// <param name="p"></param>
		/// <param name="speed"></param>
		public virtual void WalkTo(IPoint3D p, int speed)
		{
			WalkTo(p.X, p.Y, p.Z, speed);
		}

		/// <summary>
		/// Walk to the spawn point
		/// </summary>
		public virtual void WalkToSpawn()
		{
			WalkToSpawn((int)(MaxSpeed / 2.5));
		}

		/// <summary>
		/// Walk to the spawn point with specified speed
		/// </summary>
		public virtual void WalkToSpawn(int speed)
		{
			StopAttack();
			StopFollow();
			//WalkTo(SpawnX+Random(750)-350, SpawnY+Random(750)-350, SpawnZ, MaxSpeed/3);
			m_isReturningHome = true;
			WalkTo(SpawnX, SpawnY, SpawnZ, speed);
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

			if (m_arriveAtTargetAction != null)
			{
				m_arriveAtTargetAction.Stop();
				m_arriveAtTargetAction = null;
			}
			if (m_closeToTargetAction != null)
			{
				m_closeToTargetAction.Stop();
				m_closeToTargetAction = null;
			}

			m_X = X;
			m_Y = Y;
			m_Z = Z;

			m_targetX = 0;
			m_targetY = 0;
			m_targetZ = 0;

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
			if (m_arriveAtTargetAction != null)
			{
				m_arriveAtTargetAction.Stop();
				m_arriveAtTargetAction = null;
			}
			if (m_closeToTargetAction != null)
			{
				m_closeToTargetAction.Stop();
				m_closeToTargetAction = null;
			}
			//This broadcasts an update!
			CurrentSpeed = 0;
			m_isReturningHome = false;
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
			if (m_followTimer.IsAlive)
				m_followTimer.Stop();
			if (followTarget == null || followTarget.ObjectState != eObjectState.Active) return;
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
			lock (m_followTimer)
			{
				if (m_followTimer.IsAlive)
					m_followTimer.Stop();
				m_followTarget.Target = null;
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
			//sirru
			else if (m_attackers.Count == 0 && this.Spells.Count > 0 && this.TargetObject != null && GameServer.ServerRules.IsAllowedToAttack(this, (this.TargetObject as GameLiving), true))
			{
				if (TargetObject.Realm == 0 || Realm == 0)
					m_lastAttackTickPvE = m_CurrentRegion.Time;
				else m_lastAttackTickPvP = m_CurrentRegion.Time;
				if (this.CurrentRegion.Time - LastAttackedByEnemyTick > 10 * 1000)
				{
					//Check for negatively casting spells
					StandardMobBrain stanBrain = (StandardMobBrain)Brain;
					if (stanBrain != null)
						((StandardMobBrain)stanBrain).CheckSpells(false);
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
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				return 0;
			}

			//Stop following if we have no target
			if (followTarget == null || followTarget.ObjectState != eObjectState.Active || CurrentRegionID != followTarget.CurrentRegionID)
			{
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				return 0;
			}

			//Calculate the difference between our position and the players position
			float diffx = (long)followTarget.X - X;
			float diffy = (long)followTarget.Y - Y;
			float diffz = (long)followTarget.Z - Z;

			//SH: Removed Z checks when one of the two Z values is zero(on ground)
			float distance = 0;
			if (followTarget.Z == 0 || Z == 0)
				distance = (float)Math.Sqrt(diffx * diffx + diffy * diffy);
			else
				distance = (float)Math.Sqrt(diffx * diffx + diffy * diffy + diffz * diffz);

			//if distance is greater then the max follow distance, stop following and return home
			if (distance > m_followMaxDist)
			{
				StopFollow();
				Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
				this.WalkToSpawn();
				return 0;
			}
			int newX, newY, newZ;

			 //Check for any formations
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
							StopFollow();
							Notify(GameNPCEvent.FollowLostTarget, this, new FollowLostTargetEventArgs(followTarget));
							brain.ClearAggroList();
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

			//Are we in range yet?
			if (distance <= m_followMinDist)
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
                StopMoveOnPath();

            if (CurrentWayPoint == null)
            {
                if (log.IsWarnEnabled)
                    log.Warn("No path to travel on for " + Name);
                return;
            }
            PathingNormalSpeed = speed;

            //if (Point3D.GetDistance(npc.CurrentWayPoint, npc)<100)
            //not sure because here use point3D get distance but why??
            if (WorldMgr.CheckDistance(CurrentWayPoint, this, 100))
            {
                if (CurrentWayPoint.Type == ePathType.Path_Reverse && CurrentWayPoint.FiredFlag)
                    CurrentWayPoint = CurrentWayPoint.Prev;
                else
                    CurrentWayPoint = CurrentWayPoint.Next;
                if ((CurrentWayPoint.Type == ePathType.Loop) && (CurrentWayPoint.Next == null))
                {
                    CurrentWayPoint = MovementMgr.FindFirstPathPoint(CurrentWayPoint);
                }
            }
            if (CurrentWayPoint != null)
            {
                GameEventMgr.AddHandler(this, GameNPCEvent.CloseToTarget, new DOLEventHandler(OnCloseToWaypoint));
                WalkTo(CurrentWayPoint, Math.Min(speed, CurrentWayPoint.MaxSpeed));
                m_IsMovingOnPath = true;
				Notify(GameNPCEvent.PathMoveStarts, this);
            }
            else
            {
                StopMoveOnPath();
            }
        }

        /// <summary>
        /// Stop move on path
        /// </summary>
        public void StopMoveOnPath()
        {
            if (!IsMovingOnPath)
                return;

            GameEventMgr.RemoveHandler(this, GameNPCEvent.CloseToTarget, new DOLEventHandler(OnCloseToWaypoint));
            Notify(GameNPCEvent.PathMoveEnds, this);
            m_IsMovingOnPath = false;
        }

        /// <summary>
        /// decides what to do on reached waypoint in path
        /// </summary>
        /// <param name="e"></param>
        /// <param name="n"></param>
        /// <param name="args"></param>
        protected void OnCloseToWaypoint(DOLEvent e, object n, EventArgs args)
        {
            if (!IsMovingOnPath || n != this)
                return;

            if (CurrentWayPoint != null)
            {
                WaypointDelayAction waitTimer = new WaypointDelayAction(this);
                waitTimer.Start(Math.Max(1, CurrentWayPoint.WaitTime * 100));
            }
            else
                StopMoveOnPath();
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
                    npc.StopMoveOnPath();
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
					if (log.IsDebugEnabled)
					{
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
		public override void LoadFromDatabase(DataObject obj)
		{
			if (obj == null) return;
			base.LoadFromDatabase(obj);
			if (!(obj is Mob)) return;
			Mob npc = (Mob)obj;
			INpcTemplate npcTemplate = NpcTemplateMgr.GetTemplate(npc.NPCTemplateID);
			if (npcTemplate != null)
				LoadTemplate(npcTemplate);
			Name = npc.Name;
			GuildName = npc.Guild;
			m_X = npc.X;
			m_Y = npc.Y;
			m_Z = npc.Z;
			m_Heading = (ushort)(npc.Heading & 0xFFF);
			m_maxSpeedBase = npc.Speed;
			m_currentSpeed = 0;
			CurrentRegionID = npc.Region;
			Realm = npc.Realm;
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
				asms.AddRange(Scripts.ScriptMgr.Scripts);
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
			
			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
			if (aggroBrain != null)
			{
				if (npc.AggroRange == 0)
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
					aggroBrain.AggroLevel = npc.AggroLevel;
					aggroBrain.AggroRange = npc.AggroRange;
				}
			}
			m_bodyType = npc.BodyType;
			m_houseNumber = npc.HouseNumber;
			m_maxdistance = npc.MaxDistance;
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
				mob = new Mob();

			mob.Name = Name;
			mob.Guild = GuildName;
			mob.X = X;
			mob.Y = Y;
			mob.Z = Z;
			mob.Heading = Heading;
			mob.Speed = MaxSpeedBase;
			mob.Region = CurrentRegionID;
			mob.Realm = Realm;
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
			if (Brain.GetType().FullName != typeof(StandardMobBrain).FullName)
				mob.Brain = Brain.GetType().FullName;
			IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
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
			GameNpcInventoryTemplate equip = new GameNpcInventoryTemplate();
			this.Name = template.Name;
			this.GuildName = template.GuildName;
			foreach (string str in template.Model.Split(';'))
			{
				if (str.Length == 0) continue;
				ushort i = ushort.Parse(str);
				m_models.Add(i);
			}
			this.Model = (ushort)m_models[Util.Random(m_models.Count - 1)];

			byte size = 50;
			if (!Util.IsEmpty(template.Size))
			{
				string[] splitSize = template.Size.Split(';');
				if (splitSize.Length == 1)
					size = byte.Parse(splitSize[0]);
				else size = (byte)Util.Random(int.Parse(splitSize[0]), int.Parse(splitSize[1]));
			}
			this.Size = size;

			byte level = 0;
			if (!Util.IsEmpty(template.Level))
			{
				string[] splitLevel = template.Level.Split(';');
				if (splitLevel.Length == 1)
					level = byte.Parse(splitLevel[0]);
				else level = (byte)Util.Random(int.Parse(splitLevel[0]), int.Parse(splitLevel[1]));
			}
			this.Level = level;

            // Stats
            this.Constitution = (short)template.Constitution;
            this.Dexterity = (short)template.Dexterity;
            this.Strength = (short)template.Strength;
            this.Quickness = (short)template.Quickness;
            this.Intelligence = (short)template.Intelligence;
            this.Piety = (short)template.Piety;
            this.Empathy = (short)template.Empathy;
            this.Charisma = (short)template.Charisma;

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
			if (template.Inventory != null && template.Inventory.Length > 0)
			{
				m_models.Clear();
				int x = 0;
				int y = 0;
				foreach (string str in template.Inventory.Split(';'))
				{
					//Get the armor location
					string[] loc = str.Split(':');
					x = Convert.ToInt32(loc[0]);
					if (!str.Contains("|")) y = Convert.ToInt32(loc[1]);
					if (x == 10 || x == 12 || x == 13)
					{
						m_equipLoc.Add(x);
						//Get the Equipment model
						m_equipModel.Add(x, loc[1]);
					}
					else equip.AddNPCEquipment((DOL.GS.eInventorySlot)x, y);

				}
				if (m_equipLoc.Count > 1)
				{
					x = Util.Random(m_equipLoc.Count - 1);
					x = Convert.ToInt32(m_equipLoc[x]);
				}
				if (m_equipModel.ContainsKey(x))
				{
					string str = m_equipModel[x].ToString();
					if (!str.Contains("|")) y = Convert.ToInt32(m_equipModel[x]);
					else
					{
						foreach (string st in str.Split('|'))
						{
							if (st.Length == 0) continue;
							y = Convert.ToInt32(st);
							m_models.Add(y);
						}
						y = Util.Random(m_models.Count - 1);
						y = Convert.ToInt32(m_models[y]);
					}
					equip.AddNPCEquipment((DOL.GS.eInventorySlot)x, y);
				}
				this.Inventory = new GameNPCInventory(equip);
			}
			if (this.Inventory != null)
			{
				if (this.Inventory.GetItem(eInventorySlot.DistanceWeapon) != null)
					this.SwitchWeapon(eActiveWeaponSlot.Distance);
				else if (Inventory.GetItem(eInventorySlot.TwoHandWeapon) != null)
					this.SwitchWeapon(eActiveWeaponSlot.TwoHanded);
			}
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

			m_ownBrain = (Name.EndsWith("retriever")
				|| Name.EndsWith("messenger")) // Don't like this hack, but can't specify a brain in npctemplate at the moment
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
		private AbstractQuest HasQuest(Type questType)
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
			m_spawnX = X;
			m_spawnY = Y;
			m_spawnZ = Z;
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
                CurrentWayPoint = path;
                MoveOnPath(path.MaxSpeed);
            }

			if (m_houseNumber > 0)
			{
				log.Info("NPC '" + Name + "' added to house N°" + m_houseNumber);
				CurrentHouse = HouseMgr.GetHouse(m_houseNumber);
				if (CurrentHouse == null)
					log.Warn("House " + CurrentHouse + " for NPC " + Name + " doesn't exist !!!");
				else
					log.Info("Confirmed number: " + CurrentHouse.HouseNumber.ToString());
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
				StopMoveOnPath();
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
			m_X = x;
			m_Y = y;
			m_Z = z;
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
				IAggressiveBrain aggroBrain = Brain as IAggressiveBrain;
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
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = base.GetExamineMessages(player);
			list.Add(LanguageMgr.GetTranslation(player.Client, "GameNPC.GetExamineMessages.YouExamine", GetName(0, false), GetPronoun(0, true), GetAggroLevelString(player, false)));
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
		#region Interact/SayTo
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
				player.Out.SendMessage(GetName(0, true) + " gives you a dirty look.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					player.Out.SendMessage("You are already riding this " + name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}

				if (GetFreeArrayLocation() == -1)
				{
					player.Out.SendMessage("This " + name + " is full.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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

			switch (loc)
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

		private bool m_canFight = true;

		/// <summary>
		/// Can this NPC engage in melee
		/// </summary>
		public bool CanFight
		{
			get { return m_canFight; }
			set { m_canFight = value; }
		}

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
			if (!CanFight) return;

            if (IsMovingOnPath)
                StopMoveOnPath();

			if (this.Brain is IControlledBrain)
			{
				if ((this.Brain as IControlledBrain).AggressionState == eAggressionState.Passive)
					return;
				GamePlayer playerowner = null;
				if ((playerowner = ((IControlledBrain)Brain).GetPlayerOwner()) != null)
					playerowner.Stealth(false);
			}

			TargetObject = attackTarget;
			if (TargetObject.Realm == 0 || Realm == 0)
				m_lastAttackTickPvE = m_CurrentRegion.Time;
			else
				m_lastAttackTickPvP = m_CurrentRegion.Time;

			//These checks are now handled in CheckSpells inside a spell action.  We need an attacking mob to have a SpellAction timer
			if (m_attackers.Count == 0 /*&& this.Spells.Count > 0 && this.CurrentRegion.Time - LastAttackedByEnemyTick > 10 * 1000*/)
			{
				if (SpellTimer == null)
					SpellTimer = new SpellAction(this);
				if (!SpellTimer.IsAlive)
					SpellTimer.Start(1);
			}
			base.StartAttack(attackTarget);

			if (AttackState)
			{
				if (ActiveWeaponSlot == eActiveWeaponSlot.Distance)
				{
					if (IsMoving)
						StopFollow();
					//Follow(attackTarget, 1000, 5000);	// follow at archery range
				}
				else
					Follow(attackTarget, 90, 5000);	// follow at stickrange
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
			if (IsWorthReward)
				DropLoot(killer);

			Message.SystemToArea(this, GetName(0, true) + " dies!", eChatType.CT_PlayerDied, killer);
			if (killer is GamePlayer)
				((GamePlayer)killer).Out.SendMessage(GetName(0, true) + " dies!", eChatType.CT_PlayerDied, eChatLoc.CL_SystemWindow);
			StopFollow();
			base.Die(killer);

			// deal out exp and realm points based on server rules
			GameServer.ServerRules.OnNPCKilled(this, killer);

			Delete();

			if ((Faction != null) && (killer is GamePlayer))
			{
				GamePlayer player = killer as GamePlayer;
				Faction.KillMember(player);
			}

			// remove temp properties
			TempProperties.RemoveAll();

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
			else SwitchWeapon(eActiveWeaponSlot.Standard);
			StopAttack();
			StopMoving();
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
			StopFollow();
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
					if (WorldMgr.GetDistance(this, attacker) > 150)
						return false;
				}
			}

			StopAttack();
			SwitchToMelee(attacker);

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
				if (m_respawnInterval > 0)
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
		/// Is the mob alive
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
		/// Is the mob respawning
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
			int origSpawnX = m_spawnX;
			int origSpawnY = m_spawnY;
			//X=(m_spawnX+Random(750)-350); //new SpawnX = oldSpawn +- 350 coords
			//Y=(m_spawnY+Random(750)-350);	//new SpawnX = oldSpawn +- 350 coords
			X = m_spawnX;
			Y = m_spawnY;
			Z = m_spawnZ;
			AddToWorld();
			m_spawnX = origSpawnX;
			m_spawnY = origSpawnY;
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
			if (!InCombat || Util.Chance(50)) // mobs have only 50% chance to heal itself 
			{
				period = base.HealthRegenerationTimerCallback(selfRegenerationTimer);
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
		/// Stops all attack actions, including following target
		/// </summary>
		public override void StopAttack()
		{
			base.StopAttack();
			StopFollow();
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
								killerPlayer.Out.SendMessage("You find an additional " + Money.GetString(value - lootTemplate.Value) + " thanks to your realm owning outposts!", eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
						}

						loot = new GameMoney(value, this);
						loot.Name = lootTemplate.Name;
						loot.Model = (ushort)lootTemplate.Model;
					}
					else
					{
						loot = new GameInventoryItem(new InventoryItem(lootTemplate));
						loot.X = X;
						loot.Y = Y;
						loot.Z = Z;
						loot.Heading = Heading;
						loot.CurrentRegion = CurrentRegion;
						if (((GameInventoryItem)loot).Item.Id_nb == "aurulite")
						{
							((GameInventoryItem)loot).Item.Count = ((GameInventoryItem)loot).Item.PackSize;
						}
					}

					bool playerAttacker = false;
					foreach (GameObject gainer in m_xpGainers.Keys)
					{
						if (gainer is GamePlayer)
						{
							playerAttacker = true;
							if (loot.Realm == 0)
								loot.Realm = ((GamePlayer)gainer).Realm;
						}
						loot.AddOwner(gainer);
						if (gainer is GameNPC)
						{
							IControlledBrain brain = ((GameNPC)gainer).Brain as IControlledBrain;
							if (brain != null)
							{
								playerAttacker = true;
								loot.AddOwner(brain.Owner);
							}
						}
					}
					if (!playerAttacker) return; // no loot if mob kills another mob

					message = String.Format("{0} drops {1}.",
						GetName(0, true),
						char.IsLower(loot.Name[0]) ? loot.GetName(1, false) : loot.Name);
						
					dropMessages.Add(message);
					loot.AddToWorld();

					foreach (GameObject gainer in m_xpGainers.Keys)
					{
						if (gainer is GamePlayer)
						{
							GamePlayer player = gainer as GamePlayer;
							if (player.Autoloot && WorldMgr.CheckDistance(loot, player, 700))
							{
								if (player.PlayerGroup == null || (player.PlayerGroup != null && player == player.PlayerGroup.Leader))
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

			GamePlayer attackerPlayer = healSource as GamePlayer;
			if (attackerPlayer == null)
				return;

			PlayerGroup attackerGroup = attackerPlayer.PlayerGroup;
			if (attackerGroup != null)
			{
				ArrayList xpGainers = new ArrayList(8);
				lock (attackerGroup)
				{
					// collect "helping" group players in range
					for (int i = 0; i < attackerGroup.PlayerCount; i++)
					{
						GamePlayer player = attackerGroup[i];
						if (WorldMgr.CheckDistance(player, this, WorldMgr.MAX_EXPFORKILL_DISTANCE) && player.IsAlive && player.ObjectState == eObjectState.Active)
							xpGainers.Add(player);
					}
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

		//Checks if the target already has the effect
		private bool HasEffect(GameLiving target, Spell spell)
		{
			foreach (IGameEffect effect in target.EffectList)
			{
				if (effect is GameSpellEffect)
				{
					GameSpellEffect speffect = effect as GameSpellEffect;
					if (speffect.Spell.SpellType == spell.SpellType
						&& speffect.Spell.EffectGroup == spell.EffectGroup)
					{
						return true;
					}
				}
			}
			return false;
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
				if (((StandardMobBrain)owner.Brain).CheckSpells(false))
				{
					Stop();
					return;
				}
				else
				{
					//If we aren't a distance NPC, lets make sure we are in range to attack the target!
					if (WorldMgr.GetDistance(owner, owner.TargetObject) > 90)
						((GameNPC)owner).Follow(owner.TargetObject, 90, 5000);
				}
				Interval = 500;
			}
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
		}

		#endregion
		#region ControlledNPCs
		/// <summary>
		/// Gets all the minions/subpets of this NPC - uses recursion to find all of its
		/// pet's pets
		/// </summary>
		/// <returns>List of all pets</returns>
		public List<GameNPC> GetAllPets()
		{
			//No pets, return null
			if (ControlledNpcList == null)
				return null;

			List<GameNPC> pets = new List<GameNPC>();

			foreach (IControlledBrain icb in ControlledNpcList)
			{
				//Is this really a necessary check?
				if (icb == null)
					continue;
				//Add this pet
				pets.Add(icb.Body);
				//Add this pet's pets
				List<GameNPC> petsSubPets = icb.Body.GetAllPets();
				if (petsSubPets != null)
					pets.AddRange(petsSubPets);
			}
			return pets;
		}

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
		/// Gets the controlled object of this player at the position in the array
		/// </summary>
		/// <param name="pet_position"></param>
		public IControlledBrain ControlledMinion(int pet_position)
		{
			return m_controlledNpc[pet_position];
		}

		/// <summary>
		/// Adds a pet to the current array of pets
		/// </summary>
		/// <param name="controlledNpc">The brain to add to the list</param>
		/// <returns>Whether the pet was added or not</returns>
		public bool AddControlledNpc(IControlledBrain controlledNpc)
		{
			IControlledBrain[] brainlist = ControlledNpcList;
			foreach (IControlledBrain icb in brainlist)
			{
				if (icb == controlledNpc)
					return false;
			}

			if (controlledNpc.Owner != this)
				throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");
			//Find the next spot for this new pet
			int i = 0;
			for (; i < m_controlledNpc.Length; i++)
			{
				if (ControlledMinion(i) == null)
					break;
			}
			//If we didn't find a spot return false
			if (i >= m_controlledNpc.Length)
				return false;
			m_controlledNpc[i] = controlledNpc;
			PetCounter++;
			return true;
		}

		/// <summary>
		/// Removes the brain from 
		/// </summary>
		/// <param name="controlledNpc">The brain to find and remove</param>
		/// <returns>Whether the pet was removed</returns>
		public bool RemoveControlledNpc(IControlledBrain controlledNpc)
		{
			if (controlledNpc == null) return false;
			IControlledBrain[] brainlist = ControlledNpcList;
			int i = 0;
			bool found = false;
			//Try to find the minion in the list
			for (; i < brainlist.Length; i++)
			{
				//Found it
				if (brainlist[i] == controlledNpc)
				{
					found = true;
					break;
				}
			}
			lock (ControlledNpcList)
			{
				//Found it, lets remove it
				if (found)
				{
					//TODO: this prints two messages - what's a better way to do this
					if (m_controlledNpc[i].Body.IsAlive)
						m_controlledNpc[i].Body.Die(this);
					m_controlledNpc[i] = null;

					//Only decrement, we just lost one pet
					PetCounter--;
				}
			}

			return found;
		}

		///// <summary>
		///// Sets the controlled minions for the player's commander
		///// </summary>
		///// <remarks>This function should only be used for making minions</remarks>
		///// <param name="controlledNpc"></param>
		///// <param name="pet_position">When a pet dies, we need to know which one</param>
		//public bool SetControlledNpc(IControlledBrain controlledNpc, int pet_position)
		//{
		//    if (pet_position >= m_controlledNpc.Length || pet_position < 0) return false;
		//    if (controlledNpc == ControlledMinion(pet_position)) return false;
		//    if (controlledNpc == null)
		//    {
		//        if (m_controlledNpc[pet_position] == null)
		//            return false;
		//        if(m_controlledNpc[pet_position].Body.IsAlive)
		//            m_controlledNpc[pet_position].Body.Die(this);
		//        m_controlledNpc[pet_position] = null;

		//        //Only decrement, we just lost one pet
		//        PetCounter--;
		//        return true;
		//    }
		//    else
		//    {
		//        if (controlledNpc.Owner != this)
		//            throw new ArgumentException("ControlledNpc with wrong owner is set (player=" + Name + ", owner=" + controlledNpc.Owner.Name + ")", "controlledNpc");
		//        //Find the next spot for this new pet
		//        int i = 0;
		//        for (; i < m_controlledNpc.Length; i++)
		//        {
		//            if (ControlledMinion(i) == null)
		//                break;
		//        }
		//        //If we didn't find a spot return false
		//        if (i >= m_controlledNpc.Length)
		//            return false;
		//        m_controlledNpc[i] = controlledNpc;
		//        PetCounter++;
		//        return true;
		//    }
		//}

		/// <summary>
		/// Commands controlled object to attack
		/// </summary>
		public override void CommandNpcAttack()
		{
			if (Brain is IControlledBrain)
			{
				//edit for BD
				//Make the minion's attack
				if (!CanFight) return;
				if (WorldMgr.GetDistance(TargetObject, this) >= 500)
				{
					ChargeAbility charge;
					if ((charge = (ChargeAbility)GetAbility(typeof(ChargeAbility))) != null && GetSkillDisabledDuration(charge) == 0)
					{
						charge.Execute(this);
						//Out.SendMessage("Your " + icb.Body.Name + " begins to charge " + target.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				((IControlledBrain)Brain).Attack(TargetObject);
			}
		}

		/// <summary>
		/// Releases controlled object
		/// </summary>
		/// <remarks>This works differently than GamePlayer's.  Since an npc can have more than one pet,
		/// we don't call owner.CommandNpcRelease().  Instead we call a body.CommandNpcRelease(), it gets its owner, finds its place in the array and then removes ITSELF!</remarks>
		public override void CommandNpcRelease()
		{
			IControlledBrain npc = Brain as IControlledBrain;
			//This method shouldn't be called if this is the player's main pet.  Only minions and subpets
			//should call this method.
			if (npc == null || !npc.IsMinion) return;
			GameNPC owner = (GameNPC)npc.Owner;

			//Added function in GameNPC to kill the pet for us
			owner.RemoveControlledNpc(npc);

			Notify(GamePlayerEvent.CommandNpcRelease, this);
		}

		/// <summary>
		/// Commands controlled object to follow
		/// </summary>
		public override void CommandNpcFollow()
		{
			if (Brain is IControlledBrain)
				((IControlledBrain)Brain).Follow(((IControlledBrain)Brain).Owner);
		}

		/// <summary>
		/// Commands controlled object to stay where it is
		/// </summary>
		public override void CommandNpcStay()
		{
			if (Brain is IControlledBrain)
				((IControlledBrain)Brain).Follow(((IControlledBrain)Brain).Owner);
		}

		/// <summary>
		/// Commands controlled object to go to players location
		/// </summary>
		public override void CommandNpcComeHere()
		{
			if (Brain is IControlledBrain)
				((IControlledBrain)Brain).Follow(((IControlledBrain)Brain).Owner);
		}

		/// <summary>
		/// Commands controlled object to go to target
		/// </summary>
		public override void CommandNpcGoTarget()
		{
			if (Brain is IControlledBrain)
				((IControlledBrain)Brain).Follow(((IControlledBrain)Brain).Owner);
		}

		/// <summary>
		/// Changes controlled object state to passive
		/// </summary>
		public override void CommandNpcPassive()
		{
			if (Brain is IControlledBrain)
			{
				((IControlledBrain)Brain).AggressionState = eAggressionState.Passive;
				StopAttack();
				StopCurrentSpellcast();
			}
		}

		/// <summary>
		/// Changes controlled object state to aggressive
		/// </summary>
		public override void CommandNpcAgressive()
		{
			if (Brain is IControlledBrain && Brain.Body.CanFight)
				((IControlledBrain)Brain).AggressionState = eAggressionState.Aggressive;
		}

		/// <summary>
		/// Changes controlled object state to defensive
		/// </summary>
		public override void CommandNpcDefensive()
		{
			if (Brain is IControlledBrain && Brain.Body.CanFight)
				((IControlledBrain)Brain).AggressionState = eAggressionState.Defensive;
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
				return (brain == null) ? false : (brain is IAggressiveBrain);
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
        private void BroadcastLoot(ArrayList dropMessages)
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

			for (eStat stat = eStat._First; stat <= eStat.CHR; ++stat)
				if (m_charStat[stat - eStat._First] <= 0) m_charStat[stat - eStat._First] = 30;

			if (Strength < (20 + Level * 6))
				Strength = (short)(20 + Level * 6);
		}

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
			m_targetX = 0;
			m_targetY = 0;
			m_followMinDist = 100;
			m_followMaxDist = 3000;
			m_flags = 0;
			m_maxdistance = 0;
			m_boatowner_id = "";

			// Mob stats should be a minimum of 30 each and strength should
			// be on par with that of a templated player.

			for (eStat stat = eStat._First; stat <= eStat.CHR; ++stat)
				m_charStat[stat - eStat._First] = 30;

			Strength = (short)(20 + Level * 6);

			//m_factionName = "";
			LinkedFactions = new ArrayList(1);
			if (m_ownBrain == null)
			{
				m_ownBrain = new StandardMobBrain();
				m_ownBrain.Body = this;
			}
		}

		/// <summary>
		/// create npc from template
		/// </summary>
		/// <param name="template">template of generator</param>
		public GameNPC(INpcTemplate template)
			: this()
		{
			if (template == null) return;

			// Load template.

			LoadTemplate(template);

			// Copy stats from template, we'll do a sanity check
			// afterwards.

			Strength = (short) template.Strength;
			Constitution = (short) template.Constitution;
			Dexterity = (short)template.Dexterity;
			Quickness = (short) template.Quickness;
			Intelligence = (short) template.Intelligence;
			Piety = (short) template.Piety;
			Charisma = (short) template.Charisma;
			Empathy = (short) template.Empathy;
			CheckStats();

            m_boatowner_id = "";
        }
	}
}
