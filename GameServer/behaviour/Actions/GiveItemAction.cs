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
using DOL.Language;

namespace DOL.GS.Behaviour.Actions
{
    [ActionAttribute(ActionType = eActionType.GiveItem,IsNullableQ=true)]
    public class GiveItemAction : AbstractAction<ItemTemplate,GameNPC>
    {               

        public GiveItemAction(GameNPC defaultNPC,  Object p, Object q)
            : base(defaultNPC, eActionType.GiveItem, p, q)
        {                
        }


        public GiveItemAction(GameNPC defaultNPC,  ItemTemplate itemTemplate, GameNPC itemGiver)
            : this(defaultNPC, (object) itemTemplate, (object)itemGiver) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);
			InventoryItem inventoryItem = GameInventoryItem.Create<ItemTemplate>(P as ItemTemplate);

            if (Q == null)
            {

                if (!player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, inventoryItem))
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Behaviour.GiveItemAction.GiveButInventFull", inventoryItem.GetName(0, false)), eChatType.CT_System, eChatLoc.CL_SystemWindow);                    
                }
                else
                {
                    player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client, "Behaviour.GiveItemAction.YouReceiveItem", inventoryItem.GetName(0, false)), eChatType.CT_Loot, eChatLoc.CL_SystemWindow);
                }
            }
            else
            {                
                player.ReceiveItem(Q, inventoryItem);                
            }            
        }
    }
}
