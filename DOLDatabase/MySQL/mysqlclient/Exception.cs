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
using System.Runtime.Serialization;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// The exception that is thrown when MySQL returns an error. This class cannot be inherited.
	/// </summary>
	/// <include file='docs/MySqlException.xml' path='MyDocs/MyMembers[@name="Class"]/*'/>
	[Serializable]
	public sealed class MySqlException : SystemException
	{
		private int			errorCode;
		private bool		isFatal;

		internal MySqlException() 
		{
		}

		internal MySqlException(string msg) : base(msg)
		{
		}

		internal MySqlException(string msg, Exception ex) : base(msg, ex)
		{
		}

		internal MySqlException(string msg, bool isFatal, Exception inner) : base (msg, inner)
		{
			this.isFatal = isFatal;
		}

		internal MySqlException(string msg, int errno) : this(msg)
		{
			errorCode = errno;
		}

		private MySqlException(SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}

		/// <summary>
		/// Gets a number that identifies the type of error.
		/// </summary>
		public int Number 
		{
			get { return errorCode; } 
		}

		/// <summary>
		/// True if this exception was fatal and cause the closing of the connection, false otherwise.
		/// </summary>
		internal bool IsFatal 
		{
			get { return isFatal; }
		}
	}
}
