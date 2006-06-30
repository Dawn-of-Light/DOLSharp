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
	/// Summary description for MySqlDateTime.
	/// </summary>
	public class MySqlDateTime : MySqlValue, IConvertible, IComparable
	{
		private int	year, month, day, hour, minute, second;
		private static string	fullPattern;
		private static string	shortPattern;

		internal MySqlDateTime( int year, int month, int day, int hour, int minute, 
			int second, MySqlDbType type ) : this(type)
		{
			this.year = year;
			this.month = month;
			this.day = day;
			this.hour = hour;
			this.minute = minute;
			this.second = second;

			// we construct a date that is guaranteed not have zeros in the date part
			// we do this for comparison 
//			DateTime d = DateTime.MinValue;
//			d = d.AddYears(year+1).AddMonths(month+1).AddDays(day+1).AddHours(hour);
//			d = d.AddMinutes(minute).AddSeconds(second);
//			comparingDate = d;

			if (fullPattern == null)
				ComposePatterns();
		}

		internal MySqlDateTime( MySqlDbType type ) 
		{
			mySqlDbType = type;
			objectValue = this;
		}

		internal MySqlDateTime(DateTime val, MySqlDbType type) : this(type)
		{
			year = val.Year;
			month = val.Month;
			day = val.Day;
			hour = val.Hour;
			minute = val.Minute;
			second = val.Second;
		}

		#region Properties

		/// <summary>
		/// Indicates if this object contains a value that can be represented as a DateTime
		/// </summary>
		public bool IsValidDateTime 
		{
			get 
			{
				return year != 0 && month != 0 && day != 0;
			}
		}

		/// <summary>Returns the year portion of this datetime</summary>
		public int Year 
		{
			get { return year; }
			set { year = value; }
		}

		/// <summary>Returns the month portion of this datetime</summary>
		public int Month 
		{
			get { return month; }
			set { month = value; }
		}

		/// <summary>Returns the day portion of this datetime</summary>
		public int Day 
		{
			get { return day; }
			set { day = value; }
		}

		/// <summary>Returns the hour portion of this datetime</summary>
		public int Hour 
		{
			get { return hour; }
			set { hour = value; }
		}

		/// <summary>Returns the minute portion of this datetime</summary>
		public int Minute 
		{
			get { return minute; }
			set { minute = value; }
		}

		/// <summary>Returns the second portion of this datetime</summary>
		public int Second 
		{
			get { return second; }
			set { second = value; }
		}

		#endregion

		private void SerializeText( PacketWriter writer, DateTime value ) 
		{
			string val = String.Empty;

			if (mySqlDbType == MySqlDbType.Timestamp && !writer.Version.isAtLeast(4,1,0))
				val = String.Format("{0:0000}{1:00}{2:00}{3:00}{4:00}{5:00}",
					value.Year, value.Month, value.Day, value.Hour, value.Minute, value.Second );
			else 
			{
				val = String.Format("{0:0000}-{1:00}-{2:00} {3:00}:{4:00}:{5:00}", value.Year, value.Month, 
					value.Day, value.Hour, value.Minute, value.Second );
			}
			writer.WriteStringNoNull( "'" + val + "'" );
		}


		internal override void Serialize(PacketWriter writer, bool binary, object value, int length)
		{
			if (value is MySqlDateTime)
				value = (value as MySqlDateTime).GetDateTime();

			if (value is string)
				value = DateTime.Parse((string)value, 
					System.Globalization.CultureInfo.CurrentCulture);

			if (! (value is DateTime))
				throw new MySqlException( "Only DateTime objects can be serialized by MySqlDateTime" );

			DateTime dtValue = (DateTime)value;
			if (! binary)
			{
				SerializeText( writer, dtValue );
				return;
			}

			if (mySqlDbType == MySqlDbType.Timestamp)
				writer.WriteByte( 11 );
			else
				writer.WriteByte( 7 );

			writer.WriteInteger( dtValue.Year, 2 );
			writer.WriteByte( (byte)dtValue.Month );
			writer.WriteByte( (byte)dtValue.Day );
			if (mySqlDbType == MySqlDbType.Date) 
			{
				writer.WriteByte( 0 );
				writer.WriteByte( 0 );
				writer.WriteByte( 0 );
			}
			else 
			{
				writer.WriteByte( (byte)dtValue.Hour );
				writer.WriteByte( (byte)dtValue.Minute  );
				writer.WriteByte( (byte)dtValue.Second );
			}
			
			if (mySqlDbType == MySqlDbType.Timestamp)
				writer.WriteInteger( dtValue.Millisecond, 4 );
		}

		internal override DbType DbType 
		{
			get 
			{ 
				switch (mySqlDbType) 
				{
					case MySqlDbType.Date:			
					case MySqlDbType.Newdate:
						return DbType.Date;
				}
				return DbType.DateTime;
			}
		}

		internal override string GetMySqlTypeName()
		{
			switch (mySqlDbType) 
			{
				case MySqlDbType.Date: return "DATE";
				case MySqlDbType.Newdate: return "NEWDATE";
				case MySqlDbType.Timestamp: return "TIMESTAMP";
			}
			return "DATETIME";
		}

		/// <summary>Returns this value as a DateTime</summary>
		public DateTime GetDateTime()
		{
			if (! IsValidDateTime)
				throw new MySqlConversionException("Unable to convert MySQL date/time value to System.DateTime");			

			return new DateTime( year, month, day, hour, minute, second );
		}

		private MySqlDateTime Parse40Timestamp( string s ) 
		{
			int pos = 0;
			year = month = day = 1;
			hour = minute = second = 0;

			if (s.Length == 14 || s.Length == 8)
			{
				year = int.Parse(s.Substring(pos, 4));
				pos += 4;
			}
			else 
			{
				year = int.Parse(s.Substring(pos, 2));
				pos += 2;
				if (year >= 70)
					year += 1900;
				else
					year += 2000;
			}

			if (s.Length > 2)
			{
				month = int.Parse(s.Substring(pos, 2));
				pos += 2;
			}
			if (s.Length > 4)
			{
				day = int.Parse(s.Substring(pos, 2));
				pos += 2;
			}
			if (s.Length > 8)
			{
				hour = int.Parse(s.Substring(pos, 2));
				minute = int.Parse(s.Substring(pos+2, 2));
				pos += 4;
			}
			if (s.Length > 10)
				second = int.Parse(s.Substring(pos, 2));

			return new MySqlDateTime(year, month, day, hour, minute, 
				                     second, mySqlDbType );
		}

		internal MySqlDateTime ParseMySql( string s, bool is41 ) 
		{
			if (mySqlDbType == MySqlDbType.Timestamp && ! is41)
				return Parse40Timestamp(s);

			string[] parts = s.Split( '-', ' ', ':' );
			
			int year = int.Parse( parts[0] );
			int month = int.Parse( parts[1] );
			int day = int.Parse( parts[2] );

			int hour = 0, minute = 0, second = 0;
			if (parts.Length > 3) 
			{
				hour = int.Parse( parts[3] );
				minute = int.Parse( parts[4] );
				second = int.Parse( parts[5] );
			}

			return new MySqlDateTime( year, month, day, hour, minute, second, mySqlDbType );
		}

		internal override Type SystemType
		{
			get { return typeof(MySqlDateTime); }
		}

		internal override MySqlValue ReadValue(PacketReader reader, long length)
		{
			if (length >= 0) 
			{
				string value = reader.ReadString( length );
				return ParseMySql( value, reader.Version.isAtLeast(4,1,0) );
			}

			long bufLength = reader.ReadByte();

			int year = reader.ReadInteger(2);
			int month = reader.ReadByte();
			int day = reader.ReadByte();
			int hour = 0, minute = 0, second = 0;

			if (bufLength > 4) 
			{
				hour = reader.ReadByte();
				minute = reader.ReadByte();
				second = reader.ReadByte();
			}
		
			if (bufLength > 7)
				reader.ReadInteger(4);
		
			return new MySqlDateTime( year, month, day, hour, minute, second, mySqlDbType );
		}

		internal override void Skip(PacketReader reader)
		{
			long len = reader.ReadByte();
			reader.Skip( len );
		}

		/// <summary>Returns a MySQL specific string representation of this value</summary>
		public override string ToString()
		{
			if (this.IsValidDateTime) 
			{
				DateTime d = new DateTime( year, month, day, hour, minute, second );
				return (mySqlDbType == MySqlDbType.Date) ? d.ToString("d") : d.ToString();
			}

			if (mySqlDbType == MySqlDbType.Date)
				return String.Format( shortPattern, year, month, day);

			if (hour >= 12)
				fullPattern = fullPattern.Replace("A", "P");
			return String.Format( fullPattern, year, month, day, hour, minute, second);
		}

		private void ComposePatterns() 
		{
			DateTime tempDT = new DateTime(1, 2, 3, 4, 5, 6);
			fullPattern = tempDT.ToString();
			fullPattern = fullPattern.Replace( "0001", "{0:0000}" );
			if (fullPattern.IndexOf("02") != -1)
				fullPattern = fullPattern.Replace( "02", "{1:00}" );
			else
				fullPattern = fullPattern.Replace("2", "{1}" );
			if (fullPattern.IndexOf("03") != -1)
				fullPattern = fullPattern.Replace( "03", "{2:00}" );
			else
				fullPattern = fullPattern.Replace("3", "{2}" );
			if (fullPattern.IndexOf("04") != -1)
				fullPattern = fullPattern.Replace( "04", "{3:00}" );
			else
				fullPattern = fullPattern.Replace("4", "{3}" );
			if (fullPattern.IndexOf("05") != -1)
				fullPattern = fullPattern.Replace( "05", "{4:00}" );
			else
				fullPattern = fullPattern.Replace("5", "{4}" );
			if (fullPattern.IndexOf("06") != -1)
				fullPattern = fullPattern.Replace( "06", "{5:00}" );
			else
				fullPattern = fullPattern.Replace("6", "{5}" );

			shortPattern = tempDT.ToString("d");
			shortPattern = shortPattern.Replace( "0001", "{0:0000}" );
			if (shortPattern.IndexOf("02") != -1)
				shortPattern = shortPattern.Replace( "02", "{1:00}" );
			else
				shortPattern = shortPattern.Replace("2", "{1}" );
			if (shortPattern.IndexOf("03") != -1)
				shortPattern = shortPattern.Replace( "03", "{2:00}" );
			else
				shortPattern = shortPattern.Replace("3", "{2}" );
		}

		/// <summary></summary>
		/// <param name="val"></param>
		/// <returns></returns>
		public static explicit operator DateTime( MySqlDateTime val ) 
		{
			if (! val.IsValidDateTime) return DateTime.MinValue;
			return val.GetDateTime();
		}

		#region IConvertible Members

		ulong IConvertible.ToUInt64 (IFormatProvider provider)
		{
			return 0;
		}

		sbyte IConvertible.ToSByte(IFormatProvider provider)
		{
			// TODO:  Add MySqlDateTime.ToSByte implementation
			return 0;
		}

		double IConvertible.ToDouble(IFormatProvider provider)
		{
			return 0;
		}

		DateTime IConvertible.ToDateTime(IFormatProvider provider)
		{
			return this.GetDateTime();
		}

		float IConvertible.ToSingle(IFormatProvider provider)
		{
			return 0;
		}

		bool IConvertible.ToBoolean(IFormatProvider provider)
		{
			return false;
		}

		int IConvertible.ToInt32(IFormatProvider provider)
		{
			return 0;
		}

		ushort IConvertible.ToUInt16(IFormatProvider provider)
		{
			return 0;
		}

		short IConvertible.ToInt16(IFormatProvider provider)
		{
			return 0;
		}

		string System.IConvertible.ToString(IFormatProvider provider)
		{
			return null;
		}

		byte IConvertible.ToByte(IFormatProvider provider)
		{
			return 0;
		}

		char IConvertible.ToChar(IFormatProvider provider)
		{
			return '\0';
		}

		long IConvertible.ToInt64(IFormatProvider provider)
		{
			return 0;
		}

		System.TypeCode IConvertible.GetTypeCode()
		{
			return new System.TypeCode ();
		}

		decimal IConvertible.ToDecimal(IFormatProvider provider)
		{
			return 0;
		}

		object IConvertible.ToType(Type conversionType, IFormatProvider provider)
		{
			return null;
		}

		uint IConvertible.ToUInt32(IFormatProvider provider)
		{
			return 0;
		}

		#endregion

		#region IComparable Members

		int IComparable.CompareTo(object obj)
		{
			MySqlDateTime otherDate = (MySqlDateTime)obj;

			if (Year < otherDate.Year) return -1;
			else if (Year > otherDate.Year) return 1;

			if (Month < otherDate.Month) return -1;
			else if (Month > otherDate.Month) return 1;

			if (Day < otherDate.Day) return -1;
			else if (Day > otherDate.Day) return 1;

			if (Hour < otherDate.Hour) return -1;
			else if (Hour > otherDate.Hour) return 1;

			if (Minute < otherDate.Minute) return -1;
			else if (Minute > otherDate.Minute) return 1;

			if (Second < otherDate.Second) return -1;
			else if (Second > otherDate.Second) return 1;

			return 0;
		}

		#endregion
	}
}
