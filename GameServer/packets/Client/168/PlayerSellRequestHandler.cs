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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.SellRequest, "Handles player selling", eClientStatus.PlayerInGame)]
    public class PlayerSellRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            packet.ReadInt(); // X
            packet.ReadInt(); // Y
            packet.ReadShort(); // id
            ushort itemSlot = packet.ReadShort();

            if (client.Player.TargetObject == null)
            {
                client.Out.SendMessage("You must select an NPC to sell to.", eChatType.CT_Merchant, eChatLoc.CL_SystemWindow);
                return;
            }

            lock (client.Player.Inventory)
            {
                InventoryItem item = client.Player.Inventory.GetItem((eInventorySlot)itemSlot);
                if (item == null)
                {
                    return;
                }

                if (client.Player.TargetObject is GameMerchant merchant)
                {
                    // Let the merchant choos how to handle the trade.
                    merchant.OnPlayerSell(client.Player, item);
                }
                else
                {
                    (client.Player.TargetObject as GameLotMarker)?.OnPlayerSell(client.Player, item);
                }
            }
        }
    }
}