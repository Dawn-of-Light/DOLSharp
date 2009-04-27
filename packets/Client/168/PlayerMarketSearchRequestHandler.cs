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
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using DOL.Database;
using DOL.GS.Housing;
using System.Text;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, 0x11, "Handles player market search")]
    public class PlayerMarketSearchRequestHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const string EXPLORER_LIST = "explorerList";
        public List<InventoryItem> ExplorerItems()
        {
            List<InventoryItem> list = new List<InventoryItem>();
            DataObject[] obj = GameServer.Database.SelectObjects(typeof(InventoryItem), "SlotPosition > '1499'");
            foreach (InventoryItem itm in obj)
            {
                list.Add(itm);
            }
            return list;
        }


        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client.Player == null)
                return 0;

            string filter = packet.ReadString(64);
            int slot = (int)packet.ReadInt();
            int skill = (int)packet.ReadInt();
            int resist = (int)packet.ReadInt();
            int bonus = (int)packet.ReadInt();
            int hp = (int)packet.ReadInt();
            int power = (int)packet.ReadInt();
            int proc = (int)packet.ReadInt();
            int qtyMin = (int)packet.ReadInt();
            int qtyMax = (int)packet.ReadInt();
            int levelMin = (int)packet.ReadInt();
            int levelMax = (int)packet.ReadInt();
            int priceMin = (int)packet.ReadInt();
            int priceMax = (int)packet.ReadInt();
            int visual = (int)packet.ReadInt();
            byte page = (byte)packet.ReadByte();
            byte unk1 = (byte)packet.ReadByte();
            short unk2 = (short)packet.ReadShort();

            int requestedPage = (int)page;

            int firstSlot = 0 + (requestedPage * 20);
            int lastSlot = 19 + (requestedPage * 20);

            List<InventoryItem> list = new List<InventoryItem>();
            List<InventoryItem> ausgabe = new List<InventoryItem>();
            StringBuilder sql = new StringBuilder();

            sql.Append("SlotPosition > '1499'");

            if (filter != null && filter != "")
                sql.Append(" AND Name LIKE '%" + filter + "%'");
            #region slot
            if (slot != -1)
            {
                switch (slot)
                {
                    case 0:
                        sql.Append(" AND Item_Type = '22'");
                        break;
                    case 1:
                        sql.Append(" AND Item_Type = '23'");
                        break;
                    case 2:
                        sql.Append(" AND Item_Type = '21'");
                        break;
                    case 3:
                        sql.Append(" AND Item_Type = '28'");
                        break;
                    case 4:
                        sql.Append(" AND Item_Type = '27'");
                        break;
                    case 5:
                        sql.Append(" AND Item_Type = '25'");
                        break;
                    case 6:
                        sql.Append(" AND Item_Type IN (35, 36)");
                        break;
                    case 7:
                        sql.Append(" AND Item_Type IN (33, 34)");
                        break;
                    case 8:
                        sql.Append(" AND Item_Type = '32'");
                        break;
                    case 9:
                        sql.Append(" AND Item_Type = '29'");
                        break;
                    case 10:
                        sql.Append(" AND Item_Type = '26'");
                        break;
                    case 11:
                        sql.Append(" AND Item_Type = '24'");
                        break;
                    case 12:
                        sql.Append(" AND Item_Type IN (10, 11)");
                        break;
                    case 13:
                        sql.Append(" AND Object_Type = '42'");
                        break;
                    case 14:
                        sql.Append(" AND Item_Type = '12'");
                        break;
                    case 15:
                        sql.Append(" AND Item_Type = '13'");
                        break;
                    case 16:
                        sql.Append(" AND Item_Type = '11'");
                        break;
                    case 17:
                        sql.Append(" AND Object_Type = '45'");
                        break;
                    case 18:
                        sql.Append(" AND (Item_Type = '0' OR Object_Type = '0')");
                        break;
                }
            }
            #endregion
            string qryString = sql.ToString();

            DataObject[] obj = GameServer.Database.SelectObjects(typeof(InventoryItem), qryString);

            foreach (InventoryItem item in obj)
            {
                list.Add(item);
            }
            int itemCount = list.Count;
            int pageCount = (int)Math.Ceiling((double)itemCount / 20) - 1;
            lock (list)
            {
                foreach (InventoryItem i in list)
                {
                    if (list.IndexOf(i) >= firstSlot && list.IndexOf(i) <= lastSlot)
                        ausgabe.Add(i);
                }
            }
            int itemsOnPage = ausgabe.Count;
            if (itemsOnPage > 0)
            {
                client.Player.Out.SendMarketExplorerWindow(ausgabe, page, (byte)pageCount);
                client.Player.Out.SendMessage(itemsOnPage.ToString() + " Results found for page " + (page + 1) + ".", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                client.Player.TempProperties.removeProperty(EXPLORER_LIST);
                client.Player.TempProperties.setProperty(EXPLORER_LIST, ausgabe);
            }
            else
                client.Player.Out.SendMessage("No Items found", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
            return 1;
        }
    }
}