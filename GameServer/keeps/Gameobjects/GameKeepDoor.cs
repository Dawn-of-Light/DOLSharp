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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;


namespace DOL.GS
{
	/// <summary>
	/// keep door in world
	/// </summary>
	public class GameKeepDoor : GameLiving, IDoor
	{
		/// <summary>
		/// the keep door constructor
		/// </summary>
		/// <param name="keep"></param>
		public GameKeepDoor(AbstractGameKeep keep) : base()
		{
			Level = 0;
			Keep = keep;
			Health = MaxHealth;
			Name = "Keep Door";
			GameEventMgr.AddHandler(this,GameLivingEvent.Dying,new DOLEventHandler(OpenDoor));
			keep.Doors.Add(this);
			Region = keep.Region;
			Realm = (byte)keep.Realm;
			//Don't waste timer resources
			m_healthRegenerationPeriod = 3600000; //3600000 ms = 3600 seconds = 1 hour
		}

		#region properties

		/// <summary>
		/// This hold the door index which is unique
		/// </summary>
		private int m_doorID;

		/// <summary>
		/// The door index which is unique
		/// </summary>
		public int DoorID
		{
			get
			{
				return this.m_doorID;
			}
		}

		/// <summary>
		/// This flag is send in packet(keep door =4, regular door = 0)
		/// </summary>
		public int Flag
		{
			get
			{
				return 4;
			}
		}

		/// <summary>
		/// Get the realm of the keep door from keep owner
		/// </summary>
		public override byte Realm
		{
			get
			{
				return (byte)Keep.Realm;
			}
		}

		/// <summary>
		/// This hold keep owner
		/// </summary>
		private AbstractGameKeep m_keep;

		/// <summary>
		/// Keep owner of the door
		/// </summary>
		public AbstractGameKeep Keep
		{
			get
			{
				return m_keep;
			}
			set
			{
				m_keep = value;
			}
		}

		/// <summary>
		/// door state (open or closed)
		/// </summary>
		private eDoorState m_state;

		/// <summary>
		/// door state (open or closed)
		/// call the broadcast of state in area
		/// </summary>
		public eDoorState State
		{
			get
			{
				if (Alive)
					return eDoorState.Closed;
				else
					return eDoorState.Open;
			}
			set	{
				if (m_state != value)
				{
					m_state = value;
					BroadcastDoorStatus();
				}
			}
		}

		/// <summary>
		/// The level of door is keep level now
		/// </summary>
		public override byte Level
		{
			get
			{
				return (byte)Keep.Level;
			}
		}


		#endregion

		#region function override

