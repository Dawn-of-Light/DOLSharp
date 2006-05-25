using System;
using DOL.Database;
using DOL.GS.Database;
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
				IDoor mydoor = DoorMgr.GetDoor(m_doorId);

				if (mydoor != null)
				{
					if (!player.Position.CheckSquareDistance(mydoor.Position, (uint)(WorldMgr.PICKUP_DISTANCE*WorldMgr.PICKUP_DISTANCE)))
					{
						player.Out.SendMessage("You are too far away to open this door!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
						return;
					}
					if (m_doorState == 0x01) // the client always sens 0x01 ...
					{
						if(mydoor.State == eDoorState.Open)
						{
							mydoor.Close();
						}	
						else
						{
							mydoor.Open();
						}
					}
				}
				else
				{
					if(player.Client.Account.PrivLevel == ePrivLevel.Player)
					{
						player.Out.SendMessage("Sorry, this door is not in the database yet and it can't be opened!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.TempProperties.setProperty("NewDoorID", m_doorId);
						player.Out.SendCustomDialog("This door is not in the database yet, do you want to add it now ?", new CustomDialogResponse(AddDoorDialogResponse));
					}
				}			
			}

			/// <summary>
			/// The callback hook used when a player answear to the blacksmith dialog
			/// </summary>
			protected void AddDoorDialogResponse(GamePlayer player, byte response)
			{
				int newDoorID = player.TempProperties.getIntProperty("NewDoorID", -1);
				player.TempProperties.removeProperty("NewDoorID");

				if (response != 0x01)
					return;

				GameDoor newDoor = new GameDoor();
				newDoor.Name ="door";
				newDoor.DoorID = newDoorID;
				newDoor.Region = player.Region;
				newDoor.Position = player.Position;
				newDoor.Realm = player.Realm;
				newDoor.Model = 0xFFFF;

				if(DoorMgr.AddDoor(newDoor))
				{
					newDoor.AddToWorld();

					GameServer.Database.AddNewObject(newDoor);

					player.Out.SendMessage("The door with the DoorID "+newDoorID+" is now saved in your database.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);	
				}
			}
		}
	}
}