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
using NUnit.Framework;

namespace DOL.Integration.Server
{
	[TestFixture]
	public class InventoryTest : ServerTests
	{
		static GamePlayer player;
		static ItemTemplate itemt;
		static ItemUnique itemu;
		
		public InventoryTest() {}
		
		// the following is not in the constructor because TestFixtureSetup
		// is not initialized at this time, thus we can't connect to server
		public void InventoryTestCreation()
		{
			player = CreateMockGamePlayer();
			Assert.That(player, Is.Not.Null, "Player is null !");
			itemt = DOLDB<ItemTemplate>.SelectObject(DB.Column(nameof(ItemTemplate.Id_nb)).IsEqualTo("championDocharWardenBlade"));
			Assert.That(itemt, Is.Not.Null, "ItemTemplate is null !");
			itemu = new ItemUnique();
			itemu.Id_nb = "tunik"+DateTime.Now.Ticks;
			GameServer.Database.AddObject(itemu);
			Assert.That(itemu, Is.Not.Null, "ItemUnique is created !");
			_ = DOLDB<ItemTemplate>.SelectObject(DB.Column(nameof(ItemTemplate.Id_nb)).IsEqualTo("traitors_dagger_hib"));
		}

		/* Tests for items - 1/ IT 2/ IU 3/ Ghost
		 * 
		 * 1) create an invitem
		 * 2) check inventory table
		 * 3) delete this invitem
		 * 4) check inventory table
		 * 5) check IT / IU tables
		 * 
		 */
		[Test, Explicit]
		public void InventoryFromIT()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Ghost Inventory entry based on ItemTemplate");

			InventoryItem ii = GameInventoryItem.Create(itemt);
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.That(ii, Is.Not.Null, "ii-t #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.That(iicheck, Is.Not.Null, "ii-t #2 : saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			GameServer.Database.DeleteObject(ii);
			iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.That(iicheck, Is.Null, "ii-t #3 : deleted from db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			var itcheck = GameServer.Database.FindObjectByKey<ItemTemplate>(itemt.Id_nb);
			Assert.That(itcheck, Is.Not.Null, "ii-t #4 : not deleted from db " + itemt.Id_nb);
		}
		
		[Test, Explicit]
		public void InventoryFromIU()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Inventory entry based on ItemUnique");

			InventoryItem ii = GameInventoryItem.Create(itemu);
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.That(ii, Is.Not.Null, "ii-u #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.That(iicheck, Is.Not.Null, "ii-u #2 : saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			var iucheck = GameServer.Database.FindObjectByKey<ItemUnique>(itemu.Id_nb);
			Assert.That(iicheck, Is.Not.Null, "ii-u #3 : saved to db " + itemu.Id_nb);
			GameServer.Database.DeleteObject(ii);
			iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.That(iicheck, Is.Null, "ii-u #4 : deleted from db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			iucheck = GameServer.Database.FindObjectByKey<ItemUnique>(itemu.Id_nb);
			Assert.That(iucheck, Is.Null, "ii-t #5 : deleted from db " + itemu.Id_nb);
			
		}
		
		[Test, Explicit]
		public void InventoryFromNull()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Ghost Inventory entry based on ItemTemplate");

			InventoryItem ii = new InventoryItem();
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.That(ii, Is.Not.Null, "ii-g #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.That(iicheck, Is.Null, "ii-g #2 : not saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
		}
	}
}
