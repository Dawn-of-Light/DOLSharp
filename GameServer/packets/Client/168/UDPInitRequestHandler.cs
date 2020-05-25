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
using System.Net;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.UDP, eClientPackets.UDPInitRequest, "Handles UDP init", eClientStatus.None)]
	public class UDPInitRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			string localIP;
			ushort localPort;
			if (client.Version >= GameClient.eClientVersion.Version1124)
			{
				localIP = packet.ReadString(20);
				localPort = packet.ReadShort();
			}
			else
			{
				localIP = packet.ReadString(22);
				localPort = packet.ReadShort();
			}
			client.LocalIP = localIP;
			// client.UdpEndPoint = new IPEndPoint(IPAddress.Parse(localIP), localPort);
			client.Out.SendUDPInitReply();
		}
	}
}
