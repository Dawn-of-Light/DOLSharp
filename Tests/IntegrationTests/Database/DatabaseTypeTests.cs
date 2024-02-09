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
using System.Linq;

using DOL.Database;
using NUnit.Framework;

namespace DOL.Integration.Database
{
	[TestFixture]
	public class DatabaseTypeTests
	{
		public DatabaseTypeTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		
		[OneTimeSetUp]
		public void SetUp()
		{
			Database.RegisterDataObject(typeof(ComplexTypeTestTable));
			foreach (var obj in Database.SelectAllObjects<ComplexTypeTestTable>())
				Database.DeleteObject(obj);
			Database.RegisterDataObject(typeof(ComplexTypeTestTableWithNull));
			foreach (var obj in Database.SelectAllObjects<ComplexTypeTestTableWithNull>())
				Database.DeleteObject(obj);
		}
		
		[Test]
		public void TestBooleanNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Bool = false;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Boolean (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Boolean (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Boolean (NonNull) Test.");
			Assert.That(objRetrieved.Bool, Is.EqualTo(obj.Bool), "DatabaseTypeTests: in Boolean (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Bool = true;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Boolean (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Boolean (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Boolean (NonNull) Test.");
			Assert.That(objReRetrieved.Bool, Is.EqualTo(obj.Bool), "DatabaseTypeTests: in Boolean (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestBooleanNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Bool = false;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Boolean (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Boolean (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Boolean (Null) Test.");
			Assert.That(objRetrieved.Bool, Is.EqualTo(obj.Bool), "DatabaseTypeTests: in Boolean (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Bool = true;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Boolean (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Boolean (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Boolean (Null) Test.");
			Assert.That(objReRetrieved.Bool, Is.EqualTo(obj.Bool), "DatabaseTypeTests: in Boolean (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestCharNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Char = char.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Char (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Char (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (NonNull) Test.");
			Assert.That(objRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Char = char.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Char (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Char (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (NonNull) Test.");
			Assert.That(objReRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Char = default(char);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Char (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Char (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (NonNull) Test.");
			Assert.That(objReReRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestCharNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Char = char.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Char (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Char (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (Null) Test.");
			Assert.That(objRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Char = char.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Char (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Char (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (Null) Test.");
			Assert.That(objReRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Char = default(char);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Char (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Char (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Char (Null) Test.");
			Assert.That(objReReRetrieved.Char, Is.EqualTo(obj.Char), "DatabaseTypeTests: in Char (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestByteNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Byte = byte.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Byte (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Byte (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (NonNull) Test.");
			Assert.That(objRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Byte = byte.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Byte (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Byte (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (NonNull) Test.");
			Assert.That(objReRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Byte = default(byte);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Byte (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Byte (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (NonNull) Test.");
			Assert.That(objReReRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestByteNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Byte = byte.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Byte (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Byte (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (Null) Test.");
			Assert.That(objRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Byte = byte.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Byte (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Byte (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (Null) Test.");
			Assert.That(objReRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Byte = default(byte);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Byte (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Byte (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Byte (Null) Test.");
			Assert.That(objReReRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Byte (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestSbyteNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Sbyte = sbyte.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Sbyte (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Sbyte (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (NonNull) Test.");
			Assert.That(objRetrieved.Sbyte, Is.EqualTo(obj.Sbyte), "DatabaseTypeTests: in Sbyte (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Sbyte = sbyte.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Sbyte (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Sbyte (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (NonNull) Test.");
			Assert.That(objReRetrieved.Sbyte, Is.EqualTo(obj.Sbyte), "DatabaseTypeTests: in Sbyte (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Sbyte = default(sbyte);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Sbyte (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Sbyte (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (NonNull) Test.");
			Assert.That(objReReRetrieved.Sbyte, Is.EqualTo(obj.Sbyte), "DatabaseTypeTests: in Sbyte (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestSbyteNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Sbyte = sbyte.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Sbyte (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Sbyte (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (Null) Test.");
			Assert.That(objRetrieved.Sbyte, Is.EqualTo(obj.Sbyte), "DatabaseTypeTests: in Sbyte (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Sbyte = sbyte.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Sbyte (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Sbyte (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (Null) Test.");
			Assert.That(objReRetrieved.Sbyte, Is.EqualTo(obj.Sbyte), "DatabaseTypeTests: in Sbyte (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Sbyte = default(sbyte);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Sbyte (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Sbyte (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Sbyte (Null) Test.");
			Assert.That(objReReRetrieved.Byte, Is.EqualTo(obj.Byte), "DatabaseTypeTests: in Sbyte (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestUShortNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.UShort = ushort.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in UShort (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in UShort (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (NonNull) Test.");
			Assert.That(objRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.UShort = ushort.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in UShort (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in UShort (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (NonNull) Test.");
			Assert.That(objReRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.UShort = default(ushort);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in UShort (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in UShort (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (NonNull) Test.");
			Assert.That(objReReRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestUShortNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.UShort = ushort.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in UShort (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in UShort (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (Null) Test.");
			Assert.That(objRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.UShort = ushort.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in UShort (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in UShort (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (Null) Test.");
			Assert.That(objReRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.UShort = default(ushort);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in UShort (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in UShort (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UShort (Null) Test.");
			Assert.That(objReReRetrieved.UShort, Is.EqualTo(obj.UShort), "DatabaseTypeTests: in UShort (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestShortNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Short = short.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Short (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Short (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (NonNull) Test.");
			Assert.That(objRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Short = short.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Short (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Short (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (NonNull) Test.");
			Assert.That(objReRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Short = default(short);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Short (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Short (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (NonNull) Test.");
			Assert.That(objReReRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestShortNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Short = short.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Short (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Short (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (Null) Test.");
			Assert.That(objRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Short = short.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Short (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Short (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (Null) Test.");
			Assert.That(objReRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Short = default(short);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Short (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Short (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Short (Null) Test.");
			Assert.That(objReReRetrieved.Short, Is.EqualTo(obj.Short), "DatabaseTypeTests: in Short (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestUIntNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.UInt = uint.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in UInt (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in UInt (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (NonNull) Test.");
			Assert.That(objRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.UInt = uint.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in UInt (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in UInt (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (NonNull) Test.");
			Assert.That(objReRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.UInt = default(uint);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in UInt (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in UInt (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (NonNull) Test.");
			Assert.That(objReReRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestUIntNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.UInt = uint.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in UInt (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in UInt (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (Null) Test.");
			Assert.That(objRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.UInt = uint.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in UInt (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in UInt (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (Null) Test.");
			Assert.That(objReRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.UInt = default(uint);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in UInt (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in UInt (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in UInt (Null) Test.");
			Assert.That(objReReRetrieved.UInt, Is.EqualTo(obj.UInt), "DatabaseTypeTests: in UInt (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestIntNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Int = int.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Int (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Int (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (NonNull) Test.");
			Assert.That(objRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Int = int.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Int (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Int (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (NonNull) Test.");
			Assert.That(objReRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Int = default(int);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Int (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Int (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (NonNull) Test.");
			Assert.That(objReReRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestIntNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Int = int.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Int (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Int (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (Null) Test.");
			Assert.That(objRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Int = int.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Int (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Int (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (Null) Test.");
			Assert.That(objReRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Int = default(int);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Int (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Int (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Int (Null) Test.");
			Assert.That(objReReRetrieved.Int, Is.EqualTo(obj.Int), "DatabaseTypeTests: in Int (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestULongNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.ULong = ulong.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in ULong (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in ULong (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (NonNull) Test.");
			Assert.That(objRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.ULong = ulong.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in ULong (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in ULong (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (NonNull) Test.");
			Assert.That(objReRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.ULong = default(ulong);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in ULong (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in ULong (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (NonNull) Test.");
			Assert.That(objReReRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestULongNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.ULong = ulong.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in ULong (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in ULong (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (Null) Test.");
			Assert.That(objRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.ULong = ulong.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in ULong (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in ULong (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (Null) Test.");
			Assert.That(objReRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.ULong = default(ulong);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in ULong (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in ULong (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in ULong (Null) Test.");
			Assert.That(objReReRetrieved.ULong, Is.EqualTo(obj.ULong), "DatabaseTypeTests: in ULong (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestLongNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Long = long.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Long (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Long (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (NonNull) Test.");
			Assert.That(objRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Long = long.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Long (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Long (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (NonNull) Test.");
			Assert.That(objReRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Long = default(long);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Long (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Long (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (NonNull) Test.");
			Assert.That(objReReRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestLongNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Long = long.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Long (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Long (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (Null) Test.");
			Assert.That(objRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Long = long.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Long (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Long (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (Null) Test.");
			Assert.That(objReRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Long = default(long);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Long (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Long (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Long (Null) Test.");
			Assert.That(objReReRetrieved.Long, Is.EqualTo(obj.Long), "DatabaseTypeTests: in Long (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestFloatNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Float = float.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Float (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Float (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (NonNull) Test.");
			Assert.That(objRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Float = float.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Float (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Float (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (NonNull) Test.");
			Assert.That(objReRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Float = (float)0.1;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Float (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Float (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (NonNull) Test.");
			Assert.That(objReReRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestFloatNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Float = float.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Float (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Float (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (Null) Test.");
			Assert.That(objRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Float = float.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Float (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Float (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (Null) Test.");
			Assert.That(objReRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Float = (float)0.1;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Float (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Float (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Float (Null) Test.");
			Assert.That(objReReRetrieved.Float, Is.EqualTo(obj.Float), "DatabaseTypeTests: in Float (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestDoubleNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Double = double.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Double (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Double (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (NonNull) Test.");
			Assert.That(objRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Double = double.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Double (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Double (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (NonNull) Test.");
			Assert.That(objReRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Double = (double)0.1;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Double (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Double (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (NonNull) Test.");
			Assert.That(objReReRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestDoubleNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Double = double.MinValue;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Double (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Double (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (Null) Test.");
			Assert.That(objRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Double = double.MaxValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Double (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Double (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (Null) Test.");
			Assert.That(objReRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Double = (double)0.1;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Double (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Double (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Double (Null) Test.");
			Assert.That(objReReRetrieved.Double, Is.EqualTo(obj.Double), "DatabaseTypeTests: in Double (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestDateTimeNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.DateTime = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in DateTime (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in DateTime (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (NonNull) Test.");
			Assert.That(objRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.DateTime = DateTime.MinValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in DateTime (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in DateTime (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (NonNull) Test.");
			Assert.That(objReRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.DateTime = default(DateTime);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in DateTime (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in DateTime (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (NonNull) Test.");
			Assert.That(objReReRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestDateTimeNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.DateTime = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in DateTime (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in DateTime (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (Null) Test.");
			Assert.That(objRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.DateTime = DateTime.MinValue;
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in DateTime (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in DateTime (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (Null) Test.");
			Assert.That(objReRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.DateTime = default(DateTime);
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in DateTime (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in DateTime (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in DateTime (Null) Test.");
			Assert.That(objReReRetrieved.DateTime, Is.EqualTo(obj.DateTime), "DatabaseTypeTests: in DateTime (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestStringNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.String = "a";
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in String (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in String (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (NonNull) Test.");
			Assert.That(objRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.String = new string(Enumerable.Range(0, 199).Select(i => 'z').Concat(new [] { '@' }).ToArray());
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in String (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in String (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (NonNull) Test.");
			Assert.That(objReRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.String = string.Empty;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in String (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in String (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (NonNull) Test.");
			Assert.That(objReReRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestStringNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.String = null;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in String (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in String (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (Null) Test.");
			Assert.That(objRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.String = new string(Enumerable.Range(0, 199).Select(i => 'z').Concat(new [] { '@' }).ToArray());
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in String (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in String (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (Null) Test.");
			Assert.That(objReRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.String = "a";
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in String (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in String (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in String (Null) Test.");
			Assert.That(objReReRetrieved.String, Is.EqualTo(obj.String), "DatabaseTypeTests: in String (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestTextNonNull()
		{
			var obj = new ComplexTypeTestTable();
			// Set Value that should not be null
			obj.String = string.Empty;
			obj.Text = string.Empty;
			obj.DateTime = DateTime.Now;
			
			// Set Default Value for Tests.
			obj.Text = "a";
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Text (NonNull) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Text (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (NonNull) Test.");
			Assert.That(objRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Text = new string(Enumerable.Range(0, 65534).Select(i => 'z').Concat(new [] { '@' }).ToArray());
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Text (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Text (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (NonNull) Test.");
			Assert.That(objReRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (NonNull) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Text = string.Empty;
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Text (NonNull) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Text (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (NonNull) Test.");
			Assert.That(objReReRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestTextNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Text = null;
			
			// Test Add
			var inserted = Database.AddObject(obj);
			Assert.That(inserted, Is.True, "DatabaseTypeTests: Could not insert object in Text (Null) Test.");
			Assert.That(obj.IsPersisted, Is.True, "DatabaseTypeTests: Inserted Object in Text (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (Null) Test.");
			Assert.That(objRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Text = new string(Enumerable.Range(0, 65534).Select(i => 'z').Concat(new [] { '@' }).ToArray());
			var saved = Database.SaveObject(obj);
			Assert.That(saved, Is.True, "DatabaseTypeTests: Could not save objet in Text (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Saved Object in Text (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (Null) Test.");
			Assert.That(objReRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (Null) Saved Value and Retrieved Value should be Equal.");
			
			// Test Re-Save
			obj.Text = "a";
			var reSaved = Database.SaveObject(obj);
			Assert.That(reSaved, Is.True, "DatabaseTypeTests: Could not Re-Save objet in Text (Null) Test.");
			Assert.That(obj.Dirty, Is.False, "DatabaseTypeTests: Re-Saved Object in Text (Null) Test still have Dirty Flag set.");
			
			// Test Re-Re-Read
			var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.That(objReReRetrieved, Is.Not.Null, $"DatabaseTypeTests: Could not retrieve object (ID {obj.PrimaryKey}) in Text (Null) Test.");
			Assert.That(objReReRetrieved.Text, Is.EqualTo(obj.Text), "DatabaseTypeTests: in Text (Null) Saved Value and Retrieved Value should be Equal.");
		}
		
	}
}
