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
namespace DOL.GS.PacketHandler.Client.v168
{
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.PlayerSitRequest, "Handles Player Sit Request.", eClientStatus.PlayerInGame)]
    public class PlayerSitRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            var status = (byte)packet.ReadByte();

            new SitRequestHandler(client.Player, status != 0x00).Start(1);
        }

        /// <summary>
        /// Handles player sit requests
        /// </summary>
        private class SitRequestHandler : RegionAction
        {
            /// <summary>
            /// The new sit state
            /// </summary>
            private readonly bool _sit;

            /// <summary>
            /// Constructs a new SitRequestHandler
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="sit">The new sit state</param>
            public SitRequestHandler(GamePlayer actionSource, bool sit) : base(actionSource)
            {
                _sit = sit;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;

                player.Sit(_sit);
            }
        }
    }
}