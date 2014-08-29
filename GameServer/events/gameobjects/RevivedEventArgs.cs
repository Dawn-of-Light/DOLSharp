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

using DOL.GS;
using DOL.GS.Spells;

namespace DOL.Events
{
    /// <summary>
    /// Holds the arguments for the Revived event of GamePlayer
    /// </summary>
    public class RevivedEventArgs : EventArgs
    {

        /// <summary>
        /// The source of revive (rezzer) or null
        /// </summary>
        private GameObject m_source;

        /// <summary>
        /// The spell if one used, else null
        /// </summary>
        private ISpellHandler m_spellHandler;

        /// <summary>
        /// Reduction of Illness in percent
        /// </summary>
        private byte m_illnessReduction;
        
        /// <summary>
        /// Constructs a new Revived event args
        /// </summary>
        public RevivedEventArgs(GameObject source, ISpellHandler spellHandler, byte illnessReduction)
        {
            m_source = source;
            m_spellHandler = spellHandler;
            m_illnessReduction = illnessReduction;
        }

        /// <summary>
        /// The source of revive (rezzer) or null
        /// </summary>
        public GameObject Source
        {
            get { return m_source; }
        }

        /// <summary>
        /// The spell if one used, else null
        /// </summary>
        public ISpellHandler SpellHandler
        {
            get { return m_spellHandler; }
        }
        
        /// <summary>
        /// Reduction of Illness in percent
        /// </summary>
        public byte IllnessReduction
        {
        	get { return m_illnessReduction; }
        }

    }
}
