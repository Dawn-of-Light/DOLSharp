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
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;

using DOL.Database.Attributes;
using DOL.Database.Connection;
using DOL.Database.Handlers;

using log4net;

namespace DOL.Database
{
	/// <summary>
	/// Default Object Database Base Implementation
	/// </summary>
	public abstract class ObjectDatabase : IObjectDatabase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		protected static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Number Format Info to Use for Database
		/// </summary>
		protected static readonly NumberFormatInfo Nfi = new CultureInfo("en-US", false).NumberFormat;

		/// <summary>
		/// Data Table Handlers for this Database Handler
		/// </summary>
		protected readonly Dictionary<string, DataTableHandler> TableDatasets = new Dictionary<string, DataTableHandler>();

		/// <summary>
		/// Connection String for this Database
		/// </summary>
		protected string ConnectionString { get; set; }
		
		/// <summary>
		/// Creates a new Instance of <see cref="ObjectDatabase"/>
		/// </summary>
		/// <param name="ConnectionString">Database Connection String</param>
		protected ObjectDatabase(string ConnectionString)
		{
			this.ConnectionString = ConnectionString;
		}
		
		/// <summary>
		/// Helper to Retrieve Table Handler from Object Type
		/// Return Real Table Handler for Modifications Queries
		/// </summary>
		/// <param name="objectType">Object Type</param>
		/// <returns>DataTableHandler for this Object Type or null.</returns>
		protected DataTableHandler GetTableHandler(Type objectType)
		{
			var tableName = AttributesUtils.GetTableName(objectType);
			DataTableHandler handler;
			return TableDatasets.TryGetValue(tableName, out handler) ? handler : null;
		}
		
		/// <summary>
		/// Helper to Retrieve Table or View Handler from Object Type
		/// Return View or Table for Select Queries
		/// </summary>
		/// <param name="objectType">Object Type</param>
		/// <returns>DataTableHandler for this Object Type or null.</returns>
		protected DataTableHandler GetTableOrViewHandler(Type objectType)
		{
			var tableName = AttributesUtils.GetTableOrViewName(objectType);
			DataTableHandler handler;
			return TableDatasets.TryGetValue(tableName, out handler) ? handler : null;
		}

		#region Public Add Objects Implementation
		/// <summary>
		/// Insert a new DataObject into the database and save it
		/// </summary>
		/// <param name="dataObject">DataObject to Add into database</param>
		/// <returns>True if the DataObject was added.</returns>
		public bool AddObject(DataObject dataObject)
		{
			return AddObject(new [] { dataObject });
		}
		
