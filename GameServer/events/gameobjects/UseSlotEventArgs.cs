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

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for UseSlot event of GamePlayer.
	/// </summary>
	public class UseSlotEventArgs : EventArgs
	{
		private int m_slot;
		private int m_type;

		/// <summary>
		/// Constructs new UseSlotEventArgs
		/// </summary>
		/// <param name="slot">The used slot</param>
		/// <param name="type">The type of 'use' used (0=simple click on icon, 1=/use, 2=/use2)</param>
		public UseSlotEventArgs(int slot, int type)
		{
			this.m_slot = slot;
			this.m_type = type;
		}

		/// <summary>
		/// Gets the slot that was used
		/// </summary>
		public int Slot
		{
			get { return m_slot; }
		}

		/// <summary>
		/// Gets the type of 'use' used (0=simple click on icon, 1=/use, 2=/use2)
		/// </summary>
		public int Type
		{
			get { return m_type; }
		}
	}
}
