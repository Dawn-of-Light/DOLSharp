﻿/*
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
using System.Reflection;
using System.Data;
using System.Data.Common;
using DataTable = System.Data.DataTable;

using DOL.Database.Connection;
using DOL.Database.Transaction;
using IsolationLevel = DOL.Database.Transaction.IsolationLevel;

using System.Data.SQLite;

using log4net;

namespace DOL.Database.Handlers
{
	public class SQLiteObjectDatabase : SQLObjectDatabase
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Create a new instance of <see cref="SQLiteObjectDatabase"/>
		/// </summary>
		/// <param name="ConnectionString">Database Connection String</param>
		public SQLiteObjectDatabase(string ConnectionString)
			: base(ConnectionString)
		{
		}
		
		#region SQLite Implementation
		/// <summary>
		/// Convert a Table ElementBinding to Database Type string (Upper)
		/// </summary>
		/// <param name="bind">ElementBindind to Convert</param>
		/// <param name="table">DataTableHandler for Special cases</param>
		/// <returns>Database Type string ToUpper</returns>
		protected virtual string GetDatabaseType(ElementBinding bind, DataTableHandler table)
		{
			string type = null;
			// Check Value Type
			if (bind.ValueType == typeof(char))
			{
				type = "UNSIGNED SMALLINT(5)";
			}
			else if (bind.ValueType == typeof(sbyte))
			{
				// override to prevent byte conversion
				type = "SMALLINT(3)";
			}
			else if (bind.ValueType == typeof(short))
			{
				type = "SMALLINT(6)";
			}
			else if (bind.ValueType == typeof(int))
			{
				type = "INT(11)";
			}
			else if (bind.ValueType == typeof(long))
			{
				type = "BIGINT(20)";
			}
			else if (bind.ValueType == typeof(byte))
			{
				type = "UNSIGNED TINYINT(3)";
			}
			else if (bind.ValueType == typeof(ushort))
			{
				type = "UNSIGNED SMALLINT(5)";
			}
			else if (bind.ValueType == typeof(uint))
			{
				type = "UNSIGNED INT(10)";
			}
			else if (bind.ValueType == typeof(ulong))
			{
				type = "UNSIGNED BIGINT(20)";
			}
			else if (bind.ValueType == typeof(bool))			  
			{
				type = "TINYINT(1)";
			}
			else if (bind.ValueType == typeof(DateTime))
			{
				type = "DATETIME";
			}
			else if (bind.ValueType == typeof(float))
			{
				type = "FLOAT";
			}
			else if (bind.ValueType == typeof(double))
			{
				type = "DOUBLE";
			}
			else if (bind.ValueType == typeof(string))
			{
				if (bind.DataElement != null && bind.DataElement.Varchar > 0)
				{
					type = string.Format("VARCHAR({0})", bind.DataElement.Varchar);
				}
				else if (table.Table.PrimaryKey.Any(key => key.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase))
				         || table.Table.Constraints.OfType<UniqueConstraint>().Any(cstrnt => cstrnt.Columns.Any(col => col.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase)))
				         || (table.Table.ExtendedProperties["INDEXES"] != null && (table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>)
				             .Any(kv => kv.Value.Any(col => col.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase)))))
				{
					// If is in Primary Key Constraint or Unique Constraint or Index row, cast to Varchar.
					type = "VARCHAR(255)";
				}
				else
				{
					type = "TEXT";
				}
			}
			else
			{
				type = "BLOB";
			}
			
			if (bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement)
				type = "INTEGER";
			
			return type;
		}
		
		/// <summary>
		/// Fill SQL Command Parameter with Converted Values.
		/// </summary>
		/// <param name="parameter">Parameter collection for this Command</param>
		/// <param name="dbParams">DbParameter Object to Fill</param>
		protected override void FillSQLParameter(IEnumerable<QueryParameter> parameter, DbParameterCollection dbParams)
		{
			// Specififc Handling for Char Cast from DB Integer
			// And Non Signed Integer Handling
    		foreach(var param in parameter)
    		{
    			if (param.Value is char)
    				dbParams[param.Name].Value = Convert.ToUInt16(param.Value);
    			else if (param.Value is uint)
    				dbParams[param.Name].Value = Convert.ToInt64(param.Value);
    			else if (param.Value is ulong)
    				dbParams[param.Name].Value = unchecked((long)Convert.ToUInt64(param.Value));
    			else
    				dbParams[param.Name].Value = param.Value;
    		}
		}
		
		/// <summary>
		/// Set Value to DataObject Field according to ElementBinding
		/// Override for SQLite to Handle some Specific Case (Unsigned Int64...)
		/// </summary>
		/// <param name="obj">DataObject to Fill</param>
		/// <param name="bind">ElementBinding for the targeted Member</param>
		/// <param name="value">Object Value to Fill</param>
		protected override void DatabaseSetValue(DataObject obj, ElementBinding bind, object value)
		{
			try
			{
				if (value != null && bind.ValueType == typeof(ulong))
				{
					bind.SetValue(obj, unchecked((ulong)Convert.ToInt64(value)));
					return;
				}
			}
			catch (Exception e)
			{
				if (Log.IsErrorEnabled)
					Log.ErrorFormat("{0}: {1} = {2} doesnt fit to {3}\n{4}", obj.TableName, bind.ColumnName, value.GetType().FullName, bind.ValueType, e);
			}
			
			base.DatabaseSetValue(obj, bind, value);
		}
		/// <summary>
		/// Get Database Column Definition for ElementBinding
		/// </summary>
		/// <param name="bind">ElementBinding for Column Definition</param>
		/// <param name="table">DataTableHanlder for Special cases</param>
		/// <returns>Column Definitnion string.</returns>
		protected virtual string GetColumnDefinition(ElementBinding bind, DataTableHandler table)
		{
			string type = GetDatabaseType(bind, table);
			string defaultDef = null;
						
			// Check for Default Value depending on Constraints and Type
			if (bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement)
			{
				defaultDef = "NOT NULL PRIMARY KEY AUTOINCREMENT";
			}
			else if (bind.DataElement != null && bind.DataElement.AllowDbNull)
			{
				defaultDef = "DEFAULT NULL";
			}
			else if (bind.ValueType == typeof(DateTime))
			{
				defaultDef = "NOT NULL DEFAULT '2000-01-01 00:00:00'";
			}
            else if (bind.ValueType == typeof(string))
            {
                defaultDef = "NOT NULL DEFAULT ''";
            }
			else
			{
                defaultDef = "NOT NULL DEFAULT 0";
			}
			
			// Force Case Insensitive Text Field to Match MySQL Behavior 
			if (bind.ValueType == typeof(string))
				defaultDef = string.Format("{0} {1}", defaultDef, "COLLATE NOCASE");
			
			return string.Format("`{0}` {1} {2}", bind.ColumnName, type, defaultDef);
		}
		#endregion
		
		#region Create / Alter Table
		/// <summary>
		/// Check for Table Existence, Create or Alter accordingly
		/// </summary>
		/// <param name="table">Table Handler</param>
		public override void CheckOrCreateTableImpl(DataTableHandler table)
		{
			var currentTableColumns = new List<TableRowBindind>();
			try
			{
				ExecuteSelectImpl(string.Format("PRAGMA TABLE_INFO(`{0}`)", table.TableName),
				                  reader =>
				                  {
				                  	while (reader.Read())
				                  	{
				                  		var column = reader.GetString(1);
				                  		var colType = reader.GetString(2);
				                  		var allowNull = !reader.GetBoolean(3);
				                  		var primary = reader.GetInt64(5) > 0;
				                  		currentTableColumns.Add(new TableRowBindind(column, colType, allowNull, primary));
				                  		if (log.IsDebugEnabled)
				                  			log.DebugFormat("CheckOrCreateTable: Found Column {0} in existing table {1}", column, table.TableName);
				                  	}
				                  	if (log.IsDebugEnabled)
				                  		log.DebugFormat("CheckOrCreateTable: {0} columns existing in table {1}", currentTableColumns.Count, table.TableName);
				                  }, IsolationLevel.DEFAULT);
			}
			catch (Exception e)
			{
				if (log.IsDebugEnabled)
					log.Debug("CheckOrCreateTable: ", e);
			}
			
			// Create Table or Alter Table
			if (currentTableColumns.Any())
			{
				AlterTable(currentTableColumns, table);
			}
			else
			{
				if (log.IsWarnEnabled)
					log.WarnFormat("Table {0} doesn't exist, creating it...", table.TableName);

				CreateTable(table);
			}
		}
		
		/// <summary>
		/// Helper Method to build Table Definition String
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected string GetTableDefinition(DataTableHandler table)
		{
			var columnDef = table.FieldElementBindings
				.Select(bind => GetColumnDefinition(bind, table));
			
			var primaryFields = new string[]{};
			if (!table.FieldElementBindings.Any(bind => bind.PrimaryKey != null && bind.PrimaryKey.AutoIncrement))
				primaryFields = new [] { string.Format("PRIMARY KEY ({0})",
				                              string.Join(", ", table.Table.PrimaryKey.Select(pk => string.Format("`{0}`", pk.ColumnName)))) };
			
			// Create Table First
			return string.Format("CREATE TABLE IF NOT EXISTS `{0}` ({1})", table.TableName,
			                            string.Join(", \n", columnDef.Concat(primaryFields)));
		}
		
		/// <summary>
		/// Helper Method to build Table Indexes Definition String
		/// </summary>
		/// <param name="table"></param>
		/// <returns></returns>
		protected IEnumerable<string> GetIndexesDefinition(DataTableHandler table)
		{
			// Indexes and Constraints
			var uniqueFields = table.Table.Constraints.OfType<UniqueConstraint>().Where(cstrnt => !cstrnt.IsPrimaryKey)
				.Select(cstrnt => string.Format("CREATE UNIQUE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", cstrnt.ConstraintName,
				                                string.Join(", ", cstrnt.Columns.Select(col => string.Format("`{0}`", col.ColumnName))),
				                                table.TableName));
			
			var indexes = table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>;
			
			var indexesFields = indexes == null ? new string[] { }
				: indexes.Select(index => string.Format("CREATE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", index.Key,
			                                        string.Join(", ", index.Value.Select(col => string.Format("`{0}`", col.ColumnName))),
			                                        table.TableName));
			
			return uniqueFields.Concat(indexesFields);
		}
		
		/// <summary>
		/// Create a New Table from DataTableHandler Definition
		/// </summary>
		/// <param name="table">DataTableHandler Definition to Create in Database</param>
		protected void CreateTable(DataTableHandler table)
		{
			ExecuteNonQueryImpl(GetTableDefinition(table));
			
			foreach (var commands in GetIndexesDefinition(table))
				ExecuteNonQueryImpl(commands);
		}
		
		/// <summary>
		/// Check if this Table need Alteration
		/// </summary>
		/// <param name="currentColumns">Current Existing Columns</param>
		/// <param name="table">DataTableHandler to Implement</param>
		protected bool CheckTableAlteration(IEnumerable<TableRowBindind> currentColumns, DataTableHandler table)
		{
			// Check for Any differences in Columns
			if (table.FieldElementBindings
			    .Any(bind => {
			         	var column = currentColumns.FirstOrDefault(col => col.ColumnName.Equals(bind.ColumnName, StringComparison.OrdinalIgnoreCase));
			         	
			         	if (column != null)
			         	{
			         		// Check Null
			         		if ((bind.DataElement != null && bind.DataElement.AllowDbNull) != column.AllowDbNull)
			         			return true;
			         		
			         		// Check Type
			         		if (!GetDatabaseType(bind, table).Equals(column.ColumnType, StringComparison.OrdinalIgnoreCase))
			         			return true;
			         		
			         		// Field are identical
			         		return false;
			         	}
			         	// Field missing
			         	return true;
			         }))
				return true;
			
			// Check for Any Difference in Primary Keys
			if (table.Table.PrimaryKey.Length != currentColumns.Count(col => col.Primary)
				|| table.Table.PrimaryKey.Any(pk => {
			                                  	var column = currentColumns.FirstOrDefault(col => col.ColumnName.Equals(pk.ColumnName, StringComparison.OrdinalIgnoreCase));
			                                  	
			                                  	if (column != null && column.Primary)
			                                  		return false;
			                                  	
			                                  	return true;
			                                  }))
				return true;
			
			// No Alteration Needed
			return false;
		}
		
		/// <summary>
		/// Check if Table Indexes Need Alteration
		/// </summary>
		/// <param name="table">DataTableHandler to Implement</param>
		protected void CheckTableIndexAlteration(DataTableHandler table)
		{
			// Query Existing Indexes
			var currentIndexes = new List<Tuple<string, string>>();
			try
			{
				ExecuteSelectImpl("SELECT name, sql FROM sqlite_master WHERE type == 'index' AND sql is not null AND tbl_name == @tableName",
				                  new QueryParameter("@tableName", table.TableName),
				                  reader =>
				                  {
				                  	while (reader.Read())
				                  		currentIndexes.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
				                  }, IsolationLevel.DEFAULT);
			}
			catch (Exception e)
			{
				if (log.IsDebugEnabled)
					log.Debug("CheckTableIndexAlteration: ", e);
				
				throw;
			}
			
			var sortedIndexes = currentIndexes.Select(ind => {
			                                          	var unique = ind.Item2.Trim().StartsWith("CREATE UNIQUE", StringComparison.OrdinalIgnoreCase);
			                                          	var columns = ind.Item2.Substring(ind.Item2.IndexOf('(')).Split(',').Select(sp => sp.Trim('`', '(', ')', ' '));
			                                          	return new { KeyName = ind.Item1, Unique = unique, Columns = columns.ToArray() };
			                                          }).ToArray();
			if (log.IsDebugEnabled)
				log.DebugFormat("CheckTableIndexAlteration: {0} Indexes existing in table {1}", sortedIndexes.Length, table.TableName);

			var tableIndexes = table.Table.ExtendedProperties["INDEXES"] as Dictionary<string, DataColumn[]>;
			
			var alterQueries = new List<string>();
			
			// Check for Index Removal
			foreach (var existing in sortedIndexes)
			{
				if (log.IsDebugEnabled)
					log.DebugFormat("CheckTableIndexAlteration: Found Index `{0}` (Unique:{1}) on ({2}) in existing table {3}", existing.KeyName, existing.Unique, string.Join(", ", existing.Columns), table.TableName);
				
				DataColumn[] realindex;
				if(tableIndexes.TryGetValue(existing.KeyName, out realindex))
				{
					// Check for index modifications
					if (realindex.Length != existing.Columns.Length
					    || !realindex.All(col => existing.Columns.Any(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
					{
						alterQueries.Add(string.Format("DROP INDEX `{0}`", existing.KeyName));
						alterQueries.Add(string.Format("CREATE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", existing.KeyName, string.Join(", ", realindex.Select(col => string.Format("`{0}`", col))), table.TableName));
					}
				}
				else
				{
					// Check for Unique
					var realunique = table.Table.Constraints.OfType<UniqueConstraint>().FirstOrDefault(cstrnt => !cstrnt.IsPrimaryKey && cstrnt.ConstraintName.Equals(existing.KeyName, StringComparison.OrdinalIgnoreCase));
					if (realunique == null)
						alterQueries.Add(string.Format("DROP INDEX `{0}`", existing.KeyName));
					else if (realunique.Columns.Length != existing.Columns.Length
					         || !realunique.Columns.All(col => existing.Columns.Any(c => c.Equals(col.ColumnName, StringComparison.OrdinalIgnoreCase))))
					{
						alterQueries.Add(string.Format("DROP INDEX `{0}`", existing.KeyName));
						alterQueries.Add(string.Format("CREATE UNIQUE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", existing.KeyName, string.Join(", ", realunique.Columns.Select(col => string.Format("`{0}`", col))), table.TableName));
					}
				}
			}
			
			// Missing Indexes
			foreach (var missing in tableIndexes.Where(kv => sortedIndexes.All(c => !c.KeyName.Equals(kv.Key, StringComparison.OrdinalIgnoreCase))))
				alterQueries.Add(string.Format("CREATE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", missing.Key, string.Join(", ", missing.Value.Select(col => string.Format("`{0}`", col))), table.TableName));
			
			foreach (var missing in table.Table.Constraints.OfType<UniqueConstraint>().Where(cstrnt => !cstrnt.IsPrimaryKey && sortedIndexes.All(c => !c.KeyName.Equals(cstrnt.ConstraintName, StringComparison.OrdinalIgnoreCase))))
				alterQueries.Add(string.Format("CREATE UNIQUE INDEX IF NOT EXISTS `{0}` ON `{2}` ({1})", missing.ConstraintName, string.Join(", ", missing.Columns.Select(col => string.Format("`{0}`", col))), table.TableName));
			
			if (!alterQueries.Any())
				return;
			
			if (log.IsDebugEnabled)
				log.DebugFormat("Altering Table Indexes {0} this could take a few minutes...", table.TableName);
			
			foreach (var query in alterQueries)
				ExecuteNonQueryImpl(query);
		}
		
		/// <summary>
		/// Alter an Existing Table to Match DataTableHandler Definition
		/// </summary>
		/// <param name="currentColumns">Current Existing Columns</param>
		/// <param name="table">DataTableHandler to Implement</param>
		protected void AlterTable(IEnumerable<TableRowBindind> currentColumns, DataTableHandler table)
		{
			// If Column are not modified Alter Table is not needed...
			if  (!CheckTableAlteration(currentColumns, table))
			{
				// Table not Altered check for Indexes and return
				CheckTableIndexAlteration(table);
				return;
			}
			
			if (log.IsInfoEnabled)
				log.InfoFormat("Altering Table {0} this could take a few minutes...", table.TableName);
			
			var currentIndexes = new List<string>();
			try
			{
				ExecuteSelectImpl("SELECT name FROM sqlite_master WHERE type == 'index' AND sql is not null AND tbl_name == @tableName",
				                  new QueryParameter("@tableName", table.TableName),
				                  reader =>
				                  {
				                  	while (reader.Read())
				                  		currentIndexes.Add(reader.GetString(0));
				                  }, IsolationLevel.DEFAULT);
			}
			catch (Exception e)
			{
				if (log.IsDebugEnabled)
					log.Debug("AlterTableImpl: ", e);

				if (log.IsWarnEnabled)
					log.WarnFormat("AlterTableImpl: Error While Altering Table {0}, no modifications...", table.TableName);

				throw;
			}

			using (var conn = new SQLiteConnection(ConnectionString))
			{
				conn.Open();
				using(var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					try
					{
						// Delete Indexes
						foreach(var index in currentIndexes)
						{
							using (var command = new SQLiteCommand(string.Format("DROP INDEX `{0}`", index), conn))
							{
								command.Transaction = tran;
								command.ExecuteNonQuery();
							}
						}
						
						// Rename Table
						using (var command = new SQLiteCommand(string.Format("ALTER TABLE `{0}` RENAME TO `{0}_bkp`", table.TableName), conn))
						{
							command.Transaction = tran;
							command.ExecuteNonQuery();
						}
						
						// Create New Table
						using (var command = new SQLiteCommand(GetTableDefinition(table), conn))
						{
							command.Transaction = tran;
							command.ExecuteNonQuery();
						}
						
						// Create Indexes
						foreach (var index in GetIndexesDefinition(table))
						{
							using (var command = new SQLiteCommand(index, conn))
							{
								command.Transaction = tran;
								command.ExecuteNonQuery();
							}
						}
						
                        // Copy Data, Convert Null to Default when needed...
                        var matchingColumns = table.FieldElementBindings.Join(currentColumns, bind => bind.ColumnName, col => col.ColumnName, (bind, col) => new { bind, col }, StringComparer.OrdinalIgnoreCase);
                        var columns = matchingColumns.Select(match => {
                        if (match.bind.DataElement != null && match.bind.DataElement.AllowDbNull == false && match.col.AllowDbNull == true)
                                                                 {
                                                                     if (match.bind.ValueType == typeof(DateTime))
                                                                         return new { Target = match.bind.ColumnName, Source = string.Format("IFNULL(`{0}`, {1})", match.bind.ColumnName, "'2000-01-01 00:00:00'") };
                                                                     if (match.bind.ValueType == typeof(string))
                                                                         return new { Target = match.bind.ColumnName, Source = string.Format("IFNULL(`{0}`, {1})", match.bind.ColumnName, "''") };
                                                                     
                                                                     return new { Target = match.bind.ColumnName, Source = string.Format("IFNULL(`{0}`, {1})", match.bind.ColumnName, "0") };
                                                                 }
                                                                 
                                                                 return new { Target = match.bind.ColumnName, Source = string.Format("`{0}`", match.bind.ColumnName) };
                                                             });
                        
                        using (var command = new SQLiteCommand(string.Format("INSERT INTO `{0}` ({1}) SELECT {2} FROM `{0}_bkp`", table.TableName, string.Join(", ", columns.Select(c => c.Target)), string.Join(", ", columns.Select(c => c.Source))), conn))
						{
                            if (log.IsDebugEnabled)
                                log.DebugFormat("AlterTableImpl, Insert/Select: {0}", command.CommandText);

                            command.Transaction = tran;
							command.ExecuteNonQuery();
						}

						// Drop Renamed Table
						using (var command = new SQLiteCommand(string.Format("DROP TABLE `{0}_bkp`", table.TableName), conn))
						{
							command.Transaction = tran;
							command.ExecuteNonQuery();
						}
						
						tran.Commit();
						if (log.IsInfoEnabled)
							log.InfoFormat("AlterTableImpl: Table {0} Altered...", table.TableName);
					}
					catch (Exception e)
					{
						tran.Rollback();
						if (log.IsDebugEnabled)
							log.Debug("AlterTableImpl: ", e);
						
						if (log.IsWarnEnabled)
							log.WarnFormat("AlterTableImpl: Error While Altering Table {0}, rollback...\n{1}", table.TableName, e);
                        
                        throw;
					}
					
				}
			}
		}
		#endregion

		#region Property implementation
		/// <summary>
		/// The connection type to DB (xml, mysql,...)
		/// </summary>
		public override ConnectionType ConnectionType { get { return ConnectionType.DATABASE_SQLITE; } }
		#endregion
		
		#region SQLObject Implementation
		/// <summary>
		/// Raw SQL Select Implementation with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Command for reading</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
		/// <param name="Reader">Reader Method</param>
		/// <param name="Isolation">Transaction Isolation</param>
		protected override void ExecuteSelectImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, Action<IDataReader> Reader, IsolationLevel Isolation)
		{
			if (log.IsDebugEnabled)
				log.DebugFormat("ExecuteSelectImpl: {0}", SQLCommand);

			var repeat = false;
			var current = 0;
			do
			{
				repeat = false;

				using (var conn = new SQLiteConnection(ConnectionString))
				{
				    using (var cmd = new SQLiteCommand(SQLCommand, conn))
					{
						try
						{
					    	conn.Open();
					    	cmd.Prepare();
					    	
					    	long start = (DateTime.UtcNow.Ticks / 10000);
					    	
					    	// Register Parameter
					    	foreach(var keys in parameters.First().Select(kv => kv.Name))
					    		cmd.Parameters.Add(new SQLiteParameter(keys));
					    	
					    	foreach(var parameter in parameters.Skip(current))
					    	{
					    		FillSQLParameter(parameter, cmd.Parameters);
					    	
							    using (var reader = cmd.ExecuteReader())
							    {
							    	try
							    	{
							        	Reader(reader);
							    	}
							    	catch (Exception es)
							    	{
							    		if(log.IsWarnEnabled)
							    			log.WarnFormat("ExecuteSelectImpl: Exception in Select Callback : {2}{0}{2}{1}", es, Environment.StackTrace, Environment.NewLine);
							    	}
							    	finally
							    	{
							    		reader.Close();
							    	}
							    }
							    current++;
					    	}
					    
						    if (log.IsDebugEnabled)
								log.DebugFormat("ExecuteSelectImpl: SQL Select ({0}) exec time {1}ms", Isolation, ((DateTime.UtcNow.Ticks / 10000) - start));
							else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
								log.WarnFormat("ExecuteSelectImpl: SQL Select ({0}) took {1}ms!\n{2}", Isolation, ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);
						
						}
						catch (Exception e)
						{
							if (!HandleException(e))
							{
								if (log.IsErrorEnabled)
									log.ErrorFormat("ExecuteSelectImpl: UnHandled Exception for Select Query \"{0}\"\n{1}", SQLCommand, e);
								
								throw;
							}
							repeat = true;
						}
						finally
						{
							conn.Close();
						}
				    }
				}
			}
			while (repeat);
		}
		
		/// <summary>
		/// Implementation of Raw Non-Query with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Raw Command</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
		/// <returns>True if the Command succeeded</returns>
		protected override IEnumerable<int> ExecuteNonQueryImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters)
		{
			if (log.IsDebugEnabled)
				log.DebugFormat("ExecuteNonQueryImpl: {0}", SQLCommand);

			var affected = new List<int>();
			var repeat = false;
			var current = 0;
			do
			{
				repeat = false;

				using (var conn = new SQLiteConnection(ConnectionString))
				{
					using (var cmd = new SQLiteCommand(SQLCommand, conn))
					{
						try
						{
					    	conn.Open();
					    	cmd.Prepare();
						    
					    	long start = (DateTime.UtcNow.Ticks / 10000);
						    
					    	// Register Parameter
					    	foreach(var keys in parameters.First().Select(kv => kv.Name))
					    		cmd.Parameters.Add(new SQLiteParameter(keys));
					    	
					    	foreach(var parameter in parameters.Skip(current))
					    	{
					    		FillSQLParameter(parameter, cmd.Parameters);
					    		var result = -1;
					    		try
					    		{
							    	result = cmd.ExecuteNonQuery();
							    	affected.Add(result);
					    		}
					    		catch (SQLiteException sqle)
					    		{
					    			if (HandleSQLException(sqle))
					    			{
    									affected.Add(result);
    									if (log.IsErrorEnabled)
											log.ErrorFormat("ExecuteNonQueryImpl: Constraint Violation for raw query \"{0}\"\n{1}\n{2}", SQLCommand, sqle, Environment.StackTrace);
					    			}
					    			else
					    			{
					    				throw;
					    			}
					    		}
							    current++;
							    
							    if (log.IsDebugEnabled && result < 1)
							    	log.DebugFormat("ExecuteNonQueryImpl: No Change for raw query \"{0}\"", SQLCommand);
					    	}
						    
						    
						    if (log.IsDebugEnabled)
								log.DebugFormat("ExecuteNonQueryImpl: SQL NonQuery exec time {0}ms", ((DateTime.UtcNow.Ticks / 10000) - start));
							else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
								log.WarnFormat("ExecuteNonQueryImpl: SQL NonQuery took {0}ms!\n{1}", ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);
						}
						catch (Exception e)
						{
							if (!HandleException(e))
							{
								if(log.IsErrorEnabled)
									log.ErrorFormat("ExecuteNonQueryImpl: UnHandled Exception for raw query \"{0}\"\n{1}", SQLCommand, e);
								
								throw;
							}
							repeat = true;
						}
						finally
						{
							conn.Close();
						}
					}
				}
			} 
			while (repeat);
			
			return affected;
		}

		/// <summary>
		/// Implementation of Scalar Query with Parameters for Prepared Query
		/// </summary>
		/// <param name="SQLCommand">Scalar Command</param>
		/// <param name="parameters">Collection of Parameters for Single/Multiple Read</param>
		/// <param name="retrieveLastInsertID">Return Last Insert ID of each Command instead of Scalar</param>
		/// <returns>Objects Returned by Scalar</returns>
		protected override object[] ExecuteScalarImpl(string SQLCommand, IEnumerable<IEnumerable<QueryParameter>> parameters, bool retrieveLastInsertID)
		{
			if (log.IsDebugEnabled)
				log.DebugFormat("ExecuteScalarImpl: {0}", SQLCommand);
			
			var obj = new List<object>();
			var repeat = false;
			var current = 0;
			do
			{
				repeat = false;
				
				using (var conn = new SQLiteConnection(ConnectionString))
				{					    
					using (var cmd = new SQLiteCommand(SQLCommand, conn))
					{
						try
						{
						    conn.Open();
					    	cmd.Prepare();
						    long start = (DateTime.UtcNow.Ticks / 10000);

						    // Register Parameter
					    	foreach(var keys in parameters.First().Select(kv => kv.Name))
					    		cmd.Parameters.Add(new SQLiteParameter(keys));
					    	
					    	foreach(var parameter in parameters.Skip(current))
					    	{
					    		FillSQLParameter(parameter, cmd.Parameters);
					    		
					    		if (retrieveLastInsertID)
					    		{
					    			using (var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
					    			{
					    				try
					    				{
						    				cmd.Transaction = tran;
						    				try
						    				{
							    				cmd.ExecuteNonQuery();
							    				obj.Add(conn.LastInsertRowId);
						    				}
								    		catch (SQLiteException sqle)
								    		{
								    			if (HandleSQLException(sqle))
								    			{
			    									obj.Add(-1);
			    									if (log.IsErrorEnabled)
														log.ErrorFormat("ExecuteScalarImpl: Constraint Violation for command \"{0}\"\n{1}\n{2}", SQLCommand, sqle, Environment.StackTrace);
								    			}
								    			else
								    			{
								    				throw;
								    			}
								    		}
							    			
							    			tran.Commit();
					    				}
					    				catch (Exception te)
					    				{
					    					tran.Rollback();
					    					if (log.IsErrorEnabled)
					    						log.ErrorFormat("ExecuteScalarImpl: Error in Transaction (Rollback) for command : {0}\n{1}", SQLCommand, te);
					    				}
					    			}
					    		}
					    		else
					    		{
					    			var result = cmd.ExecuteScalar();
									obj.Add(result);
					    		}
								current++;
					    	}

					    	if (log.IsDebugEnabled)
								log.DebugFormat("ExecuteScalarImpl: SQL ScalarQuery exec time {0}ms", ((DateTime.UtcNow.Ticks / 10000) - start));
							else if (log.IsWarnEnabled && (DateTime.UtcNow.Ticks / 10000) - start > 500)
								log.WarnFormat("ExecuteScalarImpl: SQL ScalarQuery took {0}ms!\n{1}", ((DateTime.UtcNow.Ticks / 10000) - start), SQLCommand);
						}
						catch (Exception e)
						{
	
							if (!HandleException(e))
							{
								if(log.IsErrorEnabled)
									log.ErrorFormat("ExecuteScalarImpl: UnHandled Exception for command \"{0}\"\n{1}", SQLCommand, e);
								
								throw;
							}
	
							repeat = true;
						}
						finally
						{
							conn.Close();
						}
					}
				}
			} 
			while (repeat);

			return obj.ToArray();
		}
		#endregion
		/// <summary>
		/// Handle Non Fatal SQL Query Exception
		/// </summary>
		/// <param name="sqle">SQL Excepiton</param>
		/// <returns>True if handled, False otherwise</returns>
		protected static bool HandleSQLException(SQLiteException sqle)
		{
			switch (sqle.ResultCode)
			{
				case SQLiteErrorCode.Constraint:
				case SQLiteErrorCode.Constraint_Check:
				case SQLiteErrorCode.Constraint_ForeignKey:
				case SQLiteErrorCode.Constraint_Unique:
				case SQLiteErrorCode.Constraint_NotNull:
					return true;
				default:
					return false;
			}
		}
		
		/// <summary>
		/// escape the strange character from string
		/// </summary>
		/// <param name="rawInput">the string</param>
		/// <returns>the string with escaped character</returns>
		public override string Escape(string rawInput)
		{
			return rawInput.Replace("'", "''");
		}
	}
}
