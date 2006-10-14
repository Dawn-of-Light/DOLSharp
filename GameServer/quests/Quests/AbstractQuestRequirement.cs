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
    public abstract class AbstractQuestRequirement : IQuestRequirement
    {
        private eRequirementType type;
        private Object n;
        private Object v;
        private eComparator comparator;
        private BaseQuestPart questPart;

        /// <summary>
        /// QuestPart of requirement
        /// </summary>
        public BaseQuestPart QuestPart
        {
            get { return questPart; }
        }

        /// <summary>
        /// R: RequirmentType
        /// </summary>
        public eRequirementType RequirementType
        {
            get { return type; }
        }
        /// <summary>
        /// N: first Requirment Variable
        /// </summary>
        public Object N
        {
            get { return n; }
        }
        /// <summary>
        /// V: Secoond Requirmenet Variable
        /// </summary>
        public Object V
        {
            get { return v; }
        }
        /// <summary>
        /// C: Requirement Comparator
        /// </summary>
        public eComparator Comparator
        {
            get { return comparator; }
        }

        /// <summary>
        /// returns the NPC of the requirement
        /// </summary>
        public GameNPC NPC
        {
            get { return QuestPart.NPC; }
        }

        /// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="type">RequirementType</param>
        /// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="comp">Comparator used if some values are veeing compared</param>
        public AbstractQuestRequirement(BaseQuestPart questPart, eRequirementType type, Object n, Object v, eComparator comp)
        {
            this.questPart = questPart;
            this.type = type;
            this.n = n;
            this.v = v;
            this.comparator = comp;
        }

        public abstract bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);
    }
}
