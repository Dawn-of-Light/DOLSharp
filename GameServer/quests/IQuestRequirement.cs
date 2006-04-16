using System;
using System.Text;
using DOL.Events;

namespace DOL.GS.Quests
{
    /// <summary>
    /// Requirements describe what must be true to allow a QuestAction to fire.
    /// Level of player, Step of Quest, Class of Player, etc... There are also some variables to add
    /// additional parameters. To fire a QuestAction ALL requirements must be fulfilled.         
    /// </summary>
    public interface IQuestRequirement
    {
        /// <summary>
        /// Checks the requirement whenever a trigger associated with this questpart fires.(returns true)
        /// </summary>        
        /// <param name="e">DolEvent of notify call</param>
        /// <param name="sender">Sender of notify call</param>
        /// <param name="args">EventArgs of notify call</param>
        /// <param name="player">GamePlayer this call is related to, can be null</param>
        /// <returns>true if all Requirements forQuestPart where fullfilled, else false</returns>
        bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);
    }
}
