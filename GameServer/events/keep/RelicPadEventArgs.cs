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
    /// Holds the arguments for the RelicPad event
    /// </summary>
    public class RelicPadEventArgs : EventArgs
    {

        /// <summary>
        /// The player
        /// </summary>
        private GamePlayer m_player;

        /// <summary>
        /// The player
        /// </summary>
        private GameRelic m_relic;

        /// <summary>
        /// Constructs a new KeepEventArgs
        /// </summary>
        public RelicPadEventArgs(GamePlayer player, GameRelic relic)
        {
            m_player = player;
            m_relic = relic;
        }

        /// <summary>
        /// Gets the player
        /// </summary>
        public GamePlayer Player
        {
            get { return m_player; }
        }

        /// <summary>
        /// Gets the player
        /// </summary>
        public GameRelic Relic
        {
            get { return m_relic; }
        }
    }
}