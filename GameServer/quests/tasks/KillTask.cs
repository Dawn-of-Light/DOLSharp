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
using DOL.GS.Collections;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares a Kill Task
	/// </summary>
	public class KillTask : AbstractTask
	{
	    #region Declaration

        /// <summary>
        /// The name of the mob to kill
        /// </summary>
        protected string m_targetMobName = null;

        /// <summary>
        /// Gets or sets the name of the mob to kill
        /// </summary>
        public string TargetMobName
        {
            get { return m_targetMobName; }
            set { m_targetMobName = value; }
        }

        /// <summary>
        /// Store if we can give the reward to the player
        /// </summary>
        protected bool m_targetKilled = false;

        /// <summary>
        /// Gets or sets if we can give the reward to the player
        /// </summary>
        public bool TargetKilled
        {
            get { return m_targetKilled; }
            set { m_targetKilled = value; }
        }
	    
        /// <summary>
        /// Retrieves the description for the current task
        /// </summary>
        public override string Description
        {
            get
            {
                if (TargetKilled)
                {
                    return "[Task] You have killed your target and must now return to "+RewardGiverName+" for your reward!";
                }
                else
                {
                    return "[Task] You have been asked to kill "+TargetMobName+".";
                }
            }
        }

        #endregion

	    /// <summary>
        /// Start the task
        /// </summary>
        /// <param name="taskPlayer">The player doing the quest</param>
        /// <param name="taskGiver">The npc who give the task</param>
        public override bool StartTask(GamePlayer taskPlayer, GameMob taskGiver)
        {
            GameMob mobToKill = GetRandomMob(taskPlayer);
            if (mobToKill != null)
            {
                RewardGiverName = taskGiver.Name;
                TargetMobName = mobToKill.Name;

                taskPlayer.Out.SendMessage(RewardGiverName + " says, \"Very well " + taskPlayer.Name + ", it's good to see adventurers willing to help out the realm in such times.  Search to the northeast and kill " + mobToKill.Name + " and return to me for your reward.  Good luck!\"", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
            
                base.StartTask(taskPlayer, taskGiver);
                return true;
            }

            return false;
        }
	    
        /// <summary>
        /// The reward money table
        /// </summary>
        private static readonly int[] MoneyReward = new int[20] { 28, 57, 77, 105, 140, 190, 257, 347, 470, 632, 735, 852, 987, 1147, 1330, 1542, 1790, 2077, 2407, 2801 };					

        /// <summary>
        /// Called to finish the task.
        /// </summary>
        public override void FinishTask()
        {
            const ushort Scarto = 3; // Add/Remove % to the Result

            int ValueScarto = ((MoneyReward[m_taskPlayer.Level - 1] / 100) * Scarto);
            m_taskPlayer.AddMoney(Util.Random(MoneyReward[m_taskPlayer.Level - 1] - ValueScarto, MoneyReward[m_taskPlayer.Level - 1] + ValueScarto), RewardGiverName + " give you {0}!");

            double coef = (m_taskPlayer.Level < 19 ? 0.3 : 0.25); // 30% if level < 19 else 25 %
            long rewardXP = (long)((m_taskPlayer.ExperienceForNextLevel - m_taskPlayer.Experience) * coef);

            m_taskPlayer.Out.SendMessage("You have completed your task and earn " + rewardXP + " experience!", eChatType.CT_Important, eChatLoc.CL_SystemWindow);
            m_taskPlayer.GainExperience(rewardXP, 0, 0, false);

            base.FinishTask();
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
		public override void Notify(DOLEvent e, object sender, EventArgs args)
		{
			GamePlayer player = sender as GamePlayer;

			if (player == null || !(player.Task is KillTask))
				return;

            if (e == GameLivingEvent.EnemyKilled)
            {
                EnemyKilledEventArgs gArgs = (EnemyKilledEventArgs)args;
                if (gArgs.Target.Name == TargetMobName)
                {
                    lock (gArgs.Target.XPGainers.SyncRoot)
                    {
                        if (gArgs.Target.XPGainers.Keys.Count == 0)
                        {
                            return;
                        }

                        foreach (GameLiving gainer in gArgs.Target.XPGainers.Keys)
                        {
                            //If the killed npc is gray for one of the xpGainer (no matter if player or another npc)
                            //it is't worth anything either
                            if (gainer.IsObjectGreyCon(gArgs.Target))
                            {
                                return;
                            }
                        }
                    }
                    
                    player.Out.SendMessage("You must now return to "+RewardGiverName+" to receive your reward!", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                    TargetKilled = true;

                    player.Out.SendTaskUpdate();
                }
            }
            else if (e == GameObjectEvent.InteractWith)
			{
                InteractWithEventArgs gArgs = (InteractWithEventArgs)args;
                if (gArgs.Target.Name == RewardGiverName)
				{
                    if (TargetKilled)
                    {
                        player.Out.SendMessage(RewardGiverName + " says, \"Good work " + player.Name + ". Here is your reward as promised.\"", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
            
                        FinishTask();
                    }
				}
			}
		}

		/// <summary>
        /// Find a Random Mob bleue or yellow in the player ZONE
		/// </summary>
		/// <param name="player">The GamePlayer Object</param>		
		/// <returns>The GameMob Searched</returns>
        public static GameMob GetRandomMob(GamePlayer player)
		{
		    IList allValidMob = new ArrayList(1);
		    foreach (GameMob mob in player.Region.GetZone(player.Position).GetAllObjects(typeof(GameMob)))
		    {
		        if (mob.Realm == (byte)eRealm.None)
		        {
		            int conLevel = (int)player.GetConLevel(mob);
		            if (conLevel == -1 || conLevel == 0)
		            {
		                // bleue or yellow
		                allValidMob.Add(mob);
		            }
		        }
		    }
		    
            if (allValidMob.Count > 0) return (GameMob)allValidMob[Util.Random(allValidMob.Count - 1)];
            return null;
		}
	}
}
