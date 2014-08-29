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
	/// n,n Relation Table between KeepData TemplateName and KeepComponent TemplateName
	/// </summary>
	[DataTable(TableName="keepdataxkeepdata_position")]
	public class KeepDataXKeepDataPosition : DataObject 
	{
		private long m_keepdataXKeepData_PositionID;
		
		/// <summary>
		/// Primary Key AI
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long KeepdataXKeepData_PositionID {
			get { return m_keepdataXKeepData_PositionID; }
			set { m_keepdataXKeepData_PositionID = value; }
		}
		
		private string m_templateName;
		
		/// <summary>
		/// KeepData TemplateName
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string TemplateName {
			get { return m_templateName; }
			set { m_templateName = value; Dirty = true; }
		}
		
		private string m_positionTemplateName;
		
		/// <summary>
		/// KeepComponent TemplateName
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string PositionTemplateName {
			get { return m_positionTemplateName; }
			set { m_positionTemplateName = value; Dirty = true; }
		}
/*
		/// <summary>
		/// Last time this record was updated.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public DateTime Updated {
			get { return DateTime.UtcNow; }
			set { Dirty = true; }
		}
		*/
		/// <summary>
		/// Relation to Component Collection for this ComponentTemplate
		/// </summary>
		[Relation(LocalField = "PositionTemplateName", RemoteField = "PositionTemplateName", AutoLoad = true, AutoDelete = false)]
		public KeepDataPosition[] Position;
		
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public KeepDataXKeepDataPosition()
		{
		}
	}
}
