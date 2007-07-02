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
using System.Data;
using System.ComponentModel;
using System.IO;
using System.Collections;
using System.Text;
using MySql.Data.Common;

//VaNaTiC->disabling warning "cref-attribut: ..."
#pragma warning disable 419
//VaNaTiC<-

namespace MySql.Data.MySqlClient
{
	/// <include file='docs/mysqlcommand.xml' path='docs/ClassSummary/*'/>
#if WINDOWS
	[System.Drawing.ToolboxBitmap( typeof(MySqlCommand), "MySqlClient.resources.command.bmp")]
#endif
	[System.ComponentModel.DesignerCategory("Code")]
	public sealed class MySqlCommand : Component, IDbCommand, ICloneable
	{
		MySqlConnection				connection;
		MySqlTransaction			curTransaction;
		string						cmdText;
		CommandType					cmdType;
		long						updateCount;
		UpdateRowSource				updatedRowSource;
		MySqlParameterCollection	parameters;
		private ArrayList			sqlBuffers;
		private PreparedStatement	preparedStatement;
		private ArrayList			parameterMap;
		private StoredProcedure		storedProcedure;
		private CommandResult		lastResult;

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor1/*'/>
		public MySqlCommand()
		{
			cmdType = CommandType.Text;
			parameterMap = new ArrayList();
			parameters = new MySqlParameterCollection();
			updatedRowSource = UpdateRowSource.Both;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor2/*'/>
		public MySqlCommand(string cmdText) : this()
		{
			CommandText = cmdText;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor3/*'/>
		public MySqlCommand(string cmdText, MySqlConnection connection) : this(cmdText)
		{
			Connection = connection;
			if (connection != null)
				parameters.ParameterMarker = connection.ParameterMarker;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ctor4/*'/>
		public MySqlCommand(string cmdText, MySqlConnection connection, 
			MySqlTransaction transaction) : this(cmdText, connection)
		{
			curTransaction = transaction;
		} 

		#region Properties

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandText/*'/>
		[Category("Data")]
		[Description("Command text to execute")]
#if WINDOWS
		[Editor("MySql.Data.Common.Design.SqlCommandTextEditor,MySqlClient.Design", typeof(System.Drawing.Design.UITypeEditor))]
#endif
		public string CommandText
		{
			get { return cmdText; }
			set { cmdText = value;  this.preparedStatement=null; }
		}

		internal int UpdateCount 
		{
			get { return (int)updateCount; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandTimeout/*'/>
		[Category("Misc")]
		[Description("Time to wait for command to execute")]
		public int CommandTimeout
		{
			// TODO: support this
			get  { return 0; }
			set  { if (value != 0) throw new NotSupportedException(); }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/CommandType/*'/>
		[Category("Data")]
		public CommandType CommandType
		{
			get { return cmdType; }
			set { cmdType = value; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/IsPrepared/*'/>
		[Browsable(false)]
		public bool IsPrepared 
		{
			get { return preparedStatement != null; }
		}

		IDbConnection IDbCommand.Connection 
		{
			get { return connection; }
			set { Connection = (MySqlConnection)value; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Connection/*'/>
		[Category("Behavior")]
		[Description("Connection used by the command")]
		public MySqlConnection Connection
		{
			get { return connection;  }
			set
			{
				/*
				* The connection is associated with the transaction
				* so set the transaction object to return a null reference if the connection 
				* is reset.
				*/
				if (connection != value)
					this.Transaction = null;

