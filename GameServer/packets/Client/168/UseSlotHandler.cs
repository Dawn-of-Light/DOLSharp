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
 //patch 0060
namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// Handles spell cast requests from client
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.UseSlot, "Handle Player Use Slot Request.", eClientStatus.PlayerInGame)]
	

    public class UseSlotHandler : IPacketHandler
    {
        

        public void HandlePacket(GameClient client, GSPacketIn packet)
        {
            client.Player.X = (int)packet.ReadFloatLowEndian();
            client.Player.Y = (int)packet.ReadFloatLowEndian();
            client.Player.Z = (int)packet.ReadFloatLowEndian();
            client.Player.CurrentSpeed = (short)packet.ReadFloatLowEndian();
            short casterSpeed = client.Player.CurrentSpeed;
            client.Player.Heading = packet.ReadShort();
            ushort targetVisible = packet.ReadShort(); // target visible ? 0xA000 : 0x0000
            int slot = packet.ReadByte();
            int type = packet.ReadByte();

            new UseSlotAction1124(client.Player, casterSpeed, targetVisible, slot, type).Start(1);
        }

        

        

        /// <summary>
        /// Handles player use slot actions
        /// </summary>
        protected class UseSlotAction1124 : RegionAction
        {
            /// <summary>
            /// The speed and flags data
            /// </summary>
            protected readonly int m_flagSpeedData;

            /// <summary>
            /// The speed and flags data
            /// </summary>
            protected readonly ushort m_targetVisible;

            /// <summary>
            /// The slot index
            /// </summary>
            protected readonly int m_slot;

            /// <summary>
            /// The use type
            /// </summary>
            protected readonly int m_useType;

            /// <summary>
            /// Constructs a new UseSlotAction
            /// </summary>
            /// <param name="actionSource">The action source</param>
            /// <param name="flagSpeedData">The speed and flags data</param>
            /// <param name="slot">The slot index</param>
            /// <param name="useType">The use type</param>
            public UseSlotAction1124(GamePlayer actionSource, int flagSpeedData, ushort targetVisible, int slot, int useType) : base(actionSource)
            {
                m_flagSpeedData = flagSpeedData;
                m_slot = slot;
                m_useType = useType;
                m_targetVisible = targetVisible;
            }

            /// <summary>
            /// Called on every timer tick
            /// </summary>
            protected override void OnTick()
            {
                var player = (GamePlayer)m_actionSource;
                if ((m_flagSpeedData & 0x200) != 0)
                {
                    player.CurrentSpeed = (short)(-(m_flagSpeedData & 0x1ff)); // backward movement
                }
                else
                {
                    player.CurrentSpeed = (short)(m_flagSpeedData & 0x1ff); // forwardmovement
                }
                player.IsStrafing = (m_flagSpeedData & 0x4000) != 0;
                player.TargetInView = (m_targetVisible & 0xa000) != 0;
                player.GroundTargetInView = ((m_targetVisible & 0x1000) != 0);
                player.UseSlot(m_slot, m_useType);
            }
        }
        
    }
}
