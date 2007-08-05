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

namespace DOL.GS
{
	/// <summary>
	/// Represents a housing Vault
	/// </summary>
	public class GameHouseVault : GameStaticItem
	{
		#region Interact

		/// <summary>
		/// Called when a player right clicks on the vaultkeeper
		/// </summary>
		/// <param name="player">Player that interacted with the vaultkeeper</param>
		/// <returns>True if succeeded</returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player))
				return false;

			if (CurrentHouse != null && CurrentHouse.CanViewHouseVault(player))
			{
				ArrayList vaultItems = null;
				DataObject[] items = GameServer.Database.SelectObjects(typeof(InventoryItem), "OwnerID = '" + (HouseMgr.GetOwners(CurrentHouse.DatabaseItem)[0] as Character).ObjectId + "'");
				foreach (InventoryItem item in items)
				{
					if (vaultItems == null)
						vaultItems = new ArrayList();
					vaultItems.Add(item);
				}

				player.Out.SendInventoryItemsUpdate(0x04, vaultItems);
			}
			return true;
		}

		#endregion
	}
}