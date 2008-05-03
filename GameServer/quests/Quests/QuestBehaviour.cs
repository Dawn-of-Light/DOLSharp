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
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Database2;
using DOL.Events;
using DOL.AI.Brain;
using log4net;
using System.Reflection;
using DOL.GS.Behaviour;

namespace DOL.GS.Quests
{    
        			
	/// <summary>
	/// BaseQuestParts are the core element of the new questsystem,
    /// you can add as many QuestAction to a quest as you want. 
    /// 
    /// A QuestAction contains basically 3 Things: Trigger, Requirements, Actions 
    ///
    /// Triggers: A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    ///
    /// Requirements: Requirements describe what must be true to allow a QuestAction to fire.
    /// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
    /// additional parameters. To fire a QuestAction ALL requirements must be fulfilled. 
    ///
    /// Actions: If one trigger and all requirements are fulfilled the corresponding actions of
    /// a QuestAction will we executed one after another. Actions can be more or less anything:
    /// at the moment there are: GiveItem, TakeItem, Talk, Give Quest, Increase Quest Step, FinishQuest,
    /// etc....
	/// </summary>
	public class QuestBehaviour : BaseBehaviour
    {

        public const string NUMBER_OF_EXECUTIONS = "quest.numberOfExecutions";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Variables
                
        private Type questType;		
        
        private int maxNumberOfExecutions;                       

        #endregion

        #region Properties        
        

        /// <summary>
        /// Type of quest this questpart belnogs to
        /// </summary>
        public Type QuestType
        {
            get { return questType; }
            set { questType = value; }
        }        

        public int MaxNumberOfExecutions
        {
            get { return maxNumberOfExecutions; }
        }         

        #endregion
        

        /// <summary>
        /// Creates a QuestPart for the given questtype with the default npc.
        /// </summary>
        /// <param name="questType">type of Quest this QuestPart will belong to.</param>
        /// <param name="npc">NPC associated with his questpart typically NPC talking to or mob killing, etc...</param>        
        public QuestBehaviour(Type questType, GameNPC npc) 
            : this (questType,npc,-1) { }

        /// <summary>
        /// Creates a QuestPart for the given questtype with the default npc.
        /// </summary>
        /// <param name="questType">type of Quest this QuestPart will belong to.</param>
        /// <param name="npc">NPC associated with his questpart typically NPC talking to or mob killing, etc...</param>        
        /// <param name="executions">Maximum number of executions the questpart should be execute during one quest for each player</param>
        public QuestBehaviour(Type questType, GameNPC npc, int executions) : base (npc)
        {
            this.questType = questType;            
            this.maxNumberOfExecutions = executions;            
        }                

        /// <summary>
        /// This method is called by the BaseQuest whenever a event associated with the Quest accurs
        /// or a automatically added eventhandler for the trigers fires
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>        
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>        
        public override void Notify(DOLEvent e, object sender, EventArgs args)
        {            
            GamePlayer player = BehaviourUtils.GuessGamePlayerFromNotify(e, sender, args);

            if (player == null)
            {
				//if (log.IsDebugEnabled)
				//    log.Debug("Couldn't guess player for EventArgs " + args + ". Triggers with this eventargs type won't work within quests.");
                return;
            }
            AbstractQuest quest = player.IsDoingQuest(QuestType);
            
            int executions = 0;
            if (quest != null && quest.GetCustomProperty(this.ID + "_" + NUMBER_OF_EXECUTIONS) != null)
            {
                executions = Convert.ToInt32(quest.GetCustomProperty(ID + "_" + NUMBER_OF_EXECUTIONS));                
            }

            if (MaxNumberOfExecutions < 0 || executions < this.MaxNumberOfExecutions)
            {
                if (CheckTriggers(e, sender, args) && CheckRequirements(e, sender, args))
                {
                    foreach (IBehaviourAction action in Actions)
                    {
                        action.Perform(e, sender, args);
                    }
                    if (quest != null)
                        quest.SetCustomProperty(this.ID + "_" + NUMBER_OF_EXECUTIONS, Convert.ToString(executions + 1));                                                        
                }                
            }
		}
	}
}
