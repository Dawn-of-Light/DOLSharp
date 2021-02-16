using DOL.Database;
using DOL.GS.PacketHandler;
using DOL.Language;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DOL.GS
{
    public class CraftingCacheManager
    {
		
		private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //All item in database should have id_nb tolower for not cast tolower (taking so much time...)
        /*All registered items should not contains any null.. or will not get added. 
        This permits craft system to be less attractive with database querys*/
        private static List<Tuple<DBCraftedItem, ItemTemplate>> craftedItemList = new List<Tuple<DBCraftedItem, ItemTemplate>>();
		public static List<Tuple<DBCraftedItem, ItemTemplate>> CraftedItemList { get => craftedItemList; set => craftedItemList = value; }

		private static List<DBCraftedXItem> craftedxItemList = new List<DBCraftedXItem>();
		public static List<DBCraftedXItem> CraftedxItemList { get => craftedxItemList; set => craftedxItemList = value; }

		private static object CacheLock = new object();
        public static bool m_reload = false;

		/// <summary>
        /// Empty all items into the CraftedItemCache
        /// </summary>
        public static bool Empty_Craft_Ram()
        {
            m_reload = true;
            lock (CacheLock)
            {
                CraftedItemList.Clear();
                CraftedxItemList.Clear();
            }
            m_reload = false;
            return true;
        }
		/// <summary>
		/// This function is called each time a player tries to make a item
		/// </summary>
		public static void CraftItem(ushort itemID, GamePlayer player)
		{
			int index = -1;
			bool updateMemory = false;
			DBCraftedItem recipe = null;

			if (!m_reload)
				index = CraftedItemList.FindIndex(a => a.Item1.CraftedItemID == itemID.ToString());
			if (index != -1)
				recipe = CraftedItemList[index].Item1;

			if (recipe == null)
				recipe = GameServer.Database.FindObjectByKey<DBCraftedItem>(itemID.ToString());
			if (recipe != null)
				//Here for register dbcrafteditem but if a rawmaterials is missing will not register any... break a craft flood databasse possibility
				updateMemory = true;

			if (recipe != null)
			{
				ItemTemplate itemToCraft = null;

				if (!m_reload && !updateMemory)
					itemToCraft = CraftedItemList[index].Item2;

				if (itemToCraft == null && updateMemory)
					itemToCraft = GameServer.Database.FindObjectByKey<ItemTemplate>(recipe.Id_nb);

				List<DBCraftedXItem> rawMaterials = new List<DBCraftedXItem>();
				bool ismissingrawmaterial = false;

				if (!m_reload && !updateMemory)
					rawMaterials = (from i in CraftedxItemList where i.CraftedItemId_nb == recipe.Id_nb select i).ToList();
				long totalprice = 0;
				if (rawMaterials.Count == 0 && updateMemory)
				{
					rawMaterials = GameServer.Database.SelectObjects<DBCraftedXItem>("`CraftedItemId_nb` = @CraftedItemId_nb", new QueryParameter("@CraftedItemId_nb", recipe.Id_nb)).ToList();
					foreach (DBCraftedXItem dbitem in rawMaterials)
					{
						if (dbitem == null)
							break;
						ItemTemplate ingredient = null;
						ingredient = GameServer.Database.FindObjectByKey<ItemTemplate>(dbitem.IngredientId_nb);
						if (ingredient == null)
						{
							log.Error("Missing raw materials for Craftitem= " + itemID + ". Missing is " + dbitem.IngredientId_nb);
							ismissingrawmaterial = true;
							break;
						}
						totalprice += ingredient.Price * dbitem.Count;
					}
					if (ismissingrawmaterial)
						rawMaterials.Clear();
				}
				if (rawMaterials.Count > 0)
				{
					if (itemToCraft != null)
					{
						if (updateMemory)
						{
							long pricetoset = Math.Abs(totalprice * 2 * ServerProperties.Properties.CRAFTING_SELLBACK_PERCENT / 100); // 95 % of crafting raw materials price is live values
							if (pricetoset > 0 && itemToCraft.Price != pricetoset)
							{
								itemToCraft.Price = pricetoset;
								itemToCraft.AllowUpdate = true;
								itemToCraft.Dirty = true;
								itemToCraft.Id_nb = itemToCraft.Id_nb.ToLower();
								if (GameServer.Database.SaveObject(itemToCraft))
								{
									if (ServerProperties.Properties.CRAFTING_MEMORY_DEBUG)
										log.Error("Craft: " + itemToCraft.Id_nb + " rawmaterials price= " + totalprice + ". Corrected price to= " + pricetoset);
								}
								else
								{
									if (ServerProperties.Properties.CRAFTING_MEMORY_DEBUG)
										log.Error("Craft: " + itemToCraft.Id_nb + " rawmaterials price= " + totalprice + ". Corrected price to= " + pricetoset + " Not Saved");
								}
								GameServer.Database.UpdateInCache<ItemTemplate>(itemToCraft.Id_nb);
								itemToCraft.Dirty = false;
								itemToCraft.AllowUpdate = false;
                                CraftedItemList.Add(new Tuple<DBCraftedItem, ItemTemplate>(recipe, itemToCraft));
							}
							else
                                CraftedItemList.Add(new Tuple<DBCraftedItem, ItemTemplate>(recipe, itemToCraft));

							CraftedxItemList.AddRange(rawMaterials);
						}
						AbstractCraftingSkill skill = CraftingMgr.getSkillbyEnum((eCraftingSkill)recipe.CraftingSkillType);
						if (skill != null)
							skill.CraftItem(player, recipe, itemToCraft, rawMaterials);
						else
							player.Out.SendMessage(LanguageMgr.GetTranslation(player.Client.Account.Language, "GamePlayer.CraftItem.DontHaveAbilityMake"), eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
					else
					{
						player.Out.SendMessage("Crafted ItemTemplate (" + recipe.Id_nb + ") not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
					}
				}
				else
				{
					player.Out.SendMessage("Craft recipe for (" + recipe.Id_nb + ") is missing raw materials!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
				}
			}
			else
			{
				player.Out.SendMessage("CraftedItemID: (" + itemID + ") not implemented yet.", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			}
		}
	}
}
