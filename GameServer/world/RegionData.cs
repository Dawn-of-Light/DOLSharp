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
using DOL.Database;

namespace DOL.GS
{
	/// <summary>
	/// Helper class for region registration
	/// </summary>
	public class RegionData : IComparable
	{
		/// <summary>
		/// The region id
		/// </summary>
		public ushort Id;
		/// <summary>
		/// The region name
		/// </summary>
		public string Name;
		/// <summary>
		/// The region description
		/// </summary>
		public string Description;
		/// <summary>
		/// The region IP
		/// </summary>
		public string Ip;
		/// <summary>
		/// The region port
		/// </summary>
		public ushort Port;
		/// <summary>
		/// The region water level
		/// </summary>
		public int WaterLevel;
		/// <summary>
		/// The region diving flag
		/// </summary>
		public bool DivingEnabled;
		/// <summary>
		/// The region housing flag
		/// </summary>
		public bool HousingEnabled;
		/// <summary>
		/// The region expansion
		/// </summary>
		public int Expansion;
		/// <summary>
		/// The region mobs
		/// </summary>
		public Mob[] Mobs;

		/// <summary>
		/// Compares 2 objects
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public int CompareTo(object obj)
		{
			RegionData cmp = obj as RegionData;
			if (cmp == null) return -1;
			return cmp.Mobs.Length - Mobs.Length;
		}
	}
}
