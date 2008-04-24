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
			client.Player.Heading = (ushort)(head & 0xFFF);
			packet.Skip(1); // unknown
			int flags = packet.ReadByte();
//			client.Player.PetInView = ((flags & 0x04) != 0); // TODO
			client.Player.GroundTargetInView = ((flags & 0x08) != 0);
			client.Player.TargetInView = ((flags & 0x10) != 0);


			byte[] con = packet.ToArray();
			con[5] &= 0xC1; //11 00 00 01 = 0x80(Torch) + 0x40(Unknown) + 0x1(Unknown), all other in view check's not need send anyone
			//stealth is set here
			if (client.Player.IsStealthed)
			{
				con[5] |= 0x02;
			}
			con[8] = (byte)((con[8] & 0x80) | client.Player.HealthPercent);

			GSUDPPacketOut outpak = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerHeading));
			//Now copy the whole content of the packet
			outpak.Write(con, 0, /*con.Length*/10);
			outpak.WritePacketLength();

			GSUDPPacketOut outpak190 = null;

//			byte[] outp = outpak.GetBuffer();
//			outpak = null;

			foreach(GamePlayer player in client.Player.GetPlayersInRadius(WorldMgr.VISIBILITY_DISTANCE))
			{
				if(player != null && player != client.Player)
				{
					if (player.Client.Version >= GameClient.eClientVersion.Version190)
					{
						if (outpak190 == null)
						{
	 						outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(ePackets.PlayerHeading));
	 						byte[] con190 = (byte[]) con.Clone();
							//Now copy the whole content of the packet
							outpak190.Write(con190, 0, /*con190.Lenght*/10);
							outpak190.WriteByte(client.Player.ManaPercent);
							outpak190.WriteByte(client.Player.EndurancePercent);
							outpak190.WritePacketLength();
//							byte[] outp190 = outpak190.GetBuffer();
//							outpak190 = null;// ?
						}
						player.Out.SendUDPRaw(outpak190);
					}
					else
						player.Out.SendUDPRaw(outpak);
				}
			}
			return 1;
		}
	}
}
