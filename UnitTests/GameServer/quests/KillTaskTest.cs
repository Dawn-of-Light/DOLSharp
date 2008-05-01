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
using System.Net.Sockets;
using DOL.Database;
using DOL.Events;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.GS.Quests.Tests
{
	/// <summary>
	/// Zusammenfassung für GamePlayerTest.
	/// </summary>
	[TestFixture]
	public class KillTaskTest : DOLTestCase
	{
		public KillTaskTest()
		{			
		}		

		[Test] public void CreateKillTask()
		{
			GamePlayer player = CreateMockGamePlayer();
			player.Level=5;
			player.AddToWorld();

			// Trainer for taks
			GameTrainer trainer = new GameTrainer();
			trainer.Name ="Tester";
			
			Console.WriteLine(player.Name);

			// player must have trainer selected when task given.
			player.TargetObject = trainer;
			
			// mob for task			
			if (KillTask.BuildTask(player,trainer)) 
			{
				KillTask task =(KillTask) player.Task;				

				Assert.IsNotNull(task);
				Assert.IsTrue(task.TaskActive);

				Console.WriteLine("Mob:"+ task.MobName);
				Console.WriteLine("Item:"+ task.ItemName);
				Console.WriteLine(""+ task.Description);

				// Check Notify Event handling
				InventoryItem item = new InventoryItem();
				item.Name = task.ItemName;

				GameNPC mob = new GameNPC();
				mob.Name = task.MobName;
				mob.X = player.X;
				mob.Y = player.Y;
				mob.Z = player.Z;
				mob.Level = player.Level;
				mob.CurrentRegionID = player.CurrentRegionID;
				mob.AddToWorld();
			
				// First we kill mob
				mob.XPGainers.Add(player,1.0F);
				task.Notify(GameNPCEvent.EnemyKilled,player,new EnemyKilledEventArgs(mob));						

				// arificial pickup Item
				player.Inventory.AddItem(eInventorySlot.FirstEmptyBackpack, item);
			
				// Check item in Inventory
				if (player.Inventory.GetFirstItemByName(task.ItemName,eInventorySlot.FirstBackpack,eInventorySlot.LastBackpack) != null)
					Assert.Fail("Player didn't recieve task item.");
			
				// Now give item tro trainer
				task.Notify(GamePlayerEvent.GiveItem,player,new GiveItemEventArgs(player,trainer,item));

				if (player.Task.TaskActive || player.Task==null)
					Assert.Fail("Task did not finished proper in Notify");
			}
		}
	}
}