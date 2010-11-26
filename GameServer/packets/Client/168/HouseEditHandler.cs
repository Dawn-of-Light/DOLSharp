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
using System.Collections.Generic;

namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP, 0x01, "Change handler for outside/inside look (houses).")]
	public class HouseEditHandler : IPacketHandler
	{
		private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		#region IPacketHandler Members

		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			ushort playerID = packet.ReadShort(); // no use for that.

			// house is null, return
			var house = client.Player.CurrentHouse;
			if(house == null)
				return;

			// grab all valid changes
			var changes = new List<int>();
			for (int i = 0; i < 10; i++)
			{
				int swtch = packet.ReadByte();
				int change = packet.ReadByte();

				if (swtch != 255)
				{
					changes.Add(change);
				}
			}

			// apply changes
			if (changes.Count > 0)
			{
				house.Edit(client.Player, changes);
			}
		}

		#endregion
	}
}