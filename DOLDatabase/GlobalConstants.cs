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

namespace DOL
{
    /// <summary>
    /// Defines the realms for various packets and search functions etc.
    /// </summary>
    public enum eRealm : byte
    {
        /// <summary>
        /// First realm number, for use in all arrays
        /// </summary>
        _First = 0,
        /// <summary>
        /// No specific realm
        /// </summary>
        None = 0,
        /// <summary>
        /// Albion Realm
        /// </summary>
        Albion = 1,
        /// <summary>
        /// Midgard Realm
        /// </summary>
        Midgard = 2,
        /// <summary>
        /// Hibernia Realm
        /// </summary>
        Hibernia = 3,
        /// <summary>
        /// Last player realm number, for use in all arrays
        /// </summary>
        _LastPlayerRealm = 3,
        /// <summary>
        /// LastRealmNumber to allow dynamic allocation of realm specific arrays.
        /// </summary>
        _Last = 6

    };

    /// <summary>
    /// priviledge of user
    /// </summary>
    public enum ePrivLevel : byte
    {
        Player = 1,
        GM = 2,
        Admin = 3,
    }
}