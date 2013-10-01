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
	/// Holds the arguments for the TurnTo event of GameNPC
	/// </summary>
	public class TurnToEventArgs : EventArgs
	{
		private int x;
		private int y;

		/// <summary>
		/// Constructs a new TurnToEventArgs
		/// </summary>
		/// <param name="x">the target x</param>
		/// <param name="y">the target y</param>
		public TurnToEventArgs(int x, int y)
		{
			this.x=x;
			this.y=y;
		}

		/// <summary>
		/// Gets the target X
		/// </summary>
		public int X
		{
			get { return x; }
		}		
		
		/// <summary>
		/// Gets the target Y
		/// </summary>
		public int Y
		{
			get { return y; }
		}
	}
}
