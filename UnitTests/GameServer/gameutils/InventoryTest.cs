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
using DOL.Database;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.Tests
{
	/// <summary>
	/// Unit tests for the new Inventory system
	/// </summary>
	[TestFixture]
	public class InventoryTest : DOLTestCase
	{
		static GamePlayer player;
		static ItemTemplate itemt;
		static ItemUnique itemu;
		static ItemTemplate itema;
		
		public InventoryTest() {}
		
		// the following is not in the constructor because TestFixtureSetup
		// is not initialized at this time, thus we can't connect to server
		public void InventoryTestCreation()
		{
			player = CreateMockGamePlayer();
			Assert.IsNotNull(player, "Player is null !");
			itemt = GameServer.Database.SelectObject<ItemTemplate>("Id_nb = 'championDocharWardenBlade'");
			Assert.IsNotNull(itemt, "ItemTemplate is null !");
			itemu = new ItemUnique();
			itemu.Id_nb = "tunik"+DateTime.Now.Ticks;
			GameServer.Database.AddObject(itemu);
			Assert.IsNotNull(itemu, "ItemUnique is created !");
			itema = GameServer.Database.SelectObject<ItemTemplate>("id_nb= 'traitors_dagger_alb");
		}

		/* Tests for items - 1/ IT 2/ IU 3/ Ghost
		 * 
		 * 1) create an invitem
		 * 2) check inventory table
		 * 3) delete this invitem
		 * 4) check inventory table
		 * 5) check IT / IU tables
		 * 
		 * Tests for specific items: artefact, boats
		 * 
		 * 1) artefact creation
		 * 
		 * Tests for floor throw, blacksmith, repair
		 * 
		 * 1) the type given IT IU Ghost is not changed during the operation
		 * 
		 */
		[Test] public void InventoryFromIT()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Ghost Inventory entry based on ItemTemplate");

			InventoryItem ii = new InventoryItem(itemt);
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.IsNotNull(ii, "ii-t #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.IsNotNull(iicheck, "ii-t #2 : saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			GameServer.Database.DeleteObject(ii);
			iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.IsNull(iicheck, "ii-t #3 : deleted from db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			var itcheck = GameServer.Database.FindObjectByKey<ItemTemplate>(itemt.Id_nb);
			Assert.IsNotNull(itcheck, "ii-t #4 : not deleted from db " + itemt.Id_nb);
		}
		[Test] public void InventoryFromIU()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Inventory entry based on ItemUnique");

			InventoryItem ii = new InventoryItem(itemu);
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.IsNotNull(ii, "ii-u #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.IsNotNull(iicheck, "ii-u #2 : saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			var iucheck = GameServer.Database.FindObjectByKey<ItemUnique>(itemu.Id_nb);
			Assert.IsNotNull(iicheck, "ii-u #3 : saved to db " + itemu.Id_nb);
			GameServer.Database.DeleteObject(ii);
			iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.IsNull(iicheck, "ii-u #4 : deleted from db " + ii.Template.Id_nb + " to " + ii.OwnerID);
			iucheck = GameServer.Database.FindObjectByKey<ItemUnique>(itemu.Id_nb);
			Assert.IsNull(iucheck, "ii-t #5 : deleted from db " + itemu.Id_nb);
			
		}
		[Test] public void InventoryFromNull()
		{
			InventoryTestCreation();
			Console.WriteLine("Creation of Ghost Inventory entry based on ItemTemplate");

			InventoryItem ii = new InventoryItem();
			player.Inventory.AddItem(eInventorySlot.FirstBackpack, ii);
			Assert.IsNotNull(ii, "ii-g #1 : " + ii.Template.Id_nb + " created & added to " + ii.OwnerID);
			var iicheck = GameServer.Database.FindObjectByKey<InventoryItem>(ii.ObjectId);
			Assert.IsNull(iicheck, "ii-g #2 : not saved in db " + ii.Template.Id_nb + " to " + ii.OwnerID);
		}
		[Test] public void InventoryArtifact()
		{
			InventoryTestCreation();
			Console.WriteLine("InventoryItem linked to an artifact: replace XP via bonuses");
			
			InventoryArtifact ia = new InventoryArtifact(itema);
			Assert.IsNotNull(ia, "ia #1 : " + ia.Template.Id_nb + " created & added to " + ia.OwnerID);
			player.Inventory.AddItem(eInventorySlot.RightHandWeapon, ia);
			Assert.IsNotNull(ia, "ia #2 : " + ia.Template.Id_nb + " created & added to " + ia.OwnerID);
			var iacheck = GameServer.Database.FindObjectByKey<InventoryItem>(ia.ObjectId);
			Assert.IsNotNull(iacheck, "ia #3 : " + ia.OwnerID + " saved to db");			
		}
	}
}
