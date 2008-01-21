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
using System.Net.Sockets;
using DOL.Database2;
using DOL.Events;
using DOL.GS;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.GS.Tests
{
	/// <summary>
	/// Unittest for the Region Class
	/// </summary>
	[TestFixture]
	public class RegionTest : DOLTestCase
	{		
		public static bool notified = false;

		public RegionTest()
		{
		}

		

		[Test] public void AddObject()
		{
			Region region = WorldMgr.GetRegion(1);
			GameObject obj = new GameNPC();
			obj.Name="TestObject";
			obj.X = 400000;
			obj.Y = 200000;
			obj.Z = 2000;
			obj.CurrentRegion = region;

			obj.AddToWorld();

			if (obj.ObjectID<0)
				Assert.Fail("Failed to add object to Region. ObjectId < 0");

			Assert.AreEqual(region.GetObject((ushort)obj.ObjectID),obj);
		}


		[Test] public void AddArea()
		{
			Region region = WorldMgr.GetRegion(1);
			IArea insertArea = region.AddArea(new Area.Circle(null,1000,1000,0,500));

			Assert.IsNotNull(insertArea);

			IList areas = region.GetAreasOfSpot(501,1000,0);			
			Assert.IsTrue(areas.Count>0);

			bool found = false;
			foreach( IArea ar in areas)
			{
				if (ar == insertArea) 
				{
					found = true;	
					break;
				}
			}
			Assert.IsTrue(found);

			//
			areas = region.GetAreasOfSpot(1499,1000,2000);			
			Assert.IsTrue(areas.Count>0);

			found = false;
			foreach( IArea ar in areas)
			{
				if (ar == insertArea) 
				{
					found = true;	
					break;
				}
			}
			Assert.IsTrue(found);


			//Notify test
			notified=false;

			GamePlayer player = CreateMockGamePlayer();
			
			insertArea.RegisterPlayerEnter(new DOLEventHandler(NotifyTest));
			insertArea.OnPlayerEnter(player);

			Assert.IsTrue(notified);

			region.RemoveArea(insertArea);

			areas = region.GetAreasOfSpot(1499,1000,2000);
			Assert.IsTrue(areas.Count==0);

		}

		public static void NotifyTest(DOLEvent e, object sender, EventArgs args)
		{
			Console.WriteLine("notified");
			notified = true;
		}

		[Test] public void RemoveObject()
		{			
		}
	}
}
