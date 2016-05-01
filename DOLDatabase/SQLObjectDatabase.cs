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
using System.Data;
using System.Data.Common;
using System.Reflection;

using DOL.Database.Connection;
using DOL.Database.Attributes;
using DOL.Database.UniqueID;

namespace DOL.Database
{
	/// <summary>
	/// Abstract Base Class for SQL based Database Connector
	/// </summary>
	public abstract class SQLObjectDatabase : ObjectDatabase 
	{
		/// <summary>
		/// Create a new instance of <see cref="SQLObjectDatabase"/>
		/// </summary>
		/// <param name="ConnectionString">Database Connection String</param>
		protected SQLObjectDatabase(string ConnectionString)
			: base(ConnectionString)
		{
			
		}
		
		#region ObjectDatabase Base Implementation for SQL
		/// <summary>
		/// Register Data Object Type if not already Registered
		/// </summary>
		/// <param name="dataObjectType">DataObject Type</param>
		public override void RegisterDataObject(Type dataObjectType)
		{
			var tableName = AttributesUtils.GetTableOrViewName(dataObjectType);
			var isView = AttributesUtils.GetViewName(dataObjectType) != null;
			var viewAs = AttributesUtils.GetViewAs(dataObjectType);
			
			if (TableDatasets.ContainsKey(tableName))
				return;
			
			var dataTableHandler = new DataTableHandler(dataObjectType);

			try
			{
				if (isView && !string.IsNullOrEmpty(viewAs))
				{
					ExecuteNonQueryImpl(string.Format("DROP VIEW IF EXISTS `{0}`", tableName));
					ExecuteNonQueryImpl(string.Format("CREATE VIEW `{0}` AS {1}", tableName, string.Format(viewAs, string.Format("`{0}`", AttributesUtils.GetTableName(dataObjectType)))));
				}
				else
				{
					CheckOrCreateTableImpl(dataTableHandler);
				}
				
				TableDatasets.Add(tableName, dataTableHandler);
				if (dataTableHandler.UsesPreCaching)
					SelectObjectsImpl(dataTableHandler, "", new [] { new KeyValuePair<string, object>[] { } }, Transaction.IsolationLevel.DEFAULT);
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("RegisterDataObject: Error While Registering Table \"{0}\"\n{1}", tableName, e);
			}
		}
		
		/// <summary>
		/// Gets the format for date times
		/// </summary>
		/// <returns></returns>
		public virtual string GetDBDateFormat()
		{
			return "yyyy-MM-dd HH:mm:ss";
		}
		
		/// <summary>
		/// Escape wrong characters from string for Database Insertion
		/// </summary>
		/// <param name="rawInput">String to Escape</param>
		/// <returns>Escaped String</returns>
		public override string Escape(string rawInput)
		{
			rawInput = rawInput.Replace("\\", "\\\\");
			rawInput = rawInput.Replace("\"", "\\\"");
			rawInput = rawInput.Replace("'", "\\'");
			return rawInput.Replace("’", "\\’");
		}
		#endregion
		
