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
	[DataTable(TableName = "househookpointoffset")]
	public class HouseHookpointOffset : DataObject
	{
		private long m_id;
		private int m_houseModel;
		private int m_hookpointID;
		private int m_x;
		private int m_y;
		private int m_z;
		private int m_heading;

		public HouseHookpointOffset()
		{
		}

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
		public int HouseModel
		{
			get { return m_houseModel; }
			set
			{
				Dirty = true;
				m_houseModel = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int HookpointID
		{
			get { return m_hookpointID; }
			set
			{
				Dirty = true;
				m_hookpointID = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int X
		{
			get { return m_x; }
			set
			{
				Dirty = true;
				m_x = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Y
		{
			get { return m_y; }
			set
			{
				Dirty = true;
				m_y = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Z
		{
			get { return m_z; }
			set
			{
				Dirty = true;
				m_z = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Heading
		{
			get { return m_heading; }
			set
			{
				Dirty = true;
				m_heading = value;
			}
		}
	}
}
