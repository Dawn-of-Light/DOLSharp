using System;
using System.Collections;
using DOL.Database2;

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

		Min_Inv           = 7,

		HorseArmor        = 7, // Equipment, horse armor
		HorseBarding      = 8, // Equipment, horse barding
		Horse             = 9, // Equipment, horse

		RightHandWeapon = 10,//Equipment, Visible
		MinEquipable	  = 10,
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

		Mithril			  = 80,
		Platinum		  = 81,
		Gold			  = 82,
		Silver			  = 83,
		Copper			  = 84,

		PlayerPaperDoll   = 100,
		FirstVault        = 110,
		LastVault         = 149,

		HousingInventory_First = 150,
		HousingInventory_Last = 249,	

		HouseVault_First = 1000,
		HouseVault_Last = 1399,

		Consignment_First = 1500,
		Consignment_Last = 1599,

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
	/// Description résume de IGameInventory.
	/// </summary>		
	public interface IGameInventory
	{
		bool            LoadFromDatabase(UInt64 inventoryID);
		bool            SaveIntoDatabase(UInt64 inventoryID);

        bool LoadFromDatabase(string inventoryID);
        bool SaveIntoDatabase(string inventoryID);

		bool			AddItem(eInventorySlot slot, InventoryItem item);
		bool			AddCountToStack(InventoryItem item, int count);
		bool			AddTemplate(ItemTemplate template, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
		bool            RemoveItem(InventoryItem item);
		bool			RemoveCountFromStack(InventoryItem item, int count);
		bool			RemoveTemplate(string templateID, int count, eInventorySlot minSlot, eInventorySlot maxSlot);
		bool            MoveItem(eInventorySlot fromSlot, eInventorySlot toSlot, int itemCount);
		InventoryItem   GetItem(eInventorySlot slot);
		ICollection     GetItemRange(eInventorySlot minSlot, eInventorySlot maxSlot);

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

		ICollection     VisibleItems    { get; }
		ICollection     EquippedItems   { get; }
		ICollection     AllItems        { get; }

		int             InventoryWeight { get; }
	}
}
