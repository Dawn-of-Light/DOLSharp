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
	/// Keep Data Position Handles all object attached to Keep relative to Keep Data location
	/// it can be cumulative templates handled by cross relation table or dynamically by scripts.
	/// Have one template for Doors, One template for Patrols, One template for Guards, enable them all
	/// or only some of them based on scripted pre-requisite.
	/// </summary>
	[DataTable(TableName="keepdata_position")]
	public class KeepDataPosition : DataObject
	{
		private long m_keepData_PositionID;
		
		/// <summary>
		/// Primary Key AI.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long KeepData_PositionID {
			get { return m_keepData_PositionID; }
			set { m_keepData_PositionID = value; }
		}
		
		private string m_positionTemplateName;
		
		/// <summary>
		/// PositionTemplateName linked accross Keep Template Link Table.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public string PositionTemplateName {
			get { return m_positionTemplateName; }
			set { m_positionTemplateName = value; }
		}
		
		private byte m_skinFilter;
		
		/// <summary>
		/// Should this element only apply to identified Component SkinID ?
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte SkinFilter {
			get { return m_skinFilter; }
			set { m_skinFilter = value; }
		}
		
		private byte m_height;
		
		/// <summary>
		/// Height of this Position
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte Height {
			get { return m_height; }
			set { m_height = value; }
		}
		
		private int m_xoff;
		
		/// <summary>
		/// X offset of this Position
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Xoff {
			get { return m_xoff; }
			set { m_xoff = value; }
		}
		
		private int m_yoff;
		
		/// <summary>
		/// Y offset of this Position
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Yoff {
			get { return m_yoff; }
			set { m_yoff = value; }
		}
		
		private int m_zoff;
		
		/// <summary>
		/// Z offset of this Position
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int Zoff {
			get { return m_zoff; }
			set { m_zoff = value; }
		}
		
		private int m_hoff;
		
		/// <summary>
		/// Height offset of this Position.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = false)]
		public int Hoff {
			get { return m_hoff; }
			set { m_hoff = value; }
		}
		
		private byte m_rotation;
		
		/// <summary>
		/// Rotation of this Posiiton.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = false)]
		public byte Rotation {
			get { return m_rotation; }
			set { m_rotation = value; }
		}
		
		private byte m_index;
		
		/// <summary>
		/// Index of this Position.
		/// </summary>
		[DataElement(AllowDbNull = true, Index = false)]
		public byte Index {
			get { return m_index; }
			set { m_index = value; }
		}
		
		private string m_classType;
		
		/// <summary>
		/// Class type Implemented at this position.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public string ClassType {
			get { return m_classType; }
			set { m_classType = value; }
		}
		
		private string m_classParams;
		
		/// <summary>
		/// Class params, if any...
		/// </summary>
		[DataElement(AllowDbNull = true, Index = false)]
		public string ClassParams {
			get { return m_classParams; }
			set { m_classParams = value; }
		}		
		/*
		/// <summary>
		/// Last time this record was updated.
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public DateTime Updated {
			get { return DateTime.UtcNow; }
			set { Dirty = true; }
		}		*/
		
		/// <summary>
		/// Default Constructort
		/// </summary>
		public KeepDataPosition()
		{
		}
	}
}
