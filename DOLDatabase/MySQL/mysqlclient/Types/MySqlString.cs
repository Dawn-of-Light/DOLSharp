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
using System.IO;
using MySql.Data.MySqlClient;

namespace MySql.Data.Types
{
	/// <summary>
	/// Summary description for MySqlString.
	/// </summary>
	internal class MySqlString : MySqlValue
	{
		public MySqlString(string val, MySqlDbType type)
		{
			Value = val;
			mySqlDbType = type;
		}

		private static string EscapeString(string s)
		{
			s = s.Replace("\\", "\\\\");
			s = s.Replace("\'", "\\\'");
			s = s.Replace("\"", "\\\"");
			s = s.Replace("`", "\\`");
//			s = s.Replace("", "\\");
//			s = s.Replace("", "\\");
//			s = s.Replace("", "\\");
			return s;
		}


		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			string v = value.ToString();
			if (length > 0)
				v = v.Substring(0, length);

			if (binary)
				writer.WriteLenString( v );
			else
				writer.WriteStringNoNull( "'" + EscapeString(v) + "'" );
		}

		public byte[] ToBytes( System.Text.Encoding encoding ) 
		{
			PacketWriter p = new PacketWriter();
			p.Encoding = encoding;
			p.WriteLenString( Value );

			MemoryStream ms = (p.Stream as MemoryStream);

			byte[] buff = new byte[ ms.Length ];
			Array.Copy( ms.GetBuffer(), 0, buff, 0, (int)ms.Length );
			return buff;
		}

		public string Value
		{
			get { return objectValue as string; }
			set { objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(String); }
		}

		internal override string GetMySqlTypeName()
		{
			switch (mySqlDbType) 
			{
				case MySqlDbType.Set: return "SET";
				case MySqlDbType.Enum: return "ENUM";
			}
			return "VARCHAR";
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			string s = String.Empty;
			if (length == -1)
				s = reader.ReadLenString();
			else
				s = reader.ReadString( length );
			return new MySqlString( s, mySqlDbType );
		}

		internal override void Skip(PacketReader reader)
		{
			long len = reader.GetFieldLength();
			reader.Skip( len );
		}

	}
}
