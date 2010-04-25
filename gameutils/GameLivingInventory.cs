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
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using DOL.Database;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description rsume de GameLivingInventory.
	/// </summary>
	public abstract class GameLivingInventory : IGameInventory
	{
		public static readonly eInventorySlot[] EQUIP_SLOTS =
			{
				eInventorySlot.Horse,
				eInventorySlot.HorseArmor,
				eInventorySlot.HorseBarding,
				eInventorySlot.RightHandWeapon,
				eInventorySlot.LeftHandWeapon,
				eInventorySlot.TwoHandWeapon,
				eInventorySlot.DistanceWeapon,
				eInventorySlot.FirstQuiver,
				eInventorySlot.SecondQuiver,
				eInventorySlot.ThirdQuiver,
				eInventorySlot.FourthQuiver,
				eInventorySlot.HeadArmor,
				eInventorySlot.HandsArmor,
				eInventorySlot.FeetArmor,
				eInventorySlot.Jewellery,
				eInventorySlot.TorsoArmor,
				eInventorySlot.Cloak,
				eInventorySlot.LegsArmor,
				eInventorySlot.ArmsArmor,
				eInventorySlot.Neck,
				eInventorySlot.Waist,
				eInventorySlot.LeftBracer,
				eInventorySlot.RightBracer,
				eInventorySlot.LeftRing,
				eInventorySlot.RightRing,
				eInventorySlot.Mythical,
			};

		protected static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//Defines the visible slots that will be displayed to players
		protected static readonly eInventorySlot[] VISIBLE_SLOTS =
			{
				eInventorySlot.RightHandWeapon,
				eInventorySlot.LeftHandWeapon,
				eInventorySlot.TwoHandWeapon,
				eInventorySlot.DistanceWeapon,
				eInventorySlot.HeadArmor,
				eInventorySlot.HandsArmor,
				eInventorySlot.FeetArmor,
				eInventorySlot.TorsoArmor,
				eInventorySlot.Cloak,
				eInventorySlot.LegsArmor,
				eInventorySlot.ArmsArmor
			};

		#region Constructor/Declaration/LoadDatabase/SaveDatabase

		/// <summary>
		/// The complete inventory of all living including
		/// for players the vault, the equipped items and the backpack
		/// and for mob the quest drops ect ...
		/// </summary>
		protected readonly Dictionary<eInventorySlot, InventoryItem> m_items;

		/// <summary>
		/// Holds all changed slots
		/// </summary>
		protected List<eInventorySlot> m_changedSlots;

		/// <summary>
		/// Holds the begin changes counter for slot updates
		/// </summary>
		protected int m_changesCounter;

		/// <summary>
		/// Constructs a new empty inventory
		/// </summary>
		protected GameLivingInventory()
		{
			m_items = new Dictionary<eInventorySlot, InventoryItem>();
			m_changedSlots = new List<eInventorySlot>();
		}

		/// <summary>
		/// LoadFromDatabase
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public virtual bool LoadFromDatabase(string id)
		{
			return false;
		}

		/// <summary>
		/// SaveIntoDatabase
		/// </summary>
		/// <returns></returns>
		public virtual bool SaveIntoDatabase(string id)
		{
			return false;
		}

		#endregion

		#region Get Inventory Informations

		/// <summary>
		/// Counts used/free slots between min and max
		/// </summary>
		/// <param name="countUsed"></param>
		/// <param name="minSlot"></param>
		/// <param name="maxSlot"></param>
		/// <returns></returns>
		public int CountSlots(bool countUsed, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			int result = 0;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.ContainsKey(i))
					{
						if (countUsed)
							result++;
					}
					else
					{
						if (!countUsed)
							result++;
					}
				}

				return result;
			}
		}

		/// <summary>
		/// Count items of some type
		/// </summary>
		/// <param name="itemtemplateID">template to count</param>
		/// <param name="minSlot">first slot</param>
		/// <param name="maxSlot">last slot</param>
		/// <returns>number of matched items found</returns>
		public int CountItemTemplate(string itemtemplateID, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				int count = 0;

				// If lower slot is greater than upper slot, flip the values.
				if (minSlot > maxSlot)
				{
					eInventorySlot tmp = minSlot;
					minSlot = maxSlot;
					maxSlot = tmp;
				}

				InventoryItem item;

				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.TryGetValue(i, out item) && item.Id_nb == itemtemplateID)
					{
						count += item.Count;
					}
				}

				return count++;
			}
		}

		/// <summary>
		/// Checks if specified count of slots is free
		/// </summary>
		/// <param name="count"></param>
		/// <param name="minSlot"></param>
		/// <param name="maxSlot"></param>
		/// <returns></returns>
		public bool IsSlotsFree(int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			if (count < 1)
				return true;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.ContainsKey(i))
						continue;

					count--;

					if (count <= 0)
						return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Find the first empty slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid if they are all full</returns>
		public virtual eInventorySlot FindFirstEmptySlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first, last, true, true);
		}

		/// <summary>
		/// Find the last empty slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindLastEmptySlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first, last, false, true);
		}

		/// <summary>
		/// Find the first full slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindFirstFullSlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first, last, true, false);
		}

		/// <summary>
		/// Find the last full slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindLastFullSlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first, last, false, false);
		}

		/// <summary>
		/// Check if the slot is valid in the inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected virtual eInventorySlot GetValidInventorySlot(eInventorySlot slot)
		{
			if ((slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.FourthQuiver)
			    || (slot >= eInventorySlot.HeadArmor && slot <= eInventorySlot.Neck)
			    || (slot >= eInventorySlot.HorseArmor && slot <= eInventorySlot.Horse)
			    || (slot >= eInventorySlot.Waist && slot <= eInventorySlot.Mythical)
			    || (slot == eInventorySlot.Ground))
				return slot;

			return eInventorySlot.Invalid;
		}

		/// <summary>
		/// Searches between two slots for the first or last full or empty slot
		/// </summary>
		/// <param name="first"></param>
		/// <param name="last"></param>
		/// <param name="searchFirst"></param>
		/// <param name="searchNull"></param>
		/// <returns></returns>
		protected virtual eInventorySlot FindSlot(eInventorySlot first, eInventorySlot last, bool searchFirst, bool searchNull)
		{
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				first = GetValidInventorySlot(first);
				last = GetValidInventorySlot(last);

				if (first == eInventorySlot.Invalid || last == eInventorySlot.Invalid)
					return eInventorySlot.Invalid;

				// If first/last slots are identical, check to see if the slot is full/empty and return based on 
				// whether we instructed to find an empty or a full slot.
				if (first == last)
				{
					// If slot is empty, and we wanted an empty slot, or if slot is full, and we wanted
					// a full slot, return the given slot, otherwise return invalid.
					return !m_items.ContainsKey(first) == searchNull ? first : eInventorySlot.Invalid;
				}

				// If lower slot is greater than upper slot, flip the values.
				if (first > last)
				{
					eInventorySlot tmp = first;
					first = last;
					last = tmp;
				}

				for (int i = 0; i <= last - first; i++)
				{
					var testSlot = (int) (searchFirst ? (first + i) : (last - i));

					if (!m_items.ContainsKey((eInventorySlot) testSlot) == searchNull)
						return (eInventorySlot) testSlot;
				}

				return eInventorySlot.Invalid;
			}
		}

		#endregion

		#region Find Item

		/// <summary>
		/// Get all the items in the specified range
		/// </summary>
		/// <param name="minSlot">Slot Position where begin the search</param>
		/// <param name="maxSlot">Slot Position where stop the search</param>
		/// <returns>all items found</returns>
		public virtual ICollection<InventoryItem> GetItemRange(eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			minSlot = GetValidInventorySlot(minSlot);
			maxSlot = GetValidInventorySlot(maxSlot);
			if (minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
				return null;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			var items = new List<InventoryItem>();

			lock (m_items)
			{
				InventoryItem item;

				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.TryGetValue(i, out item))
					{
						items.Add(item);
					}
				}
			}

			return items;
		}

		/// <summary>
		/// Searches for the first occurrence of an item with given
		/// ID between specified slots
		/// </summary>
		/// <param name="uniqueID">item ID</param>
		/// <param name="minSlot">fist slot for search</param>
		/// <param name="maxSlot">last slot for search</param>
		/// <returns>found item or null</returns>
		public InventoryItem GetFirstItemByID(string uniqueID, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			minSlot = GetValidInventorySlot(minSlot);
			maxSlot = GetValidInventorySlot(maxSlot);
			if (minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
				return null;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items)
			{
				InventoryItem item;

				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.TryGetValue(i, out item))
					{
						if (item.Id_nb == uniqueID)
							return item;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Searches for the first occurrence of an item with given
		/// objecttype between specified slots
		/// </summary>
		/// <param name="objectType">object Type</param>
		/// <param name="minSlot">fist slot for search</param>
		/// <param name="maxSlot">last slot for search</param>
		/// <returns>found item or null</returns>
		public InventoryItem GetFirstItemByObjectType(int objectType, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			minSlot = GetValidInventorySlot(minSlot);
			maxSlot = GetValidInventorySlot(maxSlot);
			if (minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
				return null;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items)
			{
				InventoryItem item;

				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.TryGetValue(i, out item))
					{
						if (item.Object_Type == objectType)
							return item;
					}
				}
			}

			return null;
		}

		/// <summary>
		/// Searches for the first occurrence of an item with given
		/// name between specified slots
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="minSlot">fist slot for search</param>
		/// <param name="maxSlot">last slot for search</param>
		/// <returns>found item or null</returns>
		public InventoryItem GetFirstItemByName(string name, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			minSlot = GetValidInventorySlot(minSlot);
			maxSlot = GetValidInventorySlot(maxSlot);
			if (minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
				return null;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items)
			{
				InventoryItem item;

				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					if (m_items.TryGetValue(i, out item))
					{
						if (item.Name == name)
							return item;
					}
				}
			}

			return null;
		}

		#endregion

		#region Add/Remove/Move/Get

		/// <summary>
		/// Adds an item to the inventory and DB
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <returns>The eInventorySlot where the item has been added</returns>
		public virtual bool AddItem(eInventorySlot slot, InventoryItem item)
		{
			if (item == null)
				return false;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				slot = GetValidInventorySlot(slot);
				if (slot == eInventorySlot.Invalid) return false;

				if (m_items.ContainsKey(slot))
				{
					if (Log.IsErrorEnabled)
						Log.Error("Inventory.AddItem -> Destination slot is not empty (" + (int) slot + ")\n\n" + Environment.StackTrace);

					return false;
				}

				m_items.Add(slot, item);

				item.SlotPosition = (int)slot;

				if (item.OwnerID != null)
				{
					item.OwnerID = null; // owner ID for NPC
				}

				if (!m_changedSlots.Contains(slot))
					m_changedSlots.Add(slot);

				if (m_changesCounter <= 0)
					UpdateChangedSlots();

				return true;
			}
		}

		public virtual bool AddTradeItem(eInventorySlot slot, InventoryItem item)
		{
			return false;
		}


		/// <summary>
		/// Removes all items from the inventory
		/// </summary>
		public virtual void ClearInventory()
		{
			var tempList = new List<InventoryItem>(m_items.Count);
			foreach (var entry in m_items)
			{
				tempList.Add(entry.Value);
			}

			foreach (InventoryItem item in tempList)
			{
				RemoveItem(item);
			}
		}

		/// <summary>
		/// Removes an item from the inventory
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		public virtual bool RemoveItem(InventoryItem item)
		{
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (item == null)
					return false;

				var slot = (eInventorySlot) item.SlotPosition;

				if (m_items.ContainsKey(slot))
				{
					m_items.Remove(slot);

					if (!m_changedSlots.Contains(slot))
						m_changedSlots.Add(slot);

					item.OwnerID = null;
					item.SlotPosition = (int) eInventorySlot.Invalid;

					if (m_changesCounter <= 0)
						UpdateChangedSlots();

					return true;
				}
			}

			return false;
		}


		public virtual bool RemoveTradeItem(InventoryItem item)
		{
			return false;
		}

		/// <summary>
		/// Adds count of items to the inventory item
		/// </summary>
		/// <param name="item"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public virtual bool AddCountToStack(InventoryItem item, int count)
		{
			if (item == null)
				return false;

			if (count <= 0)
				return false;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				var slot = (eInventorySlot) item.SlotPosition;

				if (m_items.ContainsKey(slot))
				{
					if (item.Count + count > item.MaxCount) return false;

					item.Count += count;

					if (!m_changedSlots.Contains(slot))
						m_changedSlots.Add(slot);

					if (m_changesCounter <= 0)
						UpdateChangedSlots();

					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Removes count of items from the inventory item
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <param name="count">the count of items to be removed from the stack</param>
		/// <returns>true one item removed</returns>
		public virtual bool RemoveCountFromStack(InventoryItem item, int count)
		{
			if (item == null)
				return false;

			if (count <= 0)
				return false;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				var slot = (eInventorySlot) item.SlotPosition;

				if (m_items.ContainsKey(slot))
				{
					if (item.Count < count)
						return false;

					if (item.Count == count)
					{
						return RemoveItem(item);
					}

					item.Count -= count;

					if (!m_changedSlots.Contains(slot))
						m_changedSlots.Add(slot);

					if (m_changesCounter <= 0)
						UpdateChangedSlots();

					return true;
				}

				return false;
			}
		}

		/// <summary>
		/// Get the item to the inventory in the specified slot
		/// </summary>
		/// <param name="slot">SlotPosition</param>
		/// <returns>the item in the specified slot if the slot is valid and null if not</returns>
		public virtual InventoryItem GetItem(eInventorySlot slot)
		{
			slot = GetValidInventorySlot(slot);
			if (slot == eInventorySlot.Invalid)
				return null;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{			          
				InventoryItem item;
				m_items.TryGetValue(slot, out item);
					return item;
				//else
				//	return null;
			}
		}

		/// <summary>
		/// Exchange two Items in form specified slot
		/// </summary>
		/// <param name="fromSlot">Source slot</param>
		/// <param name="toSlot">Destination slot</param>
		/// <param name="itemCount"></param>
		/// <returns>true if successfull false if not</returns>
		public virtual bool MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				fromSlot = GetValidInventorySlot(fromSlot);
				toSlot = GetValidInventorySlot(toSlot);
				if (fromSlot == eInventorySlot.Invalid || toSlot == eInventorySlot.Invalid)
					return false;

				InventoryItem fromItem;
				InventoryItem toItem;

				m_items.TryGetValue(fromSlot, out fromItem);
				m_items.TryGetValue(toSlot, out toItem);

				if (!CombineItems(fromItem, toItem) && !StackItems(fromSlot, toSlot, itemCount))
				{
					ExchangeItems(fromSlot, toSlot);
				}

				if (!m_changedSlots.Contains(fromSlot))
					m_changedSlots.Add(fromSlot);

				if (!m_changedSlots.Contains(toSlot))
					m_changedSlots.Add(toSlot);

				if (m_changesCounter <= 0)
					UpdateChangedSlots();

				return true;
			}
		}

		/// <summary>
		/// Get the list of all visible items
		/// </summary>
		public virtual ICollection<InventoryItem> VisibleItems
		{
			get
			{
				var items = new List<InventoryItem>(VISIBLE_SLOTS.Length);

				lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach (eInventorySlot slot in VISIBLE_SLOTS)
					{
						InventoryItem item;

						if (m_items.TryGetValue(slot, out item))
						{
							items.Add(item);
						}
					}
				}

				return items;
			}
		}

		/// <summary>
		/// Get the list of all equipped items
		/// </summary>
		public virtual ICollection<InventoryItem> EquippedItems
		{
			get
			{
				var items = new List<InventoryItem>(EQUIP_SLOTS.Length);

				lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach (eInventorySlot slot in EQUIP_SLOTS)
					{
						InventoryItem item;

						if (m_items.TryGetValue(slot, out item))
						{
							items.Add(item);
						}
					}
				}

				return items;
			}
		}

		/// <summary>
		/// Get the list of all items in the inventory
		/// </summary>
		public virtual ICollection<InventoryItem> AllItems
		{
			get { return m_items.Values; }
		}

		#endregion

		#region AddTemplate/RemoveTemplate

		/// <summary>
		/// Adds needed amount of items to inventory if there
		/// is enough space else nothing is done
		/// </summary>
		/// <param name="template">The ItemTemplate</param>
		/// <param name="count">The count of items to add</param>
		/// <param name="minSlot">The first slot</param>
		/// <param name="maxSlot">The last slot</param>
		/// <returns>True if all items were added</returns>
		public virtual bool AddTemplate(InventoryItem  template, int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			// Make sure template isn't null.
			if (template == null)
				return false;

			// Make sure we have a positive item count.
			if (count <= 0)
				return false;

			// If lower slot is greater than upper slot, flip the values.
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			// Make sure lower slot is within inventory bounds.
			if (minSlot < eInventorySlot.Min_Inv)
				return false;

			// Make sure upper slot is within inventory bounds.
			if (maxSlot > eInventorySlot.Max_Inv)
				return false;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				var changedSlots = new Dictionary<eInventorySlot, int>(); // value: <0 = new item count; >0 = add to old
				bool fits = false;
				int itemcount = count;
				eInventorySlot i = minSlot;

				// Find open slots/existing stacks to fit in these items.
				do
				{
					// Get the current slot, make sure it's valid.
					eInventorySlot curSlot = GetValidInventorySlot(i);
					if (curSlot == eInventorySlot.Invalid)
						continue;

					InventoryItem curItem;

					// Make sure slot isn't empty.
					if (!m_items.TryGetValue(curSlot, out curItem))
						continue;

					// If slot isn't of the same type as our template, we can't stack, so skip.
					if (curItem.Id_nb != template.Id_nb)
						continue;

					// Can't add to an already maxed-out stack.
					if (curItem.Count >= curItem.MaxCount)
						continue;

					// Get the number of free spaces left in the given stack.
					int countFree = curItem.MaxCount - curItem.Count;

					// See if we can fit all our items in the given stack.  If not, fit what we can.
					int countAdd = count;
					if (countAdd > countFree)
						countAdd = countFree;

					// Set the number of items to add to the stack at the given slot. (positive value indicates we're adding to a stack, not to an empty slot.)
					changedSlots[curSlot] = countAdd;

					// Reduce the overall count of items we need to fit.
					count -= countAdd;

					// If we've fit everything in existing stacks, we're done trying to find matches.
					if (count == 0)
					{
						fits = true;
						break;
					}

					// Exceptional error, count should never be negative.
					if (count < 0)
					{
						throw new Exception("Count is less than zero while filling gaps, should never happen!");
					}
				} while (++i <= maxSlot);

				if (!fits)
				{
					// We couldn't manage to find existing stacks, or enough existing stacks, to add our items to.  
					// We now need to find totally open slots to put our items in.
					for (i = minSlot; i <= maxSlot; i++)
					{
						// Get the current slot, make sure it's valid.
						eInventorySlot curSlot = GetValidInventorySlot(i);
						if (curSlot == eInventorySlot.Invalid)
							continue;

						// Skip any slots we already found as being occupied.
						if (changedSlots.ContainsKey(curSlot))
							continue;

						// Skip any slots that are already in use.
						if (m_items.ContainsKey(curSlot))
							continue;

						// If the max stack count is less than remaining items to add, we can only add the max
						// stack count and must find remaining slots to allocate the rest of the items to.
						int countAdd = count;
						if (countAdd > template.MaxCount)
							countAdd = template.MaxCount;

						// Set the number of items to add to the given slot. (negative amount indicates we're adding a new item, not stacking)
						changedSlots[curSlot] = -countAdd;

						// Reduce the overall count of items we need to fit.
						count -= countAdd;

						// If we've fit everything in existing stacks and open slots, we're done trying to find matches.
						if (count == 0)
						{
							fits = true;
							break;
						}

						// Exceptional error, count should never be negative.
						if (count < 0)
						{
							throw new Exception("Count is less than zero while adding new items, should never happen!");
						}
					}
				}

				// If we still weren't able to fit all the items, then this is a failed add.
				if (!fits)
					return false;

				// Add new items
				BeginChanges();

				try
				{
					foreach (var de in changedSlots)
					{
						InventoryItem item;
						eInventorySlot itemSlot = de.Key;
						int itemCount = de.Value;

						if (itemCount > 0) // existing item should be changed
						{
							if (m_items.TryGetValue(itemSlot, out item))
							{
								AddCountToStack(item, itemCount);
							}
						}
						else if (itemCount < 0) // new item should be added
						{
							item = new InventoryItem(template);
							item.Count = -itemCount;
							AddItem(itemSlot, item);
						}
					}
				}
				finally
				{
					CommitChanges();
				}

				return true;
			}
		}

		/// <summary>
		/// Removes needed amount of items from inventory if
		/// enough amount of items are in inventory
		/// </summary>
		/// <param name="templateID">The ItemTemplate ID</param>
		/// <param name="count">The count of items to add</param>
		/// <param name="minSlot">The first slot</param>
		/// <param name="maxSlot">The last slot</param>
		/// <returns>True if all items were added</returns>
		public virtual bool RemoveTemplate(string templateID, int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			if (templateID == null)
				return false;

			if (count <= 0)
				return false;

			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			if (minSlot < eInventorySlot.Min_Inv) return false;
			if (maxSlot > eInventorySlot.Max_Inv) return false;

			lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				var changedSlots = new Dictionary<InventoryItem, int>();
				// value: null = remove item completely; >0 = remove count from stack
				bool remove = false;

				for (eInventorySlot slot = minSlot; slot <= maxSlot; slot++)
				{
					InventoryItem item;

					if (!m_items.TryGetValue(slot, out item))
						continue;

					if (item.Id_nb != templateID)
						continue;

					if (count >= item.Count)
					{
						count -= item.Count;
						changedSlots.Add(item, -1); // remove completely
					}
					else
					{
						changedSlots.Add(item, count); // remove count
						count = 0;
					}

					if (count == 0)
					{
						remove = true;
						break;
					}
					else if (count < 0)
					{
						throw new Exception("Count less than zero while removing template.");
					}
				}

				if (!remove)
					return false;


				BeginChanges();

				try
				{
					foreach (var de in changedSlots)
					{
						InventoryItem item = de.Key;

						if (de.Value == -1)
						{
							if (!RemoveItem(item))
							{
								CommitChanges();
								throw new Exception("Error removing item.");
							}
						}
						else if (!RemoveCountFromStack(item, de.Value))
						{
							CommitChanges();
							throw new Exception("Error removing count from stack.");
						}
					}
				}
				finally
				{
					CommitChanges();
				}

				return true;
			}
		}

		#endregion

		#region Combine/Exchange/Stack Items

		/// <summary>
		/// Combine 2 items together if possible
		/// </summary>
		/// <param name="fromItem">First Item</param>
		/// <param name="toItem">Second Item</param>
		/// <returns>true if items combined successfully</returns>
		protected virtual bool CombineItems(InventoryItem fromItem, InventoryItem toItem)
		{
			return false;
		}

		/// <summary>
		/// Stack an item with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <param name="itemCount">How many items to move</param>
		/// <returns>true if items stacked successfully</returns>
		protected virtual bool StackItems(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount)
		{
			return false;
		}

		/// <summary>
		/// Exchange one item position with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <returns>true if items exchanged successfully</returns>
		protected virtual bool ExchangeItems(eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			InventoryItem newFromItem;
			InventoryItem newToItem;

			m_items.TryGetValue(fromSlot, out newToItem);
			m_items.TryGetValue(toSlot, out newFromItem);

			// Make sure one of the slots has an item, otherwise there is nothing to exchange.
			if (newFromItem == null && newToItem == null)
				return false;

			// Swap the items.
			m_items[fromSlot] = newFromItem;
			m_items[toSlot] = newToItem;

			// If 'toSlot' wasn't empty, adjust the slot position for the item now in 'fromSlot', otherwise clear the new slot.
			if (newFromItem != null)
			{
				newFromItem.SlotPosition = (int) fromSlot;
			}
			else
			{
				m_items.Remove(fromSlot);
			}

			// If 'fromSlot' wasn't empty, adjust the slot position for the item now in 'toSlot', otherwise clear the new slot.
			if (newToItem != null)
			{
				newToItem.SlotPosition = (int) toSlot;
			}
			else
			{
				m_items.Remove(toSlot);
			}

			return true;
		}

		#endregion Combine/Exchange/Stack Items

		#region Encumberance

		/// <summary>
		/// Gets the inventory weight
		/// </summary>
		public virtual int InventoryWeight
		{
			get
			{
				InventoryItem item;
				int weight = 0;

				lock (m_items) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach (eInventorySlot slot in EQUIP_SLOTS)
					{
						if (m_items.TryGetValue(slot, out item))
						{
							weight += item.Weight;
						}
					}
				}

				return weight/10;
			}
		}

		#endregion

		#region BeginChanges/CommitChanges/UpdateSlots

		/// <summary>
		/// Increments changes counter
		/// </summary>
		public void BeginChanges()
		{
			Interlocked.Increment(ref m_changesCounter);
		}

		/// <summary>
		/// Commits changes if all started changes are finished
		/// </summary>
		public void CommitChanges()
		{
			int changes = Interlocked.Decrement(ref m_changesCounter);
			if (changes < 0)
			{
				if (Log.IsErrorEnabled)
					Log.Error("Inventory changes counter is below zero (forgot to use BeginChanges?)!\n\n" + Environment.StackTrace);

				Thread.VolatileWrite(ref m_changesCounter, 0);
			}

			if (changes <= 0 && m_changedSlots.Count > 0)
			{
				UpdateChangedSlots();
			}
		}

		/// <summary>
		/// Updates changed slots, inventory is already locked
		/// </summary>
		protected virtual void UpdateChangedSlots()
		{
			m_changedSlots.Clear();
		}

		#endregion

		//Defines all the slots that hold equipment
	}
}