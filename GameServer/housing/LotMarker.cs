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
using DOL.GS.PacketHandler;

namespace DOL.GS.Housing
{
	public class GameLotMarker : GameStaticItem
	{
		public GameLotMarker() : base()
		{
			SaveInDB = false;
		}

		private DBHouse m_dbitem;

		public DBHouse DatabaseItem
		{
			get { return m_dbitem; }
			set { m_dbitem = value; }
		}

		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You target lot number " + DatabaseItem.HouseNumber + ".");
			if (DatabaseItem.OwnerIDs == null || DatabaseItem.OwnerIDs == "")
			{
				list.Add(" It can be bought for " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + ".");
			}
			return list;
		}

		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
			{
				return false;
			}
			if (DatabaseItem.OwnerIDs == null || DatabaseItem.OwnerIDs == "")
			{
				player.Out.SendCustomDialog("Do you want to buy this lot?\r\n It costs " + Money.GetString(HouseTemplateMgr.GetLotPrice(DatabaseItem)) + "!", new CustomDialogResponse(BuyLot));
			}
			else
			{
				if (HouseMgr.IsOwner(DatabaseItem, player))
				{
					player.Out.SendMerchantWindow(HouseTemplateMgr.GetLotMarkerItems(this), eMerchantWindowType.Normal);
				}
				else
				{
					player.Out.SendMessage("You do not own this lot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			return true;
		}

		private void BuyLot(GamePlayer player, byte response)
		{
			if (response != 0x01) return;
			lock (DatabaseItem) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (DatabaseItem.OwnerIDs != null && DatabaseItem.OwnerIDs != "") return;
				if (HouseMgr.GetHouseNumberByPlayer(player) != 0 && player.Client.Account.PrivLevel <= 1)
				{
					player.Out.SendMessage("You already own another lot or house (Number " + HouseMgr.GetHouseNumberByPlayer(player) + ").", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
					return;
				}
				if (player.RemoveMoney(HouseTemplateMgr.GetLotPrice(DatabaseItem), "You just bought this lot for {0}.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
				{
					DatabaseItem.LastPaid = DateTime.Now;
					HouseMgr.AddOwner(DatabaseItem,player);
				}
				else
				{
					player.Out.SendMessage("You dont have enough money!", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				}
			}
		}

		public override bool ReceiveItem(GameLiving source, InventoryItem item)
		{
			if (source == null || item == null) return false;
			if (!(source is GamePlayer)) return false;

			GamePlayer player = (GamePlayer) source;
			if (HouseMgr.IsOwner(DatabaseItem, player))
			{
				switch (item.Id_nb)
				{
					case "alb_cottage_deed":
						CreateHouse(player, 1);
						break;
					case "alb_house_deed":
						CreateHouse(player, 2);
						break;
					case "alb_villa_deed":
						CreateHouse(player, 3);
						break;
					case "alb_mansion_deed":
						CreateHouse(player, 4);
						break;
					case "mid_cottage_deed":
						CreateHouse(player, 5);
						break;
					case "mid_house_deed":
						CreateHouse(player, 6);
						break;
					case "mid_villa_deed":
						CreateHouse(player, 7);
						break;
					case "mid_mansion_deed":
						CreateHouse(player, 8);
						break;
					case "hib_cottage_deed":
						CreateHouse(player, 9);
						break;
					case "hib_house_deed":
						CreateHouse(player, 10);
						break;
					case "hib_villa_deed":
						CreateHouse(player, 11);
						break;
					case "hib_mansion_deed":
						CreateHouse(player, 12);
						break;
					default:
						player.Out.SendMessage("That would make no sense!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
				player.Inventory.RemoveItem(item);
				return true;
			}
			else
			{
				player.Out.SendMessage("You do not own this lot!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
			return false;
		}

		private void CreateHouse(GamePlayer player, int model)
		{
			DatabaseItem.Model = model;
			DatabaseItem.Name = player.Name;
			DatabaseItem.RoofMaterial = 0;
			DatabaseItem.DoorMaterial = 0;
			DatabaseItem.WallMaterial = 0;
			DatabaseItem.TrussMaterial = 0;
			DatabaseItem.WindowMaterial = 0;
			DatabaseItem.PorchMaterial = 0;

			if (player.Guild != null)
			{
				DatabaseItem.Emblem = player.Guild.theGuildDB.Emblem;
			}

			House house = new House(DatabaseItem);
			HouseMgr.AddHouse(house);
			// move all players outside the mesh
			foreach (GamePlayer p in player.GetPlayersInRadius(500))
				house.Exit(p, true);
			this.RemoveFromWorld();
			this.Delete();
		}

		public virtual bool OnPlayerBuy(GamePlayer player, int item_slot, int number)
		{
			GameMerchant.OnPlayerBuy(player, item_slot,number, HouseTemplateMgr.GetLotMarkerItems(this));
			return true;
		}

		public virtual bool OnPlayerSell(GamePlayer player, InventoryItem item)
		{
			if (!item.IsDropable)
			{
				player.Out.SendMessage("This item can't be sold.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
				return false;
			}
			return true;
		}

		public long OnPlayerAppraise(GamePlayer player, InventoryItem item, bool silent)
		{
			if (item == null)
				return 0;

			int itemCount = Math.Max(1, item.Count);
			return item.Value*itemCount/2;
		}

		public override void SaveIntoDatabase()
		{
			// do nothing !!!
		}

		public static void SpawnLotMarker(DBHouse house)
		{
			GameLotMarker obj = new GameLotMarker();
			obj.X = house.X;
			obj.Y = house.Y;
			obj.Z = house.Z;
			obj.CurrentRegionID = (ushort)house.RegionID;
			obj.Heading = (ushort) house.Heading;
			obj.Name = "Lot Marker";
			obj.Model = 1308;
			obj.DatabaseItem = house;

			//No clue how we can check if a region
			//is in albion, midgard or hibernia instead
			//of checking the region id directly
			switch (obj.CurrentRegionID)
			{
				case 2:
					obj.Model = 1308;
					obj.Name = "Albion Lot";
					break; //ALB
				case 102:
					obj.Model = 1306;
					obj.Name = "Midgard Lot";
					break; //MID
				default: //case 202:
					obj.Model = 1307;
					obj.Name = "Hibernia Lot";
					break; //HIB
			}
			obj.AddToWorld();
		}
	}
}