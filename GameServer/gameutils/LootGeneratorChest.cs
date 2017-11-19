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

using DOL.Database;

namespace DOL.GS
{
    /// <summary>
    /// LootGeneratorChest
    /// Adds money chests to a Mobs droppable loot based on a chance set in server properties
    /// </summary>
    public class LootGeneratorChest : LootGeneratorBase
	{	
		int SMALLCHEST_CHANCE = ServerProperties.Properties.BASE_SMALLCHEST_CHANCE;
		int LARGECHEST_CHANCE = ServerProperties.Properties.BASE_LARGECHEST_CHANCE;
		public override LootList GenerateLoot(GameNPC mob, GameObject killer)
		{
			LootList loot = base.GenerateLoot(mob, killer);
			int small = SMALLCHEST_CHANCE;
			int large = LARGECHEST_CHANCE;
			if (Util.Chance(small))
			{
				int lvl = mob.Level + 1;
				if (lvl < 1) lvl = 1;
				int minLoot = ServerProperties.Properties.SMALLCHEST_MULTIPLIER * (lvl * lvl); 
				long moneyCount = minLoot + Util.Random(minLoot >> 1);
				moneyCount = (long)((double)moneyCount * ServerProperties.Properties.MONEY_DROP);
				ItemTemplate money = new ItemTemplate();
				money.Model = 488;
				money.Name = "small chest";
				money.Level = 0;
				money.Price = moneyCount;
				loot.AddFixed(money, 1);
			}
			if (Util.Chance(large))
			{
				int lvl = mob.Level + 1;
				if (lvl < 1) lvl = 1;
				int minLoot = ServerProperties.Properties.LARGECHEST_MULTIPLIER * (lvl * lvl); 
				long moneyCount = minLoot + Util.Random(minLoot >> 1);
				moneyCount = (long)((double)moneyCount * ServerProperties.Properties.MONEY_DROP);
				ItemTemplate money = new ItemTemplate();
				money.Model = 488;
				money.Name = "large chest";
				money.Level = 0;
				money.Price = moneyCount;
				loot.AddFixed(money, 1);
			}
			return loot;
		}
	}
}