		/// <summary>
		/// This methode is override to remove XP system
		/// </summary>
		/// <param name="source">the damage source</param>
		/// <param name="damageType">the damage type</param>
		/// <param name="damageAmount">the amount of damage</param>
		/// <param name="criticalAmount">the amount of critical damage</param>
		public override void TakeDamage(GameLiving source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			//Work around the XP system
			if(Alive)
			{
				Health -= (damageAmount	+ criticalAmount);
				if(!Alive)
				{
					Health = 0;
					Die(source);
				}
			}
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// It teleport player in the keep if player and keep have the same realm
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			//todo constant here too
			if(!Position.CheckSquareDistance(player.Position, 175*175) && player.Client.Account.PrivLevel == ePrivLevel.Player) return false;

			if (player.Mez)
			{
				player.Out.SendMessage("You are mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.Stun)
			{
				player.Out.SendMessage("You are stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (GameServer.ServerRules.IsSameRealm(player, this, true)) 
			{ 
				Point keepPos;
				if (IsObjectInFront(player, 180)) 
					keepPos = GetSpotFromHeading(-300);
				else 
					keepPos = GetSpotFromHeading(300);
				keepPos.Z = player.Position.Z + 100;
				player.MoveTo((ushort)RegionId, keepPos, (ushort)player.Heading); 
				return true; 
			}
			return false;
		}

		/// <summary>
		/// Called to create an keep door in the world
		/// </summary>
		/// <returns>true when created</returns>
		public override bool AddToWorld()
		{
			if(!base.AddToWorld()) return false;
			foreach(GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendDoorCreate(this);
			return true;
		}

		/// <summary>
		/// Starts the power regeneration
		/// </summary>
		public override void StartPowerRegeneration()
		{
			//No regeneration for doors
			return;
		}
		/// <summary>
		/// Starts the endurance regeneration
		/// </summary>
		public override void StartEnduranceRegeneration()
		{
			//No regeneration for doors
			return;
		}
		#endregion

		#region Save/load DB

		/// <summary>
		/// save the keep door object in DB
		/// </summary>
		public override void SaveIntoDatabase()
		{
			DBDoor obj = null;
			if (InternalID != null)
				obj = (DBDoor) GameServer.Database.FindObjectByKey(typeof (DBDoor), InternalID);
			if (obj == null)
				obj = new DBDoor();
			obj.Name = this.Name;
			obj.Heading = this.Heading;
			Point pos = Position;
			obj.X = pos.X;
			obj.Y = pos.Y;
			obj.Z = pos.Z;
			obj.DoorID = this.DoorID;
			obj.Health = this.Health;
			obj.KeepID = this.Keep.KeepID;
			/*if (InternalID == null)
			{
				GameServer.Database.AddNewObject(obj);
				InternalID = obj.ObjectId;
			}
			else
				GameServer.Database.SaveObject(obj);*/
		}

		/// <summary>
		/// load the keep door object from DB object
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(object obj)
		{
			DBDoor dbdoor = obj as DBDoor;
			if (dbdoor == null)return;
			InternalID = dbdoor.DoorID.ToString();
			Name = dbdoor.Name;
			Health = dbdoor.Health;
			m_doorID = (int)dbdoor.DoorID;
			Position = new Point(dbdoor.X, dbdoor.Y, dbdoor.Z);

			Heading = (ushort)dbdoor.Heading;
			AddToWorld();
		}
		#endregion

		/// <summary>
		/// call when player try to open door
		/// </summary>
		public void Open()
		{
			//do nothing because gamekeep must be destroyed to be open
		}
		/// <summary>
		/// call when player try to close door
		/// </summary>
		public void Close()
		{
			//do nothing because gamekeep must be destroyed to be open
		}

		/// <summary>
		/// This function is called when door "die" to open door
		/// </summary>
		/// <param name="e"></param>
		/// <param name="o"></param>
		/// <param name="args"></param>
		public void OpenDoor(DOLEvent e, object o, EventArgs args)
		{
			lock(this)
			{
				m_state = eDoorState.Open;
			}
			BroadcastDoorStatus();
		}

		/// <summary>
		/// This methode is called when door is repair or keep is reset
		/// </summary>
		public virtual void CloseDoor()
		{
			lock(this)
			{
				m_state = eDoorState.Closed;
			}
			BroadcastDoorStatus();
		}

		/// <summary>
		/// boradcast the door statut to all player near the door
		/// </summary>
		public virtual void BroadcastDoorStatus()
		{
			foreach(GameClient client in WorldMgr.GetClientsOfRegion((ushort)RegionId))
			{
				client.Player.Out.SendDoorState(this);
			}
		}

		/// <summary>
		/// This Function is called when door has been repaired
		/// </summary>
		/// <param name="amount">how many HP is repaired</param>
		public void Repair(int amount)
		{
			Health += amount;
			if (HealthPercent > 15 && m_state == eDoorState.Open)
				CloseDoor();
		}
		/// <summary>
		/// This Function is called when keep is taken to repair door
		/// </summary>
		/// <param name="realm">new realm of keep taken</param>
		public void Reset(eRealm realm)
		{
			this.Realm = (byte)realm;
			this.Health = this.MaxHealth;
			CloseDoor();
		}
	}
}
