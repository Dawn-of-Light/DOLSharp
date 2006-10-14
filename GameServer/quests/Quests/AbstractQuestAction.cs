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
        private BaseQuestPart questPart;

        /// <summary>
        /// QuestPart of action
        /// </summary>
        public BaseQuestPart QuestPart
        {
            get { return questPart; }
        }
        /// <summary>
        /// a#: action type
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
        }
        /// <summary>
        /// Second Action Variable
        /// </summary>
        public Object Q
        {
            get { return q; }
        }

        /// <summary>
        /// returns the NPC of the action
        /// </summary>
        public GameNPC NPC
        {
            get { return QuestPart.NPC; }
        }

        public AbstractQuestAction(BaseQuestPart questPart, eActionType actionType, Object p, Object q)
        {
            this.questPart = questPart;
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
