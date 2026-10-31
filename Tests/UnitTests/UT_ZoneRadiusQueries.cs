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
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using DOL.Database;
using DOL.GS;
using DOL.GS.Geometry;
using NUnit.Framework;

namespace DOL.UnitTests.Gameserver
{
    [TestFixture, NonParallelizable]
    public class UT_ZoneRadiusQueries
    {
        class Player : GamePlayer
        {
            readonly Region region;
            public Player(Region region, int x, int y, int z=0) : base(null,new DOLCharacters())
            {
                this.region=region;
                Position=Position.Create(region.ID,x,y,z);
                ObjectState=eObjectState.Active;
            }
            public override Region CurrentRegion {get=>region;set{}}
            public override void LoadFromDatabase(DataObject obj) {}
        }
        static Region NewRegion() => new Region(null,new RegionData {Id=64997});
        static Zone NewZone(Region region,int offset=0)
        {
            var zone=new Zone(region,(ushort)(offset==0 ? 64997:64996),"Radius test",offset,0,65536,65536,64997,false,0,false,0,0,0,0,0);
            region.Zones.Add(zone);return zone;
        }
        delegate ArrayList Query(Zone.eGameObjectType type,Coordinate center,ushort radius,ArrayList partial,bool ignoreZ);
        static ArrayList Search(Zone zone,Coordinate center,ushort radius,ArrayList partial=null,bool ignoreZ=false)
        {
            var method=typeof(Zone).GetMethod("GetObjectsInRadius",BindingFlags.Instance|BindingFlags.NonPublic,null,
                new[] {typeof(Zone.eGameObjectType),typeof(Coordinate),typeof(ushort),typeof(ArrayList),typeof(bool)},null);
            return method.CreateDelegate<Query>(zone)(Zone.eGameObjectType.PLAYER,center,radius,partial ?? new ArrayList(),ignoreZ);
        }
        [TestCase(false),TestCase(true)]
        public void DenseCrowdPreservesOrderAndExistingPrefix(bool wholeCell)
        {
            var region=NewRegion();var zone=NewZone(region);
            try
            {
                var players=Enumerable.Range(0,1000).Select(i=>new Player(region,10000+i%20*5,10000+i/20*3)).ToArray();
                foreach(var player in players) zone.ObjectEnterZone(player);
                zone.ObjectEnterZone(players[0]); // A repeated spatial node must not duplicate its object.
                var center=Coordinate.Create(wholeCell ? 11000:10000,10000,0);
                // Spatial nodes prepend: the repeated node is first, followed by
                // the original nodes in reverse insertion order.
                var expected=new[] {players[0]}.Concat(players.Skip(1).Reverse()).ToArray();
                Assert.That(Search(zone,center,4096).Cast<Player>(),Is.EqualTo(expected));
                Assert.That(Search(zone,center,4096).Cast<Player>(),Is.EqualTo(expected)); // Reused pool must be cleared.
                var prefix=new ArrayList {players[999],players[999]};
                var result=Search(zone,center,4096,prefix);
                Assert.That(result,Is.SameAs(prefix));
                Assert.That(result.Cast<Player>(),Is.EqualTo(new[] {players[999],players[999]}.Concat(expected.Where(p=>p!=players[999]))));
            }
            finally {zone.Delete();}
        }
        [Test]
        public void RadiusBoundaryHeightAndInactiveFilteringRemainUnchanged()
        {
            var region=NewRegion();var zone=NewZone(region);
            try
            {
                var inside=new Player(region,10000,10000);
                var edge=new Player(region,10100,10000);
                var outside=new Player(region,10101,10000);
                var above=new Player(region,10000,10000,101);
                var inactive=new Player(region,10000,10000) {ObjectState=GameObject.eObjectState.Inactive};
                foreach(var player in new[] {inside,edge,outside,above,inactive}) zone.ObjectEnterZone(player);
                Assert.That(Search(zone,inside.Coordinate,100).Cast<Player>(),Is.EqualTo(new[] {edge,inside}));
                Assert.That(Search(zone,inside.Coordinate,100,ignoreZ:true).Cast<Player>(),Is.EqualTo(new[] {above,edge,inside}));
            }
            finally {zone.Delete();}
        }
        [Test]
        public void RegionQueriesKeepBothSidesOfZoneBoundaryAndDistanceEnumeration()
        {
            var region=NewRegion();var left=NewZone(region);var right=NewZone(region,65536);
            try
            {
                var a=new Player(region,65500,10000);var b=new Player(region,65550,10000);
                left.ObjectEnterZone(a);right.ObjectEnterZone(b);
                Assert.That(region.GetPlayersInRadius(a.Coordinate,100,false,false).Cast<GamePlayer>(),Is.EqualTo(new[] {a,b}));
                Assert.That(region.GetPlayersInRadius(a.Coordinate,100,true,false).Cast<object>().Count(),Is.EqualTo(2));
            }
            finally {left.Delete();right.Delete();}
        }
        [Test]
        public void ConcurrentQueriesDoNotShareMembershipOrRetainPreviousResults()
        {
            var region=NewRegion();var zone=NewZone(region);
            try
            {
                var a=new Player(region,10000,10000);var b=new Player(region,20000,10000);
                zone.ObjectEnterZone(a);zone.ObjectEnterZone(b);
                Parallel.For(0,256,i=>
                {
                    var expected=i%2==0 ? a:b;
                    Assert.That(Search(zone,expected.Coordinate,100).Cast<Player>(),Is.EqualTo(new[] {expected}));
                });
            }
            finally {zone.Delete();}
        }
    }
}
