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

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0xE0^168,"Show warmap")]
	public class WarmapShowRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int code = packet.ReadByte();
			int RealmMap = packet.ReadByte();
			int keepId = packet.ReadByte();
			/*if (code == 0)
			{
			client.Out.SendWarmapUpdate(KeepMgr.getKeepsByRealmMap(RealmMap));
			}
			else if (code == 2) // Teleport
			{
				if(keepId == 1 || keepId == 2) // Border Keep
					return 0;
				AbstractGameKeep keep = KeepMgr.getKeepByID(keepId);
				if (keep == null) return 1;
				client.Player.MoveTo((ushort)keep.Region.RegionID, keep.Position, (ushort)keep.Heading);
			}
			else if (code == 1) // TODO Warmap Update
				return 0;
//			client.Out.SendMessage(string.Format("you try request warmap:{0} {1} 0x{2:X4}",RealmMapBefore,RealmMap, unk1), eChatType.CT_Advise, eChatLoc.CL_SystemWindow);
			*/return 1;
		}
	}
}