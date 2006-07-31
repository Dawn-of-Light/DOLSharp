using System;
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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x31^168,"Handles open/close door requests!")]
	public class DoorRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int doorId = (int)packet.ReadInt();
			byte doorState = (byte)packet.ReadByte();

			new ChangeDoorAction(client.Player, doorId, doorState).Start(1);

			return 1;
		}

		/// <summary>
		/// Handles the door state change actions
		/// </summary>
		protected class ChangeDoorAction : RegionAction
		{
			/// <summary>
			/// The target door Id
			/// </summary>
			protected readonly int m_doorId;
			/// <summary>
			/// The door state
			/// </summary>
			protected readonly int m_doorState;

			/// <summary>
			/// Constructs a new ChangeDoorAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="doorId">The target door Id</param>
			/// <param name="doorState">The door state</param>
			public ChangeDoorAction(GamePlayer actionSource, int doorId, int doorState) : base(actionSource)
			{
				m_doorId = doorId;
				m_doorState = doorState;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				GamePlayer player = (GamePlayer)m_actionSource;
				IDoor mydoor = DoorMgr.getDoorByID(m_doorId);

				if (mydoor != null)
				{
					if (mydoor is GameKeepDoor)
					{
						GameKeepDoor door = mydoor as GameKeepDoor;
						//portal keeps left click = right click
						if (door.Keep is GameKeepTower && door.Keep.KeepComponents.Count > 1)
							door.Interact(player);
					}
					else
					{
						if (!WorldMgr.CheckDistance(player, mydoor, WorldMgr.PICKUP_DISTANCE))
						{
							player.Out.SendMessage("You are too far away to open this door!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							return;
						}
						if (m_doorState == 0x01)
						{
							mydoor.Open();
						}
						else
						{
							mydoor.Close();
						}
					}
				}
				else
				{
					//else basic quick hack
					GameDoor door = new GameDoor();
					door.DoorID = m_doorId;
					door.X = player.X;
					door.Y = player.Y;
					door.Z = player.Z;
					door.CurrentRegion = player.CurrentRegion;
					door.Open();
				}			
			}
		}
	}
}