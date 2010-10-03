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
using DOL;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.Server
{
	/// <summary>
	/// Unit tests for the Zone Class
	/// </summary>
	[TestFixture]
	public class ZoneTest : ServerTests
	{

		public ZoneTest()
		{
		}

		[Test]
		public void GetZCoordinateTest()
		{

		}

		[Test]
		public void GetRandomNPCTest()
		{
			Zone zone = WorldMgr.GetZone(0);
			Assert.IsNotNull(zone);

			StartWatch();
			GameNPC npc = zone.GetRandomNPC(eRealm.None, 5, 7);
			if (npc != null)
			{
				Console.WriteLine("Found NPC from Realm None in " + zone.ZoneRegion.Description + "/" + zone.Description + ":" + npc.Name + " level:" + npc.Level);
				if (npc.Level < 5 || npc.Level > 7)
					Assert.Fail("NPC Level out of defined range");

				if (npc.Realm != eRealm.None)
					Assert.Fail("NPC wrong Realm");
			}
			else
			{
				Console.WriteLine("nothing found in " + zone.ZoneRegion.Description + "/" + zone.Description);
			}
			StopWatch();

			StartWatch();
			npc = zone.GetRandomNPC(eRealm.Albion);
			if (npc != null)
			{
				Console.WriteLine("Found Albion NPC in " + zone.ZoneRegion.Description + "/" + zone.Description + ":" + npc.Name);

				if (npc.Realm != eRealm.Albion)
					Assert.Fail("NPC wrong Realm");
			}
			else
			{
				Console.WriteLine("nothing found in " + zone.ZoneRegion.Description + "/" + zone.Description);
			}
			StopWatch();
		}
	}
}
