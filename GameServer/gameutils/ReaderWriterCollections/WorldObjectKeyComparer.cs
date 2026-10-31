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
using System.Collections.Generic;

namespace DOL.GS
{
    // Tuple's general-purpose comparison boxes ushort components. Cache keys
    // have exactly two numeric components, so compare and hash them directly.
    internal sealed class WorldObjectKeyComparer : IEqualityComparer<Tuple<ushort, ushort>>
    {
        internal static readonly WorldObjectKeyComparer Instance = new WorldObjectKeyComparer();
        private WorldObjectKeyComparer() { }

        public bool Equals(Tuple<ushort, ushort> left, Tuple<ushort, ushort> right)
        {
            if (ReferenceEquals(left, right)) return true;
            return left != null && right != null && left.Item1 == right.Item1 && left.Item2 == right.Item2;
        }

        public int GetHashCode(Tuple<ushort, ushort> key) =>
            key == null ? 0 : (key.Item1 << 16) | key.Item2;
    }
}
