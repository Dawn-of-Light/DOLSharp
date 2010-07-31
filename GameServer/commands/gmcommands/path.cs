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
using DOL.Database;
using DOL.GS.Movement;
using DOL.GS.PacketHandler;

namespace DOL.GS.Commands
{
	[CmdAttribute(
	   "&path",
	   ePrivLevel.GM,
		"There are several path functions",
		"/path create - creates a new temporary path, deleting any existing temporary path",
		"/path load <pathname> - loads a path from db",
		"/path add [speedlimit] [wait time in second] - adds a point at the end of the current path",
		"/path save <pathname> - saves a path to db",
		"/path travel - makes a target npc travel the current path",
		"/path stop - clears the path for a targeted npc and tells npc to walk to spawn",
		"/path speed [speedlimit] - sets the speed of all path nodes",
		"/path assignhorseroute <Destination> - sets the current path as horseroute on stablemaster",
		"/path hide - hides all path markers but does not delete the path",
		"/path delete - deletes the temporary path",
		"/path type - changes the paths type")]
	public class PathCommandHandler : AbstractCommandHandler, ICommandHandler
	{
		protected string TEMP_PATH_FIRST = "TEMP_PATH_FIRST";
		protected string TEMP_PATH_LAST = "TEMP_PATH_LAST";
		protected string TEMP_PATH_OBJS = "TEMP_PATH_OBJS";

		private void CreateTempPathObject(GameClient client, PathPoint pp, String name)
		{
			//Create a new object
			GameStaticItem obj = new GameStaticItem();
			//Fill the object variables
			obj.X = pp.X;
			obj.Y = pp.Y;
			obj.Z = pp.Z + 2; // raise a bit off of ground level
			obj.CurrentRegion = client.Player.CurrentRegion;
			obj.Heading = client.Player.Heading;
			obj.Name = name;
            obj.Model = 488;
			obj.Emblem = 0;
			obj.AddToWorld();

			ArrayList objs = (ArrayList)client.Player.TempProperties.getProperty<object>(TEMP_PATH_OBJS, null);
			if (objs == null)
				objs = new ArrayList();
			objs.Add(obj);
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, objs);
		}

