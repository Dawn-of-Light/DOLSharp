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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x0E, "Handles housing decoration rotation")]
	public class HousingDecorationRotateHandler : IPacketHandler
	{
		private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int unk1 = packet.ReadByte();
			int position = packet.ReadByte();
			ushort housenumber = packet.ReadShort();
			ushort angle = packet.ReadShort();
			ushort unk2 = packet.ReadShort();

			House house = HouseMgr.GetHouse(housenumber);
			if (house == null)
				return 1;
			if (client.Player == null) return 1;

			if (!house.IsOwner(client.Player))
				return 1;
			IndoorItem iitem = ((IndoorItem)house.IndoorItems[position]);
			if (iitem == null)
			{
				client.Player.Out.SendMessage("error: id was null", eChatType.CT_Help, eChatLoc.CL_SystemWindow);
				return 1;
			} //should this ever happen?
			int old = iitem.Rotation;
			iitem.Rotation = (iitem.Rotation + angle) % 360;
			if (iitem.Rotation < 0)
				iitem.Rotation = 360 + iitem.Rotation;

			client.Player.Out.SendMessage(string.Format("Interior decoration rotated from {0} degrees to {1}", old, iitem.Rotation), eChatType.CT_System, eChatLoc.CL_SystemWindow);
			foreach (GamePlayer plr in house.GetAllPlayersInHouse())
				plr.Client.Out.SendFurniture(house, position);
			return 1;
		}

	}
}