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
using MySql.Data.MySqlClient;

namespace MySql.Data.Common
{
	/// <summary>
	/// Summary description for Version.
	/// </summary>
	internal struct DBVersion
	{
		private int		major;
		private int		minor;
		private int		build;
		private string	srcString;

		public DBVersion( string s, int major, int minor, int build)
		{
			this.major = major;
			this.minor = minor;
			this.build = build;
			srcString = s;
		}

		public static DBVersion Parse( string versionString )
		{
			int start = 0;
			int index = versionString.IndexOf('.', start);
			if (index == -1) 
				throw new MySqlException(Resources.GetString("BadVersionFormat"));
			string val = versionString.Substring(start, index-start).Trim();
			int major = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

			start = index+1;
			index = versionString.IndexOf('.', start);
			if (index == -1) 
				throw new MySqlException(Resources.GetString("BadVersionFormat"));
			val = versionString.Substring(start, index-start).Trim();
			int minor = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

			start = index+1;
			int i = start;
			while (i < versionString.Length && Char.IsDigit(versionString, i))
				i++;
			val = versionString.Substring(start, i-start).Trim();
			int build = Convert.ToInt32(val, System.Globalization.NumberFormatInfo.InvariantInfo);

			return new DBVersion(versionString, major, minor, build);
		}

		public bool isAtLeast(int major, int minor, int build)
		{
			if (this.major > major) return true;
			if (this.major == major && this.minor > minor) return true;
			if (this.major == major && this.minor == minor && this.build >= build) return true;
			return false;
		}

		public override string ToString()
		{
			return srcString;
		}

	}
}
