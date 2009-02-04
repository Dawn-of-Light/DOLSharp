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
using System.Diagnostics;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Provides methods to output messages to our log
	/// </summary>
	internal class Logger
	{
		// private ctor
		private Logger() 
		{
		}

		static public void LogCommand( DBCmd cmd, string text)
		{
			if (text.Length > 300)
				text = text.Substring( 0, 300 );
			string msg = String.Format("Executing command {0} with text ='{1}'",
				Enum.GetName( typeof(DBCmd), cmd ), text );
			Trace.WriteLine( msg );
		}

		static public void LogException( Exception ex )
		{
			string msg = String.Format("EXCEPTION: " + ex.Message);
			Trace.WriteLine( msg );
		}

		static public void LogWarning(string s)
		{
			Trace.WriteLine("WARNING:" + s);
		}
	}
}
