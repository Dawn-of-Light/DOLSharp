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
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.PlayerHeadingUpdate, ClientStatus.PlayerInGame)]
	public class PlayerHeadingUpdateHandler : IPacketHandler
	{
		/// <summary>
		/// Defines a logger for this class.
		/// </summary>
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			if (client == null || client.Player == null)
				return;

			if (client.Player.ObjectState != GameObject.eObjectState.Active) return;

			ushort sessionId = packet.ReadShort(); // session ID
			if (client.SessionID != sessionId)
			{
//				GameServer.BanAccount(client, 120, "Hack sessionId", string.Format("Wrong sessionId:0x{0} in 0xBA packet (SessionID:{1})", sessionId, client.SessionID));
				return; // client hack
			}

			ushort head = packet.ReadShort();
			client.Player.Heading = (ushort)(head & 0xFFF);
			packet.Skip(1); // unknown
			int flags = packet.ReadByte();
//			client.Player.PetInView = ((flags & 0x04) != 0); // TODO
			client.Player.GroundTargetInView = ((flags & 0x08) != 0);
			client.Player.TargetInView = ((flags & 0x10) != 0);

			byte[] con = packet.ToArray();
			if (!client.Player.IsAlive)
			{
				con[9] = 5; // set dead state
			}
			else if (client.Player.Steed != null && client.Player.Steed.ObjectState == GameObject.eObjectState.Active)
			{
				client.Player.Heading = client.Player.Steed.Heading;
				con[9] = 6; // Set ride state
				con[7] = (byte)(client.Player.Steed.RiderSlot(client.Player)); // there rider slot this player
				con[2] = (byte)(client.Player.Steed.ObjectID >> 8); //heading = steed ID
				con[3] = (byte)(client.Player.Steed.ObjectID & 0xFF);
			}
			con[5] &= 0xC0; //11 00 00 00 = 0x80(Torch) + 0x40(Unknown), all other in view check's not need send anyone
			if (client.Player.IsWireframe)
			{
				con[5] |= 0x01;
			}
			//stealth is set here
			if (client.Player.IsStealthed)
			{
				con[5] |= 0x02;
			}
			con[8] = (byte)((con[8] & 0x80) | client.Player.HealthPercent);

			GSUDPPacketOut outpak = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerHeading));
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
	 						outpak190 = new GSUDPPacketOut(client.Out.GetPacketCode(eServerPackets.PlayerHeading));
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
		}
	}
}
