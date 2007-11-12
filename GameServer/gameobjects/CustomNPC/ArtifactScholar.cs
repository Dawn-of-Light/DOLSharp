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
    public class ArtifactScholar : GameNPC
    {
        public ArtifactScholar()
            : base()
        {
        }

        public override bool AddToWorld()
        {
            return base.AddToWorld();
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
