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
using System.Collections.Specialized;
using DOL.GS.PacketHandler;

namespace DOL.GS
{
	/// <summary>
	/// This class represent a item in a merchant windows
	/// </summary>
	public class MerchantItem
	{
		#region Declaration

		/// <summary>
		/// The unique db identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// Gets or sets the unique db identifier
		/// </summary>
		public int MerchantItemID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// The position of the item in the page (first, second ect ...)
		/// </summary>
		private int m_position;

		/// <summary>
		/// Gets or sets the position of the item in the page
		/// </summary>
		public int Position
		{
			get { return m_position; }
			set	{ m_position = value; }
		}

		/// <summary>
		/// The itemTemplate linked with this merchantItem
		/// </summary>
		private GenericItemTemplate m_itemTemplate;

		/// <summary>
		/// Gets or sets the itemTemplate linked with this merchantItem
		/// </summary>
		public GenericItemTemplate ItemTemplate
		{
			get { return m_itemTemplate; }
			set	{ m_itemTemplate = value; }
		}

		#endregion
	}
}
