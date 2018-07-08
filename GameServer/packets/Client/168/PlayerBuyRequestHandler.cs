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
using System.Reflection;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.BuyRequest, "Handles player buy", eClientStatus.PlayerInGame)]
    public class PlayerBuyRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            if (client.Player == null)
            {
                return;
            }

            packet.ReadInt(); // X
            packet.ReadInt(); // Y
            packet.ReadShort(); // id
            ushort itemSlot = packet.ReadShort();
            byte itemCount = (byte)packet.ReadByte();
            byte menuId = (byte)packet.ReadByte();

            switch ((eMerchantWindowType)menuId)
            {
                case eMerchantWindowType.HousingInsideShop:
                case eMerchantWindowType.HousingOutsideShop:
                case eMerchantWindowType.HousingBindstoneHookpoint:
                case eMerchantWindowType.HousingCraftingHookpoint:
                case eMerchantWindowType.HousingNPCHookpoint:
                case eMerchantWindowType.HousingVaultHookpoint:
                    {
                        HouseMgr.BuyHousingItem(client.Player, itemSlot, itemCount, (eMerchantWindowType)menuId);
                        break;
                    }

                default:
                    {
                        if (client.Player.TargetObject == null)
                        {
                            return;
                        }

                        // Forward the buy process to the merchant
                        if (client.Player.TargetObject is GameMerchant merchant)
                        {
                            // Let merchant choose what happens
                            merchant.OnPlayerBuy(client.Player, itemSlot, itemCount);
                        }
                        else
                        {
                            (client.Player.TargetObject as GameLotMarker)?.OnPlayerBuy(client.Player, itemSlot, itemCount);
                        }

                        break;
                    }
            }
        }
    }
}