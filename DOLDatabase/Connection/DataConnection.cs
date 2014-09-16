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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.IO;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using log4net;
using MySql.Data.MySqlClient;

namespace DOL.Database.Connection
{
	/// <summary>
	/// Called after mysql query.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public delegate void QueryCallback(IDataReader reader);

	/// <summary>
	/// Class for Handling the Connection to the ADO.Net Layer of the Databases.
	/// Funktions for loading and storing the complete Dataset are in there.
	/// </summary>
	public class DataConnection
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		private string connString;
		private ConnectionType connType;

		/// <summary>
		/// Constructor to set up a Database
		/// </summary>
		/// <param name="connType">Connection-Type the Database should use</param>
		/// <param name="connString">Connection-String to indicate the Parameters of the Datasource.
		///     XML = Directory where the XML-Files should be stored
		///     MYSQL = ADO.NET ConnectionString 
		///     MSSQL = ADO.NET ConnectionString 
		///     OLEDB = ADO.NET ConnectionString 
		///     ODBC = ADO.NET ConnectionString 
		/// </param>
		public DataConnection(ConnectionType connType, string connString)
		{
			this.connType = connType;

			//if Directory has no trailing \ than append it ;-)
			if (connType == ConnectionType.DATABASE_XML)
			{
				if (connString[connString.Length - 1] != Path.DirectorySeparatorChar)
					this.connString = connString + Path.DirectorySeparatorChar;

				if (!Directory.Exists(connString))
				{
					try
					{
						Directory.CreateDirectory(connString);
					}
					catch (Exception)
					{
					}
				}
			}
			else
			{
				// Options of MySQL connection string
				if (!connString.Contains("Treat Tiny As Boolean"))
				{
					connString += ";Treat Tiny As Boolean=False";
				}

				this.connString = connString;
			}
		}

		/// <summary>
		/// Check if SQL connection
		/// </summary>
		public bool IsSQLConnection
		{
			get { return connType == ConnectionType.DATABASE_MYSQL || connType == ConnectionType.DATABASE_SQLITE; }
		}

		/// <summary>
		/// The connection type to DB (xml, mysql,...)
		/// </summary>
		public ConnectionType ConnectionType
		{
			get { return connType; }
		}

		/// <summary>
		/// escape the strange character from string
		/// </summary>
		/// <param name="s">the string</param>
		/// <returns>the string with escaped character</returns>
		public string Escape(string s)
		{
			if (!IsSQLConnection)
			{
				s = s.Replace("'", "''");
			}
			else
			{
				s = s.Replace("\\", "\\\\");
				s = s.Replace("\"", "\\\"");
				s = s.Replace("'", "\\'");
				s = s.Replace("’", "\\’");
				//s = s.Replace("’", "\\’");//have it now in the mysqlstring object
			}
			return s;
		}

