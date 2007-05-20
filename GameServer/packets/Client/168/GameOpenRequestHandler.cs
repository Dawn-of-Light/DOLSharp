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

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x17^168,"Checks if UDP is working for the client")]
	public class GameOpenRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int flag = packet.ReadByte();
			client.UDPPingTime = DateTime.Now.Ticks;
			client.UDPConfirm = flag == 1;
			client.Out.SendGameOpenReply();
			client.Out.SendStatusUpdate(); // based on 1.74 logs
			client.Out.SendUpdatePoints(); // based on 1.74 logs
			if (client.Player != null)
				client.Player.UpdateDisabledSkills(); // based on 1.74 logs
			return 1;
		}
	}
}
