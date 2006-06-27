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
using System.Data;
using System.Data.Odbc;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using log4net;
using MySql.Data.MySqlClient;

namespace DOL.Database.Connection
{
	/// <summary>
	/// Class for Handling the Connection to the ADO.Net Layer of the Databases.
	/// Funktions for loading and storing the complete Dataset are in there.
	/// </summary>
	public class DataConnection
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		private string connString;
		private ConnectionType connType;

		/// <summary>
		/// Constructor to set up a Database
		/// </summary>
		/// <param name="connType">Connection-Type the Database should use</param>
		/// <param name="connString">Connection-String to indicate the Parameters of the Datasource.
		///     XML = Directory where the XML-Files sould be stored
		///     MYSQL = ADO.NET ConnectionString 
		///     MSSQL = ADO.NET ConnectionString 
		///     OLEDB = ADO.NET ConnectionString 
		///     ODBC = ADO.NET ConnectionString 
		/// </param>
		public DataConnection(ConnectionType connType, string connString)
		{
			this.connType = connType;
			this.connString = connString;

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
		}

		/// <summary>
		/// Check if SQL connection
		/// </summary>
		public bool IsSQLConnection
		{
			get { return connType == ConnectionType.DATABASE_MYSQL; }
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
				s = s.Replace("'","''");
			}
			else
			{
				s = s.Replace("'", "\\'");
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
				if (log.IsDebugEnabled) {
					log.Debug("SQL: "+sqlcommand);
				}
				MySqlConnection conn = new MySqlConnection(connString);
				MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);
				long start1 = Environment.TickCount;
				conn.Open();
				if (Environment.TickCount - start1 > 1000)
				{
					if(log.IsWarnEnabled)
						log.Warn("Gaining SQL connection took " + (Environment.TickCount - start1) + "ms");
				}

