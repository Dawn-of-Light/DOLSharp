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
using DOL.GS;

namespace DOL.GS
{
	/// <summary>
	/// List containing all possible candidates for loot drop
	/// </summary>
	public class LootList
	{
		// number of random items to drop
		int m_dropCount;

		/// <summary>
		/// m_dropCount items will be chosen from randomitemdrops list depending on their chance, to drop for mob loot
		/// </summary>
		ArrayList m_randomItemDrops;

		/// <summary>
		/// Items in fixed drop list will ALWAYS drop for mob loot
		/// </summary>
		ArrayList m_fixedItemDrops;

		public LootList() : this(1) { }

		public LootList(int dropCount)
		{
			m_dropCount = dropCount;
			m_fixedItemDrops = new ArrayList(2);
			m_randomItemDrops = new ArrayList(15);
		}

		public int DropCount
		{
			get { return m_dropCount; }
			set { m_dropCount = value; }
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

		public void AddRandom(int chance, ItemTemplate loot)
		{
			LootEntry entry = new LootEntry(chance, loot);
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

			if (list.m_dropCount > m_dropCount)
				m_dropCount = list.m_dropCount;
		}

		/// <summary>
		/// Returns a list of ItemTemplates chosen from Random and Fixed loot.
		/// </summary>
		/// <returns></returns>
	       public ItemTemplate[] GetLoot()
          {
             ArrayList loot = new ArrayList(m_fixedItemDrops.Count + m_dropCount);
             loot.AddRange(m_fixedItemDrops);

                int dice;
                ArrayList lootCandidates = new ArrayList();

                if ( m_dropCount != 0)
                {
                    // Big logic change. The intended flow of the original code was to iterate over the drop list
                    // DropCount times, each iteration doing the following:
                    // 1.) clear lootCandidates array
                    // 2.) for each loot item in m_randomItemDrops roll D100 against frequency
                    // 3.) each winner gts into lootCandidates array
                    // 4.) pick one random winner out of lootCandidates, add to the returned loot list
                    // The bug reported was caused by the loot.Add being outside the main loop. only
                    // the last winner made it into the loot array that is returned.
                    // There is a secondary defect that surfaces once the loop issue is fixed. There is
                    // a high chance of duplicate items dropping. This is counter to live game server
                    // behavior.
                    //
                    // New logic iterates once accross m_randomItemDrops, rolling D100 against frequency.
                    // Winner items are placed into lootCandidates. Once that loop is complete, we enter
                    // into a second loop. In each iteration, we select one random winner from lootCandidates,
                    // add it the to returned loot array, then remove it from lootCandidates. This avoids
                    // code-induced duplicates. I imagine that ArrayList.RemoveAt() is a little expensive on cpu
                    // cycles as it has to collapse the array each time an item is removed. A second option is
                    // to build a matching array of booleans, set all to false, and each time a winner is copied
                    // to loot, set the value in the matching array at the same index value to true. This introduces
                    // the need for a nested loop to avoid true values in the matching array, and if not coded properly,
                    // will hang the server as it loops an infinite number of times. The third option is to ignore
                    // all of this, say that dupes are o.k., pull loot.Add inside the main loop of the old code
                    // and move on.
                                   
                    // new logic
                    lootCandidates.Clear();
                    foreach (LootEntry lootEntry in m_randomItemDrops)
                    {
                        dice = Util.Random(1, 100);
                        if (lootEntry.Chance >= dice)
                            lootCandidates.Add(lootEntry.ItemTemplate);
                    }
                    // At this point, the candidate list is filled with items that passed
                    // the %chance to drop, so we need to put DropCount of them into the return loot object.
                    //
                    // Initial control thought is to use the ArrayList.RemoveAt method and change the loop control
                    // to include a check on lootCandidates.Count. This assumes that the removeAt function re-builds
                    // the array and updates ArrayList.Count. If not, we'll be throwing exceptions like mad.

                    // copy random winners to returned loot list
                    if (lootCandidates.Count > 0)
                    {
                        for (int i = 0; (i < m_dropCount && lootCandidates.Count != 0); i++)
                        {
                            int tmpidx;
                            tmpidx = Util.Random(lootCandidates.Count - 1);
                            loot.Add(lootCandidates[tmpidx]);
                            lootCandidates.RemoveAt(tmpidx);
                        }
                           
                    }
                }
               
             return (ItemTemplate[])loot.ToArray(typeof(ItemTemplate));
          }
	}

	/// <summary>
	/// Container class for entries in the randomdroplist
	/// </summary>
	class LootEntry
	{
		int m_chance;
		ItemTemplate m_itemTemplate;

		public LootEntry(int chance, ItemTemplate item)
		{
			m_chance = chance;
			m_itemTemplate = item;
		}

		public ItemTemplate ItemTemplate
		{
			get { return m_itemTemplate; }
		}

        public int Chance
        {
            get { return m_chance; }
        }
	}
}