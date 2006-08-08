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
using System.Data;
using System.Reflection;
using log4net;
using MySql.Data.MySqlClient;

namespace DOL.Database.MySql
{
	/// <summary>
	/// Called after mysql query.
	/// </summary>
	/// <param name="reader">The reader.</param>
	public delegate void QueryCallback(MySqlDataReader reader);
	
	/// <summary>
	/// Helper class for MySql DAOs.
	/// </summary>
	public class MySqlState : IDisposable
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		/// <summary>
		/// Holds the state's connection string.
		/// </summary>
		private readonly string m_connectionString;

		/// <summary>
		/// Acquires the connection.
		/// </summary>
		/// <returns></returns>
		public MySqlConnection AcquireConnection()
		{
			return new MySqlConnection(m_connectionString);
		}

		/// <summary>
		/// Releases the connection.
		/// </summary>
		/// <param name="connection">The connection.</param>
		public void ReleaseConnection(MySqlConnection connection)
		{
			connection.Close();
		}
		
		///<summary>
		///Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		///</summary>
		///<filterpriority>2</filterpriority>
		public void Dispose()
		{
		}

		/// <summary>
		/// Escapes the string.
		/// </summary>
		/// <param name="str">The string to escape.</param>
		/// <returns>The escaped string.</returns>
		public string EscapeString(string str)
		{
			return str;
		}

		/// <summary>
		/// Executes the non query SQL command.
		/// </summary>
		/// <param name="sqlCommand">The SQL command.</param>
		/// <returns>The count of rows affected.</returns>
		public int ExecuteNonQuery(string sqlCommand)
		{
			MySqlConnection connection = AcquireConnection();
			try
			{
				MySqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = sqlCommand;
				cmd.CommandType = CommandType.Text;
				return cmd.ExecuteNonQuery();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("non query command: " + sqlCommand, e);
			}
			finally
			{
				ReleaseConnection(connection);
			}
		}

		/// <summary>
		/// Executes the scalar SQL command.
		/// </summary>
		/// <param name="sqlCommand">The SQL command.</param>
		/// <returns>The scalar.</returns>
		public int ExecuteScalar(string sqlCommand)
		{
			MySqlConnection connection = AcquireConnection();
			try
			{
				MySqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = sqlCommand;
				cmd.CommandType = CommandType.Text;
				return cmd.ExecuteScalar();
			}
			catch(Exception e)
			{
				throw new DolDatabaseException("scalar command: " + sqlCommand, e);
			}
			finally
			{
				ReleaseConnection(connection);
			}
		}

		/// <summary>
		/// Executes the query.
		/// </summary>
		/// <param name="sqlCommand">The SQL command.</param>
		/// <param name="callback">The callback.</param>
		public void ExecuteQuery(string sqlCommand, CommandBehavior behaviour, QueryCallback callback)
		{
			if (callback == null)
			{
				throw new ArgumentNullException("callback");
			}
			
			MySqlConnection connection = AcquireConnection();
			MySqlDataReader reader = null;
			try
			{
				MySqlCommand cmd = connection.CreateCommand();
				cmd.CommandText = sqlCommand;
				cmd.CommandType = CommandType.Text;
				reader = cmd.ExecuteReader(behaviour);
				
				callback(reader);
			}
			catch (Exception e)
			{
				throw new DolDatabaseException("query command: " + sqlCommand, e);
			}
			finally
			{
				ReleaseConnection(connection);
				if (reader != null && !reader.IsClosed)
				{
					reader.Close();
				}
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MySqlState"/> class.
		/// </summary>
		/// <param name="param">The param.</param>
		/// <exception cref="ArgumentNullException">Thrown if <paramref name="param"/> is null.</exception>
		public MySqlState(IDictionary<string, string> param)
		{
			if (param == null)
			{
				throw new ArgumentNullException("param", "params can't be null");
			}

			if (!param.TryGetValue("connectionString", out m_connectionString)
				|| m_connectionString == null)
			{
				throw new ArgumentException("no connection string", "param");
			}
		}
	}
}
