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
            get { return (int)(eInventorySlot.Consignment_First); }
        }

        /// <summary>
        /// Last slot in the DB.
        /// </summary>
        public int LastSlot
        {
            get { return (int)(eInventorySlot.Consignment_Last); }
        }

        #region Token return

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer player = source as GamePlayer;

            if (player == null)
                return false;

            if (!player.IsWithinRadius(this, 500))
            {
                ((GamePlayer)source).Out.SendMessage("You are to far away to give anything to " + this.Name + ".", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (item != null)
            {
                // ALBION
                if (item.Id_nb == "entrancehousingalb")
                {
                    player.MoveTo(2, 584832, 561279, 3576, 2144);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketcaerwent")
                {
                    player.MoveTo(2, 557035, 560048, 3624, 1641);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketrilan")
                {
                    player.MoveTo(2, 559906, 491141, 3392, 1829);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketbrisworthy")
                {
                    player.MoveTo(2, 489474, 489323, 3600, 3633);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketstoneleigh")
                {
                    player.MoveTo(2, 428964, 490962, 3624, 1806);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketchiltern")
                {
                    player.MoveTo(2, 428128, 557606, 3624, 3888);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketsherborne")
                {
                    player.MoveTo(2, 428840, 622221, 3248, 1813);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketaylesbury")
                {
                    player.MoveTo(2, 492794, 621373, 3624, 1643);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketoldsarum")
                {
                    player.MoveTo(2, 560030, 622022, 3624, 1819);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketdalton")
                {
                    player.MoveTo(2, 489334, 559242, 3720, 1821);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();
                    return true;
                }

                // MIDGARD
                else if (item.Id_nb == "entrancehousingmid")
                {
                    player.MoveTo(102, 526881, 561661, 3633, 80);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }

                else if (item.Id_nb == "marketerikstaad")
                {
                    player.MoveTo(102, 554099, 565239, 3624, 504);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }

                else if (item.Id_nb == "marketarothi")
                {
                    player.MoveTo(102, 558093, 485250, 3488, 1231);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketkaupang")
                {
                    TargetObject = this;
                    player.MoveTo(102, 625574, 483303, 3592, 2547);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketstavgaard")
                {
                    player.MoveTo(102, 686901, 490396, 3744, 332);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketcarlingford")
                {
                    player.MoveTo(102, 625056, 557887, 3696, 1366);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketholmestrand")
                {
                    player.MoveTo(102, 686903, 556050, 3712, 313);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketnittedal")
                {
                    player.MoveTo(102, 689199, 616329, 3488, 1252);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketfrisia")
                {
                    player.MoveTo(102, 622620, 615491, 3704, 804);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketwyndham")
                {
                    player.MoveTo(102, 555839, 621432, 3744, 314);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }

                // HIBERNIA
                else if (item.Id_nb == "entrancehousinghib")
                {
                    player.MoveTo(202, 555246, 526470, 3008, 1055);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketmeath")
                {
                    player.MoveTo(202, 564448, 559995, 3008, 1024);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }

                else if (item.Id_nb == "marketkilcullen")
                {
                    player.MoveTo(202, 618653, 561227, 3032, 3087);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketaberillan")
                {
                    player.MoveTo(202, 615145, 619457, 3008, 3064);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "markettorrylin")
                {
                    player.MoveTo(202, 566890, 620027, 3008, 1500);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "markettullamore")
                {
                    player.MoveTo(202, 560999, 692301, 3032, 1030);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketbroughshane")
                {
                    player.MoveTo(202, 618653, 692296, 3032, 3090);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketmoycullen")
                {
                    player.MoveTo(202, 495552, 686733, 2960, 1077);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketsaeranthal")
                {
                    player.MoveTo(202, 493148, 620361, 2952, 2471);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
                else if (item.Id_nb == "marketdunshire")
                {
                    player.MoveTo(202, 495494, 555646, 2960, 1057);
                    player.Inventory.RemoveItem(item);
                    player.SaveIntoDatabase();

                    return true;
                }
            }
            return base.ReceiveItem(source, item);
        }

        #endregion Token return

        /// <summary>
        /// Inventory of the Consignment Merchant
        /// </summary>
        public Dictionary<int, InventoryItem> ConInventory
        {
            get
            {
                var inventory = new Dictionary<int, InventoryItem>();
                int slotOffset = -FirstSlot + (int)(eInventorySlot.HousingInventory_First);
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
                return (InventoryItem[])(GameServer.Database.SelectObjects<InventoryItem>(sqlWhere));
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
                    merchant.Quantity += (int)m_totalMoney;
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

            if ((int)fromSlot >= (int)eInventorySlot.Consignment_First)
            {
                fromSlot = (eInventorySlot)(RecalculateSlot((int)fromSlot));
            }

            if (!inventory.ContainsKey((int)fromSlot))
                return null;

            IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);
            InventoryItem fromItem = inventory[(int)fromSlot];
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
            updateItems.Add((int)fromSlot, toItem);

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

            if ((int)fromSlot >= (int)eInventorySlot.Consignment_First)
                fromSlot = (eInventorySlot)(RecalculateSlot((int)fromSlot));

            if (!inventory.ContainsKey((int)fromSlot))
                return null;

            IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(2);
            InventoryItem fromItem = null, toItem = null;

            fromItem = inventory[(int)fromSlot];

            if ((int)toSlot >= (int)eInventorySlot.Consignment_First)
                toSlot = (eInventorySlot)(RecalculateSlot((int)toSlot));

            if (inventory.ContainsKey((int)toSlot))
            {
                toItem = inventory[(int)toSlot];
                toItem.SlotPosition = fromItem.SlotPosition;
                GameServer.Database.SaveObject(toItem);
            }

            int newPosi = (int)(toSlot) -
                          (int)(eInventorySlot.HousingInventory_First) +
                          FirstSlot;
            fromItem.SlotPosition = newPosi;
            GameServer.Database.SaveObject(fromItem);

            updateItems.Add((int)fromSlot, toItem);
            updateItems.Add((int)toSlot, fromItem);
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

            if (inventory.ContainsKey((int)toSlot))
            {
                InventoryItem toItem = inventory[(int)toSlot];
                GameServer.Database.DeleteObject(toItem);

                playerInventory.AddItem(fromSlot, toItem);
            }

            House house = HouseMgr.GetHouse(HouseNumber);

            fromItem.SlotPosition = (int)(toSlot);

            var price = player.TempProperties.getProperty<int>(PlayerSetMarketPriceHandler.NEW_PRICE);
            player.TempProperties.removeProperty(PlayerSetMarketPriceHandler.NEW_PRICE);

            if (fromItem.OwnerID != house.OwnerID)
            {
                fromItem.OwnerID = house.OwnerID;
            }

            fromItem.SellPrice = price;
            fromItem.OwnerLot = (ushort)HouseNumber; // used to mark the lot for market explorer
            GameServer.Database.AddObject(fromItem);

            if ((int)toSlot >= (int)eInventorySlot.Consignment_First)
            {
                toSlot = (eInventorySlot)(RecalculateSlot((int)toSlot));
            }

            updateItems.Add((int)toSlot, fromItem);

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

            if ((int)fromSlot >= (int)eInventorySlot.Consignment_First)
            {
                fromSlot = (eInventorySlot)(RecalculateSlot((int)fromSlot));
            }

            InventoryItem fromItem = inventory[(int)fromSlot];
            if (fromItem == null)
                return;

            int totalValue = fromItem.SellPrice;
            int orgValue = totalValue;
            if (byMarketExplorer)
            {
                int additionalfee = (totalValue * 20) / 100;
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
            if (slot >= (int)eInventorySlot.Consignment_First)
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


/*
 * Just have to run this query, adds all needed token in the databse, also add the merchantlist
 * Albion = PortMerchantHousingAlb
 * Midgard = PortMerchantHousingMid
 * Hibernia = PortMerchantHousingHib
 * 
INSERT INTO `itemtemplate` (`ItemTemplate_ID`, `Id_nb`, `Name`, `Level`, `Durability`, `Condition`, `MaxDurability`, `MaxCondition`, `Quality`, `DPS_AF`, `SPD_ABS`, `Hand`, `Type_Damage`, `Object_Type`, `Item_Type`, `Color`, `Emblem`, `Effect`, `Weight`, `Model`, `Extension`, `Bonus`, `Bonus1`, `Bonus2`, `Bonus3`, `Bonus4`, `Bonus5`, `Bonus6`, `Bonus7`, `Bonus8`, `Bonus9`, `Bonus10`, `ExtraBonus`, `Bonus1Type`, `Bonus2Type`, `Bonus3Type`, `Bonus4Type`, `Bonus5Type`, `Bonus6Type`, `Bonus7Type`, `Bonus8Type`, `Bonus9Type`, `Bonus10Type`, `ExtraBonusType`, `IsPickable`, `IsDropable`, `CanDropAsLoot`, `IsTradable`, `Price`, `MaxCount`, `IsIndestructible`, `IsNotLosingDur`, `PackSize`, `Charges`, `MaxCharges`, `Charges1`, `MaxCharges1`, `SpellID`, `SpellID1`, `ProcSpellID`, `ProcSpellID1`, `PoisonSpellID`, `PoisonMaxCharges`, `PoisonCharges`, `Realm`, `AllowedClasses`, `CanUseEvery`, `Flags`, `BonusLevel`, `LevelRequirement`, `PackageID`, `Description`, `ClassType`, `ProcChance`) VALUES
('entrancehousingalb', 'entrancehousingalb', 'Return token to Caerwent entrance', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('entrancehousinghib', 'entrancehousinghib', 'Return token to Meath entrance', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('entrancehousingmid', 'entrancehousingmid', 'Return token to Housing entrance', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketaberillan', 'marketaberillan', 'Token return to Market of Aberillan', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketarothi', 'marketarothi', 'Jeton de retour Marché d''Arothi', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketaylesbury', 'marketaylesbury', 'Token return to Market of Aylesbury', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketbrisworthy', 'marketbrisworthy', 'Token return to Market of Brisworthy', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketbroughshane', 'marketbroughshane', 'Token return to Market of Broughshane', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketcaerwent', 'marketcaerwent', 'Token return to Market of Caerwent', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketcarlingford', 'marketcarlingford', 'Token return to Market of Carlingford', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketchiltern', 'marketchiltern', 'Token return to Market of Chiltern', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketdalton', 'marketdalton', 'Token return to Market of Dalton', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketdunshire', 'marketdunshire', 'Token return to Market of Dunshire', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketerikstaad', 'marketerikstaad', 'Jeton de retour Marché d''Erikstaad', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketfrisia', 'marketfrisia', 'Token return to Market of Frisia', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketholmestrand', 'marketholmestrand', 'Token return to Market of Holmestrand', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketkaupang', 'marketkaupang', 'Token return to Market of Kaupang', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketkilcullen', 'marketkilcullen', 'Token return to Market of Kilcullen', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketkillcullen', 'marketkillcullen', 'Token return to Market of Kilcullen', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketmeath', 'marketmeath', 'Token return to Market of Meath', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketmoycullen', 'marketmoycullen', 'Token return to Market of Moycullen', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketnittedal', 'marketnittedal', 'Token return to Market of Nittedal', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketoldsarum', 'marketoldsarum', 'Token return to Market of Old Sarum', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketrilan', 'marketrilan', 'Token return to Market of Rilan', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketsaeranthal', 'marketsaeranthal', 'Token return to Market of Saeranthal', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketsherborne', 'marketsherborne', 'Token return to Market of Sherborne', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('marketstavgaard', 'marketstavgaard', 'Token return to Market of Stavgaard', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0),
('marketstoneleigh', 'marketstoneleigh', 'Token return to Market of Stoneleigh', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, '0', 0, 0, 0, 0, '', '', '', 0),
('markettorrylin', 'markettorrylin', 'Token return to Market of Torrylin', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('markettullamore', 'markettullamore', 'Token return to Market of Tullamore', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3, '0', 0, 0, 0, 0, '', '', '', 0),
('marketwyndham', 'marketwyndham', 'Token return to Market of Wyndham', 0, 50000, 50000, 50000, 50000, 85, 0, 0, 0, 0, 0, 35, 0, 0, 0, 10, 485, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 500, 1, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, '0', 0, 0, 0, 0, '', '', '', 0);
    
INSERT INTO `merchantitem` (`MerchantItem_ID`, `ItemListID`, `ItemTemplateID`, `PageNumber`, `SlotPosition`, `PackageID`) VALUES
('204280b2-b6fb-4041-bf11-3c9b434c2e4c', 'PortMerchantHousingHib', 'marketdunshire', 0, 9, NULL),
('20f4f965-5848-46aa-a211-56a3d25d5efb', 'PortMerchantHousingAlb', 'marketchiltern', 0, 5, NULL),
('28eb35cf-1a0e-4dee-a1cd-f2cd72b1c0b1', 'PortMerchantHousingAlb', 'marketrilan', 0, 2, NULL),
('4084de34-896a-4ea8-96da-6f40dd8d88a2', 'PortMerchantHousingMid', 'marketerikstaad', 0, 1, NULL),
('44c63a42-f043-4fc4-8c39-1d1a28da27c9', 'PortMerchantHousingHib', 'marketmeath', 0, 1, NULL),
('4743ae0a-23ec-4cf2-8efe-6d34f722eb22', 'PortMerchantHousingHib', 'marketsaeranthal', 0, 8, NULL),
('5d475184-f7d3-41f1-abb5-d6f5340ebd19', 'PortMerchantHousingAlb', 'marketaylesbury', 0, 7, NULL),
('5de46ee8-4107-4406-9132-da8441d185fd', 'PortMerchantHousingMid', 'marketstavgaard', 0, 4, NULL),
('624b989d-ba5e-4989-88c9-3f72bd431c08', 'PortMerchantHousingMid', 'marketwyndham', 0, 9, NULL),
('657a966b-c558-4326-b1b7-9b381daf043e', 'PortMerchantHousingMid', 'entrancehousingmid', 0, 0, NULL),
('6ec755ad-eb04-4290-b698-e357d1aa4261', 'PortMerchantHousingAlb', 'marketsherborne', 0, 6, NULL),
('7558d4dc-27d6-422a-a591-d0d43666d93a', 'PortMerchantHousingMid', 'marketkaupang', 0, 3, NULL),
('75892dfe-cba0-4604-a21d-4fa4186df99f', 'PortMerchantHousingHib', 'entrancehousinghib', 0, 0, NULL),
('761d6772-07d9-4783-aff5-f1aa0c1c9335', 'PortMerchantHousingMid', 'marketarothi', 0, 2, NULL),
('773d466c-9de7-403f-a383-3069fdaac13a', 'PortMerchantHousingMid', 'marketfrisia', 0, 8, NULL),
('79d4ab1d-1e69-49ce-884c-c62f3a0a2e09', 'PortMerchantHousingHib', 'marketmoycullen', 0, 7, NULL),
('7e93386f-117e-4653-a414-135b78943f7c', 'PortMerchantHousingMid', 'marketcarlingford', 0, 5, NULL),
('83d90272-82d4-44a6-94af-1a83a1ba6f73', 'PortMerchantHousingAlb', 'marketstoneleigh', 0, 4, NULL),
('90e48c1a-23c9-4059-a139-fdf42b80106a', 'PortMerchantHousingMid', 'marketnittedal', 0, 7, NULL),
('a17688fc-038a-4d46-9f10-5633fff35304', 'PortMerchantHousingHib', 'markettorrylin', 0, 4, NULL),
('a6b83ba8-73a6-4caa-be0d-11621a4bcf6e', 'PortMerchantHousingHib', 'marketkilcullen', 0, 2, NULL),
('ac9179a4-22c2-4a14-bcfd-e7195943ff1d', 'PortMerchantHousingHib', 'marketbroughshane', 0, 6, NULL),
('bb3b60bb-c1fd-4506-b7c8-490ff13cf51d', 'PortMerchantHousingMid', 'marketholmestrand', 0, 6, NULL),
('cd66879c-ba5e-48b9-900c-fd3bce5c99bc', 'PortMerchantHousingAlb', 'marketoldsarum', 0, 8, NULL),
('d1f058f9-82b2-49c2-98cf-470965c41590', 'PortMerchantHousingAlb', 'entrancehousingalb', 0, 0, NULL),
('d3549e07-3e39-4f06-b7b7-6b789c7ba00a', 'PortMerchantHousingHib', 'marketaberillan', 0, 3, NULL),
('d999f092-ca79-4d89-9574-c2d8624bc99c', 'PortMerchantHousingAlb', 'marketbrisworthy', 0, 3, NULL),
('dac83cf5-903a-48be-baab-e7272923de4a', 'PortMerchantHousingAlb', 'marketdalton', 0, 9, NULL),
('ed615aa0-9246-4df2-a7b8-11f879bf0f46', 'PortMerchantHousingHib', 'markettullamore', 0, 5, NULL),
('efda9497-77a1-4f5d-ac23-ab28c7aa51ae', 'PortMerchantHousingAlb', 'marketcaerwent', 0, 1, NULL);
*/