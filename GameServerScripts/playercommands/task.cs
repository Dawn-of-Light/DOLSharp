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
			if (args.Length > 1)
			{
				if (args[1] == "abort")
				{
					if (client.Player.Task != null) client.Player.Task.ExpireTask();
				}
			}
			else
			{
				TaskCommand(client.Player);
			}
			return 1;
		}

		/// <summary>
		/// Execute /Task Command
		/// Same if Player write /Task or press TaskButtom
		/// </summary>
		/// <param name="player">The GamePlayer Object</param>
		/// <returns>True if Command Execute Succesfully</returns>
		public static bool TaskCommand(GamePlayer player)
		{
			if (player.TargetObject == null || player.TargetObject == player)
			{
				AbstractTask task = player.Task;

				if (task != null)
				{
                    player.Out.SendMessage(task.Description, eChatType.CT_System, eChatLoc.CL_SystemWindow);
                   // player.Out.SendMessage("You have 116 minutes left to complete this task.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You have no current personal task.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			else
			{
				 return TaskMgr.CanGiveTask(player, player.TargetObject as GameMob);
			}
		}
	}

}