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
using System.Linq;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.RegionListRequest, "Handles sending the region overview", eClientStatus.None)]
	public class RegionListRequestHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			var slot = packet.ReadByte();
			if (slot >= 0x14)
				slot += 300 - 0x14;
			else if (slot >= 0x0A)
				slot += 200 - 0x0A;
			else
				slot += 100;
			var character = client.Account.Characters.FirstOrDefault(c => c.AccountSlot == slot);
			client.LoadPlayer(character);

			client.Out.SendRegions();
		}
	}
}
