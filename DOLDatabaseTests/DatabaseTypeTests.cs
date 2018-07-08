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
    /// Description of DatabaseTypeTests.
    /// </summary>
    [TestFixture]
    public class DatabaseTypeTests
    {
        public DatabaseTypeTests()
        {
            Database = DatabaseSetUp.Database;
        }

        protected SQLObjectDatabase Database { get; set; }

        [TestFixtureSetUp]
        public void SetUp()
        {
            Database.RegisterDataObject(typeof(ComplexTypeTestTable));
            foreach (var obj in Database.SelectAllObjects<ComplexTypeTestTable>())
            {
                Database.DeleteObject(obj);
            }

            Database.RegisterDataObject(typeof(ComplexTypeTestTableWithNull));
            foreach (var obj in Database.SelectAllObjects<ComplexTypeTestTableWithNull>())
            {
                Database.DeleteObject(obj);
            }
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Boolean (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Boolean (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Bool, objRetrieved.Bool, "DatabaseTypeTests: in Boolean (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Bool = true;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Boolean (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Boolean (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
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
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Boolean (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Boolean (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Bool, objRetrieved.Bool, "DatabaseTypeTests: in Boolean (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Bool = true;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Boolean (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Boolean (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Boolean (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Bool, objReRetrieved.Bool, "DatabaseTypeTests: in Boolean (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Char (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Char (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objRetrieved.Char, "DatabaseTypeTests: in Char (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Char = char.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Char (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Char (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objReRetrieved.Char, "DatabaseTypeTests: in Char (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Char = default(char);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Char (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Char (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objReReRetrieved.Char, "DatabaseTypeTests: in Char (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestCharNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Char = char.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Char (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Char (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objRetrieved.Char, "DatabaseTypeTests: in Char (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Char = char.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Char (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Char (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objReRetrieved.Char, "DatabaseTypeTests: in Char (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Char = default(char);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Char (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Char (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Char (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Char, objReReRetrieved.Char, "DatabaseTypeTests: in Char (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Byte (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Byte (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objRetrieved.Byte, "DatabaseTypeTests: in Byte (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Byte = byte.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Byte (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Byte (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objReRetrieved.Byte, "DatabaseTypeTests: in Byte (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Byte = default(byte);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Byte (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Byte (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objReReRetrieved.Byte, "DatabaseTypeTests: in Byte (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestByteNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Byte = byte.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Byte (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Byte (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objRetrieved.Byte, "DatabaseTypeTests: in Byte (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Byte = byte.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Byte (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Byte (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objReRetrieved.Byte, "DatabaseTypeTests: in Byte (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Byte = default(byte);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Byte (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Byte (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Byte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objReReRetrieved.Byte, "DatabaseTypeTests: in Byte (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Sbyte (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Sbyte (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Sbyte, objRetrieved.Sbyte, "DatabaseTypeTests: in Sbyte (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Sbyte = sbyte.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Sbyte (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Sbyte (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Sbyte, objReRetrieved.Sbyte, "DatabaseTypeTests: in Sbyte (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Sbyte = default(sbyte);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Sbyte (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Sbyte (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Sbyte, objReReRetrieved.Sbyte, "DatabaseTypeTests: in Sbyte (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestSbyteNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Sbyte = sbyte.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Sbyte (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Sbyte (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Sbyte, objRetrieved.Sbyte, "DatabaseTypeTests: in Sbyte (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Sbyte = sbyte.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Sbyte (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Sbyte (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Sbyte, objReRetrieved.Sbyte, "DatabaseTypeTests: in Sbyte (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Sbyte = default(sbyte);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Sbyte (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Sbyte (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Sbyte (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Byte, objReReRetrieved.Byte, "DatabaseTypeTests: in Sbyte (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in UShort (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in UShort (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objRetrieved.UShort, "DatabaseTypeTests: in UShort (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.UShort = ushort.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in UShort (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in UShort (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objReRetrieved.UShort, "DatabaseTypeTests: in UShort (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.UShort = default(ushort);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in UShort (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in UShort (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objReReRetrieved.UShort, "DatabaseTypeTests: in UShort (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestUShortNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.UShort = ushort.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in UShort (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in UShort (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objRetrieved.UShort, "DatabaseTypeTests: in UShort (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.UShort = ushort.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in UShort (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in UShort (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objReRetrieved.UShort, "DatabaseTypeTests: in UShort (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.UShort = default(ushort);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in UShort (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in UShort (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UShort (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UShort, objReReRetrieved.UShort, "DatabaseTypeTests: in UShort (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Short (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Short (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objRetrieved.Short, "DatabaseTypeTests: in Short (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Short = short.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Short (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Short (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objReRetrieved.Short, "DatabaseTypeTests: in Short (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Short = default(short);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Short (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Short (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objReReRetrieved.Short, "DatabaseTypeTests: in Short (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestShortNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Short = short.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Short (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Short (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objRetrieved.Short, "DatabaseTypeTests: in Short (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Short = short.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Short (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Short (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objReRetrieved.Short, "DatabaseTypeTests: in Short (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Short = default(short);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Short (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Short (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Short (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Short, objReReRetrieved.Short, "DatabaseTypeTests: in Short (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in UInt (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in UInt (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objRetrieved.UInt, "DatabaseTypeTests: in UInt (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.UInt = uint.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in UInt (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in UInt (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objReRetrieved.UInt, "DatabaseTypeTests: in UInt (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.UInt = default(uint);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in UInt (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in UInt (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objReReRetrieved.UInt, "DatabaseTypeTests: in UInt (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestUIntNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.UInt = uint.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in UInt (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in UInt (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objRetrieved.UInt, "DatabaseTypeTests: in UInt (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.UInt = uint.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in UInt (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in UInt (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objReRetrieved.UInt, "DatabaseTypeTests: in UInt (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.UInt = default(uint);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in UInt (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in UInt (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in UInt (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.UInt, objReReRetrieved.UInt, "DatabaseTypeTests: in UInt (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Int (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Int (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objRetrieved.Int, "DatabaseTypeTests: in Int (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Int = int.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Int (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Int (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objReRetrieved.Int, "DatabaseTypeTests: in Int (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Int = default(int);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Int (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Int (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objReReRetrieved.Int, "DatabaseTypeTests: in Int (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestIntNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Int = int.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Int (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Int (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objRetrieved.Int, "DatabaseTypeTests: in Int (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Int = int.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Int (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Int (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objReRetrieved.Int, "DatabaseTypeTests: in Int (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Int = default(int);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Int (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Int (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Int (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Int, objReReRetrieved.Int, "DatabaseTypeTests: in Int (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in ULong (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in ULong (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objRetrieved.ULong, "DatabaseTypeTests: in ULong (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.ULong = ulong.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in ULong (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in ULong (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objReRetrieved.ULong, "DatabaseTypeTests: in ULong (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.ULong = default(ulong);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in ULong (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in ULong (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objReReRetrieved.ULong, "DatabaseTypeTests: in ULong (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestULongNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.ULong = ulong.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in ULong (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in ULong (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objRetrieved.ULong, "DatabaseTypeTests: in ULong (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.ULong = ulong.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in ULong (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in ULong (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objReRetrieved.ULong, "DatabaseTypeTests: in ULong (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.ULong = default(ulong);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in ULong (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in ULong (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in ULong (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.ULong, objReReRetrieved.ULong, "DatabaseTypeTests: in ULong (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Long (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Long (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objRetrieved.Long, "DatabaseTypeTests: in Long (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Long = long.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Long (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Long (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objReRetrieved.Long, "DatabaseTypeTests: in Long (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Long = default(long);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Long (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Long (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objReReRetrieved.Long, "DatabaseTypeTests: in Long (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestLongNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Long = long.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Long (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Long (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objRetrieved.Long, "DatabaseTypeTests: in Long (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Long = long.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Long (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Long (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objReRetrieved.Long, "DatabaseTypeTests: in Long (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Long = default(long);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Long (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Long (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Long (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Long, objReReRetrieved.Long, "DatabaseTypeTests: in Long (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Float (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Float (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objRetrieved.Float, "DatabaseTypeTests: in Float (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Float = float.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Float (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Float (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objReRetrieved.Float, "DatabaseTypeTests: in Float (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Float = (float)0.1;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Float (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Float (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objReReRetrieved.Float, "DatabaseTypeTests: in Float (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestFloatNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Float = float.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Float (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Float (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objRetrieved.Float, "DatabaseTypeTests: in Float (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Float = float.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Float (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Float (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objReRetrieved.Float, "DatabaseTypeTests: in Float (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Float = (float)0.1;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Float (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Float (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Float (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Float, objReReRetrieved.Float, "DatabaseTypeTests: in Float (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Double (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Double (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objRetrieved.Double, "DatabaseTypeTests: in Double (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Double = double.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Double (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Double (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objReRetrieved.Double, "DatabaseTypeTests: in Double (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Double = (double)0.1;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Double (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Double (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objReReRetrieved.Double, "DatabaseTypeTests: in Double (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestDoubleNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Double = double.MinValue;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Double (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Double (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objRetrieved.Double, "DatabaseTypeTests: in Double (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Double = double.MaxValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Double (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Double (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objReRetrieved.Double, "DatabaseTypeTests: in Double (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Double = (double)0.1;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Double (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Double (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Double (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Double, objReReRetrieved.Double, "DatabaseTypeTests: in Double (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in DateTime (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in DateTime (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objRetrieved.DateTime, "DatabaseTypeTests: in DateTime (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.DateTime = DateTime.MinValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in DateTime (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in DateTime (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objReRetrieved.DateTime, "DatabaseTypeTests: in DateTime (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.DateTime = default(DateTime);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in DateTime (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in DateTime (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objReReRetrieved.DateTime, "DatabaseTypeTests: in DateTime (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestDateTimeNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.DateTime = new DateTime(DateTime.MaxValue.Year, DateTime.MaxValue.Month, DateTime.MaxValue.Day, DateTime.MaxValue.Hour, DateTime.MaxValue.Minute, DateTime.MaxValue.Second);

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in DateTime (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in DateTime (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objRetrieved.DateTime, "DatabaseTypeTests: in DateTime (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.DateTime = DateTime.MinValue;
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in DateTime (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in DateTime (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objReRetrieved.DateTime, "DatabaseTypeTests: in DateTime (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.DateTime = default(DateTime);
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in DateTime (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in DateTime (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in DateTime (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.DateTime, objReReRetrieved.DateTime, "DatabaseTypeTests: in DateTime (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in String (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in String (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objRetrieved.String, "DatabaseTypeTests: in String (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.String = new string(Enumerable.Range(0, 199).Select(i => 'z').Concat(new [] { '@' }).ToArray());
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in String (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in String (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objReRetrieved.String, "DatabaseTypeTests: in String (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.String = string.Empty;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in String (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in String (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objReReRetrieved.String, "DatabaseTypeTests: in String (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestStringNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.String = null;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in String (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in String (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objRetrieved.String, "DatabaseTypeTests: in String (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.String = new string(Enumerable.Range(0, 199).Select(i => 'z').Concat(new [] { '@' }).ToArray());
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in String (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in String (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objReRetrieved.String, "DatabaseTypeTests: in String (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.String = "a";
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in String (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in String (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in String (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.String, objReReRetrieved.String, "DatabaseTypeTests: in String (Null) Saved Value and Retrieved Value should be Equal.");
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
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Text (NonNull) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Text (NonNull) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objRetrieved.Text, "DatabaseTypeTests: in Text (NonNull) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Text = new string(Enumerable.Range(0, 65534).Select(i => 'z').Concat(new [] { '@' }).ToArray());
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Text (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Text (NonNull) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objReRetrieved.Text, "DatabaseTypeTests: in Text (NonNull) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Text = string.Empty;
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Text (NonNull) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Text (NonNull) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTable>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (NonNull) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objReReRetrieved.Text, "DatabaseTypeTests: in Text (NonNull) Saved Value and Retrieved Value should be Equal.");
        }

        [Test]
        public void TestTextNull()
        {
            var obj = new ComplexTypeTestTableWithNull();

            // Set Default Value for Tests.
            obj.Text = null;

            // Test Add
            var inserted = Database.AddObject(obj);
            Assert.IsTrue(inserted, "DatabaseTypeTests: Could not insert object in Text (Null) Test.");
            Assert.IsTrue(obj.IsPersisted, "DatabaseTypeTests: Inserted Object in Text (Null) Test doesn't have Persisted Flag set.");

            // Test Read
            var objRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objRetrieved.Text, "DatabaseTypeTests: in Text (Null) Insterted Value and Retrieved Value should be Equal.");

            // Test Save
            obj.Text = new string(Enumerable.Range(0, 65534).Select(i => 'z').Concat(new [] { '@' }).ToArray());
            var saved = Database.SaveObject(obj);
            Assert.IsTrue(saved, "DatabaseTypeTests: Could not save objet in Text (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Saved Object in Text (Null) Test still have Dirty Flag set.");

            // Test Re-Read
            var objReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objReRetrieved.Text, "DatabaseTypeTests: in Text (Null) Saved Value and Retrieved Value should be Equal.");

            // Test Re-Save
            obj.Text = "a";
            var reSaved = Database.SaveObject(obj);
            Assert.IsTrue(reSaved, "DatabaseTypeTests: Could not Re-Save objet in Text (Null) Test.");
            Assert.IsFalse(obj.Dirty, "DatabaseTypeTests: Re-Saved Object in Text (Null) Test still have Dirty Flag set.");

            // Test Re-Re-Read
            var objReReRetrieved = Database.FindObjectByKey<ComplexTypeTestTableWithNull>(obj.PrimaryKey);
            Assert.IsNotNull(objReReRetrieved, "DatabaseTypeTests: Could not retrieve object (ID {0}) in Text (Null) Test.", obj.PrimaryKey);
            Assert.AreEqual(obj.Text, objReReRetrieved.Text, "DatabaseTypeTests: in Text (Null) Saved Value and Retrieved Value should be Equal.");
        }
    }
}
