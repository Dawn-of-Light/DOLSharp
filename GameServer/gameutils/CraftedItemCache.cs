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
        private static List<Tuple<DBCraftedItem, ItemTemplate>> craftedItemList = new List<Tuple<DBCraftedItem, ItemTemplate>>();

        private static List<DBCraftedXItem> craftedxItemList = new List<DBCraftedXItem>();

        private static object CacheLock = new object();
        public static bool m_reload = false;

        public static List<Tuple<DBCraftedItem, ItemTemplate>> CraftedItemList { get => craftedItemList; set => craftedItemList = value; }
        public static List<DBCraftedXItem> CraftedxItemList { get => craftedxItemList; set => craftedxItemList = value; }

        /// <summary>
        /// Empty all items into the CraftedItemCache
        /// </summary>
        public static bool Reload()
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
    }
}
