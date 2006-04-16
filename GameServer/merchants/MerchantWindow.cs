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
	/// This class represent a merchant windows
	/// </summary>
	public class MerchantWindow
	{
		#region Declaration

		/// <summary>
		/// The maximum number of pages supported by clients
		/// </summary>
		public const int MAX_PAGES_IN_MERCHANTWINDOW = 5;

		/// <summary>
		/// The unique db identifier
		/// </summary>
		private int m_id;

		/// <summary>
		/// Gets or sets the unique db identifier
		/// </summary>
		public int MerchantWindowID
		{
			get { return m_id; }
			set	{ m_id = value; }
		}

		/// <summary>
		/// The collection of all the pages in this window
		/// </summary>
		private IDictionary  m_merchantPages;

		/// <summary>
		/// Gets or sets the collection of all the pages in this window
		/// </summary>
		public IDictionary MerchantPages
		{
			get
			{
				if(m_merchantPages == null) m_merchantPages = new HybridDictionary();
				return m_merchantPages;
			}
			set	{ m_merchantPages = value; }
		}

		#endregion
	}
}
