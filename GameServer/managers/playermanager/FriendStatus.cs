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

namespace DOL.GS.Friends
{
    /// <summary>
    /// Offline Friend Status Object to display in Social Windows
    /// </summary>
    public sealed class FriendStatus
    {
        /// <summary>
        /// Friend Name
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// Friend Level
        /// </summary>
        public int Level { get; private set; }
        /// <summary>
        /// Friend Class ID
        /// </summary>
        public int ClassID { get; private set; }
        /// <summary>
        /// Friend LastPlayed
        /// </summary>
        public DateTime LastPlayed { get; private set; }

        /// <summary>
        /// Create a new instance of <see cref="FriendStatus"/>
        /// </summary>
        /// <param name="Name">Friend Name</param>
        /// <param name="Level">Friend Level</param>
        /// <param name="ClassID">Friend Class ID</param>
        /// <param name="LastPlayed">Friend LastPlayed</param>
        public FriendStatus(string Name, int Level, int ClassID, DateTime LastPlayed)
        {
            this.Name = Name;
            this.Level = Level;
            this.ClassID = ClassID;
            this.LastPlayed = LastPlayed;
        }
    }
}
