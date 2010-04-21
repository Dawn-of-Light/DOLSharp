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
	/// Database Storage of Tasks
	/// </summary>
	[DataTable(TableName="LootTemplate")]
	public class LootTemplate : DataObject
	{
		protected string	m_TemplateName = "";
		protected string	m_ItemTemplateID = "";
		protected int		m_Chance = 99;
		protected int		m_count = 1;
		
		public LootTemplate()
		{
			AutoSave = false;
		}
		
		[DataElement(AllowDbNull=false, Index=true)]
		public string TemplateName
		{
			get {return m_TemplateName;}
			set
			{
				Dirty = true;
				m_TemplateName = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string ItemTemplateID
		{
			get {return m_ItemTemplateID;}
			set
			{
				Dirty = true;
				m_ItemTemplateID = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public int Chance
		{
			get {return m_Chance;}
			set
			{
				Dirty = true;
				m_Chance = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Count
		{
			get
			{
				return Math.Max(1, m_count);
			}
			set
			{
				Dirty = true;
				m_count = value;
			}
		}
	}
}
