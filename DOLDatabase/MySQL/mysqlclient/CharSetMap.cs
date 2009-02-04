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
using System.Text;
using System.Collections;
using MySql.Data.Common;

namespace MySql.Data.MySqlClient
{
	/// <summary>
	/// Summary description for CharSetMap.
	/// </summary>
	internal class CharSetMap
	{
		private static Hashtable	mapping;

		// Declare a private ctor so a default one won't be made by the compiler
		private CharSetMap() 
		{
		}

		/// <summary>
		/// Returns the text encoding for a given MySQL character set name
		/// </summary>
		/// <param name="version">Version of the connection requesting the encoding</param>
		/// <param name="CharSetName">Name of the character set to get the encoding for</param>
		/// <returns>Encoding object for the given character set name</returns>
		public static Encoding GetEncoding( DBVersion version, string CharSetName ) 
		{
			if (mapping == null )
				InitializeMapping();
			try 
			{
				if (! mapping.Contains( CharSetName ))
					throw new MySqlException("Character set '" + CharSetName + "' is not supported");

				object value = mapping[ CharSetName ];

				// if we are 4.0 or earlier and we are requesting latin1, then just return latin1 directly
				if (value.Equals("latin1") && version.isAtLeast(4,1,0))
					value = 1252;

				if (value is string)
					return Encoding.GetEncoding( (string)value );

				return Encoding.GetEncoding( (int)value );
			}
			catch (System.NotSupportedException) 
			{
				return Encoding.GetEncoding(0);
			}
		}

		/// <summary>
		/// 
		/// </summary>
		private static void InitializeMapping()
		{
			LoadCharsetMap();
		}

		private static void LoadCharsetMap()
		{
			mapping = new Hashtable();

			mapping.Add("big5", "big5");		// Traditional Chinese
			mapping.Add("sjis", "sjis");		// Shift-JIS
			mapping.Add("gb2312", "gb2312");
			mapping.Add("latin1", "latin1");
			mapping.Add("latin2", "latin2");
			mapping.Add("latin3", "latin3");
			mapping.Add("latin4", "latin4");
			mapping.Add("latin5", "latin5");
			mapping.Add("greek", "greek");
			mapping.Add("hebrew", "hebrew");
			mapping.Add("utf8", "utf-8");
			mapping.Add("ucs2", "UTF-16BE");
			mapping.Add("cp1251", 1251);
			mapping.Add("tis620", 874);
			mapping.Add("binary", "latin1");
			mapping.Add("cp1250", 1250);
			mapping.Add("cp932", 932);

			// relatively sure about
/*			mapping.Add( "default", 0 );
			mapping.Add( "cp1251", 1251 );		// Russian
			mapping.Add( "win1251", 1251 );
			mapping.Add( "gbk", 936 );			// Simplified Chinese
			mapping.Add( "cp866", 866 );
			mapping.Add( "euc_kr", 949 );

			// maybe, maybe not...
			mapping.Add( "win1250", 1250 );		// Central Eurpoe
			mapping.Add( "win1251ukr", 1251 );
			mapping.Add( "latin1_de", 1252 );	// Latin1 German
			mapping.Add( "german1", 1252 );		// German
			mapping.Add( "danish", 1252 );		// Danish
			mapping.Add( "dos", 437 );			// Dos
			mapping.Add( "pclatin2", 852 );		
			mapping.Add( "win1250ch", 1250 );
			mapping.Add( "cp1257", 1257 );
			mapping.Add( "usa7", 646 );
			mapping.Add( "czech", 912 );
			mapping.Add( "hungarian", 912 );
			mapping.Add( "croat", 912 ); */

			/*			("gb2312", "EUC_CN");
						("ujis", "EUC_JP");
						("latvian", "ISO8859_13");
						("latvian1", "ISO8859_13");
						("estonia", "ISO8859_13");
						("koi8_ru", "KOI8_R");
						("tis620", "TIS620");
						("macroman", "MacRoman");
						("macce", "MacCentralEurope");
			*/

		}
	}
}