				connection = (MySqlConnection)value;
				if (connection != null)
					parameters.ParameterMarker = connection.ParameterMarker;
			}
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Parameters/*'/>
		[Category("Data")]
		[Description("The parameters collection")]
		[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
		public MySqlParameterCollection Parameters
		{
			get  { return parameters; }
		}

		IDataParameterCollection IDbCommand.Parameters
		{
			get  { return parameters; }
		}

		IDbTransaction IDbCommand.Transaction 
		{
			get { return Transaction; }
			set { Transaction = (MySqlTransaction)value; }
		}


		/// <include file='docs/mysqlcommand.xml' path='docs/Transaction/*'/>
		[Browsable(false)]
		public MySqlTransaction Transaction
		{
			get { return curTransaction; }
			set { curTransaction = (MySqlTransaction)value; }
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/UpdatedRowSource/*'/>
		[Category("Behavior")]
		public UpdateRowSource UpdatedRowSource
		{
			get 
			{ 
				return updatedRowSource;  
			}
			set 
			{ 
				updatedRowSource = value; 
			}
		}
		#endregion

		#region Methods

		/// <summary>
		/// Attempts to cancel the execution of a MySqlCommand.  This operation is not supported.
		/// </summary>
		/// <remarks>
		/// Cancelling an executing command is currently not supported on any version of MySQL.
		/// </remarks>
		/// <exception cref="NotSupportedException">This operation is not supported.</exception>
		public void Cancel()
		{
			throw new NotSupportedException();
		}

		/// <summary>
		/// Creates a new instance of a <see cref="MySqlParameter"/> object.
		/// </summary>
		/// <remarks>
		/// This method is a strongly-typed version of <see cref="IDbCommand.CreateParameter"/>.
		/// </remarks>
		/// <returns>A <see cref="MySqlParameter"/> object.</returns>
		/// 
		public MySqlParameter CreateParameter()
		{
			return new MySqlParameter();
		}

		IDbDataParameter IDbCommand.CreateParameter()
		{
			return this.CreateParameter();
		}

		/// <summary>
		/// Executes all remaining command buffers
		/// </summary>
		internal void Consume()
		{
			CommandResult result = GetNextResultSet(null);
			while (result != null)
			{
				result.Consume();
				result = GetNextResultSet(null);
			}

			// if we were executing a stored procedure and we are out of sql buffers to execute, 
			// then we need to perform some additional work to get our inout and out parameters
			if (storedProcedure != null && sqlBuffers.Count == 0)
				storedProcedure.UpdateParameters(Parameters);
		}

		/// <summary>
		/// Executes command buffers until we hit the next resultset
		/// </summary>
		/// <returns>CommandResult containing the next resultset when hit
		/// or null if no more resultsets were found</returns>
		internal CommandResult GetNextResultSet(MySqlDataReader reader)
		{
			// if  we are supposed to return only a single resultset and our reader
			// is calling us back again, then return null
			if (reader != null && 
				(reader.Behavior & CommandBehavior.SingleResult) != 0 &&
				lastResult != null) return null;

			// if the last result we returned has more results
			if (lastResult != null && lastResult.ReadNextResult(false) )
				return lastResult;
			lastResult = null;

			CommandResult result = null;

			// if we haven't prepared a statement and don't have any sql buffers
			// to execute, we are done
			if (preparedStatement == null)
			{
				if (sqlBuffers == null || sqlBuffers.Count == 0)
					return null;
			}
			else if (preparedStatement.ExecutionCount > 0)
				return null;


			// if we have a prepared statement, we execute it instead
			if (preparedStatement != null)
			{
				result = preparedStatement.Execute( parameters );

				if (! result.IsResultSet) 
				{
					if (updateCount == -1) updateCount = 0;
					updateCount += (long)result.AffectedRows;
				}
			}
			else while (sqlBuffers.Count > 0)
			{
				MemoryStream sqlStream = (MemoryStream)sqlBuffers[0];

				using (sqlStream) 
				{
					result = connection.driver.SendQuery( sqlStream.GetBuffer(), (int)sqlStream.Length, false );
					sqlBuffers.RemoveAt( 0 );
				}
	
				if (result.AffectedRows != -1)
				{
					if (updateCount == -1) updateCount = 0;
					updateCount += (long)result.AffectedRows;
				}

				// if this is a resultset, then we break out of our execution loop
				if (result.IsResultSet)
					break;
			}

			if (result.IsResultSet) 
			{
				lastResult = result;
				return result;
			}
			return null;
		}

		/// <summary>
		/// Check the connection to make sure
		///		- it is open
		///		- it is not currently being used by a reader
		///		- and we have the right version of MySQL for the requested command type
		/// </summary>
		private void CheckState()
		{
			// There must be a valid and open connection.
			if (connection == null || connection.State != ConnectionState.Open)
				throw new InvalidOperationException(Resources.GetString("ConnectionMustBeOpen"));

			// Data readers have to be closed first
			if (connection.Reader != null)
				throw new MySqlException(Resources.GetString("DataReaderOpen"));

			if (CommandType == CommandType.StoredProcedure && ! connection.driver.Version.isAtLeast(5,0,0))
				throw new MySqlException(Resources.GetString("SPNotSupported"));
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteNonQuery/*'/>
		public int ExecuteNonQuery()
		{
			CheckState();

			updateCount = 0;

			if (preparedStatement == null)
				sqlBuffers = PrepareSqlBuffers(CommandText);
			else
				preparedStatement.ExecutionCount = 0;

			try 
			{
				Consume();
			}
			catch (MySqlException ex) 
			{
				if (ex.IsFatal) connection.Terminate();
				throw;
			}

			return (int)updateCount;
		}

		IDataReader IDbCommand.ExecuteReader ()
		{
			return ExecuteReader ();
		}

		IDataReader IDbCommand.ExecuteReader (CommandBehavior behavior)
		{
			return ExecuteReader (behavior);
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteReader/*'/>
		public MySqlDataReader ExecuteReader()
		{
			return ExecuteReader(CommandBehavior.Default);
		}


		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteReader1/*'/>
		public MySqlDataReader ExecuteReader(CommandBehavior behavior)
		{
			CheckState();

			string sql = TrimSemicolons(cmdText);

			if (0 != (behavior & CommandBehavior.SchemaOnly))
			{
				sql = "SET SQL_SELECT_LIMIT=0;" + sql + ";SET sql_select_limit=-1";
			}

			if (0 != (behavior & CommandBehavior.SingleRow))
			{
				sql = "SET SQL_SELECT_LIMIT=1;" + sql + ";SET sql_select_limit=-1";
			}

			updateCount = -1;
			MySqlDataReader reader = new MySqlDataReader(this, behavior);

			// if we don't have a prepared statement, then prepare our sql for execution
			if (preparedStatement == null)
				sqlBuffers = PrepareSqlBuffers(sql);
			else
				preparedStatement.ExecutionCount = 0;

			reader.NextResult();
			connection.Reader = reader;
			return reader;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/ExecuteScalar/*'/>
		public object ExecuteScalar()
		{
			// ExecuteReader will check out state

			updateCount = -1;

			object val = null;
			MySqlDataReader reader = ExecuteReader();
			if (reader.Read())
				val = reader.GetValue(0);
			reader.Close();

			return val;
		}

		/// <include file='docs/mysqlcommand.xml' path='docs/Prepare/*'/>
		public void Prepare()
		{
			if (connection == null)
				throw new InvalidOperationException(Resources.GetString("ConnectionNotSet"));
			if (connection.State != ConnectionState.Open)
				throw new InvalidOperationException(Resources.GetString("ConnectionNotOpen"));
			if (! connection.driver.Version.isAtLeast( 4,1,0)) 
				return;

			// strip out names from parameter markers
			string psSQL = CommandText;

			if (CommandType == CommandType.StoredProcedure)
			{
				if (storedProcedure == null)
					storedProcedure = new StoredProcedure(connection);
				psSQL = storedProcedure.Prepare(this);
			}
			psSQL = PrepareCommandText(psSQL);

			// ask our connection to send the prepare command
			preparedStatement = connection.driver.Prepare(psSQL, (string[])parameterMap.ToArray(typeof(string)));
		}
		#endregion


		#region Private Methods

		private string TrimSemicolons(string sql)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder(sql);
			int start = 0;
			while (sb[start] == ';')
				start++;

			int end = sb.Length-1;
			while (sb[end] == ';')
				end--;
			return sb.ToString(start, end-start+1);
		}

		/// <summary>
		/// Serializes the given parameter to the given memory stream
		/// </summary>
		/// <param name="writer">PacketWriter to stream parameter data to</param>
		/// <param name="parmName">Name of the parameter to serialize</param>
		/// <remarks>
		/// <para>This method is called by PrepareSqlBuffers to convert the given
		/// parameter to bytes and write those bytes to the given memory stream.
		/// </para>
		/// </remarks>
		/// <returns>True if the parameter was successfully serialized, false otherwise.</returns>
		private bool SerializeParameter( PacketWriter writer, string parmName )
		{
			int index = parameters.IndexOf(parmName);
			if (index == -1)
			{
				// if we are using old syntax, we can't throw exceptions for parameters
				// not defined.
				if (connection.Settings.UseOldSyntax) return false;
				throw new MySqlException("Parameter '" + parmName + "' must be defined");
			}
			MySqlParameter parameter = parameters[index];
			parameter.Serialize( writer, false );
			return true;
		}


		/// <summary>
		/// Prepares the necessary byte buffers from the given CommandText
		/// </summary>
		/// <returns>Array of byte buffers, one for each SQL command</returns>
		/// <remarks>
		/// Converts the CommandText into an array of tokens 
		/// using TokenizeSql and then into one or more byte buffers that can be
		/// sent to the server.  If the server supports batching (and we  have enabled it),
		/// then all commands result in a single byte array, otherwise a single buffer
		/// is created for each SQL command (as separated by ';').
		/// The SQL text is converted to bytes using the active encoding for the server.
		/// </remarks>
		private ArrayList PrepareSqlBuffers(string sql)
		{
			ArrayList buffers = new ArrayList();
			PacketWriter writer = new PacketWriter();
			writer.Encoding = connection.Encoding;
			writer.Version = connection.driver.Version;

			// if we are executing as a stored procedure, then we need to add the call
			// keyword.
			if (CommandType == CommandType.StoredProcedure)
			{
				if (storedProcedure == null)
					storedProcedure = new StoredProcedure(connection);
				sql = storedProcedure.Prepare( this );
			}

			// tokenize the SQL
			sql = sql.TrimStart(';').TrimEnd(';');
			ArrayList tokens = TokenizeSql( sql );

			foreach (string token in tokens)
			{
				if (token.Trim().Length == 0) continue;
				if (token == ";" && ! connection.driver.SupportsBatch)
				{
					MemoryStream ms = (MemoryStream)writer.Stream;
					if (ms.Length > 0)
						buffers.Add( ms );

					writer = new PacketWriter();
					writer.Encoding = connection.Encoding;
					writer.Version = connection.driver.Version;
					continue;
				}
				else if (token[0] == parameters.ParameterMarker) 
				{
					if (SerializeParameter( writer, token )) continue;
				}

				// our fall through case is to write the token to the byte stream
				writer.WriteStringNoNull( token );
			}

			// capture any buffer that is left over
			MemoryStream mStream = (MemoryStream)writer.Stream;
			if (mStream.Length > 0)
				buffers.Add( mStream );

			return buffers;
		}

		/// <summary>
		/// Prepares CommandText for use with the Prepare method
		/// </summary>
		/// <returns>Command text stripped of all paramter names</returns>
		/// <remarks>
		/// Takes the output of TokenizeSql and creates a single string of SQL
		/// that only contains '?' markers for each parameter.  It also creates
		/// the parameterMap array list that includes all the paramter names in the
		/// order they appeared in the SQL
		/// </remarks>
		private string PrepareCommandText(string text)
		{
			StringBuilder	newSQL = new StringBuilder();

			// tokenize the sql first
			ArrayList tokens = TokenizeSql(text);
			parameterMap.Clear();

			foreach (string token in tokens)
			{
				if ( token[0] != parameters.ParameterMarker )
					newSQL.Append( token );
				else
				{
					parameterMap.Add( token );
					newSQL.Append( parameters.ParameterMarker );
				}
			}

			return newSQL.ToString();
		}

		/// <summary>
		/// Breaks the given SQL up into 'tokens' that are easier to output
		/// into another form (bytes, preparedText, etc).
		/// </summary>
		/// <param name="sql">SQL to be tokenized</param>
		/// <returns>Array of tokens</returns>
		/// <remarks>The SQL is tokenized at parameter markers ('?') and at 
		/// (';') sql end markers if the server doesn't support batching.
		/// </remarks>
		private ArrayList TokenizeSql( string sql )
		{
			char			delim = Char.MinValue;
			StringBuilder	sqlPart = new StringBuilder();
			bool			escaped = false;
			ArrayList		tokens = new ArrayList();

			for (int i=0; i < sql.Length; i++)
			{
				char c = sql[i];
				if (escaped)
					escaped = !escaped;
				else if (c == delim) 
					delim = Char.MinValue;
				else if (c == ';' && !escaped && delim == Char.MinValue && !connection.driver.SupportsBatch)
				{
					tokens.Add( sqlPart.ToString() );
					tokens.Add( ";" );
					sqlPart.Remove( 0, sqlPart.Length ); 
					continue;
				}
				else if ((c == '\'' || c == '\"') & ! escaped & delim == Char.MinValue)
					delim=c;
				else if (c == '\\') 
					escaped = ! escaped;
				else if (c == parameters.ParameterMarker && delim == Char.MinValue && ! escaped) 
				{
					tokens.Add( sqlPart.ToString() );
					sqlPart.Remove( 0, sqlPart.Length ); 
				}
				else if (sqlPart.Length > 0 && sqlPart[0] == parameters.ParameterMarker && 
					! Char.IsLetterOrDigit(c) && c != '_' && c != '.' && c != '$' 
					&& c != '@')
				{
					tokens.Add( sqlPart.ToString() );
					sqlPart.Remove( 0, sqlPart.Length ); 
				}

				sqlPart.Append(c);
			}
			tokens.Add( sqlPart.ToString() );
			return tokens;
		}
		#endregion

		#region ICloneable
		/// <summary>
		/// Creates a clone of this MySqlCommand object.  CommandText, Connection, and Transaction properties
		/// are included as well as the entire parameter list.
		/// </summary>
		/// <returns>The cloned MySqlCommand object</returns>
		object ICloneable.Clone() 
		{
			MySqlCommand clone = new MySqlCommand(cmdText, connection, curTransaction);
			foreach (MySqlParameter p in parameters) 
			{
				clone.Parameters.Add((p as ICloneable).Clone());
			}
			return clone;
		}
		#endregion
	}
}
