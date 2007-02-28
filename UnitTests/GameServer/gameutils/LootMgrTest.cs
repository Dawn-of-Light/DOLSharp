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
using DOL.Database;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.GS.Tests
{
	
	/// <summary>
	/// Unittest for the LootMgr Class
	/// </summary>
	[TestFixture]
	public class LootMgrTest : DOLTestCase
	{
		public LootMgrTest()
		{
			
		}

		[Test] public void TestLootGenerator()
		{						
			GameNPC mob = new GameNPC();
			mob.Level = 6;
			mob.Name="impling";

			for (int i=0;i< 15; i++) 
			{
				Console.WriteLine("Loot "+i);
				ItemTemplate[] loot = LootMgr.GetLoot(mob, null);
				foreach (ItemTemplate item in loot)
				{
					Console.WriteLine(mob.Name+" drops "+item.Name);
				}	
			}
			
			Console.WriteLine("Drops finished");
		}
	}
}
