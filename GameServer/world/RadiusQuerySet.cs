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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace DOL.GS
{
    // A query owns its set until all zones have been visited. Never keep game-object
    // references between queries, or share an active set with concurrent/nested queries.
    internal sealed class RadiusQuerySet : System.IDisposable
    {
        private const int MaxRetainedSets = 32;
        private static readonly ConcurrentBag<HashSet<object>> Pool = new ConcurrentBag<HashSet<object>>();
        private static int retainedSets;
        private HashSet<object> seen;

        internal RadiusQuerySet()
        {
            if (Pool.TryTake(out seen))
                Interlocked.Decrement(ref retainedSets);
            else
                seen = new HashSet<object>();
        }

        internal HashSet<object> Seen => seen;

        public void Dispose()
        {
            var completed = Interlocked.Exchange(ref seen, null);
            if (completed == null) return;
            completed.Clear();
            if (Interlocked.Increment(ref retainedSets) <= MaxRetainedSets)
                Pool.Add(completed);
            else
                Interlocked.Decrement(ref retainedSets);
        }
    }
}
