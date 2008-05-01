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
using System.Text;
using DOL.GS.PacketHandler;
using DOL.Events;
using DOL.GS.Behaviour.Attributes;
using DOL.GS.Behaviour;
using DOL.Database;
using System.Collections;
using DOL.Language;

namespace DOL.GS.Behaviour.Actions
{
    [ActionAttribute(ActionType = eActionType.TakeItem,DefaultValueQ=1)]
    public class TakeItemAction : AbstractAction<ItemTemplate, int>
    {

        public TakeItemAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.TakeItem, p, q)
        {
        }


        public TakeItemAction(GameNPC defaultNPC,   ItemTemplate itemTemplate, int quantity)
            : this(defaultNPC, (object)itemTemplate,(object) quantity) { }



        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
            int count = Q;
            ItemTemplate itemToRemove = P;

            Hashtable dataSlots = new Hashtable(10);
            lock (player.Inventory)
            {
                ICollection allBackpackItems = player.Inventory.GetItemRange(eInventorySlot.FirstBackpack, eInventorySlot.LastBackpack);

                bool result = false;
                foreach (InventoryItem item in allBackpackItems)
                {
                    if (item.Name == itemToRemove.Name)
                    {

                        if (item.IsStackable) // is the item is stackable
                        {
                            if (item.Count >= count)
                            {
                                if (item.Count == count)
                                {
                                    dataSlots.Add(item, null);
                                }
                                else
                                {
                                    dataSlots.Add(item, count);
                                }
                                result = true;
                                break;
                            }
                            else
                            {
                                dataSlots.Add(item, null);
                                count -= item.Count;
                            }
                        }
                        else
                        {
                            dataSlots.Add(item, null);
                            if (count <= 1)
                            {
                                result = true;
                                break;
                            }
                            else
                            {
                                count--;
                            }
                        }
                    }
                }
                if (result == false)
                {
                    return;
                }

                GamePlayerInventory playerInventory = player.Inventory as GamePlayerInventory;
                playerInventory.BeginChanges();
                foreach (DictionaryEntry de in dataSlots)
                {
                    if (de.Value == null)
                    {
                        playerInventory.RemoveItem((InventoryItem)de.Key);
                    }
                    else
                    {
                        playerInventory.RemoveCountFromStack((InventoryItem)de.Key, (int)de.Value);
                    }
                }
                playerInventory.CommitChanges();

                if (NPC != null)
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Behaviour.TakeItemAction.YouGiveItemToNPC", itemToRemove.Name, NPC.GetName(0, false)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                }
                else
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Behaviour.TakeItemAction.YouGiveItem", itemToRemove.Name), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                }
            }
        }
    }
}
