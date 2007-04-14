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
    [QuestActionAttribute(ActionType = eActionType.OfferQuest,DefaultValueP=eDefaultValueConstants.QuestType)]
    class OfferQuestAction : AbstractQuestAction<Type,String>
    {               

        public OfferQuestAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
            : base(questPart, actionType, p, q) {                
        }


        public OfferQuestAction(BaseQuestPart questPart,  Type questType, String offerMessage)
            : this(questPart, eActionType.OfferQuest, (object)questType, (object)offerMessage) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            string message = QuestPartUtils.GetPersonalizedMessage(Q, player);
            QuestMgr.ProposeQuestToPlayer(P, message, player, NPC);
        }
    }
}
