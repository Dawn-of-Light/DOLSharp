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
using DOL.Events;
using DOL.GS.PacketHandler;
using System.Reflection;
using log4net;


namespace DOL.GS
{
	/// <summary>
	/// keep door in world
	/// </summary>
	public class GameKeepDoor : GameLiving, IDoor
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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
			CurrentRegion = keep.CurrentRegion;
			Realm = (byte)keep.Realm;			
			m_oldHealthPercent = HealthPercent;
			m_healthRegenerationPeriod = 3600000; //3600000 ms = 3600 seconds = 1 hour
		}
		
		#region properties

		private byte m_oldHealthPercent;
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
				return m_doorID;	
			}
			set
			{
				m_doorID = value;
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
		public override void TakeDamage(GameObject source, eDamageType damageType, int damageAmount, int criticalAmount)
		{
			if (damageType != eDamageType.Slash && damageType != eDamageType.Thrust && damageType != eDamageType.Crush)
			{
				if (source is GamePlayer)
					(source as GamePlayer).Out.SendMessage("Your attack has no effect on the door!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			//only on hp change
			if (m_oldHealthPercent != HealthPercent)
			{
				m_oldHealthPercent = HealthPercent;
				foreach(GamePlayer player in this.GetPlayersInRadius(WorldMgr.OBJ_UPDATE_DISTANCE))
					player.Out.SendKeepDoorUpdate(this);
			}

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

		public override int ChangeHealth(GameObject changeSource, GameLiving.eHealthChangeType healthChangeType, int changeAmount)
		{
			if (healthChangeType == eHealthChangeType.Spell)
			{
				if (changeSource is GamePlayer)
					(changeSource as GamePlayer).Out.SendMessage("Your spell has no effect on the door!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return 0;
			}
			return base.ChangeHealth(changeSource, healthChangeType, changeAmount);
		}

		/// <summary>
		/// This function is called from the ObjectInteractRequestHandler
		/// It teleport player in the keep if player and keep have the same realm
		/// </summary>
		/// <param name="player">GamePlayer that interacts with this object</param>
		/// <returns>false if interaction is prevented</returns>
		public override bool Interact(GamePlayer player)
		{
			int keepId = (DoorID - 700000000) / 100000;
			int keepPiece = (DoorID - 700000000 - keepId * 100000) / 10000;
			int componentId = (DoorID - 700000000 - keepId * 100000 - keepPiece * 10000) / 100;
			int doorIndex = (DoorID - 700000000 - keepId * 100000 - keepPiece * 10000 - componentId * 100);
			
			if (!WorldMgr.CheckDistance(this, player, WorldMgr.INTERACT_DISTANCE) && player.Client.Account.PrivLevel == 1) return false;

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


			if (GameServer.ServerRules.IsSameRealm(player, this, true) || player.Client.Account.PrivLevel != 1)
			{
				int keepx = 0, keepy = 0, keepz = Z, distance = 0;

				//calculate distance
				//normal door
				if (doorIndex == 1)
					distance = 300;
				//side or internal door
				else if (doorIndex == 2)
					distance = 150;

				//calculate Z
				if (m_keep is GameKeepTower)
				{
					//when entering a tower, we need to raise Z
					//portal keeps are considered towers too, so we check component count
					if (IsObjectInFront(player, 180))
					{
						if (m_keep.KeepComponents.Count == 1 && doorIndex == 1)
							keepz = Z + 83;
					}
				}
				else
				{
					//when entering a keeps inner door, we need to raise Z
					if (IsObjectInFront(player, 180)) 
					{
						//To find out if a door is the keeps inner door, we compare the distance between
						//the component for the keep and the component for the gate
						int keepdistance = int.MaxValue;
						int gatedistance = int.MaxValue;
						foreach (GameKeepComponent c in this.Keep.KeepComponents)
						{
							if ((GameKeepComponent.eComponentSkin)c.Skin == GameKeepComponent.eComponentSkin.Keep)
							{
								keepdistance = WorldMgr.GetDistance(this, c);
							}
							if ((GameKeepComponent.eComponentSkin)c.Skin == GameKeepComponent.eComponentSkin.Gate)
							{
								gatedistance = WorldMgr.GetDistance(this, c);
							}
							//when these are filled we can stop the search
							if (keepdistance != int.MaxValue && gatedistance != int.MaxValue)
								break;
						}
						if (doorIndex == 1 && keepdistance < gatedistance)
							keepz = Z + 92;//checked in game with lvl 1 keep
					}
				}

				//calculate x y
				if (IsObjectInFront(player, 180))
					GetSpotFromHeading(-distance, out keepx, out keepy);
				else
					GetSpotFromHeading(distance, out keepx, out keepy);

				//move player
				player.MoveTo(CurrentRegionID, keepx, keepy, keepz, player.Heading);
			}
			return base.Interact(player);
		}

		public override System.Collections.IList GetExamineMessages(GamePlayer player)
		{
			/*
			 * You select the Keep Gate. It belongs to your realm.
			 * You target [the Keep Gate]
			 * 
			 * You select the Keep Gate. It belongs to an enemy realm and can be attacked!
			 * You target [the Keep Gate]
			 * 
			 * You select the Postern Door. It belongs to an enemy realm!
			 * You target [the Postern Door]
			 */

			System.Collections.IList list = base.GetExamineMessages(player);
			if (GameServer.ServerRules.IsSameRealm(player, this, true) || player.Client.Account.PrivLevel != 1)
				list.Add("You select the " + Name + ". It belongs to your realm.");
			else list.Add("You select the " + Name + ". It belongs to an enemy realm!");
			return list;
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
			bool New = false;
			if (InternalID != null)
				obj = (DBDoor) GameServer.Database.FindObjectByKey(typeof (DBDoor), InternalID);			

			if (obj == null)
			{
				obj = new DBDoor();				
				New = true;
			}

			obj.Name = Name;
			obj.Heading = Heading;
			obj.X = X;
			obj.Y = Y;
			obj.Z = Z;
			obj.InternalID = DoorID;
			obj.ObjectId = DoorID.ToString();
			obj.Health = Health;
			obj.KeepID = Keep.KeepID;
			if (New)
			{
				GameServer.Database.AddNewObject(obj);				
			}
			else
				GameServer.Database.SaveObject(obj);
		}

		/// <summary>
		/// load the keep door object from DB object
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(DataObject obj)
		{
			base.LoadFromDatabase(obj);
			DBDoor dbdoor = obj as DBDoor;
			if (dbdoor == null)return;
			Name = dbdoor.Name;
			Health = dbdoor.Health;
			m_oldHealthPercent = HealthPercent;
			m_doorID = dbdoor.InternalID;
			X = dbdoor.X;
			Y = dbdoor.Y;
			Z = dbdoor.Z;
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
			foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				player.Out.SendMessage("The Keep Gate is broken!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
			foreach(GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
			{	
				client.Out.SendDoorState(this);
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
			Realm = (byte)realm;
			Health = MaxHealth;
			m_oldHealthPercent = HealthPercent;
			CloseDoor();
		}
	}
}
