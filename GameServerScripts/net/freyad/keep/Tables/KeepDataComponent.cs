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

namespace net.freyad.keep
{
	/// <summary>
	/// KeepData Component Table
	/// </summary>
	[DataTable(TableName="keepdata_component")]
	public class KeepDataComponent : DataObject
	{
		private long m_keepData_ComponentID;
		
		/// <summary>
		/// Primary Key AI
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long KeepData_ComponentID {
			get { return m_keepData_ComponentID; }
			set { m_keepData_ComponentID = value; }
		}
		
		private string m_templateName;
		
		/// <summary>
		/// Component Template Name
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string TemplateName {
			get { return m_templateName; }
			set { m_templateName = value; Dirty = true; }
		}
				
		private sbyte m_x;
		
		/// <summary>
		/// X position of this Keep Component
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public sbyte X {
			get { return m_x; }
			set { m_x = value; }
		}
		
		private sbyte m_y;
		
		/// <summary>
		/// Y position of this Keep Component
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public sbyte Y {
			get { return m_y; }
			set { m_y = value; }
		}
		
		private byte m_heading;
		
		/// <summary>
		/// Heading position of this Keep Component
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte Heading {
			get { return m_heading; }
			set { m_heading = value; }
		}
		
		private byte m_skin;
		
		/// <summary>
		/// Skin ID of this Keep Component
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte Skin {
			get { return m_skin; }
			set { m_skin = value; }
		}
		
		private byte m_index;
		
		/// <summary>
		/// Index of this Keep Component
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte Index {
			get { return m_index; }
			set { m_index = value; }
		}
/*
		/// <summary>
		/// Last time this record was updated.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public DateTime Updated {
			get { return DateTime.UtcNow; }
			set { Dirty = true; }
		}*/

		
		/// <summary>
		/// Default constructor
		/// </summary>
		public KeepDataComponent()
		{
		}
	}
}
