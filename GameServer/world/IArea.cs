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
using DOL.Events;

namespace DOL.GS
{	
	/// <summary>
	/// Interface for areas within game, extend this or AbstractArea if you need to define a new area shape that isn't already defined.
	/// Defined ones:
	/// - Area.Cricle
	/// - Area.Square
	/// </summary>
	public interface IArea
	{					
		/// <summary>
		/// Returns the ID of this zone
		/// </summary>
		int AreaID { get; set;}		

		/// <summary>
		/// Checks wether is intersects with given zone.
		/// This is needed to build an area.zone mapping cache for performance.		
		/// </summary>
		/// <param name="zone"></param>
		/// <returns></returns>
		bool IsIntersectingZone(Zone zone);
		
		/// <summary>
		/// Checks wether given spot is within areas range or not
		/// </summary>
		/// <param name="spot"></param>
		/// <returns></returns>
		bool IsContaining(Point spot);
		
		/// <summary>
		/// Called whenever a player leaves the given area
		/// </summary>
		/// <param name="player"></param>
		void OnPlayerLeave(GamePlayer player);

		/// <summary>
		/// Called whenever a player enters the given area
		/// </summary>
		/// <param name="player"></param>
		void OnPlayerEnter(GamePlayer player);
	}
}
