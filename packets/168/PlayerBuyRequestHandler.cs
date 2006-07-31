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
using System.Reflection;
using DOL.Database;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0xD0 ^ 168, "Handles player buy")]
	public class PlayerBuyRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint X = packet.ReadInt();
			uint Y = packet.ReadInt();
			ushort id = packet.ReadShort();
			ushort item_slot = packet.ReadShort();
			byte item_count = (byte) packet.ReadByte();
			byte merchantWindowType = (byte) packet.ReadByte();

			if (client.Player.TargetObject == null && merchantWindowType != (byte)eMerchantWindowType.HousingInsideShop && merchantWindowType != (byte)eMerchantWindowType.HousingOutsideShop)
				return 0;

			MerchantTradeItems items = null;

			//--------------------------------------------------------------------------
			//Retrieve the item list, the list can be retrieved from any object. Usually
			//a merchant but could be anything eg. Housing Lot Markers etc.
			if (merchantWindowType == (byte)eMerchantWindowType.HousingInsideShop)
				items = HouseTemplateMgr.IndoorShopItems;
			else if (merchantWindowType == (byte)eMerchantWindowType.HousingOutsideShop)
				items = HouseTemplateMgr.OutdoorShopItems;
			else if (client.Player.TargetObject is GameMerchant)
			{
				//If merchant doesn't want to sell, return
				if (!((GameMerchant) client.Player.TargetObject).OnPlayerBuy(client.Player, item_slot, item_count))
					return 0;

				//Get the items from the merchant for further interaction
				items = ((GameMerchant) client.Player.TargetObject).TradeItems;

				//if (items == null || items.ItemsList == null)
				//	return 0;
			}
			else if (client.Player.TargetObject is GameLotMarker)
			{
				if (!((GameLotMarker) client.Player.TargetObject).OnPlayerBuy(client.Player, item_slot, item_count))
					return 0;

				items = HouseTemplateMgr.GetLotMarkerItems((GameLotMarker) client.Player.TargetObject);
			}
			else
			{
				//If buying from other objects should be possible you have to add else if blocks
				//to retrieve the item list here
				return 0;
			}
			//--------------------------------------------------------------------------


			//Check item count
			//if(item_slot > items.ItemsList.Count)
			//	return 0;

			//Get the template
			int pagenumber = item_slot/MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;
			int slotnumber = item_slot%MerchantTradeItems.MAX_ITEM_IN_TRADEWINDOWS;

			ItemTemplate template = items.GetItem(pagenumber, (eMerchantWindowSlot) slotnumber);
			if (template == null)
				return 1;

			//Calculate the amout of items
			int amountToBuy = item_count;
			if (template.PackSize > 0)
				amountToBuy *= template.PackSize;

			if (amountToBuy <= 0)
				return 0;


			lock (client.Player.Inventory)
			{
				//Calculate the value of items
				long totalValue = item_count*template.Value;
				if (client.Player.TargetObject is GameBountyMerchant)
				{ 
					if (client.Player.BountyPoints < totalValue) 
					{ 
						client.Player.Out.SendMessage("You need " + totalValue + " BPs to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow); 
						return 0; 
					} 
					if (!client.Player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)) 
					{ 
						client.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow); 
						return 1; 
					} 
					//Generate the buy message 
					string message; 
					if (item_count > 1) 
						message = "You just bought " + item_count + " pieces of " + template.GetName(1, false) + " for " +totalValue+" BPs."; 
					else 
						message = "You just bought " + template.GetName(1, false) + " for "+totalValue+" BPs."; 
					client.Player.BountyPoints -= totalValue; 
					client.Player.Out.SendUpdatePoints(); 
					client.Player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow); 
				} 
				else if(client.Player.TargetObject is GameCountMerchant)
				{
					GameCountMerchant merc = client.Player.TargetObject as GameCountMerchant;
					int count = 0;
					foreach(InventoryItem item in client.Player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,eInventorySlot.LastBackpack))
					{
						if (item.Id_nb == merc.moneyItem.Item.Id_nb)
							count += item.Count;
					}

					if(count < totalValue)
					{
						client.Player.Out.SendMessage("You need " + totalValue + " " + merc.moneyItem.Name +" to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow); 
						return 0;
					}

					if (!client.Player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack)) 
					{ 
						client.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow); 
						return 1; 
					}

					foreach(InventoryItem item in client.Player.Inventory.GetItemRange(eInventorySlot.FirstBackpack,eInventorySlot.LastBackpack))
					{
						if (item.Id_nb == merc.moneyItem.Item.Id_nb)
						{
							if(item.Count <= totalValue)
							{
								totalValue -= item.Count;
								client.Player.Inventory.RemoveItem(item);
							}
							else
							{								
								client.Player.Inventory.RemoveCountFromStack(item,(int)totalValue);
								totalValue = 0;
							}
						}
						if(totalValue == 0)
							break;
					}

					//Generate the buy message 
					string message; 
					if (item_count > 1) 
						message = "You just bought " + item_count + " pieces of " + template.GetName(1, false) + " for " +item_count*template.Value+" "+merc.moneyItem.Name; 
					else 
						message = "You just bought " + template.GetName(1, false) + " for "+item_count*template.Value+" "+merc.moneyItem.Name; 					
					client.Player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow); 
				}
				else 
				{
					if (client.Player.GetCurrentMoney() < totalValue)
					{
						client.Player.Out.SendMessage("You need " + Money.GetString(totalValue) + " to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 0;
					}

					if (!client.Player.Inventory.AddTemplate(template, amountToBuy, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						client.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
						return 1;
					}

					//Generate the buy message
					string message;
					if (item_count > 1)
						message = "You just bought " + item_count + " pieces of " + template.GetName(1, false) + " for {0}.";
					else
						message = "You just bought " + template.GetName(1, false) + " for {0}.";

					// Check if player has enough money and subtract the money
					if (!client.Player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
					{
						throw new Exception("Money amount changed while adding items.");
					}
				}
			}
			return 1;
		}
	}
}