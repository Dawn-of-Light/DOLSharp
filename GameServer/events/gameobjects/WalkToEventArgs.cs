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
	/// Holds the arguments for the WalkTo event of GameNPC
	/// </summary>
	public class WalkToEventArgs : EventArgs
	{
		private int x;
		private int y;
		private int z;
		private int speed;

		/// <summary>
		/// Constructs a new WalkToEventArgs
		/// </summary>
		/// <param name="x">the target x</param>
		/// <param name="y">the target y</param>
		/// <param name="z">the target z</param>
		/// <param name="speed">the walk speed</param>
		public WalkToEventArgs(int x, int y, int z, int speed)
		{
			this.x=x;
			this.y=y;
			this.z=z;
			this.speed=speed;
		}

		/// <summary>
		/// Gets the target x
		/// </summary>
		public int X
		{
			get { return x; }
		}		
		
		/// <summary>
		/// Gets the target y
		/// </summary>
		public int Y
		{
			get { return y; }
		}
		
		/// <summary>
		/// Gets the target z
		/// </summary>
		public int Z
		{
			get { return z; }
		}
		
		/// <summary>
		/// Gets the walk speed
		/// </summary>
		public int Speed
		{
			get { return speed; }
		}
	}
}
