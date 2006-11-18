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
		private GameNPC defaultNPC;
		private Type questType;

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
			set { n = value; }
        }
        /// <summary>
        /// V: Secoond Requirmenet Variable
        /// </summary>
        public Object V
        {
            get { return v; }
			set { v = value; }
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
            get { return defaultNPC; }
        }

		public Type QuestType
		{
			get { return questType; }
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
            this.defaultNPC = questPart.NPC;
			this.questType = questPart.QuestType;
            this.type = type;
            this.n = n;
            this.v = v;
            this.comparator = comp;
        }

		/// <summary>
        /// Creates a new QuestRequirement and does some basich compativilite checks for the parameters
        /// </summary>
        /// <param name="type">RequirementType</param>
        /// <param name="n">First Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="v">Second Requirement Variable, meaning depends on RequirementType</param>
        /// <param name="comp">Comparator used if some values are veeing compared</param>
        public AbstractQuestRequirement(GameNPC defaultNPC, Type questType, eRequirementType type, Object n, Object v, eComparator comp)
        {
            this.defaultNPC = defaultNPC;
			this.questType = questType;
            this.type = type;
            this.n = n;
            this.v = v;
            this.comparator = comp;
        }

        public abstract bool Check(DOLEvent e, object sender, EventArgs args, GamePlayer player);
    }
}
