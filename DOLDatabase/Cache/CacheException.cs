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

namespace DOL.Database.Cache {

	/// <summary>
	/// Represents any exception from an <c>ICache</c>
	/// </summary>
	public class CacheException : DatabaseException {
		/// <summary>
		/// Constructor for an CacheException that indicates a Problem with the Cache
		/// </summary>
		/// <param name="s">String that describes the Error</param>
		public CacheException(string s) : base(s) { }

		/// <summary>
		/// Constructor for an CacheException that indicates a Problem with the Cache
		/// </summary>
		/// <param name="e">Exception that is the Reason for the Cache-Problem</param>
		public CacheException(Exception e) : base(e) { }
	}

}
