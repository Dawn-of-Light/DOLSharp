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
using DOL.Database2;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.GS.Tests
{
	/// <summary>
	/// Unit tests for the Zone Class
	/// </summary>
	[TestFixture]
	public class DatabaseTest : DOLTestCase
	{				

		public DatabaseTest()
		{
		}

		[Test] public void TestSelect()
		{
			Console.WriteLine("TestSelect();");
			DataObject[] obs = GameServer.Database.SelectAllObjects(typeof(ItemTemplate));
			Console.WriteLine("ItemTemplates Type="+obs.GetType());
			MerchantItem[] items = (MerchantItem[])GameServer.Database.SelectAllObjects(typeof(MerchantItem));
			Console.WriteLine("MerchantItems Type="+items.GetType());
		}			

	}
}
