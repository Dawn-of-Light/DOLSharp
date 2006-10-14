/*
 * Created by SharpDevelop.
 * User: Hugo
 * Date: 31.10.2005
 * Time: 22:05
 * 
 * To change this template use Tools | Options | Coding | Edit Standard Headers.
 */

using System;
using DOL.GS.PacketHandler;
using System.Collections;
using DOL.Database;
using DOL.Events;
using DOL.AI.Brain;
using log4net;
using System.Reflection;

namespace DOL.GS.Quests
{    
    
    /// <summary>
    /// Type of textoutput this one is used for general text messages within questpart.   
    /// </summary>
    public enum eTextType : byte
    {
        /// <summary>
        /// No output at all
        /// </summary>
        None = 0x00,
        /// <summary>
        /// EMOT : display the text localy without monster's name (local channel)
        /// </summary>
        /// <remarks>Tested</remarks>
        Emote = 0x01,
        /// <summary>
        /// BROA : broadcast the text in the entire zone (broadcast channel)
        /// </summary>
        Broadcast = 0x02,
        /// <summary>
        /// DIAG : display the text in a dialog box with an OK button
        /// </summary>
        Dialog = 0x03,  
        /// <summary>
        /// DSAY : only the player will hear localy what the monster say (local channel)
        /// </summary>
        DirectSay = 0x04, 
        /// <summary>
        /// READ : open a description (bracket) windows saying what is written on the item
        /// </summary>
        Read = 0x05, 
        /// <summary>
        /// TALK : monster will talk locally (local channel)
        /// </summary>
        /// <remarks>Tested</remarks>
        Talk = 0x06, 
        /// <summary>
        /// WHIS : text appears in a description window, words with brackets are keywords
        /// </summary>
        Whisper = 0x07
    }
			
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
	public class BaseQuestPart
    {
        /// <summary>
        /// Player Constant will be replaced by players name in output messages.
        /// </summary>
        const string PLAYER = "{Player}";

        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        #region Variables
                
        private Type questType;               
        private eTextType textType;               
        private String actionMessage;        
		private GameNPC npc;
		
		private IList requirements;              
        private IList actions;        
        private IList triggers;

        private bool questpartAdded;        

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

        /// <summary>
        /// NPC assigned with questPart
        /// </summary>
        public GameNPC NPC
        {
            get { return npc; }
            set { npc = value; }
        }

        /// <summary>
        /// Type of textoutput this questpart shall use
        /// </summary>
        public eTextType TextType
        {
            get { return textType; }
            set { textType = value; }
        }

        /// <summary>
        /// Wether this questpart has already been added to a quest or not?
        /// </summary>
        internal bool QuestPartAdded
        {
            get { return questpartAdded; }
            set { questpartAdded = value; }
        }
        /// <summary>
        /// List of triggers that can initiate a questpart
        /// Note: At least one trigger must be fulfilled to fire questpart
        /// </summary>
        public IList Triggers
        {
            get { return triggers; }
        }

        /// <summary>
        /// List of action that will be performed whenever this questpart fires, depending on requirements and triggers
        /// All actions of questpart will be performed once questpart fires
        /// </summary>
        public IList Actions
        {
            get { return actions; }
        }

        /// <summary>
        /// List of Requirements that must be fulfilled to fire this questpart
        /// Note: All Requirmenets must be fulfilled to fire questpart
        /// </summary>
        public IList Requirements
        {
            get { return requirements; }
        }

        /// <summary>
        /// Text to display
        /// </summary>
        public String TextMessage
        {
            get { return actionMessage; }
            set { actionMessage = value; }
        }

        #endregion
        /// <summary>
        /// Creates a more or less empty QuestPart, use AddTrigger, AddRequirement and AddAction to fill it properly
        /// </summary>
        /// <param name="questType">type of Quest this QuestPart will belong to.</param>
        /// <param name="npc">NPC associated with his questpart typically NPC talking to or mob killing, etc...</param>
        public BaseQuestPart(Type questType, GameNPC npc) : this(questType,npc,eTextType.None,null) {}