		/// <summary>
		/// Execute a non query like update or delete
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns>number of rows affected</returns>
		public int ExecuteNonQuery(string sqlcommand)
		{
			if (connType == ConnectionType.DATABASE_MYSQL)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				int affected = 0;
				bool repeat = false;
				do
				{
					repeat = false;

					using (var conn = new MySqlConnection(connString))
					{
						using (var cmd = conn.CreateCommand())
						{
							try
							{
					
							    cmd.CommandText = sqlcommand;
							    conn.Open();
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    affected = cmd.ExecuteNonQuery();
							    
							    if (log.IsDebugEnabled)
									log.Debug("SQL NonQuery exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL NonQuery took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							}
							catch (Exception e)
							{
								if (!HandleException(e))
								{
									throw;
								}
								repeat = true;
							}
						}
					}

				} 
				while (repeat);

				return affected;
			}
			else if (connType == ConnectionType.DATABASE_SQLITE)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				int affected = 0;
				bool repeat = false;
				do
				{
					repeat = false;

					using (var conn = new SQLiteConnection(connString))
					{
						using (var cmd = new SQLiteCommand(sqlcommand, conn))
						{
							try
							{
						    	conn.Open();
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    affected = cmd.ExecuteNonQuery();
							    
							    if (log.IsDebugEnabled)
									log.Debug("SQL NonQuery exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL NonQuery took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							}
							catch (Exception e)
							{
								if (!HandleException(e))
								{
									if(log.IsErrorEnabled)
										log.ErrorFormat("Error while NonQuery: {0}", sqlcommand);
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

			if (log.IsWarnEnabled)
				log.Warn("SQL NonQuery's not supported for this connection type");

			return 0;
		}

		/// <summary>
		/// Handles the exception.
		/// </summary>
		/// <param name="e">The exception.</param>
		/// <returns><code>true</code> if operation should be repeated, <code>false</code> otherwise.</returns>
		private static bool HandleException(Exception e)
		{
			bool ret = false;
			SocketException socketException = e.InnerException == null
			                                  	? null
			                                  	: e.InnerException.InnerException as SocketException;
			if (socketException == null)
			{
				socketException = e.InnerException as SocketException;
			}

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
						{
							ret = true;
							break;
						}
				}

				log.WarnFormat("Socket exception: ({0}) {1}; repeat: {2}", socketException.ErrorCode, socketException.Message, ret);
			}

			return ret;
		}

		/// <summary>
		/// Execute select on sql database
		/// Close returned datareader when done or use using(reader) { ... }
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <param name="callback"></param>
		public void ExecuteSelect(string sqlcommand, QueryCallback callback, Transaction.IsolationLevel isolation)
		{
			if (connType == ConnectionType.DATABASE_MYSQL)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				bool repeat = false;
				do
				{
					repeat = false;

					using (var conn = new MySqlConnection(connString))
					{
						using (var cmd = conn.CreateCommand())
						{
							try
							{
							    cmd.CommandText = sqlcommand;
								conn.Open();
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    using (var reader = cmd.ExecuteReader())
							    {
							    	try
							    	{
							        	callback(reader);
							    	}
							    	catch (Exception es)
							    	{
							    		if(log.IsWarnEnabled)
							    			log.Warn("Exception in Select Callback : ", es);
							    	}
							    }
						    
							    if (log.IsDebugEnabled)
									log.Debug("SQL Select (" + isolation + ") exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL Select (" + isolation + ") took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							
							}
							catch (Exception e)
							{
								if (!HandleException(e))
								{
									if (log.IsErrorEnabled)
										log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
									throw;
								}
		
								repeat = true;
							}
						}
					}
					
				}
				while (repeat);

				return;
			}
			else if (connType == ConnectionType.DATABASE_SQLITE)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				bool repeat = false;
				do
				{
					repeat = false;

					using (var conn = new SQLiteConnection(connString))
					{
					    using (var cmd = new SQLiteCommand(sqlcommand, conn))
						{
							try
							{
						    	conn.Open();
							    
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    using (var reader = cmd.ExecuteReader())
							    {
							    	try
							    	{
							        	callback(reader);
							    	}
							    	catch (Exception es)
							    	{
							    		if(log.IsWarnEnabled)
							    			log.Warn("Exception in Select Callback : ", es);
							    	}
							    	finally
							    	{
							    		reader.Close();
							    	}
							    }
						    
							    if (log.IsDebugEnabled)
									log.Debug("SQL Select (" + isolation + ") exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL Select (" + isolation + ") took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							
							}
							catch (Exception e)
							{
								if (!HandleException(e))
								{
									if (log.IsErrorEnabled)
										log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
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

				return;
			}

			if (log.IsWarnEnabled)
				log.Warn("SQL Selects not supported for this connection type");
		}

		/// <summary>
		/// Execute scalar on sql database
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		public object ExecuteScalar(string sqlcommand)
		{
			if (connType == ConnectionType.DATABASE_MYSQL)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				object obj = null;
				bool repeat = false;
				do
				{
					repeat = false;
					
					using (var conn = new MySqlConnection(connString))
					{
						using (var cmd = conn.CreateCommand())
						{
							try
							{
					
								conn.Open();
							    cmd.CommandText = sqlcommand;
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    obj = cmd.ExecuteScalar();
							    
								if (log.IsDebugEnabled)
									log.Debug("SQL Select exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL Select took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							}
							catch (Exception e)
							{
		
								if (!HandleException(e))
								{
									if (log.IsErrorEnabled)
										log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
									throw;
								}
		
								repeat = true;
							}
						}
					}
				} 
				while (repeat);

				return obj;
			}
			else if (ConnectionType == ConnectionType.DATABASE_SQLITE)
			{
				if (log.IsDebugEnabled)
				{
					log.Debug("SQL: " + sqlcommand);
				}

				object obj = null;
				bool repeat = false;
				do
				{
					repeat = false;
					
					using (var conn = new SQLiteConnection(connString))
					{					    
						using (var cmd = new SQLiteCommand(sqlcommand, conn))
						{
							try
							{
							    conn.Open();
							
							    long start = (DateTime.UtcNow.Ticks / 10000);
							    obj = cmd.ExecuteScalar();
							    
								if (log.IsDebugEnabled)
									log.Debug("SQL Select exec time " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms");
								else if ((DateTime.UtcNow.Ticks / 10000) - start > 500 && log.IsWarnEnabled)
									log.Warn("SQL Select took " + ((DateTime.UtcNow.Ticks / 10000) - start) + "ms!\n" + sqlcommand);
							}
							catch (Exception e)
							{
		
								if (!HandleException(e))
								{
									if (log.IsErrorEnabled)
										log.Error("ExecuteSelect: \"" + sqlcommand + "\"\n", e);
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

				return obj;
			}

			if (log.IsWarnEnabled)
				log.Warn("SQL Scalar not supported for this connection type");

			return null;
		}

		/// <summary>
		/// Create the table in mysql
		/// </summary>
		/// <param name="table">the table to create</param>
		public void CheckOrCreateTable(DataTable table)
		{
			if (connType == ConnectionType.DATABASE_MYSQL || connType == ConnectionType.DATABASE_SQLITE)
			{
				var currentTableColumns = new ArrayList();
				try
				{
					if (connType == ConnectionType.DATABASE_MYSQL)
					{
					
						ExecuteSelect("DESCRIBE `" + table.TableName + "`", delegate(IDataReader reader)
					                                                    	{
					                                                    		while (reader.Read())
					                                                    		{
					                                                    			currentTableColumns.Add(reader.GetString(0).ToLower());
					                                                    			log.Debug(reader.GetString(0).ToLower());
					                                                    		}
					                                                    		if (log.IsDebugEnabled)
					                                                    			log.Debug(currentTableColumns.Count + " in table");
					                                                    	}, Transaction.IsolationLevel.DEFAULT);
					}
					else if (connType == ConnectionType.DATABASE_SQLITE)	
					{
						ExecuteSelect("PRAGMA TABLE_INFO(\"" + table.TableName + "\")", delegate(IDataReader reader)
					                                                    	{
					                                                    		while (reader.Read())
					                                                    		{
					                                                    			currentTableColumns.Add(reader.GetString(1).ToLower());
					                                                    			log.Debug(reader.GetString(1).ToLower());
					                                                    		}
					                                                    		if (log.IsDebugEnabled)
					                                                    			log.Debug(currentTableColumns.Count + " in table");
					                                                    	}, Transaction.IsolationLevel.DEFAULT);
					}
				}
				catch (Exception e)
				{
					if (log.IsDebugEnabled)
						log.Debug(e.ToString());

					if (log.IsWarnEnabled)
						log.Warn("Table " + table.TableName + " doesn't exist, creating it...");
				}

				var sb = new StringBuilder();
				var primaryKeys = new Dictionary<string, DataColumn>();
				for (int i = 0; i < table.PrimaryKey.Length; i++)
				{
					primaryKeys[table.PrimaryKey[i].ColumnName] = table.PrimaryKey[i];
				}

				var columnDefs = new List<string>();
				var alterAddColumnDefs = new List<string>();
				bool alterPrimaryKey = false;
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Type systype = table.Columns[i].DataType;

					string column = "";

					column += "`" + table.Columns[i].ColumnName + "` ";

					if (connType == ConnectionType.DATABASE_SQLITE && (table.Columns[i].AutoIncrement || systype == typeof (sbyte) || systype == typeof (byte)
					                                                   || systype == typeof (short) || systype == typeof (ushort) || systype == typeof (int)
					                                                   || systype == typeof (uint) || systype == typeof (long) || systype == typeof (ulong)))
					{
						column += "INTEGER";
					}
					else if (systype == typeof (char))
					{
						column += "SMALLINT UNSIGNED";
					}
					else if (systype == typeof (DateTime))
					{
						column += "DATETIME DEFAULT '2000-01-01'";
					}
					else if (systype == typeof (sbyte))
					{
						column += "TINYINT";
					}
					else if (systype == typeof (short))
					{
						column += "SMALLINT";
					}
					else if (systype == typeof (int))
					{
						column += "INT";
					}
					else if (systype == typeof (long))
					{
						column += "BIGINT";
					}
					else if (systype == typeof (byte))
					{
						column += "TINYINT UNSIGNED";
					}
					else if (systype == typeof (ushort))
					{
						column += "SMALLINT UNSIGNED";
					}
					else if (systype == typeof (uint))
					{
						column += "INT UNSIGNED";
					}
					else if (systype == typeof (ulong))
					{
						column += "BIGINT UNSIGNED";
					}
					else if (systype == typeof (float))
					{
						column += "FLOAT";
					}
					else if (systype == typeof (double))
					{
						column += "DOUBLE";
					}
					else if (systype == typeof (bool))
					{
						column += "TINYINT(1)";
					}
					else if (systype == typeof (string))
					{
						if (primaryKeys.ContainsKey(table.Columns[i].ColumnName) ||
						    table.Columns[i].ExtendedProperties.ContainsKey("INDEX") ||
						    table.Columns[i].ExtendedProperties.ContainsKey("VARCHAR") ||
						    table.Columns[i].Unique)
						{
							if (table.Columns[i].ExtendedProperties.ContainsKey("VARCHAR"))
							{
								column += "VARCHAR(" + table.Columns[i].ExtendedProperties["VARCHAR"] + ")";
							}
							else
							{
								column += "VARCHAR(255)";
							}
						}
						else
						{
							column += "TEXT";
						}
					}
					else
					{
						column += "BLOB";
					}

					if (!table.Columns[i].AllowDBNull)
					{
						if(!(connType == ConnectionType.DATABASE_SQLITE && table.Columns[i].AutoIncrement))
							column += " NOT NULL";
					}
					
					if (table.Columns[i].AutoIncrement)
					{
						if (connType == ConnectionType.DATABASE_SQLITE)
						{
							column += " PRIMARY KEY AUTOINCREMENT";
						}
						else
						{
							column += " AUTO_INCREMENT";
						}
					}
					columnDefs.Add(column);

					// if the column doesnt exist but the table, then alter table
					if (currentTableColumns.Count > 0 && !currentTableColumns.Contains(table.Columns[i].ColumnName.ToLower()))
					{
						log.Debug("added for alteration " + table.Columns[i].ColumnName.ToLower());
						
						// if this column is added for alteration and is a primary key, we must switch key.
						if (table.Columns[i].AutoIncrement)
						{
							alterPrimaryKey = true;
							column += " PRIMARY KEY";
						}

						// Column def, without index or anything else.
						alterAddColumnDefs.Add(column);
					}
				}

				string columndef = string.Join(", ", columnDefs.ToArray());
				List<string> followingQueries = new List<string>();

				// create primary keys
				if (table.PrimaryKey.Length > 0)
				{
					bool hasAutoInc = false;
					
					foreach (DataColumn col in table.PrimaryKey)
					{
						if (col.AutoIncrement)
						{
							hasAutoInc = true;
							break;
						}
					}

					
					if ((connType == ConnectionType.DATABASE_SQLITE) && hasAutoInc)
					{
						columndef += ", UNIQUE (";
					}
					else
					{
						columndef += ", PRIMARY KEY (";
					}

					bool first = true;
					for (int i = 0; i < table.PrimaryKey.Length; i++)
					{
						if (!first)
						{
							columndef += ", ";
						}
						else
						{
							first = false;
						}
						columndef += "`" + table.PrimaryKey[i].ColumnName + "`";
					}
					columndef += ")";
				}

				// unique indexes				
				for (int i = 0; i < table.Columns.Count; i++)
				{
					if (table.Columns[i].Unique && !primaryKeys.ContainsKey(table.Columns[i].ColumnName))
					{
						if (connType == ConnectionType.DATABASE_SQLITE)
						{
							followingQueries.Add("CREATE UNIQUE INDEX IF NOT EXISTS \"" + table.TableName + "." + table.Columns[i].ColumnName + "\" ON \"" + table.TableName + "\"(\"" + table.Columns[i].ColumnName + "\")");
						}
						else
						{
							columndef += ", UNIQUE INDEX (`" + table.Columns[i].ColumnName + "`)";
						}
					}
				}

				// indexes
				for (int i = 0; i < table.Columns.Count; i++)
				{
					if (table.Columns[i].ExtendedProperties.ContainsKey("INDEX")
						&& !primaryKeys.ContainsKey(table.Columns[i].ColumnName)
					    && !table.Columns[i].Unique)
					{
						if (connType == ConnectionType.DATABASE_SQLITE)
						{
							string indexQuery = "CREATE INDEX IF NOT EXISTS \"" + table.TableName + "." + table.Columns[i].ColumnName + "\" ON \"" + table.TableName + "\"(\"" + table.Columns[i].ColumnName + "\"";
							
							if (table.Columns[i].ExtendedProperties.ContainsKey("INDEXCOLUMNS"))
							{
								indexQuery += ", " + table.Columns[i].ExtendedProperties["INDEXCOLUMNS"];
							}
							
							indexQuery += ")";
							
							followingQueries.Add(indexQuery);
						}
						else
						{
							columndef += ", INDEX (`" + table.Columns[i].ColumnName + "`";
	
							if (table.Columns[i].ExtendedProperties.ContainsKey("INDEXCOLUMNS"))
							{
								columndef += ", " + table.Columns[i].ExtendedProperties["INDEXCOLUMNS"];
							}
	
							columndef += ")";
						}
					}
				}

				sb.Append("CREATE TABLE IF NOT EXISTS `" + table.TableName + "` (" + columndef + ")");

				try
				{
					if (log.IsDebugEnabled)
						log.Debug(sb.ToString());

					ExecuteNonQuery(sb.ToString());

					if (followingQueries.Count > 0)
						foreach (string folqr in followingQueries)
							ExecuteNonQuery(folqr);

				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("Error while creating table " + table.TableName, e);
				}


				// alter table if needed
				
				// alter primary key, only work for migration to Auto Inc for now
				if (alterPrimaryKey)
				{
					string alterTable = "ALTER TABLE `" + table.TableName + "` DROP PRIMARY KEY";
					
					try
					{
						log.Warn("Altering table " + table.TableName);
						if (log.IsDebugEnabled)
						{
							log.Debug(alterTable);
						}
						ExecuteNonQuery(alterTable);
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.ErrorFormat("Error while altering table table {0} : {1}\n{2}", table.TableName, alterTable, e);
					}
				}

				
				if (alterAddColumnDefs.Count > 0)
				{
					columndef = string.Join(", ", alterAddColumnDefs.ToArray());
					string alterTable = "ALTER TABLE `" + table.TableName + "` ADD (" + columndef + ")";
					
					try
					{
						log.Warn("Altering table " + table.TableName);
						if (log.IsDebugEnabled)
						{
							log.Debug(alterTable);
						}
						ExecuteNonQuery(alterTable);
					}
					catch (Exception e)
					{
						if (log.IsErrorEnabled)
							log.ErrorFormat("Error while altering table table {0} : {1}\n{2}", table.TableName, alterTable, e);
					}
				}
			}
		}

		/// <summary>
		/// Gets the format for date times
		/// </summary>
		/// <returns></returns>
		public string GetDBDateFormat()
		{
			switch (connType)
			{
				case ConnectionType.DATABASE_MYSQL:
				case ConnectionType.DATABASE_SQLITE:
					return "yyyy-MM-dd HH:mm:ss";
			}

			return "yyyy-MM-dd HH:mm:ss";
		}

		/// <summary>
		/// Load an Dataset with the a Table
		/// </summary>
		/// <param name="tableName">Name of the Table to Load in the DataSet</param>
		/// <param name="dataSet">DataSet that sould be filled</param>
		/// <exception cref="DatabaseException"></exception>
		public void LoadDataSet(string tableName, DataSet dataSet)
		{
			dataSet.Clear();
			switch (connType)
			{
				case ConnectionType.DATABASE_MSSQL:
					{
						try
						{
							var conn = new SqlConnection(connString);
							var adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);

							adapter.Fill(dataSet.Tables[tableName]);
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not load the Database-Table", ex);
						}

						break;
					}
				case ConnectionType.DATABASE_ODBC:
					{
						try
						{
							var conn = new OdbcConnection(connString);
							var adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);

							adapter.Fill(dataSet.Tables[tableName]);
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not load the Database-Table", ex);
						}

						break;
					}
				case ConnectionType.DATABASE_OLEDB:
					{
						try
						{
							var conn = new OleDbConnection(connString);
							var adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);

							adapter.Fill(dataSet.Tables[tableName]);
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not load the Database-Table", ex);
						}
						break;
					}
				case ConnectionType.DATABASE_XML:
					{
						try
						{
							dataSet.Tables[tableName].ReadXml(connString + tableName + ".xml");
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not load the Database-Table", ex);
						}
						break;

					}
			}
		}

		/// <summary>
		/// Writes all Changes in a Dataset to the Table
		/// </summary>
		/// <param name="tableName">Name of the Table to update</param>
		/// <param name="dataSet">DataSet set contains the Changes that sould be written</param>
		/// <exception cref="DatabaseException"></exception>
		public void SaveDataSet(string tableName, DataSet dataSet)
		{
			if (dataSet.HasChanges() == false)
				return;

			switch (connType)
			{
				case ConnectionType.DATABASE_XML:
					{
						try
						{
							dataSet.WriteXml(connString + tableName + ".xml");
							dataSet.AcceptChanges();
							dataSet.WriteXmlSchema(connString + tableName + ".xsd");
						}
						catch (Exception e)
						{
							throw new DatabaseException("Could not save Databases in XML-Files!", e);
						}

						break;
					}
				case ConnectionType.DATABASE_MSSQL:
					{
						try
						{
							var conn = new SqlConnection(connString);
							var adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);
							var builder = new SqlCommandBuilder(adapter);

							adapter.DeleteCommand = builder.GetDeleteCommand();
							adapter.UpdateCommand = builder.GetUpdateCommand();
							adapter.InsertCommand = builder.GetInsertCommand();

							lock (dataSet) // lock dataset to prevent changes to it
							{
								adapter.ContinueUpdateOnError = true;
								DataSet changes = dataSet.GetChanges();
								adapter.Update(changes, tableName);
								PrintDatasetErrors(changes);
								dataSet.AcceptChanges();
							}

							conn.Close();
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not save the Table " + tableName, ex);
						}

						break;
					}
				case ConnectionType.DATABASE_ODBC:
					{
						try
						{
							var conn = new OdbcConnection(connString);
							var adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);
							var builder = new OdbcCommandBuilder(adapter);

							adapter.DeleteCommand = builder.GetDeleteCommand();
							adapter.UpdateCommand = builder.GetUpdateCommand();
							adapter.InsertCommand = builder.GetInsertCommand();

							DataSet changes;
							lock (dataSet) // lock dataset to prevent changes to it
							{
								adapter.ContinueUpdateOnError = true;
								changes = dataSet.GetChanges();
								adapter.Update(changes, tableName);
								dataSet.AcceptChanges();
							}

							PrintDatasetErrors(changes);

							conn.Close();
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not save the Database-Table", ex);
						}

						break;
					}
				case ConnectionType.DATABASE_MYSQL:
					{
						return; // not needed anymore
					}
				case ConnectionType.DATABASE_SQLITE:
					{
						return; // not needed anymore
					}
				case ConnectionType.DATABASE_OLEDB:
					{
						try
						{
							var conn = new OleDbConnection(connString);
							var adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);
							var builder = new OleDbCommandBuilder(adapter);

							adapter.DeleteCommand = builder.GetDeleteCommand();
							adapter.UpdateCommand = builder.GetUpdateCommand();
							adapter.InsertCommand = builder.GetInsertCommand();

							DataSet changes;
							lock (dataSet) // lock dataset to prevent changes to it
							{
								adapter.ContinueUpdateOnError = true;
								changes = dataSet.GetChanges();
								adapter.Update(changes, tableName);
								dataSet.AcceptChanges();
							}

							PrintDatasetErrors(changes);

							conn.Close();
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not save the Database-Table", ex);
						}
						break;
					}
			}
		}

		/// <summary>
		/// Print the dataset error
		/// </summary>
		/// <param name="dataset">the dataset to check</param>
		public void PrintDatasetErrors(DataSet dataset)
		{
			if (dataset.HasErrors)
			{
				foreach (DataTable table in dataset.Tables)
				{
					if (table.HasErrors)
					{
						foreach (DataRow row in table.Rows)
						{
							if (row.HasErrors && row.RowState == DataRowState.Deleted)
							{
								if (log.IsErrorEnabled)
								{
									log.Error("Error deleting row in table " + table.TableName + ": " + row.RowError);

									var sb = new StringBuilder();
									foreach (DataColumn col in table.Columns)
									{
										sb.Append(col.ColumnName + "=" + row[col, DataRowVersion.Original] + " ");
									}

									log.Error(sb.ToString());
								}
							}
							else if (row.HasErrors)
							{
								if (log.IsErrorEnabled)
								{
									log.Error("Error updating table " + table.TableName + ": " + row.RowError + row.GetColumnsInError());

									var sb = new StringBuilder();
									foreach (DataColumn col in table.Columns)
									{
										sb.Append(col.ColumnName + "=" + row[col] + " ");
									}

									log.Error(sb.ToString());

									sb = new StringBuilder("Affected columns: ");
									foreach (DataColumn col in row.GetColumnsInError())
									{
										sb.Append(col.ColumnName + " ");
									}

									log.Error(sb.ToString());
								}
							}
						}
					}
				}
			}
		}
	}
}