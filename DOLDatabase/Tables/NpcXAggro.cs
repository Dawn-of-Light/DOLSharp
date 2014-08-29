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
	/// Link NPC to Aggro Template Record.
	/// </summary>
	[DataTable(TableName = "npc_xaggro")]
	public class NpcXAggro : DataObject
	{
		private long m_npc_xaggroID;
		
		/// <summary>
		/// Npc X Aggro Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xaggroID {
			get { return m_npc_xaggroID; }
			set { m_npc_xaggroID = value; }
		}
		
		private string m_spotID;
		
		/// <summary>
		/// Spot ID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string SpotID {
			get { return m_spotID; }
			set { m_spotID = value; Dirty = true; }
		}
		
		private string m_aggroID;
		
		/// <summary>
		/// Aggro ID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true, Varchar = 150)]
		public string AggroID {
			get { return m_aggroID; }
			set { m_aggroID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Link to NpcAggro Table (n,n)
		/// </summary>
		[Relation(LocalField = "AggroID", RemoteField = "AggroID", AutoLoad = true, AutoDelete = false)]
		public NpcAggro Aggro;
		
		public NpcXAggro()
		{
		}
	}
}
