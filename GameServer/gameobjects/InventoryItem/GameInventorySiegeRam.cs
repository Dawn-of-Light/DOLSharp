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
using System.Reflection;
using System.Collections.Generic;

using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;

using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a Siege Ram inventory item
	/// </summary>
	public class GameInventorySiegeRam : GameInventoryItem, IGameInventoryItem, ITranslatableObject
	{

		public GameInventorySiegeRam()
			: base()
		{
		}

		public GameInventorySiegeRam(ItemTemplate template)
			: base(template)
		{
		}

		public GameInventorySiegeRam(ItemUnique template)
			: base(template)
		{
		}

		public GameInventorySiegeRam(InventoryItem item)
			: base(item)
		{
			OwnerID = item.OwnerID;
			ObjectId = item.ObjectId;
		}


		/// <summary>
		/// Try and use this item
		/// </summary>
		/// <param name="player"></param>
		/// <returns>true if item use is handled here</returns>
		public override bool Use(GamePlayer player)
		{
			// Create the siege ram 
			GameSiegeRam ram = new GameSiegeRam();

			// Level, name, model and other properties of the siege ram are retrieved from the inventory item
			ram.Level = Convert.ToByte(Level);
			ram.Name = Name;
			ram.Model = (ushort) Model;
			ram.Position = player.Position;
			ram.Realm = player.Realm;
			ram.AddToWorld();

			// Remove current item from player's inventory 
			InventoryItem item = player.Inventory.GetItem((eInventorySlot)SlotPosition);
			player.Inventory.RemoveItem(item);

			return true;
		}
	}

}
