// Copyright (C) 2004-2005 MySQL AB
//
// This program is free software; you can redistribute it and/or modify
// it under the terms of the GNU General Public License version 2 as published by
// the Free Software Foundation
//
// There are special exceptions to the terms and conditions of the GPL 
// as it is applied to this software. View the full text of the 
// exception in file EXCEPTIONS in the directory of this software 
// distribution.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA 

using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <include file='docs/MySqlCommandBuilder.xml' path='docs/class/*'/>
	[ToolboxItem(false)]
	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlCommandBuilder : Component
	{
		private MySqlDataAdapter	_adapter;
		private string				_QuotePrefix;
		private string				_QuoteSuffix;
		private DataTable			_schema;
		private string				tableName;
		private string				schemaName;

		private	MySqlCommand		_updateCmd;
		private MySqlCommand		_insertCmd;
		private MySqlCommand		_deleteCmd;

		private char				marker = '?';
		private bool				lastOneWins;

		#region Constructors

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor/*'/>
		public MySqlCommandBuilder()
		{
			_QuotePrefix = _QuoteSuffix = "`";
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor1/*'/>
		public MySqlCommandBuilder(bool lastOneWins) : this()
		{
			this.lastOneWins = lastOneWins;
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor2/*'/>
		public MySqlCommandBuilder(MySqlDataAdapter adapter) : this()
		{
			DataAdapter = adapter;
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/Ctor3/*'/>
		public MySqlCommandBuilder(MySqlDataAdapter adapter, bool lastOneWins) : this(lastOneWins)
		{
			DataAdapter = adapter;
		}

		#endregion

		#region Properties

		/// <include file='docs/mysqlcommandBuilder.xml' path='docs/DataAdapter/*'/>
		public MySqlDataAdapter DataAdapter 
		{
			get { return _adapter; }
			set 
			{ 
				if (_adapter != null) 
				{
					_adapter.RowUpdating -= new MySqlRowUpdatingEventHandler(OnRowUpdating);
				}
				if (value == null)
					throw new ArgumentException(Resources.GetString("ParameterCannotBeNull"), "value");
				_adapter = value;
				_adapter.RowUpdating += new MySqlRowUpdatingEventHandler(OnRowUpdating);
			}
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/QuotePrefix/*'/>
		public string QuotePrefix 
		{
			get { return _QuotePrefix; }
			set { _QuotePrefix = value; }
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/QuoteSuffix/*'/>
		public string QuoteSuffix
		{
			get { return _QuoteSuffix; }
			set { _QuoteSuffix = value; }
		}

		private string TableName 
		{
			get 
			{
				if (schemaName != null && schemaName.Length > 0)
					return Quote(schemaName) + "." + Quote(tableName);
				return Quote(tableName);
			}
		}

		#endregion

		#region Public Methods
		/// <summary>
		/// Retrieves parameter information from the stored procedure specified in the MySqlCommand and populates the Parameters collection of the specified MySqlCommand object.
		/// This method is not currently supported since stored procedures are not available in MySql.
		/// </summary>
		/// <param name="command">The MySqlCommand referencing the stored procedure from which the parameter information is to be derived. The derived parameters are added to the Parameters collection of the MySqlCommand.</param>
		/// <exception cref="InvalidOperationException">The command text is not a valid stored procedure name.</exception>
		public static void DeriveParameters(MySqlCommand command)
		{
			// this is just to make FxCop happy until we support this routine
			string text = command.CommandText;
			throw new MySqlException("DeriveParameters is not supported (due to MySql not supporting SP)");
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/GetDeleteCommand/*'/>
		public MySqlCommand GetDeleteCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateDeleteCommand();
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/GetInsertCommand/*'/>
		public MySqlCommand GetInsertCommand()
		{
			if (_schema == null)
				GenerateSchema();
			return CreateInsertCommand();
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/GetUpdateCommand/*'/>
		public MySqlCommand GetUpdateCommand() 
		{
			if (_schema == null)
				GenerateSchema();
			return CreateUpdateCommand();
		}

		/// <include file='docs/MySqlCommandBuilder.xml' path='docs/RefreshSchema/*'/>
		public void RefreshSchema()
		{
			_schema = null;
			_insertCmd = null;
			_deleteCmd = null;
			_updateCmd = null;
			tableName = null;
			schemaName = null;
		}
		#endregion

		#region Private Methods

		private void GenerateSchema()
		{
			// set the parameter marker
			MySqlConnection conn = (MySqlConnection)_adapter.SelectCommand.Connection;
			marker = conn.ParameterMarker;

			if (_adapter == null)
				throw new MySqlException(Resources.GetString("AdapterIsNull"));
			if (_adapter.SelectCommand == null)
				throw new MySqlException(Resources.GetString("AdapterSelectIsNull"));

			MySqlDataReader dr = _adapter.SelectCommand.ExecuteReader(CommandBehavior.SchemaOnly | CommandBehavior.KeyInfo);
			_schema = dr.GetSchemaTable();
			dr.Close();

			// make sure we got at least one unique or key field and count base table names
			bool   hasKeyOrUnique=false;

			foreach (DataRow row in _schema.Rows)
			{
				string rowTableName = row["BaseTableName"].ToString();
				string rowSchemaName = row["BaseSchemaName"].ToString();

				if (true == (bool)row["IsKey"] || true == (bool)row["IsUnique"])
					hasKeyOrUnique=true;

				if (tableName == null)
				{
					schemaName = rowSchemaName;
					tableName = rowTableName;
				}
				else if (tableName != rowTableName && rowTableName.Length > 0)
					throw new InvalidOperationException(Resources.GetString("CBMultiTableNotSupported"));
				else if (schemaName != rowSchemaName && rowSchemaName.Length > 0)
					throw new InvalidOperationException(Resources.GetString("CBMultiTableNotSupported"));
			}
			if (! hasKeyOrUnique)
				throw new InvalidOperationException(Resources.GetString("CBNoKeyColumn"));
		}

		private string Quote(string table_or_column)
		{
			if (_QuotePrefix == null || _QuoteSuffix == null)
				return table_or_column;
			return _QuotePrefix + table_or_column + _QuoteSuffix;
		}

		private static string GetParameterName(string columnName)
		{
			string colName = columnName.Replace(" ", "");
			return colName;
		}

		private MySqlParameter CreateParameter(DataRow row, bool Original)
		{
			MySqlParameter p;
			string colName = GetParameterName( row["ColumnName"].ToString() );
			MySqlDbType type = (MySqlDbType)row["ProviderType"];

			if (Original)
				p = new MySqlParameter( "Original_" + colName, type, ParameterDirection.Input, 
					(string)row["ColumnName"], DataRowVersion.Original, DBNull.Value );
			else
				p = new MySqlParameter( colName, type, ParameterDirection.Input, 
					(string)row["ColumnName"], DataRowVersion.Current, DBNull.Value );
			return p;
		}

		private MySqlCommand CreateBaseCommand()
		{
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = _adapter.SelectCommand.Connection;
			cmd.CommandTimeout = _adapter.SelectCommand.CommandTimeout;
			cmd.Transaction = _adapter.SelectCommand.Transaction;
			return cmd;
		}

		private MySqlCommand CreateDeleteCommand()
		{
			if (_deleteCmd != null) return _deleteCmd;

			MySqlCommand cmd = CreateBaseCommand();

			cmd.CommandText = "DELETE FROM " + TableName + 
				" WHERE " + CreateOriginalWhere(cmd);

			_deleteCmd = cmd;
			return cmd;
		}

		private string CreateFinalSelect(bool forinsert)
		{
			StringBuilder sel = new StringBuilder();
			StringBuilder where = new StringBuilder();

			foreach (DataRow row in _schema.Rows)
			{
				// don't include functions in where clause
				string baseTableName = (string)row["BaseTableName"];
				if (baseTableName == null || baseTableName.Length == 0)
					continue;

				string colname = Quote(row["ColumnName"].ToString());
				string parmName = GetParameterName( row["ColumnName"].ToString() );

				if (sel.Length > 0)
					sel.Append(", ");
				sel.Append( colname );
				if ((bool)row["IsKey"] == false) continue;
				if (where.Length > 0)
					where.Append(" AND ");
				where.Append( "(" + colname + "=" );
				if (forinsert) 
				{
					if ((bool)row["IsAutoIncrement"])
						where.Append("last_insert_id()");
					else if ((bool)row["IsKey"])
						where.Append( marker + parmName);
				}
				else 
				{
					where.Append(marker + "Original_" + parmName);
				}
				where.Append(")");
			}
			return "SELECT " + sel.ToString() + " FROM " + TableName +
				" WHERE " + where.ToString();
		}

		private string CreateOriginalWhere(MySqlCommand cmd)
		{
			StringBuilder wherestr = new StringBuilder();

			foreach (DataRow row in _schema.Rows)
			{
				// don't include functions in where clause
				string baseTableName = (string)row["BaseTableName"];
				if (baseTableName == null || baseTableName.Length == 0)
					continue;

				// if we are doing last one wins and this column is not a key or is not
				// unique, then we don't care about it
				if (true != (bool)row["IsKey"] && true != (bool)row["IsUnique"] && lastOneWins)
					continue;

				if (! IncludedInWhereClause(row)) continue;

				// first update the where clause since it will contain all parameters
//				if (wherestr.Length > 0)
//					wherestr.Append(" AND ");
				string colname = Quote((string)row["ColumnName"]);

				MySqlParameter op = CreateParameter(row, true);
				cmd.Parameters.Add(op);

				wherestr.Append( colname + " <=> " + marker + op.ParameterName + " AND ");
//				if ((bool)row["AllowDBNull"] == true) 
//					wherestr.Append( " or (" + colname + " IS NULL and ?" + op.ParameterName + " IS NULL)");
				//wherestr.Append(")");
			}
			wherestr.Remove( wherestr.Length-5, 5 ); // remove the trailling " AND "
			return wherestr.ToString();
		}

		private MySqlCommand CreateUpdateCommand()
		{
			if (_updateCmd != null) return _updateCmd; 

			MySqlCommand cmd = CreateBaseCommand();

			StringBuilder setstr = new StringBuilder();
		
			foreach (DataRow schemaRow in _schema.Rows)
			{
				// don't include functions in where clause
				string baseTableName = (string)schemaRow["BaseTableName"];
				if (baseTableName == null || baseTableName.Length == 0)
					continue;

				string colname = Quote((string)schemaRow["ColumnName"]);

				if (! IncludedInUpdate(schemaRow)) continue;

				if (setstr.Length > 0) 
					setstr.Append(", ");

				MySqlParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname + "=" + marker + p.ParameterName );
			}

			cmd.CommandText = "UPDATE " + TableName + " SET " + setstr.ToString() + 
				" WHERE " + CreateOriginalWhere(cmd);
			cmd.CommandText += "; " + CreateFinalSelect(false);

			_updateCmd = cmd;
			return cmd;
		}

		private MySqlCommand CreateInsertCommand()
		{
			if (_insertCmd != null) return _insertCmd;

			MySqlCommand cmd = CreateBaseCommand();

			StringBuilder setstr = new StringBuilder();
			StringBuilder valstr = new StringBuilder();
			foreach (DataRow schemaRow in _schema.Rows)
			{
				// don't include functions in where clause
				string baseTableName = (string)schemaRow["BaseTableName"];
				if (baseTableName == null || baseTableName.Length == 0)
					continue;

				string colname = Quote((string)schemaRow["ColumnName"]);

				if (!IncludedInInsert(schemaRow)) continue;

				if (setstr.Length > 0) 
				{
					setstr.Append(", ");
					valstr.Append(", ");
				}

				MySqlParameter p = CreateParameter(schemaRow, false);
				cmd.Parameters.Add(p);

				setstr.Append( colname );
				valstr.Append( marker + p.ParameterName );
			}

			cmd.CommandText = "INSERT INTO " + TableName + " (" + setstr.ToString() + ") " +
				" VALUES (" + valstr.ToString() + ")";
			cmd.CommandText += "; " + CreateFinalSelect(true);

			_insertCmd = cmd;
			return cmd;
		}

		private static bool IncludedInInsert(DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:

			/*			if ((bool) schemaRow ["IsHidden"])
							return false;
						if ((bool) schemaRow ["IsExpression"])
							return false;*/
			if ((bool) schemaRow ["IsRowVersion"])
				return false;
			if ((bool) schemaRow ["IsReadOnly"])
				return false;
			return true;
		}

		private static bool IncludedInUpdate(DataRow schemaRow)
		{
			// If the parameter has one of these properties, then we don't include it in the insert:

			//			if ((bool) schemaRow ["IsHidden"])
			//				return false;
			if ((bool) schemaRow ["IsRowVersion"])
				return false;
			return true;
		}

		private static bool IncludedInWhereClause(DataRow schemaRow)
		{
			// just to shut fxcop up
			bool hasErrors = schemaRow.HasErrors;
			//			if ((bool) schemaRow ["IsLong"])
			//				return false;
			return true;
		}

		private static void SetParameterValues(MySqlCommand cmd, DataRow dataRow)
		{
			foreach (MySqlParameter p in cmd.Parameters)
			{
				if (p.SourceVersion == DataRowVersion.Original)
//				if (p.ParameterName.Length >= 8 && p.ParameterName.Substring(0, 8).Equals("Original"))
					p.Value = dataRow[p.SourceColumn, DataRowVersion.Original];
				else
					p.Value = dataRow[p.SourceColumn, DataRowVersion.Current];
			}
		}

		private void OnRowUpdating(object sender, MySqlRowUpdatingEventArgs args)
		{
			// make sure we are still to proceed
			if (args.Status != UpdateStatus.Continue) return;

			if (_schema == null)
				GenerateSchema();

			if (StatementType.Delete == args.StatementType)
				args.Command = CreateDeleteCommand();
			else if (StatementType.Update == args.StatementType)
				args.Command = CreateUpdateCommand();
			else if (StatementType.Insert == args.StatementType)
				args.Command = CreateInsertCommand();
			else if (StatementType.Select == args.StatementType)
				return;

			SetParameterValues(args.Command, args.Row);
		}
		#endregion

	}
}
