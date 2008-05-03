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
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using System.Threading;
using DOL.Database2;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Description rsume de GameLivingInventory.
	/// </summary>
	public abstract class GameLivingInventory : IGameInventory
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

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

		//Defines all the slots that hold equipment
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

		#region Constructor/Declaration/LoadDatabase/SaveDatabase
		/// <summary>
		/// The complete inventory of all living including
		/// for players the vault, the equipped items and the backpack
		/// and for mob the quest drops ect ...
		/// </summary>
		protected readonly HybridDictionary  m_items;

		/// <summary>
		/// Holds the begin changes counter for slot updates
		/// </summary>
		protected int m_changesCounter;

		/// <summary>
		/// Holds all changed slots
		/// </summary>
		protected ArrayList m_changedSlots;

		/// <summary>
		/// Constructs a new empty inventory
		/// </summary>
		public GameLivingInventory()
		{
			m_items = new HybridDictionary();
			m_changedSlots = new ArrayList(1);
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
		/// Check if the slot is valid in the inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected virtual eInventorySlot GetValidInventorySlot(eInventorySlot slot)
		{
			if(    ( slot >= eInventorySlot.RightHandWeapon && slot <= eInventorySlot.FourthQuiver)
				|| ( slot >= eInventorySlot.HeadArmor && slot <= eInventorySlot.Neck)
				|| ( slot >= eInventorySlot.HorseArmor && slot <= eInventorySlot.Horse)
				|| ( slot >= eInventorySlot.Waist && slot <= eInventorySlot.Mythical)
				|| ( slot == eInventorySlot.Ground))
				return slot;

			return eInventorySlot.Invalid;
		}

		/// <summary>
		/// Counts used/free slots between min and max
		/// </summary>
		/// <param name="countUsed"></param>
		/// <param name="minSlot"></param>
		/// <param name="maxSlot"></param>
		/// <returns></returns>
		public int CountSlots(bool countUsed, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			if(minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			int result = 0;

			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				for(int i = (int)minSlot; i <= (int)(maxSlot); i++)
				{
					if (m_items.Contains(i))
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
			lock (m_items.SyncRoot)// Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{ 
				int count = 0;
				if (minSlot > maxSlot)
				{
					eInventorySlot tmp = minSlot;
					minSlot = maxSlot;
					maxSlot = tmp;
				}

				for (int i = (int)minSlot; i <= (int)(maxSlot); i++)
				{
					InventoryItem item = (InventoryItem)m_items[i];
					if (item != null && item.Id_nb == itemtemplateID) count += item.Count;
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
			if(count < 1) return true;
			if(minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				for(int i = (int)minSlot; i <= (int)(maxSlot); i++)
				{
					if (m_items.Contains(i)) continue;
					count--;
					if (count <= 0)
						return true;
				}

				return false;
			}
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
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				first = GetValidInventorySlot(first);
				last = GetValidInventorySlot(last);
				if(first == eInventorySlot.Invalid || last == eInventorySlot.Invalid)
					return eInventorySlot.Invalid;

				if(first == last)
				{
					if((m_items[(int)first]==null) == searchNull)
						return first;
      
					return eInventorySlot.Invalid;
				}
    
				if(first > last)
				{
					eInventorySlot tmp = first;
					first = last;
					last = tmp;
				}

				for(int i = 0; i <= last-first; i++)
				{
					int testSlot = (int)(searchFirst?(first+i):(last-i));
					if((m_items[testSlot]==null) == searchNull)
						return (eInventorySlot) testSlot;
				}
				return eInventorySlot.Invalid;
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
			return FindSlot(first,last,true,true);
		}

		/// <summary>
		/// Find the last empty slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindLastEmptySlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first,last,false,true);
		}

		/// <summary>
		/// Find the first full slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindFirstFullSlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first,last,true,false);
		}

		/// <summary>
		/// Find the last full slot in the inventory
		/// </summary>
		/// <param name="first">SlotPosition to start the search</param>
		/// <param name="last">SlotPosition to stop the search</param>
		/// <returns>the empty inventory slot or eInventorySlot.Invalid</returns>
		public virtual eInventorySlot FindLastFullSlot(eInventorySlot first, eInventorySlot last)
		{
			return FindSlot(first,last,false,false);
		}
		#endregion

		#region Find Item
		/// <summary>
		/// Get all the items in the specified range
		/// </summary>
		/// <param name="minSlot">Slot Position where begin the search</param>
		/// <param name="maxSlot">Slot Position where stop the search</param>
		/// <returns>all items found</returns>
		public virtual ICollection GetItemRange(eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			minSlot = GetValidInventorySlot(minSlot);
			maxSlot = GetValidInventorySlot(maxSlot);
			if (minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
				return null;

			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			ArrayList items = new ArrayList();
			lock (m_items.SyncRoot)
			{
				for (int i = (int)minSlot; i <= (int)maxSlot; i++)
				{
					InventoryItem item = m_items[i] as InventoryItem;
					if (item != null)
						items.Add(item);
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

			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items.SyncRoot)
			{
				for (int i = (int)minSlot; i <= (int)maxSlot; i++)
				{
					InventoryItem item = m_items[i] as InventoryItem;
					if (item != null)
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

			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items.SyncRoot)
			{
				for (int i = (int)minSlot; i <= (int)maxSlot; i++)
				{
					InventoryItem item = m_items[i] as InventoryItem;
					if (item != null)
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

			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}

			lock (m_items.SyncRoot)
			{
				for (int i = (int)minSlot; i <= (int)maxSlot; i++)
				{
					InventoryItem item = m_items[i] as InventoryItem;
					if (item != null)
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
			if (item == null) return false;
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				slot = GetValidInventorySlot(slot);
				if (slot == eInventorySlot.Invalid) return false;
				if (m_items.Contains((int)slot))
				{
					if (log.IsErrorEnabled)
						log.Error("Inventory.AddItem -> Destination slot is not empty ("+(int)slot+")\n\n" + Environment.StackTrace);
					return false;
				}
				m_items.Add((int)slot, item);
				item.SlotPosition=(int)slot;
				if (item.OwnerID != null)
				{
					item.OwnerID = null; // owner ID for NPC
				}

				if (!m_changedSlots.Contains((int)slot))
					m_changedSlots.Add((int)slot);
				if (m_changesCounter <= 0)
					UpdateChangedSlots();
				return true;
			}
		}

		/// <summary>
		/// Removes all items from the inventory
		/// </summary>
		public virtual void ClearInventory()
		{
			List<InventoryItem> tempList = new List<InventoryItem>(m_items.Values.Count);
			foreach (InventoryItem item in m_items.Values)
				tempList.Add(item);
			foreach (InventoryItem item in tempList)
				RemoveItem(item);
		}

		/// <summary>
		/// Removes an item from the inventory
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		public virtual bool RemoveItem(InventoryItem item)
		{
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (item == null) return false;
				if (m_items.Contains(item.SlotPosition))
				{
					m_items.Remove(item.SlotPosition);

					if (!m_changedSlots.Contains(item.SlotPosition))
						m_changedSlots.Add(item.SlotPosition);

					item.OwnerID = null;
					item.SlotPosition = (int)eInventorySlot.Invalid;

					if (m_changesCounter <= 0)
						UpdateChangedSlots();
					return true;
				}
			}
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
			if (item == null) return false;
			if (count <= 0) return false;
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (m_items.Contains(item.SlotPosition))
				{
					if(item.Count+count > item.MaxCount) return false;

					item.Weight += item.Weight/item.Count*count;
					item.Count += count;

					if (!m_changedSlots.Contains(item.SlotPosition))
						m_changedSlots.Add(item.SlotPosition);
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
			if (item == null) return false;
			if (count <= 0) return false;
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				if (m_items.Contains(item.SlotPosition))
				{
					if(item.Count < count) return false;
					int itemSlot = item.SlotPosition;
					if(item.Count == count)
					{
						return RemoveItem(item);
					}
					else
					{
						item.Weight -= item.Weight/item.Count*count;
						item.Count -= count;

						if (!m_changedSlots.Contains(itemSlot))
							m_changedSlots.Add(itemSlot);
						if (m_changesCounter <= 0)
							UpdateChangedSlots();
						return true;
					}
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
			if (slot == eInventorySlot.Invalid) return null;
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				return (m_items[(int)slot] as InventoryItem);
			}
		}

		/// <summary>
		/// Exchange two Items in form specified slot
		/// </summary>
		/// <param name="fromSlot">Source slot</param>
		/// <param name="toSlot">Destination slot</param>
		/// <param name="itemCount"></param>
		/// <returns>true if successfull false if not</returns>
		public virtual bool MoveItem(eInventorySlot fromSlot,eInventorySlot toSlot, int itemCount)
		{
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				fromSlot = GetValidInventorySlot(fromSlot);
				toSlot = GetValidInventorySlot(toSlot);
				if (fromSlot == eInventorySlot.Invalid || toSlot == eInventorySlot.Invalid)
					return false;

				if (!CombineItems((InventoryItem)m_items[(int)fromSlot], (InventoryItem)m_items[(int)toSlot]) &&
					!StackItems((int)fromSlot, (int)toSlot, itemCount)) 
				{
					ExchangeItems((int)fromSlot, (int)toSlot);
				}

				if (!m_changedSlots.Contains((int)fromSlot))
					m_changedSlots.Add((int)fromSlot);
				if (!m_changedSlots.Contains((int)toSlot))
					m_changedSlots.Add((int)toSlot);
				if (m_changesCounter <= 0)
					UpdateChangedSlots();
				return true;
			}
		}

		/// <summary>
		/// Get the list of all visible items
		/// </summary>
		public virtual ICollection VisibleItems  
		{
			get
			{
				ArrayList items = new ArrayList(VISIBLE_SLOTS.Length);
				lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach(eInventorySlot slot in VISIBLE_SLOTS)
					{
						object item = m_items[(int)slot];
						if(item!=null) items.Add(item);
					}
				}
				return items;
			}
		}

		/// <summary>
		/// Get the list of all equipped items
		/// </summary>
		public virtual ICollection EquippedItems 
		{ 
			get
			{
				ArrayList items = new ArrayList();
				lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach(eInventorySlot slot in EQUIP_SLOTS)
					{
						object item = m_items[(int)slot];
						if(item!=null) items.Add(item);
					}
				}
				return items;    
			}
		}

		/// <summary>
		/// Get the list of all items in the inventory
		/// </summary>
		public virtual ICollection AllItems
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
		public virtual bool AddTemplate(ItemTemplate template, int count, eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			if (template == null) return false;
			if (count <= 0) return false;
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}
			if (minSlot < eInventorySlot.Min_Inv) return false;
			if (maxSlot > eInventorySlot.Max_Inv) return false;
			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				Hashtable changedSlots = new Hashtable(); // value: <0 = new item count; >0 = add to old
				bool fits = false;
				int itemcount = count;
				eInventorySlot i = minSlot;

				// fill the gaps in existing stacks
				do
				{
					eInventorySlot curSlot = GetValidInventorySlot(i);
					if (curSlot == eInventorySlot.Invalid) continue; // skip slots not valid in this inventory

					InventoryItem curItem = (InventoryItem)m_items[(int)curSlot];
					if (curItem == null) continue; // skip empty slots
					if (curItem.Id_nb != template.Id_nb) continue;
					if (curItem.Count >= curItem.MaxCount) continue;

					int countFree = curItem.MaxCount - curItem.Count;
					int countAdd = count;
					if (countAdd > countFree)
						countAdd = countFree;
					changedSlots[(int)curSlot] = countAdd; // existing item should be changed
					count -= countAdd;

					if (count == 0)
					{
						fits = true;
						break;
					}
					else if (count < 0)
					{
						throw new Exception("Count is less than zero while filling gaps, should never happen!");
					}
				} while (++i <= maxSlot);

				if (!fits)
				{
					// still not fits? add new items.
					for (i = minSlot; i <= maxSlot; i++)
					{
						eInventorySlot curSlot = GetValidInventorySlot(i);
						if (curSlot == eInventorySlot.Invalid) continue; // skip slots not valid in this inventory
						if (changedSlots.Contains((int)curSlot)) continue; // skip reserved slots
						if (m_items.Contains((int)curSlot)) continue; // skip used slots

						int countAdd = count;
						if (countAdd > template.MaxCount)
							countAdd = template.MaxCount;
						changedSlots[(int)curSlot] = -countAdd; // new item should be added
						count -= countAdd;

						if (count == 0)
						{
							fits = true;
							break;
						}
						else if (count < 0)
						{
							throw new Exception("Count is less than zero while adding new items, should never happen!");
						}
					}
				}

				if (!fits) return false;

				// Add new items
				BeginChanges();

				try
				{
					foreach (DictionaryEntry de in changedSlots)
					{
						int slot = (int)de.Key;
						int value = (int)de.Value;
						if (value > 0) // existing item should be changed
						{
							InventoryItem item = (InventoryItem)m_items[slot];
							AddCountToStack(item, value);
						}
						else if (value < 0) // new item should be added
						{
							InventoryItem item = new InventoryItem(template);
							item.Count = -value;
							item.Weight = (template.Weight / itemcount) * -value;
							AddItem((eInventorySlot)slot, item);
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
			if (templateID == null) return false;
			if (count <= 0) return false;
			if (minSlot > maxSlot)
			{
				eInventorySlot tmp = minSlot;
				minSlot = maxSlot;
				maxSlot = tmp;
			}
			if (minSlot < eInventorySlot.Min_Inv) return false;
			if (maxSlot > eInventorySlot.Max_Inv) return false;

			lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
			{
				Hashtable changedSlots = new Hashtable(); // value: null = remove item completely; >0 = remove count from stack
				bool remove = false;
				for (eInventorySlot i = minSlot; i <= maxSlot; i++)
				{
					InventoryItem item = (InventoryItem)m_items[(int)i];
					if (item == null) continue;
					if (item.Id_nb != templateID) continue;

					if (count >= item.Count)
					{
						count -= item.Count;
						changedSlots.Add(item, null); // remove completely
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

				if (!remove) return false;


				BeginChanges();

				try
				{
					foreach (DictionaryEntry de in changedSlots)
					{
						InventoryItem item = (InventoryItem)de.Key;
						if (de.Value == null)
						{
							if (!RemoveItem(item))
							{
								CommitChanges();
								throw new Exception("Error removing item.");
							}
						}
						else if (!RemoveCountFromStack(item, (int)de.Value))
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
		protected virtual bool StackItems(int fromSlot,int toSlot, int itemCount)
		{
			return false;
		}

		/// <summary>
		/// Exchange one item position with another one
		/// </summary>
		/// <param name="fromSlot">First SlotPosition</param>
		/// <param name="toSlot">Second SlotPosition</param>
		/// <returns>true if items exchanged successfully</returns>
		protected virtual bool ExchangeItems(int fromSlot, int toSlot)
		{
			InventoryItem newFromItem = (InventoryItem)m_items[toSlot];
			InventoryItem newToItem = (InventoryItem)m_items[fromSlot];
			m_items[fromSlot] = newFromItem;
			m_items[toSlot] = newToItem;

			if (newFromItem != null)
				newFromItem.SlotPosition = fromSlot;
			else
				m_items.Remove(fromSlot);

			if (newToItem != null)
				newToItem.SlotPosition = toSlot;
			else
				m_items.Remove(toSlot);

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
				InventoryItem item = null;
				int weight = 0;
				lock (m_items.SyncRoot) // Mannen 10:56 PM 10/30/2006 - Fixing every lock(this)
				{
					foreach(eInventorySlot slot in EQUIP_SLOTS)
					{
						item = m_items[(int)slot] as InventoryItem;
						if(item != null)
							weight += item.Weight;
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
				if (log.IsErrorEnabled)
					log.Error("Inventory changes counter is bellow zero (forgot to use BeginChanges?)!\n\n" + Environment.StackTrace);
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
	}
}
