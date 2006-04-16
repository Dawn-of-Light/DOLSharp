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
using DOL.GS.Database;
using NHibernate.Expression;

namespace DOL.GS.PacketHandler.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x6A^168,"Checks for bad character names")]
	public class BadNameCheckRequestHandler : IPacketHandler
	{
		public int HandlePacket(GameClient client, GSPacketIn packet)
		{
			string name = packet.ReadString(30).ToLower();
			
			bool bad = false;

			IList allInvalidNames = GameServer.Database.SelectAllObjects(typeof(InvalidName));
			foreach(InvalidName inv in allInvalidNames)
			{
				if(name.IndexOf(inv.Name.ToLower()) != -1)
				{
					bad = true;
					break;
				}
			}

			//if(bad)
			//DOLConsole.WriteLine(String.Format("Name {0} is bad!",name));
			//else
			//DOLConsole.WriteLine(String.Format("Name {0} seems to be a good name!",name));

			client.Out.SendBadNameCheckReply(name, bad);
			return 1;
		}
	}
}