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


namespace DOL.Database2
{
    /// <summary>
    /// (procs) Database Storage of StyleXSpell
    /// </summary>
    [Serializable]//TableName = "StyleXSpell")]
    public class DBStyleXSpell : DatabaseObject
    {
        static bool m_autoSave = false;
        protected int m_SpellID;
        protected int m_ClassID;
        protected int m_StyleID;
        protected int m_Chance;


        /// <summary>
        /// The Constructor
        /// </summary>
        public DBStyleXSpell()
            : base()
        {
        }

        /// <summary>
        /// The Spell ID
        /// </summary>
        
        public int SpellID
        {
            get { return m_SpellID; }
            set { m_SpellID = value; m_Dirty = true; }
        }

        /// <summary>
        /// The ClassID, required for style subsitute procs (0 is not a substitute style)
        /// </summary>
        
        public int ClassID
        {
            get { return m_ClassID; }
            set { m_ClassID = value; m_Dirty = true; }
        }

        /// <summary>
        /// The StyleID
        /// </summary>
        
        public int StyleID
        {
            get { return m_StyleID; }
            set { m_StyleID = value; m_Dirty = true; }
        }

        /// <summary>
        /// The Chance to add to the styleeffect list
        /// </summary>
        
        public int Chance
        {
            get { return m_Chance; }
            set { m_Chance = value; m_Dirty = true; }
        }

    }
}
