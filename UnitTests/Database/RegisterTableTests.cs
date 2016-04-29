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
using System.Reflection;

using DOL.GS;
using DOL.Database;
using DOL.Database.Attributes;

using NUnit.Framework;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Description of RegisterTableTests.
	/// </summary>
	[TestFixture, Explicit]
	public class RegisterTableTests
	{
		public RegisterTableTests()
		{
			Database = DatabaseSetUp.Database;
		}
		
		protected SQLObjectDatabase Database { get; set; }
		
		/// <summary>
		/// Test to Register all Assemblies Tables
		/// </summary>
		[Test]
		public void TestAllAvailableTables()
		{
			foreach (Assembly assembly in new [] { typeof(GameServer).Assembly, typeof(DataObject).Assembly })
			{
				// Walk through each type in the assembly
				foreach (Type type in assembly.GetTypes())
				{
					// Pick up a class
					// Aredhel: Ok, I know checking for InventoryArtifact type
					// is a hack, but I currently have no better idea.
					if (type.IsClass != true || type == typeof (InventoryArtifact))
						continue;
					object[] attrib = type.GetCustomAttributes(typeof(DataTable), true);
					if (attrib.Length > 0)
					{
						var dth = new DataTableHandler(type);
						Database.CheckOrCreateTableImpl(dth);
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
			Database.CheckOrCreateTableImpl(dth);
		}
	}
}
