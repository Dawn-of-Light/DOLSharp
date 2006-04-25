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
using System.Collections;
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.AI;
using DOL.AI.Brain;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Quests;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.Tasks
{
    
	/// <summary>
	/// The task manadger class
	/// </summary>
    public class TaskMgr
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// This is the last level where the task are avalable
        /// </summary>
        private static ushort MAX_TASK_LEVEL = 20;

        /// <summary>
        /// Check if Player can accept a new Task
        /// </summary>
        /// <param name="player">The GamePlayer Object</param>
        /// <returns>True Player have no other Chart</returns>
        public static bool CanGiveTask(GamePlayer player, GameMob taskGiver)
        {
            if (player == null || taskGiver == null) return false;

            if (!player.Position.CheckSquareDistance(taskGiver.Position, WorldMgr.WHISPER_DISTANCE * WorldMgr.WHISPER_DISTANCE))
            {
                player.Out.SendMessage("Your target is too far away!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }
            
            if (player.Level > MAX_TASK_LEVEL)
            {
                player.Out.SendMessage("Tasks are available only for level " + MAX_TASK_LEVEL + " or less!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.Task != null)
            {
                player.Out.SendMessage("You already have a Task. Select yourself and type /Task for more Information.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }

            if (player.TaskDone >= player.Level)
            {
                player.Out.SendMessage("You cannot do more than " + player.Level + " tasks at your level!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }

            AbstractTask newTask = null;
            if (taskGiver.OwnBrain is PeaceBrain)
            {
                if (taskGiver is GameMerchant)
                {
                    newTask = new MoneyTask();
                }
                else if (taskGiver is GameCraftMaster && ((GameCraftMaster)taskGiver).TheCraftingSkill == player.CraftingPrimarySkill)
                {
                  //  newTask = new CraftTask();
                }
                else
                {
                    newTask = new KillTask();
                }
            }

            if (newTask != null && newTask.StartTask(player, taskGiver))
            {
                player.Out.SendDialogBox(eDialogCode.SimpleWarning, 0, 0, 0, 0, eDialogType.Ok, false, "You have been given a task!");
                return true;
            }
            else
            {
                player.Out.SendMessage(taskGiver.GetName(0, true) + " does not have tasks to give.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
                return false;
            }
        }
    }
}
