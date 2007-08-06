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
using DOL.Database.Attributes;

namespace DOL.Database
{
	/// <summary>
	/// 
	/// </summary>
	[DataTable(TableName="line_x_spell")]
	public class DBLineXSpell : DataObject
	{
		protected string m_line_name;
		protected int m_spellid;
		protected int m_level;

		static bool	m_autoSave;

		public DBLineXSpell()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get
			{
				return m_autoSave;
			}
			set
			{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull=false, Index=true)]
		public string LineName
		{
			get
			{
				return m_line_name;
			}
			set
			{
				Dirty = true;
				m_line_name = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public int SpellID
		{
			get
			{
				return m_spellid;
			}
			set
			{
				Dirty = true;
				m_spellid = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public int Level
		{
			get
			{
				return m_level;
			}
			set
			{
				Dirty = true;
				m_level = value;
			}
		}
	}
}
