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

//yeah for the DBOutdoorItem class!

namespace DOL.GS.Housing
{
	public class OutdoorItem
	{
		public OutdoorItem()
		{}

		public OutdoorItem(DBHouseOutdoorItem dbitem)
		{
			Model = dbitem.Model;
			Position = dbitem.Position;
			Rotation = dbitem.Rotation;
			BaseItem = GameServer.Database.FindObjectByKey<ItemTemplate>(dbitem.BaseItemID);
			DatabaseItem = dbitem;
		}

		public int Model { get; set; }

		public int Position { get; set; }

		public int Rotation { get; set; }

		public ItemTemplate BaseItem { get; set; }

		public DBHouseOutdoorItem DatabaseItem { get; set; }

		public DBHouseOutdoorItem CreateDBOutdoorItem(int houseNumber)
		{
			var dbitem = new DBHouseOutdoorItem
			             	{
			             		HouseNumber = houseNumber,
			             		Model = Model,
			             		Position = Position,
			             		BaseItemID = BaseItem.Id_nb,
			             		Rotation = Rotation
			             	};

			return dbitem;
		}
	}
}