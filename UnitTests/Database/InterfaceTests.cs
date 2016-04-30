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
using System.Collections.Generic;
using System.Diagnostics;

using DOL.Database;
using DOL.Database.Attributes;

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Description of InterfaceTests.
	/// </summary>
	[TestFixture]
	public class InterfaceTests
	{
		public InterfaceTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		
		#region Test Add
		/// <summary>
		/// Test IObjectDatabase.Add(DataObject)
		/// </summary>
		[Test]
		public void TestSingleAdd()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var obj = new TestTable { TestField = "Test Single Add" };
			
			var objId = obj.ObjectId;
			
			Assert.IsFalse(obj.IsPersisted, "Test Object should not have Persisted Flag before adding...");
			Assert.IsTrue(obj.Dirty, "Test Object should be Dirty before adding...");
			
			var inserted = Database.AddObject(obj);
			
			Assert.IsTrue(inserted, "Test Object should be inserted successfully, something went wrong...");
			Assert.IsTrue(obj.IsPersisted, "Test Object should have Persisted Flag after adding to database...");
			Assert.IsFalse(obj.Dirty, "Test Object should not be Dirty after adding to database...");
			Assert.AreEqual(objId, obj.ObjectId, "Test Object should have kept its ObjectId from Creation...");
			
			var retrieved = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.IsNotNull(retrieved, "Test Object previously added should be retrieved from database...");
			Assert.AreEqual(objId, retrieved.ObjectId, "Test Object previously added and retrieved object should have same ObjectId...");
			Assert.AreEqual(obj.TestField, retrieved.TestField, "Test Object previously added and retrieved object should have equals fields...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.Add(IEnumerable`DataObject)
		/// </summary>
		[Test]
		public void TestMultiAddSameTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multi Add Same Table #{0}", i) }).ToArray();
			var objsId = objs.Select(obj => obj.ObjectId).ToArray();
			
			foreach (var obj in objs)
			{
				Assert.IsFalse(obj.IsPersisted, "Test Objects should not have Persisted Flag before adding...");
				Assert.IsTrue(obj.Dirty, "Test Objects should be Dirty before adding...");
			}
			
			var inserted = Database.AddObject(objs);
			Assert.IsTrue(inserted, "Test Objects should be inserted successfully, something went wrong...");
			foreach (var obj in objs)
			{
				Assert.IsTrue(obj.IsPersisted, "Test Objects should have Persisted Flag after adding to database...");
				Assert.IsFalse(obj.Dirty, "Test Objects should not be Dirty after adding to database...");
			}
			CollectionAssert.AreEqual(objsId, objs.Select(obj => obj.ObjectId), "Test Objects should have kept their ObjectId from Creation...");
			
			var retrieved = Database.FindObjectByKey<TestTable>(objsId);
			
			Assert.IsNotNull(retrieved, "Test Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsId, retrieved.Select(obj => obj.ObjectId), "Test Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieved.Select(obj => obj.TestField), "Test Objects previously added and retrieved objects should have equals fields...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.Add(IEnumerable`DataObject)
		/// With Different DataObject Types...
		/// </summary>
		[Test]
		public void TestMultiAddDifferentTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var objsTestTable = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multi Add Different Table #{0}", i) }).ToArray();
			var objsTestTableId = objsTestTable.Select(obj => obj.ObjectId).ToArray();
			var objsTestTableAutoInc = Enumerable.Range(0, 10).Select(i => new TestTableAutoInc { TestField = string.Format("Test Auto Inc Multi Add Different Table #{0}", i) }).ToArray();
			var objsTestTableAutoIncId = objsTestTableAutoInc.Select(obj => obj.ObjectId).ToArray();
			var objsTestTableRelations = Enumerable.Range(0, 10).Select(i => new TestTableRelations { TestField = string.Format("Test Relation Multi Add Different Table #{0}", i) }).ToArray();
			var objsTestTableRelationsId = objsTestTableRelations.Select(obj => obj.ObjectId).ToArray();
			
			// Fill Relation
			foreach (var obj in objsTestTableRelations)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { ForeignTestField = obj.ObjectId }).ToArray();
			
			var objsTestTableRelationsEntriesId = objsTestTableRelations.ToDictionary(kv => kv.ObjectId, kv => kv.Entries.Select(ent => ent.ObjectId).ToArray());
			
			// Concat All this
			var objs = objsTestTable.Cast<DataObject>().Concat(objsTestTableAutoInc).Concat(objsTestTableRelations);
			
			foreach (var obj in objs)
			{
				Assert.IsFalse(obj.IsPersisted, "Different Test Objects should not have Persisted Flag before adding...");
				Assert.IsTrue(obj.Dirty, "Different Test Objects should be Dirty before adding...");
			}
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "Different Test Objects should be inserted successfully, something went wrong...");
			foreach (var obj in objs)
			{
				Assert.IsTrue(obj.IsPersisted, "Test Objects should have Persisted Flag after adding to database...");
				Assert.IsFalse(obj.Dirty, "Test Objects should not be Dirty after adding to database...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
				{
					Assert.IsTrue(ent.IsPersisted, "TestTableRelationEntries Objects should have Persisted Flag after adding to database...");
					Assert.IsFalse(ent.Dirty, "TestTableRelationEntries Objects should not be Dirty after adding to database...");
				}
			}
			
			CollectionAssert.AreEquivalent(objsTestTableId, objs.Where(obj => obj.GetType() == typeof(TestTable)).Select(obj => obj.ObjectId), "TestTable Objects should have kept their ObjectId from Creation...");
			CollectionAssert.AreEquivalent(objsTestTableRelationsId, objs.Where(obj => obj.GetType() == typeof(TestTableRelations)).Select(obj => obj.ObjectId), "TestTableRelations Objects should have kept their ObjectId from Creation...");
			CollectionAssert.AreNotEquivalent(objsTestTableAutoIncId, objs.Where(obj => obj.GetType() == typeof(TestTableAutoInc)).Select(obj => obj.ObjectId), "TestTableAutoInc Objects should NOT have kept their ObjectId from Creation...");
			
			foreach (var obj in objs.Where(obj => obj.GetType() == typeof(TestTableRelations)).Cast<TestTableRelations>())
				CollectionAssert.AreEquivalent(objsTestTableRelationsEntriesId[obj.ObjectId], obj.Entries.Select(ent => ent.ObjectId),
				                               "TestTableRelationsEntries Objects should have kept their ObjectId from Creation...");
			
			var retrievedTestTable = Database.FindObjectByKey<TestTable>(objsTestTableId);
			Assert.IsNotNull(retrievedTestTable, "TestTable Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsTestTableId, retrievedTestTable.Select(obj => obj.ObjectId), "TestTable Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objsTestTable.Select(obj => obj.TestField), retrievedTestTable.Select(obj => obj.TestField), "TestTable Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableAutoInc = Database.FindObjectByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(key => (object)key.PrimaryKey));
			Assert.IsNotNull(retrievedTestTableAutoInc, "TestTableAutoInc Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsTestTableAutoInc.Select(key => key.PrimaryKey.ToString()), retrievedTestTableAutoInc.Select(obj => obj.ObjectId), "TestTableAutoInc Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objsTestTableAutoInc.Select(obj => obj.TestField), retrievedTestTableAutoInc.Select(obj => obj.TestField), "TestTableAutoInc Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableRelations = Database.FindObjectByKey<TestTableRelations>(objsTestTableRelationsId);
			Assert.IsNotNull(retrievedTestTableRelations, "TestTableRelations Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsTestTableRelationsId, retrievedTestTableRelations.Select(obj => obj.ObjectId), "TestTableRelations Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objsTestTableRelations.Select(obj => obj.TestField), retrievedTestTableRelations.Select(obj => obj.TestField), "TestTableRelations Objects previously added and retrieved objects should have equals fields...");
			
			foreach (var obj in retrievedTestTableRelations)
				CollectionAssert.AreEquivalent(objsTestTableRelationsEntriesId[obj.ObjectId], obj.Entries.Select(ent => ent.ObjectId),
				                               "TestTableRelationsEntries Objects previously added and retrieved objects should have same ObjectId...");
		}

		/// <summary>
		/// Test Add with a null Value
		/// </summary>
		[Test]
		public void TestSingleAddNull()
		{
			DataObject dbo = null;
			Assert.Throws(typeof(NullReferenceException), () => Database.AddObject(dbo), "Adding a Single Null Object Should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test Add with a non registered Value
		/// </summary>
		[Test]
		public void TestSingleAddNonRegistered()
		{
			var dbo = new TableNotRegistered();
			var added = Database.AddObject(dbo);
			Assert.IsFalse(added, "Adding a Single non registered Object should not return success...");
			Assert.IsFalse(dbo.IsPersisted, "Adding a Single non registered Object should not have Persisted Flag...");
		}
		
		/// <summary>
		/// Test Add with multiple null Value
		/// </summary>
		[Test]
		public void TestMultipleAddWithNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Add with some null Values" }, null };
			Assert.Throws(typeof(NullReferenceException), () => Database.AddObject(dbo), "Adding a Multiple with Null Objects Should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test Add with multiple non registered Value
		/// </summary>
		[Test]
		public void TestMultipleAddNonRegistered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Add with some non registered Values" }, new TableNotRegistered() };
			var added = Database.AddObject(dbo);
			Assert.IsFalse(added, "Adding Multiple non registered Object should not return success...");
			Assert.IsFalse(dbo[1].IsPersisted, "Adding a Multiple non registered Object should not have Persisted Flag on unregistered Object...");
			Assert.IsTrue(dbo[0].IsPersisted, "Adding a Multiple non registered Object should have Persisted Flag on registered Object...");
		}
		
		[Test, Explicit]
		public void BenchSingleAdd()
		{
			Database.RegisterDataObject(typeof(TestTable));
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));

			Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelations>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsEntries>());
			
			Assert.IsEmpty(Database.SelectAllObjects<TestTable>(), "Database shouldn't have any record For TestTable.");
			Assert.IsEmpty(Database.SelectAllObjects<TestTableRelations>(), "Database shouldn't have any record For TestTable.");
			Assert.IsEmpty(Database.SelectAllObjects<TestTableRelationsEntries>(), "Database shouldn't have any record For TestTable.");
			
			var objs = Enumerable.Range(0, 100).Select(i => new TestTable { TestField = string.Format("Bench Single Add '{0}'", i) }).ToArray();

			var times = new List<long>();
			foreach (var obj in objs)
			{
				var stopWatch = Stopwatch.StartNew();
				Database.AddObject(obj);
				stopWatch.Stop();
				times.Add(stopWatch.ElapsedMilliseconds);
			}
						
			var relationObjs = Enumerable.Range(0, 100).Select(i => new TestTableRelations { TestField = string.Format("Bench Single Relations Add '{0}'", i) }).ToArray();
			foreach (var obj in relationObjs)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { ForeignTestField = obj.ObjectId }).ToArray();
			
			var timesRelations =  new List<long>();
			foreach (var obj in relationObjs)
			{
				var stopWatch = Stopwatch.StartNew();
				Database.AddObject(obj);
				stopWatch.Stop();
				timesRelations.Add(stopWatch.ElapsedMilliseconds);
			}

			Console.WriteLine("Bench Single TestTable Add Total Elapse {3}ms, Average {0}ms, Min {1}ms, Max {2}ms", times.Average(), times.Min(), times.Max(), times.Sum());
			Console.WriteLine("Bench Single TestTableRelations Add Total Elapse {3}ms, Average {0}ms, Min {1}ms, Max {2}ms", timesRelations.Average(), timesRelations.Min(), timesRelations.Max(), timesRelations.Sum());
		}
		
		[Test, Explicit]
		public void BenchMultipleAdd()
		{
			Database.RegisterDataObject(typeof(TestTable));
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));

			Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelations>());
			Database.DeleteObject(Database.SelectAllObjects<TestTableRelationsEntries>());
			
			Assert.IsEmpty(Database.SelectAllObjects<TestTable>(), "Database shouldn't have any record For TestTable.");
			Assert.IsEmpty(Database.SelectAllObjects<TestTableRelations>(), "Database shouldn't have any record For TestTable.");
			Assert.IsEmpty(Database.SelectAllObjects<TestTableRelationsEntries>(), "Database shouldn't have any record For TestTable.");
			
			var objs = Enumerable.Range(0, 100).Select(i => new TestTable { TestField = string.Format("Bench Multiple Add '{0}'", i) }).ToArray();

			var stopWatch = Stopwatch.StartNew();
			Database.AddObject(objs);
			stopWatch.Stop();
			var times = stopWatch.ElapsedMilliseconds;
						
			var relationObjs = Enumerable.Range(0, 100).Select(i => new TestTableRelations { TestField = string.Format("Bench Multiple Relations Add '{0}'", i) }).ToArray();
			foreach (var obj in relationObjs)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { ForeignTestField = obj.ObjectId }).ToArray();
			
			var stopWatchRelation = Stopwatch.StartNew();
			Database.AddObject(relationObjs);
			stopWatchRelation.Stop();
			var timesRelations = stopWatchRelation.ElapsedMilliseconds;

			Console.WriteLine("Bench Multiple TestTable Add Elapsed Total {0}ms", times);			
			Console.WriteLine("Bench Multiple TestTableRelations Add Elapsed Total {0}ms", timesRelations);
		}
		#endregion

		#region Test Save
		/// <summary>
		/// Test IObjectDatabase.SaveObject(DataObject)
		/// </summary>
		[Test]
		public void TestSingleSave()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var obj = new TestTable { TestField = "Test Single Save" };
			var inserted = Database.AddObject(obj);
			
			obj.TestField = "Test Single Save New Value";
			Assert.IsTrue(obj.Dirty, "Changing TestTable Object should set Dirty Flag...");
			
			var saved = Database.SaveObject(obj);
			
			Assert.IsTrue(saved, "Changed Object Should be Saved Successfully in database...");
			Assert.IsFalse(obj.Dirty, "Changed Object should not have Dirty flag after Saving...");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.IsNotNull(retrieve, "Changed Object Should be retrieved from database...");
			Assert.AreEqual(obj.TestField, retrieve.TestField, "Previously Changed Object and newly retrieved Object should have the same field value...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// </summary>
		[Test]
		public void TestMultiSaveSameTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multi Save Same Table #{0}", i) }).ToArray();
			var inserted = Database.AddObject(objs);
			
			var current = 0;
			foreach (var obj in objs)
			{
				obj.TestField = string.Format("Test Multi Save Same Table Changed #{0}", current);
				current++;
				Assert.IsTrue(obj.Dirty, "Changing TestTable Objects should set Dirty Flag...");
			}
			
			var saved = Database.SaveObject(objs);
			
			Assert.IsTrue(saved, "Changed Objects Should be Saved Successfully in database...");
			foreach (var obj in objs)
				Assert.IsFalse(obj.Dirty, "Changed Objects should not have Dirty flag after Saving...");
			
			var retrieved = Database.FindObjectByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			
			Assert.IsNotNull(retrieved, "Changed Objects Collection Should be retrieved from database...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.TestField), retrieved.Select(obj => obj.TestField), "Previously Changed Objects and newly retrieved Objects should have the same field value...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// With Different DataObject Type
		/// </summary>
		[Test]
		public void TestMultiSaveDifferentTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var objsTestTable = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multi Save Different Table #{0}", i) }).ToArray();
			var objsTestTableAutoInc = Enumerable.Range(0, 10).Select(i => new TestTableAutoInc { TestField = string.Format("Test Auto Inc Multi Save Different Table #{0}", i) }).ToArray();
			var objsTestTableRelations = Enumerable.Range(0, 10).Select(i => new TestTableRelations { TestField = string.Format("Test Relation Multi Save Different Table #{0}", i) }).ToArray();
			
			// Fill Relation
			foreach (var obj in objsTestTableRelations)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { ForeignTestField = obj.ObjectId }).ToArray();

			var objs = objsTestTable.Concat(objsTestTableAutoInc).Concat(objsTestTableRelations);
			
			var inserted = Database.AddObject(objs);
			
			var current = 0;
			foreach (var obj in objs)
			{
				obj.TestField = string.Format("Test Multi Save Same Table Changed #{0}", current);
				current++;
				Assert.IsTrue(obj.Dirty, "Changing TestTable Objects should set Dirty Flag...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
					ent.Dirty = true;
			}
			
			var saved = Database.SaveObject(objs);
			
			Assert.IsTrue(saved, "Changed Objects Should be Saved Successfully in database...");
			foreach (var obj in objs)
				Assert.IsFalse(obj.Dirty, "Changed Objects should not have Dirty flag after Saving...");
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
					Assert.IsFalse(ent.Dirty, "Changed Objects Relations should not have Dirty flag after Saving...");
			}
			
			var retrieved = Database.FindObjectByKey<TestTable>(objsTestTable.Select(obj => obj.ObjectId))
				.Concat(Database.FindObjectByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(obj => obj.ObjectId)))
				.Concat(Database.FindObjectByKey<TestTableRelations>(objsTestTableRelations.Select(obj => obj.ObjectId)));
			
			Assert.IsNotNull(retrieved, "Changed Objects Collection Should be retrieved from database...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.TestField), retrieved.Select(obj => obj.TestField), "Previously Changed Objects and newly retrieved Objects should have the same field value...");
			
			foreach (var obj in retrieved.Where(o => o.GetType() == typeof(TestTableRelations)).OfType<TestTableRelations>())
			{
				CollectionAssert.AreEquivalent(objsTestTableRelations.First(o => o.ObjectId == obj.ObjectId).Entries.Select(o => o.ObjectId),
				                               obj.Entries.Select(o => o.ObjectId), "Previously Changed Objects and newly retrieved Objects should have the same Relations Collections...");
			}
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(DataObject)
		/// With null Value
		/// </summary>
		[Test]
		public void TestSingleSaveNull()
		{
			DataObject obj = null;
			Assert.Throws(typeof(NullReferenceException), () => Database.SaveObject(obj), "Trying to save a null object should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(DataObject)
		/// With non-registered Value
		/// </summary>
		[Test]
		public void TestSingleSaveNonRegistered()
		{
			DataObject obj = new TableNotRegistered();
			obj.Dirty = true;
			var saved = Database.SaveObject(obj);
			Assert.IsFalse(saved, "Trying to save a non registered object should not return success...");
			Assert.IsTrue(obj.Dirty, "Failing to save a non registered object should not remove Dirty Flag...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// With null Value
		/// </summary>
		[Test]
		public void TestMultiSaveNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Save with some null Values" }, null };
			Assert.Throws(typeof(NullReferenceException), () => Database.SaveObject(dbo), "Saving a Multiple with Null Objects Should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// With non-registered Value
		/// </summary>
		[Test]
		public void TestMultiSaveNonRegistered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Save with some non registered Values" }, new TableNotRegistered() };
			var added = Database.AddObject(dbo);
			
			foreach (var obj in dbo)
				obj.Dirty = true;
			
			var saved = Database.SaveObject(dbo);
			
			Assert.IsFalse(saved, "Saving Multiple non registered Object should not return success...");
			Assert.IsFalse(dbo[0].Dirty, "Saving a Multiple non registered Object should not have Dirty Flag on registered Object...");
			Assert.IsTrue(dbo[1].Dirty, "Saving a Multiple non registered Object should have Dirty Flag on registered Object...");
		}
		#endregion
		
		#region Test Delete
		/// <summary>
		/// Test IObjectDatabase.SaveObject(DataObject)
		/// </summary>
		[Test]
		public void TestSingleDelete()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var obj = new TestTable { TestField = "Test Single Delete" };
			var inserted = Database.AddObject(obj);
			
			Assert.IsTrue(obj.IsPersisted, "Added Object should have Persisted Flag...");
			
			var deleted = Database.DeleteObject(obj);
			
			Assert.IsTrue(deleted, "Added Object should be Successfully Deleted from database...");
			Assert.IsFalse(obj.IsPersisted, "Deleted Object should not have Persisted Flag");
			Assert.IsTrue(obj.IsDeleted, "Deleted Object should have Deleted Flag");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			Assert.IsNull(retrieve, "Retrieving Deleted Object should return null...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// </summary>
		[Test]
		public void TestMultiDeleteSameTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multiple Delete #{0}", i) }).ToArray();
			var inserted = Database.AddObject(objs);
			
			foreach (var obj in objs)
				Assert.IsTrue(obj.IsPersisted, "Added Objects should have Persisted Flag...");
			
			var deleted = Database.DeleteObject(objs);
			
			Assert.IsTrue(deleted, "Added Objects should be Successfully Deleted from database...");
			foreach (var obj in objs)
			{
				Assert.IsFalse(obj.IsPersisted, "Deleted Objects should not have Persisted Flag");
				Assert.IsTrue(obj.IsDeleted, "Deleted Objects should have Deleted Flag");
			}
			
			var retrieve = Database.FindObjectByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			Assert.IsNotNull(retrieve, "Retrieving Deleted Objects Collection should not return null...");
			Assert.IsEmpty(retrieve, "Retrieved Deleted Objects Collection should be empty...");
		}
		
		// TODO : Finish Delete Tests
		
		#endregion
		
		#region Test Select All
		/// <summary>
		/// Test IObjectDatabase.SelectAll`TObject()
		/// </summary>
		[Test]
		public void TestSelectAll()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var objs = Database.SelectAllObjects<TestTable>();
			
			Assert.IsNotNull(objs, "Retrieving from a Registered Table should not return null...");
			
			var delete = Database.DeleteObject(objs);
			
			Assert.IsTrue(delete, "TestTable Objects should be deleted successfully...");
			
			objs = Database.SelectAllObjects<TestTable>();
			Assert.IsNotNull(objs, "TestTable Select All from an Empty table should not return null...");
			Assert.IsEmpty(objs, "TestTable Select All from an Empty table should return an Empty Collection...");
			
			objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Select All Object #{0}", i) }).ToList();
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "TestTable Objects should be inserted successfully...");
			
			var retrieve = Database.SelectAllObjects<TestTable>();
			Assert.IsNotEmpty(retrieve, "TestTable Objects Select All should return previously inserted objects...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.ObjectId), retrieve.Select(obj => obj.ObjectId), "TestTable Select All should return Objects with Same ID as Inserted...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.TestField), retrieve.Select(obj => obj.TestField), "TestTable Select All shoud return Objects with same Value Field as Inserted...");			
		}
		
		/// <summary>
		/// Test IObjectDatabase.SelectAll`TObject()
		/// With Non Registered Table
		/// </summary>
		[Test]
		public void TestSelectAllNonRegistered()
		{
			Assert.Throws(typeof(DatabaseException), () => Database.SelectAllObjects<TableNotRegistered>(), "Trying to Query a Non Registered Table should throw a DatabaseException...");
		}
		#endregion
		
		#region Test Select Objects
		/// <summary>
		/// Test IObjectDatabase.SelectObjects`TObject(string, KeyValuePair`string, object)
		/// </summary>
		[Test]
		public void TestSelectObjects()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var objInitial = new TestTable { TestField = "Select Objects Null Where Test" };
			Database.AddObject(objInitial);

			var allobjects = Database.SelectAllObjects<TestTable>();
			
			Assert.IsNotEmpty(allobjects, "This Test Need some Data to be Accurate...");
			
			var nullWhere = Database.SelectObjects<TestTable>("", new KeyValuePair<string, object>[] { });
			
			CollectionAssert.AreEqual(allobjects.Select(obj => obj.ObjectId), nullWhere.Select(obj => obj.ObjectId), "Select Objects with Null Where clause should retrieve Objects similar to Select All...");
			
			var dumbWhere = Database.SelectObjects<TestTable>("@whClause", new KeyValuePair<string, object>("@whClause", 1));
			
			CollectionAssert.AreEqual(allobjects.Select(obj => obj.ObjectId), dumbWhere.Select(obj => obj.ObjectId), "Select Objects with Dumb Where clause should retrieve Objects similar to Select All...");
			
			var simpleWhere = Database.SelectObjects<TestTable>("`TestField` = @TestField", new KeyValuePair<string, object>("@TestField", objInitial.TestField));
			
			CollectionAssert.Contains(simpleWhere.Select(obj => obj.ObjectId), objInitial.ObjectId, "Select Objects with Simple Where clause should retrieve Object similar to Created one...");
			
			var complexWhere = Database.SelectObjects<TestTable>("`TestField` = @TestField AND `Test_Table_ID` = @ObjectId", new [] { new KeyValuePair<string, object>("@TestField", objInitial.TestField), new KeyValuePair<string, object>("@ObjectId", objInitial.ObjectId) });
			
			CollectionAssert.Contains(complexWhere.Select(obj => obj.ObjectId.ToLower()), objInitial.ObjectId.ToLower(), "Select Objects with Complex Where clause should retrieve Object similar to Created one...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SelectObjects`TObject(string, IEnumerable`IEnumerable`KeyValuePair`string, object)
		/// </summary>
		[Test]
		public void TestSelectObjectsWithMultipleWhereClause()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var delete = Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			Assert.IsTrue(delete, "TestTable Objects should be deleted successfully...");
			Assert.IsEmpty(Database.SelectAllObjects<TestTable>(), "This test needs an Empty Table to Run Successfully...");

			var objs = new []{ "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" }
			.Select(grp => Enumerable.Range(0, 100).Select(i => new TestTable { TestField = grp } )).ToDictionary(kv => kv.First().TestField, kv => kv.ToArray());
			
			var added = Database.AddObject(objs.SelectMany(obj => obj.Value));
			
			Assert.IsTrue(added, "TestTable Objects should be added successfully...");
			
			var parameters = new []{ "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" }
			.Select(grp => new [] { new KeyValuePair<string, object>("@TestField", grp) } );
			
			var retrieve = Database.SelectObjects<TestTable>("`TestField` = @TestField", parameters);
			var objectByGroup = new []{ "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" }
			.Select((grp, index) => new { Grp = grp, Objects = retrieve.ElementAt(index) });
			
			Assert.IsNotNull(retrieve, "Retrieve Sets from Select Objects should not return null value...");
			Assert.IsNotEmpty(retrieve, "Retrieve Set from Select Objects should not be Empty...");
			
			foreach (var sets in objectByGroup)
			{
				Assert.IsNotEmpty(sets.Objects, "Retrieve SubSets from Select Objects should not be Empty...");
				Assert.IsTrue(sets.Objects.All(obj => obj.TestField.Equals(sets.Grp, StringComparison.OrdinalIgnoreCase)),
				              "Retrieve SubSets from Select Objects should have the Where Clause Matching their Field Value...");
				CollectionAssert.AreEquivalent(objs[sets.Grp].Select(obj => obj.ObjectId), sets.Objects.Select(obj => obj.ObjectId),
				                              "Retrieve SubSets from Select Objects should return the same ObjectId Sets as Created...");
				
			}
			
			var orderedObjs = objs.SelectMany(obj => obj.Value).ToArray();
			var parameterMany = orderedObjs.Select(obj => new [] { new KeyValuePair<string, object>("@Testfield", obj.TestField), new KeyValuePair<string, object>("@ObjectId", obj.ObjectId) });
			var retrieveMany = Database.SelectObjects<TestTable>("`TestField` = @TestField AND `Test_Table_ID` = @ObjectId", parameterMany);
			
			Assert.IsNotNull(retrieveMany, "Retrieve Sets from Select Objects should not return null value...");
			Assert.IsNotEmpty(retrieveMany, "Retrieve Set from Select Objects should not be Empty...");

			var resultsMany = retrieveMany.Select(obj => obj.Single());
			CollectionAssert.AreEqual(orderedObjs.Select(obj => obj.ObjectId.ToLower()), resultsMany.Select(obj => obj.ObjectId.ToLower()),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set ObjectId...");
			CollectionAssert.AreEqual(orderedObjs.Select(obj => obj.TestField.ToLower()), resultsMany.Select(obj => obj.TestField.ToLower()),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set Field Value...");
		}
		
		// TODO Finish Test Select Objects
		
		#endregion
	}
}
