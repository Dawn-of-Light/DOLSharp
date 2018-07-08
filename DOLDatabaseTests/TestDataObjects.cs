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
        [Relation(LocalField = "Test_TableRelation_ID", RemoteField = "Test_TableRelationEntry_ID", AutoLoad = true, AutoDelete = true)]
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
        [Relation(LocalField = "Test_TableRelations_ID", RemoteField = "ForeignTestField", AutoLoad = true, AutoDelete = true)]
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
    /// Basic Table with Multiple Unique Constraints
    /// </summary>
    [DataTable(TableName = "Test_TableMultiUnique")]
    public class TestTableMultiUnique : TestTable
    {
        string m_strUniquePart;

        [DataElement(UniqueColumns="IntUniquePart")]
        public string StrUniquePart { get { return m_strUniquePart; } set { Dirty = true; m_strUniquePart = value; } }

        int m_intUniquePart;

        [DataElement]
        public int IntUniquePart { get { return m_intUniquePart; } set { Dirty = true; m_intUniquePart = value; } }

        public TestTableMultiUnique() { }
    }

    /// <summary>
    /// Basic Table with Primary Key Constraint
    /// </summary>
    [DataTable(TableName = "Test_TablePrimaryKey")]
    public class TestTablePrimaryKey : TestTable
    {
        string m_primaryKey;

        [PrimaryKey]
        public string PrimaryKey { get { return m_primaryKey; } set { Dirty = true; m_primaryKey = value; } }
    }

    /// <summary>
    /// Basic Table with Primary Key Constraint and Unique Key Constraint
    /// </summary>
    [DataTable(TableName = "Test_TablePrimaryKeyUnique")]
    public class TestTablePrimaryKeyUnique : TestTablePrimaryKey
    {
        string m_unique;

        [DataElement(Unique = true)]
        public string Unique { get { return m_unique; } set { Dirty = true; m_unique = value; } }
    }

    /// <summary>
    /// Basic Table with Relations and no AutoLoad
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsNoAutoload")]
    public class TestTableRelationsWithNoAutoLoad : TestTable
    {
        [Relation(LocalField = "Test_TableRelationsNoAutoload_ID", RemoteField = "ForeignTestField", AutoLoad = false, AutoDelete = true)]
        public TestTableRelationsEntries[] Entries;

        public TestTableRelationsWithNoAutoLoad() { }
    }

    /// <summary>
    /// Basic Table with Relations and no AutoDelete
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsNoAutodelete")]
    public class TestTableRelationsWithNoAutoDelete : TestTable
    {
        [Relation(LocalField = "Test_TableRelationsNoAutodelete_ID", RemoteField = "ForeignTestField", AutoLoad = true, AutoDelete = false)]
        public TestTableRelationsEntries[] Entries;

        public TestTableRelationsWithNoAutoDelete() { }
    }

    /// <summary>
    /// Basic Table with Relations with Precache
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsPrecache")]
    public class TestTableRelationsWithPrecache : TestTable
    {
        [Relation(LocalField = "Test_TableRelationsPrecache_ID", RemoteField = "ForeignTestField", AutoLoad = true, AutoDelete = true)]
        public TestTableRelationsEntriesPrecached[] Entries;

        public TestTableRelationsWithPrecache() { }
    }

    /// <summary>
    /// Basic Table with Relations Entries Precached
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsEntriesPrecached", PreCache = true)]
    public class TestTableRelationsEntriesPrecached : TestTable
    {
        string m_foreignTestField;

        [DataElement(Varchar = 255, Index = true)]
        public string ForeignTestField { get { return m_foreignTestField; } set { Dirty = true; m_foreignTestField = value; } }

        public TestTableRelationsEntriesPrecached() { }
    }

    /// <summary>
    /// Basic Table with Relations with Precache
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsPrecachePrimary")]
    public class TestTableRelationsWithPrecacheAndPrimary : TestTable
    {
        [Relation(LocalField = "Test_TableRelationsPrecachePrimary_ID", RemoteField = "Test_TableRelationsEntryPrecached_ID", AutoLoad = true, AutoDelete = true)]
        public TestTableRelationsEntryPrecached Entry;

        public TestTableRelationsWithPrecacheAndPrimary() { }
    }

    /// <summary>
    /// Basic Table with Relations Entries Precached
    /// </summary>
    [DataTable(TableName = "Test_TableRelationsEntryPrecached", PreCache = true)]
    public class TestTableRelationsEntryPrecached : TestTable
    {
        string m_foreignTestField;

        [DataElement(Varchar = 255, Index = true)]
        public string ForeignTestField { get { return m_foreignTestField; } set { Dirty = true; m_foreignTestField = value; } }

        public TestTableRelationsEntryPrecached() { }
    }

    /// <summary>
    /// Basic Table with ReadOnly field
    /// </summary>
    [DataTable(TableName = "Test_TableReadOnly")]
    public class TestTableWithReadOnly : TestTable
    {
        [ReadOnly]
        [DataElement]
        public string ReadOnly { get; set; }

        public TestTableWithReadOnly() { }
    }

    /// <summary>
    /// Basic Table for Base View
    /// Base Table for View should better use some well defined Primary Key for DML !!
    /// </summary>
    [DataTable(TableName = "Test_TableBaseView")]
    public class TestTableBaseView : TestTableAutoInc
    {
        string m_viewValue;

        [DataElement]
        public string ViewValue { get { return m_viewValue; } set { Dirty = true; m_viewValue = value; } }
    }

    /// <summary>
    /// Basic Table being a View of Base View
    /// </summary>
    [DataTable(TableName = "Test_TableBaseView", ViewName = "Test_TableAsView", ViewAs = "SELECT *, 'Weird Indeed' as `WeirdValue` FROM {0} WHERE `ViewValue` != 'HIDDEN'")]
    public class TestTableAsView : TestTableBaseView
    {
        [DataElement]
        public string WeirdValue { get; set; }
    }

    /// <summary>
    /// Basic Table being a View of Base View and Implementing a Relation based on existing field.
    /// </summary>
    [DataTable(TableName = "Test_TableBaseView", ViewName = "Test_TableAsViewWithRelations", ViewAs = "SELECT * FROM {0}")]
    public class TestTableAsViewWithRelations : TestTableBaseView
    {
        [Relation(LocalField = "ViewValue", RemoteField = "ForeignTestField", AutoLoad = true, AutoDelete = true)]
        public TestTableRelationsEntries[] Entries;
    }

    /// <summary>
    /// Basic Table with Primary Key using PreCache Behavior
    /// </summary>
    [DataTable(TableName = "", PreCache = true)]
    public class TestTablePrecachedPrimaryKey : TestTablePrimaryKey
    {
        string m_precachedValue;

        [DataElement]
        public string PrecachedValue { get { return m_precachedValue; } set { Dirty = true; m_precachedValue = value; } }

        public TestTablePrecachedPrimaryKey() { }
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

    /// <summary>
    /// Test table that shouldn't be registered to database
    /// </summary>
    public class TableNotRegistered : DataObject
    {
        public TableNotRegistered() { }
    }
}
