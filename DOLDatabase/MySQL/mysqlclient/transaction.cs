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

namespace MySql.Data.MySqlClient
{
	/// <include file='docs/MySqlTransaction.xml' path='docs/Class/*'/>
	public sealed class MySqlTransaction : IDbTransaction
	{
		private IsolationLevel	level;
		private MySqlConnection	conn;
		private bool			open;

		internal MySqlTransaction( MySqlConnection c, IsolationLevel il) 
		{
			conn = c;
			level = il;
			open = true;
		}

		#region Properties

		IDbConnection IDbTransaction.Connection 
		{
			get { return (MySqlConnection)Connection; }
		}

		/// <summary>
		/// Gets the <see cref="MySqlConnection"/> object associated with the transaction, or a null reference (Nothing in Visual Basic) if the transaction is no longer valid.
		/// </summary>
		/// <value>The <see cref="MySqlConnection"/> object associated with this transaction.</value>
		/// <remarks>
		/// A single application may have multiple database connections, each 
		/// with zero or more transactions. This property enables you to 
		/// determine the connection object associated with a particular 
		/// transaction created by <see cref="MySqlConnection.BeginTransaction"/>.
		/// </remarks>
		public MySqlConnection Connection
		{
			get { return conn;	} 
		}

		/// <summary>
		/// Specifies the <see cref="IsolationLevel"/> for this transaction.
		/// </summary>
		/// <value>
		/// The <see cref="IsolationLevel"/> for this transaction. The default is <b>ReadCommitted</b>.
		/// </value>
		/// <remarks>
		/// Parallel transactions are not supported. Therefore, the IsolationLevel 
		/// applies to the entire transaction.
		/// </remarks>
		public IsolationLevel IsolationLevel 
		{
			get { return level; }
		}

		#endregion

		void System.IDisposable.Dispose() 
		{
		}

		/// <include file='docs/MySqlTransaction.xml' path='docs/Commit/*'/>
		public void Commit()
		{
			if (conn == null || conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!open)
				throw new InvalidOperationException("Transaction has already been committed or is not pending");
			try 
			{
				MySqlCommand cmd = new MySqlCommand( "COMMIT", conn );
				cmd.ExecuteNonQuery();
				open = false;
			}
			catch (MySqlException) 
			{
				throw;
			}
		}

		/// <include file='docs/MySqlTransaction.xml' path='docs/Rollback/*'/>
		public void Rollback()
		{
			if (conn == null || conn.State != ConnectionState.Open)
				throw new InvalidOperationException("Connection must be valid and open to commit transaction");
			if (!open)
				throw new InvalidOperationException("Transaction has already been rolled back or is not pending");
			try 
			{
				MySqlCommand cmd = new MySqlCommand( "ROLLBACK", conn );
				cmd.ExecuteNonQuery();
				open = false;
			}
			catch (MySqlException) 
			{
				throw;
			}
		}
	}
}
