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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database.Tests
{
	/// <summary>
	/// Test table handling Custom Params
	/// </summary>
	[DataTable(TableName = "Test_TableWithCustomParams")]
	public class TableWithCustomParams : DataObject
	{
		string m_testValue;
		[DataElement(Index = true, Varchar = 255, AllowDbNull = true)]
		public string TestValue { get { return m_testValue; } set { Dirty = true; m_testValue = value; } }
		
		[Relation(LocalField = "TestValue", RemoteField = "TestValue", AutoLoad = true, AutoDelete = true)]
		public TableCustomParams[] CustomParams;
		
		public TableWithCustomParams()
		{
		}
	}
	
	[DataTable(TableName = "Test_TableCustomParams")]
	public class TableCustomParams : CustomParam
	{	
		string m_testValue;
		[DataElement(Index = true, Varchar = 255, AllowDbNull = true)]
		public string TestValue { get { return m_testValue; } set { Dirty = true; m_testValue = value; } }
		
		public TableCustomParams()
		{
		}
		
		public TableCustomParams(string TestValue, string KeyName, string Value)
		{
			this.TestValue = TestValue;
			this.KeyName = KeyName;
			this.Value = Value;
		}
	}
}
