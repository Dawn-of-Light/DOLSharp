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
using System.Collections;
using DOL.Database;

namespace DOL.GS.Housing
{
	public sealed class HouseTemplateMgr
	{
		public static void Initialize()
		{
			LoadItemLists();
		}

		public static int LOT_PRICE_PER_HOUR = (int) (1.2*1000*100*100); //1.2p
		public static int LOT_PRICE_MINIMUM = (300*100*100); //300g
		public static int LOT_PRICE_START = (95*1000*100*100); //95p

		public static int GetLotPrice(DBHouse house)
		{
			TimeSpan diff = (DateTime.Now - house.CreationTime);
			int price = LOT_PRICE_START - (int)(diff.TotalHours*LOT_PRICE_PER_HOUR);
			if (price < LOT_PRICE_MINIMUM)
			{
				return LOT_PRICE_MINIMUM;
			}
			return price;
		}

		public static MerchantTradeItems GetLotMarkerItems(GameLotMarker marker)
		{
			switch (marker.CurrentRegionID)
			{
				case 2:
					return AlbionLotMarkerItems;
				case 102:
					return MidgardLotMarkerItems;
				default:
					return HiberniaLotMarkerItems;
			}
		}

		public static MerchantTradeItems AlbionLotMarkerItems;
		public static MerchantTradeItems MidgardLotMarkerItems;
		public static MerchantTradeItems HiberniaLotMarkerItems;

		public static MerchantTradeItems IndoorMenuItems;
		public static MerchantTradeItems IndoorShopItems;
		public static MerchantTradeItems OutdoorMenuItems;
		public static MerchantTradeItems OutdoorShopItems;

		public static MerchantTradeItems IndoorNPCMenuItems;
		public static MerchantTradeItems IndoorVaultMenuItems;
		public static MerchantTradeItems IndoorCraftMenuItems;
		public static MerchantTradeItems IndoorBindstoneMenuItems;

		static void LoadItemLists()
		{
			AlbionLotMarkerItems = new MerchantTradeItems("alb_lotmarker");
			MidgardLotMarkerItems = new MerchantTradeItems("mid_lotmarker");
			HiberniaLotMarkerItems = new MerchantTradeItems("hib_lotmarker");

			IndoorMenuItems = new MerchantTradeItems("housing_indoor_menu");
			IndoorShopItems = new MerchantTradeItems("housing_indoor_shop");
			OutdoorMenuItems = new MerchantTradeItems("housing_outdoor_menu");
			OutdoorShopItems = new MerchantTradeItems("housing_outdoor_shop");

			IndoorNPCMenuItems = new MerchantTradeItems("housing_indoor_npc");
			IndoorVaultMenuItems = new MerchantTradeItems("housing_indoor_vault");
			IndoorCraftMenuItems = new MerchantTradeItems("housing_indoor_craft");
			IndoorBindstoneMenuItems = new MerchantTradeItems("housing_indoor_bindstone");
		}
	}
}
