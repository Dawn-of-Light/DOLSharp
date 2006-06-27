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
	/// Summary description for MySqlInt32.
	/// </summary>
	internal class MySqlInt32 : MySqlValue
	{
		private int	mValue;

		public MySqlInt32(MySqlDbType type) : base()
		{
			dbType = DbType.Int32;
			mySqlDbType = type;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			int v = Convert.ToInt32(value);
			if (binary)
				writer.Write(BitConverter.GetBytes(v));
			else
				writer.WriteStringNoNull(v.ToString(numberFormat));
		}

		public int Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		internal override Type SystemType
		{
			get { return typeof(Int32); }
		}

		internal override string GetMySqlTypeName()
		{
			if (mySqlDbType == MySqlDbType.Int24)
				return "MEDIUMINT";
			return "INT";
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length == -1) 
			{
				if (mySqlDbType == MySqlDbType.Int24)
					Value = reader.ReadInteger(3);
				else
					Value = (int)reader.ReadLong(4);
			}
			else 
			{
				string value = reader.ReadString(length);
				Value = Int32.Parse(value, numberFormat);
			}
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			reader.Skip( 4 );
		}
	}
}
