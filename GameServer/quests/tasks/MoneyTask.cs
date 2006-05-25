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
using DOL.AI;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS.Collections;
using DOL.GS.Database;
using DOL.Events;
using DOL.GS.PacketHandler;
using DOL.GS.Scripts;

namespace DOL.GS.Quests
{
	/// <summary>
	/// Declares a Money Task
	/// </summary>
	public class MoneyTask : AbstractTask
	{
	    #region Declaration

	     /// <summary>
        /// The name of the item to give
        /// </summary>
        protected string m_itemName = null;

        /// <summary>
        /// Gets or sets the name of the item to give
        /// </summary>
        public string ItemName
        {
            get { return m_itemName; }
            set { m_itemName = value; }
        }
	    
        /// <summary>
        /// Retrieves the description for the current task
        /// </summary>
        public override string Description
        {
            get
            {
               return "[Task] You have been asked to give "+ItemName+" to "+RewardGiverName+".";
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
            GameMob moneyTaskTarget = GetRandomMob(taskPlayer);
            if (moneyTaskTarget != null)
            {
                RewardGiverName = moneyTaskTarget.Name;
                taskPlayer.Out.SendMessage(taskGiver.Name + " says, \"Please take this to "+RewardGiverName+". "+moneyTaskTarget.GetPronoun(0, true)+" can be found in "+moneyTaskTarget.Region.Description+" in "+moneyTaskTarget.Region.GetZone(moneyTaskTarget.Position).Description+" to the south of here.\"", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                
                GenericItem item = GetRandomGenericItem(RewardGiverName);
                ItemName = item.Name;
                taskPlayer.ReceiveItem(taskGiver, item);
            
                base.StartTask(taskPlayer, taskGiver);
                return true;
            }

            return false;
        }
	    
        /// <summary>
        /// The reward money table
        /// </summary>
        private static readonly int[] MoneyReward = new int[20] {25,34,46,63,84,114,154,208,282,379,441,511,592,688,798,925,1074,1246,1444,1830};

        /// <summary>
        /// Called to finish the task.
        /// </summary>
        public override void FinishTask()
        {
            const ushort Scarto = 3; // Add/Remove % to the Result

            int ValueScarto = ((MoneyReward[m_taskPlayer.Level - 1] / 100) * Scarto);
            m_taskPlayer.AddMoney(Util.Random(MoneyReward[m_taskPlayer.Level - 1] - ValueScarto, MoneyReward[m_taskPlayer.Level - 1] + ValueScarto), RewardGiverName + " give you {0}!");

            double coef = 0.1; // 10%
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

			if (player == null || !(player.Task is MoneyTask))
				return;

            if (e == GamePlayerEvent.GiveItem)
			{
				GiveItemEventArgs gArgs = (GiveItemEventArgs)args;				
	
				if(gArgs.Target.Name == RewardGiverName && gArgs.Item.Name == ItemName)
				{
					player.Inventory.RemoveItem(gArgs.Item);
				    
				    player.Out.SendMessage(RewardGiverName + " says, \"I've been waiting for this, thank you!\"", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                
					FinishTask();
				}
			}	
		}
	    
	    // used to generate generic item
		private static readonly string[] StrFormat = {"{0}'s {1} {2}","{1} {2} of {0}"};
		private static readonly string[] Middle = {"Little","Small","Large","Big","Thin"};
		private static readonly string[] TaskObjects = {"Potion","Flask","Necklace","Ring","Dyes","Green Gem","Note","Scroll","Book","Hammer","Crown","Blue orb","Red orb","Silver orb","Wand","Bottle","Gold key","Silver key","Mirror","Scroll tube","Four leaf clover","Vine","Silver earring","Gold bracelet","Gold necklace","Amulet","Letter"};
		private static readonly int[] ObjectModels = {99,99,101,103,229,113,498,499,500,510,511,523,524,606,552,554,583,584,592,603,607,611,621,622,623,101,498};

	    /// <summary>
		/// Generate an Item random Named for NPC Drop
		/// </summary>
		/// <param name="rewardGiverName">Base Name of the NPC</param>
		/// <returns>A Generated NPC Item</returns>
		public static GenericItem GetRandomGenericItem(string rewardGiverName)
		{			
			int id = Util.Random(0, TaskObjects.Length - 1);
			
	        GenericItem item = new GenericItem();
	        item.IsDropable = false;
	        item.IsSaleable = false;
	        item.IsTradable = false;
	        item.Level = 0;
	        item.Weight = 5;
			item.Value = 0;
	        item.Model = ObjectModels[id];
	        item.Realm = eRealm.None;
	        item.Name = string.Format(StrFormat[Util.Random(StrFormat.Length - 1)], rewardGiverName, Middle[Util.Random(Middle.Length - 1)], TaskObjects[id]);
	        
	        return item;
	     }
	    
	    /// <summary>
        /// Find a random npc with a peace brain in the player ZONE
		/// </summary>
		/// <param name="player">The GamePlayer Object</param>		
		/// <returns>The GameMob Searched</returns>
        public static GameMob GetRandomMob(GamePlayer player)
		{
		    IList allValidMob = new ArrayList(1);
		    foreach (GameMob mob in player.Region.GetZone(player.Position).GetAllObjects(typeof(GameMob)))
		    {
		        if (mob.OwnBrain is PeaceBrain && (mob is GameMerchant || mob is GameTrainer || mob is GameCraftMaster))
                {
                    allValidMob.Add(mob);
		        }
		    }
		    
            if (allValidMob.Count > 0) return (GameMob)allValidMob[Util.Random(allValidMob.Count - 1)];
            return null;
		}
	}
}