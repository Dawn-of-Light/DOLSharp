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
	[DataTable(TableName = "house_hookpointoffset")]
	public class HouseHookpointOffset : DataObject
	{
		private int m_model;
		private int m_hookpoint;
		private int m_offX;
		private int m_offY;
		private int m_offZ;
		private int m_offH;

		public HouseHookpointOffset()
		{
			AutoSave = false;
		}

		[DataElement(AllowDbNull = false)]
		public int Model
		{
			get { return m_model; }
			set
			{
				Dirty = true;
				m_model = value;
			}
		}

		[DataElement(AllowDbNull = false)]
		public int Hookpoint
		{
			get { return m_hookpoint; }
			set
			{
				Dirty = true;
				m_hookpoint = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int OffX
		{
			get { return m_offX; }
			set
			{
				Dirty = true;
				m_offX = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int OffY
		{
			get { return m_offY; }
			set
			{
				Dirty = true;
				m_offY = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int OffZ
		{
			get { return m_offZ; }
			set
			{
				Dirty = true;
				m_offZ = value;
			}
		}

		[DataElement(AllowDbNull = true)]
		public int OffH
		{
			get { return m_offH; }
			set
			{
				Dirty = true;
				m_offH = value;
			}
		}
	}
}
