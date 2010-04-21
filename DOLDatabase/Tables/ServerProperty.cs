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
	[DataTable(TableName = "ServerProperty")]
	public class ServerProperty: DataObject
	{
		private string m_category;
		private string m_key;
		private string m_description;
		private string m_defaultValue;
		private string m_value;

		public ServerProperty()
		{
			m_category = "";
			m_key = "";
			m_description = "";

			m_defaultValue = ""; ;
			m_value = "";
		}

		[DataElement(AllowDbNull = false)]
		public string Category
		{
			get
			{
				return m_category;
			}
			set
			{
				m_category = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Key
		{
			get
			{
				return m_key;
			}
			set
			{
				m_key = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string DefaultValue
		{
			get
			{
				return m_defaultValue;
			}
			set
			{
				m_defaultValue = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Value
		{
			get
			{
				return m_value;
			}
			set
			{
				m_value = value;
				Dirty = true;
			}
		}
	}
}
