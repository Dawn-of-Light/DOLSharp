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
	/// Summary description for MySqlBinary
	/// </summary>
	internal class MySqlBinary : MySqlValue
	{
		private byte[]	mValue;
		private bool	isBinary;

		public MySqlBinary(byte[] val, MySqlDbType type)
		{
			Value = val;
			mySqlDbType = type;
		}

		internal string ToString( System.Text.Encoding encoding ) 
		{
			return encoding.GetString( (byte[])mValue );
		}

		internal override void Serialize(PacketWriter writer, bool binary, object ourValue, int length)
		{
			byte[] buffToWrite = null;

			if (ourValue is System.Byte[])
				buffToWrite = (byte[])ourValue;
			else if (ourValue is String) 
			{
				string s = (ourValue as string).Substring(0, length);
				buffToWrite = writer.Encoding.GetBytes(s);
				length = buffToWrite.Length;
			}
			else if (ourValue is Char[]) 
			{
				buffToWrite = writer.Encoding.GetBytes( (ourValue as char[]) );
				length = buffToWrite.Length;
			}

			if ( buffToWrite == null )
				throw new MySqlException( "Only byte arrays and strings can be serialized by MySqlBinary" );

			if (binary) 
			{
				writer.WriteLength( length );
				writer.Write( buffToWrite, 0, length );
			}
			else 
			{
				if ( writer.Version.isAtLeast(4,1,0))
					writer.WriteStringNoNull( "_binary " );

				writer.WriteByte( (byte)'\'');
				EscapeByteArray( buffToWrite, length, writer );
				writer.WriteByte((byte)'\'');
			}
		}

		public bool IsBinary 
		{
			get { return isBinary; }
			set { isBinary = value; }
		}

		public byte[] Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		private void EscapeByteArray( byte[] bytes, int length, PacketWriter writer )
		{
			System.IO.MemoryStream ms = (System.IO.MemoryStream)writer.Stream;
			ms.Capacity += (length * 2);

			for (int x=0; x < length; x++)
			{
				byte b = bytes[x];
				if (b == '\0') 
				{
					writer.WriteByte( (byte)'\\' );
					writer.WriteByte( (byte)'0' );
				}
				
				else if (b == '\\' || b == '\'' || b == '\"')
				{
					writer.WriteByte( (byte)'\\' );
					writer.WriteByte( b );
				}
				else
					writer.WriteByte( b );
			}
		}


		internal override DbType DbType
		{
			get
			{
				if (isBinary) return DbType.Binary;
				return DbType.String;
			}
		}


		internal override Type SystemType
		{
			get { return typeof(byte[]); }
		}

		internal override string GetMySqlTypeName()
		{
			switch (mySqlDbType) 
			{
				case MySqlDbType.TinyBlob:		return "TINY_BLOB";
				case MySqlDbType.MediumBlob:	return "MEDIUM_BLOB";
				case MySqlDbType.LongBlob:		return "LONG_BLOB";
				case MySqlDbType.Blob:			
				default:
					return "BLOB";
			}
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length == -1)
				length = (long)reader.GetFieldLength();

			byte[] newBuff = new byte[length];
			reader.Read( ref newBuff, 0, length );
			return new MySqlBinary( newBuff, mySqlDbType );
		}

		internal override void Skip(PacketReader reader)
		{
			long len = reader.GetFieldLength();
			reader.Skip( len );
		}

	}
}
