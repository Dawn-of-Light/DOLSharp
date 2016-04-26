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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using DataTable = System.Data.DataTable;

using DOL.Database.Cache;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// DataTableHandler
	/// </summary>
	public sealed class DataTableHandler
	{
		private readonly ICache _cache;
		private readonly DataSet _dset;
		
		/// <summary>
		/// Has Relations
		/// </summary>
		public bool HasRelations { get; private set; }
		/// <summary>
		/// Pre Cache Directory Handler
		/// </summary>
		private readonly ConcurrentDictionary<object, DataObject> _precache;		
		/// <summary>
		/// The Table Name for this Handler
		/// </summary>
		public string TableName { get; private set; }
		/// <summary>
		/// Element Bindings for this Handler
		/// </summary>
		public ElementBinding[] ElementBindings { get; private set; }
		/// <summary>
		/// Retrieve Element Bindings for DataTable Fields Only
		/// </summary>
		public IEnumerable<ElementBinding> FieldElementBindings 
		{
			get { return ElementBindings.Where(bind => bind.Relation == null); }
		}
		/// <summary>
		/// Data Table Handled
		/// </summary>
		public DataTable Table { get; private set; }
		
		/// <summary>
		/// Create new instance of <see cref="DataTableHandler"/>
		/// </summary>
		/// <param name="type"></param>
		public DataTableHandler(Type type)
		{
			// Init Cache and Table Params
			TableName = AttributesUtils.GetTableOrViewName(type);

			HasRelations = false;
			UsesPreCaching = AttributesUtils.GetPreCachedFlag(type);
			if (UsesPreCaching)
				_precache = new ConcurrentDictionary<object, DataObject>();
			
			_cache = new SimpleCache();
			_dset = new DataSet();
			_dset.DataSetName = TableName;
			_dset.EnforceConstraints = true;
			_dset.CaseSensitive = false;
			
			// Parse Table Type
			ElementBindings = type.GetMembers().Select(member => new ElementBinding(member)).Where(bind => bind.IsDataElementBinding).ToArray();
			
			// If no Primary Key AutoIncrement add GUID
			if (FieldElementBindings.Any(bind => bind.PrimaryKey != null && !bind.PrimaryKey.AutoIncrement))
				ElementBindings = ElementBindings.Concat(new [] {
				                                         	new ElementBinding(type.GetProperty("ObjectId"),
				                                         	                   new DataElement(){ Unique = true },
				                                         	                   string.Format("{0}_ID", TableName))
				                                         }).ToArray();
			else if (FieldElementBindings.All(bind => bind.PrimaryKey == null))
				ElementBindings = ElementBindings.Concat(new [] {
				                                         	new ElementBinding(type.GetProperty("ObjectId"),
				                                         	                   new PrimaryKey(),
				                                         	                   string.Format("{0}_ID", TableName))
				                                         }).ToArray();
			// Prepare Table
			Table = new DataTable(TableName);
			var multipleUnique = new List<ElementBinding>();
			var indexes = new Dictionary<string, ElementBinding[]>();
			
			//Build Table for DataSet
			foreach (var bind in ElementBindings)
			{
				if (bind.Relation != null)
				{
					HasRelations = true;
					continue;
				}
				
				var column = Table.Columns.Add(bind.ColumnName, bind.ValueType);
				
				if (bind.PrimaryKey != null)
				{
					column.AutoIncrement = bind.PrimaryKey.AutoIncrement;
					Table.PrimaryKey = new [] { column };
				}
				
				if (bind.DataElement != null)
				{
					column.AllowDBNull = bind.DataElement.AllowDbNull;
					
					// Store Multi Unique for definition after table
					// Single Unique can be defined directly.
					if (!string.IsNullOrEmpty(bind.DataElement.UniqueColumns))
					{
						multipleUnique.Add(bind);

					}
					else if (bind.DataElement.Unique)
					{
						Table.Constraints.Add(new UniqueConstraint(string.Format("UNIQUE_{0}", bind.ColumnName), column));
					}
					
					// Store Indexes for definition after table
					if (!string.IsNullOrEmpty(bind.DataElement.IndexColumns))
						indexes.Add(string.Format("INDEX_{0}", bind.ColumnName), bind.DataElement.IndexColumns.Split(',')
						            .Select(col => FieldElementBindings.FirstOrDefault(ind => ind.ColumnName.Equals(col.Trim(), StringComparison.OrdinalIgnoreCase)))
						            .Concat(new [] { bind }).ToArray());
					else if (bind.DataElement.Index)
						indexes.Add(string.Format("INDEX_{0}", bind.ColumnName), new [] { bind });
					
					if (bind.DataElement.Varchar > 0)
						column.ExtendedProperties.Add("VARCHAR", bind.DataElement.Varchar);
				}
			}
			// Set Indexes when all columns are set
			Table.ExtendedProperties.Add("INDEXES", indexes.Select(kv => new KeyValuePair<string, DataColumn[]>(
				kv.Key,
				kv.Value.Select(bind => Table.Columns[bind.ColumnName]).ToArray()))
				.ToDictionary(kv => kv.Key, kv => kv.Value));
			
			// Set Unique Constraints when all columns are set.
			foreach (var bind in multipleUnique)
			{
				var columns = bind.DataElement.UniqueColumns.Split(',').Select(column => column.Trim()).Concat(new [] { bind.ColumnName });
				Table.Constraints.Add(new UniqueConstraint(string.Format("UNIQUE_{0}", bind.ColumnName),
				                                           columns.Select(column => Table.Columns[column]).ToArray()));
			}
			
			// Set Table for DataSet
			_dset.Tables.Add(Table);
		}

		/// <summary>
		/// Cache
		/// </summary>
		public ICache Cache
		{
			get { return _cache; }
		}

		/// <summary>
		/// DataSet
		/// </summary>
		public DataSet DataSet
		{
			get { return _dset; }
		}

		/// <summary>
		/// Uses Precaching
		/// </summary>
		public bool UsesPreCaching { get; set; }

		/// <summary>
		/// Set Cache Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <param name="obj">The value DataObject</param>
		public void SetCacheObject(object key, DataObject obj)
		{
			_cache[key] = obj;
		}

		/// <summary>
		/// Get Cache Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetCacheObject(object key)
		{
			return _cache[key] as DataObject;
		}
		
		#region PreCache Handling
		/// <summary>
		/// Set Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <param name="obj">The value DataObject</param>
		public void SetPreCachedObject(object key, DataObject obj)
		{
			_precache.AddOrUpdate(key, obj, (k, v) => obj);
		}

		/// <summary>
		/// Get Pre-Cached Object
		/// </summary>
		/// <param name="key">The key object</param>
		/// <returns>The value DataObject</returns>
		public DataObject GetPreCachedObject(object key)
		{
			DataObject obj;
			return _precache.TryGetValue(key, out obj) ? obj : null;
		}
		
		/// <summary>
		/// Search Pre-Cached Object
		/// </summary>
		/// <param name="whereClause">Select Object when True</param>
		/// <returns>IEnumerable of DataObjects</returns>
		public IEnumerable<DataObject> SearchPreCachedObjects(Func<DataObject, bool> whereClause)
		{
			return _precache.Where(kv => whereClause(kv.Value)).Select(kv => kv.Value);
		}
		#endregion
	}
}