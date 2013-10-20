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
//							Written by Doulbousiouf (01/11/2004)					//
using System.Collections;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// Represents an in-game VaultKeeper NPC
	/// </summary>
	[NPCGuildScript("Vault Keeper")]
	public class GameVaultKeeper : GameNPC
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public GameVaultKeeper() : base()
		{
		}

		#region Examine Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			List<string> list = new List<string>();
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "VaultKeeper.YouExamine", GetName(0, false, player.Client.Account.Language, this),
                                                       GetPronoun(0, true, player.Client.Account.Language), GetAggroLevelString(player, false)));
            list.Add(LanguageMgr.GetTranslation(player.Client.Account.Language, "VaultKeeper.RightClick"));
            return list;
		}

		#endregion Examine Message

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

			if (player.ActiveInventoryObject != null)
			{
				player.ActiveInventoryObject.RemoveObserver(player);
			}

			player.ActiveInventoryObject = null;

			TurnTo(player, 10000);
			var items = player.Inventory.GetItemRange(eInventorySlot.FirstVault, eInventorySlot.LastVault);
			player.Out.SendInventoryItemsUpdate(eInventoryWindowType.PlayerVault, items.Count > 0 ? items : null);
			return true;
		}

		#endregion examine message
	}
}