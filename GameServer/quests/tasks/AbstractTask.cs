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
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;
using log4net;

namespace DOL.GS.Quests
{

	/// <summary>
	/// Declares the abstract task class from which all user created
	/// task must derive!
	/// </summary>
	public abstract class AbstractTask
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Declaration

        /// <summary>
        /// The unique active task identifier in the db
        /// </summary>
        protected int m_id;

        /// <summary>
        /// The player doing the task
        /// </summary>
        protected GamePlayer m_taskPlayer = null;

        /// <summary>
        /// The npc who give the reward at the end of the quest
        /// </summary>
        protected string m_rewardGiverName = null;

        /// <summary>
        /// The time left until the task will expire
        /// </summary>
        protected TimeSpan m_timeLeft;
		
        /// <summary>
        /// Gets or sets the unique active task identifier in the db
        /// </summary>
        public int AbstractTaskID
        {
            get { return m_id; }
            set { m_id = value; }
        }

        /// <summary>
        /// Gets or sets the player doing the task
        /// </summary>
        public GamePlayer TaskPlayer
        {
            get { return m_taskPlayer; }
            set { m_taskPlayer = value; }
        }

        /// <summary>
        /// Gets or sets the name of the npc who give the reward at the end of the quest
        /// </summary>
        public string RewardGiverName
        {
            get { return m_rewardGiverName; }
            set { m_rewardGiverName = value; }
        }

        /// <summary>
        /// Gets or sets the time left until the task will expire
        /// </summary>
        public TimeSpan TimeLeft
        {
            get { return m_timeLeft; }
            set
            {
                m_timeLeft = value;

              /*  m_expireTimer = new RegionTimer(m_taskPlayer);
                m_expireTimer.Callback = new RegionTimerCallback(TaskExpireTimerCallback);

                TimeSpan timeLeft;
                if (m_timeLeft.Minutes > 60)
                {
                    timeLeft = m_timeLeft.Subtract(new TimeSpan(0, 60, 0));
                }
                else if (m_timeLeft.Minutes > 10)
                {
                    timeLeft = m_timeLeft.Subtract(new TimeSpan(0, 10, 0));
                }
                else if (m_timeLeft.Minutes > 1)
                {
                    timeLeft = m_timeLeft.Subtract(new TimeSpan(0, 1, 0));
                }
                else
                {
                    timeLeft = m_timeLeft;
                }

                m_expireTimer.Start(timeLeft.Milliseconds);*/
            }
        }

        /// <summary>
        /// Task expire timer
        /// </summary>
        protected RegionTimer m_expireTimer;

        /// <summary>
        /// Retrieves the description for the current task
        /// </summary>
        public abstract string Description
        {
            get;
        }

        #endregion

        /// <summary>
        /// Timer callback for task expire
        /// </summary>
        /// <param name="callingTimer">the calling timer</param>
        /// <returns>the new intervall</returns>
        protected int TaskExpireTimerCallback(RegionTimer callingTimer)
        {
            if (m_taskPlayer.ObjectState != eObjectState.Active || !m_taskPlayer.Alive)
            {
                m_expireTimer = null;
                return 0;
            }

            TimeSpan newTimeLeft = m_timeLeft.Subtract(DateTime.Now.Subtract(m_taskPlayer.LastPlayed));
            if (newTimeLeft.Minutes >= 1)
            {
                m_taskPlayer.Out.SendMessage("Your task will expire in " + newTimeLeft.Minutes + " minutes.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                if (newTimeLeft.Minutes > 60)
                {
                    return newTimeLeft.Subtract(new TimeSpan(0, 60, 0)).Milliseconds;
                }
                else if (newTimeLeft.Minutes > 10)
                {
                    return newTimeLeft.Subtract(new TimeSpan(0, 10, 0)).Milliseconds;
                }
                else
                {
                    return newTimeLeft.Subtract(new TimeSpan(0, 1, 0)).Milliseconds;
                }
            }
            else
            {
                ExpireTask();
            }

            m_expireTimer = null;
            return 0;
        }

        /// <summary>
        /// Start the task
        /// </summary>
        /// <param name="taskPlayer">The player doing the task</param>
        /// <param name="taskGiver">The npc who give the task</param>
        public virtual bool StartTask(GamePlayer taskPlayer, GameMob taskGiver)
        {
            TaskPlayer = taskPlayer;
            taskPlayer.Task = this;
            TimeLeft = new TimeSpan(2, 0, 0);

            m_taskPlayer.Out.SendTaskUpdate();
            return true;
        }
	    
        /// <summary>
        /// Called to finish the task.
        /// </summary>
        public virtual void FinishTask()
        {
            m_taskPlayer.Task = null;
            m_taskPlayer.TaskDone ++;

            m_taskPlayer.Out.SendTaskUpdate();
            if(AbstractTaskID != 0) GameServer.Database.DeleteObject(this);
        }

        /// <summary>
        /// Called when the task fail
        /// </summary>
        public virtual void ExpireTask()
        {
            m_taskPlayer.Out.SendMessage("Your fail your task!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            m_taskPlayer.Task = null;

            m_taskPlayer.Out.SendTaskUpdate();
            if (AbstractTaskID != 0) GameServer.Database.DeleteObject(this);
        }

        /// <summary>
        /// This method needs to be implemented in each task.
        /// It is the core of the task. The global event hook of the GamePlayer.
        /// This method will be called whenever a GamePlayer with this task
        /// fires ANY event!
        /// </summary>
        /// <param name="e">The event type</param>
        /// <param name="sender">The sender of the event</param>
        /// <param name="args">The event arguments</param>
        public abstract void Notify(DOLEvent e, object sender, EventArgs args);

	}
}