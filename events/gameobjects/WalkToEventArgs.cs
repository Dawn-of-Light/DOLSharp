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
	/// Holds the arguments for the WalkTo event of GameNPC
	/// </summary>
	public class WalkToEventArgs : EventArgs
	{
		Point p;
		private int speed;

		/// <summary>
		/// Constructs a new WalkToEventArgs.
		/// </summary>
		/// <param name="walkTarget">the target point to walk to.</param>
		/// <param name="speed">the walk speed.</param>
		public WalkToEventArgs(Point walkTarget, int speed)
		{
			this.p = walkTarget;
			this.speed = speed;
		}

		/// <summary>
		/// Gets the walk target.
		/// </summary>
		public Point WalkTarget
		{
			get { return p; }
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
