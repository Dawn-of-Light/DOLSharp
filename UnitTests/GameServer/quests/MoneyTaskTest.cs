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
using DOL.Events;
using DOL.Tests;
using NUnit.Framework;

namespace DOL.GS.Quests.Tests
{
	/// <summary>
	/// Zusammenfassung für GamePlayerTest.
	/// </summary>
	[TestFixture]
	public class MoneyTaskTest : DOLTestCase
	{
		public MoneyTaskTest()
		{
		}

		[Test]
		public void CreateMoneyTask()
		{
			GamePlayer player = CreateMockGamePlayer();

			GameMerchant merchant = new GameMerchant();
			merchant.Name = "Tester";
			merchant.Realm = eRealm.Albion;
			Console.WriteLine(player.Name);

			if (MoneyTask.CheckAvailability(player, merchant))
			{
				if (MoneyTask.BuildTask(player, merchant))
				{
					MoneyTask task = (MoneyTask)player.Task;


					Assert.IsNotNull(task);
					Console.WriteLine("XP" + task.RewardXP);
					Console.WriteLine("Item:" + task.ItemName);
					Console.WriteLine("Item:" + task.Name);
					Console.WriteLine("Item:" + task.Description);

					// Check Notify Event handling
					InventoryItem item = new InventoryItem();
					item.Name = task.ItemName;

					GameNPC npc = new GameNPC();
					npc.Name = task.RecieverName;
					task.Notify(GamePlayerEvent.GiveItem, player, new GiveItemEventArgs(player, npc, item));

					if (player.Task.TaskActive || player.Task == null)
						Assert.Fail("Task did not finished proper in Notify");
				}
			}
		}
	}
}