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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[Cmd(
		"&clientlist",
		ePrivLevel.GM,
		"Show a list of currently playing clients and their ID's")]
	public class ClientListCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			ArrayList clients = WorldMgr.GetAllPlayingClients();
			ArrayList message = new ArrayList();

			foreach (GameClient gc in clients)
			{
				if (gc.Player != null)
				{
					message.Add("(" + gc.SessionID + ") " + gc.Player.Name);
				}
			}

			client.Out.SendCustomTextWindow("[ Playing Client List ]", message);
			return;
		}
	}
}