		#region ObjectDatabase Objects Implementations
		/// <summary>
		/// Adds new DataObjects to the database.
		/// </summary>
		/// <param name="dataObjects">DataObjects to add to the database</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were added successfully; false otherwise</returns>
		protected override IEnumerable<bool> AddObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
		{
			var success = new List<bool>();
			if (!dataObjects.Any())
				return success;
			
			try
			{
				// Check Primary Keys
				var usePrimaryAutoInc = tableHandler.FieldElementBindings.Any(bind => bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement);
				
				// Columns
				var columns = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey == null || !bind.PrimaryKey.AutoIncrement)
					.Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToArray();
				
				// Prepare SQL Query
				var command = string.Format("INSERT INTO `{0}` ({1}) VALUES({2})", tableHandler.TableName,
				                            string.Join(", ", columns.Select(col => col.ColumnName)),
				                            string.Join(", ", columns.Select(col => col.ParamName)));
				
				var objs = dataObjects.ToArray();
				// Init Object Id GUID
				foreach (var obj in objs.Where(obj => obj.ObjectId == null))
					obj.ObjectId = IDGenerator.GenerateID();
				// Build Parameters
				var parameters = objs.Select(obj => columns.Select(col => new KeyValuePair<string, object>(col.ParamName, col.Binding.GetValue(obj))));
				
				// Primary Key Auto Inc Handler
				if (usePrimaryAutoInc)
				{
					var lastId = ExecuteScalarImpl(command, parameters, true);
					
					var binding = tableHandler.FieldElementBindings.First(bind => bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement);
					var resultByObjects = lastId.Select((result, index) => new { Result = Convert.ToInt64(result), DataObject = objs[index] });
					
					foreach (var result in resultByObjects)
					{
						if (result.Result > 0)
						{
							DatabaseSetValue(result.DataObject, binding, result.Result);
							result.DataObject.ObjectId = result.Result.ToString();
							result.DataObject.Dirty = false;
							result.DataObject.IsPersisted = true;
							result.DataObject.IsDeleted = false;
							if (tableHandler.UsesPreCaching)
								tableHandler.SetPreCachedObject(result.Result, result.DataObject);
							success.Add(true);
						}
						else
						{
							if (Log.IsErrorEnabled)
								Log.ErrorFormat("Error adding data object into {0} Object = {1}, UsePrimaryAutoInc, Query = {2}", tableHandler.TableName, result.DataObject, command);
							
							success.Add(false);
						}
					}

				}
				else
				{
					var affected = ExecuteNonQueryImpl(command, parameters);
					var resultByObjects = affected.Select((result, index) => new { Result = result, DataObject = objs[index] });
					
					foreach (var result in resultByObjects)
					{
						if (result.Result > 0)
						{
							result.DataObject.Dirty = false;
							result.DataObject.IsPersisted = true;
							result.DataObject.IsDeleted = false;
							if (tableHandler.UsesPreCaching)
								tableHandler.SetPreCachedObject(result.DataObject.ObjectId, result.DataObject);
							success.Add(true);
						}
						else
						{
							if (Log.IsErrorEnabled)
								Log.ErrorFormat("Error adding data object into {0} Object = {1} Query = {2}", tableHandler.TableName, result.DataObject, command);
							success.Add(false);
						}
					}
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("Error while adding data objects in table: {0}\n{1}", tableHandler.TableName, e);
			}

			return success;
		}

		/// <summary>
		/// Saves Persisted DataObjects into Database
		/// </summary>
		/// <param name="dataObjects">DataObjects to Save</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were saved successfully; false otherwise</returns>
		protected override IEnumerable<bool> SaveObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
		{
			var success = new List<bool>();
			if (!dataObjects.Any())
				return success;
			
			try
			{
				// Columns Filtering out ReadOnly
				var columns = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey == null && bind.ReadOnly == null)
					.Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToArray();
				// Primary Key
				var primary = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey != null)
					.Select(bind => new { Binding = bind, ColumnName = string.Format("`{0}`", bind.ColumnName), ParamName = string.Format("@{0}", bind.ColumnName) }).ToArray();
				
				if (!primary.Any())
					throw new DatabaseException(string.Format("Table {0} has no primary key for saving...", tableHandler.TableName));
				
				var command = string.Format("UPDATE `{0}` SET {1} WHERE {2}", tableHandler.TableName,
				                            string.Join(", ", columns.Select(col => string.Format("{0} = {1}", col.ColumnName, col.ParamName))),
				                            string.Join(" AND ", primary.Select(col => string.Format("{0} = {1}", col.ColumnName, col.ParamName))));
				
				var objs = dataObjects.ToArray();
				var parameters = objs.Select(obj => columns.Concat(primary).Select(col => new KeyValuePair<string, object>(col.ParamName, col.Binding.GetValue(obj))));
				
				var affected = ExecuteNonQueryImpl(command, parameters);
				var resultByObjects = affected.Select((result, index) => new { Result = result, DataObject = objs[index] });
				
