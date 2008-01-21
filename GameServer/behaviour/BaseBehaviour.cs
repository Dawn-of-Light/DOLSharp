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
using System.Collections.Generic;

namespace DOL.GS.Behaviour
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
	public class BaseBehaviour
    {        

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Variables
                        
		private GameNPC npc;
		
		private List<IBehaviourRequirement> requirements;              
        private List<IBehaviourAction> actions;        
        private List<IBehaviourTrigger> triggers;       

        private DOLEventHandler eventHandler;

        private int id;               

        #endregion

        #region Properties        

        public int ID
        {
            get { return id; }
            set { id = value; }
        }        

        /// <summary>
        /// NPC assigned with behaviour
        /// </summary>
        public GameNPC NPC
        {
            get { return npc; }
            set { npc = value; }
        }
        
        /// <summary>
        /// List of triggers that can initiate a behaviour
        /// Note: At least one trigger must be fulfilled to fire behaviour
        /// </summary>
        public List<IBehaviourTrigger> Triggers
        {
            get { return triggers; }
        }

        /// <summary>
        /// List of action that will be performed whenever this questpart fires, depending on requirements and triggers
        /// All actions of questpart will be performed once questpart fires
        /// </summary>
        public List<IBehaviourAction> Actions
        {
            get { return actions; }
        }

        /// <summary>
        /// List of Requirements that must be fulfilled to fire this questpart
        /// Note: All Requirmenets must be fulfilled to fire questpart
        /// </summary>
        public List<IBehaviourRequirement> Requirements
        {
            get { return requirements; }
        }

        public DOLEventHandler NotifyHandler
	    {
		    get { return eventHandler;}
		    set { eventHandler = value;}
	    }

        #endregion
        
        /// <summary>
        /// Creates a Behaviour for the given default npc.
        /// </summary>        
        /// <param name="npc">NPC associated with his behaviour typically NPC talking to or mob killing, etc...</param>                
        public BaseBehaviour(GameNPC npc)
        {            
            this.NPC = npc;
            NotifyHandler = new DOLEventHandler(this.Notify);
        }

        #region Triggers

        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// Both keyword and variable will be null.
        /// </summary>
        /// <param name="triggerType"></param>
        public void AddTrigger(eTriggerType triggerType)
        {
            AddTrigger(triggerType, null, null);
        }

        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// Variable I will be null
        /// </summary>
        /// <param name="triggerType">triggertype</param>
        /// <param name="keyword">keyword (K), meaning depends on triggertype</param>
        public void AddTrigger(eTriggerType triggerType, Object keyword)
        {
            AddTrigger(triggerType, keyword,null);
        }
        
        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// </summary>
        /// <param name="triggerType">triggertype</param>
        /// <param name="keyword">keyword (K), meaning depends on triggertype</param>
        /// <param name="var">variable (I), meaning depends on triggertype</param>
        public void AddTrigger(eTriggerType triggerType, Object keyword , Object var)
        {
            IBehaviourTrigger trigger = null;

            Type type = BehaviourMgr.GetTypeForTriggerType(triggerType);
            if (type != null)
            {
                trigger = (IBehaviourTrigger)Activator.CreateInstance(type, new object[] { this.NPC, NotifyHandler, keyword, var });
                AddTrigger(trigger);
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error("No registered trigger found for TriggerType " + triggerType);
            }            
        }

        

        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// </summary>
        /// <param name="trigger"></param>
        public void AddTrigger(IBehaviourTrigger trigger)
        {
            if (triggers == null)
                triggers = new List<IBehaviourTrigger>();
            
            triggers.Add(trigger);            
            trigger.Register();
        } 

        /// <summary>
        /// Checks the added triggers, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        protected virtual bool CheckTriggers(DOLEvent e, object sender, EventArgs args)
        {            
            if (triggers != null)
            {
                foreach (IBehaviourTrigger trigger in triggers)
                {
                    if (trigger.Check(e, sender, args))
                        return true;
                }
            }
            return false;
        }
        #endregion

        #region Actions        

        /// <summary>
        /// Adds an Action to the QuestPart that will be performed once the QuestPart fires
        /// </summary>
        /// <param name="actionType">ActionType</param>
        /// <param name="p">First Action Variable, meaning depends on ActionType</param>
        public void AddAction(eActionType actionType, Object p)
        {
            AddAction(actionType, p, null);
        }

        /// <summary>
        /// Adds an Action to the QuestPart that will be performed once the QuestPart fires
        /// </summary>
        /// <param name="actionType">ActionType</param>
        /// <param name="p">First Action Variable, meaning depends on ActionType</param>
        /// <param name="q">Second Action Variable, meaning depends on ActionType</param>
        public void AddAction(eActionType actionType, Object p, Object q)
        {            
            IBehaviourAction action = null;

            Type type = BehaviourMgr.GetTypeForActionType(actionType);
            if (type != null)
            {                
                action = (IBehaviourAction)Activator.CreateInstance(type, new object[] { this.NPC, p, q });
                AddAction(action);
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error("No registered action found for ActionType " + actionType);
            }
            
        }

        /// <summary>
        /// Adds an Action to the QuestPart that will be performed once the QuestPart fires
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(IBehaviourAction action)
        {
            if (actions == null)
                actions = new List<IBehaviourAction>();
            
            actions.Add(action);
        }               

        #endregion

        #region Requirements        

        /// <summary>
        /// Adds a new Requirement to the QuestPart.
        /// V will be null and Comparator will be eComparator.None
        /// </summary>
        /// <param name="requirementType">RequirementType</param>
        /// <param name="requirementN">First Requirement Variable, meaning depends on RequirementType</param>
        public void AddRequirement(eRequirementType requirementType, Object requirementN)
        {
            AddRequirement(requirementType, requirementN, null, eComparator.None);
        }

        /// <summary>
        /// Adds a new Requirement to the QuestPart.
        /// Comparator will be eComparator.None
        /// </summary>
        /// <param name="requirementType">RequirementType</param>
        /// <param name="requirementN">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="requirmentV">Second Requirement Variable, meaning depends on RequirementType</param>
        public void AddRequirement(eRequirementType requirementType, Object requirementN, Object requirmentV)
        {
            AddRequirement(requirementType, requirementN, requirmentV, eComparator.None);
        }

        /// <summary>
        /// Adds a new Requirement to the QuestPart.
        /// V will be null
        /// </summary>
        /// <param name="requirementType"></param>
        /// <param name="requirementN">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="requirementComparator">Comparator used if some values are veeing compared</param>
        public void AddRequirement(eRequirementType requirementType, Object requirementN, eComparator requirementComparator)
        {
            AddRequirement(requirementType, requirementN, null, requirementComparator);
        }

        /// <summary>
        /// Adds a new Requirement to the QuestPart.        
        /// </summary>
        /// <param name="requ"></param>
        public void AddRequirement(IBehaviourRequirement requ)
        {
            if (requirements == null)
                requirements = new List<IBehaviourRequirement>();
            
            requirements.Add(requ);
        }
        /// <summary>
        /// Adds a new Requirement to the QuestPart.
        /// </summary>
        /// <param name="requirementType">RequirementType</param>
        /// <param name="requirementN">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="requirementV">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="requirementComparator">Comparator used if some values are veeing compared</param>        
        public void AddRequirement(eRequirementType requirementType, Object requirementN, Object requirementV,eComparator requirementComparator) {

            IBehaviourRequirement requ = null;

            Type type = BehaviourMgr.GetTypeForRequirementType(requirementType);
            if (type != null)
            {                                
                requ = (IBehaviourRequirement)Activator.CreateInstance(type, new object[] { this.NPC, requirementN, requirementV, requirementComparator });
                AddRequirement(requ);
            }
            else
            {
                if (log.IsErrorEnabled)
                    log.Error("No registered requirement found for RequirementType " + requirementType);
            }

            
		}

        /// <summary>
        /// Checks the added requirements whenever a trigger associated with this questpart fires.(returns true)
        /// </summary>        
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>        
        /// <returns>true if all Requirements forQuestPart where fullfilled, else false</returns>
        protected virtual bool CheckRequirements(DOLEvent e, object sender, EventArgs args)
        {			
            if (requirements != null)
            {
                foreach (IBehaviourRequirement requirement in requirements)
                {
                    if (!requirement.Check(e, sender, args))
                        return false;                    
                }
            }
			return true;
        }
        
        #endregion        

        /// <summary>
        /// This method is called by the BaseQuest whenever a event associated with the Quest accurs
        /// or a automatically added eventhandler for the triggers fires
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        public virtual void Notify(DOLEvent e, object sender, EventArgs args)
        {
            if (CheckTriggers(e, sender, args) && CheckRequirements(e, sender, args))
            {
                foreach (IBehaviourAction action in actions)
                {
                    action.Perform(e, sender, args);
                }
            }
		}
	}
}
