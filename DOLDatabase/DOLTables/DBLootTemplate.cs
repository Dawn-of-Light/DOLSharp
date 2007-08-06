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
	[DataTable(TableName="loot_template")]
	public class DBLootTemplate : DataObject
	{
		protected string	m_TemplateName = "";
		protected string	m_ItemTemplateID = "";
		protected int		m_Chance = 99;
		static bool			m_autoSave;
		
		public DBLootTemplate()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get	{return m_autoSave;}
			set	{m_autoSave = value;}
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

		[Relation(LocalField = "ItemTemplateID", RemoteField = "Id_nb", AutoLoad = true, AutoDelete=false)]
		public ItemTemplate ItemTemplate;		
	}
}
