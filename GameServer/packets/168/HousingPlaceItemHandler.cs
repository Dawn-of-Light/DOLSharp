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
using DOL.GS.Database;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0C, "Handles things like placing indoor/outdoor items")]
	public class HousingPlaceItemHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unkown1 = packet.ReadByte();
			int slot = packet.ReadByte();
			int housenumber = packet.ReadShort();
			int unkown2 = packet.ReadByte();
			int position = packet.ReadByte();
			int method = packet.ReadByte();
			int rotation = packet.ReadByte(); //garden items only
			int xpos = packet.ReadShort(); //x for inside objs
			int ypos = packet.ReadShort(); //y for inside objs.

			/*GenericItem orgitem = client.Player.Inventory.GetItem((eInventorySlot) slot);
			House house = (House) HouseMgr.GetHouse(client.Player.Region, housenumber);

			if (house == null)
				return 1;

			switch (method)
			{
				case 1:

					if (house.OutdoorItems.Count > 30)
					{
						client.Player.Out.SendMessage("You have already placed 30 objects. You can't place more.", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}

					client.Player.Inventory.RemoveItem(orgitem);

					OutdoorItem iitem = new OutdoorItem();
					iitem.Model = orgitem.Model;
					iitem.BaseItem = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), orgitem.ItemID);
					iitem.Position = Convert.ToByte(position);
					iitem.Rotation = Convert.ToByte(rotation);

					//add item in db
					DBHouseOutdoorItem idbitem = iitem.CreateDBOutdoorItem(housenumber);
					iitem.DatabaseItem = idbitem;
					GameServer.Database.AddNewObject(idbitem);

					//add item to outdooritems
					house.OutdoorItems.Add(iitem);

					client.Player.Out.SendMessage("Garden Object placed. " + (30 - house.OutdoorItems.Count) + " slots remaining.", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
					client.Player.Out.SendMessage("You drop the " + orgitem.Name + " into your into your garden!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);

					foreach (GamePlayer player in house.Region.GetPlayerInRadius(house.Position, WorldMgr.OBJ_UPDATE_DISTANCE, false))
					{
						player.Out.SendGarden(house);
					}
					break;

				case 2:
				case 3:

					if ((int)orgitem.ObjectType == 51 && method == 3)
					{
						client.Player.Out.SendMessage("You have to put this item on the floor, not on a wall!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}
					if ((int)orgitem.ObjectType == 50 && method == 2)
					{
						client.Player.Out.SendMessage("You have to put this item on a wall, not on the floor!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}

					IndoorItem oitem = new IndoorItem();
					oitem.Model = orgitem.Model;
				//	oitem.Color = orgitem.Color;
					oitem.X = xpos;
					oitem.Y = ypos;
					oitem.Rotation = 0;
					oitem.Size = 100; //? dont know how this is defined. maybe DPS_AF or something.
					oitem.Position = position;
					oitem.Placemode = method;
					oitem.BaseItem = null;

					if ((int)orgitem.ObjectType == 50 || (int)orgitem.ObjectType == 51)
					{
						//its a housing item, so lets take it!
						client.Player.Inventory.RemoveItem(orgitem);
						//set right base item, so we can recreate it on take.
						oitem.BaseItem = (GenericItemTemplate) GameServer.Database.FindObjectByKey(typeof (GenericItemTemplate), orgitem.ItemID);
					}

					DBHouseIndoorItem odbitem = oitem.CreateDBIndoorItem(housenumber);
					oitem.DatabaseItem = odbitem;
					GameServer.Database.AddNewObject(odbitem);
					house.IndoorItems.Add(oitem);

					switch (method)
					{
						case 2:
							client.Player.Out.SendMessage("You put " + orgitem.Name + " on the wall of the house.(Position=" + position + ",X=" + xpos + ",Y=" + ypos + ")", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
						case 3:
							client.Player.Out.SendMessage("You put " + orgitem.Name + " on the floor of the house.(Position=" + position + ",X=" + xpos + ",Y=" + ypos + ")", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
					}
					//TODO: Move this into packetlib
					GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HousingItem));
					pak.WriteShort((ushort) housenumber);
					pak.WriteByte(0x01); //cnt
					pak.WriteByte(0x00); //upd
					pak.WriteByte(Convert.ToByte(house.IndoorItems.Count));
					pak.WriteShort((ushort) oitem.Model);
					pak.WriteShort((ushort) oitem.Color);
					pak.WriteByte(0x00);
					pak.WriteByte(0x00);
					pak.WriteShort((ushort) oitem.X);
					pak.WriteShort((ushort) oitem.Y);
					pak.WriteShort((ushort) oitem.Rotation);
					pak.WriteByte(Convert.ToByte(oitem.Size));
					pak.WriteByte(Convert.ToByte(oitem.Position));
					pak.WriteByte(Convert.ToByte(oitem.Placemode - 2));
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
					{
						plr.Out.SendTCP(pak);
					}


					break;

				case 4:

					switch (orgitem.ItemID)
					{
						case "porch_deed":
							if (house.EditPorch(true))
							{
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else
							{
								client.Player.Out.SendMessage("This house HAS a porch!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							}
							return 1;

						case "porch_remove_deed":
							if (house.EditPorch(false))
							{
								client.Player.Inventory.RemoveItem(orgitem);
							}
							else
							{
								client.Player.Out.SendMessage("This house HAS NO a porch!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							}
							return 1;

						default:
							client.Player.Out.SendMessage("That would make no sense!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							return 1;

					}

				case 5:
					client.Player.Out.SendMessage("No hookpoints now!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
					break;

				default:
					break;
			}*/

			return 1;
		}
	}
}