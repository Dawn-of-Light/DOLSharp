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
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerAppraiseItemRequest, "Player Appraise Item Request handler.", eClientStatus.PlayerInGame)]
    public class PlayerAppraiseItemRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            packet.ReadInt(); // X
            packet.ReadInt(); // Y
            packet.ReadShort(); // id
            ushort itemSlot = packet.ReadShort();

            new AppraiseActionHandler(client.Player, itemSlot).Start(1);
        }

        /// <summary>
        /// Handles item apprise actions
        /// </summary>
        private class AppraiseActionHandler : RegionAction
        {
            /// <summary>
            /// The item slot
            /// </summary>
            private readonly int _slot;

            /// <summary>
            /// Constructs a new AppraiseAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="slot">The item slot</param>
            public AppraiseActionHandler(GamePlayer actionSource, int slot) : base(actionSource)
            {
                _slot = slot;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;

                if (player.TargetObject == null)
                {
                    return;
                }

                InventoryItem item = player.Inventory.GetItem((eInventorySlot)_slot);

                if (player.TargetObject is GameMerchant merchant)
                {
                    merchant.OnPlayerAppraise(player, item, false);
                }
                else
                {
                    (player.TargetObject as GameLotMarker)?.OnPlayerAppraise(player, item, false);
                }
            }
        }
    }
}