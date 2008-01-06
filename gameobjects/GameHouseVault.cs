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
	/// A house vault.
	/// </summary>
	/// <author>Aredhel</author>
	public class GameHouseVault : GameStaticItem, IHouseHookpointItem
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public enum Permissions
		{
			None = 0x00,
			Remove = 0x01,
			Add = 0x02,
			View = 0x04
		}

		/// <summary>
		/// Number of items a single vault can hold.
		/// </summary>
		public const int Size = 100;

		/// <summary>
		/// Create a new house vault.
		/// </summary>
		/// <param name="vaultIndex"></param>
		public GameHouseVault(ItemTemplate itemTemplate, int vaultIndex)
		{
			if (vaultIndex < 0 || vaultIndex > 3)
				throw new ArgumentOutOfRangeException();

			if (itemTemplate == null)
				throw new ArgumentNullException();

			Name = itemTemplate.Name;
			Model = (ushort)(itemTemplate.Model);

			m_templateID = itemTemplate.Id_nb;
			m_vaultIndex = vaultIndex;
		}

		#region Interact

		/// <summary>
		/// Player interacting with this vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player) || CurrentHouse == null)
				return false;

			if (!CanView(player))
			{
				player.Out.SendMessage("You don't have permission to view this vault!",
					eChatType.CT_System, eChatLoc.CL_SystemWindow);
				return false;
			}

			player.ActiveVault = this;
			player.Out.SendInventoryItemsUpdate(Inventory, 0x04);
			return true;
		}

		/// <summary>
		/// First slot in the DB.
		/// </summary>
		public int FirstSlot
		{
			get { return (int)(eInventorySlot.HouseVault_First) + Size * Index; }
		}

		/// <summary>
		/// Last slot in the DB.
		/// </summary>
		public int LastSlot
		{
			get { return (int)(eInventorySlot.HouseVault_First) + Size * (Index + 1) - 1; }
		}

		/// <summary>
		/// Inventory for this vault.
		/// </summary>
		public Dictionary<int, InventoryItem> Inventory
		{
			get
			{
				Dictionary<int, InventoryItem> inventory = new Dictionary<int, InventoryItem>();
				int slotOffset = - FirstSlot + (int)(eInventorySlot.HousingInventory_First);
				foreach (InventoryItem item in Items)
					if (item != null)
						inventory.Add(item.SlotPosition + slotOffset, item);
				return inventory;
			}
		}

		/// <summary>
		/// List of items in the vault.
		/// </summary>
		public InventoryItem[] Items
		{
			get
			{
				String sqlWhere = String.Format("OwnerID = '{0}' and SlotPosition >= {1} and SlotPosition <= {2}",
					GameServer.Database.Escape((HouseMgr.GetOwners(CurrentHouse.DatabaseItem)[0] as Character).ObjectId),
					FirstSlot, LastSlot);
				return (InventoryItem[])(GameServer.Database.SelectObjects(typeof(InventoryItem), sqlWhere));
			}
		}

		/// <summary>
		/// Move an item from, to or inside a house vault.
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		public IDictionary<int, InventoryItem> MoveItem(IGameInventory playerInventory,
			eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			if (fromSlot == toSlot)
				return null;

			if (fromSlot >= eInventorySlot.HousingInventory_First &&
				fromSlot <= eInventorySlot.HousingInventory_Last)
			{
				if (toSlot >= eInventorySlot.HousingInventory_First &&
					toSlot <= eInventorySlot.HousingInventory_Last)
					return MoveItemInsideVault(fromSlot, toSlot);

				return MoveItemFromVault(playerInventory, fromSlot, toSlot);
			}

			if (toSlot >= eInventorySlot.HousingInventory_First &&
				toSlot <= eInventorySlot.HousingInventory_Last)
				return MoveItemToVault(playerInventory, fromSlot, toSlot);

			// Neither slot is a vault slot, who the heck called us?

			return null;
		}

		/// <summary>
		/// Move an item from the vault.
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemFromVault(IGameInventory playerInventory,
			eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving to the backpack.

			if (toSlot < eInventorySlot.FirstBackpack || toSlot > eInventorySlot.LastBackpack)
				return null;

			Dictionary<int, InventoryItem> inventory = Inventory;

			if (!inventory.ContainsKey((int)fromSlot))
				return null;

			Dictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);
			InventoryItem fromItem = inventory[(int)fromSlot];
			InventoryItem toItem = playerInventory.GetItem(toSlot);

			if (toItem != null)
			{
				playerInventory.RemoveItem(toItem);
				toItem.SlotPosition = fromItem.SlotPosition;
				GameServer.Database.AddNewObject(toItem);
				GameServer.Database.SaveObject(toItem);
			}

			GameServer.Database.DeleteObject(fromItem);
			playerInventory.AddItem(toSlot, fromItem);
			updateItems.Add((int)fromSlot, toItem);
			return updateItems;
		}

		/// <summary>
		/// Move an item to the vault.
		/// </summary>
		/// <param name="playerInventory"></param>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemToVault(IGameInventory playerInventory, 
			eInventorySlot fromSlot, eInventorySlot toSlot)
		{
			// We will only allow moving from the backpack.

			if (fromSlot < eInventorySlot.FirstBackpack || fromSlot > eInventorySlot.LastBackpack)
				return null;

			InventoryItem fromItem = playerInventory.GetItem(fromSlot);

			if (fromItem == null)
				return null;

			Dictionary<int, InventoryItem> inventory = Inventory;
			Dictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(1);

			playerInventory.RemoveItem(fromItem);

			if (inventory.ContainsKey((int)toSlot))
			{
				InventoryItem toItem = inventory[(int)toSlot];
				GameServer.Database.DeleteObject(toItem);
				playerInventory.AddItem(fromSlot, toItem);
			}

			fromItem.OwnerID = (HouseMgr.GetOwners(CurrentHouse.DatabaseItem)[0] as Character).ObjectId;
			fromItem.SlotPosition = (int)(toSlot) -
				(int)(eInventorySlot.HousingInventory_First) +
				FirstSlot;
			GameServer.Database.AddNewObject(fromItem);
			GameServer.Database.SaveObject(fromItem);

			updateItems.Add((int)toSlot, fromItem);
			return updateItems;
		}

		/// <summary>
		/// Move an item around inside the vault.
		/// </summary>
		/// <param name="fromSlot"></param>
		/// <param name="toSlot"></param>
		/// <returns></returns>
		protected IDictionary<int, InventoryItem> MoveItemInsideVault(eInventorySlot fromSlot,
			eInventorySlot toSlot)
		{
			Dictionary<int, InventoryItem> inventory = Inventory;

			if (!inventory.ContainsKey((int)fromSlot))
				return null;

			Dictionary<int, InventoryItem> updateItems = new Dictionary<int, InventoryItem>(2);
			InventoryItem fromItem = null, toItem = null;

			fromItem = inventory[(int)fromSlot];

			if (inventory.ContainsKey((int)toSlot))
			{
				toItem = inventory[(int)toSlot];
				toItem.SlotPosition = fromItem.SlotPosition;
				GameServer.Database.SaveObject(toItem);
			}

			fromItem.SlotPosition = (int)(toSlot) -
				(int)(eInventorySlot.HousingInventory_First) +
				FirstSlot;
			GameServer.Database.SaveObject(fromItem);

			updateItems.Add((int)fromSlot, toItem);
			updateItems.Add((int)toSlot, fromItem);
			return updateItems;
		}

		#endregion

		#region Permissions

		/// <summary>
		/// Whether or not this player can view the contents of this
		/// vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public bool CanView(GamePlayer player)
		{
			if (IsOwner(player) || player.Client.Account.PrivLevel > 1)
				return true;

			return ((GetPlayerPermissions(player) & (byte)(Permissions.View)) > 0);
		}

		/// <summary>
		/// Get permissions this player has for this vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		private byte GetPlayerPermissions(GamePlayer player)
		{
			DBHousePermissions housePermissions = CurrentHouse.GetPlayerPermissions(player);
			if (housePermissions != null)
			{
				switch (Index)
				{
					case 0: return housePermissions.Vault1;
					case 1: return housePermissions.Vault2;
					case 2: return housePermissions.Vault3;
					case 3: return housePermissions.Vault4;
				}
			}
			return (byte)(Permissions.None);
		}

		#endregion

		#region IHouseHookpointItem Implementation

		private int m_vaultIndex;

		/// <summary>
		/// Index of this vault.
		/// </summary>
		public int Index
		{
			get { return m_vaultIndex; }
		}

		private String m_templateID;

		/// <summary>
		/// Template ID for this vault.
		/// </summary>
		public String TemplateID
		{
			get { return m_templateID; }
		}

		private DBHousepointItem m_hookedItem = null;

		/// <summary>
		/// Attach this vault to a hookpoint in a house.
		/// </summary>
		/// <param name="house"></param>
		/// <param name="hookpointID"></param>
		/// <returns></returns>
		public bool Attach(House house, uint hookpointID)
		{
			if (house == null)
				return false;

			// Register vault in the DB.

			DBHousepointItem hookedItem = new DBHousepointItem();
			hookedItem.HouseID = house.HouseNumber;
			hookedItem.Position = hookpointID;
			hookedItem.ItemTemplateID = m_templateID;
			hookedItem.Index = (byte)Index;
			GameServer.Database.AddNewObject(hookedItem);

			// Now add the vault to the house.

			return Attach(house, hookedItem);
		}

		/// <summary>
		/// Attach this vault to a hookpoint in a house.
		/// </summary>
		/// <param name="house"></param>
		/// <param name="hookedItem"></param>
		/// <returns></returns>
		public bool Attach(House house, DBHousepointItem hookedItem)
		{
			if (house == null || hookedItem == null)
				return false;

			m_hookedItem = hookedItem;

			IPoint3D position = house.GetHookpointLocation(hookedItem.Position);
			if (position == null)
				return false;

			CurrentHouse = house;
			CurrentRegionID = house.RegionID;
			InHouse = true;
			X = position.X;
			Y = position.Y;
			Z = position.Z;
			Heading = house.GetHookpointHeading(hookedItem.Position);
			AddToWorld();
			return true;
		}

		/// <summary>
		/// Remove this vault from a hookpoint in the house.
		/// </summary>
		/// <returns></returns>
		public bool Detach()
		{
			if (m_hookedItem == null)
				return false;

			RemoveFromWorld();

			// Unregister this vault from the DB.

			GameServer.Database.DeleteObject(m_hookedItem);
			m_hookedItem = null;
			return true;
		}

		#endregion
	}
}