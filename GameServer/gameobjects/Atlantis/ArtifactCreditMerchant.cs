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
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// A bounty merchant for artifact credit.
    /// </summary>
    /// <author>Aredhel</author>
    public class ArtifactCreditMerchant : GameBountyMerchant
    {
        /// <summary>
        /// A player right-clicked the merchant.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player))
                return false;

            String intro = String.Format("You're {0}, right? Please tell me you're here to share your [tales of battle] in the frontiers.",
                player.Name);

            SayTo(player, intro);
            return true;
        }

        /// <summary>
        /// Merchant has received a /whisper.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public override bool WhisperReceive(GameLiving source, String text)
        {
            GamePlayer player = source as GamePlayer;

            if (player == null)
                return false;

            switch (text.ToLower())
            {
                case "tales of battle":
                    String reply = String.Format("Oh, how I wish I could leave the lands of Atlantis at will as you do. {0} {1}",
                        "I'd [leave] in a heartbeat if I could, but I am only sustained by the lingering magic",
                        "of the Atlanteans. This hall is as far as I can go.");

                    SayTo(player, reply);
                    return true;
                case "leave":
                    reply = String.Format("I'd even join you in your war if I could, {0} {1} {2}",
                        "anything for a little adventure, a little excitement. I remember a time when we",
                        "sphinxes were greatly feared as guardians of more than just [knowledge].",
                        "Ah, but that time has long since passed.");

                    SayTo(player, reply);
                    return true;
                case "knowledge":
                    reply = String.Format("Indeed, that is all I am now: a living tome of knowledge, {0} {1}",
                        "particularly of the Atlanteans' lost artifacts. Perhaps you'd be willing to trade",
                        "stories of your adventures in the frontiers for some of this knowledge.");

                    SayTo(player, reply);
                    return true;
            }

            return base.WhisperReceive(source, text);
        }

        /// <summary>
        /// The merchant received an item from another GameLiving.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="item"></param>
        /// <returns></returns>
        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            GamePlayer player = source as GamePlayer;

            if (player != null && item != null && item.Name.EndsWith("Credit"))
            {
                if (ArtifactMgr.GrantArtifactBountyCredit(player, item.Name))
                {
                    lock (player.Inventory)
                        player.Inventory.RemoveItem(item);

                    return true;
                }
            }

            return base.ReceiveItem(source, item);
        }
    }
}
