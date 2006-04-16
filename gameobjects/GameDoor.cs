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
using System.Text;
using DOL.GS.Database;

namespace DOL.GS
{
	/// <summary>
	/// GameDoor is class for regular door
	/// </summary>		
	public class GameDoor : PersistantGameObject, IDoor
	{
		#region Properties

		/// <summary>
		/// The time interval after which door will be closed, in milliseconds
		/// </summary>
		protected const int CLOSE_DOOR_TIME = 10000;

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

		/// <summary>
		/// last open tick
		/// </summary>
		protected long m_lastOpenTick;

		/// <summary>
		/// gets/sets gametick when this door has been oppened
		/// </summary>
		public virtual long LastOpenTick
		{
			get { return m_lastOpenTick; }
			set { m_lastOpenTick = value; }
		}

		/// <summary>
		/// this is flag for packet (0 for regular door and 4 for keep door)
		/// It seems to be the type of the door (in costwold all door are 1)
		/// </summary>
		public int Flag
		{
			get { return 0; }
		}

		/// <summary>
		/// This hold the state of door
		/// </summary>
		private eDoorState m_state = eDoorState.Closed;

		/// <summary>
		/// The state of door (open or close)
		/// </summary>
		public eDoorState State
		{
			get { return m_state; }
			set { m_state = value; }
		}

		/// <summary>
		/// The sync object for door changes
		/// </summary>
		private readonly object m_doorSync = new object();

		/// <summary>
		/// The timed action that will close the door
		/// </summary>
		protected CloseDoorAction m_closeDoorAction;

		#endregion

		#region Open / Close
		/// <summary>
		/// Call this function to open the door
		/// </summary>
		public void Open()
		{
			lock(m_doorSync)
			{
				LastOpenTick = Region.Time;
				m_state = eDoorState.Open;
			
				BroadcastUpdate();

				m_closeDoorAction = new CloseDoorAction(this);
				m_closeDoorAction.Start(CLOSE_DOOR_TIME);
			}
		}
		/// <summary>
		/// Call this function to close the door
		/// </summary>
		public void Close()
		{
			lock(m_doorSync)
			{
				if(LastOpenTick + 2000 <= Region.Time)
				{
					m_state = eDoorState.Closed;

					BroadcastUpdate();

					if (m_closeDoorAction != null)
					{
						m_closeDoorAction.Stop();
						m_closeDoorAction = null;
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
			public CloseDoorAction(GameDoor door) : base(door)
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
		#endregion

		#region Broadcats update / create / remove
		/// <summary>
		/// Broadcasts the door state to all players around
		/// </summary>
		public virtual void BroadcastUpdate()
		{
			foreach (GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendDoorState(this);
			}
		}

		/// <summary>
		/// Broadcasts the object to all players around
		/// </summary>
		public override void BroadcastCreate()
		{
			if(ObjectState!=eObjectState.Active) return;
			foreach(GamePlayer player in GetInRadius(typeof(GamePlayer), WorldMgr.VISIBILITY_DISTANCE))
			{
				player.Out.SendItemCreate(this);
				player.Out.SendDoorState(this);
			}
		}

		/// <summary>
		/// Remove the object to all players around
		/// </summary>
		public override void BroadcastRemove()
		{
			// do nothing because doors model can't be removed client side ...
		}
		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(base.ToString())
				.Append(" doorID=").Append(m_doorID)
				.Append(" lastOpenTick=").Append(m_lastOpenTick)
				.Append(" state=").Append(m_state)
				.Append(" Flag=").Append(Flag)
				.ToString();
		}
	}
}