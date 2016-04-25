/*
 * DAWN OF LIGHT - The first free open source DAoC server emulator
 * 
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU General Public License
 * as published by the Free Software Foundation; either version 2
 * of the License, or (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with this program; if not, write to the Free Software
 * Foundation, Inc., 59 Temple Place - Suite 330, Boston, MA  02111-1307, USA.
 *
 */
using System;

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database.Tests
{

	/// <summary>
	/// Complex Table Implementing All Type Non Nullable
	/// </summary>
	[DataTable(TableName = "Test_ComplexTypeTestTable")]
	public class ComplexTypeTestTable : DataObject
	{
		long m_primaryKey;
		[PrimaryKey(AutoIncrement = true)]
		public long PrimaryKey { get { return m_primaryKey; } set { Dirty = true; m_primaryKey = value; } }

		char m_char;
		[DataElement(AllowDbNull = false)]
		public char Char { get { return m_char; } set { Dirty = true; m_char = value; } }
		byte m_sbyte;
		[DataElement(AllowDbNull = false)]
		public byte Sbyte { get { return m_sbyte; } set { Dirty = true; m_sbyte = value; } }
		short m_short;
		[DataElement(AllowDbNull = false)]
		public short Short { get { return m_short; } set { Dirty = true; m_short = value; } }
		int m_int;
		[DataElement(AllowDbNull = false)]
		public int Int { get { return m_int; } set { Dirty = true; m_int = value; } }
		long m_long;
		[DataElement(AllowDbNull = false)]
		public long Long { get { return m_long; } set { Dirty = true; m_long = value; } }
		byte m_byte;
		[DataElement(AllowDbNull = false)]
		public byte Byte { get { return m_byte; } set { Dirty = true; m_byte = value; } }
		ushort m_ushort;
		[DataElement(AllowDbNull = false)]
		public ushort UShort { get { return m_ushort; } set { Dirty = true; m_ushort = value; } }
		uint m_uint;
		[DataElement(AllowDbNull = false)]
		public uint UInt { get { return m_uint; } set { Dirty = true; m_uint = value; } }
		ulong m_ulong;
		[DataElement(AllowDbNull = false)]
		public ulong ULong { get { return m_ulong; } set { Dirty = true; m_ulong = value; } }
		string m_string;
		[DataElement(Varchar = 200, AllowDbNull = false)]
		public string String { get { return m_string; } set { Dirty = true; m_string = value; } }
		string m_text;
		[DataElement(AllowDbNull = false)]
		public string Text { get { return m_text; } set { Dirty = true; m_text = value; } }
		float m_float;
		[DataElement(AllowDbNull = false)]
		public float Float { get { return m_float; } set { Dirty = true; m_float = value; } }
		double m_double;
		[DataElement(AllowDbNull = false)]
		public double Double { get { return m_double; } set { Dirty = true; m_double = value; } }
		bool m_bool;
		[DataElement(AllowDbNull = false)]
		public bool Bool { get { return m_bool; } set { Dirty = true; m_bool = value; } }
		DateTime m_dateTime;
		[DataElement(AllowDbNull = false)]
		public DateTime DateTime { get { return m_dateTime; } set { Dirty = true; m_dateTime = value; } }
	}
	
	/// <summary>
	/// Complex Table Implementing All Type Nullable
	/// </summary>
	[DataTable(TableName = "Test_ComplexTypeTestTableWithNull")]
	public class ComplexTypeTestTableWithNull : DataObject
	{
		long m_primaryKey;
		[PrimaryKey(AutoIncrement = true)]
		public long PrimaryKey { get { return m_primaryKey; } set { Dirty = true; m_primaryKey = value; } }

		char m_char;
		[DataElement(AllowDbNull = true)]
		public char Char { get { return m_char; } set { Dirty = true; m_char = value; } }
		byte m_sbyte;
		[DataElement(AllowDbNull = true)]
		public byte Sbyte { get { return m_sbyte; } set { Dirty = true; m_sbyte = value; } }
		short m_short;
		[DataElement(AllowDbNull = true)]
		public short Short { get { return m_short; } set { Dirty = true; m_short = value; } }
		int m_int;
		[DataElement(AllowDbNull = true)]
		public int Int { get { return m_int; } set { Dirty = true; m_int = value; } }
		long m_long;
		[DataElement(AllowDbNull = true)]
		public long Long { get { return m_long; } set { Dirty = true; m_long = value; } }
		byte m_byte;
		[DataElement(AllowDbNull = true)]
		public byte Byte { get { return m_byte; } set { Dirty = true; m_byte = value; } }
		ushort m_ushort;
		[DataElement(AllowDbNull = true)]
		public ushort UShort { get { return m_ushort; } set { Dirty = true; m_ushort = value; } }
		uint m_uint;
		[DataElement(AllowDbNull = true)]
		public uint UInt { get { return m_uint; } set { Dirty = true; m_uint = value; } }
		ulong m_ulong;
		[DataElement(AllowDbNull = true)]
		public ulong ULong { get { return m_ulong; } set { Dirty = true; m_ulong = value; } }
		string m_string;
		[DataElement(Varchar = 200, AllowDbNull = true)]
		public string String { get { return m_string; } set { Dirty = true; m_string = value; } }
		string m_text;
		[DataElement(AllowDbNull = true)]
		public string Text { get { return m_text; } set { Dirty = true; m_text = value; } }
		float m_float;
		[DataElement(AllowDbNull = true)]
		public float Float { get { return m_float; } set { Dirty = true; m_float = value; } }
		double m_double;
		[DataElement(AllowDbNull = true)]
		public double Double { get { return m_double; } set { Dirty = true; m_double = value; } }
		bool m_bool;
		[DataElement(AllowDbNull = true)]
		public bool Bool { get { return m_bool; } set { Dirty = true; m_bool = value; } }
		DateTime m_dateTime;
		[DataElement(AllowDbNull = true)]
		public DateTime DateTime { get { return m_dateTime; } set { Dirty = true; m_dateTime = value; } }
	}
}
