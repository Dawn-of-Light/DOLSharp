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
	[DataTable(TableName="ZoneDB")]
	public class ZoneDB : DataObject
	{
		static bool		m_autoSave;
		private string	m_description;
		private ushort	m_zoneID;
		private int	m_offsetx;
		private int	m_offsety;
		private int	m_height;
		private int	m_width;
		private ushort	m_regionID;

		public ZoneDB()
		{
			m_autoSave=false;
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
		[DataElement(AllowDbNull=false)]
		public string Description
		{
			get
			{
				return m_description;
			}
			set
			{
				Dirty = true;
				m_description = value;
			}
		}
		[DataElement(AllowDbNull=false, Index=true)]
		public ushort ZoneID
		{
			get
			{
				return m_zoneID;
			}
			set
			{
				Dirty = true;
				m_zoneID = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int OffsetX
		{
			get
			{
				return m_offsetx;
			}
			set
			{
				Dirty = true;
				m_offsetx = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int OffsetY
		{
			get
			{
				return m_offsety;
			}
			set
			{
				Dirty = true;
				m_offsety = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Height
		{
			get
			{
				return m_height;
			}
			set
			{
				Dirty = true;
				m_height = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int	Width
		{
			get
			{
				return m_width;
			}
			set
			{
				Dirty = true;
				m_width = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public ushort RegionID
		{
			get
			{
				return m_regionID;
			}
			set
			{
				Dirty = true;
				m_regionID = value;
			}
		}
	}
}