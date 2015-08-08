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

// MarketCache by Tolakram.  Donated from Storm

using System;
using System.Collections;
using System.Collections.Generic;

using DOL.Database;
using DOL.Database.Transaction;
using DOL.GS;
using DOL.GS.PacketHandler;
using DOL.Events;

using log4net;

namespace DOL.GS
{
	public class MarketCache
	{
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private static Dictionary<string, InventoryItem> m_itemCache = null;

		private static object CacheLock = new object();

		/// <summary>
		/// Return a List of all items in the cache
		/// </summary>
		public static List<InventoryItem> Items
		{
			get { return new List<InventoryItem>(m_itemCache.Values); }
		}


		/// <summary>
		/// Load or reload all items into the market cache
		/// </summary>
		public static bool Initialize()
		{
			log.Info("Building Market Cache ....");
			try
			{
				m_itemCache = new Dictionary<string, InventoryItem>();
				string query = "(`SlotPosition` >= " + (int)eInventorySlot.Consignment_First + " AND `SlotPosition` <= " + (int)eInventorySlot.Consignment_Last + " AND OwnerLot > 0)";

				IList<InventoryItem> list = GameServer.Database.SelectObjects<InventoryItem>(query);

				foreach (InventoryItem item in list)
				{
					GameInventoryItem playerItem = GameInventoryItem.Create(item);
					m_itemCache.Add(item.ObjectId, playerItem);
				}

				log.Info("Market Cache initialized with " + m_itemCache.Count + " items.");
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Add an item to the market cache
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool AddItem(InventoryItem item)
		{
			bool added = false;

			if (item != null && item.OwnerID != null)
			{
				lock (CacheLock)
				{
					if (m_itemCache.ContainsKey(item.ObjectId) == false)
					{
						m_itemCache.Add(item.ObjectId, item);
						added = true;
					}
					else
					{
						log.Error("Attempted to add duplicate item to Market Cache " + item.ObjectId);
					}
				}
			}

			return added;
		}

		/// <summary>
		/// Remove an item from the market cache
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public static bool RemoveItem(InventoryItem item)
		{
			bool removed = false;

			if (item != null)
			{
				lock (CacheLock)
				{
					if (m_itemCache.ContainsKey(item.ObjectId))
					{
						m_itemCache.Remove(item.ObjectId);
						removed = true;
					}
				}
			}

			return removed;
		}

	}
}
