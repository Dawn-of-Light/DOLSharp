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
        /// The /played when the task started
        /// </summary>
        protected long m_startingPlayedTime;
		
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
        /// Gets or sets the /played time when the task started
        /// </summary>
        public long StartingPlayedTime
        {
            get { return m_startingPlayedTime; }
            set { m_startingPlayedTime = value; }
        }

        /// <summary>
        /// Retrieves the description for the current task
        /// </summary>
        public abstract string Description
        {
            get;
        }

        /// <summary>
        /// Task expire timer
        /// </summary>
        protected RegionTimer m_taskExpireTimer;

        /// <summary>
        /// Gets or sets the RegionTimer of task expiration
        /// </summary>
        public RegionTimer TaskExpireTimer
        {
            get { return m_taskExpireTimer; }
            set { m_taskExpireTimer = value; }
        }
	    
        #endregion

        /// <summary>
		/// Starts the task expiration timer.
		/// </summary>
		public virtual void StartTaskExpireTimer()
		{
			if (m_taskPlayer.ObjectState != eObjectState.Active) return;
			if (m_taskExpireTimer.IsAlive) return;

            int secondLeftBeforeExpire = (int)((StartingPlayedTime + 2 * 3600) - m_taskPlayer.PlayedTime);
            if (secondLeftBeforeExpire <= 0)
            {
                m_taskExpireTimer.Start(1);
            }
            else
            {
                if (secondLeftBeforeExpire >= 3600) // 1 hour
                {
                    secondLeftBeforeExpire -= 3600;
                }
                else if(secondLeftBeforeExpire >= 600) // 10 min
                {
                    secondLeftBeforeExpire -= 600;
                }
                else if(secondLeftBeforeExpire >= 120) // 2 min
                {
                    secondLeftBeforeExpire -= 120;
                }

                m_taskExpireTimer.Start(secondLeftBeforeExpire * 1000);
            }
		}

        /// <summary>
        /// Stop the task expire timer
         /// </summary>
        public virtual void StopTaskExpireTimer()
        {
            if (m_taskExpireTimer == null) return;
            m_taskExpireTimer.Stop();
        }
    
        /// <summary>
        /// Timer callback for task expire
        /// </summary>
        /// <param name="callingTimer">the calling timer</param>
        /// <returns>the new intervall</returns>
        public int TaskExpireTimerCallback(RegionTimer callingTimer)
        {
            if (m_taskPlayer.ObjectState != eObjectState.Active || !m_taskPlayer.Alive)
            {
                return 0;
            }

            int secondLeftBeforeExpire = (int)((StartingPlayedTime + 2 * 3600) - m_taskPlayer.PlayedTime);
            if (secondLeftBeforeExpire <= 0)
            {
                ExpireTask();
                return 0;
            }
            else
            {
                m_taskPlayer.Out.SendMessage("Your task will expire in less than " + ((secondLeftBeforeExpire / 60) + 1) + " minutes.", eChatType.CT_Important, eChatLoc.CL_SystemWindow);

                if (secondLeftBeforeExpire >= 3600) // 1 hour
                {
                    secondLeftBeforeExpire -= 3600;
                }
                else if (secondLeftBeforeExpire >= 600) // 10 min
                {
                    secondLeftBeforeExpire -= 600;
                }
                else if (secondLeftBeforeExpire >= 120) // 2 min
                {
                    secondLeftBeforeExpire -= 120;
                }
                
                return secondLeftBeforeExpire * 1000;
            }
        }

        /// <summary>
        /// Start the task
        /// </summary>
        /// <param name="taskPlayer">The player doing the task</param>
        /// <param name="taskGiver">The npc who give the task</param>
        public virtual bool StartTask(GamePlayer taskPlayer, GameMob taskGiver)
        {
            TaskPlayer = taskPlayer;
            StartingPlayedTime = taskPlayer.PlayedTime;
            taskPlayer.Task = this;

            TaskExpireTimer = new RegionTimer(taskPlayer);
            TaskExpireTimer.Callback = new RegionTimerCallback(TaskExpireTimerCallback);

            StartTaskExpireTimer();
            
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
            
            StopTaskExpireTimer();

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

            StopTaskExpireTimer();
            
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