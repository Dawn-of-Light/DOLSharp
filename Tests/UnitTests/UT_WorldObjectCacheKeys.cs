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
using System.Threading.Tasks;
using DOL.GS;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture]
    public class UT_WorldObjectCacheKeys
    {
        [TestCase(false)]
        [TestCase(true)]
        public void IndependentlyConstructedKeysPreserveRegionAndObjectIdentity(bool housing)
        {
            var client = new GameClient(null);
            var cache = housing ? client.HouseUpdateArray : client.GameObjectUpdateArray;
            var pairs = new[] { Tuple.Create((ushort)0,(ushort)0), Tuple.Create(ushort.MaxValue,ushort.MaxValue),
                Tuple.Create((ushort)0,ushort.MaxValue), Tuple.Create(ushort.MaxValue,(ushort)0),
                Tuple.Create((ushort)1,(ushort)7), Tuple.Create((ushort)2,(ushort)7), Tuple.Create((ushort)1,(ushort)8) };
            for (int i=0;i<pairs.Length;i++) cache.Add(pairs[i],i);
            for (int i=0;i<pairs.Length;i++)
            {
                var key=Tuple.Create(pairs[i].Item1,pairs[i].Item2);
                Assert.That(cache[key],Is.EqualTo(i));
                Assert.That(cache.AddIfNotExists(key,999),Is.False);
                Assert.That(cache.UpdateIfExists(key,i+100),Is.True);
                Assert.That(cache.TryGetValue(key,out long value),Is.True);
                Assert.That(value,Is.EqualTo(i+100));
            }
            Assert.That(cache.Count,Is.EqualTo(pairs.Length));
            foreach(var key in pairs)
                Assert.That(cache.TryRemove(Tuple.Create(key.Item1,key.Item2),out _),Is.True);
            Assert.That(cache.Count,Is.Zero);
        }
        [Test]
        public void MixedRegionOperationsMatchDefaultTupleDictionary()
        {
            var cache=new GameClient(null).GameObjectUpdateArray;
            var expected=new Dictionary<Tuple<ushort,ushort>,long>();
            var random=new Random(3829);
            for(int i=0;i<4000;i++)
            {
                var key=Tuple.Create((ushort)random.Next(0,8),(ushort)random.Next(0,256));
                if(i%3==0)
                    Assert.That(cache.TryRemove(key,out _),Is.EqualTo(expected.Remove(key)));
                else
                {
                    cache[key]=i;
                    expected[key]=i;
                }
                bool found=expected.TryGetValue(key,out long value);
                Assert.That(cache.TryGetValue(Tuple.Create(key.Item1,key.Item2),out long actual),Is.EqualTo(found));
                Assert.That(actual,Is.EqualTo(value));
            }
            Assert.That(cache.ToArray(),Is.EquivalentTo(expected.ToArray()));
        }
        [Test]
        public void ConcurrentClientsAndRegionsDoNotAliasCacheEntries()
        {
            var clients=Enumerable.Range(0,8).Select(_=>new GameClient(null)).ToArray();
            Parallel.For(0,8,region=>
            {
                foreach(var client in clients)
                    for(int id=1;id<=128;id++)
                        client.GameObjectUpdateArray[Tuple.Create((ushort)region,(ushort)id)]=region*1000+id;
            });
            foreach(var client in clients)
            {
                Assert.That(client.GameObjectUpdateArray.Count,Is.EqualTo(1024));
                for(int region=0;region<8;region++)
                    for(int id=1;id<=128;id++)
                        Assert.That(client.GameObjectUpdateArray[Tuple.Create((ushort)region,(ushort)id)],Is.EqualTo(region*1000+id));
            }
        }
    }
}
