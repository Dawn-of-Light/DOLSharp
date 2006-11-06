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

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x2D ^ 168, "handle Looking for a group")]
	public class LookingForAGroupHandler : IPacketHandler
	{
		//rewritten by Corillian so if it doesn't work you know who to yell at ;)
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			byte grouped = (byte)packet.ReadByte();
			ArrayList list = new ArrayList();
			if (grouped != 0x00)
			{
				ArrayList groups = GroupMgr.ListGroupByStatus(0x00);
				if (groups != null)
				{
					foreach (PlayerGroup group in groups)
						if (GameServer.ServerRules.IsAllowedToGroup(group.Leader, client.Player, true))
						{
							list.Add(group.Leader);
						}
				}
			}

			ArrayList Lfg = GroupMgr.LookingForGroupPlayers();

			if (Lfg != null)
			{
				foreach (GamePlayer player in Lfg)
				{
					if (player != client.Player && GameServer.ServerRules.IsAllowedToGroup(client.Player, player, true))
					{
						list.Add(player);
					}
				}
			}

			client.Out.SendFindGroupWindowUpdate((GamePlayer[])list.ToArray(typeof(GamePlayer)));
			return 1;
		}
	}
}