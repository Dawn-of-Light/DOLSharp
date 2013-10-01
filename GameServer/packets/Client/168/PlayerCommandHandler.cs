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


namespace DOL.GS.PacketHandler.Client.v168
{
	[PacketHandler(PacketHandlerType.TCP,0x07^168,"Handles the players commands")]
	public class PlayerCommandHandler : IPacketHandler
	{
		public void HandlePacket(GameClient client, GSPacketIn packet)
		{
			packet.Skip(8);
			string cmdLine = packet.ReadString(255);
			if(!ScriptMgr.HandleCommand(client, cmdLine))
			{
				if (cmdLine[0] == '&')
				{
					cmdLine = '/' + cmdLine.Remove(0, 1);
				}
				client.Out.SendMessage("No such command ("+cmdLine+")",eChatType.CT_System,eChatLoc.CL_SystemWindow);
			}
		}
	}
}