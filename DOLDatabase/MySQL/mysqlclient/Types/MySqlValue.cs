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
using System.Globalization;
using MySql.Data.MySqlClient;

namespace MySql.Data.Types
{
	/// <summary>
	/// 
	/// </summary>
	public abstract class MySqlValue
	{
		/// <summary></summary>
		protected static NumberFormatInfo		numberFormat = null;

		/// <summary></summary>
		protected object		objectValue;

		/// <summary>The generic dbtype of this value</summary>
		protected DbType		dbType;

		/// <summary>The specific MySQL db type</summary>
		protected MySqlDbType	mySqlDbType;

		/// <summary>The MySQL specific typename of this value</summary>
		protected string		mySqlTypeName;

		/// <summary>The system type represented by this value</summary>
		protected Type			classType;

		/// <summary>Is this value null</summary>
		protected bool			isNull;

		/// <summary>
		/// 
		/// </summary>
		public MySqlValue() 
		{
			isNull = false;

			if (numberFormat == null)
			{
				numberFormat = (NumberFormatInfo)NumberFormatInfo.InvariantInfo.Clone();
				numberFormat.NumberDecimalSeparator = ".";
			}
		}

		/// <summary>Returns the value of this field as an object</summary>
		public object ValueAsObject 
		{
			get { return objectValue; }
		}

		// abstract methods
		internal abstract void	Serialize( PacketWriter writer, bool binary, object value, int length );

		/// <summary></summary>
		internal abstract string GetMySqlTypeName();
		internal abstract MySqlValue ReadValue( PacketReader reader, long length );
		internal abstract void Skip( PacketReader reader );

		internal abstract Type SystemType 
		{
			get;
		}

		/// <summary>
		/// 
		/// </summary>
		public bool IsNull 
		{
			get { return isNull; }
			set { isNull = value; if (isNull) objectValue = null; }
		}

		internal virtual DbType DbType 
		{
			get { return dbType; }
		}

		internal MySqlDbType MySqlDbType 
		{
			get { return mySqlDbType; }
		}

		/// <summary>Returns a string representation of this value</summary>
		public override string ToString() 
		{
			return ValueAsObject.ToString();
		}

		internal static MySqlValue GetMySqlValue( MySqlDbType type, bool unsigned, bool binary )
		{
			switch (type) 
			{
				case MySqlDbType.Byte: 
					if (unsigned) return new MySqlUByte();
					return new MySqlByte();

				case MySqlDbType.Int16: 
					if (unsigned) return new MySqlUInt16();
					return new MySqlInt16();

				case MySqlDbType.Int24:
				case MySqlDbType.Int32: 
				case MySqlDbType.Year:
					if (unsigned) return new MySqlUInt32(type);
					return new MySqlInt32(type);

				case MySqlDbType.Bit:
					return new MySqlBit();

				case MySqlDbType.Int64: 
					if (unsigned)
						return new MySqlUInt64();
					return new MySqlInt64();

				case MySqlDbType.Time:
					return new MySqlTimeSpan();

				case MySqlDbType.Date:
				case MySqlDbType.Datetime:
				case MySqlDbType.Newdate:
				case MySqlDbType.Timestamp: return new MySqlDateTime(type);

				case MySqlDbType.Decimal: 
				case MySqlDbType.NewDecimal:
					return new MySqlDecimal();

				case MySqlDbType.Float: return new MySqlFloat();

				case MySqlDbType.Double: return new MySqlDouble();

				case MySqlDbType.Set:
				case MySqlDbType.Enum:
				case MySqlDbType.String:
				case MySqlDbType.VarChar: 
					return new MySqlString(null, type);

				case MySqlDbType.Blob:
				case MySqlDbType.MediumBlob:
				case MySqlDbType.LongBlob:
				default:
					if (binary) return new MySqlBinary( null, type );
					return new MySqlString( null, type );
			}
		}


	}

}
