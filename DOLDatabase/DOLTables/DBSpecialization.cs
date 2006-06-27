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
	[DataTable(TableName="Specialization")]
	public class DBSpecialization : DataObject
	{
		protected string m_keyName;
		protected string m_name = "unknown spec";
		protected string m_description = "no description";
		protected ushort m_icon = 0;

		static bool	m_autoSave;

		public DBSpecialization()
		{
			m_autoSave = false;
		}

		override public bool AutoSave
		{
			get {	return m_autoSave;	}
			set	{
				m_autoSave = value;
			}
		}

		[DataElement(AllowDbNull=false,Unique=true)]
		public string KeyName {
			get {	return m_keyName;	}
			set	{
				Dirty = true;
				m_keyName = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public string Name 
		{
			get {	return m_name;	}
			set {
				Dirty = true;
				m_name = value;
			}
		}

		[DataElement(AllowDbNull=false)]
		public ushort Icon
		{
			get {	return m_icon;	}
			set {
				Dirty = true;
				m_icon = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public string Description 
		{
			get {	return m_description;	}
			set {
				Dirty = true;
				m_description = value;
			}
		}

		[Relation(LocalField = "KeyName", RemoteField = "SpecKeyName", AutoLoad = true, AutoDelete=true)]
		public DBStyle[] Styles;
	}
}