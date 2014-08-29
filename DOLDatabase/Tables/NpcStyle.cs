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
	/// Npc Style List Templated by string ID.
	/// </summary>
	[DataTable(TableName="npc_style")]
	public class NpcStyle : DataObject
	{
		private long m_npc_styleID;
		
		/// <summary>
		/// Npc Style List Primary Key Auto Increment.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_styleID {
			get { return m_npc_styleID; }
			set { m_npc_styleID = value; }
		}
		
		private string m_styleListID;
		
		/// <summary>
		/// Style List ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string StyleListID {
			get { return m_styleListID; }
			set { m_styleListID = value; Dirty = true; }
		}

		private int m_styleID;
		
		/// <summary>
		/// Style ID reference (class = 0)
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int StyleID {
			get { return m_styleID; }
			set { m_styleID = value; Dirty = true; }
		}

		/// <summary>
		/// Link to Style Table (n, n), multiple Styles with each class
		/// </summary>
		[Relation(LocalField = "StyleID", RemoteField = "ID", AutoLoad = true, AutoDelete = false)]		
		public DBStyle[] Styles;
		
		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcStyle()
		{
		}
	}
}
