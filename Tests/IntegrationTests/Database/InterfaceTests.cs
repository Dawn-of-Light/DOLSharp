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
using NUnit.Framework;

namespace DOL.Integration.Database
{
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
		public void TestMultipleAddSameTable()
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
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objsId);
			
			Assert.IsNotNull(retrieved, "Test Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsId, retrieved.Select(obj => obj.ObjectId), "Test Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieved.Select(obj => obj.TestField), "Test Objects previously added and retrieved objects should have equals fields...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.Add(IEnumerable`DataObject)
		/// With Different DataObject Types...
		/// </summary>
		[Test]
		public void TestMultipleAddDifferentTable()
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
			
			var retrievedTestTable = Database.FindObjectsByKey<TestTable>(objsTestTableId);
			Assert.IsNotNull(retrievedTestTable, "TestTable Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsTestTableId, retrievedTestTable.Select(obj => obj.ObjectId), "TestTable Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objsTestTable.Select(obj => obj.TestField), retrievedTestTable.Select(obj => obj.TestField), "TestTable Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableAutoInc = Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(key => (object)key.PrimaryKey));
			Assert.IsNotNull(retrievedTestTableAutoInc, "TestTableAutoInc Objects retrieved Collection should not be null...");
			CollectionAssert.AreEqual(objsTestTableAutoInc.Select(key => key.PrimaryKey.ToString()), retrievedTestTableAutoInc.Select(obj => obj.ObjectId), "TestTableAutoInc Objects previously added and retrieved objects should have same ObjectId...");
			CollectionAssert.AreEqual(objsTestTableAutoInc.Select(obj => obj.TestField), retrievedTestTableAutoInc.Select(obj => obj.TestField), "TestTableAutoInc Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableRelations = Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelationsId);
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
		public void TestMultipleSaveSameTable()
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
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			
			Assert.IsNotNull(retrieved, "Changed Objects Collection Should be retrieved from database...");
			CollectionAssert.AreEquivalent(objs.Select(obj => obj.TestField), retrieved.Select(obj => obj.TestField), "Previously Changed Objects and newly retrieved Objects should have the same field value...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// With Different DataObject Type
		/// </summary>
		[Test]
		public void TestMultipleSaveDifferentTable()
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
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objsTestTable.Select(obj => obj.ObjectId))
				.Concat(Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(obj => obj.ObjectId)))
				.Concat(Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelations.Select(obj => obj.ObjectId)));
			
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
		public void TestMultipleSaveNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Save with some null Values" }, null };
			Assert.Throws(typeof(NullReferenceException), () => Database.SaveObject(dbo), "Saving a Collection with Null Objects Should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.SaveObject(IEnumerable`DataObject)
		/// With non-registered Value
		/// </summary>
		[Test]
		public void TestMultipleSaveNonRegistered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Save with some non registered Values" }, new TableNotRegistered() };
			var added = Database.AddObject(dbo);
			
			foreach (var obj in dbo)
				obj.Dirty = true;
			
			var saved = Database.SaveObject(dbo);
			
			Assert.IsFalse(saved, "Saving Multiple non registered Object should not return success...");
			Assert.IsFalse(dbo[0].Dirty, "Saving a Collection with non registered Object should not have Dirty Flag on registered Object...");
			Assert.IsTrue(dbo[1].Dirty, "Saving a Collection with non registered Object should have Dirty Flag on non-registered Object...");
		}
		#endregion
		
		#region Test Delete
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(DataObject)
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
		/// Test IObjectDatabase.DeleteObject(IEnumerable`DataObject)
		/// </summary>
		[Test]
		public void TestMultipleDeleteSameTable()
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
			
			var retrieve = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			Assert.IsNotNull(retrieve, "Retrieving Deleted Objects Collection should not return null...");
			Assert.IsEmpty(retrieve.Where(obj => obj != null), "Retrieved Deleted Objects Collection should be empty...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(IEnumerable`DataObject)
		/// With Different DataObject Type
		/// </summary>
		[Test]
		public void TestMultipleDeleteDifferentTable()
		{
			Database.RegisterDataObject(typeof(TestTable));
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			Database.RegisterDataObject(typeof(TestTableRelations));
			Database.RegisterDataObject(typeof(TestTableRelationsEntries));
			
			var objsTestTable = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Multi Delete Different Table #{0}", i) }).ToArray();
			var objsTestTableAutoInc = Enumerable.Range(0, 10).Select(i => new TestTableAutoInc { TestField = string.Format("Test Auto Inc Multi Delete Different Table #{0}", i) }).ToArray();
			var objsTestTableRelations = Enumerable.Range(0, 10).Select(i => new TestTableRelations { TestField = string.Format("Test Relation Multi Delete Different Table #{0}", i) }).ToArray();
			
			// Fill Relation
			foreach (var obj in objsTestTableRelations)
				obj.Entries = Enumerable.Range(0, 5).Select(i => new TestTableRelationsEntries { ForeignTestField = obj.ObjectId }).ToArray();

			var objs = objsTestTable.Concat(objsTestTableAutoInc).Concat(objsTestTableRelations);
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "Object Should be inserted to test Delete Cases...");
			
			// Delete Objects
			var deleted = Database.DeleteObject(objs);
			
			Assert.IsTrue(deleted, "Object should be deleted successfully...");
			
			foreach (var obj in objs)
			{
					Assert.IsTrue(obj.IsDeleted, "Deleted Objects should have Deleted Flag...");
					Assert.IsFalse(obj.IsPersisted, "Deleted Objects should not have Persisted Flag...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
				{
					Assert.IsTrue(ent.IsDeleted, "Deleted Objects should have Deleted Flag...");
					Assert.IsFalse(ent.IsPersisted, "Deleted Objects should not have Persisted Flag...");
				}
			}

			var retrieved = Database.FindObjectsByKey<TestTable>(objsTestTable.Select(obj => obj.ObjectId))
				.Concat(Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(obj => obj.ObjectId)))
				.Concat(Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelations.Select(obj => obj.ObjectId)));
			
			Assert.IsNotNull(retrieved, "Deleted Objects Collection Should be retrieved Empty not null from database...");
			Assert.IsEmpty(retrieved.Where(obj => obj != null), "Deleted Objects Collection Should be retrieved Empty from database...");
			
			var relRetrieved = Database.FindObjectsByKey<TestTableRelationsEntries>(objsTestTableRelations.SelectMany(obj => obj.Entries).Select(ent => ent.ObjectId));

			Assert.IsNotNull(relRetrieved, "Deleted Objects Collection Should be retrieved Empty not null from database...");
			Assert.IsEmpty(relRetrieved.Where(obj => obj != null), "Deleted Objects Collection Should be retrieved Empty from database...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(DataObject)
		/// With Null Object
		/// </summary>
		[Test]
		public void TestSingleDeleteNull()
		{
			DataObject obj = null;
			Assert.Throws(typeof(NullReferenceException), () => Database.DeleteObject(obj), "Trying to delete a null object should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(DataObject)
		/// With non-registered Value
		/// </summary>
		[Test]
		public void TestSingleDeleteNonRegistered()
		{
			DataObject obj = new TableNotRegistered();
			var deleted = Database.DeleteObject(obj);
			Assert.IsFalse(deleted, "Trying to Delete a non registered object should not return success...");
			Assert.IsFalse(obj.IsDeleted, "Failing to Delete a non registered object should not set Deleted Flag...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(IEnumerable`DataObject)
		/// With null Value
		/// </summary>
		[Test]
		public void TestMultipleDeleteNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Delete with some null Values" }, null };
			Assert.Throws(typeof(NullReferenceException), () => Database.DeleteObject(dbo), "Deleting a Collection with Null Objects Should throw NullReferenceException...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.DeleteObject(IEnumerable`DataObject)
		/// With non-registered Value
		/// </summary>
		[Test]
		public void TestMutipleDeleteNonRegistered()
		{
			Database.RegisterDataObject(typeof(TestTable));
			var dbo = new DataObject[] { new TestTable { TestField = "Test Multiple Delete with some non registered Values" }, new TableNotRegistered() };
			var added = Database.AddObject(dbo);
	
			var deleted = Database.DeleteObject(dbo);
			
			Assert.IsFalse(deleted, "Deleting Multiple non registered Object should not return success...");
			Assert.IsTrue(dbo[0].IsDeleted, "Deleting a Collection with non registered Object should set Deleted Flag on registered Object...");
			Assert.IsFalse(dbo[1].IsDeleted, "Deleting a Collection with non registered Object should not set Deleted Flag non-registered Object...");
		}
		
		#endregion
		
		#region Find ObjectByKey
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(object key)
		/// </summary>
		[Test]
		public void TestSingleFindObjectByKey()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var obj = new TestTable { TestField = "Test For Single Find Object By Key" };
			
			var inserted = Database.AddObject(obj);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could not retrieve previously added Object...");
			Assert.AreEqual(obj.ObjectId, retrieve.ObjectId, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieve.TestField, "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCase = Database.FindObjectByKey<TestTable>(obj.ObjectId.ToUpper());
			
			Assert.IsNotNull(retrieveCase, "Find Object By Key Could not retrieve previously added Object using Case Mismatch...");
			Assert.AreEqual(obj.ObjectId, retrieveCase.ObjectId, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieveCase.TestField, "Find Object By Key Should return similar Object to created one...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectsByKey`TObject(IEnumerable`object key)
		/// </summary>
		[Test]
		public void TestMultipleFindObjectByKey()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test For Multiple Find Objects By Key #{0}", i) }).ToArray();
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieve.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.ObjectId), retrieve.Select(obj => obj.ObjectId), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieve.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCase = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId.ToUpper()));
			
			Assert.IsNotNull(retrieveCase, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieveCase.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects using Case Mismatch...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.ObjectId), retrieveCase.Select(obj => obj.ObjectId), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieveCase.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(object key)
		/// </summary>
		[Test]
		public void TestSingleFindObjectByKeyPrimaryKey()
		{
			Database.RegisterDataObject(typeof(TestTablePrimaryKey));
			
			var obj = new TestTablePrimaryKey { PrimaryKey = DOL.Database.UniqueID.IDGenerator.GenerateID(), TestField = "Test For Single Find Object By Primary Key" };
			
			var inserted = Database.AddObject(obj);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTablePrimaryKey>(obj.PrimaryKey);
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could not retrieve previously added Object...");
			Assert.AreEqual(obj.PrimaryKey, retrieve.PrimaryKey, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieve.TestField, "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCase = Database.FindObjectByKey<TestTablePrimaryKey>(obj.PrimaryKey.ToUpper());
			
			Assert.IsNotNull(retrieveCase, "Find Object By Key Could not retrieve previously added Object using Case Mismatch...");
			Assert.AreEqual(obj.PrimaryKey, retrieveCase.PrimaryKey, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieveCase.TestField, "Find Object By Key Should return similar Object to created one...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectsByKey`TObject(IEnumerable`object key)
		/// </summary>
		[Test]
		public void TestMultipleFindObjectByKeyPrimaryKey()
		{
			Database.RegisterDataObject(typeof(TestTablePrimaryKey));
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTablePrimaryKey { PrimaryKey = DOL.Database.UniqueID.IDGenerator.GenerateID(),
			                                          	TestField = string.Format("Test For Multiple Find Objects By Key #{0}", i) }).ToArray();
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTablePrimaryKey>(objs.Select(obj => obj.PrimaryKey));
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieve.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.PrimaryKey), retrieve.Select(obj => obj.PrimaryKey), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieve.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCase = Database.FindObjectsByKey<TestTablePrimaryKey>(objs.Select(obj => obj.PrimaryKey.ToUpper()));
			
			Assert.IsNotNull(retrieveCase, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieveCase.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects using Case Mismatch...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.PrimaryKey), retrieveCase.Select(obj => obj.PrimaryKey), "Find Object By Key Should return similar Object to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieveCase.Select(obj => obj.TestField), "Find Object By Key Should return similar Object to created ones...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(object key)
		/// </summary>
		[Test]
		public void TestSingleFindObjectByKeyPrimaryKeyAutoInc()
		{
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			
			var obj = new TestTableAutoInc { TestField = "Test For Single Find Object By Primary Key Auto Inc" };
			
			var inserted = Database.AddObject(obj);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTableAutoInc>(obj.PrimaryKey);
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could not retrieve previously added Object...");
			Assert.AreEqual(obj.PrimaryKey, retrieve.PrimaryKey, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieve.TestField, "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCast = Database.FindObjectByKey<TestTableAutoInc>((long)obj.PrimaryKey);
			
			Assert.IsNotNull(retrieveCast, "Find Object By Key Could not retrieve previously added Object using Numeric Cast...");
			Assert.AreEqual(obj.PrimaryKey, retrieveCast.PrimaryKey, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieveCast.TestField, "Find Object By Key Should return similar Object to created one...");
			
			var retrieveString = Database.FindObjectByKey<TestTableAutoInc>(obj.PrimaryKey.ToString());
			
			Assert.IsNotNull(retrieveString, "Find Object By Key Could not retrieve previously added Object using String Cast...");
			Assert.AreEqual(obj.PrimaryKey, retrieveString.PrimaryKey, "Find Object By Key Should return similar Object to created one...");
			Assert.AreEqual(obj.TestField, retrieveString.TestField, "Find Object By Key Should return similar Object to created one...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectsByKey`TObject(IEnumerable`object key)
		/// </summary>
		[Test]
		public void TestMultipleFindObjectByKeyPrimaryKeyAutoInc()
		{
			Database.RegisterDataObject(typeof(TestTableAutoInc));
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTableAutoInc { TestField = string.Format("Test For Multiple Find Objects By Key Auto Inc #{0}", i) }).ToArray();
			
			var inserted = Database.AddObject(objs);
			
			Assert.IsTrue(inserted, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => obj.PrimaryKey).Cast<object>());
			
			Assert.IsNotNull(retrieve, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieve.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.PrimaryKey), retrieve.Select(obj => obj.PrimaryKey), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieve.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCast = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => (long)obj.PrimaryKey).Cast<object>());
			
			Assert.IsNotNull(retrieveCast, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieveCast.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects using Numeric Cast...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.PrimaryKey), retrieveCast.Select(obj => obj.PrimaryKey), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieveCast.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveString = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => obj.PrimaryKey.ToString()));
			
			Assert.IsNotNull(retrieveString, "Find Object By Key Could should not return a null collection...");
			Assert.IsNotEmpty(retrieveString.Where(obj => obj != null), "Find Object By Key Could not retrieve previously added Objects using String Cast...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.PrimaryKey), retrieveString.Select(obj => obj.PrimaryKey), "Find Object By Key Should return similar Objects to created ones...");
			CollectionAssert.AreEqual(objs.Select(obj => obj.TestField), retrieveString.Select(obj => obj.TestField), "Find Object By Key Should return similar Objects to created ones...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(object key)
		/// With Null Value
		/// </summary>
		[Test]
		public void TestSingleFindObjectByKeyWithNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			object key = null;
			var dbo = Database.FindObjectByKey<TestTable>(key);
			Assert.IsNull(dbo, "Searching an Object By Key with a Null Key should return a null object...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(IEnumerable`object key)
		/// With Some Null Values
		/// </summary>
		[Test]
		public void TestMultipleFindObjectByKeyWithNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			object key = null;
			var dbo = new TestTable { TestField = "Test Searching Object by Key with some Null" };
			Database.AddObject(dbo);
			var result = Database.FindObjectsByKey<TestTable>(new object[] { key, dbo.ObjectId }).ToArray();
			Assert.IsNotEmpty(result, "Searching with multiple keys including null shoud not return an empty collection...");
			Assert.IsNull(result[0], "Searching with multiple keys including null should return null data object for null key...");
			Assert.IsNotNull(result[1], "Searching with multiple keys including null should return a valid data object for non null key...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(object key)
		/// With Unregistered Table
		/// </summary>
		[Test]
		public void TestSingleFindObjectByKeyWithNonRegistered()
		{
			Assert.Throws(typeof(DatabaseException), () => Database.FindObjectByKey<TableNotRegistered>(1), "");
		}
		
		/// <summary>
		/// Test IObjectDatabase.FindObjectByKey`TObject(IEnumerable`object key)
		/// With Unregistered Table and multiple key
		/// </summary>
		[Test]
		public void TestMultipleFindObjectByKeyWithNonRegistered()
		{
			Assert.Throws(typeof(DatabaseException), () => Database.FindObjectByKey<TableNotRegistered>(new object[] { 1, 2 }), "");
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
			
			Assert.IsNotEmpty(allobjects, "Select Objects Test Need some Data to be Accurate...");
			
			var nullWhere = Database.SelectObjects<TestTable>("", new QueryParameter[] { });
			
			CollectionAssert.AreEqual(allobjects.Select(obj => obj.ObjectId), nullWhere.Select(obj => obj.ObjectId), "Select Objects with Null Where clause should retrieve Objects similar to Select All...");
			
			var dumbWhere = Database.SelectObjects<TestTable>("@whClause", new QueryParameter("@whClause", 1));
			
			CollectionAssert.AreEqual(allobjects.Select(obj => obj.ObjectId), dumbWhere.Select(obj => obj.ObjectId), "Select Objects with Dumb Where clause should retrieve Objects similar to Select All...");
			
			var simpleWhere = Database.SelectObjects<TestTable>(DB.Column("TestField").IsEqualTo(objInitial.TestField));
			
			CollectionAssert.Contains(simpleWhere.Select(obj => obj.ObjectId), objInitial.ObjectId, "Select Objects with Simple Where clause should retrieve Object similar to Created one...");

			var complexWhere = Database.SelectObjects<TestTable>(DB.Column("TestField").IsEqualTo(objInitial.TestField).And(DB.Column("Test_Table_ID").IsEqualTo(objInitial.ObjectId)));
			
			CollectionAssert.Contains(complexWhere.Select(obj => obj.ObjectId.ToLower()), objInitial.ObjectId.ToLower(), "Select Objects with Complex Where clause should retrieve Object similar to Created one...");

			Assert.IsTrue(Database.DeleteObject(Database.SelectObjects<TestTable>(DB.Column("TestField").IsNull())));
			var objNull = new TestTable { TestField = null };
			var nullAdd = Database.AddObject(objNull);
			
			Assert.IsTrue(nullAdd, "Select Objects null parameter Test Need some null object to be Accurate...");

			var nullParam = Database.SelectObjects<TestTable>(DB.Column("TestField").IsEqualTo(null));
			
			CollectionAssert.IsEmpty(nullParam, "Select Objects with Null Parameter Query should not return any record...");

			var resultsWithTestfieldNull = Database.SelectObjects<TestTable>(DB.Column("TestField").IsNull());
			var allObjectsAfter = Database.SelectAllObjects<TestTable>();

			Assert.AreEqual(1, resultsWithTestfieldNull.Count);
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

			var parameters = new[] { "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" };
			var retrieve = Database.MultipleSelectObjects<TestTable>(parameters.Select(parameter => DB.Column("TestField").IsEqualTo(parameter)));

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
			var retrieveMany = Database.MultipleSelectObjects<TestTable>(orderedObjs.Select(obj => DB.Column("TestField").IsEqualTo(obj.TestField).And(DB.Column("Test_Table_ID").IsEqualTo(obj.ObjectId))));
			
			Assert.IsNotNull(retrieveMany, "Retrieve Sets from Select Objects should not return null value...");
			Assert.IsNotEmpty(retrieveMany, "Retrieve Set from Select Objects should not be Empty...");

			var resultsMany = retrieveMany.Select(obj => obj.Single());
			CollectionAssert.AreEqual(orderedObjs.Select(obj => obj.ObjectId.ToLower()), resultsMany.Select(obj => obj.ObjectId.ToLower()),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set ObjectId...");
			CollectionAssert.AreEqual(orderedObjs.Select(obj => obj.TestField.ToLower()), resultsMany.Select(obj => obj.TestField.ToLower()),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set Field Value...");

			var parameterManyWithMissing = new [] { new [] { new QueryParameter("@TestField", "No Known Value"), new QueryParameter("@ObjectId", "Probably Nothing") },
			new [] { new QueryParameter("@TestField", "Absolutely None"), new QueryParameter("@ObjectId", "Nothing for Sure") } }
			.Concat(orderedObjs.Select(obj => new [] { new QueryParameter("@Testfield", obj.TestField), new QueryParameter("@ObjectId", obj.ObjectId) }));
			
			var retrieveManyWithMissing = Database.SelectObjects<TestTable>("`TestField` = @TestField AND `Test_Table_ID` = @ObjectId", parameterManyWithMissing);
			
			Assert.IsNotNull(retrieveManyWithMissing, "Retrieve Sets from Select Objects should not return null value...");
			Assert.IsNotEmpty(retrieveManyWithMissing, "Retrieve Set from Select Objects should not be Empty...");

			var resultsManyWithMissing = retrieveManyWithMissing.Select(obj => obj.SingleOrDefault());
			CollectionAssert.AreEqual(new [] { "", "" }.Concat(orderedObjs.Select(obj => obj.ObjectId.ToLower())), resultsManyWithMissing.Select(obj => obj != null ? obj.ObjectId.ToLower() : string.Empty),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set ObjectId...");
			CollectionAssert.AreEqual(new [] { "", "" }.Concat(orderedObjs.Select(obj => obj.TestField.ToLower())), resultsManyWithMissing.Select(obj => obj != null ? obj.TestField.ToLower() : string.Empty),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set Field Value...");
			
			
		}
		
		/// <summary>
		/// Test IObjectDatabase.SelectObject(s)`TObject()
		/// With Non Registered Table
		/// </summary>
		[Test]
		public void TestSelectObjectsNonRegistered()
		{
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObject<TableNotRegistered>("1"), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObject<TableNotRegistered>("1", DOL.Database.Transaction.IsolationLevel.DEFAULT), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>("1"), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>("1", DOL.Database.Transaction.IsolationLevel.DEFAULT), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>("1", new QueryParameter()), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>("1", new [] { new QueryParameter() }), "Trying to Query a Non Registered Table should throw a DatabaseException...");
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>("1", new [] { new [] { new QueryParameter() } }), "Trying to Query a Non Registered Table should throw a DatabaseException...");

		}
		
		/// <summary>
		/// Test IObjectDatabase.SelectObject(s)`TObject()
		/// With null or Empty Values
		/// </summary>
		[Test]
		public void TestSelectObjectsWithNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var delete = Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			Assert.IsTrue(delete, "TestTable Objects should be deleted successfully...");
			Assert.IsEmpty(Database.SelectAllObjects<TestTable>(), "This test needs an Empty Table to Run Successfully...");
			
			var objInitial = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Select Objects Null Values Test #{0}", i) });
			Database.AddObject(objInitial);
			
			var allobjects = Database.SelectAllObjects<TestTable>();
			
			Assert.IsNotEmpty(allobjects, "This Test Need some Data to be Accurate...");
			
			var retrieveNull = Database.SelectObject<TestTable>((string)null);
			
			CollectionAssert.Contains(allobjects.Select(obj => obj.ObjectId), retrieveNull.ObjectId, "");
			
			var retrieveNullWithIsolation = Database.SelectObject<TestTable>(null, DOL.Database.Transaction.IsolationLevel.SERIALIZABLE);
			
			CollectionAssert.Contains(allobjects.Select(obj => obj.ObjectId), retrieveNullWithIsolation.ObjectId, "");

			var retrieveMultipleNull = Database.SelectObjects<TestTable>((string)null);

			CollectionAssert.AreEquivalent(allobjects.Select(obj => obj.ObjectId), retrieveMultipleNull.Select(obj => obj.ObjectId), "");
			
			var retrieveMultipleNullWithIsolation = Database.SelectObjects<TestTable>(null, DOL.Database.Transaction.IsolationLevel.SERIALIZABLE);

			CollectionAssert.AreEquivalent(allobjects.Select(obj => obj.ObjectId), retrieveMultipleNullWithIsolation.Select(obj => obj.ObjectId), "");
			
			var retrieveParameter = Database.SelectObjects<TestTable>(null, new QueryParameter());
			
			CollectionAssert.AreEquivalent(allobjects.Select(obj => obj.ObjectId), retrieveParameter.Select(obj => obj.ObjectId), "");
			
			var retrieveParameters = Database.SelectObjects<TestTable>(null, new QueryParameter[] { });
			
			CollectionAssert.AreEquivalent(allobjects.Select(obj => obj.ObjectId), retrieveParameters.Select(obj => obj.ObjectId), "");

			var retrieveMultipleParameters = Database.SelectObjects<TestTable>(null, new [] { new QueryParameter[] { } });
			
			CollectionAssert.AreEquivalent(allobjects.Select(obj => obj.ObjectId), retrieveMultipleParameters.First().Select(obj => obj.ObjectId), "");
			
			Assert.Throws(typeof(ArgumentNullException), () => Database.SelectObjects<TestTable>(null, (QueryParameter)null), "");
			
			Assert.Throws(typeof(ArgumentNullException), () => Database.SelectObjects<TestTable>(null, (IEnumerable<QueryParameter>)null), "");
			
			Assert.Throws(typeof(ArgumentNullException), () => Database.SelectObjects<TestTable>(null, (IEnumerable<IEnumerable<QueryParameter>>)null), "");
			
			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObjects<TestTable>(null, new QueryParameter[] { null }), "");
			
			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObjects<TestTable>(null, new [] { new QueryParameter[] { null } }), "");
			
			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObjects<TestTable>(null, new QueryParameter[][] { null }), "");

			Assert.Throws(typeof(ArgumentException), () => Database.SelectObjects<TestTable>(null, new QueryParameter[][] {  }), "");

			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObjects<TestTable>((WhereExpression)null), "");
		}
		
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
			Assert.Throws(typeof(DatabaseException), () => Database.SelectAllObjects<TableNotRegistered>(DOL.Database.Transaction.IsolationLevel.DEFAULT), "Trying to Query a Non Registered Table should throw a DatabaseException...");
		}
		#endregion
		
		#region Test Count Objects
		/// <summary>
		/// Test IObjectDatabase.GetObjectCount`TObject
		/// </summary>
		[Test]
		public void TestCountObject()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			
			var count = Database.GetObjectCount<TestTable>();
			
			Assert.AreEqual(0, count, "Test Table shouldn't Have any Object after deleting all records...");
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Count Object Test #{0}", i) });
			
			Database.AddObject(objs);
			
			var newCount = Database.GetObjectCount<TestTable>();
			
			Assert.AreEqual(10, newCount, "Test Table should return same object count as added collection...");
			
			var whereCount = Database.GetObjectCount<TestTable>("1");
			
			Assert.AreEqual(10, whereCount, "Test Table should return same object count as added collection...");
			
			var filterCount = Database.GetObjectCount<TestTable>("`TestField` LIKE '%1'");
			
			Assert.AreEqual(1, filterCount, "Test Table should return same object count as filtered collection...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.GetObjectCount`TObject
		/// with null where clause
		/// </summary>
		[Test]
		public void TestCountObjectWithNull()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			
			var count = Database.GetObjectCount<TestTable>(null);
			
			Assert.AreEqual(0, count, "Test Table shouldn't Have any Object after deleting all records...");
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Count Object Test #{0}", i) });
			
			Database.AddObject(objs);
			
			var newCount = Database.GetObjectCount<TestTable>(null);
			
			Assert.AreEqual(10, newCount, "Test Table should return same object count as added collection...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.GetObjectCount`TObject
		/// with non registered object
		/// </summary>
		[Test]
		public void TestCountObjectWithNotRegistered()
		{
			Assert.Throws(typeof(DatabaseException), () => Database.GetObjectCount<TableNotRegistered>(), "Get Object Count should throw exception for unregistered tables...");
			Assert.Throws(typeof(DatabaseException), () => Database.GetObjectCount<TableNotRegistered>("1"), "Get Object Count should throw exception for unregistered tables...");
		}
		#endregion
		
		#region Test Escape
		/// <summary>
		/// Test IObjectDatabase.Escape(string)
		/// </summary>
		[Test]
		public virtual void TestEscape()
		{
			var test = "'";
			
			Assert.AreEqual("''", Database.Escape(test), "Sqlite String Escape Test Failure...");
		}
		
		/// <summary>
		/// Test IObjectDatabase.Escape(string)
		/// With null Value
		/// </summary>
		[Test]
		public virtual void TestEscapeWithNull()
		{
			Assert.Throws(typeof(NullReferenceException), () => Database.Escape(null), "SQL Escape string with Null value should throw Null Reference Exception...");
		}
		#endregion
	}
}
