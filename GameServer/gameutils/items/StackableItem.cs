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
using System;
using System.Reflection;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a StackableItem
	/// </summary> 
	public class StackableItem : GenericItem, IStackableItem
	{
		#region Declaraction
		/// <summary>
		/// The max count of the item
		/// </summary>
		private int m_maxCount;

		/// <summary>
		/// The count of items in the same inventory slot
		/// </summary>
		private int m_count;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the maximum number of items stackables in the same inventory slot
		/// </summary>
		public int MaxCount
		{
			get { return m_maxCount; }
			set	{ m_maxCount = value; }
		}

		/// <summary>
		/// Gets or sets how much items are stacked in the same inventory slot (it set so the value and the weight)
		/// </summary>
		public int Count
		{
			get { return m_count; }
			set { m_count = value; }
		}

		#endregion

		/// <summary>
		/// Checks if the object can stack with the param
		/// </summary>
		public virtual bool CanStackWith(IStackableItem item)
		{
			StackableItem itemStackable = item as StackableItem;
			if(itemStackable == null) return false;
			
			return (Name == itemStackable.Name 
				&& Level == itemStackable.Level 
				&& Realm == itemStackable.Realm 
				&& Model == itemStackable.Model 
				&& IsSaleable == itemStackable.IsSaleable 
				&& IsDropable == itemStackable.IsDropable
				&& IsTradable == itemStackable.IsTradable
				&& QuestName == itemStackable.QuestName
				&& CrafterName == itemStackable.CrafterName
				&& MaxCount == itemStackable.MaxCount);
		}

		/// <summary>
		/// Create a shallow Copy of the oject stack
		/// </summary>
		public virtual IStackableItem ShallowCopy()
		{
			StackableItem item = (StackableItem)MemberwiseClone();
			item.ItemID = 0; // tag the item as non already saved
			item.SlotPosition = (int)eInventorySlot.Invalid;
			return item;
		}
	}
}	