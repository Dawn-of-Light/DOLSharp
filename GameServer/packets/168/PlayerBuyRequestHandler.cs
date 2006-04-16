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
using System.Reflection;
using DOL.GS.Database;
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

			if(item_count <= 0) return -1;

			IGameMerchant merchant = client.Player.TargetObject as IGameMerchant;
			if(merchant == null)
			{
				client.Player.Out.SendMessage("Only a merchant can sell this item.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
				return 0;
			}

			if(!merchant.OnPlayerBuy(client.Player, item_slot, item_count))
			{
				client.Player.Out.SendMessage("This merchant can't sell your this item.", eChatType.CT_Merchant,eChatLoc.CL_SystemWindow);
				return 0;
			}

			//Get the template
			int pageNumber = item_slot / MerchantPage.MAX_ITEMS_IN_MERCHANTPAGE;
			int slotNumber = item_slot % MerchantPage.MAX_ITEMS_IN_MERCHANTPAGE;

			eCurrencyType currencyType = eCurrencyType.Money;
			GenericItemTemplate itemTemplate = null;

			MerchantWindow window = GameServer.Database.FindObjectByKey(typeof(MerchantWindow), merchant.MerchantWindowID) as MerchantWindow;	
			if (window != null)
			{
				MerchantPage page = window.MerchantPages[pageNumber] as MerchantPage;
				if(page != null)
				{
					currencyType = page.Currency;
					MerchantItem merchantItem = page.MerchantItems[slotNumber] as MerchantItem;
					if (merchantItem != null)
					{
						itemTemplate = merchantItem.ItemTemplate;
					}
				}
			}

			if(itemTemplate == null) return 1;

			lock (client.Player.Inventory)
			{
				// First : check if enough space in the inventory to but this item
				int slotNeeded = item_count;
				
				if(itemTemplate is IStackableItemTemplate)
				{
					IStackableItem itemToAdd = (IStackableItem)itemTemplate.CreateInstance();

					int countToAdd = item_count * itemToAdd.Count;
					foreach (GenericItem currentItem in client.Player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
					{
						IStackableItem currentItemStackable = currentItem as IStackableItem;
						if(currentItemStackable != null && currentItemStackable.CanStackWith(itemToAdd))
						{
                            countToAdd -= (currentItemStackable.MaxCount - currentItemStackable.Count);
							if(countToAdd <= 0)
							{
								break;
							}
						}
					}
					slotNeeded = countToAdd / itemToAdd.MaxCount;
				}

				if(!client.Player.Inventory.IsSlotsFree(slotNeeded, eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack))
				{
					client.Out.SendMessage("Not enough inventory space to buy that.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return 1;
				}

				// Second : check if enough money to buy that
				switch(currencyType)
				{
					case eCurrencyType.Money :
						{
							long moneyNeeded = itemTemplate.Value * item_count;

							// Check if player has enough money and subtract the money
							if (client.Player.RemoveMoney(moneyNeeded, null, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
							{
								if(item_count > 1)
								{
									client.Player.Out.SendMessage("You just bought " + item_count + " pieces of " + itemTemplate.Name + " for "+ Money.GetString(moneyNeeded) +".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									client.Player.Out.SendMessage("You just bought " + itemTemplate.Name + " for "+ Money.GetString(moneyNeeded) +".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								// don't return here, we have not add the bought item to the player inventory
							}
							else
							{
								client.Player.Out.SendMessage("You need " + Money.GetString(moneyNeeded) + " to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
						}
						break;
					case eCurrencyType.BountyPoint :
						{
							long bountyPointNeeded = itemTemplate.Value * item_count;

							// Check if player has enough money and subtract the money
							if (client.Player.RemoveBountyPoints(bountyPointNeeded))
							{
								if(item_count > 1)
								{
									client.Player.Out.SendMessage("You just bought " + item_count + " pieces of " + itemTemplate.Name + " for "+ bountyPointNeeded +" bounty points.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								else
								{
									client.Player.Out.SendMessage("You just bought " + itemTemplate.Name + " for "+ bountyPointNeeded +" bounty points.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								}
								// don't return here, we have not add the bought item to the player inventory
							}
							else
							{
								client.Player.Out.SendMessage("You need " + bountyPointNeeded + " bounty points to buy this.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
								return 0;
							}
						}
						break;

						//TODO : the others currency type ...
				}

				// now we add the bought object to the player inventory
				for(int i = 0 ; i < item_count ; i++)
				{
					client.Player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, itemTemplate.CreateInstance());
				}
			}
			return 1;
		}
	}
}