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

        private object k; //trigger keyword 
        private eTriggerType triggerType; // t## : trigger type, see following description (NONE:no trigger)
        private object i;        
		private GameLiving defaultNPC;
		private DOLEventHandler notifyHandler;
		private Type questType;

        /// <summary>
        /// Trigger Keyword
        /// </summary>
        public Object K
        {
            get { return k; }
			set { k = value; }
        }

        /// <summary>
        /// Trigger Variable
        /// </summary>
        public object I
        {
            get { return i; }
			set { i = value; }
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
        public GameLiving NPC
        {
            get { return defaultNPC; }
        }

		public DOLEventHandler NotifyHandler
		{
			get { return notifyHandler; }
		}

		public Type QuestType
		{
			get { return questType; }
		}

    	/// <summary>
    	/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
    	/// </summary>
		/// <param name="questPart"></param>
    	/// <param name="type">Triggertype</param>
    	/// <param name="keyword">keyword (K), meaning depends on triggertype</param>
    	/// <param name="var">variable (I), meaning depends on triggertype</param>
    	public AbstractQuestTrigger(BaseQuestPart questPart,eTriggerType type, String keyword, object var)
    	{
    		this.defaultNPC = questPart.NPC;
			this.notifyHandler = new DOLEventHandler(questPart.Notify);
			this.questType = questPart.QuestType;
    		this.triggerType = type;
    		this.i = var;
    		this.k = keyword;
    	}

		/// <summary>
		/// Creates a new questtrigger and does some simple triggertype parameter compatibility checking
		/// </summary>
		/// <param name="defaultNPC"></param>
		/// <param name="notifyHandler"></param>
		/// <param name="type">Triggertype</param>
		/// <param name="keyword">keyword (K), meaning depends on triggertype</param>
		/// <param name="var">variable (I), meaning depends on triggertype</param>
		public AbstractQuestTrigger(GameLiving defaultNPC,DOLEventHandler notifyHandler, eTriggerType type, String keyword, object var)
		{
			this.defaultNPC = defaultNPC;
			this.notifyHandler = notifyHandler;
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

		/// <summary>
		/// Registers the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// This method will be called multiple times, so use AddHandlerUnique to make
		/// sure only one handler is actually registered
		/// </remarks>
        public abstract void Register();

		/// <summary>
		/// Unregisters the needed EventHandler for this Trigger
		/// </summary>
		/// <remarks>
		/// Don't remove handlers that will be used by other triggers etc.
		/// This is rather difficult since we don't know which events other triggers use.
		/// </remarks>
        public abstract void Unregister();
    }

}
