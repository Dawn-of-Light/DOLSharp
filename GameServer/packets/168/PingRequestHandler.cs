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


namespace DOL.GS.PacketHandler.v168
{
	/// <summary>
	/// Handles the ping packet
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x0B^168,"Sends the ping reply")]
	public class PingRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Called when the packet has been received
		/// </summary>
		/// <param name="client">Client that sent the packet</param>
		/// <param name="packet">Packet data</param>
		/// <returns>Non zero if function was successfull</returns>
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			packet.Skip(4); //Skip the first 4 bytes
			client.PingTime = DateTime.Now.Ticks;
			ulong timestamp = packet.ReadInt();
			client.Out.SendPingReply(timestamp,packet.Sequence);
			return 1;
		}
	}
}
