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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="ChampSpecs")]
	public class DBChampSpecs : DataObject
	{
		protected int m_idLine;
        protected int m_skillIndex;
        protected int m_index;
        protected ushort m_spellid;
        protected int m_order;
        protected int m_cost;

		static bool	m_autoSave;

        public DBChampSpecs()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get {	return m_autoSave;	}
			set	{
				m_autoSave = value;
			}
		}
        // how much spell costs
        [DataElement(AllowDbNull = false)]
        public int Cost
        {
            get { return m_cost; }
            set
            {
                Dirty = true;
                m_cost = value;
            }
        }
        // MAIN identifying number, first tier and all spells that are on same training window share this
        [DataElement(AllowDbNull = false)]
        public int IdLine
        {
            get { return m_idLine; }
            set
            {
                Dirty = true;
                m_idLine = value;
            }
        }
        // SECOND tier, this is a row and all spells that share this number + idline build off each other
        [DataElement(AllowDbNull = false)]
        public int SkillIndex
        {
            get { return m_skillIndex; }
            set
            {
                Dirty = true;
                m_skillIndex = value;
            }
        }
        // THIRD tier, identifier for ordering in ascending order 
        [DataElement(AllowDbNull = false)]
        public int Index
        {
            get { return m_index; }
            set
            {
                Dirty = true;
                m_index = value;
            }
        }
        // SPELL ID from DBSpell that the player is buying
        [DataElement(AllowDbNull = false)]
        public ushort SpellID
        {
            get { return m_spellid; }
            set
            {
                Dirty = true;
                m_spellid = value;
            }
        }
	}
}