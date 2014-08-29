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
	/// Link Between NPC Spot ID and Ability List.
	/// </summary>
	[DataTable(TableName="npc_xability")]
	public class NpcXAbility : DataObject
	{
		private long m_npc_xabilityID;
		
		/// <summary>
		/// Npc X Type Primary Key Auto Increment.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xabilityID {
			get { return m_npc_xabilityID; }
			set { m_npc_xabilityID = value; }
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

		private string m_abilityListID;
		
		/// <summary>
		/// Ability List ID Reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string AbilityListID {
			get { return m_abilityListID; }
			set { m_abilityListID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Relation to Ability List (1, n)
		/// </summary>
		[Relation(LocalField = "AbilityListID", RemoteField = "AbilityListID", AutoLoad = true, AutoDelete = false)]
		public NpcAbility[] AbilityList;
		
		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcXAbility()
		{
		}
	}
}
