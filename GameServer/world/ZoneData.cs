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

namespace DOL.GS
{
    // Dinberg: Added this for instances as we dont want to have to parse XML every time we create an instance,
    // but we need to put zones into the instance.

    /// <summary>
    /// Holds the information of a child of the zone config file, that can be used later for instance creation.
    /// </summary>
    public class ZoneData
    {
        public ushort ZoneID { get; set; }

        public ushort RegionID { get; set; }

        public byte OffX { get; set; }

        public byte OffY { get; set; }

        public byte Height { get; set; }

        public byte Width { get; set; }

        public string Description { get; set; }

        public byte DivingFlag { get; set; }

        public int WaterLevel { get; set; }

        public bool IsLava { get; set; }
    }
}
