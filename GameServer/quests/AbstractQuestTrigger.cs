using System;
using System.Text;
using DOL.Events;

namespace DOL.GS.Quests
{
    /// <summary>
    /// A trigger defines the circumstances under which a certain QuestAction is fired.
    /// This can be eTriggerAction.Interact, eTriggerAction.GiveItem, eTriggerAction.Attack, etc...
    /// Additional there are two variables to add the needed parameters for the triggertype (Item to give for GiveItem, NPC to interact for Interact, etc...). To fire a QuestAction at least one of the added triggers must be fulfilled. 
    /// </summary>        
    public abstract class AbstractQuestTrigger : IQuestTrigger
    {

        private String k; //trigger keyword 
        private eTriggerType triggerType; // t## : trigger type, see following description (NONE:no trigger)
        private object i;
        private BaseQuestPart questPart;
        
        /// <summary>
        /// QuestPart of trigger
        /// </summary>
        public BaseQuestPart QuestPart
        {
            get { return questPart; }
        }

        /// <summary>
        /// Trigger Keyword
        /// </summary>
        public String K
        {
            get { return k; }
        }

        /// <summary>
        /// Trigger Variable
        /// </summary>
        public object I
        {
            get { return i; }
        }

        /// <summary>
        /// Triggertype
        /// </summary>
        public eTriggerType TriggerType
        {
            get { return triggerType; }
        }

        /// <summary>
        /// returns the NPC of the trigger
        /// </summary>
        public GameNPC NPC
        {
            get { return QuestPart.NPC; }
        }

        /// <summary>
        /// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
        /// </summary>
        /// <param name="type">Triggertype</param>
        /// <param name="keyword">keyword (K), meaning depends on triggertype</param>
        /// <param name="var">variable (I), meaning depends on triggertype</param>
        public AbstractQuestTrigger(BaseQuestPart questPart,eTriggerType type, String keyword, object var)
        {
            this.questPart = questPart;
            this.triggerType = type;
            this.i = var;
            this.k = keyword;
            
        }

        /// <summary>
        /// Checks the trigger, this method is called whenever a event associated with this questparts quest
        /// or a manualy associated eventhandler is notified.
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if QuestPart should be executes, else false</returns>
        public abstract bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);

        public abstract void Register();
        
        public abstract void Unregister();
    }

}
