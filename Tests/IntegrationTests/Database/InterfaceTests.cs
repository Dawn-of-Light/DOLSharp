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
			
			Assert.That(obj.IsPersisted, Is.False, "Test Object should not have Persisted Flag before adding...");
			Assert.That(obj.Dirty, Is.True, "Test Object should be Dirty before adding...");
			
			var inserted = Database.AddObject(obj);
			
			Assert.That(inserted, Is.True, "Test Object should be inserted successfully, something went wrong...");
			Assert.That(obj.IsPersisted, Is.True, "Test Object should have Persisted Flag after adding to database...");
			Assert.That(obj.Dirty, Is.False, "Test Object should not be Dirty after adding to database...");
			Assert.That(obj.ObjectId, Is.EqualTo(objId), "Test Object should have kept its ObjectId from Creation...");
			
			var retrieved = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.That(retrieved, Is.Not.Null, "Test Object previously added should be retrieved from database...");
			Assert.That(retrieved.ObjectId, Is.EqualTo(objId), "Test Object previously added and retrieved object should have same ObjectId...");
			Assert.That(retrieved.TestField, Is.EqualTo(obj.TestField), "Test Object previously added and retrieved object should have equals fields...");
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
				Assert.That(obj.IsPersisted, Is.False, "Test Objects should not have Persisted Flag before adding...");
				Assert.That(obj.Dirty, Is.True, "Test Objects should be Dirty before adding...");
			}
			
			var inserted = Database.AddObject(objs);
			Assert.That(inserted, Is.True, "Test Objects should be inserted successfully, something went wrong...");
			foreach (var obj in objs)
			{
				Assert.That(obj.IsPersisted, Is.True, "Test Objects should have Persisted Flag after adding to database...");
				Assert.That(obj.Dirty, Is.False, "Test Objects should not be Dirty after adding to database...");
			}
			Assert.That(objs.Select(obj => obj.ObjectId), Is.EqualTo(objsId), "Test Objects should have kept their ObjectId from Creation...");
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objsId);
			
			Assert.That(retrieved, Is.Not.Null, "Test Objects retrieved Collection should not be null...");
			Assert.That(retrieved.Select(obj => obj.ObjectId), Is.EqualTo(objsId), "Test Objects previously added and retrieved objects should have same ObjectId...");
			Assert.That(retrieved.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Test Objects previously added and retrieved objects should have equals fields...");
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
				Assert.That(obj.IsPersisted, Is.False, "Different Test Objects should not have Persisted Flag before adding...");
				Assert.That(obj.Dirty, Is.True, "Different Test Objects should be Dirty before adding...");
			}
			
			var inserted = Database.AddObject(objs);
			
			Assert.That(inserted, Is.True, "Different Test Objects should be inserted successfully, something went wrong...");
			foreach (var obj in objs)
			{
				Assert.That(obj.IsPersisted, Is.True, "Test Objects should have Persisted Flag after adding to database...");
				Assert.That(obj.Dirty, Is.False, "Test Objects should not be Dirty after adding to database...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
				{
					Assert.That(ent.IsPersisted, Is.True, "TestTableRelationEntries Objects should have Persisted Flag after adding to database...");
					Assert.That(ent.Dirty, Is.False, "TestTableRelationEntries Objects should not be Dirty after adding to database...");
				}
			}
			
			Assert.That(objs.Where(obj => obj.GetType() == typeof(TestTable)).Select(obj => obj.ObjectId), Is.EquivalentTo(objsTestTableId), "TestTable Objects should have kept their ObjectId from Creation...");
			Assert.That(objs.Where(obj => obj.GetType() == typeof(TestTableRelations)).Select(obj => obj.ObjectId), Is.EquivalentTo(objsTestTableRelationsId), "TestTableRelations Objects should have kept their ObjectId from Creation...");
			Assert.That(objs.Where(obj => obj.GetType() == typeof(TestTableAutoInc)).Select(obj => obj.ObjectId), Is.Not.EquivalentTo(objsTestTableAutoIncId), "TestTableAutoInc Objects should NOT have kept their ObjectId from Creation...");
			
			foreach (var obj in objs.Where(obj => obj.GetType() == typeof(TestTableRelations)).Cast<TestTableRelations>())
				Assert.That(obj.Entries.Select(ent => ent.ObjectId), Is.EquivalentTo(objsTestTableRelationsEntriesId[obj.ObjectId]),
				                               "TestTableRelationsEntries Objects should have kept their ObjectId from Creation...");
			
			var retrievedTestTable = Database.FindObjectsByKey<TestTable>(objsTestTableId);
			Assert.That(retrievedTestTable, Is.Not.Null, "TestTable Objects retrieved Collection should not be null...");
			Assert.That(retrievedTestTable.Select(obj => obj.ObjectId), Is.EqualTo(objsTestTableId), "TestTable Objects previously added and retrieved objects should have same ObjectId...");
			Assert.That(retrievedTestTable.Select(obj => obj.TestField), Is.EqualTo(objsTestTable.Select(obj => obj.TestField)), "TestTable Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableAutoInc = Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(key => (object)key.PrimaryKey));
			Assert.That(retrievedTestTableAutoInc, Is.Not.Null, "TestTableAutoInc Objects retrieved Collection should not be null...");
			Assert.That(retrievedTestTableAutoInc.Select(obj => obj.ObjectId), Is.EqualTo(objsTestTableAutoInc.Select(key => key.PrimaryKey.ToString())), "TestTableAutoInc Objects previously added and retrieved objects should have same ObjectId...");
			Assert.That(retrievedTestTableAutoInc.Select(obj => obj.TestField), Is.EqualTo(objsTestTableAutoInc.Select(obj => obj.TestField)), "TestTableAutoInc Objects previously added and retrieved objects should have equals fields...");
			
			var retrievedTestTableRelations = Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelationsId);
			Assert.That(retrievedTestTableRelations, Is.Not.Null, "TestTableRelations Objects retrieved Collection should not be null...");
			Assert.That(retrievedTestTableRelations.Select(obj => obj.ObjectId), Is.EqualTo(objsTestTableRelationsId), "TestTableRelations Objects previously added and retrieved objects should have same ObjectId...");
			Assert.That(retrievedTestTableRelations.Select(obj => obj.TestField), Is.EqualTo(objsTestTableRelations.Select(obj => obj.TestField)), "TestTableRelations Objects previously added and retrieved objects should have equals fields...");
			
			foreach (var obj in retrievedTestTableRelations)
				Assert.That(obj.Entries.Select(ent => ent.ObjectId), Is.EquivalentTo(objsTestTableRelationsEntriesId[obj.ObjectId]),
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
			Assert.That(added, Is.False, "Adding a Single non registered Object should not return success...");
			Assert.That(dbo.IsPersisted, Is.False, "Adding a Single non registered Object should not have Persisted Flag...");
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
			Assert.That(added, Is.False, "Adding Multiple non registered Object should not return success...");
			Assert.That(dbo[1].IsPersisted, Is.False, "Adding a Multiple non registered Object should not have Persisted Flag on unregistered Object...");
			Assert.That(dbo[0].IsPersisted, Is.True, "Adding a Multiple non registered Object should have Persisted Flag on registered Object...");
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
			
			Assert.That(Database.SelectAllObjects<TestTable>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			Assert.That(Database.SelectAllObjects<TestTableRelations>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			Assert.That(Database.SelectAllObjects<TestTableRelationsEntries>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			
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
			
			Assert.That(Database.SelectAllObjects<TestTable>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			Assert.That(Database.SelectAllObjects<TestTableRelations>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			Assert.That(Database.SelectAllObjects<TestTableRelationsEntries>(), Is.Empty, "Database shouldn't have any record For TestTable.");
			
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
			Assert.That(obj.Dirty, Is.True, "Changing TestTable Object should set Dirty Flag...");
			
			var saved = Database.SaveObject(obj);
			
			Assert.That(saved, Is.True, "Changed Object Should be Saved Successfully in database...");
			Assert.That(obj.Dirty, Is.False, "Changed Object should not have Dirty flag after Saving...");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Changed Object Should be retrieved from database...");
			Assert.That(retrieve.TestField, Is.EqualTo(obj.TestField), "Previously Changed Object and newly retrieved Object should have the same field value...");
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
				Assert.That(obj.Dirty, Is.True, "Changing TestTable Objects should set Dirty Flag...");
			}
			
			var saved = Database.SaveObject(objs);
			
			Assert.That(saved, Is.True, "Changed Objects Should be Saved Successfully in database...");
			foreach (var obj in objs)
				Assert.That(obj.Dirty, Is.False, "Changed Objects should not have Dirty flag after Saving...");
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			
			Assert.That(retrieved, Is.Not.Null, "Changed Objects Collection Should be retrieved from database...");
			Assert.That(retrieved.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "Previously Changed Objects and newly retrieved Objects should have the same field value...");
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
				Assert.That(obj.Dirty, Is.True, "Changing TestTable Objects should set Dirty Flag...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
					ent.Dirty = true;
			}
			
			var saved = Database.SaveObject(objs);
			
			Assert.That(saved, Is.True, "Changed Objects Should be Saved Successfully in database...");
			foreach (var obj in objs)
				Assert.That(obj.Dirty, Is.False, "Changed Objects should not have Dirty flag after Saving...");
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
					Assert.That(ent.Dirty, Is.False, "Changed Objects Relations should not have Dirty flag after Saving...");
			}
			
			var retrieved = Database.FindObjectsByKey<TestTable>(objsTestTable.Select(obj => obj.ObjectId))
				.Concat(Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(obj => obj.ObjectId)))
				.Concat(Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelations.Select(obj => obj.ObjectId)));
			
			Assert.That(retrieved, Is.Not.Null, "Changed Objects Collection Should be retrieved from database...");
			Assert.That(retrieved.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "Previously Changed Objects and newly retrieved Objects should have the same field value...");
			
			foreach (var obj in retrieved.Where(o => o.GetType() == typeof(TestTableRelations)).OfType<TestTableRelations>())
			{
				Assert.That(objsTestTableRelations.First(o => o.ObjectId == obj.ObjectId).Entries.Select(o => o.ObjectId),
				                               Is.EquivalentTo(obj.Entries.Select(o => o.ObjectId)), "Previously Changed Objects and newly retrieved Objects should have the same Relations Collections...");
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
			Assert.That(saved, Is.False, "Trying to save a non registered object should not return success...");
			Assert.That(obj.Dirty, Is.True, "Failing to save a non registered object should not remove Dirty Flag...");
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
			
			Assert.That(saved, Is.False, "Saving Multiple non registered Object should not return success...");
			Assert.That(dbo[0].Dirty, Is.False, "Saving a Collection with non registered Object should not have Dirty Flag on registered Object...");
			Assert.That(dbo[1].Dirty, Is.True, "Saving a Collection with non registered Object should have Dirty Flag on non-registered Object...");
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
			
			Assert.That(obj.IsPersisted, Is.True, "Added Object should have Persisted Flag...");
			
			var deleted = Database.DeleteObject(obj);
			
			Assert.That(deleted, Is.True, "Added Object should be Successfully Deleted from database...");
			Assert.That(obj.IsPersisted, Is.False, "Deleted Object should not have Persisted Flag");
			Assert.That(obj.IsDeleted, Is.True, "Deleted Object should have Deleted Flag");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			Assert.That(retrieve, Is.Null, "Retrieving Deleted Object should return null...");
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
				Assert.That(obj.IsPersisted, Is.True, "Added Objects should have Persisted Flag...");
			
			var deleted = Database.DeleteObject(objs);
			
			Assert.That(deleted, Is.True, "Added Objects should be Successfully Deleted from database...");
			foreach (var obj in objs)
			{
				Assert.That(obj.IsPersisted, Is.False, "Deleted Objects should not have Persisted Flag");
				Assert.That(obj.IsDeleted, Is.True, "Deleted Objects should have Deleted Flag");
			}
			
			var retrieve = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			Assert.That(retrieve, Is.Not.Null, "Retrieving Deleted Objects Collection should not return null...");
			Assert.That(retrieve.Where(obj => obj != null), Is.Empty, "Retrieved Deleted Objects Collection should be empty...");
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
			
			Assert.That(inserted, Is.True, "Object Should be inserted to test Delete Cases...");
			
			// Delete Objects
			var deleted = Database.DeleteObject(objs);
			
			Assert.That(deleted, Is.True, "Object should be deleted successfully...");
			
			foreach (var obj in objs)
			{
					Assert.That(obj.IsDeleted, Is.True, "Deleted Objects should have Deleted Flag...");
					Assert.That(obj.IsPersisted, Is.False, "Deleted Objects should not have Persisted Flag...");
			}
			
			foreach (var obj in objsTestTableRelations)
			{
				foreach (var ent in obj.Entries)
				{
					Assert.That(ent.IsDeleted, Is.True, "Deleted Objects should have Deleted Flag...");
					Assert.That(ent.IsPersisted, Is.False, "Deleted Objects should not have Persisted Flag...");
				}
			}

			var retrieved = Database.FindObjectsByKey<TestTable>(objsTestTable.Select(obj => obj.ObjectId))
				.Concat(Database.FindObjectsByKey<TestTableAutoInc>(objsTestTableAutoInc.Select(obj => obj.ObjectId)))
				.Concat(Database.FindObjectsByKey<TestTableRelations>(objsTestTableRelations.Select(obj => obj.ObjectId)));
			
			Assert.That(retrieved, Is.Not.Null, "Deleted Objects Collection Should be retrieved Empty not null from database...");
			Assert.That(retrieved.Where(obj => obj != null), Is.Empty, "Deleted Objects Collection Should be retrieved Empty from database...");
			
			var relRetrieved = Database.FindObjectsByKey<TestTableRelationsEntries>(objsTestTableRelations.SelectMany(obj => obj.Entries).Select(ent => ent.ObjectId));

			Assert.That(relRetrieved, Is.Not.Null, "Deleted Objects Collection Should be retrieved Empty not null from database...");
			Assert.That(relRetrieved.Where(obj => obj != null), Is.Empty, "Deleted Objects Collection Should be retrieved Empty from database...");
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
			Assert.That(deleted, Is.False, "Trying to Delete a non registered object should not return success...");
			Assert.That(obj.IsDeleted, Is.False, "Failing to Delete a non registered object should not set Deleted Flag...");
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
			
			Assert.That(deleted, Is.False, "Deleting Multiple non registered Object should not return success...");
			Assert.That(dbo[0].IsDeleted, Is.True, "Deleting a Collection with non registered Object should set Deleted Flag on registered Object...");
			Assert.That(dbo[1].IsDeleted, Is.False, "Deleting a Collection with non registered Object should not set Deleted Flag non-registered Object...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTable>(obj.ObjectId);
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object...");
			Assert.That(retrieve.ObjectId, Is.EqualTo(obj.ObjectId), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieve.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCase = Database.FindObjectByKey<TestTable>(obj.ObjectId.ToUpper());
			
			Assert.That(retrieveCase, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object using Case Mismatch...");
			Assert.That(retrieveCase.ObjectId, Is.EqualTo(obj.ObjectId), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieveCase.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId));
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieve.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects...");
			Assert.That(retrieve.Select(obj => obj.ObjectId), Is.EqualTo(objs.Select(obj => obj.ObjectId)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCase = Database.FindObjectsByKey<TestTable>(objs.Select(obj => obj.ObjectId.ToUpper()));
			
			Assert.That(retrieveCase, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieveCase.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects using Case Mismatch...");
			Assert.That(retrieveCase.Select(obj => obj.ObjectId), Is.EqualTo(objs.Select(obj => obj.ObjectId)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieveCase.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTablePrimaryKey>(obj.PrimaryKey);
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object...");
			Assert.That(retrieve.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieve.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCase = Database.FindObjectByKey<TestTablePrimaryKey>(obj.PrimaryKey.ToUpper());
			
			Assert.That(retrieveCase, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object using Case Mismatch...");
			Assert.That(retrieveCase.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieveCase.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTablePrimaryKey>(objs.Select(obj => obj.PrimaryKey));
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieve.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects...");
			Assert.That(retrieve.Select(obj => obj.PrimaryKey), Is.EqualTo(objs.Select(obj => obj.PrimaryKey)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCase = Database.FindObjectsByKey<TestTablePrimaryKey>(objs.Select(obj => obj.PrimaryKey.ToUpper()));
			
			Assert.That(retrieveCase, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieveCase.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects using Case Mismatch...");
			Assert.That(retrieveCase.Select(obj => obj.PrimaryKey), Is.EqualTo(objs.Select(obj => obj.PrimaryKey)), "Find Object By Key Should return similar Object to created ones...");
			Assert.That(retrieveCase.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Object to created ones...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add object in database...");
			
			var retrieve = Database.FindObjectByKey<TestTableAutoInc>(obj.PrimaryKey);
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object...");
			Assert.That(retrieve.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieve.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
			
			var retrieveCast = Database.FindObjectByKey<TestTableAutoInc>((long)obj.PrimaryKey);
			
			Assert.That(retrieveCast, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object using Numeric Cast...");
			Assert.That(retrieveCast.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieveCast.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
			
			var retrieveString = Database.FindObjectByKey<TestTableAutoInc>(obj.PrimaryKey.ToString());
			
			Assert.That(retrieveString, Is.Not.Null, "Find Object By Key Could not retrieve previously added Object using String Cast...");
			Assert.That(retrieveString.PrimaryKey, Is.EqualTo(obj.PrimaryKey), "Find Object By Key Should return similar Object to created one...");
			Assert.That(retrieveString.TestField, Is.EqualTo(obj.TestField), "Find Object By Key Should return similar Object to created one...");
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
			
			Assert.That(inserted, Is.True, "Find Object By Key Test Could not add objects in database...");
			
			var retrieve = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => obj.PrimaryKey).Cast<object>());
			
			Assert.That(retrieve, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieve.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects...");
			Assert.That(retrieve.Select(obj => obj.PrimaryKey), Is.EqualTo(objs.Select(obj => obj.PrimaryKey)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveCast = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => (long)obj.PrimaryKey).Cast<object>());
			
			Assert.That(retrieveCast, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieveCast.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects using Numeric Cast...");
			Assert.That(retrieveCast.Select(obj => obj.PrimaryKey), Is.EqualTo(objs.Select(obj => obj.PrimaryKey)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieveCast.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
			
			var retrieveString = Database.FindObjectsByKey<TestTableAutoInc>(objs.Select(obj => obj.PrimaryKey.ToString()));
			
			Assert.That(retrieveString, Is.Not.Null, "Find Object By Key Could should not return a null collection...");
			Assert.That(retrieveString.Where(obj => obj != null), Is.Not.Empty, "Find Object By Key Could not retrieve previously added Objects using String Cast...");
			Assert.That(retrieveString.Select(obj => obj.PrimaryKey), Is.EqualTo(objs.Select(obj => obj.PrimaryKey)), "Find Object By Key Should return similar Objects to created ones...");
			Assert.That(retrieveString.Select(obj => obj.TestField), Is.EqualTo(objs.Select(obj => obj.TestField)), "Find Object By Key Should return similar Objects to created ones...");
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
			Assert.That(dbo, Is.Null, "Searching an Object By Key with a Null Key should return a null object...");
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
			Assert.That(result, Is.Not.Empty, "Searching with multiple keys including null shoud not return an empty collection...");
			Assert.That(result[0], Is.Null, "Searching with multiple keys including null should return null data object for null key...");
			Assert.That(result[1], Is.Not.Null, "Searching with multiple keys including null should return a valid data object for non null key...");
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
			
			Assert.That(allobjects, Is.Not.Empty, "Select Objects Test Need some Data to be Accurate...");
			
			var simpleWhere = Database.SelectObjects<TestTable>(DB.Column(nameof(TestTable.TestField)).IsEqualTo(objInitial.TestField));
			
			Assert.That(simpleWhere.Select(obj => obj.ObjectId), Has.Member(objInitial.ObjectId), "Select Objects with Simple Where clause should retrieve Object similar to Created one...");

			var complexWhere = Database.SelectObjects<TestTable>(DB.Column(nameof(TestTable.TestField)).IsEqualTo(objInitial.TestField).And(DB.Column("Test_Table_ID").IsEqualTo(objInitial.ObjectId)));
			
			Assert.That(complexWhere.Select(obj => obj.ObjectId.ToLower()), Has.Member(objInitial.ObjectId.ToLower()), "Select Objects with Complex Where clause should retrieve Object similar to Created one...");

			Assert.That(Database.DeleteObject(Database.SelectObjects<TestTable>(DB.Column(nameof(TestTable.TestField)).IsNull())), Is.True);
			var objNull = new TestTable { TestField = null };
			var nullAdd = Database.AddObject(objNull);
			
			Assert.That(nullAdd, Is.True, "Select Objects null parameter Test Need some null object to be Accurate...");

			var nullParam = Database.SelectObjects<TestTable>(DB.Column(nameof(TestTable.TestField)).IsEqualTo(null));
			
			Assert.That(nullParam, Is.Empty, "Select Objects with Null Parameter Query should not return any record...");

			var resultsWithTestfieldNull = Database.SelectObjects<TestTable>(DB.Column(nameof(TestTable.TestField)).IsNull());
			var allObjectsAfter = Database.SelectAllObjects<TestTable>();

			Assert.That(resultsWithTestfieldNull.Count, Is.EqualTo(1));
		}

		[Test]
		public void SelectObjects_KeywordAsColumnName_SameAsAddedObjects()
		{
			Database.RegisterDataObject(typeof(SqlKeywordTable));
			var firstEntry = new SqlKeywordTable { Type = "SomeText" };
			Database.AddObject(firstEntry);

			var result = Database.SelectObjects<SqlKeywordTable>(DB.Column(nameof(SqlKeywordTable.Type)).IsEqualTo(firstEntry.Type));

			Assert.That(result.Select(obj => obj.ObjectId), Has.Member(firstEntry.ObjectId), "Select Objects with Simple Where clause should retrieve Object similar to Created one...");
		}

		/// <summary>
		/// Test IObjectDatabase.SelectObjects`TObject(string, IEnumerable`IEnumerable`KeyValuePair`string, object)
		/// </summary>
		[Test]
		public void TestSelectObjectsWithMultipleWhereClause()
		{
			Database.RegisterDataObject(typeof(TestTable));
			
			var delete = Database.DeleteObject(Database.SelectAllObjects<TestTable>());
			Assert.That(delete, Is.True, "TestTable Objects should be deleted successfully...");
			Assert.That(Database.SelectAllObjects<TestTable>(), Is.Empty, "This test needs an Empty Table to Run Successfully...");

			var objs = new []{ "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" }
			.Select(grp => Enumerable.Range(0, 100).Select(i => new TestTable { TestField = grp } )).ToDictionary(kv => kv.First().TestField, kv => kv.ToArray());
			
			var added = Database.AddObject(objs.SelectMany(obj => obj.Value));
			
			Assert.That(added, Is.True, "TestTable Objects should be added successfully...");

			var parameters = new[] { "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" };
			var retrieve = Database.MultipleSelectObjects<TestTable>(parameters.Select(parameter => DB.Column(nameof(TestTable.TestField)).IsEqualTo(parameter)));

			var objectByGroup = new []{ "Test Select Group1", "Test Select Group2", "Test Select Group3", "Test Select Group4" }
			.Select((grp, index) => new { Grp = grp, Objects = retrieve.ElementAt(index) });
			
			Assert.That(retrieve, Is.Not.Null, "Retrieve Sets from Select Objects should not return null value...");
			Assert.That(retrieve, Is.Not.Empty, "Retrieve Set from Select Objects should not be Empty...");
			
			foreach (var sets in objectByGroup)
			{
				Assert.That(sets.Objects, Is.Not.Empty, "Retrieve SubSets from Select Objects should not be Empty...");
				Assert.That(sets.Objects.All(obj => obj.TestField.Equals(sets.Grp, StringComparison.OrdinalIgnoreCase)), Is.True,
				              "Retrieve SubSets from Select Objects should have the Where Clause Matching their Field Value...");
				Assert.That(sets.Objects.Select(obj => obj.ObjectId), Is.EquivalentTo(objs[sets.Grp].Select(obj => obj.ObjectId)),
				                              "Retrieve SubSets from Select Objects should return the same ObjectId Sets as Created...");
				
			}

			var orderedObjs = objs.SelectMany(obj => obj.Value).ToArray();
			var retrieveMany = Database.MultipleSelectObjects<TestTable>(orderedObjs.Select(obj => DB.Column(nameof(TestTable.TestField)).IsEqualTo(obj.TestField).And(DB.Column("Test_Table_ID").IsEqualTo(obj.ObjectId))));
			
			Assert.That(retrieveMany, Is.Not.Null, "Retrieve Sets from Select Objects should not return null value...");
			Assert.That(retrieveMany, Is.Not.Empty, "Retrieve Set from Select Objects should not be Empty...");

			var resultsMany = retrieveMany.Select(obj => obj.Single());
			Assert.That(resultsMany.Select(obj => obj.ObjectId.ToLower()), Is.EqualTo(orderedObjs.Select(obj => obj.ObjectId.ToLower())),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set ObjectId...");
			Assert.That(resultsMany.Select(obj => obj.TestField.ToLower()), Is.EqualTo(orderedObjs.Select(obj => obj.TestField.ToLower())),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set Field Value...");

			var parameterManyWithMissing = new [] { ("No Known Value", "Probably Nothing"),("Absolutely None","Nothing for Sure")}
			.Concat(orderedObjs.Select(obj => (obj.TestField, obj.ObjectId)));
			var manyQueriesWithMissing = parameterManyWithMissing.Select(tuple => DB.Column(nameof(TestTable.TestField)).IsEqualTo(tuple.Item1).And(DB.Column("Test_Table_ID").IsEqualTo(tuple.Item2)));
			var retrieveManyWithMissing = Database.MultipleSelectObjects<TestTable>(manyQueriesWithMissing);
			
			Assert.That(retrieveManyWithMissing, Is.Not.Null, "Retrieve Sets from Select Objects should not return null value...");
			Assert.That(retrieveManyWithMissing, Is.Not.Empty, "Retrieve Set from Select Objects should not be Empty...");

			var resultsManyWithMissing = retrieveManyWithMissing.Select(obj => obj.SingleOrDefault());
			Assert.That(resultsManyWithMissing.Select(obj => obj != null ? obj.ObjectId.ToLower() : string.Empty), Is.EqualTo(new [] { "", "" }.Concat(orderedObjs.Select(obj => obj.ObjectId.ToLower()))),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set ObjectId...");
			Assert.That(resultsManyWithMissing.Select(obj => obj != null ? obj.TestField.ToLower() : string.Empty), Is.EqualTo(new [] { "", "" }.Concat(orderedObjs.Select(obj => obj.TestField.ToLower()))),
			                          "Retrieve Sets from Select Objects should be Equal to Parameter Set Field Value...");
			
			
		}

		/// <summary>
		/// Test IObjectDatabase.SelectObject(s)`TObject()
		/// With Non Registered Table
		/// </summary>
		[Test]
		public void TestSelectObjectsNonRegistered()
		{
			var assertFailMessage = "Trying to Query a Non Registered Table should throw a DatabaseException...";
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObject<TableNotRegistered>(DB.Column("a").IsEqualTo(null)), assertFailMessage);
			Assert.Throws(typeof(DatabaseException), () => Database.SelectObjects<TableNotRegistered>(DB.Column("a").IsEqualTo(null)), assertFailMessage);
			Assert.Throws(typeof(DatabaseException), () => Database.MultipleSelectObjects<TableNotRegistered>(new[] { DB.Column("a").IsEqualTo(null) }), assertFailMessage);
			Assert.Throws(typeof(DatabaseException), () => Database.SelectAllObjects<TableNotRegistered>(), assertFailMessage);
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
			Assert.That(delete, Is.True, "TestTable Objects should be deleted successfully...");
			Assert.That(Database.SelectAllObjects<TestTable>(), Is.Empty, "This test needs an Empty Table to Run Successfully...");
			
			var objInitial = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Select Objects Null Values Test #{0}", i) });
			Database.AddObject(objInitial);
			
			var allobjects = Database.SelectAllObjects<TestTable>();
			
			Assert.That(allobjects, Is.Not.Empty, "This Test Need some Data to be Accurate...");

			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObject<TestTable>((WhereClause)null), "");
			Assert.Throws(typeof(NullReferenceException), () => Database.SelectObjects<TestTable>((WhereClause)null), "");
			Assert.Throws(typeof(ArgumentNullException), () => Database.MultipleSelectObjects<TestTable>(null));
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
			
			Assert.That(objs, Is.Not.Null, "Retrieving from a Registered Table should not return null...");
			
			var delete = Database.DeleteObject(objs);
			
			Assert.That(delete, Is.True, "TestTable Objects should be deleted successfully...");
			
			objs = Database.SelectAllObjects<TestTable>();
			Assert.That(objs, Is.Not.Null, "TestTable Select All from an Empty table should not return null...");
			Assert.That(objs, Is.Empty, "TestTable Select All from an Empty table should return an Empty Collection...");
			
			objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Test Select All Object #{0}", i) }).ToList();
			
			var inserted = Database.AddObject(objs);
			
			Assert.That(inserted, Is.True, "TestTable Objects should be inserted successfully...");
			
			var retrieve = Database.SelectAllObjects<TestTable>();
			Assert.That(retrieve, Is.Not.Empty, "TestTable Objects Select All should return previously inserted objects...");
			Assert.That(retrieve.Select(obj => obj.ObjectId), Is.EquivalentTo(objs.Select(obj => obj.ObjectId)), "TestTable Select All should return Objects with Same ID as Inserted...");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "TestTable Select All shoud return Objects with same Value Field as Inserted...");			
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
			
			Assert.That(count, Is.EqualTo(0), "Test Table shouldn't Have any Object after deleting all records...");
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Count Object Test #{0}", i) });
			
			Database.AddObject(objs);
			
			var newCount = Database.GetObjectCount<TestTable>();
			
			Assert.That(newCount, Is.EqualTo(10), "Test Table should return same object count as added collection...");
			
			var whereCount = Database.GetObjectCount<TestTable>("1");
			
			Assert.That(whereCount, Is.EqualTo(10), "Test Table should return same object count as added collection...");
			
			var filterCount = Database.GetObjectCount<TestTable>("`TestField` LIKE '%1'");
			
			Assert.That(filterCount, Is.EqualTo(1), "Test Table should return same object count as filtered collection...");
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
			
			Assert.That(count, Is.EqualTo(0), "Test Table shouldn't Have any Object after deleting all records...");
			
			var objs = Enumerable.Range(0, 10).Select(i => new TestTable { TestField = string.Format("Count Object Test #{0}", i) });
			
			Database.AddObject(objs);
			
			var newCount = Database.GetObjectCount<TestTable>(null);
			
			Assert.That(newCount, Is.EqualTo(10), "Test Table should return same object count as added collection...");
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
			
			Assert.That(Database.Escape(test), Is.EqualTo("''"), "Sqlite String Escape Test Failure...");
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
