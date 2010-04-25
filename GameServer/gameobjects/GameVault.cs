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
using System.Collections;
using DOL.Database;
using DOL.GS.Housing;
using System;
using DOL.GS.PacketHandler;
using System.Collections.Generic;
using log4net;
using System.Reflection;

namespace DOL.GS
{
	/// <summary>
	/// A vault.
	/// </summary>
	/// <author>Aredhel, Tolakram</author>
	public class GameVault : GameStaticItem
	{
		/// <summary>
		/// This is used to synchronize actions on the vault.
		/// </summary>
		protected object m_vaultSync = new object();


		/// <summary>
		/// Number of items a single vault can hold.
		/// </summary>
		public const int Size = 100;

		protected int m_vaultIndex = 0;

		/// <summary>
		/// Index of this vault.
		/// </summary>
		public int Index
		{
			get { return m_vaultIndex; }
			set { m_vaultIndex = value; }
		}

		/// <summary>
		/// Create a new house vault.
		/// </summary>
		/// <param name="vaultIndex"></param>
		public GameVault()
		{
		}

		public virtual string GetOwner(GamePlayer player)
		{
			return player.InternalID;
		}

		#region Interact

		/// <summary>
		/// Player interacting with this vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (!CanView(player))
			{
				player.Out.SendMessage("You don't have permission to view this vault!",
					eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

            if (player.ActiveConMerchant != null)
                player.ActiveConMerchant = null;

			player.ActiveVault = this;
			player.Out.SendInventoryItemsUpdate(VaultInventory(player), 0x04);
			return true;
		}

		/// <summary>
		/// First slot in the DB.
		/// </summary>
		public virtual int FirstSlot
		{
			get { return (int)(eInventorySlot.HouseVault_First) + Size * Index; }
		}

		/// <summary>
		/// Last slot in the DB.
		/// </summary>
		public virtual int LastSlot
		{
			get { return (int)(eInventorySlot.HouseVault_First) + Size * (Index + 1) - 1; }
		}

		/// <summary>
		/// Inventory for this vault.
		/// </summary>
		public virtual Dictionary<int, InventoryItem> VaultInventory(GamePlayer player)
		{
			Dictionary<int, InventoryItem> inventory = new Dictionary<int, InventoryItem>();
			int slotOffset = - FirstSlot + (int)(eInventorySlot.HousingInventory_First);
			foreach (InventoryItem item in GetItems(player))
			{
				if (item != null)
				{
					if (!inventory.ContainsKey(item.SlotPosition + slotOffset))
					{
						inventory.Add(item.SlotPosition + slotOffset, item);
					}
					else
					{
						log.ErrorFormat("GAMEVAULT: Duplicate item {0}, owner {1}, position {2}", item.Name, item.OwnerID, (item.SlotPosition + slotOffset));
					}
				}
			}
			return inventory;
		}

		/// <summary>
		/// List of items in the vault.
		/// </summary>
		public InventoryItem[] GetItems(GamePlayer player)
		{
			String sqlWhere = String.Format("OwnerID = '{0}' and SlotPosition >= {1} and SlotPosition <= {2}",
                GetOwner(player),
				FirstSlot, LastSlot);

			return (InventoryItem[])(GameServer.Database.SelectObjects<InventoryItem>(sqlWhere));
		}

		/// <summary>
		/// Move an item from, to or inside a house vault.
		/// </summary>
		public virtual void MoveItem(GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot )
		{
			if (fromSlot == toSlot)
				return;

			lock (m_vaultSync)
			{
				if (fromSlot == toSlot)
				{
					NotifyObservers(player, null);
				}
				else if (fromSlot >= eInventorySlot.HousingInventory_First && fromSlot <= eInventorySlot.HousingInventory_Last)
				{
					if (toSlot >= eInventorySlot.HousingInventory_First && toSlot <= eInventorySlot.HousingInventory_Last)
					{
						NotifyObservers(player, MoveItemInsideVault(player, fromSlot, toSlot));
					}

					NotifyObservers(player, MoveItemFromVault(player, fromSlot, toSlot));
				}
				else if (toSlot >= eInventorySlot.HousingInventory_First && toSlot <= eInventorySlot.HousingInventory_Last)
				{
					NotifyObservers(player, MoveItemToVault(player, fromSlot, toSlot));
				}
			}
		}

		/// <summary>
		/// Move an item from the vault.
		/// </summary>
		protected virtual IDictionary<int, InventoryItem> MoveItemFromVault(GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving to the backpack.

			if (toSlot < eInventorySlot.FirstBackpack || toSlot > eInventorySlot.LastBackpack)
				return null;

			IDictionary<int, InventoryItem> inventory = VaultInventory(player);

			if (!inventory.ContainsKey((int)fromSlot))
				return null;

			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);
			InventoryItem fromItem = inventory[(int)fromSlot];
			InventoryItem toItem = player.Inventory.GetItem(toSlot);

			// if there is an item in the players target inventory slot then move it to the vault
			if (toItem != null)
			{
				player.Inventory.RemoveTradeItem(toItem);
				toItem.SlotPosition = fromItem.SlotPosition;
				toItem.OwnerID = GetOwner(player);
				GameServer.Database.SaveObject(toItem);
			}

			player.Inventory.AddTradeItem(toSlot, fromItem);

			updateItems.Add((int)fromSlot, toItem);
			return updateItems;
		}

