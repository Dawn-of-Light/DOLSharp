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
	[PacketHandler(PacketHandlerType.TCP, 0x03, "Handles housing Users permissions requests from menu")]
	public class HouseUsersPermissionsRequestHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unk1 = packet.ReadByte();
			int unk2 = packet.ReadByte();
			ushort housenumber = packet.ReadShort();
			House house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return 1;
			if (client.Player == null) return 1;

            if (!house.HasOwnerPermissions(client.Player) && client.Account.PrivLevel == 1)
				return 1;


			GSTCPPacketOut pak = new GSTCPPacketOut(client.Out.GetPacketCode(ePackets.HouseUserPermissions));

			pak.WriteByte((byte)house.CharsPermissions.Count);			// Number of permissions
			pak.WriteByte(0x00);				// ?
			pak.WriteShort(housenumber);			// House N°

			foreach (DBHouseCharsXPerms perm in house.CharsPermissions)
			{
				pak.WriteByte((byte)perm.Slot);				// Slot
				pak.WriteByte((byte)0x00);			// ?
				pak.WriteByte((byte)0x00);			// ?
				pak.WriteByte((byte)perm.PermissionType);		// Type (Guild, Class, Race ...)
				pak.WriteByte((byte)perm.PermissionLevel);	// Level (Friend, Visitor ...)
				pak.WritePascalString(perm.DisplayName);
			}
			client.Out.SendTCP(pak);
			return 1;
		}
	}
}
