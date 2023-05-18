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
 */

using System;
using System.Collections;
using DOL.Database;
using System.Collections.Generic;

// Tolakram - January 7, 2012
 
namespace DOL.GS
{
	/// <summary>
	/// Interface for a GameInventoryObject
	/// This is an object or NPC that can interact with a players inventory, buy, or sell items
	/// </summary>		
	public interface IGameInventoryObject
	{
		object LockObject();

		int FirstClientSlot { get; }
		int LastClientSlot { get; }
		int FirstDBSlot { get; }
		int LastDBSlot { get; }
		string GetOwner(GamePlayer player);
		IList<InventoryItem> DBItems(GamePlayer player = null);
		Dictionary<int, InventoryItem> GetClientInventory(GamePlayer player);
		bool CanHandleMove(GamePlayer player, ushort fromClientSlot, ushort toClientSlot);
		bool MoveItem(GamePlayer player, ushort fromClientSlot, ushort toClientSlot);
		bool OnAddItem(GamePlayer player, InventoryItem item);
		bool OnRemoveItem(GamePlayer player, InventoryItem item);
		bool SetSellPrice(GamePlayer player, ushort clientSlot, uint sellPrice);
		bool SearchInventory(GamePlayer player, MarketSearch.SearchData searchData);
		void AddObserver(GamePlayer player);
		void RemoveObserver(GamePlayer player);
	}

	/// <summary>
	/// This is an extension class for GameInventoryObjects.  It's a way to get around the fact C# doesn't support multiple inheritance. 
	/// We want the ability for a GameInventoryObject to be a game static object, or an NPC, or anything else, and yet still contain common functionality 
	/// for an inventory object with code written in just one place
	/// </summary>
	public static class GameInventoryObjectExtensions
	{
		public const string ITEM_BEING_ADDED = "ItemBeingAddedToObject";
		public const string TEMP_SEARCH_KEY = "TempSearchKey";

		/// <summary>
		/// Can this object handle the move request?
		/// </summary>
		public static bool CanHandleRequest(this IGameInventoryObject thisObject, GamePlayer player, ushort fromClientSlot, ushort toClientSlot)
		{
			// make sure from or to slots involve this object
			if ((fromClientSlot >= thisObject.FirstClientSlot && fromClientSlot <= thisObject.LastClientSlot) ||
				(toClientSlot >= thisObject.FirstClientSlot && toClientSlot <= thisObject.LastClientSlot))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Get the items of this object, mapped to the client inventory slots
		/// </summary>
		public static Dictionary<int, InventoryItem> GetClientItems(this IGameInventoryObject thisObject, GamePlayer player)
		{
			var inventory = new Dictionary<int, InventoryItem>();
			int slotOffset = thisObject.FirstClientSlot - thisObject.FirstDBSlot;
			foreach (InventoryItem item in thisObject.DBItems(player))
			{
				if (item != null)
				{
					if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
					{
						inventory.Add(item.SlotPosition + slotOffset, item);
					}
				}
			}

			return inventory;
		}


		/// <summary>
		/// Move an item from the inventory object to a player's backpack (uses client slots)
		/// </summary>
		public static IDictionary<int, InventoryItem> MoveItemFromObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromClientSlot, eInventorySlot toClientSlot)
		{
			// We will only allow moving to the backpack.

			if (toClientSlot < eInventorySlot.FirstBackpack || toClientSlot > eInventorySlot.LastBackpack)
				return null;

			lock (thisObject.LockObject())
			{
				Dictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

				if (inventory.ContainsKey((int)fromClientSlot) == false)
				{
					ChatUtil.SendErrorMessage(player, "Item not found in slot " + (int)fromClientSlot);
					return null;
				}

				InventoryItem fromItem = inventory[(int)fromClientSlot];
				InventoryItem toItem = player.Inventory.GetItem(toClientSlot);

				// if there is an item in the players target inventory slot then move it to the object
				if (toItem != null)
				{
					player.Inventory.RemoveTradeItem(toItem);
					toItem.SlotPosition = fromItem.SlotPosition;
					toItem.OwnerID = thisObject.GetOwner(player);
					thisObject.OnAddItem(player, toItem);
					GameServer.Database.SaveObject(toItem);
				}

				thisObject.OnRemoveItem(player, fromItem);

				// Create the GameInventoryItem from this InventoryItem.  This simply wraps the InventoryItem, 
				// which is still updated when this item is moved around
				InventoryItem objectItem = GameInventoryItem.Create(fromItem);

				player.Inventory.AddTradeItem(toClientSlot, objectItem);

				var updateItems = new Dictionary<int, InventoryItem>(1);
				updateItems.Add((int)fromClientSlot, toItem);

				return updateItems;
			}
		}

		/// <summary>
		/// Move an item from a player's backpack to this inventory object (uses client slots)
		/// </summary>
		public static IDictionary<int, InventoryItem> MoveItemToObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromClientSlot, eInventorySlot toClientSlot)
		{
			// We will only allow moving from the backpack.

			if (fromClientSlot < eInventorySlot.FirstBackpack || fromClientSlot > eInventorySlot.LastBackpack)
				return null;

			InventoryItem fromItem = player.Inventory.GetItem(fromClientSlot);

			if (fromItem == null)
				return null;

			lock (thisObject.LockObject())
			{
				Dictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

				player.Inventory.RemoveTradeItem(fromItem);

				// if there is an item in the objects target slot then move it to the players inventory
				if (inventory.ContainsKey((int)toClientSlot))
				{
					InventoryItem toItem = GameInventoryItem.Create(inventory[(int)toClientSlot]);
					thisObject.OnRemoveItem(player, toItem);
					player.Inventory.AddTradeItem(fromClientSlot, toItem);
				}

				fromItem.OwnerID = thisObject.GetOwner(player);
				fromItem.SlotPosition = (int)(toClientSlot) - (int)(thisObject.FirstClientSlot) + thisObject.FirstDBSlot;
				thisObject.OnAddItem(player, fromItem);
				GameServer.Database.SaveObject(fromItem);

				var updateItems = new Dictionary<int, InventoryItem>(1);
				updateItems.Add((int)toClientSlot, fromItem);

				// for objects that support doing something when added (setting a price, for example)
				player.TempProperties.setProperty(ITEM_BEING_ADDED, fromItem);

				return updateItems;
			}
		}

		/// <summary>
		/// Move an item around inside this object (uses client slots)
		/// </summary>
		public static IDictionary<int, InventoryItem> MoveItemInsideObject(this IGameInventoryObject thisObject, GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			lock (thisObject.LockObject())
			{
				IDictionary<int, InventoryItem> inventory = thisObject.GetClientInventory(player);

				if (!inventory.ContainsKey((int)fromSlot))
					return null;

				var updateItems = new Dictionary<int, InventoryItem>(2);
				InventoryItem fromItem = null, toItem = null;

				fromItem = inventory[(int)fromSlot];

				if (inventory.ContainsKey((int)toSlot))
				{
					toItem = inventory[(int)toSlot];
					toItem.SlotPosition = fromItem.SlotPosition;

					GameServer.Database.SaveObject(toItem);
				}

				fromItem.SlotPosition = (int)toSlot - (int)(thisObject.FirstClientSlot) + thisObject.FirstDBSlot;
				GameServer.Database.SaveObject(fromItem);

				updateItems.Add((int)fromSlot, toItem);
				updateItems.Add((int)toSlot, fromItem);

				return updateItems;
			}
		}
	}
}
