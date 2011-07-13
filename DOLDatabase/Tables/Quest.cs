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
using System.Collections.Specialized;
using System.Text;
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="Quest")]
	public class DBQuest : DataObject
	{
		private string		m_name;
		private	string		m_characterid;
		private	int			m_step;
		private string		m_customPropertiesString;

		/// <summary>
		/// Constructor
		/// </summary>
		public DBQuest() : this("",1,"")
		{
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="name">The quest name</param>
		/// <param name="step">The step number</param>
		/// <param name="charname">The character name</param>
		public DBQuest(string name, int step, string charname)
		{
			m_name = name;
			m_step = step;
			m_characterid = charname;
		}
		/// <summary>
		/// Quest Name
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public string Name
		{
			get
			{
				return m_name;
			}
			set
			{
				Dirty = true;
				m_name = value;
			}
		}

		/// <summary>
		/// Quest Step
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false)]
		public int Step
		{
			get
			{
				return m_step;
			}
			set
			{
				Dirty = true;
				m_step = value;
			}
		}

		/// <summary>
		/// Character Name
		/// </summary>
		[DataElement(AllowDbNull=false,Unique=false,Index=true)]
		public string Character_ID
		{
			get
			{
				return m_characterid;
			}
			set
			{
				Dirty = true;
				m_characterid = value;
			}
		}

		/// <summary>
		/// Custom properties string
		/// </summary>
		[DataElement(AllowDbNull=true,Unique=false)]
		public string CustomPropertiesString
		{
			get
			{
				return m_customPropertiesString;
			}
			set
			{
				Dirty = true;
				m_customPropertiesString = value;
			}
		}
	}
}
