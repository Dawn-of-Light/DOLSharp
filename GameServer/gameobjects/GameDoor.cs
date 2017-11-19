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
using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using System.Threading;

namespace DOL.GS
{
    /// <summary>
    /// GameDoor is class for regular door
    /// </summary>
    public class GameDoor : GameLiving, IDoor
	{
		private bool m_openDead = false;
		private static Timer m_timer;
		protected volatile uint m_lastUpdateTickCount = uint.MinValue;
		private readonly object m_LockObject = new object();
		private uint m_flags = 0;


		/// <summary>
		/// The time interval after which door will be closed, in milliseconds
		/// On live this is usually 5 seconds
		/// </summary>
		protected const int CLOSE_DOOR_TIME = 8000;
		/// <summary>
		/// The timed action that will close the door
		/// </summary>
		protected GameTimer m_closeDoorAction;

		/// <summary>
		/// Creates a new GameDoor object
		/// </summary>
		public GameDoor()
			: base()
		{
			m_state = eDoorState.Closed;
			m_model = 0xFFFF;
		}
		
		/// <summary>
		/// Loads this door from a door table slot
		/// </summary>
		/// <param name="obj">DBDoor</param>
		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			DBDoor m_dbdoor = obj as DBDoor;
			if (m_dbdoor == null) return;
			Zone curZone = WorldMgr.GetZone((ushort)(m_dbdoor.InternalID / 1000000));
			if (curZone == null) return;
            CurrentRegion = curZone.ZoneRegion;
			m_name = m_dbdoor.Name;
			m_Heading = (ushort)m_dbdoor.Heading;
			X = m_dbdoor.X;
			Y = m_dbdoor.Y;
			Z = m_dbdoor.Z;
			m_level = 0;
			m_model = 0xFFFF;
			m_doorID = m_dbdoor.InternalID;
            m_guildName = m_dbdoor.Guild;
            m_Realm = (eRealm)m_dbdoor.Realm;
            m_level = m_dbdoor.Level;
            m_health = m_dbdoor.MaxHealth;
            m_maxHealth = m_dbdoor.MaxHealth;
			m_locked = m_dbdoor.Locked;
			m_flags = m_dbdoor.Flags;

