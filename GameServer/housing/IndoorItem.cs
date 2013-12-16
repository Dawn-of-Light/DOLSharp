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

using DOL.Database;

//yeah for the DBIndoorItem class!

namespace DOL.GS.Housing
{
	public class IndoorItem
	{
		public IndoorItem()
		{}

		public IndoorItem(DBHouseIndoorItem dbitem)
		{
			Model = dbitem.Model;
			Color = dbitem.Color;
			X = (short) dbitem.X;
			Y = (short) dbitem.Y;
			Rotation = dbitem.Rotation;
			Size = dbitem.Size;
			Emblem = dbitem.Emblem;
			Position = dbitem.Position;
			PlacementMode = dbitem.Placemode;
			BaseItem = GameServer.Database.FindObjectByKey<ItemTemplate>(dbitem.BaseItemID);
			DatabaseItem = dbitem;
		}

		public int Model { get; set; }

		public int Color { get; set; }

		public short X { get; set; }

		public short Y { get; set; }

		public int Rotation { get; set; }

		public int Size { get; set; }

		public int Emblem { get; set; }

		public int Position { get; set; }

		public int PlacementMode { get; set; }

		public ItemTemplate BaseItem { get; set; }

		public DBHouseIndoorItem DatabaseItem { get; set; }

		public DBHouseIndoorItem CreateDBIndoorItem(int houseNumber)
		{
			var dbitem = new DBHouseIndoorItem
			             	{
			             		HouseNumber = houseNumber,
			             		Model = Model,
			             		Position = Position,
			             		Placemode = PlacementMode,
			             		X = X,
			             		Y = Y
			             	};

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