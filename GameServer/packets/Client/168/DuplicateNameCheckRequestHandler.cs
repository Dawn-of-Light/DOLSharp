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

using DOL.Database;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.DuplicateNameCheck, "Checks if a character name already exists", eClientStatus.LoggedIn)]
	public class DupNameCheckRequestHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			string name = packet.ReadString(30);

			var character = GameServer.Database.SelectObjects<DOLCharacters>("`Name` = @Name", new QueryParameter("@Name", name)).FirstOrDefault();
			byte result = 0;
			// Bad Name check.
			if (character != null)
				result = 0x02;
			else if (GameServer.Instance.PlayerManager.InvalidNames[name])
				result = 0x01;

			client.Out.SendDupNameCheckReply(name, result);
		}
	}
}
