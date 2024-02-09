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
			Assert.That(TestData.IsPersisted, Is.False, "Newly Created Data Object should not be persisted...");
			Assert.That(TestData.CustomParams.First().IsPersisted, Is.False, "Newly Created Param Object should not be persisted...");
			
			// Insert Object
			var paramsInserted = TestData.CustomParams.Select(o => Database.AddObject(o)).ToArray();
			var inserted = Database.AddObject(TestData);
			
			Assert.That(inserted, Is.True, "Test Object not inserted properly in Database !");
			Assert.That(paramsInserted.All(result => result), Is.True, "Params Objects not inserted properly in Database !");
			
			// Check Saved Object is Persisted
			Assert.That(TestData.IsPersisted, Is.True, "Newly Created Data Object should be persisted...");
			Assert.That(TestData.CustomParams.First().IsPersisted, Is.True, "Newly Created Param Object should be persisted...");

			// Retrieve Object From Database
			var RetrieveData = Database.FindObjectByKey<TableWithCustomParams>(TestData.ObjectId);
			
			// Check Retrieved object is Persisted
			Assert.That(RetrieveData.IsPersisted, Is.True, "Retrieved Data Object should be persisted...");
			Assert.That(RetrieveData.CustomParams.First().IsPersisted, Is.True, "Retrieved Param Object should be persisted...");
			
			// Compare both Objects
			Assert.That(RetrieveData.ObjectId, Is.EqualTo(TestData.ObjectId), "Newly Created and Inserted Data Object should have the same ID than Retrieved Object.");
			
			Assert.That(RetrieveData.CustomParams.Length,
			                Is.EqualTo(TestData.CustomParams.Length),
			                "Saved Object and Retrieved Object doesn't have the same amount of Custom Params");
			
			Assert.That(RetrieveData.CustomParams.First(param => param.KeyName == "TestParam").Value,
                            Is.EqualTo(TestData.CustomParams.First(param => param.KeyName == "TestParam").Value),
			               "Both Saved Object and Retrieved Object should have similar Custom Params...");
		}
	}
}
