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
	/// <summary>
	/// Commands table
	/// </summary>
	[DataTable(TableName = "Commands")]
	public class DBCommand : DataObject
	{
		private string m_name;
		private string m_aliases;
		private string m_privlevels;
		private string m_implementation;
		private string m_description;
		private string m_usages;

		/// <summary>
		/// Command to handle
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				m_name = value;
			}
		}

		/// <summary>
		/// Other names the command goes by (separator = ';')
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Aliases
		{
			get
			{
				return m_aliases;
			}
			set
			{
				m_aliases = value;
			}
		}

		/// <summary>
		/// Serialized PrivLevels required for this command (separator = ';')
		/// </summary>
		[DataElement(AllowDbNull = false)]
		public string PrivLevels
		{
			get
			{
				return m_privlevels;
			}
			set
			{
				m_privlevels = value;
			}
		}

		/// <summary>
		/// Implementation of the command
		/// </summary>
		[PrimaryKey]
		public string Implementation
		{
			get
			{
				return m_implementation;
			}
			set
			{
				m_implementation = value;
			}
		}

		/// <summary>
		/// Description of the command
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				m_description = value;
			}
		}

		/// <summary>
		/// How to use the command (serialized, separator = '\n')
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public string Usages
		{
			get
			{
				return m_usages;
			}
			set
			{
				m_usages = value;
			}
		}
	}
}
