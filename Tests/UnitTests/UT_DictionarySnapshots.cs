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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DOL.GS;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_DictionarySnapshots
    {
        const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
        static IDisposable Rent(ReaderWriterDictionary<int, object> dictionary) =>
            (IDisposable)dictionary.GetType().GetMethod("RentSnapshot", Hidden).Invoke(dictionary, null);
        static KeyValuePair<int, object>[] Entries(IDisposable snapshot)
        {
            int count = (int)snapshot.GetType().GetProperty("Count", Hidden).GetValue(snapshot);
            var indexer = snapshot.GetType().GetProperty("Item", Hidden);
            return Enumerable.Range(0, count).Select(i => (KeyValuePair<int, object>)indexer.GetValue(snapshot, new object[] { i })).ToArray();
        }
        [Test]
        public void WritersCanChangeCacheWithoutChangingOutstandingSnapshot()
        {
            var first = new object(); var second = new object();
            var dictionary = new ReaderWriterDictionary<int, object> { { 1, first }, { 2, second } };
            using var original = Rent(dictionary);
            Task.Run(() => { dictionary[1] = second; dictionary.Remove(2); dictionary.Add(3, first); }).GetAwaiter().GetResult();
            Assert.That(Entries(original), Is.EqualTo(new[] { new KeyValuePair<int, object>(1, first), new KeyValuePair<int, object>(2, second) }));
            using var current = Rent(dictionary);
            Assert.That(Entries(current), Is.EqualTo(dictionary.ToArray()));
            dictionary.Clear();
            using var empty = Rent(dictionary);
            Assert.That(Entries(empty), Is.Empty);
        }
        [Test]
        public void ConcurrentRentalsAndRepeatedDisposalKeepOutstandingSnapshotsIntact()
        {
            var value = new object();
            var dictionary = new ReaderWriterDictionary<int, object> { { 1, value } };
            using var retained = Rent(dictionary);
            Parallel.For(0, 256, i =>
            {
                var own = new ReaderWriterDictionary<int, object> { { i, value } };
                using var snapshot = Rent(own);
                Assert.That(Entries(snapshot).Single().Key, Is.EqualTo(i));
                snapshot.Dispose();
                snapshot.Dispose();
            });
            Assert.That(Entries(retained).Single().Value, Is.SameAs(value));
        }
    }
}
