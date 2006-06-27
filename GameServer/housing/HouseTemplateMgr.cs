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
using DOL.GS.Database;
using NHibernate.Expression;

namespace DOL.GS.Housing
{
	public sealed class HouseTemplateMgr
	{
		public static void Initialize()
		{
			CheckItemTemplates();
			CheckMerchantItemTemplates();
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
			switch (marker.RegionId)
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

		static void LoadItemLists()
		{
			AlbionLotMarkerItems = new MerchantTradeItems("alb_lotmarker");
			MidgardLotMarkerItems = new MerchantTradeItems("mid_lotmarker");
			HiberniaLotMarkerItems = new MerchantTradeItems("hib_lotmarker");

			IndoorMenuItems = new MerchantTradeItems("housing_indoor_menu");
			IndoorShopItems = new MerchantTradeItems("housing_indoor_shop");
			OutdoorMenuItems = new MerchantTradeItems("housing_outdoor_menu");
			OutdoorShopItems = new MerchantTradeItems("housing_outdoor_shop");
		}

		static void CheckItemTemplates()
		{
			CheckItemTemplate("Albion cottage deed", "alb_cottage_deed", 499, 0, 10000000, 0, 0, 0, 0);
			CheckItemTemplate("Albion house deed", "alb_house_deed", 499, 0, 100000000, 0, 0, 0, 0);
			CheckItemTemplate("Albion villa deed", "alb_villa_deed", 499, 0, 400000000, 0, 0, 0, 0);
			CheckItemTemplate("Albion mansion deed", "alb_mansion_deed", 499, 0, 1000000000, 0, 0, 0, 0);
			CheckItemTemplate("Midgard cottage deed", "mid_cottage_deed", 499, 0, 10000000, 0, 0, 0, 0);
			CheckItemTemplate("Midgard house deed", "mid_house_deed", 499, 0, 100000000, 0, 0, 0, 0);
			CheckItemTemplate("Midgard villa deed", "mid_villa_deed", 499, 0, 400000000, 0, 0, 0, 0);
			CheckItemTemplate("Midgard mansion deed", "mid_mansion_deed", 499, 0, 1000000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia cottage deed", "hib_cottage_deed", 499, 0, 10000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia house deed", "hib_house_deed", 499, 0, 100000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia villa deed", "hib_villa_deed", 499, 0, 400000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia mansion deed", "hib_mansion_deed", 499, 0, 1000000000, 0, 0, 0, 0);
			CheckItemTemplate("Porch deed", "porch_deed", 499, 0, 5000000, 0, 0, 0, 0);
			CheckItemTemplate("Porch remove deed", "porch_remove_deed", 499, 0, 100, 0, 0, 0, 0);
		}

		static void CheckMerchantItemTemplates()
		{
			string[] alblotmarkeritems = {"alb_cottage_deed", "alb_house_deed", "alb_villa_deed", "alb_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("alb_lotmarker", alblotmarkeritems);
			string[] midlotmarkeritems = {"mid_cottage_deed", "mid_house_deed", "mid_villa_deed", "mid_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("mid_lotmarker", midlotmarkeritems);
			string[] hiblotmarkeritems = {"hib_cottage_deed", "hib_house_deed", "hib_villa_deed", "hib_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("hib_lotmarker", hiblotmarkeritems);
		}

		static void CheckMerchantItems(string merchantid, string[] itemids)
		{
			IList merchantitems = GameServer.Database.SelectObjects(typeof (MerchantItem), Expression.Eq("ItemListID", merchantid));
			int slot = 0;
			foreach (string itemid in itemids)
			{
				bool found = false;
				foreach (MerchantItem dbitem in merchantitems)
				{
					if (dbitem.ItemTemplateID == itemid)
					{
						found = true;
						break;
					}
					slot += 1;
				}
				if (!found)
				{
					MerchantItem newitem = new MerchantItem();
					newitem.ItemListID = merchantid;
					newitem.ItemTemplateID = itemid;
					newitem.SlotPosition = (slot%30);
					newitem.PageNumber = (slot/30);
					GameServer.Database.AddNewObject(newitem);
					slot += 1;
				}
			}
		}

		static void CheckItemTemplate(string name, string id, int model, int objtype, int copper, int dps, int spd, int hand, int weight)
		{
            //TODO convert this method
            /*
			SwordTemplate templateitem = (SwordTemplate) GameServer.Database.FindObjectByKey(typeof (SwordTemplate), id);
			if (templateitem == null)
			{
				templateitem = new SwordTemplate();
				templateitem.ItemTemplateID = id;
				templateitem.Name = name;
				templateitem.Model = model;
				templateitem.Level = 0;
				templateitem.IsDropable = true;
                templateitem.IsSaleable = true;
                templateitem.IsTradable = true;
                templateitem.DamagePerSecond = (byte)dps;
                templateitem.Speed = spd;
                templateitem.HandNeeded = (eHandNeeded)hand;
				templateitem.Weight = weight;
                templateitem.Value = copper;
				GameServer.Database.AddNewObject(templateitem);
			}
             */
		}
	}
}