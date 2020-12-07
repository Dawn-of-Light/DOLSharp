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
using DOL.Database;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
    /// <summary>
    /// LootGeneratorDreadedSeal
    /// Adds Glowing Dreaded Seal to loot
    /// </summary>
    public class DreadedSealCollector : GameNPC
    {
        private void SendReply(GamePlayer target, string msg)
        {
            target.Out.SendMessage(msg, eChatType.CT_System, eChatLoc.CL_ChatWindow);
        }

        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            player.Out.SendMessage("Hand me any Dreaded Seal and I'll give you realm points!",
                    eChatType.CT_Say, eChatLoc.CL_ChatWindow);
 
            return true;
        }

        protected static readonly string[] SealKeys =
            {
                "glowing_dreaded_seal",
                "sanguine_dreaded_seal",
                "lambent_dreaded_seal",
                "lambent_dreaded_seal2",
                "fulgent_dreaded_seal",
                "effulgent_dreaded_seal"
            };

        protected static readonly long[] SealValues = { 1, 1, 10, 10, 50, 250 };

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer player = source as GamePlayer;
            int Level = player.Level;
            long currentrps = player.RealmPoints;
            long maxrps = 66181501;

            if (GetDistanceTo(player) > WorldMgr.INTERACT_DISTANCE)
            {
                ((GamePlayer)source).Out.SendMessage("You are too far away to give anything to me "
                + player.Name + ". Come a little closer.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player != null && item != null)
            {
                int index = Array.IndexOf(SealKeys, item.Id_nb);

                if (index < 0)
                    return base.ReceiveItem(source, item);

                long amount = SealValues[index];

                if (Level <= 20)
                {
                    ((GamePlayer)source).Out.SendMessage("You are too young yet to make use of these items "
                    + player.Name + ". Come back in " + (21 - Level) + " levels.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    return false;
                }
                else if (Level < 26)
                {
                    amount *= 20 * item.Count; // At level 21 to 25 you get 20 Realm Points per set of seals.
                    player.GainBountyPoints(1);             // Force a +1 BP gain
                }
                else if (Level < 31)
                {
                    amount *= 30 * item.Count; // At level 26 to 30 you get 30 Realm Points per set of seals.
                    player.GainBountyPoints(2);             // Force a +2 BP gain
                }
                else if (Level < 36)
                {
                    amount *= 50 * item.Count; // At level 31 to 35 you get 50 Realm Points per set of seals.
                    player.GainBountyPoints(3);            // Force a +3 BP gain
                }
                else if (Level < 41)
                    amount *= 300 * item.Count; // At level 36 to 40 you get 300 Realm Points per set of seals.
                else if (Level < 46)
                    amount *= 700 * item.Count; // At level 41 to 45 you get 700 Realm Points per set of seals.
                else if (Level < 50)
                    amount *= 1500 * item.Count; // At level 46 to 49 you get 1500 Realm Points per set of seals.
                else
                    amount *= 3000 * item.Count; // At level 50 you get 3000 Realm Points per set of seals.

                if (currentrps < maxrps)
                {
                    if (amount + currentrps > maxrps)
                        amount = maxrps - currentrps; // only give enough realm points to reach max

                    player.GainRealmPoints(amount);
                }

                if (Level > 35)
                {
                    player.GainBountyPoints(amount / 55);
                } // Only BP+ those of 36+ to prevent double BP gains

                player.Inventory.RemoveItem(item);
                player.Out.SendUpdatePoints();

                return true;
            }

            return base.ReceiveItem(source, item);
        }
    }
}
