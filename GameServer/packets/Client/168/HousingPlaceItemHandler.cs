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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0C, "Handles things like placing indoor/outdoor items")]
	public class HousingPlaceItemHandler : IPacketHandler
	{
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private int position;

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unknow1	= packet.ReadByte();		// 1=Money 0=Item (?)
			int slot		= packet.ReadByte();		// Item/money slot
			ushort housenumber = packet.ReadShort();	// N° of house
			int unknow2	= (byte)packet.ReadByte();	
			position	= (byte)packet.ReadByte();
			int method	= packet.ReadByte();		// 2=Wall 3=Floor
			int rotation	= packet.ReadByte();		// garden items only
			short xpos	= (short)packet.ReadShort();	// x for inside objs
			short ypos	= (short)packet.ReadShort();	// y for inside objs.

			//log.Info("U1: " + unknow1 + " - U2: " + unknow2);

			InventoryItem orgitem = client.Player.Inventory.GetItem((eInventorySlot) slot);
			House house = (House) HouseMgr.GetHouse(client.Player.CurrentRegionID,housenumber);

			//log.Info("position: " + position + " - rotation: " + rotation);
			if (orgitem == null) return 1;
			if (house == null) return 1;
			if (client.Player == null) return 1;
			
			if ((slot >= 244) && (slot <= 248)) // money
			{
                if (!house.CanPayRent(client.Player))
                    return 1;
				long MoneyToAdd = position;
				switch (slot)
				{
					case 248: MoneyToAdd *= 1; break;
					case 247: MoneyToAdd *= 100; break;
					case 246: MoneyToAdd *= 10000; break;
					case 245: MoneyToAdd *= 10000000; break;
					case 244: MoneyToAdd *= 10000000000; break;
				}
				client.Player.TempProperties.setProperty("MoneyForHouseRent", MoneyToAdd);
				client.Player.TempProperties.setProperty("HouseForHouseRent", house);
				client.Player.Out.SendInventorySlotsUpdate(null);
				client.Player.Out.SendHousePayRentDialog("Housing07");
				
				return 1;
			}

			int pos;
			switch (method)
			{
				case 1:
                    if (!house.CanAddGarden(client.Player))
                        return 1;
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
					client.Player.Out.SendMessage(string.Format("Garden Object placed. {0} slots remaining.", (30 - house.OutdoorItems.Count)), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
					client.Player.Out.SendMessage(string.Format("You drop the {0} onto your garden !", orgitem.Name), eChatType.CT_Help, eChatLoc.CL_SystemWindow);

					foreach (GamePlayer player in WorldMgr.GetPlayersCloseToSpot((ushort) house.RegionID, house.X, house.Y, house.Z, WorldMgr.OBJ_UPDATE_DISTANCE))
						player.Out.SendGarden(house);

					break;

				case 2:
				case 3:
                    if (!house.CanAddInterior(client.Player))
                        return 1;
					if (orgitem.Object_Type != 50 && method == 2)
					{
						client.Player.Out.SendMessage("This object can't be placed on a wall !", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return 1;
					}
					if (orgitem.Object_Type != 51 && method == 3)
					{
						client.Player.Out.SendMessage("This object can't be placed on the floor !", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
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
					client.Player.Out.SendMessage(string.Format("Indoor Object placed. {0} slots remaining.", (40 - house.IndoorItems.Count)), eChatType.CT_Help, eChatLoc.CL_SystemWindow);

					switch (method)
					{
						case 2:
							client.Player.Out.SendMessage(string.Format("You drop the {0} onto the wall of your house !", orgitem.Name), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
						case 3:
							client.Player.Out.SendMessage(string.Format("You drop the {0} onto the floor of your house !", orgitem.Name), eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							break;
					}
					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
						plr.Out.SendFurniture(house, pos);

					break;

				case 4:
                    if (!house.IsOwner(client.Player) && !house.CanAddGarden(client.Player))
                        return 1;
					switch (orgitem.Id_nb)
					{
						case "porch_deed":
							if (house.EditPorch(true))
								client.Player.Inventory.RemoveItem(orgitem);
							else
							{
								client.Player.Out.SendMessage("This house already has a porch !", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
								client.Out.SendInventorySlotsUpdate(new int[] { slot });
							}
							return 1;

						case "porch_remove_deed":
							if (house.EditPorch(false))
								client.Player.Inventory.RemoveItem(orgitem);
							else
							{
								client.Player.Out.SendMessage("This house has no porch !", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
								client.Out.SendInventorySlotsUpdate(new int[] { slot });
							}
							return 1;

						default:
							client.Player.Out.SendMessage("That would make no sense!", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							client.Out.SendInventorySlotsUpdate(new int[] { slot });
							return 1;

					}

				case 5:
					{
						if (!house.CanAddInterior(client.Player))
							return 1;

						if (orgitem.Object_Type != (int)eObjectType.HouseNPC
							&& orgitem.Object_Type != (int)eObjectType.HouseBindstone
							&& orgitem.Object_Type != (int)eObjectType.HouseVault
							&& orgitem.Object_Type != (int)eObjectType.HouseInteriorObject)
						{
							client.Player.Out.SendMessage("This object can't be placed on a house hookpoint !", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
							return 1;
						}

						DBHousepointItem point = new DBHousepointItem();
						point.HouseID = house.HouseNumber;
						point.ItemTemplateID = orgitem.Id_nb;
						point.Position = (uint)position;

						GameServer.Database.AddNewObject(point);

						house.FillHookpoint(orgitem, (uint)position, orgitem.Id_nb);
						house.HousepointItems[point.Position] = point;

						client.Player.Inventory.RemoveItem(orgitem);

						if (house.GetHookpointLocation((uint)position) == null)
						{
							client.Player.Out.SendMessage("The housepoint ID is: " + position, eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							client.Player.Out.SendMessage("Stand as close to the housepoint as possible and face the proper direction and press accept", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
							client.Player.Out.SendCustomDialog("Log this housepoint location?", new CustomDialogResponse(LogLocation));
						}
						break;
					}
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

		/// <summary>
		/// Does the player want to log the offset location of the missing housepoint
		/// </summary>
		/// <param name="player">The player</param>
		/// <param name="response">1 = yes 0 = no</param>
		protected void LogLocation(GamePlayer player, byte response)
		{
			if (response != 0x01)
				return;

			if (player.CurrentHouse == null)
				return;

			log.Error("Position: " + position + " Offset: " + (player.X - player.CurrentHouse.X) + ", " + (player.Y - player.CurrentHouse.Y) + ", " + (player.Z - 25000) + ", " + (player.Heading - player.CurrentHouse.Heading));

			player.Out.SendMessage("Logged housepoint position " + position + " in error.log!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
		}
	}
}