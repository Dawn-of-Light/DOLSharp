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
using System.Collections.Specialized;
using System.Reflection;
using System.Text;
using DOL.GS.Database;
using log4net;

namespace DOL.GS
{
	public enum eInventorySlot : int
	{
		LastEmptyQuiver		= -6,
		FirstEmptyQuiver	= -5,
		LastEmptyVault      = -4,
		FirstEmptyVault     = -3,
		LastEmptyBackpack   = -2,
		FirstEmptyBackpack  = -1,

		Invalid           = 0,
		Ground            = 1,

		Min_Inv           = 10,

		RightHandWeapon   = 10,//Equipment, Visible
		LeftHandWeapon    = 11,//Equipment, Visible
		TwoHandWeapon     = 12,//Equipment, Visible
		DistanceWeapon    = 13,//Equipment, Visible
		FirstQuiver		  = 14,
		SecondQuiver	  = 15,
		ThirdQuiver		  = 16,
		FourthQuiver	  = 17,
		HeadArmor         = 21,//Equipment, Visible
		HandsArmor        = 22,//Equipment, Visible
		FeetArmor         = 23,//Equipment, Visible
		Jewellery         = 24,//Equipment
		TorsoArmor        = 25,//Equipment, Visible
		Cloak             = 26,//Equipment, Visible
		LegsArmor         = 27,//Equipment, Visible
		ArmsArmor         = 28,//Equipment, Visible
		Neck              = 29,//Equipment
		Waist             = 32,//Equipment
		LeftBracer        = 33,//Equipment
		RightBracer       = 34,//Equipment
		LeftRing          = 35,//Equipment
		RightRing         = 36,//Equipment

		FirstBackpack     = 40,
		LastBackpack      = 79,

		Mithril			  = 80,
		Platinum		  = 81,
		Gold			  = 82,
		Silver			  = 83,
		Copper			  = 84,

		PlayerPaperDoll   = 100,
		FirstVault        = 110,
		LastVault         = 149,

		Max_Inv			  = 149,

		// money slots changed since 178
		Mithril178		  = 500,
		Platinum178		  = 501,
		Gold178			  = 502,
		Silver178		  = 503,
		Copper178		  = 504,
		PlayerPaperDoll178   = 600,
		
	}

