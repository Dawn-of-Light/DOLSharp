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
	/// Holds the arguments for the MoveTo event of GameObjects
	/// </summary>
	public class MoveToEventArgs : EventArgs
	{
		private ushort regionID;
		private Point position;
		private ushort heading;

		/// <summary>
		/// Constructs new MoveToEventArgs
		/// </summary>
		/// <param name="regionId">the target regionid</param>
		/// <param name="position">the target position</param>
		/// <param name="heading">the target heading</param>
		public MoveToEventArgs(ushort regionId, Point position, ushort heading)
		{
			this.regionID = regionId;
			this.position = position;
			this.heading = heading;
		}

		/// <summary>
		/// Gets the target RegionID
		/// </summary>
		public ushort RegionId
		{
			get { return regionID; }
		}
		
		/// <summary>
		/// Gets the target position.
		/// </summary>
		public Point NewPosition
		{
			get { return position; }
		}

		/// <summary>
		/// Gets the target heading
		/// </summary>
		public ushort Heading
		{
			get { return heading; }
		}
	}
}
