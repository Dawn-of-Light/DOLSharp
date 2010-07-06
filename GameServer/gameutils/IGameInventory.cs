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
using System.Collections.Generic;

namespace DOL.GS
{
	public enum eInventorySlot : int
	{
		LastEmptyBagHorse	= -8,
		FirstEmptyBagHorse	= -7,
		LastEmptyQuiver		= -6,
		FirstEmptyQuiver	= -5,
		LastEmptyVault      = -4,
		FirstEmptyVault     = -3,
		LastEmptyBackpack   = -2,
		FirstEmptyBackpack  = -1,

		Invalid           = 0,
		Ground            = 1,

		Min_Inv           = 7,

		HorseArmor        = 7, // Equipment, horse armor
		HorseBarding      = 8, // Equipment, horse barding
		Horse             = 9, // Equipment, horse

		MinEquipable	  = 10,
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
		Mythical		  = 37,
		MaxEquipable	  = 37,

		FirstBackpack     = 40,
		LastBackpack      = 79,
		
		FirstBagHorse	= 80,
		LastBagHorse	= 95,

		PlayerPaperDoll   = 100,
		
		Mithril			  = 101,
		Platinum		  = 102,
		Gold			  = 103,
		Silver			  = 104,
		Copper			  = 105,
		
		FirstVault        = 110,
		LastVault         = 149,

		HousingInventory_First = 150,
		HousingInventory_Last = 249,	

		HouseVault_First = 1000,
		HouseVault_Last = 1399,

		Consignment_First = 1500,
		Consignment_Last = 1599,

        MarketExplorerFirst = 1000,

		//FirstFixLoot      = 256, //You can define drops that will ALWAYS occur (eg quest drops etc.)
		//LastFixLoot       = 356, //100 drops should be enough ... if not, just raise this var, we have thousands free
		//LootPagesStart    = 500, //Let's say each loot page is 100 slots in size, lots of space for random drops
		
		// money slots changed since 178
		Mithril178		  = 500,
		Platinum178		  = 501,
		Gold178			  = 502,
		Silver178		  = 503,
		Copper178		  = 504,
		NewPlayerPaperDoll= 600,

		Max_Inv = 249,
	}

	/// <summary>
	/// Interface for GameInventory
	/// </summary>		
	public interface IGameInventory
	{
		bool            LoadFromDatabase(string inventoryID);
		bool            SaveIntoDatabase(string inventoryID);

		bool			AddItem(eInventorySlot slot, InventoryItem item);
						/// <summary>
						/// Add an item to Inventory and save.  This assumes item is already in the database and is being transferred.
						/// </summary>
						/// <param name="slot"></param>
						/// <param name="item"></param>
						/// <returns></returns>
		bool			AddTradeItem(eInventorySlot slot, InventoryItem item);
		bool			AddCountToStack(InventoryItem item, int count);
		bool			AddTemplate(InventoryItem template, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
		bool            RemoveItem(InventoryItem item);
						/// <summary>
						/// Remove an item from Inventory and update owner and position but do not remove from the database.
						/// This is use for transferring items.
						/// </summary>
						/// <param name="item"></param>
						/// <returns></returns>
		bool            RemoveTradeItem(InventoryItem item);
		bool			RemoveCountFromStack(InventoryItem item, int count);
		bool			RemoveTemplate(string templateID, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
		bool            MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount);
		InventoryItem   GetItem(eInventorySlot slot);
		ICollection<InventoryItem> GetItemRange(eInventorySlot minSlot, eInventorySlot maxSlot);

		void            BeginChanges();
		void            CommitChanges();
		void			ClearInventory();

		int				CountSlots(bool countUsed, eInventorySlot minSlot, eInventorySlot maxSlot);
		int				CountItemTemplate(string itemtemplateID, eInventorySlot minSlot, eInventorySlot maxSlot);
		bool			IsSlotsFree(int count, eInventorySlot minSlot, eInventorySlot maxSlot);
		
		eInventorySlot	FindFirstEmptySlot(eInventorySlot first, eInventorySlot last);
		eInventorySlot	FindLastEmptySlot(eInventorySlot first, eInventorySlot last);
		eInventorySlot	FindFirstFullSlot(eInventorySlot first, eInventorySlot last);
		eInventorySlot	FindLastFullSlot(eInventorySlot first, eInventorySlot last);

		InventoryItem	GetFirstItemByID(string uniqueID, eInventorySlot minSlot, eInventorySlot maxSlot);
		InventoryItem	GetFirstItemByObjectType(int objectType, eInventorySlot minSlot, eInventorySlot maxSlot);
		InventoryItem   GetFirstItemByName(string name ,eInventorySlot minSlot, eInventorySlot maxSlot);

		ICollection<InventoryItem> VisibleItems		{ get; }
		ICollection<InventoryItem> EquippedItems	{ get; }
		ICollection<InventoryItem> AllItems			{ get; }

		int InventoryWeight { get; }
	}
}
