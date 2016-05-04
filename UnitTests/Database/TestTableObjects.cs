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
	/// Test Table with Multiple Overlapping Index
	/// </summary>
	[DataTable(TableName = "Test_TableWithMultiIndexes")]
	public class TestTableWithMultiIndexes : DataObject
	{
		[DataElement(IndexColumns = "Index2")]
		public string Index1 { get; set; }
		[DataElement(IndexColumns = "Index3")]
		public string Index2 { get; set; }
		[DataElement]
		public string Index3 { get; set; }
	}

	/// <summary>
	/// Test Table Migration with No Primary Key
	/// </summary>
	[DataTable(TableName = "Test_TableMigrationNoPrimary")]
	public class TestTableWithNoPrimaryV1 : DataObject
	{
		[DataElement]
		public string Value { get; set; }
	}
	
	/// <summary>
	/// Test Table Migration To Auto Increment Primary Key
	/// </summary>
	[DataTable(TableName = "Test_TableMigrationNoPrimary")]
	public class TestTableWithNoPrimaryV2 : DataObject
	{
		[PrimaryKey(AutoIncrement = true)]
		public int PrimaryKey { get; set; }
		[DataElement]
		public string Value { get; set; }
	}
	
	/// <summary>
	/// Test Table Migration To Auto Increment Primary Key changing name
	/// </summary>
	[DataTable(TableName = "Test_TableMigrationNoPrimary")]
	public class TestTableWithNoPrimaryV3 : DataObject
	{
		[PrimaryKey(AutoIncrement = true)]
		public int PrimaryKey2 { get; set; }
		[DataElement]
		public string Value { get; set; }
	}
	
	/// <summary>
	/// Test Table Migration with different Types
	/// </summary>
	[DataTable(TableName = "Test_TableMigrationTypes")]
	public class TestTableDifferentTypesV1 : DataObject
	{
		[DataElement(Varchar = 100)]
		public string StringValue { get; set; }
		[DataElement]
		public int IntValue { get; set; }
		[DataElement]
		public DateTime DateValue { get; set; }
	}
	
	/// <summary>
	/// Test Table Migration with different Types
	/// </summary>
	[DataTable(TableName = "Test_TableMigrationTypes")]
	public class TestTableDifferentTypesV2 : DataObject
	{
		[DataElement]
		public string StringValue { get; set; }
		[DataElement]
		public byte IntValue { get; set; }
		[DataElement]
		public string DateValue { get; set; }
	}	
}
