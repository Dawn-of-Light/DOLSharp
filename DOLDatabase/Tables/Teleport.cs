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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Teleport location table.
	/// </summary>
	/// <author>Aredhel</author>
	[DataTable(TableName = "Teleport")]
	public class Teleport : DataObject
	{
	    /// <summary>
		/// Create a new teleport location.
		/// </summary>
		public Teleport()
		{
			Type = string.Empty;
			TeleportID = "UNDEFINED";
			Realm = 0;
			RegionID = 0;
			X = 0;
			Y = 0;
			Z = 0;
			Heading = 0;
		}

		/// <summary>
		/// Teleporter type.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Type { get; set; }

	    /// <summary>
		/// ID for this teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)] // Dre: Index or Unique ?
		public string TeleportID { get; set; }

	    /// <summary>
		/// Realm for this teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Realm { get; set; }

	    /// <summary>
		/// Realm for this teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int RegionID { get; set; }

	    /// <summary>
		/// X coordinate for teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int X { get; set; }

	    /// <summary>
		/// Y coordinate for teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Y { get; set; }

	    /// <summary>
		/// Z coordinate for teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Z { get; set; }

	    /// <summary>
		/// Heading for teleport location.
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public int Heading { get; set; }
	}
}