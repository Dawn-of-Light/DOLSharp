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
using DOL.Events;
using DOL.GS;
using DOL.GS.Geometry;
using NUnit.Framework;

namespace DOL.Integration.Server
{
	[TestFixture]
	public class RegionTest : ServerTests
	{		
		public static bool notified = false;

		public RegionTest()
		{
		}		

		[Test, Explicit]
		public void AddObject()
		{
			GameObject obj = new GameNPC();
			obj.Name="TestObject";
            obj.Position = Position.Create(regionID: 1, x: 400000, y: 200000, z: 2000);

			obj.AddToWorld();

			if (obj.ObjectID<0)
				Assert.Fail("Failed to add object to Region. ObjectId < 0");

			Assert.That(obj, Is.EqualTo(obj.CurrentRegion.GetObject((ushort)obj.ObjectID)));
		}


		[Test, Explicit]
		public void AddArea()
		{
			Region region = WorldMgr.GetRegion(1);
            var circleLocation = Coordinate.Create(1000,1000,0);
			IArea insertArea = region.AddArea(new Area.Circle(null,circleLocation,500));

			Assert.That(insertArea, Is.Not.Null);

			var areas = region.GetAreasOfSpot(Coordinate.Create(501,1000,0));
			Assert.That(areas.Count>0, Is.True);

			bool found = false;
			foreach( IArea ar in areas)
			{
				if (ar == insertArea)
				{
					found = true;
					break;
				}
			}
			Assert.That(found, Is.True);

			//
			areas = region.GetAreasOfSpot(Coordinate.Create(1499,1000,2000));
			Assert.That(areas.Count>0, Is.True);

			found = false;
			foreach( IArea ar in areas)
			{
				if (ar == insertArea) 
				{
					found = true;	
					break;
				}
			}
			Assert.That(found, Is.True);


			//Notify test
			notified=false;

			GamePlayer player = CreateMockGamePlayer();
			
			insertArea.RegisterPlayerEnter(new DOLEventHandler(NotifyTest));
			insertArea.OnPlayerEnter(player);

			Assert.That(notified, Is.True);

			region.RemoveArea(insertArea);

			areas = region.GetAreasOfSpot(Coordinate.Create(1499,1000,2000));
			Assert.That(areas.Count==0, Is.True);

		}

		public static void NotifyTest(DOLEvent e, object sender, EventArgs args)
		{
			Console.WriteLine("notified");
			notified = true;
		}

		[Test]
		public void RemoveObject()
		{			
		}
	}
}
