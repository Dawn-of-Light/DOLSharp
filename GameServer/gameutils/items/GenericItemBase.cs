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
using System.Text;
using DOL.GS.Database;
using DOL.GS.PacketHandler;
using log4net;

namespace DOL.GS
{
	/// <summary>
	/// Summary description for a GenericItemTemplate
	/// </summary> 
	public abstract class GenericItemBase
	{
		#region Declaraction
		/// <summary>
		/// The unique generic item identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// The item model
		/// </summary>
		private int m_model;

		/// <summary>
		/// The inventory slot where this item is stored
		/// </summary>
		private int m_slotPosition;

		#endregion

		#region Get and Set
		/// <summary>
		/// Gets or sets the unique generic item identifier
		/// </summary>
		public int ItemID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// Gets or sets the graphic model of the item
		/// </summary>
		public int Model
		{
			get { return m_model; }
			set	{ m_model = value; }
		}

		/// <summary>
		/// Gets or sets the inventory slot where this item is stored
		/// </summary>
		public int SlotPosition
		{
			get { return m_slotPosition; }
			set	{ m_slotPosition = value; }
		}

		#endregion

		/// <summary>
		/// Returns the string representation of the object
		/// </summary>
		/// <returns></returns>
		public override string ToString()
		{
			return new StringBuilder(128)
				.Append(GetType().FullName)
				.Append(" ItemID =").Append(m_id)
				.Append(" model =").Append(m_model)
				.Append(" slotPosition =").Append(m_slotPosition)
				.ToString();
		}
	}
}