        /// <summary>
        /// Creates a QuestPart that will display the defined message, whenever it fires.
        /// </summary>
        /// <param name="questType">type of Quest this QuestPart will belong to.</param>
        /// <param name="npc">NPC associated with his questpart typically NPC talking to or mob killing, etc...</param>
        /// <param name="textType">type of Textoutput, one of eTextType</param>
        /// <param name="actionMessage">Message to display</param>
        public BaseQuestPart(Type questType, GameNPC npc, eTextType textType, String actionMessage)
        {
            this.QuestType = questType;
            this.NPC = npc;
            this.TextType = textType;
            this.TextMessage = actionMessage;
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
        public void AddTrigger(eTriggerType triggerType, String keyword)
        {
            AddTrigger(triggerType, keyword,null);
        }
        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// </summary>
        /// <param name="triggerType">triggertype</param>
        /// <param name="keyword">keyword (K), meaning depends on triggertype</param>
        /// <param name="var">variable (I), meaning depends on triggertype</param>
        public void AddTrigger(eTriggerType triggerType, String keyword , Object var)
        {           
            BaseQuestTrigger trigger = new BaseQuestTrigger(this,triggerType, keyword, var);
            AddTrigger(trigger);
        }

        /// <summary>
        /// Adds a trigger to the questpart for details about parameters look at documentation of used triggertype
        /// </summary>
        /// <param name="trigger"></param>
        public void AddTrigger(IQuestTrigger trigger)
        {
            if (triggers == null)
                triggers = new ArrayList();
            
            triggers.Add(trigger);

            // if the questpart was already added, we have to reregister the events for the new triggers
            if (QuestPartAdded)
                trigger.Register();
        } 

        /// <summary>
        /// Checks the added triggers, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if QuestPart should be executes, else false</returns>
        protected virtual bool CheckTriggers(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {            
            if (triggers != null)
            {
                foreach (IQuestTrigger trigger in triggers)
                {
                    if (trigger.Check(e, sender, args, player))
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
            IQuestAction action = new BaseQuestAction(this,actionType, p, q);
            AddAction(action);
        }

        /// <summary>
        /// Adds an Action to the QuestPart that will be performed once the QuestPart fires
        /// </summary>
        /// <param name="action"></param>
        public void AddAction(IQuestAction action)
        {
            if (actions == null)
                actions = new ArrayList();
            
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
        public void AddRequirement(IQuestRequirement requ)
        {
            if (requirements == null)
                requirements = new ArrayList();
            
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
            IQuestRequirement requ = new BaseQuestRequirement(this,requirementType,requirementN,requirementV,requirementComparator);
            AddRequirement(requ);
		}

        /// <summary>
        /// Checks the added requirements whenever a trigger associated with this questpart fires.(returns true)
        /// </summary>        
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if all Requirements forQuestPart where fullfilled, else false</returns>
        protected virtual bool CheckRequirements(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {			
            if (requirements != null)
            {
                foreach (IQuestRequirement requirement in requirements)
                {
                    if (!requirement.Check(e, sender, args, player))
                        return false;                    
                }
            }
			return true;
        }
        
        #endregion


        /// <summary>
        /// QuestPart Notify method 
        /// </summary>
        /// <param name="e"></param>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        public void Notify(DOLEvent e, object sender, EventArgs args)
        {
            GamePlayer player = null;
            
            if (sender is GamePlayer)
                player = sender as GamePlayer;
            else if (e == GameLivingEvent.WhisperReceive || e==GameLivingEvent.Interact )
            {
                player = ((SourceEventArgs)args).Source as GamePlayer;
            }
            else if (e == AreaEvent.PlayerEnter || e == AreaEvent.PlayerLeave)
            {
                AreaEventArgs aArgs = (AreaEventArgs)args;
                player = aArgs.GameObject as GamePlayer;
            }
            else if (e == GameLivingEvent.Dying)
            {
                //all players in visible distance will get notify.
                GameLiving living = sender as GameLiving;
                if (sender!=null){
                    foreach (GamePlayer vizPlayer in living.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
                    {
                        Notify(e, sender, args, vizPlayer);
                    }
                    return;
                }
            }

            if (player != null)
                Notify(e, sender, args, player);
            else
                log.Warn("BaseQuestPart: Event without player occured." + e);
        }

        /// <summary>
        /// This method is called by the BaseQuest whenever a event associated with the Quest accurs
        /// or a automatically added eventhandler for the trigers fires
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        public void Notify(DOLEvent e, object sender, EventArgs args, GamePlayer player)
        {
            if (CheckTriggers(e,sender,args,player) && CheckRequirements(e, sender, args, player))
            {
                string playerMessage = GetPersonalizedMessage(TextMessage,player);
                switch (TextType)
                {
                    case eTextType.None: break;
                    case eTextType.Emote:
                        player.Out.SendMessage(playerMessage, eChatType.CT_Emote, eChatLoc.CL_SystemWindow);
                        break;
                    case eTextType.Broadcast:                         
                        foreach (GameClient clientz in WorldMgr.GetAllPlayingClients())
                        {
                            clientz.Player.Out.SendMessage(playerMessage, eChatType.CT_Broadcast, eChatLoc.CL_SystemWindow);
                        }
                        break;
                    case eTextType.Dialog:                             
                        player.Out.SendCustomDialog(playerMessage, null);
                        break;
                    case eTextType.DirectSay:
                        NPC.TurnTo(player);
                        NPC.SayTo(player, playerMessage);
                        break;
                    case eTextType.Read:
                        player.Out.SendMessage("[ " + playerMessage + " ]", eChatType.CT_Emote, eChatLoc.CL_PopupWindow);
                        break;
                    case eTextType.Talk:
                        NPC.TurnTo(player);
                        NPC.SayTo(player, playerMessage);
                        break;
                    case eTextType.Whisper:
                        NPC.TurnTo(player);
                        NPC.SayTo(player, playerMessage);
                        break;
                }                
                
                if (actions == null)
                    return;
                foreach (IQuestAction action in actions)
                {
                    action.Perform(e, sender, args, player);
                }
            }            
		}

        /// <summary>
        /// Personalizes the given message by replacing all instances of PLAYER with the actual name of the player
        /// </summary>
        /// <param name="message">message to personalize</param>
        /// <param name="player">Player's name to insert</param>
        /// <returns>message with actual name of player instead of PLAYER</returns>
        public static string GetPersonalizedMessage(string message, GamePlayer player)
        {
            if (message == null || player == null)
                return message;

            string playerMessage;
            int playerIndex = message.IndexOf(PLAYER);
            if (playerIndex == 0)
                playerMessage = message.Replace(PLAYER, player.GetName(0, true));
            else if (playerIndex > 0)
                playerMessage = message.Replace(PLAYER, player.GetName(0, false));
            else
                playerMessage = message;

            return playerMessage;
        }
	}
}
