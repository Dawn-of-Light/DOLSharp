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
using System.Data.Common;
using Microsoft.Data.Sqlite;

using DOL.Database.Connection;

namespace DOL.Database.Handlers
{
	public class SQLiteObjectDatabase : SQLObjectDatabase
	{
		#region Property implementation
		public override ConnectionType ConnectionType { get { return ConnectionType.DATABASE_SQLITE; } }
		protected override string PreCommandDirectives
			=> $"PRAGMA journal_mode='{Config.GetValueOf("Journal Mode")}';"
			+ $"PRAGMA cache_size='{Config.GetValueOf("Cache Size")}';"
			+ $"PRAGMA synchronous='{Config.GetValueOf("Synchronous")}';";
		#endregion

		public SQLiteObjectDatabase(string ConnectionString)
			: base(ConnectionString)
		{
			Config = new DbConfig(ConnectionString);
			Config.AddDefaultOption("Version", "3");
			Config.AddDefaultOption("Pooling", "False");
			Config.AddDefaultOption("Cache Size", "1073741824");
			Config.AddDefaultOption("Journal Mode", "Memory");
			Config.AddDefaultOption("Synchronous", "Off");
			Config.AddDefaultOption("Foreign Keys", "True");
			Config.AddDefaultOption("Default Timeout", "60");
			Config.SuppressFromConnectionString("Version", "Cache Size", "Journal Mode", "Synchronous", "Default Timeout");
			this.ConnectionString = Config.ConnectionString;
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
				if (log.IsErrorEnabled)
                    log.ErrorFormat("{0}: {1} = {2} doesnt fit to {3}\n{4}", obj.TableName, bind.ColumnName, value.GetType().FullName, bind.ValueType, e);
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
					new[] { Array.Empty<QueryParameter>() },
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
					});
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
			
			var primaryFields = Array.Empty<string>();
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
			
			var indexesFields = indexes == null ? Array.Empty<string>()
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
				                  new[] { new[] { new QueryParameter("@tableName", table.TableName) } },
				                  reader =>
				                  {
				                  	while (reader.Read())
				                  		currentIndexes.Add(new Tuple<string, string>(reader.GetString(0), reader.GetString(1)));
				                  });
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
				                  new[] { new[] { new QueryParameter("@tableName", table.TableName) } },
				                  reader =>
				                  {
				                  	while (reader.Read())
				                  		currentIndexes.Add(reader.GetString(0));
				                  });
			}
			catch (Exception e)
			{
				if (log.IsDebugEnabled)
					log.Debug("AlterTableImpl: ", e);

				if (log.IsWarnEnabled)
					log.WarnFormat("AlterTableImpl: Error While Altering Table {0}, no modifications...", table.TableName);

				throw;
			}

			using (var conn = new SqliteConnection(ConnectionString))
			{
				conn.Open();
				using(var tran = conn.BeginTransaction(System.Data.IsolationLevel.ReadCommitted))
				{
					try
					{
						// Delete Indexes
						foreach(var index in currentIndexes)
						{
							using (var command = new SqliteCommand(string.Format("DROP INDEX `{0}`", index), conn))
							{
								command.Transaction = tran;
								command.ExecuteNonQuery();
							}
						}
						
						// Rename Table
						using (var command = new SqliteCommand(string.Format("ALTER TABLE `{0}` RENAME TO `{0}_bkp`", table.TableName), conn))
						{
							command.Transaction = tran;
							command.ExecuteNonQuery();
						}
						
						// Create New Table
						using (var command = new SqliteCommand(GetTableDefinition(table), conn))
						{
							command.Transaction = tran;
							command.ExecuteNonQuery();
						}
						
						// Create Indexes
						foreach (var index in GetIndexesDefinition(table))
						{
							using (var command = new SqliteCommand(index, conn))
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
                        
                        using (var command = new SqliteCommand(string.Format("INSERT INTO `{0}` ({1}) SELECT {2} FROM `{0}_bkp`", table.TableName, string.Join(", ", columns.Select(c => c.Target)), string.Join(", ", columns.Select(c => c.Source))), conn))
						{
                            if (log.IsDebugEnabled)
                                log.DebugFormat("AlterTableImpl, Insert/Select: {0}", command.CommandText);

                            command.Transaction = tran;
							command.ExecuteNonQuery();
						}

						// Drop Renamed Table
						using (var command = new SqliteCommand(string.Format("DROP TABLE `{0}_bkp`", table.TableName), conn))
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

        #region SQLObject Implementation
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

				if (!parameters.Any()) throw new ArgumentException("No parameter list was given.");

				using (var conn = new SqliteConnection(ConnectionString))
				{					    
					using (var cmd = conn.CreateCommand())
					{
						try
						{
							cmd.CommandText = SQLCommand;
							conn.Open();
							long start = (DateTime.UtcNow.Ticks / 10000);

							foreach (var parameter in parameters.Skip(current))
							{
								FillSQLParameter(parameter, cmd.Parameters);
								cmd.Prepare();

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
												obj.Add(GetLastInsertedID(conn));
											}
											catch (Exception ex)
											{
												if (HandleSQLException(ex))
												{
													obj.Add(-1);
													if (log.IsErrorEnabled)
														log.ErrorFormat("ExecuteScalarImpl: Constraint Violation for command \"{0}\"\n{1}\n{2}", SQLCommand, ex, Environment.StackTrace);
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
								if (log.IsErrorEnabled)
									log.ErrorFormat("ExecuteScalarImpl: UnHandled Exception for command \"{0}\"\n{1}", SQLCommand, e);

								throw;
							}
							repeat = true;
						}
						finally
						{
							CloseConnection(conn);
						}
					}
				}
			} 
			while (repeat);

			return obj.ToArray();
		}

		private long GetLastInsertedID(SqliteConnection conn)
		{
			using (var cmd = conn.CreateCommand())
			{
				var sql = "SELECT last_insert_rowid()";
				cmd.CommandText = sql;
				var lastID = (long)cmd.ExecuteScalar();
				return lastID;
			}
		}

		#endregion

		public override DbConnection CreateConnection() => new SqliteConnection(ConnectionString);

		protected override void CloseConnection(DbConnection connection)
		{
			connection.Close();
		}

		protected override DbParameter ConvertToDBParameter(QueryParameter queryParameter)
		{
			var dbParam = new SqliteParameter();
			dbParam.ParameterName = queryParameter.Name;

			if (queryParameter.Value == null) dbParam.Value = DBNull.Value;
			else if (queryParameter.Value is char)
				dbParam.Value = Convert.ToUInt16(queryParameter.Value);
			else if (queryParameter.Value is uint)
				dbParam.Value = Convert.ToInt64(queryParameter.Value);
			else if (dbParam.Value is ulong)
				dbParam.Value = unchecked((long)Convert.ToUInt64(queryParameter.Value));
			else
				dbParam.Value = queryParameter.Value;

			return dbParam;
		}

		protected override bool HandleSQLException(Exception e)
		{
			if (e is SqliteException sqle)
			{
				if (sqle.SqliteErrorCode == 19) return true;
				switch (sqle.SqliteExtendedErrorCode)
				{
					case 19: //SqliteErrorCode.Constraint:
					case 275: //SqliteErrorCode.Constraint_Check:
					case 787: //SQLiteErrorCode.Constraint_ForeignKey:
					case 2067: //SQLiteErrorCode.Constraint_Unique:
					case 1299: //SQLiteErrorCode.Constraint_NotNull:
						return true;
					default:
						return false;
				}
			}
			return false;
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
