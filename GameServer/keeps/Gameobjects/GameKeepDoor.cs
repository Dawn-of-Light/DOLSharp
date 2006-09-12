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
using System.Reflection;

using DOL.Database;
using DOL.Events;
using DOL.GS.PacketHandler;

using log4net;


namespace DOL.GS.Keeps
{
	/// <summary>
	/// keep door in world
	/// </summary>
	public class GameKeepDoor : GameLiving, IDoor, IKeepItem
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region properties

		private byte m_oldHealthPercent;

		private int m_doorID;
		/// <summary>
		/// The door index which is unique
		/// </summary>
		public int DoorID
		{
			get { return m_doorID; }
		}

		public int OwnerKeepID
		{
			get { return (m_doorID / 100000) % 1000; }
		}

		public int TowerNum
		{
			get { return (m_doorID / 10000) % 10; }
		}

		public int KeepID
		{
			get { return OwnerKeepID + TowerNum * 256; }
		}

		public int ComponentID
		{
			get { return (m_doorID / 100) % 100; }
		}

		public int DoorIndex
		{
			get { return m_doorID % 10; }
		}

		/// <summary>
		/// This flag is send in packet(keep door = 4, regular door = 0)
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
				return (byte)this.Component.Keep.Realm;
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
				if (IsAlive)
					return eDoorState.Closed;
				else
					return eDoorState.Open;
			}
			set
			{
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
				return (byte)this.Component.Keep.Level;
			}
		}

		public void UpdateLevel()
		{
			Health = MaxHealth;
		}

		public bool IsAttackableDoor
		{
			get
			{
				if (this.Component.Keep is GameKeepTower)
				{
					if (this.DoorIndex == 1)
						return true;
				}
				else if (this.Component.Keep is GameKeep)
				{
					if (this.Component.Skin == 10)
					{
						if (this.DoorIndex == 1)
							return true;
					}
					if (this.Component.Skin == 0)
					{
						if (this.DoorIndex == 1 ||
							this.DoorIndex == 2)
							return true;
					}
				}
				return false;
			}
		}

		public override int Health
		{
			get
			{
				if (!IsAttackableDoor)
					return 0;
				return base.Health;
			}
			set
			{
				base.Health = value;
			}
		}

		public override int RealmPointsValue
		{
			get
			{
				if (IsAttackableDoor)
					return MaxHealth / 10;
				else return MaxHealth / 100;
			}
		}

		public override long ExperienceValue
		{
			get
			{
				return 0;
			}
		}

		public override string Name
		{
			get
			{
				if (IsAttackableDoor)
					return "Keep Door";
				else return "Postern Door";
			}
		}

		private string m_templateID;
		public string TemplateID
		{
			get { return m_templateID; }
		}

		private GameKeepComponent m_component;
		public GameKeepComponent Component
		{
			get { return m_component; }
		}

		private DBKeepPosition m_position;
		public DBKeepPosition Position
		{
			get { return m_position; }
			set { m_position = value; }
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
			this.Component.Keep.LastAttackedByEnemyTick = this.CurrentRegion.Time;
			if (damageType != eDamageType.Slash && damageType != eDamageType.Thrust && damageType != eDamageType.Crush && damageType != eDamageType.Natural)
			{
				if (source is GamePlayer)
					(source as GamePlayer).Out.SendMessage("Your attack has no effect on the door!", eChatType.CT_SpellResisted, eChatLoc.CL_SystemWindow);
				return;
			}
			//only on hp change
			if (m_oldHealthPercent != HealthPercent)
			{
				m_oldHealthPercent = HealthPercent;
				foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
					client.Out.SendObjectUpdate(this);
			}

			base.TakeDamage(source, damageType, damageAmount, criticalAmount);
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
			if (!base.Interact(player))
				return false;

			if (player.IsMezzed)
			{
				player.Out.SendMessage("You are mesmerized!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			if (player.IsStunned)
			{
				player.Out.SendMessage("You are stunned!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}


			if (GameServer.ServerRules.IsSameRealm(player, this, true) || player.Client.Account.PrivLevel != 1)
			{
				int keepx = 0, keepy = 0, keepz = Z, distance = 0;

				//calculate distance
				//normal door
				if (DoorIndex == 1)
					distance = 300;
				//side or internal door
				else
					distance = 100;

				//calculate Z
				if (this.Component.Keep is GameKeepTower)
				{
					//when entering a tower, we need to raise Z
					//portal keeps are considered towers too, so we check component count
					if (IsObjectInFront(player, 180, false))
					{
						if (this.Component.Keep.KeepComponents.Count == 1 && DoorIndex == 1)
							keepz = Z + 83;
					}
				}
				else
				{
					//when entering a keeps inner door, we need to raise Z
					if (IsObjectInFront(player, 180, false))
					{
						//To find out if a door is the keeps inner door, we compare the distance between
						//the component for the keep and the component for the gate
						int keepdistance = int.MaxValue;
						int gatedistance = int.MaxValue;
						foreach (GameKeepComponent c in this.Component.Keep.KeepComponents)
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
						if (DoorIndex == 1 && keepdistance < gatedistance)
							keepz = Z + 92;//checked in game with lvl 1 keep
					}
				}

				//calculate x y
				if (IsObjectInFront(player, 180, false))
					GetSpotFromHeading(-distance, out keepx, out keepy);
				else
					GetSpotFromHeading(distance, out keepx, out keepy);

				//move player
				player.MoveTo(CurrentRegionID, keepx, keepy, keepz, player.Heading);
			}
			return base.Interact(player);
		}

		public override IList GetExamineMessages(GamePlayer player)
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

			IList list = base.GetExamineMessages(player);
			string text = "You select the " + Name + ".";
			if (GameServer.ServerRules.IsSameRealm(player, this, true) || player.Client.Account.PrivLevel != 1)
				text = text + " It belongs to your realm.";
			else
			{
				if (IsAttackableDoor)
					text = text + " It belongs to an enemy realm and can be attacked!";
				else text = text + " It belongs to an enemy realm!";
			}
			list.Add(text);
			return list;
		}

		public override string GetName(int article, bool firstLetterUppercase)
		{
			return "the " + base.GetName(article, firstLetterUppercase);
		}

		/// <summary>
		/// Called to create an keep door in the world
		/// </summary>
		/// <returns>true when created</returns>
		public override bool AddToWorld()
		{
			if (!base.AddToWorld()) return false;
			foreach (GamePlayer player in GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				player.Out.SendObjectCreate(this);
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

		}

		/// <summary>
		/// load the keep door object from DB object
		/// </summary>
		/// <param name="obj"></param>
		public override void LoadFromDatabase(DataObject obj)
		{

		}

		public void LoadFromPosition(DBKeepPosition pos, GameKeepComponent component)
		{
			m_templateID = pos.TemplateID;
			m_component = component;

			PositionMgr.LoadKeepItemPosition(pos, this);
			component.Keep.Doors[m_templateID] = this;

			m_Level = 0;
			m_health = MaxHealth;
			m_Name = "Keep Door";
			m_oldHealthPercent = HealthPercent;
			m_healthRegenerationPeriod = 3600000; //3600000 ms = 3600 seconds = 1 hour
			m_doorID = GenerateDoorID();
			this.m_Model = 0xFFFF;

			DoorMgr.Doors[m_doorID] = this;
			this.AddToWorld();
		}

		public void MoveToPosition(DBKeepPosition position)
		{ }

		public int GenerateDoorID()
		{
			int doortype = 7;
			int ownerKeepID = 0;
			int towerIndex = 0;
			if (m_component.Keep is GameKeepTower)
			{
				GameKeepTower tower = m_component.Keep as GameKeepTower;
				ownerKeepID = tower.Keep.KeepID;
				towerIndex = tower.KeepID >> 8;
			}
			else
				ownerKeepID = m_component.Keep.KeepID;
			int componentID = m_component.ID;

			//index not sure yet
			int doorIndex = this.Position.TemplateType;
			int id = 0;
			//add door type
			id += doortype * 100000000;
			id += ownerKeepID * 100000;
			id += towerIndex * 10000;
			id += componentID * 100;
			id += doorIndex;
			return id;
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
		public override void  Die(GameObject killer)
		{
			base.Die(killer);
			foreach (GamePlayer player in this.GetPlayersInRadius(WorldMgr.INFO_DISTANCE))
				player.Out.SendMessage("The Keep Gate is broken!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			lock (this)
			{
				m_state = eDoorState.Open;
			}
			BroadcastDoorStatus();
		}

		/// <summary>
		/// This method is called when door is repair or keep is reset
		/// </summary>
		public virtual void CloseDoor()
		{
			lock (this)
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
			foreach (GameClient client in WorldMgr.GetClientsOfRegion(CurrentRegionID))
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