				int affected = 0;
				try
				{
					long start = Environment.TickCount;
					affected = cmd.ExecuteNonQuery();
					
					if(log.IsDebugEnabled)
						log.Debug("SQL NonQuery exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL NonQuery took " + (Environment.TickCount - start1) + "ms!\n"+sqlcommand);

					conn.Close();
				}
				catch (Exception e)
				{
					conn.Close();
					throw e;
				}
				return affected;
			}
			if(log.IsWarnEnabled)
				log.Warn("SQL NonQuery's not supported for this connection type");
			return 0;
		}

		/// <summary>
		/// Execute select on sql database
		/// Close returned datareader when done or use using(reader) { ... }
		/// </summary>
		/// <param name="sqlcommand"></param>
		/// <returns></returns>
		public IDataReader ExecuteSelect(string sqlcommand)
		{
			if (connType == ConnectionType.DATABASE_MYSQL)
			{
				if (log.IsDebugEnabled) {
					log.Debug("SQL: "+sqlcommand);
				}
				MySqlConnection conn = new MySqlConnection(connString);
				MySqlCommand cmd = new MySqlCommand(sqlcommand, conn);
				long start1 = Environment.TickCount;
				conn.Open();
				if (Environment.TickCount - start1 > 1000)
				{
					if(log.IsWarnEnabled)
						log.Warn("Gaining SQL connection took " + (Environment.TickCount - start1) + "ms");
				}
				try
				{
					long start = Environment.TickCount;
					MySqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection);
					if(log.IsDebugEnabled)
						log.Debug("SQL Select exec time " + (Environment.TickCount - start) + "ms");
					else if (Environment.TickCount - start > 500 && log.IsWarnEnabled)
						log.Warn("SQL Select took " + (Environment.TickCount - start1) + "ms!\n"+sqlcommand);
					return reader;
				}
				catch (Exception e)
				{
					if (log.IsErrorEnabled)
						log.Error("ExecuteSelect: \""+sqlcommand+"\"\n", e);
					conn.Close();
					throw e;
				}
			}
			if(log.IsWarnEnabled)
				log.Warn("SQL Selects not supported for this connection type");
			return null;
		}

		/// <summary>
		/// Create the table in mysql
		/// </summary>
		/// <param name="table">the table to create</param>
		public void CheckOrCreateTable(DataTable table)
		{
			if (connType == ConnectionType.DATABASE_MYSQL)
			{
				StringBuilder sb = new StringBuilder();
				Hashtable primaryKeys = new Hashtable();
				for (int i = 0; i < table.PrimaryKey.Length; i++)
				{
					primaryKeys[table.PrimaryKey[i].ColumnName] = table.PrimaryKey[i];
				}
				bool first = true;
				string columndef = "";
				for (int i = 0; i < table.Columns.Count; i++)
				{
					Type systype = table.Columns[i].DataType;

					if (!first)
					{
						columndef += ", ";
					}
					else
					{
						first = false;
					}

					columndef += table.Columns[i].ColumnName + " ";
					if (systype == typeof (System.Char))
					{
						columndef += "SMALLINT UNSIGNED";
					}
					else if (systype == typeof (DateTime))
					{
						columndef += "DATETIME";
					}
					else if (systype == typeof (System.SByte))
					{
						columndef += "TINYINT";
					}
					else if (systype == typeof (System.Int16))
					{
						columndef += "SMALLINT";
					}
					else if (systype == typeof (System.Int32))
					{
						columndef += "INT";
					}
					else if (systype == typeof (System.Int64))
					{
						columndef += "BIGINT";
					}
					else if (systype == typeof (System.Byte))
					{
						columndef += "TINYINT UNSIGNED";
					}
					else if (systype == typeof (System.UInt16))
					{
						columndef += "SMALLINT UNSIGNED";
					}
					else if (systype == typeof (System.UInt32))
					{
						columndef += "INT UNSIGNED";
					}
					else if (systype == typeof (System.UInt64))
					{
						columndef += "BIGINT UNSIGNED";
					}
					else if (systype == typeof (System.Single))
					{
						columndef += "FLOAT";
					}
					else if (systype == typeof (System.Double))
					{
						columndef += "DOUBLE";
					}
					else if (systype == typeof (System.Boolean))
					{
						columndef += "TINYINT(1)";
					}
					else if (systype == typeof (System.String))
					{
						if (primaryKeys[table.Columns[i].ColumnName] != null ||
							table.Columns[i].ExtendedProperties.ContainsKey("INDEX") ||
							table.Columns[i].Unique)
						{
							columndef += "VARCHAR(255)";
						}
						else
						{
							columndef += "TEXT";
						}
					}
					else
					{
						columndef += "BLOB";
					}
					if (!table.Columns[i].AllowDBNull)
					{
						columndef += " NOT NULL";
					}
				}


				// create primary keys
				if (table.PrimaryKey.Length > 0)
				{
					columndef += ", PRIMARY KEY (";
					first = true;
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
						columndef += table.PrimaryKey[i].ColumnName;
					}
					columndef += ")";
				}

				// unique indexes				
				for (int i = 0; i < table.Columns.Count; i++)
				{
					if (table.Columns[i].Unique && primaryKeys[table.Columns[i].ColumnName] == null)
					{
						columndef += ", UNIQUE INDEX (" + table.Columns[i].ColumnName + ")";
					}
				}

				// indexes
				for (int i = 0; i < table.Columns.Count; i++)
				{
					if (table.Columns[i].ExtendedProperties.ContainsKey("INDEX")
						&& primaryKeys[table.Columns[i].ColumnName] == null
						&& !table.Columns[i].Unique)
					{
						columndef += ", INDEX (" + table.Columns[i].ColumnName + ")";
					}
				}
				sb.Append("CREATE TABLE IF NOT EXISTS " + table.TableName + " (" + columndef + ")");

				try
				{
					if(log.IsDebugEnabled)
						log.Debug(sb.ToString());
					ExecuteNonQuery(sb.ToString());
				}
				catch (Exception e)
				{
					if(log.IsErrorEnabled)
						log.Error("Error while creating table " + table.TableName,e);
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
				case ConnectionType.DATABASE_XML:
					{
						string filename = connString + tableName + ".xml";
						try
						{
							dataSet.ReadXmlSchema(connString + tableName + ".xsd");
							dataSet.ReadXml(filename, XmlReadMode.IgnoreSchema);
							dataSet.AcceptChanges();
						}
						catch (System.IO.FileNotFoundException)
						{
							try
							{
								dataSet.WriteXml(filename);
								dataSet.WriteXmlSchema(connString + tableName + ".xsd");
							}
							catch (Exception ex)
							{
								throw new DatabaseException("Could not create XML-Databasefiles (Directory present ?)", ex);
							}
						}
						catch (System.Data.ConstraintException e)
						{
							if(log.IsErrorEnabled)
								log.Error("At least one item in the table \"" + tableName + "\" violated a constraint (non-null, unique or foreign-key)!",e);
							throw e;
						}
						break;
					}
				case ConnectionType.DATABASE_MSSQL:
					{
						try
						{
							SqlConnection conn = new SqlConnection(connString);
							SqlDataAdapter adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);

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
							OdbcConnection conn = new OdbcConnection(connString);
							OdbcDataAdapter adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);

							adapter.Fill(dataSet.Tables[tableName]);
						}
						catch (Exception ex)
						{
							throw new DatabaseException("Could not load the Database-Table", ex);
						}
						break;
					}
				case ConnectionType.DATABASE_MYSQL:
					{
						return; // not needed anymore
						/*try
					{
						DOLConsole.WriteLine("Loading table "+tableName);
						MySqlConnection conn = new MySqlConnection(connString);
						MySqlDataAdapter adapter = new MySqlDataAdapter("SELECT * from `" + tableName + "`", conn);

						adapter.Fill(dataSet.Tables[tableName]);
					}
					catch (Exception ex)
					{
						throw new DatabaseException("Could not load the Database-Table", ex);
					}
					break;*/
					}
				case ConnectionType.DATABASE_OLEDB:
					{
						try
						{
							OleDbConnection conn = new OleDbConnection(connString);
							OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);

							adapter.Fill(dataSet.Tables[tableName]);
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
							SqlConnection conn = new SqlConnection(connString);
							SqlDataAdapter adapter = new SqlDataAdapter("SELECT * from " + tableName, conn);
							SqlCommandBuilder builder = new SqlCommandBuilder(adapter);

							adapter.DeleteCommand = builder.GetDeleteCommand();
							adapter.UpdateCommand = builder.GetUpdateCommand();
							adapter.InsertCommand = builder.GetInsertCommand();

							DataSet changes;
							lock (dataSet) // lock dataset to prevent changes to it
							{
								adapter.ContinueUpdateOnError = true;
								changes = dataSet.GetChanges();
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
							OdbcConnection conn = new OdbcConnection(connString);
							OdbcDataAdapter adapter = new OdbcDataAdapter("SELECT * from " + tableName, conn);
							OdbcCommandBuilder builder = new OdbcCommandBuilder(adapter);

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
/*
					MySqlConnection conn = null;
					try
					{
						DOLConsole.LogLine("write "+tableName);
						conn = new MySqlConnection(connString);

						DOLConsole.LogLine("open connection "+tableName);
						conn.Open();
						MySqlDataAdapter adapter = m_mysqladapter[tableName] as MySqlDataAdapter;
						if (adapter == null) 	// only create if previous not exist, saves time
						{
							DOLConsole.LogLine("build adapter "+tableName);
							adapter = new MySqlDataAdapter("SELECT * from `" + tableName + "`", conn);
							DOLConsole.LogLine("build commandbuilder "+tableName);
							MySqlCommandBuilder builder = new MySqlCommandBuilder(adapter, true);	// last one wins
							DOLConsole.LogLine("create delete command "+tableName);
							adapter.DeleteCommand = builder.GetDeleteCommand();
							DOLConsole.LogLine("create update command "+tableName);
							adapter.UpdateCommand = builder.GetUpdateCommand();
							DOLConsole.LogLine("create insert command "+tableName);
							adapter.InsertCommand = builder.GetInsertCommand();
							m_mysqladapter[tableName] = adapter;
						}

						DOLConsole.LogLine("commit changes "+tableName);
						DataSet changes;
						// last one wins means we dont have to bother with concurrency modification
						//lock (dataSet)  // lock dataset to prevent changes to it
						//{ 
							adapter.ContinueUpdateOnError = true;
							DOLConsole.LogLine("get changes "+tableName);
							changes = dataSet.GetChanges();
							DOLConsole.LogLine(changes.Tables[tableName].Rows.Count+" "+tableName+" to commit");
							DOLConsole.LogLine("commit changes"+tableName);
							int count = adapter.Update(changes, tableName);
							DOLConsole.LogLine(count+" changes committed "+tableName);
							DOLConsole.LogLine("accept changes "+tableName);
							dataSet.AcceptChanges(); 
							DOLConsole.LogLine("changes accepted "+tableName);
						//}
						DOLConsole.LogLine("changes complete "+tableName);
						PrintDatasetErrors(changes);
						conn.Close();
					}
					catch (Exception ex)
					{
						try { if (conn!=null) conn.Close(); } catch {}
						throw new DatabaseException("Could not save the Database-Table", ex);
					}
					

					break;*/
					}
				case ConnectionType.DATABASE_OLEDB:
					{
						try
						{
							OleDbConnection conn = new OleDbConnection(connString);
							OleDbDataAdapter adapter = new OleDbDataAdapter("SELECT * from " + tableName, conn);
							OleDbCommandBuilder builder = new OleDbCommandBuilder(adapter);

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
								if(log.IsErrorEnabled)
								{
									log.Error("Error deleting row in table " + table.TableName + ": " + row.RowError);
									StringBuilder sb = new StringBuilder();
									foreach (DataColumn col in table.Columns)
									{
										sb.Append(col.ColumnName + "=" + row[col, DataRowVersion.Original] + " ");
									}
									log.Error(sb.ToString());
								}
							}
							else if (row.HasErrors)
							{
								if(log.IsErrorEnabled)
								{
									log.Error("Error updating table " + table.TableName + ": " + row.RowError + row.GetColumnsInError());
									StringBuilder sb = new StringBuilder();
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