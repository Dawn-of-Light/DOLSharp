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
	[DataTable(TableName="RegionDB")]
	public class RegionDB : DataObject
	{
		static bool		m_autoSave;
		private string	m_name;
		private string	m_description;
		private ushort	m_regionID;
		private int		m_waterLevel;
		private int		m_port;
		private string 	m_regionIP;
		private int		m_expansion;
		private bool	m_isDivingEnabled;
		private bool	m_isHousingEnabled;
	//	private bool	m_tutorial;
		private bool	m_instance;

		public RegionDB()
		{
			m_autoSave=false;
			m_regionIP = "127.0.0.1";
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
		[DataElement(AllowDbNull = false)]
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
		[DataElement(AllowDbNull = true)]
		public int WaterLevel
		{
			get
			{
				return m_waterLevel;
			}
			set
			{
				Dirty = true;
				m_waterLevel = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int Port
		{
			get
			{
				return m_port;
			}
			set
			{
				Dirty = true;
				m_port = value;
			}
		}
		[DataElement(AllowDbNull = false)]
		public string	RegionIP
		{
			get
			{
				return m_regionIP;
			}
			set
			{
				Dirty = true;
				m_regionIP = value;
			}
		}
		[DataElement(AllowDbNull = true)]
		public int	Expansion
		{
			get
			{
				return m_expansion;
			}
			set
			{
				Dirty = true;
				m_expansion = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public bool IsDivingEnabled
		{
			get
			{
				return m_isDivingEnabled;
			}
			set
			{
				Dirty = true;
				m_isDivingEnabled = value;
			}
		}
		[DataElement(AllowDbNull=true)]
		public bool IsHousingEnabled
		{
			get
			{
				return m_isHousingEnabled;
			}
			set
			{	
				Dirty = true;
				m_isHousingEnabled = false;
			}
		}
	/*	[DataElement(AllowDbNull=true)]
		public bool Tutorial
		{
			get
			{
				return m_tutorial;
			}
			set
			{
				Dirty = true;
				m_tutorial = value;
			}
		}	   */
		[DataElement(AllowDbNull=true)]
		public bool Instance
		{
			get
			{
				return m_instance;
			}
			set
			{
				Dirty = true;
				m_instance = value;
			}
		}
	}
}