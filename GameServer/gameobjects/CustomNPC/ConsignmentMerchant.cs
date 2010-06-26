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
using System.Collections.Generic;
using System.Reflection;
using DOL.Database;
using DOL.GS.Housing;
using DOL.GS.PacketHandler;
using DOL.GS.PacketHandler.Client.v168;
using log4net;

namespace DOL.GS
{
	public static class ConsignmentMoney
	{
		public static bool UseBP;
		//Set this to true to make the Consignment Merchant use Bountypoints instead of Money
	}

	public class GameConsignmentMerchant : GameNPC
	{
		public const int ConsignmentSize = 100;
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private readonly Dictionary<string, GamePlayer> _observers = new Dictionary<string, GamePlayer>();
		private readonly object m_moneyLock = new object();
		private readonly object m_vaultSync = new object();
		private long m_totalMoney;

		/// <summary>
		/// First slot in the DB.
		/// </summary>
		public int FirstSlot
		{
			get { return (int) (eInventorySlot.Consignment_First); }
		}

		/// <summary>
		/// Last slot in the DB.
		/// </summary>
		public int LastSlot
		{
			get { return (int) (eInventorySlot.Consignment_Last); }
		}

		/// <summary>
		/// Inventory of the Consignment Merchant
		/// </summary>
		public Dictionary<int, InventoryItem> ConInventory
		{
			get
			{
				var inventory = new Dictionary<int, InventoryItem>();
				int slotOffset = -FirstSlot + (int) (eInventorySlot.HousingInventory_First);
				foreach (InventoryItem item in Items)
				{
					if (item != null)
					{
						if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
						{
							inventory.Add(item.SlotPosition + slotOffset, item);
						}
					}
				}

				return inventory;
			}
		}

		/// <summary>
		/// List of items in the Consignment Merchants Inventory
		/// </summary>
		public InventoryItem[] Items
		{
			get
			{
				House house = HouseMgr.GetHouse(CurrentRegionID, HouseNumber);
				String sqlWhere = String.Format("OwnerID = '{0}' and SlotPosition >= {1} and SlotPosition <= {2}",
				                                house.DatabaseItem.OwnerID, FirstSlot, LastSlot);
				return (InventoryItem[]) (GameServer.Database.SelectObjects<InventoryItem>(sqlWhere));
			}
		}

		/// <summary>
		///  Gets or sets the total amount of money held by this consignment merchant.
		/// </summary>
		public long TotalMoney
		{
			get
			{
				lock (m_moneyLock)
				{
					return m_totalMoney;
				}
			}
			set
			{
				lock (m_moneyLock)
				{
					m_totalMoney = value;

					// update DB entry
					var merchant = GameServer.Database.SelectObject<DBHouseMerchant>("HouseNumber = '" + HouseNumber + "'");
					merchant.Quantity += (int) m_totalMoney;
					GameServer.Database.SaveObject(merchant);
				}
			}
		}

		/// <summary>
		/// Checks if the Player is allowed to move an Item
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanMove(GamePlayer player)
		{
			House house = HouseMgr.GetHouse(CurrentRegionID, HouseNumber);
			if (house == null)
				return false;
			if (house.HasOwnerPermissions(player))
				return true;
			return false;
		}

		/// <summary>
		/// Move an item from, to or inside a consignment merchants inventory.
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns> 
		public void MoveItem(GamePlayer player, IGameInventory playerInventory, eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			if (fromSlot == toSlot)
				return;

			House house = HouseMgr.GetHouse(HouseNumber);
			if (house == null)
				return;

			lock (m_vaultSync)
			{
				if (fromSlot == toSlot)
				{
					NotifyObservers(null);
				}
				else if (fromSlot >= eInventorySlot.Consignment_First && fromSlot <= eInventorySlot.Consignment_Last)
				{
					if (toSlot >= eInventorySlot.Consignment_First && toSlot <= eInventorySlot.Consignment_Last)
					{
						NotifyObservers(MoveItemInsideMerchant(fromSlot, toSlot));
					}
					else if (!house.HasOwnerPermissions(player))
					{
						OnPlayerBuy(player, playerInventory, fromSlot, toSlot, false);
					}
					else
					{
						NotifyObservers(MoveItemFromMerchant(player, playerInventory, fromSlot, toSlot));
					}
				}
				else if (toSlot >= eInventorySlot.Consignment_First && toSlot <= eInventorySlot.Consignment_Last)
				{
					NotifyObservers(MoveItemToMerchant(player, playerInventory, fromSlot, toSlot));
				}
			}
		}

