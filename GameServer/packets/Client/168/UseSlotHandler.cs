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

using DOL.GS.Geometry;

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
            var player = client.Player;
			if (client.Version >= GameClient.eClientVersion.Version1124)
			{
				var x = (int)packet.ReadFloatLowEndian();
				var y = (int)packet.ReadFloatLowEndian();
				var z = (int)packet.ReadFloatLowEndian();
				player.CurrentSpeed = (short)packet.ReadFloatLowEndian();
				var heading = packet.ReadShort();
                player.Position = Position.Create(player.Position.RegionID, x, y, z, heading);
			}
			int flagSpeedData = packet.ReadShort();
			int slot = packet.ReadByte();
			int type = packet.ReadByte();

			new UseSlotAction(player, flagSpeedData, slot, type).Start(1);
		}

		/// <summary>
		/// Handles player use slot actions
		/// </summary>
		protected class UseSlotAction : RegionAction
		{
			/// <summary>
			/// The speed and flags data
			/// </summary>
			protected readonly int m_flagSpeedData;

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
			public UseSlotAction(GamePlayer actionSource, int flagSpeedData, int slot, int useType) : base(actionSource)
			{
				m_flagSpeedData = flagSpeedData;
				m_slot = slot;
				m_useType = useType;
			}

			/// <summary>
			/// Called on every timer tick
			/// </summary>
			protected override void OnTick()
			{
				var player = (GamePlayer) m_actionSource;
				if ((m_flagSpeedData & 0x200) != 0)
				{
					player.CurrentSpeed = (short)(-(m_flagSpeedData & 0x1ff)); // backward movement
				}
				else
				{
					player.CurrentSpeed = (short)(m_flagSpeedData & 0x1ff); // forwardmovement
				}
				player.IsStrafing = (m_flagSpeedData & 0x4000) != 0;
				player.TargetInView = (m_flagSpeedData & 0xa000) != 0; // why 2 bits? that has to be figured out
				player.GroundTargetInView = ((m_flagSpeedData & 0x1000) != 0);
				player.UseSlot(m_slot, m_useType);
			}
		}
	}
}