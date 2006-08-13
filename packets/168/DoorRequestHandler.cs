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

using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x31^168,"Handles open/close door requests!")]
	public class DoorRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int doorId = (int)packet.ReadInt();
			byte doorState = (byte)packet.ReadByte();

			if(client.Account.PrivLevel > 1)
			{
				int doorType = doorId / 100000000;
				if (doorType == 7)
				{
					int keepId = (doorId - 700000000) / 100000;
					int keepPiece = (doorId - 700000000 - keepId * 100000) / 10000;
					int componentId = (doorId - 700000000 - keepId * 100000 - keepPiece * 10000) / 100;
					int doorIndex = (doorId - 700000000 - keepId * 100000 - keepPiece * 10000 - componentId * 100);
					client.Out.SendDebugMessage("Keep DoorID:{0} state:{1} (KeepID:{2} componentId:{3} doorIndex:{4} KeepPiece:{5})", doorId, doorState, keepId + keepPiece * 256, componentId, doorIndex, keepPiece);
				}
				else if(doorType == 9)
				{
					doorType = doorId / 10000000;
					int doorIndex = doorId - doorType * 10000000;
					client.Out.SendDebugMessage("House DoorID:{0} state:{1} (doorType:{2} doorIndex:{3})", doorId, doorState, doorType, doorIndex);
				}
				else
				{
					int zoneDoor = (int)(doorId / 1000000);
					int fixture = (int)(doorId - zoneDoor * 1000000);
					int fixturePiece = fixture;
					fixture /= 100;
					fixturePiece = fixturePiece - fixture * 100;
					client.Out.SendDebugMessage("Fixture DoorID:{0} state:{1} (zone:{2} fixture:{3} fixturePiece:{4})",doorId, doorState, zoneDoor, fixture, fixturePiece);
				}
			}
			/*
			if (client.Player.TargetObject == null)
			{
				client.Out.SendDebugMessage("target object is null, sending object update request packet");
				GSTCPPacketOut pak = new GSTCPPacketOut(0xD5);
				pak.WriteByte(0);
				client.Out.SendTCP(pak);
			}*/

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
					if (player.Client.Account.PrivLevel > 1)
					{
						DOL.Database.DBDoor doorCheck = GameServer.Database.SelectObject(typeof(DOL.Database.DBDoor), "InternalID = " + m_doorId) as DOL.Database.DBDoor;
						if (doorCheck == null)
						{

							player.Out.SendMessage("The door's ID is: " + m_doorId.ToString(), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							player.Out.SendMessage("Stand as close to the door as possible and face outwards (best done from inside and behind it", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

							player.Out.SendCustomDialog("Add this door to the database?", new CustomDialogResponse(AddingDoor));
						}
					}
				}		
				}

			protected void AddingDoor(GamePlayer player, byte response)
			{
				if (response != 0x01)
					return;

				DOL.Database.DBDoor door = new DOL.Database.DBDoor();
				door.ObjectId = null;
				door.InternalID = m_doorId;
				door.X = player.X;
				door.Y = player.Y;
				door.Z = player.Z;
				door.Health = 0;
				door.Name = "door";
				door.Heading = player.Heading;
				AbstractGameKeep keep = KeepMgr.getKeepCloseToSpot(player.CurrentRegionID, player, 2000);
				if (keep != null)
				{
					door.KeepID = keep.KeepID;
					door.Name = "Keep Door";
					door.Health = (keep.Level + 1) * 10000;
				}
				GameServer.Database.AddNewObject(door);
				player.Out.SendMessage("Added door " + m_doorId + " to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				DoorMgr.Init();
			}
		}
	}
}
