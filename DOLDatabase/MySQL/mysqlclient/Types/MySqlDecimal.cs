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
using MySql.Data.MySqlClient;

namespace MySql.Data.Types
{
	/// <summary>
	/// Summary description for MySqlDecimal.
	/// </summary>
	internal class MySqlDecimal : MySqlValue
	{
		private byte	precision;
		private byte	scale;
		private Decimal	mValue;

		public MySqlDecimal() : base()
		{
			dbType = DbType.Decimal;
			mySqlDbType = MySqlDbType.Decimal;
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

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			Decimal v = Convert.ToDecimal( value );
			if (binary) 
			{
				writer.WriteLenString( v.ToString(numberFormat) );
			}
			else 
			{
				writer.WriteStringNoNull(v.ToString(numberFormat));
			}
		}


		public Decimal Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(Decimal); }
		}

		internal override string GetMySqlTypeName()
		{
			return "DECIMAL";
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length == -1) 
			{
				string value = reader.ReadLenString();
				Value = Decimal.Parse( value, numberFormat );
			}
			else 
			{
				string value = reader.ReadString( length );
				Value = Decimal.Parse( value, numberFormat );
			}
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			long len = reader.GetFieldLength();
			reader.Skip( len );
		}
		
	}
}
