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
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using System;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&quest",
		ePrivLevel.Player,
		"Display the players completed quests", "/quest")]
	public class QuestCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			string message = "\n";
			if (client.Player.QuestList.Count == 0)
				message += "You have no currently pending quests.\n";
			else
			{
				message += "You are currently working on the following quests:\n";
				foreach (AbstractQuest quest in client.Player.QuestList)
				{
					message += String.Format("On step {0} of quest '{1}'\n", quest.Step, quest.Name);
					message += String.Format("What to do: {0}", quest.Description);
				}
			}
			if (client.Player.QuestListFinished.Count == 0)
				message += "\nYou have not yet completed any quests.\n";
			else
			{
				message += "\nYou have completed the following quests:\n";
				foreach (AbstractQuest quest in client.Player.QuestListFinished)
					message += quest.Name + ", completed.\n";
			}
			DisplayMessage(client, message);
		}
	}
}