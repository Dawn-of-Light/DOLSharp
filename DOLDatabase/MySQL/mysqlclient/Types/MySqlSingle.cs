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
	/// Summary description for MySqlFloat.
	/// </summary>
	internal class MySqlFloat : MySqlValue
	{
		private Single	mValue;

		public MySqlFloat() : base()
		{
			dbType = DbType.Single;
			mySqlDbType = MySqlDbType.Float;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			Single v = Convert.ToSingle( value );
			if (binary)
				writer.Write( BitConverter.GetBytes( v ) );
			else
				writer.WriteStringNoNull( v.ToString(numberFormat) );
		}

		public Single Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value; }
		}

		public static float MaxValue 
		{
			get { return float.Parse(float.MaxValue.ToString("R")); }
		}

		public static float MinValue 
		{
			get { return float.Parse(float.MinValue.ToString("R")); }
		}

		internal override Type SystemType
		{
			get { return typeof(Single); }
		}

		internal override string GetMySqlTypeName()
		{
			return "FLOAT";
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length == -1) 
			{
				byte[] b = new byte[4];
				reader.Read( ref b, 0, 4 );
				Value = BitConverter.ToSingle( b, 0 );
			}
			else 
			{
				string value = reader.ReadString( length );
				Value = Parse(value);
			}
			return this;
		}

		internal override void Skip(PacketReader reader)
		{
			reader.Skip(4);
		}

		private float Parse(string s) 
		{
			float result = 0;
			try 
			{
				result = Single.Parse(s, numberFormat);
			}
			catch (Exception) 
			{
				s = s.ToLower();
				bool isNeg = s.StartsWith(numberFormat.NegativeSign);

				if (s.IndexOf("e+") != -1)
					return isNeg ? MinValue : MaxValue;
				return 0;
			}
			return result;
		}

	}
}
