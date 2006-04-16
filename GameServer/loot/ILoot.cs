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
using DOL.GS.Database;

namespace DOL.GS.Loot
{
	/// <summary>
	/// Interface for loot :)
	/// </summary>
	public interface ILoot
	{
		/// <summary>
		/// The unique db id of tis loot
		/// </summary>	
		int LootID { get; set; }

		/// <summary>
		/// The chance to see this loot on the ground (0 - 1000)
		/// </summary>	
		int Chance { get; set; }

		/// <summary>
		/// This method is used to return the object to add to the world base on this loot
		/// </summary>	
		GameObjectTimed GetLoot(GameMob killedMob, GameLiving killer);
	}
}
