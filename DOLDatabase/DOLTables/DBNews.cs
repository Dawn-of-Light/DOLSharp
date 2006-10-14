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
	/// Account table
	/// </summary>
	[DataTable(TableName = "News")]
	public class DBNews : DataObject
	{
		private DateTime m_creationDate;
		private byte m_type;
		private byte m_realm;
		private string m_text;
		private static bool m_autoSave;

		/// <summary>
		/// Create account row in DB
		/// </summary>
		public DBNews()
		{
			m_creationDate = DateTime.Now;
			m_type = 0;
			m_realm = 0;
			m_text = "";
			m_autoSave = true;
		}

		/// <summary>
		/// Auto save this table
		/// </summary>
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

		[DataElement(AllowDbNull = false)]
		public DateTime CreationDate
		{
			get
			{
				return m_creationDate;
			}
			set
			{
				m_creationDate = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = false)]
		public byte Type
		{
			get
			{
				return m_type;
			}
			set
			{
				m_type = value;
				Dirty = true;
			}
		}

		[DataElement(AllowDbNull = true)]
		public byte Realm
		{
			get
			{
				return m_realm;
			}
			set
			{
				Dirty = true;
				m_realm = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string Text
		{
			get
			{
				return m_text;
			}
			set
			{
				Dirty = true;
				m_text = value;
			}
		}
	}
}
