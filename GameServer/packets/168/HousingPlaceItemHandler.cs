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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0C, "Handles things like placing indoor/outdoor items")]
	public class HousingPlaceItemHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unkown1 = packet.ReadByte();
			int slot = packet.ReadByte();
			ushort housenumber = packet.ReadShort();
			int unkown2 = (byte)packet.ReadByte();
			int position = (byte)packet.ReadByte();
			int method = packet.ReadByte();
			int rotation = packet.ReadByte(); //garden items only
			short xpos = (short)packet.ReadShort(); //x for inside objs
			short ypos = (short)packet.ReadShort(); //y for inside objs.
			InventoryItem orgitem = client.Player.Inventory.GetItem((eInventorySlot) slot);
			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID,housenumber);
			if (orgitem == null)
				return 1;

			if (house == null)
				return 1;

			if (client.Player == null) return 1;
			if (!house.IsOwner(client.Player)) return 1;

			int pos;
			switch (method)
			{
				case 1:

					if (house.OutdoorItems.Count >= 30)
					{
						client.Player.Out.SendMessage("You have already placed 30 objects. You can't place more.", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}
					pos = GetFirstFreeSlot(house.OutdoorItems);
					client.Player.Inventory.RemoveItem(orgitem);

					OutdoorItem oitem = new OutdoorItem();
					oitem.Model = orgitem.Model;
					oitem.BaseItem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), orgitem.Id_nb);
					oitem.Position = Convert.ToByte(position);
					oitem.Rotation = Convert.ToByte(rotation);

					//add item in db
					DBHouseOutdoorItem odbitem = oitem.CreateDBOutdoorItem(housenumber);
					oitem.DatabaseItem = odbitem;
					GameServer.Database.AddNewObject(odbitem);

					//add item to outdooritems
					house.OutdoorItems.Add(pos, oitem);

					client.Player.Out.SendMessage("Garden Object placed. " + (30 - house.OutdoorItems.Count) + " slots remaining.", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
					client.Player.Out.SendMessage("You drop the " + orgitem.Name + " into your into your garden!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);

					foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort) house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
					{
						player.Out.SendGarden(house);
					}
					break;

				case 2:
				case 3:

					if (orgitem.Object_Type != 50 && method == 2)
					{
						client.Player.Out.SendMessage("This object can't be placed on a wall!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}
					if (orgitem.Object_Type != 51 && method == 3)
					{
						client.Player.Out.SendMessage("This object can't be placed on a floor!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}

					if (house.IndoorItems.Count >= 40)
					{
						client.Player.Out.SendMessage("You have already placed 40 objects. You can't place more.", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}
					IndoorItem iitem = new IndoorItem();
					iitem.Model = orgitem.Model;
					iitem.Color = orgitem.Color;
					iitem.X = xpos;
					iitem.Y = ypos;
					iitem.Rotation = 0;
					iitem.Size = 100; //? dont know how this is defined. maybe DPS_AF or something.
					iitem.Position = position;
					iitem.Placemode = method;
					iitem.BaseItem = null;
					pos = GetFirstFreeSlot(house.IndoorItems);
					if (orgitem.Object_Type == 50 || orgitem.Object_Type == 51)
					{
						//its a housing item, so lets take it!
						client.Player.Inventory.RemoveItem(orgitem);
						//set right base item, so we can recreate it on take.
						iitem.BaseItem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), orgitem.Id_nb);
					}

					DBHouseIndoorItem idbitem = iitem.CreateDBIndoorItem(housenumber);
					iitem.DatabaseItem = idbitem;
					GameServer.Database.AddNewObject(idbitem);
					house.IndoorItems.Add(pos, iitem);

					switch (method)
					{
						case 2:
							client.Player.Out.SendMessage(string.Format("You drop the {0} onto the wall of your house! (surf={1} p={2},{3},{4})", orgitem.Name, position, xpos, ypos, rotation), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
						case 3:
							client.Player.Out.SendMessage(string.Format("You drop the {0} onto the floor of your house! (surf={1} p={2},{3},{4})", orgitem.Name, position, xpos, ypos, rotation), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
					}
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
					{
						plr.Out.SendFurniture(house, pos);
					}
					break;

				case 4:

					switch (orgitem.Id_nb)
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
			}
			return 1;
		}

		protected int GetFirstFreeSlot(Hashtable tbl)
		{
			int i = 0;//tbl.Count;
			while(tbl.Contains(i))
				i++;
			return i;
		}
	}
}