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
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handle housing pickup item requests from the client.
	/// </summary>
	[PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerPickupHouseItem, ClientStatus.PlayerInGame)]
	public class HousingPickupItemHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		/// <summary>
		/// Handle the packet
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unknown = packet.ReadByte();
			int position = packet.ReadByte();
			int housenumber = packet.ReadShort();
			int method = packet.ReadByte();

			House house = HouseMgr.GetHouse(client.Player.CurrentRegionID, housenumber);

			if (house == null) return;
			if (client.Player == null) return;

			//log.DebugFormat("House PickupItem - Method: {0}, Position: {0}", method, position);

			switch (method)
			{
				case 1: //garden item
					// no permission to remove items from the garden, return
					if (!house.CanChangeGarden(client.Player, DecorationPermissions.Remove))
						return;

					foreach (var entry in house.OutdoorItems)
					{
						// continue if this is not the item in question
						OutdoorItem oitem = entry.Value;
						if (oitem.Position != position)
							continue;

						int i = entry.Key;
						GameServer.Database.DeleteObject(oitem.DatabaseItem); //delete the database instance

						// return indoor item into inventory item, add to player inventory
						var invitem = GameInventoryItem.Create<ItemTemplate>((house.OutdoorItems[i]).BaseItem);
						client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, invitem);
						house.OutdoorItems.Remove(i);

						// update garden
						client.Out.SendGarden(house);

						ChatUtil.SendSystemMessage(client, "Garden object removed.");
						ChatUtil.SendSystemMessage(client, string.Format("You get {0} and put it in your backpack.", invitem.Name));
						return;
					}

					//no object @ position
					ChatUtil.SendSystemMessage(client, "There is no Garden Tile at slot " + position + "!");
					break;

				case 2:
				case 3: //wall/floor mode
					// no permission to remove items from the interior, return
					if (!house.CanChangeInterior(client.Player, DecorationPermissions.Remove))
						return;

					if (house.IndoorItems.ContainsKey(position) == false)
						return;

					IndoorItem iitem = house.IndoorItems[position];
					if (iitem == null)
					{
						client.Player.Out.SendMessage("error: id was null", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
						return;
					} 

					if (iitem.BaseItem != null)
					{
						var item = GameInventoryItem.Create<ItemTemplate>((house.IndoorItems[(position)]).BaseItem);
						if (GetItemBack(item))
						{
							if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item))
							{
								string removalMsg = string.Format("The {0} is cleared from the {1}.", item.Name,
								                                  (method == 2 ? "wall surface" : "floor"));

								ChatUtil.SendSystemMessage(client, removalMsg);
							}
							else
							{
								ChatUtil.SendSystemMessage(client, "You need place in your inventory !");
								return;
							}
						}
						else
						{
							ChatUtil.SendSystemMessage(client, "The " + item.Name + " is cleared from the wall surface.");
						}
					}
					else if (iitem.DatabaseItem.BaseItemID.Contains("GuildBanner"))
					{
						var it = new ItemTemplate
						         	{
						         		Id_nb = iitem.DatabaseItem.BaseItemID,
						         		CanDropAsLoot = false,
						         		IsDropable = true,
						         		IsPickable = true,
						         		IsTradable = true,
						         		Item_Type = 41,
						         		Level = 1,
						         		MaxCharges = 1,
						         		MaxCount = 1,
						         		Model = iitem.DatabaseItem.Model,
						         		Emblem = iitem.DatabaseItem.Emblem,
						         		Object_Type = (int) eObjectType.HouseWallObject,
						         		Realm = 0,
						         		Quality = 100
						         	};

						string[] idnb = iitem.DatabaseItem.BaseItemID.Split('_');
						it.Name = idnb[1] + "'s Banner";

						// TODO: Once again with guild banners, templates are memory only and will not load correctly once player logs out - tolakram
						var inv = GameInventoryItem.Create<ItemTemplate>(it);
						if (client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inv))
						{
							string invMsg = string.Format("The {0} is cleared from the {1}.", inv.Name,
							                              (method == 2 ? "wall surface" : "floor"));

							ChatUtil.SendSystemMessage(client, invMsg);
						}
						else
						{
							ChatUtil.SendSystemMessage(client, "You need place in your inventory !");
							return;
						}
					}
					else if (method == 2)
					{
						ChatUtil.SendSystemMessage(client, "The decoration item is cleared from the wall surface.");
					}
					else
					{
						ChatUtil.SendSystemMessage(client, "The decoration item is cleared from the floor.");
					}

					GameServer.Database.DeleteObject((house.IndoorItems[(position)]).DatabaseItem);
					house.IndoorItems.Remove(position);

					var pak = new GSTCPPacketOut(client.Out.GetPacketCode(eServerPackets.HousingItem));
					pak.WriteShort((ushort) housenumber);
					pak.WriteByte(0x01);
					pak.WriteByte(0x00);
					pak.WriteByte((byte) position);
					pak.WriteByte(0x00);

					foreach (GamePlayer plr in house.GetAllPlayersInHouse())
					{
						plr.Out.SendTCP(pak);
					}

					break;
			}
		}

		#endregion

		private static bool GetItemBack(InventoryItem item)
		{
			#region item types

			switch (item.Object_Type)
			{
				case (int) eObjectType.Axe:
				case (int) eObjectType.Blades:
				case (int) eObjectType.Blunt:
				case (int) eObjectType.CelticSpear:
				case (int) eObjectType.CompositeBow:
				case (int) eObjectType.Crossbow:
				case (int) eObjectType.Flexible:
				case (int) eObjectType.Hammer:
				case (int) eObjectType.HandToHand:
				case (int) eObjectType.LargeWeapons:
				case (int) eObjectType.LeftAxe:
				case (int) eObjectType.Longbow:
				case (int) eObjectType.MaulerStaff:
				case (int) eObjectType.Piercing:
				case (int) eObjectType.PolearmWeapon:
				case (int) eObjectType.RecurvedBow:
				case (int) eObjectType.Scythe:
				case (int) eObjectType.Shield:
				case (int) eObjectType.SlashingWeapon:
				case (int) eObjectType.Spear:
				case (int) eObjectType.Staff:
				case (int) eObjectType.Sword:
				case (int) eObjectType.Thrown:
				case (int) eObjectType.ThrustWeapon:
				case (int) eObjectType.TwoHandedWeapon:
					return false;
				default:
					return true;
			}

			#endregion
		}
	}
}