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
using DOL.GS.Housing;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x05, "Handles housing permissions requests from menu")]
	public class HousePermissionsRequestHandler : IPacketHandler
	{
		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			int pid = packet.ReadShort();
			ushort housenumber = packet.ReadShort();

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

			// send out the house permissions
			using (var pak = new GSTCPPacketOut(client.Out.GetPacketCode(eServerPackets.HousingPermissions)))
			{
				pak.WriteByte(HousingConstants.MaxPermissionLevel); // number of permissions ?
				pak.WriteByte(0x00); // unknown
				pak.WriteShort((ushort)house.HouseNumber);

				foreach (var entry in house.PermissionLevels)
				{
					var level = entry.Key;
					var permission = entry.Value;

					pak.WriteByte((byte)level);
					pak.WriteByte(permission.CanEnterHouse ? (byte)1 : (byte)0);
					pak.WriteByte(permission.Vault1);
					pak.WriteByte(permission.Vault2);
					pak.WriteByte(permission.Vault3);
					pak.WriteByte(permission.Vault4);
					pak.WriteByte(permission.CanChangeExternalAppearance ? (byte)1 : (byte)0);
					pak.WriteByte(permission.ChangeInterior);
					pak.WriteByte(permission.ChangeGarden);
					pak.WriteByte(permission.CanBanish ? (byte)1 : (byte)0);
					pak.WriteByte(permission.CanUseMerchants ? (byte)1 : (byte)0);
					pak.WriteByte(permission.CanUseTools ? (byte)1 : (byte)0);
					pak.WriteByte(permission.CanBindInHouse ? (byte)1 : (byte)0);
					pak.WriteByte(permission.ConsignmentMerchant);
					pak.WriteByte(permission.CanPayRent ? (byte)1 : (byte)0);
					pak.WriteByte(0x00); // ??
				}

				client.Out.SendTCP(pak);
			}
		}

		#endregion
	}
}