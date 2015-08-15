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
	/// <summary>
	/// Handles spell cast requests from client
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.UseSlot, "Handle Player Use Slot Request.", eClientStatus.PlayerInGame)]
	public class UseSlotHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int flagSpeedData = packet.ReadShort();
			int slot = packet.ReadByte();
			int type = packet.ReadByte();

			new RegionTimerAction<GamePlayer>(client.Player, pl => {
			                                  	if ((flagSpeedData & 0x200) != 0)
			                                  	{
			                                  		pl.CurrentSpeed = (short)(-(flagSpeedData & 0x1ff)); // backward movement
			                                  	}
			                                  	else
			                                  	{
			                                  		pl.CurrentSpeed = (short)(flagSpeedData & 0x1ff); // forwardmovement
			                                  	}
			                                  	pl.IsStrafing = (flagSpeedData & 0x4000) != 0;
			                                  	pl.TargetInView = (flagSpeedData & 0xa000) != 0; // why 2 bits? that has to be figured out
			                                  	pl.GroundTargetInView = ((flagSpeedData & 0x1000) != 0);
			                                  	pl.UseSlot(slot, type);
			                                  }).Start(1);
		}
		#endregion
	}
}