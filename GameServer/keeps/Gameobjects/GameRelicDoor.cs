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

using DOL.Database2;
using DOL.Events;
using DOL.GS.PacketHandler;

using log4net;


namespace DOL.GS.Keeps
{
	/// <summary>
	/// relic keep door in world
	/// </summary>
	public class GameRelicDoor : GameLiving, IDoor
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		#region properties

		private int m_doorID;
		/// <summary>
		/// The door index which is unique
		/// </summary>
		public int DoorID
		{
			get { return m_doorID; }
			set { m_doorID = value; }
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
		/// door state (open or closed)
		/// </summary>
		private eDoorState m_state;

		/// <summary>
		/// door state (open or closed)
		/// call the broadcast of state in area
		/// </summary>
		public eDoorState State
		{
			get { return m_state; }
			set
			{
				if (m_state != value)
				{
					m_state = value;
					BroadcastDoorStatus();
				}
			}
		}

		public override int Health
		{
			get
			{
				return MaxHealth;
			}
			set
			{
			}
		}

		public override string Name
		{
			get
			{
				return "Relic Gate";
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
			return;
		}

		public override int ChangeHealth(GameObject changeSource, GameLiving.eHealthChangeType healthChangeType, int changeAmount)
		{
			return 0;
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
				int x = 0;
				int y = 0;
				//calculate x y
				if (IsObjectInFront(player, 180, false))
					GetSpotFromHeading(-500, out x, out y);
				else
					GetSpotFromHeading(500, out x, out y);

				//move player
				player.MoveTo(CurrentRegionID, x, y, player.Z, player.Heading);
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
			if (this.Realm == player.Realm)
				text = text + " It belongs to your realm.";
			else
			{
				text = text + " It belongs to an enemy realm!";
			}
			list.Add(text);
			return list;
		}

		public override string GetName(int article, bool firstLetterUppercase)
		{
			return "the " + base.GetName(article, firstLetterUppercase);
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

		public override void StartHealthRegeneration()
		{
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
		public override void LoadFromDatabase(DatabaseObject obj)
		{
			DBDoor door = obj as DBDoor;
			if (door == null)
				return;
			base.LoadFromDatabase(obj);

			Zone curZone = WorldMgr.GetZone((ushort)(door.InternalID / 1000000));
			if (curZone == null) return;
			this.CurrentRegion = curZone.ZoneRegion;
			m_Name = door.Name;
			m_Heading = (ushort)door.Heading;
			m_X = door.X;
			m_Y = door.Y;
			m_Z = door.Z;
			m_Level = 0;
			m_Model = 0xFFFF;
			m_doorID = door.InternalID;
			m_state = eDoorState.Closed;
			this.AddToWorld();

			m_health = MaxHealth;
			StartHealthRegeneration();
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

		public virtual void OpenDoor()
		{
			m_state = eDoorState.Open;
			BroadcastDoorStatus();
		}

		/// <summary>
		/// This method is called when door is repair or keep is reset
		/// </summary>
		public virtual void CloseDoor()
		{
			m_state = eDoorState.Closed;
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

		/*
		 * Note that 'enter' and 'exit' commands will also work at these doors.
		 */

		public override bool WhisperReceive(GameLiving source, string str)
		{
			if (!base.WhisperReceive(source, str))
				return false;

			if (source is GamePlayer == false)
				return false;

			str = str.ToLower();

			if (str.Contains("enter") || str.Contains("exit"))
				Interact(source as GamePlayer);
			return true;
		}

		public override bool SayReceive(GameLiving source, string str)
		{
			if (!base.SayReceive(source, str))
				return false;

			if (source is GamePlayer == false)
				return false;

			str = str.ToLower();

			if (str.Contains("enter") || str.Contains("exit"))
				Interact(source as GamePlayer);
			return true;
		}

		public void NPCManipulateDoorRequest(GameNPC npc, bool open)
		{ }
	}
}
