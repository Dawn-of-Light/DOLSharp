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

using DOL.GS.Quests;
using DOL.Language;

namespace DOL.GS.Commands
{
	// Command handler for the various /commands used in quests


	[CmdAttribute(
		"&search",
		ePrivLevel.Player,
		"Search the current area.",
		"/search")]
	public class QuestSearchCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (IsSpammingCommand(client.Player, "search"))
				return;

			GamePlayer player = client.Player;

			if (player == null)
				return;

			bool searched = false;

			foreach (AbstractQuest quest in player.QuestList)
			{
				if (quest.Command(player, AbstractQuest.eQuestCommand.Search))
				{
					searched = true;
				}
			}

            // Also check for DataQuests started via searching

            if (searched == false)
            {
                foreach (AbstractArea area in player.CurrentAreas)
                {
                    if (area is QuestSearchArea && (area as QuestSearchArea).DataQuest != null && (area as QuestSearchArea).Step == 0)
                    {
                        if ((area as QuestSearchArea).DataQuest.Command(player, AbstractQuest.eQuestCommand.SearchStart, area))
                        {
                            searched = true;
                        }
                    }
                }
            }

			if (searched == false)
			{
				player.Out.SendMessage(LanguageMgr.GetTranslation(client, "Scripts.Players.Questcommands.Cannot"), PacketHandler.eChatType.CT_Important, PacketHandler.eChatLoc.CL_SystemWindow);
			}
		}
	}
}