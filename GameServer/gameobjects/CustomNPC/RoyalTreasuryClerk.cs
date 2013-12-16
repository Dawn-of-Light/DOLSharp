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
using System.Text;
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Special NPC for giving DR players items
	/// </summary>
	public class RoyalTreasuryClerk : GameNPC
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Interact with the NPC.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player) || player == null)
				return false;

			if (GlobalConstants.IsExpansionEnabled((int)eClientExpansion.DarknessRising))
			{
				SayTo(player, "Let me check to see if you are owed any items ... ");

				if (player.Inventory.CountItemTemplate("Personal_Bind_Recall_Stone", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == 0)
				{
					SayTo(player, "I see you are missing your Personal Bind Recall Stone.  Would you like [another]?");
				}

				return true;
			}

			return false;
		}

		/// <summary>
		/// Talk to the NPC.
		/// </summary>
		/// <param name="source"></param>
		/// <param name="str"></param>
		/// <returns></returns>
		public override bool WhisperReceive(GameLiving source, string text)
		{
			if (!base.WhisperReceive(source, text) || !(source is GamePlayer))
				return false;

			GamePlayer player = source as GamePlayer;

			if (text.ToLower() == "another")
			{
				if (player.Inventory.CountItemTemplate("Personal_Bind_Recall_Stone", eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == 0)
				{
					SayTo(player, "Very well then, here's your Personal Bind Recall Stone, may it serve you well.");
					player.ReceiveItem(this, "Personal_Bind_Recall_Stone");
				}
				return true;
			}

			return true;
		}
	}
}
