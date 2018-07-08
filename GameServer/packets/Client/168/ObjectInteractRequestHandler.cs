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
    [PacketHandler(PacketHandlerType.TCP, eClientPackets.ObjectInteractRequest, "Handles Client Interact Request", eClientStatus.PlayerInGame)]
    public class ObjectInteractRequestHandler : IPacketHandler
    {
        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            packet.ReadInt(); //playerX
            packet.ReadInt(); // playerY
            packet.ReadShort(); // sessionId
            ushort targetOid = packet.ReadShort();

            // TODO: utilize these client-sent coordinates to possibly check for exploits which are spoofing position packets but not spoofing them everywhere
            new InteractActionHandler(client.Player, targetOid).Start(1);
        }

        /// <summary>
        /// Handles player interact actions
        /// </summary>
        private class InteractActionHandler : RegionAction
        {
            /// <summary>
            /// The interact target OID
            /// </summary>
            private readonly int _targetOid;

            /// <summary>
            /// Constructs a new InterractActionHandler
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="targetOid">The interact target OID</param>
            public InteractActionHandler(GamePlayer actionSource, int targetOid) : base(actionSource)
            {
                _targetOid = targetOid;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;
                Region region = player.CurrentRegion;

                GameObject obj = region?.GetObject((ushort)_targetOid);
                obj?.Interact(player);
            }
        }
    }
}