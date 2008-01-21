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
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&adddoor",
		ePrivLevel.GM,
		"adds a door to the game",
		"/adddoor <doorID>")]
	public class AddDoorCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			int doorid = 0;
			if (args.Length == 2)
			{
				try
				{
					doorid = Convert.ToInt32( args[1] );
				}
				catch
				{
					DisplayMessage(client, "doorid must be a ushort whitch are equal to real door id");
					return;
				}
				DBDoor door = new DBDoor();
				door.X = client.Player.X;
				door.Y = client.Player.Y;
				door.Z = client.Player.Z;
				door.Heading = client.Player.Heading;
				door.InternalID = doorid;
				GameServer.Database.AddNewObject(door);
				DisplayMessage(client, "door created with id = " + door.ObjectId);
			}
			else
			{
				DisplayMessage(client, "/adddoor <id> this id must be same that door select packet and you must hit command when you are on door");
			}
		}
	}
}