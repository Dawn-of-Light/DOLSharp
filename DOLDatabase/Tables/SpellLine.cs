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
	[DataTable(TableName="SpellLine")]
	public class DBSpellLine : DataObject
	{
		protected string m_name="unknown";
		protected string m_keyname;
		protected string m_spec="unknown";
		protected bool m_isBaseLine=true;

		static bool	m_autoSave;

		public DBSpellLine()
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

		[DataElement(AllowDbNull=false,Unique=true)]
		public string KeyName
		{
			get
			{
				return m_keyname;
			}
			set
			{
				Dirty = true;
				m_keyname = value;
			}
		}

		[DataElement(AllowDbNull=true)]
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

		[DataElement(AllowDbNull=true)]
		public string Spec
		{
			get
			{
				return m_spec;
			}
			set
			{
				Dirty = true;
				m_spec = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public bool IsBaseLine
		{
			get
			{
				return m_isBaseLine;
			}
			set
			{
				Dirty = true;
				m_isBaseLine = value;
			}
		}
	}
}
