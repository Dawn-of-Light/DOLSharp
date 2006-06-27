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
	/// Summary description for MySqlByte.
	/// </summary>
	internal class MySqlByte : MySqlValue
	{
		private sbyte	mValue;

		public MySqlByte() : base()
		{
			dbType = DbType.SByte;
			mySqlDbType = MySqlDbType.Byte;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			sbyte v = Convert.ToSByte( value );
			if (binary)
				writer.WriteByte( (byte)v );
			else
				writer.WriteStringNoNull( v.ToString() );
		}


		public sbyte Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(SByte); }
		}

		internal override string GetMySqlTypeName()
		{ 
			return "TINYINT"; 
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length == -1)
				Value = (sbyte)reader.ReadByte();
			else 
				Value = SByte.Parse( reader.ReadString( length ) );
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			reader.ReadByte();
		}


	}
}
