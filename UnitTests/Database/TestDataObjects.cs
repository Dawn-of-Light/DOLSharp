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
	/// Basic Test Table
	/// </summary>
	[DataTable(TableName = "Test_Table")]
	public class TestTable : DataObject
	{
		string m_testField;
		[DataElement]
		public string TestField { get { return m_testField; } set { Dirty = true; m_testField = value; } }
		
		public TestTable() { }
	}
	
	/// <summary>
	/// Basic Test Table with Auto Increment Primary Key
	/// </summary>
	[DataTable(TableName = "Test_TableAutoInc")]
	public class TestTableAutoInc : TestTable
	{
		int m_primaryKey;
		[PrimaryKey(AutoIncrement = true)]
		public int PrimaryKey { get { return m_primaryKey; } set { Dirty = true; m_primaryKey = value; } }
		
		public TestTableAutoInc() { }
	}
	
	/// <summary>
	/// Basic Test Table with Unique Field
	/// </summary>
	[DataTable(TableName = "Test_TableUniqueField")]
	public class TestTableUniqueField : TestTable
	{
		int m_unique;
		[DataElement(Unique = true)]
		public int Unique { get { return m_unique; } set { Dirty = true; m_unique = value; } }
		
		public TestTableUniqueField() { }
	}
	
	/// <summary>
	/// Basic Test Table with Relation 1-1
	/// </summary>
	[DataTable(TableName = "Test_TableRelation")]
	public class TestTableRelation : TestTable
	{
		[Relation(LocalField = "ObjectId", RemoteField = "Test_TableRelationEntry_ID", AutoLoad = true, AutoDelete = true)]
		public TestTableRelationEntry Entry;
		
		public TestTableRelation() { }
	}
	
	/// <summary>
	/// Basic Table with Relation Entry
	/// </summary>
	[DataTable(TableName = "Test_TableRelationEntry")]
	public class TestTableRelationEntry : TestTable
	{
		public TestTableRelationEntry() { }
	}
	
	/// <summary>
	/// Basic Test Table with Relation 1-n
	/// </summary>
	[DataTable(TableName = "Test_TableRelations")]
	public class TestTableRelations : TestTable
	{
		[Relation(LocalField = "ObjectId", RemoteField = "ForeignTestField", AutoLoad = true, AutoDelete = true)]
		public TestTableRelationsEntries[] Entries;
		
		public TestTableRelations() { }
	}
	
	/// <summary>
	/// Basic Table with Relations Entries
	/// </summary>
	[DataTable(TableName = "Test_TableRelationsEntries")]
	public class TestTableRelationsEntries : TestTable
	{
		string m_foreignTestField;
		[DataElement(Varchar = 255, Index = true)]
		public string ForeignTestField { get { return m_foreignTestField; } set { Dirty = true; m_foreignTestField = value; } }
		
		public TestTableRelationsEntries() { }
	}
	
	/// <summary>
	/// Test table handling Custom Params
	/// </summary>
	[DataTable(TableName = "Test_TableWithCustomParams")]
	public class TableWithCustomParams : DataObject
	{
		string m_testValue;
		[DataElement(Index = true, Varchar = 255, AllowDbNull = true)]
		public string TestValue { get { return m_testValue; } set { Dirty = true; m_testValue = value; } }
		
		[Relation(LocalField = "TestValue", RemoteField = "TestValue", AutoLoad = true, AutoDelete = true)]
		public TableCustomParams[] CustomParams;
		
		public TableWithCustomParams() { }
	}
	
	/// <summary>
	/// Test Custom Params Table for <see cref="TableWithCustomParams" />
	/// </summary>
	[DataTable(TableName = "Test_TableCustomParams")]
	public class TableCustomParams : CustomParam
	{	
		string m_testValue;
		[DataElement(Index = true, Varchar = 255, AllowDbNull = true)]
		public string TestValue { get { return m_testValue; } set { Dirty = true; m_testValue = value; } }
		
		public TableCustomParams() { }
		
		public TableCustomParams(string TestValue, string KeyName, string Value)
		{
			this.TestValue = TestValue;
			this.KeyName = KeyName;
			this.Value = Value;
		}
	}
}
