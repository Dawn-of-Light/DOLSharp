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
using System.Collections;

namespace DOL.GS.Database
{
	/// <summary>
	/// Database Storage of Tasks
	/// </summary>
	public class DBLootTemplate
	{
		protected int		m_id;
		protected string	m_TemplateName = "";
		protected string	m_ItemTemplateID = "";
		protected int		m_Chance = 99;
		protected GenericItemTemplate	m_itemTemplate;
		
		public int LootTemplateID
		{
			get
			{
				return m_id;
			}
			set
			{
				m_id = value;
			}
		}
		
		public string TemplateName
		{
			get 
			{
				return m_TemplateName;
			}
			set	
			{
				m_TemplateName = value;
			}
		}

		public string ItemTemplateID
		{
			get 
			{
				return m_ItemTemplateID;
			}
			set	
			{
				m_ItemTemplateID = value;
			}
		}

		public int Chance
		{
			get
			{
				return m_Chance;
			}
			set	
			{
				m_Chance = value;
			}
		}

		public GenericItemTemplate ItemTemplate
		{
			get
			{
				if(m_itemTemplate == null) m_itemTemplate = new GenericItemTemplate();
				return m_itemTemplate;
			}
			set	
			{
				m_itemTemplate = value;
			}
		}	
	}
}
