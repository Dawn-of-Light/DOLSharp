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
using DOL.GS;
using DOL.Database;
using NUnit.Framework;

namespace DOL.Server.Tests
{
	/// <summary>
	/// Test Inventory Class for Unit Test
	/// </summary>
	public class TestInventory : GameLivingInventory
	{
	}

	/// <summary>
	/// Unit Test for Game Living Inventory
	/// </summary>
	[TestFixture]
	public class GameLivingInventoryTest : ServerTests
	{
		public GameLivingInventoryTest() : base()
		{
			
		}

		[Test]
		public void TestAddTemplate()
		{
			GameLivingInventory gameLivingInventory = new TestInventory();

			ItemTemplate template = new ItemTemplate();
			Random rand = new Random();
			template.Id_nb = "blankItem" + rand.Next().ToString();
			template.Name = "a blank item";
			template.MaxCount = 10;
			if (template == null)
				Console.WriteLine("template null");
			if (gameLivingInventory.AddTemplate(GameInventoryItem.Create(template), 7, eInventorySlot.RightHandWeapon, eInventorySlot.FourthQuiver))
				Console.WriteLine("addtemplate 7 blank item");
			else
				Console.WriteLine("can not add 7 blank item");
			Console.WriteLine("----PRINT AFTER FIRST ADD 7 TEMPLATE-----");
			PrintInventory(gameLivingInventory);

			if (gameLivingInventory.AddTemplate(GameInventoryItem.Create(template), 4, eInventorySlot.RightHandWeapon, eInventorySlot.FourthQuiver))
				Console.WriteLine("addtemplate 4 blank item");
			else
				Console.WriteLine("can not add 4 blank item");
			Console.WriteLine("----PRINT AFTER SECOND ADD 4 TEMPLATE-----");
			PrintInventory(gameLivingInventory);
			//here must have 10 item in a slot and 1 in another
			
		}
		
		public void PrintInventory(GameLivingInventory gameLivingInventory)
		{
			foreach(InventoryItem myitem in gameLivingInventory.AllItems)
			{
				Console.WriteLine("item ["+ myitem.SlotPosition +"] : " + myitem.Name + "(" +myitem.Count +")");
			}
		}
	}
}
