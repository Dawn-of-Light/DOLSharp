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

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Basic Database Tests
	/// </summary>
	[TestFixture]
	public class DatabaseTests
	{
		public DatabaseTests()
		{
		}
		
		/// <summary>
		/// Basic Tests For a Test Table
		/// </summary>
		[Test]
		public void TestTable()
		{
			// Prepare and Cleanup
			DatabaseSetUp.Database.RegisterDataObject(typeof(TestTable));
			
			var all = DatabaseSetUp.Database.SelectAllObjects<TestTable>();
			
			foreach(var obj in all)
				DatabaseSetUp.Database.DeleteObject(obj);
			
			var none = DatabaseSetUp.Database.SelectAllObjects<TestTable>();
			
			Assert.IsEmpty(none, "Database shouldn't have any record For TestTable.");
			
			var testValues = new [] { "TestValue 1", "TestValue 2", "TestValue 3" };
			
			// Add Some Data
			foreach (var values in testValues)
			{
				var data = new TestTable()
				{
					TestField = values,
				};
				
				var inserted = DatabaseSetUp.Database.AddObject(data);
				Assert.IsTrue(inserted, "TestTable Entry not Inserted properly for Value {0}.", values);
			}
			
			var retrieve = DatabaseSetUp.Database.SelectAllObjects<TestTable>();
			
			Assert.AreEqual(testValues.Length, retrieve.Count, "Retrieved Test Table Entries Count is not equals to Test Values Count.");
			
			CollectionAssert.AreEquivalent(testValues, retrieve.Select(o => o.TestField), "Retrieved Test Entries and Test Values should be Equivalent.");
			Assert.IsTrue(retrieve.All(o => o.IsPersisted), "All Retrieved Test Entries should be Persisted in database.");
			
			// Modify Some Data
			var modObj = retrieve.First(o => o.TestField == testValues.First());
			modObj.TestField = "TestValue 4";
			
			Assert.IsTrue(modObj.Dirty, "Test Table Object should be Dirty after Modifications.");
			
			var saved = DatabaseSetUp.Database.SaveObject(modObj);
			
			Assert.IsTrue(saved, "Test Table Object could not be saved correctly.");
			
			Assert.IsFalse(modObj.Dirty, "Test Table Object should not be Dirty after Object Save.");
			
			testValues = new [] { modObj.TestField, "TestValue 2", "TestValue 3" };

			retrieve = DatabaseSetUp.Database.SelectAllObjects<TestTable>();
			
			CollectionAssert.AreEquivalent(testValues, retrieve.Select(o => o.TestField), "Retrieved Test Entries after Modification should be Equivalent to Test Values.");
			
			// Delete Some Data
			
			var delObj = retrieve.First();
			
			var deleted = DatabaseSetUp.Database.DeleteObject(delObj);
			
			Assert.IsTrue(deleted, "Test Table Object could not be deleted correctly.");
			Assert.IsTrue(delObj.IsDeleted, "Test Table Deleted Object does not have delete flags set correctly.");
			
			testValues = retrieve.Skip(1).Select(o => o.TestField).ToArray();
			
			retrieve = DatabaseSetUp.Database.SelectAllObjects<TestTable>();
			
			CollectionAssert.AreEquivalent(testValues, retrieve.Select(o => o.TestField), "Retrieved Test Entries after Deletion should be Equivalent to Test Values.");
			
			// Find Object By Key
			var keyObject = retrieve.First();
			Assert.IsNotNullOrEmpty(keyObject.ObjectId, "Test Table Retrieved Object should have an Object Id");
			
			var retrieveKeyObj = DatabaseSetUp.Database.FindObjectByKey<TestTable>(keyObject.ObjectId);
			Assert.IsNotNull(retrieveKeyObj, "Test Table Retrieved Object by Key should not be null.");
			Assert.AreEqual(retrieveKeyObj.ObjectId, keyObject.ObjectId, "Test Table Key Object and Retrieved Key Object should have same Object Id.");
			Assert.AreEqual(retrieveKeyObj.TestField, keyObject.TestField, "Test Table Key Object and Retrieved Key Object should have same Values.");
		}
		
		/// <summary>
		/// Test Table with Primary Auto Increment
		/// </summary>
		[Test]
		public void TestTableAutoIncrement()
		{
			// Prepare and Cleanup
			DatabaseSetUp.Database.RegisterDataObject(typeof(TestTableAutoInc));
			
			var all = DatabaseSetUp.Database.SelectAllObjects<TestTableAutoInc>();
			
			foreach(var obj in all)
				DatabaseSetUp.Database.DeleteObject(obj);
			
			var none = DatabaseSetUp.Database.SelectAllObjects<TestTableAutoInc>();
			
			Assert.IsEmpty(none, "Database shouldn't have any record For TestTableAutoInc.");
			
			var addObj = new TestTableAutoInc() { TestField = "Test AutoInc" };
			
			// Insert a Test Object for guessing last auto increment
			var inserted = DatabaseSetUp.Database.AddObject(addObj);
			
			Assert.IsTrue(inserted, "Test Table Auto Inc could not insert a new Entry.");
			
			var autoInc = addObj.PrimaryKey;
			
			Assert.AreNotEqual(autoInc, default(int), "Test Table Auto Inc Primary should not be Default value after insertion.");
			
			// Add Another Object to Check Primary Key Increment
			var otherObj = new TestTableAutoInc() { TestField = "Test AutoInc Other" };
			
			var otherInsert = DatabaseSetUp.Database.AddObject(otherObj);
			
			Assert.IsTrue(otherInsert, "Test Table Auto Inc could not insert an other Entry.");
			
			var otherAutoInc = otherObj.PrimaryKey;
			Assert.Greater(otherAutoInc, autoInc, "Newly Inserted Test Table Auto Inc Other Entry should have a Greater Primary Key Increment.");
			
			// Try Deleting and Re-inserting
			var reDeleted = DatabaseSetUp.Database.DeleteObject(otherObj);
			Assert.IsTrue(reDeleted, "Test Table Auto Inc could not delete other Entry from Table.");
			Assert.IsTrue(otherObj.IsDeleted, "Test Table Auto Inc other Entry deleted Flag should be true.");
			Assert.IsFalse(otherObj.IsPersisted, "Test Table Auto Inc other Entry Persisted Flag should be false.");
			
			otherObj.PrimaryKey = default(int);
			var reInserted = DatabaseSetUp.Database.AddObject(otherObj);
			Assert.IsTrue(reInserted, "Test Table Auto Inc could not insert other Entry in Table again.");
			
			Assert.Greater(otherObj.PrimaryKey, otherAutoInc, "Re-Added Test Table Auto Inc Entry should have a Greater Primary Key Increment.");
			
			// Try modifying to check that Primary Key is Used for Update Where Clause
			otherObj.TestField = "Test AutoInc Other Modified !";
			Assert.IsTrue(otherObj.Dirty, "Test Table Auto Inc Other Object should be Dirty after Modifications.");
			var modified = DatabaseSetUp.Database.SaveObject(otherObj);
			Assert.IsTrue(modified, "Test Table Auto Inc Other Object could not be Modified.");
			Assert.IsFalse(otherObj.Dirty, "Test Table Auto Inc Other Object should not be Dirty after save.");
			
			var retrieve = DatabaseSetUp.Database.FindObjectByKey<TestTableAutoInc>(otherObj.PrimaryKey);
			Assert.IsNotNull(retrieve, "Test Table Auto Inc Other Object could not be Retrieved through Primary Key.");
			Assert.AreEqual(otherObj.TestField, retrieve.TestField, "Test Table Auto Inc Retrieved Object is different from Other Object.");
		}
	}
}
