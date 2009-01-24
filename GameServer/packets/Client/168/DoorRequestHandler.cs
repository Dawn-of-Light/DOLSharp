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
using System.Collections.Generic;
using DOL.Database;
using DOL.Language;
using DOL.GS.Keeps;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x31 ^ 168, "Handles open/close door requests!")]
	public class DoorRequestHandler : IPacketHandler
	{

		public static int DoorIDhandler;
		/// <summary>
		/// door index which is unique
		/// </summary>
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int DoorID = (int)packet.ReadInt();
			DoorIDhandler = DoorID;
			byte doorState = (byte)packet.ReadByte();
			int doorType = DoorID / 100000000;
			if (client.Account.PrivLevel > 1)
			{
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
					client.Out.SendDebugMessage("DoorID:{0} state:{1} zone:{2} fixture:{3} fixturePiece:{4} Type:{5})", DoorID, doorState, zoneDoor, fixture, fixturePiece, doorType);
				}
			}

			GameDoor target = client.Player.TargetObject as GameDoor;
			
			if( target != null && WorldMgr.GetDistance(client.Player, target) > 500)
			{
				client.Player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return 0;
			}
		
			DBDoor DOOR = (DBDoor)GameServer.Database.SelectObject(typeof(DBDoor), "InternalID = '" + DoorID + "'");

			if (DOOR != null)
			{
				if( doorType == 7 || doorType == 9 )
				{
					new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
					return 1;
				}
				
				if (client.Account.PrivLevel == 1 )
				{
					if (DOOR.Locked == 0)
					{
						if (DOOR.Health == 0)
						{
							new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
							return 1;					
						}
						if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
						{
							if( DOOR.Realm != 0)
							{
								new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
								return 1;
							}
						}
						if( GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal )
						{
							if( (eRealm)client.Player.Realm == (eRealm)DOOR.Realm || DOOR.Realm == 6 )
							{
								new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
								return 1;
							}
						}
					}
				}
				if( client.Account.PrivLevel > 1 )
				{
					new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
					return 1;
				}
			}
			
			if (DOOR == null)
			{
								
				if(doorType != 9 && client.Account.PrivLevel > 1)
					client.Player.Out.SendCustomDialog("This door is not in the database. Place yourself nearest to this door and clic Accept", new CustomDialogResponse(AddingDoor));

				new ChangeDoorAction(client.Player, DoorID, doorState).Start(1);
				return 1;
			}
			return 0;
		}

		public void AddingDoor ( GamePlayer player, byte response )
		{
			if( response != 0x01 )
				return;

			int doorType = DoorIDhandler / 100000000;
			if( doorType == 7 )
			{
				PositionMgr.CreateDoor(DoorIDhandler, player);
			}
			else
			{
				DBDoor door = new DBDoor( );
				door.ObjectId = null;
				door.InternalID = DoorIDhandler;
				door.Name = "door";
				door.Type = DoorIDhandler / 100000000;
				door.Level = 20;
				door.Realm = 6;
				door.MaxHealth = 2545;
				door.Health = 2545;
				door.Locked = 0;
				door.X = player.X;
				door.Y = player.Y;
				door.Z = player.Z;
				door.Heading = player.Heading;
				GameServer.Database.AddNewObject(door);

				player.Out.SendMessage("Added door " + DoorIDhandler + " to the database!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				DoorMgr.Init( );
			}
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
				List<IDoor> doorList = DoorMgr.getDoorByID(m_doorId);

				if (doorList.Count > 0)
				{
					bool success = false;
					foreach (IDoor mydoor in doorList)
					{
						if (success)
							break;
						if (mydoor is GameKeepDoor)
						{
							GameKeepDoor door = mydoor as GameKeepDoor;
							//portal keeps left click = right click
							if (door.Component.Keep is GameKeepTower && door.Component.Keep.KeepComponents.Count > 1)
								door.Interact(player);
							success = true;
						}
						else
						{
							if (WorldMgr.CheckDistance(player, mydoor, WorldMgr.PICKUP_DISTANCE))
							{
								if (m_doorState == 0x01)
									mydoor.Open();
								else
									mydoor.Close();
								success = true;
							}
						}
					}
					if (!success)
						player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "DoorRequestHandler.OnTick.TooFarAway", doorList[0].Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{

					//new frontiers we don't want this, i.e. relic gates etc
					if (player.CurrentRegionID == 163 && player.Client.Account.PrivLevel == 1)
						return;
					/*
					//create a bug report
					BugReport report = new BugReport();
					report.DateSubmitted = DateTime.Now;
					report.ID = GameServer.Database.GetObjectCount(typeof(BugReport)) + 1;
					report.Message = "There is a missing door at location Region: " + player.CurrentRegionID + " X:" + player.X + " Y: " + player.Y + " Z: " + player.Z;
					report.Submitter = player.Name;
					GameServer.Database.AddNewObject(report);
					 */

					//else basic quick hack
					GameDoor door = new GameDoor();
					door.DoorID = m_doorId;
					door.X = player.X;
					door.Y = player.Y;
					door.Z = player.Z;
					door.Realm = (eRealm)6;
					door.CurrentRegion = player.CurrentRegion;
					door.Open();
				}
			}			
		}
	}
}
