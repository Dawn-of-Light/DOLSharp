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
using DOL.Database.UniqueID;

using NUnit.Framework;

namespace DOL.Integration.Database
{
	[TestFixture]
	public class DatabaseTests
	{
		public DatabaseTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		
		[Test]
		public void TestTable()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTable));
			
			var all = Database.SelectAllObjects<TestTable>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTable>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTable.");
			
			var testValues = new [] { "TestValue 1", "TestValue 2", "TestValue 3" };
			
			// Add Some Data
			foreach (var values in testValues)
			{
				var data = new TestTable()
				{
					TestField = values,
				};
				
				var inserted = Database.AddObject(data);
				Assert.That(inserted, Is.True, "TestTable Entry not Inserted properly for Value {0}.", values);
			}
			
			var retrieve = Database.SelectAllObjects<TestTable>();
			
			Assert.That(retrieve.Count, Is.EqualTo(testValues.Length), "Retrieved Test Table Entries Count is not equals to Test Values Count.");
			
			Assert.That(retrieve.Select(o => o.TestField), Is.EquivalentTo(testValues), "Retrieved Test Entries and Test Values should be Equivalent.");
			Assert.That(retrieve.All(o => o.IsPersisted), Is.True, "All Retrieved Test Entries should be Persisted in database.");
			
			// Modify Some Data
			var modObj = retrieve.First(o => o.TestField == testValues.First());
			modObj.TestField = "TestValue 4";
			
			Assert.That(modObj.Dirty, Is.True, "Test Table Object should be Dirty after Modifications.");
			
			var saved = Database.SaveObject(modObj);
			
			Assert.That(saved, Is.True, "Test Table Object could not be saved correctly.");
			
			Assert.That(modObj.Dirty, Is.False, "Test Table Object should not be Dirty after Object Save.");
			
			testValues = new [] { modObj.TestField, "TestValue 2", "TestValue 3" };

			retrieve = Database.SelectAllObjects<TestTable>();
			
			Assert.That(retrieve.Select(o => o.TestField), Is.EquivalentTo(testValues), "Retrieved Test Entries after Modification should be Equivalent to Test Values.");
			
			// Delete Some Data
			
			var delObj = retrieve.First();
			
			var deleted = Database.DeleteObject(delObj);
			
			Assert.That(deleted, Is.True, "Test Table Object could not be deleted correctly.");
			Assert.That(delObj.IsDeleted, Is.True, "Test Table Deleted Object does not have delete flags set correctly.");
			
			testValues = retrieve.Skip(1).Select(o => o.TestField).ToArray();
			
			retrieve = Database.SelectAllObjects<TestTable>();
			
			Assert.That(retrieve.Select(o => o.TestField), Is.EquivalentTo(testValues), "Retrieved Test Entries after Deletion should be Equivalent to Test Values.");
			
			// Find Object By Key
			var keyObject = retrieve.First();
			Assert.That(keyObject.ObjectId, Is.Not.Null.Or.Empty, "Test Table Retrieved Object should have an Object Id");
			
			var retrieveKeyObj = Database.FindObjectByKey<TestTable>(keyObject.ObjectId);
			Assert.That(retrieveKeyObj, Is.Not.Null, "Test Table Retrieved Object by Key should not be null.");
			Assert.That(keyObject.ObjectId, Is.EqualTo(retrieveKeyObj.ObjectId), "Test Table Key Object and Retrieved Key Object should have same Object Id.");
			Assert.That(keyObject.TestField, Is.EqualTo(retrieveKeyObj.TestField), "Test Table Key Object and Retrieved Key Object should have same Values.");

			// Find Objects by Key
			var keys = retrieve.Take(2).Select(r => r.ObjectId).Append("__bad__id__").ToList();
			var retrieveKeyObjects = Database.FindObjectsByKey<TestTable>(keys);
			Assert.That(retrieveKeyObjects, Is.Not.Null, "Test Table Retrieved Objects by Key should not be null.");
			Assert.That(retrieveKeyObjects.Count == 3, Is.True, "Test Table Retrieved Objects by Key should contains 3 objects");
			Assert.That(keys.Contains(retrieveKeyObjects[0].ObjectId), Is.True, "Test Table first Key Object and Retrieved first Key Object should have same Object Id.");
			Assert.That(keys.Contains(retrieveKeyObjects[1].ObjectId), Is.True, "Test Table second Key Object and Retrieved second Key Object should have same Object Id.");
			Assert.That(retrieveKeyObjects[2], Is.Null, "Test Table third object should be null (it's an invalid id)");
		}
		
		/// <summary>
		/// Test Table with Primary Auto Increment
		/// </summary>
		[Test]
		public void TestTableAutoIncrement()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			
			var all = Database.SelectAllObjects<TestTableAutoInc>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTableAutoInc>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableAutoInc.");
			
			var addObj = new TestTableAutoInc() { TestField = "Test AutoInc" };
			
			// Insert a Test Object for guessing last auto increment
			var inserted = Database.AddObject(addObj);
			
			Assert.That(inserted, Is.True, "Test Table Auto Inc could not insert a new Entry.");
			
			var autoInc = addObj.PrimaryKey;
			
			Assert.That(default(int), Is.Not.EqualTo(autoInc), "Test Table Auto Inc Primary should not be Default value after insertion.");
			
			// Add Another Object to Check Primary Key Increment
			var otherObj = new TestTableAutoInc() { TestField = "Test AutoInc Other" };
			
			var otherInsert = Database.AddObject(otherObj);
			
			Assert.That(otherInsert, Is.True, "Test Table Auto Inc could not insert an other Entry.");
			
			var otherAutoInc = otherObj.PrimaryKey;
			Assert.That(otherAutoInc, Is.GreaterThan(autoInc), "Newly Inserted Test Table Auto Inc Other Entry should have a Greater Primary Key Increment.");
			
			// Try Deleting and Re-inserting
			var reDeleted = Database.DeleteObject(otherObj);
			Assert.That(reDeleted, Is.True, "Test Table Auto Inc could not delete other Entry from Table.");
			Assert.That(otherObj.IsDeleted, Is.True, "Test Table Auto Inc other Entry deleted Flag should be true.");
			Assert.That(otherObj.IsPersisted, Is.False, "Test Table Auto Inc other Entry Persisted Flag should be false.");
			
			otherObj.PrimaryKey = default(int);
			var reInserted = Database.AddObject(otherObj);
			Assert.That(reInserted, Is.True, "Test Table Auto Inc could not insert other Entry in Table again.");
			
			Assert.That(otherObj.PrimaryKey, Is.GreaterThan(otherAutoInc), "Re-Added Test Table Auto Inc Entry should have a Greater Primary Key Increment.");
			
			// Try modifying to check that Primary Key is Used for Update Where Clause
			otherObj.TestField = "Test AutoInc Other Modified !";
			Assert.That(otherObj.Dirty, Is.True, "Test Table Auto Inc Other Object should be Dirty after Modifications.");
			var modified = Database.SaveObject(otherObj);
			Assert.That(modified, Is.True, "Test Table Auto Inc Other Object could not be Modified.");
			Assert.That(otherObj.Dirty, Is.False, "Test Table Auto Inc Other Object should not be Dirty after save.");
			
			var retrieve = Database.FindObjectByKey<TestTableAutoInc>(otherObj.PrimaryKey);
			Assert.That(retrieve, Is.Not.Null, "Test Table Auto Inc Other Object could not be Retrieved through Primary Key.");
			Assert.That(retrieve.TestField, Is.EqualTo(otherObj.TestField), "Test Table Auto Inc Retrieved Object is different from Other Object.");
		}
		
		/// <summary>
		/// Test Table with Unique Field
		/// </summary>
		[Test]
		public void TestTableUnique()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableUniqueField));
			
			var all = Database.SelectAllObjects<TestTableUniqueField>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTableUniqueField>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableUniqueField.");
			
			// Test Add
			var uniqueObj = new TestTableUniqueField { TestField = "Test Value Unique", Unique = 1 };
			
			var inserted = Database.AddObject(uniqueObj);
			
			Assert.That(inserted, Is.True, "Test Table Unique Field could not insert unique object.");
			
			// Try Adding with unique Value
			var otherUniqueObj = new TestTableUniqueField { TestField = "Test Value Other Unique", Unique = 1 };
			
			var otherInserted = Database.AddObject(otherUniqueObj);
			
			Assert.That(otherInserted, Is.False, "Test Table Unique Field with Other Object violating unique constraint should not be inserted.");
			
			// Try Adding with non unique Value
			var otherNonUniqueObj = new TestTableUniqueField { TestField = "Test Value Other Non-Unique", Unique = 2 };
			
			var nonUniqueInserted = Database.AddObject(otherNonUniqueObj);
			
			Assert.That(nonUniqueInserted, Is.True, "Test Table Unique Field with Other Non Unique Object could not be inserted");
			
			// Try saving with unique Value
			var retrieved = Database.FindObjectByKey<TestTableUniqueField>(otherNonUniqueObj.ObjectId);
			
			retrieved.Unique = 1;
			
			var saved = Database.SaveObject(retrieved);
			
			Assert.That(saved, Is.False, "Test Table Unique Field with Retrieved Object violating unique constraint should not be saved.");
			
			// Delete Previous Unique and Try Reinsert.
			var deleted = Database.DeleteObject(uniqueObj);
			Assert.That(deleted, Is.True, "Test Table Unique Field could not delete unique object.");
			Assert.That(uniqueObj.IsDeleted, Is.True, "Test Table Unique Field unique object should have delete flag set.");
			
			var retrievedSaved = Database.SaveObject(retrieved);
			
			Assert.That(retrievedSaved, Is.True, "Test Table Unique Field Retrieved Object could not be inserted after deleting previous constraint violating object.");
		}
		
		/// <summary>
		/// Test Table with Multiple Unique Fields
		/// </summary>
		[Test]
		public void TestTableMultiUnique()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableMultiUnique));
			
			var all = Database.SelectAllObjects<TestTableMultiUnique>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTableMultiUnique>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableMultiUnique.");
			// Test Add
			var uniqueObj = new TestTableMultiUnique { StrUniquePart = "Test Value Unique", IntUniquePart = 1 };
			
			var inserted = Database.AddObject(uniqueObj);
			
			Assert.That(inserted, Is.True, "Test Table Unique Multiple Field could not insert unique object.");
			
			// Try Adding with unique Value
			var otherUniqueObj = new TestTableMultiUnique { StrUniquePart = "Test Value Unique", IntUniquePart = 1 };
			
			var otherInserted = Database.AddObject(otherUniqueObj);
			
			Assert.That(otherInserted, Is.False, "Test Table Unique Multiple Field with Other Object violating unique constraint should not be inserted.");
			
			// Try Adding with non unique Value
			var otherNonUniqueObj = new TestTableMultiUnique { StrUniquePart = "Test Value Other Non-Unique", IntUniquePart = 2 };
			
			var nonUniqueInserted = Database.AddObject(otherNonUniqueObj);
			
			Assert.That(nonUniqueInserted, Is.True, "Test Table Unique Multiple Field with Other Non Unique Object could not be inserted");
			
			// Try saving with unique Value
			var retrieved = Database.FindObjectByKey<TestTableMultiUnique>(otherNonUniqueObj.ObjectId);
			
			retrieved.StrUniquePart = "Test Value Unique";
			retrieved.IntUniquePart = 1;
			
			var saved = Database.SaveObject(retrieved);
			
			Assert.That(saved, Is.False, "Test Table Unique Multiple Field with Retrieved Object violating unique constraint should not be saved.");
			
			// Delete Previous Unique and Try Reinsert.
			var deleted = Database.DeleteObject(uniqueObj);
			Assert.That(deleted, Is.True, "Test Table Unique Field could not delete unique object.");
			Assert.That(uniqueObj.IsDeleted, Is.True, "Test Table Unique Field unique object should have delete flag set.");
			
			var retrievedSaved = Database.SaveObject(retrieved);
			
			Assert.That(retrievedSaved, Is.True, "Test Table Unique Field Retrieved Object could not be inserted after deleting previous constraint violating object.");
		}
		
		/// <summary>
		/// Test Table with Relation 1-1
		/// </summary>
		[Test]
		public void TestTableRelation()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableRelation));
			Database.RegisterDataObject(typeof(TestTableRelationEntry));
			
			var all = Database.SelectAllObjects<TestTableRelationEntry>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTableRelationEntry>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableRelationEntry.");
			
			var allrel = Database.SelectAllObjects<TestTableRelation>();
			
			foreach(var obj in allrel)
				Database.DeleteObject(obj);
			
			var nonerel = Database.SelectAllObjects<TestTableRelation>();
			
			Assert.That(nonerel, Is.Empty, "Database shouldn't have any record For TestTableRelation.");
			
			// Try Add with no Relation
			var noRelObj = new TestTableRelation() { TestField = "RelationTestValue" };
			
			var inserted = Database.AddObject(noRelObj);
			
			Assert.That(inserted, Is.True, "Test Table Relation could not insert object with no relation.");
			Assert.That(noRelObj.Entry, Is.Null, "Test Table Relation object with no relation should have null Entry.");
			
			// Try Select
			var selectInserted = Database.SelectAllObjects<TestTableRelation>();
			Assert.That(selectInserted, Is.Not.Empty, "Test Table Relation could not retrieve objects without relation.");
			
			// Try Adding Relation
			var relObj = new TestTableRelationEntry() { TestField = "RelationEntryTestValue", ObjectId = noRelObj.ObjectId };
			
			var relInserted = Database.AddObject(relObj);
			
			Assert.That(relInserted, Is.True, "Test Table Relation Entry could not be inserted.");
			
			noRelObj.Entry = relObj;
			
			var saved = Database.SaveObject(noRelObj);
			
			Assert.That(saved, Is.True, "Test Table Relation could not save Object with a new relation Added.");
			
			// Try Retrieving Relation
			var retrieve = Database.FindObjectByKey<TestTableRelation>(noRelObj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Test Table Relation could not retrieve relation object by ObjectId.");
			Assert.That(retrieve.Entry, Is.Not.Null, "Test Table Relation retrieved object have no entry object.");
			Assert.That(retrieve.Entry.TestField, Is.EqualTo(relObj.TestField), "Test Table Relation retrieved object Entry Relation is different from created object.");
			
			// Try Deleting Relation
			var deleted = Database.DeleteObject(noRelObj);
			
			Assert.That(deleted, Is.True, "Test Table Relation could not delete object with relation.");
			Assert.That(noRelObj.IsDeleted, Is.True, "Test Table Relation deleted object should have deleted flag set.");
			Assert.That(noRelObj.Entry.IsDeleted, Is.True, "Test Table Relation deleted object Entry should have deleted flag set.");
			
			// Check that Relation was deleted
			var relRetrieve = Database.FindObjectByKey<TestTableRelationEntry>(relObj.ObjectId);
			
			Assert.That(relRetrieve, Is.Null, "Test Table Relation Entry was not auto deleted with relation object.");
			Assert.That(relObj.IsDeleted, Is.True, "Test Table Relation Entry should have deleted flag set after auto delete.");
			
			// Check Null Relation
			var nullObj = new TestTableRelation() { TestField = "RelationNullTestValue" };
			
			var nullAdded = Database.AddObject(nullObj);
			
			Assert.That(nullAdded, Is.True, "Test Table Relation need an object for the Null Relation Test.");
			
			nullObj.ObjectId = null;
			nullObj.Entry = new TestTableRelationEntry() { TestField = "RelationEntryNullTestValue" };
			
			Database.FillObjectRelations(nullObj);

			Assert.That(nullObj.Entry, Is.Null, "Test Table Relation should not have entry with null local value.");
			
			// Check Allow Add Restriction
			var allowAddRestriction =  new TestTableRelation() { TestField = "RelationTestValueAllowAddRestriction" };
			
			allowAddRestriction.Entry = new TestTableRelationEntry() { TestField = "RelationEntryTestValueAllowAddRestriction", ObjectId = allowAddRestriction.ObjectId, AllowAdd = false };
			
			var restrictionAdded = Database.AddObject(allowAddRestriction);
			
			Assert.That(restrictionAdded, Is.False, "Test Table Relation should return error when relation failed saving.");
			Assert.That(allowAddRestriction.IsPersisted, Is.True, "Test Table Relation should be persisted in database.");			
			Assert.That(allowAddRestriction.Entry.IsPersisted, Is.False, "Test Table Relation Entry should not be persisted if AllowAdd Restriction is in place.");
			
			// Check Dirty Restriction
			allowAddRestriction.Entry.AllowAdd = true;
			
			var restrictionSaved = Database.SaveObject(allowAddRestriction);
			
			Assert.That(restrictionSaved, Is.True, "Test Table Relation restriction test should have the restricted object seved to database.");
			Assert.That(allowAddRestriction.Entry.IsPersisted, Is.True, "Test Table Relation Entry restriction test should be persisted.");
			
			Assert.That(allowAddRestriction.Dirty, Is.False, "Test Table Relation restriction object should not have Dirty flag after saving.");
			
			var modifiedText = "RelationEntryTestValueAllowAddRestrictionModified";
			allowAddRestriction.Entry.TestField = modifiedText;
			allowAddRestriction.Entry.Dirty = false;
			
			Assert.That(allowAddRestriction.Entry.Dirty, Is.False, "Test Table Relation Entry need to have its Dirty flag off for this Test.");
			
			var modifySaved = Database.SaveObject(allowAddRestriction);
			
			Assert.That(modifySaved, Is.True, "Test Table Relation should be saved correctly with Dirty flag off.");
			
			Database.FillObjectRelations(allowAddRestriction);
			
			Assert.That(allowAddRestriction.Entry.TestField, Is.Not.EqualTo(modifiedText), "Test Table Relation Entry should not have been saved with modified text.");
			
			// Check Allow Delete Restriction
			var restrictionEntry = allowAddRestriction.Entry;
			allowAddRestriction.Entry.AllowDelete = false;
			
			var restrictionDeleted = Database.DeleteObject(allowAddRestriction);
			
			Assert.That(restrictionDeleted, Is.False, "Test Table Relation should not be deleted correctly.");
			Assert.That(restrictionEntry.IsPersisted, Is.True, "Test Table Relation Entry should still be Persisted.");
			Assert.That(restrictionEntry.IsDeleted, Is.False, "Test Table Relation Entry should not be deleted.");
			Assert.That(allowAddRestriction.IsPersisted, Is.False, "Test Table Relation object should not be persisted.");
		}

		/// <summary>
		/// Test Table with Relation 1-n
		/// </summary>
		[Test]
		public void TestTableRelations()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var all = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableRelationsEntries.");
			
			var allrel = Database.SelectAllObjects<TestTableRelations>();
			
			foreach(var obj in allrel)
				Database.DeleteObject(obj);
			
			var nonerel = Database.SelectAllObjects<TestTableRelations>();
			
			Assert.That(nonerel, Is.Empty, "Database shouldn't have any record For TestTableRelations.");
			
			// Try Add With no Relation
			var noRelObj = new TestTableRelations() { TestField = "RelationsTestValue" };
			
			var inserted = Database.AddObject(noRelObj);
			
			Assert.That(inserted, Is.True, "Test Table Relations could not insert object with no relation.");
			Assert.That(noRelObj.Entries, Is.Null, "Test Table Relations object with no relation should have null Entry.");
			
			// Try Selecting
			var selectInserted = Database.SelectAllObjects<TestTableRelations>();
			Assert.That(selectInserted, Is.Not.Empty, "Test Table Relations could not retrieve objects without relation.");
			
			// Try Adding Relation
			var testValues = new[] { "RelationsEntriesTestValue 1", "RelationsEntriesTestValue 2", "RelationsEntriesTestValue 3" };
			
			var relObjs = testValues.Select(val => new TestTableRelationsEntries() { TestField = val, ForeignTestField = noRelObj.ObjectId }).ToArray();
			
			var relInserted = relObjs.Select(o => Database.AddObject(o)).ToArray();
			
			Assert.That(relInserted.All(res => res), Is.True, "Test Table Relations Entries could not be inserted.");
			
			noRelObj.Entries = relObjs;
			
			var saved = Database.SaveObject(noRelObj);
			
			Assert.That(saved, Is.True, "Test Table Relations could not save Object with a new relations Added.");
			
			// Try Retrieving Relation
			var retrieve = Database.FindObjectByKey<TestTableRelations>(noRelObj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Test Table Relations could not retrieve relations object by ObjectId.");
			Assert.That(retrieve.Entries, Is.Not.Null, "Test Table Relations retrieved object have no entries objects.");
			Assert.That(retrieve.Entries.Select(o => o.TestField), Is.EquivalentTo(testValues),
			                               "Test Table Relations retrieved objects Entries Relation are different from created objects.");

			// Try Deleting Relation
			var deleted = Database.DeleteObject(noRelObj);
			
			Assert.That(deleted, Is.True, "Test Table Relations could not delete object with relations.");
			Assert.That(noRelObj.IsDeleted, Is.True, "Test Table Relations deleted object should have deleted flag set.");
			Assert.That(noRelObj.Entries.All(ent => ent.IsDeleted), Is.True, "Test Table Relations deleted object should have all Entries with deleted flag set.");
			
			// Check that Relation was deleted
			var relRetrieve = Database.SelectAllObjects<TestTableRelationsEntries>().Where(o => o.ForeignTestField == noRelObj.ObjectId);
			
			Assert.That(relRetrieve, Is.Empty, "Test Table Relations Entries were not auto deleted with relations object.");
			Assert.That(relObjs.All(o => o.IsDeleted), Is.True, "Test Table Relations Entries should have deleted flags set after auto delete.");
			
			// Check Null Relation
			var nullObj = new TestTableRelations() { TestField = "RelationsNullTestValue" };
			
			var nullAdded = Database.AddObject(nullObj);
			
			Assert.That(nullAdded, Is.True, "Test Table Relations need an object for the Null Relation Test.");
			
			nullObj.ObjectId = null;
			nullObj.Entries = Array.Empty<TestTableRelationsEntries>();
			
			Database.FillObjectRelations(nullObj);

			Assert.That(nullObj.Entries, Is.Null, "Test Table Relations should have null entries with null local value.");
			
			// Check Allow Add Restriction
			var allowAddRestriction =  new TestTableRelations() { TestField = "RelationTestValueAllowAddRestriction" };
			
			allowAddRestriction.Entries = new [] { new TestTableRelationsEntries() { TestField = "RelationsEntriesTestValueAllowAddRestriction", ForeignTestField = allowAddRestriction.ObjectId, AllowAdd = false } };
			
			var restrictionAdded = Database.AddObject(allowAddRestriction);
			
			Assert.That(restrictionAdded, Is.False, "Test Table Relations should return false when missing relation adding.");
			Assert.That(allowAddRestriction.IsPersisted, Is.True, "Test Table Relations should be persisted.");
			
			Assert.That(allowAddRestriction.Entries[0].IsPersisted, Is.False, "Test Table Relations Entries should not be persisted if AllowAdd Restriction is in place.");
			
			// Check Dirty Restriction
			allowAddRestriction.Entries[0].AllowAdd = true;
			
			var restrictionSaved = Database.SaveObject(allowAddRestriction);
			
			Assert.That(restrictionSaved, Is.True, "Test Table Relations restriction test should have the restricted object seved to database.");
			Assert.That(allowAddRestriction.Entries[0].IsPersisted, Is.True, "Test Table Relations Entries restriction test should be persisted.");
			
			Assert.That(allowAddRestriction.Dirty, Is.False, "Test Table Relations restriction object should not have Dirty flag after saving.");
			
			var modifiedText = "RelationEntryTestValueAllowAddRestrictionModified";
			allowAddRestriction.Entries[0].TestField = modifiedText;
			allowAddRestriction.Entries[0].Dirty = false;
			
			Assert.That(allowAddRestriction.Entries[0].Dirty, Is.False, "Test Table Relations Entries need to have its Dirty flag off for this Test.");
			
			var modifySaved = Database.SaveObject(allowAddRestriction);
			
			Assert.That(modifySaved, Is.True, "Test Table Relations should be saved correctly with Dirty flag off.");
			
			Database.FillObjectRelations(allowAddRestriction);
			
			Assert.That(allowAddRestriction.Entries[0].TestField, Is.Not.EqualTo(modifiedText), "Test Table Relations Entries should not have been saved with modified text.");
			
			// Check Allow Delete Restriction
			var restrictionEntry = allowAddRestriction.Entries[0];
			allowAddRestriction.Entries[0].AllowDelete = false;
			
			var restrictionDeleted = Database.DeleteObject(allowAddRestriction);
			
			Assert.That(restrictionDeleted, Is.False, "Test Table Relations should not be deleted correctly.");
			Assert.That(restrictionEntry.IsPersisted, Is.True, "Test Table Relations Entries should still be Persisted.");
			Assert.That(restrictionEntry.IsDeleted, Is.False, "Test Table Relations Entries should not be deleted.");
			Assert.That(allowAddRestriction.IsPersisted, Is.False, "Test Table Relations object should not be persisted.");
		}
		
		/// <summary>
		/// Test Table with Relation 1-n No Autoload
		/// </summary>
		[Test]
		public void TestTableRelationsNoAutoload()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableRelationsWithNoAutoLoad));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var all = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableRelationsEntries.");
			
			var allrel = Database.SelectAllObjects<TestTableRelationsWithNoAutoLoad>();
			
			Database.DeleteObject(allrel);
			
			var nonerel = Database.SelectAllObjects<TestTableRelationsWithNoAutoLoad>();
			
			Assert.That(nonerel, Is.Empty, "Database shouldn't have any record For TestTableRelationsWithNoAutoLoad.");
			
			// Try Add With no Relation
			var noRelObj = new TestTableRelationsWithNoAutoLoad() { TestField = "RelationsTestValue" };
			
			var inserted = Database.AddObject(noRelObj);
			
			Assert.That(inserted, Is.True, "Test Table Relations (NoAutoLoad) could not insert object with no relation.");
			Assert.That(noRelObj.Entries, Is.Null, "Test Table Relations (NoAutoLoad) object with no relation should have null Entry.");
			
			// Try Adding Relation
			var testValues = new[] { "RelationsEntriesTestValue 1", "RelationsEntriesTestValue 2", "RelationsEntriesTestValue 3" };
			
			var relObjs = testValues.Select(val => new TestTableRelationsEntries() { TestField = val, ForeignTestField = noRelObj.ObjectId }).ToArray();
			
			var relInserted = Database.AddObject(relObjs);
			
			Assert.That(relInserted, Is.True, "Test Table Relations Entries could not be inserted.");
			
			noRelObj.Entries = relObjs;
			
			var saved = Database.SaveObject(noRelObj);
			
			Assert.That(saved, Is.True, "Test Table Relations (NoAutoLoad) could not save Object with a new relations Added.");
			
			// Try Retrieving Relation
			var retrieve = Database.FindObjectByKey<TestTableRelationsWithNoAutoLoad>(noRelObj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Test Table Relations (NoAutoLoad) could not retrieve relations object by ObjectId.");
			Assert.That(retrieve.Entries, Is.Null, "Test Table Relations (NoAutoLoad) retrieved object should not have entries objects.");
			
			Database.FillObjectRelations(retrieve);
			Assert.That(retrieve.Entries, Is.Not.Null, "Test Table Relations (NoAutoLoad) retrieved object should have entries objects after filling.");
			
			Assert.That(retrieve.Entries.Select(o => o.TestField), Is.EquivalentTo(testValues), 
			                               "Test Table Relations (NoAutoLoad) retrieved objects Entries Relation are different from created objects.");
			
			var changedRel = Database.FindObjectByKey<TestTableRelationsEntries>(retrieve.Entries[0].ObjectId);
			
			changedRel.TestField = "Changed Test Value for Relation NoAutoload";
			
			var resaved = Database.SaveObject(changedRel);
			
			Assert.That(resaved, Is.True, "Changed Relation (NoAutoLoad) could not be saved to database...");

			var newTestValues = new[] { changedRel.TestField, retrieve.Entries[1].TestField, retrieve.Entries[2].TestField };
			
			Database.FillObjectRelations(retrieve);
			
			Assert.That(retrieve.Entries.Select(obj => obj.TestField), Is.EquivalentTo(newTestValues), "Test Table Relations (NoAutoLoad) refreshed objects Entries Relation are different from changed objects.");
			
			// Try Deleting Relation
			var deleted = Database.DeleteObject(noRelObj);
			
			Assert.That(deleted, Is.True, "Test Table Relations (NoAutoLoad) could not delete object with relations.");
			Assert.That(noRelObj.IsDeleted, Is.True, "Test Table Relations (NoAutoLoad) deleted object should have deleted flag set.");
			
			// Check that Relations were deleted
			var relRetrieve = Database.SelectAllObjects<TestTableRelationsEntries>().Where(o => o.ForeignTestField == noRelObj.ObjectId);
			
			Assert.That(relRetrieve, Is.Empty, "Test Table Relations (NoAutoLoad) Entries were not auto deleted with relations object.");
			Assert.That(relObjs.All(o => o.IsDeleted), Is.True, "Test Table Relations (NoAutoLoad) Entries should have deleted flags set after auto delete.");
		}
		
		/// <summary>
		/// Test Table with Relation 1-n No AutoDelete
		/// </summary>
		[Test]
		public void TestTableRelationsWithNoAutoDelete()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableRelationsWithNoAutoDelete));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var all = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableRelationsEntries.");
			
			var allrel = Database.SelectAllObjects<TestTableRelationsWithNoAutoDelete>();
			
			Database.DeleteObject(allrel);
			
			var nonerel = Database.SelectAllObjects<TestTableRelationsWithNoAutoDelete>();
			
			Assert.That(nonerel, Is.Empty, "Database shouldn't have any record For TestTableRelationsWithNoAutoDelete.");
			
			// Add Relation Object
			var relObj = new TestTableRelationsWithNoAutoDelete() { TestField = "RelationsTestValue NoAutoDelete" };
			var testValues = new[] { "RelationsEntriesTestValue 1", "RelationsEntriesTestValue 2", "RelationsEntriesTestValue 3" };
			
			relObj.Entries = testValues.Select(obj => new TestTableRelationsEntries { TestField = obj, ForeignTestField = relObj.ObjectId } ).ToArray();
			
			var inserted = Database.AddObject(relObj);
			
			Assert.That(inserted, Is.True, "Test Table Relations (NoAutoDelete) could not insert object with no relation.");
			
			// Try Retrieving Relation
			var retrieve = Database.FindObjectByKey<TestTableRelationsWithNoAutoDelete>(relObj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Test Table Relations (NoAutoDelete) could not retrieve relations object by ObjectId.");
			Assert.That(retrieve.Entries, Is.Not.Empty, "Test Table Relations (NoAutoDelete) retrieved object should have entries objects.");
			Assert.That(retrieve.Entries.Select(obj => obj.TestField), Is.EquivalentTo(testValues), "Test Table Relations (NoAutoDelete) retrieved object Entries should have same values as created.");
			
			// Try Deleting Relation
			var deleted = Database.DeleteObject(relObj);
			
			Assert.That(deleted, Is.True, "Test Table Relations (NoAutoDelete) could not delete object with relations.");
			Assert.That(relObj.IsDeleted, Is.True, "Test Table Relations (NoAutoDelete) deleted object should have deleted flag set.");
			
			// Check that Relation were not deleted
			var relRetrieve = Database.SelectAllObjects<TestTableRelationsEntries>().Where(o => o.ForeignTestField == relObj.ObjectId);
			
			Assert.That(relRetrieve, Is.Not.Empty, "Test Table Relations (NoAutoDelete) Entries should not be auto deleted with relations object.");
			Assert.That(relObj.Entries.All(o => !o.IsDeleted), Is.True, "Test Table Relations (NoAutoDelete) Entries should not have deleted flags set after delete.");
			Assert.That(relRetrieve.Select(obj => obj.TestField), Is.EquivalentTo(testValues), "Test Table Relations (NoAutoDelete) Entries should have the same element as created.");
		}
		
		/// <summary>
		/// Test Table with Primary Key
		/// </summary>
		[Test]
		public void TestTablePrimaryKey()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTablePrimaryKey));
			
			var all = Database.SelectAllObjects<TestTablePrimaryKey>();
			
			foreach(var obj in all)
				Database.DeleteObject(obj);
			
			var none = Database.SelectAllObjects<TestTablePrimaryKey>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableUniqueField.");
			
			// Test Add
			var uniqueObj = new TestTablePrimaryKey { TestField = "Test Value", PrimaryKey = "Key1" };
			
			var inserted = Database.AddObject(uniqueObj);
			
			Assert.That(inserted, Is.True, "Test Table Primary Key could not insert unique key object.");
			
			// Try Adding with unique Value
			var otherUniqueObj = new TestTablePrimaryKey { TestField = "Test Value Other", PrimaryKey = "Key1" };
			
			var otherInserted = Database.AddObject(otherUniqueObj);
			
			Assert.That(otherInserted, Is.False, "Test Table Primary Key with Other Object violating key constraint should not be inserted.");
			
			// Try Adding with non unique Value
			var otherNonUniqueObj = new TestTablePrimaryKey { TestField = "Test Value Other Non-Primary", PrimaryKey = "Key2" };
			
			var nonUniqueInserted = Database.AddObject(otherNonUniqueObj);
			
			Assert.That(nonUniqueInserted, Is.True, "Test Table Primary Key with Other Non Unique Key could not be inserted");
			
			// Try saving with unique Value
			var retrieved = Database.FindObjectByKey<TestTablePrimaryKey>(otherNonUniqueObj.PrimaryKey);
			
			retrieved.ObjectId = uniqueObj.ObjectId;
			retrieved.TestField = "Changed Test Field";
			
			var saved = Database.SaveObject(retrieved);
			
			Assert.That(saved, Is.False, "Test Table Primary Key with Retrieved Object violating key constraint should not be saved.");
			
			// Delete Previous Unique and Try Reinsert.
			var deleted = Database.DeleteObject(uniqueObj);
			Assert.That(deleted, Is.True, "Test Table Primary Key could not delete unique key object.");
			Assert.That(uniqueObj.IsDeleted, Is.True, "Test Table Primary Key unique key object should have delete flag set.");
			
			var retrievedSaved = Database.SaveObject(retrieved);
			
			Assert.That(retrievedSaved, Is.True, "Test Table Primary Key Retrieved Object could not be inserted after deleting previous constraint violating object.");
		}
		
		/// <summary>
		/// Test Table with Primary Key
		/// </summary>
		[Test]
		public void TestTablePrimaryKeyWithUnique()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTablePrimaryKeyUnique));
			
			var all = Database.SelectAllObjects<TestTablePrimaryKeyUnique>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTablePrimaryKeyUnique>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTablePrimaryKeyUnique.");
			
			// Test Add
			var uniqueObj = new TestTablePrimaryKeyUnique { TestField = "Test Value", PrimaryKey = "Key1", Unique = "Unique" };
			
			var inserted = Database.AddObject(uniqueObj);
			
			Assert.That(inserted, Is.True, "Test Table Primary Key With Unique could not insert unique key object.");
			
			// Try Adding with unique Value
			var otherUniqueObj = new TestTablePrimaryKeyUnique { TestField = "Test Value Other", PrimaryKey = "Key1", Unique = "Unique" };
			
			var otherInserted = Database.AddObject(otherUniqueObj);
			
			Assert.That(otherInserted, Is.False, "Test Table Primary Key With Unique with Other Object violating key constraint should not be inserted.");
			
			// Try Adding with non unique Value
			var otherNonUniqueObj = new TestTablePrimaryKeyUnique { TestField = "Test Value Other Non-Primary", PrimaryKey = "Key2", Unique = "NonUnique" };
			
			var nonUniqueInserted = Database.AddObject(otherNonUniqueObj);
			
			Assert.That(nonUniqueInserted, Is.True, "Test Table Primary Key With Unique with Other Non Unique Key could not be inserted");
			
			// Try saving with unique Value
			var retrieved = Database.FindObjectByKey<TestTablePrimaryKeyUnique>(otherNonUniqueObj.PrimaryKey);
			
			retrieved.Unique = "Unique";
			
			var saved = Database.SaveObject(retrieved);
			
			Assert.That(saved, Is.False, "Test Table Primary Key With Unique with Retrieved Object violating key constraint should not be saved.");
			
			// Delete Previous Unique and Try Reinsert.
			var deleted = Database.DeleteObject(uniqueObj);
			Assert.That(deleted, Is.True, "Test Table Primary Key With Unique could not delete unique key object.");
			Assert.That(uniqueObj.IsDeleted, Is.True, "Test Table Primary Key With Unique unique key object should have delete flag set.");
			
			var retrievedSaved = Database.SaveObject(retrieved);
			
			Assert.That(retrievedSaved, Is.True, "Test Table Primary Key With Unique Retrieved Object could not be inserted after deleting previous constraint violating object.");
		}
		
		/// <summary>
		/// Test Table With ReadOnly Field
		/// </summary>
		[Test]
		public void TestTableWithReadOnly()
		{
			// Prepare and Cleanup
			Database.RegisterDataObject(typeof(TestTableWithReadOnly));
			
			var all = Database.SelectAllObjects<TestTableWithReadOnly>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTableWithReadOnly>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableWithReadOnly.");
			
			var testObj = new TestTableWithReadOnly { TestField = "Test For Read Only", ReadOnly = "ReadOnly Value" };
			
			var inserted = Database.AddObject(testObj);
			
			Assert.That(inserted, Is.True, "Test Table With Readonly couldn't insert object in database.");
			
			// Edit Readonly
			testObj.TestField = "Changed Test For Read Only";
			testObj.ReadOnly = "ReadOnly Value !!CHANGED!!";
			
			Assert.That(testObj.Dirty, Is.True, "Test Table With Readonly should set Dirty Flag when changing TestField.");
			
			var saved = Database.SaveObject(testObj);
			Assert.That(saved, Is.True, "Test Table With Readonly couldn't save object in database.");
			Assert.That(testObj.Dirty, Is.False, "Test Table With Readonly should no have Dirty Flag when saved.");
			
			// Retrieve
			var retrieved = Database.FindObjectByKey<TestTableWithReadOnly>(testObj.ObjectId);
			Assert.That(retrieved, Is.Not.Null, "Test Table With Readonly couldn't retrieve object in database.");
			Assert.That(retrieved.ReadOnly, Is.Not.EqualTo(testObj.ReadOnly), "Test Table With ReadOnly shouldn't have saved ReadOnly Field Change.");
			Assert.That(retrieved.TestField, Is.EqualTo(testObj.TestField), "Test Table With ReadOnly should have saved Test Field Change.");
			Assert.That(retrieved.ReadOnly, Is.EqualTo("ReadOnly Value"), "Test Table With ReadOnly should have the same ReadOnly Field Value as Created.");
		}
		
		/// <summary>
		/// Test Table With View
		/// </summary>
		[Test]
		public void TestTableWithView()
		{
			Database.RegisterDataObject(typeof(TestTableBaseView));
			Database.RegisterDataObject(typeof(TestTableAsView));
			
			var all = Database.SelectAllObjects<TestTableBaseView>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTableBaseView>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableBaseView.");
			
			// Insert some Object in Base View
			var objBase = new TestTableBaseView { TestField = "TestField for Base View", ViewValue = "This is a Value to be retrieve in View..." };
			
			var inserted = Database.AddObject(objBase);
			
			Assert.That(inserted, Is.True, "Test Table With View could not insert object in database.");
			
			// Retrieve From View
			var retrieve = Database.SelectAllObjects<TestTableAsView>().FirstOrDefault();
			
			Assert.That(retrieve, Is.Not.Null, "Test Table With View could not retrieve inserted Object from view .");
			Assert.That(retrieve.PrimaryKey, Is.EqualTo(objBase.PrimaryKey), "Test Table With View should retrieve same value from Base and View Object.");
			Assert.That(retrieve.TestField, Is.EqualTo(objBase.TestField), "Test Table With View should retrieve same value from Base and View Object.");
			Assert.That(retrieve.ViewValue, Is.EqualTo(objBase.ViewValue), "Test Table With View should retrieve same value from Base and View Object.");
			Assert.That(retrieve.WeirdValue, Is.EqualTo("Weird Indeed"), "Test Table With View should retrieve View Object with 'Weird Value' set as expected from ViewAs.");
			
			// Try Add View Value
			var objView = new TestTableAsView { TestField = "TestField for As View", ViewValue = "This is a Value to saved in Base...", WeirdValue = "Anything..." };
			
			var viewInsert = Database.AddObject(objView);
			
			Assert.That(viewInsert, Is.True, "Test Table With View should be able to Insert View Object as Base Object in Database.");
			
			// Retrieve from Base
			var baseRetrieve = Database.FindObjectByKey<TestTableBaseView>(objView.PrimaryKey);
			
			Assert.That(baseRetrieve, Is.Not.Null, "Test Table With View should be able to retrieve object from View Key.");
			Assert.That(baseRetrieve.PrimaryKey, Is.EqualTo(objView.PrimaryKey), "Test Table With View should retrieve same value from View and Base Object.");
			Assert.That(baseRetrieve.TestField, Is.EqualTo(objView.TestField), "Test Table With View should retrieve same value from View and Base Object.");
			Assert.That(baseRetrieve.ViewValue, Is.EqualTo(objView.ViewValue), "Test Table With View should retrieve same value from View and Base Object.");
			
			// Try Save Modified View Value
			var viewRetrieve = Database.FindObjectByKey<TestTableAsView>(objView.PrimaryKey);
			Assert.That(viewRetrieve, Is.Not.Null, "Test Table With View should be able to retrieve object from View Key.");
			
			viewRetrieve.TestField = "Changed TestField for As View";
			viewRetrieve.ViewValue = "Changed Value to be saved in Base...";
			
			var saved = Database.SaveObject(viewRetrieve);
			
			Assert.That(saved, Is.True, "Test Table With View should be able to Save View Object as Base Object in Database.");
			Assert.That(viewRetrieve.Dirty, Is.False, "Test Table With View should not have Dirty Flag on View Object Saved.");
			
			// Retrieve Saved
			
			var saveRetrieve = Database.FindObjectByKey<TestTableBaseView>(viewRetrieve.PrimaryKey);
			
			Assert.That(saveRetrieve, Is.Not.Null, "Test Table With View should be able to retrieve object from Base Key.");
			Assert.That(saveRetrieve.PrimaryKey, Is.EqualTo(viewRetrieve.PrimaryKey), "Test Table With View should retrieve same value from View and Base Object.");
			Assert.That(saveRetrieve.TestField, Is.EqualTo(viewRetrieve.TestField), "Test Table With View should retrieve same value from View and Base Object.");
			Assert.That(saveRetrieve.ViewValue, Is.EqualTo(viewRetrieve.ViewValue), "Test Table With View should retrieve same value from View and Base Object.");
			
			// Try Filter Match (HIDDEN)
			var objHidden = new TestTableBaseView { TestField = "TestField for HIDDEN Base View", ViewValue = "HIDDEN" };
			
			var savedHidden = Database.AddObject(objHidden);
			Assert.That(savedHidden, Is.True, "Test Table With View could not insert object in database.");
			
			var retrieveHidden = Database.FindObjectByKey<TestTableAsView>(objHidden.PrimaryKey);
			Assert.That(retrieveHidden, Is.Null, "Test Table With View should not be able to retrieve object filtered out of View As Query.");
		}
		
		
		/// <summary>
		/// Test Table With View And Relation
		/// </summary>
		[Test]
		public void TestTableWithViewAndRelation()
		{
			Database.RegisterDataObject(typeof(TestTableBaseView));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			Database.RegisterDataObject(typeof(TestTableAsViewWithRelations));
			
			var all = Database.SelectAllObjects<TestTableBaseView>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTableBaseView>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTableBaseView.");
			
			var relall = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Database.DeleteObject(relall);
			
			var relnone = Database.SelectAllObjects<TestTableRelationsEntries>();
			
			Assert.That(relnone, Is.Empty, "Database shouldn't have any record For TestTableRelationsEntries.");
			
			// Insert some Object in Base View
			var objBase = new TestTableBaseView { TestField = "TestField for Base View With Relation", ViewValue = "This is a Value to be retrieve in View Relation..." };
			
			var inserted = Database.AddObject(objBase);
			
			Assert.That(inserted, Is.True, "Test Table With View And Relation could not insert object in database.");
			
			var entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { TestField = string.Format("View Relation Test Field #{0}", i), ForeignTestField = objBase.ViewValue });
			
			var relInserted = Database.AddObject(entries);
			
			Assert.That(relInserted, Is.True, "Test Table With View And Relation could not insert relations object in database.");
			
			// Retrieve
			var viewRetrieve = Database.FindObjectByKey<TestTableAsViewWithRelations>(objBase.PrimaryKey);
			
			Assert.That(viewRetrieve, Is.Not.Null, "Test Table With View And Relation could not retrieve object from database.");
			Assert.That(viewRetrieve.Entries, Is.Not.Empty, "Test Table With View And Relation should have Relations Populated.");
			Assert.That(viewRetrieve.Entries.Select(obj => obj.TestField), Is.EquivalentTo(entries.Select(obj => obj.TestField)), "Test Table With View And Relation should have Relations similar to Created Object");
		}
		
		/// <summary>
		/// Test Table with Primary Key and Precache Enabled
		/// Similar to ItemTemplate Table...
		/// </summary>
		[Test]
		public void TestTablePrimaryKeyWithPrecache()
		{
			Database.RegisterDataObject(typeof(TestTablePrecachedPrimaryKey));
			
			var all = Database.SelectAllObjects<TestTablePrecachedPrimaryKey>();
			
			Database.DeleteObject(all);
			
			var none = Database.SelectAllObjects<TestTablePrecachedPrimaryKey>();
			
			Assert.That(none, Is.Empty, "Database shouldn't have any record For TestTablePrecachedPrimaryKey.");
			
			// Insert some Object
			var obj = new TestTablePrecachedPrimaryKey { TestField = "Test For Precached Table with Primary Key", PrecachedValue = "Some Value for Precache Table With Primary Key",
				PrimaryKey = "New_Object_Precached_With_PrimaryKey" };
			
			var inserted = Database.AddObject(obj);
			
			Assert.That(inserted, Is.True, "Test Table Precached With Primary Key could not insert object in database.");
			
			// Find it !
			var retrieve = Database.FindObjectByKey<TestTablePrecachedPrimaryKey>(obj.PrimaryKey);
			
			Assert.That(retrieve, Is.Not.Null, "Test Table Precached With Primary Key could not retrieve object from database or precache using primary key.");
			Assert.That(retrieve.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Test Table Precached With Primary Key should retrieve Object with similar primary key.");
			
			// Find it with case insensitive...
			var retrieveCase = Database.FindObjectByKey<TestTablePrecachedPrimaryKey>(obj.PrimaryKey.ToUpper());
			
			Assert.That(retrieveCase, Is.Not.Null, "Test Table Precached With Primary Key should be able to retrieve object with primary key using different case.");
			Assert.That(retrieveCase.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Test Table Precached With Primary Key should retrieve Object with similar primary key.");
		}
		
		/// <summary>
		/// Test Table with Relations to a Precached Table
		/// Try Fill Object Relation with Precached Where Clause
		/// </summary>
		[Test]
		public void TestTableRelationsWithPrecached()
		{
			Database.RegisterDataObject(typeof(TestTableRelationsWithPrecache));
			Database.RegisterDataObject(typeof(TestTableRelationsEntriesPrecached));
			
			// Cleanup
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsWithPrecache>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsEntriesPrecached>());
			
			Assert.That(Database.SelectAllObjects<TestTableRelationsWithPrecache>(), Is.Empty, "Relations Test With Precache should begin with Empty Table.");
			Assert.That(Database.SelectAllObjects<TestTableRelationsEntriesPrecached>(), Is.Empty, "Relations Test With Precache should begin with Empty Table.");
			
			// Objects
			var objs = Enumerable.Range(0, 10).Select(i => new TestTableRelationsWithPrecache { TestField = string.Format("Test Table Relation with Precache #{0}", i) }).ToArray();
			
			foreach (var obj in objs)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntriesPrecached { ForeignTestField = obj.ObjectId, TestField = string.Format("Test Table Relation Entry with Precache #{0}", i) }).ToArray();
			
			var added = Database.AddObject(objs);
			
			Assert.That(added, Is.True, "Relations Test With Precache could not insert test objects.");
			
			// Select
			var retrieve = Database.SelectAllObjects<TestTableRelationsWithPrecache>();
			
			Assert.That(retrieve.Select(obj => obj.ObjectId), Is.EquivalentTo(objs.Select(obj => obj.ObjectId)), "Relations Test With Precache should retrieve object similar to created ones.");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "Relations Test With Precache should retrieve object similar to created ones.");
			
			foreach (var ret in retrieve)
			{
				Assert.That(ret.Entries.Select(obj => obj.ObjectId), Is.EquivalentTo(objs.FirstOrDefault(obj => obj.ObjectId.Equals(ret.ObjectId)).Entries.Select(obj => obj.ObjectId)), "Relations Test With Precache should retrieve object entries similar to created ones.");
				Assert.That(ret.Entries.Select(obj => obj.TestField), Is.EquivalentTo(objs.FirstOrDefault(obj => obj.ObjectId.Equals(ret.ObjectId)).Entries.Select(obj => obj.TestField)), "Relations Test With Precache should retrieve object entries similar to created ones.");
			}
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsEntriesPrecached>());
			Assert.That(Database.SelectAllObjects<TestTableRelationsEntriesPrecached>(), Is.Empty, "Relations Test With Precache should have Empty Entry Table for cache test.");
			
			Database.FillObjectRelations(objs);
			
			foreach (var obj in objs)
				Assert.That(obj.Entries, Is.Null, "Relation Test With Precache should have objects with null relations after entry deletion.");
			
			// Try Select with no relation
			var norel = new TestTableRelationsWithPrecache { TestField = "Test Table Relation with Precache without relations..." };
			Database.AddObject(norel);
			var selectInserted = Database.SelectAllObjects<TestTableRelationsWithPrecache>().Single(obj => obj.TestField.Equals(norel.TestField));

			Assert.That(selectInserted.Entries, Is.Null, "Relations Test With Precache should return null for Entries stored without relation.");
			
			// Try Fill with null Local Value
			var reAddEntries = Database.AddObject(Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntriesPrecached { ForeignTestField = IDGenerator.GenerateID(), TestField = string.Format("Test Table NOT Related Entry with Precache #{0}", i) }));
			
			Assert.That(reAddEntries, Is.True, "Relations Test With Precache Relation Entries could not be inserted for testing...");
			
			foreach(var obj in objs)
			{
				obj.ObjectId = null;
				obj.Entries = Array.Empty<TestTableRelationsEntriesPrecached>();
			}
			
			Database.FillObjectRelations(objs);
			
			foreach (var obj in objs)
				Assert.That(obj.Entries, Is.Null, "Relation Test With Precache should have objects with null relations with null local.");
		}
		
		[Test]
		public void TestTableRelationWithPrecacheAndPrimaryRemote()
		{
			Database.RegisterDataObject(typeof(TestTableRelationsWithPrecacheAndPrimary));
			Database.RegisterDataObject(typeof(TestTableRelationsEntryPrecached));
			
			// Cleanup
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsWithPrecacheAndPrimary>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsEntryPrecached>());
			
			Assert.That(Database.SelectAllObjects<TestTableRelationsWithPrecacheAndPrimary>(), Is.Empty, "Relations Test With Precache and Primary should begin with Empty Table.");
			Assert.That(Database.SelectAllObjects<TestTableRelationsEntryPrecached>(), Is.Empty, "Relations Test With Precache and Primary should begin with Empty Table.");
			
			// Add one object with no relation
			var norel = new TestTableRelationsWithPrecacheAndPrimary { TestField = "Table Relation With Precache and Primary Remote" };
			var added = Database.AddObject(norel);
			
			Assert.That(added, Is.True, "Relations Test With Precache and Primary could not add test object.");
			
			// Try selecting
			var retrieve = Database.SelectAllObjects<TestTableRelationsWithPrecacheAndPrimary>().Single();
			Assert.That(retrieve.TestField, Is.EqualTo(norel.TestField), "Relations Test With Precache and Primary should retrieve similar object.");
			Assert.That(retrieve.Entry, Is.Null, "Relations Test With Precache and Primary should not have relation set for no relation object.");
			
			// Add relation
			
			var relobj = new TestTableRelationsEntryPrecached { ForeignTestField = "Some Data", ObjectId = norel.ObjectId };
			var reladded = Database.AddObject(relobj);
			
			Assert.That(reladded, Is.True, "Relations Test With Precache and Primary could not add test relation entry.");
			
			// Try selecting
			var relretrieve = Database.SelectAllObjects<TestTableRelationsWithPrecacheAndPrimary>().Single();
			Assert.That(relretrieve.TestField, Is.EqualTo(norel.TestField), "Relations Test With Precache and Primary should retrieve similar object.");
			Assert.That(relretrieve.Entry, Is.Not.Null, "Relations Test With Precache and Primary should have valid relation entry.");
			Assert.That(relretrieve.Entry.ForeignTestField, Is.EqualTo(relobj.ForeignTestField), "Relations Test With Precache and Primary should retrieve similar relation object.");
			
			// Try Null Value Relation
			relretrieve.ObjectId = null;
			
			Database.FillObjectRelations(relretrieve);
			
			Assert.That(relretrieve.Entry, Is.Null, "Relations Test With Precache and Primary should return null value for null local field relation...");
		}

		[Test]
		public void SelectObject_TestFieldContainsSpecialISO88591signs_TestfieldIsUnaltered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dataObject = new TestTable();
			dataObject.TestField = "¡¢£¤¥¦§¨©ª«¬®¯°±²³´µ¶·¸¹º»¼½¾¿ÀÁÂÃÄÅÆÇÈÉÊËÌÍÎÏÐÑÒÓÔÕÖ×ØÙÚÛÜÝÞßàáâãäåæçèéêëìíîïðñòóôõö÷øùúûüýþÿ";

			AddOrReplaceObject(dataObject);

			var actual = Database.FindObjectByKey<TestTable>(dataObject.ObjectId).TestField;
			var expected = dataObject.TestField;
			Assert.That(actual, Is.EqualTo(expected));
		}

		[Test]
		public void SelectObject_TestFieldContainsSpecialCp1252signs_TestfieldIsUnaltered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dataObject = new TestTable();
			dataObject.TestField = "€‚ƒ„…†‡ˆ‰Š‹ŒŽ‘’“”•–—˜™š›œžŸ";

			AddOrReplaceObject(dataObject);

			var actual = Database.FindObjectByKey<TestTable>(dataObject.ObjectId).TestField;
			var expected = dataObject.TestField;
			Assert.That(actual, Is.EqualTo(expected));
		}

		private void AddOrReplaceObject<T>(T dataObject) where T : DataObject
		{
			var dataObjectFromDatabase = Database.FindObjectByKey<T>(dataObject.ObjectId);
			if (dataObjectFromDatabase != null) Database.DeleteObject(dataObject);
			Database.AddObject(dataObject);
		}
	}
}
