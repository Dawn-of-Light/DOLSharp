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
	public class GameHouseVault : GameVault, IHouseHookpointItem
	{
		/// <summary>
		/// House vault permission bits.
		/// </summary>
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
		public GameHouseVault(ItemTemplate itemTemplate, int vaultIndex) : base()
		{
			if (itemTemplate == null)
				throw new ArgumentNullException();

			Name = itemTemplate.Name;
			Model = (ushort)(itemTemplate.Model);
			m_templateID = itemTemplate.Id_nb;
			Index = vaultIndex;
		}


		public override string GetOwner(GamePlayer player)
		{
			return HouseMgr.GetOwner(CurrentHouse.DatabaseItem);
		}


		#region Interact

		/// <summary>
		/// This list holds all the players that are currently viewing
		/// the vault; it is needed to update the contents of the vault
		/// for any one observer if there is a change.
		/// </summary>
		private Dictionary<String, GamePlayer> m_observers = new Dictionary<String, GamePlayer>();

		/// <summary>
		/// Player interacting with this vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!player.InHouse)
				return false;

			if (!base.Interact(player) || CurrentHouse == null)
				return false;

			if (!m_observers.ContainsKey(player.Name))
				m_observers.Add(player.Name, player);

			return true;
		}


		/// <summary>
		/// Send inventory updates to all players actively viewing this vault;
		/// players that are too far away will be considered inactive.
		/// </summary>
		/// <param name="updateItems"></param>
		protected override void NotifyObservers(GamePlayer player, IDictionary<int, InventoryItem> updateItems )
		{
			IList<String> inactiveList = new List<String>();
			foreach (GamePlayer observer in m_observers.Values)
			{
				if (observer.ActiveVault != this)
				{
					inactiveList.Add(observer.Name);
					continue;
				}

				if (!this.IsWithinRadius(observer, WorldMgr.INFO_DISTANCE))
				{
					if (observer.Client.Account.PrivLevel > 1)
					{
						observer.Out.SendMessage(String.Format("You are too far away from house vault {0} and will not receive any more updates.",
							Index + 1), eChatType.CT_Skill, eChatLoc.CL_SystemWindow);
					}

					observer.ActiveVault = null;
					inactiveList.Add(observer.Name);
					continue;
				}

				observer.Client.Out.SendInventoryItemsUpdate(updateItems, 0);
			}

			// Now remove all inactive observers.

			foreach (String observerName in inactiveList)
				m_observers.Remove(observerName);
		}

		#endregion

		#region Permissions

		/// <summary>
		/// Whether or not this player can view the contents of this
		/// vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CanView(GamePlayer player)
		{
			if (IsOwner(player) || player.Client.Account.PrivLevel > 1)
				return true;

			return ((GetPlayerPermissions(player) & (byte)(Permissions.View)) > 0);
		}

        /// <summary>
        /// Whether or not this player can move items inside the vault
        /// </summary>
        /// <param name="player"></param>
        /// <returns></returns>
        public override bool CanMove(GamePlayer player)
        {
            if (CurrentHouse.IsOwner(player) || player.Client.Account.PrivLevel > 1)
                return true;

            return ((GetPlayerPermissions(player) & (byte)(Permissions.Remove)) > 0);
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
		public bool Attach(House house, uint hookpointID, ushort heading)
		{
			if (house == null)
				return false;

			// Register vault in the DB.

			DBHousepointItem hookedItem = new DBHousepointItem();
			hookedItem.HouseID = house.HouseNumber;
			hookedItem.Position = hookpointID;
			hookedItem.Heading = (ushort)(heading % 4096);
			hookedItem.ItemTemplateID = m_templateID;
			hookedItem.Index = (byte)Index;
			GameServer.Database.AddObject(hookedItem);

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
			Heading = (ushort)(hookedItem.Heading % 4096);
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

			lock (m_vaultSync)
			{
				foreach (GamePlayer observer in m_observers.Values)
					observer.ActiveVault = null;

				m_observers.Clear();
				RemoveFromWorld();

				// Unregister this vault from the DB.

				GameServer.Database.DeleteObject(m_hookedItem);
				m_hookedItem = null;
			}

			return true;
		}

		#endregion
	}
}