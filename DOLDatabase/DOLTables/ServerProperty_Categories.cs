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
	[DataTable(TableName = "serverproperty_categories")]
	public class ServerPropertyCategories: DataObject
	{
		private string 	m_name;
		private int 	m_parent;
		
		private static bool m_autoSave;		
		public override bool AutoSave {
			get { return m_autoSave; }
			set { m_autoSave = value; }
		}

		public ServerPropertyCategories()
		{
			m_name = null;
			m_parent = 0;
		}

		[DataElement(AllowDbNull = false)]
		public string Name {
			get { return m_name; }
			set { m_name = value; Dirty=true;}
		}

		[DataElement(AllowDbNull = false)]
		public int Parent {
			get { return m_parent; }
			set { m_parent = value; }
		}
	}
}