	/// <summary>
	/// Description rsume de GameLivingInventory.
	/// </summary>
	public abstract class GameLivingInventory
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		//Defines the visible slots that will be displayed to players
		public static readonly eInventorySlot[] VISIBLE_SLOTS = 
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
		};  
  
		//Defines all slots where a armor part can be equipped
		public static readonly eInventorySlot[] ARMOR_SLOTS = 
		{ 
			eInventorySlot.HeadArmor,
			eInventorySlot.HandsArmor,
			eInventorySlot.FeetArmor,
			eInventorySlot.TorsoArmor,
			eInventorySlot.LegsArmor,
			eInventorySlot.ArmsArmor
		};   

		#region Declaration

		/// <summary>
		/// The unique ID of the inventory
		/// </summary>
		protected int m_id;	
	
		/// <summary>
		/// Returns the unique ID of this inventory
		/// </summary>
		public int InventoryID
		{
			get { return m_id; }
			set { m_id = value; }
		}

		/// <summary>
		/// The complete inventory of all living including
		/// for players the vault, the equipped items and the backpack
		/// and for mob the quest drops ect ...
		/// </summary>
		protected IDictionary  m_items = new HybridDictionary();

		/// <summary>
		/// Get or set the inventory hash
		/// </summary>
		public virtual IDictionary InventoryItems
		{
			get { return m_items; }
			set { m_items = value; }
		}

		#endregion

		#region Get Inventory Informations
		/// <summary>
		/// Check if the slot is valid in this inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected abstract eInventorySlot GetValidInventorySlot(eInventorySlot slot);

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

			lock (this)
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
		private eInventorySlot FindSlot(eInventorySlot first, eInventorySlot last, bool searchFirst, bool searchNull)
		{
			lock (this)
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
			lock (this)
			{
				minSlot = GetValidInventorySlot(minSlot);
				maxSlot = GetValidInventorySlot(maxSlot);
				if(minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
					return null;

				if(minSlot > maxSlot)
				{
					eInventorySlot tmp = minSlot;
					minSlot = maxSlot;
					maxSlot = tmp;
				}

				ArrayList items = new ArrayList();
				for(int i=(int)minSlot; i<=(int)maxSlot;i++)
				{
					if (m_items.Contains(i))
						items.Add(m_items[i]);
				}
				return items;
			}
		}

		/// <summary>
		/// Searches for the first occurrence of an item with given
		/// name between specified slots
		/// </summary>
		/// <param name="name">name</param>
		/// <param name="minSlot">fist slot for search</param>
		/// <param name="maxSlot">last slot for search</param>
		/// <returns>found item or null</returns>
		public GenericItem GetFirstItemByName(string name ,eInventorySlot minSlot, eInventorySlot maxSlot)
		{
			lock (this)
			{		
				minSlot = GetValidInventorySlot(minSlot);
				maxSlot = GetValidInventorySlot(maxSlot);
				if(minSlot == eInventorySlot.Invalid || maxSlot == eInventorySlot.Invalid)
					return null;

				if(minSlot > maxSlot)
				{
					eInventorySlot tmp = minSlot;
					minSlot = maxSlot;
					maxSlot = tmp;
				}

				for(int i=(int)minSlot; i<=(int)maxSlot;i++)
				{
					GenericItem item = m_items[i] as GenericItem;
					if (item!=null)
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
		public abstract bool AddItem(eInventorySlot slot, GenericItemBase item);

		/// <summary>
		/// Removes an item from the inventory
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>true if successfull</returns>
		public abstract bool RemoveItem(GenericItemBase item);

		/// <summary>
		/// Get the item to the inventory in the specified slot
		/// </summary>
		/// <param name="slot">SlotPosition</param>
		/// <returns>the item in the specified slot if the slot is valid and null if not</returns>
		public virtual GenericItemBase GetItem(eInventorySlot slot)
		{
			lock (this)
			{
				slot = GetValidInventorySlot(slot);
				if (slot == eInventorySlot.Invalid) return null;
				return (m_items[(int)slot] as GenericItem);
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
				lock (this)
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
				lock (this)
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
		/// Get the list of all equipped armor items
		/// </summary>
		public virtual ICollection ArmorItems 
		{ 
			get
			{
				ArrayList items = new ArrayList();
				lock (this)
				{
					foreach(eInventorySlot slot in ARMOR_SLOTS)
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

		#region Encumberance
		/// <summary>
		/// Gets the inventory weight
		/// </summary>
		public virtual int InventoryWeight
		{ 
			get
			{
				GenericItem item = null;
				int weight=0;
				lock (this)
				{
					foreach(eInventorySlot slot in EQUIP_SLOTS)
					{
						item = m_items[(int)slot] as GenericItem;
						if(item!=null)
							weight+=item.Weight;
					}
				}
				return weight/10;
			}
		}	
		#endregion

		#region IsCloakHoodUp
		/// <summary>
		/// Holds the living's cloak hood state
		/// </summary>
		protected bool m_isCloakHoodUp;

		/// <summary>
		/// Sets/gets the living's cloak hood state
		/// </summary>
		public virtual bool IsCloakHoodUp
		{
			get { return m_isCloakHoodUp; }
			set { m_isCloakHoodUp = value; }
		}
		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			StringBuilder str = new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" InventoryID =").Append(m_id)
				.Append(" weight =").Append(InventoryWeight)
				.Append(" Items =[");
				foreach(GenericItemBase item in InventoryItems.Values)
				{
					str.Append(item.ToString())
					.Append(" , ");
				}
				str.Append(" ]");
				return str.ToString();
		}
	}
}
