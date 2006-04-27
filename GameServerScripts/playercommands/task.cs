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
/*
 * LightBringer Task System
 * Author:	LightBringer
 * Date:	20040817
*/

using System.Collections;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS.Tasks;

namespace DOL.GS.Scripts
{
	[CmdAttribute("&task", (uint) ePrivLevel.Player, "Ask for a Task from Guards or Merchants", "/task")]
	public class TaskCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
            if (client.Player.TargetObject == null || client.Player.TargetObject == client.Player)
			{
                AbstractTask task = client.Player.Task;

				if (task != null)
				{
                    client.Player.Out.SendMessage(task.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                    long minutesLeft = ((task.StartingPlayedTime + 2 * 3600) - client.Player.PlayedTime) / 60;
                    client.Player.Out.SendMessage("You have " + (minutesLeft > 0 ? minutesLeft + " minutes" : "less than a minute") + " left to complete this task.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
                    client.Player.Out.SendMessage("You have no current personal task.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
               TaskMgr.CanGiveTask(client.Player, client.Player.TargetObject as GameMob);
			}
            return 0;
		}
	}

}