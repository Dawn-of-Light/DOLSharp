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
using System.Reflection;
using System.Linq;

using DOL.Database;
using DOL.Events;

using log4net;

namespace DOL.GS.PacketHandler.Client.v168
{
	/// <summary>
	/// No longer used after version 1.104
	/// </summary>
	[PacketHandlerAttribute(PacketHandlerType.TCP, eClientPackets.CharacterDeleteRequest, "Handles character delete requests", eClientStatus.LoggedIn)]
	public class CharacterDeleteRequestHandler : IPacketHandler
	{
		private static readonly ILog Log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			string charName = packet.ReadString(30);
			DOLCharacters[] chars = client.Account.Characters;

			var foundChar = chars?.FirstOrDefault(ch => ch.Name.Equals(charName, StringComparison.OrdinalIgnoreCase));
			if (foundChar != null)
			{
				var slot = foundChar.AccountSlot;
				CharacterCreateRequestHandler.CheckForDeletedCharacter(foundChar.AccountName, client, slot);
			}
		}
	}
}
