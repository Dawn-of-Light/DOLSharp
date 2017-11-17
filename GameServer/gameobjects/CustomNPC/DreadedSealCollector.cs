using System;
//using System.Reflection;
//using DOL.AI.Brain;
using DOL.Database;
//using DOL.Events;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Language;

namespace DOL.GS
{
    /// <summary>
    /// LootGeneratorDreadedSeal
    /// Adds Glowing Dreaded Seal to loot
    /// </summary>
    public class DreadedSealCollector : GameNPC
    {
        private int m_count; // count of items, for stack
        private long amount = 0;

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

            if (player != null && item != null && currentrps < maxrps && item.Id_nb == "glowing_dreaded_seal"
                || item.Id_nb == "sanguine_dreaded_seal"
                || item.Id_nb == "lambent_dreaded_seal"
                || item.Id_nb == "lambent_dreaded_seal2"
                || item.Id_nb == "fulgent_dreaded_seal"
                || item.Id_nb == "effulgent_dreaded_seal")
            {
                m_count = item.Count;
                if (Level <= 20)
                {
                    ((GamePlayer)source).Out.SendMessage("You are too young yet to make use of these items "
                    + player.Name + ". Come back in " + (21 - Level) + " levels.", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
                    return false;
                }
                else if (Level > 20 & Level < 26)
                {
                    amount += (item.Price / 150) * m_count; // At level 21 to 25 you get 20 Realm Points per set of seals.
                    player.GainBountyPoints(1);             // Force a +1 BP gain
                }
                else if (Level > 25 & Level < 31)
                {
                    amount += (item.Price / 100) * m_count; // At level 26 to 30 you get 30 Realm Points per set of seals.
                    player.GainBountyPoints(2);             // Force a +2 BP gain
                }
                else if (Level > 30 & Level < 36)
                {
                    amount += (item.Price / 60) * m_count; // At level 31 to 35 you get 50 Realm Points per set of seals.
                    player.GainBountyPoints(3);            // Force a +3 BP gain
                }
                else if (Level > 35 & Level < 41)
                    amount += (item.Price / 10) * m_count; // At level 36 to 40 you get 300 Realm Points per set of seals.
                else if (Level > 40 & Level < 46)
                    amount += (item.Price / 4) * m_count; // At level 41 to 45 you get 700 Realm Points per set of seals.
                else if (Level > 45 & Level < 50)
                    amount += (item.Price / 2) * m_count; // At level 46 to 49 you get 1500 Realm Points per set of seals.
                else if (Level > 49)
                    amount += item.Price * m_count; // At level 50 you get 3000 Realm Points per set of seals.

                if (amount + currentrps > maxrps)
                    amount = maxrps - currentrps; // only give enough realm points to reach max

                player.GainRealmPoints(amount);

                if (Level > 35)
                {
                    player.GainBountyPoints(amount / 55);
                } // Only BP+ those of 36+ to prevent double BP gains
                player.Inventory.RemoveItem(item);
                player.Out.SendUpdatePoints();
                amount = 0;
                m_count = 0;
                currentrps = 0;
                return base.ReceiveItem(source, item);
            }

            ((GamePlayer)source).Out.SendMessage("I am not interested in that item, come back with something useful "
            + player.Name + ".", eChatType.CT_Say, eChatLoc.CL_ChatWindow);
            return false;
        }
    }
}
