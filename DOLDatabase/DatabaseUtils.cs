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
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace DOL.Database
{
	/// <summary>
	/// Database Oriented Tools for custom uses.
	/// </summary>
	public static class DatabaseUtils
	{
		/// <summary>
		/// List all Unique Members of a DataTable, Using them for duplicate Matching.
		/// </summary>
		/// <param name="objectType">DataObject Type</param>
		/// <returns>List of MemberInfo having Unique Attributes</returns>
		public static ElementBinding[][] GetUniqueMembers(Type objectType)
		{
			var tableHandler = new DataTableHandler(objectType);
			
			var uniques = tableHandler.Table.Constraints.OfType<UniqueConstraint>().Where(constraint => !constraint.IsPrimaryKey)
				.Select(constraint => constraint.Columns
				        .Select(col => tableHandler.FieldElementBindings.Single(bind => bind.ColumnName.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase)))
				        .ToArray());
			
			var primary = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey != null
			                                                      && !bind.PrimaryKey.AutoIncrement
			                                                      && !bind.ColumnName.Equals(tableHandler.PrimaryKeyColumnName, StringComparison.OrdinalIgnoreCase))
				.ToArray();
			
			return new [] { primary }.Concat(uniques).ToArray();
		}
		
		/// <summary>
		/// List Remarkable Members of a DataTable, Using them for Default Ordering
		/// This includes non-generated Primary Key, Unique Field, and Indexed Fields (optionnaly)
		/// </summary>
		/// <param name="objectType">DataObject Type</param>
		/// <param name="forceIndexes">Returns Indexes even if enough Unique type have been gathered</param>
		/// <returns>List of Remkarkable MemberInfo of given DataObject</returns>
		public static ElementBinding[] GetRemarkableMembers(Type objectType, bool forceIndexes)
		{
			var tableHandler = new DataTableHandler(objectType);
			
			// Find unique Fields
			var remarkableMember = GetUniqueMembers(objectType);
			
			// We don't have enough fields, Try indexes !
			if (remarkableMember.Length < 1 || forceIndexes)
			{
				var indexes = tableHandler.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>;
				
				return remarkableMember.SelectMany(constraint => constraint)
					.Concat(indexes.SelectMany(index => index.Value).Select(col => tableHandler.FieldElementBindings
					                                                        .Single(bind => bind.ColumnName.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
					.ToArray();
			}

			return remarkableMember.SelectMany(constraint => constraint).ToArray();
		}
	}
}
