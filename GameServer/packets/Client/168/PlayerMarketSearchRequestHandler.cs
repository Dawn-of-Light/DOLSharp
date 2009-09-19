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
			if(client.Version >= GameClient.eClientVersion.Version198)
			{
				// Dunnerholl 2009-07-28 Version 1.98 introduced new options to Market search. 12 Bytes were added, but only 7 are in usage so far in my findings.
				// update this, when packets change and keep in mind, that this code reflects only the 1.98 changes
				byte armorType = page; // page is now used for the armorType (still has to be logged, i just checked that 2 means leather, 0 = standard
				byte damageType = (byte)packet.ReadByte(); // 1=crush, 2=slash, 3=thrust
				// 3 bytes unused
				packet.Skip(3);
				byte playerCrafted = (byte)packet.ReadByte(); // 1 = show only Player crafted, 0 = all
				// 3 bytes unused
				packet.Skip(3);
				page = (byte)packet.ReadByte(); // page is now sent here
				byte unknown = (byte)packet.ReadByte(); // always been 0xE5, if u page it is 0x4B, tested on alb only
				byte unknown2 = (byte)packet.ReadByte(); //always been 0x12, if u page it is 0x7C, tested on alb only
				byte unknown3 = (byte)packet.ReadByte(); //always been 0x00, if u page it is 0x1B, tested on alb only
			}
            int requestedPage = (int)page;

            int firstSlot = 0 + (requestedPage * 20);
            int lastSlot = 19 + (requestedPage * 20);

            StringBuilder sql = new StringBuilder();

            sql.Append("SlotPosition > '1499'");

            if (filter != null && filter != "")
                sql.Append(" AND Name LIKE '%" + filter + "%'");


			#region Slot
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

			#region Bonus
			if (bonus > 0)
				sql.Append(" AND (Bonus >= '" + bonus + "')");
			#endregion

			#region Price
			if (priceMax > 0 && priceMin < priceMax)
				sql.Append(" AND (SellPrice >= '" + priceMin + "' AND SellPrice <= '" + priceMax + "')");
			#endregion

			#region Level
			if (levelMax > 0 && levelMin < levelMax)
				sql.Append(" AND (Level >= '" + levelMin + "' AND Level <= '" + levelMax + "')");
			#endregion

			#region Visual Effect
			if (visual > 0)
				sql.Append(" AND (Effect > '0')");
			#endregion

			#region Skill
			if (skill > 0)
			sql.Append(" AND (Bonus1Type = '" + skill + "' OR " +
								"Bonus2Type = '" + skill + "' OR " +
								"Bonus3Type = '" + skill + "' OR " +
								"Bonus4Type = '" + skill + "' OR " +
								"Bonus5Type = '" + skill + "' OR " +
								"Bonus6Type = '" + skill + "' OR " +
								"Bonus7Type = '" + skill + "' OR " +
								"Bonus8Type = '" + skill + "' OR " +
								"Bonus9Type = '" + skill + "' OR " +
								"Bonus10Type = '" + skill + "' OR " +
								"ExtraBonusType = '" + skill + "')");
			#endregion

			#region Resist
			if(resist > 0)
			sql.Append(" AND (Bonus1Type = '" + resist + "' OR " +
								"Bonus2Type = '" + resist + "' OR " +
								"Bonus3Type = '" + resist + "' OR " +
								"Bonus4Type = '" + resist + "' OR " +
								"Bonus5Type = '" + resist + "' OR " +
								"Bonus6Type = '" + resist + "' OR " +
								"Bonus7Type = '" + resist + "' OR " +
								"Bonus8Type = '" + resist + "' OR " +
								"Bonus9Type = '" + resist + "' OR " +
								"Bonus10Type = '" + resist + "' OR " +
								"ExtraBonusType = '" + resist + "')");
			#endregion

			#region Health
			if(hp > 0)
			sql.Append(" AND (Bonus1Type = '" + eProperty.MaxHealth + "' AND Bonus1 >= '" + hp + "' OR " +
								"Bonus2Type = '" + eProperty.MaxHealth + "' AND Bonus2 >= '" + hp + "' OR " +
								"Bonus3Type = '" + eProperty.MaxHealth + "' AND Bonus3 >= '" + hp + "' OR " +
								"Bonus4Type = '" + eProperty.MaxHealth + "' AND Bonus4 >= '" + hp + "' OR " +
								"Bonus5Type = '" + eProperty.MaxHealth + "' AND Bonus5 >= '" + hp + "' OR " +
								"Bonus6Type = '" + eProperty.MaxHealth + "' AND Bonus6 >= '" + hp + "' OR " +
								"Bonus7Type = '" + eProperty.MaxHealth + "' AND Bonus7 >= '" + hp + "' OR " +
								"Bonus8Type = '" + eProperty.MaxHealth + "' AND Bonus8 >= '" + hp + "' OR " +
								"Bonus9Type = '" + eProperty.MaxHealth + "' AND Bonus9 >= '" + hp + "' OR " +
								"Bonus10Type = '" + eProperty.MaxHealth + "' AND Bonus10 >= '" + hp + "' OR " +
								"ExtraBonusType = '" + eProperty.MaxHealth + "' AND ExtraBonus >= '" + hp + "')");
			#endregion

			#region Power
			if(power > 0)
			sql.Append(" AND (Bonus1Type = '" + eProperty.MaxMana + "' AND Bonus1 >= '" + power + "' OR " +
								"Bonus2Type = '" + eProperty.MaxMana + "' AND Bonus2 >= '" + power + "' OR " +
								"Bonus3Type = '" + eProperty.MaxMana + "' AND Bonus3 >= '" + power + "' OR " +
								"Bonus4Type = '" + eProperty.MaxMana + "' AND Bonus4 >= '" + power + "' OR " +
								"Bonus5Type = '" + eProperty.MaxMana + "' AND Bonus5 >= '" + power + "' OR " +
								"Bonus6Type = '" + eProperty.MaxMana + "' AND Bonus6 >= '" + power + "' OR " +
								"Bonus7Type = '" + eProperty.MaxMana + "' AND Bonus7 >= '" + power + "' OR " +
								"Bonus8Type = '" + eProperty.MaxMana + "' AND Bonus8 >= '" + power + "' OR " +
								"Bonus9Type = '" + eProperty.MaxMana + "' AND Bonus9 >= '" + power + "' OR " +
								"Bonus10Type = '" + eProperty.MaxMana + "' AND Bonus10 >= '" + power + "' OR " +
								"ExtraBonusType = '" + eProperty.MaxMana + "' AND ExtraBonus >= '" + power + "')");
			#endregion

			string qryString = sql.ToString();

            InventoryItem[] items = (InventoryItem[])GameServer.Database.SelectObjects(typeof(InventoryItem), qryString);
			int itemsOnPage = page < (int)Math.Ceiling((double)items.Length / 20) ? 20 : items.Length % 20;
            if (itemsOnPage > 0)
            {
				int itemCount = items.Length;
				int pageCount = (int)Math.Ceiling((double)itemCount / 20) - 1;
				client.Player.Out.SendMarketExplorerWindow(items, page, (byte)pageCount);
                client.Player.Out.SendMessage(itemsOnPage.ToString() + " Results found for page " + (page + 1) + ".", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
                client.Player.TempProperties.removeProperty(EXPLORER_LIST);
				client.Player.TempProperties.setProperty(EXPLORER_LIST, items);
            }
            else
                client.Player.Out.SendMessage("No Items found", eChatType.CT_Important, eChatLoc.CL_ChatWindow);
            return 1;
        }
    }
}