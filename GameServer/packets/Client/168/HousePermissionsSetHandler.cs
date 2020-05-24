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
using DOL.Database;
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.HousePermissionSet, "Handles housing permissions changes", eClientStatus.PlayerInGame)]
	public class HousePermissionsSetHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int level = packet.ReadByte();
			int unk1 = packet.ReadByte();
			ushort housenumber = packet.ReadShort();

			// make sure permission level is within bounds
			if (level < HousingConstants.MinPermissionLevel || level > HousingConstants.MaxPermissionLevel)
				return;

			// house is null, return
			var house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return;

			// player is null, return
			if (client.Player == null)
				return;

			// player has no owner permissions and isn't a GM or admin, return
			if (!house.HasOwnerPermissions(client.Player) && client.Account.PrivLevel <= 1)
				return;

			// read in the permission values
			DBHousePermissions permission = house.PermissionLevels[level];

			permission.CanEnterHouse = (packet.ReadByte() != 0);
			permission.Vault1 = (byte) packet.ReadByte();
			permission.Vault2 = (byte) packet.ReadByte();
			permission.Vault3 = (byte) packet.ReadByte();
			permission.Vault4 = (byte) packet.ReadByte();
			permission.CanChangeExternalAppearance = (packet.ReadByte() != 0);
			permission.ChangeInterior = (byte) packet.ReadByte();
			permission.ChangeGarden = (byte) packet.ReadByte();
			permission.CanBanish = (packet.ReadByte() != 0);
			permission.CanUseMerchants = (packet.ReadByte() != 0);
			permission.CanUseTools = (packet.ReadByte() != 0);
			permission.CanBindInHouse = (packet.ReadByte() != 0);
			permission.ConsignmentMerchant = (byte) packet.ReadByte();
			permission.CanPayRent = (packet.ReadByte() != 0);
			int unk2 = (byte) packet.ReadByte();

			// save the updated permission
			GameServer.Database.SaveObject(permission);
		}
	}
}