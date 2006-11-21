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
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Interface for loot generators
	/// </summary>
	/// 
	public interface ILootGenerator
	{
		/// <summary>
		/// Returns the priority of this lootgenerator,
		/// if priority == 0 can be used together with other generators on the same mob.
		/// If a generator in the list of possibe generators has priority>0 only the generator with the biggest priority will be used.
		/// This can be useful if you want to define a general generator for almost all mobs and define a
		/// special one with priority>0 for a special mob that should use the default generator.
		/// </summary>
		int ExclusivePriority
		{
			get;
			set;
		}

		/// <summary>
		/// Generates a list of ItemTemplates that this mob should drop
		/// </summary>		
		/// <param name="mob">Mob that drops loot</param>
		/// <param name="killer"></param>
		/// <returns>List of ItemTemplates</returns>
		LootList GenerateLoot(GameMob mob, GameObject killer);
	}
}
