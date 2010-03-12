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
using DOL.Database;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, 0x1A, "Set market price")]
    public class PlayerSetMarketPriceHandler : IPacketHandler
    {
        /// <summary>
        /// Defines a logger for this class.
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        public const string NEW_PRICE = "newPrice";
        public int HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client.Player == null)
                return 0;

            int slot = packet.ReadByte();
            int unk1 = packet.ReadByte();
            ushort unk2 = packet.ReadShort();
            uint price = packet.ReadInt();
            Consignment con = client.Player.ActiveConMerchant;
            House house = HouseMgr.GetHouse(con.HouseNumber);
            if (house == null)
                return 0;
            if (!house.HasOwnerPermissions(client.Player))
                return 0;
            int dbSlot = (int)eInventorySlot.Consignment_First + slot;
            InventoryItem item = GameServer.Database.SelectObject<InventoryItem>("OwnerID = '" + client.Player.PlayerCharacter.ObjectId + "' AND SlotPosition = '" + dbSlot.ToString() + "'");
            if (item != null)
            {
                item.SellPrice = (int)price;
                GameServer.Database.SaveObject(item);
            }
            else
            {
                client.Player.TempProperties.setProperty(NEW_PRICE, (int)price);
            }

            // another update required here,currently the player needs to reopen the window to see the price, thats why we msg him
            client.Out.SendMessage("New price set! (open the merchant window again to see the price)", eChatType.CT_System, eChatLoc.CL_SystemWindow);
            return 1;
        }
    }
}