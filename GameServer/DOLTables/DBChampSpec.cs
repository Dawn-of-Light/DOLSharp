/* FROM LOHX
 * 
 * tier system works like so : 
   IDLINE 1:
       SkillIndex 1:
               Index 1
               Index 2
               Index 3
               Index 4
               Index 5
       SkillIndex 2:
               Index 1
               Index 2
               Index 3
               Index 4
               Index 5
  IdLine 2:
       SkillIndex 1:
           ect ect ect ect ect ect
 */
using System;


namespace DOL.Database2
{
	/// <summary>
	/// 
	/// </summary>
	[Serializable]//TableName="ChampSpecs")]
	public class DBChampSpecs : DatabaseObject
	{
		protected int m_idLine;
        protected int m_skillIndex;
        protected int m_index;
        protected int m_spellid;
        protected int m_order;
        protected int m_cost;

		static bool	m_autoSave;

        public DBChampSpecs()
            : base()
		{
			m_autoSave = false;
		}

        // how much spell costs
        
        public int Cost
        {
            get { return m_cost; }
            set
            {
                m_Dirty = true;
                m_cost = value;
            }
        }
        // MAIN identifying number, first tier and all spells that are on same training window share this
        
        public int IdLine
        {
            get { return m_idLine; }
            set
            {
                m_Dirty = true;
                m_idLine = value;
            }
        }
        // SECOND tier, this is a row and all spells that share this number + idline build off each other
        
        public int SkillIndex
        {
            get { return m_skillIndex; }
            set
            {
                m_Dirty = true;
                m_skillIndex = value;
            }
        }
        // THIRD tier, identifier for ordering in ascending order 
        
        public int Index
        {
            get { return m_index; }
            set
            {
                m_Dirty = true;
                m_index = value;
            }
        }
        // SPELL ID from DBSpell that the player is buying
        
        public int SpellID
        {
            get { return m_spellid; }
            set
            {
                m_Dirty = true;
                m_spellid = value;
            }
        }
	}
}