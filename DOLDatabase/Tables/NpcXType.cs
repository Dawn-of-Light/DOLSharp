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

using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Link Table between Npc and Type Templates.
	/// </summary>
	[DataTable(TableName="npc_xtype")]
	public class NpcXType : DataObject
	{
		private long m_npc_xtypeID;
		
		/// <summary>
		/// Npc X Type Primary Key Auto Increment.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xtypeID {
			get { return m_npc_xtypeID; }
			set { m_npc_xtypeID = value; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Spot ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Unique = true)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_typeID;
		
		/// <summary>
		/// Type ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string TypeID {
			get { return m_typeID; }
			set { m_typeID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Relation to Npc Type Table (n,1)
		/// </summary>
		[Relation(LocalField = "TypeID", RemoteField = "TypeID", AutoLoad = true, AutoDelete = false)]
		public NpcType Type;
		
		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcXType()
		{
		}
	}
}