				foreach (var result in resultByObjects)
				{
					if (result.Result > 0)
					{
						result.DataObject.Dirty = false;
						result.DataObject.IsPersisted = true;
						if (tableHandler.UsesPreCaching)
						{
							var autoInc = primary.FirstOrDefault(col => col.Binding.PrimaryKey.AutoIncrement);
							tableHandler.SetPreCachedObject(autoInc == null ? result.DataObject.ObjectId : autoInc.Binding.GetValue(result.DataObject), result.DataObject);
						}
						success.Add(true);
					}
					else
					{
						if (Log.IsErrorEnabled)
							Log.ErrorFormat("Error saving data object in table {0} Object = {1} --- keyvalue changed? {2}\n{3}", tableHandler.TableName, result.DataObject, command, Environment.StackTrace);
						success.Add(false);
					}
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("Error while saving data object in table: {0}\n{1}", tableHandler.TableName, e);
			}

			return success;
		}

		/// <summary>
		/// Deletes DataObjects from the database.
		/// </summary>
		/// <param name="dataObjects">DataObjects to delete from the database</param>
		/// <param name="tableHandler">Table Handler for the DataObjects Collection</param>
		/// <returns>True if objects were deleted successfully; false otherwise</returns>
		protected override IEnumerable<bool> DeleteObjectImpl(DataTableHandler tableHandler, IEnumerable<DataObject> dataObjects)
		{
			var success = new List<bool>();
			if (!dataObjects.Any())
				return success;
			
			try
			{
				// Get Primary Keys
				var primary = tableHandler.FieldElementBindings.Where(bind => bind.PrimaryKey != null).ToArray();
				
				if (!primary.Any())
					throw new DatabaseException(string.Format("Table {0} has no primary key for deletion...", tableHandler.TableName));
				
				var command = string.Format("DELETE FROM `{0}` WHERE {1}", tableHandler.TableName,
	                        string.Join(" AND ", primary.Select(col => string.Format("`{0}` = @{0}", col.ColumnName))));
				
				var objs = dataObjects.ToArray();
				var parameters = objs.Select(obj => primary.Select(pk => new KeyValuePair<string, object>(string.Format("@{0}", pk.ColumnName), pk.GetValue(obj))));
				
				var affected = ExecuteNonQueryImpl(command, parameters);
				var resultByObjects = affected.Select((result, index) => new { Result = result, DataObject = objs[index] });
				
				foreach (var result in resultByObjects)
				{
					if (result.Result > 0)
					{
						result.DataObject.IsPersisted = false;
						result.DataObject.IsDeleted = true;
						if (tableHandler.UsesPreCaching)
						{
							var autoInc = primary.FirstOrDefault(col => col.PrimaryKey.AutoIncrement);
							tableHandler.DeletePreCachedObject(autoInc == null ? result.DataObject.ObjectId : autoInc.GetValue(result.DataObject));
						}
						success.Add(true);
					}
					else
					{
						if (Log.IsErrorEnabled)
							Log.ErrorFormat("Error deleting data object from table {0} Object = {1} --- keyvalue changed? {2}\n{3}", tableHandler.TableName, result.DataObject, command, Environment.StackTrace);
						success.Add(false);
					}
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("Error while deleting data object in table: {0}\n{1}", tableHandler.TableName, e);
			}
			
			return success;
		}
		#endregion
		
		#region ObjectDatabase Select Implementation
		/// <summary>
		/// Gets the number of objects in a given table in the database based on a given set of criteria. (where clause)
		/// </summary>
		/// <typeparam name="TObject">the type of objects to retrieve</typeparam>
		/// <param name="whereExpression">the where clause to filter object count on</param>
		/// <returns>a positive integer representing the number of objects that matched the given criteria; zero if no such objects existed</returns>
		protected override int GetObjectCountImpl<TObject>(string whereExpression)
		{
			string tableName = AttributesUtils.GetTableOrViewName(typeof(TObject));
			DataTableHandler tableHandler;
			if (!TableDatasets.TryGetValue(tableName, out tableHandler))
				throw new DatabaseException(string.Format("Table {0} is not registered for Database Connection...", tableName));
			
			string command = null;
			if (string.IsNullOrEmpty(whereExpression))
				command = string.Format("SELECT COUNT(*) FROM `{0}`", tableName);
			else
				command = string.Format("SELECT COUNT(*) FROM `{0}` WHERE {1}", tableName, whereExpression);
			
			var count = ExecuteScalarImpl(command);
			
			return count is long ? (int)((long)count) : (int)count;
		}
		
		/// <summary>
		/// Retrieve a Collection of DataObjects Sets from database filtered by Parametrized Where Expression
		/// </summary>
		/// <param name="tableHandler">Table Handler for these DataObjects</param>
		/// <param name="whereExpression">Parametrized Where Expression</param>
		/// <param name="parameters">Parameters for filtering</param>
		/// <param name="isolation">Isolation Level</param>
		/// <returns>Collection of DataObjects Sets matching Parametrized Where Expression</returns>
		protected override IEnumerable<IEnumerable<DataObject>> SelectObjectsImpl(DataTableHandler tableHandler, string whereExpression, IEnumerable<IEnumerable<KeyValuePair<string, object>>> parameters, Transaction.IsolationLevel isolation)
		{
			var columns = tableHandler.FieldElementBindings.ToArray();
			
			string command = null;
			if (!string.IsNullOrEmpty(whereExpression))
				command = string.Format("SELECT {0} FROM `{1}` WHERE {2}",
				                        string.Join(", ", columns.Select(col => string.Format("`{0}`", col.ColumnName))),
				                        tableHandler.TableName,
				                        whereExpression);
			else
				command = string.Format("SELECT {0} FROM `{1}`",
				                        string.Join(", ", columns.Select(col => string.Format("`{0}`", col.ColumnName))),
				                        tableHandler.TableName);
			
			var dataObjects = new List<List<DataObject>>();
			ExecuteSelectImpl(command, parameters, reader => {
			                  	var list = new List<DataObject>();
			                  	dataObjects.Add(list);
			                  	var data = new object[reader.FieldCount];
			                  	while(reader.Read())
			                  	{
			                  		reader.GetValues(data);
			                  		var obj = Activator.CreateInstance(tableHandler.ObjectType) as DataObject;
			                  		
			                  		// Fill Object
			                  		var current = 0;
			                  		foreach(var column in columns)
			                  		{
			                  			DatabaseSetValue(obj, column, data[current]);
			                  			current++;
			                  		}
			                  		
			                  		// Set Primary Key
			                  		var primary = columns.FirstOrDefault(col => col.PrimaryKey != null);
			                  		if (primary != null)
			                  			obj.ObjectId = primary.GetValue(obj).ToString();
			                  		
									list.Add(obj);
									obj.Dirty = false;
									obj.IsPersisted = true;
									if (tableHandler.UsesPreCaching && primary != null)
										tableHandler.SetPreCachedObject(primary.GetValue(obj), obj);
			                  	}
			                  }, isolation);
			
			return dataObjects.ToArray();
		}
		
		/// <summary>
		/// Set Value to DataObject Field according to ElementBinding
		/// </summary>
		/// <param name="obj">DataObject to Fill</param>
		/// <param name="bind">ElementBinding for the targeted Member</param>
		/// <param name="value">Object Value to Fill</param>
		protected virtual void DatabaseSetValue(DataObject obj, ElementBinding bind, object value)
		{
			if (value == null || value.GetType().IsInstanceOfType(DBNull.Value))
				return;
			
			try
			{
				if (bind.ValueType == typeof(bool))
					bind.SetValue(obj, Convert.ToBoolean(value));
				else if (bind.ValueType == typeof(char))
					bind.SetValue(obj, Convert.ToChar(value));
				else if (bind.ValueType == typeof(sbyte))
					bind.SetValue(obj, Convert.ToSByte(value));
				else if (bind.ValueType == typeof(short))
					bind.SetValue(obj, Convert.ToInt16(value));
				else if (bind.ValueType == typeof(int))
					bind.SetValue(obj, Convert.ToInt32(value));
				else if (bind.ValueType == typeof(long))
					bind.SetValue(obj, Convert.ToInt64(value));
				else if (bind.ValueType == typeof(byte))
					bind.SetValue(obj, Convert.ToByte(value));
				else if (bind.ValueType == typeof(ushort))
					bind.SetValue(obj, Convert.ToUInt16(value));
				else if (bind.ValueType == typeof(uint))
					bind.SetValue(obj, Convert.ToUInt32(value));
				else if (bind.ValueType == typeof(ulong))
					bind.SetValue(obj, Convert.ToUInt64(value));
				else if (bind.ValueType == typeof(DateTime))
					bind.SetValue(obj, Convert.ToDateTime(value));
				else if (bind.ValueType == typeof(float))
					bind.SetValue(obj, Convert.ToSingle(value));
				else if (bind.ValueType == typeof(double))
					bind.SetValue(obj, Convert.ToDouble(value));
				else if (bind.ValueType == typeof(string))
					bind.SetValue(obj, Convert.ToString(value));
				else
					bind.SetValue(obj, value);
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("{0}: {1} = {2} doesnt fit to {3}\n{4}", obj.TableName, bind.ColumnName, value.GetType().FullName, bind.ValueType, e);
			}
		}
		
		/// <summary>
		/// Fill SQL Command Parameter with Converted Values.
		/// </summary>
		/// <param name="parameter">Parameter collection for this Command</param>
		/// <param name="dbParams">DbParameter Object to Fill</param>
		protected virtual void FillSQLParameter(IEnumerable<KeyValuePair<string, object>> parameter, DbParameterCollection dbParams)
		{
			// Specififc Handling for Char Cast from DB Integer
    		foreach(var param in parameter)
    		{
    			if (param.Value is char)
    				dbParams[param.Key].Value = Convert.ToUInt16(param.Value);
    			else
    				dbParams[param.Key].Value = param.Value;
    		}
		}
		#endregion
		
		#region Abstract Properties		
		/// <summary>
		/// The connection type to DB (xml, mysql,...)
		/// </summary>
		public abstract ConnectionType ConnectionType {	get; }
		#endregion

		#region Table Implementation
		/// <summary>
		/// Check for Table Existence, Create or Alter accordingly
		/// </summary>
		/// <param name="table">Table Handler</param>
		public abstract void CheckOrCreateTableImpl(DataTableHandler table);
		#endregion
		
		#region Select Implementation
		/// <summary>
		/// Raw SQL Select Implementation
		/// </summary>
		/// <param name="SQLCommand">Command for reading</param>
		/// <param name="Reader">Reader Method</param>
		/// <param name="Isolation">Transaction Isolation</param>
		protected void ExecuteSelectImpl(string SQLCommand, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
		{
			ExecuteSelectImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { } }, Reader, Isolation);
		}

