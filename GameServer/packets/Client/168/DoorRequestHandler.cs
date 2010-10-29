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
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Keeps;
using DOL.GS.ServerProperties;
using DOL.Language;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.DoorRequest, ClientStatus.PlayerInGame)]
	public class DoorRequestHandler : IPacketHandler
	{
		public static int m_handlerDoorID;

		#region IPacketHandler Members

		/// <summary>
		/// door index which is unique
		/// </summary>
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			var doorID = (int) packet.ReadInt();
			m_handlerDoorID = doorID;
			var doorState = (byte) packet.ReadByte();
			int doorType = doorID/100000000;

			int radius = ServerProperties.Properties.WORLD_PICKUP_DISTANCE * 2;
			int zoneDoor = (int)(doorID / 1000000);

			string debugText = "";

			// For ToA the client always sends the same ID so we need to construct an id using the current zone
			if (client.Player.CurrentRegion.Expansion == (int)eClientExpansion.TrialsOfAtlantis)
			{
				debugText = "ToA DoorID: " + doorID + " ";

				doorID -= zoneDoor * 1000000;
				zoneDoor = client.Player.CurrentZone.ID;
				doorID += zoneDoor * 1000000;
				m_handlerDoorID = doorID;

				// experimental to handle a few odd TOA door issues
				if (client.Player.CurrentRegion.IsDungeon)
					radius *= 4;
			}

			// debug text
			if (client.Account.PrivLevel > 1 || Properties.ENABLE_DEBUG)
			{
				if (doorType == 7)
				{
					int ownerKeepId = (doorID/100000)%1000;
					int towerNum = (doorID/10000)%10;
					int keepID = ownerKeepId + towerNum*256;
					int componentID = (doorID/100)%100;
					int doorIndex = doorID%10;
					client.Out.SendDebugMessage(
						"Keep Door ID: {0} state:{1} (Owner Keep: {6} KeepID:{2} ComponentID:{3} DoorIndex:{4} TowerNumber:{5})", doorID,
						doorState, keepID, componentID, doorIndex, towerNum, ownerKeepId);
				}
				else if (doorType == 9)
				{
					int doorIndex = doorID - doorType*10000000;
					client.Out.SendDebugMessage("House DoorID:{0} state:{1} (doorType:{2} doorIndex:{3})", doorID, doorState, doorType,
					                            doorIndex);
				}
				else
				{
					int fixture = (doorID - zoneDoor*1000000);
					int fixturePiece = fixture;
					fixture /= 100;
					fixturePiece = fixturePiece - fixture*100;

					client.Out.SendDebugMessage("{6}DoorID:{0} state:{1} zone:{2} fixture:{3} fixturePiece:{4} Type:{5}",
												doorID, doorState, zoneDoor, fixture, fixturePiece, doorType, debugText);
				}
			}

			var target = client.Player.TargetObject as GameDoor;

			if (target != null && !client.Player.IsWithinRadius(target, radius))
			{
				client.Player.Out.SendMessage("You are too far to open this door", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
				return;
			}

			var door = GameServer.Database.SelectObject<DBDoor>("InternalID = '" + doorID + "'");

			if (door != null)
			{
				if (doorType == 7 || doorType == 9)
				{
					new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
					return;
				}

				if (client.Account.PrivLevel == 1)
				{
					if (door.Locked == 0)
					{
						if (door.Health == 0)
						{
							new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
							return;
						}

						if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_PvP)
						{
							if (door.Realm != 0)
							{
								new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
								return;
							}
						}

						if (GameServer.Instance.Configuration.ServerType == eGameServerType.GST_Normal)
						{
							if (client.Player.Realm == (eRealm) door.Realm || door.Realm == 6)
							{
								new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
								return;
							}
						}
					}
				}

				if (client.Account.PrivLevel > 1)
				{
					client.Out.SendDebugMessage("GM: Forcing locked door open.");
					new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
					return;
				}
			}

			if (door == null)
			{
				if (doorType != 9 && client.Account.PrivLevel > 1 && client.Player.CurrentRegion.IsInstance == false)
				{
					if (client.Player.TempProperties.getProperty(DoorMgr.WANT_TO_ADD_DOORS, false))
					{
						client.Player.Out.SendCustomDialog(
							"This door is not in the database. Place yourself nearest to this door and click Accept to add it.", AddingDoor);
					}
					else
					{
						client.Player.Out.SendMessage(
							"This door is not in the database. Use '/door show' to enable the add door dialog when targeting doors.",
							eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
				}

				new ChangeDoorAction(client.Player, doorID, doorState, radius).Start(1);
				return;
			}
		}

		#endregion

		public void AddingDoor(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			int doorType = m_handlerDoorID/100000000;
			if (doorType == 7)
			{
				PositionMgr.CreateDoor(m_handlerDoorID, player);
			}
			else
			{
				var door = new DBDoor();
				door.ObjectId = null;
				door.InternalID = m_handlerDoorID;
				door.Name = "door";
				door.Type = m_handlerDoorID/100000000;
				door.Level = 20;
				door.Realm = 6;
				door.MaxHealth = 2545;
				door.Health = 2545;
				door.Locked = 0;
				door.X = player.X;
				door.Y = player.Y;
				door.Z = player.Z;
				door.Heading = player.Heading;
				GameServer.Database.AddObject(door);

				player.Out.SendMessage("Added door " + m_handlerDoorID + " to the database!", eChatType.CT_Important,
				                       eChatLoc.CL_SystemWindow);
				DoorMgr.Init();
			}
		}

		#region Nested type: ChangeDoorAction

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
			/// allowed distance to door
			/// </summary>
			protected readonly int m_radius;

			/// <summary>
			/// Constructs a new ChangeDoorAction
			/// </summary>
			/// <param name="actionSource">The action source</param>
			/// <param name="doorId">The target door Id</param>
			/// <param name="doorState">The door state</param>
			public ChangeDoorAction(GamePlayer actionSource, int doorId, int doorState, int radius)
				: base(actionSource)
			{
				m_doorId = doorId;
				m_doorState = doorState;
				m_radius = radius;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;
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
							var door = mydoor as GameKeepDoor;
							//portal keeps left click = right click
							if (door.Component.Keep is GameKeepTower && door.Component.Keep.KeepComponents.Count > 1)
								door.Interact(player);
							success = true;
						}
						else
						{
							if (player.IsWithinRadius(mydoor, m_radius))
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
						player.Out.SendMessage(
							LanguageMgr.GetTranslation(player.Client, "DoorRequestHandler.OnTick.TooFarAway", doorList[0].Name),
							eChatType.CT_System, eChatLoc.CL_SystemWindow);
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
					report.ID = GameServer.Database.GetObjectCount<BugReport>() + 1;
					report.Message = "There is a missing door at location Region: " + player.CurrentRegionID + " X:" + player.X + " Y: " + player.Y + " Z: " + player.Z;
					report.Submitter = player.Name;
					GameServer.Database.AddObject(report);
					 */

					player.Out.SendDebugMessage("Door {0} not found in door list, opening via GM door hack.", m_doorId);

					//else basic quick hack
					var door = new GameDoor();
					door.DoorID = m_doorId;
					door.X = player.X;
					door.Y = player.Y;
					door.Z = player.Z;
					door.Realm = eRealm.Door;
					door.CurrentRegion = player.CurrentRegion;
					door.Open();
				}
			}
		}

		#endregion
	}
}