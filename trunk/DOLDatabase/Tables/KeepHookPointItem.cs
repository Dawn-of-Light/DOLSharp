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

using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "KeepHookPointItem")]
	public class DBKeepHookPointItem : DataObject
	{
		private int m_keepID;
		private int m_componentID;
		private int m_hookPointID;
		private string m_classType;

		public DBKeepHookPointItem()
			: base()
		{ }

		public DBKeepHookPointItem(int keepID, int componentID, int hookPointID, string classType)
			: base()
		{
			m_keepID = keepID;
			m_componentID = componentID;
			m_hookPointID = hookPointID;
			m_classType = classType;
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public int KeepID
		{
			get { return m_keepID; }
			set
			{
				Dirty = true;
				m_keepID = value;
			}
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public int ComponentID
		{
			get { return m_componentID; }
			set
			{
				Dirty = true;
				m_componentID = value;
			}
		}

		[DataElement(AllowDbNull = false, Index = true)]
		public int HookPointID
		{
			get { return m_hookPointID; }
			set
			{
				Dirty = true;
				m_hookPointID = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public string ClassType
		{
			get { return m_classType; }
			set
			{
				Dirty = true;
				m_classType = value;
			}
		}
	}
}