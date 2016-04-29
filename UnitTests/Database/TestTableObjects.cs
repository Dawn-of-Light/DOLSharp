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
	[DataTable(TableName = "Test_TableWithMultiIndexes")]
	public class TestTableWithMultiIndexes : DataObject
	{
		public string m_index1;
		[DataElement(IndexColumns = "Index2")]
		public string Index1 { get; set; }
		public string m_index2;
		[DataElement(IndexColumns = "Index3")]
		public string Index2 { get; set; }
		public string m_index3;
		[DataElement]
		public string Index3 { get; set; }
	}

}
