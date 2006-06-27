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
	/// Database Storage of Mob LootTemplate Relation
	/// </summary>
	public class DBMobXLootTemplate
	{
		protected int		m_id;
		protected string	m_MobName = "";
		protected string	m_LootTemplateName = "";
		protected int		m_dropCount;
		
		public DBMobXLootTemplate()
		{
			m_MobName = "";
			m_LootTemplateName = "";			
			m_dropCount=1;
		}

		public int MobXLootTemplateID
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
		
		public string MobName
		{
			get 
			{
				return m_MobName;
			}
			set	
			{
				m_MobName = value;
			}
		}
		
		public string LootTemplateName
		{
			get 
			{
				return m_LootTemplateName;
			}
			set	
			{
				m_LootTemplateName = value;
			}
		}

		public int DropCount
		{
			get 
			{
				return m_dropCount;
			}
			set	
			{
				m_dropCount = value;
			}
		}
	}
}
