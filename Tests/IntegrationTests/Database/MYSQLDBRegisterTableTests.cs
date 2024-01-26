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
using DOL.Database.Connection;
using DOL.Database.Attributes;

using NUnit.Framework;
using DOL.Database;

namespace DOL.Integration.Database.MySQL
{
	[TestFixture, Explicit]
	public class MYSQLDBRegisterTableTests : RegisterTableTests
	{
		public MYSQLDBRegisterTableTests()
		{
			Database = MySQLDBSetUp.Database;
		}
		
		protected override SQLObjectDatabase GetDatabaseV2 { get { return (SQLObjectDatabase)ObjectDatabase.GetObjectDatabase(ConnectionType.DATABASE_MYSQL, MySQLDBSetUp.ConnectionString); } }
	
		[Test]
		public void TestTableWithBrokenPrimaryKey()
		{
			// Destroy previous table
			Database.ExecuteNonQuery(string.Format("DROP TABLE IF EXISTS `{0}`", AttributesUtils.GetTableName(typeof(TestTableWithBrokenPrimaryV1))));
			// Create Table
			Database.RegisterDataObject(typeof(TestTableWithBrokenPrimaryV1));
			// Break Primary Key
			Database.ExecuteNonQuery(string.Format("ALTER TABLE `{0}` DROP PRIMARY KEY", AttributesUtils.GetTableName(typeof(TestTableWithBrokenPrimaryV1))));
			
			// Get a new Database Object to Trigger Migration
			var DatabaseV2 = GetDatabaseV2;
			
			// Trigger False Migration
			DatabaseV2.RegisterDataObject(typeof(TestTableWithBrokenPrimaryV2));
			
			var adds = DatabaseV2.AddObject(new [] {
			                                	new TestTableWithBrokenPrimaryV2 { PrimaryKey = 1 },
			                                	new TestTableWithBrokenPrimaryV2 { PrimaryKey = 1 },
			                                });
			
			Assert.That(adds, Is.False, "Primary Key was not restored and duplicate key were inserted !");
		}
	}
}
