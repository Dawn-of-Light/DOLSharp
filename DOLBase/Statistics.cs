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

namespace DOL
{
	/// <summary>
	/// Static class that holds various statistics about the server instance.
	/// </summary>
	public static partial class Statistics
	{
		/// <summary>
		/// The total number of bytes received.
		/// </summary>
		public static long BytesIn;

		/// <summary>
		/// The total number of bytes sent.
		/// </summary>
		public static long BytesOut;

		/// <summary>
		/// The total number of packets received.
		/// </summary>
		public static long PacketsIn;

		/// <summary>
		/// The total number of packets sent.
		/// </summary>
		public static long PacketsOut;
	}
}