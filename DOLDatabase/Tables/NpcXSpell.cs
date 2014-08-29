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
	/// Link Between NPC Spot ID and Spell List
	/// </summary>
	[DataTable(TableName="npc_xspell")]
	public class NpcXSpell : DataObject
	{
		private long m_npc_xspellID;
		
		/// <summary>
		/// Npc X Type Primary Key Auto Increment.
		/// </summary>
		[PrimaryKey(AutoIncrement = true)]
		public long Npc_xspellID {
			get { return m_npc_xspellID; }
			set { m_npc_xspellID = value; }
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
		
		private string m_spellListID;
		
		/// <summary>
		/// Spell List ID reference
		/// </summary>
		[DataElement(AllowDbNull = false, Varchar = 150, Index = true)]
		public string SpellListID {
			get { return m_spellListID; }
			set { m_spellListID = value; Dirty = true; }
		}
		
		/// <summary>
		/// Relation to Spell List (1,n)
		/// </summary>
		[Relation(LocalField = "SpellListID", RemoteField = "SpellListID", AutoLoad = true, AutoDelete = false)]
		public NpcSpell[] SpellList;

		/// <summary>
		/// Default Constructror
		/// </summary>
		public NpcXSpell()
		{
		}
	}
}
