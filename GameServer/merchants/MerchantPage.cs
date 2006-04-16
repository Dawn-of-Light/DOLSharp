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
	/// This enumeration holds all possible currency type
	/// </summary>
	public enum eCurrencyType : byte
	{
		Money		= 0,
		BountyPoint	= 1,
		// TODO ect ...
	};

	/// <summary>
	/// This class represent a page in the merchant windows
	/// </summary>
	public class MerchantPage
	{
		#region Declaration

		/// <summary>
		/// The maximum number of items on one page
		/// </summary>
		public const byte MAX_ITEMS_IN_MERCHANTPAGE = 30;

		/// <summary>
		/// The unique db identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// Gets or sets the unique db identifier
		/// </summary>
		public int MerchantPageID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// The postion of the page in the merchant list (first page, second page ect ...)
		/// </summary>
		private int m_position;

		/// <summary>
		/// Gets or sets the position of the page in the merchant list
		/// </summary>
		public int Position
		{
			get { return m_position; }
			set	{ m_position = value; }
		}

		/// <summary>
		/// The currency used to buy a item in this page
		/// </summary>
		private eCurrencyType m_currency;

		/// <summary>
		/// Gets or sets the currency used to buy a item in this page
		/// </summary>
		public eCurrencyType Currency
		{
			get { return m_currency; }
			set	{ m_currency = value; }
		}

		/// <summary>
		/// The collection of all the items in this page
		/// </summary>
		private IDictionary  m_merchantItems;

		/// <summary>
		/// Gets or sets the collection of all the items in this page
		/// </summary>
		public IDictionary MerchantItems
		{
			get
			{
				if(m_merchantItems == null) m_merchantItems = new HybridDictionary();
				return m_merchantItems;
			}
			set	{ m_merchantItems = value; }
		}

		#endregion
	}
}
