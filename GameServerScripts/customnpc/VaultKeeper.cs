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
using NHibernate.Mapping.Attributes;

namespace DOL.GS.Scripts
{
	/// <summary>
	/// Represents an in-game VaultKeeper NPC
	/// </summary>
	[Subclass(NameType=typeof(GameVaultKeeper), ExtendsType=typeof(GameMob))] 
	public class GameVaultKeeper : GameMob
	{
		#region Examine Message

		/// <summary>
		/// Adds messages to ArrayList which are sent when object is targeted
		/// </summary>
		/// <param name="player">GamePlayer that is examining this object</param>
		/// <returns>list with string messages</returns>
		public override IList GetExamineMessages(GamePlayer player)
		{
			IList list = new ArrayList();
			list.Add("You examine " + GetName(0, false) + ".  " + GetPronoun(0, true) + " is " + GetAggroLevelString(player, false) + " and vaultkeeper.");
			list.Add("[Right click to display a vault window]");
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
			if (!base.Interact(player)) return false;

			TurnTo(player, 10000);
			
			ArrayList slotToUpdate = new ArrayList();
			for(int i = (int)eInventorySlot.FirstVault ; i <= (int)eInventorySlot.LastVault ; i++)
			{
				if(player.Inventory.GetItem((eInventorySlot)i) != null) slotToUpdate.Add(i);
			}
			player.Out.SendInventorySlotsUpdate(0x03, slotToUpdate);
			
			return true;
		}

		#endregion examine message
	}
}