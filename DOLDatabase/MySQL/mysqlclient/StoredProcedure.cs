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

		private string GetParameterList(string spName, bool isProc) 
		{
			MySqlCommand cmd = new MySqlCommand();
			cmd.Connection = connection;

			int dotIndex = spName.IndexOf(".");
			// query the mysql.proc table for the procedure parameter list
			// if our spname as a dot in it, then we assume the first part is the 
			// database name.  If there is no dot, then we use database() as 
			// the current database.
			if (dotIndex == -1)
				cmd.CommandText = "SELECT param_list FROM mysql.proc WHERE db=database() ";
			else
			{
				string db = spName.Substring(0, dotIndex);
				cmd.Parameters.Add("db", db);
				spName = spName.Substring(dotIndex+1, spName.Length - dotIndex-1);
				cmd.CommandText = String.Format("SELECT param_list FROM mysql.proc " + 
					"WHERE db=_latin1 {0}db ", connection.ParameterMarker);
			}

			cmd.CommandText += String.Format("AND name=_latin1 {0}name AND type='{1}'",
				connection.ParameterMarker, isProc ? "PROCEDURE" : "FUNCTION");

			//cmd.Parameters.Add("db", connection.Database);
			cmd.Parameters.Add("name", spName);
			MySqlDataReader reader = null;

			try 
			{
				reader = cmd.ExecuteReader();
				if (!reader.Read()) return null;
				return reader.GetString(0);
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

		private string GetReturnParameter(MySqlCommand cmd)
		{
			foreach (MySqlParameter p in cmd.Parameters)
				if (p.Direction == ParameterDirection.ReturnValue)
					return hash + p.ParameterName;
			return null;
		}

		private string PrepareAsFunction(MySqlCommand cmd)
		{
			return null;
		}

		/// <summary>
		/// Creates the proper command text for executing the given stored procedure
		/// </summary>
		/// <param name="cmd"></param>
		/// <returns></returns>
		public string Prepare(MySqlCommand cmd)
		{
			// if we have a return value paramter, then we treat it as a 
			// stored function
			string retParm = GetReturnParameter(cmd);
			bool isProc = retParm == null;

			string setStr = String.Empty;
			string sqlStr = String.Empty;
			
			outSelect = String.Empty;
			try 
			{
				string param_list = GetParameterList(cmd.CommandText, isProc);

				if (param_list != null && param_list.Length > 0)
				{
					string[] paramDefs = Utility.ContextSplit( param_list, ",", "()" );
					foreach (string paramDef in paramDefs) 
					{
						string[] parts = Utility.ContextSplit(paramDef.ToLower(), " \t\r\n", "");
						if (parts.Length == 0) continue;
						string direction = parts.Length >= 3 ? parts[0] : "in";
						string vName = parts.Length >= 3 ? parts[1] : parts[0];

						string pName = connection.ParameterMarker + vName;
						vName = "@" + hash + vName;

						if (direction.Equals("in"))
							sqlStr += pName + ", ";
						else if (direction == "out") 
						{
							sqlStr += vName + ", ";
							outSelect += vName + ", ";
						}
						else if (direction == "inout")
						{
							setStr += "set " + vName + "=" + pName + ";";
							sqlStr += vName + ", ";
							outSelect += vName + ", ";
						}
					}
				}
				sqlStr = sqlStr.TrimEnd(' ', ',');
				outSelect = outSelect.TrimEnd(' ', ',');
				if (isProc)
					sqlStr = "call " + cmd.CommandText + "(" + sqlStr + ")";
				else
				{
					sqlStr = "set @" + retParm + "=" + cmd.CommandText + "(" + sqlStr + ")";
					outSelect = "@" + retParm;
				}
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
	}
}
