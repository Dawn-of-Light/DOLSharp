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
	/// Summary description for a StackableItemTemplate
	/// </summary> 
	public class StackableItemTemplate : GenericItemTemplate
	{
		#region Declaraction
		/// <summary>
		/// The max count of the template
		/// </summary>
		protected int m_maxCount;

		/// <summary>
		/// The number of instance to buy
		/// </summary>
		protected int m_packSize;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets how much items could be stack in the same inventory slot
		/// </summary>
		public int MaxCount
		{
			get { return m_maxCount; }
			set	{ m_maxCount = value; }
		}

		/// <summary>
		/// Gets or sets how much items will be buy
		/// </summary>
		public int PackSize
		{
			get { return m_packSize; }
			set	{ m_packSize = value; }
		}

		#endregion

		/// <summary>
		/// Create 1 object usable by players using this template
		/// </summary>
		public override GenericItem CreateInstance()
		{
			StackableItem item = new StackableItem();
			item.Name = m_name;
			item.Level = m_level;
			item.Weight = m_weight * m_packSize;
			item.Value = m_value * m_packSize;
			item.Realm = m_realm;
			item.Model = m_model;
			item.IsSaleable = m_isSaleable;
			item.IsTradable = m_isTradable;
			item.IsDropable = m_isDropable;
			item.MaxCount = m_maxCount;
			item.Count = m_packSize;
			return item;
		}
	}
}	