using DOL.Database;
using log4net;
using System;
using System.Collections.Generic;

namespace DOL.GS
{
    public class CraftedItemCache
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        //All item in database should have id_nb tolower for not cast tolower (taking so much time...)
        /*All registered items should not contains any null.. or will not get added. 
        This permits craft system to be less attractive with database querys*/
        public static List<Tuple<DBCraftedItem, ItemTemplate>> craftedItemList = new List<Tuple<DBCraftedItem, ItemTemplate>>();

        public static List<DBCraftedXItem> craftedxItemList = new List<DBCraftedXItem>();

        private static object CacheLock = new object();
        public static bool m_reload = false;
        /// <summary>
        /// Load or reload all items into the market cache
        /// </summary>
        public static bool Reload()
        {
            m_reload = true;
            lock (CacheLock)
            {
                craftedItemList.Clear();
                craftedxItemList.Clear();
            }
            m_reload = false;
            //Uncomment for test all and add to list at startup
            return true;// CraftedItemCache.Initialize();
        }
        /// <summary>
        /// Load or reload all items into the market cache
        /// </summary>
        public static bool Initialize()
        {
            lock (CacheLock)
            {
                //Uncomment for test all and add to list at startup
                /*log.Info("Building CraftedItem Cache ....");
                try
                {

                    List<DBCraftedItem> craftlist = new List<DBCraftedItem>(GameServer.Database.SelectAllObjects<DBCraftedItem>());

                    foreach (DBCraftedItem item in craftlist)
                    {
                        if (item == null)
                            break;
                        string itemid = item.Id_nb;
                        ItemTemplate crafteditemtemplate = null;
                        crafteditemtemplate = GameServer.Database.FindObjectByKey<ItemTemplate>(itemid);
                        int index = -1;
                        if (crafteditemtemplate != null)
                            index = craftedItemList.FindIndex(a => a.Item1 == item && a.Item2 == crafteditemtemplate);

                        if (index != -1)
                        {
                            log.Error("DBCraftedItem= " + item.Id_nb + " have twice record into database for realm " + item.Realm);
                            continue;
                        }

                        if (crafteditemtemplate != null)
                            craftedItemList.Add(new Tuple<DBCraftedItem, ItemTemplate>(item, crafteditemtemplate));
                        else
                            log.Error("crafted itemtemplate is null: " + item.Id_nb);
                    }

                    log.Info("Crafted Cache initialized with " + craftedItemList.Count + " items.");
                }
                catch (Exception e)
                {
                    log.Info(e.ToString());
                    m_reload = true;
                    return false;
                }

                log.Info("Building Craftedxitem Cache ....");
                try
                {

                    List<DBCraftedXItem> craftedlistxitem = new List<DBCraftedXItem>(GameServer.Database.SelectAllObjects<DBCraftedXItem>());
                    List<DBCraftedXItem> toaddlist = new List<DBCraftedXItem>();

                    foreach (Tuple<DBCraftedItem, ItemTemplate> tup in craftedItemList)
                    {
                        toaddlist = (from i in craftedlistxitem where i.CraftedItemId_nb == tup.Item2.Id_nb select i).ToList();
                        for (int i = 0; i < toaddlist.Count; i++)
                        {
                            ItemTemplate ingredient = null;
                            ingredient = GameServer.Database.FindObjectByKey<ItemTemplate>(toaddlist[i].IngredientId_nb);
                            if (ingredient == null)
                                log.Info("ingredients is null= " + toaddlist[i].IngredientId_nb + " for DBcraftedItem= " + tup.Item1.CraftedItemID);
                            else

                                craftedxItemList.Add(toaddlist[i]);
                        }
                    }

                    log.Info("Craftedxitem Cache initialized with " + craftedxItemList.Count + " items.");
                }
                catch (Exception e)
                {
                    log.Info(e.ToString());
                    m_reload = true;
                    return false;
                }
                m_reload = false;*/
                return true;
            }
        }
    }
}
