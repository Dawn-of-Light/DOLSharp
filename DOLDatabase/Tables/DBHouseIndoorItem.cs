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

namespace DOL
{
	namespace Database
	{
		[DataTable(TableName="DBIndoorItem")]
		public class DBHouseIndoorItem : DataObject
		{
			//important data
			private int m_housenumber;
			private int m_model;
			private int m_position;
			private int m_placemode;
			private int m_xpos;
			private int m_ypos;
			private string m_baseitemid;
			//"can-be-null" data (well, i dont know if size can be 0)
			private int m_color;
			private int m_emblem;
			private int m_rotation;
			private int m_size;

			public DBHouseIndoorItem()
			{
				AutoSave=false;
			}

			[DataElement(AllowDbNull=false, Index=true)]
			public int HouseNumber
			{
				get
				{
					return m_housenumber;
				}
				set
				{
					Dirty = true;
					m_housenumber = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Model
			{
				get
				{
					return m_model;
				}
				set
				{
					Dirty = true;
					m_model = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Position
			{
				get
				{
					return m_position;
				}
				set
				{
					Dirty = true;
					m_position = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Placemode
			{
				get
				{
					return m_placemode;
				}
				set
				{
					Dirty = true;
					m_placemode = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int X
			{
				get
				{
					return m_xpos;
				}
				set
				{
					Dirty = true;
					m_xpos = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public int Y
			{
				get
				{
					return m_ypos;
				}
				set
				{
					Dirty = true;
					m_ypos = value;
				}
			}
			[DataElement(AllowDbNull=false)]
			public string BaseItemID
			{
				get
				{
					return m_baseitemid;
				}
				set
				{
					Dirty = true;
					m_baseitemid = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Color
			{
				get
				{
					return m_color;
				}
				set
				{
					Dirty = true;
					m_color = value;
				}
			}
			[DataElement(AllowDbNull = true)]
			public int Emblem
			{
				get
				{
					return m_emblem;
				}
				set
				{
					Dirty = true;
					m_emblem = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Rotation
			{
				get
				{
					return m_rotation;
				}
				set
				{
					Dirty = true;
					m_rotation = value;
				}
			}
			[DataElement(AllowDbNull=true)]
			public int Size
			{
				get
				{
					return m_size;
				}
				set
				{
					Dirty = true;
					m_size = value;
				}
			}
		}
	}
}
