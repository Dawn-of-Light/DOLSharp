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
namespace DOL.Events
{
    /// <summary>
    /// Holds the arguments for the Dying event of GameLivings
    /// </summary>
    public class HealthChangedEventArgs : EventArgs
    {

        /// <summary>
        /// The source of changing
        /// </summary>
        private GameObject m_changesource;

        /// <summary>
        /// The type of changing
        /// </summary>
        private GameLiving.eHealthChangeType m_changetype;


        /// <summary>
        /// The amount of changing
        /// </summary>
        private int m_changeamount;

        /// <summary>
        /// Constructs a new Dying event args
        /// </summary>
        public HealthChangedEventArgs(GameObject source, GameLiving.eHealthChangeType type, int amount)
        {
            m_changesource = source;
            m_changetype = type;
            m_changeamount = amount;
        }

        public GameObject ChangeSource
        {
            get { return m_changesource; }
        }

        public GameLiving.eHealthChangeType ChangeType
        {
            get { return m_changetype; }
        }

        public int ChangeAmount
        {
            get { return m_changeamount; }
        }
    }
}