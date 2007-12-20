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

namespace DOL.GS.Commands
{
	[CmdAttribute("&task", ePrivLevel.Player, "Ask for a Task from Guards or Merchants", "/task")]
	public class TaskCommandHandler : ICommandHandler
	{
		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length > 1)
			{
				if (args[1] == "abort")
				{
					if (client.Player.Task != null && client.Player.Task.TaskActive)
						client.Player.Task.ExpireTask();
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
			if (player.Task != null)
				player.Task.CheckTaskExpired();

			if (player.TargetObject == null || player.TargetObject == player)
			{
				//TaskMgr.UpdateDiaryWindow(Player);
				AbstractTask task = player.Task;

				if (task != null && task.TaskActive)
				{
					IList messages = new ArrayList(4);
					messages.Add("You are on " + task.Name);
					messages.Add("What to do: " + task.Description);
					messages.Add(" ");
					messages.Add("Task will expire at " + task.TimeOut.ToShortTimeString());
					messages.Add("You have done " + task.TasksDone + " tasks out of " + AbstractTask.MaxTasksDone(player.Level) + " until now.");
					player.Out.SendCustomTextWindow("Tasks (Snapshot)", messages);
				}
				else if (task != null && task.TasksDone >= AbstractTask.MaxTasksDone(player.Level))
				{
					player.Out.SendMessage("You can do no more tasks at your current level", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				else
				{
					player.Out.SendMessage("You have currently no pending task", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
				return true;
			}
			else
			{
				if (WorldMgr.GetDistance(player.X, player.Y, 0, player.TargetObject.X, player.TargetObject.Y, 0) <= WorldMgr.WHISPER_DISTANCE)
					//ToDo: Use follow Line instead of Above when Dol will support Mob with a Z coordinate.
					//foreach(GameNPC mob in WorldMgr.GetNPCsCloseToObject(Player, WorldMgr.WHISPER_DISTANCE))
				{
					if (player.TargetObject is GameLiving)
					{
						if (KillTask.CheckAvailability(player, (GameLiving) player.TargetObject))
						{
							KillTask.BuildTask(player, (GameLiving) player.TargetObject);
							return true;
						}
						else if (MoneyTask.CheckAvailability(player, (GameLiving) player.TargetObject))
						{
							MoneyTask.BuildTask(player, (GameLiving) player.TargetObject);
							return true;
						}
						else if (CraftTask.CheckAvailability(player, (GameLiving) player.TargetObject))
						{
							CraftTask.BuildTask(player, (GameLiving) player.TargetObject);
							return true;
						}
					}
					return false;
				}
				else
				{
					player.Out.SendMessage(player.TargetObject.GetName(0, true) + " is too far away to interact", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					return false;
				}
			}
		}
	}

}