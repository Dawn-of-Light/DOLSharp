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
	/// Summary description for MySqlDateTime.
	/// </summary>
	internal class MySqlTimeSpan : MySqlValue
	{
		private TimeSpan	mValue;

		public MySqlTimeSpan() 
		{
			dbType = DbType.Time;
			mySqlDbType = MySqlDbType.Time;
		}

		public MySqlTimeSpan(TimeSpan val) : this()
		{
			mValue = val;
		}

		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			if (! (value is TimeSpan))
				throw new MySqlException("Only TimeSpan objects can be serialized by MySqlTimeSpan");

			TimeSpan ts = (TimeSpan)value;
			if (binary) 
			{			
				writer.WriteByte( 8 );
				writer.WriteByte( (byte)(ts.TotalSeconds < 0 ? 1 : 0 ));
				writer.WriteInteger( ts.Days, 4 );
				writer.WriteByte( (byte)ts.Hours );
				writer.WriteByte( (byte)ts.Minutes );
				writer.WriteByte( (byte)ts.Seconds );
			}
			else 
			{
				writer.WriteStringNoNull( String.Format("'{0} {1:00}:{2:00}:{3:00}.{4}'", 
					ts.Days, ts.Hours, ts.Minutes, ts.Seconds, ts.Milliseconds ) );
			}
		}


		public override string ToString()
		{
			return String.Format("{0} {1:00}:{2:00}:{3:00}.{4}", 
				mValue.Days, mValue.Hours, mValue.Minutes, mValue.Seconds, mValue.Milliseconds );
		}


		internal override string GetMySqlTypeName()
		{
			return "TIME";
		}

		public TimeSpan Value
		{
			get { return mValue; }
			set { mValue = value; objectValue = value;} 
		}

		private void ParseMySql( string s, bool is41 ) 
		{
			string[] parts = s.Split(':');
			int hours = Int32.Parse( parts[0] );
			int mins = Int32.Parse( parts[1] );
			int secs = Int32.Parse( parts[2] );
			int days = hours / 24;
			hours = hours - (days * 24);
			Value = new TimeSpan( days, hours, mins, secs, 0 );
		}

		internal override Type SystemType
		{
			get { return typeof(TimeSpan); }
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length >= 0) 
			{
				string value = reader.ReadString( length );
				ParseMySql( value, reader.Version.isAtLeast(4,1,0));
				return this;
			}

			long bufLength = reader.ReadByte();
			int negate = 0;
			if (bufLength > 0)
				negate = reader.ReadByte();

			if (bufLength == 0)
				IsNull = true;
			else if (bufLength == 5)
				Value = new TimeSpan( reader.ReadInteger( 4 ), 0, 0, 0 );
			else if (bufLength == 8)
				Value = new TimeSpan( reader.ReadInteger(4), 
					reader.ReadByte(), reader.ReadByte(), reader.ReadByte() );
			else 
				Value = new TimeSpan( reader.ReadInteger(4), 
					reader.ReadByte(), reader.ReadByte(), reader.ReadByte(),
					reader.ReadInteger(4) / 1000000 );

			if (negate == 1)
				Value = mValue.Negate();
			return this;
		}

		public string ToMySqlString(bool is41) 
		{
			return mValue.ToString();
		}

		internal override void Skip(PacketReader reader)
		{
			long len = (long)reader.ReadByte();
			reader.Skip( len );
		}

	}
}
