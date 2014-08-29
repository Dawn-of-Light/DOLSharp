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
using System.Collections.Generic;
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// List containing all possible candidates for loot drop
	/// </summary>
	public class LootList
	{
		/// <summary>
		/// number of random items to drop
		/// </summary>
		public int DropCount { get; set; }

		/// <summary>
		/// m_dropCount items will be chosen from randomitemdrops list depending on their chance, to drop for mob loot
		/// </summary>
		private readonly List<LootEntry> m_randomItemDrops;

		/// <summary>
		/// Items in fixed drop list will ALWAYS drop for mob loot
		/// </summary>
		private readonly List<ItemTemplate> m_fixedItemDrops;

		public LootList() : this(1) { }

		public LootList(int dropCount)
		{
			DropCount = dropCount;
			m_fixedItemDrops = new List<ItemTemplate>(2);
			m_randomItemDrops = new List<LootEntry>(15);
		}

		/// <summary>
		/// Adds dropCount pieces of this item to the list of fixed drops.
		/// </summary>
		/// <param name="loot"></param>
		public void AddFixed(ItemTemplate loot, int dropCount)
		{
			for (int drop = 0; drop < dropCount; drop++)
				m_fixedItemDrops.Add(loot);
		}

		public void AddRandom(int chance, ItemTemplate loot, int count = 1)
		{
			LootEntry entry = new LootEntry(chance, loot, count);
			m_randomItemDrops.Add(entry);
		}

		/// <summary>
		/// Merges two list into one big list, containing items of both and having the bigger dropCount
		/// </summary>
		/// <param name="list"></param>
		public void AddAll(LootList list)
		{
			if (list.m_randomItemDrops != null)
			{
				m_randomItemDrops.AddRange(list.m_randomItemDrops);
			}

			if (list.m_fixedItemDrops != null)
			{
				m_fixedItemDrops.AddRange(list.m_fixedItemDrops);
			}

			if (list.DropCount > DropCount)
				DropCount = list.DropCount;
		}

		/// <summary>
		/// Returns a list of ItemTemplates chosen from Random and Fixed loot.
		/// </summary>
		/// <returns></returns>
		public ItemTemplate[] GetLoot()
		{
			List<ItemTemplate> loot = new List<ItemTemplate>(m_fixedItemDrops.Count + DropCount);
			loot.AddRange(m_fixedItemDrops);

			if (DropCount > 0)
			{

				// new logic
				List<LootEntry> lootCandidates = new List<LootEntry>();
				foreach (LootEntry lootEntry in m_randomItemDrops)
				{
					if (lootEntry.Chance >= Util.Random(0, 100))
						lootCandidates.Add(lootEntry);
				}
				
				if(lootCandidates.Count < 1)
					return loot.ToArray();
				
				// Pick up random item
				int n = 0;
				do
				{
					int rnd = Util.Random(0, lootCandidates.Count-1);
					
					loot.Add(lootCandidates[rnd].ItemTemplate);
					
					(lootCandidates[rnd].Count)--;
					
					if(lootCandidates[rnd].Count < 1)
						lootCandidates.RemoveAt(rnd);
					
					n++;
				}
				while(lootCandidates.Count > 0 && n < DropCount);
			}

			return loot.ToArray();
		}
	}

	/// <summary>
	/// Container class for entries in the randomdroplist
	/// </summary>
	internal class LootEntry
	{
		public readonly int Chance;
		public readonly ItemTemplate ItemTemplate;
		public int Count;

		public LootEntry(int chance, ItemTemplate item, int count)
		{
			Chance = chance;
			ItemTemplate = item;
			Count = count;
		}
	}
}