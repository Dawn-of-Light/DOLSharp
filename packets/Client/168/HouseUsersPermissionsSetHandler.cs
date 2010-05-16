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
using System.Reflection;
using DOL.GS.Housing;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x06, "Handles housing Users permissions requests")]
	public class HouseUsersPermissionsSetHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int permissionSlot = packet.ReadByte();
			int newPermissionLevel = packet.ReadByte();
			ushort houseNumber = packet.ReadShort();

			// house is null, return
			var house = HouseMgr.GetHouse(houseNumber);
			if (house == null)
				return 1;

			// player is null, return
			if (client.Player == null)
				return 1;

			// can't set permissions unless you're the owner.
			if (!house.HasOwnerPermissions(client.Player) && client.Account.PrivLevel <= 1)
				return 1;

			// check if we're setting or removing permissions
			if (newPermissionLevel == 100)
			{
				house.RemovePermission(permissionSlot);
			}
			else
			{
				house.AdjustPermissionSlot(permissionSlot, newPermissionLevel);
			}

			return 1;
		}

		#endregion
	}
}