		/// <summary>
		/// Insert new DataObjects into the database and save them
		/// </summary>
		/// <param name="dataObjects">DataObjects to Add into database</param>
		/// <returns>True if All DataObjects were added.</returns>
		public bool AddObject(IEnumerable<DataObject> dataObjects)
		{
			var success = true;
			foreach (var grp in dataObjects.GroupBy(obj => obj.GetType()))
			{
				var tableHandler = GetTableHandler(grp.Key);
				
				if (tableHandler == null)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("AddObject: DataObject Type ({0}) not registered !", grp.Key.FullName);
					success = false;
					continue;
				}
				
				foreach (var allowed in grp.GroupBy(item => item.AllowAdd))
				{
					if (allowed.Key)
					{
						var objs = allowed.ToArray();
						var results = AddObjectImpl(tableHandler, objs);
						
						var resultsByObjs = results.Select((result, index) => new { Success = result, DataObject = objs[index] })
							.GroupBy(obj => obj.Success);
						
						foreach (var resultGrp in resultsByObjs)
						{
							if (resultGrp.Key)
							{
								// Save in Precache if tablehandler use it
								if (tableHandler.UsesPreCaching)
								{
									var primary = tableHandler.PrimaryKey;
									if (primary != null)
									{
										foreach (var successObj in resultGrp.Select(obj => obj.DataObject))
											tableHandler.SetPreCachedObject(primary.GetValue(successObj), successObj);
									}
								}
								
								// Success Objects Need Relations Save
								if (tableHandler.HasRelations)
									success &= SaveObjectRelations(tableHandler, resultGrp.Select(obj => obj.DataObject));
							}
							else
							{
								if (log.IsErrorEnabled)
								{
									foreach(var obj in resultGrp)
										log.ErrorFormat("AddObjects: DataObject ({0}) could not be inserted into database...", obj.DataObject);
								}
								success = false;
							}
						}
					}
					else
					{
						if (log.IsWarnEnabled)
						{
							foreach (var obj in allowed)
								log.WarnFormat("AddObject: DataObject ({0}) not allowed to be added to Database", obj);
						}
						success = false;
					}
				}
			}
			return success;
		}
		#endregion
		#region Public Save Objects Implementation
		/// <summary>
		/// Saves a DataObject to database if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObject">DataObject to Save in database</param>
		/// <returns>True is the DataObject was saved.</returns>
		public bool SaveObject(DataObject dataObject)
		{
			return SaveObject(new [] { dataObject });
		}
		
		/// <summary>
		/// Save DataObjects to database if saving is allowed and object is dirty
		/// </summary>
		/// <param name="dataObjects">DataObjects to Save in database</param>
		/// <returns>True if All DataObjects were saved.</returns>
		public bool SaveObject(IEnumerable<DataObject> dataObjects)
		{
			var success = true;
			foreach (var grp in dataObjects.GroupBy(obj => obj.GetType()))
			{
				var tableHandler = GetTableHandler(grp.Key);
				
				if (tableHandler == null)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("SaveObject: DataObject Type ({0}) not registered !", grp.Key.FullName);
					success = false;
					continue;
				}
				
				var objs = grp.Where(obj => obj.Dirty).ToArray();
				var results = SaveObjectImpl(tableHandler, objs);
				var resultsByObjs = results.Select((result, index) => new { Success = result, DataObject = objs[index] })
					.GroupBy(obj => obj.Success);
				
				foreach (var resultGrp in resultsByObjs)
				{
					if (resultGrp.Key)
					{
						// Save in Precache if tablehandler use it
						if (tableHandler.UsesPreCaching)
						{
							var primary = tableHandler.PrimaryKey;
							if (primary != null)
							{
								foreach (var successObj in resultGrp.Select(obj => obj.DataObject))
									tableHandler.SetPreCachedObject(primary.GetValue(successObj), successObj);
							}
						}
					}
					else
					{
						if (log.IsErrorEnabled)
						{
							foreach(var obj in resultGrp)
								log.ErrorFormat("SaveObject: DataObject ({0}) could not be saved into database...", obj.DataObject);
						}
						success = false;
					}
				}
				
				if (tableHandler.HasRelations)
					success &= SaveObjectRelations(tableHandler, grp);				
			}
			return success;
		}
		#endregion
		#region Public Delete Objects Implementation
		/// <summary>
		/// Delete a DataObject from database if deletion is allowed
		/// </summary>
		/// <param name="dataObject">DataObject to Delete from database</param>
		/// <returns>True if the DataObject was deleted.</returns>
		public bool DeleteObject(DataObject dataObject)
		{
			return DeleteObject(new [] { dataObject });
		}
		
		/// <summary>
		/// Delete DataObjects from database if deletion is allowed
		/// </summary>
		/// <param name="dataObjects">DataObjects to Delete from database</param>
		/// <returns>True if All DataObjects were deleted.</returns>
		public bool DeleteObject(IEnumerable<DataObject> dataObjects)
		{
			var success = true;
			foreach (var grp in dataObjects.GroupBy(obj => obj.GetType()))
			{
				var tableHandler = GetTableHandler(grp.Key);
				
				if (tableHandler == null)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("DeleteObject: DataObject Type ({0}) not registered !", grp.Key.FullName);
					success = false;
					continue;
				}
				
				foreach (var allowed in grp.GroupBy(item => item.AllowDelete))
				{
					if (allowed.Key)
					{
						var objs = allowed.ToArray();
						var results = DeleteObjectImpl(tableHandler, objs);
						
						var resultsByObjs = results.Select((result, index) => new { Success = result, DataObject = objs[index] })
							.GroupBy(obj => obj.Success);
						
						foreach (var resultGrp in resultsByObjs)
						{
							if (resultGrp.Key)
							{
								// Delete in Precache if tablehandler use it
								if (tableHandler.UsesPreCaching)
								{
									var primary = tableHandler.PrimaryKey;
									if (primary != null)
									{
										foreach (var successObj in resultGrp.Select(obj => obj.DataObject))
											tableHandler.DeletePreCachedObject(primary.GetValue(successObj));
									}
								}
								
								// Success Objects Need to check Relations that should be deleted
								if (tableHandler.HasRelations)
									success &= DeleteObjectRelations(tableHandler, resultGrp.Select(obj => obj.DataObject));
							}
							else
							{
								if (log.IsErrorEnabled)
								{
									foreach(var obj in resultGrp)
										log.ErrorFormat("DeleteObject: DataObject ({0}) could not be deleted from database...", obj.DataObject);
								}
								success = false;
							}
						}
					}
					else
					{
						if (log.IsWarnEnabled)
						{
							foreach (var obj in allowed)
								log.WarnFormat("DeleteObject: DataObject ({0}) not allowed to be deleted from Database", obj);
						}
						success = false;
					}
				}
			}
			return success;
		}
		#endregion
		#region Relation Update Handling
		/// <summary>
		/// Save Relations Objects attached to DataObjects
		/// </summary>
		/// <param name="tableHandler">TableHandler for Source DataObjects Relation</param>
		/// <param name="dataObjects">DataObjects to parse</param>
		/// <returns>True if all Relations were saved</returns>
		protected bool SaveObjectRelations(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
		{
			var success = true;
			foreach (var relation in tableHandler.ElementBindings.Where(bind => bind.Relation != null))
			{
				// Relation Check
				var remoteHandler = GetTableHandler(relation.ValueType);
				if (remoteHandler == null)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("SaveObjectRelations: Remote Table for Type ({0}) is not registered !", relation.ValueType.FullName);
					success = false;
					continue;
				}

				// Check For Array Type
				var groups = relation.ValueType.HasElementType
					? dataObjects.Select(obj => new { Source = obj, Enumerable = (IEnumerable<DataObject>)relation.GetValue(obj) })
					.Where(obj => obj.Enumerable != null).Select(obj => obj.Enumerable.Select(rel => new { Local = obj.Source, Remote = rel }))
					.SelectMany(obj => obj).Where(obj => obj.Remote != null).GroupBy(obj => obj.Remote.IsPersisted)
					: dataObjects.Select(obj => new { Local = obj, Remote = (DataObject)relation.GetValue(obj) }).Where(obj => obj.Remote != null).GroupBy(obj => obj.Remote.IsPersisted);
				
				foreach (var grp in groups)
				{
					// Group by object that can be added or saved
					foreach (var allowed in grp.GroupBy(obj => grp.Key ? obj.Remote.Dirty : obj.Remote.AllowAdd))
					{
						if (allowed.Key)
						{
							var objs = allowed.ToArray();
							var results = grp.Key ? SaveObjectImpl(remoteHandler, objs.Select(obj => obj.Remote)) : AddObjectImpl(remoteHandler, objs.Select(obj => obj.Remote));
							
							var resultsByObjs = results.Select((result, index) => new { Success = result, RelObject = objs[index] });
							
							foreach (var resultGrp in resultsByObjs.GroupBy(obj => obj.Success))
							{
								if (resultGrp.Key)
								{
									// Update in Precache if tablehandler use it
									if (remoteHandler.UsesPreCaching)
									{
										var primary = remoteHandler.PrimaryKey;
										if (primary != null)
										{
											foreach (var successObj in resultGrp.Select(obj => obj.RelObject.Remote))
												remoteHandler.SetPreCachedObject(primary.GetValue(successObj), successObj);
										}
									}
								}
								else
								{
									if (log.IsErrorEnabled)
									{
										foreach (var result in resultGrp)
											log.ErrorFormat("SaveObjectRelations: {0} Relation ({1}) of DataObject ({2}) failed for Object ({3})", grp.Key ? "Saving" : "Adding",
											                relation.ValueType, result.RelObject.Local, result.RelObject.Remote);
									}
									success = false;
								}
							}
						}
						else
						{
							// Objects that could not be added can lead to failure
							if (!grp.Key)
							{
								if (log.IsWarnEnabled)
								{
									foreach (var obj in allowed)
										log.WarnFormat("SaveObjectRelations: DataObject ({0}) not allowed to be added to Database", obj);
								}
								success = false;
							}
						}
					}
				}
			}
			return success;
		}
		
		/// <summary>
		/// Delete Relations Objects attached to DataObjects
		/// </summary>
		/// <param name="tableHandler">TableHandler for Source DataObjects Relation</param>
		/// <param name="dataObjects">DataObjects to parse</param>
		/// <returns>True if all Relations were deleted</returns>
		public bool DeleteObjectRelations(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
		{
			var success = true;
			foreach (var relation in tableHandler.ElementBindings.Where(bind => bind.Relation != null && bind.Relation.AutoDelete))
			{
				// Relation Check
				var remoteHandler = GetTableHandler(relation.ValueType);
				if (remoteHandler == null)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("DeleteObjectRelations: Remote Table for Type ({0}) is not registered !", relation.ValueType.FullName);
					success = false;
					continue;
				}

				// Check For Array Type
				var groups = relation.ValueType.HasElementType
					? dataObjects.Select(obj => new { Source = obj, Enumerable = (IEnumerable<DataObject>)relation.GetValue(obj) })
					.Where(obj => obj.Enumerable != null).Select(obj => obj.Enumerable.Select(rel => new { Local = obj.Source, Remote = rel }))
					.SelectMany(obj => obj).Where(obj => obj.Remote != null && obj.Remote.IsPersisted)
					: dataObjects.Select(obj => new { Local = obj, Remote = (DataObject)relation.GetValue(obj) }).Where(obj => obj.Remote != null && obj.Remote.IsPersisted);
				
				foreach (var grp in groups.GroupBy(obj => obj.Remote.AllowDelete))
				{
					if (grp.Key)
					{
						var objs = grp.ToArray();
						var results = DeleteObjectImpl(remoteHandler, objs.Select(obj => obj.Remote));
						
						var resultsByObjs = results.Select((result, index) => new { Success = result, RelObject = objs[index] });
						
						foreach (var resultGrp in resultsByObjs.GroupBy(obj => obj.Success))
						{
							if (resultGrp.Key)
							{
								// Delete in Precache if tablehandler use it
								if (remoteHandler.UsesPreCaching)
								{
									var primary = remoteHandler.PrimaryKey;
									if (primary != null)
									{
										foreach (var successObj in resultGrp.Select(obj => obj.RelObject.Remote))
											remoteHandler.DeletePreCachedObject(primary.GetValue(successObj));
									}
								}
							}
							else
							{
								foreach (var result in resultGrp)
								{
									if (log.IsErrorEnabled)
										log.ErrorFormat("DeleteObjectRelations: Deleting Relation ({0}) of DataObject ({1}) failed for Object ({2})",
										                relation.ValueType, result.RelObject.Local, result.RelObject.Remote);
								}
								success = false;
							}
						}
					}
					else
					{
						// Objects that could not be deleted can lead to failure
						if (log.IsWarnEnabled)
						{
							foreach (var obj in grp)
								log.WarnFormat("DeleteObjectRelations: DataObject ({0}) not allowed to be deleted from Database", obj);
						}
						success = false;
					}
				}
				
			}
			return success;
		}
		#endregion
		#region Relation Select/Fill Handling
		/// <summary>
		/// Populate or Refresh Objects Relations
		/// </summary>
		/// <param name="dataObjects">Objects to Populate</param>
		public void FillObjectRelations(IEnumerable<DataObject> dataObjects)
		{
			// Interface Call, force Refresh
			FillObjectRelations(dataObjects, true);
		}
		
		/// <summary>
		/// Populate or Refresh Object Relations
		/// </summary>
		/// <param name="dataObject">Object to Populate</param>
		public void FillObjectRelations(DataObject dataObject)
		{
			// Interface Call, force Refresh
			FillObjectRelations(new [] { dataObject }, true);
		}
		
		/// <summary>
		/// Populate or Refresh Objects Relations
		/// </summary>
		/// <param name="dataObjects">Objects to Populate</param>
		/// <param name="force">Force Refresh even if Autoload is False</param>
		protected virtual void FillObjectRelations(IEnumerable<DataObject> dataObjects, bool force)
		{
			var groups = dataObjects.GroupBy(obj => obj.GetType());
			
			foreach (var grp in groups)
			{
				var dataType = grp.Key;
				var tableName = AttributesUtils.GetTableOrViewName(dataType);
				try
				{
					
					DataTableHandler tableHandler;
					if (!TableDatasets.TryGetValue(tableName, out tableHandler))
						throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", tableName));
					
					if (!tableHandler.HasRelations)
						return;
					
					var relations = tableHandler.ElementBindings.Where(bind => bind.Relation != null);
					foreach (var relation in relations)
					{
						// Check if Loading is needed
						if (!(relation.Relation.AutoLoad || force))
							continue;
						
						var remoteName = AttributesUtils.GetTableOrViewName(relation.ValueType);						
						try
						{
							DataTableHandler remoteHandler;
							if (!TableDatasets.TryGetValue(remoteName, out remoteHandler))
								throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", remoteName));

							// Select Object On Relation Constraint
							var localBind = tableHandler.FieldElementBindings.Single(bind => bind.ColumnName.Equals(relation.Relation.LocalField, StringComparison.OrdinalIgnoreCase));
							var remoteBind = remoteHandler.FieldElementBindings.Single(bind => bind.ColumnName.Equals(relation.Relation.RemoteField, StringComparison.OrdinalIgnoreCase));
							
							FillObjectRelationsImpl(relation, localBind, remoteBind, remoteHandler, grp);
						}
						catch (Exception re)
						{
							if (log.IsErrorEnabled)
								log.ErrorFormat("Could not Retrieve Objects from Relation (Table {0}, Local {1}, Remote Table {2}, Remote {3})\n{4}", tableName,
								                relation.Relation.LocalField, AttributesUtils.GetTableOrViewName(relation.ValueType), relation.Relation.RemoteField, re);
						}
					}
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.ErrorFormat("Could not Resolve Relations for Table {0}\n{1}", tableName, e);
				}
			}
		}
		
		/// <summary>
		/// Populate or Refresh Object Relation Implementation
		/// </summary>
		/// <param name="relationBind">Element Binding for Relation Field</param>
		/// <param name="localBind">Local Binding for Value Match</param>
		/// <param name="remoteBind">Remote Binding for Column Match</param>
		/// <param name="remoteHandler">Remote Table Handler for Cache Retrieving</param>
		/// <param name="dataObjects">DataObjects to Populate</param>
		protected virtual void FillObjectRelationsImpl(ElementBinding relationBind, ElementBinding localBind, ElementBinding remoteBind, DataTableHandler remoteHandler, IEnumerable<DataObject> dataObjects)
		{
			var type = relationBind.ValueType;
			var isElementType = false;
			if (type.HasElementType)
			{
				type = type.GetElementType();
				isElementType = true;
			}
			
			var objects = dataObjects.ToArray();
			IEnumerable<IEnumerable<DataObject>> objsResults = null;
			
			// Handle Cache Search if relevent or use a Select Query
			if (remoteHandler.UsesPreCaching)
			{
				// Search with Primary Key or use a Where Clause
				if (remoteHandler.PrimaryKeys.All(pk => pk.ColumnName.Equals(remoteBind.ColumnName, StringComparison.OrdinalIgnoreCase)))
				{
					objsResults = objects.Select(obj => {
					                             	var local = localBind.GetValue(obj);
					                             	if (local == null)
					                             		return Array.Empty<DataObject>();
					                             	
					                             	var retrieve = remoteHandler.GetPreCachedObject(local);
					                             	if (retrieve == null)
					                             		return Array.Empty<DataObject>();
					                             	
					                             	return new [] { retrieve };
					                             });
				}
				else
				{
					objsResults = objects
						.Select(obj => remoteHandler.SearchPreCachedObjects(rem => {
						                                                    	var local = localBind.GetValue(obj);
						                                                    	var remote = remoteBind.GetValue(rem);
						                                                    	if (local == null || remote == null)
						                                                    		return false;
						                                                    	
						                                                    	if (localBind.ValueType == typeof(string) || remoteBind.ValueType == typeof(string))
						                                                    		return remote.ToString().Equals(local.ToString(), StringComparison.OrdinalIgnoreCase);
						                                                    	
						                                                    	return remote == local;
						                                                    }));
				}
			}
			else
			{
				var whereClauses = objects.Select(obj => DB.Column(remoteBind.ColumnName).IsEqualTo(localBind.GetValue(obj)));
				objsResults = MultipleSelectObjectsImpl(remoteHandler, whereClauses);
			}
			
			var resultByObjs = objsResults.Select((obj, index) => new { DataObject = objects[index], Results = obj }).ToArray();
			
			// Store Relations
			foreach (var result in resultByObjs)
			{
				if (isElementType)
				{
					if (result.Results.Any())
					{
						MethodInfo castMethod = typeof(Enumerable).GetMethod("OfType").MakeGenericMethod(type);
						MethodInfo methodToArray = typeof(Enumerable).GetMethod("ToArray").MakeGenericMethod(type);
						relationBind.SetValue(result.DataObject, methodToArray.Invoke(null, new object[] { castMethod.Invoke(null, new object[] { result.Results }) }));
					}
					else
					{
						relationBind.SetValue(result.DataObject, null);
					}
				}
				else
				{
					relationBind.SetValue(result.DataObject, result.Results.SingleOrDefault());
				}
			}
			
			// Fill Sub Relations
			FillObjectRelations(resultByObjs.SelectMany(result => result.Results), false);
		}
		#endregion
		#region Public Object Select with Key API
		/// <summary>
		/// Retrieve a DataObject from database based on its primary key value. 
		/// </summary>
		/// <param name="key">Primary Key Value</param>
		/// <returns>Object found or null if not found</returns>
		public TObject FindObjectByKey<TObject>(object key)
			where TObject : DataObject
		{
			return FindObjectsByKey<TObject>(new [] { key }).FirstOrDefault();
		}
		
		/// <summary>
		/// Retrieve a Collection of DataObjects from database based on their primary key values
		/// </summary>
		/// <param name="keys">Collection of Primary Key Values</param>
		/// <returns>Collection of DataObject with primary key matching values</returns>
		public virtual IList<TObject> FindObjectsByKey<TObject>(IEnumerable<object> keys)
			where TObject : DataObject
		{
			var tableHandler = GetTableOrViewHandler(typeof(TObject));
			if (tableHandler == null)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("FindObjectByKey: DataObject Type ({0}) not registered !", typeof(TObject).FullName);
				
				throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", typeof(TObject).FullName));
			}
			
			if (tableHandler.UsesPreCaching)
				return keys.Select(key => tableHandler.GetPreCachedObject(key)).Cast<TObject>().ToArray();
			
			var objs = FindObjectByKeyImpl(tableHandler, keys).Cast<TObject>().ToArray();
			
			FillObjectRelations(objs.Where(obj => obj != null), false);
			
			return objs;
		}
		
		/// <summary>
		/// Retrieve a Collection of DataObjects from database based on their primary key values
		/// </summary>
		/// <param name="tableHandler">Table Handler for the DataObjects to Retrieve</param>
		/// <param name="keys">Collection of Primary Key Values</param>
		/// <returns>Collection of DataObject with primary key matching values</returns>
		protected abstract IEnumerable<DataObject> FindObjectByKeyImpl(DataTableHandler tableHandler, IEnumerable<object> keys);
		#endregion

		#region Public Parameterized Query Abstraction
		public TObject SelectObject<TObject>(WhereClause whereClause)
			where TObject : DataObject
		{
			return SelectObjects<TObject>(whereClause).FirstOrDefault();
		}

		public IList<TObject> SelectObjects<TObject>(WhereClause whereClause)
			where TObject : DataObject
		{
			return MultipleSelectObjects<TObject>(new[] { whereClause }).First();
		}

		public IList<IList<TObject>> MultipleSelectObjects<TObject>(IEnumerable<WhereClause> whereClauseBatch)
			where TObject : DataObject
		{
			if (whereClauseBatch == null) throw new ArgumentNullException("Parameter whereClauseBatch may not be null.");

			var tableHandler = GetTableOrViewHandler(typeof(TObject));
			if (tableHandler == null)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("SelectObjects: DataObject Type ({0}) not registered !", typeof(TObject).FullName);

				throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", typeof(TObject).FullName));
			}

			var objs = MultipleSelectObjectsImpl(tableHandler, whereClauseBatch).Select(res => res.OfType<TObject>().ToArray()).ToArray();

			FillObjectRelations(objs.SelectMany(obj => obj), false);

			return objs;
		}
		#endregion
		
		#region Public Object Select All API
		public IList<TObject> SelectAllObjects<TObject>()
			where TObject : DataObject
		{
			var tableHandler = GetTableOrViewHandler(typeof(TObject));
			if (tableHandler == null)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("SelectAllObjects: DataObject Type ({0}) not registered !", typeof(TObject).FullName);

				throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", typeof(TObject).FullName));
			}

			if (tableHandler.UsesPreCaching)
				return tableHandler.SearchPreCachedObjects(obj => obj != null).OfType<TObject>().ToArray();

			var dataObjects = MultipleSelectObjectsImpl(tableHandler, new[] { WhereClause.Empty }).Single().OfType<TObject>().ToArray();

			FillObjectRelations(dataObjects, false);

			return dataObjects;
		}
		#endregion
		
		#region Public API
		/// <summary>
		/// Gets the number of objects in a given table in the database.
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <returns>a positive integer representing the number of objects; zero if no object exists</returns>
		public int GetObjectCount<TObject>()
			where TObject : DataObject
		{
			return GetObjectCount<TObject>(string.Empty);
		}

		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="whereExpression">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		public int GetObjectCount<TObject>(string whereExpression)
			where TObject : DataObject
		{
			return GetObjectCountImpl<TObject>(whereExpression);
		}

		/// <summary>
		/// Register Data Object Type if not already Registered
		/// </summary>
		/// <param name="dataObjectType">DataObject Type</param>
		public virtual void RegisterDataObject(Type dataObjectType)
		{
			var tableName = AttributesUtils.GetTableOrViewName(dataObjectType);
			if (TableDatasets.ContainsKey(tableName))
				return;
			
			var dataTableHandler = new DataTableHandler(dataObjectType);
			TableDatasets.Add(tableName, dataTableHandler);
		}

		/// <summary>
		/// escape the strange character from string
		/// </summary>
		/// <param name="rawInput">the string</param>
		/// <returns>the string with escaped character</returns>
		public abstract string Escape(string rawInput);
		
		/// <summary>
		/// Execute a Raw Non-Query on the Database
		/// </summary>
		/// <param name="rawQuery">Raw Command</param>
		/// <returns>True if the Command succeeded</returns>
		public virtual bool ExecuteNonQuery(string rawQuery)
		{
			throw new NotImplementedException();
		}

		#endregion

		#region Implementation
		/// <summary>
		/// Adds new DataObjects to the database.
		/// </summary>
		/// <param name="dataObjects">DataObjects to add to the database</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were added successfully; false otherwise</returns>
		protected abstract IEnumerable<bool> AddObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

		/// <summary>
		/// Saves Persisted DataObjects into Database
		/// </summary>
		/// <param name="dataObjects">DataObjects to Save</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were saved successfully; false otherwise</returns>
		protected abstract IEnumerable<bool> SaveObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

		/// <summary>
		/// Deletes DataObjects from the database.
		/// </summary>
		/// <param name="dataObjects">DataObjects to delete from the database</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were deleted successfully; false otherwise</returns>
		protected abstract IEnumerable<bool> DeleteObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects);

		/// <summary>
		/// Retrieve a Collection of DataObjects Sets from database filtered by Parametrized Where Expression
		/// </summary>
		/// <param name="tableHandler">Table Handler for these DataObjects</param>
		/// <param name="whereExpression">Parametrized Where Expression</param>
		/// <param name="parameters">Parameters for filtering</param>
		/// <param name="isolation">Isolation Level</param>
		/// <returns>Collection of DataObjects Sets matching Parametrized Where Expression</returns>
		protected abstract IList<IList<DataObject>> SelectObjectsImpl(DataTableHandler tableHandler, string whereExpression, IEnumerable<IEnumerable<QueryParameter>> parameters, Transaction.IsolationLevel isolation);

		protected abstract IList<IList<DataObject>> MultipleSelectObjectsImpl(DataTableHandler tableHandler, IEnumerable<WhereClause> whereClauseBatch);

		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="whereExpression">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		protected abstract int GetObjectCountImpl<TObject>(string whereExpression)
			where TObject : DataObject;
		#endregion

		#region Cache
		/// <summary>
		/// Selects object from the database and updates or adds entry in the pre-cache.
		/// </summary>
		/// <typeparam name="TObject">DataObject Type to Query</typeparam>
		/// <param name="key">Key to Update</param>
		/// <returns>True if Object was found with given key</returns>
		public bool UpdateInCache<TObject>(object key)
			where TObject : DataObject
		{
			return UpdateObjsInCache<TObject>(new [] { key });
		}
		
		/// <summary>
		/// Selects objects from the database and updates or adds entries in the pre-cache.
		/// </summary>
		/// <typeparam name="TObject">DataObject Type to Query</typeparam>
		/// <param name="keys">Key Collection to Update</param>
		/// <returns>True if All Objects were found with given keys</returns>
		public bool UpdateObjsInCache<TObject>(IEnumerable<object> keys)
			where TObject : DataObject
		{
			var tableHandler = GetTableOrViewHandler(typeof(TObject));
			if (tableHandler == null)
			{
				if (log.IsErrorEnabled)
					log.ErrorFormat("UpdateInCache: DataObject Type ({0}) not registered !", typeof(TObject).FullName);
				
				throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", typeof(TObject).FullName));
			}
			
			var keysArray = keys.ToArray();
			var objs = FindObjectByKeyImpl(tableHandler, keysArray);
			var objsByKey = objs.Select((obj, i) => new { Key = keysArray[i], DataObject = obj });
			
			var success = true;
			foreach (var obj in objsByKey)
			{
				if (obj.DataObject != null)
					tableHandler.SetPreCachedObject(obj.Key, obj.DataObject);
				else
					success = false;
			}
			
			return success;
		}

		#endregion

		#region Factory

		public static IObjectDatabase GetObjectDatabase(ConnectionType connectionType, string connectionString)
		{
			if (connectionType == ConnectionType.DATABASE_MYSQL)
				return new MySQLObjectDatabase(connectionString);
			if (connectionType == ConnectionType.DATABASE_SQLITE)
				return new SQLiteObjectDatabase(connectionString);

			return null;
		}

		#endregion
	}
}
