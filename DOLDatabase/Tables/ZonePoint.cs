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
		private int	m_targetX;
		private int	m_targetY;
		private int	m_targetZ;
		private ushort	m_targetRegion;
		private ushort	m_realm;
		private ushort	m_targetHeading;
		private int m_sourceX;
		private int m_sourceY;
		private int m_sourceZ;
		private ushort m_sourceRegion;
		private string	m_classType = "";

		public ZonePoint()
		{
			AllowAdd=false;
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
		public int TargetX
		{
			get
			{
				return m_targetX;
			}
			set
			{
				Dirty = true;
				m_targetX = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int TargetY
		{
			get
			{
				return m_targetY;
			}
			set
			{
				Dirty = true;
				m_targetY = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public int TargetZ
		{
			get
			{
				return m_targetZ;
			}
			set
			{
				Dirty = true;
				m_targetZ = value;
			}
		}

		[DataElement(AllowDbNull=true)]
		public ushort TargetRegion
		{
			get
			{
				return m_targetRegion;
			}
			set
			{
				Dirty = true;
				m_targetRegion = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public ushort TargetHeading
		{
			get
			{
				return m_targetHeading;
			}
			set
			{
				Dirty = true;
				m_targetHeading = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SourceX
		{
			get
			{
				return m_sourceX;
			}
			set
			{
				Dirty = true;
				m_sourceX = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SourceY
		{
			get
			{
				return m_sourceY;
			}
			set
			{
				Dirty = true;
				m_sourceY = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int SourceZ
		{
			get
			{
				return m_sourceZ;
			}
			set
			{
				Dirty = true;
				m_sourceZ = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public ushort SourceRegion
		{
			get
			{
				return m_sourceRegion;
			}
			set
			{
				Dirty = true;
				m_sourceRegion = value;
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