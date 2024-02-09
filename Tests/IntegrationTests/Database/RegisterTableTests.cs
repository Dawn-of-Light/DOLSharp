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
using System.Reflection;

using DOL.Database;
using DOL.Database.Connection;
using DOL.Database.Attributes;

using NUnit.Framework;

namespace DOL.Integration.Database
{
	[TestFixture]
	public class RegisterTableTests
	{
		public RegisterTableTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		protected virtual SQLObjectDatabase GetDatabaseV2 { get { return (SQLObjectDatabase)ObjectDatabase.GetObjectDatabase(ConnectionType.DATABASE_SQLITE, DatabaseSetUp.ConnectionString); } }
		
		/// <summary>
		/// Test to Register all Assemblies Tables
		/// </summary>
		[Test]
		public void TestAllAvailableTables()
		{
			foreach (Assembly assembly in new [] { typeof(GS.GameServer).Assembly, typeof(DataObject).Assembly })
			{
				// Walk through each type in the assembly
				foreach (Type type in assembly.GetTypes())
				{
					if (!type.IsClass || type.IsAbstract)
						continue;
					
					var attrib = type.GetCustomAttributes<DataTable>(false);
					if (attrib.Any())
					{
						Assert.DoesNotThrow( () => {
						                    	var dth = new DataTableHandler(type);
						                    	Database.CheckOrCreateTableImpl(dth);
						                    }, "Registering All Projects Tables should not throw Exceptions... (Failed on Type {0})", type.FullName);
						
						Database.RegisterDataObject(type);
						var selectall = typeof(IObjectDatabase).GetMethod("SelectAllObjects", Array.Empty<Type>() ).MakeGenericMethod(type);
						object objs = null;
						Assert.DoesNotThrow( () => { objs = selectall.Invoke(Database, Array.Empty<object>() ); }, "Registered tables should not Throw Exception on Select All... (Failed on Type {0})", type);
						Assert.That(objs, Is.Not.Null);
					}
				}
			}
		}
		
		/// <summary>
		/// Wrong Data Object shouldn't be registered
		/// </summary>
		[Test]
		public void TestWrongDataObject()
		{
			Assert.Throws(typeof(ArgumentException), () => {
			              	var dth = new DataTableHandler(typeof(AttributesUtils));
			              	Database.CheckOrCreateTableImpl(dth);
			              }, "Registering a wrong DataObject should throw Argument Exception");
		}
		
		/// <summary>
		/// Multi Index DataObject
		/// </summary>
		[Test]
		public void TestMultiIndexesDataObject()
		{
			var dth = new DataTableHandler(typeof(TestTableWithMultiIndexes));
			Assert.DoesNotThrow(() => Database.CheckOrCreateTableImpl(dth), "Registering Test Table with Overlapping Indexes should not Throw exceptions.");
		}
		
		/// <summary>
		/// Test Table Migration from no PK to PK Auto Inc
		/// </summary>
		[Test]
		public void TestTableMigrationFromNoPrimaryKeyToAutoInc()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableWithNoPrimaryV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			Database.RegisterDataObject(typeof(TestTableWithNoPrimaryV1));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableWithNoPrimaryV1>());
			
			Assert.That(Database.SelectAllObjects<TestTableWithNoPrimaryV1>(), Is.Empty, "Test Table TestTableWithNoPrimaryV1 should be empty to begin this tests.");
			
			var objs = new [] { "TestObj1", "TestObj2", "TestObj3" }.Select(ent => new TestTableWithNoPrimaryV1 { Value = ent }).ToArray();
			
			Database.AddObject(objs);
			
