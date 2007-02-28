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
using System.Text;

namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for Utility.
	/// </summary>
	internal abstract class DBConnectionString
	{
		protected Hashtable	keyValues;
		protected string	connectionName = String.Empty;
		protected string	connectString;

		public DBConnectionString()
		{	
			keyValues = new Hashtable(new CaseInsensitiveHashCodeProvider(), 
				new CaseInsensitiveComparer());
		}

		public void LoadDefaultValues()
		{
			keyValues = GetDefaultValues();
		}

		public void SetConnectionString(string value)
		{
			Hashtable ht = Parse(value);			
			connectString = value;
			keyValues = ht;
		}

		protected string GetString(string name) 
		{
			if (! keyValues.ContainsKey(name)) return String.Empty;
			if (keyValues[name] == null) return String.Empty;
			return (keyValues[name] as string);
		}

		protected int GetInt( string name ) 
		{
			return Convert.ToInt32(keyValues[name], System.Globalization.NumberFormatInfo.InvariantInfo);
		}

		protected bool GetBool( string name ) 
		{
			object val = keyValues[name];
			if (val.Equals(true) || val.Equals("true") || val.Equals("yes") || val.Equals(1))
				return true;
			return false;
		}

		protected string RemoveKeys(string value, string[] keys)
		{
			System.Text.StringBuilder sb = new System.Text.StringBuilder();
			string[] pairs = Utility.ContextSplit(value, ";", "\"'");
			foreach (string keyvalue in pairs)
			{
				string test = keyvalue.Trim().ToLower();
				if (test.StartsWith("pwd") || test.StartsWith("password"))
					continue;
				sb.Append(keyvalue);
				sb.Append(";");
			}
			sb.Remove(sb.Length-1, 1);  // remove the trailing ;
			return sb.ToString();
		}

		protected virtual bool ConnectionParameterParsed(Hashtable hash, string key, string value)
		{
			string lowerKey =  key.ToLower(System.Globalization.CultureInfo.InvariantCulture);

			switch (lowerKey)
			{
				case "persist security info":
					hash["persist security info"] = value;
					return true;

				case "uid":
				case "username":
				case "user id":
				case "user name": 
				case "userid":
					hash["user id"] = value;
					return true;

				case "password": 
				case "pwd":
					hash["password"] = value;
					return true;

				case "host":
				case "server":
				case "data source":
				case "datasource":
				case "address":
				case "addr":
				case "network address":
					hash["host"] = value;
					return true;
				
				case "initial catalog":
				case "database":
					hash["database"] = value;
					return true;

				case "connection timeout":
				case "connect timeout":
					hash["connect timeout"] = Int32.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
					return true;

				case "port":
					hash["port"] = Int32.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
					return true;

				case "pooling":
					hash["pooling"] = 
						value.ToLower() == "yes" || value.ToLower() == "true";
					return true;

				case "min pool size":
					hash["min pool size"] = Int32.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
					return true;

				case "max pool size":
					hash["max pool size"] = Int32.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
					return true;

				case "connection lifetime":
					hash["connect lifetime"] = Int32.Parse(value, System.Globalization.NumberFormatInfo.InvariantInfo);
					return true;
			}
			return false;
		}

		protected virtual Hashtable GetDefaultValues()
		{
			return null;
		}

		protected static Hashtable ParseKeyValuePairs(string src)
		{
			String[] keyvalues = src.Split(';');
			String[] newkeyvalues = new String[keyvalues.Length];
			int		 x = 0;

			// first run through the array and check for any keys that
			// have ; in their value
			foreach (String keyvalue in keyvalues) 
			{
				// check for trailing ; at the end of the connection string
				if (keyvalue.Length == 0) continue;

				// this value has an '=' sign so we are ok
				if (keyvalue.IndexOf('=') >= 0) 
				{
					newkeyvalues[x++] = keyvalue;
				}
				else 
				{
					newkeyvalues[x-1] += ";";
					newkeyvalues[x-1] += keyvalue;
				}
			}

			Hashtable hash = new Hashtable();

			// now we run through our normalized key-values, splitting on equals
			for (int y=0; y < x; y++) 
			{
				String[] parts = newkeyvalues[y].Split('=');

				// first trim off any space and lowercase the key
				parts[0] = parts[0].Trim().ToLower();
				parts[1] = parts[1].Trim();

				// we also want to clear off any quotes
				parts[0] = parts[0].Trim('\'', '"');
				parts[1] = parts[1].Trim('\'', '"');

				hash[parts[0]] = parts[1];
			}
			return hash;
		}

		protected virtual Hashtable Parse(string newConnectString) 
		{
			Hashtable hash = ParseKeyValuePairs( newConnectString );
			Hashtable newHash = GetDefaultValues();

			foreach (object key in hash.Keys)
				ConnectionParameterParsed( newHash, (string)key, (string)hash[key] );
			return newHash;
		}


	}
}
