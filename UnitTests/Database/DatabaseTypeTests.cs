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

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Description of DatabaseTypeTests.
	/// </summary>
	[TestFixture]
	public class DatabaseTypeTests
	{
		public DatabaseTypeTests()
		{
		}
		
		[SetUp]
		public void SetUp()
		{
			DatabaseSetUp.Database.RegisterDataObject(typeof(ComplexTypeTestTable));
			DatabaseSetUp.Database.RegisterDataObject(typeof(ComplexTypeTestTableWithNull));			
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
			var inserted = DatabaseSetUp.Database.AddObject(obj);
			Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Boolean (NonNull) Test.");
			Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Boolean (NonNull) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = DatabaseSetUp.Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (NonNull) Test.", obj.PrimaryKey);
			Assert.AreEqual(obj.Bool, objRetrieved.Bool, "DatabaseTypeTests: in Boolean (NonNull) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Bool = true;
			var saved = DatabaseSetUp.Database.SaveObject(obj);
			Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Boolean (NonNull) Test.");
			Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Boolean (NonNull) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = DatabaseSetUp.Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
			Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (NonNull) Test.", obj.PrimaryKey);
			Assert.AreEqual(obj.Bool, objReRetrieved.Bool, "DatabaseTypeTests: in Boolean (NonNull) Saved Value and Retrieved Value should be Equal.");
		}
		
		[Test]
		public void TestBooleanNull()
		{
			var obj = new ComplexTypeTestTableWithNull();
			
			// Set Default Value for Tests.
			obj.Bool = false;
			
			// Test Add
			var inserted = DatabaseSetUp.Database.AddObject(obj);
			Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Boolean (Null) Test.");
			Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Boolean (Null) Test doesn't have Persisted Flag set.");
			
			// Test Read
			var objRetrieved = DatabaseSetUp.Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (Null) Test.", obj.PrimaryKey);
			Assert.AreEqual(obj.Bool, objRetrieved.Bool, "DatabaseTypeTests: in Boolean (Null) Insterted Value and Retrieved Value should be Equal.");
			
			// Test Save
			obj.Bool = true;
			var saved = DatabaseSetUp.Database.SaveObject(obj);
			Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Boolean (Null) Test.");
			Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Boolean (Null) Test still have Dirty Flag set.");
			
			// Test Re-Read
			var objReRetrieved = DatabaseSetUp.Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
			Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (Null) Test.", obj.PrimaryKey);
			Assert.AreEqual(obj.Bool, objReRetrieved.Bool, "DatabaseTypeTests: in Boolean (Null) Saved Value and Retrieved Value should be Equal.");
		}
	}
}
