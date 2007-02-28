using System;
using System.Text;
using DOL.Events;

namespace DOL.GS.Quests
{
    /// <summary>
    /// If one trigger and all requirements are fulfilled the corresponding actions of
    /// a QuestAction will we executed one after another. Actions can be more or less anything:
    /// at the moment there are: GiveItem, TakeItem, Talk, Give Quest, Increase Quest Step, FinishQuest,
    /// etc....
    /// </summary>
    public abstract class AbstractQuestAction : IQuestAction
    {

        private eActionType actionType;
        private Object q;
        private Object p;
		private GameNPC defaultNPC;
		private Type questType;
        
        /// <summary>
        /// The action type
        /// </summary>
        public eActionType ActionType
        {
            get { return actionType; }
        }
        /// <summary>
        /// First Action Variable
        /// </summary>
        public Object P
        {
            get { return p; }
			set { p = value; }
        }
        /// <summary>
        /// Second Action Variable
        /// </summary>
        public Object Q
        {
            get { return q; }
			set { q = value; }
        }

		/// <summary>
		/// returns the NPC of the action
		/// </summary>
		public GameNPC NPC
		{
			get { return defaultNPC; }
		}

		/// <summary>
		/// Gets the type of the quest.
		/// </summary>
		/// <value>The type of the quest.</value>
		public Type QuestType
		{
			get { return questType; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractQuestAction"/> class.
		/// </summary>
		/// <param name="questPart">The quest part.</param>
		/// <param name="actionType">Type of the action.</param>
		/// <param name="p">The parameter p.</param>
		/// <param name="q">The parameter q.</param>
        public AbstractQuestAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
        {
            this.defaultNPC = questPart.NPC;
			this.questType = questPart.QuestType;
            this.actionType = actionType;
            this.q = q;
            this.p = p;
        }

		/// <summary>
		/// Initializes a new instance of the <see cref="AbstractQuestAction"/> class.
		/// </summary>
		/// <param name="npc">The default NPC.</param>
		/// <param name="questType">Default Type of the quest.</param>
		/// <param name="actionType">Type of the action.</param>
		/// <param name="p">The parameter p.</param>
		/// <param name="q">The parameter q.</param>
		public AbstractQuestAction(GameNPC npc,Type questType, eActionType actionType, Object p, Object q)
		{
			this.defaultNPC = npc;
			this.questType = questType;
			this.actionType = actionType;
			this.q = q;
			this.p = p;
		}

        /// <summary>
        /// Action performed 
        /// Can be used in subclasses to define special behaviour of actions
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>        
        public abstract void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player);        
    }
}
