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
using log4net;
using System.Reflection;

namespace DOL.GS
{
    /// <summary>
    /// Djinn stone base class.
    /// </summary>
    /// <author>Aredhel</author>
    public class DjinnStone : GameStaticItem
    {
        private AncientBoundDjinn m_djinn;
        
        /// <summary>
        /// The djinn bound to this stone.
        /// </summary>
        protected AncientBoundDjinn Djinn
        {
            get { return m_djinn;  }
            set { m_djinn = value; }
        }
    }
}
