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

namespace DOL.Database.Cache {
	
	/// <summary>
	/// Implementors define a caching algorithm.
	/// </summary>
	/// <remarks>
	/// All implementations MUST be threadsafe
	/// </remarks>
	public interface ICache {

		/// <summary>
		/// Gets a Collection of all Key that are in the Cache at the Moment
		/// </summary>
		/// <value>All Keys that are in the Cache</value>

		ICollection Keys
		{
			get;
		}

		/// <summary>
		/// Gets or sets cached data
		/// </summary>
		/// <value>The cached object or <c>null</c></value>
		/// <exception cref="CacheException"></exception>
		object this [object key] 
		{
			get;
			set;
		}
	
	}


}
