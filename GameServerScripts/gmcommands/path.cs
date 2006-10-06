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

namespace DOL.GS.Scripts
{
	[CmdAttribute(
		"&path",
		(uint)ePrivLevel.GM,
		"There are several path functions",
		"/path create - creates a temp path in ram",
		"/path load <pathname> - loads a path from db",
		"/path add [speedlimit] [wait time in second] - adds a point at the end of the current path",
		"/path save <pathname> - saves a path to db",
		"/path travel - makes a target mob travel the current path",
		"/path assignhorseroute <Destination> - sets the current path as horseroute on stablemaster",
		"/path hide - removes the temporary objects shown when a path is loaded or created")]
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
			obj.Z = pp.Z;
			obj.CurrentRegion = client.Player.CurrentRegion;
			obj.Heading = client.Player.Heading;
			obj.Name = name;
			obj.Model = 1293;
			obj.Emblem = 0;
			obj.AddToWorld();

			ArrayList objs = (ArrayList)client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			if (objs == null)
				objs = new ArrayList();
			objs.Add(obj);
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, objs);
		}

		private void RemoveAllTempPathObjects(GameClient client)
		{
			ArrayList objs = (ArrayList)client.Player.TempProperties.getObjectProperty(TEMP_PATH_OBJS, null);
			if (objs == null)
				return;

			foreach (GameStaticItem obj in objs)
				obj.Delete();
			client.Player.TempProperties.setProperty(TEMP_PATH_OBJS, null);
		}

		private int PathCreate(GameClient client)
		{
			//Remove old temp objects
			RemoveAllTempPathObjects(client);

			PathPoint startpoint = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, 100000, ePathType.Once);
			client.Player.TempProperties.setProperty(TEMP_PATH_FIRST, startpoint);
			client.Player.TempProperties.setProperty(TEMP_PATH_LAST, startpoint);
			client.Player.Out.SendMessage("Path creation started! You can add new pathpoints via /path add now!", eChatType.CT_System, eChatLoc.CL_SystemWindow);
			CreateTempPathObject(client, startpoint, "TMP PP 1");

			return 1;
		}

		private int PathAdd(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
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
					DisplayError(client, "No valid speedlimit '{0}'!", args[2]);
					return 0;
				}

                if (args.Length > 3)
                {
                    try
                    {
                        waittime = int.Parse(args[3]);
                    }
                    catch
                    {
                        DisplayError(client, "No valid wait time '{0}'!", args[3]);
                    }
                }
			}

			PathPoint newpp = new PathPoint(client.Player.X, client.Player.Y, client.Player.Z, speedlimit, path.Type);
            newpp.WaitTime = waittime*10;
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
			return 1;
		}

		private int PathTravel(GameClient client)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (client.Player.TargetObject == null || !(client.Player.TargetObject is GameNPC))
			{
				DisplayError(client, "You need to select a mob first!");
				return 0;
			}
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}
			int speed = Math.Min(((GameNPC)client.Player.TargetObject).MaxSpeedBase, path.MaxSpeed);
			((GameNPC)client.Player.TargetObject).CurrentWayPoint = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_FIRST, null);
			MovementMgr.Instance.MoveOnPath(((GameNPC)client.Player.TargetObject), speed);
			return 1;
		}

		private int PathType(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayError(client, "Usage: /path type <pathtype>");
				DisplayError(client, "Current path type is '{0}'", path.Type.ToString());
				DisplayError(client, "Possible pathtype values are:");
				DisplayError(client, String.Join(", ", Enum.GetNames(typeof(ePathType))));
				return 0;
			}
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create or /path load first!");
				return 0;
			}

			ePathType pathType = ePathType.Once;
			try
			{
				pathType = (ePathType)Enum.Parse(typeof(ePathType), args[2], true);
			}
			catch
			{
				DisplayError(client, "Usage: /path type <pathtype>");
				DisplayError(client, "Current path type is '{0}'", path.Type.ToString());
				DisplayError(client, "PathType must be one of the following:");
				DisplayError(client, String.Join(", ", Enum.GetNames(typeof(ePathType))));
				return 0;
			}

			path.Type = pathType;
			PathPoint temp = path.Prev;
			while ((temp != null) && (temp != path))
			{
				temp.Type = pathType;
				temp = temp.Prev;
			}
			DisplayError(client, "Current path type set to '{0}'", path.Type.ToString());
			return 1;
		}

		private int PathLoad(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplayError(client, "Usage: /path load <pathname>");
				return 0;
			}
			string pathname = String.Join(" ", args, 2, args.Length - 2);
			PathPoint path = MovementMgr.Instance.LoadPath(pathname);
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
				return 1;
			}
			DisplayError(client, "Path '{0}' not found!", pathname);
			return 0;
		}

		private int PathSave(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayError(client, "Usage: /path save <pathname>");
				return 0;
			}
			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}
			string pathname = String.Join(" ", args, 2, args.Length - 2);
			MovementMgr.Instance.SavePath(pathname, path);
			DisplayMessage(client, "Path saved as '{0}'", pathname);
			return 1;
		}

		private int PathAssignHorseroute(GameClient client, string[] args)
		{
			PathPoint path = (PathPoint)client.Player.TempProperties.getObjectProperty(TEMP_PATH_LAST, null);
			if (args.Length < 2)
			{
				DisplayError(client, "Usage: /path assignhorseroute <destination>");
				return 0;
			}

			if (path == null)
			{
				DisplayError(client, "No path created yet! Use /path create first!");
				return 0;
			}

			GameMerchant merchant = null;
			if (client.Player.TargetObject is GameStableMaster)
				merchant = client.Player.TargetObject as GameStableMaster;
			if (client.Player.TargetObject is GameBoatStableMaster)
				merchant = client.Player.TargetObject as GameBoatStableMaster;
			if (merchant == null)
			{
				DisplayError(client, "You must select a stable master to assign a horseroute!");
				return 0;
			}
			string target = String.Join(" ", args, 2, args.Length - 2);;
			bool ticketFound = false;
			string ticket = "Ticket to " + target;
			if (merchant.TradeItems != null)
			{
				foreach (ItemTemplate template in merchant.TradeItems.GetAllItems().Values)
				{
					if (template != null && template.Name.ToLower() == ticket.ToLower())
					{
						ticketFound = true;
						break;
					}

				}
			}
			if (!ticketFound)
			{
				DisplayError(client, "Stablemaster has no {0}!", ticket);
				return 0;
			}
			MovementMgr.Instance.SavePath(merchant.Name + "=>" + target, path);
			return 1;
		}

		public int OnCommand(GameClient client, string[] args)
		{
			if (args.Length < 2)
			{
				DisplaySyntax(client);
				return 0;
			}
			switch (args[1])
			{
				case "create": return PathCreate(client);
				case "add": return PathAdd(client, args);
				case "travel": return PathTravel(client);
				case "type": return PathType(client, args);
				case "save": return PathSave(client, args);
				case "load": return PathLoad(client, args);
				case "assignhorseroute": return PathAssignHorseroute(client, args);
				case "hide": RemoveAllTempPathObjects(client); return 1;
			}
			DisplaySyntax(client);
			return 0;
		}
	}
}