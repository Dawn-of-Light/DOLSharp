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
using DOL.GS.PacketHandler;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&quest", //command to handle
		 (uint) ePrivLevel.Player, //minimum privelege level
		 "Send the player's quest list", //command description
		 "/quest")] //usage
	public class QuestCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			GamePlayer player = client.Player;
			Iesi.Collections.ISet activeQuests = player.ActiveQuests;
			lock (activeQuests)
			{
				client.Out.SendMessage(" ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("You are currently doing the following quests :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach (AbstractQuest q in activeQuests)
				{
					string desc = q.Description;
					client.Out.SendMessage("Step " + q.Step + " of the quest '" + q.Name + ".'", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					client.Out.SendMessage("What you have to do : '" + desc.Substring(desc.IndexOf("]") + 1) + ".'" , eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}

			Iesi.Collections.ISet finishedQuests = player.FinishedQuests;
			lock (finishedQuests)
			{
				client.Out.SendMessage(" ", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				client.Out.SendMessage("You have finished the following quests :", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				foreach (AbstractQuest q in finishedQuests)
				{
					client.Out.SendMessage(q.Name + ", finished.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			return 0;
		}
	}
}