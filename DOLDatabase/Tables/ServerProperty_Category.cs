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

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "serverproperty_category")]
	public class ServerPropertyCategory: DataObject
	{
		private string 	m_base_cat;
		private string 	m_parent_cat;
		private string 	m_display_name;
		
		private static bool m_autoSave;
		public override bool AutoSave {
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		public ServerPropertyCategory()
		{
			m_base_cat = null;
			m_parent_cat = null;
			m_display_name = null;

		}
		
		[DataElement(AllowDbNull = false)]
		public string BaseCategory {
			get { return m_base_cat; }
			set { m_base_cat = value;Dirty=true;}
		}

		[DataElement(AllowDbNull = true)]
		public string ParentCategory {
			get { return m_parent_cat; }
			set { m_parent_cat = value; }
		}
		
		[DataElement(AllowDbNull = false)]
		public string DisplayName {
			get { return m_display_name; }
			set { m_display_name = value;}
		}
	}
}