		private void RemoveAllTempPathObjects(GameClient client)
		{
			ArrayList objs = (ArrayList)client.Player.TempProperties.getProperty<object>(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;

			// remove the markers
			foreach (GameStaticItem obj in objs)
				obj.Delete();

			// clear the path point array
			objs.Clear();

			// remove all path properties
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, null);
			client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, null);
			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, null);
		}

		private void PathHide(GameClient client)
		{
			ArrayList objs = (ArrayList)client.Player.TempProperties.getProperty<object>(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;

			// remove the markers
			foreach (GameStaticItem obj in objs)
				obj.Delete();
		}

		private void PathCreate(GameClient client)
		{
			//Remove old temp objects
			RemoveAllTempPathObjects(client);

			PathPoint startpoint = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, 5000, ePathType.Once);
			client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, startpoint);
			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, startpoint);
			client.Player.Out.SendMessage("Path creation started! You can add new pathpoints via /path add now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			CreateTempPathObject(client, startpoint, "TMP PP 1");
		}

		private void PathAdd(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
			if (path == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create first!");
				return;
			}

			int speedlimit = 1000;
			int waittime = 0;
			if (args.Length > 2)
			{
				try
				{
					speedlimit = int.Parse(args[2]);
				}
				catch
				{
					DisplayMessage(client, "No valid speedlimit '{0}'!", args[2]);
					return;
				}

				if (args.Length > 3)
				{
					try
					{
						waittime = int.Parse(args[3]);
					}
					catch
					{
						DisplayMessage(client, "No valid wait time '{0}'!", args[3]);
					}
				}
			}

			PathPoint newpp = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, speedlimit, path.Type);
			newpp.WaitTime = waittime * 10;
			path.Next = newpp;
			newpp.Prev = path;
			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, newpp);

			int len = 0;
			while (path.Prev != null)
			{
				len++;
				path = path.Prev;
			}
			len += 2;
			CreateTempPathObject(client, newpp, "TMP PP " + len);
			DisplayMessage(client, "Pathpoint added. Current pathlength = {0}", len);
		}

		private void PathSpeed(GameClient client, string[] args)
		{
			int speedlimit = 80;

			if (args.Length < 3)
			{
				DisplayMessage(client, "No valid speedlimit '{0}'!", args[2]);
				return;
			}

			try
			{
				speedlimit = int.Parse(args[2]);
			}
			catch
			{
				DisplayMessage(client, "No valid speedlimit '{0}'!", args[2]);
				return;
			}

			PathPoint pathpoint = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_FIRST, null);

			if (pathpoint == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create first!");
				return;
			}

			pathpoint.MaxSpeed = speedlimit;

			while (pathpoint.Next != null)
			{
				pathpoint = pathpoint.Next;
				pathpoint.MaxSpeed = speedlimit;
			}
		}

		private void PathTravel(GameClient client)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
			if (client.Player.TargetObject == null || !(client.Player.TargetObject is GameNPC))
			{
				DisplayMessage(client, "You need to select a mob first!");
				return;
			}

			if (path == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create first!");
				return;
			}
			short speed = Math.Min(((GameNPC)client.Player.TargetObject).MaxSpeedBase, (short)path.MaxSpeed);

			// clear any current path
			((GameNPC)client.Player.TargetObject).CurrentWayPoint = null;

			// set the new path
			((GameNPC)client.Player.TargetObject).CurrentWayPoint = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_FIRST, null);

			((GameNPC)client.Player.TargetObject).MoveOnPath(speed);
		}

		private void PathStop(GameClient client)
		{
			if (client.Player.TargetObject == null || !(client.Player.TargetObject is GameNPC))
			{
				DisplayMessage(client, "You need to select a mob first!");
				return;
			}

			// clear any current path
			((GameNPC)client.Player.TargetObject).CurrentWayPoint = null;
			((GameNPC)client.Player.TargetObject).WalkToSpawn();
		}

		private void PathType(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayMessage(client, "Usage: /path type <pathtype>");
				DisplayMessage(client, "Current path type is '{0}'", path.Type.ToString());
				DisplayMessage(client, "Possible pathtype values are:");
				DisplayMessage(client, String.Join(", ", Enum.GetNames(typeof(ePathType))));
				return;
			}
			if (path == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create or /path load first!");
				return;
			}

			ePathType pathType = ePathType.Once;
			try
			{
				pathType = (ePathType)Enum.Parse(typeof(ePathType), args[2], true);
			}
			catch
			{
				DisplayMessage(client, "Usage: /path type <pathtype>");
				DisplayMessage(client, "Current path type is '{0}'", path.Type.ToString());
				DisplayMessage(client, "PathType must be one of the following:");
				DisplayMessage(client, String.Join(", ", Enum.GetNames(typeof(ePathType))));
				return;
			}

			path.Type = pathType;
			PathPoint temp = path.Prev;
			while ((temp != null) && (temp != path))
			{
				temp.Type = pathType;
				temp = temp.Prev;
			}
			DisplayMessage(client, "Current path type set to '{0}'", path.Type.ToString());
		}

		private void PathLoad(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplayMessage(client, "Usage: /path load <pathname>");
				return;
			}
			string pathname = String.Join(" ", args, 2, args.Length - 2);
			PathPoint path = MovementMgr.LoadPath(pathname);
			if (path != null)
			{
				RemoveAllTempPathObjects(client);
				DisplayMessage(client, "Path '{0}' loaded.", pathname);
				client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, path);
				int len = 1;
				while (path.Next != null)
				{
					CreateTempPathObject(client, path, "TMP PP " + len);
					path = path.Next;
					len++;
				}
				client.Player.TempProperties.setProperty(TEMP_PATH_LAST, path);
				return;
			}
			DisplayMessage(client, "Path '{0}' not found!", pathname);
		}

		private void PathSave(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayMessage(client, "Usage: /path save <pathname>");
				return;
			}

			if (path == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create first!");
				return;
			}

			string pathname = String.Join(" ", args, 2, args.Length - 2);
			MovementMgr.SavePath(pathname, path);
			DisplayMessage(client, "Path saved as '{0}'", pathname);
		}

		private void PathAssignHorseroute(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getProperty<object>(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayMessage(client, "Usage: /path assignhorseroute <destination>");
				return;
			}

			if (path == null)
			{
				DisplayMessage(client, "No path created yet! Use /path create first!");
				return;
			}

			GameMerchant merchant = null;
			if (client.Player.TargetObject is GameStableMaster)
				merchant = client.Player.TargetObject as GameStableMaster;
			if (client.Player.TargetObject is GameBoatStableMaster)
				merchant = client.Player.TargetObject as GameBoatStableMaster;
			if (merchant == null)
			{
				DisplayMessage(client, "You must select a stable master to assign a horseroute!");
				return;
			}
			string target = String.Join(" ", args, 2, args.Length - 2); ;
			bool ticketFound = false;
			string ticket = "Ticket to " + target;
			// Most //
			// With the new horse system, the stablemasters are using the item.Id_nb to find the horse route in the database
			// So we have to save a path in the database with the Id_nb as a PathID
			// The following string will contain the item Id_nb if it is found in the merchant list
			string pathname = "";
			if (merchant.TradeItems != null)
			{
				foreach (ItemTemplate template in merchant.TradeItems.GetAllItems().Values)
				{
					if (template != null && template.Name.ToLower() == ticket.ToLower())
					{
						ticketFound = true;
						pathname = template.Id_nb;
						break;
					}

				}
			}
			if (!ticketFound)
			{
				DisplayMessage(client, "Stablemaster has no {0}!", ticket);
				return;
			}
			MovementMgr.SavePath(pathname, path);
		}

		public void OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return;
			}

			switch (args[1].ToLower())
			{
				case "create":
					{
						PathCreate(client);
						break;
					}
				case "add":
					{
						PathAdd(client, args);
						break;
					}
				case "travel":
					{
						PathTravel(client);
						break;
					}
				case "stop":
					{
						PathStop(client);
						break;
					}
				case "speed":
					{
						PathSpeed(client, args);
						break;
					}
				case "type":
					{
						PathType(client, args);
						break;
					}
				case "save":
					{
						PathSave(client, args);
						break;
					}
				case "load":
					{
						PathLoad(client, args);
						break;
					}
				case "assignhorseroute":
					{
						PathAssignHorseroute(client, args);
						break;
					}
				case "hide":
					{
						PathHide(client);
						break;
					}
				case "delete":
					{
						RemoveAllTempPathObjects(client);
						break;
					}
				default:
					{
						DisplaySyntax(client);
						break;
					}
			}
		}
	}
}