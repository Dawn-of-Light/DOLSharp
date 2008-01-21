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
using DOL.Database2;


namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP,0x63^168,"Checks if a character name already exists")]
	public class DupNameCheckRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string name=packet.ReadString(30);
			string select = string.Format("Name = '{0}'",GameServer.Database.Escape(name));
			Character character = (Character) GameServer.Database.SelectObject(typeof(Character), select);
			bool nameExists = (character != null);
			if (!nameExists)
			{
				character = (CharacterArchive)GameServer.Database.SelectObject(typeof(CharacterArchive), select);
				nameExists = (character != null);
			}

			client.Out.SendDupNameCheckReply(name,nameExists);
			return 1;
		}
	}
}
