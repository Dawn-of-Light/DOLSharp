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
	/// Npc Spell List Templated by string ID.
	/// </summary>
	[DataTable(TableName="npc_spell")]
	public class NpcSpell : DataObject
	{
		private long m_npc_spellID;
		
		/// <summary>
		/// Spell List Primary Key Auto Increment.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_spellID {
			get { return m_npc_spellID; }
			set { m_npc_spellID = value; }
		}
		
		private string m_spellListID;
		
		/// <summary>
		/// Spell List ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string SpellListID {
			get { return m_spellListID; }
			set { m_spellListID = value; Dirty = true; }
		}
		
		private int m_spellID;
		
		/// <summary>
		/// Spell ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Index = true)]
		public int SpellID {
			get { return m_spellID; }
			set { m_spellID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Link to Spell Table (n, 1)
		/// </summary>
		[Relation(LocalField = "SpellID", RemoteField = "SpellID", AutoLoad = true, AutoDelete = false)]		
		public DBSpell Spell;

		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcSpell()
		{
		}
	}
}
