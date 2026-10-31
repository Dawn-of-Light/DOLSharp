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
using System.Reflection;
using DOL.AI.Brain;
using DOL.Database;
using DOL.GS;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture, NonParallelizable]
    public class UT_WorldCacheUpdates
    {
        const ushort RegionId = 64994;
        const long Now = 1000000;
        class TestRegion : Region
        {
            public TestRegion(GameObject[] objects) : base(null, new RegionData { Id = RegionId }) { m_objects = objects; }
        }
        class Player : GamePlayer
        {
            public Player(GameClient client) : base(client, new DOLCharacters()) { }
            public override void LoadFromDatabase(DataObject obj) { }
            public override ushort CurrentRegionID { get => RegionId; set { } }
            public override Region CurrentRegion { get => null; set { } }
            public override bool IsVisibleTo(GameObject observer) => true;
            public override bool IsStealthed => false;
            public override IEnumerable GetPlayersInRadius(ushort radius) => Array.Empty<GamePlayer>();
        }
        class Npc : GameNPC { public override bool IsVisibleTo(GameObject observer) => true; }
        class Item : GameStaticItem { public override bool IsVisibleTo(GameObject observer) => true; }
        class Door : GameDoor { public override bool IsVisibleTo(GameObject observer) => true; }
        static void Update(string method, GamePlayer player) => typeof(WorldUpdateThread)
            .GetMethod(method, BindingFlags.NonPublic | BindingFlags.Static, null, new[] { typeof(GamePlayer), typeof(long) }, null)
            .CreateDelegate<Action<GamePlayer, long>>()(player, Now);
        static void WithRegion(GameObject[] objects, Action action)
        {
            Assert.That(WorldMgr.Regions.ContainsKey(RegionId), Is.False);
            WorldMgr.Regions.Add(RegionId, new TestRegion(objects));
            try { action(); } finally { WorldMgr.Regions.Remove(RegionId); }
        }
        [TestCase("UpdatePlayerOtherPlayers", 1000, "player")]
        [TestCase("UpdatePlayerNPCs", 8000, "npc")]
        [TestCase("UpdatePlayerItems", 30000, "item")]
        [TestCase("UpdatePlayerDoors", 30000, "door")]
        public void CleanupPreservesExactIntervalBoundary(string method, int interval, string kind)
        {
            var oldPlayerInterval = DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL;
            var oldNpcInterval = DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL;
            var oldItemInterval = DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL;
            try
            {
                DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL = 1000;
                DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL = 8000;
                DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL = 30000;
                var client = new GameClient(null) { Out = new TestPacketLib() };
                var observer = new Player(client);
                Func<GameObject> create = kind == "player" ? () => new Player(null) :
                    kind == "npc" ? () => new Npc() : kind == "item" ? () => new Item() : () => new Door();
                var aged = create(); aged.ObjectID = 1;
                var recent = create(); recent.ObjectID = 2;
                var agedKey = Tuple.Create(RegionId, (ushort)1);
                var recentKey = Tuple.Create(RegionId, (ushort)2);
                client.GameObjectUpdateArray.Add(agedKey, Now - interval);
                client.GameObjectUpdateArray.Add(recentKey, Now - interval + 1);
                WithRegion(new[] { aged, recent }, () => Update(method, observer));
                Assert.That(client.GameObjectUpdateArray.ContainsKey(agedKey), Is.False);
                Assert.That(client.GameObjectUpdateArray[recentKey], Is.EqualTo(Now - interval + 1));
            }
            finally
            {
                DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL = oldPlayerInterval;
                DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL = oldNpcInterval;
                DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL = oldItemInterval;
            }
        }
        [Test]
        public void OwnedPetIsStillExcludedFromNpcCleanup()
        {
            var client = new GameClient(null) { Out = new TestPacketLib() };
            var observer = new Player(client);
            var pet = new Npc { ObjectID = 1 };
            pet.SetOwnBrain(new ControlledNpcBrain(observer));
            var key = Tuple.Create(RegionId, (ushort)1);
            client.GameObjectUpdateArray.Add(key, 0);
            WithRegion(new GameObject[] { pet }, () => Update("UpdatePlayerNPCs", observer));
            Assert.That(client.GameObjectUpdateArray[key], Is.Zero);
        }
        [Test]
        public void LaterCleanupPassSeesEntriesAddedWhileSendingPlayerUpdates()
        {
            var oldPlayerInterval = DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL;
            var oldNpcInterval = DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL;
            var oldItemInterval = DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL;
            try
            {
                DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL = 1000;
                DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL = 8000;
                DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL = 30000;
                var client = new GameClient(null);
                var observer = new Player(client);
                var departed = new Player(null) { ObjectID = 1 };
                var item = new Item { ObjectID = 2 };
                var itemKey = Tuple.Create(RegionId, (ushort)2);
                client.GameObjectUpdateArray.Add(Tuple.Create(RegionId, (ushort)1), 0);
                int sends = 0;
                client.Out = new TestPacketLib { SendPlayerForgedPositionMethod = (_, p) =>
                {
                    sends++;
                    client.GameObjectUpdateArray.Add(itemKey, 0);
                }};
                WithRegion(new GameObject[] { departed, item }, () => Update("UpdatePlayerWorld", observer));
                Assert.That(sends, Is.EqualTo(1));
                Assert.That(client.GameObjectUpdateArray.Count, Is.Zero);
            }
            finally
            {
                DOL.GS.ServerProperties.Properties.WORLD_PLAYERTOPLAYER_UPDATE_INTERVAL = oldPlayerInterval;
                DOL.GS.ServerProperties.Properties.WORLD_NPC_UPDATE_INTERVAL = oldNpcInterval;
                DOL.GS.ServerProperties.Properties.WORLD_OBJECT_UPDATE_INTERVAL = oldItemInterval;
            }
        }
    }
}
