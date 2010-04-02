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

/*
 * Credits go to: 
 * - Echostorm's Mob Drop Loot System
 * - Roach's modifications to add loottemplate base mobdrops  
 */

using System;
using DOL.Database ;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// Database Storage of Mob LootTemplate Relation
	/// </summary>
	[DataTable(TableName="MobXLootTemplate")]
	public class MobXLootTemplate : DataObject
	{
		private string	m_MobName = "";
		private string	m_LootTemplateName = "";
		private int		m_dropCount;
		private static bool			m_autoSave;
		
		/// <summary>
		/// Constructor
		/// </summary>
		public MobXLootTemplate()
		{
			m_MobName = "";
			m_LootTemplateName = "";			
			m_dropCount=1;
			m_autoSave = false;
		}

		/// <summary>
		/// Auto Save
		/// </summary>
		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
		}
		
		/// <summary>
		/// Mob Name
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string MobName
		{
			get {return m_MobName;}
			set	
			{
				Dirty = true;
				m_MobName = value;
			}
		}
		
		/// <summary>
		/// Loot Template Name
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public string LootTemplateName
		{
			get {return m_LootTemplateName;}
			set	
			{
				Dirty = true;
				m_LootTemplateName = value;
			}
		}

		/// <summary>
		/// Drop Count
		/// </summary>
		[DataElement(AllowDbNull=false)]
		public int DropCount
		{
			get {return m_dropCount;}
			set	
			{
				Dirty = true;
				m_dropCount = value;
			}
		}
	}
}
