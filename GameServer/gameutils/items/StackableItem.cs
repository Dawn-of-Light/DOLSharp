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
	public class StackableItem : GenericItem
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
			set
			{
				if(m_count <= 0) // to allow to set it the first time
				{
					m_count = value;
				} 
				else
				{
					int countToAdd = value - m_count;
					Weight += countToAdd * Weight / m_count;
					Value +=  countToAdd * Value / m_count;
					m_count += countToAdd;
				}
			}
		}

		#endregion

		/// <summary>
		/// Checks if the object can stack with the param
		/// </summary>
		public virtual bool CanStackWith(StackableItem item)
		{
			return (Name == item.Name 
				&& Level == item.Level 
				&& Realm == item.Realm 
				&& Model == item.Model 
				&& IsSaleable == item.IsSaleable 
				&& IsDropable == item.IsDropable
				&& IsTradable == item.IsTradable
				&& QuestName == item.QuestName
				&& CrafterName == item.CrafterName
				&& MaxCount == item.MaxCount);
		}

		/// <summary>
		/// Create a shallow Copy of the oject stack
		/// </summary>
		public virtual StackableItem ShallowCopy()
		{
			return (StackableItem)MemberwiseClone();
		}
	}
}	