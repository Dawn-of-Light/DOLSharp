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

namespace DOL.Database.Connection
{
	/// <summary>
	/// Existing Table Row Binding
	/// </summary>
	public sealed class TableRowBindind
	{
		/// <summary>
		/// Column Name
		/// </summary>
		public string ColumnName { get; private set; }
		/// <summary>
		/// Column Type
		/// </summary>
		public string ColumnType { get; private set; }
		/// <summary>
		/// Column Allow Null
		/// </summary>
		public bool AllowDbNull { get; private set; }
		/// <summary>
		/// Column Allow Null
		/// </summary>
		public bool Primary { get; private set; }
		
		/// <summary>
		/// Create new instance of <see cref="TableRowBindind"/>
		/// </summary>
		/// <param name="ColumnName">Row Column Name</param>
		/// <param name="ColumnType">Row Column Type</param>
		/// <param name="AllowDbNull">Row DB Null</param>
		public TableRowBindind(string ColumnName, string ColumnType, bool AllowDbNull, bool Primary)
		{
			this.ColumnName = ColumnName;
			this.ColumnType = ColumnType;
			this.AllowDbNull = AllowDbNull;
			this.Primary = Primary;
		}
	}
}
