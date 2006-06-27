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
	/// Account table
	/// </summary>
	public class DBLootOTD
	{
		private int m_id;
		private string m_itemTemplateID;
		private int m_minLevel;
		private string m_classAllowed;
		private string m_mobName;
		private GenericItemTemplate m_itemTemplate;
		
		public int LootOTDID
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

		/// <summary>
		/// The item template id of the OTD
		/// </summary>
		public string ItemTemplateID
		{
			get
			{
				return m_itemTemplateID;
			}
			set
			{   
				m_itemTemplateID = value;
			}
		}

		/// <summary>
		/// The minimum level require to drop the OTD
		/// </summary>
		public int MinLevel
		{
			get
			{
				return m_minLevel;
			}
			set
			{   
				m_minLevel = value;
			}
		}

		/// <summary>
		/// The class allowed to drop OTD
		/// </summary>
		public string SerializedClassAllowed
		{
			get
			{
				return m_classAllowed;
			}
			set
			{   
				m_classAllowed = value;
			}
		}
		/// <summary>
		/// The mob who drop the OTD
		/// </summary>
		public string MobName
		{
			get
			{
				return m_mobName;
			}
			set
			{   
				m_mobName = value;
			}
		}

		public GenericItemTemplate item
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
