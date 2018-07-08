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
using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// Helper class for region registration
    /// </summary>
    public class RegionData : IComparable
    {
        /// <summary>
        /// The region id
        /// </summary>
        public ushort Id { get; set; }
        /// <summary>
        /// The region name
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// The region description
        /// </summary>
        public string Description { get; set; }
        /// <summary>
        /// The region IP
        /// </summary>
        public string Ip { get; set; }
        /// <summary>
        /// The region port
        /// </summary>
        public ushort Port { get; set; }
        /// <summary>
        /// The region water level
        /// </summary>
        public int WaterLevel { get; set; }
        /// <summary>
        /// The region diving flag
        /// </summary>
        public bool DivingEnabled { get; set; }
        /// <summary>
        /// The region housing flag
        /// </summary>
        public bool HousingEnabled { get; set; }
        /// <summary>
        /// The region expansion
        /// </summary>
        public int Expansion { get; set; }
        /// <summary>
        /// The region mobs
        /// </summary>
        public Mob[] Mobs { get; set; }
        /// <summary>
        /// The class type of this region, blank for default
        /// </summary>
        public string ClassType { get; set; }
        /// <summary>
        /// Should this region be treated as part of the Frontier?
        /// </summary>
        public bool IsFrontier { get; set; }

        /// <summary>
        /// Compares 2 objects
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (!(obj is RegionData cmp))
            {
                return -1;
            }

            return cmp.Mobs.Length - Mobs.Length;
        }
    }
}
