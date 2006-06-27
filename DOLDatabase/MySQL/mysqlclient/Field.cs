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
using System.Data.SqlTypes;
using System.Globalization;
using System.Text;
using MySql.Data.Common;
using MySql.Data.Types;

namespace MySql.Data.MySqlClient
{
	internal enum ColumnFlags : int
	{
		NOT_NULL		= 1,
		PRIMARY_KEY		= 2,
		UNIQUE_KEY		= 4,
		MULTIPLE_KEY	= 8,
		BLOB			= 16,
		UNSIGNED		= 32,
		ZERO_FILL		= 64,
		BINARY			= 128,
		ENUM			= 256,
		AUTO_INCREMENT	= 512,
		TIMESTAMP		= 1024,
		SET				= 2048,
		NUMBER			= 32768
	};
	
	/// <summary>
	/// Summary description for Field.
	/// </summary>
	internal class MySqlField 
	{
		#region Fields

		// public fields
		public		string		CatalogName;
		public		int			ColumnLength;
		public		string		ColumnName;
		public		string		OriginalColumnName;
		public		string		TableName;
		public		string		RealTableName;
		public		string		DatabaseName;
		public		Encoding	Encoding;

		// protected fields
		protected	ColumnFlags	colFlags;
		protected	int			charSetIndex;
		protected	byte		precision;
		protected	byte		scale;
		protected	MySqlDbType	mySqlDbType;
		protected	DBVersion	connVersion;

		#endregion

		public MySqlField( DBVersion connVersion ) 
		{
			this.connVersion = connVersion;
		}

		#region Properties

		public int CharactetSetIndex 
		{
			get { return charSetIndex; }
			set { charSetIndex = value; }
		}

		public MySqlDbType	Type 
		{
			get { return mySqlDbType; }
			set { mySqlDbType = value; }
		}

		public byte Precision
		{
			get { return precision; }
			set { precision = value; }
		}

		public byte Scale
		{
			get { return scale; }
			set { scale = value; }
		}

		public ColumnFlags Flags 
		{ 
			get { return colFlags; }
			set { colFlags = value; }
		}

		public bool IsAutoIncrement
		{
			get { return (colFlags & ColumnFlags.AUTO_INCREMENT) > 0; }
		}

		public bool IsNumeric
		{
			get { return (colFlags & ColumnFlags.NUMBER) > 0; }
		}

		public bool AllowsNull
		{
			get { return (colFlags & ColumnFlags.NOT_NULL) == 0; }
		}

		public bool IsUnique
		{
			get { return (colFlags & ColumnFlags.UNIQUE_KEY) > 0; }
		}

		public bool IsPrimaryKey
		{
			get { return (colFlags & ColumnFlags.PRIMARY_KEY) > 0; }
		}

		public bool IsBlob
		{
			get { return (colFlags & ColumnFlags.BLOB) > 0; }
		}

		public bool IsBinary
		{
			get { return (colFlags & ColumnFlags.BINARY) > 0; }
		}

		public bool IsUnsigned
		{
			get { return (colFlags & ColumnFlags.UNSIGNED) > 0; }
		}

#endregion

		public MySqlValue GetValueObject() 
		{
			MySqlValue valueObject = MySqlValue.GetMySqlValue( mySqlDbType, IsUnsigned, IsBinary );
			return valueObject;
		}


	}

}
