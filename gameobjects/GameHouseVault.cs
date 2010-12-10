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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS
{
	/// <summary>
	/// A house vault.
	/// </summary>
	/// <author>Aredhel</author>
	public class GameHouseVault : GameVault, IHouseHookpointItem
	{
		/// <summary>
		/// This list holds all the players that are currently viewing
		/// the vault; it is needed to update the contents of the vault
		/// for any one observer if there is a change.
		/// </summary>
		private readonly Dictionary<string, GamePlayer> _observers = new Dictionary<string, GamePlayer>();

		private readonly string _templateID;
		private readonly object _vaultLock = new object();
		private DBHouseHookpointItem _hookedItem;

		/// <summary>
		/// Create a new house vault.
		/// </summary>
		/// <param name="vaultIndex"></param>
		public GameHouseVault(ItemTemplate itemTemplate, int vaultIndex)
		{
			if (itemTemplate == null)
				throw new ArgumentNullException();

			Name = itemTemplate.Name;
			Model = (ushort) (itemTemplate.Model);
			_templateID = itemTemplate.Id_nb;
			Index = vaultIndex;
		}

		public override int VaultSize
		{
			get { return HousingConstants.VaultSize; }
		}

		#region IHouseHookpointItem Members

		/// <summary>
		/// Template ID for this vault.
		/// </summary>
		public string TemplateID
		{
			get { return _templateID; }
		}

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

            // register vault in the DB.
            var hookedItem = new DBHouseHookpointItem
            {
                HouseNumber = house.HouseNumber,
                HookpointID = hookpointID,
                Heading = (ushort)(heading % 4096),
                ItemTemplateID = _templateID,
                Index = (byte)Index
            };

            var hpitem = GameServer.Database.SelectObjects<DBHouseHookpointItem>("HouseNumber = '" + house.HouseNumber + "' AND HookpointID = '" + hookpointID + "'");

            if (hpitem == null)
                GameServer.Database.AddObject(hookedItem);

            // now add the vault to the house.
            return Attach(house, hookedItem);
        }

		/// <summary>
		/// Attach this vault to a hookpoint in a house.
		/// </summary>
		/// <param name="house"></param>
		/// <param name="hookedItem"></param>
		/// <returns></returns>
		public bool Attach(House house, DBHouseHookpointItem hookedItem)
		{
			if (house == null || hookedItem == null)
				return false;

			_hookedItem = hookedItem;

			IPoint3D position = house.GetHookpointLocation(hookedItem.HookpointID);
			if (position == null)
				return false;

			CurrentHouse = house;
			CurrentRegionID = house.RegionID;
			InHouse = true;
			X = position.X;
			Y = position.Y;
			Z = position.Z;
			Heading = (ushort) (hookedItem.Heading%4096);
			AddToWorld();

			return true;
		}

		/// <summary>
		/// Remove this vault from a hookpoint in the house.
		/// </summary>
		/// <returns></returns>
		public bool Detach()
		{
			if (_hookedItem == null)
				return false;

			lock (m_vaultSync)
			{
				foreach (GamePlayer observer in _observers.Values)
				{
					observer.ActiveVault = null;
				}

				_observers.Clear();
				RemoveFromWorld();

				// unregister this vault from the DB.
				GameServer.Database.DeleteObject(_hookedItem);
				_hookedItem = null;
			}

			return true;
		}

		#endregion

		public override string GetOwner(GamePlayer player)
		{
			return CurrentHouse.DatabaseItem.OwnerID;
		}

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

			lock (_vaultLock)
			{
				if (!_observers.ContainsKey(player.Name))
				{
					_observers.Add(player.Name, player);
				}
			}

			return true;
		}

		/// <summary>
		/// Send inventory updates to all players actively viewing this vault;
		/// players that are too far away will be considered inactive.
		/// </summary>
		/// <param name="updateItems"></param>
		protected override void NotifyObservers(GamePlayer player, IDictionary<int, InventoryItem> updateItems)
		{
			var inactiveList = new List<string>();

			lock (_vaultLock)
			{
				foreach (GamePlayer observer in _observers.Values)
				{
					if (observer.ActiveVault != this)
					{
						inactiveList.Add(observer.Name);
						continue;
					}

					if (!IsWithinRadius(observer, WorldMgr.INFO_DISTANCE))
					{
						observer.ActiveVault = null;
						inactiveList.Add(observer.Name);

						continue;
					}

					observer.Client.Out.SendInventoryItemsUpdate(updateItems, 0);
				}

				// now remove all inactive observers.
				foreach (string observerName in inactiveList)
				{
					_observers.Remove(observerName);
				}
			}
		}

		/// <summary>
		/// Whether or not this player can view the contents of this
		/// vault.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CanView(GamePlayer player)
		{
			return CurrentHouse.CanUseVault(player, this, VaultPermissions.View);
		}

		/// <summary>
		/// Whether or not this player can move items inside the vault
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CanAddItems(GamePlayer player)
		{
			return CurrentHouse.CanUseVault(player, this, VaultPermissions.Add);
		}

		/// <summary>
		/// Whether or not this player can move items inside the vault
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool CanRemoveItems(GamePlayer player)
		{
			return CurrentHouse.CanUseVault(player, this, VaultPermissions.Remove);
		}
	}
}