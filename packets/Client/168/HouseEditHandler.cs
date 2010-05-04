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
using System.Collections.Generic;
using DOL.Database;
using System.Reflection;
using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x01,"Change handler for outside/inside look (houses).")]
	public class HouseEditHandler : IPacketHandler
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			int swtch;
			int chnge;
			ushort playerID = packet.ReadShort(); // no use for that.

			var changes = new List<int>();
			for(int i = 0; i < 10; i++)
			{
				swtch = packet.ReadByte();
				chnge = packet.ReadByte();
				if(swtch != 255)
				{
					changes.Add(chnge);
				}
			}

			if(changes.Count > 0 && client.Player.CurrentHouse != null)
			{
				client.Player.CurrentHouse.Edit(client.Player, changes);
			}

			return 1;
		}
	}
}