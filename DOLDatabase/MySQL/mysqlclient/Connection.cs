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
using System.Collections.Specialized;
using System.Text;
using System.ComponentModel;
using System.Globalization;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <include file='docs/MySqlConnection.xml' path='docs/ClassSummary/*'/>
	[System.Drawing.ToolboxBitmap( typeof(MySqlConnection), "MySqlClient.resources.connection.bmp")]
	[System.ComponentModel.DesignerCategory("Code")]
	[ToolboxItem(true)]
	public sealed class MySqlConnection : Component, IDbConnection, IDisposable, ICloneable
	{
		internal ConnectionState			state;
		internal Driver						driver;
		private  MySqlDataReader			dataReader;
		private  MySqlConnectionString		settings;
		private  bool						hasBeenOpen;

		/// <include file='docs/MySqlConnection.xml' path='docs/StateChange/*'/>
		public event StateChangeEventHandler		StateChange;

		/// <include file='docs/MySqlConnection.xml' path='docs/InfoMessage/*'/>
		public event MySqlInfoMessageEventHandler	InfoMessage;


		/// <include file='docs/MySqlConnection.xml' path='docs/DefaultCtor/*'/>
		public MySqlConnection()
		{
			//TODO: add event data to StateChange docs
			settings = new MySqlConnectionString();
			settings.LoadDefaultValues();
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Ctor1/*'/>
		public MySqlConnection(string connectionString) : this()
		{
			ConnectionString = connectionString;
		}

		#region Interal Methods & Properties

		internal MySqlConnectionString Settings 
		{
			get { return settings; }
		}

		internal MySqlDataReader Reader
		{
			get { return dataReader; }
			set { dataReader = value; }
		}

		internal char ParameterMarker 
		{
			get { if (settings.UseOldSyntax) return '@'; return '?'; }
		}

		internal void OnInfoMessage( MySqlInfoMessageEventArgs args ) 
		{
			if (InfoMessage != null) 
			{
				InfoMessage( this, args );
			}
		}

		#endregion

		#region Properties

		/// <summary>
		/// Returns the id of the server thread this connection is executing on
		/// </summary>
		[Browsable(false)] 
		public int ServerThread 
		{
			get { return driver.ThreadID; }
		}

		/// <summary>
		/// Gets the name of the MySQL server to which to connect.
		/// </summary>
		[Browsable(true)]
		public string DataSource
		{
			get { return settings.Server; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ConnectionTimeout/*'/>
		[Browsable(true)]
		public int ConnectionTimeout
		{
			get { return settings.ConnectionTimeout; }
		}
		
		/// <include file='docs/MySqlConnection.xml' path='docs/Database/*'/>
		[Browsable(true)]
		public string Database
		{
			get	{ return settings.Database; }
		}

		/// <summary>
		/// Indicates if this connection should use compression when communicating with the server.
		/// </summary>
		[Browsable(false)]
		public bool UseCompression
		{
			get { return settings.UseCompression; }
		}
		
		/// <include file='docs/MySqlConnection.xml' path='docs/State/*'/>
		[Browsable(false)]
		public ConnectionState State
		{
			get { return state; }
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/ServerVersion/*'/>
		[Browsable(false)]
		public string ServerVersion 
		{
			get { return  driver.Version.ToString(); }
		}

		internal Encoding Encoding 
		{
			get 
			{
				if (driver == null)
					return System.Text.Encoding.Default;
				else 
					return driver.Encoding;
			}
		}


		/// <include file='docs/MySqlConnection.xml' path='docs/ConnectionString/*'/>
#if WINDOWS
		[Editor("MySql.Data.MySqlClient.Design.ConnectionStringTypeEditor,MySqlClient.Design", typeof(System.Drawing.Design.UITypeEditor))]
#endif
		[Browsable(true)]
		[Category("Data")]
		[Description("Information used to connect to a DataSource, such as 'Server=xxx;UserId=yyy;Password=zzz;Database=dbdb'.")]
		public string ConnectionString
		{
			get
			{
				// Always return exactly what the user set.
				// Security-sensitive information may be removed.
				return settings.GetConnectionString(!hasBeenOpen);
			}
			set
			{
				if (this.State != ConnectionState.Closed)
					throw new MySqlException("Not allowed to change the 'ConnectionString' property while the connection (state=" + State + ").");

				settings.SetConnectionString(value);
				if ( driver != null)
					driver.Settings = settings;
			}
		}

		#endregion

		#region Transactions

		/// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction/*'/>
		public MySqlTransaction BeginTransaction()
		{
			return this.BeginTransaction( IsolationLevel.RepeatableRead );
		}

		IDbTransaction IDbConnection.BeginTransaction()
		{
			return BeginTransaction();
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/BeginTransaction1/*'/>
		public MySqlTransaction BeginTransaction(IsolationLevel iso)
		{
			//TODO: check note in help
			if (state != ConnectionState.Open)
				throw new InvalidOperationException(Resources.GetString("ConnectionNotOpen"));

			MySqlTransaction t = new MySqlTransaction(this, iso);

			MySqlCommand cmd = new MySqlCommand("", this);

			cmd.CommandText = "SET SESSION TRANSACTION ISOLATION LEVEL ";
			switch (iso) 
			{
				case IsolationLevel.ReadCommitted:
					cmd.CommandText += "READ COMMITTED"; break;
				case IsolationLevel.ReadUncommitted:
					cmd.CommandText += "READ UNCOMMITTED"; break;
				case IsolationLevel.RepeatableRead:
					cmd.CommandText += "REPEATABLE READ"; break;
				case IsolationLevel.Serializable:
					cmd.CommandText += "SERIALIZABLE"; break;
				case IsolationLevel.Chaos:
					throw new NotSupportedException(Resources.GetString("ChaosNotSupported"));
			}

			cmd.ExecuteNonQuery();

			cmd.CommandText = "BEGIN";
			cmd.ExecuteNonQuery();

			return t;
		}

		IDbTransaction IDbConnection.BeginTransaction(IsolationLevel il)
		{
			return BeginTransaction(il);
		}
		#endregion

		/// <include file='docs/MySqlConnection.xml' path='docs/ChangeDatabase/*'/>
		public void ChangeDatabase(string databaseName)
		{
			if (databaseName == null || databaseName.Trim().Length == 0)
				throw new ArgumentException(
					Resources.GetString("ParameterIsInvalid"), "database");

			if (state != ConnectionState.Open)
				throw new InvalidOperationException(
					Resources.GetString("ConnectionNotOpen"));

			driver.SetDatabase(databaseName);
			settings.Database = databaseName;
		}

		internal void SetState( ConnectionState newState ) 
		{
			ConnectionState oldState = state;
			state = newState;
			if (this.StateChange != null)
				StateChange(this, new StateChangeEventArgs( oldState, newState ));
		}

		/// <summary>
		/// Ping
		/// </summary>
		/// <returns></returns>
		public bool Ping() 
		{
			return driver.Ping();
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Open/*'/>
		public void Open()
		{
			if (state == ConnectionState.Open)
				throw new InvalidOperationException(
					Resources.GetString("ConnectionAlreadyOpen"));

			SetState(ConnectionState.Connecting);

			try 
			{
				if (settings.Pooling) 
				{
					driver = MySqlPoolManager.GetConnection(settings);
				}
				else
				{
					driver = Driver.Create(settings);
				}
			}
			catch (Exception)
			{
				SetState(ConnectionState.Closed);
				throw;
			}

			// if the user is using old syntax, let them know
			if ( driver.Settings.UseOldSyntax)
				Logger.LogWarning("You are using old syntax that will be removed in future versions");

			SetState(ConnectionState.Open);
			driver.Configure(this);
			if (settings.Database != null && settings.Database.Length != 0)
				ChangeDatabase(settings.Database);
			hasBeenOpen = true;
		}

		internal void Terminate()
		{
			try 
			{
				if (settings.Pooling)
					MySqlPoolManager.ReleaseConnection(driver);
				else
					driver.Close();
			}
			catch (Exception) { }

			SetState(ConnectionState.Closed);
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/Close/*'/>
		public void Close()
		{
			//TODO: rollback any pending transaction
			if (state == ConnectionState.Closed) return;

			if (dataReader != null)
				dataReader.Close();

			Terminate();
		}

		IDbCommand IDbConnection.CreateCommand()
		{
			return CreateCommand();
		}

		/// <include file='docs/MySqlConnection.xml' path='docs/CreateCommand/*'/>
		public MySqlCommand CreateCommand()
		{
			// Return a new instance of a command object.
			MySqlCommand c = new MySqlCommand();
			c.Connection = this;
			return c;
		}

		#region ICloneable
		/// <summary>
		/// Creates a new MySqlConnection object with the exact same ConnectionString value
		/// </summary>
		/// <returns>A cloned MySqlConnection object</returns>
		object ICloneable.Clone()
		{
			MySqlConnection clone = new MySqlConnection();
			clone.ConnectionString = this.ConnectionString;
			//TODO:  how deep should this go?
			return clone;
		}
		#endregion

		#region IDisposeable

		/// <summary>
		/// Releases the resources used by the MySqlConnection.
		/// </summary>
		protected override void Dispose(bool disposing) 
		{
			if (disposing)
			{
				if (State == ConnectionState.Open)
					Close();
			}
			base.Dispose(disposing);
		}

		#endregion
  }

	/// <summary>
	/// Represents the method that will handle the <see cref="MySqlConnection.InfoMessage"/> event of a 
	/// <see cref="MySqlConnection"/>.
	/// </summary>
	public delegate void MySqlInfoMessageEventHandler(object sender, MySqlInfoMessageEventArgs args);

	/// <summary>
	/// Provides data for the InfoMessage event. This class cannot be inherited.
	/// </summary>
	public class MySqlInfoMessageEventArgs : EventArgs
	{
		/// <summary>
		/// 
		/// </summary>
		public MySqlError[]	errors;
	}
}
