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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using DOL.Database;
using DOL.GS;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture, NonParallelizable]
    public class UT_WorldPlayerUpdates
    {
        const ushort RegionId = 64999;
        class TestRegion : Region
        {
            public TestRegion(GameObject[] objects) : base(null, new RegionData { Id = RegionId }) { m_objects = objects; }
        }
        class TestPlayer : GamePlayer
        {
            public bool Visible = true, Stealthed;
            public IEnumerable Neighbors = Array.Empty<GamePlayer>();
            public TestPlayer(GameClient client) : base(client, new DOLCharacters()) { }
            public override void LoadFromDatabase(DataObject obj) { }
            public override ushort CurrentRegionID { get => RegionId; set { } }
            public override bool IsVisibleTo(GameObject observer) => Visible;
            public override bool IsStealthed => Stealthed;
            public override bool CanDetect(GamePlayer other) => false;
            public override IEnumerable GetPlayersInRadius(ushort radius) => Neighbors;
        }
        [Test]
        public void CachedVisiblePlayersKeepOrderAndRecentUpdateInterval()
        {
            var client = new GameClient(null);
            var sent = new List<GamePlayer>();
            client.Out = new TestPacketLib { SendPlayerForgedPositionMethod = (_, player) => sent.Add(player) };
            var observer = new TestPlayer(client);
            var players = Enumerable.Range(0, 1000).Select(i => new TestPlayer(null) { ObjectID = i+1 }).ToArray();
            observer.Neighbors = players.Reverse().ToArray();
            var region = new TestRegion(players.Cast<GameObject>().ToArray());
            const long now = 1000000;
            foreach (var p in players) client.GameObjectUpdateArray.Add(Tuple.Create(RegionId,(ushort)p.ObjectID), p.ObjectID == 500 ? now : 0);
            WithRegion(region, () => Update(observer, now));
            Assert.That(sent, Is.EqualTo(players.Reverse().Where(p => p.ObjectID != 500).ToArray()));
            Assert.That(client.GameObjectUpdateArray.Count, Is.EqualTo(1000));
        }
        [Test]
        public void OutOfRangeCacheIsRemovedWithoutLeakingHiddenOrStealthedPlayers()
        {
            var client = new GameClient(null);
            var sent = new List<GamePlayer>();
            client.Out = new TestPacketLib { SendPlayerForgedPositionMethod = (_, player) => sent.Add(player) };
            var observer = new TestPlayer(client);
            var visible = new TestPlayer(null) { ObjectID=1 };
            var hidden = new TestPlayer(null) { ObjectID=2, Visible=false };
            var stealth = new TestPlayer(null) { ObjectID=3, Stealthed=true };
            var departed = new TestPlayer(null) { ObjectID=4 };
            observer.Neighbors = new[] { visible, hidden, stealth };
            var objects = new GameObject[] { visible,hidden,stealth,departed };
            foreach (var p in objects) client.GameObjectUpdateArray.Add(Tuple.Create(RegionId,(ushort)p.ObjectID),0);
            WithRegion(new TestRegion(objects), () => Update(observer,1000000));
            Assert.That(sent, Is.EqualTo(new GamePlayer[] { departed,visible }));
            Assert.That(client.GameObjectUpdateArray.Keys.Select(k=>k.Item2), Is.EquivalentTo(new ushort[] {1}));
        }
        static void Update(GamePlayer observer,long now) => typeof(WorldUpdateThread).GetMethod("UpdatePlayerOtherPlayers",BindingFlags.NonPublic|BindingFlags.Static).Invoke(null,new object[] {observer,now});
        static void WithRegion(Region region,Action action)
        {
            Assert.That(WorldMgr.Regions.ContainsKey(RegionId),Is.False);
            WorldMgr.Regions.Add(RegionId,region);
            try { action(); } finally { WorldMgr.Regions.Remove(RegionId); }
        }
    }
}
