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
	/// Holds the arguments for the RiderMount event of GameNPC
	/// </summary>
	public class RiderMountEventArgs : EventArgs
	{
		private GamePlayer rider;
		private GameLiving steed;

		/// <summary>
		/// Constructs a new RiderMountEventArgs
		/// </summary>
		/// <param name="rider">the rider mounting</param>
		/// <param name="steed">the steed being mounted</param>
		public RiderMountEventArgs(GamePlayer rider, GameLiving steed)
		{
			this.rider = rider;
			this.steed = steed;
		}

		/// <summary>
		/// Gets the GamePlayer rider who is mounting the steed
		/// </summary>
		public GamePlayer Rider
		{
			get { return rider; }
		}

		/// <summary>
		/// Gets the GameLiving steed who is being mounted by the rider
		/// </summary>
		public GameLiving Steed
		{
			get { return steed; }
		}

	}
}
