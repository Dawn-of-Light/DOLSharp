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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0D, "Handles things like pickup indoor/outdoor items")]
	public class HousingPickupItemHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unkown = packet.ReadByte();
			int position = packet.ReadByte();
			int housenumber = packet.ReadShort();
			int method = packet.ReadByte();


			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID,housenumber);

			if (house == null)
				return 1;

			switch (method)
			{
				case 1: //garden item

					for (int i = 0; i < house.OutdoorItems.Count; i++)
					{
						if (((OutdoorItem) house.OutdoorItems[i]).Position == position)
						{
							GameServer.Database.DeleteObject(((OutdoorItem) house.OutdoorItems[i]).DatabaseItem); //delete the database instance

							InventoryItem invitem = new InventoryItem();
							invitem.CopyFrom(((OutdoorItem) house.OutdoorItems[i]).BaseItem);
							client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);

							house.OutdoorItems.RemoveAt(i);

							client.Out.SendGarden(house);

							client.Out.SendMessage("Garden Tile Removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							client.Out.SendMessage("You get " + invitem.Name + " and put it in your backpack.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					//no object @ position
					client.Out.SendMessage("There is no Garden Tile at slot " + position + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;

				case 2:
				case 3: //wall/floor mode
					IndoorItem iitem = ((IndoorItem) house.IndoorItems[(position)]);
					if (iitem == null)
					{
						client.Player.Out.SendMessage("error: id was null", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					} //should this ever happen?

					if (iitem.BaseItem != null)
					{
						InventoryItem item = new InventoryItem();
						item.CopyFrom(((IndoorItem) house.IndoorItems[(position)]).BaseItem);
						if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
						{
							client.Player.Out.SendMessage("The " + item.Name + " is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							//you need free slot :p
							return 1;
						}
					}
					else
					{
						client.Player.Out.SendMessage("The decoration item is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}

					GameServer.Database.DeleteObject(((IndoorItem) house.IndoorItems[(position)]).DatabaseItem);
					house.IndoorItems.RemoveAt((position));

					GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HousingItem));
					pak.WriteShort((ushort) housenumber);
					pak.WriteByte(0x01);
					pak.WriteByte(0x00);
					pak.WriteByte(Convert.ToByte(position));
					pak.WriteByte(0x00);
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
					{
						plr.Out.SendTCP(pak);
					}

					break;
			}
			return 1;
		}
	}
}