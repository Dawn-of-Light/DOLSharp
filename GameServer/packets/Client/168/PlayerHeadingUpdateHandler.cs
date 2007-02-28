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
using System.Collections;
using System.Reflection;
using DOL.GS;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x12^168,"Handles player direction updates")]
	public class PlayerHeadingUpdateHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			if(client.Player.ObjectState!=GameObject.eObjectState.Active) return 1;

			packet.Skip(2); // session ID
			ushort head = packet.ReadShort();
			client.Player.Heading=(ushort)(head&0xFFF);

			byte[] con = packet.ToArray();
			//stealth is set here
			if (client.Player.IsStealthed)
			{
				con[5]|=0x02;
			}
			con[8] = (byte)((con[8] & 0x80) | client.Player.HealthPercent);

			GSUDPPacketOut outpak = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerHeading));
			//Now copy the whole content of the packet
			outpak.Write(con,0,con.Length);
			outpak.WritePacketLength();
			byte[] outp = outpak.GetBuffer();
			outpak = null;

			foreach(GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
				if(player != null && player != client.Player)
				{
					player.Out.SendUDP(outp);
				}
			return 1;
		}
	}
}
