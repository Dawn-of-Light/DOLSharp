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
using DOL.GS;

namespace DOL.Events
{
	/// <summary>
	/// Holds the arguments for the TurnTo event of GameNPC
	/// </summary>
	public class TurnToEventArgs : EventArgs
	{
		private Point m_point;

		/// <summary>
		/// Constructs a new TurnToEventArgs
		/// </summary>
		/// <param name="x">the target x</param>
		/// <param name="y">the target y</param>
		public TurnToEventArgs(Point point)
		{
			m_point = point;
		}

		/// <summary>
		/// Gets the target point.
		/// </summary>
		public Point Point
		{
			get { return m_point; }
		}
	}
}