		/// <summary>
		/// Raw SQL Select Implementation with Single Parameter for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Command for reading</param>
		/// <param name="param">Parameter for Single Read</param>
		/// <param name="Reader">Reader Method</param>
		/// <param name="Isolation">Transaction Isolation</param>
		protected void ExecuteSelectImpl(string SQLCommand, KeyValuePair<string, object> param, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
		{
			ExecuteSelectImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { param } }, Reader, Isolation);
		}
		
		/// <summary>
		/// Raw SQL Select Implementation with Parameters for Single Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Command for reading</param>
		/// <param name="parameter">Collection of Parameters for Single Read</param>
		/// <param name="Reader">Reader Method</param>
		/// <param name="Isolation">Transaction Isolation</param>
		protected void ExecuteSelectImpl(string SQLCommand, IEnumerable<KeyValuePair<string, object>> parameter, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation)
		{
			ExecuteSelectImpl(SQLCommand, new [] { parameter }, Reader, Isolation);
		}
		
		/// <summary>
		/// Raw SQL Select Implementation with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Command for reading</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
		/// <param name="Reader">Reader Method</param>
		/// <param name="Isolation">Transaction Isolation</param>
		protected abstract void ExecuteSelectImpl(string SQLCommand, IEnumerable<IEnumerable<KeyValuePair<string, object>>> parameters, Action<IDataReader> Reader, Transaction.IsolationLevel Isolation);
		#endregion
		
		#region Non Query Implementation
		/// <summary>
		/// Execute a Raw Non-Query on the Database
		/// </summary>
		/// <param name="rawQuery">Raw Command</param>
		/// <returns>True if the Command succeeded</returns>
		public override bool ExecuteNonQuery(string rawQuery)
		{
			try
			{
				return ExecuteNonQueryImpl(rawQuery) > 1;
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("Error while executing raw query \"{0}\"\n{1}", rawQuery, e);
			}
			
			return false;
		}
		
		/// <summary>
		/// Implementation of Raw Non-Query
		/// </summary>
		/// <param name="SQLCommand">Raw Command</param>
		protected int ExecuteNonQueryImpl(string SQLCommand)
		{
			return ExecuteNonQueryImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { }}).First();
		}
		
		/// <summary>
		/// Raw Non-Query Implementation with Single Parameter for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Raw Command</param>
		/// <param name="param">Parameter for Single Command</param>
		protected int ExecuteNonQueryImpl(string SQLCommand, KeyValuePair<string, object> param)
		{
			return ExecuteNonQueryImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { param }}).First();
		}
		
		/// <summary>
		/// Raw Non-Query Implementation with Parameters for Single Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Raw Command</param>
		/// <param name="parameter">Collection of Parameters for Single Command</param>
		protected int ExecuteNonQueryImpl(string SQLCommand, IEnumerable<KeyValuePair<string, object>> parameter)
		{
			return ExecuteNonQueryImpl(SQLCommand, new [] { parameter }).First();
		}
		
		/// <summary>
		/// Implementation of Raw Non-Query with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Raw Command</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Command</param>
		/// <returns>True foreach Command that succeeded</returns>
		protected abstract IEnumerable<int> ExecuteNonQueryImpl(string SQLCommand, IEnumerable<IEnumerable<KeyValuePair<string, object>>> parameters);
		#endregion
		
		#region Scalar Implementation
		/// <summary>
		/// Implementation of Scalar Query
		/// </summary>
		/// <param name="SQLCommand">Scalar Command</param>
		/// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
		/// <returns>Object Returned by Scalar</returns>
		protected object ExecuteScalarImpl(string SQLCommand, bool retrieveLastInsertID = false)
		{
			return ExecuteScalarImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { }}, retrieveLastInsertID).First();
		}
		
		/// <summary>
		/// Implementation of Scalar Query with Single Parameter for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Scalar Command</param>
		/// <param name="param">Parameter for Single Command</param>
		/// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
		/// <returns>Object Returned by Scalar</returns>
		protected object ExecuteScalarImpl(string SQLCommand, KeyValuePair<string, object> param, bool retrieveLastInsertID = false)
		{
			return ExecuteScalarImpl(SQLCommand, new [] { new KeyValuePair<string, object>[] { param }}, retrieveLastInsertID).First();
		}
		
		/// <summary>
		/// Implementation of Scalar Query with Parameters for Single Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Scalar Command</param>
		/// <param name="parameter">Collection of Parameters for Single Command</param>
		/// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
		/// <returns>Object Returned by Scalar</returns>
		protected object ExecuteScalarImpl(string SQLCommand, IEnumerable<KeyValuePair<string, object>> parameter, bool retrieveLastInsertID = false)
		{
			return ExecuteScalarImpl(SQLCommand, new [] { parameter }, retrieveLastInsertID).First();
		}
		
		/// <summary>
		/// Implementation of Scalar Query with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Scalar Command</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
		/// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
		/// <returns>Objects Returned by Scalar</returns>
		protected abstract object[] ExecuteScalarImpl(string SQLCommand, IEnumerable<IEnumerable<KeyValuePair<string, object>>> parameters, bool retrieveLastInsertID);
		#endregion
		
		#region Specific
		protected virtual bool HandleException(Exception e)
		{
			bool ret = false;
			var socketException = e.InnerException == null
				? null
				: e.InnerException.InnerException as System.Net.Sockets.SocketException;
			
			if (socketException == null)
				socketException = e.InnerException as System.Net.Sockets.SocketException;

			if (socketException != null)
			{
				// Handle socket exception. Error codes:
				// http://msdn2.microsoft.com/en-us/library/ms740668.aspx
				// 10052 = Network dropped connection on reset.
				// 10053 = Software caused connection abort.
				// 10054 = Connection reset by peer.
				// 10057 = Socket is not connected.
				// 10058 = Cannot send after socket shutdown.
				switch (socketException.ErrorCode)
				{
					case 10052:
					case 10053:
					case 10054:
					case 10057:
					case 10058:
						ret = true;
						break;
				}

				if (Log.IsWarnEnabled)
					Log.WarnFormat("Socket exception: ({0}) {1}; repeat: {2}", socketException.ErrorCode, socketException.Message, ret);
			}

			return ret;
		}
		#endregion
	}
}
