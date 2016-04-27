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
using System.Linq;
using System.Reflection;

namespace DOL.Database.Attributes
{
	/// <summary>
	/// Utils Method for Handling DOL Database Attributes
	/// </summary>
	public static class AttributesUtils
	{
		/// <summary>
		/// Returns the TableName from Type if DataTable Attribute is found 
		/// </summary>
		/// <param name="type">Type inherited from DataObject</param>
		/// <returns>Table Name from DataTable Attribute or ClassName</returns>
		public static string GetTableName(Type type)
		{
			// Check if Type is Element
			if (type.HasElementType)
				type = type.GetElementType();
			
			var dataTable = type.GetCustomAttributes<DataTable>(true).FirstOrDefault();
			
			if (dataTable != null && !string.IsNullOrEmpty(dataTable.TableName))
				return dataTable.TableName;
			
			return type.Name;
		}

		/// <summary>
		/// Returns the ViewName from Type if DataTable Attribute is found 
		/// </summary>
		/// <param name="type">Type inherited from DataObject</param>
		/// <returns>View Name from DataTable Attribute or null</returns>
		public static string GetViewName(Type type)
		{
			// Check if Type is Element
			if (type.HasElementType)
				type = type.GetElementType();
			
			var dataTable = type.GetCustomAttributes<DataTable>(true).FirstOrDefault();
			
			if (dataTable != null && !string.IsNullOrEmpty(dataTable.ViewName))
				return dataTable.ViewName;
			
			return null;
		}

		/// <summary>
		/// Return Table View or Table Name
		/// </summary>
		/// <param name="type">Type inherited from DataObject</param>
		/// <returns>View Name if available, Table Name default</returns>
		public static string GetTableOrViewName(Type type)
		{
			// Graveen: introducing view selection hack (before rewriting the layer :D)
			// basically, a view must exist and is created with the following:
			//
			//	[DataTable(TableName="InventoryItem",ViewName = "MarketItem")]
			//	public class SomeMarketItems : InventoryItem {};
			//
			//  here, we rely on the view called MarketItem,
			//  based on the InventoryItem table. We have to tell to the code
			//  only to bypass the id generated with FROM by the above
			//  code.
			// 			
			return GetViewName(type) ?? GetTableName(type);
		}
		
		/// <summary>
		/// Is this Data Table Pre-Cached on startup?
		/// </summary>
		/// <param name="type">Type inherited from DataObject</param>
		/// <returns>True if Pre-Cached Flag is set</returns>
		public static bool GetPreCachedFlag(Type type)
		{
			var dataTable = type.GetCustomAttributes<DataTable>(true).FirstOrDefault();
			
			if (dataTable != null)
				return dataTable.PreCache;

			return false;
		}
	}
}
