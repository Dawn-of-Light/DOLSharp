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
    public interface IQuestAction
    {
        /// <summary>
        /// Action performed 
        /// Can be used in subclasses to define special behaviour of actions
        /// </summary>
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>        
        void Perform(DOLEvent e, object sender, EventArgs args, GamePlayer player);
    }
}
