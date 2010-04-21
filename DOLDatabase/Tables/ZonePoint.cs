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
	[DataTable(TableName="ZonePoint")]
	public class ZonePoint : DataObject
	{

		private ushort	m_id;
		private int	m_x;
		private int	m_y;
		private int	m_z;
		private ushort	m_region;
		private ushort	m_realm;
		private ushort	m_heading;
		private string	m_classType = "";

		public ZonePoint()
		{
			AutoSave=false;
		}

		[DataElement(AllowDbNull=false, Index=true)]
		public ushort Id
		{
			get
			{
				return m_id;
			}
			set
			{
				Dirty = true;
				m_id = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public int X
		{
			get
			{
				return m_x;
			}
			set
			{
				Dirty = true;
				m_x = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public int Y
		{
			get
			{
				return m_y;
			}
			set
			{
				Dirty = true;
				m_y = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public int Z
		{
			get
			{
				return m_z;
			}
			set
			{
				Dirty = true;
				m_z = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public ushort Region
		{
			get
			{
				return m_region;
			}
			set
			{
				Dirty = true;
				m_region = value;
			}
		}
		[DataElement(AllowDbNull=true, Index=true)]
		public ushort Realm
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
		[DataElement(AllowDbNull=true)]
		public ushort Heading
		{
			get
			{
				return m_heading;
			}
			set
			{
				Dirty = true;
				m_heading = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public string ClassType
		{
			get
			{
				return m_classType;
			}
			set
			{
				Dirty = true;
				m_classType = value;
			}
		}
	}
}