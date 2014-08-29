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
	/// Npc Aggro Table describe the aggressiveness of a Mob.
	/// </summary>
	[DataTable(TableName = "npc_aggro")]
	public class NpcAggro : DataObject
	{
		private long m_npc_AggroID;
		
		/// <summary>
		/// Npc Aggro Table Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_AggroID {
			get { return m_npc_AggroID; }
			set { m_npc_AggroID = value; }
		}
		
		private string m_aggroID;
		
		/// <summary>
		/// Npc Aggro Template string ID for easier reference
		/// </summary>
		[DataElement(AllowDbNull = false, Unique = true, Varchar = 150)]
		public string AggroID {
			get { return m_aggroID; }
			set { m_aggroID = value; Dirty = true; }
		}
		
		private byte m_aggroLevel;
		
		/// <summary>
		/// Npc Aggro Amount, 0 Neutral, 0-25 Hostile, 25-100+ Aggressive
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte AggroLevel {
			get { return m_aggroLevel; }
			set { m_aggroLevel = value; Dirty = true; }
		}
		
		private ushort m_aggroRange;
		
		/// <summary>
		/// Range at which aggro Level is effective
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort AggroRange {
			get { return m_aggroRange; }
			set { m_aggroRange = value; Dirty = true; }
		}
		
		private ushort m_maxDistance;
		
		/// <summary>
		/// Max Distance for chasing an ennemy.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public ushort MaxDistance {
			get { return m_maxDistance; }
			set { m_maxDistance = value; Dirty = true; }
		}
		
		/// <summary>
		/// Default Constructor
		/// </summary>
		public NpcAggro()
		{
		}
	}
}
