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
	/// Summary description for MySqlUInt64.
	/// </summary>
	internal class MySqlBit : MySqlValue
	{
		private ulong	mValue;
		private byte[]	buffer;

		public MySqlBit() : base()
		{
			buffer = new byte[8];
			dbType = DbType.UInt64;
			mySqlDbType = MySqlDbType.Bit;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			ulong v = Convert.ToUInt64( value );
			if (binary)
				writer.Write( BitConverter.GetBytes( v ) );
			else
				writer.WriteStringNoNull( v.ToString() );
		}

		public ulong Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(UInt64); }
		}

		internal override string GetMySqlTypeName()
		{
			return "BIT";
		}

		internal override MySqlValue ReadValue( PacketReader reader, long length )
		{
			if (length == -1) 
			{
				length = reader.GetFieldLength();
			}
			Array.Clear(buffer, 0, buffer.Length);
			for (long i=length-1; i >= 0; i--)
				buffer[i] = (byte)reader.ReadByte();
			Value = BitConverter.ToUInt64(buffer, 0);
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			long len = reader.GetFieldLength();
			reader.Skip(len);
		}
	}
}
