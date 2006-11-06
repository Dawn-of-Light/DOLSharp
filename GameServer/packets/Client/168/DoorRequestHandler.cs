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

using DOL.Database;
using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x31 ^ 168, "Handles open/close door requests!")]
	public class DoorRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int DoorID = (int)packet.ReadInt();
			byte doorState = (byte)packet.ReadByte();

			if (client.Account.PrivLevel > 1)
			{
				int doorType = DoorID / 100000000;
				if (doorType == 7)
				{
					int ownerKeepId = (DoorID / 100000) % 1000;
					int towerNum = (DoorID / 10000) % 10;
					int keepID = ownerKeepId + towerNum * 256;
					int componentID = (DoorID / 100) % 100;
					int doorIndex = DoorID % 10;
					client.Out.SendDebugMessage("Keep Door ID: {0} state:{1} (Owner Keep: {6} KeepID:{2} ComponentID:{3} DoorIndex:{4} TowerNumber:{5})", DoorID, doorState, keepID, componentID, doorIndex, towerNum, ownerKeepId);
				}
				else if (doorType == 9)
				{
					int doorIndex = DoorID - doorType * 10000000;
					client.Out.SendDebugMessage("House DoorID:{0} state:{1} (doorType:{2} doorIndex:{3})", DoorID, doorState, doorType, doorIndex);
				}
				else
				{
					int zoneDoor = (int)(DoorID / 1000000);
					int fixture = (int)(DoorID - zoneDoor * 1000000);
					int fixturePiece = fixture;
					fixture /= 100;
					fixturePiece = fixturePiece - fixture * 100;
					client.Out.SendDebugMessage("Fixture DoorID:{0} state:{1} (zone:{2} fixture:{3} fixturePiece:{4})", DoorID, doorState, zoneDoor, fixture, fixturePiece);
				}
			}

			new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);

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
			public ChangeDoorAction(GamePlayer actionSource, int doorId, int doorState)
				: base(actionSource)
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
						if (door.Component.Keep is GameKeepTower && door.Component.Keep.KeepComponents.Count > 1)
							door.Interact(player);
					}
					else
					{
						if (!WorldMgr.CheckDistance(player, mydoor, WorldMgr.PICKUP_DISTANCE))
						{
							player.Out.SendMessage("The " + mydoor.Name + " is too far away!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					//new frontiers we don't want this, i.e. relic gates etc
					if (player.CurrentRegionID == 163 && player.Client.Account.PrivLevel == 1)
						return;

					//create a bug report
					BugReport report = new BugReport();
					report.DateSubmitted = DateTime.Now;
					report.ID = GameServer.Database.GetObjectCount(typeof(BugReport)) + 1;
					report.Message = "There is a missing door at location Region: " + player.CurrentRegionID + " X:" + player.X + " Y: " + player.Y;
					report.Submitter = player.Name;
					GameServer.Database.AddNewObject(report);

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
						int doorType = m_doorId / 100000000;
						if (doorType == 7)
						{
							player.Out.SendCustomDialog("Add this door to the database?", new CustomDialogResponse(AddingDoor));
						}
						else
						{
							DBDoor doorCheck = GameServer.Database.SelectObject(typeof(DBDoor), "InternalID = " + m_doorId) as DOL.Database.DBDoor;
							if (doorCheck == null)
							{
								player.Out.SendMessage("The door's ID is: " + m_doorId.ToString(), eChatType.CT_Important, eChatLoc.CL_SystemWindow);
								player.Out.SendMessage("Stand as close to the door as possible and face outwards (best done from inside and behind it", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

								player.Out.SendCustomDialog("Add this door to the database?", new CustomDialogResponse(AddingDoor));
							}
						}
					}
				}
			}

			protected void AddingDoor(GamePlayer player, byte response)
			{
				if (response != 0x01)
					return;

				int doorType = m_doorId / 100000000;
				if (doorType == 7)
				{
					PositionMgr.CreateDoor(m_doorId, player);
				}
				else
				{

					DBDoor door = new DBDoor();
					door.ObjectId = null;
					door.InternalID = m_doorId;
					door.X = player.X;
					door.Y = player.Y;
					door.Z = player.Z;
					door.Name = "door";
					door.Heading = player.Heading;
					GameServer.Database.AddNewObject(door);

					player.Out.SendMessage("Added door " + m_doorId + " to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					DoorMgr.Init();
				}
			}
		}
	}
}
