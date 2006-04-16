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
using System.Collections;
using System.Reflection;
using System.Text;
using System.Threading;
using DOL.GS.Database;
using NHibernate.Expression;
using log4net;

namespace DOL.GS
{
	public class GameNpcInventory : GameLivingInventory
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Check if the slot is valid in the inventory
		/// </summary>
		/// <param name="slot">SlotPosition to check</param>
		/// <returns>the slot if it's valid or eInventorySlot.Invalid if not</returns>
		protected override eInventorySlot GetValidInventorySlot(eInventorySlot slot)
		{
			foreach (eInventorySlot visibleSlot in VISIBLE_SLOTS)
				if (visibleSlot == slot)
					return slot;
			return eInventorySlot.Invalid;
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="slot"></param>
		/// <param name="item"></param>
		/// <returns>false</returns>
		public override bool AddItem(eInventorySlot slot, GenericItemBase item)
		{
			if (item == null) return false;
			lock (this)
			{
				slot = GetValidInventorySlot(slot);
				if (slot == eInventorySlot.Invalid) return false;
				if (m_items.Contains((int)slot))
				{
					if (log.IsErrorEnabled)
						log.Error("Inventory.AddItem -> Destination slot is not empty ("+(int)slot+")\n\n" + Environment.StackTrace);
					return false;
				}
				m_items.Add((int)slot, item);
				item.SlotPosition=(int)slot;

				return true;
			}
		}

		/// <summary>
		/// Overridden. Inventory template cannot be modified.
		/// </summary>
		/// <param name="item">the item to remove</param>
		/// <returns>false</returns>
		public override bool RemoveItem(GenericItemBase item)
		{
			lock(this)
			{
				if (item == null) return false;
				if (m_items.Contains(item.SlotPosition))
				{
					m_items.Remove(item.SlotPosition);

					item.SlotPosition = (int)eInventorySlot.Invalid;

					return true;
				}
			}
			return false;
		}
	}
}