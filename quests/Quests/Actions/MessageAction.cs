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
    [QuestActionAttribute(ActionType = eActionType.Message)]
    class MessageAction : AbstractQuestAction<String,eTextType>
    {

        public MessageAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
            : base(questPart, actionType, p, q) {                           
        }


        public MessageAction(BaseQuestPart questPart,  String message, eTextType messageType)
            : this(questPart, eActionType.Message, (object)message, (object)messageType) { }
        


        public override void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            String message = QuestPartUtils.GetPersonalizedMessage(P, player);
            switch (Q)
            {
                case eTextType.Dialog:
                    player.Out.SendCustomDialog(message, null);
                    break;
                case eTextType.Emote:
                    player.Out.SendMessage(message, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                    break;
                case eTextType.Broadcast:
                    foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
                    {
                        clientz.Player.Out.SendMessage(message, eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                    }
                    break;
                case eTextType.Read:
                    player.Out.SendMessage("You read: \"" + message + "\"", eChatType.CT_Emote, eChatLoc.CL_PopupWindow);
                    break;  
                case eTextType.None:
                    //nohting
                    break;
            }
        }
    }
}
