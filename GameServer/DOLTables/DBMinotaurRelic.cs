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
    /// DB relic is database of relic
    /// </summary>
    [Serializable]//TableName = "Minotaurrelic", PreCache = true)]
    public class DBMinotaurRelic : DatabaseObject
    {
        static bool m_autoSave;
        private string m_Name;
        private int m_relicID;
        private ushort m_Model;
        private int m_spawny;
        private int m_spawnx;
        private int m_spawnz;
        private int m_spawnheading;
        private int m_spawnregion;
        private string m_relictarget;
        private int m_spell;
        private int m_effect;

        /// <summary>
        /// Create a relic row
        /// </summary>
        public DBMinotaurRelic()
            : base()
        {
            m_autoSave = true;
        }

        /// <summary>
        /// relic type, 0 is melee, 1 is magic
        /// </summary>
        
        public int relicSpell
        {
            get { return m_spell; }
            set
            {
                m_Dirty = true;
                m_spell = value;
            }
        }

        
        public string relicTarget
        {
            get { return m_relictarget; }
            set 
            {
                m_Dirty = true; 
                m_relictarget = value;
            }
        }

        
        public string Name
        {
            get { return m_Name; }
            set 
            {
                m_Dirty = true; 
                m_Name = value;
            }
        }

        
        public ushort Model
        {
            get { return m_Model; }
            set 
            {
                m_Dirty = true; 
                m_Model = value;
            }
        }

        
        public int SpawnX
        {
            get { return m_spawnx; }
            set 
            {
                m_Dirty = true; 
                m_spawnx = value;
            }
        }

        
        public int SpawnY
        {
            get { return m_spawny; }
            set
            {
                m_Dirty = true;
                m_spawny = value;
            }
        }

        
        public int SpawnZ
        {
            get { return m_spawnz; }
            set
            {
                m_Dirty = true;
                m_spawnz = value;
            }
        }

        
        public int SpawnHeading
        {
            get { return m_spawnheading; }
            set
            {
                m_Dirty = true;
                m_spawnheading = value;
            }
        }

        
        public int SpawnRegion
        {
            get { return m_spawnregion; }
            set
            {
                m_Dirty = true;
                m_spawnregion = value;
            }
        }

        
        public int Effect
        {
            get { return m_effect; }
            set
            {
                m_Dirty = true;
                m_effect = value;
            }
        }

        /// <summary>
        /// Index of relic
        /// </summary>
        ////[PrimaryKey]
        public int RelicID
        {
            get { return m_relicID; }
            set
            {
                m_Dirty = true;
                m_relicID = value;
            }
        }
    }
}
