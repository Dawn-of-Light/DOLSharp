/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;
using System.Collections;
using DOL.Database2;
using DOL.Events;
namespace DOL.GS
{
    public class ChampSpecMgr
    {
        #region list / sorter
        public static ArrayList ChampSpecs = new ArrayList();
        public class Sorter : IComparer
        {
            //Lohx add - for sorting arraylist ascending
            int IComparer.Compare(object x, object y)
            {
                ChampSpec spec1 = (ChampSpec)x;
                ChampSpec spec2 = (ChampSpec)y;
                return spec1.Index.CompareTo(spec2.Index);
            }
        }
        #endregion

        #region load champspecs
        [ScriptLoadedEvent]
        public static void OnScriptCompiled(DOLEvent e, object sender, EventArgs args)
        {
            foreach (DBChampSpecs spec in GameServer.Database.SelectObjects(typeof(DBChampSpecs)))
            {
                ChampSpec newspec = new ChampSpec(spec.IdLine, spec.SkillIndex, spec.Index, spec.Cost, spec.SpellID);
                ChampSpecs.Add(newspec);
            }

        }
        #endregion

        public static ChampSpec GetAbilityFromIndex(int idline, int row, int index)
        {
            IList specs = ChampSpecMgr.GetAbilityForIndex(idline, row);
            foreach (ChampSpec spec in specs)
            {
                if (spec.IdLine == idline && spec.SkillIndex == row && spec.Index == index)
                {
                    return spec;
                }
            }
            return null;
        }

        #region ability for index
        public static IList GetAbilityForIndex(int idline, int skillindex)
        {
            ArrayList list = new ArrayList();

            foreach (ChampSpec spec in ChampSpecs)
            {
                if (spec.IdLine == idline && spec.SkillIndex == skillindex)
                {
                    list.Add(spec);
                    list.Sort(new Sorter());
                }
            }
            return list;
        }
        #endregion
    }

    #region ChampSpec class
    public class ChampSpec 
    {
        public ChampSpec(int idline, int skillindex, int index, int cost, int spellid)
        {
            m_idline = idline;
            m_skillIndex = skillindex;
            m_index = index;
            m_cost = cost;
            m_spellid = spellid;
        }
        protected int m_idline;
        public int IdLine
        {
            get { return m_idline; }
            set { m_idline = value; }
        }
        protected int m_spellid;
        public int SpellID
        {
            get { return m_spellid; }
            set { m_spellid = value; }
        }
        protected int m_cost;
        public int Cost
        {
            get { return m_cost; }
            set { m_cost = value; }
        }
        protected int m_index;
        public int Index
        {
            get { return m_index; }
            set { m_index = value; }
        }
        protected int m_skillIndex;
        public int SkillIndex
        {
            get { return m_skillIndex; }
            set { m_skillIndex = value; }
        }

    }
    #endregion

}