            AddToWorld();
		}
		/// <summary>
		/// save this door to a door table slot
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBDoor obj = null;
			if (InternalID != null)
				obj = GameServer.Database.FindObjectByKey<DBDoor>(InternalID);
			if (obj == null)
				obj = new DBDoor();
			obj.Name = Name;
			obj.InternalID = DoorID;
			obj.Type = DoorID / 100000000;
            obj.Guild = GuildName;
			obj.Flags = Flag;
            obj.Realm = (byte)Realm;
            obj.Level = Level;
            obj.MaxHealth = MaxHealth;
			obj.Health = MaxHealth;
			obj.Locked = Locked;
			if (InternalID == null)
			{
				GameServer.Database.AddObject(obj);
				InternalID = obj.ObjectId;
			}
			else
				GameServer.Database.SaveObject(obj);
		}

		#region Properties

		private int m_locked;
		/// <summary>
		/// door open = 0 / lock = 1 
		/// </summary>
		public virtual int Locked
		{
			get { return m_locked; }
			set { m_locked = value; }
		}

		/// <summary>
		/// this hold the door index which is unique
		/// </summary>
		private int m_doorID;

		/// <summary>
		/// door index which is unique
		/// </summary>
		public virtual int DoorID
		{
			get { return m_doorID; }
			set { m_doorID = value; }
		}

		/// <summary>
		/// Get the ZoneID of this door
		/// </summary>
		public virtual ushort ZoneID
		{
			get { return (ushort)(DoorID / 1000000); }
		}

		private int m_type;

		/// <summary>
		/// Door Type
		/// </summary>
		public virtual int Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		/// <summary>
		/// This is used to identify what sound a door makes when open / close
		/// </summary>
		public virtual uint Flag
		{
			get { return m_flags; }
			set { m_flags = value; }
		}

		/// <summary>
		/// This hold the state of door
		/// </summary>
		protected eDoorState m_state;

		/// <summary>
		/// The state of door (open or close)
		/// </summary>
		public virtual eDoorState State
		{
			get { return m_state; }
			set
			{
				if (m_state != value)
				{
					lock (m_LockObject)
					{
						m_state = value;
						foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.SendDoorUpdate(this);
						}
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Call this function to open the door
		/// </summary>
		public virtual void Open(GameLiving opener = null)
		{
			if (Locked == 0)
                State = eDoorState.Open;
			
			if (HealthPercent > 40 || !m_openDead)
			{
				lock (m_LockObject)
				{
					if (m_closeDoorAction == null)
					{
						m_closeDoorAction = new CloseDoorAction(this);
					}
					m_closeDoorAction.Start(CLOSE_DOOR_TIME);
				}
			}
		}

		public virtual byte Status
		{
			get
			{
			//	if( this.HealthPercent == 0 ) return 0x01;//broken
				return 0x00;
			}
		}

		/// <summary>
		/// Call this function to close the door
		/// </summary>
		public virtual void Close(GameLiving closer = null)
		{
			if (!m_openDead)
                State = eDoorState.Closed;
			m_closeDoorAction = null;
		}

		/// <summary>
		/// Allow a NPC to manipulate the door
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="open"></param>
		public virtual void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{
			npc.TurnTo(X, Y);
			if (open && m_state != eDoorState.Open)
                Open();
			else if (!open && m_state != eDoorState.Closed)
                Close();

		}
		
		public override int Health
		{
			get { return m_health; }
			set
			{

				int maxhealth = MaxHealth;
				if( value >= maxhealth )
				{
					m_health = maxhealth;

					lock( m_xpGainers.SyncRoot )
					{
						m_xpGainers.Clear( );
					}
				}
				else if( value > 0 )
				{
					m_health = value;
				}
				else
				{
					m_health = 0;
				}

				if( IsAlive && m_health < maxhealth )
				{
					StartHealthRegeneration( );
				}
			}
		}
		
		/// <summary>
		/// Get the solidity of the door
		/// </summary>
		public override int MaxHealth
		{
			get {	return 5 * GetModified(eProperty.MaxHealth);}
		}
		
		/// <summary>
		/// No regeneration over time of the door
		/// </summary>
		/// <param name="killer"></param>
		public override void Die(GameObject killer)
		{
			base.Die(killer);
			StartHealthRegeneration();
		}

        /// <summary>
		/// Broadcasts the Door Update to all players around
		/// </summary>
		public override void BroadcastUpdate()
		{
			base.BroadcastUpdate();
			
			m_lastUpdateTickCount = (uint)Environment.TickCount;
		}
		
		private static long m_healthregentimer = 0;
		
		public virtual void RegenDoorHealth ()
		{
			Health = 0;
			if (Locked == 0)
				Open();
			
			m_healthregentimer = 9999;
			m_timer = new Timer(new TimerCallback(StartHealthRegen), null, 0, 1000);

		}
		
		public virtual void StartHealthRegen(object param)
		{
			if (HealthPercent >= 40)
			{
				m_timer.Dispose( );
				m_openDead = false;
				Close( );
				return;
			}
				
			if (Health == MaxHealth)
			{
				m_timer.Dispose( );
				m_openDead = false;
				Close();
				return;
			}

			if( m_healthregentimer <= 0 )
			{
				m_timer.Dispose();
				m_openDead = false;
				Close( );
				return;
			}
            Health += Level * 2;
			m_healthregentimer -= 10;
		}

		public override void TakeDamage ( GameObject source, eDamageType damageType, int damageAmount, int criticalAmount )
		{
			
			if( !m_openDead && Realm != eRealm.Door )
			{
				base.TakeDamage(source, damageType, damageAmount, criticalAmount);

				double damageDealt = damageAmount + criticalAmount;
			}
				
			GamePlayer attackerPlayer = source as GamePlayer;
			if( attackerPlayer != null)
			{
				if( !m_openDead && Realm != eRealm.Door )
				{
                    attackerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);

				}
				if( !m_openDead && Realm != eRealm.Door )
				{
					Health -= damageAmount + criticalAmount;
			
					if( !IsAlive )
					{
                        attackerPlayer.Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						Die(source);
						m_openDead = true;
						RegenDoorHealth();
						if( Locked == 0 )
							Open( );
								
						Group attackerGroup = attackerPlayer.Group;
						if( attackerGroup != null )
						{
							foreach( GameLiving living in attackerGroup.GetMembersInTheGroup( ) )
							{
                                ((GamePlayer)living).Out.SendMessage(LanguageMgr.GetTranslation(attackerPlayer.Client.Account.Language, "GameDoor.NowOpen", Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
							}
						}
					}
				}
			}
		}
		/// <summary>
		/// The action that closes the door after specified duration
		/// </summary>
		protected class CloseDoorAction : RegionAction
		{
			/// <summary>
			/// Constructs a new close door action
			/// </summary>
			/// <param name="door">The door that should be closed</param>
			public CloseDoorAction(GameDoor door)
				: base(door)
			{
			}

			/// <summary>
			/// This function is called to close the door 10 seconds after it was opened
			/// </summary>
			protected override void OnTick()
			{
				GameDoor door = (GameDoor)m_actionSource;
				door.Close();
			}
		}
	}
}
