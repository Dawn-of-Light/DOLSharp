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
using System.Collections;
using System.ComponentModel;
using System.Text;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	internal enum ConnectionProtocol 
	{
		Sockets, NamedPipe, UnixSocket, SharedMemory
	}

	/// <summary>
	/// Summary description for MySqlConnectionString.
	/// </summary>
	internal sealed class MySqlConnectionString : DBConnectionString
	{
		private Hashtable	defaults;

		public MySqlConnectionString() : base()
		{
		}

		public MySqlConnectionString(string connectString) : this()
		{
			SetConnectionString(connectString);
		}

		#region Server Properties

/*		public string Name 
		{
			get { return connectionName; }
			set { connectionName = value; }
		}
*/

		[Category("Connection")]
		[Description("The name or IP address of the server to use")]
		public string Server 
		{
			get { return GetString("host"); }
		}

		[Category("Connection")]
		[Description("Port to use when connecting with sockets")]
		[DefaultValue(3306)]
		public int Port 
		{
			get { return GetInt("port"); }
		}

		[Category("Connection")]
		[Description("Protocol to use for connection to MySQL")]
		[DefaultValue(ConnectionProtocol.Sockets)]
		public ConnectionProtocol Protocol
		{
			get { return (ConnectionProtocol)keyValues["protocol"]; }
		}

		[Category("Connection")]
		[Description("Name of pipe to use when connecting with named pipes (Win32 only)")]
		public string PipeName 
		{
			get { return GetString("pipeName"); }
		}

		[Category("Connection")]
		[Description("Should the connection ues compression")]
		[DefaultValue(false)]
		public bool UseCompression 
		{
			get { return GetBool("compress"); }
		}

		[Category("Connection")]
		[Description("Database to use initially")]
		[Editor("MySql.Data.MySqlClient.Design.DatabaseTypeEditor,MySqlClient.Design", typeof(System.Drawing.Design.UITypeEditor))]
		public string Database
		{
			get { return GetString("database"); }
			set { keyValues["database"] = value; }
		}

		[Category("Connection")]
		[Description("Number of seconds to wait for the connection to succeed")]
		[DefaultValue(15)]
		public int ConnectionTimeout
		{
			get { return GetInt("connect timeout"); }
		}

		[Category("Connection")]
		[Description("Allows execution of multiple SQL commands in a single statement")]
		[DefaultValue(true)]
		public bool AllowBatch 
		{
			get { return GetBool("allow batch"); }
		}

		[Category("Connection")]
		[Description("Enables output of diagnostic messages")]
		[DefaultValue(false)]
		public bool Logging
		{
			get { return GetBool("logging"); }
		}

		[Category("Connection")]
		[Description("Name of the shared memory object to use")]
		[DefaultValue("MYSQL")]
		public string SharedMemoryName 
		{
			get { return GetString("memname"); }
		}

		[Category("Connection")]
		[Description("Allows the use of old style @ syntax for parameters")]
		[DefaultValue(false)]
		public bool UseOldSyntax 
		{
			get { return GetBool("oldsyntax"); }
		}
		#endregion

		#region Authentication Properties

		[Category("Authentication")]
		[Description("The username to connect as")]
		public string UserId 
		{
			get { return GetString("user id"); }
		}

		[Category("Authentication")]
		[Description("The password to use for authentication")]
		public string Password 
		{
			get { return GetString("password"); }
		}

/*		[Category("Authentication")]
		[Description("Should the connection use SSL.  This currently has no effect.")]
		[DefaultValue(false)]
		public bool UseSSL
		{
			get { return GetBool("use ssl"); }
//			set { keyValues["use ssl"] = value; }
		}
*/
		[Category("Authentication")]
		[Description("Show user password in connection string")]
		[DefaultValue(false)]
		public bool PersistSecurityInfo 
		{
			get { return GetBool("persist security info"); }
		}
		#endregion

		#region Pooling Properties

		[Category("Pooling")]
		[Description("Should the connection support pooling")]
		[DefaultValue(true)]
		public bool Pooling 
		{
			get { return GetBool("pooling"); }
		}

		[Category("Pooling")]
		[Description("Minimum number of connections to have in this pool")]
		[DefaultValue(0)]
		public int MinPoolSize 
		{
			get { return GetInt("min pool size"); }
		}

		[Category("Pooling")]
		[Description("Maximum number of connections to have in this pool")]
		[DefaultValue(100)]
		public int MaxPoolSize 
		{
			get { return GetInt("max pool size"); }
		}

		[Category("Pooling")]
		[Description("Maximum number of seconds a connection should live.  This is checked when a connection is returned to the pool.")]
		[DefaultValue(0)]
		public int ConnectionLifetime 
		{
			get { return GetInt("connect lifetime"); }
		}

		[Category("Pooling")]
		[Description("Should connections pulled from connection pools be reset")]
		[DefaultValue(true)]
		public bool ResetPooledConnections
		{
			get { return GetBool("reset_pooled_conn"); }
		}

		[Category("Pooling")]
		[Description("Should connections pulled from connection pools use cached server configuration")]
		[DefaultValue(false)]
		public bool CacheServerConfig
		{
			get { return GetBool("cache_server_config"); }
		}

		#endregion

		#region Other Properties

		[Category("Other")]
		[Description("Should zero datetimes be supported")]
		[DefaultValue(false)]
		public bool AllowZeroDateTime 
		{
			get { return GetBool("allowzerodatetime"); }
		}

		[Category("Other")]
		[Description("Should illegal datetime values be converted to DateTime.MinValue")]
		[DefaultValue(false)]
		public bool ConvertZeroDateTime 
		{
			get { return GetBool("convertzerodatetime"); }
		}

		[Category("Other")]
		[Description("Character set this connection should use")]
		[DefaultValue(null)]
		public string CharacterSet 
		{
			get { return GetString("charset"); }
		}

		#endregion

		/// <summary>
		/// Takes a given connection string and returns it, possible
		/// stripping out the password info
		/// </summary>
		/// <returns></returns>
		public string GetConnectionString(bool includePass)
		{
			if (connectString == null) return String.Empty;//CreateConnectionString();

			string connStr = connectString;
			if (! PersistSecurityInfo && !includePass)
				connStr = RemovePassword(connStr);

			return connStr;
/*
			StringBuilder str = new StringBuilder();
			Hashtable ht = ParseKeyValuePairs( connectString );

			if (! PersistSecurityInfo) 
				ht.Remove("password");

			foreach( string key in ht.Keys)
				str.AppendFormat("{0}={1};", key, ht[key]);

			if (str.Length > 0)
				str.Remove( str.Length-1, 1 );

			return str.ToString();*/
		}


		private string RemovePassword(string connStr)
		{
			return RemoveKeys(connStr, new string[2] { "password", "pwd" });
		}

		/// <summary>
		/// Uses the values in the keyValues hash to create a
		/// connection string
		/// </summary>
		/// <returns></returns>
		public string CreateConnectionString()
		{
			string cStr = String.Empty;

			Hashtable values = (Hashtable)keyValues.Clone();
			Hashtable defaultValues = GetDefaultValues();

			if (!PersistSecurityInfo && values.Contains("password") )
				values.Remove( "password" );

			// we always return the server key.  It's not needed but 
			// seems weird for it not to be there.
			cStr = "server=" + values["host"] + ";";
			values.Remove("server");

			foreach (string key in values.Keys)
			{
				if (values[key] != null && defaultValues[key] != null &&
					!values[key].Equals( defaultValues[key]))
					cStr += key + "=" + values[key] + ";";
			}

			return cStr;
		}

		protected override Hashtable GetDefaultValues()
		{
			defaults = base.GetDefaultValues();
			if (defaults == null)
			{
				defaults = new Hashtable(new CaseInsensitiveHashCodeProvider(), 
					new CaseInsensitiveComparer());
				defaults["host"] = String.Empty;
				defaults["connect lifetime"] = 0;
				defaults["user id"] = String.Empty;
				defaults["password"] = String.Empty;
				defaults["database"] = null;
				defaults["charset"] = null;
				defaults["pooling"] = true;
				defaults["min pool size"] = 0;
				defaults["protocol"] = ConnectionProtocol.Sockets;
				defaults["max pool size"] = 100;
				defaults["connect timeout"] = 15;
				defaults["port"] = 3306;
				defaults["useSSL"] = false;
				defaults["compress"] = false;
				defaults["persist security info"] = false;
				defaults["allow batch"] = true;
				defaults["logging"] = false;
				defaults["oldsyntax"] = false;
				defaults["pipeName"] = "MySQL";
				defaults["memname"] = "MYSQL";
				defaults["allowzerodatetime"] = false;
				defaults["convertzerodatetime"] = false;
				defaults["reset_pooled_conn"] = true;
				defaults["cache_server_config"] = false;
			}
			return (Hashtable)defaults.Clone();
		}

		protected override bool ConnectionParameterParsed(Hashtable hash, string key, string value)
		{
			string lowerCaseKey = key.ToLower();
			string lowerCaseValue = value.Trim().ToLower();
			bool boolVal = lowerCaseValue == "yes" || lowerCaseValue == "true";

			switch (lowerCaseKey)
			{
				case "cache server configuration":
				case "cacheserverconfig":
				case "cacheserverconfiguration":
					hash["cache_server_config"] = boolVal;
					break;

				case "reset pooled connections":
				case "resetpooledconnections":
				case "reset connections":
					hash["reset_pooled_conn"] = boolVal;
					break;

				case "character set":
				case "charset":
					hash["charset"] = value;
					break;

				case "use compression":
				case "compress":
					hash["compress"] = boolVal;
					break;

				case "protocol":
					if (value == "socket" || value == "tcp")
						hash["protocol"] = ConnectionProtocol.Sockets;
					else if (value == "pipe")
						hash["protocol"] = ConnectionProtocol.NamedPipe;
					else if (value == "unix")
						hash["protocol"] = ConnectionProtocol.UnixSocket;
					else if (value == "memory")
						hash["protocol"] = ConnectionProtocol.SharedMemory;
					break;

				case "pipe name":
				case "pipe":
					hash["pipeName"] = value;
					break;

				case "allow batch":
					hash["allow batch"] = boolVal;
					break;

				case "logging":
					hash["logging"] = boolVal;
					break;

				case "shared memory name":
					hash["memname"] = value;
					break;

				case "old syntax":
				case "oldsyntax":
					hash["oldsyntax"] = boolVal;
					break;

				case "convert zero datetime":
				case "convertzerodatetime":
					hash["convertzerodatetime"] = boolVal;
					break;

				case "allow zero datetime":
				case "allowzerodatetime":
					hash["allowzerodatetime"] = boolVal;
					break;

				default:
					if (! base.ConnectionParameterParsed(hash, key, value))
						throw new ArgumentException(Resources.GetString("KeywordNotSupported"), key);
					break;
			}

			return true;
		}

	}
}
