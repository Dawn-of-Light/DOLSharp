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
	/// Description of NpcXBaseTemplate.
	/// </summary>
	[DataTable(TableName = "npc_xbasetemplate")]
	public class NpcXBaseTemplate : DataObject
	{
		private long m_npc_xbasetemplateID;
		
		/// <summary>
		/// Npc X Base Template Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xbasetemplateID {
			get { return m_npc_xbasetemplateID; }
			set { m_npc_xbasetemplateID = value; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Npc Spot ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 150)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_templateID;
		
		/// <summary>
		/// Base Template reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 150)]
		public string TemplateID {
			get { return m_templateID; }
			set { m_templateID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Link to Base Template (n, 1)
		/// </summary>
		[Relation(LocalField = "TemplateID", RemoteField = "TemplateID", AutoLoad = true, AutoDelete = false)]
		public NpcBaseTemplate BaseTemplate;
		
		public NpcXBaseTemplate()
		{
		}
	}
}
