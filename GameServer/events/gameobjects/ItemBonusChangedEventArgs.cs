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
using System.Collections.Generic;
using System.Text;

namespace DOL.Events
{
	/// <summary>
	/// Holds arguments for an item bonus change (for example when an
	/// artifact gains a new bonus).
	/// </summary>
	/// <author>Aredhel</author>
	class ItemBonusChangedEventArgs : EventArgs
	{
		private int m_bonusType, m_bonusAmount;

		public ItemBonusChangedEventArgs(int bonusType, int bonusAmount)
		{
			m_bonusType = bonusType;
			m_bonusAmount = bonusAmount;
		}

		/// <summary>
		/// The bonus type.
		/// </summary>
		public int BonusType
		{
			get { return m_bonusType; }
		}

		/// <summary>
		/// The bonus amount.
		/// </summary>
		public int BonusAmount
		{
			get { return m_bonusAmount; }
		}
	}
}
