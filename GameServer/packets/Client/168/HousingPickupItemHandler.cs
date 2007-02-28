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

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handle housing pickup item requests from the client.
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, 0x0D, "Handles things like pickup indoor/outdoor items")]
	public class HousingPickupItemHandler : IPacketHandler
	{
		/// <summary>
		/// Handle the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unknown = packet.ReadByte();
			int position = packet.ReadByte();
			int housenumber = packet.ReadShort();
			int method = packet.ReadByte();

			//HouseMgr.Logger.Debug("HousingPickupItemHandler unknown" + unknown + " position " + position + " method " + method);

			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID, housenumber);

			if (house == null) return 1;
			if (client.Player == null) return 1;

			switch (method)
			{
				case 1: //garden item
                    if (!house.CanRemoveGarden(client.Player))
                        return 1;
					foreach(DictionaryEntry entry in house.OutdoorItems)
					{
						OutdoorItem oitem = (OutdoorItem)entry.Value;
						if (oitem.Position != position) 
						    continue;
						int i = (int)entry.Key;
						GameServer.Database.DeleteObject(((OutdoorItem) house.OutdoorItems[i]).DatabaseItem); //delete the database instance
						InventoryItem invitem = new InventoryItem();
						invitem.CopyFrom(((OutdoorItem) house.OutdoorItems[i]).BaseItem);
						client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
						house.OutdoorItems.Remove(i);
						client.Out.SendGarden(house);
						client.Out.SendMessage("Garden object removed.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						client.Out.SendMessage(string.Format("You get {0} and put it in your backpack.", invitem.Name), eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}
					//no object @ position
					client.Out.SendMessage("There is no Garden Tile at slot " + position + "!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					break;

				case 2:
				case 3: //wall/floor mode
                    if (!house.CanRemoveInterior(client.Player))
                        return 1;
					IndoorItem iitem = ((IndoorItem) house.IndoorItems[position]);
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
							if (method == 2)
								client.Player.Out.SendMessage("The " + item.Name + " is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							else client.Player.Out.SendMessage("The " + item.Name + " is cleared from the floor.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						}
						else
						{
							client.Player.Out.SendMessage("You need place in your inventory !", eChatType.CT_System, eChatLoc.CL_SystemWindow);
							return 1;
						}
					}
					else
						if (method == 2)
							client.Player.Out.SendMessage("The decoration item is cleared from the wall surface.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						else client.Player.Out.SendMessage("The decoration item is cleared from the floor.", eChatType.CT_System, eChatLoc.CL_SystemWindow);

					GameServer.Database.DeleteObject(((IndoorItem) house.IndoorItems[(position)]).DatabaseItem);
					house.IndoorItems.Remove(position);

					GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HousingItem));
					pak.WriteShort((ushort) housenumber);
					pak.WriteByte(0x01);
					pak.WriteByte(0x00);
					pak.WriteByte((byte)position);
					pak.WriteByte(0x00);
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
						plr.Out.SendTCP(pak);

					break;
			}
			return 1;
		}
	}
}