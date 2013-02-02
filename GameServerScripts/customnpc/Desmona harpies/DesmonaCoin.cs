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
using DOL.Database;

namespace DOL.GS.Scripts
{
    public class DesmonaCoin : GameInventoryItem
    {
        private DesmonaCoin() { }


        public DesmonaCoin(ItemTemplate template)
            : base(template)
        {
        }
        public DesmonaCoin(ItemUnique template)
            : base(template)
        {
        }
        public DesmonaCoin(InventoryItem item)
            : base(item)
        {
        }

        public override void Delve(List<string> delve, GamePlayer player)
        {
            base.Delve(delve, player);
        }
        public override void OnReceive(GamePlayer player)
        {
            base.OnReceive(player);

            foreach (GameNPC npc in player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
                npc.BroadcastUpdate();
        }
        public override void OnLose(GamePlayer player)
        {
            base.OnLose(player);

            foreach (GameNPC npc in player.GetNPCsInRadius(WorldMgr.VISIBILITY_DISTANCE))
                npc.BroadcastUpdate();
        }
    }
}