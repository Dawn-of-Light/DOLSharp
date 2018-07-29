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
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using NUnit.Framework;

namespace DOL.Server.Tests
{
	/// <summary>
	/// Unit tests for the Database Class
	/// </summary>
	[TestFixture]
	public class DatabaseTest : ServerTests
	{				

		public DatabaseTest()
		{
		}

		[Test]
		public void TestSelect()
		{
			Console.WriteLine("TestSelect();");

			var obs = GameServer.Database.SelectAllObjects<ItemTemplate>();
			Console.WriteLine("ItemTemplates Type="+obs.GetType());

			var items = GameServer.Database.SelectAllObjects<MerchantItem>();
			Console.WriteLine("MerchantItems Type="+items.GetType());
			
		}			

	}
}
