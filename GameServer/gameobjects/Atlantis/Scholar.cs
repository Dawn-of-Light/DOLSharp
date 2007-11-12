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
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// The scholars handing out the artifacts.
    /// </summary>
    /// <author>Aredhel</author>
    public class Scholar : Researcher
    {
        public Scholar()
            : base() { }


        /// <summary>
        /// Interact with scholar.
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool Interact(GamePlayer player)
        {
            if (!base.Interact(player)) return false;

            String intro = String.Format("Which artifact may I assist you with, {0}? ",
                player.CharacterClass.Name);

            intro += "I study the lore and magic of the following artifacts: ";

            // TODO: Add the actual list of artifacts, this should come from the DB.

            SayTo(player, eChatLoc.CL_PopupWindow, intro);

            intro = String.Format("{0}, did you find any of the stories that chronicle the powers of the artifacts? ",
                player.Name);

            intro += "We can unlock the powers of these artifacts by studying the stories. ";
            intro += "I can take the story and unlock the artifact's magic.";

            SayTo(player, eChatLoc.CL_PopupWindow, intro);
            return true;
        }

        public override bool ReceiveItem(GameLiving source, InventoryItem item)
        {
            if (!(source is GamePlayer))
                return false;

            GamePlayer player = source as GamePlayer;

            //if (!ArtifactMgr.IsArtifactBook(item))
            //{
            //    player.Out.SendMessage(String.Format("{0} does not want that item.", GetName(0, true)));
            //    return false;
            //}

            return false;
        }
    }
}
