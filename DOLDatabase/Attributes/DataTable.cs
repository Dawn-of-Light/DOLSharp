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

namespace DOL.Database.Attributes
{
	/// <summary>
	/// Attribute to mark a Derived Class of DataObject as Table
	/// Mainly to set the TableName differnt to Classname
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class DataTable : Attribute
	{
		/// <summary>
		/// Constrctor of DataTable sets the TableName-Property to null.
		/// </summary>
		public DataTable()
		{
			TableName = null;
			PreCache = false;
		}

		/// <summary>
		/// TableName-Property if null the Classname is used as Tablename.
		/// </summary>
		/// <value>The TableName that sould be used or <c>null</c> for Classname</value>
		public string TableName { get; set; }

		/// <summary>
		/// The view is based on the ViewTarget table
		/// </summary>
		public string ViewName { get; set; }

		/// <summary>
		/// If preloading data is required for performance in Findobjectbykey
		/// Uses more memory then
		/// </summary>
		/// <value>true if enabled</value>
		public bool PreCache { get; set; }
	}
}
