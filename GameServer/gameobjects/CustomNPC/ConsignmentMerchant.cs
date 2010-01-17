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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DOL.AI.Brain;
using DOL.Database;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS
{
    public static class ConsignmentMoney
    {
        public static bool UseBP = false; //Set this to true to make the Consignment Merchant use Bountypoints instead of Money
    }

    public class Consignment : GameNPC
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private Dictionary<String, GamePlayer> m_observers = new Dictionary<String, GamePlayer>();
        public const int ConsignmentSize = 100;
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

        /// <summary>
        /// Inventory of the Consignment Merchant
        /// </summary>
        public Dictionary<int, InventoryItem> ConInventory
        {
            get
            {
                Dictionary<int, InventoryItem> inventory = new Dictionary<int, InventoryItem>();
                int slotOffset = -FirstSlot + (int)(eInventorySlot.HousingInventory_First);
                foreach (InventoryItem item in Items)
                    if (item != null)
                    {
                        if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
                            inventory.Add(item.SlotPosition + slotOffset, item);
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
                House house = HouseMgr.GetHouse(this.CurrentRegionID, HouseNumber);
                String sqlWhere = String.Format("OwnerID = '{0}' and SlotPosition >= {1} and SlotPosition <= {2}",
                    HouseMgr.GetOwner(house.DatabaseItem), FirstSlot, LastSlot);
                return (InventoryItem[])(GameServer.Database.SelectObjects(typeof(InventoryItem), sqlWhere));
            }
        }

        /// <summary>
        /// Checks if the Player is allowed to move an Item
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public bool CanMove(GamePlayer player)
        {
            House house = HouseMgr.GetHouse(this.CurrentRegionID, HouseNumber);
            if (house == null)
                return false;
            if (house.HasOwnerPermissions(player))
                return true;
            return false;
        }

        private object m_vaultSync = new object();

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
            House house = HouseMgr.GetHouse(this.HouseNumber);
            if (house == null)
                return;

            lock (m_vaultSync)
            {
                if (fromSlot == toSlot) NotifyObservers(null);
                else if (fromSlot >= eInventorySlot.Consignment_First && fromSlot <= eInventorySlot.Consignment_Last)
                {
                    if (toSlot >= eInventorySlot.Consignment_First && toSlot <= eInventorySlot.Consignment_Last)
                        NotifyObservers(MoveItemInsideMerchant(fromSlot, toSlot));
                    else if (!house.HasOwnerPermissions(player))
                        OnPlayerBuy(player, playerInventory, fromSlot, toSlot, false);
                    else
                        NotifyObservers(MoveItemFromMerchant(player, playerInventory, fromSlot, toSlot));
                }
                else if (toSlot >= eInventorySlot.Consignment_First &&
                    toSlot <= eInventorySlot.Consignment_Last)
                    NotifyObservers(MoveItemToMerchant(player, playerInventory, fromSlot, toSlot));
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
                fromSlot = (eInventorySlot)(RecalculateSlot((int)fromSlot));

            if (!inventory.ContainsKey((int)fromSlot))
                return null;
            IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);
            InventoryItem fromItem = inventory[(int)fromSlot];
            InventoryItem toItem = playerInventory.GetItem(toSlot);
            if (toItem != null)
            {
                playerInventory.RemoveItem(toItem);
                toItem.SlotPosition = fromItem.SlotPosition;
                GameServer.Database.AddNewObject(toItem);
            }

            GameServer.Database.DeleteObject(fromItem);
            if (fromItem.OwnerID != player.PlayerCharacter.ObjectId)
                fromItem.OwnerID = player.PlayerCharacter.ObjectId;
            if (fromItem.SellPrice != 0)
                fromItem.SellPrice = 0;
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
        protected IDictionary<int, InventoryItem> MoveItemInsideMerchant(eInventorySlot fromSlot,
            eInventorySlot toSlot)
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
            House house = HouseMgr.GetHouse(this.HouseNumber);
            fromItem.SlotPosition = (int)(toSlot);
            int price = player.TempProperties.getProperty<int>(DOL.GS.PacketHandler.Client.v168.PlayerSetMarketPriceHandler.NEW_PRICE);
            player.TempProperties.removeProperty(DOL.GS.PacketHandler.Client.v168.PlayerSetMarketPriceHandler.NEW_PRICE);
            if (fromItem.OwnerID != house.OwnerIDs)
                fromItem.OwnerID = house.OwnerIDs;
            fromItem.SellPrice = price;
            fromItem.OwnerLot = (ushort)this.HouseNumber; // used to mark the lot for market explorer
            GameServer.Database.AddNewObject(fromItem);

            if ((int)toSlot >= (int)eInventorySlot.Consignment_First)
                toSlot = (eInventorySlot)(RecalculateSlot((int)toSlot));
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
        public void OnPlayerBuy(GamePlayer player, IGameInventory playerInventory,
            eInventorySlot fromSlot, eInventorySlot toSlot, bool byMarketExplorer)
        {
            IDictionary<int, InventoryItem> inventory = ConInventory;

            if ((int)fromSlot >= (int)eInventorySlot.Consignment_First)
                fromSlot = (eInventorySlot)(RecalculateSlot((int)fromSlot));

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
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameMerchant.OnPlayerBuy.YouNeedBP", totalValue), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                }
                else
                {
                    if (player.GetCurrentMoney() < totalValue)
                    {
                        player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameMerchant.OnPlayerBuy.YouNeed", Money.GetString(totalValue)), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                        return;
                    }
                }
                if (player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack) == eInventorySlot.Invalid)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "GameMerchant.OnPlayerBuy.NotInventorySpace"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    return;
                }
                //Generate the buy message
                string message;
                if (ConsignmentMoney.UseBP) // we buy with Bountypoints
                {
                    message = LanguageMgr.GetTranslation(player.Client, "GameMerchant.OnPlayerBuy.BoughtBP", ((ItemTemplate)fromItem).GetName(1, false), totalValue);
                    player.Out.SendMessage(message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                    player.BountyPoints -= totalValue;
                    player.Out.SendUpdatePoints();
                }
                else //we buy with money
                {
                    message = LanguageMgr.GetTranslation(player.Client, "GameMerchant.OnPlayerBuy.Bought", ((ItemTemplate)fromItem).GetName(1, false), Money.GetString(totalValue));
                    if (!player.RemoveMoney(totalValue, message, eChatType.CT_Merchant, eChatLoc.CL_SystemWindow))
                    {
                        throw new Exception("Money amount changed while adding items.");
                    }
                }
                DBHouseMerchant merchant = (DBHouseMerchant)GameServer.Database.SelectObject(typeof(DBHouseMerchant), "HouseNumber = '" + this.HouseNumber + "'");
                merchant.Quantity += orgValue;
                GameServer.Database.SaveObject(merchant);
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
            IList<String> inactiveList = new List<String>();
            foreach (GamePlayer observer in m_observers.Values)
            {
                if (observer.ActiveConMerchant != this)
                {
                    inactiveList.Add(observer.Name);
                    continue;
                }

                if (observer.IsWithinRadius(this, WorldMgr.INTERACT_DISTANCE ))
                {
                    if (observer.Client.Account.PrivLevel > 1)
                        observer.Out.SendMessage("You are to far away from the Consignment Merchant and will no longer receive updates.", eChatType.CT_Skill, eChatLoc.CL_SystemWindow);

                    observer.ActiveConMerchant = null;
                    inactiveList.Add(observer.Name);
                    continue;
                }
                observer.Client.Out.SendInventoryItemsUpdate(updateItems, 0);
            }

            // Now remove all inactive observers.

            foreach (String observerName in inactiveList)
                m_observers.Remove(observerName);
        }

        /// <summary>
        /// Recalculation is required for different merchant window/db entry indexing
        /// </summary>
        /// <param name="slot"></param>
        /// <returns></returns>
        protected int RecalculateSlot(int slot)
        {
            if (slot >= (int)eInventorySlot.Consignment_First)
                slot -= 1350;
            else if (slot >= 150 && slot <= 249)
                slot += 1350;
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
                player.ActiveVault = null;

            if (!m_observers.ContainsKey(player.Name))
                m_observers.Add(player.Name, player);
            player.ActiveConMerchant = this;
            House h = HouseMgr.GetHouse(this.CurrentRegionID, this.HouseNumber);
            if (h == null)
                return false;
            if (h.HasOwnerPermissions(player))
            {
                DBHouseMerchant merchant = (DBHouseMerchant)GameServer.Database.SelectObject(typeof(DBHouseMerchant), "HouseNumber = '" + HouseNumber + "'");
                player.Out.SendInventoryItemsUpdate(ConInventory, 0x05);
                long amount = (long)merchant.Quantity;
                player.Out.SendConsignmentMerchantMoney((ushort)Money.GetMithril(amount), (ushort)Money.GetPlatinum(amount), (ushort)Money.GetGold(amount), (byte)Money.GetSilver(amount), (byte)Money.GetCopper(amount));
                if (ConsignmentMoney.UseBP)
                {
                    player.Out.SendMessage("Your Merchant currently holds " + merchant.Quantity.ToString() + " BountyPoints.", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                }
            }
            else
                player.Out.SendInventoryItemsUpdate(ConInventory, 0x06);
            return true;
        }

        public override bool AddToWorld()
        {
            GameNpcInventoryTemplate template = new GameNpcInventoryTemplate();
            switch (Realm)
            {
                case eRealm.Albion:
                    {
                        Model = 92;
                        template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 310, 81);
                        template.AddNPCEquipment(eInventorySlot.FeetArmor, 1301);
                        template.AddNPCEquipment(eInventorySlot.LegsArmor, 1312);
                        if (Util.Chance(50))
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1005, 67);
                        else
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1313);
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
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1300);
                        else
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 993);
                        template.AddNPCEquipment(eInventorySlot.Cloak, 669, 51);
                    }
                    break;
                case eRealm.Hibernia:
                    {
                        Model = 335;
                        template.AddNPCEquipment(eInventorySlot.RightHandWeapon, 457, 81);
                        template.AddNPCEquipment(eInventorySlot.FeetArmor, 1333);
                        if (Util.Chance(50))
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1336);
                        else
                            template.AddNPCEquipment(eInventorySlot.TorsoArmor, 1008);
                        template.AddNPCEquipment(eInventorySlot.Cloak, 669);
                    }
                    break;
            }
            Inventory = template.CloseTemplate();
            base.AddToWorld();
            House house = HouseMgr.GetHouse(HouseNumber);
            house.ConsignmentMerchant = this;
            SetEmblem();
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
                DBGuild guild = (DBGuild)GameServer.Database.SelectObject(typeof(DBGuild), "GuildName = '" + house.DatabaseItem.GuildName + "'");
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
