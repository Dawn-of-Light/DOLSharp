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
using System.Collections;
using System.Collections.Generic;
using DOL.Language;
using DOL.GS.PacketHandler;
using DOL.Database;
using DOL.GS.Spells;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// This class represents a relic in a players inventory
	/// </summary>
	public class GameInventoryRelic : GameInventoryItem
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public GameInventoryRelic()
			: base()
		{
		}

		public GameInventoryRelic(ItemTemplate template)
			: base(template)
		{
		}

		public GameInventoryRelic(ItemUnique template)
			: base(template)
		{
		}

		public GameInventoryRelic(InventoryItem item)
			: base(item)
		{
			OwnerID = item.OwnerID;
			ObjectId = item.ObjectId;
		}

		/// <summary>
		/// Can this item be saved or loaded from the database?
		/// </summary>
		public override bool CanPersist
		{
			get { return false; } // relics can never be saved
		}
	}
}
