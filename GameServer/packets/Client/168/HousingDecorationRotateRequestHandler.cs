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
using DOL.Database;
using DOL.GS.Housing;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x18, "Handles housing decoration request")]
	public class HousingDecorationRotateRequestHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort housenumber = packet.ReadShort();
			byte index = (byte)packet.ReadByte();
			byte unk1 = (byte)packet.ReadByte();

			House house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return 1;
			if (client.Player == null) return 1;
			// Working only for inside items.
			if (!client.Player.InHouse) return 1;
			
			if (!house.HasOwnerPermissions(client.Player) && !house.CanAddInterior(client.Player))
				return 1;

			GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HouseDecorationRotate));
			pak.WriteShort(housenumber);
			pak.WriteByte(index);
			pak.WriteByte(0x01);
			client.Out.SendTCP(pak);

			return 1;
		}

	}
}