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
/* Created by Schaf
 * Last modified by Schaf on 10.12.2004 20:09
 */

using System;

using DOL.Database;
using DOL.Database.Attributes;

namespace DOL.Database
{
	[DataTable(TableName = "househookpointitem")]
	public class DBHouseHookpointItem : DataObject
	{
		private long m_id;
		private int m_houseNumber; // the number of the house
		private uint m_hookpointID;
		private ushort m_heading;
		private string m_templateID; // the item template id of the item placed
		private byte m_index;

		public DBHouseHookpointItem(){}

		[PrimaryKey(AutoIncrement = true)]
		public long ID
		{
			get { return m_id; }
			set
			{
				Dirty = true;
				m_id = value;
			}
		}


		[DataElement(AllowDbNull = false)]
		public int HouseNumber
		{
			get { return m_houseNumber; }
			set
			{
				Dirty = true;
				m_houseNumber = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public uint HookpointID
		{
			get { return m_hookpointID; }
			set
			{
				Dirty = true;
				m_hookpointID = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public ushort Heading
		{
			get { return m_heading; }
			set
			{
				Dirty = true;
				m_heading = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public string ItemTemplateID
		{
			get { return m_templateID; }
			set
			{
				Dirty = true;
				m_templateID = value;
			}
		}

		/// <summary>
		/// Index of this item in case there is more than 1
		/// of the same type.
		/// </summary>
		[DataElement(AllowDbNull = true)]
		public byte Index
		{
			get { return m_index; }
			set
			{
				Dirty = true;
				m_index = value;
			}
		}

		private object m_gameObject = null;
		/// <summary>
		/// The game object attached to this hookpoint
		/// </summary>
		public object GameObject
		{
			get { return m_gameObject; }
			set { m_gameObject = value; }
		}
	}
}