			Assert.That(Database.SelectAllObjects<TestTableWithNoPrimaryV1>().Select(obj => obj.Value), Is.EquivalentTo(objs.Select(obj => obj.Value)), "Test Table TestTableWithNoPrimaryV1 Entries should be available for this test to run.");
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableWithNoPrimaryV2));
			
			var newObjs = DatabaseV2.SelectAllObjects<TestTableWithNoPrimaryV2>().ToArray();
			
			Assert.That(newObjs.Select(obj => obj.Value), Is.EquivalentTo(objs.Select(obj => obj.Value)), "Test Table Migration to TestTableWithNoPrimaryV2 should retrieve similar values that created ones...");
			
			Assert.That(newObjs.All(obj => obj.PrimaryKey != 0), Is.True, "Test Table Migration to TestTableWithNoPrimaryV2 should have created and populated Primary Key Auto Increment.");
			
			// Trigger Another Migration
			DatabaseV2 = GetDatabaseV2;
			DatabaseV2.RegisterDataObject(typeof(TestTableWithNoPrimaryV3));
			
			var newerObjs = DatabaseV2.SelectAllObjects<TestTableWithNoPrimaryV3>().ToArray();
			
			Assert.That(newerObjs.Select(obj => obj.Value), Is.EquivalentTo(objs.Select(obj => obj.Value)), "Test Table Migration to TestTableWithNoPrimaryV3 should retrieve similar values that created ones...");
			
			Assert.That(newerObjs.All(obj => obj.PrimaryKey2 != 0), Is.True, "Test Table Migration to TestTableWithNoPrimaryV3 should have created and populated Primary Key Auto Increment.");

		}
		
		/// <summary>
		/// Test Table Migration with Different Types Change
		/// </summary>
		[Test]
		public void TestTableMigrationWithDifferentTypes()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableDifferentTypesV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			Database.RegisterDataObject(typeof(TestTableDifferentTypesV1));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableDifferentTypesV1>());
			
			Assert.That(Database.SelectAllObjects<TestTableDifferentTypesV1>(), Is.Empty, "Test Table TestTableDifferentTypesV1 should be empty to begin this tests.");
			
			var datenow = DateTime.UtcNow;
			var now = new DateTime(datenow.Year, datenow.Month, datenow.Day, datenow.Hour, datenow.Minute, datenow.Second);
			var objs = new [] { "TestObj1", "TestObj2", "TestObj3" }.Select((ent, i) => new TestTableDifferentTypesV1 { StringValue = ent, IntValue = i, DateValue = now }).ToArray();
			
			Database.AddObject(objs);
			
			Assert.That(Database.SelectAllObjects<TestTableDifferentTypesV1>().Select(obj => obj.StringValue), Is.EquivalentTo(objs.Select(obj => obj.StringValue)), "Test Table TestTableDifferentTypesV1 Entries should be available for this test to run.");
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableDifferentTypesV2));
			
			var newObjs = DatabaseV2.SelectAllObjects<TestTableDifferentTypesV2>().ToArray();
			
			Assert.That(newObjs.Select(obj => obj.StringValue), Is.EquivalentTo(objs.Select(obj => obj.StringValue)), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			Assert.That(newObjs.Select(obj => obj.IntValue), Is.EquivalentTo(objs.Select(obj => obj.IntValue)), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			Assert.That(newObjs.Select(obj => Convert.ToDateTime(obj.DateValue)), Is.EquivalentTo(objs.Select(obj => obj.DateValue)), "Test Table Migration to TestTableDifferentTypesV2 should retrieve similar values that created ones...");
			
			// Trigger another Migraiton
			DatabaseV2 = GetDatabaseV2;
			DatabaseV2.RegisterDataObject(typeof(TestTableDifferentTypesV1));
			
			var newerObjs = DatabaseV2.SelectAllObjects<TestTableDifferentTypesV1>().ToArray();
			
			Assert.That(newerObjs.Select(obj => obj.StringValue), Is.EquivalentTo(objs.Select(obj => obj.StringValue)), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
			Assert.That(newerObjs.Select(obj => obj.IntValue), Is.EquivalentTo(objs.Select(obj => obj.IntValue)), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
			Assert.That(newerObjs.Select(obj => obj.DateValue), Is.EquivalentTo(objs.Select(obj => obj.DateValue)), "Test Table Migration to TestTableDifferentTypesV1 should retrieve similar values that created ones...");
		}
		
		/// <summary>
		/// Test Precached Table With Cache Update
		/// </summary>
		[Test]
		public void TestTablePrecachedUpdateCache()
		{
			Database.RegisterDataObject(typeof(TestTablePrecachedPrimaryKey));
			Database.DeleteObject(Database.SelectAllObjects<TestTablePrecachedPrimaryKey>());
			
			Assert.That(Database.SelectAllObjects<TestTablePrecachedPrimaryKey>(), Is.Empty, "Test Precached Table with Update Cache need Empty table to begin tests.");
			
			// Get a new Database Object to Trigger Cache Invalidation
			var DatabaseV2 = GetDatabaseV2;
			DatabaseV2.RegisterDataObject(typeof(TestTablePrecachedPrimaryKey));
			
			
			// Objects
			var objs = Enumerable.Range(0, 10).Select(i => new TestTablePrecachedPrimaryKey { PrimaryKey = i.ToString(), TestField = string.Format("Test update cache #{0}", i), PrecachedValue = string.Format("Cache value for update {0}", i) } );
			
			var inserted = Database.AddObject(objs);
			
			Assert.That(inserted, Is.True, "Test Precached Table with Update Cache could not insert test objects.");
			
			var update = DatabaseV2.UpdateObjsInCache<TestTablePrecachedPrimaryKey>(objs.Select(o => o.PrimaryKey));
			
			Assert.That(update, Is.True, "Test Precached Table with Update Cache could not refresh newly inserted objects.");
			
			var retrieve = DatabaseV2.SelectAllObjects<TestTablePrecachedPrimaryKey>();
			
			Assert.That(retrieve.Select(obj => obj.PrimaryKey), Is.EquivalentTo(objs.Select(obj => obj.PrimaryKey)), "Test Precached Table with Update Cache should return similar objets than created ones.");
			Assert.That(retrieve.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "Test Precached Table with Update Cache should return similar objets than created ones.");
			Assert.That(retrieve.Select(obj => obj.PrecachedValue), Is.EquivalentTo(objs.Select(obj => obj.PrecachedValue)), "Test Precached Table with Update Cache should return similar objets than created ones.");
			
			// Modify
			foreach (var obj in retrieve)
			{
				obj.TestField += " changed !";
				obj.PrecachedValue += " modified !";
			}
			
			var saved = DatabaseV2.SaveObject(retrieve);
			
			Assert.That(saved, Is.True, "Test Precached Table with Update Cache could not modify objects in database.");
			
			var retrievecached = Database.FindObjectsByKey<TestTablePrecachedPrimaryKey>(objs.Select(obj => obj.PrimaryKey));
			
			Assert.That(retrievecached.Select(obj => obj.PrimaryKey), Is.EquivalentTo(objs.Select(obj => obj.PrimaryKey)), "Test Precached Table with Update Cache should return similar cached objets than created ones.");
			Assert.That(retrievecached.Select(obj => obj.TestField), Is.EquivalentTo(objs.Select(obj => obj.TestField)), "Test Precached Table with Update Cache should return similar cached objets than created ones.");
			Assert.That(retrievecached.Select(obj => obj.PrecachedValue), Is.EquivalentTo(objs.Select(obj => obj.PrecachedValue)), "Test Precached Table with Update Cache should return similar cached objets than created ones.");
			
			// update
			var updated = Database.UpdateObjsInCache<TestTablePrecachedPrimaryKey>(objs.Select(obj => obj.PrimaryKey));
			
			Assert.That(updated, Is.True, "Test Precached Table with Update Cache could not update objects cache from database.");
			
			var retrieveupdated = Database.FindObjectsByKey<TestTablePrecachedPrimaryKey>(objs.Select(obj => obj.PrimaryKey));
			
			Assert.That(retrieveupdated.Select(obj => obj.PrimaryKey), Is.EquivalentTo(retrieve.Select(obj => obj.PrimaryKey)), "Test Precached Table with Update Cache should return similar updated objets than modified ones.");
			Assert.That(retrieveupdated.Select(obj => obj.TestField), Is.EquivalentTo(retrieve.Select(obj => obj.TestField)), "Test Precached Table with Update Cache should return similar updated objets than modified ones.");
			Assert.That(retrieveupdated.Select(obj => obj.PrecachedValue), Is.EquivalentTo(retrieve.Select(obj => obj.PrecachedValue)), "Test Precached Table with Update Cache should return similar updated objets than modified ones.");
		}

		/// <summary>
		/// Test Table Migration from no PK to PK Auto Inc
		/// </summary>
		[Test]
		public void TestTableMigrationChangingPrimaryKey()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableWithPrimaryChangingV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			Database.RegisterDataObject(typeof(TestTableWithPrimaryChangingV1));
			
			Database.DeleteObject(Database.SelectAllObjects<TestTableWithPrimaryChangingV1>());
			
			Assert.That(Database.SelectAllObjects<TestTableWithPrimaryChangingV1>(), Is.Empty, "Test Table TestTableWithPrimaryChangingV1 should be empty to begin this tests.");
			
			var objs = new [] { "TestObj1", "TestObj2", "TestObj3" }.Select((ent, i) => new TestTableWithPrimaryChangingV1 { PrimaryKey = i, Value = ent }).ToArray();
			
			Database.AddObject(objs);
			
			Assert.That(Database.SelectAllObjects<TestTableWithPrimaryChangingV1>().Select(obj => obj.Value), Is.EquivalentTo(objs.Select(obj => obj.Value)), "Test Table TestTableWithPrimaryChangingV1 Entries should be available for this test to run.");
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableWithPrimaryChangingV2));
			
			var newObjs = DatabaseV2.SelectAllObjects<TestTableWithPrimaryChangingV2>().ToArray();
			
			Assert.That(newObjs.Select(obj => obj.Value), Is.EquivalentTo(objs.Select(obj => obj.Value)), "Test Table Migration to TestTableWithPrimaryChangingV2 should retrieve similar values that created ones...");
			Assert.That(newObjs.Select(obj => obj.PrimaryKey), Is.EquivalentTo(objs.Select(obj => obj.PrimaryKey)), "Test Table Migration to TestTableWithPrimaryChangingV2 should retrieve similar values that created ones...");
		}
		
        [Test]
        public void TestTableMigrationToNonNullValue()
        {
            // Destroy previous table
            Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableMigrationNullToNonNull))));
            
            // Get a new Database Object to Trigger Migration
            var DatabaseV2 = GetDatabaseV2;
            
            Database.RegisterDataObject(typeof(TestTableMigrationNullToNonNull));
            
            Database.DeleteObject(Database.SelectAllObjects<TestTableMigrationNullToNonNull>());
            
            Assert.That(Database.SelectAllObjects<TestTableMigrationNullToNonNull>(), Is.Empty, "Test Table TestTableMigrationNullToNonNull should be empty to begin this tests.");
            
            var objs = new [] { "TestObj1", null, "TestObj3" }.Select((ent, i) => new TestTableMigrationNullToNonNull { StringValue = ent }).ToArray();
            
            Database.AddObject(objs);
            
            Assert.That(Database.SelectAllObjects<TestTableMigrationNullToNonNull>().Select(obj => obj.StringValue), Is.EquivalentTo(objs.Select(obj => obj.StringValue)), "Test Table TestTableMigrationNullToNonNull Entries should be available for this test to run.");
            
            // Trigger False Migration
            DatabaseV2.RegisterDataObject(typeof(TestTableMigrationNullFromNull));
            
            var newObjs = DatabaseV2.SelectAllObjects<TestTableMigrationNullFromNull>().ToArray();
            
            Assert.That(newObjs.Select(obj => obj.StringValue), Is.EquivalentTo(objs.Select(obj => obj.StringValue ?? string.Empty)), "Test Table Migration to TestTableMigrationNullFromNull should retrieve similar values that created ones...");
            Assert.That(newObjs.Select(obj => obj.IntValue), Is.EqualTo(Enumerable.Repeat(0, 3)), "Test Table Migration to TestTableMigrationNullFromNull should retrieve all default int value to 0...");
        }
	}
}