		/// <summary>
		/// Move an Item from the merchant 
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemFromMerchant(GamePlayer player, IGameInventory playerInventory,
		                                                               eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving to the backpack.            
			if (toSlot < eInventorySlot.FirstBackpack || toSlot > eInventorySlot.LastBackpack)
				return null;

			IDictionary<int, InventoryItem> inventory = ConInventory;

			if ((int) fromSlot >= (int) eInventorySlot.Consignment_First)
			{
				fromSlot = (eInventorySlot) (RecalculateSlot((int) fromSlot));
			}

			if (!inventory.ContainsKey((int) fromSlot))
				return null;

			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);
			InventoryItem fromItem = inventory[(int) fromSlot];
			InventoryItem toItem = playerInventory.GetItem(toSlot);
			if (toItem != null)
			{
				playerInventory.RemoveItem(toItem);
				toItem.SlotPosition = fromItem.SlotPosition;
				GameServer.Database.AddObject(toItem);
			}

			GameServer.Database.DeleteObject(fromItem);

			if (fromItem.OwnerID != player.DBCharacter.ObjectId)
			{
				fromItem.OwnerID = player.DBCharacter.ObjectId;
			}

			if (fromItem.SellPrice != 0)
			{
				fromItem.SellPrice = 0;
			}

			fromItem.OwnerLot = 0;
			playerInventory.AddItem(toSlot, fromItem);
			updateItems.Add((int) fromSlot, toItem);

			return updateItems;
		}

		/// <summary>
		/// Move an item around inside the Merchant.
		/// </summary>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemInsideMerchant(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			IDictionary<int, InventoryItem> inventory = ConInventory;

			if ((int) fromSlot >= (int) eInventorySlot.Consignment_First)
				fromSlot = (eInventorySlot) (RecalculateSlot((int) fromSlot));

			if (!inventory.ContainsKey((int) fromSlot))
				return null;

			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(2);
			InventoryItem fromItem = null, toItem = null;

			fromItem = inventory[(int) fromSlot];

			if ((int) toSlot >= (int) eInventorySlot.Consignment_First)
				toSlot = (eInventorySlot) (RecalculateSlot((int) toSlot));

			if (inventory.ContainsKey((int) toSlot))
			{
				toItem = inventory[(int) toSlot];
				toItem.SlotPosition = fromItem.SlotPosition;
				GameServer.Database.SaveObject(toItem);
			}

			int newPosi = (int) (toSlot) -
			              (int) (eInventorySlot.HousingInventory_First) +
			              FirstSlot;
			fromItem.SlotPosition = newPosi;
			GameServer.Database.SaveObject(fromItem);

