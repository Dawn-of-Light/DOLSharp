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

using DOL.Database2; //yeah for the DBOutdoorItem class!

namespace DOL.GS.Housing
{
	public class OutdoorItem
	{
		int m_model;
		public int Model
		{
			get { return m_model; }
			set { m_model= value; }
		}
		int m_position;
		public int Position
		{
			get { return m_position; }
			set { m_position = value;}
		}
		int m_rotation;
		public int Rotation
		{
			get { return m_rotation; }
			set { m_rotation = value;}
		}
		ItemTemplate m_baseitem;
		public ItemTemplate BaseItem
		{
			get { return m_baseitem; }
			set { m_baseitem = value;}			
		}
		DBHouseOutdoorItem m_databaseitem;
		public DBHouseOutdoorItem DatabaseItem
		{
			get { return m_databaseitem; }
			set { m_databaseitem=value;  }
		}
		public void CopyFrom(DBHouseOutdoorItem dbitem)
		{
			this.Model = dbitem.Model;
			this.Position = dbitem.Position;
			this.Rotation = dbitem.Rotation;
			this.BaseItem = (ItemTemplate)GameServer.Database.GetDatabaseObjectFromID(dbitem.BaseItemID);
			this.DatabaseItem = dbitem;
		}

		public DBHouseOutdoorItem CreateDBOutdoorItem(int HouseNumber)
		{
			DBHouseOutdoorItem dbitem = new DBHouseOutdoorItem();
			dbitem.HouseNumber = HouseNumber;
			dbitem.Model = Model;
			dbitem.Position = Position;
			dbitem.BaseItemID = BaseItem.ID;
			dbitem.Rotation = Rotation;
			return dbitem;
		}
	}
}