		/// <summary>
		/// Move an item to the vault.
		/// </summary>
		protected virtual IDictionary<int, InventoryItem> MoveItemToVault(GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving from the backpack.

			if (fromSlot < eInventorySlot.FirstBackpack || fromSlot > eInventorySlot.LastBackpack)
				return null;

			InventoryItem fromItem = player.Inventory.GetItem(fromSlot);

			if (fromItem == null)
				return null;

			IDictionary<int, InventoryItem> inventory = VaultInventory(player);
			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);

			player.Inventory.RemoveTradeItem(fromItem);

			// if there is an item in the vaults target slot then move it to the players inventory
			if (inventory.ContainsKey((int)toSlot))
			{
				InventoryItem toItem = inventory[(int)toSlot];
				player.Inventory.AddTradeItem(fromSlot, toItem);
			}

            fromItem.OwnerID = GetOwner(player);
			fromItem.SlotPosition = (int)(toSlot) - (int)(eInventorySlot.HousingInventory_First) + FirstSlot;
			GameServer.Database.SaveObject(fromItem);

			updateItems.Add((int)toSlot, fromItem);
			return updateItems;
		}

		/// <summary>
		/// Move an item around inside the vault.
		/// </summary>
		protected virtual IDictionary<int, InventoryItem> MoveItemInsideVault(GamePlayer player, eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			IDictionary<int, InventoryItem> inventory = VaultInventory(player);

			if (!inventory.ContainsKey((int)fromSlot))
				return null;

			IDictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(2);
			InventoryItem fromItem = null, toItem = null;

			fromItem = inventory[(int)fromSlot];

			if (inventory.ContainsKey((int)toSlot))
			{
				toItem = inventory[(int)toSlot];
				toItem.SlotPosition = fromItem.SlotPosition;
				GameServer.Database.SaveObject(toItem);
			}

			fromItem.SlotPosition = (int)(toSlot) - (int)(eInventorySlot.HousingInventory_First) + FirstSlot;
			GameServer.Database.SaveObject(fromItem);

			updateItems.Add((int)fromSlot, toItem);
			updateItems.Add((int)toSlot, fromItem);
			return updateItems;
		}

		/// <summary>
		/// Send inventory updates to all players actively viewing this vault;
		/// players that are too far away will be considered inactive.
		/// </summary>
		/// <param name="updateItems"></param>
		protected virtual void NotifyObservers(GamePlayer player, IDictionary<int, InventoryItem> updateItems)
		{
			player.Client.Out.SendInventoryItemsUpdate(updateItems, 0);
		}

		#endregion

		#region Permissions

		/// <summary>
		/// Whether or not this player can view the contents of this
		/// vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public virtual bool CanView(GamePlayer player)
		{
			return true;
		}

        /// <summary>
        /// Whether or not this player can move items inside the vault
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public virtual bool CanMove(GamePlayer player)
        {
			return true;
        }

		#endregion
	}
}