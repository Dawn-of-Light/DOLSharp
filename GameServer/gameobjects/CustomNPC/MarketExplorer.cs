using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using DOL;
using DOL.GS;
using DOL.GS.GameEvents;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.Events;
using DOL.GS.Housing;


namespace DOL.GS
{
    public class MarketExplorer : GameNPC
    {

        private const string EXPLORER_ITEM_WEAK = "MarketExplorerItem";

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;
            player.ActiveConMerchant = null;
            player.ActiveVault = null;

            player.Out.SendMarketExplorerWindow();
            return true;
        }

        public void BuyItem(InventoryItem item, GamePlayer player)
        {
            player.TempProperties.setProperty(EXPLORER_ITEM_WEAK, new WeakRef(item));
            player.Out.SendCustomDialog("Buying directly from the Market Explorer costs an additional 20% fee.\nDo you want to buy the Item?", new CustomDialogResponse(MarketExplorerBuyDialogue));
        }

        private void MarketExplorerBuyDialogue(GamePlayer player, byte response)
        {
            if (response != 0x01)
            {
                player.TempProperties.removeProperty(EXPLORER_ITEM_WEAK);
                return;
            }
            WeakReference itemWeak = (WeakReference)player.TempProperties.getProperty<object>(EXPLORER_ITEM_WEAK, new WeakRef(null));
            InventoryItem item = (InventoryItem)itemWeak.Target;
            player.TempProperties.removeProperty(EXPLORER_ITEM_WEAK);

            GameConsignmentMerchant consign = HouseMgr.GetConsignmentByHouseNumber((int)item.OwnerLot);

            if (consign == null)
            {
                player.Out.SendMessage("No Consignment Merchant found", eChatType.CT_Merchant, eChatLoc.CL_ChatWindow);
                return;
            }

            IGameInventory inv = player.Inventory;
            eInventorySlot fromSlot = (eInventorySlot)item.SlotPosition;
            eInventorySlot toSlot = player.Inventory.FindFirstEmptySlot(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

            consign.OnPlayerBuy(player, inv, fromSlot, toSlot, true);
        }
    }
}
