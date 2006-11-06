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
	[PacketHandler(PacketHandlerType.TCP, 0x05, "Handles housing permissions requests from menu")]
	public class HousePermissionsRequestHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int pid = packet.ReadShort();
			ushort housenumber = packet.ReadShort();
			
			House house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return 1;
			if (client.Player == null) return 1;

			if (!house.IsOwner(client.Player) && client.Account.PrivLevel == 1)
				return 1;
			client.Out.SendHousePermissions(house);
			return 1;
		}

	}
}