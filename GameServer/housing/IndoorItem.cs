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
using System.Collections;

using DOL.Database2; //yeah for the DBIndoorItem class!

namespace DOL.GS.Housing
{
	public class IndoorItem
	{
		int m_model;
		public int Model
		{
			get { return m_model; }
			set { m_model= value; }
		}
		int m_color;
		public int Color
		{
			get { return m_color; }
			set { m_color = value;}
		}
		short m_x;
		public short X
		{
			get { return m_x; }
			set { m_x = value;}
		}
		short m_y;
		public short Y
		{
			get { return m_y; }
			set { m_y = value;}
		}
		int m_rotation;
		public int Rotation
		{
			get { return m_rotation; }
			set { m_rotation = value;}
		}
		int m_size;
		public int Size
		{
			get { return m_size; }
			set { m_size = value;}
		}
		int m_position;
		public int Position
		{
			get { return m_position; }
			set { m_position = value;}
		}
		int m_placemode;
		public int Placemode
		{
			get { return m_placemode;  }
			set { m_placemode = value; }
		}
		ItemTemplate m_baseitem;
		public ItemTemplate BaseItem
		{
			get { return m_baseitem; }
			set { m_baseitem = value;}			
		}
		DBHouseIndoorItem m_databaseitem;
		public DBHouseIndoorItem DatabaseItem
		{
			get { return m_databaseitem; }
			set { m_databaseitem=value;  }
		}
		public void CopyFrom(DBHouseIndoorItem dbitem)
		{
			this.Model = dbitem.Model;
			this.Color = dbitem.Color;
			this.X = (short)dbitem.X;
			this.Y = (short)dbitem.Y;
			this.Rotation = dbitem.Rotation;
			this.Size = dbitem.Size;
			this.Position = dbitem.Position;
			this.Placemode = dbitem.Placemode;
			this.BaseItem = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromIDnb(typeof(ItemTemplate), dbitem.BaseItemID);
			this.DatabaseItem = dbitem;
		}

		public DBHouseIndoorItem CreateDBIndoorItem(int HouseNumber)
		{
			DBHouseIndoorItem dbitem = new DBHouseIndoorItem();
			dbitem.HouseNumber = HouseNumber;
			dbitem.Model = Model;
			dbitem.Position = Position;
			dbitem.Placemode = Placemode;
			dbitem.X = X;
			dbitem.Y = Y;
			if (BaseItem != null)
			{
				dbitem.BaseItemID = BaseItem.Id_nb;
			}
			else
			{
				dbitem.BaseItemID = "null";
			}
			dbitem.Color = Color;
			dbitem.Rotation = Rotation;
			dbitem.Size = Size;
			return dbitem;
		}
	}
}
