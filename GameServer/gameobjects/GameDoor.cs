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
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using DOL.GS.Utils;
using DOL.GS.Quests;
using System.Threading;
using DOL.AI.Brain;
using DOL.Events;
using DOL.GS.Effects;
using DOL.GS.Keeps;
using DOL.GS.PropertyCalc;
using DOL.GS.SkillHandler;
using DOL.GS.Spells;
using DOL.GS.Styles;
using DOL.GS.PacketHandler.Client.v168;

namespace DOL.GS
{
	/// <summary>
	/// GameDoor is class for regular door
	/// </summary>
	public class GameDoor : GameLiving, IDoor
	{
		private bool OpenDead = false;
		private static Timer m_timer;
		protected volatile uint m_lastUpdateTickCount = uint.MinValue;
		private readonly object m_LockObject = new object();
		/// <summary>
		/// The time interval after which door will be closed, in milliseconds
		/// </summary>
		protected const int CLOSE_DOOR_TIME = 10000;
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
			//this.Realm = 0;
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
			this.CurrentRegion = curZone.ZoneRegion;
			m_Name = m_dbdoor.Name;
			m_Heading = (ushort)m_dbdoor.Heading;
			m_X = m_dbdoor.X;
			m_Y = m_dbdoor.Y;
			m_Z = m_dbdoor.Z;
			m_Level = 0;
			m_Model = 0xFFFF;
			m_doorID = m_dbdoor.InternalID;
            m_Guild = m_dbdoor.Guild;
            m_Realm = (eRealm)m_dbdoor.Realm;
            m_Level = m_dbdoor.Level;
            m_health = m_dbdoor.MaxHealth;
            m_maxHealth = m_dbdoor.MaxHealth;
			m_locked = m_dbdoor.Locked;
			//m_model = m_dbdoor.Model;
			this.AddToWorld();
		}
		/// <summary>
		/// save this door to a door table slot
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBDoor obj = null;
			if (InternalID != null)
				obj = (DBDoor)GameServer.Database.FindObjectByKey(typeof(DBDoor), InternalID);
			if (obj == null)
				obj = new DBDoor();
			obj.Name = this.Name;
		//	obj.Heading = this.Heading;
		//	obj.X = this.X;
		//	obj.Y = this.Y;
		//	obj.Z = this.Z;
			obj.InternalID = this.DoorID;
			obj.Type = DoorID / 100000000;
            obj.Guild = this.Guild;
            obj.Realm = (byte)this.Realm;
            obj.Level = this.Level;
            obj.MaxHealth = this.MaxHealth;
			obj.Health = this.MaxHealth;
			obj.Locked = this.Locked;
			if (InternalID == null)
			{
				GameServer.Database.AddNewObject(obj);
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
		public int Locked
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
		public int DoorID
		{
			get { return m_doorID; }
			set { m_doorID = value; }
		}

		private int m_type;

		/// <summary>
		/// Door Type
		/// </summary>
		public int Type
		{
			get { return m_type; }
			set { m_type = value; }
		}
		/// <summary>
		/// this is flag for packet (0 for regular door and 4 for keep door)
		/// </summary>
		public int Flag
		{
			get { return 0; }
		}

		/// <summary>
		/// This hold the state of door
		/// </summary>
		private eDoorState m_state;

		/// <summary>
		/// The state of door (open or close)
		/// </summary>
		public eDoorState State
		{
			get { return m_state; }
			set
			{
				if (m_state != value)
				{
					lock (m_LockObject)
					{
						m_state = value;
						foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
						{
							player.Out.SendDoorState(this);
						}
					}
				}
			}
		}

		#endregion

		/// <summary>
		/// Call this function to open the door
		/// </summary>
		public void Open()
		{
			if (Locked == 0)
				this.State = eDoorState.Open;
			
			if (HealthPercent > 40 || !OpenDead)
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

		public byte Status
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
		public void Close()
		{
			if (!OpenDead)
				this.State = eDoorState.Closed;
			m_closeDoorAction = null;
		}

		/// <summary>
		/// Allow a NPC to manipulate the door
		/// </summary>
		/// <param name="npc"></param>
		/// <param name="open"></param>
		public void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{
			npc.TurnTo(this.X, this.Y);
			if (open && m_state != eDoorState.Open)
				this.Open();
			else if (!open && m_state != eDoorState.Closed)
				this.Close();

		}
		
		public virtual int Health
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
		
		public virtual void BroadcastUpdate ()
		{
			if( ObjectState != eObjectState.Active ) return;
			foreach( GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE) )
			{
				if( player == null ) continue;
				player.Out.SendObjectUpdate(this);
				player.CurrentUpdateArray[ObjectID - 1] = true;
			}
			m_lastUpdateTickCount = (uint)Environment.TickCount;
		}
		
		
		private static long m_healthregentimer = 0;
		
		public void RegenDoorHealth ()
		{
			Health = 0;
			if (Locked == 0)
				Open();
			
			m_healthregentimer = 9999;
			m_timer = new Timer(new TimerCallback(StartHealthRegen), null, 0, 1000);

		}
		
		public void StartHealthRegen(object param)
		{
			if (HealthPercent >= 40)
			{
				m_timer.Dispose( );
				OpenDead = false;
				Close( );
				return;
			}
				
			if (Health == MaxHealth)
			{
				m_timer.Dispose( );
				OpenDead = false;
				Close();
				return;
			}

			if( m_healthregentimer <= 0 )
			{
				m_timer.Dispose();
				OpenDead = false;
				Close( );
				return;
			}
			this.Health += this.Level*2;
			m_healthregentimer -= 10;
		}

		public override void TakeDamage ( GameObject source, eDamageType damageType, int damageAmount, int criticalAmount )
		{
			
			if( !OpenDead && this.Realm != eRealm.Door )
			{
				base.TakeDamage(source, damageType, damageAmount, criticalAmount);

				double damageDealt = damageAmount + criticalAmount;
			}
				
			GamePlayer attackerPlayer = source as GamePlayer;
			if( attackerPlayer != null)
			{
				if( !OpenDead && this.Realm != eRealm.Door )
				{
					attackerPlayer.Out.SendMessage("The door is now open", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				if( !OpenDead && this.Realm != eRealm.Door )
				{
					Health -= damageAmount + criticalAmount;
			
					if( !IsAlive )
					{
						attackerPlayer.Out.SendMessage("The door is now open", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						Die(source);
						OpenDead = true;
						RegenDoorHealth();
						if( Locked == 0 )
							Open( );
				
				
						Group attackerGroup = attackerPlayer.Group;
						if( attackerGroup != null )
						{
							foreach( GameLiving living in attackerGroup.GetMembersInTheGroup( ) )
							{
						 		((GamePlayer)living).Out.SendMessage("The door is now open", eChatType.CT_System, eChatLoc.CL_SystemWindow);

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
