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
	/// NPC Ability List Templated by string ID.
	/// </summary>
	[DataTable(TableName="npc_ability")]
	public class NpcAbility : DataObject
	{
		private long m_npc_AbilityID;
		
		/// <summary>
		/// NPC Ability List Primary Key Auto Increment
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_AbilityID {
			get { return m_npc_AbilityID; }
			set { m_npc_AbilityID = value; }
		}
		
		private string m_AbilityListID;
		
		/// <summary>
		/// Ability List ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string AbilityListID {
			get { return m_AbilityListID; }
			set { m_AbilityListID = value; Dirty = true; }
		}
		
		private string m_keyName;
		
		/// <summary>
		/// Ability KeyName Reference.
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 100, Index = true)]
		public string KeyName {
			get { return m_keyName; }
			set { m_keyName = value; Dirty = true; }
		}
		
		private byte m_AbilityLevel;
		
		/// <summary>
		/// Ability level (Default to 1)
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public byte AbilityLevel {
			get { return m_AbilityLevel; }
			set { m_AbilityLevel = value; Dirty = true; }
		}
		
		/// <summary>
		/// Relation to Ability (n,1)
		/// </summary>
		[Relation(LocalField = "KeyName", RemoteField = "KeyName", AutoLoad = true, AutoDelete = false)]		
		public DBAbility Ability;

		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcAbility()
		{
			// Default to Level 1
			m_AbilityLevel = 1;
		}
	}
}
