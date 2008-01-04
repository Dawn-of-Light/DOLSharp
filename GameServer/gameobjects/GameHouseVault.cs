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

namespace DOL.GS
{
	/// <summary>
	/// A house vault.
	/// </summary>
	/// <author>Aredhel</author>
	public class GameHouseVault : GameStaticItem, IHouseHookpointItem
	{
		public enum Permissions
		{
			None = 0x00,
			Remove = 0x01,
			Add = 0x02,
			View = 0x04
		}

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
			ArrayList vaultItems = null;
			String sqlWhere = String.Format("OwnerID = '{0}' and SlotPosition >= {1} and SlotPosition <= {2}",
				GameServer.Database.Escape((HouseMgr.GetOwners(CurrentHouse.DatabaseItem)[0] as Character).ObjectId),
				((int)eInventorySlot.HouseVault_First) + 100 * Index,
				((int)eInventorySlot.HouseVault_First) + 100 * Index + 99);
			DataObject[] items = GameServer.Database.SelectObjects(typeof(InventoryItem), sqlWhere);
			foreach (InventoryItem item in items)
			{
				if (vaultItems == null)
					vaultItems = new ArrayList();
				vaultItems.Add(item);
			}

			player.Out.SendInventoryItemsUpdate(0x04, vaultItems);
			return true;
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

			player.Out.SendMessage(String.Format("Vault permissions: 0x{0:X2}", GetPlayerPermissions(player)),
				eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
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