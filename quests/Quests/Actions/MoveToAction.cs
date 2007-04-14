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
using DOL.GS.Quests.Attributes;

namespace DOL.GS.Quests.Actions
{
    [QuestActionAttribute(ActionType = eActionType.MoveTo)]
    class MoveToAction : AbstractQuestAction<GameLocation,GameLiving>
    {               

        public MoveToAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
            : base(questPart, actionType, p, q) {
                
            }


        public MoveToAction(BaseQuestPart questPart, GameLocation location, GameLiving npc)
            : this(questPart, eActionType.MoveTo, (object)location,(object) npc) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            GameLiving npc = Q;

            if (P is GameLocation)
            {
                GameLocation location = (GameLocation)P;
                npc.MoveTo(location.RegionID, location.X, location.Y, location.Z, location.Heading);
            }
            else
            {
                npc.MoveTo(player.CurrentRegionID, player.X, player.Y, player.Z, (ushort)player.Heading);
            }
            
        }
    }
}
