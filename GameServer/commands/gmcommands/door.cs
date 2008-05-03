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
using DOL.Language;

namespace DOL.GS.Commands
{
	[CmdAttribute(
		"&door",
		ePrivLevel.GM,
		"GMCommands.Door.Description",
		"GMCommands.Door.Usage.Add",
		"GMCommands.Door.Usage.Remove")]
	public class DoorCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			int DoorID = 0;
			try
			{
				DoorID = Convert.ToInt32(args[2]);
			}
			catch
			{
				DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Door.InvalidDoorID"));
				return;
			}

			switch (args[1].ToLower())
			{
				#region Add
				case "add":
					{
						DBDoor door = (DBDoor)GameServer.Database.SelectObject(typeof(DBDoor), "InternalID='" + DoorID.ToString() + "'");
						if (door == null)
						{
							door = new DBDoor();
							door.X = client.Player.X;
							door.Y = client.Player.Y;
							door.Z = client.Player.Z;
							door.Heading = client.Player.Heading;
							door.InternalID = DoorID;
							GameServer.Database.AddNewObject(door);
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Door.Created", door.ObjectId));
						}
						else
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Door.DoorExist", DoorID));
						}
						break;
					}
				#endregion Add
				#region Remove
				case "remove":
					{
						DBDoor door = (DBDoor)GameServer.Database.SelectObject(typeof(DBDoor), "InternalID='" + DoorID.ToString() + "'");
						if (door != null)
						{
							door.AutoSave = false;
							GameServer.Database.DeleteObject(door);
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Door.Removed", door.InternalID));
						}
						else
						{
							DisplayMessage(client, LanguageMgr.GetTranslation(client, "GMCommands.Door.NoDoor", DoorID));
						}
						break;
					}
				#endregion Remove
				#region Default
				default:
					{
						DisplaySyntax(client);
						break;
					}
				#endregion Default
			}
		}
	}
}