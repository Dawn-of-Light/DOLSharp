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
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CreatePlayerRequest, "Handles requests for players(0x7C) in game", eClientStatus.PlayerInGame)]
	public class PlayerCreationRequestHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			uint id = 0;
			if (client.Version >= GameClient.eClientVersion.Version1126)
				id = packet.ReadIntLowEndian();
			else
				id = packet.ReadShort();
			GameClient target = WorldMgr.GetClientFromID(id);
			if (target == null)
			{
				log.Warn($"Client {client.SessionID}:{client.TcpEndpointAddress} account {client.Account?.Name ?? "null"} requested invalid client {id} --- disconnecting");

				client.Disconnect();
				return;
			}

			// DOLConsole.WriteLine("player creation request "+target.Player.Name);
			if (target.IsPlaying && target.Player != null && target.Player.ObjectState == GameObject.eObjectState.Active)
			{
				client.Out.SendPlayerCreate(target.Player);
				client.Out.SendLivingEquipmentUpdate(target.Player);
			}
		}

		public void _HandlePacket1126(GameClient client, GSPacketIn packet)
		{
			var id = packet.ReadIntLowEndian();
		}
	}
}