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
	/// Teleporter for entering and leaving the throne room.
	/// For the time being, this is a dummy class, to be overridden with
	/// the actual teleport code at some later stage. For now it will
	/// simply state that the King is busy right now and hand out
	/// Personal Bind Recall Stones - that way we don't need to add another
	/// custom NPC.
	/// </summary>
	/// <author>Aredhel</author>
	public class ThroneRoomTeleporter : GameNPC
	{
		/// <summary>
		/// Interact with the NPC.
		/// </summary>
		/// <param name="player"></param>
		/// <returns></returns>
		public override bool Interact(GamePlayer player)
		{
			if (!base.Interact(player) || player == null)
				return false;

			String king;

			switch (Realm)
			{
				case eRealm.Albion:
					king = "Constantine";
					break;
				case eRealm.Midgard:
					king = "Eirik";
					break;
				case eRealm.Hibernia:
					king = "Lamfhota";
					break;
				default: 
					king = "Dinberg";
					break;
			}

			String reply = String.Format("I am afraid, but King {0} is busy right now.", king);

			InventoryItem personalBindRecallStone =
				player.Inventory.GetFirstItemByID("Personal_Bind_Recall_Stone",
				eInventorySlot.Min_Inv, eInventorySlot.Max_Inv);

			if (personalBindRecallStone == null)
				reply += " If you're only here to get your Personal Bind Recall Stone then I'll see what I can [do].";

			SayTo(player, reply);
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

			if (text.ToLower() == "do")
			{
				if (player.Inventory.GetFirstItemByID("Personal_Bind_Recall_Stone",
					eInventorySlot.Min_Inv, eInventorySlot.Max_Inv) == null)
				{
					SayTo(player, "Very well then. Here's your Personal Bind Recall Stone, may it serve you well.");
					player.ReceiveItem(this, "Personal_Bind_Recall_Stone");
				}
				return false;
			}
			return true;
		}
	}
}
