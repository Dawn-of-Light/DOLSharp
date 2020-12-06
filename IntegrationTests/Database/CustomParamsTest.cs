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
	/// <summary>
	/// Unit tests for the Database Custom Params
	/// </summary>
	[TestFixture]
	public class CustomParamsTest
	{
		public CustomParamsTest()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		
		[Test]
		public void TableParamSaveLoadTest()
		{
			Database.RegisterDataObject(typeof(TableCustomParams));
			Database.RegisterDataObject(typeof(TableWithCustomParams));
			
			var TestData = new TableWithCustomParams();
			TestData.TestValue = "NUnitTest";
			TestData.CustomParams = new [] { new TableCustomParams(TestData.TestValue, "TestParam", Convert.ToString(true)) };
			
			// Cleanup
			var Cleanup = Database.SelectAllObjects<TableWithCustomParams>();
			foreach (var obj in Cleanup)
				Database.DeleteObject(obj);
			
			// Check Dynamic object is not Persisted
			Assert.IsFalse(TestData.IsPersisted, "Newly Created Data Object should not be persisted...");
			Assert.IsFalse(TestData.CustomParams.First().IsPersisted, "Newly Created Param Object should not be persisted...");
			
			// Insert Object
			var paramsInserted = TestData.CustomParams.Select(o => Database.AddObject(o)).ToArray();
			var inserted = Database.AddObject(TestData);
			
			Assert.IsTrue(inserted, "Test Object not inserted properly in Database !");
			Assert.IsTrue(paramsInserted.All(result => result), "Params Objects not inserted properly in Database !");
			
			// Check Saved Object is Persisted
			Assert.IsTrue(TestData.IsPersisted, "Newly Created Data Object should be persisted...");
			Assert.IsTrue(TestData.CustomParams.First().IsPersisted, "Newly Created Param Object should be persisted...");

			// Retrieve Object From Database
			var RetrieveData = Database.FindObjectByKey<TableWithCustomParams>(TestData.ObjectId);
			
			// Check Retrieved object is Persisted
			Assert.IsTrue(RetrieveData.IsPersisted, "Retrieved Data Object should be persisted...");
			Assert.IsTrue(RetrieveData.CustomParams.First().IsPersisted, "Retrieved Param Object should be persisted...");
			
			// Compare both Objects
			Assert.AreEqual(TestData.ObjectId, RetrieveData.ObjectId, "Newly Created and Inserted Data Object should have the same ID than Retrieved Object.");
			
			Assert.AreEqual(TestData.CustomParams.Length,
			                RetrieveData.CustomParams.Length,
			                "Saved Object and Retrieved Object doesn't have the same amount of Custom Params");
			
			Assert.AreEqual(TestData.CustomParams.First(param => param.KeyName == "TestParam").Value,
			                RetrieveData.CustomParams.First(param => param.KeyName == "TestParam").Value,
			               "Both Saved Object and Retrieved Object should have similar Custom Params...");
		}
	}
}
