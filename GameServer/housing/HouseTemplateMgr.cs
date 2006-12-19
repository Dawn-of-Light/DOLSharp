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

		static void CheckItemTemplates()
		{
			//lot marker
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

			//indoor npc
			CheckItemTemplate("Hastener", "hastener", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Smith", "smith", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Enchanter", "enchanter", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Emblemeer", "emblemeer", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Healer", "healer", 593, (int)eObjectType.HouseNPC, 30000000, 0, 0, 0, 0);
			CheckItemTemplate("Recharger", "recharger", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia Teleporter", "hib_teleporter", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Apprentice Merchant", "apprentice_merchant", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Grandmaster Merchant", "grandmaster_merchant", 593, (int)eObjectType.HouseNPC, 5000000, 0, 0, 0, 0);
			CheckItemTemplate("Incantation Merchant", "incantation_merchant", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Poison and Dye Supplies", "poison_dye_supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Potion, Tincture, and Enchantment Supplies", "potion_tincture_enchantment_supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Poison and Potion Supplies", "poison_potion_supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Dye, Tincture, and Enchantment Supplies", "dye_tincture_enchantment supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Taxidermy Supplies", "taxidermy_supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Siegecraft Supplies", "siegecraft_supplies", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Hibernia Vault Keeper", "hib_vault_keeper", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);
			CheckItemTemplate("Dye Supply Master", "dye_supply_master", 593, (int)eObjectType.HouseNPC, 1000000, 0, 0, 0, 0);

			//indoor craft
			CheckItemTemplate("alchemy table", "alchemy_table", 1494, (int)eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0);
			CheckItemTemplate("forge", "forge", 1495, (int)eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0);
			CheckItemTemplate("lathe", "lathe", 1496, (int)eObjectType.HouseInteriorObject, 10000000, 0, 0, 0, 0);

			//indoor bindstone
			//alb 1488 mid 1492
			CheckItemTemplate("Hibernia bindstone", "hib_bindstone", 1490, (int)eObjectType.HouseBindstone, 10000000, 0, 0, 0, 0);

			//indoor vault
			//alb 1489 mid 1493
			CheckItemTemplate("Hibernia vault", "hib_vault", 1491, (int)eObjectType.HouseVault, 10000000, 0, 0, 0, 0);
		}

		static void CheckMerchantItemTemplates()
		{
			//lot markers
			string[] alblotmarkeritems = {"alb_cottage_deed", "alb_house_deed", "alb_villa_deed", "alb_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("alb_lotmarker", alblotmarkeritems);
			string[] midlotmarkeritems = {"mid_cottage_deed", "mid_house_deed", "mid_villa_deed", "mid_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("mid_lotmarker", midlotmarkeritems);
			string[] hiblotmarkeritems = {"hib_cottage_deed", "hib_house_deed", "hib_villa_deed", "hib_mansion_deed", "porch_deed", "porch_remove_deed"};
			CheckMerchantItems("hib_lotmarker", hiblotmarkeritems);

			//hookpoints
			string[] indoornpc = { "hastener", "smith", "enchanter", "emblemeer", "healer", "recharger", "hib_teleporter", "apprentice_merchant", "grandmaster_merchant", "incantation_merchant", "poison_dye_supplies", "potion_tincture_enchantment_supplies", "poison_potion_supplies", "taxidermy_supplies", "siegecraft_supplies", "hib_vault_keeper", "dye_supply_master" };
			CheckMerchantItems("housing_indoor_npc", indoornpc);
			string[] indoorbindstone = { "hib_bindstone" };
			CheckMerchantItems("housing_indoor_bindstone", indoorbindstone);
			string[] indoorcraft = { "alchemy_table", "forge", "lathe" };
			CheckMerchantItems("housing_indoor_craft", indoorcraft);
			string[] indoorvault = { "hib_vault" };
			CheckMerchantItems("housing_indoor_vault", indoorvault);
		}

		static void CheckMerchantItems(string merchantid, string[] itemids)
		{
			DataObject[] merchantitems = GameServer.Database.SelectObjects(typeof (MerchantItem), "ItemListID=\'" + GameServer.Database.Escape(merchantid) + "\'");
			int slot = 0;
			foreach (string itemid in itemids)
			{
				bool found = false;
				foreach (DataObject dbitem in merchantitems)
				{
					if (((MerchantItem) dbitem).ItemTemplateID == itemid)
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
			ItemTemplate templateitem = (ItemTemplate) GameServer.Database.FindObjectByKey(typeof (ItemTemplate), GameServer.Database.Escape(id));
			if (templateitem == null)
			{
				templateitem = new ItemTemplate();
				templateitem.Name = name;
				templateitem.Model = model;
				templateitem.Level = 0;
				templateitem.Object_Type = objtype;
				templateitem.Id_nb = id;
				templateitem.IsPickable = true;
				templateitem.IsDropable = true;
				templateitem.DPS_AF = dps;
				templateitem.SPD_ABS = spd;
				templateitem.Hand = 0x0E;
				templateitem.Weight = weight;
				templateitem.Copper = (byte) Money.GetCopper(copper);
				templateitem.Silver = (byte) Money.GetSilver(copper);
				templateitem.Gold = (short) Money.GetGold(copper);
                templateitem.Platinum = (short)Money.GetPlatinum(copper);
				GameServer.Database.AddNewObject(templateitem);
			}
		}
	}
}