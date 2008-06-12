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
using System.Text;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for StoredProcedure.
	/// </summary>
	internal class StoredProcedure
	{
		private string			hash;
		private MySqlConnection	connection;
		private string			outSelect;

		public StoredProcedure(MySqlConnection conn)
		{
			uint code = (uint)DateTime.Now.GetHashCode();
			hash = code.ToString();
			connection = conn;
		}

		private MySqlParameter GetReturnParameter(MySqlCommand cmd)
		{
			foreach (MySqlParameter p in cmd.Parameters)
				if (p.Direction == ParameterDirection.ReturnValue)
					return p;
			return null;
		}

		private string GetParameterList(MySqlCommand procCmd, out string returns,
			string procType)
		{
			MySqlCommand cmd = new MySqlCommand();
			string spName = procCmd.CommandText;
			cmd.Connection = connection;
			returns = null;

			int dotIndex = spName.IndexOf(".");
			// query the mysql.proc table for the procedure parameter list
			// if our spname as a dot in it, then we assume the first part is the 
			// database name.  If there is no dot, then we use database() as 
			// the current database.
			if (dotIndex == -1)
				cmd.CommandText = "SELECT param_list, returns FROM mysql.proc " +
					"WHERE db=database() ";
			else
			{
				string db = spName.Substring(0, dotIndex);
				cmd.Parameters.Add("db", db);
				spName = spName.Substring(dotIndex+1, spName.Length - dotIndex-1);
				cmd.CommandText = String.Format("SELECT param_list, returns FROM mysql.proc " + 
					"WHERE db=_latin1 {0}db ", connection.ParameterMarker);
			}

			cmd.CommandText += String.Format("AND name=_latin1 {0}name",
				connection.ParameterMarker);
				
			if (procType.Length > 0)
				cmd.CommandText += " AND type='" + procType + "'";

			cmd.Parameters.Add(connection.ParameterMarker + "name", spName);
			MySqlDataReader reader = null;

			try 
			{
				reader = cmd.ExecuteReader();
				if (!reader.Read()) return null;
				if (!reader.IsDBNull(1))
					returns = reader.GetString(1);
				if (returns != null && returns.Length == 0)
					returns = null;
				string return_val = reader.GetString(0);
				if (reader.Read())
					throw new MySqlException("More than one procedure or function matches " +
						"the name '" + spName + "'");
				return return_val;
			}
			catch (Exception ex) 
			{
				Logger.LogException( ex );
				throw;
			}
			finally 
			{
				if (reader != null) reader.Close();
			}
		}

		private string CleanType(string type)
		{
			int paren_index = type.IndexOf("(");
			if (paren_index != -1)
				type = type.Substring(0, paren_index);
			return type;
		}

		private string CleanProcParameter(string parameter)
		{
			char c = parameter[0];
			if (c == '`' || c == '\'' || c == '"')
				return parameter.Substring(1, parameter.Length-2);
			return parameter;
		}

		private string[] GetParameterParts(string parameterDef)
		{
			int pos = 0;
			string[] parts = new string[3];

			string[] split = Utility.ContextSplit(parameterDef.ToLower(), " \t\r\n", "");
			if (split.Length == 0) return null;

			if (split[0] == "in" || split[0] == "out" || split[0] == "inout")
				parts[0] = split[pos++];
			else
				parts[0] = "in";

			parts[1] = CleanProcParameter(split[pos++]);
			parts[2] = CleanType(split[pos++]);
			return parts;
		}

		private string[] GetParameterDefs(MySqlCommand cmd, out string returns, 
			string procType)
		{
			string sig = GetParameterList(cmd, out returns, procType);

			if (sig == null || sig.Length == 0) 
				return null;

			string[] paramDefs = Utility.ContextSplit(sig, ",", "()");
			return paramDefs;
		}

		public void DiscoverParameters(MySqlCommand cmd, string procType)
		{
			string returns = String.Empty;
			string[] defs = GetParameterDefs(cmd, out returns, procType);

			foreach (string def in defs)
			{
				string[] parts = GetParameterParts(def);
				if (parts == null) continue;
				MySqlParameter p = new MySqlParameter(parts[1], GetType(parts[2]));
				if (parts[0] == "out")
					p.Direction = ParameterDirection.Output;
				else if (parts[0] == "inout")
					p.Direction = ParameterDirection.InputOutput;
				else
					p.Direction = ParameterDirection.Input;
				cmd.Parameters.Add(p);
			}

			if (returns != null && returns.Length != 0)
			{
				MySqlParameter p = new MySqlParameter();
				p.MySqlDbType = GetType(CleanType(returns));
				p.Direction = ParameterDirection.ReturnValue;
				cmd.Parameters.Add(p);
			}
		}

		private string CleanParameterName(string name)
		{
			if (name[0] == connection.ParameterMarker)
				return name.Remove(0,1);
			return name;
		}

		/// <summary>
		/// Creates the proper command text for executing the given stored procedure
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public string Prepare(MySqlCommand cmd)
		{
			MySqlParameter returnParameter = GetReturnParameter(cmd);

			string returnDef;
			string[] defs = GetParameterDefs(cmd, out returnDef, 
				returnParameter == null ? "PROCEDURE" : "FUNCTION");

			string sqlStr = String.Empty;
			string setStr = String.Empty;
			outSelect = String.Empty;

			try 
			{
				if (defs != null)
				{
					foreach (string def in defs)
					{
						string[] parts = GetParameterParts(def);
						if (parts == null) continue;

						int index = cmd.Parameters.IndexOf(parts[1]);
						if (index == -1)
							throw new MySqlException("Parameter '" + parts[1] + "' is not defined");

						MySqlParameter p = cmd.Parameters[index];
						string cleanName = CleanParameterName(p.ParameterName);
						string pName = connection.ParameterMarker + cleanName;
						string vName = "@" + hash + cleanName;
						if (p.Direction == ParameterDirection.Input)
						{
							sqlStr += pName + ", ";
							continue;
						}
						else if (p.Direction == ParameterDirection.InputOutput)
							setStr += "set " + vName + "=" + pName + ";";
						sqlStr += vName + ", ";
						outSelect += vName + ", ";
					}
				}

				if (returnParameter == null)
					sqlStr = "call " + cmd.CommandText + "(" + sqlStr;
				else
				{
					string vname = "@" + hash + CleanParameterName(returnParameter.ParameterName);
					sqlStr = "set " + vname + "=" + cmd.CommandText + "(" + sqlStr;
					outSelect = vname + outSelect;
				}

				sqlStr = sqlStr.TrimEnd(' ', ',');
				outSelect = outSelect.TrimEnd(' ', ',');
				sqlStr += ")";
				if (setStr.Length > 0)
					sqlStr = setStr + sqlStr;
				return sqlStr;
			}
			catch (Exception ex)
			{
				throw new MySqlException("Exception trying to retrieve parameter info for " + cmd.CommandText + ": " + ex.Message, ex);
			}
		}

		public void UpdateParameters(MySqlParameterCollection parameters)
		{
			if (outSelect.Length == 0) return;

			char marker = connection.ParameterMarker;

			MySqlCommand cmd = new MySqlCommand("SELECT " + outSelect, connection);
			MySqlDataReader reader = cmd.ExecuteReader();

			for (int i=0; i < reader.FieldCount; i++) 
			{
				string fieldName = reader.GetName(i);
				fieldName = marker + fieldName.Remove(0, hash.Length+1);
				reader.CurrentResult[i] = parameters[fieldName].GetValueObject();
			}

			reader.Read();
			for (int i=0; i < reader.FieldCount; i++)
			{
				string fieldName = reader.GetName(i);
				fieldName = marker + fieldName.Remove(0, hash.Length+1);
				parameters[fieldName].Value = reader.GetValue(i);
			}
			reader.Close();
		}

		private MySqlDbType GetType(string typename)
		{
			typename = typename.ToLower();
			bool isUnsigned = typename.IndexOf("unsigned") != -1;
			string sqlmode = connection.driver.Property("sql_mode");
			bool real_as_float = sqlmode.IndexOf("REAL_AS_FLOAT") != -1;

			int index = typename.IndexOf("(");
			if (index != -1)
				typename = typename.Substring(0, index);

			switch (typename)
			{
				case "varchar": return MySqlDbType.VarChar;
				case "date": return MySqlDbType.Date;
				case "datetime": return MySqlDbType.Datetime;
				case "decimal": 
				case "dec":
				case "fixed":
					if (connection.driver.Version.isAtLeast(5,0,3))
						return MySqlDbType.NewDecimal;
					else
						return MySqlDbType.Decimal;
				case "year":
					return MySqlDbType.Year;
				case "time":
					return MySqlDbType.Time;
				case "timestamp":
					return MySqlDbType.Timestamp;
				case "set": return MySqlDbType.Set;
				case "enum": return MySqlDbType.Enum;
				case "bit": return MySqlDbType.Bit;
				case "tinyint":
				case "bool":
				case "boolean": 
					return MySqlDbType.Byte;
				case "smallint": 
					return isUnsigned ? MySqlDbType.UInt16 : MySqlDbType.Int16;
				case "mediumint": 
					return isUnsigned ? MySqlDbType.UInt24 : MySqlDbType.Int24;
				case "int" : 
				case "integer":
					return isUnsigned ? MySqlDbType.UInt32 : MySqlDbType.Int32;
				case "bigint": 
					return isUnsigned ? MySqlDbType.UInt64 : MySqlDbType.Int64;
				case "float": return MySqlDbType.Float;
				case "double": return MySqlDbType.Double;
				case "real": return 
					 real_as_float ? MySqlDbType.Float : MySqlDbType.Double;
				case "blob":
				case "text":
					return MySqlDbType.Blob;
				case "longblob":
				case "longtext":
					return MySqlDbType.LongBlob;
				case "mediumblob":
				case "mediumtext":
					return MySqlDbType.MediumBlob;
				case "tinyblob":
				case "tinytext":
					return MySqlDbType.TinyBlob;
			}
			throw new MySqlException("Unhandled type encountered");
		}

	}
}