			updateItems.Add((int) fromSlot, toItem);
			updateItems.Add((int) toSlot, fromItem);
			return updateItems;
		}

		/// <summary>
		/// Move an item to the merchant
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemToMerchant(GamePlayer player, IGameInventory playerInventory,
		                                                             eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving from the backpack.            
			if (fromSlot < eInventorySlot.FirstBackpack || fromSlot > eInventorySlot.LastBackpack)
				return null;

			InventoryItem fromItem = playerInventory.GetItem(fromSlot);

			if (fromItem == null)
				return null;

			if (fromItem is InventoryArtifact)
				return null;

			IDictionary<int, InventoryItem> inventory = ConInventory;
			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);

			playerInventory.RemoveItem(fromItem);

			if (inventory.ContainsKey((int) toSlot))
			{
				InventoryItem toItem = inventory[(int) toSlot];
				GameServer.Database.DeleteObject(toItem);

				playerInventory.AddItem(fromSlot, toItem);
			}

			House house = HouseMgr.GetHouse(HouseNumber);

			fromItem.SlotPosition = (int) (toSlot);

			var price = player.TempProperties.getProperty<int>(PlayerSetMarketPriceHandler.NEW_PRICE);
			player.TempProperties.removeProperty(PlayerSetMarketPriceHandler.NEW_PRICE);

			if (fromItem.OwnerID != house.OwnerID)
			{
				fromItem.OwnerID = house.OwnerID;
			}

			fromItem.SellPrice = price;
			fromItem.OwnerLot = (ushort) HouseNumber; // used to mark the lot for market explorer
			GameServer.Database.AddObject(fromItem);

			if ((int) toSlot >= (int) eInventorySlot.Consignment_First)
			{
				toSlot = (eInventorySlot) (RecalculateSlot((int) toSlot));
			}

			updateItems.Add((int) toSlot, fromItem);

			return updateItems;
		}

		/// <summary>
		/// The Player is buying an Item from the merchant
		/// </summary>
		/// <param name="player"></param>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		public void OnPlayerBuy(GamePlayer player, IGameInventory playerInventory, eInventorySlot fromSlot,
		                        eInventorySlot toSlot, bool byMarketExplorer)
		{
			IDictionary<int, InventoryItem> inventory = ConInventory;

			if ((int) fromSlot >= (int) eInventorySlot.Consignment_First)
			{
				fromSlot = (eInventorySlot) (RecalculateSlot((int) fromSlot));
			}

			InventoryItem fromItem = inventory[(int) fromSlot];
			if (fromItem == null)
				return;

			int totalValue = fromItem.SellPrice;
			int orgValue = totalValue;
			if (byMarketExplorer)
			{
				int additionalfee = (totalValue*20)/100;
				totalValue += additionalfee;
			}

			lock (player.Inventory)
			{
				if (totalValue == 0)
				{
					player.Out.SendMessage("This Item can not be bought.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return;
				}

				if (ConsignmentMoney.UseBP)
				{
					if (player.BountyPoints < totalValue)
					{
						ChatUtil.SendSystemMessage(player, "GameMerchant.OnPlayerBuy.YouNeedBP", totalValue);
						return;
					}
				}
				else
				{
					if (player.GetCurrentMoney() < totalValue)
					{
						ChatUtil.SendSystemMessage(player, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue));
						return;
					}
				}

				if (player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) ==
				    eInventorySlot.Invalid)
				{
					ChatUtil.SendSystemMessage(player, "GameMerchant.OnPlayerBuy.NotInventorySpace", null);
					return;
				}

				if (ConsignmentMoney.UseBP) // we buy with Bountypoints
				{
					ChatUtil.SendMerchantMessage(player, "GameMerchant.OnPlayerBuy.BoughtBP", fromItem.GetName(1, false), totalValue);

					player.BountyPoints -= totalValue;
					player.Out.SendUpdatePoints();
				}
				else //we buy with money
				{
					if (player.RemoveMoney(totalValue))
					{
						ChatUtil.SendMerchantMessage(player, "GameMerchant.OnPlayerBuy.Bought", fromItem.GetName(1, false),
						                             Money.GetString(totalValue));
					}
				}

				TotalMoney += orgValue;

				NotifyObservers(MoveItemFromMerchant(player, playerInventory, fromSlot, toSlot));
			}
		}


		/// <summary>
		/// Send inventory updates to all players actively viewing this merchant;
		/// players that are too far away will be considered inactive.
		/// </summary>
		/// <param name="updateItems"></param>
		protected void NotifyObservers(IDictionary<int, InventoryItem> updateItems)
		{
			var inactiveList = new List<string>();
			foreach (GamePlayer observer in _observers.Values)
			{
				if (observer.ActiveConMerchant != this)
				{
					inactiveList.Add(observer.Name);
					continue;
				}

				if (observer.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE))
				{
					observer.ActiveConMerchant = null;
					inactiveList.Add(observer.Name);
					continue;
				}

				observer.Client.Out.SendInventoryItemsUpdate(updateItems, 0);
			}

			// Now remove all inactive observers.
			foreach (string observerName in inactiveList)
			{
				_observers.Remove(observerName);
			}
		}

		/// <summary>
		/// Recalculation is required for different merchant window/db entry indexing
		/// </summary>
		/// <param name="slot"></param>
		/// <returns></returns>
		protected static int RecalculateSlot(int slot)
		{
			if (slot >= (int) eInventorySlot.Consignment_First)
			{
				slot -= 1350;
			}
			else if (slot >= 150 && slot <= 249)
			{
				slot += 1350;
			}

			return slot;
		}

		/// <summary>
		/// Player interacting with this Merchant.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (player.ActiveVault != null)
			{
				player.ActiveVault = null;
			}

			if (!_observers.ContainsKey(player.Name))
			{
				_observers.Add(player.Name, player);
			}

			player.ActiveConMerchant = this;

			House h = HouseMgr.GetHouse(CurrentRegionID, HouseNumber);
			if (h == null)
				return false;

			if (h.CanUseConsignmentMerchant(player, ConsignmentPermissions.Any))
			{
				player.Out.SendInventoryItemsUpdate(ConInventory, 0x05);

				long amount = m_totalMoney;
				player.Out.SendConsignmentMerchantMoney(amount);

				if (ConsignmentMoney.UseBP)
				{
					player.Out.SendMessage("Your Merchant currently holds " + amount + " BountyPoints.",
					                       eChatType.CT_Important, eChatLoc.CL_ChatWindow);
				}
			}
			else
			{
				player.Out.SendInventoryItemsUpdate(ConInventory, 0x06);
			}

			return true;
		}

		public override bool AddToWorld()
		{
			var template = new GameNpcInventoryTemplate();
			switch (Realm)
			{
				case eRealm.Albion:
					{
						Model = 92;
						template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 310, 81);
						template.AddNPCEquipment(eInventorySlot.FeetArmor, 1301);
						template.AddNPCEquipment(eInventorySlot.LegsArmor, 1312);

						if (Util.Chance(50))
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1005, 67);
						}
						else
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1313);
						}

						template.AddNPCEquipment(eInventorySlot.Cloak, 669, 65);
					}
					break;
				case eRealm.Midgard:
					{
						Model = 156;
						template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 321, 81);
						template.AddNPCEquipment(eInventorySlot.FeetArmor, 1301);
						template.AddNPCEquipment(eInventorySlot.LegsArmor, 1303);

						if (Util.Chance(50))
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1300);
						}
						else
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 993);
						}

						template.AddNPCEquipment(eInventorySlot.Cloak, 669, 51);
					}
					break;
				case eRealm.Hibernia:
					{
						Model = 335;
						template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 457, 81);
						template.AddNPCEquipment(eInventorySlot.FeetArmor, 1333);

						if (Util.Chance(50))
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1336);
						}
						else
						{
							template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1008);
						}

						template.AddNPCEquipment(eInventorySlot.Cloak, 669);
					}
					break;
			}

			Inventory = template.CloseTemplate();
			base.AddToWorld();

			House house = HouseMgr.GetHouse(HouseNumber);
			house.ConsignmentMerchant = this;
			SetEmblem();

			// verify if OwnerLot is correct 
            var itemcon = GameServer.Database.SelectObjects<InventoryItem>("OwnerID = '" + house.OwnerID + "' AND SlotPosition >= 1500 AND SlotPosition <= 1599");
			if (itemcon.Count > 0)
			{
                for (int i = 0; i < itemcon.Count; i++)
                {
                    itemcon[i].OwnerLot = ((ushort)HouseNumber);
					GameServer.Database.SaveObject(itemcon[i]);
                }
			}

			return true;
		}

		/// <summary>
		/// Not Livelike but looks better - adds the owners guild emblem to the consignment merchants cloak
		/// </summary>
		public void SetEmblem()
		{
			if (Inventory == null)
				return;

			House house = HouseMgr.GetHouse(HouseNumber);
			if (house == null)
				return;

			if (house.DatabaseItem.GuildHouse)
			{
				var guild = GameServer.Database.SelectObject<DBGuild>("GuildName = '" + house.DatabaseItem.GuildName + "'");
				int emblem = guild.Emblem;

				InventoryItem cloak = Inventory.GetItem(eInventorySlot.Cloak);
				if (cloak != null)
				{
					cloak.Emblem = emblem;
					UpdateNPCEquipmentAppearance();
				}
			}
		}
	}
}